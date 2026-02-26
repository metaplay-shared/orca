<!-- This file is part of Metaplay SDK which is released under the Metaplay SDK License. -->

<template lang="pug">
MetaListCard(
  title="Heroes"
  :item-list="allHeroes"
  :search-fields="searchFields"
  :filter-sets="filterSets"
  :sort-options="sortOptions"
  :page-size="20"
  alternative-title-style
  class="tw-rounded-lg tw-border tw-border-neutral-200"
  data-testid="player-heroes-card"
  )
  template(#item-card="{ item }")
    MListItem(
      :class="{ 'tw-saturate-0': item?.taskState === 'Locked' }"
      :avatar-url="item.image"
      )
      div(:class="{ 'tw-text-neutral-500': item.level === 0 }") {{ getLocalizedHeroNameString(item.info) }}

      template(#top-right)
        MBadge(
          v-if="item.level > 0"
          variant="primary"
          ) Level {{ item.level }}
        MBadge(v-else) {{ item.taskState }}

      template(#bottom-left)
        div(v-if="item.taskState !== 'Locked'")
          div(v-if="item.task !== null && item.task !== undefined")
            table
              tbody
                tr
                  td(class="tw-align-top") Current Task:
                  td
                    MTooltip(
                      :content="`Item Type: ${getTask(item.task)?.itemType}\nTask ID: ${getTask(item.task)?.id}`"
                      )
                      | Task {{ getTask(item.task)?.id }}: {{ getLocalizedItemNameString(getTask(item.task)?.itemType) }}
                tr
                  td(class="tw-align-top") Requirements:
                  td
                    span(
                      v-for="(resource, index) in item.taskResources"
                      :key="index"
                      )
                      MTooltip(:content="`Requirement Type: ${resource.type}\nCount: ${resource.amount}`")
                        | {{ getLocalizedItemNameString(resource.type) }} x{{ resource.amount }}
                      span(
                        v-if="index < (item.taskResources?.length ?? 0) - 1"
                        class="tw-mr-1"
                        ) ,
                tr
                  td(class="tw-align-top") Rewards:
                  td
                    span(
                      v-for="(reward, index) in item.taskRewards"
                      :key="index"
                      )
                      MTooltip(:content="`Reward Type: ${reward.type}\nCount: ${reward.count}`")
                        | {{ getLocalizedItemNameString(reward.type) }} x{{ reward.count }}
                      span(
                        v-if="index < (item.taskRewards?.length ?? 0) - 1"
                        class="tw-mr-1"
                        ) ,
          div(v-else)
            div(class="tw-italic tw-text-neutral-400") No task available
</template>

<script lang="ts" setup>
import { computed } from 'vue'

import {
  getGameDataByLibrarySubscriptionOptions,
  getSingleLocalizationSubscriptionOptions,
  getSinglePlayerSubscriptionOptions,
} from '@metaplay/core'
import {
  MetaListSortDirection,
  MetaListSortOption,
  MetaListFilterSet,
  MetaListFilterOption,
  MetaListCard,
} from '@metaplay/meta-ui'
import { MListItem, MBadge, MTooltip } from '@metaplay/meta-ui-next'
import { useSubscription } from '@metaplay/subscriptions'

const props = defineProps<{
  /**
   * Id of the player whose heroes we want to show.
   */
  playerId: string
}>()

// Subscribe to the data we need to render this component.
// Subscriptions automatically handle caching and data refreshing for optimal performance.
const { data: gameData } = useSubscription(() => getGameDataByLibrarySubscriptionOptions(['Heroes', 'HeroTasks']))
const { data: playerData } = useSubscription(() => getSinglePlayerSubscriptionOptions(props.playerId))
const { data: localizationData } = useSubscription(() => getSingleLocalizationSubscriptionOptions('$active'))

/**
 * Converts hero IDs to human-readable names using the localization system.
 * In Project Orca, we map technical IDs like 'HeroWarrior' to
 * user-friendly names like 'Brave Warrior'. Falls back to the raw ID if no translation exists.
 */
function getLocalizedHeroNameString(key: string): string {
  return localizationData.value?.locs.en?.translations[`Hero.${key}`] ?? key
}

/**
 * Converts chain item IDs to human-readable names using the localization system.
 * Similar to hero names, this maps technical IDs like 'ChainWood' to
 * display names like 'Wood Block'. Falls back to the raw ID if no translation exists.
 */
function getLocalizedItemNameString(key: string): string {
  return localizationData.value?.locs.en?.translations[`Chain.${key}`] ?? key
}

/**
 * Defines which hero properties can be searched.
 * Currently allows searching by hero info/ID.
 */
const searchFields = ['info']

/**
 * Defines filter options to categorize heroes by their unlock status.
 * Allows filtering between locked (level 0) and unlocked (level > 0) heroes.
 */
const filterSets = [
  new MetaListFilterSet('unlocked', [
    new MetaListFilterOption('Locked', (x: any) => x.level === 0),
    new MetaListFilterOption('Unlocked', (x: any) => x.level > 0),
  ]),
]

/**
 * Defines available sorting options for the hero list.
 * Provides unsorted view and level-based sorting in both directions.
 */
const sortOptions = [
  MetaListSortOption.asUnsorted(),
  new MetaListSortOption('Level', 'level', MetaListSortDirection.Ascending),
  new MetaListSortOption('Level', 'level', MetaListSortDirection.Descending),
]

interface Hero {
  info: string
  level: number
  task: number | undefined
  taskResources?:
    | Array<{
        info: string
        type: string
        amount: number
        image: string
      }>
    | undefined
  taskRewards?:
    | Array<{
        info: string
        type: string
        level: number
        count: number
        image: string
      }>
    | undefined
  taskState: string | undefined
  image: string
}

/**
 * Merges game configuration data with player progress to create a complete hero list.
 * This computed property combines two data sources:
 * 1. Game config: Defines all heroes that exist in the game
 * 2. Player data: Shows which heroes the player has unlocked and their current progress
 *
 * For each hero, it creates a unified object containing both static info (from config)
 * and dynamic info (from player state). Unlocked heroes show level/task data,
 * while locked heroes show as level 0.
 */
const allHeroes = computed<Hero[] | undefined>(() => {
  if (gameData.value && playerData.value) {
    const availableHeroes = gameData.value.gameConfig.Heroes

    return Object.keys(availableHeroes ?? {}).map((id) => {
      if (id in playerData.value.model.heroes.heroes) {
        // eslint-disable-next-line @typescript-eslint/no-unsafe-type-assertion -- We know from the game data schema that currentTask.info is a number, but TS can't infer that.
        const taskId = playerData.value.model.heroes.heroes[id].currentTask?.info as number | undefined
        return {
          info: id,
          level: playerData.value.model.heroes.heroes[id].level.level,
          task: getTask(taskId),
          taskResources: getResourcesForTask(taskId),
          taskRewards: getRewardsForTask(taskId),
          taskState: playerData.value.model.heroes.heroes[id].currentTask?.state,
          image: `/Heroes/${id}.png`,
        }
      } else {
        return {
          info: id,
          level: 0,
          task: undefined,
          taskState: 'Locked',
          image: `/Heroes/${id}.png`,
        }
      }
    })
  } else {
    return undefined
  }
})

/**
 * Looks up task configuration data by task ID.
 * Hero tasks are defined in the game config and contain information about
 * what players need to do to progress their heroes (requirements and rewards).
 */
function getTask(id: number | undefined): any {
  if (!id) {
    return undefined
  }
  if (gameData.value?.gameConfig.HeroTasks?.[id]) {
    return gameData.value.gameConfig.HeroTasks[id]
  } else {
    return undefined
  }
}

/**
 * Extracts reward information from a hero task configuration.
 * Rewards are what players receive for completing a task (e.g., items, currency).
 * This function transforms the config data into a format suitable for UI rendering,
 * including generating image paths for visual representation.
 */
function getRewardsForTask(
  task: number | undefined
): Array<{ info: string; type: string; level: number; count: number; image: string }> | undefined {
  if (task && gameData.value?.gameConfig.HeroTasks?.[task]) {
    const rewards = gameData.value.gameConfig.HeroTasks[task].rewards
    // eslint-disable-next-line @typescript-eslint/no-unsafe-argument -- We know the keys of rewards are strings, but TS can't infer that.
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

/**
 * Extracts requirement information from a hero task configuration.
 * Requirements are what players need to have/spend to complete a task.
 * Like rewards, this transforms config data for UI display, including image paths.
 */
function getResourcesForTask(
  task: number | undefined
): Array<{ info: string; type: string; amount: number; image: string }> | undefined {
  if (task && gameData.value?.gameConfig.HeroTasks?.[task]) {
    const resources = gameData.value.gameConfig.HeroTasks[task].resources
    // eslint-disable-next-line @typescript-eslint/no-unsafe-argument -- We know the keys of resources are strings, but TS can't infer that.
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
