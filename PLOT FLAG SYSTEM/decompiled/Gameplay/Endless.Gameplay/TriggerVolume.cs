using System;
using System.Collections.Generic;
using Endless.Gameplay.LuaEnums;
using Endless.Gameplay.LuaInterfaces;
using Endless.Gameplay.Scripting;
using Endless.Props.ReferenceComponents;
using UnityEngine;

namespace Endless.Gameplay;

public class TriggerVolume : MonoBehaviour, IBaseType, IComponentBase, IScriptInjector
{
	[SerializeField]
	private WorldTrigger worldTrigger;

	[SerializeField]
	private ContextTypes objectTypes = ContextTypes.Player | ContextTypes.NPC | ContextTypes.PhysicsObject;

	public EndlessEvent OnEntered = new EndlessEvent();

	public EndlessEvent OnExited = new EndlessEvent();

	[SerializeField]
	private Transform scaleTransform;

	[SerializeField]
	private Renderer volumeVisuals;

	private List<Context> enteredContexts = new List<Context>();

	private int forward;

	private int backward;

	private int left;

	private int right;

	private int up;

	private int down;

	[SerializeField]
	[HideInInspector]
	private EndlessProp endlessProp;

	private Context context;

	private EndlessScriptComponent endlessScriptComponent;

	private object luaObject;

	public int Forward
	{
		get
		{
			return forward;
		}
		set
		{
			value = Mathf.Max(value, 0);
			forward = value;
			RecalculateZAxis();
		}
	}

	public int Backward
	{
		get
		{
			return backward;
		}
		set
		{
			value = Mathf.Max(value, 0);
			backward = value;
			RecalculateZAxis();
		}
	}

	public int Left
	{
		get
		{
			return left;
		}
		set
		{
			value = Mathf.Max(value, 0);
			left = value;
			RecalculateXAxis();
		}
	}

	public int Right
	{
		get
		{
			return right;
		}
		set
		{
			value = Mathf.Max(value, 0);
			right = value;
			RecalculateXAxis();
		}
	}

	public int Up
	{
		get
		{
			return up;
		}
		set
		{
			value = Mathf.Max(value, 0);
			up = value;
			RecalculateYAxis();
		}
	}

	public int Down
	{
		get
		{
			return down;
		}
		set
		{
			value = Mathf.Max(value, 0);
			down = value;
			RecalculateYAxis();
		}
	}

	public WorldObject WorldObject { get; private set; }

	public Context Context => context ?? (context = new Context(WorldObject));

	public NavType NavValue => NavType.Intangible;

	public object LuaObject => luaObject ?? (luaObject = new Endless.Gameplay.LuaInterfaces.TriggerVolume(this));

	public Type LuaObjectType => typeof(Endless.Gameplay.LuaInterfaces.TriggerVolume);

	private void RecalculateYAxis()
	{
		int num = up + down + 1;
		float y = (float)(up - down) / 2f;
		scaleTransform.localScale = new UnityEngine.Vector3(scaleTransform.localScale.x, num, scaleTransform.localScale.z);
		scaleTransform.localPosition = new UnityEngine.Vector3(scaleTransform.localPosition.x, y, scaleTransform.localPosition.z);
	}

	private void RecalculateXAxis()
	{
		int num = right + left + 1;
		float x = (float)(right - left) / 2f;
		scaleTransform.localScale = new UnityEngine.Vector3(num, scaleTransform.localScale.y, scaleTransform.localScale.z);
		scaleTransform.localPosition = new UnityEngine.Vector3(x, scaleTransform.localPosition.y, scaleTransform.localPosition.z);
	}

	private void RecalculateZAxis()
	{
		int num = forward + backward + 1;
		float z = (float)(forward - backward) / 2f;
		scaleTransform.localScale = new UnityEngine.Vector3(scaleTransform.localScale.x, scaleTransform.localScale.y, num);
		scaleTransform.localPosition = new UnityEngine.Vector3(scaleTransform.localPosition.x, scaleTransform.localPosition.y, z);
	}

	private void Awake()
	{
		worldTrigger.OnTriggerEnter.AddListener(HandleTriggerEntered);
		worldTrigger.OnTriggerExit.AddListener(HandleTriggerExited);
		endlessProp.OnInspectionStateChanged.AddListener(HandleInspectionStateChanged);
	}

	private void HandleTriggerEntered(WorldCollidable worldCollider, bool rollbackFrame)
	{
		if (worldCollider == null || worldCollider.WorldObject == null)
		{
			return;
		}
		WorldObject worldObject = worldCollider.WorldObject;
		Debug.Log($"Entered {worldCollider.gameObject.name}, {rollbackFrame}, {worldCollider.IsServer}");
		if (!rollbackFrame && worldCollider.IsServer)
		{
			PlayerLuaComponent component2;
			DraggablePhysicsCube component3;
			if (objectTypes.HasFlag(ContextTypes.NPC) && worldObject.TryGetUserComponent<NpcEntity>(out var component))
			{
				AttemptTrigger(component.Context);
			}
			else if (objectTypes.HasFlag(ContextTypes.Player) && worldObject.TryGetUserComponent<PlayerLuaComponent>(out component2))
			{
				AttemptTrigger(component2.Context);
			}
			else if (objectTypes.HasFlag(ContextTypes.PhysicsObject) && worldObject.TryGetUserComponent<DraggablePhysicsCube>(out component3))
			{
				AttemptTrigger(component3.Context);
			}
		}
	}

	private void HandleTriggerExited(WorldCollidable worldCollidable, bool rollbackFrame)
	{
		if (!(worldCollidable == null) && !(worldCollidable.WorldObject == null) && worldCollidable.WorldObject.Context != null)
		{
			Context item = worldCollidable.WorldObject.Context;
			if (enteredContexts.Remove(item))
			{
				OnExited.Invoke(item);
			}
		}
	}

	private void AttemptTrigger(Context context)
	{
		if (endlessScriptComponent.TryExecuteFunction("OnAttemptEnter", out bool returnValue, (object)context))
		{
			if (returnValue)
			{
				HandleContextEntered(context);
			}
		}
		else
		{
			HandleContextEntered(context);
		}
	}

	private void HandleContextEntered(Context context)
	{
		enteredContexts.Add(context);
		OnEntered.Invoke(context);
	}

	public void ChangeAcceptedObjectTypes(ContextTypes newTypes)
	{
		objectTypes = newTypes;
	}

	private void HandleInspectionStateChanged(bool isInspected)
	{
		volumeVisuals.enabled = isInspected;
	}

	public void PrefabInitialize(WorldObject worldObject)
	{
		WorldObject = worldObject;
	}

	public void ComponentInitialize(ReferenceBase referenceBase, EndlessProp endlessProp)
	{
		this.endlessProp = endlessProp;
		worldTrigger.gameObject.AddComponent<WorldTriggerCollider>().Initialize(worldTrigger);
	}

	public void ScriptInitialize(EndlessScriptComponent endlessScriptComponent)
	{
		this.endlessScriptComponent = endlessScriptComponent;
	}
}
