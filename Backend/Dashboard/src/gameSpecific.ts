// This file is part of Metaplay SDK which is released under the Metaplay SDK License.
// Import the integration API.
import type { App } from 'vue'

import { setGameSpecificInitialization } from '@metaplay/core'

/**
 * This is a Vue 3 plugin function that gets called after the SDK CorePlugin is registered but before the application is mounted.
 * Use this function to register any Vue components or plugins that you want to use to customize the dashboard.
 * @param app The Vue app instance.
 */
export function GameSpecificPlugin(app: App): void {
  // Feel free to add any customization logic here for your game!

  setGameSpecificInitialization(async (initializationApi) => {
    await Promise.all([
      initializationApi.getGameDataByLibrary(['Heroes', 'HeroTasks']),
      initializationApi.getGameDataByLibrary(['Chains']),
      initializationApi.getGameDataByLibrary(['Chains', 'Islands', 'IslandTasks'])
    ])

    // Custom IAPs
    initializationApi.addInAppPurchaseContents([
      {
        $type: 'Game.Logic.ResolvedInAppProductContent',
        getDisplayContent: (purchase) => {
          const val: string[] = []
          purchase.resources.forEach((element: { amount: string; type: string }) => {
            val.push(element.amount + 'x ' + element.type)
          })

          purchase.items.forEach((element: { count: string; level: string; type: string }) => {
            val.push(element.count + 'x ' + element.type + element.level)
          })

          return val
        },
      },
    ])

    initializationApi.addGeneratedUiFormComponent({
      filterFunction: (_props, type) => {
        return type.typeName === 'Game.Logic.LevelId<Game.Logic.ChainTypeId>'
      },
      vueComponent: async () => await import('./ChainLevelIdField.vue'),
    })

    // Custom rewards
    initializationApi.addPlayerRewards([
      {
        $type: 'Game.Logic.RewardCurrency',
        getDisplayValue: (reward) => `${reward.currencyId} x${reward.amount}`,
      },
      {
        $type: 'Game.Logic.PlayerRewardItem',
        getDisplayValue: (reward) => `Level ${reward.chainId.level} ${reward.chainId.type} x${reward.amount}`,
      },
      {
        $type: 'Game.Logic.RewardResource',
        getDisplayValue: (reward) => `${reward.currencyType} x${reward.amount}`,
      },
      {
        $type: 'Game.Logic.RewardItem',
        getDisplayValue: (reward) => `${reward.itemType} lvl ${reward.level} x${reward.amount}`,
      },
    ])

    const gameData = await initializationApi.getGameData()
    initializationApi.addPlayerResources([
      {
        displayName: 'Level',
        getAmount: (playerModel) => playerModel.level.level,
      },
      {
        displayName: 'Exp',
        getAmount: (playerModel) => {
          return (
            playerModel.level.currentXp + '/' + gameData.gameConfig.PlayerLevels[playerModel.level.level].xpToNextLevel
          )
        },
      },
      {
        displayName: 'Gold',
        getAmount: (playerModel) => playerModel.wallet.gold.purchased + playerModel.wallet.gold.earned,
      },
      {
        displayName: 'Gems',
        getAmount: (playerModel) => playerModel.wallet.gems.purchased + playerModel.wallet.gems.earned,
      },
      {
        displayName: 'Energy',
        getAmount: (playerModel) => playerModel.merge.energy.producedAtUpdate,
      },
      {
        displayName: 'Builders Free',
        getAmount: (playerModel) => playerModel.builders.free,
      },
      {
        displayName: 'Builders Total',
        getAmount: (playerModel) => playerModel.builders.total,
      },
      {
        displayName: 'Keys',
        getAmount: (playerModel) => playerModel.wallet.islandTokens.purchased + playerModel.wallet.islandTokens.earned,
      },
    ])

    // Inject custom content into the player details page to render a player's heroes and inventory nicely.
    initializationApi.addUiComponent(
      'Players/Details/Tab0',
      {
        uniqueId: 'InventoryCard',
        vueComponent: async () => await import('./HeroesAndInventoryCards.vue'),
      },
      { position: 'before' }
    )

    // Inject custom content into the player details page to render the player's merge board.
    initializationApi.addUiComponent(
      'Players/Details/Tab0',
      {
        uniqueId: 'MergeBoardCard',
        vueComponent: async () => await import('./MergeBoardCard.vue'),
      },
      { position: 'before' }
    )

    // Inject custom action button into the player admin tools.
    initializationApi.addUiComponent(
      'Players/Details/AdminActions:Gentle',
      {
        uniqueId: 'RewardPlayerCurrency',
        vueComponent: async () => await import('./PlayerActionRewardPlayerCurrency.vue'),
      },
      { position: 'before' }
    )
  })
}
