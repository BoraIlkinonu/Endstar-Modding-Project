using Endless.Gameplay.LuaEnums;
using Endless.Gameplay.Scripting;
using UnityEngine;

namespace Endless.Gameplay;

public abstract class TeleportInfo : ScriptableObject
{
	public virtual TeleportType TeleportType { get; private set; }

	public virtual uint FramesToTeleport { get; private set; }

	public abstract void TeleportStart(EndlessVisuals endlessVisuals, Animator animator, UnityEngine.Vector3 position);

	public abstract void TeleportEnd(EndlessVisuals endlessVisuals, Animator animator, UnityEngine.Vector3 position);
}
