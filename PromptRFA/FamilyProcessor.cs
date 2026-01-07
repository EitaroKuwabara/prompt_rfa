using System.IO;
using System.Reflection;
using Autodesk.Revit.DB;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PromptRFA.Creating;
using PromptRFA.Models;

namespace PromptRFA
{
    public class FamilyProcessor
    {
        public void Run(
            Autodesk.Revit.ApplicationServices.Application app
        )
        {
            Console.WriteLine(
                "--- Start Family Processing (Cloud) ---"
            );

            // 1. パス設定
            // JSONは「入力ファイル」としてクラウドのカレントディレクトリに置かれます
            string jsonPath = "components.json";

            // テンプレートは「AppBundle(ZIP)」に入れたものを探します（DLLと同じ場所）
            string? assemblyPath = Path.GetDirectoryName(
                Assembly.GetExecutingAssembly().Location
            );
            string templatePath = Path.Combine(
                assemblyPath!,
                "Metric Generic Model.rft"
            );

            // string targetFileName = "output";

            // 2. JSON読み込み
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
                    if (
                        root != null
                        && root.parameters != null
                        && !string.IsNullOrEmpty(
                            root.parameters.familyName
                        )
                    )
                    {
                        // 生成時のファイル名としては使うが、保存は output.rfa 固定にする
                        Console.WriteLine(
                            $"Parameter FamilyName: {root.parameters.familyName}"
                        );
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(
                        $"JSON Read Error: {ex.Message}"
                    );
                }
            }
            else
            {
                Console.WriteLine(
                    "Error: components.json not found in current directory."
                );
                return;
            }

            // 3. テンプレート確認
            if (!File.Exists(templatePath))
            {
                Console.WriteLine(
                    $"Error: Template not found at {templatePath}"
                );
                // 万が一見つからない場合、カレントディレクトリも探してみる
                templatePath = "Metric Generic Model.rft";
                if (!File.Exists(templatePath))
                {
                    Console.WriteLine(
                        "Error: Template not found in current directory either."
                    );
                    return;
                }
            }

            // 4. 新規ファミリ作成
            Document familyDoc = app.NewFamilyDocument(
                templatePath
            );

            try
            {
                CreateFamilyFromJSON(familyDoc, jsonPath);

                // 5. 保存 (クラウドが指定する "output.rfa" という名前で保存必須)
                // ※setup_aps.py で output.rfa を持ち帰る設定にしているため
                string rfaName = "output.rfa";

                SaveAsOptions opt = new SaveAsOptions
                {
                    OverwriteExistingFile = true,
                    Compact = true,
                };
                familyDoc.SaveAs(rfaName, opt);
                Console.WriteLine($"Saved RFA: {rfaName}");

                // 6. 画像 (オプション)
                ExportPreviewImage(
                    familyDoc,
                    "preview.png"
                );

                familyDoc.Close(false);
                Console.WriteLine(
                    "--- Finished Successfully ---"
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"Critical Error: {ex.Message}"
                );
                familyDoc.Close(false);
                throw;
            }
        }

        private void ExportPreviewImage(
            Document doc,
            string filePath
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
                        FilePath = filePath,
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
            catch
            { /* 無視 */
            }
        }

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
            JObject? specsJson =
                root.parameters.specs as JObject;
            IFamilyCreator? creator = null;

            if (type?.ToLower() == "shelf")
                creator = new ShelfCreator();
            else if (type?.ToLower() == "desk")
                creator = new DeskCreator();

            if (creator != null && specsJson != null)
            {
                creator.Execute(doc, specsJson);
            }
        }
    }
}
