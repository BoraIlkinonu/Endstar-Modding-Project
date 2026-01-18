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

namespace Endless.Creator.UI;

public class UISaveScriptAndPropAndApplyToGameHandler : UIGameObject, IUILoadingSpinnerViewCompatible
{
	public readonly struct Result
	{
		public readonly Script Script;

		public readonly Prop Prop;

		public Result(Script script, Prop prop)
		{
			Script = script;
			Prop = prop;
		}

		public override string ToString()
		{
			return string.Format("{0} Success: {1}, {2} Success: {3}", "Script", Script != null, "Prop", Prop != null);
		}
	}

	[Header("Debugging")]
	[SerializeField]
	private bool verboseLogging;

	public UnityEvent OnLoadingStarted { get; } = new UnityEvent();

	public UnityEvent OnLoadingEnded { get; } = new UnityEvent();

	public async Task<Result> SaveScriptAndPropAndApplyToGame(Script script, Prop prop)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "SaveScriptAndPropAndApplyToGame", script.AssetID, prop.AssetID);
		}
		OnLoadingStarted.Invoke();
		Script resultScript = await SaveScript(script, tryAgainOnDuplicateAssetVersionException: true);
		if (resultScript == null)
		{
			return new Result(null, null);
		}
		Prop resultProp = await SaveProp(prop, resultScript.AssetVersion, tryAgainOnDuplicateAssetVersionException: true);
		if (resultProp == null)
		{
			return new Result(resultScript, null);
		}
		try
		{
			await MonoBehaviourSingleton<GameEditor>.Instance.SetPropVersionInGameLibrary(resultProp.AssetID, resultProp.AssetVersion);
		}
		catch (Exception exception)
		{
			ErrorHandler.HandleError(ErrorCodes.UISaveScriptAndPropAndApplyToGameHandler_SaveScriptAndPropAndApplyToGame, exception);
		}
		if (verboseLogging)
		{
			DebugUtility.Log("Success!", this);
		}
		return new Result(resultScript, resultProp);
	}

	private async Task<Script> SaveScript(Script script, bool tryAgainOnDuplicateAssetVersionException)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "SaveScript", script.AssetID, tryAgainOnDuplicateAssetVersionException);
		}
		OnLoadingStarted.Invoke();
		Script script2 = script.Clone();
		if (verboseLogging)
		{
			DebugUtility.Log("Script's CURRENT version: " + script2.AssetVersion, this);
		}
		SemanticVersion semanticVersion = SemanticVersion.Parse(script2.AssetVersion).IncrementPatch();
		script2.AssetVersion = semanticVersion.ToString();
		if (verboseLogging)
		{
			DebugUtility.Log("Script's NEW version: " + script2.AssetVersion, this);
		}
		GraphQlResult graphQlResult = await EndlessServices.Instance.CloudService.UpdateAssetAsync(script2);
		if (graphQlResult.HasErrors)
		{
			Exception errorMessage = graphQlResult.GetErrorMessage();
			if (errorMessage is DuplicateAssetVersionException && tryAgainOnDuplicateAssetVersionException)
			{
				if (verboseLogging)
				{
					DebugUtility.Log("DuplicateAssetVersionException!", this);
				}
				script.AssetVersion = await GetLatestVersion(script.AssetID);
				return await SaveScript(script, tryAgainOnDuplicateAssetVersionException: false);
			}
			OnLoadingEnded.Invoke();
			ErrorHandler.HandleError(ErrorCodes.UISaveScriptAndPropAndApplytoGameHander_SaveScript, errorMessage);
			return null;
		}
		if (verboseLogging)
		{
			DebugUtility.Log("Saving Script success!", this);
		}
		Script script3 = JsonConvert.DeserializeObject<Script>(graphQlResult.GetDataMember().ToString());
		EndlessAssetCache.AddNewVersionToCache(script3);
		if (verboseLogging)
		{
			DebugUtility.Log("Script's RESULT version: " + script3.AssetVersion, this);
		}
		return script3;
	}

	private async Task<Prop> SaveProp(Prop prop, string scriptAssetVersion, bool tryAgainOnDuplicateAssetVersionException)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "SaveProp", prop.AssetID, scriptAssetVersion, tryAgainOnDuplicateAssetVersionException);
		}
		Prop prop2 = prop.Clone();
		if (verboseLogging)
		{
			DebugUtility.Log("Prop's CURRENT version: " + prop2.AssetVersion, this);
		}
		SemanticVersion semanticVersion = SemanticVersion.Parse(prop2.AssetVersion).IncrementPatch();
		prop2.AssetVersion = semanticVersion.ToString();
		prop2.ScriptAsset.AssetVersion = scriptAssetVersion;
		if (verboseLogging)
		{
			DebugUtility.Log("Prop's NEW version: " + prop2.AssetVersion, this);
		}
		GraphQlResult graphQlResult = await EndlessServices.Instance.CloudService.UpdateAssetAsync(prop2);
		if (graphQlResult.HasErrors)
		{
			Exception errorMessage = graphQlResult.GetErrorMessage();
			if (errorMessage is DuplicateAssetVersionException && tryAgainOnDuplicateAssetVersionException)
			{
				if (verboseLogging)
				{
					DebugUtility.Log("DuplicateAssetVersionException!", this);
				}
				prop.AssetVersion = await GetLatestVersion(prop.AssetID);
				return await SaveProp(prop, scriptAssetVersion, tryAgainOnDuplicateAssetVersionException: false);
			}
			OnLoadingEnded.Invoke();
			ErrorHandler.HandleError(ErrorCodes.UISaveScriptAndPropAndApplytoGameHander_SavePropAfterSavingScript, errorMessage);
			return null;
		}
		if (verboseLogging)
		{
			DebugUtility.Log("Saving Prop success!", this);
		}
		Prop prop3 = JsonConvert.DeserializeObject<Prop>(graphQlResult.GetDataMember().ToString());
		EndlessAssetCache.AddNewVersionToCache(prop3);
		if (verboseLogging)
		{
			DebugUtility.Log("Prop's RESULT version: " + prop3.AssetVersion, this);
		}
		return prop3;
	}

	private async Task<string> GetLatestVersion(SerializableGuid assetId)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "GetLatestVersion", assetId);
		}
		GraphQlResult graphQlResult = await EndlessServices.Instance.CloudService.GetVersionsAsync(assetId);
		if (graphQlResult.HasErrors)
		{
			OnLoadingEnded.Invoke();
			ErrorHandler.HandleError(ErrorCodes.UISaveScriptAndPropAndApplyToGameHandler_GetVersions, graphQlResult.GetErrorMessage());
			return null;
		}
		string[] parsedAndOrderedVersions = VersionUtilities.GetParsedAndOrderedVersions(graphQlResult.GetDataMember());
		if (parsedAndOrderedVersions.Length == 0)
		{
			return "0.0.0";
		}
		return parsedAndOrderedVersions[0];
	}
}
