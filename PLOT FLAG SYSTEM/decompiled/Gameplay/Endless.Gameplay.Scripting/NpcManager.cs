using Endless.Gameplay.LuaEnums;
using Endless.Shared;
using UnityEngine;

namespace Endless.Gameplay.Scripting;

public class NpcManager
{
	private static NpcManager instance;

	internal static NpcManager Instance => instance ?? (instance = new NpcManager());

	public Context SpawnNpc(Context instigator, CellReference cellReference, NpcConfiguration config, Context fallbackContextForPosition)
	{
		UnityEngine.Vector3 position;
		Quaternion rotation;
		if (cellReference.HasValue)
		{
			position = cellReference.GetCellPosition();
			rotation = (cellReference.Rotation.HasValue ? Quaternion.Euler(0f, cellReference.Rotation.Value, 0f) : Quaternion.Euler(0f, fallbackContextForPosition.GetYAxisRotation(), 0f));
		}
		else
		{
			position = fallbackContextForPosition.GetPosition().RoundToVector3Int();
			rotation = Quaternion.Euler(0f, fallbackContextForPosition.GetYAxisRotation(), 0f);
		}
		return MonoBehaviourSingleton<Endless.Gameplay.NpcManager>.Instance.SpawnNpc(position, rotation, config);
	}

	public Context SpawnNpc(Context instigator, UnityEngine.Vector3 position, float yAxisRotation, NpcConfiguration config)
	{
		return MonoBehaviourSingleton<Endless.Gameplay.NpcManager>.Instance.SpawnNpc(position, Quaternion.Euler(0f, yAxisRotation, 0f), config);
	}

	public Context SpawnNpc(Context instigator, UnityEngine.Vector3 position, NpcConfiguration config, Context rotationContext)
	{
		Quaternion rotation = Quaternion.Euler(0f, rotationContext.GetYAxisRotation(), 0f);
		return MonoBehaviourSingleton<Endless.Gameplay.NpcManager>.Instance.SpawnNpc(position, rotation, config);
	}

	public Context GetNpcInGroupByIndex(int group, int index)
	{
		return MonoBehaviourSingleton<Endless.Gameplay.NpcManager>.Instance.GetNpcInGroupByIndex((NpcGroup)group, index);
	}

	public int GetNumNpcsByGroup(int group)
	{
		return MonoBehaviourSingleton<Endless.Gameplay.NpcManager>.Instance.GetNumNpcsInGroup((NpcGroup)group);
	}

	public NpcConfiguration CreateNewConfiguration()
	{
		return new NpcConfiguration();
	}

	public NpcConfiguration CopyNpcConfiguration(Context context)
	{
		if (!context.IsNpc())
		{
			return new NpcConfiguration();
		}
		NpcEntity userComponent = context.WorldObject.GetUserComponent<NpcEntity>();
		if (userComponent.IsConfigured)
		{
			return new NpcConfiguration(userComponent);
		}
		return new NpcConfiguration();
	}
}
