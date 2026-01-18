using System;
using Endless.Shared;

namespace Endless.Gameplay.Scripting
{
	// Token: 0x0200049D RID: 1181
	public class CameraFadeManager
	{
		// Token: 0x170005A4 RID: 1444
		// (get) Token: 0x06001D10 RID: 7440 RVA: 0x0007EF1E File Offset: 0x0007D11E
		internal static CameraFadeManager Instance
		{
			get
			{
				if (CameraFadeManager.instance == null)
				{
					CameraFadeManager.instance = new CameraFadeManager();
				}
				return CameraFadeManager.instance;
			}
		}

		// Token: 0x06001D11 RID: 7441 RVA: 0x0007EF38 File Offset: 0x0007D138
		public void FadePlayerIn(Context instigator, Context playerContext, float duration)
		{
			PlayerLuaComponent playerLuaComponent;
			if (playerContext.WorldObject.TryGetUserComponent<PlayerLuaComponent>(out playerLuaComponent))
			{
				NetworkBehaviourSingleton<CameraFadeManager>.Instance.SendFadeIn(playerLuaComponent.WorldObject.NetworkObject.OwnerClientId, duration);
			}
		}

		// Token: 0x06001D12 RID: 7442 RVA: 0x0007EF70 File Offset: 0x0007D170
		public void FadePlayerOut(Context instigator, Context playerContext, float duration)
		{
			PlayerLuaComponent playerLuaComponent;
			if (playerContext.WorldObject.TryGetUserComponent<PlayerLuaComponent>(out playerLuaComponent))
			{
				NetworkBehaviourSingleton<CameraFadeManager>.Instance.SendFadeOut(playerLuaComponent.WorldObject.NetworkObject.OwnerClientId, duration);
			}
		}

		// Token: 0x06001D13 RID: 7443 RVA: 0x0007EFA7 File Offset: 0x0007D1A7
		public void FadeAllIn(Context instigator, float duration)
		{
			NetworkBehaviourSingleton<CameraFadeManager>.Instance.SendFadeInGlobal(duration);
		}

		// Token: 0x06001D14 RID: 7444 RVA: 0x0007EFB4 File Offset: 0x0007D1B4
		public void FadeAllOut(Context instigator, float duration)
		{
			NetworkBehaviourSingleton<CameraFadeManager>.Instance.SendFadeOutGlobal(duration);
		}

		// Token: 0x040016D1 RID: 5841
		private static CameraFadeManager instance;
	}
}
