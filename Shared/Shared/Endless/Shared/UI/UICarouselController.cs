using System;
using Endless.Shared.Debugging;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Shared.UI
{
	// Token: 0x02000114 RID: 276
	[RequireComponent(typeof(UICarouselView))]
	public class UICarouselController : UIGameObject
	{
		// Token: 0x060006A4 RID: 1700 RVA: 0x0001C41C File Offset: 0x0001A61C
		private void Start()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			base.TryGetComponent<UICarouselView>(out this.view);
			this.backButton.onClick.AddListener(new UnityAction(this.view.Back));
			this.nextButton.onClick.AddListener(new UnityAction(this.view.Next));
		}

		// Token: 0x040003DB RID: 987
		[SerializeField]
		private UIButton backButton;

		// Token: 0x040003DC RID: 988
		[SerializeField]
		private UIButton nextButton;

		// Token: 0x040003DD RID: 989
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;

		// Token: 0x040003DE RID: 990
		private UICarouselView view;
	}
}
