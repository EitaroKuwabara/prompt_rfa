using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;

namespace PromptRFA
{
    public static class SkeletonBuilder
    {
        // ★修正: 引数に wMm, dMm を追加（入力値をそのまま受け取る）
        public static void BuildSkeleton(Document doc, FamilyParameter paramWidth, FamilyParameter paramDepth, FamilyParameter paramHeight, double wMm, double dMm)
        {
            // 1. 基準要素の取得
            ReferencePlane? centerLR = GetCenterReferencePlane(doc, isLeftRight: true);
            ReferencePlane? centerFB = GetCenterReferencePlane(doc, isLeftRight: false);
            Level? refLevel = GetReferenceLevel(doc);
            View? planView = GetPlanView(doc);

            if (centerLR == null || centerFB == null || refLevel == null || planView == null)
            {
                Logger.Write("Error: スケルトン作成に必要な要素が見つかりません。");
                return;
            }

            // 2. 幅 (Width) の制御系
            // ★シンプル化: 入力されたmmをここで「Revit内部単位」に変換します
            // 例: 2000mm -> 約6.56feet
            double wInternal = UnitUtils.ConvertToInternalUnits(wMm, UnitTypeId.Millimeters);
            double halfW = wInternal / 2.0;

            // 参照面作成 (Internal Unitで座標を指定)
            // 左(-X) と 右(+X)
            ReferencePlane refLeft = CreateRefPlane(doc, planView, -halfW, 0, 0, "Left");
            ReferencePlane refRight = CreateRefPlane(doc, planView, halfW, 0, 0, "Right");
            
            doc.Regenerate(); // 確定

            // EQ寸法 (左-中心-右) -> ★EQ拘束ON
            double dimPosY_EQ = UnitUtils.ConvertToInternalUnits(600, UnitTypeId.Millimeters); 
            CreateEQDimension(doc, planView, refLeft, centerLR, refRight, isHorizontalLine: true, pos: dimPosY_EQ);

            // 全体寸法 (左-右) -> Widthパラメータ
            double dimPosY_W = UnitUtils.ConvertToInternalUnits(900, UnitTypeId.Millimeters);
            Dimension? dimWidth = CreateLinearDimension(doc, planView, refLeft, refRight, isHorizontalLine: true, pos: dimPosY_W);
            if(dimWidth != null) dimWidth.FamilyLabel = paramWidth;


            // 3. 奥行 (Depth) の制御系
            double dInternal = UnitUtils.ConvertToInternalUnits(dMm, UnitTypeId.Millimeters);
            double halfD = dInternal / 2.0;

            // 前(-Y) と 後(+Y)
            ReferencePlane refFront = CreateRefPlane(doc, planView, 0, -halfD, 1, "Front");
            ReferencePlane refBack = CreateRefPlane(doc, planView, 0, halfD, 1, "Back");
            
            doc.Regenerate();

            // EQ寸法 (前-中心-後) -> ★EQ拘束ON
            double dimPosX_EQ = UnitUtils.ConvertToInternalUnits(600, UnitTypeId.Millimeters);
            CreateEQDimension(doc, planView, refFront, centerFB, refBack, isHorizontalLine: false, pos: dimPosX_EQ);

            // 全体寸法 (前-後) -> Depthパラメータ
            double dimPosX_D = UnitUtils.ConvertToInternalUnits(900, UnitTypeId.Millimeters);
            Dimension? dimDepth = CreateLinearDimension(doc, planView, refFront, refBack, isHorizontalLine: false, pos: dimPosX_D);
            if (dimDepth != null) dimDepth.FamilyLabel = paramDepth;
            
            Logger.Write("Info: スケルトン作成完了（Internal Unit換算済み）");
        }

        // --- Helper Methods ---

        private static ReferencePlane CreateRefPlane(Document doc, View view, double x, double y, int direction, string name)
        {
            // 引数の x, y は既に Internal Unit (Feet) に変換済みとして扱います
            XYZ p1, p2;
            double len = UnitUtils.ConvertToInternalUnits(1500, UnitTypeId.Millimeters); // 線の長さ

            if (direction == 0) // Vertical (Left/Right)
            {
                p1 = new XYZ(x, y - len, 0);
                p2 = new XYZ(x, y + len, 0);
            }
            else // Horizontal (Front/Back)
            {
                p1 = new XYZ(x - len, y, 0);
                p2 = new XYZ(x + len, y, 0);
            }
            ReferencePlane rp = doc.FamilyCreate.NewReferencePlane(p1, p2, XYZ.BasisZ, view);
            rp.Name = name;
            return rp;
        }

