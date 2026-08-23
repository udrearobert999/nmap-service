import { StrictMode } from "react"
import { createRoot } from "react-dom/client"
import { ClerkProvider } from "@clerk/react"
import { dark } from "@clerk/themes"
import "./index.css"
import App from "./App.tsx"

const PUBLISHABLE_KEY = import.meta.env.VITE_CLERK_PUBLISHABLE_KEY

const monoFont =
  'ui-monospace, "SF Mono", "JetBrains Mono", "Menlo", "Consolas", monospace'

const clerkAppearance = {
  baseTheme: dark,
  variables: {
    colorBackground: "#171717",
    colorInputBackground: "#1f1f1f",
    colorPrimary: "#ededed",
    colorText: "#ededed",
    colorTextSecondary: "#8c8c8c",
    colorInputText: "#ededed",
    colorNeutral: "#ededed",
    borderRadius: "0.5rem",
    fontFamily: monoFont,
  },
  elements: {
    card: "border border-[#292929]",
    organizationSwitcherTrigger: "text-[#c9c9c9] hover:bg-[#262626]",
    userButtonPopoverCard: "border border-[#292929]",
  },
}

function MissingKey() {
  return (
    <div className="flex min-h-screen flex-col items-center justify-center gap-4 p-6 text-center">
      <h1 className="text-xl font-semibold">Configuration required</h1>
      <p className="max-w-md text-sm text-muted-foreground">
        Set <code className="font-mono">VITE_CLERK_PUBLISHABLE_KEY</code> in{" "}
        <code className="font-mono">.env.local</code> — see the README. A Clerk
        development instance with Organizations enabled is required to run the
        portal.
      </p>
    </div>
  )
}

createRoot(document.getElementById("root")!).render(
  <StrictMode>
    {PUBLISHABLE_KEY ? (
      <ClerkProvider
        publishableKey={PUBLISHABLE_KEY}
        afterSignOutUrl="/"
        appearance={clerkAppearance}
      >
        <App />
      </ClerkProvider>
    ) : (
      <MissingKey />
    )}
  </StrictMode>,
)
