import { useEffect } from "react"
import { useAuth, useUser } from "@clerk/react"
import { useQueryClient } from "@tanstack/react-query"
import { setAuth } from "@/api/auth-headers"

export function AuthBridge() {
  const { getToken, userId, orgId, orgRole } = useAuth()
  const { user } = useUser()
  const queryClient = useQueryClient()

  const email = user?.primaryEmailAddress?.emailAddress ?? null
  const role = orgRole ? orgRole.replace(/^org:/, "") : null

  useEffect(() => {
    setAuth({
      userId: userId ?? null,
      orgId: orgId ?? null,
      orgRole: role,
      email,
      getToken: () => getToken(),
    })
  }, [userId, orgId, role, email, getToken])

  useEffect(() => {
    queryClient.clear()
  }, [orgId, queryClient])

  return null
}
