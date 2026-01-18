using System;
using Endless.Data;
using Endless.Shared;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Endless.Gameplay
{
	// Token: 0x020000EB RID: 235
	public class DevLog : EndlessBehaviour, IStartSubscriber, IGameEndSubscriber
	{
		// Token: 0x06000549 RID: 1353 RVA: 0x0001B420 File Offset: 0x00019620
		private void Awake()
		{
			this.playerInputActions = new PlayerInputActions();
		}

		// Token: 0x0600054A RID: 1354 RVA: 0x0001B430 File Offset: 0x00019630
		private void OnEnable()
		{
			this.playerInputActions.Player.ToggleDevLog.performed += this.ToggleDevLog;
			this.playerInputActions.Player.ToggleDevLog.Enable();
		}

		// Token: 0x0600054B RID: 1355 RVA: 0x0001B47C File Offset: 0x0001967C
		private void OnDisable()
		{
			this.playerInputActions.Player.ToggleDevLog.performed -= this.ToggleDevLog;
			this.playerInputActions.Player.ToggleDevLog.Disable();
		}

		// Token: 0x0600054C RID: 1356 RVA: 0x0001B4C5 File Offset: 0x000196C5
		private void ToggleDevLog(InputAction.CallbackContext callbackContext)
		{
			if (!MonoBehaviourSingleton<GameplayManager>.Instance.IsPlaying)
			{
				return;
			}
			this.shown = !this.shown;
			if (this.shown)
			{
				this.Show();
				return;
			}
			this.Hide();
		}

		// Token: 0x0600054D RID: 1357 RVA: 0x0001B4F8 File Offset: 0x000196F8
		public void EndlessStart()
		{
			this.shown = false;
			this.Hide();
		}

		// Token: 0x0600054E RID: 1358 RVA: 0x0001B507 File Offset: 0x00019707
		public void EndlessGameEnd()
		{
			this.Show();
		}

		// Token: 0x0600054F RID: 1359 RVA: 0x0001B510 File Offset: 0x00019710
		private void Show()
		{
			Renderer[] array = this.renderersToManage;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].enabled = true;
			}
			Collider[] array2 = this.collidersToManage;
			for (int i = 0; i < array2.Length; i++)
			{
				array2[i].enabled = true;
			}
		}

		// Token: 0x06000550 RID: 1360 RVA: 0x0001B55C File Offset: 0x0001975C
		private void Hide()
		{
			Renderer[] array = this.renderersToManage;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].enabled = false;
			}
			Collider[] array2 = this.collidersToManage;
			for (int i = 0; i < array2.Length; i++)
			{
				array2[i].enabled = false;
			}
		}

		// Token: 0x0400040D RID: 1037
		[SerializeField]
		private Renderer[] renderersToManage;

		// Token: 0x0400040E RID: 1038
		[SerializeField]
		private Collider[] collidersToManage;

		// Token: 0x0400040F RID: 1039
		private bool shown;

		// Token: 0x04000410 RID: 1040
		private PlayerInputActions playerInputActions;
	}
}
