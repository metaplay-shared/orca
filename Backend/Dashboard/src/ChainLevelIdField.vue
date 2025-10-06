// ProjectFolder/Backend/Dashboard/src/MyGameMailField.vue
<template lang="pug">
div
  div(class="mb-1 font-weight-bold") {{ displayName }}
    MTooltip(
      v-if="displayHint"
      :content="displayHint"
      noUnderline
      class="ml-2"
      ): MBadge(shape="pill") ?
  MInputSingleSelectDropdown(
    :value="value"
    :options="possibleValues"
    :class="isValid ? 'border-success' : ''"
    no-clear
    @input="updateValue"
    )
</template>

<script setup lang="ts">
import { computed } from 'vue'

import {
  generatedUiFieldFormEmits,
  generatedUiFieldFormProps,
  useGeneratedUiFieldForm,
  getGameDataByLibrarySubscriptionOptions,
} from '@metaplay/core'
import { MTooltip, MBadge, MInputSingleSelectDropdown, type MInputSelectOption } from '@metaplay/meta-ui-next'
import { useSubscription } from '@metaplay/subscriptions'

// Override default value or the value property.
const props = defineProps({
  ...generatedUiFieldFormProps,
  value: {
    type: Object,
    default: () => ({
      type: '',
      level: '',
    }),
  },
})

const value = computed<any>(() =>
  props.value != null && props.value !== undefined && props.value.type !== ''
    ? props.value.type + ':' + props.value.level
    : possibleValues.value.find(() => true)?.value
)

const { data: gameData } = useSubscription(getGameDataByLibrarySubscriptionOptions(['Chains']))

const emit = defineEmits(generatedUiFieldFormEmits)

const { displayName, displayHint, useDefault, isValid, update } = useGeneratedUiFieldForm(props, emit)

function updateValue(value: any) {
  update({
    $type:
      'Game.Logic.LevelId`1[[Game.Logic.ChainTypeId, SharedCode, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]]',
    type: value.split(':')[0],
    level: value.split(':')[1],
  })
}

const possibleValues = computed((): MInputSelectOption[] => {
  // TODO: Improve the prop typings so we don't need to use non-null assertions.

  const libraryKey = 'Chains'
  if (gameData.value?.gameConfig[libraryKey]) {
    // eslint-disable-next-line @typescript-eslint/no-unsafe-argument
    return Object.keys(gameData.value.gameConfig[libraryKey]).map((key) => {
      // Look up if there is a prettier display name for this string id.
      // const id = coreStore.stringIdDecorators[props.fieldInfo.fieldType] ? coreStore.stringIdDecorators[props.fieldInfo.fieldType](key) : key
      return {
        label: key,
        value: key,
      }
    })
  } else if (props.fieldSchema.possibleValues) {
    return props.fieldSchema.possibleValues.map((x) => {
      return { label: x, value: x }
    })
  } else {
    return []
  }
})

useDefault(undefined, value) // Use first value if available, or undefined
</script>
