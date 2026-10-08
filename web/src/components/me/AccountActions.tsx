import { KeyRound, LogOut } from 'lucide-react'
import { Link, useNavigate } from 'react-router'
import { useAuth } from '@/auth'
import { Button, Card } from '@/components/ui'
import { buttonClass } from '@/components/ui/buttonStyles'

export function AccountActions() {
  const { signOut } = useAuth()
  const navigate = useNavigate()

  return (
    <Card className="flex flex-col items-stretch gap-3 sm:flex-row sm:items-center sm:justify-between">
      <div>
        <h3 className="text-lg font-semibold">Account</h3>
        <p className="text-muted mt-1 text-sm">Manage your password or sign out of this device.</p>
      </div>
      <div className="flex flex-col gap-2 sm:flex-row">
        <Link to="/account/change-password" className={buttonClass('secondary', 'md')}>
          <KeyRound aria-hidden className="size-4" />
          Change password
        </Link>
        <Button
          variant="ghost"
          leftIcon={<LogOut aria-hidden className="size-4" />}
          onClick={() => {
            signOut()
            navigate('/login', { replace: true })
          }}
        >
          Sign out
        </Button>
      </div>
    </Card>
  )
}
