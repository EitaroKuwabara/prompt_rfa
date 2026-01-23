// PromptRFA/Creating/DeskCreator.cs

using Autodesk.Revit.DB;
using Newtonsoft.Json.Linq;
using PromptRFA.Models;
using PromptRFA.Utils;

namespace PromptRFA.Creating
{
    public class DeskCreator : IFamilyCreator
    {
        public void Execute(Document doc, JObject specsJson)
        {
            DebugLogger.Show("Debug", "開始");
            DeskSpecs specs =
                specsJson.ToObject<DeskSpecs>()!;
            FamilyManager famMgr = doc.FamilyManager;

            using Transaction t = new(
                doc,
                "Create Parametric Desk"
            );
            {
                t.Start();

                try
                {
                    famMgr.NewType("Standard");
                }
                catch { }

                // パラメータ準備
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

                var pTopThk =
                    FamilyParameterUtils.EnsureParam(
                        doc,
                        "Top Thickness",
                        GroupTypeId.Geometry,
                        SpecTypeId.Length
                    );

                var pUnderside =
                    FamilyParameterUtils.EnsureParam(
                        doc,
                        "Under Side Height",
                        GroupTypeId.Geometry,
                        SpecTypeId.Length
                    );

                var pLegW =
                    FamilyParameterUtils.EnsureParam(
                        doc,
                        "Leg Width",
                        GroupTypeId.Geometry,
                        SpecTypeId.Length
                    );

                var pMatTop =
                    FamilyParameterUtils.EnsureMaterialParam(
                        doc,
                        "Top Material"
                    );

                var pMatLeg =
                    FamilyParameterUtils.EnsureMaterialParam(
                        doc,
                        "Leg Material"
                    );

                // 初期値セット
                famMgr.Set(pW, specs.Width / 304.8);
                famMgr.Set(pD, specs.Depth / 304.8);
                famMgr.Set(pH, specs.Height / 304.8);
                famMgr.Set(
                    pTopThk,
                    specs.TopThickness / 304.8
                );
                famMgr.Set(
                    pLegW,
                    (
                        specs.LegWidth > 0
                            ? specs.LegWidth
                            : 50.0
                    ) / 304.8
                );
                famMgr.SetFormula(
                    pUnderside,
                    "Height - Top Thickness"
                );

                DebugLogger.Show(
                    "Debug",
                    "パラメータ設定完了 -> 天板作成へ"
                );

                // パラメトリック天板の作成
                CreateTop(
                    doc,
                    pW,
                    pD,
                    pH,
                    pUnderside,
                    pMatTop,
                    specs
                );

                DebugLogger.Show(
                    "Debug",
                    "天板作成完了 -> 脚作成へ"
                );

                // 脚作成
                CreateLegs(
                    doc,
                    pLegW,
                    pUnderside,
                    pMatLeg,
                    specs
                );

                DebugLogger.Show(
                    "Debug",
                    "脚作成完了 -> コミットへ"
                );

                t.Commit();
            }
        }

