using System;
using System.Collections.Generic;
using System.Linq;
using Endless.Props;
using Endless.Shared;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI
{
	// Token: 0x020002F1 RID: 753
	public class UITransformIdentifierSelectionWindowView : UIBaseWindowView
	{
		// Token: 0x170001B6 RID: 438
		// (get) Token: 0x06000D01 RID: 3329 RVA: 0x0003E656 File Offset: 0x0003C856
		// (set) Token: 0x06000D02 RID: 3330 RVA: 0x0003E65E File Offset: 0x0003C85E
		public Action<TransformIdentifier> OnTransformIdentifierSelect { get; private set; }

		// Token: 0x06000D03 RID: 3331 RVA: 0x0003E668 File Offset: 0x0003C868
		public static UITransformIdentifierSelectionWindowView Display(UITransformIdentifier[] transformIdentifiers, Action<UITransformIdentifier> onTransformIdentifierSelect, Transform parent = null)
		{
			Dictionary<string, object> dictionary = new Dictionary<string, object>
			{
				{ "transformIdentifiers", transformIdentifiers },
				{ "onTransformIdentifierSelect", onTransformIdentifierSelect }
			};
			return (UITransformIdentifierSelectionWindowView)MonoBehaviourSingleton<UIWindowManager>.Instance.Display<UITransformIdentifierSelectionWindowView>(parent, dictionary);
		}

		// Token: 0x06000D04 RID: 3332 RVA: 0x0003E6A4 File Offset: 0x0003C8A4
		public override void Initialize(Dictionary<string, object> supplementalData)
		{
			base.Initialize(supplementalData);
			this.transformIdentifierListModel.Clear(false);
			UITransformIdentifier[] array = (UITransformIdentifier[])supplementalData["transformIdentifiers"];
			this.OnTransformIdentifierSelect = (Action<TransformIdentifier>)supplementalData["transformIdentifiers".CapitalizeFirstCharacter()];
			this.transformIdentifierListModel.Set(array.ToList<UITransformIdentifier>(), true);
		}

		// Token: 0x04000B30 RID: 2864
		[Header("UITransformIdentifierSelectionWindowView")]
		[SerializeField]
		private UITransformIdentifierListModel transformIdentifierListModel;
	}
}
