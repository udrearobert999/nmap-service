import { useState } from "react"
import { toast } from "sonner"
import { Plus } from "lucide-react"
import { useCreateScan } from "@/api/scans"
import { ApiError } from "@/api/client"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from "@/components/ui/dialog"

export function NewScanDialog() {
  const [open, setOpen] = useState(false)
  const [target, setTarget] = useState("")
  const createScan = useCreateScan()

  function submit(e: React.FormEvent) {
    e.preventDefault()
    const value = target.trim()
    if (!value) return
    createScan.mutate(value, {
      onSuccess: () => {
        toast.success("Scan queued", { description: value })
        setTarget("")
        setOpen(false)
      },
      onError: (err) => {
        const msg =
          err instanceof ApiError ? err.message : "Failed to create scan"
        toast.error("Could not queue scan", { description: msg })
      },
    })
  }

  return (
    <Dialog open={open} onOpenChange={setOpen}>
      <DialogTrigger asChild>
        <Button>
          <Plus className="mr-2 h-4 w-4" />
          New scan
        </Button>
      </DialogTrigger>
      <DialogContent>
        <form onSubmit={submit}>
          <DialogHeader>
            <DialogTitle>New scan</DialogTitle>
            <DialogDescription>
              Enter an IP address or hostname to scan. The server validates the
              target.
            </DialogDescription>
          </DialogHeader>
          <div className="grid gap-2 py-4">
            <Label htmlFor="target">Target</Label>
            <Input
              id="target"
              autoFocus
              placeholder="e.g. 192.168.1.1 or scanme.nmap.org"
              value={target}
              onChange={(e) => setTarget(e.target.value)}
            />
          </div>
          <DialogFooter>
            <Button
              type="submit"
              disabled={createScan.isPending || !target.trim()}
            >
              {createScan.isPending ? "Queuing…" : "Start scan"}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  )
}
