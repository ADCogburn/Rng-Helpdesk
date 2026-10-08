export const MIN_PASSWORD_LENGTH = 8

export interface PasswordErrors {
  currentPassword?: string
  newPassword?: string
  confirmPassword?: string
}

export function validatePasswordChange(
  current: string,
  next: string,
  confirm: string,
): PasswordErrors {
  const errors: PasswordErrors = {}
  if (!current) errors.currentPassword = 'Enter your current password.'
  if (next.length < MIN_PASSWORD_LENGTH)
    errors.newPassword = `Use at least ${MIN_PASSWORD_LENGTH} characters.`
  else if (next === current) errors.newPassword = 'New password must differ from the current one.'
  if (confirm !== next) errors.confirmPassword = 'Passwords do not match.'
  return errors
}
