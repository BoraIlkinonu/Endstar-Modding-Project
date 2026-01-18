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

namespace Endless.Gameplay;

public class DynamicVisuals : NetworkBehaviour, IScriptInjector, IComponentBase
{
	private struct ServerActiveMovementInfo
	{
		public double EndTime;

		public UnityEngine.Vector3 Start;

		public UnityEngine.Vector3 Goal;

		public bool Continuous;

		public ServerActiveMovementInfo(double endTime, UnityEngine.Vector3 start, UnityEngine.Vector3 goal, bool continuous)
		{
			EndTime = endTime;
			Start = start;
			Goal = goal;
			Continuous = continuous;
		}
	}

	private class TransformReference
	{
		public Transform Transform;

		public UnityEngine.Color EmissionColor;

		public UnityEngine.Color AlbedoColor;

		public bool Enabled;

		private List<Material> allMaterials;

		public bool DirtyPosition { get; private set; }

		public bool DirtyRotation { get; private set; }

		public bool DirtyEmissionColor { get; private set; }

		public bool DirtyAlbedoColor { get; private set; }

		public bool DirtyEnabled { get; private set; }

		public bool HasDirtyProperties
		{
			get
			{
				if (!DirtyPosition && !DirtyRotation && !DirtyEmissionColor && !DirtyAlbedoColor)
				{
					return DirtyEnabled;
				}
				return true;
			}
		}

		public List<Material> AllMaterials
		{
			get
			{
				if (allMaterials == null)
				{
					allMaterials = new List<Material>();
					Renderer[] components = Transform.GetComponents<Renderer>();
					foreach (Renderer renderer in components)
					{
						allMaterials.AddRange(renderer.materials);
					}
				}
				return allMaterials;
			}
		}

		public void SetEmissionColor(UnityEngine.Color color)
		{
			DirtyEmissionColor = true;
			EmissionColor = color;
			foreach (Material allMaterial in AllMaterials)
			{
				allMaterial.SetColor("_EmissionColor", color);
			}
		}

		public void SetAlbedoColor(UnityEngine.Color color)
		{
			DirtyAlbedoColor = true;
			AlbedoColor = color;
			foreach (Material allMaterial in AllMaterials)
			{
				allMaterial.SetColor("Albedo_Tint", color);
			}
		}

		public void SetEnabled(bool enabled)
		{
			DirtyEnabled = true;
			Enabled = enabled;
			Transform.gameObject.SetActive(enabled);
		}

		public void SetDirtyPosition()
		{
			DirtyPosition = true;
		}

		public void SetDirtyRotation()
		{
			DirtyRotation = true;
		}

		public TransformReference(Transform Transform)
		{
			this.Transform = Transform;
			DirtyPosition = false;
			DirtyRotation = false;
			DirtyEmissionColor = false;
			DirtyAlbedoColor = false;
			EmissionColor = UnityEngine.Color.black;
			AlbedoColor = UnityEngine.Color.black;
			allMaterials = null;
			DirtyEnabled = false;
		}
	}

	private Dictionary<string, Coroutine> movementRoutines = new Dictionary<string, Coroutine>();

	private Dictionary<string, Coroutine> rotationRoutines = new Dictionary<string, Coroutine>();

	private Dictionary<string, ServerActiveMovementInfo> serverActiveMovementDictionary = new Dictionary<string, ServerActiveMovementInfo>();

	private Dictionary<string, ServerActiveMovementInfo> serverActiveRotationDictionary = new Dictionary<string, ServerActiveMovementInfo>();

	private Dictionary<SerializableGuid, TransformReference> transformMap;

	private Visuals luaInterface;

	public EndlessScriptComponent scriptComponent;

	[field: SerializeField]
	public Transform VisualRoot { get; private set; }

	public bool VisualsSpawned { get; private set; }

