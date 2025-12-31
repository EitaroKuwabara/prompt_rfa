# web_api/logic/desk.py
"""
机(desk)用のロジック
"""
from .base import BaseFurnitureLogic


class DeskLogic(BaseFurnitureLogic):
    """
    机(desk)用のロジック
    """

    def get_system_instruction(self) -> str:
        """
        AIへの指示書(プロンプト)を返す
        """
        return """
        あなたはBIMと家具デザインの専門家です。
        ユーザーの要望に基づいて、机（デスク）の最適な仕様を推論してください。
        
        # 制約
        - 単位はすべてミリメートル (mm)
        - 一般的な家具寸法を考慮すること (例: 机の高さは700mm前後が一般的)

        # 出力フォーマット (JSON)
        キーは camelCase で出力してください。
        
        ★必須: "suggestedName" (PascalCase) を含めてください。
        
        {
            "suggestedName": (string, ex: "ModernOfficeDesk", "SimpleStudyTable"),
            "width": (float),
            "depth": (float),
            "height": (float),
            
            "topThickness": (float, default around 25-30),
            "legWidth": (float, default around 40-60),
            
            "topMaterialName": (string, ex: "Wood", "Glass", "Plastic"),
            "legMaterialName": (string, ex: "Steel", "Wood"),
            
            "hasDrawers": (boolean, true or false)
        }
        """

    def format_for_revit(self, params: dict) -> dict:
        """
        WebからのデータをRevit(C#)の DeskSpecs が読めるPascalCase構造に変換
        """
        return {
            "Width": params.get("width"),
            "Depth": params.get("depth"),
            "Height": params.get("height"),
            "TopThickness": params.get("topThickness"),
            "LegWidth": params.get("legWidth"),
            "TopMaterialName": params.get("topMaterialName"),
            "LegMaterialName": params.get("legMaterialName"),
            "HasDrawers": params.get("hasDrawers"),
        }
