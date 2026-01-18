using System;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Gameplay.UI
{
	// Token: 0x020003B7 RID: 951
	public class UICharacterCosmeticsDefinitionListCellView : UIBaseListCellView<CharacterCosmeticsDefinition>
	{
		// Token: 0x06001854 RID: 6228 RVA: 0x00071075 File Offset: 0x0006F275
		public override void View(UIBaseListView<CharacterCosmeticsDefinition> listView, int dataIndex)
		{
			base.View(listView, dataIndex);
			this.characterCosmeticsDefinitionPortrait.Display(base.Model.AssetId);
		}

		// Token: 0x04001387 RID: 4999
		[Header("UICharacterCosmeticsDefinitionListCellView")]
		[SerializeField]
		private UICharacterCosmeticsDefinitionPortraitView characterCosmeticsDefinitionPortrait;
	}
}
