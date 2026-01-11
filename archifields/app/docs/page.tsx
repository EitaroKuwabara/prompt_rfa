import React from "react";
import Link from "next/link";
import {Button} from "@/components/ui/button";
import {ArrowRight, Lightbulb, AlertTriangle, FileCode} from "lucide-react";

export default function DocsPage() {
  return (
    <div className="container max-w-4xl py-12 px-4 md:px-6">
      {/* ヘッダーエリア */}
      <div className="mb-12 space-y-4">
        <h1 className="text-4xl font-bold tracking-tight">User Guide</h1>
        <p className="text-xl text-muted-foreground">
          Learn how to generate Revit families with Archifields (Beta).
        </p>
      </div>

      <div className="grid gap-12">
        {/* Section 1: Quick Start */}
        <section className="space-y-6">
          <h2 className="text-2xl font-semibold flex items-center gap-2">
            🚀 Quick Start
          </h2>
          <div className="rounded-lg border bg-card p-6 shadow-sm">
            <ol className="relative border-l border-muted ml-3 space-y-8">
              <li className="mb-10 ml-6">
                <span className="absolute -left-3 flex h-6 w-6 items-center justify-center rounded-full bg-primary text-primary-foreground text-sm font-bold ring-4 ring-background">
                  1
                </span>
                <h3 className="mb-1 text-lg font-semibold">Go to Generator</h3>
                <p className="text-muted-foreground mb-2">
                  Navigate to the Generator page from the top menu.
                </p>
              </li>
              <li className="mb-10 ml-6">
                <span className="absolute -left-3 flex h-6 w-6 items-center justify-center rounded-full bg-primary text-primary-foreground text-sm font-bold ring-4 ring-background">
                  2
                </span>
                <h3 className="mb-1 text-lg font-semibold">Enter a Prompt</h3>
                <p className="text-muted-foreground mb-2">
                  Describe the furniture you want in English. <br />
                  <span className="text-sm italic">
                    e.g., &quot;Simple wooden office desk&quot;, &quot;Modern
                    bookshelf with 4 shelves&quot;
                  </span>
                </p>
              </li>
              <li className="mb-10 ml-6">
                <span className="absolute -left-3 flex h-6 w-6 items-center justify-center rounded-full bg-primary text-primary-foreground text-sm font-bold ring-4 ring-background">
                  3
                </span>
                <h3 className="mb-1 text-lg font-semibold">
                  Generate & Preview
                </h3>
                <p className="text-muted-foreground mb-2">
                  Click the &quot;Generate&quot; button. The AI will process
                  your request (approx. 30-60 seconds). Once done, a 3D preview
                  will appear in your browser.
                </p>
              </li>
              <li className="ml-6">
                <span className="absolute -left-3 flex h-6 w-6 items-center justify-center rounded-full bg-primary text-primary-foreground text-sm font-bold ring-4 ring-background">
                  4
                </span>
                <h3 className="mb-1 text-lg font-semibold">Download RFA</h3>
                <p className="text-muted-foreground mb-2">
                  If you like the result, click the &quot;Download .rfa&quot;
                  button. You can load this file directly into your Revit
                  project.
                </p>
              </li>
            </ol>
            <div className="mt-6 ml-6">
              <Button asChild>
                <Link href="/generator">
                  Try Generator Now <ArrowRight className="ml-2 h-4 w-4" />
                </Link>
              </Button>
            </div>
          </div>
        </section>

        {/* Section 2: Tips for Prompts */}
        <section className="space-y-4">
          <h2 className="text-2xl font-semibold flex items-center gap-2">
            <Lightbulb className="h-6 w-6 text-amber-500" /> Tips for Better
            Results
          </h2>
          <div className="grid gap-4 md:grid-cols-2">
            <div className="p-4 bg-muted/50 rounded-lg">
              <h3 className="font-semibold mb-2">✅ Do</h3>
              <ul className="list-disc list-inside text-sm text-muted-foreground space-y-1">
                <li>Specify dimensions (e.g., &quot;Width 1200mm&quot;).</li>
                <li>
                  Mention materials simple (e.g., &quot;Wood&quot;,
                  &quot;Glass&quot;).
                </li>
                <li>Keep it simple (e.g., &quot;Meeting table&quot;).</li>
              </ul>
            </div>
            <div className="p-4 bg-muted/50 rounded-lg">
              <h3 className="font-semibold mb-2">❌ Don&apos;t</h3>
              <ul className="list-disc list-inside text-sm text-muted-foreground space-y-1">
                <li>Complex organic shapes (e.g., &quot;Statue of a lion&quot;).</li>
                <li>Too many nested components.</li>
                <li>Non-furniture categories (e.g., &quot;Roof&quot;, &quot;Wall&quot;).</li>
              </ul>
            </div>
          </div>
        </section>

        {/* Section 3: Supported Categories */}
        <section className="space-y-4">
          <h2 className="text-2xl font-semibold flex items-center gap-2">
            <FileCode className="h-6 w-6 text-blue-500" /> Supported Categories
            (Beta)
          </h2>
          <p className="text-muted-foreground">
            Currently, the AI model is optimized for the following Revit
            categories. We are adding more categories every week.
          </p>
          <div className="flex flex-wrap gap-2">
            <span className="px-3 py-1 bg-secondary text-secondary-foreground rounded-full text-sm font-medium">
              Furniture (Desks, Tables)
            </span>
            <span className="px-3 py-1 bg-secondary text-secondary-foreground rounded-full text-sm font-medium">
              Casework (Shelves, Cabinets)
            </span>
            <span className="px-3 py-1 border border-dashed text-muted-foreground rounded-full text-sm">
              More coming soon...
            </span>
          </div>
        </section>

        {/* Section 4: Limitations */}
        <section className="space-y-4">
          <div className="bg-amber-50 dark:bg-amber-950/30 border border-amber-200 dark:border-amber-900 p-4 rounded-lg flex gap-3">
            <AlertTriangle className="h-5 w-5 text-amber-600 dark:text-amber-500 shrink-0 mt-0.5" />
            <div className="text-sm text-amber-800 dark:text-amber-200">
              <h3 className="font-semibold mb-1">Current Limitations</h3>
              <p>
                Archifields is in <strong>Public Beta</strong>. Generated
                families may sometimes have geometric errors or simplified
                parameters. Please always verify the dimensions and constraints
                before using them in production drawings.
              </p>
            </div>
          </div>
        </section>

        {/* Contact CTA */}
        <section className="border-t pt-8 mt-4">
          <h2 className="text-xl font-semibold mb-2">Need Help?</h2>
          <p className="text-muted-foreground mb-4">
            If you encounter any bugs or have feature requests, please contact
            us.
          </p>
          <Button variant="outline" asChild>
            <Link href="/contact">Contact Support</Link>
          </Button>
        </section>
      </div>
    </div>
  );
}
