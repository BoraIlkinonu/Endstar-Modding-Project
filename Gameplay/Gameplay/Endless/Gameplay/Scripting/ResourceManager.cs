using System;
using Endless.Gameplay.LuaEnums;
using Endless.Shared;

namespace Endless.Gameplay.Scripting
{
	// Token: 0x0200049E RID: 1182
	public class ResourceManager
	{
		// Token: 0x170005A5 RID: 1445
		// (get) Token: 0x06001D16 RID: 7446 RVA: 0x0007EFC1 File Offset: 0x0007D1C1
		internal static ResourceManager Instance
		{
			get
			{
				if (ResourceManager.instance == null)
				{
					ResourceManager.instance = new ResourceManager();
				}
				return ResourceManager.instance;
			}
		}

		// Token: 0x06001D17 RID: 7447 RVA: 0x0007EFD9 File Offset: 0x0007D1D9
		public void SetResourceCollectionRule(Context instigator, ResourceLibraryReference resourceReference, int rule)
		{
			NetworkBehaviourSingleton<ResourceManager>.Instance.SetCollectionRule(resourceReference, (ResourceCollectionRule)rule);
		}

		// Token: 0x06001D18 RID: 7448 RVA: 0x0007EFE8 File Offset: 0x0007D1E8
		public void ClearAllResources(Context instigator)
		{
			NetworkBehaviourSingleton<ResourceManager>.Instance.ClearAllResourcesForAllPlayers();
		}

		// Token: 0x06001D19 RID: 7449 RVA: 0x0007EFF4 File Offset: 0x0007D1F4
		public void ClearResource(Context instigator, ResourceLibraryReference resourceReference)
		{
			NetworkBehaviourSingleton<ResourceManager>.Instance.ClearResourceForAllPlayers(resourceReference);
		}

		// Token: 0x06001D1A RID: 7450 RVA: 0x0007F004 File Offset: 0x0007D204
		public void ClearAllResourcesForPlayer(Context instigator, Context playerContext)
		{
			PlayerLuaComponent playerLuaComponent;
			if (playerContext.WorldObject.TryGetUserComponent<PlayerLuaComponent>(out playerLuaComponent))
			{
				NetworkBehaviourSingleton<ResourceManager>.Instance.ClearAllResourcesForPlayer(playerLuaComponent.WorldObject.NetworkObject.OwnerClientId);
			}
		}

		// Token: 0x06001D1B RID: 7451 RVA: 0x0007F03C File Offset: 0x0007D23C
		public void ClearResourceForPlayer(Context instigator, ResourceLibraryReference resourceReference, Context playerContext)
		{
			PlayerLuaComponent playerLuaComponent;
			if (playerContext.WorldObject.TryGetUserComponent<PlayerLuaComponent>(out playerLuaComponent))
			{
				NetworkBehaviourSingleton<ResourceManager>.Instance.ClearResourceForPlayer(resourceReference, playerLuaComponent.WorldObject.NetworkObject.OwnerClientId);
			}
		}

		// Token: 0x040016D2 RID: 5842
		private static ResourceManager instance;
	}
}
