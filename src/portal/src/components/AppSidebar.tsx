import { Link, useLocation } from "react-router-dom"
import { OrganizationSwitcher, UserButton } from "@clerk/clerk-react"
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
            Network Security
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
        <div className="flex items-center justify-between gap-2 px-1 py-1 group-data-[collapsible=icon]:flex-col">
          <OrganizationSwitcher
            hidePersonal
            afterSelectOrganizationUrl="/"
            afterCreateOrganizationUrl="/"
          />
          <UserButton />
        </div>
      </SidebarFooter>
    </Sidebar>
  )
}
