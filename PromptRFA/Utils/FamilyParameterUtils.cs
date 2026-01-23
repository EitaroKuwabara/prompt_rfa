// Utils/FamilyParameterUtils.cs
// パラメータ作成、マテリアル設定

using System.Windows.Markup;
using Autodesk.Revit.DB;

namespace PromptRFA.Utils
{
    public static class FamilyParameterUtils
    {
        // <summary>
        // パラメータを取得、無ければ作成する
        // </summary>
        public static FamilyParameter EnsureParam(
            Document doc,
            string name,
            ForgeTypeId group,
            ForgeTypeId spec
        )
        {
            FamilyParameter p =
                doc.FamilyManager.get_Parameter(name);
            if (p == null)
            {
                try
                {
                    p = doc.FamilyManager.AddParameter(
                        name,
                        group,
                        spec,
                        false
                    );
                }
                catch
                {
                    // エラーは無視
                }
            }
            return p!;
        }

        // <summary>
        // マテリアルパラメータを取得、無ければ作成する
        // </summary>
        public static FamilyParameter EnsureMaterialParam(
            Document doc,
            string name
        )
        {
            FamilyParameter p =
                doc.FamilyManager.get_Parameter(name);
            p ??= doc.FamilyManager.AddParameter(
                name,
                GroupTypeId.Materials,
                SpecTypeId.Reference.Material,
                false
            );
            return p!;
        }
    }
}
