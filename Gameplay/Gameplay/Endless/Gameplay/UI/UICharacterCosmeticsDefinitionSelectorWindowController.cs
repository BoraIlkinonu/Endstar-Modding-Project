using System;
using Endless.Shared.DataTypes;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Gameplay.UI
{
	// Token: 0x02000412 RID: 1042
	public class UICharacterCosmeticsDefinitionSelectorWindowController : UIDraggableWindowController
	{
		// Token: 0x060019F4 RID: 6644 RVA: 0x00077204 File Offset: 0x00075404
		protected override void Start()
		{
			base.Start();
			this.characterCosmeticsDefinitionSelector.OnSelectedId.AddListener(new UnityAction<SerializableGuid>(this.SetCharacterCosmeticsDefinition));
		}

		// Token: 0x060019F5 RID: 6645 RVA: 0x00077228 File Offset: 0x00075428
		private void SetCharacterCosmeticsDefinition(SerializableGuid characterCosmeticsDefinitionAssetId)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "SetCharacterCosmeticsDefinition", new object[] { characterCosmeticsDefinitionAssetId });
			}
			Action<SerializableGuid> selectAction = this.view.SelectAction;
			if (selectAction == null)
			{
				return;
			}
			selectAction(characterCosmeticsDefinitionAssetId);
		}

		// Token: 0x040014A0 RID: 5280
		[Header("UICharacterCosmeticsDefinitionSelectorWindowController")]
		[SerializeField]
		private UICharacterCosmeticsDefinitionSelectorWindowView view;

		// Token: 0x040014A1 RID: 5281
		[SerializeField]
		private UICharacterCosmeticsDefinitionSelector characterCosmeticsDefinitionSelector;
	}
}
