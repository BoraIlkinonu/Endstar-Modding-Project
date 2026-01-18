using System;
using System.Collections.Generic;
using Endless.Shared.Debugging;
using Endless.Shared.Validation;
using UnityEngine;
using UnityEngine.UI;

namespace Endless.Shared.UI
{
	// Token: 0x02000240 RID: 576
	public abstract class UIBaseView<TModel, TViewStyle> : UIGameObject, IUIViewable<TModel>, IClearable, IValidatable, IUITypedViewable<TModel>, IUIViewable, IUIViewStylable<TViewStyle> where TViewStyle : Enum
	{
		// Token: 0x170002BD RID: 701
		// (get) Token: 0x06000EA0 RID: 3744 RVA: 0x0003F51E File Offset: 0x0003D71E
		// (set) Token: 0x06000EA1 RID: 3745 RVA: 0x0003F526 File Offset: 0x0003D726
		public UILayoutElement LayoutElement { get; private set; }

		// Token: 0x170002BE RID: 702
		// (get) Token: 0x06000EA2 RID: 3746 RVA: 0x0003F52F File Offset: 0x0003D72F
		// (set) Token: 0x06000EA3 RID: 3747 RVA: 0x0003F537 File Offset: 0x0003D737
		protected bool VerboseLogging { get; set; }

		// Token: 0x170002BF RID: 703
		// (get) Token: 0x06000EA4 RID: 3748
		// (set) Token: 0x06000EA5 RID: 3749
		public abstract TViewStyle Style { get; protected set; }

		// Token: 0x170002C0 RID: 704
		// (get) Token: 0x06000EA6 RID: 3750 RVA: 0x0003EEEE File Offset: 0x0003D0EE
		public Type ModelType
		{
			get
			{
				return typeof(TModel);
			}
		}

		// Token: 0x170002C1 RID: 705
		// (get) Token: 0x06000EA7 RID: 3751 RVA: 0x0003F540 File Offset: 0x0003D740
		public Enum StyleEnum
		{
			get
			{
				return this.Style;
			}
		}

		// Token: 0x06000EA8 RID: 3752
		public abstract void View(TModel model);

		// Token: 0x06000EA9 RID: 3753
		public abstract void Clear();

		// Token: 0x06000EAA RID: 3754 RVA: 0x0003F550 File Offset: 0x0003D750
		public virtual float GetPreferredHeight(object model)
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "GetPreferredHeight", "model", model), this);
			}
			float preferredHeight = this.LayoutElement.preferredHeight;
			if (this.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0}: {1}", "preferredHeight", preferredHeight), this);
			}
			return preferredHeight;
		}

		// Token: 0x06000EAB RID: 3755 RVA: 0x0003F5B0 File Offset: 0x0003D7B0
		public virtual float GetPreferredWidth(object model)
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "GetPreferredWidth", "model", model), this);
			}
			return this.LayoutElement.preferredWidth;
		}

		// Token: 0x06000EAC RID: 3756 RVA: 0x0003F5E0 File Offset: 0x0003D7E0
		public void AddToMaskableGraphics(ICollection<MaskableGraphic> collection)
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "AddToMaskableGraphics", "collection", collection.Count), this);
			}
			this.maskableGraphics.AddRange(collection);
		}

		// Token: 0x06000EAD RID: 3757 RVA: 0x0003F61C File Offset: 0x0003D81C
		public void SetMaskable(bool maskable)
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "SetMaskable", "maskable", maskable), this);
			}
			foreach (MaskableGraphic maskableGraphic in this.maskableGraphics)
			{
				if (!maskableGraphic)
				{
					DebugUtility.LogError("An item is null in maskableGraphics!", this);
				}
				else
				{
					maskableGraphic.maskable = maskable;
					if (maskableGraphic.maskable)
					{
						maskableGraphic.RecalculateClipping();
					}
				}
			}
		}

		// Token: 0x06000EAE RID: 3758 RVA: 0x0003F6BC File Offset: 0x0003D8BC
		public virtual void Validate()
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log("Validate", this);
			}
			DebugUtility.DebugHasNullItem<MaskableGraphic>(this.maskableGraphics, "maskableGraphics", this);
			DebugUtility.DebugIsNull("LayoutElement", this.LayoutElement, this);
		}

		// Token: 0x06000EAF RID: 3759 RVA: 0x0003F6F5 File Offset: 0x0003D8F5
		public void SetDefaultLayoutElementValueToCurrentValue()
		{
			if (this.defaultLayoutElementValueSet)
			{
				return;
			}
			if (this.VerboseLogging)
			{
				DebugUtility.Log("SetDefaultLayoutElementValueToCurrentValue", this);
			}
			this.defaultLayoutElementValue.CopyFrom(this.LayoutElement);
			this.defaultLayoutElementValueSet = true;
		}

		// Token: 0x06000EB0 RID: 3760 RVA: 0x0003F72B File Offset: 0x0003D92B
		public void ApplyDefaultLayoutElementValue()
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log("ApplyDefaultLayoutElementValue", this);
			}
			this.defaultLayoutElementValue.ApplyTo(this.LayoutElement);
		}

		// Token: 0x0400093A RID: 2362
		[Header("UIBaseView")]
		[SerializeField]
		private List<MaskableGraphic> maskableGraphics = new List<MaskableGraphic>();

		// Token: 0x0400093C RID: 2364
		private readonly UILayoutElementValue defaultLayoutElementValue = new UILayoutElementValue();

		// Token: 0x0400093D RID: 2365
		private bool defaultLayoutElementValueSet;
	}
}
