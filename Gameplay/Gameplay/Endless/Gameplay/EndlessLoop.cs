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

namespace Endless.Gameplay
{
	// Token: 0x0200008E RID: 142
	public class EndlessLoop : MonoBehaviourSingleton<EndlessLoop>
	{
		// Token: 0x1700007D RID: 125
		// (get) Token: 0x0600028A RID: 650 RVA: 0x0000DD64 File Offset: 0x0000BF64
		private bool IsPlaying
		{
			get
			{
				return MonoBehaviourSingleton<GameplayManager>.Instance && MonoBehaviourSingleton<GameplayManager>.Instance.IsPlaying;
			}
		}

		// Token: 0x1700007E RID: 126
		// (get) Token: 0x0600028B RID: 651 RVA: 0x0000DD7E File Offset: 0x0000BF7E
		public bool HasAwoken
		{
			get
			{
				return this.hasAwoken;
			}
		}

		// Token: 0x0600028C RID: 652 RVA: 0x0000DD86 File Offset: 0x0000BF86
		private IEnumerator RegisterBehaviourAfterPlay(Component endlessBehaviour)
		{
			IPersistantStateSubscriber persistantStateSubscriber = endlessBehaviour as IPersistantStateSubscriber;
			if (persistantStateSubscriber != null)
			{
				this.persistantStateList.Add(persistantStateSubscriber);
			}
			IAwakeSubscriber awakeSubscriber = endlessBehaviour as IAwakeSubscriber;
			if (awakeSubscriber != null)
			{
				try
				{
					awakeSubscriber.EndlessAwake();
				}
				catch (Exception ex)
				{
					Debug.LogException(ex);
				}
				this.awakeList.Add(awakeSubscriber);
			}
			IScriptAwakeSubscriber scriptAwakeSubscriber = endlessBehaviour as IScriptAwakeSubscriber;
			if (scriptAwakeSubscriber != null)
			{
				try
				{
					scriptAwakeSubscriber.EndlessScriptAwake();
				}
				catch (Exception ex2)
				{
					Debug.LogException(ex2);
				}
				this.scriptAwakeList.Add(scriptAwakeSubscriber);
			}
			IStartSubscriber startSubscriber = endlessBehaviour as IStartSubscriber;
			if (startSubscriber != null)
			{
				yield return new WaitForEndOfFrame();
				if (this.hasBegunPlay)
				{
					try
					{
						startSubscriber.EndlessStart();
					}
					catch (Exception ex3)
					{
						Debug.LogException(ex3);
					}
				}
				this.startList.Add(startSubscriber);
			}
			IUpdateSubscriber updateSubscriber = endlessBehaviour as IUpdateSubscriber;
			if (updateSubscriber != null)
			{
				this.updateList.Add(updateSubscriber);
			}
			ILateUpdateSubscriber lateUpdateSubscriber = endlessBehaviour as ILateUpdateSubscriber;
			if (lateUpdateSubscriber != null)
			{
				this.lateUpdateList.Add(lateUpdateSubscriber);
			}
			IFixedUpdateSubscriber fixedUpdateSubscriber = endlessBehaviour as IFixedUpdateSubscriber;
			if (fixedUpdateSubscriber != null)
			{
				this.fixedUpdateList.Add(fixedUpdateSubscriber);
			}
			IGameEndSubscriber gameEndSubscriber = endlessBehaviour as IGameEndSubscriber;
			if (gameEndSubscriber != null)
			{
				this.endGameList.Add(gameEndSubscriber);
			}
			yield break;
		}

		// Token: 0x0600028D RID: 653 RVA: 0x0000DD9C File Offset: 0x0000BF9C
		internal void RegisterBehaviour(Component endlessBehaviour)
		{
			if (this.hasBegunPlay)
			{
				base.StartCoroutine(this.RegisterBehaviourAfterPlay(endlessBehaviour));
				return;
			}
			IPersistantStateSubscriber persistantStateSubscriber = endlessBehaviour as IPersistantStateSubscriber;
			if (persistantStateSubscriber != null)
			{
				this.persistantStateList.Add(persistantStateSubscriber);
			}
			IAwakeSubscriber awakeSubscriber = endlessBehaviour as IAwakeSubscriber;
			if (awakeSubscriber != null)
			{
				this.awakeList.Add(awakeSubscriber);
			}
			IScriptAwakeSubscriber scriptAwakeSubscriber = endlessBehaviour as IScriptAwakeSubscriber;
			if (scriptAwakeSubscriber != null)
			{
				this.scriptAwakeList.Add(scriptAwakeSubscriber);
			}
			IStartSubscriber startSubscriber = endlessBehaviour as IStartSubscriber;
			if (startSubscriber != null)
			{
				this.startList.Add(startSubscriber);
			}
			IUpdateSubscriber updateSubscriber = endlessBehaviour as IUpdateSubscriber;
			if (updateSubscriber != null)
			{
				this.updateList.Add(updateSubscriber);
			}
			ILateUpdateSubscriber lateUpdateSubscriber = endlessBehaviour as ILateUpdateSubscriber;
			if (lateUpdateSubscriber != null)
			{
				this.lateUpdateList.Add(lateUpdateSubscriber);
			}
			IFixedUpdateSubscriber fixedUpdateSubscriber = endlessBehaviour as IFixedUpdateSubscriber;
			if (fixedUpdateSubscriber != null)
			{
				this.fixedUpdateList.Add(fixedUpdateSubscriber);
			}
			IGameEndSubscriber gameEndSubscriber = endlessBehaviour as IGameEndSubscriber;
			if (gameEndSubscriber != null)
			{
				this.endGameList.Add(gameEndSubscriber);
			}
		}

