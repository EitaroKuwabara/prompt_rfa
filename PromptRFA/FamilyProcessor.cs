using System.IO;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PromptRFA.Creating;
using PromptRFA.Models;

namespace PromptRFA
{
    public class FamilyProcessor
    {
        // Command.cs から呼ばれるメインメソッド
        public void Run(
            Autodesk.Revit.ApplicationServices.Application app
        )
        {
            // 1. ファイルパスの設定
            string projectRoot =
                @"C:\Users\81805\StudioProjects\prompt_rfa";
            string jsonPath = Path.Combine(
                projectRoot,
                @"archifields\components.json"
            );
            string outputFolder = Path.Combine(
                projectRoot,
                @"PromptRFA\OutputFamilies"
            );
            string templatePath =
                @"C:\ProgramData\Autodesk\RVT 2025\Family Templates\English\Metric Generic Model.rft";

            // フォルダ作成
            if (!Directory.Exists(outputFolder))
                Directory.CreateDirectory(outputFolder);

            // テンプレート確認
            if (!File.Exists(templatePath))
            {
                throw new FileNotFoundException(
                    "ファミリテンプレートが見つかりません: "
                        + templatePath
                );
            }

            // 2. ここで先にJSONを読み込んで、ファイル名(targetFileName)を決定する
            string targetFileName = "GeneratedShelf"; // デフォルト値
            if (File.Exists(jsonPath))
            {
                try
                {
                    string jsonContent = File.ReadAllText(
                        jsonPath
                    );
                    RootObject? root =
                        JsonConvert.DeserializeObject<RootObject>(
                            jsonContent
                        );

                    // JSONに名前があればそれを使う
                    if (
                        root != null
                        && root.parameters != null
                        && !string.IsNullOrEmpty(
                            root.parameters.familyName
                        )
                    )
                    {
                        targetFileName =
                            root.parameters.familyName;
                    }
                }
                catch
                {
                    // 読み込み失敗時はデフォルト名のまま進める
                }
            }

            TaskDialog.Show(
                "Debug Check",
                "読み込んだファイル名: " + targetFileName
            );

            // 3. 新規ファミリドキュメント作成
            Document familyDoc = app.NewFamilyDocument(
                templatePath
            );

            try
            {
                // 4. ファミリ生成ロジックを実行
                CreateFamilyFromJSON(familyDoc, jsonPath);

                // 5. 決定したファイル名で保存
                string outputPath = Path.Combine(
                    outputFolder,
                    targetFileName + ".rfa"
                );

                SaveAsOptions opt = new SaveAsOptions
                {
                    OverwriteExistingFile = true,
                };
                familyDoc.SaveAs(outputPath, opt);

                // 6. 同じ名前でプレビュー画像も保存
                ExportPreviewImage(
                    familyDoc,
                    outputFolder,
                    targetFileName
                );

                // 閉じる
                familyDoc.Close(false);
            }
            catch (Exception)
            {
                familyDoc.Close(false);
                throw;
            }
        }

        // プレビュー画像のエクスポート
        private void ExportPreviewImage(
            Document doc,
            string folder,
            string baseName
        )
        {
            try
            {
                View3D? view = new FilteredElementCollector(
                    doc
                )
                    .OfClass(typeof(View3D))
                    .Cast<View3D>()
                    .FirstOrDefault(v => !v.IsTemplate);

                if (view != null)
                {
                    var imgOpt = new ImageExportOptions
                    {
                        ZoomType = ZoomFitType.FitToPage,
                        PixelSize = 1024,
                        FilePath = Path.Combine(
                            folder,
                            baseName
                        ),
                        FitDirection =
                            FitDirectionType.Horizontal,
                        HLRandWFViewsFileType =
                            ImageFileType.PNG,
                        ImageResolution =
                            ImageResolution.DPI_150,
                        ExportRange =
                            ExportRange.SetOfViews,
                    };
                    imgOpt.SetViewsAndSheets(
                        new List<ElementId> { view.Id }
                    );
                    doc.ExportImage(imgOpt);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    "画像エクスポート失敗: " + ex.Message
                );
            }
        }

        // 実際の生成処理
        public void CreateFamilyFromJSON(
            Document doc,
            string jsonPath
        )
        {
            if (!File.Exists(jsonPath))
                return;
            string jsonContent = File.ReadAllText(jsonPath);

            RootObject? root =
                JsonConvert.DeserializeObject<RootObject>(
                    jsonContent
                );
            if (root == null || root.parameters == null)
                return;

            string? type = root.parameters.type;
            if (string.IsNullOrEmpty(type))
                return;

            IFamilyCreator? creator = null;

            switch (type.ToLower())
            {
                case "shelf":
                    creator = new ShelfCreator();
                    break;
                case "desk":
                    creator = new DeskCreator();
                    break;
            }

            if (
                creator != null
                && root.parameters.specs
                    is JObject specsJson
            )
            {
                creator.Execute(doc, specsJson);
            }
        }
    }
}
