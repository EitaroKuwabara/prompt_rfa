# web_api/schemas.py
"""
データ定義
"""
from typing import Optional
from pydantic import BaseModel


class PromptRequest(BaseModel):
    """
    フロントエンドからのリクエスト用
    """
    prompt: str
    category: str = "shelf"


class ShelfParams(BaseModel):
    """
    棚(shelf)用のパラメータ定義
    """
    width: float
    depth: float
    height: float

    topThickness: float = 30.0
    sideThickness: float = 30.0
    shelfThickness: float = 20.0

    topMaterialName: Optional[str] = "Wood"
    sideMaterialName: Optional[str] = "Wood"
    shelfMaterialName: Optional[str] = "Wood"

    shelfCount: int = 3


class DeskParams(BaseModel):
    """
    机用のパラメータ定義
    """
    width: float
    depth: float
    height: float
    topThickness: float = 30.0
    legWidth: float = 50.0
    topMaterialName: str = "Wood"
    legMaterialName: str = "Steel"
    hasDrawers: bool = False


class GenerateRequest(BaseModel):
    """
    生成リクエスト全体
    """
    command: str = "create"
    type: str  # "Shelf", "Desk"

    # 実際にはここで各パラメータを受け取るが、
    # シンプルにするため dict で受け取り、ロジック内で変換・検証する手もある
    # ここでは展開して定義します
    params: dict
