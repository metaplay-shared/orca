<template lang="pug">
MCard(
  title="Merge Boards"
  subtitle="View details of the islands that the player has access to. Select an island to view its merge board."
  )
  div(class="tw-flex")
    //- Left side: Island selector.
    div(class="tw-mb-4 tw-flex tw-w-1/4 tw-flex-col")
      div(
        v-for="island in islands"
        :key="island.info"
        class="tw-mb-1 tw-mr-1 tw-aspect-square tw-w-[5rem] tw-select-none tw-content-center tw-rounded tw-border tw-border-neutral-300 tw-text-center"
        :class="{ 'hover:tw-brightness-90 active:tw-brightness-75 tw-cursor-pointer': island.state === 'Open', 'tw-bg-neutral-600 tw-text-neutral-100': island.state === 'Hidden' || island.state === 'Revealing', 'tw-bg-blue-500 tw-text-white': selectedIsland === island.info, 'tw-bg-neutral-100': island.state === 'Open' && selectedIsland !== island.info }"
        @click="island.state === 'Open' ? (selectedIsland = island.info) : undefined"
        )
        img(
          :src="island.image"
          class="tw-pointer-events-none tw-mx-auto tw-size-7/12"
          :class="{ 'tw-opacity-50 tw-saturate-0': island.state === 'Hidden', 'tw-opacity-80 tw-saturate-0': island.state === 'Locked' }"
          )
        div(class="tw-mx-1 tw-overflow-hidden tw-text-ellipsis tw-whitespace-nowrap tw-text-xs")
          MTooltip(:content="`IslandInfo: ${island.info}`") {{ getLocalizedIslandNameString(island.info) }}
        div(class="tw-text-xs") {{ island.state }}

    //- Right side: Selected island merge board and task details.
    div(class="tw-flex tw-flex-col tw-gap-1")
      //- Selected island merge board.
      div(
        v-if="selectedIslandRenderInfos"
        class="tw-grid tw-grid-cols-7 tw-overflow-hidden tw-rounded tw-border tw-border-neutral-300 tw-bg-[]"
        style="background-color: hsl(206, 64%, 33%)"
        )
        div(
          v-for="info in selectedIslandRenderInfos"
          :style="{ scale: info.scale, backgroundImage: `url(${info.backgroundImage})`, gridColumn: `${info.gridColStart} / ${info.gridColStart + info.gridColSpan}`, gridRow: `${info.gridRowStart} / ${info.gridRowStart + info.gridRowSpan}` }"
          class="tw-relative tw-content-center tw-overflow-hidden tw-bg-cover tw-bg-center"
          )
          MTooltip(:content="tooltipContent(info)")
            img(
              v-if="!info.nonItemTile"
              :src="info.itemImage"
              :alt="info.itemType"
              :class="{ 'tw-opacity-50 tw-saturate-0': info.itemHidden }"
              class="tw-scale-[80%] tw-transform tw-transition-transform hover:tw-scale-95"
              :draggable="false"
              )
            img(
              v-else
              :src="info.itemImage"
              :alt="info.itemType"
              class="tw-my-[-25%] tw-scale-[60%] tw-opacity-50"
              :draggable="false"
              )
      //- Task details for the selected island.
      div(class="tw-flex tw-w-full tw-rounded tw-border tw-border-neutral-300 tw-bg-neutral-100 tw-px-1")
        table
          tbody
            tr
              td(class="tw-align-top") Current Task:
              td(v-if="currentIslandTasks")
                MTooltip(
                  :content="`Current Islander: ${currentIslandTasks.islander}\nTask ID: ${currentIslandTasks.id}`"
                  ) {{ getLocalizedHeroNameString(currentIslandTasks.islander) }}:{{ currentIslandTasks.id }}
              td(
                v-else
                class="tw-italic tw-text-neutral-400"
                ) No task available

            tr
              td(class="tw-align-top") Requirements:
              td(v-if="currentIslandTasks")
                span(v-for="(resource, index) in currentIslandTasks.items")
                  MTooltip(
                    :content="`Requirement Type: ${resource.type}\nLevel: ${resource.level}\nCount: ${resource.count}`"
                    ) {{ getLocalizedItemNameString(resource.type) }}:{{ resource.level }} x{{ resource.count }}
                  span(
                    v-if="index < currentIslandTasks.items.length - 1"
                    class="tw-mr-1"
                    ) ,
              td(
                v-else
                class="tw-italic tw-text-neutral-400"
                ) No requirements

            tr
              td(class="tw-align-top") Rewards:
              td(v-if="currentIslandTasks && currentIslandTasks.rewardItems.length != 0")
                span(v-for="(reward, index) in currentIslandTasks.rewardItems")
                  MTooltip(:content="`Reward Type: ${reward.type}\nCount: ${reward.count}`") {{ getLocalizedItemNameString(reward.type) }} x{{ reward.count }}
                  span(
                    v-if="index < currentIslandTasks.rewardItems.length - 1"
                    class="tw-mr-1"
                    ) ,
              div(
                v-else
                class="tw-italic tw-text-neutral-400"
                ) No reward
