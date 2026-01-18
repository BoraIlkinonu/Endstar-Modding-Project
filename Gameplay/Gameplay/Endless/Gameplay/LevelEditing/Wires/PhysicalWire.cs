using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Shared;
using Endless.Shared.DataTypes;
using UnityEngine;

namespace Endless.Gameplay.LevelEditing.Wires
{
	// Token: 0x02000505 RID: 1285
	[RequireComponent(typeof(LineRenderer), typeof(MeshCollider))]
	public class PhysicalWire : MonoBehaviour
	{
		// Token: 0x17000602 RID: 1538
		// (get) Token: 0x06001F2F RID: 7983 RVA: 0x0008A0C8 File Offset: 0x000882C8
		// (set) Token: 0x06001F30 RID: 7984 RVA: 0x0008A0D0 File Offset: 0x000882D0
		public SerializableGuid BundleId { get; private set; }

		// Token: 0x17000603 RID: 1539
		// (get) Token: 0x06001F31 RID: 7985 RVA: 0x0008A0D9 File Offset: 0x000882D9
		// (set) Token: 0x06001F32 RID: 7986 RVA: 0x0008A0E1 File Offset: 0x000882E1
		public SerializableGuid EmitterId { get; private set; }

		// Token: 0x17000604 RID: 1540
		// (get) Token: 0x06001F33 RID: 7987 RVA: 0x0008A0EA File Offset: 0x000882EA
		// (set) Token: 0x06001F34 RID: 7988 RVA: 0x0008A0F2 File Offset: 0x000882F2
		public SerializableGuid ReceiverId { get; private set; }

		// Token: 0x17000605 RID: 1541
		// (get) Token: 0x06001F35 RID: 7989 RVA: 0x0008A0FB File Offset: 0x000882FB
		public List<SerializableGuid> RerouteNodeIds
		{
			get
			{
				return this.rerouteNodeIds ?? new List<SerializableGuid>();
			}
		}

		// Token: 0x17000606 RID: 1542
		// (get) Token: 0x06001F36 RID: 7990 RVA: 0x0008A10C File Offset: 0x0008830C
		public bool IsPlaced
		{
			get
			{
				return this.physicalWireState == PhysicalWire.PhysicalWireState.Placed;
			}
		}

		// Token: 0x17000607 RID: 1543
		// (get) Token: 0x06001F37 RID: 7991 RVA: 0x0008A117 File Offset: 0x00088317
		public bool IsLive
		{
			get
			{
				return this.physicalWireState == PhysicalWire.PhysicalWireState.Live;
			}
		}

		// Token: 0x17000608 RID: 1544
		// (get) Token: 0x06001F38 RID: 7992 RVA: 0x0008A122 File Offset: 0x00088322
		public bool IsBeingEdited
		{
			get
			{
				return this.physicalWireState == PhysicalWire.PhysicalWireState.Editing;
			}
		}

		// Token: 0x06001F39 RID: 7993 RVA: 0x0008A130 File Offset: 0x00088330
		private void Awake()
		{
			this.wireRenderer = base.GetComponent<LineRenderer>();
			this.meshCollider = base.GetComponent<MeshCollider>();
			Mesh mesh = new Mesh();
			this.meshCollider.sharedMesh = mesh;
		}

		// Token: 0x06001F3A RID: 7994 RVA: 0x0008A167 File Offset: 0x00088367
		private void OnEnable()
		{
			this.ToggleBulbs(true);
		}

		// Token: 0x06001F3B RID: 7995 RVA: 0x0008A170 File Offset: 0x00088370
		private void OnDisable()
		{
			this.ToggleBulbs(false);
		}

		// Token: 0x06001F3C RID: 7996 RVA: 0x0008A179 File Offset: 0x00088379
		private void ToggleBulbs(bool active)
		{
			if (this.emitterBulb)
			{
				this.emitterBulb.gameObject.SetActive(active);
			}
			if (this.receiverBulb)
			{
				this.receiverBulb.gameObject.SetActive(active);
			}
		}

		// Token: 0x06001F3D RID: 7997 RVA: 0x0008A1B8 File Offset: 0x000883B8
		public void SetupLiveMode(SerializableGuid source, Transform sourceTransform, Vector3 initialEmitterPosition, Transform playerTransform, bool travelImmediately)
		{
			this.physicalWireState = PhysicalWire.PhysicalWireState.Live;
			this.EmitterId = source;
			this.emitter = sourceTransform;
			this.receiver = playerTransform;
			this.wireRenderer.material = this.liveMaterial;
			this.initialEmitterPosition = initialEmitterPosition;
			if (travelImmediately)
			{
				base.StartCoroutine(this.MoveEmitter());
			}
			else
			{
				this.currentEmitterPosition = this.emitter.position;
			}
			this.connectedToPlayer = true;
			this.GenerateLineRendererPositions();
		}

