using System;
using System.Collections.Generic;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Gameplay.UI
{
	// Token: 0x020003C8 RID: 968
	public class UIIconDefinitionListModel : UIBaseListModel<IconDefinition>
	{
		// Token: 0x1700050B RID: 1291
		// (get) Token: 0x06001897 RID: 6295 RVA: 0x000722D8 File Offset: 0x000704D8
		// (set) Token: 0x06001898 RID: 6296 RVA: 0x000722E0 File Offset: 0x000704E0
		public bool Initialized { get; private set; }

		// Token: 0x06001899 RID: 6297 RVA: 0x000722E9 File Offset: 0x000704E9
		private void Start()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			if (!this.Initialized)
			{
				this.Initialize();
			}
		}

		// Token: 0x0600189A RID: 6298 RVA: 0x00072314 File Offset: 0x00070514
		public void Initialize()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "Initialize", Array.Empty<object>());
			}
			if (this.Initialized)
			{
				return;
			}
			this.Initialized = true;
			List<IconDefinition> list = new List<IconDefinition>(this.source.Definitions);
			list = list.FindAll((IconDefinition characterCosmeticsDefinition) => !IconList.IconDefinitionIsMissingData(characterCosmeticsDefinition));
			this.Set(list, true);
		}

		// Token: 0x040013BE RID: 5054
		[Header("UIIconDefinitionListModel")]
		[SerializeField]
		private IconList source;
	}
}
