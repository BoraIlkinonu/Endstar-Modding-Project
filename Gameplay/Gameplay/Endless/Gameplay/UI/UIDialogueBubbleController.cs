using System;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Gameplay.UI
{
	// Token: 0x02000398 RID: 920
	[RequireComponent(typeof(UIDialogueBubbleAnchor))]
	public class UIDialogueBubbleController : UIGameObject
	{
		// Token: 0x06001772 RID: 6002 RVA: 0x0006D3D4 File Offset: 0x0006B5D4
		private void Start()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			base.TryGetComponent<UIDialogueBubbleAnchor>(out this.view);
			this.displayNextTextButton.onClick.AddListener(new UnityAction(this.DisplayNextText));
		}

		// Token: 0x06001773 RID: 6003 RVA: 0x0006D422 File Offset: 0x0006B622
		private void DisplayNextText()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "DisplayNextText", Array.Empty<object>());
			}
			throw new NotImplementedException();
		}

		// Token: 0x040012DC RID: 4828
		[SerializeField]
		private UIButton displayNextTextButton;

		// Token: 0x040012DD RID: 4829
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;

		// Token: 0x040012DE RID: 4830
		private UIDialogueBubbleAnchor view;
	}
}
