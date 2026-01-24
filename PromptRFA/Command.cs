// PromptRFA/Command.cs
using System.IO;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Newtonsoft.Json.Linq; // JObject用
using PromptRFA.Creating; // DeskCreator用
using PromptRFA.Utils; // DebugLogger用

namespace PromptRFA
{
    [Transaction(TransactionMode.Manual)]
    public class Command : IExternalCommand
    {
        // ★ここを切り替えるだけでモード変更できます！
        // true  = ローカルテストモード (Revitだけで完結。机の形を調整する時に使う)
        // false = 本番モード (ブラウザと連携するいつもの動き)
        private readonly bool IS_DEBUG_MODE = false;

        public Result Execute(
            ExternalCommandData commandData,
            ref string message,
            ElementSet elements
        )
        {
            // ロガーのスイッチを設定
            DebugLogger.IsEnabled = IS_DEBUG_MODE;
            try
            {
                // ==========================================
                // モードA: ローカルテスト (開発用)
                // ==========================================
                if (IS_DEBUG_MODE)
                {
                    return RunDebugMode(commandData);
                }
                // ==========================================
                // モードB: 本番連携 (ブラウザ連携用)
                // ==========================================
                else
                {
                    // 本番モード:Webからのデータを処理して生成
                    var processor = new FamilyProcessor();
                    // FamilyProcessor processor =
                    //     new FamilyProcessor();
                    processor.Run(
                        commandData.Application.Application
                    );

                    TaskDialog.Show(
                        "Archifields",
                        "ファミリの生成が完了しました！\nWebブラウザに戻ってダウンロードしてください。"
                    );
                    return Result.Succeeded;
                }
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return Result.Failed;
            }
        }

        // ローカルテストの処理
        private static Result RunDebugMode(
            ExternalCommandData commandData
        )
        {
            // テスト用のダミーデータ
            var specs = new JObject
            {
                // デスクのデータ
                // ["Width"] = 1200.0,
                // ["Depth"] = 700.0,
                // ["Height"] = 700.0,
                // ["TopThickness"] = 30.0,
                // ["TopMaterialName"] = "Glass",
                // ["LegWidth"] = 50.0,

                // 棚のデータ
                ["Width"] = 900.0, // 幅
                ["Depth"] = 450.0, // 奥行
                ["Height"] = 1800.0, // 高さ
                ["TopThickness"] = 25.0, // 天板厚
                ["SideThickness"] = 25.0, // 側板厚 (追加)
                ["ShelfThickness"] = 20.0, // 棚板厚 (追加)
                ["ShelfCount"] = 4, // 棚板の枚数 (追加)

                // マテリアル名
                ["TopMaterialName"] = "Wood",
                ["SideMaterialName"] = "Wood",
                ["ShelfMaterialName"] = "Wood",
            };

            // テンプレートを探す
            string tmplPath = GetTemplatePath();
            if (string.IsNullOrEmpty(tmplPath))
            {
                TaskDialog.Show(
                    "Error",
                    "テンプレートが見つかりませんでした。手動で選択してください。"
                );
                FileOpenDialog dialog = new("rft");
                if (
                    dialog.Show()
                    == ItemSelectionDialogResult.Confirmed
                )
                    tmplPath =
                        ModelPathUtils.ConvertModelPathToUserVisiblePath(
                            dialog.GetSelectedModelPath()
                        );
                else
                    return Result.Cancelled;
            }

            // メモリ上で作成して保存して開く
            var app = commandData.Application.Application;
            var doc = app.NewFamilyDocument(tmplPath);

            // Creatorを実行！
            // 机の場合
            // new DeskCreator().Execute(doc, specs);
            // 棚の場合
            new ShelfCreator().Execute(doc, specs);

            // 一時ファイルに保存して表示
            string tempPath = Path.Combine(
                Path.GetTempPath(),
                $"DebugDesk_{DateTime.Now:mmss}.rfa"
            );
            SaveAsOptions saveOpts = new SaveAsOptions
            {
                OverwriteExistingFile = true,
            };
            doc.SaveAs(tempPath, saveOpts);
            doc.Close(false);

            commandData.Application.OpenAndActivateDocument(
                tempPath
            );

            return Result.Succeeded;
        }

        // テンプレートパスを探すヘルパー
        private static string GetTemplatePath()
        {
            string[] possiblePaths =
            [
                @"C:\ProgramData\Autodesk\RVT 2025\Family Templates\Japanese\一般モデル(メートル単位).rft",
                @"C:\ProgramData\Autodesk\RVT 2024\Family Templates\Japanese\一般モデル(メートル単位).rft",
                @"C:\ProgramData\Autodesk\RVT 2025\Family Templates\English\Metric Generic Model.rft",
                @"C:\ProgramData\Autodesk\RVT 2024\Family Templates\English\Metric Generic Model.rft",
            ];

            foreach (var path in possiblePaths)
            {
                if (File.Exists(path))
                    return path;
            }
            return "";
        }
    }
}
