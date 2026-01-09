# web_api/main.py
"""
メインアプリケーション
"""
import json
import glob
import os
import shutil
import time
import olefile
from openai import OpenAI
from fastapi import FastAPI, HTTPException
from fastapi.middleware.cors import CORSMiddleware
from fastapi.responses import FileResponse
import aps_utils

# 作成したモジュールをインポート
import config
from schemas import PromptRequest, GenerateRequest

# ロジッククラスをインポート
from logic.shelf import ShelfLogic

from logic.desk import DeskLogic

app = FastAPI()

client = OpenAI(api_key=config.OPENAI_API_KEY)

app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

TEMP_DIR = "temp"
os.makedirs(
    TEMP_DIR,
    exist_ok=True,
)

# --- ヘルパー関数: ロジックの選定 ---
def get_logic(category: str):
    """
    ロジックの選定
    """
    if category.lower() == "shelf":
        return ShelfLogic()
    elif category.lower() == "desk":
        return DeskLogic()
    return None


@app.get("/")
def read_root():
    """
    ルートエンドポイント
    """
    return {"message": "Archifields API (Refactored)"}


@app.post("/suggest")
def suggest_parameters(req: PromptRequest):
    """
    パラメータ提案
    """
    print(f"Analyzing prompt for {req.category}: {req.prompt}")

    logic = get_logic(req.category)
    if not logic:
        raise HTTPException(status_code=400, detail="Unsupported category")

    system_instruction = logic.get_system_instruction()

    try:
        response = client.chat.completions.create(
            model="gpt-4o-mini",
            messages=[
                {"role": "system", "content": system_instruction},
                {"role": "user", "content": req.prompt},
            ],
            response_format={"type": "json_object"},
        )
        content = response.choices[0].message.content
        if content is None:
            raise HTTPException(status_code=500, detail="No content received from AI")
        if not isinstance(content, str):
            raise HTTPException(status_code=500, detail="Content is not a string")
        result_json = json.loads(content)
        return result_json

    except Exception as e:
        print(f"Error: {e}")
        raise HTTPException(
            status_code=500,
            detail=f"Error: {str(e)}"
        ) from e


@app.post("/generate")
def generate_family(req: GenerateRequest):
    """
    生成リクエスト
    """
    try:
        print(f"Received request: {req.type}")

        logic = get_logic(req.type)
        if not logic:
            raise HTTPException(
                status_code=400,
                detail=f"Unsupported type: {req.type}",
            )

        # ロジッククラスを使ってC#用のデータに変換
        specs_payload = logic.format_for_revit(req.params)

        final_file_name: str = "output"

        # 全体の構造を作成
        data = {
            "command": req.command,
            "parameters": {
                "familyName": final_file_name,
                "category": "Furniture",
                "type": req.type,
                "specs": specs_payload,
            },
        }

        with open(config.JSON_PATH, "w", encoding="utf-8") as f:
            json.dump(data, f, indent=4)
        print(f"Saved to {config.JSON_PATH}")

        # テンプレートのファイルのパス
        template_path = "Metric Generic Model.rft"
        if not os.path.exists(template_path):
            raise HTTPException(
                status_code=500, detail="Template file not found on server."
            )

        output_rfa = os.path.join(TEMP_DIR, "output_family.rfa")

        aps_utils.run_gen_on_cloud(
            config.JSON_PATH,
            template_path,
            output_rfa,
        )
        # 保存先のフォルダを確認
        os.makedirs(config.OUTPUT_DIR, exist_ok=True)

        # ユニークな名前で保存
        timestamp = int(time.time())
        save_filename = f"{req.type}_{timestamp}.rfa"
        save_path = os.path.join(
            config.OUTPUT_DIR,
            save_filename
        )

        # tempフォルダからOutputFamiliesフォルダにファイルを移動
        shutil.move(output_rfa, save_path)
        print(f"ファイルを移動しました: {save_path}")

        try:
            png_filename = save_filename.replace(".rfa", ".png")
            png_path = os.path.join(config.OUTPUT_DIR, png_filename)

            # プレビュー画像の抽出処理
            if olefile.isOleFile(save_path):
                with olefile.OleFileIO(save_path) as ole:
                    # Revitのプレビュー画像ストリームを探す
                    stream_name = None
                    # 一般的なストリーム名 "RevitPreview4.0" を探す
                    for entry in ole.listdir():
                        if "RevitPreview4.0" in entry:
                            stream_name = entry
                            break

                    if stream_name:
                        image_data = ole.openstream(stream_name).read()
                        # そのままPNGとして保存
                        with open(png_path, "wb") as f:
                            f.write(image_data)
                        print(f"✅ Preview extracted: {png_path}")
                    else:
                        print("⚠️ No preview stream found in RFA.")
            else:
                print("⚠️ Output file is not a valid OLE file.")

        except (OSError, IOError) as e:
            # 画像抽出に失敗しても、RFAのダウンロードはできるようにエラーは握りつぶす
            print(f"⚠️ Failed to extract preview image: {e}")

        return FileResponse(
            path=save_path,
            filename=save_filename,
            media_type="application/octet-stream",
        )

    except Exception as e:
        print(f"生成時にエラーが発生しました: {str(e)}")
        raise HTTPException(
                status_code=500,
                detail=f"Failed to generate family: {str(e)}",
            ) from e


# プレビュー・ダウンロード系は変更なし（configのパスを使用）
@app.get("/preview/latest")
def preview_latest():
    """
    最新のプレビュー画像を取得
    """
    try:
        list_of_files = glob.glob(os.path.join(config.OUTPUT_DIR, "*.png"))
        if not list_of_files:
            raise HTTPException(status_code=404, detail="No preview image found.")
        latest_file = max(list_of_files, key=os.path.getmtime)
        return FileResponse(latest_file)
    except Exception as e:
        raise HTTPException(
            status_code=500,
            detail=f"Failed to get latest preview image: {str(e)}",
        ) from e


@app.get("/download/latest")
def download_latest():
    """
    最新のRFAファイルをダウンロード
    """
    try:
        list_of_files = glob.glob(os.path.join(config.OUTPUT_DIR, "*.rfa"))
        if not list_of_files:
            raise HTTPException(status_code=404, detail="No RFA files found.")
        latest_file = max(list_of_files, key=os.path.getmtime)
        filename = os.path.basename(latest_file)
        return FileResponse(
            path=latest_file, filename=filename, media_type="application/octet-stream"
        )
    except Exception as e:
        raise HTTPException(
            status_code=500,
            detail=f"Failed to download latest RFA file: {str(e)}",
        ) from e


if __name__ == "__main__":
    import uvicorn

    uvicorn.run(app, host="0.0.0.0", port=8000)
