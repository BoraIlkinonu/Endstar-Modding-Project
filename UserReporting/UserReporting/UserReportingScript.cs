using System;
using System.Collections;
using System.Reflection;
using System.Text;
using Endless.Shared;
using Unity.Cloud.UserReporting;
using Unity.Cloud.UserReporting.Client;
using Unity.Cloud.UserReporting.Plugin;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// Token: 0x02000006 RID: 6
public class UserReportingScript : MonoBehaviour
{
	// Token: 0x0600000B RID: 11 RVA: 0x00002300 File Offset: 0x00000500
	public UserReportingScript()
	{
		this.UserReportSubmitting = new UnityEvent();
		this.unityUserReportingUpdater = new UnityUserReportingUpdater();
	}

	// Token: 0x17000001 RID: 1
	// (get) Token: 0x0600000C RID: 12 RVA: 0x0000231E File Offset: 0x0000051E
	// (set) Token: 0x0600000D RID: 13 RVA: 0x00002326 File Offset: 0x00000526
	public UserReport CurrentUserReport { get; private set; }

	// Token: 0x17000002 RID: 2
	// (get) Token: 0x0600000E RID: 14 RVA: 0x0000232F File Offset: 0x0000052F
	public UserReportingState State
	{
		get
		{
			if (this.CurrentUserReport != null)
			{
				if (this.IsInSilentMode)
				{
					return UserReportingState.Idle;
				}
				if (this.isSubmitting)
				{
					return UserReportingState.SubmittingForm;
				}
				return UserReportingState.ShowingForm;
			}
			else
			{
				if (this.isCreatingUserReport)
				{
					return UserReportingState.CreatingUserReport;
				}
				return UserReportingState.Idle;
			}
		}
	}

	// Token: 0x0600000F RID: 15 RVA: 0x0000235A File Offset: 0x0000055A
	public void CancelUserReport()
	{
		this.CurrentUserReport = null;
		this.ClearForm();
	}

	// Token: 0x06000010 RID: 16 RVA: 0x00002369 File Offset: 0x00000569
	private IEnumerator ClearError()
	{
		yield return new WaitForSecondsRealtime(10f);
		this.isShowingError = false;
		yield break;
	}

	// Token: 0x06000011 RID: 17 RVA: 0x00002378 File Offset: 0x00000578
	private void ClearForm()
	{
		this.SummaryInput.text = null;
		this.DescriptionInput.text = null;
	}

