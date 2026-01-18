using System;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI
{
	// Token: 0x020002FE RID: 766
	public class UIWiringInspectorPositioner : UIGameObject
	{
		// Token: 0x170001C1 RID: 449
		// (get) Token: 0x06000D4A RID: 3402 RVA: 0x0003FE45 File Offset: 0x0003E045
		public RectTransform LeftWiringInspectorViewContainer
		{
			get
			{
				return this.leftWiringInspectorViewContainer;
			}
		}

		// Token: 0x170001C2 RID: 450
		// (get) Token: 0x06000D4B RID: 3403 RVA: 0x0003FE4D File Offset: 0x0003E04D
		public RectTransform ReftWiringInspectorViewContainer
		{
			get
			{
				return this.rightWiringInspectorViewContainer;
			}
		}

		// Token: 0x04000B6E RID: 2926
		[SerializeField]
		private RectTransform leftWiringInspectorViewContainer;

		// Token: 0x04000B6F RID: 2927
		[SerializeField]
		private RectTransform rightWiringInspectorViewContainer;
	}
}
