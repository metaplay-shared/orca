<!-- This file is part of Metaplay SDK which is released under the Metaplay SDK License. -->

<template lang="pug">
div(v-if="playerData")
  MActionModalButton(
    modal-title="Add Currency"
    :action="rewardPlayerCurrency"
    trigger-button-label="Add Currency"
    trigger-button-full-width
    variant="primary"
    ok-button-label="Add"
    permission="api.players.reward_currency"
    @show="resetModal"
    data-testid="action-reward-currency"
    )
    template(#default)
      p You can add gold and gems to #[MBadge {{ playerData.model.playerName }}]. Adding currency occurs silently and instantaneously.
      MInputNumber(
        label="Gold"
        :model-value="newGold"
        :min="0"
        :hint-message="`Player currently has ${playerData.model.wallet.gold.earned + playerData.model.wallet.gold.purchased} gold.`"
        @update:model-value="newGold = $event"
        )
      MInputNumber(
        label="Gems"
        :model-value="newGems"
        :min="0"
        :hint-message="`Player currently has ${playerData.model.wallet.gems.earned + playerData.model.wallet.gems.purchased} gems.`"
        @update:model-value="newGems = $event"
        )
</template>

<script lang="ts" setup>
import { ref } from 'vue'

import { getSinglePlayerSubscriptionOptions } from '@metaplay/core'
import { useGameServerApi } from '@metaplay/game-server-api'
import { useNotifications, MBadge, MInputNumber, MActionModalButton } from '@metaplay/meta-ui-next'
import { useSubscription } from '@metaplay/subscriptions'

const { showSuccessNotification, showErrorNotification } = useNotifications()

const props = defineProps<{
  /**
   * ID of the player to edit.
   */
  playerId: string
}>()

const gameServerApi = useGameServerApi()
const { data: playerData, refresh: playerRefresh } = useSubscription(() =>
  getSinglePlayerSubscriptionOptions(props.playerId)
)

const newGold = ref<number | undefined>(0)
const newGems = ref<number | undefined>(0)

function resetModal(): void {
  newGold.value = 0
  newGems.value = 0
}

async function rewardPlayerCurrency(): Promise<void> {
  await gameServerApi.post(`/players/${props.playerId}/rewardCurrency`, {
    newGold: newGold.value,
    newGems: newGems.value,
  })
  showSuccessNotification('Currency rewarded.')
  playerRefresh()
}
</script>
