using System;
using Endless.Shared.Debugging;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Shared.UI
{
	// Token: 0x0200012F RID: 303
	[RequireComponent(typeof(UIDragHandler))]
	public class UITestDragInstance : UIGameObject, IDragInstance
	{
		// Token: 0x17000143 RID: 323
		// (get) Token: 0x0600076B RID: 1899 RVA: 0x0001F409 File Offset: 0x0001D609
		public GameObject GameObject
		{
			get
			{
				return base.gameObject;
			}
		}

		// Token: 0x17000144 RID: 324
		// (get) Token: 0x0600076C RID: 1900 RVA: 0x0001F411 File Offset: 0x0001D611
		public UIDragHandler DragHandler
		{
			get
			{
				if (!this.dragHandler)
				{
					base.TryGetComponent<UIDragHandler>(out this.dragHandler);
				}
				return this.dragHandler;
			}
		}

		// Token: 0x0600076D RID: 1901 RVA: 0x0001F433 File Offset: 0x0001D633
		private void Start()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			this.dragHandler.EndDragUnityEvent.AddListener(new UnityAction(this.OnEndDrag));
		}

		// Token: 0x0600076E RID: 1902 RVA: 0x0001F469 File Offset: 0x0001D669
		public void OnSpawn()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnSpawn", Array.Empty<object>());
			}
		}

		// Token: 0x0600076F RID: 1903 RVA: 0x0001F483 File Offset: 0x0001D683
		public void OnDespawn()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnDespawn", Array.Empty<object>());
			}
		}

		// Token: 0x06000770 RID: 1904 RVA: 0x0001F49D File Offset: 0x0001D69D
		private void OnEndDrag()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnEndDrag", Array.Empty<object>());
			}
		}

		// Token: 0x0400045D RID: 1117
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;

		// Token: 0x0400045E RID: 1118
		private UIDragHandler dragHandler;
	}
}
