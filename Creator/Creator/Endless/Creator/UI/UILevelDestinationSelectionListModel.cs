using System;
using System.Collections.Generic;
using Endless.Data;
using Endless.Gameplay;
using Endless.Gameplay.LevelEditing.Level;
using Endless.GraphQl;
using Endless.Matchmaking;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using Newtonsoft.Json;
using Runtime.Shared.Matchmaking;

namespace Endless.Creator.UI
{
	// Token: 0x02000137 RID: 311
	public class UILevelDestinationSelectionListModel : UIBaseLocalFilterableListModel<LevelDestination>
	{
		// Token: 0x1700007C RID: 124
		// (get) Token: 0x060004E0 RID: 1248 RVA: 0x0001B7DA File Offset: 0x000199DA
		protected override Comparison<LevelDestination> DefaultSort
		{
			get
			{
				return (LevelDestination x, LevelDestination y) => string.Compare(x.TargetLevelId, y.TargetLevelId, StringComparison.Ordinal);
			}
		}

		// Token: 0x060004E1 RID: 1249 RVA: 0x0001B7FC File Offset: 0x000199FC
		private void OnEnable()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnEnable", Array.Empty<object>());
			}
			SerializableGuid serializableGuid = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.AssetID;
			List<LevelReference> levelReferences = MonoBehaviourSingleton<GameEditor>.Instance.GetLevelReferences();
			List<LevelDestination> list = new List<LevelDestination>();
			foreach (LevelReference levelReference in levelReferences)
			{
				if (!(serializableGuid == levelReference.AssetID))
				{
					LevelDestination levelDestination = new LevelDestination
					{
						TargetLevelId = levelReference.AssetID
					};
					list.Add(levelDestination);
				}
			}
			this.Set(list, true);
		}

		// Token: 0x060004E2 RID: 1250 RVA: 0x0001B8BC File Offset: 0x00019ABC
		public override void Set(List<LevelDestination> list, bool triggerEvents)
		{
			Dictionary<SerializableGuid, string> dictionary = new Dictionary<SerializableGuid, string>(this.levelNameDictionary);
			this.levelNameDictionary.Clear();
			this.levelQueue.Clear();
			foreach (LevelDestination levelDestination in list)
			{
				SerializableGuid targetLevelId = levelDestination.TargetLevelId;
				if (!targetLevelId.IsEmpty)
				{
					string text;
					if (dictionary.TryGetValue(targetLevelId, out text))
					{
						this.levelNameDictionary.Add(targetLevelId, text);
					}
					else
					{
						this.levelQueue.Enqueue(targetLevelId);
					}
				}
			}
			this.GetNextLevelName();
			base.Set(list, triggerEvents);
		}

		// Token: 0x060004E3 RID: 1251 RVA: 0x0001B968 File Offset: 0x00019B68
		public string GetLevelName(SerializableGuid levelId)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "GetLevelName", new object[] { levelId });
			}
			return this.levelNameDictionary.GetValueOrDefault(levelId, "Loading...");
		}

		// Token: 0x060004E4 RID: 1252 RVA: 0x0001B9A0 File Offset: 0x00019BA0
		private async void GetNextLevelName()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "GetNextLevelName", Array.Empty<object>());
			}
			if (this.levelQueue.Count != 0)
			{
				SerializableGuid leveId = this.levelQueue.Dequeue();
				GraphQlResult graphQlResult = await EndlessServices.Instance.CloudService.GetAssetAsync(leveId, "", new AssetParams("level { Name }", false, null), false, 10);
				if (graphQlResult.HasErrors)
				{
					ErrorHandler.HandleError(ErrorCodes.GetNextLevelName_GetLevel, graphQlResult.GetErrorMessage(0), true, false);
				}
				else
				{
					var <>f__AnonymousType = new
					{
						Name = ""
					};
					var <>f__AnonymousType2 = JsonConvert.DeserializeAnonymousType(graphQlResult.GetDataMember().ToString(), <>f__AnonymousType);
					if (<>f__AnonymousType2 != null)
					{
						this.levelNameDictionary.Add(leveId, <>f__AnonymousType2.Name);
						base.TriggerModelChanged();
					}
				}
				if (this.levelQueue.Count > 0)
				{
					this.GetNextLevelName();
				}
			}
		}

		// Token: 0x04000480 RID: 1152
		private readonly Dictionary<SerializableGuid, string> levelNameDictionary = new Dictionary<SerializableGuid, string>();

		// Token: 0x04000481 RID: 1153
		private readonly Queue<SerializableGuid> levelQueue = new Queue<SerializableGuid>();
	}
}
