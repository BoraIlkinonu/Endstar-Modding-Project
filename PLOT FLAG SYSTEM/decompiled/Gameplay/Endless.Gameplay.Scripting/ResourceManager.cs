using Endless.Gameplay.LuaEnums;
using Endless.Shared;

namespace Endless.Gameplay.Scripting;

public class ResourceManager
{
	private static ResourceManager instance;

	internal static ResourceManager Instance
	{
		get
		{
			if (instance == null)
			{
				instance = new ResourceManager();
			}
			return instance;
		}
	}

	public void SetResourceCollectionRule(Context instigator, ResourceLibraryReference resourceReference, int rule)
	{
		NetworkBehaviourSingleton<Endless.Gameplay.ResourceManager>.Instance.SetCollectionRule(resourceReference, (ResourceCollectionRule)rule);
	}

	public void ClearAllResources(Context instigator)
	{
		NetworkBehaviourSingleton<Endless.Gameplay.ResourceManager>.Instance.ClearAllResourcesForAllPlayers();
	}

	public void ClearResource(Context instigator, ResourceLibraryReference resourceReference)
	{
		NetworkBehaviourSingleton<Endless.Gameplay.ResourceManager>.Instance.ClearResourceForAllPlayers(resourceReference);
	}

	public void ClearAllResourcesForPlayer(Context instigator, Context playerContext)
	{
		if (playerContext.WorldObject.TryGetUserComponent<PlayerLuaComponent>(out var component))
		{
			NetworkBehaviourSingleton<Endless.Gameplay.ResourceManager>.Instance.ClearAllResourcesForPlayer(component.WorldObject.NetworkObject.OwnerClientId);
		}
	}

	public void ClearResourceForPlayer(Context instigator, ResourceLibraryReference resourceReference, Context playerContext)
	{
		if (playerContext.WorldObject.TryGetUserComponent<PlayerLuaComponent>(out var component))
		{
			NetworkBehaviourSingleton<Endless.Gameplay.ResourceManager>.Instance.ClearResourceForPlayer(resourceReference, component.WorldObject.NetworkObject.OwnerClientId);
		}
	}
}
