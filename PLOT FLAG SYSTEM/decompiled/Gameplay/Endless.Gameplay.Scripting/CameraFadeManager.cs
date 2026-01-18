using Endless.Shared;

namespace Endless.Gameplay.Scripting;

public class CameraFadeManager
{
	private static CameraFadeManager instance;

	internal static CameraFadeManager Instance
	{
		get
		{
			if (instance == null)
			{
				instance = new CameraFadeManager();
			}
			return instance;
		}
	}

	public void FadePlayerIn(Context instigator, Context playerContext, float duration)
	{
		if (playerContext.WorldObject.TryGetUserComponent<PlayerLuaComponent>(out var component))
		{
			NetworkBehaviourSingleton<Endless.Gameplay.CameraFadeManager>.Instance.SendFadeIn(component.WorldObject.NetworkObject.OwnerClientId, duration);
		}
	}

	public void FadePlayerOut(Context instigator, Context playerContext, float duration)
	{
		if (playerContext.WorldObject.TryGetUserComponent<PlayerLuaComponent>(out var component))
		{
			NetworkBehaviourSingleton<Endless.Gameplay.CameraFadeManager>.Instance.SendFadeOut(component.WorldObject.NetworkObject.OwnerClientId, duration);
		}
	}

	public void FadeAllIn(Context instigator, float duration)
	{
		NetworkBehaviourSingleton<Endless.Gameplay.CameraFadeManager>.Instance.SendFadeInGlobal(duration);
	}

	public void FadeAllOut(Context instigator, float duration)
	{
		NetworkBehaviourSingleton<Endless.Gameplay.CameraFadeManager>.Instance.SendFadeOutGlobal(duration);
	}
}
