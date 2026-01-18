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

namespace Endless.Creator.UI;

public class UILevelDestinationSelectionListModel : UIBaseLocalFilterableListModel<LevelDestination>
{
	private readonly Dictionary<SerializableGuid, string> levelNameDictionary = new Dictionary<SerializableGuid, string>();

	private readonly Queue<SerializableGuid> levelQueue = new Queue<SerializableGuid>();

	protected override Comparison<LevelDestination> DefaultSort => (LevelDestination x, LevelDestination y) => string.Compare(x.TargetLevelId, y.TargetLevelId, StringComparison.Ordinal);

	private void OnEnable()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "OnEnable");
		}
		SerializableGuid serializableGuid = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.AssetID;
		List<LevelReference> levelReferences = MonoBehaviourSingleton<GameEditor>.Instance.GetLevelReferences();
		List<LevelDestination> list = new List<LevelDestination>();
		foreach (LevelReference item2 in levelReferences)
		{
			if (!(serializableGuid == item2.AssetID))
			{
				LevelDestination item = new LevelDestination
				{
					TargetLevelId = item2.AssetID
				};
				list.Add(item);
			}
		}
		Set(list, triggerEvents: true);
	}

	public override void Set(List<LevelDestination> list, bool triggerEvents)
	{
		Dictionary<SerializableGuid, string> dictionary = new Dictionary<SerializableGuid, string>(levelNameDictionary);
		levelNameDictionary.Clear();
		levelQueue.Clear();
		foreach (LevelDestination item in list)
		{
			SerializableGuid targetLevelId = item.TargetLevelId;
			if (!targetLevelId.IsEmpty)
			{
				if (dictionary.TryGetValue(targetLevelId, out var value))
				{
					levelNameDictionary.Add(targetLevelId, value);
				}
				else
				{
					levelQueue.Enqueue(targetLevelId);
				}
			}
		}
		GetNextLevelName();
		base.Set(list, triggerEvents);
	}

	public string GetLevelName(SerializableGuid levelId)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "GetLevelName", levelId);
		}
		return levelNameDictionary.GetValueOrDefault(levelId, "Loading...");
	}

	private async void GetNextLevelName()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "GetNextLevelName");
		}
		if (levelQueue.Count == 0)
		{
			return;
		}
		SerializableGuid leveId = levelQueue.Dequeue();
		GraphQlResult graphQlResult = await EndlessServices.Instance.CloudService.GetAssetAsync(leveId, "", new AssetParams("level { Name }"));
		if (graphQlResult.HasErrors)
		{
			ErrorHandler.HandleError(ErrorCodes.GetNextLevelName_GetLevel, graphQlResult.GetErrorMessage());
		}
		else
		{
			var anonymousTypeObject = new
			{
				Name = ""
			};
			var anon = JsonConvert.DeserializeAnonymousType(graphQlResult.GetDataMember().ToString(), anonymousTypeObject);
			if (anon != null)
			{
				levelNameDictionary.Add(leveId, anon.Name);
				TriggerModelChanged();
			}
		}
		if (levelQueue.Count > 0)
		{
			GetNextLevelName();
		}
	}
}
