using System;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace PromptRFA
{
    [Transaction(TransactionMode.Manual)]
    public class Command : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                // Webから更新された params.json を読み込んで実行
                FamilyProcessor processor = new FamilyProcessor();
                processor.Run(commandData.Application.Application);

                // 完了メッセージ
                TaskDialog.Show("Archifields", "ファミリの生成が完了しました！\nWebブラウザに戻ってダウンロードしてください。");
                
                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return Result.Failed;
            }
        }
    }
}