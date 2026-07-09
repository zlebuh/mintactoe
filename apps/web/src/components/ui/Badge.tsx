import { type HTMLAttributes } from 'react'
import { cn } from '../../lib/cn'

export function Badge({ className, ...props }: HTMLAttributes<HTMLSpanElement>) {
  return (
    <span
      className={cn(
        'inline-flex items-center rounded-full bg-black/5 px-3 py-1 text-xs font-semibold uppercase tracking-wide text-black/60',
        className,
      )}
      {...props}
    />
  )
}
