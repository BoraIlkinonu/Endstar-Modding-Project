using System;
using System.Collections;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Gameplay.UI
{
	// Token: 0x020003A9 RID: 937
	public class UIGameplayVisibilityHandler : UIGameObject
	{
		// Token: 0x060017EA RID: 6122 RVA: 0x0006F3C0 File Offset: 0x0006D5C0
		private IEnumerator Start()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			UIDisplayAndHideHandler[] array = this.hideOnGameplayCleanup;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].SetToHideEnd(true);
			}
			MonoBehaviourSingleton<GameplayManager>.Instance.OnGameplayStarted.AddListener(new UnityAction(this.OnGameplayStarted));
			MonoBehaviourSingleton<GameplayManager>.Instance.OnGameplayCleanup.AddListener(new UnityAction(this.OnGameplayCleanup));
			MonoBehaviourSingleton<GameplayManager>.Instance.OnGameplayStopped.AddListener(new UnityAction(this.OnGameplayStopped));
			yield return new WaitForEndOfFrame();
			this.rootDisplayAndHideHandler.SetToHideEnd(true);
			yield break;
		}

		// Token: 0x060017EB RID: 6123 RVA: 0x0006F3D0 File Offset: 0x0006D5D0
		private void OnGameplayStarted()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnGameplayStarted", Array.Empty<object>());
			}
			UIDisplayAndHideHandler[] array = this.displayOnGameplayStarted;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Display();
			}
			UIScreenManager.OnScreenSystemOpen = (Action)Delegate.Combine(UIScreenManager.OnScreenSystemOpen, new Action(this.HideRootGameplayUi));
			UIScreenManager.OnScreenSystemClose = (Action)Delegate.Combine(UIScreenManager.OnScreenSystemClose, new Action(this.DisplayRootGameplayUi));
			if (!MonoBehaviourSingleton<UIScreenManager>.Instance.IsDisplaying)
			{
				this.DisplayRootGameplayUi();
			}
		}

		// Token: 0x060017EC RID: 6124 RVA: 0x0006F464 File Offset: 0x0006D664
		private void OnGameplayCleanup()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnGameplayCleanup", Array.Empty<object>());
			}
			this.HideAllGameplayUi();
		}

		// Token: 0x060017ED RID: 6125 RVA: 0x0006F484 File Offset: 0x0006D684
		private void OnGameplayStopped()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnGameplayStopped", Array.Empty<object>());
			}
			UIScreenManager.OnScreenSystemOpen = (Action)Delegate.Remove(UIScreenManager.OnScreenSystemOpen, new Action(this.HideRootGameplayUi));
			UIScreenManager.OnScreenSystemClose = (Action)Delegate.Remove(UIScreenManager.OnScreenSystemClose, new Action(this.DisplayRootGameplayUi));
			this.HideAllGameplayUi();
		}

		// Token: 0x060017EE RID: 6126 RVA: 0x0006F4EF File Offset: 0x0006D6EF
		private void DisplayRootGameplayUi()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "DisplayRootGameplayUi", Array.Empty<object>());
			}
			this.rootDisplayAndHideHandler.Display();
		}

		// Token: 0x060017EF RID: 6127 RVA: 0x0006F514 File Offset: 0x0006D714
		private void HideAllGameplayUi()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "HideRootGameplayUi", Array.Empty<object>());
			}
			this.HideRootGameplayUi();
			foreach (UIDisplayAndHideHandler uidisplayAndHideHandler in this.hideOnGameplayCleanup)
			{
				if (uidisplayAndHideHandler != null)
				{
					uidisplayAndHideHandler.Hide();
				}
			}
		}

		// Token: 0x060017F0 RID: 6128 RVA: 0x0006F562 File Offset: 0x0006D762
		private void HideRootGameplayUi()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "HideRootGameplayUi", Array.Empty<object>());
			}
			this.rootDisplayAndHideHandler.Hide();
		}

		// Token: 0x04001337 RID: 4919
		[SerializeField]
		private UIDisplayAndHideHandler rootDisplayAndHideHandler;

		// Token: 0x04001338 RID: 4920
		[SerializeField]
		private UIDisplayAndHideHandler[] displayOnGameplayStarted = Array.Empty<UIDisplayAndHideHandler>();

		// Token: 0x04001339 RID: 4921
		[SerializeField]
		private UIDisplayAndHideHandler[] hideOnGameplayCleanup = Array.Empty<UIDisplayAndHideHandler>();

		// Token: 0x0400133A RID: 4922
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;
	}
}
