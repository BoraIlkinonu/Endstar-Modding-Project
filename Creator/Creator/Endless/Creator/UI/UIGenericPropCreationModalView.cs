using System;
using Endless.Creator.DynamicPropCreation;
using UnityEngine;
using UnityEngine.UI;

namespace Endless.Creator.UI
{
	// Token: 0x020001CA RID: 458
	public class UIGenericPropCreationModalView : UIBasePropCreationModalView
	{
		// Token: 0x170000C3 RID: 195
		// (get) Token: 0x060006D6 RID: 1750 RVA: 0x00022C36 File Offset: 0x00020E36
		// (set) Token: 0x060006D7 RID: 1751 RVA: 0x00022C3E File Offset: 0x00020E3E
		public GenericPropCreationScreenData GenericPropCreationScreenData { get; private set; }

		// Token: 0x060006D8 RID: 1752 RVA: 0x00022C47 File Offset: 0x00020E47
		public override void OnDisplay(params object[] modalData)
		{
			base.OnDisplay(modalData);
			this.GenericPropCreationScreenData = (GenericPropCreationScreenData)modalData[0];
			this.iconDisplay.texture = this.GenericPropCreationScreenData.PropIcon;
		}

		// Token: 0x04000623 RID: 1571
		[Header("UIGenericPropCreationModalView")]
		[SerializeField]
		private RawImage iconDisplay;
	}
}
