using System;
using System.Collections.Generic;
using Endless.Gameplay;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.Tweens;
using Endless.Shared.UI;
using Endless.Shared.Validation;
using UnityEngine;

namespace Endless.Core.UI
{
	// Token: 0x02000092 RID: 146
	public class UIScreenBackgroundManager : UIMonoBehaviourSingleton<UIScreenBackgroundManager>, IValidatable
	{
		// Token: 0x060002F5 RID: 757 RVA: 0x0000FE4C File Offset: 0x0000E04C
		private void Start()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			this.screenBackgrounds = base.GetComponentsInChildren<UIScreenBackground>();
			foreach (UIScreenBackground uiscreenBackground in this.screenBackgrounds)
			{
				Type type = uiscreenBackground.Key.GetType();
				this.screenBackgroundDictionary.Add(type, uiscreenBackground);
			}
			this.renderers = base.GetComponentsInChildren<Renderer>();
			this.materialPropertyBlock = new MaterialPropertyBlock();
			UIBaseScreenView.DisplayBegunAction = (Action<UIBaseScreenView>)Delegate.Combine(UIBaseScreenView.DisplayBegunAction, new Action<UIBaseScreenView>(this.Display));
			UIBaseScreenView.CloseBegunAction = (Action<UIBaseScreenView>)Delegate.Combine(UIBaseScreenView.CloseBegunAction, new Action<UIBaseScreenView>(this.HideBackground));
			UIScreenManager.OnScreenSystemClose = (Action)Delegate.Combine(UIScreenManager.OnScreenSystemClose, new Action(this.Hide));
		}

		// Token: 0x060002F6 RID: 758 RVA: 0x0000FF26 File Offset: 0x0000E126
		[ContextMenu("Validate")]
		public void Validate()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Validate", Array.Empty<object>());
			}
			this.screenBackgrounds = base.GetComponentsInChildren<UIScreenBackground>();
			DebugUtility.DebugHasDuplicates<UIScreenBackground>(this.screenBackgrounds, "screenBackgrounds", this);
		}

		// Token: 0x060002F7 RID: 759 RVA: 0x0000FF60 File Offset: 0x0000E160
		public T GetScreenBackground<T>() where T : UIScreenBackground
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "GetScreenBackground", Array.Empty<object>());
			}
			UIScreenBackground[] array = this.screenBackgrounds;
			for (int i = 0; i < array.Length; i++)
			{
				T t = array[i] as T;
				if (t != null)
				{
					return t;
				}
			}
			return default(T);
		}

		// Token: 0x060002F8 RID: 760 RVA: 0x0000FFBC File Offset: 0x0000E1BC
		private void Display(UIBaseScreenView screenDisplaying)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Display", new object[] { screenDisplaying.gameObject.name });
			}
			Type type = screenDisplaying.GetType();
			if (!this.screenBackgroundDictionary.ContainsKey(type))
			{
				return;
			}
			this.screenBackgroundDictionary[type].Display();
			this.SetState(UIScreenBackgroundManager.States.Displaying);
		}

		// Token: 0x060002F9 RID: 761 RVA: 0x00010020 File Offset: 0x0000E220
		private void HideBackground(UIBaseScreenView screenClosing)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "HideBackground", new object[] { screenClosing.gameObject.name });
			}
			if (!screenClosing)
			{
				DebugUtility.LogError(this, "HideBackground", "There was no screenClosing!", Array.Empty<object>());
				return;
			}
			Type type = screenClosing.GetType();
			if (!this.screenBackgroundDictionary.ContainsKey(type))
			{
				return;
			}
			this.screenBackgroundDictionary[type].Hide();
		}

		// Token: 0x060002FA RID: 762 RVA: 0x00010099 File Offset: 0x0000E299
		private void Hide()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Hide", Array.Empty<object>());
			}
			this.SetState(UIScreenBackgroundManager.States.Hiding);
		}

		// Token: 0x060002FB RID: 763 RVA: 0x000100BC File Offset: 0x0000E2BC
		private void SetState(UIScreenBackgroundManager.States newValue)
		{
			if (this.state == newValue)
			{
				return;
			}
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "SetState", new object[] { newValue });
			}
			this.state = newValue;
			if (this.alphaTweenCoroutine != null)
			{
				base.StopCoroutine(this.alphaTweenCoroutine);
				this.alphaTweenCoroutine = null;
			}
			if (this.tweenLocalScale.IsTweening)
			{
				this.tweenLocalScale.Cancel();
			}
			switch (this.state)
			{
			case UIScreenBackgroundManager.States.Hidden:
				base.gameObject.SetActive(false);
				return;
			case UIScreenBackgroundManager.States.Hiding:
				MonoBehaviourSingleton<CameraController>.Instance.SetMainCamera(CameraController.CameraInstanceType.Gameplay);
				this.SetStateToHidden();
				return;
			case UIScreenBackgroundManager.States.Displaying:
				MonoBehaviourSingleton<CameraController>.Instance.SetMainCamera(CameraController.CameraInstanceType.Menu);
				base.gameObject.SetActive(true);
				this.SetStateToDisplayed();
				break;
			case UIScreenBackgroundManager.States.Displayed:
				break;
			default:
				return;
			}
		}

		// Token: 0x060002FC RID: 764 RVA: 0x0001018C File Offset: 0x0000E38C
		private void SetAlpha(float alpha)
		{
			foreach (Renderer renderer in this.renderers)
			{
				renderer.GetPropertyBlock(this.materialPropertyBlock);
				this.materialPropertyBlock.SetFloat("_Alpha", alpha);
				renderer.SetPropertyBlock(this.materialPropertyBlock);
			}
		}

		// Token: 0x060002FD RID: 765 RVA: 0x000101D9 File Offset: 0x0000E3D9
		private void SetStateToHidden()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "SetStateToHidden", Array.Empty<object>());
			}
			this.SetState(UIScreenBackgroundManager.States.Hidden);
		}

		// Token: 0x060002FE RID: 766 RVA: 0x000101FA File Offset: 0x0000E3FA
		private void SetStateToDisplayed()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "SetStateToDisplayed", Array.Empty<object>());
			}
			this.SetState(UIScreenBackgroundManager.States.Displayed);
		}

		// Token: 0x0400022E RID: 558
		[SerializeField]
		private TweenLocalScale tweenLocalScale;

		// Token: 0x0400022F RID: 559
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;

		// Token: 0x04000230 RID: 560
		private UIScreenBackground[] screenBackgrounds = Array.Empty<UIScreenBackground>();

		// Token: 0x04000231 RID: 561
		private readonly Dictionary<Type, UIScreenBackground> screenBackgroundDictionary = new Dictionary<Type, UIScreenBackground>();

		// Token: 0x04000232 RID: 562
		private Renderer[] renderers = Array.Empty<Renderer>();

		// Token: 0x04000233 RID: 563
		private MaterialPropertyBlock materialPropertyBlock;

		// Token: 0x04000234 RID: 564
		private Coroutine alphaTweenCoroutine;

		// Token: 0x04000235 RID: 565
		private UIScreenBackgroundManager.States state;

		// Token: 0x02000093 RID: 147
		private enum States
		{
			// Token: 0x04000237 RID: 567
			Hidden,
			// Token: 0x04000238 RID: 568
			Hiding,
			// Token: 0x04000239 RID: 569
			Displaying,
			// Token: 0x0400023A RID: 570
			Displayed
		}
	}
}
