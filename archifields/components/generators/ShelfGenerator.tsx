"use client";

import {Button} from "@/components/ui/button";
import {Input} from "@/components/ui/input";
import {Label} from "@/components/ui/label";
import {Textarea} from "@/components/ui/textarea";
import {Card, CardContent} from "@/components/ui/card";
import {Loader2} from "lucide-react";
import {useFamilyGenerator} from "@/hooks/useFamilyGenerator";

// 親コンポーネント(page.tsx)から受け取る関数の型定義
interface ShelfGeneratorProps {
  onSuccess: () => void;
}

// バックエンドの ShelfParams と合わせる
interface ShelfParams {
  width: number;
  depth: number;
  height: number;
  topThickness: number;
  sideThickness: number;
  shelfThickness: number;
  topMaterialName: string;
  sideMaterialName: string;
  shelfMaterialName: string;
  shelfCount: number;
  [key: string]: string | number | boolean;
}

// props として { onSuccess } を受け取るように変更
export function ShelfGenerator({onSuccess}: ShelfGeneratorProps) {
  const {
    params,
    prompt,
    setPrompt,
    isAnalyzing,
    isGenerating,
    handleSuggest,
    handleGenerate,
    handleChange,
    handleStringChange,
  } = useFamilyGenerator<ShelfParams>(
    {
      width: 800,
      depth: 300,
      height: 1800,
      topThickness: 30,
      sideThickness: 30,
      shelfThickness: 20,
      topMaterialName: "Wood",
      sideMaterialName: "Wood",
      shelfMaterialName: "Wood",
      shelfCount: 4,
    },
    "Shelf",
    onSuccess
  );

  return (
    <div className="space-y-6">
      <Card>
        <CardContent className="pt-6 space-y-4">
          <div className="space-y-2">
            <Label>AI Design Assistant (Shelf)</Label>
            <Textarea
              placeholder="Example: A modern shelf with 4 shelves. The top is wood and the sides are wood. The shelves are wood."
              value={prompt}
              onChange={(e) => setPrompt(e.target.value)}
              rows={3}
            />
            <Button
              onClick={() => handleSuggest("Shelf")}
              disabled={isAnalyzing || !prompt}
              variant="secondary"
              className="w-full"
            >
              {isAnalyzing ? (
                <Loader2 className="mr-2 h-4 w-4 animate-spin" />
              ) : (
                "AI Suggest"
              )}
            </Button>
          </div>
        </CardContent>
      </Card>

      {/* 詳細パラメータ設定フォーム */}
      <div className="space-y-4 border p-4 rounded-lg bg-slate-50">
        <h3 className="font-medium text-sm text-slate-500">
          Detailed Parameter Settings (Shelf)
        </h3>

        {/* 基本寸法 */}
        <div className="grid grid-cols-3 gap-4">
          <div className="space-y-2">
            <Label>幅 (mm)</Label>
            <Input
              type="number"
              value={params.width}
              onChange={(e) => handleChange("width", e.target.value)}
            />
          </div>
          <div className="space-y-2">
            <Label>Depth (mm)</Label>
            <Input
              type="number"
              value={params.depth}
              onChange={(e) => handleChange("depth", e.target.value)}
            />
          </div>
          <div className="space-y-2">
            <Label>Height (mm)</Label>
            <Input
              type="number"
              value={params.height}
              onChange={(e) => handleChange("height", e.target.value)}
            />
          </div>
        </div>

        {/* 板厚設定 */}
        <div className="grid grid-cols-3 gap-4 border-t pt-4">
          <div className="space-y-2">
            <Label>Top Thickness (mm)</Label>
            <Input
              type="number"
              value={params.topThickness}
              onChange={(e) => handleChange("topThickness", e.target.value)}
            />
          </div>
          <div className="space-y-2">
            <Label>Side Thickness (mm)</Label>
            <Input
              type="number"
              value={params.sideThickness}
              onChange={(e) => handleChange("sideThickness", e.target.value)}
            />
          </div>
          <div className="space-y-2">
            <Label>Shelf Thickness (mm)</Label>
            <Input
              type="number"
              value={params.shelfThickness}
              onChange={(e) => handleChange("shelfThickness", e.target.value)}
            />
          </div>
        </div>

        {/* マテリアル設定 */}
        <div className="grid grid-cols-1 gap-4 border-t pt-4">
          <div className="grid grid-cols-2 gap-4">
            <div className="space-y-2">
              <Label>Top Material</Label>
              <Input
                value={params.topMaterialName}
                onChange={(e) =>
                  handleStringChange("topMaterialName", e.target.value)
                }
              />
            </div>
            <div className="space-y-2">
              <Label>Side Material</Label>
              <Input
                value={params.sideMaterialName}
                onChange={(e) =>
                  handleStringChange("sideMaterialName", e.target.value)
                }
              />
            </div>
          </div>
          <div className="grid grid-cols-2 gap-4">
            <div className="space-y-2">
              <Label>Shelf Material</Label>
              <Input
                value={params.shelfMaterialName}
                onChange={(e) =>
                  handleStringChange("shelfMaterialName", e.target.value)
                }
              />
            </div>
            <div className="space-y-2">
              <Label>Shelf Count</Label>
              <Input
                type="number"
                value={params.shelfCount}
                onChange={(e) => handleChange("shelfCount", e.target.value)}
              />
            </div>
          </div>
        </div>

        <Button
          onClick={handleGenerate}
          disabled={isGenerating}
          className="w-full bg-slate-900 text-white hover:bg-slate-800"
        >
          {isGenerating ? (
            <Loader2 className="mr-2 h-4 w-4 animate-spin" />
          ) : (
            "Generate Revit Family"
          )}
        </Button>
      </div>
    </div>
  );
}
