using System;
using System.Linq;
using System.Threading.Tasks;
using Endless.Core.UI;
using Endless.Creator.UI;
using Endless.Data;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Gameplay.UI;
using Endless.GraphQl;
using Endless.Matchmaking;
using Endless.Shared.DataTypes;
using Endless.Shared.UI;
using Newtonsoft.Json;
using Runtime.Shared.Matchmaking;
using UnityEngine;

namespace Runtime.Core.DeepLinking
{
	// Token: 0x02000012 RID: 18
	public class InspectPublishedAssetDeepLinkAction : DeepLinkAction
	{
		// Token: 0x06000047 RID: 71 RVA: 0x00003904 File Offset: 0x00001B04
		public override bool Parse(string argString)
		{
			string[] array = base.SplitArguments(argString);
			foreach (string text in array)
			{
				string[] array3 = base.ParseArgument(text);
				string text2 = array3[0];
				if (!(text2 == "assetId"))
				{
					if (!(text2 == "assetType"))
					{
						Debug.LogWarning("Unexpected arg name. Name: " + text2 + ". Value: " + array3[1]);
					}
					else
					{
						this.assetType = array3[1];
					}
				}
				else
				{
					this.assetId = array3[1];
				}
			}
			if (this.assetId == SerializableGuid.Empty || string.IsNullOrEmpty(this.assetId))
			{
				Debug.LogError("InspectPublishedAssetDeepLinkAction:: Unable to parse asset id from command line argument. Received: " + array[1]);
				return false;
			}
			if (string.IsNullOrEmpty(this.assetType))
			{
				Debug.LogError("InspectPublishedAssetDeepLinkAction:: Unable to parse asset type from command line argument. Received: " + array[1]);
				return false;
			}
			if (!InspectPublishedAssetDeepLinkAction.ValidActionTypes.Contains(this.assetType.ToLower()))
			{
				Debug.LogError("Unexpected AssetType. Value: " + this.assetType);
				return false;
			}
			return true;
		}

		// Token: 0x06000048 RID: 72 RVA: 0x00003A1C File Offset: 0x00001C1C
		public override async Task<bool> Execute()
		{
			GraphQlResult graphQlResult = await EndlessServices.Instance.CloudService.GetPublishedVersionsOfAssetAsync(this.assetId, false);
			if (graphQlResult.HasErrors)
			{
				Debug.LogException(graphQlResult.GetErrorMessage(0));
				throw new DeepLinkActionExecutionException(ErrorCodes.InspectPublishedAssetDeepLinkExecutionError_GetPublishedVersionsFailure);
			}
			var array = new <>f__AnonymousType0<string, string>[]
			{
				new
				{
					asset_version = string.Empty,
					state = string.Empty
				}
			};
			var <>f__AnonymousType = JsonConvert.DeserializeAnonymousType(graphQlResult.GetDataMember().ToString(), array).FirstOrDefault(value => value.state == UIPublishStates.Public.ToEndlessCloudServicesCompatibleString());
			if (<>f__AnonymousType == null)
			{
				throw new DeepLinkActionExecutionException(ErrorCodes.InspectPublishedAssetDeepLinkExecutionError_GetTargetVersionFailure);
			}
			GraphQlResult graphQlResult2 = await EndlessServices.Instance.CloudService.GetAssetAsync(this.assetId, <>f__AnonymousType.asset_version, new AssetParams(null, false, null), false, 10);
			if (graphQlResult2.HasErrors)
			{
				throw new DeepLinkActionExecutionException(ErrorCodes.InspectPublishedAssetDeepLinkExecutionError_GetTargetAssetDataFailure, graphQlResult2.GetErrorMessage(0));
			}
			MainMenuGameModel mainMenuGameModel = JsonConvert.DeserializeObject<MainMenuGameModel>(graphQlResult2.GetDataMember().ToString());
			UIMainMenuScreenView uimainMenuScreenView = UIMainMenuScreenView.Display(UIScreenManager.DisplayStackActions.ClearAndPush);
			uimainMenuScreenView.ViewPlaySection();
			uimainMenuScreenView.SetPlaySearchField(mainMenuGameModel.Name);
			UIGameInspectorScreenModel uigameInspectorScreenModel = new UIGameInspectorScreenModel(mainMenuGameModel, MainMenuGameContext.Play);
			UIGameInspectorScreenView.Display(UIScreenManager.DisplayStackActions.Push, uigameInspectorScreenModel);
			return true;
		}

		// Token: 0x04000030 RID: 48
		public static readonly string ActionName = "InspectPublishedAsset";

		// Token: 0x04000031 RID: 49
		private static readonly string[] ValidActionTypes = new string[] { "game" };

		// Token: 0x04000032 RID: 50
		private SerializableGuid assetId;

		// Token: 0x04000033 RID: 51
		private string assetVersion;

		// Token: 0x04000034 RID: 52
		private string assetType;
	}
}