		// Token: 0x06001F3E RID: 7998 RVA: 0x0008A22C File Offset: 0x0008842C
		public void ResetToLive()
		{
			this.physicalWireState = PhysicalWire.PhysicalWireState.Live;
			this.currentEmitterPosition = this.emitter.position;
			this.currentReceiverPosition = this.receiver.position;
			this.wireRenderer.material = this.liveMaterial;
			this.GenerateLineRendererPositions();
		}

		// Token: 0x06001F3F RID: 7999 RVA: 0x0008A279 File Offset: 0x00088479
		private IEnumerator MoveEmitter()
		{
			float timeTraveled = 0f;
			while (timeTraveled < this.bulbTravelTime)
			{
				float num = timeTraveled / this.bulbTravelTime;
				this.currentEmitterPosition = Vector3.Lerp(this.initialEmitterPosition, this.emitter.position, num * num);
				timeTraveled += Time.deltaTime;
				yield return new WaitForEndOfFrame();
			}
			this.currentEmitterPosition = this.emitter.position;
			this.emitterBulb.transform.SetParent(this.emitter, true);
			yield break;
		}

		// Token: 0x06001F40 RID: 8000 RVA: 0x0008A288 File Offset: 0x00088488
		public void Setup(SerializableGuid bundleId, SerializableGuid emitterId, Transform emitterTransform, SerializableGuid receiverId, Transform receiverTransform, IEnumerable<SerializableGuid> rerouteNodes, Color color)
		{
			this.BundleId = bundleId;
			this.physicalWireState = PhysicalWire.PhysicalWireState.Placed;
			this.wireRenderer.startColor = color;
			this.wireRenderer.endColor = color;
			this.BundleId = bundleId;
			this.EmitterId = emitterId;
			this.emitter = emitterTransform;
			this.ReceiverId = receiverId;
			this.receiver = receiverTransform;
			this.currentEmitterPosition = this.emitter.position;
			this.currentReceiverPosition = this.receiver.position;
			this.emitterBulb.transform.SetParent(this.emitter, true);
			this.receiverBulb.transform.SetParent(this.receiver, true);
			this.rerouteNodeIds = rerouteNodes.ToList<SerializableGuid>();
			this.GenerateLineRendererPositions();
		}

		// Token: 0x06001F41 RID: 8001 RVA: 0x0008A348 File Offset: 0x00088548
		public void ConvertLiveToPlaced(SerializableGuid bundleId, SerializableGuid emitterId, SerializableGuid receiverId, IEnumerable<SerializableGuid> rerouteNodes, Color color)
		{
			this.BundleId = bundleId;
			this.physicalWireState = PhysicalWire.PhysicalWireState.Placed;
			this.wireRenderer.startColor = color;
			this.wireRenderer.endColor = color;
			this.EmitterId = emitterId;
			this.ReceiverId = receiverId;
			this.wireRenderer.material = this.placedMaterial;
			this.rerouteNodeIds = rerouteNodes.ToList<SerializableGuid>();
			this.GenerateLineRendererPositions();
		}

		// Token: 0x06001F42 RID: 8002 RVA: 0x0008A3B0 File Offset: 0x000885B0
		public void GenerateLineRendererPositions()
		{
			if (!this.wireRenderer)
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
			list.Add(this.currentEmitterPosition);
			for (int i = 0; i < this.rerouteNodeIds.Count; i++)
			{
				list.Add(MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetGameObjectFromInstanceId(this.rerouteNodeIds[i]).transform.position);
			}
			if (this.connectedToPlayer)
			{
				list.Add(this.receiver.position);
			}
			else
			{
				list.Add(this.currentReceiverPosition);
			}
			this.wireRenderer.positionCount = 0;
			int num = 0;
			SerializableGuid serializableGuid = this.EmitterId;
			int num2 = BitConverter.ToInt32(serializableGuid.Guid.ToByteArray());
			serializableGuid = this.ReceiverId;
			global::System.Random random = new global::System.Random(num2 + BitConverter.ToInt32(serializableGuid.Guid.ToByteArray()));
			Vector3 vector = Vector3.zero;
			int num3 = 0;
			while (num3 + 1 < list.Count)
			{
				float num4 = (float)random.Next() * 0.25f + 0.25f;
				Vector3 vector2 = list[num3 + 1] - list[num3];
				Vector3 normalized = vector2.normalized;
				float magnitude = vector2.magnitude;
				float num5 = Mathf.Min(magnitude * num4, 2f);
				Vector3 vector3 = list[num3] + magnitude * 0.5f * normalized;
				Vector3 vector4 = Vector3.Cross(vector2.normalized, Vector3.up) * num5;
				vector4 = ((random.Next(0, 2) % 2 == 0) ? (-vector4) : vector4);
				Vector3 vector5 = Vector3.up * Mathf.Min(magnitude * num4 * 1.25f, 2f);
				Vector3 vector6 = list[num3];
				Vector3 vector7 = list[num3 + 1];
				vector = vector3 + vector4 + vector5;
				int num6 = this.basePointsPerSegment + Mathf.CeilToInt(Vector3.Distance(vector6, vector7) * this.pointsPerMeter);
				num6 = Mathf.Clamp(num6, this.pointsMinMax.x, this.pointsMinMax.y);
				float num7 = (float)num6 - 1f;
				this.wireRenderer.positionCount += num6;
				for (int j = 0; j < num6; j++)
				{
					this.wireRenderer.SetPosition(j + num, this.QuadraticLerp(vector6, vector, vector7, (float)j / num7));
				}
				if (num3 == 0 && this.emitterBulb)
				{
					this.emitterBulb.transform.position = vector6;
					this.emitterBulb.transform.LookAt(vector);
				}
				num += num6;
				num3++;
			}
			if (this.receiverBulb)
			{
				this.receiverBulb.transform.position = this.receiver.position;
				this.receiverBulb.transform.LookAt(vector);
			}
		}