        private static void CreateEQDimension(Document doc, View view, ReferencePlane p1, ReferencePlane center, ReferencePlane p2, bool isHorizontalLine, double pos)
        {
            try
            {
                ReferenceArray refArray = new ReferenceArray();
                refArray.Append(p1.GetReference());
                refArray.Append(center.GetReference());
                refArray.Append(p2.GetReference());

                Line line;
                double size = UnitUtils.ConvertToInternalUnits(2000, UnitTypeId.Millimeters); // 寸法線の長さ
                
                if (isHorizontalLine)
                {
                    XYZ start = new XYZ(-size, pos, 0);
                    XYZ end = new XYZ(size, pos, 0);
                    line = Line.CreateBound(start, end);
                }
                else
                {
                    XYZ start = new XYZ(pos, -size, 0);
                    XYZ end = new XYZ(pos, size, 0);
                    line = Line.CreateBound(start, end);
                }

                Dimension dim = doc.FamilyCreate.NewDimension(view, line, refArray);
                dim.AreSegmentsEqual = true; // ★EQ有効化
            }
            catch (Exception ex)
            {
                Logger.Write($"Error: EQ寸法作成失敗: {ex.Message}");
            }
        }

        private static Dimension? CreateLinearDimension(Document doc, View view, ReferencePlane p1, ReferencePlane p2, bool isHorizontalLine, double pos)
        {
            try
            {
                ReferenceArray refArray = new ReferenceArray();
                refArray.Append(p1.GetReference());
                refArray.Append(p2.GetReference());

                Line line;
                double size = UnitUtils.ConvertToInternalUnits(2000, UnitTypeId.Millimeters);

                if (isHorizontalLine)
                {
                    XYZ start = new XYZ(-size, pos, 0);
                    XYZ end = new XYZ(size, pos, 0);
                    line = Line.CreateBound(start, end);
                }
                else
                {
                    XYZ start = new XYZ(pos, -size, 0);
                    XYZ end = new XYZ(pos, size, 0);
                    line = Line.CreateBound(start, end);
                }

                return doc.FamilyCreate.NewDimension(view, line, refArray);
            }
            catch (Exception ex)
            {
                Logger.Write($"Error: 寸法作成失敗: {ex.Message}");
                return null;
            }
        }

        // --- 検索系ヘルパー（変更なし） ---
        private static ReferencePlane? GetCenterReferencePlane(Document doc, bool isLeftRight)
        {
            var planes = new FilteredElementCollector(doc).OfClass(typeof(ReferencePlane)).Cast<ReferencePlane>();
            foreach (var rp in planes)
            {
                string name = rp.Name;
                if (string.IsNullOrEmpty(name)) continue;
                if (isLeftRight)
                {
                    if ((name.Contains("左") && name.Contains("右")) || (name.Contains("Left") && name.Contains("Right"))) return rp;
                }
                else
                {
                    if ((name.Contains("前") && name.Contains("後")) || (name.Contains("Front") && name.Contains("Back")) || (name.Contains("正面") && name.Contains("背面"))) return rp;
                }
            }
            return null;
        }

        private static Level? GetReferenceLevel(Document doc) => new FilteredElementCollector(doc).OfClass(typeof(Level)).Cast<Level>().FirstOrDefault();

        private static View? GetPlanView(Document doc)
        {
            var view = new FilteredElementCollector(doc).OfClass(typeof(View)).Cast<View>()
                .Where(v => v.ViewType == ViewType.FloorPlan)
                .FirstOrDefault(v => v.Name == "参照レベル" || v.Name == "基準レベル" || v.Name == "Reference Level");
            return view ?? new FilteredElementCollector(doc).OfClass(typeof(View)).Cast<View>().FirstOrDefault(v => v.ViewType == ViewType.FloorPlan);
        }
    }
}