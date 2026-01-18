using System;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Gameplay.UI
{
	// Token: 0x020003B6 RID: 950
	public class UICharacterCosmeticsDefinitionListCellController : UIBaseListCellController<CharacterCosmeticsDefinition>
	{
		// Token: 0x06001850 RID: 6224 RVA: 0x00070FF4 File Offset: 0x0006F1F4
		protected override void Start()
		{
			base.Start();
			this.characterCosmeticsDefinitionPortraitController.SelectUnityEvent.AddListener(new UnityAction<CharacterCosmeticsDefinition>(this.OnSelected));
		}

		// Token: 0x06001851 RID: 6225 RVA: 0x00071018 File Offset: 0x0006F218
		protected override void OnAddButton()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnAddButton", Array.Empty<object>());
			}
			throw new NotImplementedException();
		}

		// Token: 0x06001852 RID: 6226 RVA: 0x00071037 File Offset: 0x0006F237
		private void OnSelected(CharacterCosmeticsDefinition characterCosmeticsDefinition)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnSelected", new object[] { characterCosmeticsDefinition.DisplayName });
			}
			base.ListModel.Select(base.DataIndex, true);
		}

		// Token: 0x04001386 RID: 4998
		[Header("UICharacterCosmeticsDefinitionListCellController")]
		[SerializeField]
		private UICharacterCosmeticsDefinitionPortraitController characterCosmeticsDefinitionPortraitController;
	}
}