		// Token: 0x06001F43 RID: 8003 RVA: 0x0008A6E0 File Offset: 0x000888E0
		private void FixedUpdate()
		{
			if (this.physicalWireState == PhysicalWire.PhysicalWireState.Live || this.physicalWireState == PhysicalWire.PhysicalWireState.Editing || (this.physicalWireState == PhysicalWire.PhysicalWireState.Placed && (this.currentEmitterPosition != this.emitterBulb.position || this.currentReceiverPosition != this.receiverBulb.position)))
			{
				if (this.physicalWireState == PhysicalWire.PhysicalWireState.Placed)
				{
					this.currentEmitterPosition = this.emitterBulb.position;
					this.currentReceiverPosition = this.receiverBulb.position;
				}
				this.GenerateLineRendererPositions();
				return;
			}
			if (this.wireRenderer.positionCount == 0)
			{
				this.GenerateLineRendererPositions();
			}
		}

		// Token: 0x06001F44 RID: 8004 RVA: 0x0008A77C File Offset: 0x0008897C
		private Vector3 QuadraticLerp(Vector3 pointZero, Vector3 pointOne, Vector3 pointTwo, float t)
		{
			return (1f - t) * (1f - t) * pointZero + 2f * (1f - t) * t * pointOne + t * t * pointTwo;
		}

		// Token: 0x06001F45 RID: 8005 RVA: 0x0008A7CC File Offset: 0x000889CC
		public Vector3 ReceiverTargetPosition()
		{
			return this.receiver.position;
		}

		// Token: 0x06001F46 RID: 8006 RVA: 0x0008A7D9 File Offset: 0x000889D9
		public void SetReceiverToPlayer(Transform transform)
		{
			this.connectedToPlayer = true;
			this.receiver = transform;
		}

		// Token: 0x06001F47 RID: 8007 RVA: 0x0008A7EC File Offset: 0x000889EC
		public void SetTemporaryTarget(Transform transform, SerializableGuid receiverGuid, Vector3 initialPosition, Action callback, bool travelImmediately)
		{
			this.connectedToPlayer = false;
			this.ReceiverId = receiverGuid;
			this.initialReceiverPosition = initialPosition;
			this.receiver = transform;
			if (travelImmediately)
			{
				base.StartCoroutine(this.MoveReceiver(callback));
				return;
			}
			this.currentReceiverPosition = this.receiver.position;
			callback();
		}

		// Token: 0x06001F48 RID: 8008 RVA: 0x0008A841 File Offset: 0x00088A41
		private IEnumerator MoveReceiver(Action callback)
		{
			float timeTraveled = 0f;
			while (timeTraveled < this.bulbTravelTime)
			{
				float num = timeTraveled / this.bulbTravelTime;
				this.currentReceiverPosition = Vector3.Lerp(this.initialReceiverPosition, this.receiver.position, num * num);
				timeTraveled += Time.deltaTime;
				yield return new WaitForEndOfFrame();
			}
			this.currentReceiverPosition = this.receiver.position;
			this.receiverBulb.transform.SetParent(this.receiver, true);
			if (callback != null)
			{
				callback();
			}
			yield break;
		}