	private Dictionary<SerializableGuid, TransformReference> TransformMap
	{
		get
		{
			if (transformMap == null)
			{
				transformMap = new Dictionary<SerializableGuid, TransformReference>();
				TransformIdentifier[] componentsInChildren = base.transform.parent.GetComponentsInChildren<TransformIdentifier>(includeInactive: true);
				foreach (TransformIdentifier transformIdentifier in componentsInChildren)
				{
					if (!transformMap.TryAdd(transformIdentifier.UniqueId, new TransformReference(transformIdentifier.transform)))
					{
						throw new DuplicateNameException("The prop " + WorldObject.gameObject.name + " has a duplicated Transform ID. This object needs to be fixed via the SDK.");
					}
				}
			}
			return transformMap;
		}
	}

	public Type LuaObjectType => typeof(Visuals);

	public object LuaObject
	{
		get
		{
			if (luaInterface == null)
			{
				luaInterface = new Visuals(this);
			}
			return luaInterface;
		}
	}

	[field: SerializeField]
	[field: HideInInspector]
	public WorldObject WorldObject { get; private set; }

	private void OnValidate()
	{
		if (VisualRoot == null)
		{
			VisualRoot = base.transform;
		}
	}

	public void SetPositionData(Context instigator, string transformId, UnityEngine.Vector3 goal, string callbackName = null)
	{
		SetPositionData(instigator, transformId, goal, -1f, callbackName);
	}

	public void SetPositionData(Context instigator, string transformId, UnityEngine.Vector3 goal, float duration, string callbackName = null)
	{
		SetPositionData(instigator, transformId, TransformMap[transformId].Transform.localPosition, goal, duration, callbackName);
	}

	public void SetPositionData(Context instigator, string transformId, UnityEngine.Vector3 start, UnityEngine.Vector3 goal, float duration, string callbackName = null)
	{
		TransformMap[transformId].SetDirtyPosition();
		serverActiveMovementDictionary[transformId] = new ServerActiveMovementInfo(NetworkManager.Singleton.NetworkTimeSystem.ServerTime + (double)duration, UnityEngine.Vector3.zero, goal, continuous: false);
		StartMovementRoutine(transformId, (float)NetworkManager.Singleton.NetworkTimeSystem.ServerTime, duration, start, goal, instigator, callbackName);
		StartMovementRoutine_ClientRpc(transformId, (float)NetworkManager.Singleton.NetworkTimeSystem.ServerTime, duration, start, goal, callbackName);
	}

	public void SetRotationData(string transformId, UnityEngine.Vector3 goal, Context instigator = null, string callbackName = null)
	{
		SetRotationData(transformId, goal, -1f, instigator, callbackName);
	}

	public void SetRotationData(string transformId, UnityEngine.Vector3 goal, float duration, Context instigator = null, string callbackName = null)
	{
		SetRotationData(transformId, TransformMap[transformId].Transform.localEulerAngles, goal, duration, instigator, callbackName);
	}

	public void SetContinuousRotationData(string transformId, UnityEngine.Vector3 rate, Context instigator)
	{
		if (rate.sqrMagnitude > 0f)
		{
			SetRotationData(transformId, TransformMap[transformId].Transform.localEulerAngles, rate, 0f, instigator, null, continuous: true);
		}
		else if (serverActiveRotationDictionary.ContainsKey(transformId) && serverActiveRotationDictionary[transformId].Continuous)
		{
			serverActiveRotationDictionary.Remove(transformId);
			if (rotationRoutines.ContainsKey(transformId))
			{
				StopCoroutine(rotationRoutines[transformId]);
				rotationRoutines.Remove(transformId);
			}
		}
	}

	public void SetRotationData(string transformId, UnityEngine.Vector3 start, UnityEngine.Vector3 goal, float duration, Context instigator = null, string callbackName = null, bool continuous = false)
	{
		TransformMap[transformId].SetDirtyRotation();
		serverActiveRotationDictionary[transformId] = new ServerActiveMovementInfo(NetworkManager.Singleton.NetworkTimeSystem.ServerTime + (double)duration, start, goal, continuous);
		StartRotationRoutine(transformId, (float)NetworkManager.Singleton.NetworkTimeSystem.ServerTime, duration, start, goal, instigator, callbackName, continuous);
		StartRotationRoutine_ClientRpc(transformId, (float)NetworkManager.Singleton.NetworkTimeSystem.ServerTime, duration, start, goal, callbackName, continuous);
	}

