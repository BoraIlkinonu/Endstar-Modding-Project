using Endless.Gameplay.LuaEnums;
using Endless.Gameplay.Scripting;
using UnityEngine;

namespace Endless.Gameplay;

public class MaterializeTeleportInfo : TeleportInfo
{
	[SerializeField]
	private uint frames = 20u;

	public override TeleportType TeleportType => TeleportType.Materialize;

	public override uint FramesToTeleport => frames;

	public override void TeleportStart(EndlessVisuals endlessVisuals, Animator animator, UnityEngine.Vector3 position)
	{
		if ((bool)endlessVisuals)
		{
			endlessVisuals.FadeOut();
		}
	}

	public override void TeleportEnd(EndlessVisuals endlessVisuals, Animator animator, UnityEngine.Vector3 position)
	{
		if ((bool)endlessVisuals)
		{
			endlessVisuals.FadeIn();
		}
	}
}
