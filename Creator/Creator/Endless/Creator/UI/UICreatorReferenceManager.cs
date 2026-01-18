using System;
using Endless.Shared;
using UnityEngine;

namespace Endless.Creator.UI
{
	// Token: 0x020000B5 RID: 181
	public class UICreatorReferenceManager : MonoBehaviourSingleton<UICreatorReferenceManager>
	{
		// Token: 0x17000036 RID: 54
		// (get) Token: 0x060002D3 RID: 723 RVA: 0x00012AB3 File Offset: 0x00010CB3
		// (set) Token: 0x060002D4 RID: 724 RVA: 0x00012ABB File Offset: 0x00010CBB
		public RectTransform AnchorContainer { get; private set; }
	}
}