		// Token: 0x0600028E RID: 654 RVA: 0x0000DE7C File Offset: 0x0000C07C
		internal void LoadPropStates()
		{
			MonoBehaviourSingleton<GlobalContextsManager>.Instance.LoadState();
			foreach (IPersistantStateSubscriber persistantStateSubscriber in this.persistantStateList)
			{
				try
				{
					if (persistantStateSubscriber.ShouldSaveAndLoad)
					{
						Component component = persistantStateSubscriber as Component;
						SerializableGuid instanceIdFromGameObject = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetInstanceIdFromGameObject(this.GetHighestParent(component.transform).gameObject);
						if (instanceIdFromGameObject == SerializableGuid.Empty)
						{
							Debug.LogErrorFormat(component.gameObject, "Loading state for an invalid object! Component type {0}", new object[] { component.GetType().Name });
						}
						else
						{
							persistantStateSubscriber.LoadState(MonoBehaviourSingleton<StageManager>.Instance.GetPropState(instanceIdFromGameObject, component.GetType().Name));
						}
					}
				}
				catch (Exception ex)
				{
					Debug.LogException(new Exception(string.Format("Failed to load state for {0}", persistantStateSubscriber.GetType()), ex));
				}
			}
		}

		// Token: 0x0600028F RID: 655 RVA: 0x0000DF88 File Offset: 0x0000C188
		private Transform GetHighestParent(Transform target)
		{
			Transform transform = target;
			while (transform.parent != null)
			{
				transform = transform.parent;
			}
			return transform;
		}

		// Token: 0x06000290 RID: 656 RVA: 0x0000DFB0 File Offset: 0x0000C1B0
		internal void SavePropStates()
		{
			MonoBehaviourSingleton<GlobalContextsManager>.Instance.SaveState();
			foreach (IPersistantStateSubscriber persistantStateSubscriber in this.persistantStateList)
			{
				try
				{
					if (persistantStateSubscriber.ShouldSaveAndLoad)
					{
						Component component = persistantStateSubscriber as Component;
						SerializableGuid instanceIdFromGameObject = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetInstanceIdFromGameObject(this.GetHighestParent(component.transform).gameObject);
						if (instanceIdFromGameObject == SerializableGuid.Empty)
						{
							Debug.LogError("Saving state for an invalid object! Component type " + component.GetType().Name + " - " + component.gameObject.name, component.gameObject);
						}
						else
						{
							MonoBehaviourSingleton<StageManager>.Instance.SavePropState(instanceIdFromGameObject, component.GetType().Name, persistantStateSubscriber.GetSaveState());
						}
					}
				}
				catch (Exception ex)
				{
					Debug.LogException(ex);
				}
			}
		}

		// Token: 0x06000291 RID: 657 RVA: 0x0000E0B0 File Offset: 0x0000C2B0
		internal void EnterPlayMode()
		{
			Debug.Log("Entering play mode");
			if (!this.hasBegunPlay)
			{
				this.hasBegunPlay = true;
				foreach (IAwakeSubscriber awakeSubscriber in this.awakeList)
				{
					try
					{
						awakeSubscriber.EndlessAwake();
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
					}
				}
				if (NetworkManager.Singleton.IsServer)
				{
					foreach (IScriptAwakeSubscriber scriptAwakeSubscriber in this.scriptAwakeList)
					{
						try
						{
							scriptAwakeSubscriber.EndlessScriptAwake();
						}
						catch (Exception ex2)
						{
							Debug.LogException(ex2);
						}
					}
					this.LoadPropStates();
					this.OnCharacterSpawnRequested.Invoke();
				}
				this.hasAwoken = true;
				using (List<IStartSubscriber>.Enumerator enumerator3 = this.startList.GetEnumerator())
				{
					while (enumerator3.MoveNext())
					{
						IStartSubscriber startSubscriber = enumerator3.Current;
						try
						{
							startSubscriber.EndlessStart();
						}
						catch (Exception ex3)
						{
							Debug.LogException(ex3);
						}
					}
					return;
				}
			}
			Debug.LogException(new Exception("Cannot start EndlessLoop if it is already running."));
		}

