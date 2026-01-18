using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Shared;
using Endless.Shared.DataTypes;
using UnityEngine;

namespace Endless.Gameplay.LevelEditing.Wires;

[RequireComponent(typeof(LineRenderer), typeof(MeshCollider))]
public class PhysicalWire : MonoBehaviour
{
	private enum PhysicalWireState
	{
		Live,
		Placed,
		Editing
	}

	[SerializeField]
	private float pointsPerMeter = 0.5f;

	[SerializeField]
	private int basePointsPerSegment = 10;

	[SerializeField]
	private Vector2Int pointsMinMax = new Vector2Int(3, 50);

	[SerializeField]
	private Transform emitterBulb;

	[SerializeField]
	private Transform receiverBulb;

	[SerializeField]
	private Material liveMaterial;

	[SerializeField]
	private Material placedMaterial;

	[SerializeField]
	private Material editingMaterial;

	[SerializeField]
	private Material hoveredMaterial;

	[SerializeField]
	private float bulbTravelTime = 0.2f;

	private PhysicalWireState physicalWireState;

	private Transform emitter;

	private Transform receiver;

	private LineRenderer wireRenderer;

	private MeshCollider meshCollider;

	private List<SerializableGuid> rerouteNodeIds = new List<SerializableGuid>();

	private bool connectedToPlayer = true;

	private Vector3 initialEmitterPosition;

	private Vector3 initialReceiverPosition;

	private Vector3 currentEmitterPosition;

	private Vector3 currentReceiverPosition;

	public SerializableGuid BundleId { get; private set; }

	public SerializableGuid EmitterId { get; private set; }

	public SerializableGuid ReceiverId { get; private set; }

	public List<SerializableGuid> RerouteNodeIds => rerouteNodeIds ?? new List<SerializableGuid>();

	public bool IsPlaced => physicalWireState == PhysicalWireState.Placed;

	public bool IsLive => physicalWireState == PhysicalWireState.Live;

	public bool IsBeingEdited => physicalWireState == PhysicalWireState.Editing;

	private void Awake()
	{
		wireRenderer = GetComponent<LineRenderer>();
		meshCollider = GetComponent<MeshCollider>();
		Mesh sharedMesh = new Mesh();
		meshCollider.sharedMesh = sharedMesh;
	}

	private void OnEnable()
	{
		ToggleBulbs(active: true);
	}

	private void OnDisable()
	{
		ToggleBulbs(active: false);
	}

	private void ToggleBulbs(bool active)
	{
		if ((bool)emitterBulb)
		{
			emitterBulb.gameObject.SetActive(active);
		}
		if ((bool)receiverBulb)
		{
			receiverBulb.gameObject.SetActive(active);
		}
	}

	public void SetupLiveMode(SerializableGuid source, Transform sourceTransform, Vector3 initialEmitterPosition, Transform playerTransform, bool travelImmediately)
	{
		physicalWireState = PhysicalWireState.Live;
		EmitterId = source;
		emitter = sourceTransform;
		receiver = playerTransform;
		wireRenderer.material = liveMaterial;
		this.initialEmitterPosition = initialEmitterPosition;
		if (travelImmediately)
		{
			StartCoroutine(MoveEmitter());
		}
		else
		{
			currentEmitterPosition = emitter.position;
		}
		connectedToPlayer = true;
		GenerateLineRendererPositions();
	}

	public void ResetToLive()
	{
		physicalWireState = PhysicalWireState.Live;
		currentEmitterPosition = emitter.position;
		currentReceiverPosition = receiver.position;
		wireRenderer.material = liveMaterial;
		GenerateLineRendererPositions();
	}

	private IEnumerator MoveEmitter()
	{
		float timeTraveled = 0f;
		while (timeTraveled < bulbTravelTime)
		{
			float num = timeTraveled / bulbTravelTime;
			currentEmitterPosition = Vector3.Lerp(initialEmitterPosition, emitter.position, num * num);
			timeTraveled += Time.deltaTime;
			yield return new WaitForEndOfFrame();
		}
		currentEmitterPosition = emitter.position;
		emitterBulb.transform.SetParent(emitter, worldPositionStays: true);
	}

