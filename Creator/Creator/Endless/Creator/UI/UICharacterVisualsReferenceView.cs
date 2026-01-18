using System;
using System.Linq;
using Endless.Gameplay;
using Endless.Gameplay.UI;
using Endless.Shared.DataTypes;
using Endless.Shared.Debugging;
using UnityEngine;

namespace Endless.Creator.UI
{
	// Token: 0x02000218 RID: 536
	public class UICharacterVisualsReferenceView : UIBaseAssetLibraryReferenceClassView<CharacterVisualsReference, UICharacterVisualsReferenceView.Styles>
	{
		// Token: 0x17000108 RID: 264
		// (get) Token: 0x06000890 RID: 2192 RVA: 0x00029B45 File Offset: 0x00027D45
		// (set) Token: 0x06000891 RID: 2193 RVA: 0x00029B4D File Offset: 0x00027D4D
		public override UICharacterVisualsReferenceView.Styles Style { get; protected set; }

		// Token: 0x06000892 RID: 2194 RVA: 0x00029B58 File Offset: 0x00027D58
		public override void View(CharacterVisualsReference model)
		{
			base.View(model);
			SerializableGuid id = InspectorReferenceUtility.GetId(model);
			this.characterCosmeticsDefinitionPortrait.Display(id);
		}

		// Token: 0x06000893 RID: 2195 RVA: 0x00029B80 File Offset: 0x00027D80
		protected override string GetReferenceName(CharacterVisualsReference model)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "GetReferenceName", new object[] { model });
			}
			if (!model.IsReferenceEmpty())
			{
				return "None";
			}
			SerializableGuid assetId = InspectorReferenceUtility.GetId(model);
			CharacterCosmeticsDefinition characterCosmeticsDefinition = this.characterCosmeticsList.Cosmetics.FirstOrDefault((CharacterCosmeticsDefinition item) => item.AssetId == assetId);
			if (characterCosmeticsDefinition)
			{
				return characterCosmeticsDefinition.DisplayName;
			}
			return "Missing";
		}

		// Token: 0x04000769 RID: 1897
		[SerializeField]
		private CharacterCosmeticsList characterCosmeticsList;

		// Token: 0x0400076A RID: 1898
		[SerializeField]
		private UICharacterCosmeticsDefinitionPortraitView characterCosmeticsDefinitionPortrait;

		// Token: 0x02000219 RID: 537
		public enum Styles
		{
			// Token: 0x0400076C RID: 1900
			DefaultReadWrite,
			// Token: 0x0400076D RID: 1901
			DefaultReadOnly
		}
	}
}
