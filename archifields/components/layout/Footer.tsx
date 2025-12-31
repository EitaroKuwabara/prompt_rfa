import Link from "next/link";
import {Package} from "lucide-react";

export function Footer() {
  return (
    <footer className="border-t bg-muted/40">
      <div className="container px-4 md:px-6 py-10">
        <div className="grid grid-cols-1 gap-8 md:grid-cols-4 lg:grid-cols-5">
          {/* ブランド情報 */}
          <div className="lg:col-span-2">
            <Link href="/" className="flex items-center gap-2 mb-4">
              <Package className="h-6 w-6 text-primary" />
              <span className="text-xl font-bold">Archifields</span>
            </Link>
            <p className="text-sm text-muted-foreground max-w-xs mb-6">
              建築設計をもっと自由に。
              <br />
              誰でも簡単にBIMファミリを作成・共有できる次世代のプラットフォームです。
            </p>
          </div>

          {/* リンク集 1 */}
          <div className="space-y-4">
            <h4 className="text-sm font-semibold">プラットフォーム</h4>
            <ul className="space-y-2 text-sm text-muted-foreground">
              <li>
                <Link href="/marketplace" className="hover:text-foreground">
                  マーケットプレイス
                </Link>
              </li>
              <li>
                <Link href="/generator" className="hover:text-foreground">
                  AIジェネレーター
                </Link>
              </li>
              <li>
                <Link href="/pricing" className="hover:text-foreground">
                  料金プラン
                </Link>
              </li>
            </ul>
          </div>

          {/* リンク集 2 */}
          <div className="space-y-4">
            <h4 className="text-sm font-semibold">サポート</h4>
            <ul className="space-y-2 text-sm text-muted-foreground">
              <li>
                <Link href="/docs" className="hover:text-foreground">
                  使い方ガイド
                </Link>
              </li>
              <li>
                <Link href="/manufacturers" className="hover:text-foreground">
                  メーカー向けガイド
                </Link>
              </li>
              <li>
                <Link href="/contact" className="hover:text-foreground">
                  お問い合わせ
                </Link>
              </li>
            </ul>
          </div>

          {/* リンク集 3 */}
          <div className="space-y-4">
            <h4 className="text-sm font-semibold">法的情報</h4>
            <ul className="space-y-2 text-sm text-muted-foreground">
              <li>
                <Link href="/terms" className="hover:text-foreground">
                  利用規約
                </Link>
              </li>
              <li>
                <Link href="/privacy" className="hover:text-foreground">
                  プライバシーポリシー
                </Link>
              </li>
              <li>
                <Link href="/commercial" className="hover:text-foreground">
                  特定商取引法に基づく表記
                </Link>
              </li>
            </ul>
          </div>
        </div>

        {/* コピーライト */}
        <div className="mt-10 border-t pt-6 flex flex-col md:flex-row justify-between items-center gap-4">
          <p className="text-xs text-muted-foreground">
            &copy; {new Date().getFullYear()} Archifields. All rights reserved.
          </p>
        </div>
      </div>
    </footer>
  );
}
