"use client";

import Link from "next/link";
import {Search, Menu, Package, UploadCloud} from "lucide-react";
import {Button} from "@/components/ui/button";
import {Input} from "@/components/ui/input";
import {Sheet, SheetContent, SheetTrigger} from "@/components/ui/sheet";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import {Avatar, AvatarFallback, AvatarImage} from "@/components/ui/avatar";

export function Header() {
  return (
    <header className="sticky top-0 z-50 w-full border-b bg-background/95 backdrop-blur supports-backdrop-filter:bg-background/60">
      <div className="container flex h-16 items-center justify-between px-4 md:px-6">
        {/* 左側: ロゴとデスクトップナビゲーション */}
        <div className="flex items-center gap-6 md:gap-10">
          <Link href="/" className="flex items-center gap-2">
            <Package className="h-6 w-6 text-primary" />
            <span className="text-xl font-bold tracking-tight">
              Archifields
            </span>
          </Link>
          <nav className="hidden md:flex gap-6">
            <Link
              href="/marketplace"
              className="text-sm font-medium transition-colors hover:text-primary"
            >
              マーケットプレイス
            </Link>
            <Link
              href="/generator"
              className="text-sm font-medium transition-colors hover:text-primary"
            >
              ジェネレーター
            </Link>
            <Link
              href="/manufacturers"
              className="text-sm font-medium transition-colors hover:text-primary"
            >
              メーカーの方へ
            </Link>
          </nav>
        </div>

        {/* 中央: 検索バー (デスクトップのみ) */}
        <div className="hidden md:flex flex-1 items-center justify-center px-6">
          <div className="relative w-full max-w-sm">
            <Search className="absolute left-2.5 top-2.5 h-4 w-4 text-muted-foreground" />
            <Input
              type="search"
              placeholder="ファミリを検索 (例: 椅子, TOTO, 窓)..."
              className="w-full bg-background pl-8 md:w-[300px] lg:w-[400px]"
            />
          </div>
        </div>

        {/* 右側: アクションボタン */}
        <div className="flex items-center gap-2">
          {/* アップロードボタン (メーカー/クリエイター向け) */}
          <Button
            variant="ghost"
            size="icon"
            className="hidden md:flex"
            title="ファミリをアップロード"
          >
            <UploadCloud className="h-5 w-5" />
          </Button>

          {/* ユーザーメニュー */}
          <DropdownMenu>
            <DropdownMenuTrigger asChild>
              <Button variant="ghost" className="relative h-8 w-8 rounded-full">
                <Avatar className="h-8 w-8">
                  <AvatarImage src="/avatars/01.png" alt="@user" />
                  <AvatarFallback>U</AvatarFallback>
                </Avatar>
              </Button>
            </DropdownMenuTrigger>
            <DropdownMenuContent className="w-56" align="end" forceMount>
              <DropdownMenuLabel className="font-normal">
                <div className="flex flex-col space-y-1">
                  <p className="text-sm font-medium leading-none">
                    ゲストユーザー
                  </p>
                  <p className="text-xs leading-none text-muted-foreground">
                    guest@example.com
                  </p>
                </div>
              </DropdownMenuLabel>
              <DropdownMenuSeparator />
              <DropdownMenuItem>ダッシュボード</DropdownMenuItem>
              <DropdownMenuItem>作成したファミリ</DropdownMenuItem>
              <DropdownMenuItem>お気に入り</DropdownMenuItem>
              <DropdownMenuSeparator />
              <DropdownMenuItem>ログアウト</DropdownMenuItem>
            </DropdownMenuContent>
          </DropdownMenu>

          {/* モバイルメニュー (ハンバーガー) */}
          <Sheet>
            <SheetTrigger asChild>
              <Button variant="ghost" size="icon" className="md:hidden">
                <Menu className="h-5 w-5" />
                <span className="sr-only">Toggle menu</span>
              </Button>
            </SheetTrigger>
            <SheetContent side="right">
              <nav className="grid gap-6 text-lg font-medium">
                <Link
                  href="/"
                  className="flex items-center gap-2 text-lg font-semibold"
                >
                  <Package className="h-6 w-6" />
                  <span>Archifields</span>
                </Link>
                <Link href="/marketplace" className="hover:text-primary">
                  マーケットプレイス
                </Link>
                <Link href="/generator" className="hover:text-primary">
                  ジェネレーター
                </Link>
                <Link href="/manufacturers" className="hover:text-primary">
                  メーカーの方へ
                </Link>
                <Link href="/login" className="hover:text-primary">
                  ログイン / 登録
                </Link>
              </nav>
            </SheetContent>
          </Sheet>
        </div>
      </div>
    </header>
  );
}
