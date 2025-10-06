// This file is part of Metaplay SDK which is released under the Metaplay SDK License.
/*
 * This file is the entry point of the Metaplay SDK's LiveOps Dashboard application.
 *
 * Please avoid making edit to this file because we will be updating it regularly and our changes will conflic with yours.
 * For game-specific customizations, please use the 'gameSpecific.ts' as the starting point.
 */
// Tailwind CSS.
// Vue is the frontend framework.
import { createApp } from 'vue'

// SDK's pre-configured Vue application.
import { CorePlugin, App } from '@metaplay/core'

// Game-specific integration code.
import { GameSpecificPlugin } from './gameSpecific'
import './tailwind.css'

// Create the Vue application.
// eslint-disable-next-line @typescript-eslint/no-unsafe-argument
const app = createApp(App)

// Register global core components (will look different in Vue 3).
app.use(CorePlugin)

// Trigger game-specific imports.
app.use(GameSpecificPlugin)

// All done. Mount!
app.mount('#app')
