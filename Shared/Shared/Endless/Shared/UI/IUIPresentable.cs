using System;
using UnityEngine;

namespace Endless.Shared.UI
{
	// Token: 0x02000238 RID: 568
	public interface IUIPresentable : IPoolableT, IClearable
	{
		// Token: 0x170002A5 RID: 677
		// (get) Token: 0x06000E5B RID: 3675
		Type ModelType { get; }

		// Token: 0x170002A6 RID: 678
		// (get) Token: 0x06000E5C RID: 3676
		Enum Style { get; }

		// Token: 0x170002A7 RID: 679
		// (get) Token: 0x06000E5D RID: 3677
		object ModelAsObject { get; }

		// Token: 0x170002A8 RID: 680
		// (get) Token: 0x06000E5E RID: 3678
		IUIViewable Viewable { get; }

		// Token: 0x170002A9 RID: 681
		// (get) Token: 0x06000E5F RID: 3679
		RectTransform RectTransform { get; }

		// Token: 0x1400004C RID: 76
		// (add) Token: 0x06000E60 RID: 3680
		// (remove) Token: 0x06000E61 RID: 3681
		event Action<object> OnModelChanged;

		// Token: 0x06000E62 RID: 3682
		void SetModelAsObject(object model, bool triggerOnModelChanged);

		// Token: 0x06000E63 RID: 3683
		IUIPresentable SpawnPooledInstance(Transform parent = null);

		// Token: 0x06000E64 RID: 3684
		void ReturnToPool();

		// Token: 0x06000E65 RID: 3685
		void PrewarmPool(int count);
	}
}
