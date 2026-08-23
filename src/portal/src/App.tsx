import {
  MutationCache,
  QueryCache,
  QueryClient,
  QueryClientProvider,
} from "@tanstack/react-query"
import { BrowserRouter, Route, Routes } from "react-router-dom"
import { RedirectToSignIn, SignedIn, SignedOut } from "@clerk/clerk-react"
import { toast } from "sonner"
import { ApiError } from "@/api/client"
import { AuthBridge } from "@/components/AuthBridge"
import { OrgGate } from "@/components/OrgGate"
import { AppShell } from "@/components/AppShell"
import { DashboardPage } from "@/pages/DashboardPage"
import { ScansListPage } from "@/pages/ScansListPage"
import { ScanDetailPage } from "@/pages/ScanDetailPage"
import { Toaster } from "@/components/ui/sonner"

function notifyIfNoOrg(error: unknown, key?: unknown) {
  if (error instanceof ApiError && error.status === 403) {
    if (Array.isArray(key) && key[0] === "health") return
    toast.error("No active organization", {
      id: "no-active-org",
      description:
        "Select or create a team to continue. Your account may not have access to this team on the backend.",
    })
  }
}

const queryClient = new QueryClient({
  queryCache: new QueryCache({
    onError: (error, query) => notifyIfNoOrg(error, query.queryKey),
  }),
  mutationCache: new MutationCache({
    onError: (error) => notifyIfNoOrg(error),
  }),
  defaultOptions: {
    queries: { retry: 1, refetchOnWindowFocus: false },
  },
})

export default function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <SignedOut>
        <RedirectToSignIn />
      </SignedOut>
      <SignedIn>
        <AuthBridge />
        <OrgGate>
          <BrowserRouter>
            <Routes>
              <Route element={<AppShell />}>
                <Route path="/" element={<DashboardPage />} />
                <Route path="/scans" element={<ScansListPage />} />
                <Route path="/scans/:id" element={<ScanDetailPage />} />
              </Route>
            </Routes>
          </BrowserRouter>
        </OrgGate>
      </SignedIn>
      <Toaster richColors position="top-right" />
    </QueryClientProvider>
  )
}
