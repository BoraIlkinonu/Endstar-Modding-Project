using System;
using Endless.Shared.DataTypes;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Endless.Gameplay.UI
{
	// Token: 0x0200038E RID: 910
	public class UICharacterCosmeticsDefinitionPortraitView : UIGameObject
	{
		// Token: 0x170004CF RID: 1231
		// (get) Token: 0x0600172B RID: 5931 RVA: 0x0006C62B File Offset: 0x0006A82B
		// (set) Token: 0x0600172C RID: 5932 RVA: 0x0006C633 File Offset: 0x0006A833
		public CharacterCosmeticsDefinition CharacterCosmeticsDefinition { get; private set; }

		// Token: 0x0600172D RID: 5933 RVA: 0x0006C63C File Offset: 0x0006A83C
		public void Display(SerializableGuid characterCosmeticsDefinitionAssetId)
		{
			if (this.verboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "Display", "characterCosmeticsDefinitionAssetId", characterCosmeticsDefinitionAssetId), this);
			}
			if (characterCosmeticsDefinitionAssetId == SerializableGuid.Empty)
			{
				this.Display(this.fallbackCharacterCosmeticsDefinition);
				return;
			}
			CharacterCosmeticsDefinition characterCosmeticsDefinition;
			if (this.characterCosmeticsList.TryGetDefinition(characterCosmeticsDefinitionAssetId, out characterCosmeticsDefinition))
			{
				this.CharacterCosmeticsDefinition = characterCosmeticsDefinition;
				this.Display(this.CharacterCosmeticsDefinition);
				return;
			}
			DebugUtility.LogWarning(string.Format("could not find {0} of {1} in {2}! Using {3}!", new object[] { "characterCosmeticsDefinitionAssetId", characterCosmeticsDefinitionAssetId, "characterCosmeticsList", "fallbackCharacterCosmeticsDefinition" }), this);
			this.Display(this.fallbackCharacterCosmeticsDefinition);
		}

		// Token: 0x0600172E RID: 5934 RVA: 0x0006C6F4 File Offset: 0x0006A8F4
		private void Display(CharacterCosmeticsDefinition characterCosmeticsDefinition)
		{
			if (this.verboseLogging)
			{
				DebugUtility.Log("Display ( DisplayName: " + characterCosmeticsDefinition.DisplayName + " )", this);
			}
			this.CharacterCosmeticsDefinition = characterCosmeticsDefinition;
			if (this.portraitImage)
			{
				this.portraitImage.sprite = characterCosmeticsDefinition.PortraitSprite;
			}
			if (this.displayNameText)
			{
				this.displayNameText.text = characterCosmeticsDefinition.DisplayName;
			}
		}

		// Token: 0x04001299 RID: 4761
		[SerializeField]
		private Image portraitImage;

		// Token: 0x0400129A RID: 4762
		[SerializeField]
		private TextMeshProUGUI displayNameText;

		// Token: 0x0400129B RID: 4763
		[SerializeField]
		private CharacterCosmeticsList characterCosmeticsList;

		// Token: 0x0400129C RID: 4764
		[SerializeField]
		private CharacterCosmeticsDefinition fallbackCharacterCosmeticsDefinition;

		// Token: 0x0400129D RID: 4765
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;
	}
}
