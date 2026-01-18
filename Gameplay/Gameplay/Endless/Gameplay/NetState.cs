using System;
using System.Collections.Generic;
using Endless.Gameplay.LuaEnums;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x02000125 RID: 293
	public struct NetState : INetworkSerializable, IFrameInfo
	{
		// Token: 0x17000127 RID: 295
		// (get) Token: 0x06000695 RID: 1685 RVA: 0x00020534 File Offset: 0x0001E734
		// (set) Token: 0x06000696 RID: 1686 RVA: 0x0002053C File Offset: 0x0001E73C
		public uint NetFrame
		{
			get
			{
				return this.netFrame;
			}
			set
			{
				this.netFrame = value;
			}
		}

		// Token: 0x17000128 RID: 296
		// (get) Token: 0x06000697 RID: 1687 RVA: 0x00020545 File Offset: 0x0001E745
		public bool IsStunned
		{
			get
			{
				return this.StunFrame > 0U;
			}
		}

		// Token: 0x17000129 RID: 297
		// (get) Token: 0x06000698 RID: 1688 RVA: 0x00020550 File Offset: 0x0001E750
		public bool TeleportActive
		{
			get
			{
				return this.TeleportStatus > TeleportComponent.TeleportStatusType.None;
			}
		}

		// Token: 0x1700012A RID: 298
		// (get) Token: 0x06000699 RID: 1689 RVA: 0x0002055B File Offset: 0x0001E75B
		public bool PrimarySwapActive
		{
			get
			{
				return this.PrimarySwapBlockingFrames > 0U;
			}
		}

		// Token: 0x1700012B RID: 299
		// (get) Token: 0x0600069A RID: 1690 RVA: 0x00020566 File Offset: 0x0001E766
		public bool SecondarySwapActive
		{
			get
			{
				return this.SecondarySwapBlockingFrames > 0U;
			}
		}

		// Token: 0x1700012C RID: 300
		// (get) Token: 0x0600069B RID: 1691 RVA: 0x00020571 File Offset: 0x0001E771
		public bool AnyEquipmentSwapActive
		{
			get
			{
				return this.PrimarySwapActive || this.SecondarySwapActive;
			}
		}

		// Token: 0x0600069C RID: 1692 RVA: 0x00020584 File Offset: 0x0001E784
		public void CopyTo(ref NetState copyTarget)
		{
			using (FastBufferWriter fastBufferWriter = new FastBufferWriter(600, Allocator.Temp, -1))
			{
				fastBufferWriter.WriteNetworkSerializable<NetState>(in this);
				FastBufferReader fastBufferReader = new FastBufferReader(fastBufferWriter, Allocator.Temp, -1, 0, Allocator.Temp);
				fastBufferReader.ReadValue<NetState>(out copyTarget, default(FastBufferWriter.ForNetworkSerializable));
			}
		}

		// Token: 0x1700012D RID: 301
		// (get) Token: 0x0600069D RID: 1693 RVA: 0x000205E4 File Offset: 0x0001E7E4
		// (set) Token: 0x0600069E RID: 1694 RVA: 0x000205EC File Offset: 0x0001E7EC
		public float XZInputMultiplierThisFrame
		{
			get
			{
				return this._xzMultiplierThisFrame;
			}
			set
			{
				if (value < this._xzMultiplierThisFrame)
				{
					this._xzMultiplierThisFrame = value;
				}
			}
		}

		// Token: 0x1700012E RID: 302
		// (get) Token: 0x0600069F RID: 1695 RVA: 0x00020600 File Offset: 0x0001E800
		public Vector3 CalculatedPostion
		{
			get
			{
				NetworkObject networkObject = this.ParentNetworkObject;
				if (networkObject == null)
				{
					return this.Position;
				}
				return networkObject.transform.position + this.RelativePositionToParent;
			}
		}

		// Token: 0x060006A0 RID: 1696 RVA: 0x0002063F File Offset: 0x0001E83F
		public void BouncedThisFrame()
		{
			this.BlockGrounding = true;
			this.AirborneFrames = 0;
		}

		// Token: 0x060006A1 RID: 1697 RVA: 0x00020650 File Offset: 0x0001E850
		public void ResetTempFrameValues()
		{
			this.BlockItemInput = false;
			this.BlockMotionXZ = false;
			this.BlockMotionY = false;
			this.BlockMotionY_Down = false;
			this.BlockPhysics = false;
			this.BlockJump = false;
			this.BlockGrounding = false;
			this.BlockRotation = false;
			this.InputRotation = 0f;
			this.UseInputRotation = false;
			this.BlockGravity = false;
			this.BlockSecondaryEquipmentInput = false;
			this.GroundSlope = 0f;
			this.GroundNormal = Vector3.zero;
			this._xzMultiplierThisFrame = 1f;
			this.Strafing = false;
			this.PushForceControl = 1f;
			this.AimState = CameraController.CameraType.Normal;
			this.AimMovementScaleMultiplier = 1f;
			this.HasHostileForce = false;
			this.StableGround = false;
			this.UsingAimFix = false;
			this.MovingColliderVisualsCorrection = Vector3.zero;
		}

		// Token: 0x060006A2 RID: 1698 RVA: 0x0002071C File Offset: 0x0001E91C
		public static void Send(PlayerReferenceManager playerReference, NetState state)
		{
			using (FastBufferWriter fastBufferWriter = new FastBufferWriter(600, Allocator.Temp, -1))
			{
				NetworkObjectReference networkObjectReference = playerReference.NetworkObject;
				fastBufferWriter.WriteValueSafe<NetworkObjectReference>(in networkObjectReference, default(FastBufferWriter.ForNetworkSerializable));
				BytePacker.WriteValueBitPacked(fastBufferWriter, state.netFrame);
				fastBufferWriter.WriteNetworkSerializable<NetState>(in state);
				NetworkManager.Singleton.CustomMessagingManager.SendNamedMessageToAll("NetState", fastBufferWriter, NetworkDelivery.Unreliable);
			}
		}

		// Token: 0x060006A3 RID: 1699 RVA: 0x000207A0 File Offset: 0x0001E9A0
		public static void Receive(FastBufferReader reader)
		{
			NetworkObjectReference networkObjectReference;
			reader.ReadValueSafe<NetworkObjectReference>(out networkObjectReference, default(FastBufferWriter.ForNetworkSerializable));
			uint num;
			ByteUnpacker.ReadValueBitPacked(reader, out num);
			NetworkObject networkObject;
			if (networkObjectReference.TryGet(out networkObject, null))
			{
				PlayerReferenceManager playerObject = MonoBehaviourSingleton<PlayerManager>.Instance.GetPlayerObject(networkObject.OwnerClientId);
				ref NetState referenceFromBuffer = ref playerObject.PlayerNetworkController.ClientStateRingBuffer.GetReferenceFromBuffer(num);
				reader.ReadNetworkSerializable<NetState>(out referenceFromBuffer);
				referenceFromBuffer.netFrame = num;
				referenceFromBuffer.serverVerifiedState = true;
				playerObject.PlayerNetworkController.UpdatedState(num);
			}
		}

		// Token: 0x060006A4 RID: 1700 RVA: 0x0002081C File Offset: 0x0001EA1C
		public void NetworkSerialize<NetState>(BufferSerializer<NetState> serializer) where NetState : IReaderWriter
		{
			serializer.SerializeValue(ref this.Position);
			serializer.SerializeValue<SerializableGuid>(ref this.CurrentPressedEquipment, default(FastBufferWriter.ForNetworkSerializable));
			serializer.SerializeValue<float>(ref this.CharacterRotation, default(FastBufferWriter.ForPrimitives));
			bool flag = this.ActiveWorldUseState != null;
			bool flag2 = this.ParentNetworkObject != null;
			bool teleportActive = this.TeleportActive;
			if (serializer.IsWriter)
			{
				Compression.SerializeIntToByteClamped<NetState>(serializer, this.JumpFrame);
				Compression.SerializeIntToByteClamped<NetState>(serializer, this.MotionX);
				Compression.SerializeIntToByteClamped<NetState>(serializer, this.MotionZ);
				Compression.SerializeUIntToByteClamped<NetState>(serializer, this.StunFrame);
				Compression.SerializeIntToByteClamped<NetState>(serializer, this.EquipmentPressedDuration);
				Compression.SerializeIntToByteClamped<NetState>(serializer, this.JumpPressedDuration);
				Compression.SerializeIntToByteClamped<NetState>(serializer, this.AirborneFrames);
				Compression.SerializeIntToByteClamped<NetState>(serializer, this.FramesSinceStableGround);
				Compression.SerializeIntToByteClamped<NetState>(serializer, this.FallFrames);
				Compression.SerializeFloatToUShort<NetState>(serializer, this.SlopeAngle, -1f, 1f);
				Compression.SerializeFloatToUShort<NetState>(serializer, this.CalculatedMotion.x, -10f, 10f);
				Compression.SerializeFloatToUShort<NetState>(serializer, this.CalculatedMotion.y, -10f, 10f);
				Compression.SerializeFloatToUShort<NetState>(serializer, this.CalculatedMotion.z, -10f, 10f);
				Compression.SerializeFloatToUShort<NetState>(serializer, this.LastWorldPhysics.x, -1f, 1f);
				Compression.SerializeFloatToUShort<NetState>(serializer, this.LastWorldPhysics.y, -1f, 1f);
				Compression.SerializeFloatToUShort<NetState>(serializer, this.LastWorldPhysics.z, -1f, 1f);
				Compression.SerializeUInt<NetState>(serializer, this.LastHitFrame);
				Compression.SerializeBoolsToByte<NetState>(serializer, this.Grounded, this.JumpReleasedThisJump, teleportActive, this.Downed, this.Reviving, this.Ghost, flag2, false);
				Compression.SerializeBoolsToByte<NetState>(serializer, flag, this.InputSynced, this.UseIK, this.Run, false, false, false, false);
				Compression.SerializeFloatToUShort<NetState>(serializer, this.HorizontalCameraAim, 0f, 360f);
				Compression.SerializeUInt<NetState>(serializer, this.PrimarySwapBlockingFrames);
				Compression.SerializeUInt<NetState>(serializer, this.SecondarySwapBlockingFrames);
				serializer.SerializeValue<CameraController.CameraType>(ref this.AimState, default(FastBufferWriter.ForEnums));
				if (this.AimState != CameraController.CameraType.Normal)
				{
					Compression.SerializeFloatToUShort<NetState>(serializer, this.AimMovementScaleMultiplier, 0f, 1f);
				}
				serializer.SerializeValue(ref this.AimRelativePoint);
			}
			else
			{
				this.JumpFrame = Compression.DeserializeIntFromByteClamped<NetState>(serializer);
				this.MotionX = Compression.DeserializeIntFromByteClamped<NetState>(serializer);
				this.MotionZ = Compression.DeserializeIntFromByteClamped<NetState>(serializer);
				this.StunFrame = Compression.DeserializeUIntFromByteClamped<NetState>(serializer);
				this.EquipmentPressedDuration = Compression.DeserializeIntFromByteClamped<NetState>(serializer);
				this.JumpPressedDuration = Compression.DeserializeIntFromByteClamped<NetState>(serializer);
				this.AirborneFrames = Compression.DeserializeIntFromByteClamped<NetState>(serializer);
				this.FramesSinceStableGround = Compression.DeserializeIntFromByteClamped<NetState>(serializer);
				this.FallFrames = Compression.DeserializeIntFromByteClamped<NetState>(serializer);
				this.SlopeAngle = Compression.DeserializeFloatFromUShort<NetState>(serializer, -1f, 1f);
				this.CalculatedMotion.x = Compression.DeserializeFloatFromUShort<NetState>(serializer, -10f, 10f);
				this.CalculatedMotion.y = Compression.DeserializeFloatFromUShort<NetState>(serializer, -10f, 10f);
				this.CalculatedMotion.z = Compression.DeserializeFloatFromUShort<NetState>(serializer, -10f, 10f);
				this.LastWorldPhysics.x = Compression.DeserializeFloatFromUShort<NetState>(serializer, -1f, 1f);
				this.LastWorldPhysics.y = Compression.DeserializeFloatFromUShort<NetState>(serializer, -1f, 1f);
				this.LastWorldPhysics.z = Compression.DeserializeFloatFromUShort<NetState>(serializer, -1f, 1f);
				this.LastHitFrame = Compression.DeserializeUInt<NetState>(serializer);
				Compression.DeserializeBoolsFromByte<NetState>(serializer, ref this.Grounded, ref this.JumpReleasedThisJump, ref teleportActive, ref this.Downed, ref this.Reviving, ref this.Ghost, ref flag2);
				Compression.DeserializeBoolsFromByte<NetState>(serializer, ref flag, ref this.InputSynced, ref this.UseIK, ref this.Run);
				this.HorizontalCameraAim = Compression.DeserializeFloatFromUShort<NetState>(serializer, 0f, 360f);
				this.PrimarySwapBlockingFrames = Compression.DeserializeUInt<NetState>(serializer);
				this.SecondarySwapBlockingFrames = Compression.DeserializeUInt<NetState>(serializer);
				serializer.SerializeValue<CameraController.CameraType>(ref this.AimState, default(FastBufferWriter.ForEnums));
				if (this.AimState != CameraController.CameraType.Normal)
				{
					this.AimMovementScaleMultiplier = Compression.DeserializeFloatFromUShort<NetState>(serializer, 0f, 1f);
				}
				else
				{
					this.AimMovementScaleMultiplier = 1f;
				}
				serializer.SerializeValue(ref this.AimRelativePoint);
			}
			if (teleportActive)
			{
				serializer.SerializeValue<TeleportComponent.TeleportStatusType>(ref this.TeleportStatus, default(FastBufferWriter.ForEnums));
				serializer.SerializeValue<uint>(ref this.TeleportAtFrame, default(FastBufferWriter.ForPrimitives));
				serializer.SerializeValue(ref this.TeleportPosition);
				serializer.SerializeValue<bool>(ref this.TeleportHasRotation, default(FastBufferWriter.ForPrimitives));
				serializer.SerializeValue<bool>(ref this.GameplayTeleport, default(FastBufferWriter.ForPrimitives));
				serializer.SerializeValue<TeleportType>(ref this.GameplayTeleportType, default(FastBufferWriter.ForEnums));
				serializer.SerializeValue<bool>(ref this.TeleportRotationSnapCamera, default(FastBufferWriter.ForPrimitives));
				if (this.TeleportHasRotation)
				{
					serializer.SerializeValue<float>(ref this.TeleportRotation, default(FastBufferWriter.ForPrimitives));
				}
			}
			if (flag2)
			{
				serializer.SerializeValue<NetworkObjectReference>(ref this.ParentNetworkObject, default(FastBufferWriter.ForNetworkSerializable));
				serializer.SerializeValue(ref this.RelativePositionToParent);
			}
			else if (serializer.IsReader)
			{
				this.ParentNetworkObject = default(NetworkObjectReference);
			}
			if (this.Ghost)
			{
				if (serializer.IsWriter)
				{
					Compression.SerializeIntToByteClamped<NetState>(serializer, this.GhostVerticalMotorFrame);
				}
				else
				{
					this.GhostVerticalMotorFrame = Compression.DeserializeIntFromByteClamped<NetState>(serializer);
				}
			}
			int num = 0;
			if (serializer.IsWriter)
			{
				num = ((this.PhysicsForces != null) ? this.PhysicsForces.Count : 0);
			}
			serializer.SerializeValue<int>(ref num, default(FastBufferWriter.ForPrimitives));
			serializer.SerializeValue<PlayerPhysicsTaker.PushState>(ref this.PhysicsPushState, default(FastBufferWriter.ForEnums));
			if (serializer.IsWriter)
			{
				Compression.SerializeUInt<NetState>(serializer, this.PushFrame);
			}
			else
			{
				this.PushFrame = Compression.DeserializeUInt<NetState>(serializer);
			}
			if (serializer.IsReader)
			{
				this.PhysicsForces = new List<PhysicsForceInfo>();
				for (int i = 0; i < num; i++)
				{
					PhysicsForceInfo physicsForceInfo = default(PhysicsForceInfo);
					serializer.SerializeValue<PhysicsForceInfo>(ref physicsForceInfo, default(FastBufferWriter.ForNetworkSerializable));
					this.PhysicsForces.Add(physicsForceInfo);
				}
			}
			else
			{
				for (int j = 0; j < num; j++)
				{
					PhysicsForceInfo physicsForceInfo2 = this.PhysicsForces[j];
					serializer.SerializeValue<PhysicsForceInfo>(ref physicsForceInfo2, default(FastBufferWriter.ForNetworkSerializable));
				}
			}
			serializer.SerializeValue(ref this.TotalForce);
			int num2 = 0;
			if (serializer.IsWriter)
			{
				num2 = ((this.ActiveUseStates == null) ? 0 : this.ActiveUseStates.Count);
			}
			serializer.SerializeValue<int>(ref num2, default(FastBufferWriter.ForPrimitives));
			if (serializer.IsReader)
			{
				this.ActiveUseStates = new List<UsableDefinition.UseState>();
				for (int k = 0; k < num2; k++)
				{
					UsableDefinition.UseState useState = null;
					UsableDefinition.SerializeEquipmentUseState<NetState>(serializer, ref useState);
					this.ActiveUseStates.Add(useState);
				}
			}
			else
			{
				for (int l = 0; l < num2; l++)
				{
					UsableDefinition.UseState useState2 = this.ActiveUseStates[l];
					UsableDefinition.SerializeEquipmentUseState<NetState>(serializer, ref useState2);
				}
			}
			if (flag)
			{
				UsableDefinition.SerializeEquipmentUseState<NetState>(serializer, ref this.ActiveWorldUseState);
			}
		}

		// Token: 0x060006A5 RID: 1701 RVA: 0x00020F3C File Offset: 0x0001F13C
		public void Clear()
		{
			if (this.ActiveUseStates != null)
			{
				for (int i = this.ActiveUseStates.Count - 1; i > -1; i--)
				{
					UsableDefinition.UseState.PoolState(this.ActiveUseStates[i]);
				}
				this.ActiveUseStates.Clear();
			}
			if (this.PhysicsForces != null)
			{
				this.PhysicsForces.Clear();
			}
		}

		// Token: 0x060006A6 RID: 1702 RVA: 0x00002DB0 File Offset: 0x00000FB0
		public void Initialize()
		{
		}

		// Token: 0x0400050E RID: 1294
		private uint netFrame;

		// Token: 0x0400050F RID: 1295
		public Vector3 Position;

		// Token: 0x04000510 RID: 1296
		public Vector3 CalculatedMotion;

		// Token: 0x04000511 RID: 1297
		public int JumpFrame;

		// Token: 0x04000512 RID: 1298
		public int MotionX;

		// Token: 0x04000513 RID: 1299
		public int MotionZ;

		// Token: 0x04000514 RID: 1300
		public uint StunFrame;

		// Token: 0x04000515 RID: 1301
		public float CharacterRotation;

		// Token: 0x04000516 RID: 1302
		public bool Grounded;

		// Token: 0x04000517 RID: 1303
		public float SlopeAngle;

		// Token: 0x04000518 RID: 1304
		public SerializableGuid CurrentPressedEquipment;

		// Token: 0x04000519 RID: 1305
		public int EquipmentPressedDuration;

		// Token: 0x0400051A RID: 1306
		public bool JumpReleasedThisJump;

		// Token: 0x0400051B RID: 1307
		public int JumpPressedDuration;

		// Token: 0x0400051C RID: 1308
		public int AirborneFrames;

		// Token: 0x0400051D RID: 1309
		public int FallFrames;

		// Token: 0x0400051E RID: 1310
		public Vector3 LastWorldPhysics;

		// Token: 0x0400051F RID: 1311
		public uint LastHitFrame;

		// Token: 0x04000520 RID: 1312
		public NetworkObjectReference ParentNetworkObject;

		// Token: 0x04000521 RID: 1313
		public Vector3 RelativePositionToParent;

		// Token: 0x04000522 RID: 1314
		public TeleportComponent.TeleportStatusType TeleportStatus;

		// Token: 0x04000523 RID: 1315
		public bool GameplayTeleport;

		// Token: 0x04000524 RID: 1316
		public TeleportType GameplayTeleportType;

		// Token: 0x04000525 RID: 1317
		public uint TeleportAtFrame;

		// Token: 0x04000526 RID: 1318
		public Vector3 TeleportPosition;

		// Token: 0x04000527 RID: 1319
		public bool TeleportHasRotation;

		// Token: 0x04000528 RID: 1320
		public float TeleportRotation;

		// Token: 0x04000529 RID: 1321
		public bool TeleportRotationSnapCamera;

		// Token: 0x0400052A RID: 1322
		public bool Downed;

		// Token: 0x0400052B RID: 1323
		public bool Reviving;

		// Token: 0x0400052C RID: 1324
		public bool Ghost;

		// Token: 0x0400052D RID: 1325
		public bool Run;

		// Token: 0x0400052E RID: 1326
		public bool InputSynced;

		// Token: 0x0400052F RID: 1327
		public int GhostVerticalMotorFrame;

		// Token: 0x04000530 RID: 1328
		public List<UsableDefinition.UseState> ActiveUseStates;

		// Token: 0x04000531 RID: 1329
		private NetworkObjectReference interactableNetObjRef;

		// Token: 0x04000532 RID: 1330
		public UsableDefinition.UseState ActiveWorldUseState;

		// Token: 0x04000533 RID: 1331
		public List<PhysicsForceInfo> PhysicsForces;

		// Token: 0x04000534 RID: 1332
		public Vector3 TotalForce;

		// Token: 0x04000535 RID: 1333
		public PlayerPhysicsTaker.PushState PhysicsPushState;

		// Token: 0x04000536 RID: 1334
		public uint PushFrame;

		// Token: 0x04000537 RID: 1335
		public Vector3 AimRelativePoint;

		// Token: 0x04000538 RID: 1336
		public bool UseIK;

		// Token: 0x04000539 RID: 1337
		public int FramesSinceStableGround;

		// Token: 0x0400053A RID: 1338
		public uint PrimarySwapBlockingFrames;

		// Token: 0x0400053B RID: 1339
		public uint SecondarySwapBlockingFrames;

		// Token: 0x0400053C RID: 1340
		public float HorizontalCameraAim;

		// Token: 0x0400053D RID: 1341
		public CameraController.CameraType AimState;

		// Token: 0x0400053E RID: 1342
		public float AimMovementScaleMultiplier;

		// Token: 0x0400053F RID: 1343
		public bool BlockItemInput;

		// Token: 0x04000540 RID: 1344
		public bool BlockMotionXZ;

		// Token: 0x04000541 RID: 1345
		public bool BlockMotionY;

		// Token: 0x04000542 RID: 1346
		public bool BlockMotionY_Down;

		// Token: 0x04000543 RID: 1347
		public bool BlockPhysics;

		// Token: 0x04000544 RID: 1348
		public bool BlockJump;

		// Token: 0x04000545 RID: 1349
		public bool BlockGrounding;

		// Token: 0x04000546 RID: 1350
		public bool BlockRotation;

		// Token: 0x04000547 RID: 1351
		public float InputRotation;

		// Token: 0x04000548 RID: 1352
		public bool UseInputRotation;

		// Token: 0x04000549 RID: 1353
		public bool BlockSecondaryEquipmentInput;

		// Token: 0x0400054A RID: 1354
		public float GroundSlope;

		// Token: 0x0400054B RID: 1355
		public Vector3 GroundNormal;

		// Token: 0x0400054C RID: 1356
		public bool Strafing;

		// Token: 0x0400054D RID: 1357
		public bool HasHostileForce;

		// Token: 0x0400054E RID: 1358
		public bool BlockGravity;

		// Token: 0x0400054F RID: 1359
		public bool StableGround;

		// Token: 0x04000550 RID: 1360
		public int GroundedFrames;

		// Token: 0x04000551 RID: 1361
		public bool UsingAimFix;

		// Token: 0x04000552 RID: 1362
		public Vector3 MovingColliderVisualsCorrection;

		// Token: 0x04000553 RID: 1363
		public bool serverVerifiedState;

		// Token: 0x04000554 RID: 1364
		public float PushForceControl;

		// Token: 0x04000555 RID: 1365
		private float _xzMultiplierThisFrame;
	}
}
