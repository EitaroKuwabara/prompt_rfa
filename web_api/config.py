# web_api/config.py
"""
設定ファイル
"""
import os
from dotenv import load_dotenv

load_dotenv()

# APIキー
OPENAI_API_KEY = os.getenv("OPENAI_API_KEY")

# パス設定
BASE_DIR = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))  # prompt_rfa/
JSON_DIR = os.path.join(BASE_DIR, "archifields")
JSON_PATH = os.path.join(JSON_DIR, "components.json")
OUTPUT_DIR = os.path.join(BASE_DIR, "PromptRFA", "OutputFamilies")

# フォルダ作成
if not os.path.exists(JSON_DIR):
    os.makedirs(JSON_DIR)
