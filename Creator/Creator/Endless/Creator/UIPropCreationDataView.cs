using System;
using Endless.Creator.DynamicPropCreation;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Endless.Creator
{
	// Token: 0x02000094 RID: 148
	public class UIPropCreationDataView : UIGameObject
	{
		// Token: 0x06000246 RID: 582 RVA: 0x00010998 File Offset: 0x0000EB98
		public void View(PropCreationData model)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "View", new object[] { model });
			}
			this.displayNameText.text = model.DisplayName;
			GenericPropCreationScreenData genericPropCreationScreenData = model as GenericPropCreationScreenData;
			if (genericPropCreationScreenData != null)
			{
				this.iconRawImage.texture = genericPropCreationScreenData.PropIcon;
				this.iconRawImage.enabled = true;
				this.iconImage.enabled = false;
			}
			else
			{
				this.iconImage.sprite = model.Icon;
				this.iconImage.enabled = true;
				this.iconRawImage.enabled = false;
			}
			this.subMenuIconImage.enabled = model.IsSubMenu;
		}

		// Token: 0x0400029D RID: 669
		[Header("UIPropCreationDataView")]
		[SerializeField]
		private TextMeshProUGUI displayNameText;

		// Token: 0x0400029E RID: 670
		[SerializeField]
		private Image iconImage;

		// Token: 0x0400029F RID: 671
		[SerializeField]
		private RawImage iconRawImage;

		// Token: 0x040002A0 RID: 672
		[SerializeField]
		private Image subMenuIconImage;

		// Token: 0x040002A1 RID: 673
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;
	}
}
