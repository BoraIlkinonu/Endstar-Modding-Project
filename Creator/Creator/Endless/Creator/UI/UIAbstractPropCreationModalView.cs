using System;
using Endless.Creator.DynamicPropCreation;
using Endless.Shared.DataTypes;
using UnityEngine;
using UnityEngine.UI;

namespace Endless.Creator.UI
{
	// Token: 0x020001C5 RID: 453
	public class UIAbstractPropCreationModalView : UIBasePropCreationModalView
	{
		// Token: 0x170000BA RID: 186
		// (get) Token: 0x060006B9 RID: 1721 RVA: 0x000227F6 File Offset: 0x000209F6
		// (set) Token: 0x060006BA RID: 1722 RVA: 0x000227FE File Offset: 0x000209FE
		public AbstractPropCreationScreenData AbstractPropCreationScreenData { get; private set; }

		// Token: 0x170000BB RID: 187
		// (get) Token: 0x060006BB RID: 1723 RVA: 0x00022807 File Offset: 0x00020A07
		// (set) Token: 0x060006BC RID: 1724 RVA: 0x0002280F File Offset: 0x00020A0F
		public Texture2D FinalTexture { get; private set; }

		// Token: 0x170000BC RID: 188
		// (get) Token: 0x060006BD RID: 1725 RVA: 0x00022818 File Offset: 0x00020A18
		// (set) Token: 0x060006BE RID: 1726 RVA: 0x00022820 File Offset: 0x00020A20
		public SerializableGuid IconId
		{
			get
			{
				return this.iconId;
			}
			set
			{
				this.iconId = value;
				this.FinalTexture = AbstractPropIconUtility.MergeIcon(this.AbstractPropCreationScreenData.Icon.texture, this.IconId);
				Rect rect = new Rect(0f, 0f, (float)this.FinalTexture.width, (float)this.FinalTexture.height);
				this.iconDisplay.sprite = Sprite.Create(this.FinalTexture, rect, new Vector2(0.5f, 0.5f), 100f);
			}
		}

		// Token: 0x060006BF RID: 1727 RVA: 0x000228A9 File Offset: 0x00020AA9
		public override void OnDisplay(params object[] modalData)
		{
			base.OnDisplay(modalData);
			this.AbstractPropCreationScreenData = (AbstractPropCreationScreenData)modalData[0];
			this.IconId = this.AbstractPropCreationScreenData.DefaultIconGuid;
		}

		// Token: 0x0400060C RID: 1548
		[Header("UIAbstractPropCreationModalView")]
		[SerializeField]
		private Image iconDisplay;

		// Token: 0x0400060D RID: 1549
		private SerializableGuid iconId;
	}
}
