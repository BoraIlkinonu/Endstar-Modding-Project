using System;
using System.Collections.Generic;
using Endless.Shared.Debugging;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Shared.UI
{
	// Token: 0x020001B1 RID: 433
	public class UILogsListModelHandler : UIMonoBehaviourSingleton<UILogsListModelHandler>
	{
		// Token: 0x17000218 RID: 536
		// (get) Token: 0x06000B15 RID: 2837 RVA: 0x000307C7 File Offset: 0x0002E9C7
		public UnityEvent<UILog> NewLogUnityEvent { get; } = new UnityEvent<UILog>();

		// Token: 0x17000219 RID: 537
		// (get) Token: 0x06000B16 RID: 2838 RVA: 0x000307CF File Offset: 0x0002E9CF
		public IReadOnlyList<UILog> List
		{
			get
			{
				return this.list;
			}
		}

		// Token: 0x06000B17 RID: 2839 RVA: 0x000307D7 File Offset: 0x0002E9D7
		private void Start()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
		}

		// Token: 0x06000B18 RID: 2840 RVA: 0x000307F4 File Offset: 0x0002E9F4
		private void OnLogMessageReceived(string condition, string stackTrace, LogType type)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnLogMessageReceived", new object[] { condition, stackTrace, type });
			}
			UILog uilog = new UILog(condition, stackTrace, type, this.list.Count);
			this.list.Add(uilog);
			this.NewLogUnityEvent.Invoke(uilog);
		}

		// Token: 0x04000720 RID: 1824
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;

		// Token: 0x04000721 RID: 1825
		private readonly List<UILog> list = new List<UILog>();
	}
}
