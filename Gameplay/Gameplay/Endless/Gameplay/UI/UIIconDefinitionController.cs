using System;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Gameplay.UI
{
	// Token: 0x020003AB RID: 939
	public class UIIconDefinitionController : UIGameObject
	{
		// Token: 0x170004EE RID: 1262
		// (get) Token: 0x060017F8 RID: 6136 RVA: 0x0006F699 File Offset: 0x0006D899
		public UnityEvent<IconDefinition> SelectUnityEvent { get; } = new UnityEvent<IconDefinition>();

		// Token: 0x060017F9 RID: 6137 RVA: 0x0006F6A1 File Offset: 0x0006D8A1
		private void Start()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			this.selectButton.onClick.AddListener(new UnityAction(this.OnSelect));
		}

		// Token: 0x060017FA RID: 6138 RVA: 0x0006F6D7 File Offset: 0x0006D8D7
		private void OnSelect()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnSelect", Array.Empty<object>());
			}
			this.SelectUnityEvent.Invoke(this.view.Model);
		}

		// Token: 0x0400133E RID: 4926
		[SerializeField]
		private UIIconDefinitionView view;

		// Token: 0x0400133F RID: 4927
		[SerializeField]
		private UIButton selectButton;

		// Token: 0x04001340 RID: 4928
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;
	}
}
