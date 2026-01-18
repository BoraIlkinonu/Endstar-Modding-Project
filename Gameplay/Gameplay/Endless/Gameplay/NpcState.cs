using System;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x0200022D RID: 557
	public struct NpcState : IFrameInfo, INetworkSerializable
	{
		// Token: 0x1700021F RID: 543
		// (get) Token: 0x06000B89 RID: 2953 RVA: 0x0003FA79 File Offset: 0x0003DC79
		// (set) Token: 0x06000B8A RID: 2954 RVA: 0x0003FA81 File Offset: 0x0003DC81
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

		// Token: 0x06000B8B RID: 2955 RVA: 0x0003FA8C File Offset: 0x0003DC8C
		public void Clear()
		{
			this.netFrame = 0U;
			this.Position = Vector3.zero;
			this.Rotation = 0f;
			this.attack = 0;
			this.isMoving = false;
			this.isAirborne = false;
			this.jumped = false;
			this.slopeAngle = 0f;
			this.VelX = 0f;
			this.VelY = 0f;
			this.VelZ = 0f;
			this.AngularVelocity = 0f;
			this.HorizVelMagnitude = 0f;
			this.airTime = 0f;
			this.fallTime = 0f;
			this.taunt = 0;
			this.fidget = 0;
			this.landed = false;
			this.isGrounded = false;
			this.LargePush = false;
			this.PhysicsForceExit = false;
			this.LoopSmallPush = false;
			this.SmallPush = false;
			this.EndSmallPush = false;
			this.ImminentlyAttacking = false;
			this.walking = false;
		}

		// Token: 0x06000B8C RID: 2956 RVA: 0x00002DB0 File Offset: 0x00000FB0
		public void Initialize()
		{
		}

		// Token: 0x06000B8D RID: 2957 RVA: 0x0003FB78 File Offset: 0x0003DD78
		public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
		{
			serializer.SerializeValue<uint>(ref this.netFrame, default(FastBufferWriter.ForPrimitives));
			serializer.SerializeValue(ref this.Position);
			serializer.SerializeValue<float>(ref this.Rotation, default(FastBufferWriter.ForPrimitives));
			serializer.SerializeValue<int>(ref this.attack, default(FastBufferWriter.ForPrimitives));
			serializer.SerializeValue<int>(ref this.taunt, default(FastBufferWriter.ForPrimitives));
			serializer.SerializeValue<int>(ref this.fidget, default(FastBufferWriter.ForPrimitives));
			serializer.SerializeValue<bool>(ref this.jumped, default(FastBufferWriter.ForPrimitives));
			serializer.SerializeValue<float>(ref this.fallTime, default(FastBufferWriter.ForPrimitives));
			serializer.SerializeValue<float>(ref this.VelX, default(FastBufferWriter.ForPrimitives));
			serializer.SerializeValue<float>(ref this.VelY, default(FastBufferWriter.ForPrimitives));
			serializer.SerializeValue<float>(ref this.VelZ, default(FastBufferWriter.ForPrimitives));
			serializer.SerializeValue<bool>(ref this.isMoving, default(FastBufferWriter.ForPrimitives));
			serializer.SerializeValue<float>(ref this.AngularVelocity, default(FastBufferWriter.ForPrimitives));
			serializer.SerializeValue<float>(ref this.HorizVelMagnitude, default(FastBufferWriter.ForPrimitives));
			serializer.SerializeValue<bool>(ref this.LargePush, default(FastBufferWriter.ForPrimitives));
			serializer.SerializeValue<bool>(ref this.PhysicsForceExit, default(FastBufferWriter.ForPrimitives));
			serializer.SerializeValue<bool>(ref this.isAirborne, default(FastBufferWriter.ForPrimitives));
			serializer.SerializeValue<bool>(ref this.zLock, default(FastBufferWriter.ForPrimitives));
			serializer.SerializeValue<bool>(ref this.landed, default(FastBufferWriter.ForPrimitives));
			serializer.SerializeValue<bool>(ref this.isGrounded, default(FastBufferWriter.ForPrimitives));
			serializer.SerializeValue<bool>(ref this.LoopSmallPush, default(FastBufferWriter.ForPrimitives));
			serializer.SerializeValue<bool>(ref this.SmallPush, default(FastBufferWriter.ForPrimitives));
			serializer.SerializeValue<bool>(ref this.EndSmallPush, default(FastBufferWriter.ForPrimitives));
			serializer.SerializeValue<bool>(ref this.ImminentlyAttacking, default(FastBufferWriter.ForPrimitives));
			serializer.SerializeValue<bool>(ref this.walking, default(FastBufferWriter.ForPrimitives));
		}

		// Token: 0x06000B8E RID: 2958 RVA: 0x0003FDA4 File Offset: 0x0003DFA4
		public static void Send(NpcState simulatedState, uint key)
		{
			using (FastBufferWriter fastBufferWriter = new FastBufferWriter(600, Allocator.Temp, -1))
			{
				fastBufferWriter.WriteValueSafe<uint>(in key, default(FastBufferWriter.ForPrimitives));
				fastBufferWriter.WriteNetworkSerializable<NpcState>(in simulatedState);
				NetworkManager.Singleton.CustomMessagingManager.SendNamedMessageToAll("AiState", fastBufferWriter, NetworkDelivery.Unreliable);
			}
		}

		// Token: 0x06000B8F RID: 2959 RVA: 0x0003FE10 File Offset: 0x0003E010
		public static void Receive(FastBufferReader reader)
		{
			uint num;
			reader.ReadValueSafe<uint>(out num, default(FastBufferWriter.ForPrimitives));
			NpcState npcState;
			reader.ReadNetworkSerializable<NpcState>(out npcState);
			IndividualStateUpdater.ReceiveAiStates(num, npcState);
		}

		// Token: 0x04000ACA RID: 2762
		private uint netFrame;

		// Token: 0x04000ACB RID: 2763
		public Vector3 Position;

		// Token: 0x04000ACC RID: 2764
		public float Rotation;

		// Token: 0x04000ACD RID: 2765
		public int attack;

		// Token: 0x04000ACE RID: 2766
		public bool jumped;

		// Token: 0x04000ACF RID: 2767
		public int taunt;

		// Token: 0x04000AD0 RID: 2768
		public int fidget;

		// Token: 0x04000AD1 RID: 2769
		public bool isMoving;

		// Token: 0x04000AD2 RID: 2770
		public bool isGrounded;

		// Token: 0x04000AD3 RID: 2771
		public bool isAirborne;

		// Token: 0x04000AD4 RID: 2772
		public float slopeAngle;

		// Token: 0x04000AD5 RID: 2773
		public float airTime;

		// Token: 0x04000AD6 RID: 2774
		public float fallTime;

		// Token: 0x04000AD7 RID: 2775
		public bool zLock;

		// Token: 0x04000AD8 RID: 2776
		public float VelX;

		// Token: 0x04000AD9 RID: 2777
		public float VelY;

		// Token: 0x04000ADA RID: 2778
		public float VelZ;

		// Token: 0x04000ADB RID: 2779
		public float AngularVelocity;

		// Token: 0x04000ADC RID: 2780
		public float HorizVelMagnitude;

		// Token: 0x04000ADD RID: 2781
		public bool landed;

		// Token: 0x04000ADE RID: 2782
		public bool LargePush;

		// Token: 0x04000ADF RID: 2783
		public bool PhysicsForceExit;

		// Token: 0x04000AE0 RID: 2784
		public bool LoopSmallPush;

		// Token: 0x04000AE1 RID: 2785
		public bool SmallPush;

		// Token: 0x04000AE2 RID: 2786
		public bool EndSmallPush;

		// Token: 0x04000AE3 RID: 2787
		public bool ImminentlyAttacking;

		// Token: 0x04000AE4 RID: 2788
		public bool IsAwaitingTeleport;

		// Token: 0x04000AE5 RID: 2789
		public bool walking;
	}
}
