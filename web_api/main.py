# web_api/main.py
"""
メインアプリケーション
"""
import json
import glob
import os
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
    allow_origins=["http://localhost:3000"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)


# class GenRequest(BaseModel):
#     """
#     リクエストボディの定義
#     """
#     category: str
#     text_prompt: str
#     parameters: Dict[str, Any] = {}

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
        # specs: dict[str, Any] = {}

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

        return FileResponse(
            path=output_rfa,
            filename=f"{req.type}_CloudGen.rfa",
            media_type="application/octet-stream",
        )

    except Exception as e:
        raise HTTPException(
                status_code=500,
                detail=f"Failed to write JSON: {str(e)}",
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