	public void Setup(SerializableGuid bundleId, SerializableGuid emitterId, Transform emitterTransform, SerializableGuid receiverId, Transform receiverTransform, IEnumerable<SerializableGuid> rerouteNodes, Color color)
	{
		BundleId = bundleId;
		physicalWireState = PhysicalWireState.Placed;
		wireRenderer.startColor = color;
		wireRenderer.endColor = color;
		BundleId = bundleId;
		EmitterId = emitterId;
		emitter = emitterTransform;
		ReceiverId = receiverId;
		receiver = receiverTransform;
		currentEmitterPosition = emitter.position;
		currentReceiverPosition = receiver.position;
		emitterBulb.transform.SetParent(emitter, worldPositionStays: true);
		receiverBulb.transform.SetParent(receiver, worldPositionStays: true);
		rerouteNodeIds = rerouteNodes.ToList();
		GenerateLineRendererPositions();
	}

	public void ConvertLiveToPlaced(SerializableGuid bundleId, SerializableGuid emitterId, SerializableGuid receiverId, IEnumerable<SerializableGuid> rerouteNodes, Color color)
	{
		BundleId = bundleId;
		physicalWireState = PhysicalWireState.Placed;
		wireRenderer.startColor = color;
		wireRenderer.endColor = color;
		EmitterId = emitterId;
		ReceiverId = receiverId;
		wireRenderer.material = placedMaterial;
		rerouteNodeIds = rerouteNodes.ToList();
		GenerateLineRendererPositions();
	}

	public void GenerateLineRendererPositions()
	{
		if (!wireRenderer)
		{
			Debug.LogWarning("Attempting to generate line renderer positions for a wire renderer that doesnt' exist!");
			return;
		}
		if (!MonoBehaviourSingleton<StageManager>.Instance)
		{
			Debug.LogWarning("Attempted to generate line renderer positions without a StageManager Instance.");
			return;
		}
		if (!MonoBehaviourSingleton<StageManager>.Instance.ActiveStage)
		{
			Debug.LogWarning("Attempted to generate line renderer positions without an Active Stage.");
			return;
		}
		List<Vector3> list = new List<Vector3>();
		list.Add(currentEmitterPosition);
		for (int i = 0; i < rerouteNodeIds.Count; i++)
		{
			list.Add(MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetGameObjectFromInstanceId(rerouteNodeIds[i]).transform.position);
		}
		if (connectedToPlayer)
		{
			list.Add(receiver.position);
		}
		else
		{
			list.Add(currentReceiverPosition);
		}
		wireRenderer.positionCount = 0;
		int num = 0;
		System.Random random = new System.Random(BitConverter.ToInt32(EmitterId.Guid.ToByteArray()) + BitConverter.ToInt32(ReceiverId.Guid.ToByteArray()));
		Vector3 vector = Vector3.zero;
		for (int j = 0; j + 1 < list.Count; j++)
		{
			float num2 = (float)random.Next() * 0.25f + 0.25f;
			Vector3 vector2 = list[j + 1] - list[j];
			Vector3 normalized = vector2.normalized;
			float magnitude = vector2.magnitude;
			float num3 = Mathf.Min(magnitude * num2, 2f);
			Vector3 vector3 = list[j] + magnitude * 0.5f * normalized;
			Vector3 vector4 = Vector3.Cross(vector2.normalized, Vector3.up) * num3;
			vector4 = ((random.Next(0, 2) % 2 == 0) ? (-vector4) : vector4);
			Vector3 vector5 = Vector3.up * Mathf.Min(magnitude * num2 * 1.25f, 2f);
			Vector3 vector6 = list[j];
			Vector3 vector7 = list[j + 1];
			vector = vector3 + vector4 + vector5;
			int value = basePointsPerSegment + Mathf.CeilToInt(Vector3.Distance(vector6, vector7) * pointsPerMeter);
			value = Mathf.Clamp(value, pointsMinMax.x, pointsMinMax.y);
			float num4 = (float)value - 1f;
			wireRenderer.positionCount += value;
			for (int k = 0; k < value; k++)
			{
				wireRenderer.SetPosition(k + num, QuadraticLerp(vector6, vector, vector7, (float)k / num4));
			}
			if (j == 0 && (bool)emitterBulb)
			{
				emitterBulb.transform.position = vector6;
				emitterBulb.transform.LookAt(vector);
			}
			num += value;
		}
		if ((bool)receiverBulb)
		{
			receiverBulb.transform.position = receiver.position;
			receiverBulb.transform.LookAt(vector);
		}
	}

