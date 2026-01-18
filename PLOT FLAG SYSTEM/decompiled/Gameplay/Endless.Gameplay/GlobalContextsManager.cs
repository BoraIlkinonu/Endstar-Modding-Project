using System.Collections.Generic;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Gameplay.Scripting;
using Endless.Shared;
using Endless.Shared.DataTypes;
using UnityEngine;

namespace Endless.Gameplay;

public class GlobalContextsManager : EndlessBehaviourSingleton<GlobalContextsManager>, IBaseType, IComponentBase
{
	[SerializeField]
	private WorldObject gameWorldObject;

	private Dictionary<SerializableGuid, string> levelContexts = new Dictionary<SerializableGuid, string>();

	public Context Context => Context.StaticGameContext;

	public WorldObject WorldObject => gameWorldObject;

	protected override void Awake()
	{
		base.Awake();
		gameWorldObject.Initialize(this, null);
	}

	public void SaveState()
	{
		string value = Context.StaticLevelContext.ToJson();
		SerializableGuid activeLevelGuid = MonoBehaviourSingleton<StageManager>.Instance.ActiveLevelGuid;
		if (!levelContexts.TryAdd(activeLevelGuid, value))
		{
			levelContexts[activeLevelGuid] = value;
		}
	}

	public void LoadState()
	{
		if (Context.StaticGameContext == null)
		{
			Context.StaticGameContext = new Context(gameWorldObject);
		}
		Context.StaticLevelContext = new Context(gameWorldObject);
		if (levelContexts.TryGetValue(MonoBehaviourSingleton<StageManager>.Instance.ActiveLevelGuid, out var value))
		{
			Context.StaticLevelContext.LoadFromJson(value);
		}
	}

	public void ClearContexts()
	{
		Context.StaticGameContext = new Context(gameWorldObject);
		levelContexts = new Dictionary<SerializableGuid, string>();
	}

	public void PrefabInitialize(WorldObject worldObject)
	{
	}
}
