using System;
using System.Collections;
using System.Collections.Generic;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Gameplay.Scripting;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Gameplay;

public class EndlessLoop : MonoBehaviourSingleton<EndlessLoop>
{
	public UnityEvent OnCharacterSpawnRequested = new UnityEvent();

	private List<IPersistantStateSubscriber> persistantStateList = new List<IPersistantStateSubscriber>();

	private List<IAwakeSubscriber> awakeList = new List<IAwakeSubscriber>();

	private List<IScriptAwakeSubscriber> scriptAwakeList = new List<IScriptAwakeSubscriber>();

	private List<IStartSubscriber> startList = new List<IStartSubscriber>();

	private List<IUpdateSubscriber> updateList = new List<IUpdateSubscriber>();

	private List<ILateUpdateSubscriber> lateUpdateList = new List<ILateUpdateSubscriber>();

	private List<IFixedUpdateSubscriber> fixedUpdateList = new List<IFixedUpdateSubscriber>();

	private List<IGameEndSubscriber> endGameList = new List<IGameEndSubscriber>();

	private bool hasAwoken;

	private bool hasBegunPlay;

	private bool isPaused;

	private bool IsPlaying
	{
		get
		{
			if ((bool)MonoBehaviourSingleton<GameplayManager>.Instance)
			{
				return MonoBehaviourSingleton<GameplayManager>.Instance.IsPlaying;
			}
			return false;
		}
	}

	public bool HasAwoken => hasAwoken;

