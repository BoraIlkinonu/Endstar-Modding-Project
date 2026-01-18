using System;
using System.Collections.Generic;
using Endless.Gameplay;
using Endless.Shared.DataTypes;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI
{
	// Token: 0x02000217 RID: 535
	public class UICharacterVisualsReferencePresenter : UIBaseAssetLibraryReferenceClassPresenter<CharacterVisualsReference>
	{
		// Token: 0x17000105 RID: 261
		// (get) Token: 0x06000889 RID: 2185 RVA: 0x00029A1C File Offset: 0x00027C1C
		protected override IEnumerable<object> SelectionOptions
		{
			get
			{
				return this.characterCosmeticsList.Cosmetics;
			}
		}

		// Token: 0x17000106 RID: 262
		// (get) Token: 0x0600088A RID: 2186 RVA: 0x00029A29 File Offset: 0x00027C29
		protected override string IEnumerableWindowTitle
		{
			get
			{
				return "Select a Character Visual Reference";
			}
		}

		// Token: 0x17000107 RID: 263
		// (get) Token: 0x0600088B RID: 2187 RVA: 0x00029A30 File Offset: 0x00027C30
		protected override SelectionType SelectionType
		{
			get
			{
				return SelectionType.MustSelect1;
			}
		}

		// Token: 0x0600088C RID: 2188 RVA: 0x00029A34 File Offset: 0x00027C34
		protected override void SetSelection(List<object> selection)
		{
			List<object> list = new List<object>();
			foreach (object obj in selection)
			{
				CharacterVisualsReference characterVisualsReference = ReferenceFactory.CreateCharacterVisualsReference((obj as CharacterCosmeticsDefinition).AssetId);
				list.Add(characterVisualsReference);
			}
			base.SetSelection(list);
		}

		// Token: 0x0600088D RID: 2189 RVA: 0x00029AA0 File Offset: 0x00027CA0
		protected override CharacterVisualsReference CreateDefaultModel()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "CreateDefaultModel", Array.Empty<object>());
			}
			return ReferenceFactory.CreateCharacterVisualsReference(this.defaultCharacterCosmeticsDefinition.AssetId);
		}

		// Token: 0x0600088E RID: 2190 RVA: 0x00029ACC File Offset: 0x00027CCC
		protected override void UpdateOriginalSelection(CharacterVisualsReference model)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "UpdateOriginalSelection", new object[] { model });
			}
			if (model.IsReferenceEmpty())
			{
				this.originalSelection.Add(this.defaultCharacterCosmeticsDefinition);
				return;
			}
			SerializableGuid id = InspectorReferenceUtility.GetId(model);
			CharacterCosmeticsDefinition characterCosmeticsDefinition2;
			CharacterCosmeticsDefinition characterCosmeticsDefinition = (this.characterCosmeticsList.TryGetDefinition(id, out characterCosmeticsDefinition2) ? characterCosmeticsDefinition2 : this.defaultCharacterCosmeticsDefinition);
			this.originalSelection.Add(characterCosmeticsDefinition);
		}

		// Token: 0x04000766 RID: 1894
		[Header("UICharacterVisualsReferencePresenter")]
		[SerializeField]
		private CharacterCosmeticsList characterCosmeticsList;

		// Token: 0x04000767 RID: 1895
		[SerializeField]
		private CharacterCosmeticsDefinition defaultCharacterCosmeticsDefinition;
	}
}