	private void FixedUpdate()
	{
		if (physicalWireState == PhysicalWireState.Live || physicalWireState == PhysicalWireState.Editing || (physicalWireState == PhysicalWireState.Placed && (currentEmitterPosition != emitterBulb.position || currentReceiverPosition != receiverBulb.position)))
		{
			if (physicalWireState == PhysicalWireState.Placed)
			{
				currentEmitterPosition = emitterBulb.position;
				currentReceiverPosition = receiverBulb.position;
			}
			GenerateLineRendererPositions();
		}
		else if (wireRenderer.positionCount == 0)
		{
			GenerateLineRendererPositions();
		}
	}

	private Vector3 QuadraticLerp(Vector3 pointZero, Vector3 pointOne, Vector3 pointTwo, float t)
	{
		return (1f - t) * (1f - t) * pointZero + 2f * (1f - t) * t * pointOne + t * t * pointTwo;
	}

	public Vector3 ReceiverTargetPosition()
	{
		return receiver.position;
	}

	public void SetReceiverToPlayer(Transform transform)
	{
		connectedToPlayer = true;
		receiver = transform;
	}

	public void SetTemporaryTarget(Transform transform, SerializableGuid receiverGuid, Vector3 initialPosition, Action callback, bool travelImmediately)
	{
		connectedToPlayer = false;
		ReceiverId = receiverGuid;
		initialReceiverPosition = initialPosition;
		receiver = transform;
		if (travelImmediately)
		{
			StartCoroutine(MoveReceiver(callback));
			return;
		}
		currentReceiverPosition = receiver.position;
		callback();
	}

	private IEnumerator MoveReceiver(Action callback)
	{
		float timeTraveled = 0f;
		while (timeTraveled < bulbTravelTime)
		{
			float num = timeTraveled / bulbTravelTime;
			currentReceiverPosition = Vector3.Lerp(initialReceiverPosition, receiver.position, num * num);
			timeTraveled += Time.deltaTime;
			yield return new WaitForEndOfFrame();
		}
		currentReceiverPosition = receiver.position;
		receiverBulb.transform.SetParent(receiver, worldPositionStays: true);
		callback?.Invoke();
	}

	public void SetEditing()
	{
		wireRenderer.material = editingMaterial;
		physicalWireState = PhysicalWireState.Editing;
	}

	public void SetPlaced()
	{
		wireRenderer.material = placedMaterial;
		physicalWireState = PhysicalWireState.Placed;
	}

	public bool ContainsRerouteNodeId(SerializableGuid instanceId)
	{
		return rerouteNodeIds.Any((SerializableGuid id) => id == instanceId);
	}

	public void Hover()
	{
		if (physicalWireState == PhysicalWireState.Placed)
		{
			wireRenderer.material = hoveredMaterial;
		}
	}

	public void Unhovered()
	{
		if (physicalWireState == PhysicalWireState.Placed)
		{
			wireRenderer.material = placedMaterial;
		}
	}

	private void OnDestroy()
	{
		if ((bool)emitterBulb)
		{
			UnityEngine.Object.Destroy(emitterBulb.gameObject);
		}
		if ((bool)receiverBulb)
		{
			UnityEngine.Object.Destroy(receiverBulb.gameObject);
		}
	}

	public void BakeCollision()
	{
		if (wireRenderer.positionCount > 0)
		{
			Mesh sharedMesh = meshCollider.sharedMesh;
			wireRenderer.BakeMesh(sharedMesh, useTransform: true);
			meshCollider.sharedMesh = sharedMesh;
		}
	}
}
