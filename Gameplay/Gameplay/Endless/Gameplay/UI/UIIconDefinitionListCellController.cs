using System;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Gameplay.UI
{
	// Token: 0x020003C6 RID: 966
	public class UIIconDefinitionListCellController : UIBaseListCellController<IconDefinition>
	{
		// Token: 0x06001891 RID: 6289 RVA: 0x00072245 File Offset: 0x00070445
		protected override void Start()
		{
			base.Start();
			this.iconDefinitionController.SelectUnityEvent.AddListener(new UnityAction<IconDefinition>(this.SelectIconDefinition));
		}

		// Token: 0x06001892 RID: 6290 RVA: 0x00072269 File Offset: 0x00070469
		protected override void OnAddButton()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnAddButton", Array.Empty<object>());
			}
			throw new NotImplementedException();
		}

		// Token: 0x06001893 RID: 6291 RVA: 0x00072288 File Offset: 0x00070488
		private void SelectIconDefinition(IconDefinition iconDefinition)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "SelectIconDefinition", new object[] { iconDefinition });
			}
			this.ToggleSelected();
		}

		// Token: 0x040013BC RID: 5052
		[Header("UIIconDefinitionListCellController")]
		[SerializeField]
		private UIIconDefinitionController iconDefinitionController;
	}
}
