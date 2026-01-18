using System;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Creator.UI
{
	// Token: 0x020001C0 RID: 448
	public class UINewLevelStateModalController : UIGameObject
	{
		// Token: 0x060006A7 RID: 1703 RVA: 0x000222D0 File Offset: 0x000204D0
		private void Start()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			this.nameInputField.onValueChanged.AddListener(new UnityAction<string>(this.HandleCreateButtonInteractability));
			this.createButton.onClick.AddListener(new UnityAction(this.Create));
		}

		// Token: 0x060006A8 RID: 1704 RVA: 0x0002232D File Offset: 0x0002052D
		private void HandleCreateButtonInteractability(string inputValue)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "HandleCreateButtonInteractability", new object[] { inputValue });
			}
			this.createButton.interactable = !this.nameInputField.IsNullOrEmptyOrWhiteSpace(false);
		}

		// Token: 0x060006A9 RID: 1705 RVA: 0x00022368 File Offset: 0x00020568
		private void Create()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Create", Array.Empty<object>());
			}
			if (this.nameInputField.IsNullOrEmptyOrWhiteSpace(true))
			{
				return;
			}
			string text = this.nameInputField.text;
			string text2 = this.descriptionInputField.text;
			LevelStateTemplateSourceBase levelStateTemplateSourceBase = this.levelStateTemplateListModel.SelectedTypedList[0];
			MonoBehaviourSingleton<UIModalManager>.Instance.CloseAndClearStack();
			Action<string, string, LevelStateTemplateSourceBase> createLevel = UINewLevelStateModalController.CreateLevel;
			if (createLevel == null)
			{
				return;
			}
			createLevel(text, text2, levelStateTemplateSourceBase);
		}

		// Token: 0x040005F7 RID: 1527
		public static Action<string, string, LevelStateTemplateSourceBase> CreateLevel;

		// Token: 0x040005F8 RID: 1528
		[SerializeField]
		private UIInputField nameInputField;

		// Token: 0x040005F9 RID: 1529
		[SerializeField]
		private UIInputField descriptionInputField;

		// Token: 0x040005FA RID: 1530
		[SerializeField]
		private UILevelStateTemplateListModel levelStateTemplateListModel;

		// Token: 0x040005FB RID: 1531
		[SerializeField]
		private UIButton createButton;

		// Token: 0x040005FC RID: 1532
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;
	}
}
