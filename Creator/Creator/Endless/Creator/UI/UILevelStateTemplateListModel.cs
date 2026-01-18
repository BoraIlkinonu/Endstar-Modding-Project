using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using Endless.Shared.Validation;
using UnityEngine;

namespace Endless.Creator.UI
{
	// Token: 0x02000140 RID: 320
	public class UILevelStateTemplateListModel : UIBaseLocalFilterableListModel<LevelStateTemplateSourceBase>, IValidatable
	{
		// Token: 0x1700007E RID: 126
		// (get) Token: 0x060004F7 RID: 1271 RVA: 0x0001BE06 File Offset: 0x0001A006
		protected override Comparison<LevelStateTemplateSourceBase> DefaultSort
		{
			get
			{
				return (LevelStateTemplateSourceBase x, LevelStateTemplateSourceBase y) => 0;
			}
		}

		// Token: 0x060004F8 RID: 1272 RVA: 0x0001BE27 File Offset: 0x0001A027
		private void OnEnable()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnEnable", Array.Empty<object>());
			}
			this.SetAllEntriesToLevelStateTemplateContainer();
		}

		// Token: 0x060004F9 RID: 1273 RVA: 0x0001BE48 File Offset: 0x0001A048
		[ContextMenu("Validate")]
		public void Validate()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "Validate", Array.Empty<object>());
			}
			if (this.defaultSelected == null)
			{
				DebugUtility.LogError("defaultSelected can not be null!", this);
				return;
			}
			if (this.levelStateTemplateArray == null)
			{
				DebugUtility.LogError("levelStateTemplateArray can not be null!", this);
				return;
			}
			bool flag = false;
			using (IEnumerator enumerator = this.levelStateTemplateArray.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (!((LevelStateTemplateSourceBase)enumerator.Current != this.defaultSelected))
					{
						flag = true;
						break;
					}
				}
			}
			if (!flag)
			{
				DebugUtility.LogError("The defaultSelected must be in levelStateTemplateArray!", this);
			}
		}

		// Token: 0x060004FA RID: 1274 RVA: 0x0001BF08 File Offset: 0x0001A108
		public override void Clear(bool triggerEvents)
		{
			base.Clear(triggerEvents);
			this.SetAllEntriesToLevelStateTemplateContainer();
		}

		// Token: 0x060004FB RID: 1275 RVA: 0x0001BF18 File Offset: 0x0001A118
		private void SetAllEntriesToLevelStateTemplateContainer()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "SetAllEntriesToLevelStateTemplateContainer", Array.Empty<object>());
			}
			List<LevelStateTemplateSourceBase> list = this.levelStateTemplateArray.Value.ToList<LevelStateTemplateSourceBase>();
			this.Set(list, false);
			int num = this.levelStateTemplateArray.Value.IndexOf(this.defaultSelected);
			this.Select(num, true);
		}

		// Token: 0x04000497 RID: 1175
		[Header("UILevelStateTemplateListModel")]
		[SerializeField]
		private LevelStateTemplateArray levelStateTemplateArray;

		// Token: 0x04000498 RID: 1176
		[SerializeField]
		private LevelStateTemplateSourceBase defaultSelected;
	}
}
