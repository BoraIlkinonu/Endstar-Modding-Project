using System;
using Endless.Gameplay.SoVariables;
using Endless.Shared;
using Endless.Shared.Debugging;
using UnityEngine;

namespace Endless.Gameplay.UI
{
	// Token: 0x0200038A RID: 906
	public class UIInteractionPromptAnchorManager : MonoBehaviourSingleton<UIInteractionPromptAnchorManager>
	{
		// Token: 0x0600171D RID: 5917 RVA: 0x0006C040 File Offset: 0x0006A240
		public UIInteractionPromptAnchor CreateInstance(Transform target, PlayerReferenceManager playerReferenceManager, UIInteractionPromptVariable interactionPrompt, Vector3? offset = null)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "CreateInstance", new object[] { target, this.container, playerReferenceManager, interactionPrompt, offset });
			}
			return UIInteractionPromptAnchor.CreateInstance(this.interactionPromptAnchorSource, target, this.container, playerReferenceManager, interactionPrompt, offset);
		}

		// Token: 0x0400128A RID: 4746
		[SerializeField]
		private UIInteractionPromptAnchor interactionPromptAnchorSource;

		// Token: 0x0400128B RID: 4747
		[SerializeField]
		private RectTransform container;

		// Token: 0x0400128C RID: 4748
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;
	}
}
