import Link from "next/link";
import {Button} from "@/components/ui/button";
import {ArrowRight, Box, Code, Upload} from "lucide-react";

export default function Home() {
  return (
    <div className="flex flex-col min-h-screen">
      {/* ヒーローセクション */}
      <section className="flex-1 flex flex-col items-center justify-center space-y-10 px-4 md:px-6 py-24 text-center bg-linear-to-b from-background to-muted/20">
        <div className="space-y-4 max-w-3xl">
          <h1 className="text-4xl font-extrabold tracking-tighter sm:text-5xl md:text-6xl lg:text-7xl">
            BIMファミリを、
            <br className="hidden sm:inline" />
            <span className="text-primary">もっと自由に、誰でも簡単に。</span>
          </h1>
          <p className="mx-auto max-w-[700px] text-muted-foreground md:text-xl">
            Archifieldsは、設計者とメーカーをつなぐBIMプラットフォームです。
            AIによる自動生成機能で、必要なファミリを数秒で手に入れましょう。
          </p>
        </div>
        <div className="flex flex-col sm:flex-row gap-4">
          <Link href="/generator">
            <Button size="lg" className="h-12 px-8 text-lg">
              今すぐ生成する <ArrowRight className="ml-2 h-5 w-5" />
            </Button>
          </Link>
          <Link href="/marketplace">
            <Button variant="outline" size="lg" className="h-12 px-8 text-lg">
              マーケットを見る
            </Button>
          </Link>
        </div>
      </section>

      {/* 機能紹介セクション */}
      <section className="container px-4 md:px-6 py-12 md:py-24">
        <div className="grid gap-12 sm:grid-cols-2 lg:grid-cols-3">
          <div className="flex flex-col items-center space-y-4 text-center">
            <div className="p-4 bg-primary/10 rounded-full">
              <Code className="h-8 w-8 text-primary" />
            </div>
            <h3 className="text-xl font-bold">パラメトリック生成</h3>
            <p className="text-muted-foreground">
              寸法を入力するだけで、完全に拘束されたRevitファミリ(RFA)を自動コード生成します。
            </p>
          </div>
          <div className="flex flex-col items-center space-y-4 text-center">
            <div className="p-4 bg-primary/10 rounded-full">
              <Box className="h-8 w-8 text-primary" />
            </div>
            <h3 className="text-xl font-bold">マーケットプレイス</h3>
            <p className="text-muted-foreground">
              世界中のクリエイターやメーカーが作成した高品質なファミリを検索・ダウンロードできます。
            </p>
          </div>
          <div className="flex flex-col items-center space-y-4 text-center">
            <div className="p-4 bg-primary/10 rounded-full">
              <Upload className="h-8 w-8 text-primary" />
            </div>
            <h3 className="text-xl font-bold">メーカー向け機能</h3>
            <p className="text-muted-foreground">
              自社製品のファミリをアップロードして、世界中の設計者に使ってもらいましょう。
            </p>
          </div>
        </div>
      </section>
    </div>
  );
}
