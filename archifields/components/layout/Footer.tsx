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
              Make architecture design more free.
              <br />
              Anyone can easily create and share BIM families with the next generation platform.
            </p>
          </div>

          {/* リンク集 1 */}
          <div className="space-y-4">
            <h4 className="text-sm font-semibold">Platform</h4>
            <ul className="space-y-2 text-sm text-muted-foreground">
              <li>
                <Link href="/marketplace" className="hover:text-foreground">
                  Marketplace
                </Link>
              </li>
              <li>
                <Link href="/generator" className="hover:text-foreground">
                  Generator
                </Link>
              </li>
              <li>
                <Link href="/pricing" className="hover:text-foreground">
                  Pricing Plan
                </Link>
              </li>
            </ul>
          </div>

          {/* リンク集 2 */}
          <div className="space-y-4">
            <h4 className="text-sm font-semibold">Support</h4>
            <ul className="space-y-2 text-sm text-muted-foreground">
              <li>
                <Link href="/docs" className="hover:text-foreground">
                  Usage Guide
                </Link>
              </li>
              <li>
                <Link href="/manufacturers" className="hover:text-foreground">
                  Guide for Manufacturers
                </Link>
              </li>
              <li>
                <Link href="/contact" className="hover:text-foreground">
                  Contact
                </Link>
              </li>
            </ul>
          </div>

          {/* リンク集 3 */}
          <div className="space-y-4">
            <h4 className="text-sm font-semibold">Legal Information</h4>
            <ul className="space-y-2 text-sm text-muted-foreground">
              <li>
                <Link href="/legal/terms" className="hover:text-foreground">
                  Terms of Service
                </Link>
              </li>
              <li>
                <Link href="/legal/privacy" className="hover:text-foreground">
                  Privacy Policy
                </Link>
              </li>
              <li>
                <Link href="/legal/commercial" className="hover:text-foreground">
                  Notation based on the Specified Commercial Transactions Act
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
