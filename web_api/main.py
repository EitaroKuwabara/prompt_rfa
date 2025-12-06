# web_api/main.py
"""
Revit AI ServerのAPIを提供する
"""
import os
import json
from fastapi import FastAPI
from pydantic import BaseModel
from openai import OpenAI, APIError
from dotenv import load_dotenv

# .envファイルからAPIキーを読み込む
load_dotenv()

app = FastAPI()

# OpenAIクライアントの初期化 (APIキーが必要です)
# ※APIキーがない場合はエラーになります
client = OpenAI(api_key=os.getenv("OPENAI_API_KEY"))


# リクエストの定義
class FamilyRequest(BaseModel):
    """
    ユーザーの入力「幅1000の机...」
    """

    text: str  # ユーザーの入力「幅1000の机...」


# レスポンスの定義 (C#側と合わせる)
class FamilyParameterResponse(BaseModel):
    """
    Revitファミリ作成用のパラメータ(JSON)
    """

    Width: float
    Depth: float
    Height: float
    Name: str


JSON_FILE_PATH = os.path.join(
    os.path.dirname(os.path.dirname(__file__)),
    "params.json",
)


@app.post("/generate_params", response_model=FamilyParameterResponse)
async def generate_params(req: FamilyRequest):
    """
    ユーザーのテキストからRevitファミリ作成用のパラメータ(JSON)を生成し、ファイルに保存してからレスポンスを返す
    """
    try:
        # AIへの指示（プロンプト）
        system_prompt = """
        あなたはRevitのファミリパラメータ設定アシスタントです。
        ユーザーの要望から家具の寸法を推測し、以下のJSON形式で出力してください。
        単位はミリメートル(mm)です。

        出力フォーマット:
        {
            "Width": <数値>,
            "Depth": <数値>,
            "Height": <数値>,
            "Name": "<短いファイル名>"
        }

        もしユーザーが具体的な寸法を指定しない場合は、一般的な家具のサイズを補完してください。
        余計な説明は不要です。JSONのみを返してください。
        """

        response = client.chat.completions.create(
            model="gpt-4o",
            messages=[
                {"role": "system", "content": system_prompt},
                {"role": "user", "content": req.text},
            ],
            response_format={"type": "json_object"},
        )

        # AIの返答を解析してパラメータを取得
        content = response.choices[0].message.content
        if content is None:
            raise ValueError("AIの返答が空です")
        data = json.loads(content)

        with open(JSON_FILE_PATH, "w", encoding="utf-8") as f:
            json.dump(data, f, indent=4)
            print(f"パラメータをファイルに保存しました: {JSON_FILE_PATH}")

        return data

    except (ValueError, json.JSONDecodeError, APIError) as e:
        print(f"Error: {e}")
        # エラー時のフォールバック
        fallback_data = {
          "Width": 1000.0,
          "Depth": 500.0,
          "Height": 700.0,
          "Name": "Error_Fallback",
        }
        with open(JSON_FILE_PATH, "w", encoding="utf-8") as f:
            json.dump(fallback_data, f, indent=4)
            print(f"パラメータをファイルに保存しました: {JSON_FILE_PATH}")
        return fallback_data


@app.get("/")
def read_root():
    """
    APIのルートエンドポイント
    """
    return {"message": "Revit AI Server is running!"}
