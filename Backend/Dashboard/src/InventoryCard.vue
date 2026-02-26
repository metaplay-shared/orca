<!-- This file is part of Metaplay SDK which is released under the Metaplay SDK License. -->

<template lang="pug">
MCard(
  title="Inventory"
  no-body-padding
  :badge="inventory?.length"
  :badge-variant="inventory?.length ? 'primary' : undefined"
  data-testid="player-inventory-card"
  )
  MList
    MListItem(
      v-for="item in inventory"
      :key="item.info"
      :avatar-url="item.image"
      )
      div {{ getLocalizedItemNameString(item.info) }} x{{ item.amount }}
      template(#bottom-left)
        div ID: {{ item.info }}
</template>

<script lang="ts" setup>
import { computed } from 'vue'

import { getSingleLocalizationSubscriptionOptions, getSinglePlayerSubscriptionOptions } from '@metaplay/core'
import { MCard, MList, MListItem } from '@metaplay/meta-ui-next'
import { useSubscription } from '@metaplay/subscriptions'

const props = defineProps<{
  /**
   * Id of the player whose heroes we want to show.
   */
  playerId: string
}>()

// Subscribe to player data and localization strings needed for inventory display.
// Subscriptions automatically handle caching and data refreshing for optimal performance.
const { data: playerData, refresh: playerRefresh } = useSubscription(() =>
  getSinglePlayerSubscriptionOptions(props.playerId)
)
const { data: localizationData } = useSubscription(() => getSingleLocalizationSubscriptionOptions('$active'))

/**
 * Converts item IDs to user-friendly names using Metaplay's localization system.
 * This ensures that technical identifiers like 'ChainWood' display as 'Wood' to users.
 */
function getLocalizedItemNameString(key: string): string {
  return localizationData.value?.locs.en?.translations[`Chain.${key}`] ?? key
}

/**
 * Transforms the player's raw inventory data into a format optimized for UI display.
 * Player inventories in Project Orca are stored as key-value pairs where keys are item IDs
 * and values are quantities. This computed property converts that into an array of objects
 * with display-friendly properties including image paths for visual representation.
 * The resulting format makes it easy to render inventory items in lists or grids.
 */
const inventory = computed(() => {
  if (playerData.value) {
    const resources = playerData.value.model.inventory.resources
    // eslint-disable-next-line @typescript-eslint/no-unsafe-argument -- We know this is an object with string keys.
    return Object.keys(resources).map((id) => {
      return {
        info: id,
        amount: resources[id],
        image: `/Board/Chains/${id}1.png`,
      }
    })
  } else {
    return undefined
  }
})
</script>
