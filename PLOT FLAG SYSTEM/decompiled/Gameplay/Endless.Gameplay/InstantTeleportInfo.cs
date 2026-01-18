using Endless.Gameplay.LuaEnums;
using Endless.Gameplay.Scripting;
using UnityEngine;

namespace Endless.Gameplay;

public class InstantTeleportInfo : TeleportInfo
{
	[SerializeField]
	private uint frames = 8u;

	public override TeleportType TeleportType => TeleportType.Instant;

	public override uint FramesToTeleport => frames;

	public override void TeleportStart(EndlessVisuals endlessVisuals, Animator animator, UnityEngine.Vector3 position)
	{
	}

	public override void TeleportEnd(EndlessVisuals endlessVisuals, Animator animator, UnityEngine.Vector3 position)
	{
	}
}