		// Token: 0x06000292 RID: 658 RVA: 0x0000E218 File Offset: 0x0000C418
		internal void ExitPlayMode()
		{
			this.hasBegunPlay = false;
			this.hasAwoken = false;
			this.isPaused = false;
			LuaInterfaceEvent.ClearAllSubscribers();
			for (int i = this.endGameList.Count - 1; i >= 0; i--)
			{
				try
				{
					this.endGameList[i].EndlessGameEnd();
				}
				catch (Exception ex)
				{
					Debug.LogException(ex);
				}
			}
		}

		// Token: 0x06000293 RID: 659 RVA: 0x0000E284 File Offset: 0x0000C484
		private void Update()
		{
			if (this.IsPlaying && !this.isPaused)
			{
				foreach (IUpdateSubscriber updateSubscriber in this.updateList)
				{
					try
					{
						updateSubscriber.EndlessUpdate();
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
					}
				}
			}
		}

		// Token: 0x06000294 RID: 660 RVA: 0x0000E2FC File Offset: 0x0000C4FC
		private void FixedUpdate()
		{
			if (this.IsPlaying && !this.isPaused)
			{
				foreach (IFixedUpdateSubscriber fixedUpdateSubscriber in this.fixedUpdateList)
				{
					try
					{
						fixedUpdateSubscriber.EndlessFixedUpdate();
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
					}
				}
			}
		}

		// Token: 0x06000295 RID: 661 RVA: 0x0000E374 File Offset: 0x0000C574
		private void LateUpdate()
		{
			if (this.IsPlaying && !this.isPaused)
			{
				foreach (ILateUpdateSubscriber lateUpdateSubscriber in this.lateUpdateList)
				{
					try
					{
						lateUpdateSubscriber.EndlessLateUpdate();
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
					}
				}
			}
		}

		// Token: 0x06000296 RID: 662 RVA: 0x0000E3EC File Offset: 0x0000C5EC
		public void RemoveBehaviour(Component endlessBehaviour)
		{
			if (endlessBehaviour is IPersistantStateSubscriber)
			{
				this.persistantStateList.Remove(endlessBehaviour as IPersistantStateSubscriber);
			}
			if (endlessBehaviour is IAwakeSubscriber)
			{
				this.awakeList.Remove(endlessBehaviour as IAwakeSubscriber);
			}
			if (endlessBehaviour is IScriptAwakeSubscriber)
			{
				this.scriptAwakeList.Remove(endlessBehaviour as IScriptAwakeSubscriber);
			}
			if (endlessBehaviour is IStartSubscriber)
			{
				this.startList.Remove(endlessBehaviour as IStartSubscriber);
			}
			if (endlessBehaviour is IUpdateSubscriber)
			{
				this.updateList.Remove(endlessBehaviour as IUpdateSubscriber);
			}
			if (endlessBehaviour is ILateUpdateSubscriber)
			{
				this.lateUpdateList.Remove(endlessBehaviour as ILateUpdateSubscriber);
			}
			if (endlessBehaviour is IGameEndSubscriber)
			{
				this.endGameList.Remove(endlessBehaviour as IGameEndSubscriber);
			}
			if (endlessBehaviour is IFixedUpdateSubscriber)
			{
				this.fixedUpdateList.Remove(endlessBehaviour as IFixedUpdateSubscriber);
			}
		}

		// Token: 0x06000297 RID: 663 RVA: 0x0000E4C9 File Offset: 0x0000C6C9
		public void SuspendPlay()
		{
			this.isPaused = true;
		}

		// Token: 0x06000298 RID: 664 RVA: 0x0000E4D2 File Offset: 0x0000C6D2
		public void UnsuspendPlay()
		{
			this.isPaused = false;
		}

		// Token: 0x04000278 RID: 632
		public UnityEvent OnCharacterSpawnRequested = new UnityEvent();

		// Token: 0x04000279 RID: 633
		private List<IPersistantStateSubscriber> persistantStateList = new List<IPersistantStateSubscriber>();

		// Token: 0x0400027A RID: 634
		private List<IAwakeSubscriber> awakeList = new List<IAwakeSubscriber>();

		// Token: 0x0400027B RID: 635
		private List<IScriptAwakeSubscriber> scriptAwakeList = new List<IScriptAwakeSubscriber>();

		// Token: 0x0400027C RID: 636
		private List<IStartSubscriber> startList = new List<IStartSubscriber>();

		// Token: 0x0400027D RID: 637
		private List<IUpdateSubscriber> updateList = new List<IUpdateSubscriber>();

		// Token: 0x0400027E RID: 638
		private List<ILateUpdateSubscriber> lateUpdateList = new List<ILateUpdateSubscriber>();

		// Token: 0x0400027F RID: 639
		private List<IFixedUpdateSubscriber> fixedUpdateList = new List<IFixedUpdateSubscriber>();

		// Token: 0x04000280 RID: 640
		private List<IGameEndSubscriber> endGameList = new List<IGameEndSubscriber>();

		// Token: 0x04000281 RID: 641
		private bool hasAwoken;

		// Token: 0x04000282 RID: 642
		private bool hasBegunPlay;

		// Token: 0x04000283 RID: 643
		private bool isPaused;
	}
}
