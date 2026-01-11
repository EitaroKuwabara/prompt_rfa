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
            BIM families,
            <br className="hidden sm:inline" />
            <span className="text-primary">make it more free and easy for anyone.</span>
          </h1>
          <p className="mx-auto max-w-[700px] text-muted-foreground md:text-xl">
            Archifields is a BIM platform that connects designers and manufacturers.
            With the automatic generation function of AI, you can get the necessary families in seconds.
          </p>
        </div>
        <div className="flex flex-col sm:flex-row gap-4">
          <Link href="/generator">
            <Button size="lg" className="h-12 px-8 text-lg">
              Generate now <ArrowRight className="ml-2 h-5 w-5" />
            </Button>
          </Link>
          <Link href="/marketplace">
            <Button variant="outline" size="lg" className="h-12 px-8 text-lg">
              See Marketplace
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
            <h3 className="text-xl font-bold">Parametric Generation</h3>
            <p className="text-muted-foreground">
              By entering dimensions, you can automatically generate a fully constrained Revit family (RFA).
            </p>
          </div>
          <div className="flex flex-col items-center space-y-4 text-center">
            <div className="p-4 bg-primary/10 rounded-full">
              <Box className="h-8 w-8 text-primary" />
            </div>
            <h3 className="text-xl font-bold">Marketplace</h3>
            <p className="text-muted-foreground">
              You can search and download high-quality families created by creators and manufacturers from all over the world.
            </p>
          </div>
          <div className="flex flex-col items-center space-y-4 text-center">
            <div className="p-4 bg-primary/10 rounded-full">
              <Upload className="h-8 w-8 text-primary" />
            </div>
            <h3 className="text-xl font-bold">Features for Manufacturers</h3>
            <p className="text-muted-foreground">
              Upload your own families and make them available to designers all over the world.
            </p>
          </div>
        </div>
      </section>
    </div>
  );
}
