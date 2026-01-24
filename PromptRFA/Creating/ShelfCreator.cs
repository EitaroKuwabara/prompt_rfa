// PromptRFA/Creating/ShelfCreator.cs
using Autodesk.Revit.DB;
using Newtonsoft.Json.Linq;
using PromptRFA.Models;
using PromptRFA.Utils;

namespace PromptRFA.Creating
{
    public class ShelfCreator : IFamilyCreator
    {
        public void Execute(Document doc, JObject specsJson)
        {
            DebugLogger.Show("Debug", "棚作成開始");
            ShelfSpecs? specs =
                specsJson.ToObject<ShelfSpecs>();
            FamilyManager famMgr = doc.FamilyManager;
            using Transaction t = new(
                doc,
                "Create Advanced Shelf"
            );
            {
                t.Start();
                try
                {
                    famMgr.NewType("Standard");
                }
                catch
                {
                    // 既にタイプがある場合は無視してOK
                }

                // 寸法パラメータを作成
                var pW = FamilyParameterUtils.EnsureParam(
                    doc,
                    "Width",
                    GroupTypeId.Geometry,
                    SpecTypeId.Length
                );
                var pD = FamilyParameterUtils.EnsureParam(
                    doc,
                    "Depth",
                    GroupTypeId.Geometry,
                    SpecTypeId.Length
                );
                var pH = FamilyParameterUtils.EnsureParam(
                    doc,
                    "Height",
                    GroupTypeId.Geometry,
                    SpecTypeId.Length
                );
                // 棚独自の板厚パラメータを作成
                var pThkTop =
                    FamilyParameterUtils.EnsureParam(
                        doc,
                        "Top Thickness",
                        GroupTypeId.Geometry,
                        SpecTypeId.Length
                    );
                var pThkSide =
                    FamilyParameterUtils.EnsureParam(
                        doc,
                        "Side Thickness",
                        GroupTypeId.Geometry,
                        SpecTypeId.Length
                    );
                var pThkShelf =
                    FamilyParameterUtils.EnsureParam(
                        doc,
                        "Shelf Thickness",
                        GroupTypeId.Geometry,
                        SpecTypeId.Length
                    );
                var pTopThk =
                    FamilyParameterUtils.EnsureParam(
                        doc,
                        "Top Thickness",
                        GroupTypeId.Geometry,
                        SpecTypeId.Length
                    );
                // マテリアルパラメータを作成
                var pTopMat =
                    FamilyParameterUtils.EnsureMaterialParam(
                        doc,
                        "Top Material"
                    );
                var pSideMat = EnsureMaterialParam(
                    doc,
                    "Side Material"
                );
                var pShelfMat = EnsureMaterialParam(
                    doc,
                    "Shelf Material"
                );

                // パラメータに初期値をセット（mm→feet変換）
                famMgr.Set(pW, specs!.Width / 304.8);
                famMgr.Set(pD, specs!.Depth / 304.8);
                famMgr.Set(pH, specs!.Height / 304.8);

                famMgr.Set(
                    pThkTop,
                    specs.TopThickness / 304.8
                );
                famMgr.Set(
                    pThkSide,
                    specs.SideThickness / 304.8
                );
                famMgr.Set(
                    pThkShelf,
                    specs.ShelfThickness / 304.8
                );

                // 寸法変換
                double width = specs!.Width / 304.8;
                double depth = specs.Depth / 304.8;
                double height = specs.Height / 304.8;
                double thkTop = specs.TopThickness / 304.8;
                double thkSide =
                    specs.SideThickness / 304.8;
                double thkShelf =
                    specs.ShelfThickness / 304.8;

                // 2. ジオメトリ作成と紐付け

                // --- 天板 (Top) ---
                Extrusion? topSolid = CreateBox(
                    doc,
                    width,
                    depth,
                    thkTop,
                    0,
                    0,
                    height - thkTop
                );
                // ★ここで紐付け: 「このソリッドのマテリアル」は「Top Materialパラメータ」に従う
                famMgr.AssociateElementParameterToFamilyParameter(
                    topSolid!.get_Parameter(
                        BuiltInParameter.MATERIAL_ID_PARAM
                    ),
                    pTopMat
                );

                // --- 側板 (Sides) ---
                // 左
                Extrusion? leftSolid = CreateBox(
                    doc,
                    thkSide,
                    depth,
                    height - thkTop,
                    -width / 2 + thkSide / 2,
                    0,
                    0
                );
                famMgr.AssociateElementParameterToFamilyParameter(
                    leftSolid!.get_Parameter(
                        BuiltInParameter.MATERIAL_ID_PARAM
                    ),
                    pSideMat
                );

                // 右
                Extrusion? rightSolid = CreateBox(
                    doc,
                    thkSide,
                    depth,
                    height - thkTop,
                    width / 2 - thkSide / 2,
                    0,
                    0
                );
                famMgr.AssociateElementParameterToFamilyParameter(
                    rightSolid!.get_Parameter(
                        BuiltInParameter.MATERIAL_ID_PARAM
                    ),
                    pSideMat
                ); // 左右ともに同じ側板用パラメータを紐付ける

                // --- 棚板 (Shelves) ---
                double effectiveH = height - thkTop;
                double? spacing =
                    effectiveH / (specs.ShelfCount + 1);

                for (int i = 1; i <= specs.ShelfCount; i++)
                {
                    double? z = i * spacing;
                    Extrusion? shelfSolid = CreateBox(
                        doc,
                        width - (thkSide * 2),
                        depth,
                        thkShelf,
                        0,
                        0,
                        z!.Value
                    );

                    // 棚板用パラメータを紐付け
                    famMgr.AssociateElementParameterToFamilyParameter(
                        shelfSolid!.get_Parameter(
                            BuiltInParameter.MATERIAL_ID_PARAM
                        ),
                        pShelfMat
                    );
                }

                t.Commit();
            }
        }

