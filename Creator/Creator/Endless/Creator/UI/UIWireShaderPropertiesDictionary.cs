using System;
using UnityEngine;

namespace Endless.Creator.UI
{
	// Token: 0x020002FB RID: 763
	[CreateAssetMenu(menuName = "ScriptableObject/UI/Creator/Dictionaries/Wire Shader Properties Dictionary")]
	public class UIWireShaderPropertiesDictionary : ScriptableObject
	{
		// Token: 0x170001BA RID: 442
		// (get) Token: 0x06000D32 RID: 3378 RVA: 0x0003F796 File Offset: 0x0003D996
		// (set) Token: 0x06000D33 RID: 3379 RVA: 0x0003F79E File Offset: 0x0003D99E
		public string Texture { get; private set; } = "_Texture";

		// Token: 0x170001BB RID: 443
		// (get) Token: 0x06000D34 RID: 3380 RVA: 0x0003F7A7 File Offset: 0x0003D9A7
		// (set) Token: 0x06000D35 RID: 3381 RVA: 0x0003F7AF File Offset: 0x0003D9AF
		public string Tiling { get; private set; } = "_Tiling";

		// Token: 0x170001BC RID: 444
		// (get) Token: 0x06000D36 RID: 3382 RVA: 0x0003F7B8 File Offset: 0x0003D9B8
		// (set) Token: 0x06000D37 RID: 3383 RVA: 0x0003F7C0 File Offset: 0x0003D9C0
		public string Flip { get; private set; } = "_Flip";

		// Token: 0x170001BD RID: 445
		// (get) Token: 0x06000D38 RID: 3384 RVA: 0x0003F7C9 File Offset: 0x0003D9C9
		// (set) Token: 0x06000D39 RID: 3385 RVA: 0x0003F7D1 File Offset: 0x0003D9D1
		public string StartColor { get; private set; } = "_Start_Color";

		// Token: 0x170001BE RID: 446
		// (get) Token: 0x06000D3A RID: 3386 RVA: 0x0003F7DA File Offset: 0x0003D9DA
		// (set) Token: 0x06000D3B RID: 3387 RVA: 0x0003F7E2 File Offset: 0x0003D9E2
		public string EndColor { get; private set; } = "_End_Color";

		// Token: 0x170001BF RID: 447
		// (get) Token: 0x06000D3C RID: 3388 RVA: 0x0003F7EB File Offset: 0x0003D9EB
		// (set) Token: 0x06000D3D RID: 3389 RVA: 0x0003F7F3 File Offset: 0x0003D9F3
		public string MainTex { get; private set; } = "_MainTex";
	}
}