		// Token: 0x06001F49 RID: 8009 RVA: 0x0008A857 File Offset: 0x00088A57
		public void SetEditing()
		{
			this.wireRenderer.material = this.editingMaterial;
			this.physicalWireState = PhysicalWire.PhysicalWireState.Editing;
		}

		// Token: 0x06001F4A RID: 8010 RVA: 0x0008A871 File Offset: 0x00088A71
		public void SetPlaced()
		{
			this.wireRenderer.material = this.placedMaterial;
			this.physicalWireState = PhysicalWire.PhysicalWireState.Placed;
		}

		// Token: 0x06001F4B RID: 8011 RVA: 0x0008A88C File Offset: 0x00088A8C
		public bool ContainsRerouteNodeId(SerializableGuid instanceId)
		{
			return this.rerouteNodeIds.Any((SerializableGuid id) => id == instanceId);
		}

		// Token: 0x06001F4C RID: 8012 RVA: 0x0008A8BD File Offset: 0x00088ABD
		public void Hover()
		{
			if (this.physicalWireState != PhysicalWire.PhysicalWireState.Placed)
			{
				return;
			}
			this.wireRenderer.material = this.hoveredMaterial;
		}

		// Token: 0x06001F4D RID: 8013 RVA: 0x0008A8DA File Offset: 0x00088ADA
		public void Unhovered()
		{
			if (this.physicalWireState != PhysicalWire.PhysicalWireState.Placed)
			{
				return;
			}
			this.wireRenderer.material = this.placedMaterial;
		}

		// Token: 0x06001F4E RID: 8014 RVA: 0x0008A8F7 File Offset: 0x00088AF7
		private void OnDestroy()
		{
			if (this.emitterBulb)
			{
				global::UnityEngine.Object.Destroy(this.emitterBulb.gameObject);
			}
			if (this.receiverBulb)
			{
				global::UnityEngine.Object.Destroy(this.receiverBulb.gameObject);
			}
		}

		// Token: 0x06001F4F RID: 8015 RVA: 0x0008A934 File Offset: 0x00088B34
		public void BakeCollision()
		{
			if (this.wireRenderer.positionCount > 0)
			{
				Mesh sharedMesh = this.meshCollider.sharedMesh;
				this.wireRenderer.BakeMesh(sharedMesh, true);
				this.meshCollider.sharedMesh = sharedMesh;
			}
		}

		// Token: 0x04001882 RID: 6274
		[SerializeField]
		private float pointsPerMeter = 0.5f;

		// Token: 0x04001883 RID: 6275
		[SerializeField]
		private int basePointsPerSegment = 10;

		// Token: 0x04001884 RID: 6276
		[SerializeField]
		private Vector2Int pointsMinMax = new Vector2Int(3, 50);

		// Token: 0x04001885 RID: 6277
		[SerializeField]
		private Transform emitterBulb;

		// Token: 0x04001886 RID: 6278
		[SerializeField]
		private Transform receiverBulb;

		// Token: 0x04001887 RID: 6279
		[SerializeField]
		private Material liveMaterial;

		// Token: 0x04001888 RID: 6280
		[SerializeField]
		private Material placedMaterial;

		// Token: 0x04001889 RID: 6281
		[SerializeField]
		private Material editingMaterial;

		// Token: 0x0400188A RID: 6282
		[SerializeField]
		private Material hoveredMaterial;

		// Token: 0x0400188B RID: 6283
		[SerializeField]
		private float bulbTravelTime = 0.2f;

		// Token: 0x0400188C RID: 6284
		private PhysicalWire.PhysicalWireState physicalWireState;

		// Token: 0x0400188D RID: 6285
		private Transform emitter;

		// Token: 0x0400188E RID: 6286
		private Transform receiver;

		// Token: 0x0400188F RID: 6287
		private LineRenderer wireRenderer;

		// Token: 0x04001890 RID: 6288
		private MeshCollider meshCollider;

		// Token: 0x04001891 RID: 6289
		private List<SerializableGuid> rerouteNodeIds = new List<SerializableGuid>();

		// Token: 0x04001892 RID: 6290
		private bool connectedToPlayer = true;

		// Token: 0x04001893 RID: 6291
		private Vector3 initialEmitterPosition;

		// Token: 0x04001894 RID: 6292
		private Vector3 initialReceiverPosition;

		// Token: 0x04001895 RID: 6293
		private Vector3 currentEmitterPosition;

		// Token: 0x04001896 RID: 6294
		private Vector3 currentReceiverPosition;

		// Token: 0x02000506 RID: 1286
		private enum PhysicalWireState
		{
			// Token: 0x0400189B RID: 6299
			Live,
			// Token: 0x0400189C RID: 6300
			Placed,
			// Token: 0x0400189D RID: 6301
			Editing
		}
	}
}
