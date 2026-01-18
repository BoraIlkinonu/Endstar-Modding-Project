using Endless.Gameplay.LuaEnums;
using UnityEngine;

namespace Endless.Gameplay;

public class TeleportComponent : MonoBehaviour
{
	public enum TeleportStatusType : byte
	{
		None,
		ActiveTeleport,
		WorldFallOff,
		OnCooldown
	}

	public enum TeleportType
	{
		Regular,
		WorldFallOff
	}

	public const uint TELEPORT_DELAY_FRAMES = 24u;

	public const uint WORLD_FALL_OFF_DELAY = 10u;

	public const uint TELEPORT_COOLDOWN_FRAMES = 80u;

	private static RaycastHit[] hitPool = new RaycastHit[20];

	public uint TeleportReadyFrame { get; protected set; }

	public bool TeleportTriggered(ref NetState state, Vector3 teleportPosition, bool ignoreCooldown, TeleportType teleportType = TeleportType.Regular, uint frameDelay = 24u, bool overrideRotation = false, float rotation = 0f, Endless.Gameplay.LuaEnums.TeleportType gameplayTeleportType = Endless.Gameplay.LuaEnums.TeleportType.Instant, bool snapCamera = false)
	{
		if (!ignoreCooldown && state.TeleportStatus == TeleportStatusType.OnCooldown)
		{
			return false;
		}
		if (state.TeleportStatus == TeleportStatusType.None || (ignoreCooldown && state.TeleportStatus == TeleportStatusType.OnCooldown) || (teleportType == TeleportType.WorldFallOff && state.TeleportStatus != TeleportStatusType.WorldFallOff))
		{
			if (teleportType == TeleportType.WorldFallOff)
			{
				state.TeleportStatus = TeleportStatusType.WorldFallOff;
				state.FallFrames = 40;
				state.AirborneFrames = 0;
			}
			else
			{
				state.TeleportStatus = TeleportStatusType.ActiveTeleport;
			}
			state.TeleportAtFrame = NetClock.CurrentFrame + frameDelay;
			TeleportReadyFrame = state.TeleportAtFrame + 80;
			state.TeleportHasRotation = overrideRotation;
			state.TeleportRotation = rotation;
			state.TeleportPosition = teleportPosition;
			if (teleportType == TeleportType.Regular)
			{
				state.GameplayTeleport = true;
				state.GameplayTeleportType = gameplayTeleportType;
				TeleportReadyFrame = state.TeleportAtFrame;
				state.TeleportRotationSnapCamera = snapCamera;
			}
			return true;
		}
		return false;
	}

	public void WorldFallOffTriggered(ref NetState state, Vector3 teleportPosition)
	{
		TeleportTriggered(ref state, teleportPosition, ignoreCooldown: true, TeleportType.WorldFallOff, 10u);
	}
}
