import { type ComponentProps } from 'react'
import * as RadixDialog from '@radix-ui/react-dialog'
import { cn } from '../../lib/cn'

export const Dialog = RadixDialog.Root
export const DialogTrigger = RadixDialog.Trigger
export const DialogClose = RadixDialog.Close

export function DialogContent({ className, children, ...props }: ComponentProps<typeof RadixDialog.Content>) {
  return (
    <RadixDialog.Portal>
      <RadixDialog.Overlay className="fixed inset-0 bg-black/40 data-[state=open]:animate-[banner-in_150ms_ease-out]" />
      <RadixDialog.Content
        className={cn(
          'fixed left-1/2 top-1/2 w-[calc(100%-2rem)] max-w-sm -translate-x-1/2 -translate-y-1/2',
          'rounded-3xl bg-white p-6 shadow-lg focus:outline-none',
          className,
        )}
        {...props}
      >
        {children}
      </RadixDialog.Content>
    </RadixDialog.Portal>
  )
}

export function DialogTitle({ className, ...props }: ComponentProps<typeof RadixDialog.Title>) {
  return <RadixDialog.Title className={cn('text-xl font-bold', className)} {...props} />
}

export function DialogDescription({ className, ...props }: ComponentProps<typeof RadixDialog.Description>) {
  return <RadixDialog.Description className={cn('mt-1 text-sm text-black/60', className)} {...props} />
}
