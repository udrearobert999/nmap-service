import { Outlet, useLocation } from "react-router-dom"
import { AppSidebar } from "./AppSidebar"
import { ConnectivityIndicator } from "./ConnectivityIndicator"
import {
  SidebarInset,
  SidebarProvider,
  SidebarTrigger,
} from "@/components/ui/sidebar"
import { Separator } from "@/components/ui/separator"

function useTitle(): string {
  const { pathname } = useLocation()
  if (pathname === "/") return "Dashboard"
  if (pathname.startsWith("/scans")) return "Scans"
  return "Vantage"
}

export function AppShell() {
  const title = useTitle()

  return (
    <SidebarProvider>
      <AppSidebar />
      <SidebarInset>
        <header className="sticky top-0 z-10 flex h-14 shrink-0 items-center gap-2 border-b bg-background/95 px-4 backdrop-blur">
          <SidebarTrigger className="-ml-1" />
          <Separator orientation="vertical" className="mr-2 h-4" />
          <span className="text-sm font-medium">{title}</span>
          <div className="ml-auto">
            <ConnectivityIndicator />
          </div>
        </header>
        <main className="flex-1 p-4 md:p-6">
          <Outlet />
        </main>
      </SidebarInset>
    </SidebarProvider>
  )
}
