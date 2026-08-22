import { useState } from "react"
import { Check, Users } from "lucide-react"
import { useSubject } from "@/lib/subject-context"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select"

// A few convenient presets; each subject maps to its own tenant/team on the
// backend, so switching demonstrates tenant isolation.
const PRESETS = ["teamA|user1", "teamA|user2", "teamB|user1", "teamC|user1"]

export function TeamSwitcher() {
  const { subject, setSubject } = useSubject()
  const [custom, setCustom] = useState("")

  const options = PRESETS.includes(subject) ? PRESETS : [subject, ...PRESETS]

  return (
    <div className="flex items-center gap-2">
      <Users className="h-4 w-4 text-muted-foreground" />
      <Select value={subject} onValueChange={setSubject}>
        <SelectTrigger className="w-[180px]" aria-label="Team / subject">
          <SelectValue placeholder="Select team" />
        </SelectTrigger>
        <SelectContent>
          {options.map((s) => (
            <SelectItem key={s} value={s}>
              {s}
            </SelectItem>
          ))}
        </SelectContent>
      </Select>
      <div className="flex items-center gap-1">
        <Input
          value={custom}
          onChange={(e) => setCustom(e.target.value)}
          placeholder="custom subject"
          className="w-[160px]"
          onKeyDown={(e) => {
            if (e.key === "Enter" && custom.trim()) {
              setSubject(custom)
              setCustom("")
            }
          }}
        />
        <Button
          size="icon"
          variant="secondary"
          aria-label="Apply custom subject"
          onClick={() => {
            if (custom.trim()) {
              setSubject(custom)
              setCustom("")
            }
          }}
        >
          <Check className="h-4 w-4" />
        </Button>
      </div>
    </div>
  )
}
