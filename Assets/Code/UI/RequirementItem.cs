using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Code.UI {
	public class RequirementItem : MonoBehaviour {
		[SerializeField] private Image StatusIndicator;
		[SerializeField] private Image ContentIcon;
		[SerializeField] private GameObject NumberContainer;
		[SerializeField] private TMP_Text NumberText;
		[SerializeField] private Button Button;

		private bool Ok {
			set => StatusIndicator.color = value ? Color.green : Color.red;
		}

		public void SetAsChain(string chainLinkName, int level, int requiredAmount, Func<bool> isOk) {
			//ContentIcon.sprite = SpriteCatalog.Instance.Get(chainLinkName + level);
			NumberContainer.SetActive(true);
			NumberText.text = requiredAmount.ToString();

			Button.onClick.RemoveAllListeners();
			/*
			Button.onClick.AddListener(
				() => { ChainInfoPopup.Show(new ChainInfoPopupPayload(ChainTypeId.FromString(chainLinkName))); }
			);
			*/

			Ok = isOk.Invoke();
		}

		public void SetAsElementDependency(string elementName, int requiredLevel, Func<bool> isOk) {
			//ContentIcon.sprite = SpriteCatalog.Instance.Get(elementName);
			NumberContainer.SetActive(true);
			NumberText.text = requiredLevel.ToString();

			Ok = isOk.Invoke();
		}

		public void SetAsResource(string resourceName, int requiredAmount, Func<bool> isOk) {
			//ContentIcon.sprite = SpriteCatalog.Instance.Get(resourceName);
			NumberContainer.SetActive(true);
			NumberText.text = requiredAmount.ToString();

			Ok = isOk.Invoke();
		}
	}
}
