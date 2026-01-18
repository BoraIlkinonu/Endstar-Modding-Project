using System;
using Endless.Shared.Debugging;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Shared.UI
{
	// Token: 0x02000277 RID: 631
	public class UISliderMinMaxHandler : UIGameObject
	{
		// Token: 0x17000300 RID: 768
		// (get) Token: 0x06000FDC RID: 4060 RVA: 0x00043DFA File Offset: 0x00041FFA
		// (set) Token: 0x06000FDD RID: 4061 RVA: 0x00043E02 File Offset: 0x00042002
		protected bool VerboseLogging { get; set; }

		// Token: 0x06000FDE RID: 4062 RVA: 0x00043E0C File Offset: 0x0004200C
		private void Start()
		{
			if (this.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			this.minimum.onValueChanged.AddListener(new UnityAction<float>(this.OnMinimumChanged));
			this.maximum.onValueChanged.AddListener(new UnityAction<float>(this.OnMaximumChanged));
		}

		// Token: 0x06000FDF RID: 4063 RVA: 0x00043E6C File Offset: 0x0004206C
		private void OnMinimumChanged(float newValue)
		{
			if (this.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnMinimumChanged", new object[] { newValue });
			}
			if (newValue > this.maximum.value)
			{
				this.maximum.SetValue(newValue, !this.sendChangedEventsOnClamp);
			}
		}

		// Token: 0x06000FE0 RID: 4064 RVA: 0x00043EC0 File Offset: 0x000420C0
		private void OnMaximumChanged(float newValue)
		{
			if (this.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnMaximumChanged", new object[] { newValue });
			}
			if (newValue < this.minimum.value)
			{
				this.minimum.SetValue(newValue, !this.sendChangedEventsOnClamp);
			}
		}

		// Token: 0x04000A1C RID: 2588
		[SerializeField]
		protected bool sendChangedEventsOnClamp;

		// Token: 0x04000A1D RID: 2589
		[SerializeField]
		private UISlider minimum;

		// Token: 0x04000A1E RID: 2590
		[SerializeField]
		private UISlider maximum;
	}
}
