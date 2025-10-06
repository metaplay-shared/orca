import TailwindPlugin from '@metaplay/tailwind-plugin'

import TailwindContainerQueries from '@tailwindcss/container-queries'
import TailwindForms from '@tailwindcss/forms'

/** @type {import('tailwindcss').Config} */
export default {
  content: [
    'index.html',
    'src/**/*.{vue,js,ts}',
    '.storybook/**/*.{ts,html}',
    './node_modules/@metaplay/core/src/**/*.{vue,ts}',
    './node_modules/@metaplay/meta-ui/src/**/*.{vue,ts}',
    './node_modules/@metaplay/meta-ui-next/src/**/*.{vue,ts}',
    './node_modules/@metaplay/event-stream/src/**/*.{vue,ts}',
  ],
  plugins: [TailwindForms, TailwindContainerQueries, TailwindPlugin],
}
