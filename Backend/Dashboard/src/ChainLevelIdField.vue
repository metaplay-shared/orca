<!-- This file is part of Metaplay SDK which is released under the Metaplay SDK License. -->

<template lang="pug">
div
  div(class="mb-1 font-weight-bold") {{ displayName }}
    MTooltip(
      v-if="displayHint"
      :content="displayHint"
      no-underline
      class="ml-2"
      ): MBadge(shape="pill") ?
  MInputSingleSelectDropdown(
    :model-value="value"
    :options="possibleValues"
    :class="isValid ? 'border-success' : ''"
    no-clear
    @update:model-value="updateValue"
    )
</template>

<script setup lang="ts">
import { computed } from 'vue'

import {
  generatedUiFieldFormEmits,
  type IGeneratedUiFieldFormProps,
  useGeneratedUiFieldForm,
  getGameDataByLibrarySubscriptionOptions,
} from '@metaplay/core'
import { MTooltip, MBadge, MInputSingleSelectDropdown, type MInputSelectOption } from '@metaplay/meta-ui-next'
import { useSubscription } from '@metaplay/subscriptions'

// Override default value or the value property.

const props = withDefaults(
  // eslint-disable-next-line vue/no-unused-properties -- Meow
  defineProps<Omit<IGeneratedUiFieldFormProps, 'value'> & { value?: { type: string; level: string } }>(),
  {
    value: () => ({ type: '', level: '' }),
  }
)

/**
 * Converts the internal chain level object into a string representation for the dropdown.
 * Project Orca uses structured objects internally (e.g. {type: 'ChainA', level: '5'})
 * but displays them as concatenated strings ('ChainA:5') in the UI.
 * This computed property handles that conversion and provides fallback to first available option.
 */
const value = computed<string | undefined>(() =>
  props.value !== null && props.value !== undefined && props.value.type !== ''
    ? `${props.value.type}:${props.value.level}`
    : possibleValues.value.find(() => true)?.value
)

const { data: gameData } = useSubscription(getGameDataByLibrarySubscriptionOptions(['Chains']))

// eslint-disable-next-line vue/define-emits-declaration -- No type available for emits in generated forms
const emit = defineEmits(generatedUiFieldFormEmits)

const { displayName, displayHint, useDefault, isValid, update } = useGeneratedUiFieldForm(props, emit)

/**
 * Converts dropdown string selection back to the backend's structured object format.
 * When a user selects 'ChainA:5' from the dropdown, this function transforms it back
 * into the internal object format that the backend expects: {type: 'ChainA', level: '5'}.
 * The $type field tells the backend what C# type to deserialize to.
 */
function updateValue(value: string | undefined): void {
  if (value === undefined) {
    update(undefined)
    return
  }

  update({
    $type:
      'Game.Logic.LevelId`1[[Game.Logic.ChainTypeId, SharedCode, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]]',
    type: value.split(':')[0],
    level: value.split(':')[1],
  })
}

/**
 * Creates dropdown options from the game's chain configuration data from the game configs.
 * Falls back to schema-defined values if game data isn't available yet.
 */
const possibleValues = computed((): MInputSelectOption[] => {
  const libraryKey = 'Chains'
  if (gameData.value?.gameConfig[libraryKey]) {
    return Object.keys(gameData.value.gameConfig[libraryKey]).map((key) => {
      // Look up if there is a prettier display name for this string id.
      // const id = coreStore.stringIdDecorators[props.fieldInfo.fieldType] ? coreStore.stringIdDecorators[props.fieldInfo.fieldType](key) : key
      return {
        label: key,
        value: key,
      }
    })
  } else if (props.fieldSchema?.possibleValues) {
    return props.fieldSchema.possibleValues.map((x) => {
      return { label: x, value: x }
    })
  } else {
    return []
  }
})

useDefault(undefined, value) // Use first value if available, or undefined
</script>
