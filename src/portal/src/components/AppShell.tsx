import { Link, Outlet } from "react-router-dom"
import { ShieldCheck } from "lucide-react"
import { TeamSwitcher } from "./TeamSwitcher"
import { useSubject } from "@/lib/subject-context"

export function AppShell() {
  const { subject } = useSubject()

  return (
    <div className="min-h-screen bg-background">
      <header className="sticky top-0 z-10 border-b bg-background/95 backdrop-blur">
        <div className="container flex h-16 items-center justify-between gap-4">
          <Link to="/" className="flex items-center gap-2 font-semibold">
            <ShieldCheck className="h-6 w-6 text-primary" />
            <span className="text-lg">Network Security Portal</span>
          </Link>
          <div className="flex items-center gap-3">
            <span className="hidden text-xs text-muted-foreground sm:inline">
              acting as
            </span>
            <span className="hidden rounded bg-muted px-2 py-1 font-mono text-xs sm:inline">
              {subject}
            </span>
            <TeamSwitcher />
          </div>
        </div>
      </header>
      <main className="container py-8">
        <Outlet />
      </main>
    </div>
  )
}