	public void SetEmissionColor(string transformId, UnityEngine.Color color)
	{
		if (TransformMap.TryGetValue(transformId, out var value))
		{
			value.SetEmissionColor(color);
			SetEmissionColor_ClientRpc(transformId, color);
		}
	}

	public void SetAlbedoColor(string transformId, UnityEngine.Color color)
	{
		if (TransformMap.TryGetValue(transformId, out var value))
		{
			value.SetAlbedoColor(color);
			SetAlbedoColor_ClientRpc(transformId, color);
		}
	}

	public void SetEnabled(string transformId, bool enabled)
	{
		if (TransformMap.TryGetValue(transformId, out var value))
		{
			value.SetEnabled(enabled);
			SetEnabled_ClientRpc(transformId, enabled);
		}
	}

	protected override void OnSynchronize<T>(ref BufferSerializer<T> serializer)
	{
		base.OnSynchronize(ref serializer);
		if (serializer.IsWriter)
		{
			Dictionary<SerializableGuid, TransformReference> dictionary = TransformMap.Where((KeyValuePair<SerializableGuid, TransformReference> i) => i.Value.HasDirtyProperties).ToDictionary((KeyValuePair<SerializableGuid, TransformReference> i) => i.Key, (KeyValuePair<SerializableGuid, TransformReference> i) => i.Value);
			Compression.SerializeInt(serializer, dictionary.Count);
			{
				foreach (KeyValuePair<SerializableGuid, TransformReference> item in dictionary)
				{
					SerializableGuid value = item.Key;
					TransformReference value2 = item.Value;
					serializer.SerializeValue(ref value, default(FastBufferWriter.ForNetworkSerializable));
					Compression.SerializeBoolsToByte(serializer, value2.DirtyPosition, value2.DirtyRotation, value2.DirtyEmissionColor, value2.DirtyAlbedoColor, value2.DirtyEnabled);
					if (value2.DirtyPosition)
					{
						UnityEngine.Vector3 value3 = item.Value.Transform.localPosition;
						serializer.SerializeValue(ref value3);
						ServerActiveMovementInfo value5;
						bool value4 = serverActiveMovementDictionary.TryGetValue(value, out value5);
						serializer.SerializeValue(ref value4, default(FastBufferWriter.ForPrimitives));
						if (value4)
						{
							serializer.SerializeValue(ref value5.Goal);
							serializer.SerializeValue(ref value5.EndTime, default(FastBufferWriter.ForPrimitives));
							serializer.SerializeValue(ref value5.Continuous, default(FastBufferWriter.ForPrimitives));
						}
					}
					if (value2.DirtyRotation)
					{
						UnityEngine.Vector3 value6 = item.Value.Transform.localRotation.eulerAngles;
						serializer.SerializeValue(ref value6);
						ServerActiveMovementInfo value8;
						bool value7 = serverActiveRotationDictionary.TryGetValue(value, out value8);
						serializer.SerializeValue(ref value7, default(FastBufferWriter.ForPrimitives));
						if (value7)
						{
							serializer.SerializeValue(ref value8.Start);
							serializer.SerializeValue(ref value8.Goal);
							serializer.SerializeValue(ref value8.EndTime, default(FastBufferWriter.ForPrimitives));
							serializer.SerializeValue(ref value8.Continuous, default(FastBufferWriter.ForPrimitives));
						}
					}
					if (value2.DirtyEmissionColor)
					{
						serializer.SerializeValue(ref value2.EmissionColor);
					}
					if (value2.DirtyAlbedoColor)
					{
						serializer.SerializeValue(ref value2.AlbedoColor);
					}
					if (value2.DirtyEnabled)
					{
						serializer.SerializeValue(ref value2.Enabled, default(FastBufferWriter.ForPrimitives));
					}
				}
				return;
			}
		}
		int num = Compression.DeserializeInt(serializer);
		for (int num2 = 0; num2 < num; num2++)
		{
			SerializableGuid value9 = SerializableGuid.Empty;
			UnityEngine.Vector3 value10 = UnityEngine.Vector3.zero;
			UnityEngine.Vector3 value11 = UnityEngine.Vector3.zero;
			bool value12 = false;
			bool value13 = false;
			serializer.SerializeValue(ref value9, default(FastBufferWriter.ForNetworkSerializable));
			bool b = false;
			bool b2 = false;
			bool b3 = false;
			bool b4 = false;
			bool b5 = false;
			Compression.DeserializeBoolsFromByte(serializer, ref b, ref b2, ref b3, ref b4, ref b5);
			if (b)
			{
				serializer.SerializeValue(ref value10);
				serializer.SerializeValue(ref value12, default(FastBufferWriter.ForPrimitives));
				if (TransformMap.TryGetValue(value9, out var value14))
				{
					value14.Transform.localRotation = Quaternion.Euler(value11);
					if (value12)
					{
						UnityEngine.Vector3 value15 = UnityEngine.Vector3.zero;
						double value16 = 0.0;
						serializer.SerializeValue(ref value15);
						serializer.SerializeValue(ref value16, default(FastBufferWriter.ForPrimitives));
						StartMovementRoutine(value9, (float)NetworkManager.Singleton.NetworkTimeSystem.ServerTime, (float)(value16 - NetworkManager.Singleton.NetworkTimeSystem.ServerTime), value10, value15);
					}
				}
				else
				{
					Debug.LogWarning("Client transform mismatch..");
				}
			}
			if (b2)
			{
				serializer.SerializeValue(ref value11);
				serializer.SerializeValue(ref value13, default(FastBufferWriter.ForPrimitives));
				if (TransformMap.TryGetValue(value9, out var value17))
				{
					value17.Transform.localRotation = Quaternion.Euler(value11);
					if (value13)
					{
						UnityEngine.Vector3 value18 = UnityEngine.Vector3.zero;
						UnityEngine.Vector3 value19 = UnityEngine.Vector3.zero;
						double value20 = 0.0;
						bool value21 = false;
						serializer.SerializeValue(ref value18);
						serializer.SerializeValue(ref value19);
						serializer.SerializeValue(ref value20, default(FastBufferWriter.ForPrimitives));
						serializer.SerializeValue(ref value21, default(FastBufferWriter.ForPrimitives));
						if (value21)
						{
							StartRotationRoutine(value9, (float)value20, 0f, value18, value19, null, null, value21);
						}
						else
						{
							StartRotationRoutine(value9, (float)NetworkManager.Singleton.NetworkTimeSystem.ServerTime, (float)(value20 - NetworkManager.Singleton.NetworkTimeSystem.ServerTime), value11, value19, null, null, value21);
						}
					}
				}
				else
				{
					Debug.LogWarning("Client transform mismatch..");
				}
			}
			if (b3)
			{
				serializer.SerializeValue(ref TransformMap[value9].EmissionColor);
				TransformMap[value9].SetEmissionColor(TransformMap[value9].EmissionColor);
			}
			if (b4)
			{
				serializer.SerializeValue(ref TransformMap[value9].AlbedoColor);
				TransformMap[value9].SetAlbedoColor(TransformMap[value9].AlbedoColor);
			}
			if (b5)
			{
				serializer.SerializeValue(ref TransformMap[value9].Enabled, default(FastBufferWriter.ForPrimitives));
				TransformMap[value9].SetEnabled(TransformMap[value9].Enabled);
			}
		}
	}

