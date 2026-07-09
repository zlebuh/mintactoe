import { type ButtonHTMLAttributes, forwardRef } from 'react'
import { cn } from '../../lib/cn'

const variantClasses = {
  primary: 'bg-brand text-white hover:bg-brand-dark active:scale-95',
  secondary: 'bg-white text-brand border-2 border-brand hover:bg-brand/10 active:scale-95',
  ghost: 'bg-transparent text-current hover:bg-black/5 active:scale-95',
} as const

export type ButtonVariant = keyof typeof variantClasses

export interface ButtonProps extends ButtonHTMLAttributes<HTMLButtonElement> {
  variant?: ButtonVariant
}

export const Button = forwardRef<HTMLButtonElement, ButtonProps>(function Button(
  { className, variant = 'primary', ...props },
  ref,
) {
  return (
    <button
      ref={ref}
      className={cn(
        'inline-flex items-center justify-center gap-2 rounded-2xl px-5 py-3 text-base font-semibold transition-all duration-150',
        'disabled:pointer-events-none disabled:opacity-40',
        variantClasses[variant],
        className,
      )}
      {...props}
    />
  )
})
