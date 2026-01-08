// archifields/hooks/useFamilyGenerator.ts
import {useState} from "react";

// 共通で使う型の定義
interface BaseParams {
  [key: string]: string | number | boolean;
}

export function useFamilyGenerator<T extends BaseParams>(
  initialParams: T,
  type: string, // "Desk" or "Shelf"
  onSuccess: () => void
) {
  const [params, setParams] = useState<T>(initialParams);
  const [prompt, setPrompt] = useState("");
  const [isAnalyzing, setIsAnalyzing] = useState(false);
  const [isGenerating, setIsGenerating] = useState(false);
  const API_BASE = process.env.NEXT_PUBLIC_API_URL || "http://localhost:8000";

  // AI提案ロジック
  const handleSuggest = async (category: string) => {
    if (!prompt) return;
    setIsAnalyzing(true);
    try {
      const res = await fetch(`${API_BASE}/suggest`, {
        method: "POST",
        headers: {"Content-Type": "application/json"},
        body: JSON.stringify({prompt, category}),
      });
      if (!res.ok) throw new Error("Suggestion failed");

      const data = await res.json();

      // AIからの提案を適用 (数値型への変換を考慮)
      setParams((prev) => {
        const next = {...prev, ...data};
        // もしprevにあるキーが数値なら、data側も数値に変換してマージ
        Object.keys(prev).forEach((key) => {
          if (typeof prev[key] === "number" && next[key] !== undefined) {
            next[key] = Number(next[key]);
          }
        });
        return next;
      });
    } catch (error) {
      console.error(error);
      alert("AI提案に失敗しました");
    } finally {
      setIsAnalyzing(false);
    }
  };

  // 生成ロジック (完了まで待機し、自動でダウンロードボタンを表示)
  const handleGenerate = async () => {
    setIsGenerating(true);
    try {
      // 完了するまでここで待機します (クラウド処理の時間分待ちます)
      const res = await fetch(`${API_BASE}/generate`, {
        method: "POST",
        headers: {"Content-Type": "application/json"},
        body: JSON.stringify({
          command: "create",
          type: type, // "Desk" or "Shelf"
          params: params,
        }),
      });

      if (!res.ok) throw new Error("Generation failed");

      // ★ここがポイント：アラートを出さずに完了通知を送る
      console.log("Generation Complete!");
      onSuccess();
    } catch (error) {
      console.error(error);
      alert(
        "生成リクエストに失敗しました。\nバックエンドが起動しているか確認してください。"
      );
    } finally {
      setIsGenerating(false);
    }
  };

  // 汎用ハンドラ (数値)
  const handleChange = (key: keyof T, value: string) => {
    const numVal = parseFloat(value);
    setParams((prev) => ({
      ...prev,
      [key]: isNaN(numVal) ? 0 : numVal,
    }));
  };

  // 汎用ハンドラ (文字列)
  const handleStringChange = (key: keyof T, value: string) => {
    setParams((prev) => ({
      ...prev,
      [key]: value || "",
    }));
  };

  // トグルハンドラ (Boolean)
  const handleCheckboxChange = (key: keyof T, checked: boolean) => {
    setParams((prev) => ({
      ...prev,
      [key]: checked,
    }));
  };

  return {
    params,
    setParams,
    prompt,
    setPrompt,
    isAnalyzing,
    isGenerating,
    handleSuggest,
    handleGenerate,
    handleChange,
    handleStringChange,
    handleCheckboxChange,
  };
}
