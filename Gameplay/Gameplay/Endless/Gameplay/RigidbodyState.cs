using System;
using Endless.Gameplay.LuaEnums;
using Endless.Shared;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x02000113 RID: 275
	public struct RigidbodyState : INetworkSerializable, IFrameInfo
	{
		// Token: 0x17000117 RID: 279
		// (get) Token: 0x0600062A RID: 1578 RVA: 0x0001E8F0 File Offset: 0x0001CAF0
		// (set) Token: 0x0600062B RID: 1579 RVA: 0x0001E8F8 File Offset: 0x0001CAF8
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

		// Token: 0x0600062C RID: 1580 RVA: 0x00002DB0 File Offset: 0x00000FB0
		public void Clear()
		{
		}

		// Token: 0x0600062D RID: 1581 RVA: 0x00002DB0 File Offset: 0x00000FB0
		public void Initialize()
		{
		}

		// Token: 0x0600062E RID: 1582 RVA: 0x0001E904 File Offset: 0x0001CB04
		public static void Send(NetworkRigidbodyController controller, RigidbodyState state)
		{
			using (FastBufferWriter fastBufferWriter = new FastBufferWriter(600, Allocator.Temp, -1))
			{
				NetworkObjectReference networkObjectReference = controller.NetworkObject;
				fastBufferWriter.WriteValueSafe<NetworkObjectReference>(in networkObjectReference, default(FastBufferWriter.ForNetworkSerializable));
				fastBufferWriter.WriteNetworkSerializable<RigidbodyState>(in state);
				NetworkManager.Singleton.CustomMessagingManager.SendNamedMessageToAll("RigidBodyState", fastBufferWriter, NetworkDelivery.Unreliable);
			}
		}

		// Token: 0x0600062F RID: 1583 RVA: 0x0001E97C File Offset: 0x0001CB7C
		public static void Receive(FastBufferReader reader)
		{
			NetworkObjectReference networkObjectReference;
			reader.ReadValueSafe<NetworkObjectReference>(out networkObjectReference, default(FastBufferWriter.ForNetworkSerializable));
			NetworkObject networkObject;
			if (networkObjectReference.TryGet(out networkObject, null))
			{
				NetworkRigidbodyController componentInChildren = networkObject.GetComponentInChildren<NetworkRigidbodyController>();
				RigidbodyState rigidbodyState;
				reader.ReadNetworkSerializable<RigidbodyState>(out rigidbodyState);
				rigidbodyState.serverVerifiedState = true;
				componentInChildren.WriteStateToBuffer(ref rigidbodyState);
				componentInChildren.UpdatedState(rigidbodyState.netFrame);
			}
		}

		// Token: 0x06000630 RID: 1584 RVA: 0x0001E9D4 File Offset: 0x0001CBD4
		public void NetworkSerialize<RigidbodyState>(BufferSerializer<RigidbodyState> serializer) where RigidbodyState : IReaderWriter
		{
			if (serializer.IsWriter)
			{
				Compression.SerializeBoolsToByte<RigidbodyState>(serializer, this.FullSync, this.Sleeping, this.Teleporting, false, false, false, false, false);
				Compression.SerializeUInt<RigidbodyState>(serializer, this.netFrame);
			}
			else
			{
				Compression.DeserializeBoolsFromByte<RigidbodyState>(serializer, ref this.FullSync, ref this.Sleeping, ref this.Teleporting);
				this.netFrame = Compression.DeserializeUInt<RigidbodyState>(serializer);
			}
			if (this.FullSync)
			{
				serializer.SerializeValue(ref this.Position);
				serializer.SerializeValue(ref this.Angles);
				serializer.SerializeValue(ref this.Velocity);
				serializer.SerializeValue(ref this.AngularVelocity);
				if (this.Teleporting)
				{
					if (serializer.IsWriter)
					{
						Compression.SerializeUInt<RigidbodyState>(serializer, this.FramesUntilTeleport);
					}
					else
					{
						this.FramesUntilTeleport = Compression.DeserializeUInt<RigidbodyState>(serializer);
					}
					serializer.SerializeValue<TeleportType>(ref this.TeleportType, default(FastBufferWriter.ForEnums));
					serializer.SerializeValue(ref this.TeleportPosition);
				}
			}
		}

		// Token: 0x040004A3 RID: 1187
		private uint netFrame;

		// Token: 0x040004A4 RID: 1188
		public bool FullSync;

		// Token: 0x040004A5 RID: 1189
		public bool Sleeping;

		// Token: 0x040004A6 RID: 1190
		public Vector3 Position;

		// Token: 0x040004A7 RID: 1191
		public Vector3 Angles;

		// Token: 0x040004A8 RID: 1192
		public Vector3 Velocity;

		// Token: 0x040004A9 RID: 1193
		public Vector3 AngularVelocity;

		// Token: 0x040004AA RID: 1194
		public bool Teleporting;

		// Token: 0x040004AB RID: 1195
		public TeleportType TeleportType;

		// Token: 0x040004AC RID: 1196
		public uint FramesUntilTeleport;

		// Token: 0x040004AD RID: 1197
		public Vector3 TeleportPosition;

		// Token: 0x040004AE RID: 1198
		public bool serverVerifiedState;
	}
}
