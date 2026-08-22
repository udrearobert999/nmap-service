import { QueryClient, QueryClientProvider } from "@tanstack/react-query"
import { BrowserRouter, Route, Routes } from "react-router-dom"
import { SubjectProvider } from "@/lib/subject-context"
import { AppShell } from "@/components/AppShell"
import { ScansListPage } from "@/pages/ScansListPage"
import { ScanDetailPage } from "@/pages/ScanDetailPage"
import { Toaster } from "@/components/ui/sonner"

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      retry: 1,
      refetchOnWindowFocus: false,
    },
  },
})

export default function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <SubjectProvider>
        <BrowserRouter>
          <Routes>
            <Route element={<AppShell />}>
              <Route path="/" element={<ScansListPage />} />
              <Route path="/scans/:id" element={<ScanDetailPage />} />
            </Route>
          </Routes>
        </BrowserRouter>
        <Toaster richColors position="top-right" />
      </SubjectProvider>
    </QueryClientProvider>
  )
}