	private IEnumerator RegisterBehaviourAfterPlay(Component endlessBehaviour)
	{
		if (endlessBehaviour is IPersistantStateSubscriber item)
		{
			persistantStateList.Add(item);
		}
		if (endlessBehaviour is IAwakeSubscriber awakeSubscriber)
		{
			try
			{
				awakeSubscriber.EndlessAwake();
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
			awakeList.Add(awakeSubscriber);
		}
		if (endlessBehaviour is IScriptAwakeSubscriber scriptAwakeSubscriber)
		{
			try
			{
				scriptAwakeSubscriber.EndlessScriptAwake();
			}
			catch (Exception exception2)
			{
				Debug.LogException(exception2);
			}
			scriptAwakeList.Add(scriptAwakeSubscriber);
		}
		if (endlessBehaviour is IStartSubscriber startSubscriber)
		{
			yield return new WaitForEndOfFrame();
			if (hasBegunPlay)
			{
				try
				{
					startSubscriber.EndlessStart();
				}
				catch (Exception exception3)
				{
					Debug.LogException(exception3);
				}
			}
			startList.Add(startSubscriber);
		}
		if (endlessBehaviour is IUpdateSubscriber item2)
		{
			updateList.Add(item2);
		}
		if (endlessBehaviour is ILateUpdateSubscriber item3)
		{
			lateUpdateList.Add(item3);
		}
		if (endlessBehaviour is IFixedUpdateSubscriber item4)
		{
			fixedUpdateList.Add(item4);
		}
		if (endlessBehaviour is IGameEndSubscriber item5)
		{
			endGameList.Add(item5);
		}
	}

	internal void RegisterBehaviour(Component endlessBehaviour)
	{
		if (hasBegunPlay)
		{
			StartCoroutine(RegisterBehaviourAfterPlay(endlessBehaviour));
			return;
		}
		if (endlessBehaviour is IPersistantStateSubscriber item)
		{
			persistantStateList.Add(item);
		}
		if (endlessBehaviour is IAwakeSubscriber item2)
		{
			awakeList.Add(item2);
		}
		if (endlessBehaviour is IScriptAwakeSubscriber item3)
		{
			scriptAwakeList.Add(item3);
		}
		if (endlessBehaviour is IStartSubscriber item4)
		{
			startList.Add(item4);
		}
		if (endlessBehaviour is IUpdateSubscriber item5)
		{
			updateList.Add(item5);
		}
		if (endlessBehaviour is ILateUpdateSubscriber item6)
		{
			lateUpdateList.Add(item6);
		}
		if (endlessBehaviour is IFixedUpdateSubscriber item7)
		{
			fixedUpdateList.Add(item7);
		}
		if (endlessBehaviour is IGameEndSubscriber item8)
		{
			endGameList.Add(item8);
		}
	}

	internal void LoadPropStates()
	{
		MonoBehaviourSingleton<GlobalContextsManager>.Instance.LoadState();
		foreach (IPersistantStateSubscriber persistantState in persistantStateList)
		{
			try
			{
				if (persistantState.ShouldSaveAndLoad)
				{
					Component component = persistantState as Component;
					SerializableGuid instanceIdFromGameObject = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetInstanceIdFromGameObject(GetHighestParent(component.transform).gameObject);
					if (instanceIdFromGameObject == SerializableGuid.Empty)
					{
						Debug.LogErrorFormat(component.gameObject, "Loading state for an invalid object! Component type {0}", component.GetType().Name);
					}
					else
					{
						persistantState.LoadState(MonoBehaviourSingleton<StageManager>.Instance.GetPropState(instanceIdFromGameObject, component.GetType().Name));
					}
				}
			}
			catch (Exception innerException)
			{
				Debug.LogException(new Exception($"Failed to load state for {persistantState.GetType()}", innerException));
			}
		}
	}

	private Transform GetHighestParent(Transform target)
	{
		Transform transform = target;
		while (transform.parent != null)
		{
			transform = transform.parent;
		}
		return transform;
	}

	internal void SavePropStates()
	{
		MonoBehaviourSingleton<GlobalContextsManager>.Instance.SaveState();
		foreach (IPersistantStateSubscriber persistantState in persistantStateList)
		{
			try
			{
				if (persistantState.ShouldSaveAndLoad)
				{
					Component component = persistantState as Component;
					SerializableGuid instanceIdFromGameObject = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetInstanceIdFromGameObject(GetHighestParent(component.transform).gameObject);
					if (instanceIdFromGameObject == SerializableGuid.Empty)
					{
						Debug.LogError("Saving state for an invalid object! Component type " + component.GetType().Name + " - " + component.gameObject.name, component.gameObject);
					}
					else
					{
						MonoBehaviourSingleton<StageManager>.Instance.SavePropState(instanceIdFromGameObject, component.GetType().Name, persistantState.GetSaveState());
					}
				}
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}
	}

	internal void EnterPlayMode()
	{
		Debug.Log("Entering play mode");
		if (!hasBegunPlay)
		{
			hasBegunPlay = true;
			foreach (IAwakeSubscriber awake in awakeList)
			{
				try
				{
					awake.EndlessAwake();
				}
				catch (Exception exception)
				{
					Debug.LogException(exception);
				}
			}
			if (NetworkManager.Singleton.IsServer)
			{
				foreach (IScriptAwakeSubscriber scriptAwake in scriptAwakeList)
				{
					try
					{
						scriptAwake.EndlessScriptAwake();
					}
					catch (Exception exception2)
					{
						Debug.LogException(exception2);
					}
				}
				LoadPropStates();
				OnCharacterSpawnRequested.Invoke();
			}
			hasAwoken = true;
			{
				foreach (IStartSubscriber start in startList)
				{
					try
					{
						start.EndlessStart();
					}
					catch (Exception exception3)
					{
						Debug.LogException(exception3);
					}
				}
				return;
			}
		}
		Debug.LogException(new Exception("Cannot start EndlessLoop if it is already running."));
	}

	internal void ExitPlayMode()
	{
		hasBegunPlay = false;
		hasAwoken = false;
		isPaused = false;
		LuaInterfaceEvent.ClearAllSubscribers();
		for (int num = endGameList.Count - 1; num >= 0; num--)
		{
			try
			{
				endGameList[num].EndlessGameEnd();
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}
	}

	private void Update()
	{
		if (!IsPlaying || isPaused)
		{
			return;
		}
		foreach (IUpdateSubscriber update in updateList)
		{
			try
			{
				update.EndlessUpdate();
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}
	}

	private void FixedUpdate()
	{
		if (!IsPlaying || isPaused)
		{
			return;
		}
		foreach (IFixedUpdateSubscriber fixedUpdate in fixedUpdateList)
		{
			try
			{
				fixedUpdate.EndlessFixedUpdate();
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}
	}

	private void LateUpdate()
	{
		if (!IsPlaying || isPaused)
		{
			return;
		}
		foreach (ILateUpdateSubscriber lateUpdate in lateUpdateList)
		{
			try
			{
				lateUpdate.EndlessLateUpdate();
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}
	}

	public void RemoveBehaviour(Component endlessBehaviour)
	{
		if (endlessBehaviour is IPersistantStateSubscriber)
		{
			persistantStateList.Remove(endlessBehaviour as IPersistantStateSubscriber);
		}
		if (endlessBehaviour is IAwakeSubscriber)
		{
			awakeList.Remove(endlessBehaviour as IAwakeSubscriber);
		}
		if (endlessBehaviour is IScriptAwakeSubscriber)
		{
			scriptAwakeList.Remove(endlessBehaviour as IScriptAwakeSubscriber);
		}
		if (endlessBehaviour is IStartSubscriber)
		{
			startList.Remove(endlessBehaviour as IStartSubscriber);
		}
		if (endlessBehaviour is IUpdateSubscriber)
		{
			updateList.Remove(endlessBehaviour as IUpdateSubscriber);
		}
		if (endlessBehaviour is ILateUpdateSubscriber)
		{
			lateUpdateList.Remove(endlessBehaviour as ILateUpdateSubscriber);
		}
		if (endlessBehaviour is IGameEndSubscriber)
		{
			endGameList.Remove(endlessBehaviour as IGameEndSubscriber);
		}
		if (endlessBehaviour is IFixedUpdateSubscriber)
		{
			fixedUpdateList.Remove(endlessBehaviour as IFixedUpdateSubscriber);
		}
	}

	public void SuspendPlay()
	{
		isPaused = true;
	}

	public void UnsuspendPlay()
	{
		isPaused = false;
	}
}
