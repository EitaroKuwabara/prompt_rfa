// archifields/components/generators/DeskGenerator.tsx
"use client";

import {Button} from "@/components/ui/button";
import {Input} from "@/components/ui/input";
import {Label} from "@/components/ui/label";
import {Textarea} from "@/components/ui/textarea";
import {Card, CardContent} from "@/components/ui/card";
import {Loader2} from "lucide-react";
import {useFamilyGenerator} from "@/hooks/useFamilyGenerator";

// 親コンポーネント(page.tsx)から受け取る関数の型定義
interface DeskGeneratorProps {
  onSuccess: () => void;
}

// バックエンドの DeskParams (schemas.py) と合わせる
interface DeskParams {
  width: number;
  depth: number;
  height: number;
  topThickness: number;
  legWidth: number;
  topMaterialName: string;
  legMaterialName: string;
  hasDrawers: boolean;
  [key: string]: string | number | boolean;
}

export function DeskGenerator({onSuccess}: DeskGeneratorProps) {
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
    handleCheckboxChange,
  } = useFamilyGenerator<DeskParams>({
    width: 1200,
    depth: 700,
    height: 700,
    topThickness: 30,
    legWidth: 50,
    topMaterialName: "Wood",
    legMaterialName: "Steel",
    hasDrawers: false,
  },
  "Desk",
  onSuccess
);

  return (
    <div className="space-y-6">
      <Card>
        <CardContent className="pt-6 space-y-4">
          <div className="space-y-2">
            <Label>AI Design Assistant (Desk)</Label>
            <Textarea
              placeholder="Example: A modern office desk. The top is glass and the legs are black metal. Also add drawers."
              value={prompt}
              onChange={(e) => setPrompt(e.target.value)}
              rows={3}
            />
            <Button
              onClick={() => handleSuggest("Desk")}
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
          Detailed Parameter Settings (Desk)
        </h3>

        {/* 基本寸法 */}
        <div className="grid grid-cols-3 gap-4">
          <div className="space-y-2">
            <Label>Width (mm)</Label>
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

        {/* 机特有の寸法 */}
        <div className="grid grid-cols-2 gap-4 border-t pt-4">
          <div className="space-y-2">
            <Label>Top Thickness (mm)</Label>
            <Input
              type="number"
              value={params.topThickness}
              onChange={(e) => handleChange("topThickness", e.target.value)}
            />
          </div>
          <div className="space-y-2">
            <Label>Leg Width (mm)</Label>
            <Input
              type="number"
              value={params.legWidth}
              onChange={(e) => handleChange("legWidth", e.target.value)}
            />
          </div>
        </div>

        {/* マテリアル設定 */}
        <div className="grid grid-cols-2 gap-4 border-t pt-4">
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
            <Label>Leg Material</Label>
            <Input
              value={params.legMaterialName}
              onChange={(e) =>
                handleStringChange("legMaterialName", e.target.value)
              }
            />
          </div>
        </div>

        {/* オプション設定 */}
        <div className="border-t pt-4">
          <div className="flex items-center space-x-2">
            <input
              type="checkbox"
              id="hasDrawers"
              className="h-4 w-4 rounded border-gray-300 text-slate-900 focus:ring-slate-900"
              checked={params.hasDrawers}
              onChange={(e) => handleCheckboxChange("hasDrawers", e.target.checked)}
            />
            <Label htmlFor="hasDrawers" className="cursor-pointer">
              Add Drawers (Simple Shape)
            </Label>
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
            "Revitファミリを生成"
          )}
        </Button>
      </div>
    </div>
  );
}