        // マテリアルパラメータが存在するか確認し、なければ作るヘルパー
        private static FamilyParameter EnsureMaterialParam(
            Document doc,
            string paramName
        )
        {
            FamilyManager mgr = doc.FamilyManager;
            FamilyParameter param = mgr.get_Parameter(
                paramName
            );

            // 新規作成: マテリアル型のインスタンスパラメータとして作成
            // ※Revit 2025などのバージョンにより引数が異なる場合がありますが、基本は以下
            param ??= mgr.AddParameter(
                paramName,
                GroupTypeId.Materials,
                SpecTypeId.Reference.Material,
                false
            );

            return param;
        }

        // Extrusion (実体) を返すように変更
        private static Extrusion? CreateBox(
            Document doc,
            double w,
            double d,
            double h,
            double cx,
            double cy,
            double bz
        )
        {
            CurveArray curveArray = new();
            XYZ p1 = new(cx - w / 2, cy - d / 2, bz);
            XYZ p2 = new(cx + w / 2, cy - d / 2, bz);
            XYZ p3 = new(cx + w / 2, cy + d / 2, bz);
            XYZ p4 = new(cx - w / 2, cy + d / 2, bz);

            curveArray.Append(Line.CreateBound(p1, p2));
            curveArray.Append(Line.CreateBound(p2, p3));
            curveArray.Append(Line.CreateBound(p3, p4));
            curveArray.Append(Line.CreateBound(p4, p1));

            CurveArrArray curveArrArray = new();
            curveArrArray.Append(curveArray);

            if (doc.IsFamilyDocument)
            {
                Plane plane = Plane.CreateByNormalAndOrigin(
                    XYZ.BasisZ,
                    new XYZ(0, 0, bz)
                );
                SketchPlane sketchPlane =
                    SketchPlane.Create(doc, plane);
                return doc.FamilyCreate.NewExtrusion(
                    true,
                    curveArrArray,
                    sketchPlane,
                    h
                );
            }
            return null;
        }
    }
}
