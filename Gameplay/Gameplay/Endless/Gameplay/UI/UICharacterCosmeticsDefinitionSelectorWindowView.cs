using System;
using System.Collections.Generic;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Gameplay.UI
{
	// Token: 0x02000411 RID: 1041
	public class UICharacterCosmeticsDefinitionSelectorWindowView : UIBaseWindowView
	{
		// Token: 0x1700053E RID: 1342
		// (get) Token: 0x060019EF RID: 6639 RVA: 0x0007716E File Offset: 0x0007536E
		// (set) Token: 0x060019F0 RID: 6640 RVA: 0x00077176 File Offset: 0x00075376
		public Action<SerializableGuid> SelectAction { get; private set; }

		// Token: 0x060019F1 RID: 6641 RVA: 0x00077180 File Offset: 0x00075380
		public static UIBaseWindowView Display(SerializableGuid selectedClientCharacterVisualId, Action<SerializableGuid> selectAction = null, Transform parent = null)
		{
			Dictionary<string, object> dictionary = new Dictionary<string, object>
			{
				{ "selectedClientCharacterVisualId", selectedClientCharacterVisualId },
				{ "SelectAction", selectAction }
			};
			return MonoBehaviourSingleton<UIWindowManager>.Instance.Display<UICharacterCosmeticsDefinitionSelectorWindowView>(parent, dictionary);
		}

		// Token: 0x060019F2 RID: 6642 RVA: 0x000771BC File Offset: 0x000753BC
		public override void Initialize(Dictionary<string, object> supplementalData)
		{
			base.Initialize(supplementalData);
			SerializableGuid serializableGuid = (SerializableGuid)supplementalData["selectedClientCharacterVisualId"];
			this.SelectAction = (Action<SerializableGuid>)supplementalData["SelectAction"];
			this.characterCosmeticsDefinitionSelector.SetSelected(serializableGuid, false);
		}

		// Token: 0x0400149E RID: 5278
		[Header("UICharacterCosmeticsDefinitionSelectorWindowView")]
		[SerializeField]
		private UICharacterCosmeticsDefinitionSelector characterCosmeticsDefinitionSelector;
	}
}
