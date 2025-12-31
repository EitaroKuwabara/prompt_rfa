# web_api/logic/base.py
"""
家具ロジックの基底クラス
"""
from abc import ABC, abstractmethod


class BaseFurnitureLogic(ABC):
    """
    全ての家具ロジックの基底クラス
    """

    @abstractmethod
    def get_system_instruction(self) -> str:
        """AIへの指示書(プロンプト)を返す"""
        raise NotImplementedError("Subclasses must implement this method")

    @abstractmethod
    def format_for_revit(self, params: dict) -> dict:
        """WebからのデータをRevit(C#)が読めるJSON構造に変換する"""
        raise NotImplementedError("Subclasses must implement this method")
