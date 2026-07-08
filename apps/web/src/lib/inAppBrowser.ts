const IN_APP_PATTERNS = [
  'FBAN',
  'FBAV',
  'Instagram',
  'TikTok',
  'Line/',
  'Snapchat',
  'Twitter',
  'MicroMessenger',
]

export function isInAppBrowser(): boolean {
  const ua = navigator.userAgent
  return IN_APP_PATTERNS.some((pattern) => ua.includes(pattern))
}
