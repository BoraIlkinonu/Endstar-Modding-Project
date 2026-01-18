using System;
using Endless.Gameplay.LuaEnums;
using Endless.Shared;
using Unity.Netcode;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x0200010D RID: 269
	public class NetworkRigidbodyAppearanceController : EndlessBehaviour, IStartSubscriber
	{
		// Token: 0x17000108 RID: 264
		// (get) Token: 0x060005F5 RID: 1525 RVA: 0x0001D8DE File Offset: 0x0001BADE
		// (set) Token: 0x060005F6 RID: 1526 RVA: 0x0001D8E6 File Offset: 0x0001BAE6
		public WorldObject WorldObject { get; protected set; }

		// Token: 0x060005F7 RID: 1527 RVA: 0x0001D8F0 File Offset: 0x0001BAF0
		public void InitAppearance(WorldObject worldObject, GameObject appearanceObject)
		{
			this.WorldObject = worldObject;
			this.appearance = appearanceObject;
			appearanceObject.transform.SetParent(base.transform);
			appearanceObject.transform.localEulerAngles = Vector3.zero;
			appearanceObject.transform.localPosition = Vector3.zero;
		}

		// Token: 0x060005F8 RID: 1528 RVA: 0x0001D93C File Offset: 0x0001BB3C
		void IStartSubscriber.EndlessStart()
		{
			if (!this.startVisible && !this.visible)
			{
				this.appearance.SetActive(false);
				return;
			}
			this.visible = true;
		}

		// Token: 0x060005F9 RID: 1529 RVA: 0x0001D962 File Offset: 0x0001BB62
		public void AddState(RigidbodyState state)
		{
			if (this.startFrame == 0U)
			{
				this.startFrame = state.NetFrame;
			}
			this.stateRingBuffer.UpdateValue(ref state);
		}

		// Token: 0x060005FA RID: 1530 RVA: 0x0001D988 File Offset: 0x0001BB88
		private void Update()
		{
			if (!MonoBehaviourSingleton<GameplayManager>.Instance.IsPlaying)
			{
				base.transform.localPosition = Vector3.zero;
				base.transform.localRotation = Quaternion.identity;
				return;
			}
			this.stateRingBuffer.ActiveInterpolationTime = (NetworkManager.Singleton.IsServer ? NetClock.ServerAppearanceTime : NetClock.ClientExtrapolatedAppearanceTime);
			if (this.appearance && !this.visible && this.startFrame != 0U && this.stateRingBuffer.PastInterpolationState.NetFrame >= this.startFrame)
			{
				this.appearance.SetActive(true);
				this.visible = true;
			}
			if (this.stateRingBuffer.PastInterpolationState.Teleporting && !this.stateRingBuffer.NextInterpolationState.Teleporting)
			{
				base.transform.position = this.stateRingBuffer.NextInterpolationState.Position;
			}
			else
			{
				base.transform.position = Vector3.Lerp(this.stateRingBuffer.PastInterpolationState.Position, this.stateRingBuffer.NextInterpolationState.Position, this.stateRingBuffer.ActiveStateLerpTime);
			}
			base.transform.rotation = Quaternion.Lerp(Quaternion.Euler(this.stateRingBuffer.PastInterpolationState.Angles), Quaternion.Euler(this.stateRingBuffer.NextInterpolationState.Angles), this.stateRingBuffer.ActiveStateLerpTime);
			if (this.teleporting && !this.stateRingBuffer.NextInterpolationState.Teleporting)
			{
				this.teleporting = false;
				RuntimeDatabase.GetTeleportInfo(this.activeTeleportType).TeleportEnd(this.WorldObject.EndlessVisuals, null, base.transform.position);
			}
			else if (!this.teleporting && this.stateRingBuffer.NextInterpolationState.Teleporting)
			{
				this.teleporting = true;
				this.activeTeleportType = this.stateRingBuffer.NextInterpolationState.TeleportType;
				RuntimeDatabase.GetTeleportInfo(this.activeTeleportType).TeleportStart(this.WorldObject.EndlessVisuals, null, base.transform.position);
			}
			this.AfterUpdate();
		}

		// Token: 0x060005FB RID: 1531 RVA: 0x00002DB0 File Offset: 0x00000FB0
		protected virtual void AfterUpdate()
		{
		}

		// Token: 0x04000477 RID: 1143
		protected InterpolationRingBuffer<RigidbodyState> stateRingBuffer = new InterpolationRingBuffer<RigidbodyState>(30);

		// Token: 0x04000478 RID: 1144
		[SerializeField]
		protected GameObject appearance;

		// Token: 0x04000479 RID: 1145
		[SerializeField]
		protected bool startVisible;

		// Token: 0x0400047A RID: 1146
		private uint startFrame;

		// Token: 0x0400047B RID: 1147
		private bool visible;

		// Token: 0x0400047C RID: 1148
		private bool teleporting;

		// Token: 0x0400047D RID: 1149
		private TeleportType activeTeleportType;
	}
}
