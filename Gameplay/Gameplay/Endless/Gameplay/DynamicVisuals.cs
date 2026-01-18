using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Endless.Gameplay.LuaInterfaces;
using Endless.Gameplay.Scripting;
using Endless.Props;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Unity.Netcode;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x02000348 RID: 840
	public class DynamicVisuals : NetworkBehaviour, IScriptInjector, IComponentBase
	{
		// Token: 0x1700043F RID: 1087
		// (get) Token: 0x0600149A RID: 5274 RVA: 0x00062675 File Offset: 0x00060875
		// (set) Token: 0x0600149B RID: 5275 RVA: 0x0006267D File Offset: 0x0006087D
		public Transform VisualRoot { get; private set; }

		// Token: 0x17000440 RID: 1088
		// (get) Token: 0x0600149C RID: 5276 RVA: 0x00062686 File Offset: 0x00060886
		// (set) Token: 0x0600149D RID: 5277 RVA: 0x0006268E File Offset: 0x0006088E
		public bool VisualsSpawned { get; private set; }

		// Token: 0x17000441 RID: 1089
		// (get) Token: 0x0600149E RID: 5278 RVA: 0x00062698 File Offset: 0x00060898
		private Dictionary<SerializableGuid, DynamicVisuals.TransformReference> TransformMap
		{
			get
			{
				if (this.transformMap == null)
				{
					this.transformMap = new Dictionary<SerializableGuid, DynamicVisuals.TransformReference>();
					foreach (TransformIdentifier transformIdentifier in base.transform.parent.GetComponentsInChildren<TransformIdentifier>(true))
					{
						if (!this.transformMap.TryAdd(transformIdentifier.UniqueId, new DynamicVisuals.TransformReference(transformIdentifier.transform)))
						{
							throw new DuplicateNameException("The prop " + this.WorldObject.gameObject.name + " has a duplicated Transform ID. This object needs to be fixed via the SDK.");
						}
					}
				}
				return this.transformMap;
			}
		}

		// Token: 0x0600149F RID: 5279 RVA: 0x00062725 File Offset: 0x00060925
		private void OnValidate()
		{
			if (this.VisualRoot == null)
			{
				this.VisualRoot = base.transform;
			}
		}

		// Token: 0x060014A0 RID: 5280 RVA: 0x00062741 File Offset: 0x00060941
		public void SetPositionData(Context instigator, string transformId, global::UnityEngine.Vector3 goal, string callbackName = null)
		{
			this.SetPositionData(instigator, transformId, goal, -1f, callbackName);
		}

		// Token: 0x060014A1 RID: 5281 RVA: 0x00062753 File Offset: 0x00060953
		public void SetPositionData(Context instigator, string transformId, global::UnityEngine.Vector3 goal, float duration, string callbackName = null)
		{
			this.SetPositionData(instigator, transformId, this.TransformMap[transformId].Transform.localPosition, goal, duration, callbackName);
		}

		// Token: 0x060014A2 RID: 5282 RVA: 0x00062780 File Offset: 0x00060980
		public void SetPositionData(Context instigator, string transformId, global::UnityEngine.Vector3 start, global::UnityEngine.Vector3 goal, float duration, string callbackName = null)
		{
			this.TransformMap[transformId].SetDirtyPosition();
			this.serverActiveMovementDictionary[transformId] = new DynamicVisuals.ServerActiveMovementInfo(NetworkManager.Singleton.NetworkTimeSystem.ServerTime + (double)duration, global::UnityEngine.Vector3.zero, goal, false);
			this.StartMovementRoutine(transformId, (float)NetworkManager.Singleton.NetworkTimeSystem.ServerTime, duration, start, goal, instigator, callbackName);
			this.StartMovementRoutine_ClientRpc(transformId, (float)NetworkManager.Singleton.NetworkTimeSystem.ServerTime, duration, start, goal, callbackName);
		}

		// Token: 0x060014A3 RID: 5283 RVA: 0x0006280C File Offset: 0x00060A0C
		public void SetRotationData(string transformId, global::UnityEngine.Vector3 goal, Context instigator = null, string callbackName = null)
		{
			this.SetRotationData(transformId, goal, -1f, instigator, callbackName);
		}

		// Token: 0x060014A4 RID: 5284 RVA: 0x0006281E File Offset: 0x00060A1E
		public void SetRotationData(string transformId, global::UnityEngine.Vector3 goal, float duration, Context instigator = null, string callbackName = null)
		{
			this.SetRotationData(transformId, this.TransformMap[transformId].Transform.localEulerAngles, goal, duration, instigator, callbackName, false);
		}

		// Token: 0x060014A5 RID: 5285 RVA: 0x0006284C File Offset: 0x00060A4C
		public void SetContinuousRotationData(string transformId, global::UnityEngine.Vector3 rate, Context instigator)
		{
			if (rate.sqrMagnitude > 0f)
			{
				this.SetRotationData(transformId, this.TransformMap[transformId].Transform.localEulerAngles, rate, 0f, instigator, null, true);
				return;
			}
			if (this.serverActiveRotationDictionary.ContainsKey(transformId) && this.serverActiveRotationDictionary[transformId].Continuous)
			{
				this.serverActiveRotationDictionary.Remove(transformId);
				if (this.rotationRoutines.ContainsKey(transformId))
				{
					base.StopCoroutine(this.rotationRoutines[transformId]);
					this.rotationRoutines.Remove(transformId);
				}
			}
		}

		// Token: 0x060014A6 RID: 5286 RVA: 0x000628F0 File Offset: 0x00060AF0
		public void SetRotationData(string transformId, global::UnityEngine.Vector3 start, global::UnityEngine.Vector3 goal, float duration, Context instigator = null, string callbackName = null, bool continuous = false)
		{
			this.TransformMap[transformId].SetDirtyRotation();
			this.serverActiveRotationDictionary[transformId] = new DynamicVisuals.ServerActiveMovementInfo(NetworkManager.Singleton.NetworkTimeSystem.ServerTime + (double)duration, start, goal, continuous);
			this.StartRotationRoutine(transformId, (float)NetworkManager.Singleton.NetworkTimeSystem.ServerTime, duration, start, goal, instigator, callbackName, continuous);
			this.StartRotationRoutine_ClientRpc(transformId, (float)NetworkManager.Singleton.NetworkTimeSystem.ServerTime, duration, start, goal, callbackName, continuous);
		}

		// Token: 0x060014A7 RID: 5287 RVA: 0x0006297C File Offset: 0x00060B7C
		public void SetEmissionColor(string transformId, global::UnityEngine.Color color)
		{
			DynamicVisuals.TransformReference transformReference;
			if (this.TransformMap.TryGetValue(transformId, out transformReference))
			{
				transformReference.SetEmissionColor(color);
				this.SetEmissionColor_ClientRpc(transformId, color);
			}
		}

		// Token: 0x060014A8 RID: 5288 RVA: 0x000629B0 File Offset: 0x00060BB0
		public void SetAlbedoColor(string transformId, global::UnityEngine.Color color)
		{
			DynamicVisuals.TransformReference transformReference;
			if (this.TransformMap.TryGetValue(transformId, out transformReference))
			{
				transformReference.SetAlbedoColor(color);
				this.SetAlbedoColor_ClientRpc(transformId, color);
			}
		}

		// Token: 0x060014A9 RID: 5289 RVA: 0x000629E4 File Offset: 0x00060BE4
		public void SetEnabled(string transformId, bool enabled)
		{
			DynamicVisuals.TransformReference transformReference;
			if (this.TransformMap.TryGetValue(transformId, out transformReference))
			{
				transformReference.SetEnabled(enabled);
				this.SetEnabled_ClientRpc(transformId, enabled);
			}
		}

		// Token: 0x060014AA RID: 5290 RVA: 0x00062A18 File Offset: 0x00060C18
		protected override void OnSynchronize<T>(ref BufferSerializer<T> serializer)
		{
			base.OnSynchronize<T>(ref serializer);
			if (serializer.IsWriter)
			{
				Dictionary<SerializableGuid, DynamicVisuals.TransformReference> dictionary = this.TransformMap.Where((KeyValuePair<SerializableGuid, DynamicVisuals.TransformReference> i) => i.Value.HasDirtyProperties).ToDictionary((KeyValuePair<SerializableGuid, DynamicVisuals.TransformReference> i) => i.Key, (KeyValuePair<SerializableGuid, DynamicVisuals.TransformReference> i) => i.Value);
				Compression.SerializeInt<T>(serializer, dictionary.Count);
				using (Dictionary<SerializableGuid, DynamicVisuals.TransformReference>.Enumerator enumerator = dictionary.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						KeyValuePair<SerializableGuid, DynamicVisuals.TransformReference> keyValuePair = enumerator.Current;
						SerializableGuid key = keyValuePair.Key;
						DynamicVisuals.TransformReference value = keyValuePair.Value;
						serializer.SerializeValue<SerializableGuid>(ref key, default(FastBufferWriter.ForNetworkSerializable));
						Compression.SerializeBoolsToByte<T>(serializer, value.DirtyPosition, value.DirtyRotation, value.DirtyEmissionColor, value.DirtyAlbedoColor, value.DirtyEnabled, false, false, false);
						if (value.DirtyPosition)
						{
							global::UnityEngine.Vector3 localPosition = keyValuePair.Value.Transform.localPosition;
							serializer.SerializeValue(ref localPosition);
							DynamicVisuals.ServerActiveMovementInfo serverActiveMovementInfo;
							bool flag = this.serverActiveMovementDictionary.TryGetValue(key, out serverActiveMovementInfo);
							serializer.SerializeValue<bool>(ref flag, default(FastBufferWriter.ForPrimitives));
							if (flag)
							{
								serializer.SerializeValue(ref serverActiveMovementInfo.Goal);
								serializer.SerializeValue<double>(ref serverActiveMovementInfo.EndTime, default(FastBufferWriter.ForPrimitives));
								serializer.SerializeValue<bool>(ref serverActiveMovementInfo.Continuous, default(FastBufferWriter.ForPrimitives));
							}
						}
						if (value.DirtyRotation)
						{
							global::UnityEngine.Vector3 eulerAngles = keyValuePair.Value.Transform.localRotation.eulerAngles;
							serializer.SerializeValue(ref eulerAngles);
							DynamicVisuals.ServerActiveMovementInfo serverActiveMovementInfo2;
							bool flag2 = this.serverActiveRotationDictionary.TryGetValue(key, out serverActiveMovementInfo2);
							serializer.SerializeValue<bool>(ref flag2, default(FastBufferWriter.ForPrimitives));
							if (flag2)
							{
								serializer.SerializeValue(ref serverActiveMovementInfo2.Start);
								serializer.SerializeValue(ref serverActiveMovementInfo2.Goal);
								serializer.SerializeValue<double>(ref serverActiveMovementInfo2.EndTime, default(FastBufferWriter.ForPrimitives));
								serializer.SerializeValue<bool>(ref serverActiveMovementInfo2.Continuous, default(FastBufferWriter.ForPrimitives));
							}
						}
						if (value.DirtyEmissionColor)
						{
							serializer.SerializeValue(ref value.EmissionColor);
						}
						if (value.DirtyAlbedoColor)
						{
							serializer.SerializeValue(ref value.AlbedoColor);
						}
						if (value.DirtyEnabled)
						{
							serializer.SerializeValue<bool>(ref value.Enabled, default(FastBufferWriter.ForPrimitives));
						}
					}
					return;
				}
			}
			int num = Compression.DeserializeInt<T>(serializer);
			for (int j = 0; j < num; j++)
			{
				SerializableGuid empty = SerializableGuid.Empty;
				global::UnityEngine.Vector3 zero = global::UnityEngine.Vector3.zero;
				global::UnityEngine.Vector3 zero2 = global::UnityEngine.Vector3.zero;
				bool flag3 = false;
				bool flag4 = false;
				serializer.SerializeValue<SerializableGuid>(ref empty, default(FastBufferWriter.ForNetworkSerializable));
				bool flag5 = false;
				bool flag6 = false;
				bool flag7 = false;
				bool flag8 = false;
				bool flag9 = false;
				Compression.DeserializeBoolsFromByte<T>(serializer, ref flag5, ref flag6, ref flag7, ref flag8, ref flag9);
				if (flag5)
				{
					serializer.SerializeValue(ref zero);
					serializer.SerializeValue<bool>(ref flag3, default(FastBufferWriter.ForPrimitives));
					DynamicVisuals.TransformReference transformReference;
					if (this.TransformMap.TryGetValue(empty, out transformReference))
					{
						transformReference.Transform.localRotation = Quaternion.Euler(zero2);
						if (flag3)
						{
							global::UnityEngine.Vector3 zero3 = global::UnityEngine.Vector3.zero;
							double num2 = 0.0;
							serializer.SerializeValue(ref zero3);
							serializer.SerializeValue<double>(ref num2, default(FastBufferWriter.ForPrimitives));
							this.StartMovementRoutine(empty, (float)NetworkManager.Singleton.NetworkTimeSystem.ServerTime, (float)(num2 - NetworkManager.Singleton.NetworkTimeSystem.ServerTime), zero, zero3, null, null);
						}
					}
					else
					{
						Debug.LogWarning("Client transform mismatch..");
					}
				}
				if (flag6)
				{
					serializer.SerializeValue(ref zero2);
					serializer.SerializeValue<bool>(ref flag4, default(FastBufferWriter.ForPrimitives));
					DynamicVisuals.TransformReference transformReference2;
					if (this.TransformMap.TryGetValue(empty, out transformReference2))
					{
						transformReference2.Transform.localRotation = Quaternion.Euler(zero2);
						if (flag4)
						{
							global::UnityEngine.Vector3 zero4 = global::UnityEngine.Vector3.zero;
							global::UnityEngine.Vector3 zero5 = global::UnityEngine.Vector3.zero;
							double num3 = 0.0;
							bool flag10 = false;
							serializer.SerializeValue(ref zero4);
							serializer.SerializeValue(ref zero5);
							serializer.SerializeValue<double>(ref num3, default(FastBufferWriter.ForPrimitives));
							serializer.SerializeValue<bool>(ref flag10, default(FastBufferWriter.ForPrimitives));
							if (flag10)
							{
								this.StartRotationRoutine(empty, (float)num3, 0f, zero4, zero5, null, null, flag10);
							}
							else
							{
								this.StartRotationRoutine(empty, (float)NetworkManager.Singleton.NetworkTimeSystem.ServerTime, (float)(num3 - NetworkManager.Singleton.NetworkTimeSystem.ServerTime), zero2, zero5, null, null, flag10);
							}
						}
					}
					else
					{
						Debug.LogWarning("Client transform mismatch..");
					}
				}
				if (flag7)
				{
					serializer.SerializeValue(ref this.TransformMap[empty].EmissionColor);
					this.TransformMap[empty].SetEmissionColor(this.TransformMap[empty].EmissionColor);
				}
				if (flag8)
				{
					serializer.SerializeValue(ref this.TransformMap[empty].AlbedoColor);
					this.TransformMap[empty].SetAlbedoColor(this.TransformMap[empty].AlbedoColor);
				}
				if (flag9)
				{
					serializer.SerializeValue<bool>(ref this.TransformMap[empty].Enabled, default(FastBufferWriter.ForPrimitives));
					this.TransformMap[empty].SetEnabled(this.TransformMap[empty].Enabled);
				}
			}
		}

		// Token: 0x060014AB RID: 5291 RVA: 0x00062FD4 File Offset: 0x000611D4
		[ClientRpc]
		private void StartMovementRoutine_ClientRpc(string transformId, float startTime, float duration, global::UnityEngine.Vector3 movementStart, global::UnityEngine.Vector3 movementGoal, string callbackName = null)
		{
			NetworkManager networkManager = base.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
			{
				ClientRpcParams clientRpcParams;
				FastBufferWriter fastBufferWriter = base.__beginSendClientRpc(548630341U, clientRpcParams, RpcDelivery.Reliable);
				bool flag = transformId != null;
				fastBufferWriter.WriteValueSafe<bool>(in flag, default(FastBufferWriter.ForPrimitives));
				if (flag)
				{
					fastBufferWriter.WriteValueSafe(transformId, false);
				}
				fastBufferWriter.WriteValueSafe<float>(in startTime, default(FastBufferWriter.ForPrimitives));
				fastBufferWriter.WriteValueSafe<float>(in duration, default(FastBufferWriter.ForPrimitives));
				fastBufferWriter.WriteValueSafe(in movementStart);
				fastBufferWriter.WriteValueSafe(in movementGoal);
				bool flag2 = callbackName != null;
				fastBufferWriter.WriteValueSafe<bool>(in flag2, default(FastBufferWriter.ForPrimitives));
				if (flag2)
				{
					fastBufferWriter.WriteValueSafe(callbackName, false);
				}
				base.__endSendClientRpc(ref fastBufferWriter, 548630341U, clientRpcParams, RpcDelivery.Reliable);
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute || (!networkManager.IsClient && !networkManager.IsHost))
			{
				return;
			}
			this.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
			if (base.IsServer)
			{
				return;
			}
			this.StartMovementRoutine(transformId, startTime, duration, movementStart, movementGoal, null, callbackName);
		}

		// Token: 0x060014AC RID: 5292 RVA: 0x0006318C File Offset: 0x0006138C
		[ClientRpc]
		private void StartRotationRoutine_ClientRpc(string transformId, float startTime, float duration, global::UnityEngine.Vector3 rotationStart, global::UnityEngine.Vector3 rotationGoal, string callbackName = null, bool continuous = false)
		{
			NetworkManager networkManager = base.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
			{
				ClientRpcParams clientRpcParams;
				FastBufferWriter fastBufferWriter = base.__beginSendClientRpc(2523749201U, clientRpcParams, RpcDelivery.Reliable);
				bool flag = transformId != null;
				fastBufferWriter.WriteValueSafe<bool>(in flag, default(FastBufferWriter.ForPrimitives));
				if (flag)
				{
					fastBufferWriter.WriteValueSafe(transformId, false);
				}
				fastBufferWriter.WriteValueSafe<float>(in startTime, default(FastBufferWriter.ForPrimitives));
				fastBufferWriter.WriteValueSafe<float>(in duration, default(FastBufferWriter.ForPrimitives));
				fastBufferWriter.WriteValueSafe(in rotationStart);
				fastBufferWriter.WriteValueSafe(in rotationGoal);
				bool flag2 = callbackName != null;
				fastBufferWriter.WriteValueSafe<bool>(in flag2, default(FastBufferWriter.ForPrimitives));
				if (flag2)
				{
					fastBufferWriter.WriteValueSafe(callbackName, false);
				}
				fastBufferWriter.WriteValueSafe<bool>(in continuous, default(FastBufferWriter.ForPrimitives));
				base.__endSendClientRpc(ref fastBufferWriter, 2523749201U, clientRpcParams, RpcDelivery.Reliable);
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute || (!networkManager.IsClient && !networkManager.IsHost))
			{
				return;
			}
			this.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
			if (base.IsServer)
			{
				return;
			}
			this.StartRotationRoutine(transformId, startTime, duration, rotationStart, rotationGoal, null, callbackName, continuous);
		}

		// Token: 0x060014AD RID: 5293 RVA: 0x00063364 File Offset: 0x00061564
		[ClientRpc]
		private void SetEmissionColor_ClientRpc(string transformId, global::UnityEngine.Color color)
		{
			NetworkManager networkManager = base.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
			{
				ClientRpcParams clientRpcParams;
				FastBufferWriter fastBufferWriter = base.__beginSendClientRpc(3273904729U, clientRpcParams, RpcDelivery.Reliable);
				bool flag = transformId != null;
				fastBufferWriter.WriteValueSafe<bool>(in flag, default(FastBufferWriter.ForPrimitives));
				if (flag)
				{
					fastBufferWriter.WriteValueSafe(transformId, false);
				}
				fastBufferWriter.WriteValueSafe(in color);
				base.__endSendClientRpc(ref fastBufferWriter, 3273904729U, clientRpcParams, RpcDelivery.Reliable);
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute || (!networkManager.IsClient && !networkManager.IsHost))
			{
				return;
			}
			this.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
			DynamicVisuals.TransformReference transformReference;
			if (!base.IsServer && this.TransformMap.TryGetValue(transformId, out transformReference))
			{
				transformReference.SetEmissionColor(color);
			}
		}

		// Token: 0x060014AE RID: 5294 RVA: 0x000634A8 File Offset: 0x000616A8
		[ClientRpc]
		private void SetAlbedoColor_ClientRpc(string transformId, global::UnityEngine.Color color)
		{
			NetworkManager networkManager = base.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
			{
				ClientRpcParams clientRpcParams;
				FastBufferWriter fastBufferWriter = base.__beginSendClientRpc(1187108704U, clientRpcParams, RpcDelivery.Reliable);
				bool flag = transformId != null;
				fastBufferWriter.WriteValueSafe<bool>(in flag, default(FastBufferWriter.ForPrimitives));
				if (flag)
				{
					fastBufferWriter.WriteValueSafe(transformId, false);
				}
				fastBufferWriter.WriteValueSafe(in color);
				base.__endSendClientRpc(ref fastBufferWriter, 1187108704U, clientRpcParams, RpcDelivery.Reliable);
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute || (!networkManager.IsClient && !networkManager.IsHost))
			{
				return;
			}
			this.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
			DynamicVisuals.TransformReference transformReference;
			if (!base.IsServer && this.TransformMap.TryGetValue(transformId, out transformReference))
			{
				transformReference.SetAlbedoColor(color);
			}
		}

		// Token: 0x060014AF RID: 5295 RVA: 0x000635EC File Offset: 0x000617EC
		[ClientRpc]
		private void SetEnabled_ClientRpc(string transformId, bool enabled)
		{
			NetworkManager networkManager = base.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
			{
				ClientRpcParams clientRpcParams;
				FastBufferWriter fastBufferWriter = base.__beginSendClientRpc(2662578777U, clientRpcParams, RpcDelivery.Reliable);
				bool flag = transformId != null;
				fastBufferWriter.WriteValueSafe<bool>(in flag, default(FastBufferWriter.ForPrimitives));
				if (flag)
				{
					fastBufferWriter.WriteValueSafe(transformId, false);
				}
				fastBufferWriter.WriteValueSafe<bool>(in enabled, default(FastBufferWriter.ForPrimitives));
				base.__endSendClientRpc(ref fastBufferWriter, 2662578777U, clientRpcParams, RpcDelivery.Reliable);
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute || (!networkManager.IsClient && !networkManager.IsHost))
			{
				return;
			}
			this.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
			DynamicVisuals.TransformReference transformReference;
			if (!base.IsServer && this.TransformMap.TryGetValue(transformId, out transformReference))
			{
				transformReference.SetEnabled(enabled);
			}
		}

		// Token: 0x060014B0 RID: 5296 RVA: 0x0006373C File Offset: 0x0006193C
		private void StartMovementRoutine(string transformId, float startTime, float duration, global::UnityEngine.Vector3 movementStart, global::UnityEngine.Vector3 movementGoal, Context instigator = null, string callbackName = null)
		{
			if (this.movementRoutines.ContainsKey(transformId))
			{
				base.StopCoroutine(this.movementRoutines[transformId]);
				this.movementRoutines.Remove(transformId);
			}
			this.movementRoutines.Add(transformId, base.StartCoroutine(this.ApplyMovement(transformId, startTime, duration, movementStart, movementGoal, instigator, callbackName)));
		}

		// Token: 0x060014B1 RID: 5297 RVA: 0x0006379C File Offset: 0x0006199C
		private IEnumerator ApplyMovement(string transformId, float startTime, float duration, global::UnityEngine.Vector3 movementStart, global::UnityEngine.Vector3 movementGoal, Context instigator = null, string callbackName = null)
		{
			Transform target = this.GetTransform(transformId);
			float num = Vector3Extensions.InverseLerp(movementStart, movementGoal, target.localPosition);
			movementStart = target.localPosition;
			float num2 = (1f - num) * duration;
			float endTime = startTime + num2;
			MovingCollider[] movingColliders = target.GetComponentsInChildren<MovingCollider>();
			while ((double)endTime > NetworkManager.Singleton.NetworkTimeSystem.ServerTime)
			{
				float num3 = Mathf.InverseLerp(startTime, endTime, (float)NetworkManager.Singleton.NetworkTimeSystem.ServerTime);
				target.localPosition = global::UnityEngine.Vector3.Lerp(movementStart, movementGoal, num3);
				foreach (MovingCollider movingCollider in movingColliders)
				{
					MonoBehaviourSingleton<MovingColliderManager>.Instance.ColliderMoved(movingCollider);
				}
				yield return null;
			}
			yield return null;
			target.localPosition = movementGoal;
			foreach (MovingCollider movingCollider2 in movingColliders)
			{
				MonoBehaviourSingleton<MovingColliderManager>.Instance.ColliderMoved(movingCollider2);
			}
			if (instigator != null && callbackName != null)
			{
				object[] array2;
				this.scriptComponent.TryExecuteFunction(callbackName, out array2, new object[] { instigator, transformId });
			}
			if (base.IsServer)
			{
				this.serverActiveMovementDictionary.Remove(transformId);
			}
			this.movementRoutines.Remove(transformId);
			yield break;
		}

		// Token: 0x060014B2 RID: 5298 RVA: 0x000637EC File Offset: 0x000619EC
		private void StartRotationRoutine(string transformId, float startTime, float duration, global::UnityEngine.Vector3 rotationStart, global::UnityEngine.Vector3 rotationGoal, Context instigator = null, string callbackName = null, bool continuous = false)
		{
			if (this.rotationRoutines.ContainsKey(transformId))
			{
				base.StopCoroutine(this.rotationRoutines[transformId]);
				this.rotationRoutines.Remove(transformId);
			}
			this.rotationRoutines.Add(transformId, base.StartCoroutine(this.ApplyRotation(transformId, startTime, duration, rotationStart, rotationGoal, instigator, callbackName, continuous)));
		}

		// Token: 0x060014B3 RID: 5299 RVA: 0x0006384C File Offset: 0x00061A4C
		private IEnumerator ApplyRotation(string transformId, float startTime, float duration, global::UnityEngine.Vector3 rotationStart, global::UnityEngine.Vector3 rotationGoal, Context instigator = null, string callbackName = null, bool continuous = false)
		{
			float endTime = 0f;
			global::UnityEngine.Vector3 rotAxis = global::UnityEngine.Vector3.zero;
			float rotRate = 0f;
			Quaternion rotStart = Quaternion.identity;
			Transform target = this.GetTransform(transformId);
			MovingCollider[] movingColliders = target.GetComponentsInChildren<MovingCollider>();
			if (continuous)
			{
				rotRate = rotationGoal.magnitude;
				rotAxis = rotationGoal.normalized;
				rotStart = Quaternion.Euler(rotationStart);
			}
			else
			{
				float num = Vector3Extensions.InverseRotationLerp(rotationStart, rotationGoal, target.localRotation.eulerAngles);
				rotationStart = target.localRotation.eulerAngles;
				float num2 = (1f - num) * duration;
				endTime = startTime + num2;
			}
			while ((double)endTime > NetworkManager.Singleton.NetworkTimeSystem.ServerTime || continuous)
			{
				if (continuous)
				{
					double num3 = NetworkManager.Singleton.NetworkTimeSystem.ServerTime - (double)startTime;
					float num4 = rotRate * (float)num3 % 360f;
					target.localRotation = rotStart * Quaternion.AngleAxis(num4, rotAxis);
				}
				else
				{
					float num5 = Mathf.InverseLerp(startTime, endTime, (float)NetworkManager.Singleton.NetworkTimeSystem.ServerTime);
					target.localRotation = Quaternion.Slerp(Quaternion.Euler(rotationStart), Quaternion.Euler(rotationGoal), num5);
				}
				foreach (MovingCollider movingCollider in movingColliders)
				{
					MonoBehaviourSingleton<MovingColliderManager>.Instance.ColliderMoved(movingCollider);
				}
				yield return null;
			}
			yield return null;
			target.localRotation = Quaternion.Euler(rotationGoal);
			if (instigator != null && callbackName != null)
			{
				object[] array2;
				this.scriptComponent.TryExecuteFunction(callbackName, out array2, new object[] { instigator, transformId });
			}
			if (base.IsServer)
			{
				this.serverActiveRotationDictionary.Remove(transformId);
			}
			foreach (MovingCollider movingCollider2 in movingColliders)
			{
				MonoBehaviourSingleton<MovingColliderManager>.Instance.ColliderMoved(movingCollider2);
			}
			this.rotationRoutines.Remove(transformId);
			yield break;
		}

		// Token: 0x060014B4 RID: 5300 RVA: 0x000638A3 File Offset: 0x00061AA3
		private Transform GetTransform(string transformId)
		{
			return this.TransformMap[transformId].Transform;
		}

		// Token: 0x17000442 RID: 1090
		// (get) Token: 0x060014B5 RID: 5301 RVA: 0x000638BB File Offset: 0x00061ABB
		public Type LuaObjectType
		{
			get
			{
				return typeof(Visuals);
			}
		}

		// Token: 0x060014B6 RID: 5302 RVA: 0x000638C7 File Offset: 0x00061AC7
		public void ScriptInitialize(EndlessScriptComponent endlessScriptComponent)
		{
			this.scriptComponent = endlessScriptComponent;
		}

		// Token: 0x17000443 RID: 1091
		// (get) Token: 0x060014B7 RID: 5303 RVA: 0x000638D0 File Offset: 0x00061AD0
		public object LuaObject
		{
			get
			{
				if (this.luaInterface == null)
				{
					this.luaInterface = new Visuals(this);
				}
				return this.luaInterface;
			}
		}

		// Token: 0x17000444 RID: 1092
		// (get) Token: 0x060014B8 RID: 5304 RVA: 0x000638EC File Offset: 0x00061AEC
		// (set) Token: 0x060014B9 RID: 5305 RVA: 0x000638F4 File Offset: 0x00061AF4
		public WorldObject WorldObject { get; private set; }

		// Token: 0x060014BA RID: 5306 RVA: 0x00063900 File Offset: 0x00061B00
		public void PrefabInitialize(WorldObject worldObject)
		{
			this.WorldObject = worldObject;
			foreach (Collider collider in worldObject.GetComponentsInChildren<Collider>())
			{
				collider.gameObject.AddComponent<MovingCollider>().Init(collider);
			}
		}

		// Token: 0x060014BC RID: 5308 RVA: 0x00063974 File Offset: 0x00061B74
		protected override void __initializeVariables()
		{
			base.__initializeVariables();
		}

		// Token: 0x060014BD RID: 5309 RVA: 0x0006398C File Offset: 0x00061B8C
		protected override void __initializeRpcs()
		{
			base.__registerRpc(548630341U, new NetworkBehaviour.RpcReceiveHandler(DynamicVisuals.__rpc_handler_548630341), "StartMovementRoutine_ClientRpc");
			base.__registerRpc(2523749201U, new NetworkBehaviour.RpcReceiveHandler(DynamicVisuals.__rpc_handler_2523749201), "StartRotationRoutine_ClientRpc");
			base.__registerRpc(3273904729U, new NetworkBehaviour.RpcReceiveHandler(DynamicVisuals.__rpc_handler_3273904729), "SetEmissionColor_ClientRpc");
			base.__registerRpc(1187108704U, new NetworkBehaviour.RpcReceiveHandler(DynamicVisuals.__rpc_handler_1187108704), "SetAlbedoColor_ClientRpc");
			base.__registerRpc(2662578777U, new NetworkBehaviour.RpcReceiveHandler(DynamicVisuals.__rpc_handler_2662578777), "SetEnabled_ClientRpc");
			base.__initializeRpcs();
		}

		// Token: 0x060014BE RID: 5310 RVA: 0x00063A30 File Offset: 0x00061C30
		private static void __rpc_handler_548630341(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
		{
			NetworkManager networkManager = target.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			bool flag;
			reader.ReadValueSafe<bool>(out flag, default(FastBufferWriter.ForPrimitives));
			string text = null;
			if (flag)
			{
				reader.ReadValueSafe(out text, false);
			}
			float num;
			reader.ReadValueSafe<float>(out num, default(FastBufferWriter.ForPrimitives));
			float num2;
			reader.ReadValueSafe<float>(out num2, default(FastBufferWriter.ForPrimitives));
			global::UnityEngine.Vector3 vector;
			reader.ReadValueSafe(out vector);
			global::UnityEngine.Vector3 vector2;
			reader.ReadValueSafe(out vector2);
			bool flag2;
			reader.ReadValueSafe<bool>(out flag2, default(FastBufferWriter.ForPrimitives));
			string text2 = null;
			if (flag2)
			{
				reader.ReadValueSafe(out text2, false);
			}
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Execute;
			((DynamicVisuals)target).StartMovementRoutine_ClientRpc(text, num, num2, vector, vector2, text2);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x060014BF RID: 5311 RVA: 0x00063B5C File Offset: 0x00061D5C
		private static void __rpc_handler_2523749201(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
		{
			NetworkManager networkManager = target.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			bool flag;
			reader.ReadValueSafe<bool>(out flag, default(FastBufferWriter.ForPrimitives));
			string text = null;
			if (flag)
			{
				reader.ReadValueSafe(out text, false);
			}
			float num;
			reader.ReadValueSafe<float>(out num, default(FastBufferWriter.ForPrimitives));
			float num2;
			reader.ReadValueSafe<float>(out num2, default(FastBufferWriter.ForPrimitives));
			global::UnityEngine.Vector3 vector;
			reader.ReadValueSafe(out vector);
			global::UnityEngine.Vector3 vector2;
			reader.ReadValueSafe(out vector2);
			bool flag2;
			reader.ReadValueSafe<bool>(out flag2, default(FastBufferWriter.ForPrimitives));
			string text2 = null;
			if (flag2)
			{
				reader.ReadValueSafe(out text2, false);
			}
			bool flag3;
			reader.ReadValueSafe<bool>(out flag3, default(FastBufferWriter.ForPrimitives));
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Execute;
			((DynamicVisuals)target).StartRotationRoutine_ClientRpc(text, num, num2, vector, vector2, text2, flag3);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x060014C0 RID: 5312 RVA: 0x00063CA4 File Offset: 0x00061EA4
		private static void __rpc_handler_3273904729(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
		{
			NetworkManager networkManager = target.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			bool flag;
			reader.ReadValueSafe<bool>(out flag, default(FastBufferWriter.ForPrimitives));
			string text = null;
			if (flag)
			{
				reader.ReadValueSafe(out text, false);
			}
			global::UnityEngine.Color color;
			reader.ReadValueSafe(out color);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Execute;
			((DynamicVisuals)target).SetEmissionColor_ClientRpc(text, color);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x060014C1 RID: 5313 RVA: 0x00063D44 File Offset: 0x00061F44
		private static void __rpc_handler_1187108704(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
		{
			NetworkManager networkManager = target.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			bool flag;
			reader.ReadValueSafe<bool>(out flag, default(FastBufferWriter.ForPrimitives));
			string text = null;
			if (flag)
			{
				reader.ReadValueSafe(out text, false);
			}
			global::UnityEngine.Color color;
			reader.ReadValueSafe(out color);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Execute;
			((DynamicVisuals)target).SetAlbedoColor_ClientRpc(text, color);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x060014C2 RID: 5314 RVA: 0x00063DE4 File Offset: 0x00061FE4
		private static void __rpc_handler_2662578777(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
		{
			NetworkManager networkManager = target.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			bool flag;
			reader.ReadValueSafe<bool>(out flag, default(FastBufferWriter.ForPrimitives));
			string text = null;
			if (flag)
			{
				reader.ReadValueSafe(out text, false);
			}
			bool flag2;
			reader.ReadValueSafe<bool>(out flag2, default(FastBufferWriter.ForPrimitives));
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Execute;
			((DynamicVisuals)target).SetEnabled_ClientRpc(text, flag2);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x060014C3 RID: 5315 RVA: 0x00063E90 File Offset: 0x00062090
		protected internal override string __getTypeName()
		{
			return "DynamicVisuals";
		}

		// Token: 0x0400110B RID: 4363
		private Dictionary<string, Coroutine> movementRoutines = new Dictionary<string, Coroutine>();

		// Token: 0x0400110C RID: 4364
		private Dictionary<string, Coroutine> rotationRoutines = new Dictionary<string, Coroutine>();

		// Token: 0x0400110D RID: 4365
		private Dictionary<string, DynamicVisuals.ServerActiveMovementInfo> serverActiveMovementDictionary = new Dictionary<string, DynamicVisuals.ServerActiveMovementInfo>();

		// Token: 0x0400110E RID: 4366
		private Dictionary<string, DynamicVisuals.ServerActiveMovementInfo> serverActiveRotationDictionary = new Dictionary<string, DynamicVisuals.ServerActiveMovementInfo>();

		// Token: 0x04001110 RID: 4368
		private Dictionary<SerializableGuid, DynamicVisuals.TransformReference> transformMap;

		// Token: 0x04001111 RID: 4369
		private Visuals luaInterface;

		// Token: 0x04001112 RID: 4370
		public EndlessScriptComponent scriptComponent;

		// Token: 0x02000349 RID: 841
		private struct ServerActiveMovementInfo
		{
			// Token: 0x060014C4 RID: 5316 RVA: 0x00063E97 File Offset: 0x00062097
			public ServerActiveMovementInfo(double endTime, global::UnityEngine.Vector3 start, global::UnityEngine.Vector3 goal, bool continuous)
			{
				this.EndTime = endTime;
				this.Start = start;
				this.Goal = goal;
				this.Continuous = continuous;
			}

			// Token: 0x04001114 RID: 4372
			public double EndTime;

			// Token: 0x04001115 RID: 4373
			public global::UnityEngine.Vector3 Start;

			// Token: 0x04001116 RID: 4374
			public global::UnityEngine.Vector3 Goal;

			// Token: 0x04001117 RID: 4375
			public bool Continuous;
		}

		// Token: 0x0200034A RID: 842
		private class TransformReference
		{
			// Token: 0x17000445 RID: 1093
			// (get) Token: 0x060014C5 RID: 5317 RVA: 0x00063EB6 File Offset: 0x000620B6
			// (set) Token: 0x060014C6 RID: 5318 RVA: 0x00063EBE File Offset: 0x000620BE
			public bool DirtyPosition { get; private set; }

			// Token: 0x17000446 RID: 1094
			// (get) Token: 0x060014C7 RID: 5319 RVA: 0x00063EC7 File Offset: 0x000620C7
			// (set) Token: 0x060014C8 RID: 5320 RVA: 0x00063ECF File Offset: 0x000620CF
			public bool DirtyRotation { get; private set; }

			// Token: 0x17000447 RID: 1095
			// (get) Token: 0x060014C9 RID: 5321 RVA: 0x00063ED8 File Offset: 0x000620D8
			// (set) Token: 0x060014CA RID: 5322 RVA: 0x00063EE0 File Offset: 0x000620E0
			public bool DirtyEmissionColor { get; private set; }

			// Token: 0x17000448 RID: 1096
			// (get) Token: 0x060014CB RID: 5323 RVA: 0x00063EE9 File Offset: 0x000620E9
			// (set) Token: 0x060014CC RID: 5324 RVA: 0x00063EF1 File Offset: 0x000620F1
			public bool DirtyAlbedoColor { get; private set; }

			// Token: 0x17000449 RID: 1097
			// (get) Token: 0x060014CD RID: 5325 RVA: 0x00063EFA File Offset: 0x000620FA
			// (set) Token: 0x060014CE RID: 5326 RVA: 0x00063F02 File Offset: 0x00062102
			public bool DirtyEnabled { get; private set; }

			// Token: 0x1700044A RID: 1098
			// (get) Token: 0x060014CF RID: 5327 RVA: 0x00063F0B File Offset: 0x0006210B
			public bool HasDirtyProperties
			{
				get
				{
					return this.DirtyPosition || this.DirtyRotation || this.DirtyEmissionColor || this.DirtyAlbedoColor || this.DirtyEnabled;
				}
			}

			// Token: 0x1700044B RID: 1099
			// (get) Token: 0x060014D0 RID: 5328 RVA: 0x00063F38 File Offset: 0x00062138
			public List<Material> AllMaterials
			{
				get
				{
					if (this.allMaterials == null)
					{
						this.allMaterials = new List<Material>();
						foreach (Renderer renderer in this.Transform.GetComponents<Renderer>())
						{
							this.allMaterials.AddRange(renderer.materials);
						}
					}
					return this.allMaterials;
				}
			}

			// Token: 0x060014D1 RID: 5329 RVA: 0x00063F90 File Offset: 0x00062190
			public void SetEmissionColor(global::UnityEngine.Color color)
			{
				this.DirtyEmissionColor = true;
				this.EmissionColor = color;
				foreach (Material material in this.AllMaterials)
				{
					material.SetColor("_EmissionColor", color);
				}
			}

			// Token: 0x060014D2 RID: 5330 RVA: 0x00063FF4 File Offset: 0x000621F4
			public void SetAlbedoColor(global::UnityEngine.Color color)
			{
				this.DirtyAlbedoColor = true;
				this.AlbedoColor = color;
				foreach (Material material in this.AllMaterials)
				{
					material.SetColor("Albedo_Tint", color);
				}
			}

			// Token: 0x060014D3 RID: 5331 RVA: 0x00064058 File Offset: 0x00062258
			public void SetEnabled(bool enabled)
			{
				this.DirtyEnabled = true;
				this.Enabled = enabled;
				this.Transform.gameObject.SetActive(enabled);
			}

			// Token: 0x060014D4 RID: 5332 RVA: 0x00064079 File Offset: 0x00062279
			public void SetDirtyPosition()
			{
				this.DirtyPosition = true;
			}

			// Token: 0x060014D5 RID: 5333 RVA: 0x00064082 File Offset: 0x00062282
			public void SetDirtyRotation()
			{
				this.DirtyRotation = true;
			}

			// Token: 0x060014D6 RID: 5334 RVA: 0x0006408C File Offset: 0x0006228C
			public TransformReference(Transform Transform)
			{
				this.Transform = Transform;
				this.DirtyPosition = false;
				this.DirtyRotation = false;
				this.DirtyEmissionColor = false;
				this.DirtyAlbedoColor = false;
				this.EmissionColor = global::UnityEngine.Color.black;
				this.AlbedoColor = global::UnityEngine.Color.black;
				this.allMaterials = null;
				this.DirtyEnabled = false;
			}

			// Token: 0x04001118 RID: 4376
			public Transform Transform;

			// Token: 0x0400111B RID: 4379
			public global::UnityEngine.Color EmissionColor;

			// Token: 0x0400111C RID: 4380
			public global::UnityEngine.Color AlbedoColor;

			// Token: 0x0400111F RID: 4383
			public bool Enabled;

			// Token: 0x04001121 RID: 4385
			private List<Material> allMaterials;
		}
	}
}
