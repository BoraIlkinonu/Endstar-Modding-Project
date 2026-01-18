using System;
using Endless.Gameplay.LuaEnums;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x020002C7 RID: 711
	public class TeleportComponent : MonoBehaviour
	{
		// Token: 0x17000333 RID: 819
		// (get) Token: 0x06001030 RID: 4144 RVA: 0x00052211 File Offset: 0x00050411
		// (set) Token: 0x06001031 RID: 4145 RVA: 0x00052219 File Offset: 0x00050419
		public uint TeleportReadyFrame { get; protected set; }

		// Token: 0x06001032 RID: 4146 RVA: 0x00052224 File Offset: 0x00050424
		public bool TeleportTriggered(ref NetState state, Vector3 teleportPosition, bool ignoreCooldown, TeleportComponent.TeleportType teleportType = TeleportComponent.TeleportType.Regular, uint frameDelay = 24U, bool overrideRotation = false, float rotation = 0f, Endless.Gameplay.LuaEnums.TeleportType gameplayTeleportType = Endless.Gameplay.LuaEnums.TeleportType.Instant, bool snapCamera = false)
		{
			if (!ignoreCooldown && state.TeleportStatus == TeleportComponent.TeleportStatusType.OnCooldown)
			{
				return false;
			}
			if (state.TeleportStatus == TeleportComponent.TeleportStatusType.None || (ignoreCooldown && state.TeleportStatus == TeleportComponent.TeleportStatusType.OnCooldown) || (teleportType == TeleportComponent.TeleportType.WorldFallOff && state.TeleportStatus != TeleportComponent.TeleportStatusType.WorldFallOff))
			{
				if (teleportType == TeleportComponent.TeleportType.WorldFallOff)
				{
					state.TeleportStatus = TeleportComponent.TeleportStatusType.WorldFallOff;
					state.FallFrames = 40;
					state.AirborneFrames = 0;
				}
				else
				{
					state.TeleportStatus = TeleportComponent.TeleportStatusType.ActiveTeleport;
				}
				state.TeleportAtFrame = NetClock.CurrentFrame + frameDelay;
				this.TeleportReadyFrame = state.TeleportAtFrame + 80U;
				state.TeleportHasRotation = overrideRotation;
				state.TeleportRotation = rotation;
				state.TeleportPosition = teleportPosition;
				if (teleportType == TeleportComponent.TeleportType.Regular)
				{
					state.GameplayTeleport = true;
					state.GameplayTeleportType = gameplayTeleportType;
					this.TeleportReadyFrame = state.TeleportAtFrame;
					state.TeleportRotationSnapCamera = snapCamera;
				}
				return true;
			}
			return false;
		}

		// Token: 0x06001033 RID: 4147 RVA: 0x000522EC File Offset: 0x000504EC
		public void WorldFallOffTriggered(ref NetState state, Vector3 teleportPosition)
		{
			this.TeleportTriggered(ref state, teleportPosition, true, TeleportComponent.TeleportType.WorldFallOff, 10U, false, 0f, Endless.Gameplay.LuaEnums.TeleportType.Instant, false);
		}

		// Token: 0x04000DD9 RID: 3545
		public const uint TELEPORT_DELAY_FRAMES = 24U;

		// Token: 0x04000DDA RID: 3546
		public const uint WORLD_FALL_OFF_DELAY = 10U;

		// Token: 0x04000DDB RID: 3547
		public const uint TELEPORT_COOLDOWN_FRAMES = 80U;

		// Token: 0x04000DDC RID: 3548
		private static RaycastHit[] hitPool = new RaycastHit[20];

		// Token: 0x020002C8 RID: 712
		public enum TeleportStatusType : byte
		{
			// Token: 0x04000DDF RID: 3551
			None,
			// Token: 0x04000DE0 RID: 3552
			ActiveTeleport,
			// Token: 0x04000DE1 RID: 3553
			WorldFallOff,
			// Token: 0x04000DE2 RID: 3554
			OnCooldown
		}

		// Token: 0x020002C9 RID: 713
		public enum TeleportType
		{
			// Token: 0x04000DE4 RID: 3556
			Regular,
			// Token: 0x04000DE5 RID: 3557
			WorldFallOff
		}
	}
}
