import { afterEach } from 'vitest'
import { cleanup } from '@testing-library/react'
import '@testing-library/jest-dom/vitest'

// Without globals: true, @testing-library/react can't auto-detect a global afterEach to
// register its own cleanup, so each test would leave the previous render's DOM mounted.
afterEach(cleanup)
