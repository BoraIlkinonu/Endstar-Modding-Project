using System;
using System.Collections.Generic;
using Endless.Shared;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Gameplay.UI
{
	// Token: 0x0200041C RID: 1052
	public class UIIconDefinitionSelectorWindowView : UIBaseWindowView
	{
		// Token: 0x17000545 RID: 1349
		// (get) Token: 0x06001A22 RID: 6690 RVA: 0x0007866E File Offset: 0x0007686E
		// (set) Token: 0x06001A23 RID: 6691 RVA: 0x00078676 File Offset: 0x00076876
		public Action<IconDefinition> OnSelect { get; private set; }

		// Token: 0x06001A24 RID: 6692 RVA: 0x00078680 File Offset: 0x00076880
		public static UIIconDefinitionSelectorWindowView Display(IconDefinition currentSelection, Action<IconDefinition> onSelect, Transform parent = null)
		{
			Dictionary<string, object> dictionary = new Dictionary<string, object>
			{
				{ "currentSelection", currentSelection },
				{ "onSelect", onSelect }
			};
			return (UIIconDefinitionSelectorWindowView)MonoBehaviourSingleton<UIWindowManager>.Instance.Display<UIIconDefinitionSelectorWindowView>(parent, dictionary);
		}

		// Token: 0x06001A25 RID: 6693 RVA: 0x000786BC File Offset: 0x000768BC
		public override void Initialize(Dictionary<string, object> supplementalData)
		{
			base.Initialize(supplementalData);
			IconDefinition iconDefinition = (IconDefinition)supplementalData["currentSelection"];
			Action<IconDefinition> action = (Action<IconDefinition>)supplementalData["onSelect"];
			this.selector.Initialize(iconDefinition, false);
			this.OnSelect = action;
		}

		// Token: 0x040014E4 RID: 5348
		[Header("UIIconDefinitionSelectorWindowView")]
		[SerializeField]
		private UIIconDefinitionSelector selector;
	}
}
