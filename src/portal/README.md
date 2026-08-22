# Network Security Portal

Web UI ("portal") for the Network Security Platform. Lets you queue nmap scans,
watch them progress live, inspect results, and request a risk assessment — all
scoped to a tenant/"team" selected in the UI.

## Stack

- **Vite 5** + **React 18** + **TypeScript**
- **Tailwind CSS v3** + **shadcn/ui** components
- **@tanstack/react-query** for data fetching + polling
- **react-router-dom** for routing
- **sonner** for toasts

Targets Node 18 (Vite 5.x). Built and verified with Node v18.18.2 / npm 9.8.1.

## Run

```bash
cd src/portal
npm install
npm run dev
```

Dev URL: **http://localhost:5173**

Other scripts:

```bash
npm run build     # tsc -b && vite build (type-check + production build)
npm run preview   # serve the production build
npm run lint      # eslint
```

## Backend / API

The dev server proxies **`/api` → `http://localhost:8080`** (see
`vite.config.ts`), so the browser makes same-origin relative calls (`/api/...`)
and there are no CORS issues. Start the backend separately on port 8080; the
portal builds against the documented contract and does not require the backend
to be up in order to run.

### Dev auth / multi-tenancy — the team switcher

In Development mode the backend authenticates by trusting a request header
**`X-Dev-Subject: <subject>`** (e.g. `teamA|user1`). Each subject maps to its own
tenant/team and only sees its own data.

Every API request from the portal sends this header. Use the **team switcher**
in the top bar to change the current subject:

- Pick a preset (`teamA|user1`, `teamB|user1`, …) from the dropdown, or type a
  custom subject and press Enter / the check button.
- The choice is persisted in `localStorage` (`portal.devSubject`).
- Switching the subject clears cached data and refetches, so you can watch
  tenant isolation: `teamA` and `teamB` see different scans.

Every create action (new scan, new risk assessment) also sends a fresh
**`X-Idempotency-Key`** (`crypto.randomUUID()`).

## Pages

- **`/` — Scans list**: table of scans (target, color-coded status badge,
  created time, #results). "New scan" opens a dialog to submit a target; the
  list polls so `Pending → Running → Completed` updates live. Click a row for
  detail.
- **`/scans/:id` — Scan detail**: scan metadata + results table (port, protocol,
  service, state, product, version). When the scan is `Completed` with results,
  a "Request risk assessment" panel POSTs `/api/risk-assessments` and polls
  `GET /api/risk-assessments/{id}` until the assessment reaches a terminal
  state, showing the status badge and overall risk score.

Status badge colors: Pending = muted, Running = blue/animated, Completed =
green, Failed = red.

## Folder structure

```
src/
  api/          typed API client (client.ts) + query hooks (scans.ts, riskAssessments.ts) + types.ts
  components/   app shell, team switcher, dialogs, status badge, shadcn ui/
  lib/          utils, subject storage + context, formatting
  pages/        ScansListPage, ScanDetailPage
```

## Note on the risk-assessment id

The contract's `POST /api/risk-assessments` returns `202 Accepted` with an empty
body. The portal reads the created id from the response's `Location` header when
present. If the backend does not send one, the panel shows a small input so you
can paste the assessment id to track it.
