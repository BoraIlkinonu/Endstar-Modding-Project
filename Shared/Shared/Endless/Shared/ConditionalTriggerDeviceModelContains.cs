using System;
using System.Linq;
using Endless.Shared.Debugging;
using UnityEngine;

namespace Endless.Shared
{
	// Token: 0x0200004B RID: 75
	public class ConditionalTriggerDeviceModelContains : BaseConditionalTrigger
	{
		// Token: 0x0600028F RID: 655 RVA: 0x0000D304 File Offset: 0x0000B504
		private void Start()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			string deviceModel = SystemInfo.deviceModel;
			if (base.VerboseLogging)
			{
				Debug.Log("deviceModel: " + deviceModel, this);
			}
			bool flag = this.triggerIfContainsAnyOfThese.Any((string word) => deviceModel.Contains(word));
			if (base.VerboseLogging)
			{
				Debug.Log(string.Format("{0}: {1}", "trigger", flag), this);
			}
			if (flag)
			{
				this.Trigger.Invoke();
			}
		}

		// Token: 0x0400014D RID: 333
		[Header("ConditionalTriggerDeviceModelContains")]
		[SerializeField]
		private string[] triggerIfContainsAnyOfThese = new string[0];
	}
}
