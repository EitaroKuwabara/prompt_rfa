// PromptRFA/Utils/FamilyGeometryUtils.cs
using Autodesk.Revit.DB;

namespace PromptRFA.Utils
{
    public enum ReferencePlaneReference
    {
        CenterLeftRight,
        CenterFrontBack,
        Bottom,
    }

    public static class FamilyGeometryUtils
    {
        /// <summary>
        /// 参照面を作成する（向きの自動補正機能付き）
        /// </summary>
        public static ReferencePlane CreateRefPlane(
            Document doc,
            View view,
            XYZ p1,
            XYZ p2,
            string name,
            ReferencePlane? matchPlane = null
        )
        {
            // 1. まず普通に作る
            ReferencePlane rp =
                doc.FamilyCreate.NewReferencePlane(
                    p1,
                    p2,
                    XYZ.BasisZ,
                    view
                );
            doc.Regenerate();

            // 2. 基準面がある場合、向きをチェックして逆なら作り直す
            if (matchPlane != null)
            {
                if (
                    rp.GetPlane()
                        .Normal.DotProduct(
                            matchPlane.GetPlane().Normal
                        ) < 0.01
                ) // 逆向き判定
                {
                    doc.Delete(rp.Id);
                    doc.Regenerate();
                    rp = doc.FamilyCreate.NewReferencePlane(
                        p2,
                        p1,
                        XYZ.BasisZ,
                        view
                    ); // 反転して再作成
                    doc.Regenerate();
                }
            }
            rp.Name = name;
            return rp;
        }

        /// <summary>
        /// 既存の参照面（中心線など）を探す
        /// </summary>
        public static ReferencePlane GetReferencePlane(
            Document doc,
            ReferencePlaneReference refType
        )
        {
            var planes = new FilteredElementCollector(doc)
                .OfClass(typeof(ReferencePlane))
                .Cast<ReferencePlane>();

            foreach (var rp in planes)
            {
                if (
                    refType
                    == ReferencePlaneReference.CenterLeftRight
                )
                {
                    if (
                        rp.Name.Contains(
                            "Center (Left/Right)"
                        )
                        || rp.Name.Contains("中心 (左/右)")
                    )
                        return rp;
                    if (
                        Math.Abs(rp.GetPlane().Normal.X)
                            > 0.9
                        && IsOrigin(rp)
                    )
                        return rp;
                }
                if (
                    refType
                    == ReferencePlaneReference.CenterFrontBack
                )
                {
                    if (
                        rp.Name.Contains(
                            "Center (Front/Back)"
                        )
                        || rp.Name.Contains("中心 (前/後)")
                    )
                        return rp;
                    if (
                        Math.Abs(rp.GetPlane().Normal.Y)
                            > 0.9
                        && IsOrigin(rp)
                    )
                        return rp;
                }
                if (
                    refType
                    == ReferencePlaneReference.Bottom
                )
                {
                    if (
                        rp.Name.Contains("Reference Level")
                        || rp.Name.Contains("参照レベル")
                    )
                        return rp;
                }
            }

            // 見つからない場合のフォールバック作成
            View planView = FindPlanView(doc)!;
            if (
                refType
                == ReferencePlaneReference.CenterLeftRight
            )
                return CreateRefPlane(
                    doc,
                    planView,
                    new XYZ(0, -5, 0),
                    new XYZ(0, 5, 0),
                    "Center (Left/Right)"
                );
            else
                return CreateRefPlane(
                    doc,
                    planView,
                    new XYZ(-5, 0, 0),
                    new XYZ(5, 0, 0),
                    "Center (Front/Back)"
                );
        }

        /// <summary>
        /// 平面図ビューを探す
        /// </summary>
        public static View? FindPlanView(Document doc)
        {
            return new FilteredElementCollector(doc)
                .OfClass(typeof(View))
                .Cast<View>()
                .FirstOrDefault(v =>
                    v.ViewType == ViewType.FloorPlan
                    && !v.IsTemplate
                );
        }

        /// <summary>
        /// 形状の面を参照面にロック(Align)する
        /// </summary>
        public static void AlignFaceToRefPlane(
            Document doc,
            Extrusion extrusion,
            XYZ faceNormal,
            ReferencePlane refPlane
        )
        {
            View? view = FindPlanView(doc);
            if (view == null)
                return;

            Options opt = new Options
            {
                ComputeReferences = true,
                DetailLevel = ViewDetailLevel.Fine,
            };
            GeometryElement geoElem =
                extrusion.get_Geometry(opt);

            foreach (GeometryObject obj in geoElem)
            {
                if (obj is Solid solid)
                {
                    foreach (Face face in solid.Faces)
                    {
                        if (
                            face is PlanarFace pf
                            && pf.FaceNormal.IsAlmostEqualTo(
                                faceNormal
                            )
                        )
                        {
                            try
                            {
                                doc.FamilyCreate.NewAlignment(
                                    view,
                                    refPlane.GetReference(),
                                    pf.Reference
                                );
                            }
                            catch { }
                            return;
                        }
                    }
                }
            }
        }

        // ヘルパー: 複数の面をまとめてAlignする
        public static void AlignFaces(
            Document doc,
            Extrusion extrusion,
            ReferencePlane[] planes
        )
        {
            View planView = FindPlanView(doc)!;
            Options opt = new Options
            {
                ComputeReferences = true,
                DetailLevel = ViewDetailLevel.Fine,
            };
            GeometryElement geoElem =
                extrusion.get_Geometry(opt);

            foreach (GeometryObject obj in geoElem)
            {
                if (obj is Solid solid)
                {
                    foreach (Face face in solid.Faces)
                    {
                        if (face is PlanarFace pf)
                        {
                            foreach (var rp in planes)
                            {
                                // 面が参照面上にあるかチェック
                                double distToPlane =
                                    Math.Abs(
                                        rp.GetPlane()
                                            .Normal.DotProduct(
                                                pf.Origin
                                                    - rp.GetPlane().Origin
                                            )
                                    );

                                // ★修正: IsParallel ではなく、外積(CrossProduct)が0に近いかで判定
                                bool isParallel = pf
                                    .FaceNormal.CrossProduct(
                                        rp.GetPlane().Normal
                                    )
                                    .IsZeroLength();

                                if (
                                    distToPlane < 0.001
                                    && isParallel
                                )
                                {
                                    try
                                    {
                                        doc.FamilyCreate.NewAlignment(
                                            planView,
                                            rp.GetReference(),
                                            pf.Reference
                                        );
                                    }
                                    catch { }
                                }
                            }
                        }
                    }
                }
            }
        }

        private static bool IsOrigin(ReferencePlane rp) =>
            rp.GetPlane().Origin.DistanceTo(XYZ.Zero)
            < 0.001;
    }
}
