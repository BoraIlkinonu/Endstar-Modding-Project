using System;
using System.Collections.Generic;
using Unity.Cloud.UserReporting.Plugin;
using UnityEngine;
using UnityEngine.XR;

// Token: 0x02000008 RID: 8
public class UserReportingXRExtensions : MonoBehaviour
{
	// Token: 0x0600001D RID: 29 RVA: 0x00002994 File Offset: 0x00000B94
	private static bool XRIsPresent()
	{
		List<XRDisplaySubsystem> list = new List<XRDisplaySubsystem>();
		SubsystemManager.GetInstances<XRDisplaySubsystem>(list);
		using (List<XRDisplaySubsystem>.Enumerator enumerator = list.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				if (enumerator.Current.running)
				{
					return true;
				}
			}
		}
		return false;
	}

	// Token: 0x0600001E RID: 30 RVA: 0x000029F4 File Offset: 0x00000BF4
	private void Start()
	{
		if (UserReportingXRExtensions.XRIsPresent())
		{
			UnityUserReporting.CurrentClient.AddDeviceMetadata("XRDeviceModel", XRSettings.loadedDeviceName);
		}
	}

	// Token: 0x0600001F RID: 31 RVA: 0x00002A14 File Offset: 0x00000C14
	private void Update()
	{
		if (UserReportingXRExtensions.XRIsPresent())
		{
			int num;
			if (XRStats.TryGetDroppedFrameCount(out num))
			{
				UnityUserReporting.CurrentClient.SampleMetric("XR.DroppedFrameCount", (double)num);
			}
			int num2;
			if (XRStats.TryGetFramePresentCount(out num2))
			{
				UnityUserReporting.CurrentClient.SampleMetric("XR.FramePresentCount", (double)num2);
			}
			float num3;
			if (XRStats.TryGetGPUTimeLastFrame(out num3))
			{
				UnityUserReporting.CurrentClient.SampleMetric("XR.GPUTimeLastFrame", (double)num3);
			}
		}
	}
}
