using System;
using Endless.Shared.Debugging;
using UnityEngine;

namespace Endless.Shared.UI
{
	// Token: 0x02000242 RID: 578
	[RequireComponent(typeof(UIInvisibleInteractableGraphic))]
	public class UIPooledInteractionBlocker : UIGameObject, IPoolableT
	{
		// Token: 0x170002C3 RID: 707
		// (get) Token: 0x06000EB4 RID: 3764 RVA: 0x0003F7A2 File Offset: 0x0003D9A2
		// (set) Token: 0x06000EB5 RID: 3765 RVA: 0x0003F7AA File Offset: 0x0003D9AA
		public MonoBehaviour Prefab { get; set; }

		// Token: 0x170002C4 RID: 708
		// (get) Token: 0x06000EB6 RID: 3766 RVA: 0x000050D2 File Offset: 0x000032D2
		public bool IsUi
		{
			get
			{
				return true;
			}
		}

		// Token: 0x06000EB7 RID: 3767 RVA: 0x0003F7B4 File Offset: 0x0003D9B4
		public void OnSpawn()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnSpawn", Array.Empty<object>());
			}
			base.RectTransform.SetAsLastSibling();
			base.RectTransform.SetAnchor(AnchorPresets.StretchAll, 0f, 0f, 0f, 0f);
		}

		// Token: 0x06000EB8 RID: 3768 RVA: 0x0003F805 File Offset: 0x0003DA05
		public void OnDespawn()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnDespawn", Array.Empty<object>());
			}
		}

		// Token: 0x04000940 RID: 2368
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;
	}
}
