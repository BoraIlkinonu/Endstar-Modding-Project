using System;
using Endless.Shared.Debugging;
using Endless.Shared.Tweens;
using Endless.Shared.Validation;
using Runtime.Shared.Extensions;
using UnityEngine;
using UnityEngine.UI;

namespace Endless.Shared.UI.Windows
{
	// Token: 0x0200028D RID: 653
	public class UIWindowTweenHandler : MonoBehaviour, IValidatable
	{
		// Token: 0x06001067 RID: 4199 RVA: 0x00045BC8 File Offset: 0x00043DC8
		public void PlayOpenTween(Action onComplete = null)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "PlayOpenTween", new object[] { onComplete.DebugIsNull() });
			}
			this.graphicRaycaster.Enable();
			this.openTweenCollection.SetToStart();
			if (onComplete != null)
			{
				this.openTweenCollection.Tween(onComplete);
				return;
			}
			this.openTweenCollection.Tween();
		}

		// Token: 0x06001068 RID: 4200 RVA: 0x00045C28 File Offset: 0x00043E28
		public bool PlayCloseTween(Action onComplete)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "PlayCloseTween", new object[] { onComplete.DebugIsNull() });
			}
			if (this.closeTweenCollection.IsAnyTweening())
			{
				Debug.LogError("closeTweenCollection is already tweening!", this);
				return false;
			}
			this.graphicRaycaster.Disable();
			this.closeTweenCollection.Tween(onComplete);
			return true;
		}

		// Token: 0x06001069 RID: 4201 RVA: 0x00045C89 File Offset: 0x00043E89
		public void Validate()
		{
			DebugUtility.DebugIsNull("graphicRaycaster", this.graphicRaycaster, this);
			DebugUtility.DebugIsNull("openTweenCollection", this.openTweenCollection, this);
			DebugUtility.DebugIsNull("closeTweenCollection", this.closeTweenCollection, this);
		}

		// Token: 0x04000A5F RID: 2655
		[SerializeField]
		private GraphicRaycaster graphicRaycaster;

		// Token: 0x04000A60 RID: 2656
		[SerializeField]
		private TweenCollection openTweenCollection;

		// Token: 0x04000A61 RID: 2657
		[SerializeField]
		private TweenCollection closeTweenCollection;

		// Token: 0x04000A62 RID: 2658
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;
	}
}
