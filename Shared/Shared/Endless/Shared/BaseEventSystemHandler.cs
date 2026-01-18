using System;
using Endless.Shared.Debugging;
using UnityEngine;

namespace Endless.Shared
{
	// Token: 0x0200004F RID: 79
	public abstract class BaseEventSystemHandler<T> : MonoBehaviour where T : BaseEventSystemHandler<T>
	{
		// Token: 0x17000060 RID: 96
		// (get) Token: 0x06000299 RID: 665 RVA: 0x0000D67C File Offset: 0x0000B87C
		// (set) Token: 0x0600029A RID: 666 RVA: 0x0000D684 File Offset: 0x0000B884
		protected bool VerboseLogging { get; set; }

		// Token: 0x17000061 RID: 97
		// (get) Token: 0x0600029B RID: 667 RVA: 0x0000D68D File Offset: 0x0000B88D
		// (set) Token: 0x0600029C RID: 668 RVA: 0x0000D695 File Offset: 0x0000B895
		protected bool SuperVerboseLogging { get; set; }

		// Token: 0x17000062 RID: 98
		// (get) Token: 0x0600029D RID: 669 RVA: 0x0000D69E File Offset: 0x0000B89E
		// (set) Token: 0x0600029E RID: 670 RVA: 0x0000D6A6 File Offset: 0x0000B8A6
		private protected bool BlockAllEvents { protected get; private set; }

		// Token: 0x17000063 RID: 99
		// (get) Token: 0x0600029F RID: 671 RVA: 0x0000D6AF File Offset: 0x0000B8AF
		protected bool HasIntercepter
		{
			get
			{
				if (this.hasIntercepterSet)
				{
					return this.hasIntercepter;
				}
				this.hasIntercepter = this.Intercepter != null;
				this.hasIntercepterSet = true;
				return this.hasIntercepter;
			}
		}

		// Token: 0x060002A0 RID: 672 RVA: 0x0000D6DF File Offset: 0x0000B8DF
		protected virtual void OnDisable()
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log("OnDisable", this);
			}
			if (this.ClearIntercepterOnDisable)
			{
				this.SetIntercepter(null);
			}
		}

		// Token: 0x060002A1 RID: 673 RVA: 0x0000D703 File Offset: 0x0000B903
		public void SetBlockAllEvents(bool newValue)
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "SetBlockAllEvents", "newValue", newValue), this);
			}
			this.BlockAllEvents = newValue;
		}

		// Token: 0x060002A2 RID: 674 RVA: 0x0000D734 File Offset: 0x0000B934
		public void SetIntercepter(BaseEventSystemHandler<T> newValue)
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log("SetIntercepter ( newValue: " + newValue.DebugSafeName(true) + " )", this);
			}
			this.Intercepter = newValue;
			this.hasIntercepter = this.Intercepter != null;
			this.hasIntercepterSet = true;
		}

		// Token: 0x04000152 RID: 338
		[Header("BaseEventSystemHandler")]
		[SerializeField]
		protected BaseEventSystemHandler<T> Intercepter;

		// Token: 0x04000153 RID: 339
		[SerializeField]
		protected bool ClearIntercepterOnDisable = true;

		// Token: 0x04000156 RID: 342
		private bool hasIntercepterSet;

		// Token: 0x04000157 RID: 343
		private bool hasIntercepter;
	}
}
