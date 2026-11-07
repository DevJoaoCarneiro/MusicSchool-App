import { defineConfig } from '@playwright/test'

const baseURL = process.env.E2E_BASE_URL ?? 'http://localhost:5173'
const videoMode = (process.env.E2E_VIDEO as 'on' | 'off' | 'retain-on-failure' | undefined) ?? 'retain-on-failure'
const slowMo = Number(process.env.E2E_SLOWMO ?? 0)
const videoSize = { width: 1440, height: 900 }

export default defineConfig({
  testDir: './e2e',
  globalSetup: './e2e/global-setup.ts',
  fullyParallel: false,
  workers: 1,
  retries: 0,
  timeout: 60_000 + slowMo * 200,
  expect: { timeout: 10_000 },
  reporter: [
    ['list'],
    ['html', { open: 'never' }],
    ...(videoMode === 'on' ? [['./e2e/video-reporter.ts'] as [string]] : []),
  ],
  use: {
    baseURL,
    channel: process.env.E2E_BROWSER_CHANNEL ?? 'chrome',
    viewport: videoSize,
    locale: 'pt-BR',
    timezoneId: 'America/Sao_Paulo',
    trace: 'retain-on-failure',
    screenshot: 'only-on-failure',
    video: { mode: videoMode, size: videoSize },
    launchOptions: { slowMo },
  },
  webServer: process.env.E2E_BASE_URL
    ? undefined
    : {
        command: 'npm run dev',
        url: baseURL,
        reuseExistingServer: true,
        timeout: 60_000,
      },
})
