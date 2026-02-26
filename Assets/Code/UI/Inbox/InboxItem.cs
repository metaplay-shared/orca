using Cysharp.Threading.Tasks;
using Game.Logic;
using Metaplay.Core.InGameMail;
using Metaplay.Core.Localization;
using Metaplay.Core.Player;
using Metaplay.Core.Rewards;
using Metaplay.Unity.DefaultIntegration;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Code.UI.Inbox {
	public class InboxItem : MonoBehaviour {
		[SerializeField] private TMP_Text Title;
		[SerializeField] private TMP_Text MessageContent;
		[SerializeField] private Button DeleteButton;
		[SerializeField] private Button ConsumeButton;

		[SerializeField] private InboxRewardItem PrefabInboxRewardItem;
		[SerializeField] private RectTransform RewardContainer;

		[Inject] private DiContainer container;

		private PlayerMailItem mailItem;

		public void Setup(PlayerMailItem playerMailItem) {
			mailItem = playerMailItem;
			SimplePlayerMail contents = ((SimplePlayerMail)mailItem.Contents);
			Title.text = contents.Title.Localizations[LanguageId.FromString("en")];

			if (contents.Body.Localizations != null) {
				MessageContent.text =
					((SimplePlayerMail)mailItem.Contents).Body.Localizations[LanguageId.FromString("en")];
			} else {
				MessageContent.text = "";
			}

			if (contents.Attachments?.Count > 0) {
				foreach (MetaPlayerRewardBase attachment in contents.Attachments) {
					if (attachment is PlayerRewardItem rewardItem) {
						string item = rewardItem.ChainId.Type.Value;
						string itemAndLevel = item + rewardItem.ChainId.Level;
						SpawnRewardIcon($"Chains/{item}/{itemAndLevel}.png", rewardItem.Amount);
					} else if (attachment is RewardCurrency rewardResource) {
						string item = rewardResource.CurrencyId.Value;
						SpawnRewardIcon($"Icons/{item}.png", rewardResource.Amount);
					}
				}
			}

			UpdateState();
			MarkAsSeen();
		}

		private void SpawnRewardIcon(string iconPath, int count) {
			InboxRewardItem rewardItem = container.InstantiatePrefabForComponent<InboxRewardItem>(PrefabInboxRewardItem, RewardContainer);
			rewardItem.Setup(iconPath, count).Forget();
		}

		private void MarkAsSeen() {
			MetaplayClient.PlayerContext.ExecuteAction(new PlayerToggleMailIsRead(mailItem.Id, true));
		}

		private void UpdateState() {
			if (mailItem.Contents.MustBeConsumed &&
				!mailItem.HasBeenConsumed) {
				DeleteButton.gameObject.SetActive(false);
				ConsumeButton.gameObject.SetActive(true);
				ConsumeButton.onClick.AddListener(ConsumeMail);
			} else {
				ConsumeButton.gameObject.SetActive(false);
				DeleteButton.gameObject.SetActive(true);
				DeleteButton.onClick.AddListener(DeleteMail);
			}
		}

		private void DeleteMail() {
			MetaplayClient.PlayerContext.ExecuteAction(new PlayerDeleteMail(mailItem.Id));
			Destroy(gameObject);
		}

		private void ConsumeMail() {
			MetaplayClient.PlayerContext.ExecuteAction(new PlayerConsumeMail(mailItem.Id));
			UpdateState();
		}
	}
}
