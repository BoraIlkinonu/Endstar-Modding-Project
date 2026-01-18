using System;
using System.Collections.Generic;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Gameplay.UI
{
	// Token: 0x020003B9 RID: 953
	public class UICharacterCosmeticsDefinitionListModel : UIBaseLocalFilterableListModel<CharacterCosmeticsDefinition>
	{
		// Token: 0x17000503 RID: 1283
		// (get) Token: 0x06001858 RID: 6232 RVA: 0x0007111B File Offset: 0x0006F31B
		// (set) Token: 0x06001859 RID: 6233 RVA: 0x00071123 File Offset: 0x0006F323
		public bool Initialized { get; private set; }

		// Token: 0x17000504 RID: 1284
		// (get) Token: 0x0600185A RID: 6234 RVA: 0x0007112C File Offset: 0x0006F32C
		protected override Comparison<CharacterCosmeticsDefinition> DefaultSort
		{
			get
			{
				return (CharacterCosmeticsDefinition x, CharacterCosmeticsDefinition y) => string.Compare(x.DisplayName, y.DisplayName, StringComparison.Ordinal);
			}
		}

		// Token: 0x0600185B RID: 6235 RVA: 0x0007114D File Offset: 0x0006F34D
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

		// Token: 0x0600185C RID: 6236 RVA: 0x00071178 File Offset: 0x0006F378
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
			List<CharacterCosmeticsDefinition> list = new List<CharacterCosmeticsDefinition>(this.characterCosmeticsList.Cosmetics);
			list = list.FindAll((CharacterCosmeticsDefinition characterCosmeticsDefinition) => !CharacterCosmeticsList.CharacterCosmeticsDefinitionIsMissingData(characterCosmeticsDefinition));
			this.Set(list, true);
			this.Initialized = true;
		}

		// Token: 0x04001388 RID: 5000
		[Header("UICharacterCosmeticsDefinitionListModel")]
		[SerializeField]
		private CharacterCosmeticsList characterCosmeticsList;
	}
}
