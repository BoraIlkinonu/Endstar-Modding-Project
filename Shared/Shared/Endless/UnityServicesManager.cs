using System;
using System.Threading.Tasks;
using Unity.Services.Analytics;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Core.Environments;
using UnityEngine;

namespace Endless
{
	// Token: 0x0200002A RID: 42
	public class UnityServicesManager
	{
		// Token: 0x14000005 RID: 5
		// (add) Token: 0x0600012B RID: 299 RVA: 0x00007F74 File Offset: 0x00006174
		// (remove) Token: 0x0600012C RID: 300 RVA: 0x00007FAC File Offset: 0x000061AC
		public event Action OnServicesInitialized;

		// Token: 0x0600012D RID: 301 RVA: 0x00007FE1 File Offset: 0x000061E1
		public void Initialize(bool useProductionEnvironment)
		{
			this.Authenticate(useProductionEnvironment);
		}

		// Token: 0x0600012E RID: 302 RVA: 0x00007FEC File Offset: 0x000061EC
		private async void Authenticate(bool useProductionEnvironment)
		{
			InitializationOptions options = new InitializationOptions();
			if (useProductionEnvironment)
			{
				options.SetEnvironmentName("production");
			}
			else
			{
				options.SetEnvironmentName("development");
			}
			bool success = false;
			int attempts = 0;
			int maxAttempts = 3;
			while (!success && attempts < maxAttempts)
			{
				int num = attempts;
				attempts = num + 1;
				try
				{
					await UnityServices.InitializeAsync(options);
					if (!AuthenticationService.Instance.IsSignedIn)
					{
						AuthenticationService.Instance.ClearSessionToken();
						await AuthenticationService.Instance.SignInAnonymouslyAsync(null);
					}
				}
				catch (Exception ex)
				{
					Debug.LogError(ex);
				}
				Debug.Log("Access Token: " + (AuthenticationService.Instance.AccessToken ?? "null"));
				success = AuthenticationService.Instance.AccessToken != null;
				if (!success)
				{
					await Task.Delay(UnityServicesManager.GetDelayTime(attempts));
				}
			}
			if (success)
			{
				AnalyticsService.Instance.StartDataCollection();
				Action onServicesInitialized = this.OnServicesInitialized;
				if (onServicesInitialized != null)
				{
					onServicesInitialized();
				}
				return;
			}
			Action onServicesInitializedFailure = UnityServicesManager.OnServicesInitializedFailure;
			if (onServicesInitializedFailure != null)
			{
				onServicesInitializedFailure();
			}
			throw new Exception(string.Format("{0} failed to Initialize after {1} {2}!", "UnityServices", attempts, "attempts"));
		}

		// Token: 0x0600012F RID: 303 RVA: 0x0000802C File Offset: 0x0000622C
		public static int GetDelayTime(int attempt)
		{
			int num = Math.Min(500 * (1 << attempt - 1), 4000);
			int num2 = global::UnityEngine.Random.Range(0, num / 2);
			return num + num2;
		}

		// Token: 0x04000094 RID: 148
		public static Action OnServicesInitializedFailure;
	}
}
