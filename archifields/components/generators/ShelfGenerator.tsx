"use client";

import {useState} from "react";
import {Button} from "@/components/ui/button";
import {Input} from "@/components/ui/input";
import {Label} from "@/components/ui/label";
import {Textarea} from "@/components/ui/textarea";
import {Card, CardContent} from "@/components/ui/card";
import {Loader2} from "lucide-react";

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
}

// props として { onSuccess } を受け取るように変更
export function ShelfGenerator({onSuccess}: ShelfGeneratorProps) {
  const [prompt, setPrompt] = useState("");
  const [isAnalyzing, setIsAnalyzing] = useState(false);
  const [isGenerating, setIsGenerating] = useState(false);

  // 初期値の設定
  const [params, setParams] = useState<ShelfParams>({
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
  });

  // AI提案ロジック
  const handleSuggest = async () => {
    if (!prompt) return;
    setIsAnalyzing(true);
    try {
      const res = await fetch("http://localhost:8000/suggest", {
        method: "POST",
        headers: {"Content-Type": "application/json"},
        body: JSON.stringify({prompt, category: "shelf"}),
      });
      if (!res.ok) throw new Error("Suggestion failed");

      const data = await res.json();
      setParams((prev) => ({
        ...prev,
        ...data,
        width: Number(data.width || prev.width),
        depth: Number(data.depth || prev.depth),
        height: Number(data.height || prev.height),
      }));
    } catch (error) {
      console.error(error);
      alert("AI提案に失敗しました");
    } finally {
      setIsAnalyzing(false);
    }
  };

  // 生成ロジック
  const handleGenerate = async () => {
    setIsGenerating(true);
    try {
      const res = await fetch("http://localhost:8000/generate", {
        method: "POST",
        headers: {"Content-Type": "application/json"},
        body: JSON.stringify({
          command: "create",
          type: "Shelf",
          params: params,
        }),
      });
      if (!res.ok) throw new Error("Generation failed");
      alert(
        "設定データを保存しました。\n\nRevitに切り替えて、アドインを実行してください。\n実行が完了したら、この画面に戻って「OK」を押してください。"
      );

      // アラートのOKが押された後、親コンポーネントに通知してプレビュー画像を更新
      onSuccess();
    } catch (error) {
      console.error(error);
      alert("生成リクエストに失敗しました");
    } finally {
      setIsGenerating(false);
    }
  };

  const handleChange = (key: keyof ShelfParams, value: string) => {
    const numVal = parseFloat(value);
    setParams((prev) => ({
      ...prev,
      [key]: isNaN(numVal) ? 0 : numVal,
    }));
  };

  const handleStringChange = (key: keyof ShelfParams, value: string) => {
    setParams((prev) => ({
      ...prev,
      [key]: value || "",
    }));
  };

  // 左右のカラム分け（grid）を削除し、純粋な入力フォームのみを返す
  return (
    <div className="space-y-6">
      <Card>
        <CardContent className="pt-6 space-y-4">
          <div className="space-y-2">
            <Label>AI 設計アシスタント (棚)</Label>
            <Textarea
              placeholder="例: 文庫本用の木製の棚。高さは1500mmくらいで、棚板はガラスにして。"
              value={prompt}
              onChange={(e) => setPrompt(e.target.value)}
              rows={3}
            />
            <Button
              onClick={handleSuggest}
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
          詳細パラメータ設定
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

        {/* 板厚設定 */}
        <div className="grid grid-cols-3 gap-4 border-t pt-4">
          <div className="space-y-2">
            <Label>天板厚</Label>
            <Input
              type="number"
              value={params.topThickness}
              onChange={(e) => handleChange("topThickness", e.target.value)}
            />
          </div>
          <div className="space-y-2">
            <Label>側板厚</Label>
            <Input
              type="number"
              value={params.sideThickness}
              onChange={(e) => handleChange("sideThickness", e.target.value)}
            />
          </div>
          <div className="space-y-2">
            <Label>棚板厚</Label>
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
              <Label>天板素材</Label>
              <Input
                value={params.topMaterialName}
                onChange={(e) =>
                  handleStringChange("topMaterialName", e.target.value)
                }
              />
            </div>
            <div className="space-y-2">
              <Label>側板素材</Label>
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
              <Label>棚板素材</Label>
              <Input
                value={params.shelfMaterialName}
                onChange={(e) =>
                  handleStringChange("shelfMaterialName", e.target.value)
                }
              />
            </div>
            <div className="space-y-2">
              <Label>棚板枚数</Label>
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
            "Revitファミリを生成"
          )}
        </Button>
      </div>
    </div>
  );
}
