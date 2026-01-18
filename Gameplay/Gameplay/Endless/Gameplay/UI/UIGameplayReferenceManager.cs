using System;
using Endless.Shared;
using UnityEngine;

namespace Endless.Gameplay.UI
{
	// Token: 0x020003A8 RID: 936
	public class UIGameplayReferenceManager : MonoBehaviourSingleton<UIGameplayReferenceManager>
	{
		// Token: 0x170004EA RID: 1258
		// (get) Token: 0x060017E5 RID: 6117 RVA: 0x0006F396 File Offset: 0x0006D596
		// (set) Token: 0x060017E6 RID: 6118 RVA: 0x0006F39E File Offset: 0x0006D59E
		public RectTransform AnchorContainer { get; private set; }

		// Token: 0x170004EB RID: 1259
		// (get) Token: 0x060017E7 RID: 6119 RVA: 0x0006F3A7 File Offset: 0x0006D5A7
		// (set) Token: 0x060017E8 RID: 6120 RVA: 0x0006F3AF File Offset: 0x0006D5AF
		public RectTransform GameplayWindowContainer { get; private set; }
	}
}
