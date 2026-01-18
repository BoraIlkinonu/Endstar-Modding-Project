using System;
using Endless.Shared.Debugging;
using Endless.Shared.Tweens;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Shared.UI
{
	// Token: 0x02000141 RID: 321
	public class UIPointerEnterAndExitHandler : UIGameObject
	{
		// Token: 0x060007F3 RID: 2035 RVA: 0x0002196C File Offset: 0x0001FB6C
		private void Start()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			this.pointerEnterHandler.PointerEnterUnityEvent.AddListener(new UnityAction(this.PlayPointerEnterTweenCollection));
			this.pointerExitHandler.PointerExitUnityEvent.AddListener(new UnityAction(this.PlayPointerExitTweenCollection));
		}

		// Token: 0x060007F4 RID: 2036 RVA: 0x000219C9 File Offset: 0x0001FBC9
		private void PlayPointerEnterTweenCollection()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "PlayPointerEnterTweenCollection", Array.Empty<object>());
			}
			this.pointerEnterTweenCollection.Tween();
		}

		// Token: 0x060007F5 RID: 2037 RVA: 0x000219EE File Offset: 0x0001FBEE
		private void PlayPointerExitTweenCollection()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "PlayPointerExitTweenCollection", Array.Empty<object>());
			}
			this.pointerExitTweenCollection.Tween();
		}

		// Token: 0x040004CC RID: 1228
		[SerializeField]
		private PointerEnterHandler pointerEnterHandler;

		// Token: 0x040004CD RID: 1229
		[SerializeField]
		private PointerExitHandler pointerExitHandler;

		// Token: 0x040004CE RID: 1230
		[SerializeField]
		private TweenCollection pointerEnterTweenCollection;

		// Token: 0x040004CF RID: 1231
		[SerializeField]
		private TweenCollection pointerExitTweenCollection;

		// Token: 0x040004D0 RID: 1232
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;
	}
}
