import type { ReactNode } from "react"
import {
  CreateOrganization,
  OrganizationList,
  useOrganization,
} from "@clerk/clerk-react"
import { ShieldCheck } from "lucide-react"

export function OrgGate({ children }: { children: ReactNode }) {
  const { organization, isLoaded } = useOrganization()

  if (!isLoaded) {
    return (
      <div className="flex min-h-screen items-center justify-center text-sm text-muted-foreground">
        Loading…
      </div>
    )
  }

  if (!organization) {
    return (
      <div className="flex min-h-screen flex-col items-center justify-center gap-6 p-6">
        <div className="flex items-center gap-2 text-lg font-semibold">
          <ShieldCheck className="h-6 w-6 text-primary" />
          Vantage
        </div>
        <p className="max-w-md text-center text-sm text-muted-foreground">
          You need an active team (organization) to continue. Select an existing
          team or create a new one below.
        </p>
        <OrganizationList
          hidePersonal
          afterSelectOrganizationUrl="/"
          afterCreateOrganizationUrl="/"
        />
        <div className="text-xs text-muted-foreground">or create a new team:</div>
        <CreateOrganization afterCreateOrganizationUrl="/" />
      </div>
    )
  }

  return <>{children}</>
}
