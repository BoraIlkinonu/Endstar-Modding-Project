using System;
using Unity.Cloud.UserReporting.Plugin;
using UnityEngine;

// Token: 0x02000003 RID: 3
public class UserReportingConfigureOnly : MonoBehaviour
{
	// Token: 0x06000003 RID: 3 RVA: 0x000020CC File Offset: 0x000002CC
	private void Start()
	{
		if (UnityUserReporting.CurrentClient == null)
		{
			UnityUserReporting.Configure();
		}
	}
}
