using System;
using System.Collections.Generic;
using Endless.Shared.Debugging;
using Runtime.Shared;
using UnityEngine;

namespace Endless.Shared.UI
{
	// Token: 0x02000210 RID: 528
	public abstract class UIBaseNumericView<TModel, TViewStyle> : UIBaseView<TModel, TViewStyle>, IUIInteractable where TViewStyle : Enum
	{
		// Token: 0x1700027F RID: 639
		// (get) Token: 0x06000DAB RID: 3499 RVA: 0x0003BF6C File Offset: 0x0003A16C
		// (set) Token: 0x06000DAC RID: 3500 RVA: 0x0003BF74 File Offset: 0x0003A174
		public bool Initialized { get; private set; }

		// Token: 0x17000280 RID: 640
		// (get) Token: 0x06000DAD RID: 3501 RVA: 0x0003BF7D File Offset: 0x0003A17D
		public IReadOnlyList<UINumericFieldView> NumericFieldViews
		{
			get
			{
				return this.numericFieldViews;
			}
		}

		// Token: 0x17000281 RID: 641
		// (get) Token: 0x06000DAE RID: 3502
		protected abstract UINumericFieldView.Types UINumericFieldViewType { get; }

		// Token: 0x06000DAF RID: 3503 RVA: 0x0003BF85 File Offset: 0x0003A185
		private void Start()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log("Start", this);
			}
			if (!this.Initialized)
			{
				this.Initialize();
			}
		}

		// Token: 0x06000DB0 RID: 3504 RVA: 0x0003BFA8 File Offset: 0x0003A1A8
		public override void Clear()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log("Clear", this);
			}
			UINumericFieldView[] array = this.numericFieldViews;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].SetValue(0f, false);
			}
		}

		// Token: 0x06000DB1 RID: 3505 RVA: 0x0003BFEB File Offset: 0x0003A1EB
		public override void View(TModel model)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "View", "model", model), this);
			}
			if (!this.Initialized)
			{
				this.Initialize();
			}
		}

		// Token: 0x06000DB2 RID: 3506 RVA: 0x0003C024 File Offset: 0x0003A224
		public void SetInteractable(bool interactable)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "SetInteractable", "interactable", interactable), this);
			}
			UINumericFieldView[] array = this.numericFieldViews;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].SetInteractable(interactable);
			}
		}

		// Token: 0x06000DB3 RID: 3507 RVA: 0x0003C078 File Offset: 0x0003A278
		public void SetRaycastTargetGraphics(bool state)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "SetRaycastTargetGraphics", "state", state), this);
			}
			if (!this.Initialized)
			{
				this.Initialize();
			}
			UINumericFieldView[] array = this.numericFieldViews;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].SetRaycastTargetGraphics(state);
			}
		}

		// Token: 0x06000DB4 RID: 3508 RVA: 0x0003C0DC File Offset: 0x0003A2DC
		public virtual void Initialize()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log("Initialize", this);
			}
			if (this.Initialized)
			{
				return;
			}
			for (int i = 0; i < this.numericFieldViews.Length; i++)
			{
				UINumericFieldView uinumericFieldView = this.numericFieldViews[i];
				base.AddToMaskableGraphics(uinumericFieldView.MaskableGraphics);
				base.SetMaskable(true);
				this.numericFieldViews[i] = uinumericFieldView;
				uinumericFieldView.SetType(this.UINumericFieldViewType);
				uinumericFieldView.OnValueChanged += this.ApplyNumericFieldViewValuesToModel;
				uinumericFieldView.SetValue(0f, false);
			}
			this.Initialized = true;
		}

		// Token: 0x06000DB5 RID: 3509
		protected abstract void ApplyNumericFieldViewValuesToModel(float fieldModel);

		// Token: 0x040008C9 RID: 2249
		[SerializeField]
		private UINumericFieldView[] numericFieldViews = Array.Empty<UINumericFieldView>();
	}
}
