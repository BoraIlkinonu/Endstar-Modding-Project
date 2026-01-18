using System;
using Endless.Shared.Debugging;
using UnityEngine;

namespace Endless.Shared
{
	// Token: 0x0200008D RID: 141
	public class SetActiveIfIsDebugBuild : BaseSetActiveIf
	{
		// Token: 0x06000404 RID: 1028 RVA: 0x00011894 File Offset: 0x0000FA94
		private void Start()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			base.SetActive(Debug.isDebugBuild);
		}
	}
}
