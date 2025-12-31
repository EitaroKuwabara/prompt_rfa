// PromptRFA/Creating/DeskCreator.cs
using System;
using System.Collections.Generic;
using Autodesk.Revit.DB;
using Newtonsoft.Json.Linq;
using PromptRFA.Models;

namespace PromptRFA.Creating
{
    public class DeskCreator : IFamilyCreator
    {
        public void Execute(Document doc, JObject specsJson)
        {
            // 1. JSONを机の型に変換
            DeskSpecs? specs =
                specsJson.ToObject<DeskSpecs>();
            FamilyManager famMgr = doc.FamilyManager;

            using (
                Transaction t = new Transaction(
                    doc,
                    "Create Desk"
                )
            )
            {
                t.Start();

                // 2. パラメータの準備 (天板用と脚用)
                FamilyParameter pTopMat =
                    EnsureMaterialParam(
                        doc,
                        "Top Material"
                    );
                FamilyParameter pLegMat =
                    EnsureMaterialParam(
                        doc,
                        "Leg Material"
                    );

                // 3. 寸法変換 (mm -> feet)
                double width = specs!.Width / 304.8;
                double depth = specs.Depth / 304.8;
                double height = specs.Height / 304.8;
                double topThk = specs.TopThickness / 304.8;
                double legW = specs.LegWidth / 304.8;

                // 4. ジオメトリ作成

                // --- A. 天板 (Top) ---
                // Z位置: 高さの頂点から下に厚み分
                Extrusion? topSolid = CreateBox(
                    doc,
                    width,
                    depth,
                    topThk,
                    0,
                    0,
                    height - topThk
                );
                if (topSolid != null)
                {
                    famMgr.AssociateElementParameterToFamilyParameter(
                        topSolid.get_Parameter(
                            BuiltInParameter.MATERIAL_ID_PARAM
                        ),
                        pTopMat
                    );
                }

                // --- B. 脚 (Legs) x 4本 ---
                // 脚の高さ = 全高 - 天板厚
                double legH = height - topThk;

                // 配置位置の計算 (中心から端へのオフセット)
                double offsetX = (width / 2) - (legW / 2);
                double offsetY = (depth / 2) - (legW / 2);

                // 4本の脚を作成 (右奥, 左奥, 左手前, 右手前)
                CreateLeg(
                    doc,
                    legW,
                    legH,
                    offsetX,
                    offsetY,
                    pLegMat,
                    famMgr
                );
                CreateLeg(
                    doc,
                    legW,
                    legH,
                    -offsetX,
                    offsetY,
                    pLegMat,
                    famMgr
                );
                CreateLeg(
                    doc,
                    legW,
                    legH,
                    -offsetX,
                    -offsetY,
                    pLegMat,
                    famMgr
                );
                CreateLeg(
                    doc,
                    legW,
                    legH,
                    offsetX,
                    -offsetY,
                    pLegMat,
                    famMgr
                );

                // --- C. 引き出し (Drawers) - オプション ---
                if (specs.HasDrawers)
                {
                    // 簡易的に天板の下に「幕板兼引き出しボックス」を追加
                    double drawerHeight = 150.0 / 304.8; // 高さ150mm固定
                    double drawerWidth = width - (legW * 2); // 脚の内側に収める
                    double drawerDepth = depth - (legW * 2);

                    // 位置: 天板の下
                    double drawerZ =
                        height - topThk - drawerHeight;

                    // 床より下に行かないように調整
                    if (drawerZ > 0)
                    {
                        Extrusion? drawerSolid = CreateBox(
                            doc,
                            drawerWidth,
                            drawerDepth,
                            drawerHeight,
                            0,
                            0,
                            drawerZ
                        );
                        if (drawerSolid != null)
                        {
                            // 引き出しは脚と同じ素材にしておく
                            famMgr.AssociateElementParameterToFamilyParameter(
                                drawerSolid.get_Parameter(
                                    BuiltInParameter.MATERIAL_ID_PARAM
                                ),
                                pLegMat
                            );
                        }
                    }
                }

                t.Commit();
            }
        }

        // 脚を1本つくるヘルパー
        private void CreateLeg(
            Document doc,
            double w,
            double h,
            double cx,
            double cy,
            FamilyParameter matParam,
            FamilyManager mgr
        )
        {
            Extrusion? leg = CreateBox(
                doc,
                w,
                w,
                h,
                cx,
                cy,
                0
            ); // BaseZ is 0 (床から)
            if (leg != null)
            {
                mgr.AssociateElementParameterToFamilyParameter(
                    leg.get_Parameter(
                        BuiltInParameter.MATERIAL_ID_PARAM
                    ),
                    matParam
                );
            }
        }

        // --- 共通ヘルパー (ShelfCreatorと同じ) ---
        private FamilyParameter EnsureMaterialParam(
            Document doc,
            string paramName
        )
        {
            FamilyManager mgr = doc.FamilyManager;
            FamilyParameter param = mgr.get_Parameter(
                paramName
            );
            if (param == null)
            {
                param = mgr.AddParameter(
                    paramName,
                    GroupTypeId.Materials,
                    SpecTypeId.Reference.Material,
                    false
                );
            }
            return param;
        }

        private Extrusion? CreateBox(
            Document doc,
            double w,
            double d,
            double h,
            double cx,
            double cy,
            double bz
        )
        {
            CurveArray curveArray = new CurveArray();
            XYZ p1 = new XYZ(cx - w / 2, cy - d / 2, bz);
            XYZ p2 = new XYZ(cx + w / 2, cy - d / 2, bz);
            XYZ p3 = new XYZ(cx + w / 2, cy + d / 2, bz);
            XYZ p4 = new XYZ(cx - w / 2, cy + d / 2, bz);

            curveArray.Append(Line.CreateBound(p1, p2));
            curveArray.Append(Line.CreateBound(p2, p3));
            curveArray.Append(Line.CreateBound(p3, p4));
            curveArray.Append(Line.CreateBound(p4, p1));

            CurveArrArray curveArrArray =
                new CurveArrArray();
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
