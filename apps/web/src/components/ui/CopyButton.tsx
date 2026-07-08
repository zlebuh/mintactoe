import { useCallback, useState } from 'react'
import { Button, type ButtonProps } from './Button'

interface CopyButtonProps extends Omit<ButtonProps, 'onClick'> {
  text: string
  label?: string
  copiedLabel?: string
}

export function CopyButton({
  text,
  label = 'Copy',
  copiedLabel = 'Copied!',
  ...props
}: CopyButtonProps) {
  const [copied, setCopied] = useState(false)

  const handleClick = useCallback(async () => {
    await navigator.clipboard.writeText(text)
    setCopied(true)
    setTimeout(() => setCopied(false), 1500)
  }, [text])

  return (
    <Button onClick={handleClick} {...props}>
      {copied ? copiedLabel : label}
    </Button>
  )
}
