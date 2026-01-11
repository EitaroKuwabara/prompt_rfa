// archifields/app/generator/page.tsx
"use client";

import {useState} from "react";
import {Download} from "lucide-react";
import {Button} from "@/components/ui/button";
import {Tabs, TabsList, TabsTrigger, TabsContent} from "@/components/ui/tabs";
import {DeskGenerator} from "@/components/generators/DeskGenerator";
import {ShelfGenerator} from "@/components/generators/ShelfGenerator";

export default function GeneratorPage() {
  const [previewUrl, setPreviewUrl] = useState<string | null>(null);

  const API_BASE = process.env.NEXT_PUBLIC_API_URL || "http://localhost:8000";

  const handleGenerateSuccess = () => {
    setPreviewUrl(`${API_BASE}/preview/latest?t=${Date.now()}`);
  };

  const handleDownload = () => {
    const timestamp = Date.now();
    window.open(`${API_BASE}/download/latest?t=${timestamp}`, "_blank");
  };

  return (
    <div className="container flex flex-col items-center justify-center min-h-[calc(100vh-14rem)] py-10">
      <div className="max-w-3xl w-full grid gap-8 lg:grid-cols-2">
        {/* 左側: 入力エリア (タブでコンポーネント切り替え) */}
        <div className="flex flex-col space-y-6">
          <div className="space-y-2">
            <h1 className="text-3xl font-bold tracking-tighter sm:text-4xl">
              Family Generator
            </h1>
            <p className="text-muted-foreground">
              Select the category and design by words or numbers.
            </p>
          </div>

          <Tabs defaultValue="desk" className="w-full">
            <TabsList className="grid w-full grid-cols-2 mb-4">
              <TabsTrigger value="desk">Desk (Desk)</TabsTrigger>
              <TabsTrigger value="shelf">Shelf (Shelf)</TabsTrigger>
            </TabsList>

            {/* 机コンポーネント呼び出し */}
            <TabsContent value="desk">
              <DeskGenerator onSuccess={handleGenerateSuccess} />
            </TabsContent>

            {/* 棚コンポーネント呼び出し */}
            <TabsContent value="shelf">
              <ShelfGenerator onSuccess={handleGenerateSuccess} />
            </TabsContent>
          </Tabs>
        </div>

        {/* 右側: プレビューエリア (共通) */}
        <div className="hidden lg:flex flex-col items-center justify-center rounded-lg border bg-muted/50 p-8 text-center min-h-[400px] sticky top-24">
          {previewUrl ? (
            <div className="flex flex-col items-center space-y-4 w-full h-full">
              <div className="relative w-full h-64 bg-white rounded-md overflow-hidden shadow-sm flex items-center justify-center border">
                {/* eslint-disable-next-line @next/next/no-img-element */}
                <img
                  src={previewUrl}
                  alt="Preview"
                  className="object-contain max-h-full max-w-full"
                />
              </div>
              <h3 className="text-xl font-semibold text-green-600">Generation Complete</h3>
              <Button
                onClick={handleDownload}
                className="w-full max-w-xs"
                variant="default"
              >
                <Download className="mr-2 h-4 w-4" /> Download (.rfa)
              </Button>
            </div>
          ) : (
            <div className="flex flex-col items-center justify-center space-y-4">
              <div className="flex h-40 w-40 items-center justify-center rounded-full bg-background shadow-sm">
                <Download className="h-10 w-10 text-muted-foreground" />
              </div>
              <h3 className="text-xl font-semibold">Preview Preparation</h3>
              <p className="text-sm text-muted-foreground max-w-xs">
                When you press the generate button, the 3D preview of the completed family will be displayed here.
              </p>
            </div>
          )}
        </div>
      </div>
    </div>
  );
}
