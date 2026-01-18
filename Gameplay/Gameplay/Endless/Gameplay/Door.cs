using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Gameplay.LuaInterfaces;
using Endless.Gameplay.Scripting;
using Endless.Props.ReferenceComponents;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x02000306 RID: 774
	public class Door : EndlessNetworkBehaviour, IPersistantStateSubscriber, IBaseType, IComponentBase, IScriptInjector
	{
		// Token: 0x1700038C RID: 908
		// (get) Token: 0x060011DC RID: 4572 RVA: 0x00058667 File Offset: 0x00056867
		public bool IsLocked
		{
			get
			{
				return this.lockable != null && this.lockable.IsLocked;
			}
		}

		// Token: 0x1700038D RID: 909
		// (get) Token: 0x060011DD RID: 4573 RVA: 0x00058684 File Offset: 0x00056884
		public bool IsOpenOrOpening
		{
			get
			{
				Door.DoorState currentState = this.doorData.Value.currentState;
				return currentState == Door.DoorState.Open || currentState == Door.DoorState.Opening;
			}
		}

		// Token: 0x1700038E RID: 910
		// (get) Token: 0x060011DE RID: 4574 RVA: 0x000586B3 File Offset: 0x000568B3
		public Door.NpcDoorInteraction CurrentNpcDoorInteraction
		{
			get
			{
				return this.npcDoorInteraction;
			}
		}

		// Token: 0x060011DF RID: 4575 RVA: 0x000586BB File Offset: 0x000568BB
		internal void OpenFromUser(Context instigator)
		{
			this.Open(instigator, !this.GetIsUserForward(instigator));
		}

		// Token: 0x060011E0 RID: 4576 RVA: 0x000586CE File Offset: 0x000568CE
		private bool GetIsUserForward(Context instigator)
		{
			return base.transform.InverseTransformPoint(instigator.WorldObject.transform.position).z > 0f;
		}

		// Token: 0x060011E1 RID: 4577 RVA: 0x000586F8 File Offset: 0x000568F8
		public void Open(Context instigator, bool forwardDirection)
		{
			if (!this.references.OpenBothDirections)
			{
				forwardDirection = true;
			}
			if (base.IsServer && !this.IsLocked)
			{
				this.lastInstigator = instigator;
				Door.DoorNetworkData value = this.doorData.Value;
				if (value.currentState == Door.DoorState.Closed)
				{
					value.doorDirection = forwardDirection;
					value.currentState = Door.DoorState.Opening;
					value.triggerFrame = NetClock.CurrentFrame + this.references.DoorOpenDelayFrames;
				}
				else if (value.currentState == Door.DoorState.Closing)
				{
					value.doorDirection = forwardDirection;
					this.switchQueued = true;
				}
				else if (value.currentState == Door.DoorState.Opening)
				{
					this.switchQueued = false;
				}
				this.doorData.Value = value;
			}
		}

		// Token: 0x060011E2 RID: 4578 RVA: 0x000587A4 File Offset: 0x000569A4
		public void Close(Context instigator)
		{
			if (base.IsServer)
			{
				this.lastInstigator = instigator;
				Door.DoorNetworkData value = this.doorData.Value;
				if (value.currentState == Door.DoorState.Open)
				{
					value.currentState = Door.DoorState.Closing;
					value.triggerFrame = NetClock.CurrentFrame + this.references.DoorOpenDelayFrames;
				}
				else if (value.currentState == Door.DoorState.Opening)
				{
					this.switchQueued = true;
				}
				else if (value.currentState == Door.DoorState.Closing)
				{
					this.switchQueued = false;
				}
				this.doorData.Value = value;
			}
		}

		// Token: 0x060011E3 RID: 4579 RVA: 0x00058828 File Offset: 0x00056A28
		public void ToggleOpen(Context instigator, bool forwardDirection)
		{
			switch (this.doorData.Value.currentState)
			{
			case Door.DoorState.Closed:
				this.Open(instigator, forwardDirection);
				return;
			case Door.DoorState.Opening:
				this.switchQueued = true;
				return;
			case Door.DoorState.Open:
				this.Close(instigator);
				return;
			case Door.DoorState.Closing:
				this.switchQueued = true;
				this.lastOpenDirection = forwardDirection;
				return;
			default:
				return;
			}
		}

		// Token: 0x060011E4 RID: 4580 RVA: 0x00058884 File Offset: 0x00056A84
		public void ToggleOpenFromUser(Context instigator)
		{
			switch (this.doorData.Value.currentState)
			{
			case Door.DoorState.Closed:
				this.OpenFromUser(instigator);
				return;
			case Door.DoorState.Opening:
				this.switchQueued = true;
				return;
			case Door.DoorState.Open:
				this.Close(instigator);
				return;
			case Door.DoorState.Closing:
				this.switchQueued = true;
				this.lastOpenDirection = !this.GetIsUserForward(instigator);
				return;
			default:
				return;
			}
		}

		// Token: 0x060011E5 RID: 4581 RVA: 0x000588E8 File Offset: 0x00056AE8
		protected virtual void HandleDoorDataUpdated(Door.DoorNetworkData previousValue, Door.DoorNetworkData newValue)
		{
			if (previousValue.currentState != newValue.currentState)
			{
				if (this.doorData.Value.currentState == Door.DoorState.Opening)
				{
					if (this.currentAnimationCoroutine != null)
					{
						base.StopCoroutine(this.currentAnimationCoroutine);
					}
					this.currentAnimationCoroutine = base.StartCoroutine(this.OpenAnimation(this.doorData.Value.doorDirection));
					return;
				}
				if (this.doorData.Value.currentState == Door.DoorState.Closing)
				{
					if (this.currentAnimationCoroutine != null)
					{
						base.StopCoroutine(this.currentAnimationCoroutine);
					}
					this.currentAnimationCoroutine = base.StartCoroutine(this.CloseAnimation());
				}
			}
		}

		// Token: 0x060011E6 RID: 4582 RVA: 0x00058989 File Offset: 0x00056B89
		private IEnumerator OpenAnimation(bool forward)
		{
			this.lastOpenDirection = forward;
			double frameTime = NetClock.GetFrameTime(this.doorData.Value.triggerFrame);
			double num;
			if (base.IsServer)
			{
				num = NetClock.LocalNetworkTime;
			}
			else
			{
				num = NetClock.ClientExtrapolatedAppearanceTime;
			}
			float elapsedTime;
			for (elapsedTime = (float)(num - frameTime); elapsedTime < 0f; elapsedTime += Time.deltaTime)
			{
				yield return null;
			}
			if (this.references.DoorAnimator)
			{
				this.references.DoorAnimator.SetTrigger(this.references.UnlockAnimName);
				yield return new WaitForSeconds(this.references.UnlockAnimTime);
			}
			Quaternion startRotation = Quaternion.identity;
			Quaternion endRotationPrimary = Quaternion.identity * Quaternion.Euler(forward ? this.references.RotationDegrees : (-this.references.RotationDegrees));
			Quaternion endRotationSecondary = Quaternion.identity * Quaternion.Euler(forward ? (-this.references.RotationDegrees) : this.references.RotationDegrees);
			while (elapsedTime < this.references.DoorOpenTime)
			{
				float num2 = elapsedTime / this.references.DoorOpenTime;
				this.references.PrimaryDoor.localRotation = Quaternion.Slerp(startRotation, endRotationPrimary, num2);
				if (this.references.SecondaryDoor)
				{
					this.references.SecondaryDoor.localRotation = Quaternion.Slerp(startRotation, endRotationSecondary, num2);
				}
				yield return null;
				elapsedTime += Time.deltaTime;
			}
			this.references.PrimaryDoor.localRotation = endRotationPrimary;
			if (this.references.SecondaryDoor)
			{
				this.references.SecondaryDoor.localRotation = endRotationSecondary;
			}
			if (base.IsServer)
			{
				MonoBehaviourSingleton<Pathfinding>.Instance.OnPathfindingUpdated -= this.HandlePathfindingUpdatedAfterOpen;
				MonoBehaviourSingleton<Pathfinding>.Instance.OnPathfindingUpdated += this.HandlePathfindingUpdatedAfterOpen;
				MonoBehaviourSingleton<NavGraph>.Instance.PropStateChanged(this.WorldObject, false);
			}
			yield break;
		}

		// Token: 0x060011E7 RID: 4583 RVA: 0x000589A0 File Offset: 0x00056BA0
		private void HandlePathfindingUpdatedAfterOpen(HashSet<SerializableGuid> updatedProps)
		{
			if (updatedProps.Contains(this.WorldObject.InstanceId))
			{
				this.OnDoorOpened.Invoke(this.lastInstigator);
				object[] array;
				this.scriptComponent.TryExecuteFunction("OnDoorOpened", out array, new object[] { this.lastInstigator });
				Door.DoorNetworkData value = this.doorData.Value;
				value.currentState = Door.DoorState.Open;
				this.doorData.Value = value;
				if (this.switchQueued)
				{
					this.switchQueued = false;
					this.Close(this.lastInstigator);
				}
				MonoBehaviourSingleton<Pathfinding>.Instance.OnPathfindingUpdated -= this.HandlePathfindingUpdatedAfterOpen;
			}
		}

		// Token: 0x060011E8 RID: 4584 RVA: 0x00058A47 File Offset: 0x00056C47
		private IEnumerator CloseAnimation()
		{
			double frameTime = NetClock.GetFrameTime(this.doorData.Value.triggerFrame);
			double num;
			if (base.IsServer)
			{
				num = NetClock.LocalNetworkTime;
			}
			else
			{
				num = NetClock.ClientExtrapolatedAppearanceTime;
			}
			float elapsedTime;
			for (elapsedTime = (float)(num - frameTime); elapsedTime < 0f; elapsedTime += Time.deltaTime)
			{
				yield return null;
			}
			Quaternion startRotationPrimary = Quaternion.Euler(this.lastOpenDirection ? this.references.RotationDegrees : (-this.references.RotationDegrees));
			Quaternion startRotationSecondary = Quaternion.Euler(this.lastOpenDirection ? (-this.references.RotationDegrees) : this.references.RotationDegrees);
			Quaternion endRotation = Quaternion.identity;
			while (elapsedTime < this.references.DoorOpenTime)
			{
				float num2 = elapsedTime / this.references.DoorOpenTime;
				this.references.PrimaryDoor.localRotation = Quaternion.Slerp(startRotationPrimary, endRotation, num2);
				if (this.references.SecondaryDoor)
				{
					this.references.SecondaryDoor.localRotation = Quaternion.Slerp(startRotationSecondary, endRotation, num2);
				}
				yield return null;
				elapsedTime += Time.deltaTime;
			}
			this.references.PrimaryDoor.localRotation = endRotation;
			if (this.references.SecondaryDoor)
			{
				this.references.SecondaryDoor.localRotation = endRotation;
			}
			if (this.references.DoorAnimator)
			{
				this.references.DoorAnimator.SetTrigger(this.references.LockAnimName);
				yield return new WaitForSeconds(this.references.LockAnimTime);
			}
			if (base.IsServer)
			{
				MonoBehaviourSingleton<NavGraph>.Instance.PropStateChanged(this.WorldObject, true);
				MonoBehaviourSingleton<Pathfinding>.Instance.OnPathfindingUpdated -= this.HandlePathfindingUpdatedAfterClose;
				MonoBehaviourSingleton<Pathfinding>.Instance.OnPathfindingUpdated += this.HandlePathfindingUpdatedAfterClose;
			}
			yield break;
		}

		// Token: 0x060011E9 RID: 4585 RVA: 0x00058A58 File Offset: 0x00056C58
		private void HandlePathfindingUpdatedAfterClose(HashSet<SerializableGuid> obj)
		{
			if (obj.Contains(this.WorldObject.InstanceId))
			{
				this.OnDoorClosed.Invoke(this.lastInstigator);
				object[] array;
				this.scriptComponent.TryExecuteFunction("OnDoorClosed", out array, new object[] { this.lastInstigator });
				Door.DoorNetworkData value = this.doorData.Value;
				value.currentState = Door.DoorState.Closed;
				this.doorData.Value = value;
				if (this.switchQueued)
				{
					this.switchQueued = false;
					this.Open(this.lastInstigator, this.lastOpenDirection);
				}
				MonoBehaviourSingleton<Pathfinding>.Instance.OnPathfindingUpdated -= this.HandlePathfindingUpdatedAfterClose;
			}
		}

		// Token: 0x060011EA RID: 4586 RVA: 0x00058B08 File Offset: 0x00056D08
		public override void OnNetworkSpawn()
		{
			base.OnNetworkSpawn();
			NetworkVariable<Door.DoorNetworkData> networkVariable = this.doorData;
			networkVariable.OnValueChanged = (NetworkVariable<Door.DoorNetworkData>.OnValueChangedDelegate)Delegate.Combine(networkVariable.OnValueChanged, new NetworkVariable<Door.DoorNetworkData>.OnValueChangedDelegate(this.HandleDoorDataUpdated));
			Door.DoorState currentState = this.doorData.Value.currentState;
			if (currentState == Door.DoorState.Open || currentState == Door.DoorState.Opening)
			{
				bool doorDirection = this.doorData.Value.doorDirection;
				this.ForceDoorPositionOpen(doorDirection);
				this.HandleInitializedOpen();
			}
		}

		// Token: 0x060011EB RID: 4587 RVA: 0x00058B7C File Offset: 0x00056D7C
		private void ForceDoorPositionOpen(bool direction)
		{
			Quaternion quaternion = Quaternion.identity * Quaternion.Euler(direction ? this.references.RotationDegrees : (-this.references.RotationDegrees));
			Quaternion quaternion2 = Quaternion.identity * Quaternion.Euler(direction ? (-this.references.RotationDegrees) : this.references.RotationDegrees);
			this.references.PrimaryDoor.localRotation = quaternion;
			if (this.references.SecondaryDoor)
			{
				this.references.SecondaryDoor.localRotation = quaternion2;
			}
		}

		// Token: 0x060011EC RID: 4588 RVA: 0x00058C1D File Offset: 0x00056E1D
		private void ForceDoorPositionClosed()
		{
			this.references.PrimaryDoor.localRotation = Quaternion.identity;
			if (this.references.SecondaryDoor)
			{
				this.references.SecondaryDoor.localRotation = Quaternion.identity;
			}
		}

		// Token: 0x060011ED RID: 4589 RVA: 0x00058C5B File Offset: 0x00056E5B
		protected virtual void HandleInitializedOpen()
		{
			if (this.references.DoorAnimator)
			{
				this.references.DoorAnimator.SetTrigger(this.references.UnlockAnimName);
			}
		}

		// Token: 0x060011EE RID: 4590 RVA: 0x00058C8A File Offset: 0x00056E8A
		public List<int3> CaptureClosedCells()
		{
			this.ForceDoorPositionClosed();
			return this.CaptureDoorCells();
		}

		// Token: 0x060011EF RID: 4591 RVA: 0x00058C98 File Offset: 0x00056E98
		public List<int3> OpenForwardAndReturnCells()
		{
			this.ForceDoorPositionOpen(true);
			return this.CaptureDoorCells();
		}

		// Token: 0x060011F0 RID: 4592 RVA: 0x00058CA8 File Offset: 0x00056EA8
		private List<int3> CaptureDoorCells()
		{
			List<int3> list = new List<int3>();
			int layer = LayerMask.NameToLayer("Default");
			IEnumerable<Collider> enumerable = from currentCollider in this.references.PrimaryDoor.GetComponentsInChildren<Collider>()
				where currentCollider.gameObject.layer == layer
				select currentCollider;
			if (this.references.SecondaryDoor != null)
			{
				enumerable = enumerable.Concat(from currentCollider in this.references.SecondaryDoor.GetComponentsInChildren<Collider>()
					where currentCollider.gameObject.layer == layer
					select currentCollider);
			}
			Bounds bounds = default(Bounds);
			foreach (Collider collider in enumerable)
			{
				if (bounds.size == global::UnityEngine.Vector3.zero)
				{
					bounds = collider.bounds;
				}
				else
				{
					bounds.Encapsulate(collider.bounds);
				}
			}
			Vector3Int vector3Int = Stage.WorldSpacePointToGridCoordinate(bounds.min);
			Vector3Int vector3Int2 = Stage.WorldSpacePointToGridCoordinate(bounds.max);
			global::UnityEngine.Vector3 vector = global::UnityEngine.Vector3.one / 2f;
			Collider[] array = new Collider[100];
			for (int i = vector3Int.x; i <= vector3Int2.x; i++)
			{
				for (int j = vector3Int.y; j <= vector3Int2.y; j++)
				{
					for (int k = vector3Int.z; k <= vector3Int2.z; k++)
					{
						Vector3Int vector3Int3 = new Vector3Int(i, j, k);
						int mask = LayerMask.GetMask(new string[] { "Default" });
						int num = Physics.OverlapBoxNonAlloc(vector3Int3, vector, array, Quaternion.identity, mask);
						bool flag = false;
						for (int l = 0; l < num; l++)
						{
							if (enumerable.Contains(array[l]))
							{
								flag = true;
								break;
							}
						}
						if (flag)
						{
							list.Add(new int3(i, j, k));
						}
					}
				}
			}
			return list;
		}

		// Token: 0x060011F1 RID: 4593 RVA: 0x00058EAC File Offset: 0x000570AC
		public List<int3> OpenBackwardAndReturnCells()
		{
			if (!this.references.OpenBothDirections)
			{
				return new List<int3>();
			}
			this.ForceDoorPositionOpen(false);
			return this.CaptureDoorCells();
		}

		// Token: 0x060011F2 RID: 4594 RVA: 0x00058ECE File Offset: 0x000570CE
		public void Close()
		{
			this.ForceDoorPositionClosed();
		}

		// Token: 0x1700038F RID: 911
		// (get) Token: 0x060011F3 RID: 4595 RVA: 0x00017586 File Offset: 0x00015786
		public bool ShouldSaveAndLoad
		{
			get
			{
				return true;
			}
		}

		// Token: 0x060011F4 RID: 4596 RVA: 0x00058ED8 File Offset: 0x000570D8
		public object GetSaveState()
		{
			Door.DoorState currentState = this.doorData.Value.currentState;
			return new ValueTuple<bool, bool>(currentState == Door.DoorState.Open || currentState == Door.DoorState.Opening, this.doorData.Value.doorDirection);
		}

		// Token: 0x060011F5 RID: 4597 RVA: 0x00058F24 File Offset: 0x00057124
		public void LoadState(object loadedState)
		{
			if (base.IsServer && loadedState != null)
			{
				ValueTuple<bool, bool> valueTuple = (ValueTuple<bool, bool>)loadedState;
				if (valueTuple.Item1)
				{
					this.LoadOpen(valueTuple.Item2);
				}
			}
		}

		// Token: 0x060011F6 RID: 4598 RVA: 0x00058F58 File Offset: 0x00057158
		private void LoadOpen(bool direction)
		{
			Quaternion quaternion = Quaternion.identity * Quaternion.Euler(direction ? this.references.RotationDegrees : (-this.references.RotationDegrees));
			Quaternion quaternion2 = Quaternion.identity * Quaternion.Euler(direction ? (-this.references.RotationDegrees) : this.references.RotationDegrees);
			this.references.PrimaryDoor.localRotation = quaternion;
			if (this.references.SecondaryDoor)
			{
				this.references.SecondaryDoor.localRotation = quaternion2;
			}
			if (base.IsServer)
			{
				Door.DoorNetworkData value = this.doorData.Value;
				value.doorDirection = direction;
				value.currentState = Door.DoorState.Open;
				this.doorData.Value = value;
				this.LoadOpen_ClientRpc(direction);
			}
		}

		// Token: 0x060011F7 RID: 4599 RVA: 0x00059030 File Offset: 0x00057230
		[ClientRpc]
		private void LoadOpen_ClientRpc(bool direction)
		{
			NetworkManager networkManager = base.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
			{
				ClientRpcParams clientRpcParams;
				FastBufferWriter fastBufferWriter = base.__beginSendClientRpc(2445460209U, clientRpcParams, RpcDelivery.Reliable);
				fastBufferWriter.WriteValueSafe<bool>(in direction, default(FastBufferWriter.ForPrimitives));
				base.__endSendClientRpc(ref fastBufferWriter, 2445460209U, clientRpcParams, RpcDelivery.Reliable);
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute || (!networkManager.IsClient && !networkManager.IsHost))
			{
				return;
			}
			this.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
			if (!base.IsServer)
			{
				this.LoadOpen(direction);
			}
		}

		// Token: 0x17000390 RID: 912
		// (get) Token: 0x060011F8 RID: 4600 RVA: 0x0005912D File Offset: 0x0005732D
		public Type ComponentReferenceType
		{
			get
			{
				return typeof(DoorReferences);
			}
		}

		// Token: 0x17000391 RID: 913
		// (get) Token: 0x060011F9 RID: 4601 RVA: 0x00059139 File Offset: 0x00057339
		// (set) Token: 0x060011FA RID: 4602 RVA: 0x00059141 File Offset: 0x00057341
		public WorldObject WorldObject { get; private set; }

		// Token: 0x17000392 RID: 914
		// (get) Token: 0x060011FB RID: 4603 RVA: 0x0005914C File Offset: 0x0005734C
		public Context Context
		{
			get
			{
				Context context;
				if ((context = this.context) == null)
				{
					context = (this.context = new Context(this.WorldObject));
				}
				return context;
			}
		}

		// Token: 0x17000393 RID: 915
		// (get) Token: 0x060011FC RID: 4604 RVA: 0x00017586 File Offset: 0x00015786
		public NavType NavValue
		{
			get
			{
				return NavType.Dynamic;
			}
		}

		// Token: 0x060011FD RID: 4605 RVA: 0x00059178 File Offset: 0x00057378
		public void PrefabInitialize(WorldObject worldObject)
		{
			this.WorldObject = worldObject;
			object obj;
			if (worldObject.TryGetUserComponent(typeof(Lockable), out obj))
			{
				this.lockable = (Lockable)obj;
			}
		}

		// Token: 0x060011FE RID: 4606 RVA: 0x000591AC File Offset: 0x000573AC
		public void ComponentInitialize(ReferenceBase referenceBase, EndlessProp endlessProp)
		{
			this.references = (DoorReferences)referenceBase;
		}

		// Token: 0x17000394 RID: 916
		// (get) Token: 0x060011FF RID: 4607 RVA: 0x000591BC File Offset: 0x000573BC
		public object LuaObject
		{
			get
			{
				Door door;
				if ((door = this.luaInterface) == null)
				{
					door = (this.luaInterface = new Door(this));
				}
				return door;
			}
		}

		// Token: 0x17000395 RID: 917
		// (get) Token: 0x06001200 RID: 4608 RVA: 0x000591E2 File Offset: 0x000573E2
		public Type LuaObjectType
		{
			get
			{
				return typeof(Door);
			}
		}

		// Token: 0x06001201 RID: 4609 RVA: 0x000591EE File Offset: 0x000573EE
		public void ScriptInitialize(EndlessScriptComponent endlessScriptComponent)
		{
			this.scriptComponent = endlessScriptComponent;
		}

		// Token: 0x06001203 RID: 4611 RVA: 0x00059240 File Offset: 0x00057440
		protected override void __initializeVariables()
		{
			bool flag = this.doorData == null;
			if (flag)
			{
				throw new Exception("Door.doorData cannot be null. All NetworkVariableBase instances must be initialized.");
			}
			this.doorData.Initialize(this);
			base.__nameNetworkVariable(this.doorData, "doorData");
			this.NetworkVariableFields.Add(this.doorData);
			base.__initializeVariables();
		}

		// Token: 0x06001204 RID: 4612 RVA: 0x000592A3 File Offset: 0x000574A3
		protected override void __initializeRpcs()
		{
			base.__registerRpc(2445460209U, new NetworkBehaviour.RpcReceiveHandler(Door.__rpc_handler_2445460209), "LoadOpen_ClientRpc");
			base.__initializeRpcs();
		}

		// Token: 0x06001205 RID: 4613 RVA: 0x000592CC File Offset: 0x000574CC
		private static void __rpc_handler_2445460209(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
		{
			NetworkManager networkManager = target.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			bool flag;
			reader.ReadValueSafe<bool>(out flag, default(FastBufferWriter.ForPrimitives));
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Execute;
			((Door)target).LoadOpen_ClientRpc(flag);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x06001206 RID: 4614 RVA: 0x0005933C File Offset: 0x0005753C
		protected internal override string __getTypeName()
		{
			return "Door";
		}

		// Token: 0x04000F36 RID: 3894
		[SerializeField]
		private Door.NpcDoorInteraction npcDoorInteraction = Door.NpcDoorInteraction.Openable;

		// Token: 0x04000F37 RID: 3895
		[SerializeField]
		[HideInInspector]
		private Lockable lockable;

		// Token: 0x04000F38 RID: 3896
		public readonly EndlessEvent OnDoorOpened = new EndlessEvent();

		// Token: 0x04000F39 RID: 3897
		public readonly EndlessEvent OnDoorClosed = new EndlessEvent();

		// Token: 0x04000F3A RID: 3898
		private readonly NetworkVariable<Door.DoorNetworkData> doorData = new NetworkVariable<Door.DoorNetworkData>(default(Door.DoorNetworkData), NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

		// Token: 0x04000F3B RID: 3899
		private bool switchQueued;

		// Token: 0x04000F3C RID: 3900
		private Coroutine currentAnimationCoroutine;

		// Token: 0x04000F3D RID: 3901
		private bool lastOpenDirection;

		// Token: 0x04000F3E RID: 3902
		private Context lastInstigator;

		// Token: 0x04000F3F RID: 3903
		private Context context;

		// Token: 0x04000F40 RID: 3904
		[SerializeField]
		private DoorReferences references;

		// Token: 0x04000F42 RID: 3906
		private Door luaInterface;

		// Token: 0x04000F43 RID: 3907
		private EndlessScriptComponent scriptComponent;

		// Token: 0x02000307 RID: 775
		protected struct DoorNetworkData : INetworkSerializable
		{
			// Token: 0x06001207 RID: 4615 RVA: 0x00059344 File Offset: 0x00057544
			public void NetworkSerialize<DoorNetworkData>(BufferSerializer<DoorNetworkData> serializer) where DoorNetworkData : IReaderWriter
			{
				serializer.SerializeValue<Door.DoorState>(ref this.currentState, default(FastBufferWriter.ForEnums));
				serializer.SerializeValue<bool>(ref this.doorDirection, default(FastBufferWriter.ForPrimitives));
				serializer.SerializeValue<uint>(ref this.triggerFrame, default(FastBufferWriter.ForPrimitives));
			}

			// Token: 0x04000F44 RID: 3908
			public Door.DoorState currentState;

			// Token: 0x04000F45 RID: 3909
			public bool doorDirection;

			// Token: 0x04000F46 RID: 3910
			public uint triggerFrame;
		}

		// Token: 0x02000308 RID: 776
		protected enum DoorState : byte
		{
			// Token: 0x04000F48 RID: 3912
			Closed,
			// Token: 0x04000F49 RID: 3913
			Opening,
			// Token: 0x04000F4A RID: 3914
			Open,
			// Token: 0x04000F4B RID: 3915
			Closing
		}

		// Token: 0x02000309 RID: 777
		public enum NpcDoorInteraction
		{
			// Token: 0x04000F4D RID: 3917
			NotOpenable,
			// Token: 0x04000F4E RID: 3918
			Openable,
			// Token: 0x04000F4F RID: 3919
			OpenAndCloseBehind
		}
	}
}
