using System;
using System.Collections.Generic;
using System.Globalization;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Shared;
using NLua;
using Newtonsoft.Json;
using UnityEngine;

namespace Endless.Gameplay.Scripting;

public class Context
{
	private Dictionary<string, string> members = new Dictionary<string, string>();

	public LuaInterfaceEvent OnBoolSet = new LuaInterfaceEvent();

	public LuaInterfaceEvent OnIntSet = new LuaInterfaceEvent();

	public LuaInterfaceEvent OnFloatSet = new LuaInterfaceEvent();

	public LuaInterfaceEvent OnStringSet = new LuaInterfaceEvent();

	public LuaInterfaceEvent OnTableSet = new LuaInterfaceEvent();

	internal WorldObject WorldObject { get; }

	internal string InternalId { get; set; }

	public string UniqueId
	{
		get
		{
			if (!string.IsNullOrEmpty(InternalId))
			{
				return InternalId;
			}
			return MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetInstanceIdFromGameObject(WorldObject.gameObject);
		}
	}

	public Context LevelContext => StaticLevelContext;

	public Context GameContext => StaticGameContext;

	public Context LastContext => StaticLastContext;

	internal static Context StaticLevelContext { get; set; }

	internal static Context StaticGameContext { get; set; }

	internal static Context StaticLastContext { get; set; }

	internal Context(WorldObject worldObject)
	{
		WorldObject = worldObject;
	}

	public bool IsNpc()
	{
		return WorldObject.BaseType is NpcEntity;
	}

	public bool IsPlayer()
	{
		PlayerLuaComponent component;
		if ((bool)WorldObject)
		{
			return WorldObject.TryGetUserComponent<PlayerLuaComponent>(out component);
		}
		return false;
	}

	public UnityEngine.Vector3 GetPosition()
	{
		return (WorldObject.BaseType as MonoBehaviour).gameObject.transform.position;
	}

	public UnityEngine.Vector3 GetRotationVector()
	{
		return (WorldObject.BaseType as MonoBehaviour).gameObject.transform.rotation.eulerAngles;
	}

	public float GetYAxisRotation()
	{
		return (WorldObject.BaseType as MonoBehaviour).gameObject.transform.rotation.eulerAngles.y;
	}

	public CellReference GetCellReference()
	{
		MonoBehaviour monoBehaviour = WorldObject.BaseType as MonoBehaviour;
		Vector3Int vector3Int = Stage.WorldSpacePointToGridCoordinate(monoBehaviour.gameObject.transform.position);
		return new CellReference
		{
			Cell = vector3Int,
			Rotation = monoBehaviour.gameObject.transform.rotation.eulerAngles.y
		};
	}

	public void SetTable(string name, LuaTable value)
	{
		string value2 = LuaTableJsonConverter.Serialize(value);
		SetValue(name, value2);
		OnTableSet?.InvokeEvent(this, name, value);
	}

	public LuaTable GetTable(string name)
	{
		if (HasMember(name))
		{
			string text = members[name];
			if (!string.IsNullOrEmpty(text))
			{
				try
				{
					return LuaTableJsonConverter.Deserialize(WorldObject.EndlessProp.ScriptComponent.Lua, text);
				}
				catch
				{
					return null;
				}
			}
		}
		return null;
	}

	public void SetFloat(string name, float value)
	{
		SetValue(name, value.ToString("R", CultureInfo.InvariantCulture));
		OnFloatSet?.InvokeEvent(this, name, value);
	}

	public float GetFloat(string name)
	{
		if (HasMember(name) && float.TryParse(members[name], NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out var result))
		{
			return result;
		}
		return 0f;
	}

	public void SetInt(string name, int value)
	{
		SetValue(name, value);
		OnIntSet?.InvokeEvent(this, name, value);
	}

	public int GetInt(string name)
	{
		if (HasMember(name) && int.TryParse(members[name], out var result))
		{
			return result;
		}
		return 0;
	}

	public void SetBool(string name, bool value)
	{
		SetValue(name, value);
		OnBoolSet?.InvokeEvent(this, name, value);
	}

	public bool GetBool(string name)
	{
		if (HasMember(name) && bool.TryParse(members[name], out var result))
		{
			return result;
		}
		return false;
	}

	public void SetString(string name, string value)
	{
		SetValue(name, value);
		OnStringSet?.InvokeEvent(this, name, value);
	}

	public string GetString(string name)
	{
		if (HasMember(name))
		{
			return members[name];
		}
		return string.Empty;
	}

	public bool HasMember(string name)
	{
		return members.ContainsKey(name);
	}

	private void SetValue(string name, object value)
	{
		string value2 = value.ToString();
		if (!members.TryAdd(name, value2))
		{
			members[name] = value2;
		}
	}

	internal void LoadFromJson(string loadedState)
	{
		if (loadedState != null)
		{
			members = JsonConvert.DeserializeObject<Dictionary<string, string>>(loadedState);
		}
	}

	internal string ToJson()
	{
		return JsonConvert.SerializeObject(members);
	}

	public object TryGetComponent(string componentName)
	{
		if (WorldObject == null)
		{
			Debug.LogError("I DONT HAVE A WORLD OBJECT");
		}
		if (componentName == "PhysicsComponent")
		{
			if (WorldObject.GetComponentInChildren(typeof(IPhysicsTaker)) is IPhysicsTaker physicsTaker)
			{
				return physicsTaker.LuaObject;
			}
			return null;
		}
		if (componentName == "Player")
		{
			if (!WorldObject.TryGetUserComponent<PlayerLuaComponent>(out var component))
			{
				return null;
			}
			return component.LuaObject;
		}
		Type type = null;
		BaseTypeDefinition componentDefinition2;
		if (MonoBehaviourSingleton<StageManager>.Instance.ComponentList.TryGetDefinition(componentName, out var componentDefinition))
		{
			type = componentDefinition.ComponentBase.GetType();
		}
		else if (MonoBehaviourSingleton<StageManager>.Instance.BaseTypeList.TryGetDefinition(componentName, out componentDefinition2))
		{
			type = componentDefinition2.ComponentReferenceType;
			if (WorldObject.BaseType is IScriptInjector { AllowLuaReference: not false } scriptInjector)
			{
				return scriptInjector.LuaObject;
			}
		}
		if (type != null && WorldObject.TryGetUserComponent(type, out var component2) && component2 is IScriptInjector { AllowLuaReference: not false } scriptInjector2)
		{
			return scriptInjector2.LuaObject;
		}
		return null;
	}

	public override string ToString()
	{
		if (WorldObject != null)
		{
			return WorldObject.gameObject.name;
		}
		return "Context with no WorldObject!?";
	}
}