        // 天板作成ロジック
        private static void CreateTop(
            Document doc,
            FamilyParameter pW,
            FamilyParameter pD,
            FamilyParameter pH,
            FamilyParameter pUnderside,
            FamilyParameter pMat,
            DeskSpecs specs
        )
        {
            DebugLogger.Show(
                "Debug",
                "天板 - 参照面作成開始"
            );
            var centerLR =
                FamilyGeometryUtils.GetReferencePlane(
                    doc,
                    ReferencePlaneReference.CenterLeftRight
                );
            var centerFB =
                FamilyGeometryUtils.GetReferencePlane(
                    doc,
                    ReferencePlaneReference.CenterFrontBack
                );
            var view = FamilyGeometryUtils.FindPlanView(
                doc
            )!;
            double hW = specs.Width / 304.8 / 2.0;
            double hD = specs.Depth / 304.8 / 2.0;
            // 参照面
            var refL = FamilyGeometryUtils.CreateRefPlane(
                doc,
                view,
                new XYZ(-hW, 0, 0),
                new XYZ(-hW, 1, 0),
                "Left",
                centerLR
            );
            var refR = FamilyGeometryUtils.CreateRefPlane(
                doc,
                view,
                new XYZ(hW, 0, 0),
                new XYZ(hW, 1, 0),
                "Right",
                centerLR
            );
            var refF = FamilyGeometryUtils.CreateRefPlane(
                doc,
                view,
                new XYZ(0, -hD, 0),
                new XYZ(1, -hD, 0),
                "Front",
                centerFB
            );
            var refB = FamilyGeometryUtils.CreateRefPlane(
                doc,
                view,
                new XYZ(0, hD, 0),
                new XYZ(1, hD, 0),
                "Back",
                centerFB
            );
            // 寸法
            DebugLogger.Show(
                "Debug",
                "天板 - 寸法作成開始"
            );
            FamilyDimensionUtils.AddDimensions(
                doc,
                view,
                refL,
                centerLR,
                refR,
                pW
            );
            FamilyDimensionUtils.AddDimensions(
                doc,
                view,
                refF,
                centerFB,
                refB,
                pD
            );
            // 形状
            DebugLogger.Show(
                "Debug",
                "天板 - 形状(Extrusion)作成開始"
            );
            CurveArrArray profile = CreateRect(
                -hW,
                -hD,
                hW,
                hD
            );
            SketchPlane sp = SketchPlane.Create(
                doc,
                Plane.CreateByNormalAndOrigin(
                    XYZ.BasisZ,
                    XYZ.Zero
                )
            );
            Extrusion top = doc.FamilyCreate.NewExtrusion(
                true,
                profile,
                sp,
                1.0
            );
            // パラメータ
            DebugLogger.Show(
                "Debug",
                "天板 - パラメータ紐付け開始"
            );
            // 上端
            doc.FamilyManager.AssociateElementParameterToFamilyParameter(
                top.get_Parameter(
                    BuiltInParameter.EXTRUSION_END_PARAM
                ),
                pH
            );
            // 下端
            doc.FamilyManager.AssociateElementParameterToFamilyParameter(
                top.get_Parameter(
                    BuiltInParameter.EXTRUSION_START_PARAM
                ),
                pUnderside
            );
            doc.Regenerate();

            DebugLogger.Show("Debug", "天板 - Align開始");

            FamilyGeometryUtils.AlignFaces(
                doc,
                top,
                [refL, refR, refF, refB]
            );
        }

