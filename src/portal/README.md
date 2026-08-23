# Network Security Portal

Web UI ("portal") for the Network Security Platform. Queue nmap scans, watch
them progress live, inspect results, and request risk assessments — all scoped
to the **team (organization)** you're signed in to via Clerk.

## Stack

- **Vite 5** + **React 18** + **TypeScript**
- **Tailwind CSS v3** + **shadcn/ui** components
- **@clerk/clerk-react** for authentication + organization (team) management
- **@tanstack/react-query** for data fetching + polling
- **react-router-dom** for routing
- **sonner** for toasts

Targets Node 18 (Vite 5.x). Built and verified with Node v18.18.2 / npm 9.8.1.

## Prerequisites — Clerk

Authentication is handled by **Clerk**. You need a **Clerk development instance**
with **Organizations enabled**:

1. Create an application at https://dashboard.clerk.com.
2. Enable **Organizations** (Configure → Organizations).
3. Copy the **Publishable key** (starts with `pk_test_`).

### Configure the key

Vite only exposes env vars prefixed with `VITE_`. Put your key in `.env.local`
(git-ignored):

```bash
cd src/portal
echo 'VITE_CLERK_PUBLISHABLE_KEY=pk_test_your_key_here' > .env.local
```

See `.env.example` for the variable name. If the key is missing, the app renders
a "set your key" message instead of crashing.

## Run

```bash
cd src/portal
npm install
npm run dev
```

Dev URL: **http://localhost:5173**

```bash
npm run build     # tsc -b && vite build
npm run preview
npm run lint
```

## Auth & multi-tenancy

Real authentication uses Clerk (`<SignedIn>` / `<SignedOut>` +
`<RedirectToSignIn>`); signed-out users can't reach the app. The app requires an
**active organization (team)** — with none, a gate screen
(`<CreateOrganization />` / `<OrganizationList hidePersonal />`) asks you to
create or select one. Switch/manage teams from the header
`<OrganizationSwitcher hidePersonal />`; manage your account via `<UserButton />`.

### The dev-backend bridge

In Development the backend does **not** validate real Clerk JWTs yet — it trusts
dev headers. `<AuthBridge/>` reads the live Clerk session and the API client
(`src/api/client.ts`, via `src/api/auth-headers.ts`) attaches on every request:

- `X-Dev-Subject` — Clerk **userId**
- `X-Dev-Org` — active **organization id** (the tenant/team)
- `X-Dev-Role` — membership role (`admin` / `member`; `org:` prefix stripped)
- `X-Dev-Email` — primary email (for "created by" display)
- `Authorization: Bearer <clerk jwt>` — harmless to the dev backend today,
  forward-compatible for real JWT validation
- `X-Idempotency-Key` — fresh `crypto.randomUUID()` on create actions

Switching the active org clears the React Query cache so lists refetch for the
new team. A request that returns **403** (no active org / no backend access)
surfaces a toast.

## Pages / features

- **`/` — Scans list**: target, color-coded status, created time, created-by,
  #results. "New scan" opens a dialog; the list polls so
  `Pending → Running → Completed` updates live.
- **`/scans/:id` — Scan detail**: metadata + results table. When `Completed`
  with results, the risk-assessment panel requests
  `POST /api/scans/{scanId}/risk-assessments` (path param, no body) and polls the
  latest assessment (`GET /api/scans/{scanId}/risk-assessment`). Completed
  assessments show the **overall risk score** and a **findings** table (port,
  service, product/version, CPE, CVSS badge [green <4 / amber 4–6.9 / red ≥7],
  KEV badge, match-confidence %, matched CVEs).
- **Connectivity indicator** (header): polls `GET /api/health` every ~10s;
  colored dots for **DB**, **Scanning**, **Assessment** (green = up, red = down).

Status badge colors: Pending = muted, Running = blue/animated, Completed =
green, Failed = red.

## Backend / API

Dev server proxies **`/api` → `http://localhost:8080`** (see `vite.config.ts`).
Start the backend separately on port 8080.
