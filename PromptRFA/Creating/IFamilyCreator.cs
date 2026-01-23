using Autodesk.Revit.DB;
using Newtonsoft.Json.Linq; // JSON解析用

namespace PromptRFA.Creating
{
    public interface IFamilyCreator
    {
        // 実行メソッド
        // doc: ファミリドキュメント
        // specsJson: JSONの "specs" 部分 (JObject)
        void Execute(Document doc, JObject specsJson);
    }
}
