using System;
using Unity.Cloud.UserReporting;
using Unity.Cloud.UserReporting.Plugin;
using UnityEngine;

// Token: 0x02000004 RID: 4
public class UserReportingMonitor : MonoBehaviour
{
	// Token: 0x06000005 RID: 5 RVA: 0x000020E4 File Offset: 0x000002E4
	public UserReportingMonitor()
	{
		this.IsEnabled = true;
		this.IsHiddenWithoutDimension = true;
		Type type = base.GetType();
		this.MonitorName = type.Name;
	}

	// Token: 0x06000006 RID: 6 RVA: 0x00002118 File Offset: 0x00000318
	private void Start()
	{
		if (UnityUserReporting.CurrentClient == null)
		{
			UnityUserReporting.Configure();
		}
	}

	// Token: 0x06000007 RID: 7 RVA: 0x00002128 File Offset: 0x00000328
	public void Trigger()
	{
		if (!this.IsEnabledAfterTrigger)
		{
			this.IsEnabled = false;
		}
		UnityUserReporting.CurrentClient.TakeScreenshot(2048, 2048, delegate(UserReportScreenshot s)
		{
		});
		UnityUserReporting.CurrentClient.TakeScreenshot(512, 512, delegate(UserReportScreenshot s)
		{
		});
		UnityUserReporting.CurrentClient.CreateUserReport(delegate(UserReport br)
		{
			if (string.IsNullOrEmpty(br.ProjectIdentifier))
			{
				Debug.LogWarning("The user report's project identifier is not set. Please setup cloud services using the Services tab or manually specify a project identifier when calling UnityUserReporting.Configure().");
			}
			br.Summary = this.Summary;
			br.DeviceMetadata.Add(new UserReportNamedValue("Monitor", this.MonitorName));
			string text = "Unknown";
			string text2 = "0.0";
			foreach (UserReportNamedValue userReportNamedValue in br.DeviceMetadata)
			{
				if (userReportNamedValue.Name == "Platform")
				{
					text = userReportNamedValue.Value;
				}
				if (userReportNamedValue.Name == "Version")
				{
					text2 = userReportNamedValue.Value;
				}
			}
			br.Dimensions.Add(new UserReportNamedValue("Monitor.Platform.Version", string.Format("{0}.{1}.{2}", this.MonitorName, text, text2)));
			br.Dimensions.Add(new UserReportNamedValue("Monitor", this.MonitorName));
			br.IsHiddenWithoutDimension = this.IsHiddenWithoutDimension;
			UnityUserReporting.CurrentClient.SendUserReport(br, delegate(bool success, UserReport br2)
			{
				this.Triggered();
			});
		});
	}

	// Token: 0x06000008 RID: 8 RVA: 0x000021C0 File Offset: 0x000003C0
	protected virtual void Triggered()
	{
	}

	// Token: 0x04000004 RID: 4
	public bool IsEnabled;

	// Token: 0x04000005 RID: 5
	public bool IsEnabledAfterTrigger;

	// Token: 0x04000006 RID: 6
	public bool IsHiddenWithoutDimension;

	// Token: 0x04000007 RID: 7
	public string MonitorName;

	// Token: 0x04000008 RID: 8
	public string Summary;
}
