using System;
using System.Collections;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Creator.UI
{
	// Token: 0x020000B6 RID: 182
	public class UICreatorVisibilityHandler : UIGameObject
	{
		// Token: 0x060002D6 RID: 726 RVA: 0x00012ACC File Offset: 0x00010CCC
		private IEnumerator Start()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			UIDisplayAndHideHandler[] array = this.hideOnCreatorEnded;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].SetToHideEnd(true);
			}
			NetworkBehaviourSingleton<CreatorManager>.Instance.OnCreatorStarted.AddListener(new UnityAction(this.OnCreatorStarted));
			NetworkBehaviourSingleton<CreatorManager>.Instance.OnCreatorEnded.AddListener(new UnityAction(this.OnCreatorEnded));
			yield return new WaitForEndOfFrame();
			this.rootDisplayAndHideHandler.SetToHideEnd(true);
			yield break;
		}

		// Token: 0x060002D7 RID: 727 RVA: 0x00012ADC File Offset: 0x00010CDC
		private void OnCreatorStarted()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnCreatorStarted", Array.Empty<object>());
			}
			UIDisplayAndHideHandler[] array = this.displayOnCreatorStarted;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Display();
			}
			UIScreenManager.OnScreenSystemOpen = (Action)Delegate.Combine(UIScreenManager.OnScreenSystemOpen, new Action(this.Hide));
			UIScreenManager.OnScreenSystemClose = (Action)Delegate.Combine(UIScreenManager.OnScreenSystemClose, new Action(this.Display));
			if (!MonoBehaviourSingleton<UIScreenManager>.Instance.IsDisplaying)
			{
				this.Display();
			}
		}

		// Token: 0x060002D8 RID: 728 RVA: 0x00012B70 File Offset: 0x00010D70
		private void OnCreatorEnded()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnCreatorEnded", Array.Empty<object>());
			}
			this.Hide();
			UIDisplayAndHideHandler[] array = this.hideOnCreatorEnded;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Hide();
			}
			UIScreenManager.OnScreenSystemOpen = (Action)Delegate.Remove(UIScreenManager.OnScreenSystemOpen, new Action(this.Hide));
			UIScreenManager.OnScreenSystemClose = (Action)Delegate.Remove(UIScreenManager.OnScreenSystemClose, new Action(this.Display));
		}

		// Token: 0x060002D9 RID: 729 RVA: 0x00012BF8 File Offset: 0x00010DF8
		private void Display()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Display", Array.Empty<object>());
			}
			this.rootDisplayAndHideHandler.Display();
		}

		// Token: 0x060002DA RID: 730 RVA: 0x00012C1D File Offset: 0x00010E1D
		private void Hide()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Hide", Array.Empty<object>());
			}
			this.rootDisplayAndHideHandler.Hide();
		}

		// Token: 0x04000304 RID: 772
		[SerializeField]
		private UIDisplayAndHideHandler rootDisplayAndHideHandler;

		// Token: 0x04000305 RID: 773
		[SerializeField]
		private UIDisplayAndHideHandler[] displayOnCreatorStarted = new UIDisplayAndHideHandler[0];

		// Token: 0x04000306 RID: 774
		[SerializeField]
		private UIDisplayAndHideHandler[] hideOnCreatorEnded = new UIDisplayAndHideHandler[0];

		// Token: 0x04000307 RID: 775
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;
	}
}
