using System;
using Endless.Shared.Debugging;

namespace Endless.Shared.UI
{
	// Token: 0x020001E7 RID: 487
	public abstract class UIBaseEnumView : UIBaseView<Enum, UIBaseEnumView.Styles>
	{
		// Token: 0x14000035 RID: 53
		// (add) Token: 0x06000C10 RID: 3088 RVA: 0x000343D8 File Offset: 0x000325D8
		// (remove) Token: 0x06000C11 RID: 3089 RVA: 0x00034410 File Offset: 0x00032610
		public event Action<Enum> OnEnumChanged;

		// Token: 0x06000C12 RID: 3090 RVA: 0x00034445 File Offset: 0x00032645
		public override void Clear()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log("Clear", this);
			}
		}

		// Token: 0x06000C13 RID: 3091 RVA: 0x0003445A File Offset: 0x0003265A
		protected void InvokeOnEnumChanged(Enum value)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "InvokeOnEnumChanged", "value", value), this);
			}
			Action<Enum> onEnumChanged = this.OnEnumChanged;
			if (onEnumChanged == null)
			{
				return;
			}
			onEnumChanged(value);
		}

		// Token: 0x020001E8 RID: 488
		public enum Styles
		{
			// Token: 0x040007CC RID: 1996
			Dropdown
		}
	}
}
