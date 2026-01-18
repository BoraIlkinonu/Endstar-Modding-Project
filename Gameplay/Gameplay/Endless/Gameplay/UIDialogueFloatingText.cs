using System;
using Endless.Shared.Tweens;
using TMPro;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x020002D1 RID: 721
	public class UIDialogueFloatingText : UISelfPoolingTween
	{
		// Token: 0x06001059 RID: 4185 RVA: 0x00052DD8 File Offset: 0x00050FD8
		public void SetupLocalPositionTween()
		{
			this.localPositionTween.From = this.localPositionTween.Target.transform.localPosition;
			this.localPositionTween.To = this.localPositionTween.From + new Vector3(0f, this.tweenDistance, 0f);
		}

		// Token: 0x0600105A RID: 4186 RVA: 0x00052E35 File Offset: 0x00051035
		public void SetDisplayText(string displayText)
		{
			this.text.SetText(displayText, true);
		}

		// Token: 0x04000E09 RID: 3593
		[SerializeField]
		private TextMeshProUGUI text;

		// Token: 0x04000E0A RID: 3594
		[SerializeField]
		private TweenLocalPosition localPositionTween;

		// Token: 0x04000E0B RID: 3595
		[SerializeField]
		private float tweenDistance;
	}
}
