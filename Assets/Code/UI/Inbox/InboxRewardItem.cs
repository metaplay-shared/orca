using Code.UI.AssetManagement;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Code.UI.Inbox {
	public class InboxRewardItem : MonoBehaviour {
		[SerializeField] private Image Icon;
		[SerializeField] private TMP_Text CountText;

		[Inject] private AddressableManager addressableManager;

		public async UniTask Setup(string iconPath, int count) {
			Icon.sprite = await addressableManager.GetLazy<Sprite>(iconPath);
			CountText.text = count.ToString();
		}
	}
}
