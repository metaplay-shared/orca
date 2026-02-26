using Code.UI.AssetManagement;
using Code.UI.HudBase;
using Code.UI.Merge;
using Code.UI.Utils;
using Cysharp.Threading.Tasks;
using Game.Logic;
using Metaplay.Unity.DefaultIntegration;
using System.Runtime.InteropServices;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Code.UI.Events {
	public class RewardRoadItem : MonoBehaviour {
		[SerializeField] private Image Icon;
		[SerializeField] private RectTransform IconSize;
		[SerializeField] private GameObject ClaimedIcon;
		[SerializeField] private Image Glow;
		[SerializeField] private TMP_Text CountLabel;

		[Inject] private AddressableManager addressableManager;

		private EventId eventId;
		private bool canBeClaimed;

		public void Setup(EventId eventId, ItemCountInfo itemInfo, bool freeToClaim, bool claimed, bool available) {
			this.eventId = eventId;
			if (itemInfo.ChainId.Type == ChainTypeId.None) {
				Icon.gameObject.SetActive(false);
				CountLabel.gameObject.SetActive(false);
				ClaimedIcon.SetActive(false);
				Glow.gameObject.SetActive(false);
				canBeClaimed = false;
			} else {
				canBeClaimed = freeToClaim && !claimed;
				ChainInfo chainInfo = MetaplayClient.PlayerModel.GameConfig.Chains[itemInfo.ChainId];
				Icon.sprite = addressableManager.GetItemIcon(chainInfo);
				CountLabel.text = $"x{itemInfo.Count}";
				ClaimedIcon.SetActive(claimed);
				Glow.gameObject.SetActive(freeToClaim);

				if (available && !claimed) {
					Icon.color = Color.white;
				} else {
					Color color = Color.gray;
					color.a = 0.5f;
					Icon.color = color;
				}
			}
		}

		private void Update() {
			if (canBeClaimed) {
				Glow.transform.Rotate(0, 0, Time.deltaTime * 50);
				Icon.transform.localScale = Vector3.one * (1 + Mathf.Sin(Time.time * 5) * 0.1f);
			}
		}

		public void ClaimClicked() {
			if (canBeClaimed) {
				MetaplayClient.PlayerContext.ExecuteAction(new PlayerClaimActivityEventRewards(eventId));
			}
		}

		private GameObject CopyRewardIcon() {
			GameObject topLayer = GameObject.Find("TopLayer");

			GameObject flyingGo = new GameObject();
			Image flyingImage = flyingGo.AddComponent<Image>();
			flyingImage.raycastTarget = false;
			flyingImage.sprite = Icon.sprite;
			RectTransform flyingRt = flyingGo.GetComponent<RectTransform>();
			flyingRt.SetParent(topLayer.transform, false);
			flyingRt.sizeDelta = IconSize.sizeDelta;
			flyingRt.position = Icon.transform.position;
			return flyingGo;
		}

		public async UniTask FlyObject(bool mainIsland) {
			GameObject objectToFly = CopyRewardIcon();
			FlightTarget flightTarget = mainIsland
				? FindObjectOfType<DockFlightTarget>()
				: FindObjectOfType<MapFlightTarget>();

			if (flightTarget == null) {
				return;
			}

			if (!flightTarget.CanFlyHere) {
				return;
			}

			await flightTarget.FlyFromAsync(objectToFly.GetComponent<RectTransform>());
		}
	}
}