</template>

<script lang="ts" setup>
import { watch, ref, computed } from 'vue'

import {
  getGameDataByLibrarySubscriptionOptions,
  getSingleLocalizationSubscriptionOptions,
  getSinglePlayerSubscriptionOptions,
} from '@metaplay/core'
import { MCard, MTooltip } from '@metaplay/meta-ui-next'
import { useSubscription } from '@metaplay/subscriptions'

const props = defineProps<{
  /**
   * Id of the player whose heroes we want to show.
   */
  playerId: string
}>()

// Subscribe to the data we need to render this component.
// Protip: subscriptions cache and refresh their data automatically. Much better than individual HTTP requests!
const { data: gameData } = useSubscription(() => getGameDataByLibrarySubscriptionOptions(['Chains', 'Islands', 'IslandTasks']))
const { data: localizationData } = useSubscription(() => getSingleLocalizationSubscriptionOptions('$active'))
const { data: playerData } = useSubscription(() => getSinglePlayerSubscriptionOptions(props.playerId))

const islands = computed(() => {
  if (playerData.value) {
    const islands = playerData.value.model.islands
    // eslint-disable-next-line @typescript-eslint/no-unsafe-argument
    return Object.keys(islands).map((id) => {
      return {
        info: islands[id].info,
        state: islands[id].state,
        image: `/Islands/${islands[id].info}.png`,
      }
    })
  } else {
    return undefined
  }
})

const selectedIsland = ref<string>()

// Preselect the current island on data load if there is no selected island.
watch(
  playerData,
  (newPlayerData) => {
    if (newPlayerData && !selectedIsland.value) {
      selectedIsland.value = newPlayerData.model.lastIsland
    }
  },
  { immediate: true }
)

const selectedIslandData = computed(() => {
  if (playerData.value && selectedIsland.value) {
    return playerData.value.model.islands[selectedIsland.value]
  } else {
    return undefined
  }
})

const currentIslandTasks = computed(() => {
  if (selectedIslandData.value !== undefined && selectedIslandData.value?.tasks?.tasks !== undefined) {
    // eslint-disable-next-line guard-for-in
    for (const key in selectedIslandData.value?.tasks.tasks) {
      const value = selectedIslandData.value?.tasks.tasks[key]
      if (value !== undefined) {
        return gameData.value?.gameConfig.IslandTasks[key + ':' + value.info.level]
      }
    }
  }
  return undefined
})

interface RenderInfo {
  backgroundImage?: string
  tileType: string
  nonItemTile: boolean
  itemImage?: string
  itemHidden?: boolean
  itemType?: string
  itemLevel?: number
  gridColStart: number
  gridColSpan: number
  gridRowStart: number
  gridRowSpan: number
  itemMergeEventScore?: number
  itemTargetIsland?: string
  scale: number
}

/**
 * A list of all grid cells on the selected island. Populated from both game config data and player state.
 */
