using System;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Gameplay.UI
{
	// Token: 0x020003CC RID: 972
	public class UINpcClassCustomizationDataListCellController : UIBaseListCellController<NpcClassCustomizationData>
	{
		// Token: 0x060018A1 RID: 6305 RVA: 0x000723B7 File Offset: 0x000705B7
		protected override void Start()
		{
			base.Start();
			this.removeButton.onClick.AddListener(new UnityAction(this.Remove));
		}

		// Token: 0x060018A2 RID: 6306 RVA: 0x000723DC File Offset: 0x000705DC
		protected override void OnAddButton()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnAddButton", Array.Empty<object>());
			}
			NpcClassCustomizationData npcClassCustomizationData = new GruntNpcCustomizationData();
			base.ListModel.Add(npcClassCustomizationData, true);
		}

		// Token: 0x040013C2 RID: 5058
		[Header("UINpcClassCustomizationDataListCellController")]
		[SerializeField]
		private UIButton removeButton;
	}
}
