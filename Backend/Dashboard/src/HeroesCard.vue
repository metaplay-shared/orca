<!-- This file is part of Metaplay SDK which is released under the Metaplay SDK License. -->

<template lang="pug">
meta-list-card(
  title="Heroes"
  :itemList="allHeroes"
  :searchFields="searchFields"
  :filterSets="filterSets"
  :sortOptions="sortOptions"
  :pageSize="20"
  data-testid="player-heroes-card"
  )
  template(#item-card="{ item }")
    MListItem(
      :class="{ 'tw-saturate-0': item?.taskState === 'Locked' }"
      :avatarUrl="item.image"
      )
      div(:class="{ 'tw-text-neutral-500': item.level === 0 }") {{ item.info }}

      template(#top-right)
        MBadge(
          v-if="item.level > 0"
          variant="primary"
          ) Level {{ item.level }}
        MBadge(v-else) {{ item.taskState }}

      template(#bottom-left)
        div(v-if="item.taskState !== 'Locked'")
          div(v-if="item.task !== null && item.task !== undefined")
            //- TODO: Current task
            div(class="tw-mt-1 tw-flex tw-justify-between")
              div
                div(class="tw-font-semibold") Current Task
                div {{ item.task?.id }}: {{ item.task?.itemType }}

              div
                div(class="tw-font-semibold") Requirements
                div(v-for="resource in item.taskResources") {{ resource.type }} x{{ resource.amount }}

              div
                div(class="tw-font-semibold") Rewards
                div(v-for="reward in item.taskRewards") {{ reward.type }} x{{ reward.count }}
          div(v-else)
            div(class="tw-text-neutral-500") No Task Available
</template>

<script lang="ts" setup>
import { computed } from 'vue'

import { getGameDataSubscriptionOptions, getSinglePlayerSubscriptionOptions } from '@metaplay/core'
import { MetaListSortDirection, MetaListSortOption, MetaListFilterSet, MetaListFilterOption } from '@metaplay/meta-ui'
import { MListItem, MBadge, MTooltip } from '@metaplay/meta-ui-next'
import { useSubscription } from '@metaplay/subscriptions'

const props = defineProps<{
  /**
   * Id of the player whose heroes we want to show.
   */
  playerId: string
}>()

// Subscribe to the data we need to render this component.
// Protip: subscriptions cache and refresh their data automatically. Much better than individual HTTP requests!
const { data: gameData } = useSubscription(() => getGameDataSubscriptionOptions())
const { data: playerData, refresh: playerRefresh } = useSubscription(() =>
  getSinglePlayerSubscriptionOptions(props.playerId)
)

/**
 * Search fields array to be passed to the meta-list-card component.
 * Protip: add custom search fields that are relevant in your game!
 */
const searchFields = ['info']

/**
 * Filter sets array to be passed to the meta-list-card component.
 * Protip: add custom filter sets that are relevant in your game!
 */
const filterSets = [
  new MetaListFilterSet('unlocked', [
    new MetaListFilterOption('Locked', (x: any) => x.level === 0),
    new MetaListFilterOption('Unlocked', (x: any) => x.level > 0),
  ]),
]

/**
 * Sort options array to be passed to the meta-list-card component.
 * Protip: add custom sort options that are relevant in your game!
 */
const sortOptions = [
  MetaListSortOption.asUnsorted(),
  new MetaListSortOption('Level', 'level', MetaListSortDirection.Ascending),
  new MetaListSortOption('Level', 'level', MetaListSortDirection.Descending),
]

/**
 * Custom computed property to look up all the possible heroes from game configs and add info about the one the player has already unlocked so it's easier to render a nice looking list.
 */
const allHeroes = computed(() => {
  if (gameData.value && playerData.value) {
    const availableHeroes = gameData.value.gameConfig.Heroes
    // eslint-disable-next-line @typescript-eslint/no-unsafe-argument
    return Object.keys(availableHeroes).map((id) => {
      if (id in playerData.value.model.heroes.heroes) {
        return {
          info: id,
          level: playerData.value.model.heroes.heroes[id].level.level,
          task: getTask(playerData.value.model.heroes.heroes[id].currentTask?.info),
          taskResources: getResourcesForTask(playerData.value.model.heroes.heroes[id].currentTask?.info),
          taskRewards: getRewardsForTask(playerData.value.model.heroes.heroes[id].currentTask?.info),
          taskState: playerData.value.model.heroes.heroes[id].currentTask?.state,
          image: `/Heroes/${id}.png`,
        }
      } else {
        return {
          info: id,
          level: 0,
          task: 0,
          taskState: 'Locked',
          image: `/Heroes/${id}.png`,
        }
      }
    })
  } else {
    return undefined
  }
})

function getTask(id: any) {
  if (gameData.value?.gameConfig.HeroTasks?.[id]) {
    return gameData.value.gameConfig.HeroTasks[id]
  } else {
    return undefined
  }
}

function getRewardsForTask(task: any) {
  if (task && gameData.value?.gameConfig.HeroTasks?.[task]) {
    const rewards = gameData.value.gameConfig.HeroTasks[task].rewards
    // eslint-disable-next-line @typescript-eslint/no-unsafe-argument
    return Object.keys(rewards).map((id) => {
      return {
        info: id,
        type: rewards[id].type,
        level: rewards[id].level,
        count: rewards[id].count,
        image: `/Board/Chains/${rewards[id].type}${rewards[id].level}.png`,
      }
    })
  } else {
    return undefined
  }
}

function getResourcesForTask(task: any) {
  if (task && gameData.value?.gameConfig.HeroTasks?.[task]) {
    const resources = gameData.value.gameConfig.HeroTasks[task].resources
    // eslint-disable-next-line @typescript-eslint/no-unsafe-argument
    return Object.keys(resources).map((id) => {
      return {
        info: id,
        type: resources[id].type,
        amount: resources[id].amount,
        image: `/Board/Chains/${resources[id].type}1.png`,
      }
    })
  } else {
    return undefined
  }
}
</script>