const selectedIslandRenderInfos = computed((): RenderInfo[] | undefined => {
  if (!selectedIsland.value || !gameData.value) return undefined

  const boardPattern = gameData.value.gameConfig.Islands[selectedIsland.value].boardPattern

  const width = boardPattern.width
  const height = boardPattern.height

  // The board pattern map is an array of grid cells starting from bottom left. We need to reorder it to start from top left.
  const orderedBoardPattern = []
  for (let y = height - 1; y >= 0; y--) {
    for (let x = 0; x < width; x++) {
      const cellIndex = y * width + x
      const cell = boardPattern.map[cellIndex]
      orderedBoardPattern.push(cell)
    }
  }

  // eslint-disable-next-line complexity
  const board: Array<RenderInfo | undefined> = orderedBoardPattern.map((tileType: string, index): RenderInfo => {
    const x = index % width
    // Y is inverted because the board pattern starts from bottom left.
    const y = height - 1 - Math.floor(index / width)

    const item = getItemForBoardLocation(x, y)
    const chainItem = gameData.value?.gameConfig.Chains[`${item?.info.type}:${item?.info.level}`]
    const nonItemTile = tileType === 'ItemHolder' || tileType === 'Ship'

    const mergeGridTile = (x ^ y) & 1 ? '/Board/MergeGrid.png' : '/Board/MergeGridAlternative.png'
    return {
      backgroundImage: tileType === 'Ground' ? mergeGridTile : undefined,
      tileType,
      nonItemTile,
      itemType: item?.info.type,
      itemLevel: item?.info.level,
      itemImage: getImage(tileType, item),
      itemHidden: !item?.isDiscovered,
      gridColStart: x + 1,
      gridColSpan: chainItem?.width || (nonItemTile ? 2 : 1),
      gridRowStart: height - y,
      gridRowSpan: chainItem?.height || 1,
      itemMergeEventScore: chainItem?.mergeEventScore,
      itemTargetIsland: chainItem?.targetIsland,
      // This is a mess! At some browser scales we the tiles scale slightly too small (some integer rounding issue?)
      // and we see gaps between the tiles. So we scale - this effectively gives them a  few extra pixels of padding to
      // cover the gaps. Because scale is a multiplier, 2x2 tiles needs to be scaled half as much as 1x1 tiles to
      // achieve the same increase in pixel size.
      scale: chainItem?.width > 1 || chainItem?.height > 1 ? 1.015 : 1.03,
    }
  })

  // Merge items that are larger than 1x1 into a single cell.
  for (let i = 0; i < board.length; ++i) {
    const cell = board[i]
    if (cell && (cell.gridColSpan > 1 || cell.gridRowSpan > 1)) {
      for (let x = 0; x < cell.gridColSpan; ++x) {
        for (let y = 0; y < cell.gridRowSpan; ++y) {
          // First, mark the other parts of this item as undefined so they don't render.
          board[i + x - y * width] = undefined
        }
      }

      // Then copy the renderable cell's data to the top left cell of the item.
      cell.gridRowStart -= cell.gridRowSpan - 1
      board[i - (cell.gridRowSpan - 1) * width] = cell

      // We have special background 2x2 and 1x2 tiles.
      if (cell.gridColSpan === 2 && cell.gridRowSpan === 2) {
        cell.backgroundImage =
          cell.backgroundImage === '/Board/MergeGrid.png'
            ? '/Board/MergeGrid2x2Alternative.png'
            : '/Board/MergeGrid2x2.png'
      } else if (cell.gridColSpan === 1 && cell.gridRowSpan === 2) {
        cell.backgroundImage =
          cell.backgroundImage === '/Board/MergeGrid.png'
            ? '/Board/MergeGrid1x2Alternative.png'
            : '/Board/MergeGrid1x2.png'
      }
    }
  }

  // Return the cells but filter out the undefined ones. This results in a sparse array that only really makes sense to
  // the grid renderer.
  return board.filter((cell) => cell !== undefined)
})

function getImage(tileType: string, item: any): string {
  // if (tileType === 'Ground') return '/Board/MergeGrid.png'
  // if (tileType === 'Sea') return '/Board/Sea.png'
  if (tileType === 'ItemHolder') return '/Board/Dock.png'
  if (tileType === 'Ship') return '/Board/Ship.png'
  if (item) return `/Board/Chains/${item.info.type + item.info.level}.png`
  return '/Board/Blank256x256.png'
}

function getItemForBoardLocation(x: number, y: number) {
  if (!selectedIslandData.value?.mergeBoard?.items) return undefined

  return selectedIslandData.value.mergeBoard.items.find((item: any) => item.x === x && item.y === y)
}

function tooltipContent(info: any): string | undefined {
  if (info.nonItemTile) {
    return `Type: ${info.tileType}`
  } else if (info.itemType) {
    return `Type: ${info.itemType}\nLevel: ${info.itemLevel}\nMerge Event Score: ${info.itemMergeEventScore}`
  } else {
    return undefined
  }
}

function getLocalizedIslandNameString(islandId: string): string {
  if (localizationData.value?.locs.en.translations['Island.' + islandId]) {
    return localizationData.value.locs.en.translations['Island.' + islandId]
  } else {
    return islandId
  }
}

function getLocalizedHeroNameString(key: string): string {
  if (localizationData.value?.locs.en.translations['Hero.' + key]) {
    return localizationData.value.locs.en.translations['Hero.' + key]
  } else {
    return key
  }
}

function getLocalizedItemNameString(key: string): string {
  if (localizationData.value?.locs.en.translations['Chain.' + key]) {
    return localizationData.value.locs.en.translations['Chain.' + key]
  } else {
    return key
  }
}
</script>
