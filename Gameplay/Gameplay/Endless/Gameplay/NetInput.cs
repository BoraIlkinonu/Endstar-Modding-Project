using System;
using Endless.Shared;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x02000121 RID: 289
	public struct NetInput : INetworkSerializable, IFrameInfo
	{
		// Token: 0x17000124 RID: 292
		// (get) Token: 0x0600068C RID: 1676 RVA: 0x000201CC File Offset: 0x0001E3CC
		// (set) Token: 0x0600068D RID: 1677 RVA: 0x000201D4 File Offset: 0x0001E3D4
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

		// Token: 0x17000125 RID: 293
		// (get) Token: 0x0600068E RID: 1678 RVA: 0x000201DD File Offset: 0x0001E3DD
		public bool PressingPrimary1
		{
			get
			{
				return this.PrimaryEquipment == NetInput.PrimaryEquipmentInput.P1 || this.PrimaryEquipment == NetInput.PrimaryEquipmentInput.Both;
			}
		}

		// Token: 0x17000126 RID: 294
		// (get) Token: 0x0600068F RID: 1679 RVA: 0x000201F3 File Offset: 0x0001E3F3
		public bool PressingPrimary2
		{
			get
			{
				return this.PrimaryEquipment == NetInput.PrimaryEquipmentInput.P2 || this.PrimaryEquipment == NetInput.PrimaryEquipmentInput.Both;
			}
		}

		// Token: 0x06000690 RID: 1680 RVA: 0x0002020C File Offset: 0x0001E40C
		public void Clear()
		{
			this.Vertical = 0;
			this.Horizontal = 0;
			this.MotionRotation = 0f;
			this.CharacterRotation = 0f;
			this.AimPitch = 0f;
			this.AimYaw = 0f;
			this.Jump = false;
			this.Ghost = false;
			this.Down = false;
			this.PrimaryEquipment = NetInput.PrimaryEquipmentInput.None;
			this.SecondaryEquipment = false;
			this.Run = false;
		}

		// Token: 0x06000691 RID: 1681 RVA: 0x00002DB0 File Offset: 0x00000FB0
		public void Initialize()
		{
		}

		// Token: 0x06000692 RID: 1682 RVA: 0x00020280 File Offset: 0x0001E480
		public void NetworkSerialize<NetInput>(BufferSerializer<NetInput> serializer) where NetInput : IReaderWriter
		{
			if (serializer.IsWriter)
			{
				Compression.SerializeBoolsToByte<NetInput>(serializer, this.Jump, this.Ghost, this.Down, this.SecondaryEquipment, this.UseIK, this.Run, false, false);
				Compression.SerializeFloatToUShort<NetInput>(serializer, this.MotionRotation, 0f, 360f);
				serializer.SerializeValue<float>(ref this.CharacterRotation, default(FastBufferWriter.ForPrimitives));
				this.inputDirection = (NetInput.InputDirection)(((this.Vertical == 0) ? 3 : ((this.Vertical > 0) ? 6 : 9)) + this.Horizontal);
				serializer.SerializeValue<NetInput.InputDirection>(ref this.inputDirection, default(FastBufferWriter.ForEnums));
				serializer.SerializeValue<float>(ref this.AimPitch, default(FastBufferWriter.ForPrimitives));
				serializer.SerializeValue<float>(ref this.AimYaw, default(FastBufferWriter.ForPrimitives));
				serializer.SerializeValue<NetInput.PrimaryEquipmentInput>(ref this.PrimaryEquipment, default(FastBufferWriter.ForEnums));
				serializer.SerializeValue(ref this.FocusPoint);
				return;
			}
			Compression.DeserializeBoolsFromByte<NetInput>(serializer, ref this.Jump, ref this.Ghost, ref this.Down, ref this.SecondaryEquipment, ref this.UseIK, ref this.Run);
			this.MotionRotation = Compression.DeserializeFloatFromUShort<NetInput>(serializer, 0f, 360f);
			serializer.SerializeValue<float>(ref this.CharacterRotation, default(FastBufferWriter.ForPrimitives));
			serializer.SerializeValue<NetInput.InputDirection>(ref this.inputDirection, default(FastBufferWriter.ForEnums));
			int num = (int)this.inputDirection;
			this.Vertical = ((num < 5) ? 0 : ((num > 7) ? (-1) : 1));
			this.Horizontal = num % 3;
			if (this.Horizontal > 1)
			{
				this.Horizontal = -1;
			}
			serializer.SerializeValue<float>(ref this.AimPitch, default(FastBufferWriter.ForPrimitives));
			serializer.SerializeValue<float>(ref this.AimYaw, default(FastBufferWriter.ForPrimitives));
			serializer.SerializeValue<NetInput.PrimaryEquipmentInput>(ref this.PrimaryEquipment, default(FastBufferWriter.ForEnums));
			serializer.SerializeValue(ref this.FocusPoint);
		}

		// Token: 0x06000693 RID: 1683 RVA: 0x00020470 File Offset: 0x0001E670
		public static void Send(NetInput input)
		{
			using (FastBufferWriter fastBufferWriter = new FastBufferWriter(600, Allocator.Temp, -1))
			{
				BytePacker.WriteValueBitPacked(fastBufferWriter, input.netFrame);
				fastBufferWriter.WriteNetworkSerializable<NetInput>(in input);
				NetworkManager.Singleton.CustomMessagingManager.SendNamedMessage("NetInput", 0UL, fastBufferWriter, NetworkDelivery.Unreliable);
			}
		}

		// Token: 0x06000694 RID: 1684 RVA: 0x000204D8 File Offset: 0x0001E6D8
		public static void Receive(ulong senderClientId, FastBufferReader reader)
		{
			PlayerReferenceManager playerObject = MonoBehaviourSingleton<PlayerManager>.Instance.GetPlayerObject(senderClientId);
			if (!playerObject)
			{
				return;
			}
			uint num;
			ByteUnpacker.ReadValueBitPacked(reader, out num);
			ref NetInput referenceFromBuffer = ref playerObject.PlayerNetworkController.ServerInputRingBuffer.GetReferenceFromBuffer(num);
			reader.ReadNetworkSerializable<NetInput>(out referenceFromBuffer);
			referenceFromBuffer.netFrame = num;
			playerObject.PlayerNetworkController.ServerInputRingBuffer.FrameUpdated(num);
		}

		// Token: 0x040004EB RID: 1259
		private uint netFrame;

		// Token: 0x040004EC RID: 1260
		public int Vertical;

		// Token: 0x040004ED RID: 1261
		public int Horizontal;

		// Token: 0x040004EE RID: 1262
		public float MotionRotation;

		// Token: 0x040004EF RID: 1263
		public float CharacterRotation;

		// Token: 0x040004F0 RID: 1264
		public float AimPitch;

		// Token: 0x040004F1 RID: 1265
		public float AimYaw;

		// Token: 0x040004F2 RID: 1266
		public bool Jump;

		// Token: 0x040004F3 RID: 1267
		public bool Ghost;

		// Token: 0x040004F4 RID: 1268
		public bool Down;

		// Token: 0x040004F5 RID: 1269
		public NetInput.PrimaryEquipmentInput PrimaryEquipment;

		// Token: 0x040004F6 RID: 1270
		public bool SecondaryEquipment;

		// Token: 0x040004F7 RID: 1271
		public bool UseIK;

		// Token: 0x040004F8 RID: 1272
		public Vector3 FocusPoint;

		// Token: 0x040004F9 RID: 1273
		public bool Run;

		// Token: 0x040004FA RID: 1274
		private NetInput.InputDirection inputDirection;

		// Token: 0x02000122 RID: 290
		public enum InputEquipmentSlot : byte
		{
			// Token: 0x040004FC RID: 1276
			None,
			// Token: 0x040004FD RID: 1277
			E1,
			// Token: 0x040004FE RID: 1278
			E2
		}

		// Token: 0x02000123 RID: 291
		public enum InputDirection : byte
		{
			// Token: 0x04000500 RID: 1280
			L = 2,
			// Token: 0x04000501 RID: 1281
			None,
			// Token: 0x04000502 RID: 1282
			R,
			// Token: 0x04000503 RID: 1283
			FL,
			// Token: 0x04000504 RID: 1284
			F,
			// Token: 0x04000505 RID: 1285
			FR,
			// Token: 0x04000506 RID: 1286
			BL,
			// Token: 0x04000507 RID: 1287
			B,
			// Token: 0x04000508 RID: 1288
			BR
		}

		// Token: 0x02000124 RID: 292
		public enum PrimaryEquipmentInput : byte
		{
			// Token: 0x0400050A RID: 1290
			None,
			// Token: 0x0400050B RID: 1291
			P1,
			// Token: 0x0400050C RID: 1292
			P2,
			// Token: 0x0400050D RID: 1293
			Both
		}
	}
}
