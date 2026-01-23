// PromptRFA/Utils/FamilyDimensionUtils.cs
using System;
using Autodesk.Revit.DB;
// using Autodesk.Revit.UI;
using PromptRFA.Utils;

namespace PromptRFA.Utils
{
    public static class FamilyDimensionUtils
    {
        public static void AddDimensions(
            Document doc,
            View view,
            ReferencePlane p1,
            ReferencePlane center,
            ReferencePlane p2,
            FamilyParameter param
        )
        {
            if (view == null)
                return;

            // 1. EQ寸法 (p1のNormal方向へ)
            XYZ dimDir = p1.GetPlane().Normal;
            Line dimLineEQ = Line.CreateBound(
                XYZ.Zero,
                dimDir
            );
            ReferenceArray refArrayEQ = new();
            refArrayEQ.Append(p1.GetReference());
            refArrayEQ.Append(center.GetReference());
            refArrayEQ.Append(p2.GetReference());

            try
            {
                Dimension dimEQ =
                    doc.FamilyCreate.NewDimension(
                        view,
                        dimLineEQ,
                        refArrayEQ
                    );
                dimEQ.AreSegmentsEqual = true;
            }
            catch
            { /* エラー時は無視 */
            }

            // 2. 全体寸法
            try
            {
                XYZ offset =
                    new XYZ(-dimDir.Y, dimDir.X, 0) * 0.5; // 少しずらす
                if (offset.IsZeroLength())
                    offset = XYZ.BasisZ * 0.5;

                Line dimLineTotal = Line.CreateBound(
                    offset,
                    offset + dimDir
                );
                ReferenceArray refArrayTotal = new();
                refArrayTotal.Append(p1.GetReference());
                refArrayTotal.Append(p2.GetReference());

                Dimension dimTotal =
                    doc.FamilyCreate.NewDimension(
                        view,
                        dimLineTotal,
                        refArrayTotal
                    );
                dimTotal.FamilyLabel = param;
            }
            catch (Exception ex)
            {
                string msg =
                    $"[AddDimensions Error]\n"
                    + $"Plane1: {p1.Name} (Normal: {p1.GetPlane().Normal})\n"
                    + $"Plane2: {p2.Name}\n"
                    + $"Dim Line Dir: {dimDir}\n"
                    + $"View Dir: {view.ViewDirection}\n"
                    + $"Error: {ex.Message}";

                DebugLogger.Show("Debug Dimensions", msg);
                throw;
            }
        }

        // 2つの参照面の間に寸法拘束を追加
        public static void AddConstraint(
            Document doc,
            View view,
            ReferencePlane p1,
            ReferencePlane p2,
            FamilyParameter param
        )
        {
            // Revitが持つ法線をそのまま使う
            XYZ dimDir = p1.GetPlane().Normal;

            // 寸法線の配置位置（始点）
            XYZ origin = p1.GetPlane().Origin;

            // 寸法線を作成（長さ1.0で作成）
            Line dimLine = Line.CreateBound(
                origin,
                origin + dimDir
            );
            ReferenceArray refs = new();
            refs.Append(p1.GetReference());
            refs.Append(p2.GetReference());
            try
            {
                doc
                    .FamilyCreate.NewDimension(
                        view,
                        dimLine,
                        refs
                    )
                    .FamilyLabel = param;
            }
            catch (Exception ex)
            {
                // 座標関係を詳細に表示
                XYZ p2Origin = p2.GetPlane().Origin;
                // P1からP2へのベクトル（実際の隙間の方向）
                XYZ gapVector = p2Origin - origin;
                // 寸法線の向きと、実際の隙間の向きが合っているか？ (内積)
                double dot = dimDir.DotProduct(gapVector);
                string directionCheck =
                    (dot > 0)
                        ? "OK (向きは合っています)"
                        : "NG! (逆向きです)";
                string msg =
                    $"[拘束エラー解析]\n"
                    + $"----------------------------------\n"
                    + $"Plane1 ({p1.Name}): {origin}\n"
                    + $"Plane2 ({p2.Name}): {p2Origin}\n"
                    + $"----------------------------------\n"
                    + $"Ref Normal (線の向き): {dimDir}\n"
                    + $"Gap Vector (実際のＰ２方向): {gapVector}\n"
                    + $"判定: {directionCheck}\n"
                    + $"----------------------------------\n"
                    + $"エラー詳細: {ex.Message}";

                DebugLogger.Show("Debug Error Info", msg);

                // ここで止める（エラーを握りつぶさない）
                throw;
            }
        }
    }
}
