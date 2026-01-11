import Link from "next/link";
import {AlertTriangle} from "lucide-react";

export function BetaBanner() {
  return (
    <div className="bg-amber-100 px-4 py-2 text-center text-sm font-medium text-amber-900 dark:bg-amber-900/30 dark:text-amber-100">
      <div className="container flex items-center justify-center gap-2">
        <AlertTriangle className="h-4 w-4" />
        <span>
          <b>Public Beta:</b> Archifields is currently in active development.
          Encountered a bug or have feedback?{" "}
          <Link
            href="/contact"
            className="underline underline-offset-4 hover:text-amber-700 dark:hover:text-amber-50"
          >
            Please let us know
          </Link>
          .
        </span>
      </div>
    </div>
  );
}
