using System;
using UnityEngine;

namespace Endless.Shared.Tweens
{
	// Token: 0x020000BA RID: 186
	public class CachedTweenAnchoredPosition
	{
		// Token: 0x0600052F RID: 1327 RVA: 0x00016DF0 File Offset: 0x00014FF0
		public CachedTweenAnchoredPosition(TweenAnchoredPosition tweenAnchoredPosition)
		{
			this.TweenAnchoredPosition = tweenAnchoredPosition;
			this.From = tweenAnchoredPosition.From;
			this.To = tweenAnchoredPosition.To;
			this.TweenAnchoredPosition.SetToStart();
		}

		// Token: 0x06000530 RID: 1328 RVA: 0x00016E43 File Offset: 0x00015043
		public void Tween(bool direction)
		{
			this.TweenAnchoredPosition.To = (direction ? this.To : this.From);
			this.TweenAnchoredPosition.Tween();
		}

		// Token: 0x040002A0 RID: 672
		public Vector2 From = Vector2.zero;

		// Token: 0x040002A1 RID: 673
		public Vector2 To = Vector2.zero;

		// Token: 0x040002A2 RID: 674
		public TweenAnchoredPosition TweenAnchoredPosition;
	}
}
