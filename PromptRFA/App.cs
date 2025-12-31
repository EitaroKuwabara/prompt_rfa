using System;
using System.Reflection;
using Autodesk.Revit.UI;

namespace PromptRFA
{
    // IExternalDBApplication ではなく IExternalApplication に変更
    public class App : IExternalApplication
    {
        public Result OnStartup(UIControlledApplication application)
        {
            // ロガー初期化
            Logger.Initialize();
            Logger.Write("OnStartup: UIのロードを開始します。");

            // 1. リボンタブの作成 ("PromptRFA" というタブが追加されます)
            string tabName = "PromptRFA";
            try
            {
                application.CreateRibbonTab(tabName);
            }
            catch { /* 既にタブがある場合は無視 */ }

            // 2. パネルの作成
            RibbonPanel panel = application.CreateRibbonPanel(tabName, "AI Generation");

            // 3. ボタンの作成
            // このDLLファイルのパスを取得
            string assemblyPath = Assembly.GetExecutingAssembly().Location;

            // ボタンの設定 (表示名, コマンドのクラス名)
            PushButtonData buttonData = new PushButtonData(
                "btnGenerate",       // 内部ID
                "Generate\nFurniture", // ボタンに表示されるテキスト
                assemblyPath,        // DLLのパス
                "PromptRFA.Command"  // 実行するクラス名 (Command.csのクラス)
            );

            // ツールチップ（マウスオーバー時の説明）
            buttonData.ToolTip = "params.jsonを読み込んで家具ファミリを生成します。";

            // パネルにボタンを追加
            panel.AddItem(buttonData);

            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }
    }
}