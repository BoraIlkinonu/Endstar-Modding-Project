using System;
using Endless.Gameplay.Stats;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Creator.UI
{
	// Token: 0x02000251 RID: 593
	public class UIPerPlayerStatView : UIStatBaseView<PerPlayerStat>
	{
		// Token: 0x14000021 RID: 33
		// (add) Token: 0x06000999 RID: 2457 RVA: 0x0002CA30 File Offset: 0x0002AC30
		// (remove) Token: 0x0600099A RID: 2458 RVA: 0x0002CA68 File Offset: 0x0002AC68
		public event Action<string> DefaultValueChanged;

		// Token: 0x14000022 RID: 34
		// (add) Token: 0x0600099B RID: 2459 RVA: 0x0002CAA0 File Offset: 0x0002ACA0
		// (remove) Token: 0x0600099C RID: 2460 RVA: 0x0002CAD8 File Offset: 0x0002ACD8
		public event Action<NumericDisplayFormat> DisplayFormatChanged;

		// Token: 0x0600099D RID: 2461 RVA: 0x0002CB0D File Offset: 0x0002AD0D
		protected override void Start()
		{
			base.Start();
			this.defaultValueInputField.onValueChanged.AddListener(new UnityAction<string>(this.InvokeDefaultValueChanged));
			this.displayFormatControl.OnModelChanged += this.InvokeDisplayFormatChanged;
		}

		// Token: 0x0600099E RID: 2462 RVA: 0x0002CB48 File Offset: 0x0002AD48
		protected override void OnDestroy()
		{
			base.OnDestroy();
			this.defaultValueInputField.onValueChanged.RemoveListener(new UnityAction<string>(this.InvokeDefaultValueChanged));
			this.displayFormatControl.OnModelChanged -= this.InvokeDisplayFormatChanged;
		}

		// Token: 0x0600099F RID: 2463 RVA: 0x0002CB83 File Offset: 0x0002AD83
		public override void View(PerPlayerStat model)
		{
			base.View(model);
			this.defaultValueInputField.SetTextWithoutNotify(model.DefaultValue);
		}

		// Token: 0x060009A0 RID: 2464 RVA: 0x0002CB9D File Offset: 0x0002AD9D
		public override void Clear()
		{
			base.Clear();
			this.defaultValueInputField.Clear(false);
		}

		// Token: 0x060009A1 RID: 2465 RVA: 0x0002CBB1 File Offset: 0x0002ADB1
		private void InvokeDefaultValueChanged(string newValue)
		{
			Action<string> defaultValueChanged = this.DefaultValueChanged;
			if (defaultValueChanged == null)
			{
				return;
			}
			defaultValueChanged(newValue);
		}

		// Token: 0x060009A2 RID: 2466 RVA: 0x0002CBC4 File Offset: 0x0002ADC4
		private void InvokeDisplayFormatChanged(object displayFormat)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "InvokeDisplayFormatChanged", "displayFormat", displayFormat), this);
			}
			NumericDisplayFormat numericDisplayFormat = (NumericDisplayFormat)displayFormat;
			Action<NumericDisplayFormat> displayFormatChanged = this.DisplayFormatChanged;
			if (displayFormatChanged == null)
			{
				return;
			}
			displayFormatChanged(numericDisplayFormat);
		}

		// Token: 0x040007E9 RID: 2025
		[SerializeField]
		private UIInputField defaultValueInputField;

		// Token: 0x040007EA RID: 2026
		[SerializeField]
		private UIEnumPresenter displayFormatControl;
	}
}