	[ClientRpc]
	private void StartMovementRoutine_ClientRpc(string transformId, float startTime, float duration, UnityEngine.Vector3 movementStart, UnityEngine.Vector3 movementGoal, string callbackName = null)
	{
		NetworkManager networkManager = base.NetworkManager;
		if ((object)networkManager == null || !networkManager.IsListening)
		{
			return;
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
		{
			ClientRpcParams clientRpcParams = default(ClientRpcParams);
			FastBufferWriter bufferWriter = __beginSendClientRpc(548630341u, clientRpcParams, RpcDelivery.Reliable);
			bool value = transformId != null;
			bufferWriter.WriteValueSafe(in value, default(FastBufferWriter.ForPrimitives));
			if (value)
			{
				bufferWriter.WriteValueSafe(transformId);
			}
			bufferWriter.WriteValueSafe(in startTime, default(FastBufferWriter.ForPrimitives));
			bufferWriter.WriteValueSafe(in duration, default(FastBufferWriter.ForPrimitives));
			bufferWriter.WriteValueSafe(in movementStart);
			bufferWriter.WriteValueSafe(in movementGoal);
			bool value2 = callbackName != null;
			bufferWriter.WriteValueSafe(in value2, default(FastBufferWriter.ForPrimitives));
			if (value2)
			{
				bufferWriter.WriteValueSafe(callbackName);
			}
			__endSendClientRpc(ref bufferWriter, 548630341u, clientRpcParams, RpcDelivery.Reliable);
		}
		if (__rpc_exec_stage == __RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
		{
			__rpc_exec_stage = __RpcExecStage.Send;
			if (!base.IsServer)
			{
				StartMovementRoutine(transformId, startTime, duration, movementStart, movementGoal, null, callbackName);
			}
		}
	}

	[ClientRpc]
	private void StartRotationRoutine_ClientRpc(string transformId, float startTime, float duration, UnityEngine.Vector3 rotationStart, UnityEngine.Vector3 rotationGoal, string callbackName = null, bool continuous = false)
	{
		NetworkManager networkManager = base.NetworkManager;
		if ((object)networkManager == null || !networkManager.IsListening)
		{
			return;
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
		{
			ClientRpcParams clientRpcParams = default(ClientRpcParams);
			FastBufferWriter bufferWriter = __beginSendClientRpc(2523749201u, clientRpcParams, RpcDelivery.Reliable);
			bool value = transformId != null;
			bufferWriter.WriteValueSafe(in value, default(FastBufferWriter.ForPrimitives));
			if (value)
			{
				bufferWriter.WriteValueSafe(transformId);
			}
			bufferWriter.WriteValueSafe(in startTime, default(FastBufferWriter.ForPrimitives));
			bufferWriter.WriteValueSafe(in duration, default(FastBufferWriter.ForPrimitives));
			bufferWriter.WriteValueSafe(in rotationStart);
			bufferWriter.WriteValueSafe(in rotationGoal);
			bool value2 = callbackName != null;
			bufferWriter.WriteValueSafe(in value2, default(FastBufferWriter.ForPrimitives));
			if (value2)
			{
				bufferWriter.WriteValueSafe(callbackName);
			}
			bufferWriter.WriteValueSafe(in continuous, default(FastBufferWriter.ForPrimitives));
			__endSendClientRpc(ref bufferWriter, 2523749201u, clientRpcParams, RpcDelivery.Reliable);
		}
		if (__rpc_exec_stage == __RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
		{
			__rpc_exec_stage = __RpcExecStage.Send;
			if (!base.IsServer)
			{
				StartRotationRoutine(transformId, startTime, duration, rotationStart, rotationGoal, null, callbackName, continuous);
			}
		}
	}

	[ClientRpc]
	private void SetEmissionColor_ClientRpc(string transformId, UnityEngine.Color color)
	{
		NetworkManager networkManager = base.NetworkManager;
		if ((object)networkManager == null || !networkManager.IsListening)
		{
			return;
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
		{
			ClientRpcParams clientRpcParams = default(ClientRpcParams);
			FastBufferWriter bufferWriter = __beginSendClientRpc(3273904729u, clientRpcParams, RpcDelivery.Reliable);
			bool value = transformId != null;
			bufferWriter.WriteValueSafe(in value, default(FastBufferWriter.ForPrimitives));
			if (value)
			{
				bufferWriter.WriteValueSafe(transformId);
			}
			bufferWriter.WriteValueSafe(in color);
			__endSendClientRpc(ref bufferWriter, 3273904729u, clientRpcParams, RpcDelivery.Reliable);
		}
		if (__rpc_exec_stage == __RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
		{
			__rpc_exec_stage = __RpcExecStage.Send;
			if (!base.IsServer && TransformMap.TryGetValue(transformId, out var value2))
			{
				value2.SetEmissionColor(color);
			}
		}
	}

	[ClientRpc]
	private void SetAlbedoColor_ClientRpc(string transformId, UnityEngine.Color color)
	{
		NetworkManager networkManager = base.NetworkManager;
		if ((object)networkManager == null || !networkManager.IsListening)
		{
			return;
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
		{
			ClientRpcParams clientRpcParams = default(ClientRpcParams);
			FastBufferWriter bufferWriter = __beginSendClientRpc(1187108704u, clientRpcParams, RpcDelivery.Reliable);
			bool value = transformId != null;
			bufferWriter.WriteValueSafe(in value, default(FastBufferWriter.ForPrimitives));
			if (value)
			{
				bufferWriter.WriteValueSafe(transformId);
			}
			bufferWriter.WriteValueSafe(in color);
			__endSendClientRpc(ref bufferWriter, 1187108704u, clientRpcParams, RpcDelivery.Reliable);
		}
		if (__rpc_exec_stage == __RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
		{
			__rpc_exec_stage = __RpcExecStage.Send;
			if (!base.IsServer && TransformMap.TryGetValue(transformId, out var value2))
			{
				value2.SetAlbedoColor(color);
			}
		}
	}

	[ClientRpc]
	private void SetEnabled_ClientRpc(string transformId, bool enabled)
	{
		NetworkManager networkManager = base.NetworkManager;
		if ((object)networkManager == null || !networkManager.IsListening)
		{
			return;
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
		{
			ClientRpcParams clientRpcParams = default(ClientRpcParams);
			FastBufferWriter bufferWriter = __beginSendClientRpc(2662578777u, clientRpcParams, RpcDelivery.Reliable);
			bool value = transformId != null;
			bufferWriter.WriteValueSafe(in value, default(FastBufferWriter.ForPrimitives));
			if (value)
			{
				bufferWriter.WriteValueSafe(transformId);
			}
			bufferWriter.WriteValueSafe(in enabled, default(FastBufferWriter.ForPrimitives));
			__endSendClientRpc(ref bufferWriter, 2662578777u, clientRpcParams, RpcDelivery.Reliable);
		}
		if (__rpc_exec_stage == __RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
		{
			__rpc_exec_stage = __RpcExecStage.Send;
			if (!base.IsServer && TransformMap.TryGetValue(transformId, out var value2))
			{
				value2.SetEnabled(enabled);
			}
		}
	}

	private void StartMovementRoutine(string transformId, float startTime, float duration, UnityEngine.Vector3 movementStart, UnityEngine.Vector3 movementGoal, Context instigator = null, string callbackName = null)
	{
		if (movementRoutines.ContainsKey(transformId))
		{
			StopCoroutine(movementRoutines[transformId]);
			movementRoutines.Remove(transformId);
		}
		movementRoutines.Add(transformId, StartCoroutine(ApplyMovement(transformId, startTime, duration, movementStart, movementGoal, instigator, callbackName)));
	}

	private IEnumerator ApplyMovement(string transformId, float startTime, float duration, UnityEngine.Vector3 movementStart, UnityEngine.Vector3 movementGoal, Context instigator = null, string callbackName = null)
	{
		Transform target = GetTransform(transformId);
		float num = Vector3Extensions.InverseLerp(movementStart, movementGoal, target.localPosition);
		movementStart = target.localPosition;
		float num2 = (1f - num) * duration;
		float endTime = startTime + num2;
		MovingCollider[] movingColliders = target.GetComponentsInChildren<MovingCollider>();
		MovingCollider[] array;
		while ((double)endTime > NetworkManager.Singleton.NetworkTimeSystem.ServerTime)
		{
			float t = Mathf.InverseLerp(startTime, endTime, (float)NetworkManager.Singleton.NetworkTimeSystem.ServerTime);
			target.localPosition = UnityEngine.Vector3.Lerp(movementStart, movementGoal, t);
			array = movingColliders;
			foreach (MovingCollider movingCollider in array)
			{
				MonoBehaviourSingleton<MovingColliderManager>.Instance.ColliderMoved(movingCollider);
			}
			yield return null;
		}
		yield return null;
		target.localPosition = movementGoal;
		array = movingColliders;
		foreach (MovingCollider movingCollider2 in array)
		{
			MonoBehaviourSingleton<MovingColliderManager>.Instance.ColliderMoved(movingCollider2);
		}
		if (instigator != null && callbackName != null)
		{
			scriptComponent.TryExecuteFunction(callbackName, out var _, instigator, transformId);
		}
		if (base.IsServer)
		{
			serverActiveMovementDictionary.Remove(transformId);
		}
		movementRoutines.Remove(transformId);
	}

	private void StartRotationRoutine(string transformId, float startTime, float duration, UnityEngine.Vector3 rotationStart, UnityEngine.Vector3 rotationGoal, Context instigator = null, string callbackName = null, bool continuous = false)
	{
		if (rotationRoutines.ContainsKey(transformId))
		{
			StopCoroutine(rotationRoutines[transformId]);
			rotationRoutines.Remove(transformId);
		}
		rotationRoutines.Add(transformId, StartCoroutine(ApplyRotation(transformId, startTime, duration, rotationStart, rotationGoal, instigator, callbackName, continuous)));
	}

	private IEnumerator ApplyRotation(string transformId, float startTime, float duration, UnityEngine.Vector3 rotationStart, UnityEngine.Vector3 rotationGoal, Context instigator = null, string callbackName = null, bool continuous = false)
	{
		float endTime = 0f;
		UnityEngine.Vector3 rotAxis = UnityEngine.Vector3.zero;
		float rotRate = 0f;
		Quaternion rotStart = Quaternion.identity;
		Transform target = GetTransform(transformId);
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
		MovingCollider[] array;
		while ((double)endTime > NetworkManager.Singleton.NetworkTimeSystem.ServerTime || continuous)
		{
			if (continuous)
			{
				double num3 = NetworkManager.Singleton.NetworkTimeSystem.ServerTime - (double)startTime;
				float angle = rotRate * (float)num3 % 360f;
				target.localRotation = rotStart * Quaternion.AngleAxis(angle, rotAxis);
			}
			else
			{
				float t = Mathf.InverseLerp(startTime, endTime, (float)NetworkManager.Singleton.NetworkTimeSystem.ServerTime);
				target.localRotation = Quaternion.Slerp(Quaternion.Euler(rotationStart), Quaternion.Euler(rotationGoal), t);
			}
			array = movingColliders;
			foreach (MovingCollider movingCollider in array)
			{
				MonoBehaviourSingleton<MovingColliderManager>.Instance.ColliderMoved(movingCollider);
			}
			yield return null;
		}
		yield return null;
		target.localRotation = Quaternion.Euler(rotationGoal);
		if (instigator != null && callbackName != null)
		{
			scriptComponent.TryExecuteFunction(callbackName, out var _, instigator, transformId);
		}
		if (base.IsServer)
		{
			serverActiveRotationDictionary.Remove(transformId);
		}
		array = movingColliders;
		foreach (MovingCollider movingCollider2 in array)
		{
			MonoBehaviourSingleton<MovingColliderManager>.Instance.ColliderMoved(movingCollider2);
		}
		rotationRoutines.Remove(transformId);
	}

	private Transform GetTransform(string transformId)
	{
		return TransformMap[transformId].Transform;
	}

	public void ScriptInitialize(EndlessScriptComponent endlessScriptComponent)
	{
		scriptComponent = endlessScriptComponent;
	}

	public void PrefabInitialize(WorldObject worldObject)
	{
		WorldObject = worldObject;
		Collider[] componentsInChildren = worldObject.GetComponentsInChildren<Collider>();
		foreach (Collider collider in componentsInChildren)
		{
			collider.gameObject.AddComponent<MovingCollider>().Init(collider);
		}
	}

	protected override void __initializeVariables()
	{
		base.__initializeVariables();
	}

	protected override void __initializeRpcs()
	{
		__registerRpc(548630341u, __rpc_handler_548630341, "StartMovementRoutine_ClientRpc");
		__registerRpc(2523749201u, __rpc_handler_2523749201, "StartRotationRoutine_ClientRpc");
		__registerRpc(3273904729u, __rpc_handler_3273904729, "SetEmissionColor_ClientRpc");
		__registerRpc(1187108704u, __rpc_handler_1187108704, "SetAlbedoColor_ClientRpc");
		__registerRpc(2662578777u, __rpc_handler_2662578777, "SetEnabled_ClientRpc");
		base.__initializeRpcs();
	}

	private static void __rpc_handler_548630341(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			reader.ReadValueSafe(out bool value, default(FastBufferWriter.ForPrimitives));
			string s = null;
			if (value)
			{
				reader.ReadValueSafe(out s, oneByteChars: false);
			}
			reader.ReadValueSafe(out float value2, default(FastBufferWriter.ForPrimitives));
			reader.ReadValueSafe(out float value3, default(FastBufferWriter.ForPrimitives));
			reader.ReadValueSafe(out UnityEngine.Vector3 value4);
			reader.ReadValueSafe(out UnityEngine.Vector3 value5);
			reader.ReadValueSafe(out bool value6, default(FastBufferWriter.ForPrimitives));
			string s2 = null;
			if (value6)
			{
				reader.ReadValueSafe(out s2, oneByteChars: false);
			}
			target.__rpc_exec_stage = __RpcExecStage.Execute;
			((DynamicVisuals)target).StartMovementRoutine_ClientRpc(s, value2, value3, value4, value5, s2);
			target.__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	private static void __rpc_handler_2523749201(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			reader.ReadValueSafe(out bool value, default(FastBufferWriter.ForPrimitives));
			string s = null;
			if (value)
			{
				reader.ReadValueSafe(out s, oneByteChars: false);
			}
			reader.ReadValueSafe(out float value2, default(FastBufferWriter.ForPrimitives));
			reader.ReadValueSafe(out float value3, default(FastBufferWriter.ForPrimitives));
			reader.ReadValueSafe(out UnityEngine.Vector3 value4);
			reader.ReadValueSafe(out UnityEngine.Vector3 value5);
			reader.ReadValueSafe(out bool value6, default(FastBufferWriter.ForPrimitives));
			string s2 = null;
			if (value6)
			{
				reader.ReadValueSafe(out s2, oneByteChars: false);
			}
			reader.ReadValueSafe(out bool value7, default(FastBufferWriter.ForPrimitives));
			target.__rpc_exec_stage = __RpcExecStage.Execute;
			((DynamicVisuals)target).StartRotationRoutine_ClientRpc(s, value2, value3, value4, value5, s2, value7);
			target.__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	private static void __rpc_handler_3273904729(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			reader.ReadValueSafe(out bool value, default(FastBufferWriter.ForPrimitives));
			string s = null;
			if (value)
			{
				reader.ReadValueSafe(out s, oneByteChars: false);
			}
			reader.ReadValueSafe(out UnityEngine.Color value2);
			target.__rpc_exec_stage = __RpcExecStage.Execute;
			((DynamicVisuals)target).SetEmissionColor_ClientRpc(s, value2);
			target.__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	private static void __rpc_handler_1187108704(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			reader.ReadValueSafe(out bool value, default(FastBufferWriter.ForPrimitives));
			string s = null;
			if (value)
			{
				reader.ReadValueSafe(out s, oneByteChars: false);
			}
			reader.ReadValueSafe(out UnityEngine.Color value2);
			target.__rpc_exec_stage = __RpcExecStage.Execute;
			((DynamicVisuals)target).SetAlbedoColor_ClientRpc(s, value2);
			target.__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	private static void __rpc_handler_2662578777(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			reader.ReadValueSafe(out bool value, default(FastBufferWriter.ForPrimitives));
			string s = null;
			if (value)
			{
				reader.ReadValueSafe(out s, oneByteChars: false);
			}
			reader.ReadValueSafe(out bool value2, default(FastBufferWriter.ForPrimitives));
			target.__rpc_exec_stage = __RpcExecStage.Execute;
			((DynamicVisuals)target).SetEnabled_ClientRpc(s, value2);
			target.__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	protected internal override string __getTypeName()
	{
		return "DynamicVisuals";
	}
}