	// Token: 0x06000012 RID: 18 RVA: 0x00002394 File Offset: 0x00000594
	public void CreateUserReport()
	{
		if (this.isCreatingUserReport)
		{
			return;
		}
		this.isCreatingUserReport = true;
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
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = 0; i < 10; i++)
			{
				stringBuilder.AppendLine("Lorem ipsum dolor sit amet, consectetur adipiscing elit. Pellentesque pharetra dui id mauris convallis dignissim. In id tortor ut augue aliquam molestie. Curabitur placerat, enim id suscipit feugiat, orci turpis malesuada diam, quis elementum purus sapien at orci. Vivamus efficitur, eros mattis suscipit mollis, lorem lectus efficitur massa, et egestas lectus tellus eu mauris. Suspendisse venenatis tempus interdum. In sed ultrices magna, a aliquet erat. Donec imperdiet nulla purus, vel rhoncus turpis fermentum et. Sed quis scelerisque velit. Integer ac urna arcu. Integer erat tellus, mollis id malesuada sed, eleifend nec justo. Donec vestibulum, lacus non volutpat elementum, ligula turpis aliquet diam, et dictum lectus metus vel mi. Sed lobortis lectus id rhoncus pharetra. Fusce ac imperdiet dolor, in rutrum lorem. Nam molestie diam tellus, a laoreet velit finibus et. Nam auctor metus purus, in elementum ante finibus at. Donec nunc lectus, dapibus quis augue sit amet, vulputate commodo felis. Morbi ut est sed.");
			}
			br.Attachments.Add(new UserReportAttachment("Sample Attachment.json", "SampleAttachment.json", "application/json", Encoding.UTF8.GetBytes(stringBuilder.ToString())));
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
			br.Dimensions.Add(new UserReportNamedValue("Platform.Version", string.Format("{0}.{1}", text, text2)));
			this.CurrentUserReport = br;
			this.isCreatingUserReport = false;
			this.SetThumbnail(br);
			if (this.IsInSilentMode)
			{
				this.SubmitUserReport();
			}
		});
	}

	// Token: 0x06000013 RID: 19 RVA: 0x0000242D File Offset: 0x0000062D
	private UserReportingClientConfiguration GetConfiguration()
	{
		return new UserReportingClientConfiguration();
	}

	// Token: 0x06000014 RID: 20 RVA: 0x00002434 File Offset: 0x00000634
	public bool IsSubmitting()
	{
		return this.isSubmitting;
	}

	// Token: 0x06000015 RID: 21 RVA: 0x0000243C File Offset: 0x0000063C
	private void SetThumbnail(UserReport userReport)
	{
		if (userReport != null && this.ThumbnailViewer != null)
		{
			byte[] array = Convert.FromBase64String(userReport.Thumbnail.DataBase64);
			Texture2D texture2D = new Texture2D(1, 1);
			texture2D.LoadImage(array);
			this.ThumbnailViewer.sprite = Sprite.Create(texture2D, new Rect(0f, 0f, (float)texture2D.width, (float)texture2D.height), new Vector2(0.5f, 0.5f));
			this.ThumbnailViewer.preserveAspect = true;
		}
	}

	// Token: 0x06000016 RID: 22 RVA: 0x000024C8 File Offset: 0x000006C8
	private void Start()
	{
		if (Application.isPlaying && global::UnityEngine.Object.FindObjectOfType<EventSystem>() == null)
		{
			GameObject gameObject = new GameObject("EventSystem");
			gameObject.AddComponent<EventSystem>();
			gameObject.AddComponent<StandaloneInputModule>();
		}
		bool flag = false;
		if (this.UserReportingPlatform == UserReportingPlatformType.Async)
		{
			Type type = Assembly.GetExecutingAssembly().GetType("Unity.Cloud.UserReporting.Plugin.Version2018_3.AsyncUnityUserReportingPlatform");
			if (type != null)
			{
				IUserReportingPlatform userReportingPlatform = Activator.CreateInstance(type) as IUserReportingPlatform;
				if (userReportingPlatform != null)
				{
					UnityUserReporting.Configure(userReportingPlatform, this.GetConfiguration());
					flag = true;
				}
			}
		}
		if (!flag)
		{
			UnityUserReporting.Configure(this.GetConfiguration());
		}
		string text = string.Format("https://userreporting.cloud.unity3d.com/api/userreporting/projects/{0}/ping", UnityUserReporting.CurrentClient.ProjectIdentifier);
		UnityUserReporting.CurrentClient.Platform.Post(text, "application/json", Encoding.UTF8.GetBytes("\"Ping\""), delegate(float upload, float download)
		{
		}, delegate(bool result, byte[] bytes)
		{
		});
	}

	// Token: 0x06000017 RID: 23 RVA: 0x000025C8 File Offset: 0x000007C8
	public void SubmitUserReport()
	{
		if (this.isSubmitting || this.CurrentUserReport == null)
		{
			return;
		}
		this.isSubmitting = true;
		if (this.SummaryInput != null)
		{
			this.CurrentUserReport.Summary = this.SummaryInput.text;
		}
		if (this.CategoryDropdown != null)
		{
			string text = this.CategoryDropdown.options[this.CategoryDropdown.value].text;
			this.CurrentUserReport.Dimensions.Add(new UserReportNamedValue("Category", text));
			this.CurrentUserReport.Fields.Add(new UserReportNamedValue("Category", text));
		}
		if (this.DescriptionInput != null)
		{
			UserReportNamedValue userReportNamedValue = default(UserReportNamedValue);
			userReportNamedValue.Name = "Description";
			userReportNamedValue.Value = this.DescriptionInput.text;
			this.CurrentUserReport.Fields.Add(userReportNamedValue);
		}
		this.ClearForm();
		this.RaiseUserReportSubmitting();
		UnityUserReporting.CurrentClient.SendUserReport(this.CurrentUserReport, delegate(float uploadProgress, float downloadProgress)
		{
			if (this.ProgressText != null)
			{
				string text2 = string.Format("{0:P}", uploadProgress);
				this.ProgressText.text = text2;
			}
		}, delegate(bool success, UserReport br2)
		{
			if (!success)
			{
				this.isShowingError = true;
				base.StartCoroutine(this.ClearError());
			}
			this.CurrentUserReport = null;
			this.isSubmitting = false;
		});
	}

	// Token: 0x06000018 RID: 24 RVA: 0x000026F0 File Offset: 0x000008F0
	private void Update()
	{
		if (this.IsHotkeyEnabled && EndlessInput.GetKey(Key.LeftShift) && EndlessInput.GetKey(Key.LeftAlt) && EndlessInput.GetKeyDown(Key.B))
		{
			this.CreateUserReport();
		}
		UnityUserReporting.CurrentClient.IsSelfReporting = this.IsSelfReporting;
		UnityUserReporting.CurrentClient.SendEventsToAnalytics = this.SendEventsToAnalytics;
		if (this.UserReportButton != null)
		{
			this.UserReportButton.interactable = this.State == UserReportingState.Idle;
		}
		if (this.UserReportForm != null)
		{
			this.UserReportForm.enabled = this.State == UserReportingState.ShowingForm;
		}
		if (this.SubmittingPopup != null)
		{
			this.SubmittingPopup.enabled = this.State == UserReportingState.SubmittingForm;
		}
		if (this.ErrorPopup != null)
		{
			this.ErrorPopup.enabled = this.isShowingError;
		}
		this.unityUserReportingUpdater.Reset();
		base.StartCoroutine(this.unityUserReportingUpdater);
	}

	// Token: 0x06000019 RID: 25 RVA: 0x000027E3 File Offset: 0x000009E3
	protected virtual void RaiseUserReportSubmitting()
	{
		if (this.UserReportSubmitting != null)
		{
			this.UserReportSubmitting.Invoke();
		}
	}

	// Token: 0x0400000C RID: 12
	[Tooltip("The category dropdown.")]
	public Dropdown CategoryDropdown;

	// Token: 0x0400000D RID: 13
	[Tooltip("The description input on the user report form.")]
	public InputField DescriptionInput;

	// Token: 0x0400000E RID: 14
	[Tooltip("The UI shown when there's an error.")]
	public Canvas ErrorPopup;

	// Token: 0x0400000F RID: 15
	private bool isCreatingUserReport;

	// Token: 0x04000010 RID: 16
	[Tooltip("A value indicating whether the hotkey is enabled (Left Alt + Left Shift + B).")]
	public bool IsHotkeyEnabled;

	// Token: 0x04000011 RID: 17
	[Tooltip("A value indicating whether the prefab is in silent mode. Silent mode does not show the user report form.")]
	public bool IsInSilentMode;

	// Token: 0x04000012 RID: 18
	[Tooltip("A value indicating whether the user report client reports metrics about itself.")]
	public bool IsSelfReporting;

	// Token: 0x04000013 RID: 19
	private bool isShowingError;

	// Token: 0x04000014 RID: 20
	private bool isSubmitting;

	// Token: 0x04000015 RID: 21
	[Tooltip("The display text for the progress text.")]
	public Text ProgressText;

	// Token: 0x04000016 RID: 22
	[Tooltip("A value indicating whether the user report client send events to analytics.")]
	public bool SendEventsToAnalytics;

	// Token: 0x04000017 RID: 23
	[Tooltip("The UI shown while submitting.")]
	public Canvas SubmittingPopup;

	// Token: 0x04000018 RID: 24
	[Tooltip("The summary input on the user report form.")]
	public InputField SummaryInput;

	// Token: 0x04000019 RID: 25
	[Tooltip("The thumbnail viewer on the user report form.")]
	public Image ThumbnailViewer;

	// Token: 0x0400001A RID: 26
	private UnityUserReportingUpdater unityUserReportingUpdater;

	// Token: 0x0400001B RID: 27
	[Tooltip("The user report button used to create a user report.")]
	public Button UserReportButton;

	// Token: 0x0400001C RID: 28
	[Tooltip("The UI for the user report form. Shown after a user report is created.")]
	public Canvas UserReportForm;

	// Token: 0x0400001D RID: 29
	[Tooltip("The User Reporting platform. Different platforms have different features but may require certain Unity versions or target platforms. The Async platform adds async screenshotting and report creation, but requires Unity 2018.3 and above, the package manager version of Unity User Reporting, and a target platform that supports asynchronous GPU readback such as DirectX.")]
	public UserReportingPlatformType UserReportingPlatform;

	// Token: 0x0400001E RID: 30
	[Tooltip("The event raised when a user report is submitting.")]
	public UnityEvent UserReportSubmitting;
}
