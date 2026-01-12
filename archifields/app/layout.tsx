import type { Metadata } from "next";
// import { Geist, Geist_Mono } from "next/font/google";
import "./globals.css";
import { Inter } from "next/font/google";
import { Analytics } from "@vercel/analytics/react";
import { Header } from "@/components/layout/Header";
import { Footer } from "@/components/layout/Footer";
import { BetaBanner } from "@/components/layout/BetaBanner";
const inter = Inter({subsets: ["latin"]});

export const metadata: Metadata = {
  title: "Archifields - BIM Family Marketplace & Generator",
  description: "Generate and share Revit families easily.",
};

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <html lang="en">
      <body className={inter.className}>
        <div className="flex min-h-screen flex-col">
          <BetaBanner />
          <Header />
          <main className="flex-1 bg-background">{children}</main>
          <Footer />
        </div>
        <Analytics />
      </body>
    </html>
  );
}




