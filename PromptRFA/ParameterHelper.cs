using System;
using System.Linq;
using Autodesk.Revit.DB;

namespace PromptRFA
{
    public static class ParameterHelper
    {
        // 長さパラメータの作成 (変更なし)
        public static FamilyParameter GetOrCreateLengthParam(Document doc, string name)
        {
            FamilyManager mgr = doc.FamilyManager;
            FamilyParameter param = mgr.get_Parameter(name);
            if (param == null)
            {
                // Revit 2022以降対応 (ForgeTypeId)
                param = mgr.AddParameter(name, GroupTypeId.Geometry, SpecTypeId.Length, false);
            }
            return param;
        }

        public static FamilyParameter GetOrCreateMaterialParam(Document doc, string paramName, string matName, Color color, int transparency = 0)
        {
            FamilyManager mgr = doc.FamilyManager;

            // 1. マテリアルを探す、なければ作る
            Material? mat = new FilteredElementCollector(doc)
                .OfClass(typeof(Material))
                .Cast<Material>()
                .FirstOrDefault(m => m.Name.Equals(matName));

            if (mat == null)
            {
                ElementId matId = Material.Create(doc, matName);
                mat = doc.GetElement(matId) as Material;
            }

            // 2. 色と透明度を設定
            if (mat != null)
            {
                mat.Color = color;
                mat.Transparency = transparency; // 0(不透明) 〜 100(透明)
            }

            // 3. ファミリパラメータを作成
            FamilyParameter param = mgr.get_Parameter(paramName);
            if (param == null)
            {
                param = mgr.AddParameter(
                    paramName,
                    GroupTypeId.Materials,
                    SpecTypeId.Reference.Material,
                    false
                );
            }

            // 4. パラメータにマテリアルを割り当て
            if (mat != null)
            {
                mgr.Set(param, mat.Id);
            }

            return param;
        }
    }
}