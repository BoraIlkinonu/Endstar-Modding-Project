using System;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Gameplay.UI
{
	// Token: 0x0200038D RID: 909
	public class UICharacterCosmeticsDefinitionPortraitController : UIGameObject
	{
		// Token: 0x170004CE RID: 1230
		// (get) Token: 0x06001727 RID: 5927 RVA: 0x0006C5AA File Offset: 0x0006A7AA
		public UnityEvent<CharacterCosmeticsDefinition> SelectUnityEvent { get; } = new UnityEvent<CharacterCosmeticsDefinition>();

		// Token: 0x06001728 RID: 5928 RVA: 0x0006C5B2 File Offset: 0x0006A7B2
		private void Start()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			this.selectButton.onClick.AddListener(new UnityAction(this.OnSelect));
		}

		// Token: 0x06001729 RID: 5929 RVA: 0x0006C5E8 File Offset: 0x0006A7E8
		private void OnSelect()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnSelect", Array.Empty<object>());
			}
			this.SelectUnityEvent.Invoke(this.view.CharacterCosmeticsDefinition);
		}

		// Token: 0x04001295 RID: 4757
		[SerializeField]
		private UICharacterCosmeticsDefinitionPortraitView view;

		// Token: 0x04001296 RID: 4758
		[SerializeField]
		private UIButton selectButton;

		// Token: 0x04001297 RID: 4759
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;
	}
}
