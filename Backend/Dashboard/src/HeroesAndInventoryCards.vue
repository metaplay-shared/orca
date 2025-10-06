<!-- This file is part of Metaplay SDK which is released under the Metaplay SDK License. -->

<template lang="pug">
HeroesCard(
  :playerId="playerId"
  class="tw-mb-3"
  )
InventoryCard(:playerId="playerId")
</template>
<script setup lang="ts">
import HeroesCard from './HeroesCard.vue'
import InventoryCard from './InventoryCard.vue'

const props = defineProps<{
  /**
   * Id of the player whose heroes we want to show.
   */
  playerId: string
}>()
</script>
