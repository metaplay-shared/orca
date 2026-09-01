/* eslint-disable @eslint-community/eslint-comments/no-unlimited-disable -- Just wholesale disabling linting here. */
/* eslint-disable -- Just wholesale disabling linting here. */

declare module 'vue' {
  import { CompatVue } from '@vue/runtime-dom'
  const Vue: CompatVue
  export default Vue
  export * from '@vue/runtime-dom'
  const { configureCompat } = Vue
  export { configureCompat }
}
