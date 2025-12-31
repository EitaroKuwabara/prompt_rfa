// PromptRFA/GeometryBuilder.cs
using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;

namespace PromptRFA
{
    public static class GeometryBuilder
    {
        // mm -> feet 変換ヘルパー
        private static double MmToFt(double mm) => UnitUtils.ConvertToInternalUnits(mm, UnitTypeId.Millimeters);

        // --- 机作成ロジック ---
        public static void CreateDeskGeometry(Document doc, double wMm, double dMm, double hMm, FamilyParameter? pMatTop, FamilyParameter? pMatLeg)
        {
            // 1. 天板 (Top)
            double tMm = 30.0; // 天板厚み
            double w = MmToFt(wMm);
            double d = MmToFt(dMm);
            double h = MmToFt(hMm);
            double t = MmToFt(tMm);

            // 天板は高さHの直下に配置
            XYZ centerTop = new XYZ(0, 0, h - t / 2.0);
            Solid solidTop = CreateBox(w, d, t, centerTop);
            CreateDirectShape(doc, solidTop, pMatTop);

            // 2. 脚 (Legs)
            double legW = MmToFt(50.0); // 脚の太さ
            double legH = h - t; // 天板の下まで
            double offsetX = w / 2.0 - legW / 2.0;
            double offsetY = d / 2.0 - legW / 2.0;

            // 4本の脚を作成
            XYZ[] legCenters = new XYZ[] {
                new XYZ(offsetX, offsetY, legH / 2.0),
                new XYZ(-offsetX, offsetY, legH / 2.0),
                new XYZ(offsetX, -offsetY, legH / 2.0),
                new XYZ(-offsetX, -offsetY, legH / 2.0)
            };

            foreach (var center in legCenters)
            {
                Solid solidLeg = CreateBox(legW, legW, legH, center);
                CreateDirectShape(doc, solidLeg, pMatLeg);
            }
        }

        // --- 棚作成ロジック ---
        public static void CreateShelfGeometry(Document doc, double wMm, double dMm, double hMm, int shelfCount, FamilyParameter? pMat)
        {
            double w = MmToFt(wMm);
            double d = MmToFt(dMm);
            double h = MmToFt(hMm);
            double thickness = MmToFt(20.0); // 板厚 20mm

            // 1. 天板 (Top Panel)
            XYZ topCenter = new XYZ(0, 0, h - thickness / 2.0);
            Solid solidTop = CreateBox(w, d, thickness, topCenter);
            CreateDirectShape(doc, solidTop, pMat);

            // 2. 側板 (Side Panels) - 左右
            double sideH = h - thickness;
            double sideX = w / 2.0 - thickness / 2.0;

            // 左側板
            Solid solidSideL = CreateBox(thickness, d, sideH, new XYZ(-sideX, 0, sideH / 2.0));
            CreateDirectShape(doc, solidSideL, pMat);

            // 右側板
            Solid solidSideR = CreateBox(thickness, d, sideH, new XYZ(sideX, 0, sideH / 2.0));
            CreateDirectShape(doc, solidSideR, pMat);

            // 3. 棚板 (Shelves) + 底板
            double baseH = thickness;
            Solid solidBase = CreateBox(w - thickness * 2, d, baseH, new XYZ(0, 0, baseH / 2.0));
            CreateDirectShape(doc, solidBase, pMat);

            // 中板の計算
            double internalH = h - thickness * 2;
            if (shelfCount > 0 && internalH > 0)
            {
                double pitch = internalH / (shelfCount + 1);

                for (int i = 1; i <= shelfCount; i++)
                {
                    double z = thickness + pitch * i;
                    Solid solidShelf = CreateBox(w - thickness * 2, d, thickness, new XYZ(0, 0, z));
                    CreateDirectShape(doc, solidShelf, pMat);
                }
            }

            // 4. 背板 (Back Panel)
            double backThick = MmToFt(5.0);
            double backY = d / 2.0 - backThick / 2.0;
            Solid solidBack = CreateBox(w, backThick, h, new XYZ(0, backY, h / 2.0));
            CreateDirectShape(doc, solidBack, pMat);
        }

        // --- 共通ヘルパー ---
        private static Solid CreateBox(double sizeX, double sizeY, double sizeZ, XYZ center)
        {
            double x = sizeX / 2.0;
            double y = sizeY / 2.0;
            double z = sizeZ / 2.0;

            XYZ pMin = new XYZ(center.X - x, center.Y - y, center.Z - z);
            XYZ pMax = new XYZ(center.X + x, center.Y + y, center.Z + z);

            List<Curve> profile = new List<Curve>();
            XYZ p1 = new XYZ(pMin.X, pMin.Y, pMin.Z);
            XYZ p2 = new XYZ(pMax.X, pMin.Y, pMin.Z);
            XYZ p3 = new XYZ(pMax.X, pMax.Y, pMin.Z);
            XYZ p4 = new XYZ(pMin.X, pMax.Y, pMin.Z);

            profile.Add(Line.CreateBound(p1, p2));
            profile.Add(Line.CreateBound(p2, p3));
            profile.Add(Line.CreateBound(p3, p4));
            profile.Add(Line.CreateBound(p4, p1));

            CurveLoop loop = CurveLoop.Create(profile);
            List<CurveLoop> loops = new List<CurveLoop> { loop };

            return GeometryCreationUtilities.CreateExtrusionGeometry(loops, XYZ.BasisZ, sizeZ);
        }

        private static void CreateDirectShape(Document doc, Solid solid, FamilyParameter? matParam)
        {
            if (solid == null) return;
            DirectShape ds = DirectShape.CreateElement(doc, new ElementId(BuiltInCategory.OST_GenericModel));
            ds.SetShape(new List<GeometryObject> { solid });
        }
    }
}