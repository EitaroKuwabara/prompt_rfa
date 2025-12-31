# web_api/logic/shelf.py
"""
棚(shelf)用のロジック
"""
from .base import BaseFurnitureLogic


class ShelfLogic(BaseFurnitureLogic):
    """
    棚(shelf)用のロジック
    """
    def get_system_instruction(self) -> str:
        """
        AIへの指示書(プロンプト)を返す
        """
        return """
        あなたはBIMと家具デザインの専門家です。
        ユーザーの要望に基づいて、収納棚の最適な仕様を推論してください。
        
        # 制約
        - 単位はすべてミリメートル (mm)
        - 一般的な家具寸法を考慮すること

        # 出力フォーマット (JSON)
        キーは camelCase で出力してください。
        "suggestedName" というキーで、この家具を表す短い英単語(PascalCase)を含めてください。
        例: "EncyclopediaShelf", "LowWideStorage", "GlassDisplay"
        {
            "suggestedName": (string),
            "width": (float),
            "depth": (float),
            "height": (float),
            "topThickness": (float, default around 25),
            "sideThickness": (float, default around 20-30),
            "shelfThickness": (float, default around 18-20),
            "topMaterialName": (string, ex: "Wood", "Glass", "Metal"),
            "sideMaterialName": (string),
            "shelfMaterialName": (string),
            "shelfCount": (int)
        }
        """

    def format_for_revit(self, params: dict) -> dict:
        # Pydanticモデルなどを経由して型安全にしても良いが、
        # ここでは辞書のキーを PascalCase (C#用) に変換する
        return {
            "Width": params.get("width"),
            "Depth": params.get("depth"),
            "Height": params.get("height"),
            "TopThickness": params.get("topThickness"),
            "SideThickness": params.get("sideThickness"),
            "ShelfThickness": params.get("shelfThickness"),
            "TopMaterialName": params.get("topMaterialName"),
            "SideMaterialName": params.get("sideMaterialName"),
            "ShelfMaterialName": params.get("shelfMaterialName"),
            "ShelfCount": params.get("shelfCount"),
        }
