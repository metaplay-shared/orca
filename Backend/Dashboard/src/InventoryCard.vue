<!-- This file is part of Metaplay SDK which is released under the Metaplay SDK License. -->

<template lang="pug">
MCard(
  title="Inventory"
  noBodyPadding
  :badge="inventory?.length"
  :badgeVariant="inventory?.length ? 'primary' : undefined"
  data-testid="player-inventory-card"
  )
  MList
    MListItem(
      v-for="item in inventory"
      :key="item.info"
      :avatarUrl="item.image"
      )
      div {{ getLocalizedItemNameString(item.info) }} x{{ item.amount }}
      template(#bottom-left)
        div ID: {{ item.info }}
</template>

<script lang="ts" setup>
import { computed } from 'vue'

import {
  getSingleLocalizationSubscriptionOptions,
  getSinglePlayerSubscriptionOptions,
} from '@metaplay/core'
import { MCard, MList, MListItem } from '@metaplay/meta-ui-next'
import { useSubscription } from '@metaplay/subscriptions'

const props = defineProps<{
  /**
   * Id of the player whose heroes we want to show.
   */
  playerId: string
}>()

// Subscribe to the data we need to render this component.
// Protip: subscriptions cache and refresh their data automatically. Much better than individual HTTP requests!
const { data: playerData, refresh: playerRefresh } = useSubscription(() =>
  getSinglePlayerSubscriptionOptions(props.playerId)
)
const { data: localizationData } = useSubscription(() => getSingleLocalizationSubscriptionOptions('$active'))

function getLocalizedItemNameString(key: string): string {
  return localizationData.value?.locs.en.translations['Chain.' + key] || key
}

const inventory = computed(() => {
  if (playerData.value) {
    const resources = playerData.value.model.inventory.resources
    // eslint-disable-next-line @typescript-eslint/no-unsafe-argument
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
