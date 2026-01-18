using System;
using System.Collections.Generic;
using Endless.Shared.Debugging;
using UnityEngine;

namespace Endless.Shared.UI
{
	// Token: 0x02000232 RID: 562
	public class UIPoolablePrewarmer : UIGameObject
	{
		// Token: 0x06000E36 RID: 3638 RVA: 0x0003DB2E File Offset: 0x0003BD2E
		private void Start()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			this.ProcessPrewarmOverrides();
			this.PrewarmAllPools();
		}

		// Token: 0x06000E37 RID: 3639 RVA: 0x0003DB54 File Offset: 0x0003BD54
		private void ProcessPrewarmOverrides()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "ProcessPrewarmOverrides", Array.Empty<object>());
			}
			foreach (UIPoolablePrewarmer.OverridePrewarmCount overridePrewarmCount in this.overridePrewarmCounts)
			{
				InterfaceReference<IPoolableT> poolable = overridePrewarmCount.Poolable;
				if (((poolable != null) ? poolable.Interface : null) == null)
				{
					DebugUtility.LogException(new NullReferenceException("There is a null item in overridePrewarmCounts!"), this);
				}
				else
				{
					IPoolableT @interface = overridePrewarmCount.Poolable.Interface;
					this.prewarmCountOverrides[@interface] = Mathf.Max(0, overridePrewarmCount.PrewarmCount);
				}
			}
		}

		// Token: 0x06000E38 RID: 3640 RVA: 0x0003DBDC File Offset: 0x0003BDDC
		private void PrewarmAllPools()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "PrewarmAllPools", Array.Empty<object>());
			}
			foreach (InterfaceReference<IUIPresentable> interfaceReference in this.presenterDictionary.Presenters)
			{
				IUIPresentable @interface = interfaceReference.Interface;
				if (@interface == null)
				{
					DebugUtility.LogException(new NullReferenceException("There is a null item in presenterDictionary.Presenters!"), this);
				}
				else
				{
					@interface.PrewarmPool(this.GetPrewarmCount(@interface));
				}
			}
			if (this.namedViewPresenterSource == null)
			{
				DebugUtility.LogException(new NullReferenceException("namedViewPresenterSource is null!"), this);
			}
			else
			{
				this.namedViewPresenterSource.PrewarmPool(this.GetPrewarmCount(this.namedViewPresenterSource));
			}
			if (this.iEnumerableItemSource == null)
			{
				DebugUtility.LogException(new NullReferenceException("iEnumerableItemSource is null!"), this);
				return;
			}
			this.iEnumerableItemSource.PrewarmPool(this.GetPrewarmCount(this.iEnumerableItemSource));
		}

		// Token: 0x06000E39 RID: 3641 RVA: 0x0003DCD4 File Offset: 0x0003BED4
		private int GetPrewarmCount(IPoolableT poolable)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "GetPrewarmCount", new object[] { poolable });
			}
			return this.prewarmCountOverrides.GetValueOrDefault(poolable, this.defaultPrewarmCount);
		}

		// Token: 0x04000917 RID: 2327
		[SerializeField]
		[Min(0f)]
		private int defaultPrewarmCount = 4;

		// Token: 0x04000918 RID: 2328
		[SerializeField]
		private UIPoolablePrewarmer.OverridePrewarmCount[] overridePrewarmCounts = Array.Empty<UIPoolablePrewarmer.OverridePrewarmCount>();

		// Token: 0x04000919 RID: 2329
		[SerializeField]
		private UIPresenterDictionary presenterDictionary;

		// Token: 0x0400091A RID: 2330
		[SerializeField]
		private UINamedViewPresenter namedViewPresenterSource;

		// Token: 0x0400091B RID: 2331
		[SerializeField]
		private UIIEnumerableItem iEnumerableItemSource;

		// Token: 0x0400091C RID: 2332
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;

		// Token: 0x0400091D RID: 2333
		private readonly Dictionary<IPoolableT, int> prewarmCountOverrides = new Dictionary<IPoolableT, int>();

		// Token: 0x02000233 RID: 563
		[Serializable]
		private class OverridePrewarmCount
		{
			// Token: 0x170002A2 RID: 674
			// (get) Token: 0x06000E3B RID: 3643 RVA: 0x0003DD2A File Offset: 0x0003BF2A
			// (set) Token: 0x06000E3C RID: 3644 RVA: 0x0003DD32 File Offset: 0x0003BF32
			public InterfaceReference<IPoolableT> Poolable { get; private set; }

			// Token: 0x170002A3 RID: 675
			// (get) Token: 0x06000E3D RID: 3645 RVA: 0x0003DD3B File Offset: 0x0003BF3B
			// (set) Token: 0x06000E3E RID: 3646 RVA: 0x0003DD43 File Offset: 0x0003BF43
			public int PrewarmCount { get; private set; } = 1;
		}
	}
}
