using System;
using System.Threading.Tasks;
using Endless.Data;
using Endless.Gameplay;
using Endless.GraphQl;
using Endless.Props.Assets;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using Newtonsoft.Json;
using Runtime.Shared.Matchmaking;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Creator.UI
{
	// Token: 0x020002C0 RID: 704
	public class UISaveScriptAndPropAndApplyToGameHandler : UIGameObject, IUILoadingSpinnerViewCompatible
	{
		// Token: 0x17000186 RID: 390
		// (get) Token: 0x06000BE1 RID: 3041 RVA: 0x000387E2 File Offset: 0x000369E2
		public UnityEvent OnLoadingStarted { get; } = new UnityEvent();

		// Token: 0x17000187 RID: 391
		// (get) Token: 0x06000BE2 RID: 3042 RVA: 0x000387EA File Offset: 0x000369EA
		public UnityEvent OnLoadingEnded { get; } = new UnityEvent();

		// Token: 0x06000BE3 RID: 3043 RVA: 0x000387F4 File Offset: 0x000369F4
		public async Task<UISaveScriptAndPropAndApplyToGameHandler.Result> SaveScriptAndPropAndApplyToGame(Script script, Prop prop)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "SaveScriptAndPropAndApplyToGame", new object[] { script.AssetID, prop.AssetID });
			}
			this.OnLoadingStarted.Invoke();
			Script script2 = await this.SaveScript(script, true);
			Script resultScript = script2;
			UISaveScriptAndPropAndApplyToGameHandler.Result result;
			if (resultScript == null)
			{
				result = new UISaveScriptAndPropAndApplyToGameHandler.Result(null, null);
			}
			else
			{
				Prop resultProp = await this.SaveProp(prop, resultScript.AssetVersion, true);
				if (resultProp == null)
				{
					result = new UISaveScriptAndPropAndApplyToGameHandler.Result(resultScript, null);
				}
				else
				{
					try
					{
						await MonoBehaviourSingleton<GameEditor>.Instance.SetPropVersionInGameLibrary(resultProp.AssetID, resultProp.AssetVersion);
					}
					catch (Exception ex)
					{
						ErrorHandler.HandleError(ErrorCodes.UISaveScriptAndPropAndApplyToGameHandler_SaveScriptAndPropAndApplyToGame, ex, true, false);
					}
					if (this.verboseLogging)
					{
						DebugUtility.Log("Success!", this);
					}
					result = new UISaveScriptAndPropAndApplyToGameHandler.Result(resultScript, resultProp);
				}
			}
			return result;
		}

		// Token: 0x06000BE4 RID: 3044 RVA: 0x00038848 File Offset: 0x00036A48
		private async Task<Script> SaveScript(Script script, bool tryAgainOnDuplicateAssetVersionException)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "SaveScript", new object[] { script.AssetID, tryAgainOnDuplicateAssetVersionException });
			}
			this.OnLoadingStarted.Invoke();
			Script script2 = script.Clone();
			if (this.verboseLogging)
			{
				DebugUtility.Log("Script's CURRENT version: " + script2.AssetVersion, this);
			}
			SemanticVersion semanticVersion = SemanticVersion.Parse(script2.AssetVersion).IncrementPatch();
			script2.AssetVersion = semanticVersion.ToString();
			if (this.verboseLogging)
			{
				DebugUtility.Log("Script's NEW version: " + script2.AssetVersion, this);
			}
			GraphQlResult graphQlResult = await EndlessServices.Instance.CloudService.UpdateAssetAsync(script2, false, false);
			Script script3;
			if (graphQlResult.HasErrors)
			{
				Exception errorMessage = graphQlResult.GetErrorMessage(0);
				if (errorMessage is DuplicateAssetVersionException && tryAgainOnDuplicateAssetVersionException)
				{
					if (this.verboseLogging)
					{
						DebugUtility.Log("DuplicateAssetVersionException!", this);
					}
					script.AssetVersion = await this.GetLatestVersion(script.AssetID);
					script3 = await this.SaveScript(script, false);
				}
				else
				{
					this.OnLoadingEnded.Invoke();
					ErrorHandler.HandleError(ErrorCodes.UISaveScriptAndPropAndApplytoGameHander_SaveScript, errorMessage, true, false);
					script3 = null;
				}
			}
			else
			{
				if (this.verboseLogging)
				{
					DebugUtility.Log("Saving Script success!", this);
				}
				Script script4 = JsonConvert.DeserializeObject<Script>(graphQlResult.GetDataMember().ToString());
				EndlessAssetCache.AddNewVersionToCache<Script>(script4);
				if (this.verboseLogging)
				{
					DebugUtility.Log("Script's RESULT version: " + script4.AssetVersion, this);
				}
				script3 = script4;
			}
			return script3;
		}

		// Token: 0x06000BE5 RID: 3045 RVA: 0x0003889C File Offset: 0x00036A9C
		private async Task<Prop> SaveProp(Prop prop, string scriptAssetVersion, bool tryAgainOnDuplicateAssetVersionException)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "SaveProp", new object[] { prop.AssetID, scriptAssetVersion, tryAgainOnDuplicateAssetVersionException });
			}
			Prop prop2 = prop.Clone();
			if (this.verboseLogging)
			{
				DebugUtility.Log("Prop's CURRENT version: " + prop2.AssetVersion, this);
			}
			SemanticVersion semanticVersion = SemanticVersion.Parse(prop2.AssetVersion).IncrementPatch();
			prop2.AssetVersion = semanticVersion.ToString();
			prop2.ScriptAsset.AssetVersion = scriptAssetVersion;
			if (this.verboseLogging)
			{
				DebugUtility.Log("Prop's NEW version: " + prop2.AssetVersion, this);
			}
			GraphQlResult graphQlResult = await EndlessServices.Instance.CloudService.UpdateAssetAsync(prop2, false, false);
			Prop prop3;
			if (graphQlResult.HasErrors)
			{
				Exception errorMessage = graphQlResult.GetErrorMessage(0);
				if (errorMessage is DuplicateAssetVersionException && tryAgainOnDuplicateAssetVersionException)
				{
					if (this.verboseLogging)
					{
						DebugUtility.Log("DuplicateAssetVersionException!", this);
					}
					prop.AssetVersion = await this.GetLatestVersion(prop.AssetID);
					prop3 = await this.SaveProp(prop, scriptAssetVersion, false);
				}
				else
				{
					this.OnLoadingEnded.Invoke();
					ErrorHandler.HandleError(ErrorCodes.UISaveScriptAndPropAndApplytoGameHander_SavePropAfterSavingScript, errorMessage, true, false);
					prop3 = null;
				}
			}
			else
			{
				if (this.verboseLogging)
				{
					DebugUtility.Log("Saving Prop success!", this);
				}
				Prop prop4 = JsonConvert.DeserializeObject<Prop>(graphQlResult.GetDataMember().ToString());
				EndlessAssetCache.AddNewVersionToCache<Prop>(prop4);
				if (this.verboseLogging)
				{
					DebugUtility.Log("Prop's RESULT version: " + prop4.AssetVersion, this);
				}
				prop3 = prop4;
			}
			return prop3;
		}

		// Token: 0x06000BE6 RID: 3046 RVA: 0x000388F8 File Offset: 0x00036AF8
		private async Task<string> GetLatestVersion(SerializableGuid assetId)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "GetLatestVersion", new object[] { assetId });
			}
			GraphQlResult graphQlResult = await EndlessServices.Instance.CloudService.GetVersionsAsync(assetId, false);
			string text;
			if (graphQlResult.HasErrors)
			{
				this.OnLoadingEnded.Invoke();
				ErrorHandler.HandleError(ErrorCodes.UISaveScriptAndPropAndApplyToGameHandler_GetVersions, graphQlResult.GetErrorMessage(0), true, false);
				text = null;
			}
			else
			{
				string[] parsedAndOrderedVersions = VersionUtilities.GetParsedAndOrderedVersions(graphQlResult.GetDataMember());
				if (parsedAndOrderedVersions.Length == 0)
				{
					text = "0.0.0";
				}
				else
				{
					text = parsedAndOrderedVersions[0];
				}
			}
			return text;
		}

		// Token: 0x04000A30 RID: 2608
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;

		// Token: 0x020002C1 RID: 705
		public readonly struct Result
		{
			// Token: 0x06000BE8 RID: 3048 RVA: 0x00038961 File Offset: 0x00036B61
			public Result(Script script, Prop prop)
			{
				this.Script = script;
				this.Prop = prop;
			}

			// Token: 0x06000BE9 RID: 3049 RVA: 0x00038974 File Offset: 0x00036B74
			public override string ToString()
			{
				return string.Format("{0} Success: {1}, {2} Success: {3}", new object[]
				{
					"Script",
					this.Script != null,
					"Prop",
					this.Prop != null
				});
			}

			// Token: 0x04000A33 RID: 2611
			public readonly Script Script;

			// Token: 0x04000A34 RID: 2612
			public readonly Prop Prop;
		}
	}
}
