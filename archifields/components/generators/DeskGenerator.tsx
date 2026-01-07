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
            <Label>AI 設計アシスタント (机)</Label>
            <Textarea
              placeholder="例: モダンなオフィスのデスク。天板はガラスで、脚は黒い金属にして。引き出しもつけて。"
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
                "AIに提案させる"
              )}
            </Button>
          </div>
        </CardContent>
      </Card>

      {/* 詳細パラメータ設定フォーム */}
      <div className="space-y-4 border p-4 rounded-lg bg-slate-50">
        <h3 className="font-medium text-sm text-slate-500">
          詳細パラメータ設定 (Desk)
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
            <Label>奥行 (mm)</Label>
            <Input
              type="number"
              value={params.depth}
              onChange={(e) => handleChange("depth", e.target.value)}
            />
          </div>
          <div className="space-y-2">
            <Label>高さ (mm)</Label>
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
            <Label>天板厚 (mm)</Label>
            <Input
              type="number"
              value={params.topThickness}
              onChange={(e) => handleChange("topThickness", e.target.value)}
            />
          </div>
          <div className="space-y-2">
            <Label>脚の太さ (mm)</Label>
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
            <Label>天板素材</Label>
            <Input
              value={params.topMaterialName}
              onChange={(e) =>
                handleStringChange("topMaterialName", e.target.value)
              }
            />
          </div>
          <div className="space-y-2">
            <Label>脚の素材</Label>
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
              引き出しを付ける (簡易形状)
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