        private static void CreateLegs(
            Document doc,
            FamilyParameter pLegW,
            FamilyParameter pUnderside,
            FamilyParameter pMat,
            DeskSpecs specs
        )
        {
            // 天板で作成した参照面を名前で探す
            DebugLogger.Show("Debug", "脚 - 参照面取得");
            var view = FamilyGeometryUtils.FindPlanView(
                doc
            )!;

            var refL = FindRP(doc, "Left")!;
            var refR = FindRP(doc, "Right")!;
            var refF = FindRP(doc, "Front")!;
            var refB = FindRP(doc, "Back")!;
            if (refL == null)
                return;
            double hW = specs.Width / 304.8 / 2.0;
            double hD = specs.Depth / 304.8 / 2.0;
            // double hH = specs.Height / 304.8 / 2.0;
            double lW =
                (specs.LegWidth > 0 ? specs.LegWidth : 50.0)
                / 304.8;
            // 内側の参照面
            DebugLogger.Show(
                "Debug",
                "脚 - 内側参照面作成"
            );
            var refLIn = FamilyGeometryUtils.CreateRefPlane(
                doc,
                view,
                new XYZ(-hW + lW, 0, 0),
                new XYZ(-hW + lW, 1, 0),
                "LeftIn",
                refL
            );
            var refRIn = FamilyGeometryUtils.CreateRefPlane(
                doc,
                view,
                new XYZ(hW - lW, 0, 0),
                new XYZ(hW - lW, 1, 0),
                "RightIn",
                refR
            );
            var refFIn = FamilyGeometryUtils.CreateRefPlane(
                doc,
                view,
                new XYZ(0, -hD + lW, 0),
                new XYZ(1, -hD + lW, 0),
                "FrontIn",
                refF
            );
            var refBIn = FamilyGeometryUtils.CreateRefPlane(
                doc,
                view,
                new XYZ(0, hD - lW, 0),
                new XYZ(1, hD - lW, 0),
                "BackIn",
                refB
            );
            // 脚幅の拘束
            DebugLogger.Show(
                "Debug",
                "脚 - 拘束(AddConstraint)開始"
            );
            FamilyDimensionUtils.AddConstraint(
                doc,
                view,
                refL,
                refLIn,
                pLegW
            );
            FamilyDimensionUtils.AddConstraint(
                doc,
                view,
                refRIn,
                refR,
                pLegW
            );
            FamilyDimensionUtils.AddConstraint(
                doc,
                view,
                refF,
                refFIn,
                pLegW
            );
            FamilyDimensionUtils.AddConstraint(
                doc,
                view,
                refBIn,
                refB,
                pLegW
            );
            // 4本の脚のプロファイル
            DebugLogger.Show("Debug", "脚 - 形状作成開始");
            CurveArrArray curves = new();
            curves.Append(
                CreateRectCurve(
                    -hW,
                    -hD,
                    -hW + lW,
                    -hD + lW
                )
            ); // 左前
            curves.Append(
                CreateRectCurve(hW - lW, -hD, hW, -hD + lW)
            ); // 右前
            curves.Append(
                CreateRectCurve(-hW, hD - lW, -hW + lW, hD)
            ); // 左奥
            curves.Append(
                CreateRectCurve(hW - lW, hD - lW, hW, hD)
            ); // 右奥
            SketchPlane sp = SketchPlane.Create(
                doc,
                Plane.CreateByNormalAndOrigin(
                    XYZ.BasisZ,
                    XYZ.Zero
                )
            );
            Extrusion legs = doc.FamilyCreate.NewExtrusion(
                true,
                curves,
                sp,
                1.0
            );
            legs.get_Parameter(
                    BuiltInParameter.EXTRUSION_START_PARAM
                )
                .Set(0.0);
            // 天板の裏まで伸ばす
            doc.FamilyManager.AssociateElementParameterToFamilyParameter(
                legs.get_Parameter(
                    BuiltInParameter.EXTRUSION_END_PARAM
                ),
                pUnderside
            );
            doc.Regenerate();
            DebugLogger.Show("Debug", "脚 - Align開始");
            FamilyGeometryUtils.AlignFaces(
                doc,
                legs,
                [
                    refL,
                    refR,
                    refF,
                    refB,
                    refLIn,
                    refRIn,
                    refFIn,
                    refBIn,
                ]
            );
        }

        // 矩形作成ヘルパー
        private static CurveArrArray CreateRect(
            double x1,
            double y1,
            double x2,
            double y2
        )
        {
            var arr = new CurveArrArray();
            arr.Append(CreateRectCurve(x1, y1, x2, y2));
            return arr;
        }

        private static CurveArray CreateRectCurve(
            double x1,
            double y1,
            double x2,
            double y2
        )
        {
            var c = new CurveArray();
            c.Append(
                Line.CreateBound(
                    new XYZ(x1, y1, 0),
                    new XYZ(x2, y1, 0)
                )
            );
            c.Append(
                Line.CreateBound(
                    new XYZ(x2, y1, 0),
                    new XYZ(x2, y2, 0)
                )
            );
            c.Append(
                Line.CreateBound(
                    new XYZ(x2, y2, 0),
                    new XYZ(x1, y2, 0)
                )
            );
            c.Append(
                Line.CreateBound(
                    new XYZ(x1, y2, 0),
                    new XYZ(x1, y1, 0)
                )
            );
            return c;
        }

        private static ReferencePlane? FindRP(
            Document doc,
            string name
        )
        {
            return new FilteredElementCollector(doc)
                .OfClass(typeof(ReferencePlane))
                .Cast<ReferencePlane>()
                .FirstOrDefault(rp => rp.Name == name);
        }
    }
}
