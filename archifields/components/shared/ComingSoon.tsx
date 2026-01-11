import Link from "next/link";
import {Button} from "@/components/ui/button";
import {Hammer} from "lucide-react";

interface ComingSoonProps {
  title: string;
  description: string;
}

export function ComingSoon({title, description}: ComingSoonProps) {
  return (
    <div className="flex h-[calc(100vh-10rem)] flex-col items-center justify-center space-y-4 text-center px-4">
      <div className="rounded-full bg-muted p-4">
        <Hammer className="h-10 w-10 text-muted-foreground" />
      </div>
      <h1 className="text-3xl font-bold tracking-tighter sm:text-4xl">
        {title}
      </h1>
      <p className="max-w-[600px] text-muted-foreground md:text-xl">
        {description}
      </p>
      <div className="flex gap-4">
        <Button asChild>
          <Link href="/">Back to Home</Link>
        </Button>
        <Button variant="outline" asChild>
          <Link href="/contact">Contact Us</Link>
        </Button>
      </div>
    </div>
  );
}
