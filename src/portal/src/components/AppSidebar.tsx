import { Link, useLocation } from "react-router-dom"
import { OrganizationSwitcher, UserButton } from "@clerk/react"
import { LayoutDashboard, Radar, ShieldCheck } from "lucide-react"
import {
  Sidebar,
  SidebarContent,
  SidebarFooter,
  SidebarGroup,
  SidebarGroupLabel,
  SidebarHeader,
  SidebarMenu,
  SidebarMenuButton,
  SidebarMenuItem,
} from "@/components/ui/sidebar"

const navItems = [
  { title: "Dashboard", to: "/", icon: LayoutDashboard, exact: true },
  { title: "Scans", to: "/scans", icon: Radar, exact: false },
]

export function AppSidebar() {
  const { pathname } = useLocation()

  const isActive = (to: string, exact: boolean) =>
    exact ? pathname === to : pathname === to || pathname.startsWith(`${to}/`)

  return (
    <Sidebar>
      <SidebarHeader className="border-b">
        <div className="flex items-center gap-2 px-2 py-1.5">
          <ShieldCheck className="h-5 w-5 text-primary" />
          <span className="text-sm font-semibold tracking-tight">
            Vantage
          </span>
        </div>
      </SidebarHeader>

      <SidebarContent>
        <SidebarGroup>
          <SidebarGroupLabel>Platform</SidebarGroupLabel>
          <SidebarMenu>
            {navItems.map((item) => (
              <SidebarMenuItem key={item.to}>
                <SidebarMenuButton
                  asChild
                  isActive={isActive(item.to, item.exact)}
                  tooltip={item.title}
                >
                  <Link to={item.to}>
                    <item.icon />
                    <span>{item.title}</span>
                  </Link>
                </SidebarMenuButton>
              </SidebarMenuItem>
            ))}
          </SidebarMenu>
        </SidebarGroup>
      </SidebarContent>

      <SidebarFooter className="border-t">
        <div className="flex w-full items-center justify-between gap-3 px-2 py-1 group-data-[collapsible=icon]:flex-col group-data-[collapsible=icon]:gap-2">
          <div className="min-w-0 flex-1 overflow-hidden">
            <OrganizationSwitcher
              hidePersonal
              afterSelectOrganizationUrl="/"
              afterCreateOrganizationUrl="/"
            />
          </div>
          <div className="shrink-0">
            <UserButton />
          </div>
        </div>
      </SidebarFooter>
    </Sidebar>
  )
}
