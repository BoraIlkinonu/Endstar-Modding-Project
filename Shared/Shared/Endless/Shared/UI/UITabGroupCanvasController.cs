using System;
using Endless.Shared.Debugging;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Shared.UI
{
	// Token: 0x0200026D RID: 621
	public class UITabGroupCanvasController : UIGameObject
	{
		// Token: 0x170002F2 RID: 754
		// (get) Token: 0x06000FA1 RID: 4001 RVA: 0x000432F1 File Offset: 0x000414F1
		// (set) Token: 0x06000FA2 RID: 4002 RVA: 0x000432F9 File Offset: 0x000414F9
		public int ActiveTabIndex { get; private set; } = -1;

		// Token: 0x06000FA3 RID: 4003 RVA: 0x00043304 File Offset: 0x00041504
		private void Start()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			this.tabGroup.Interface.OnValueChangedWithIndex.AddListener(new UnityAction<int>(this.SetActiveTabIndex));
			this.SetActiveTabIndex(this.tabGroup.Interface.ValueIndex);
		}

		// Token: 0x06000FA4 RID: 4004 RVA: 0x00043360 File Offset: 0x00041560
		private void OnDestroy()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnDestroy", Array.Empty<object>());
			}
			this.tabGroup.Interface.OnValueChangedWithIndex.RemoveListener(new UnityAction<int>(this.SetActiveTabIndex));
		}

		// Token: 0x06000FA5 RID: 4005 RVA: 0x0004339C File Offset: 0x0004159C
		public void SetActiveTabIndex(int index)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "SetActiveTabIndex", new object[] { index });
			}
			if (index < 0 || index >= this.tabCanvasHandlers.Length)
			{
				Debug.LogError(string.Format("{0} '{1}' is out of range! {2} has a size of {3}!", new object[]
				{
					"index",
					index,
					"tabCanvasHandlers",
					this.tabCanvasHandlers.Length
				}), this);
				return;
			}
			if (index == this.ActiveTabIndex)
			{
				return;
			}
			if (this.enableTransitions && this.tabTransitionHandlers.Length == this.tabCanvasHandlers.Length)
			{
				this.SetActiveTabIndexWithTransition(index);
				return;
			}
			this.SetActiveTabIndexImmediate(index);
		}

		// Token: 0x06000FA6 RID: 4006 RVA: 0x00043450 File Offset: 0x00041650
		private void SetActiveTabIndexImmediate(int index)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "SetActiveTabIndexImmediate", new object[] { index });
			}
			this.ActiveTabIndex = index;
			for (int i = 0; i < this.tabCanvasHandlers.Length; i++)
			{
				this.tabCanvasHandlers[i].Set(i == index);
			}
		}

		// Token: 0x06000FA7 RID: 4007 RVA: 0x000434AC File Offset: 0x000416AC
		private void SetActiveTabIndexWithTransition(int index)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "SetActiveTabIndexWithTransition", new object[] { index });
			}
			int previousIndex = this.ActiveTabIndex;
			this.ActiveTabIndex = index;
			if (previousIndex >= 0 && previousIndex < this.tabTransitionHandlers.Length)
			{
				this.tabTransitionHandlers[previousIndex].Hide(delegate
				{
					if (this.disableInactiveCanvases)
					{
						this.tabCanvasHandlers[previousIndex].Set(false);
					}
				});
			}
			this.tabCanvasHandlers[index].Set(true);
			this.tabTransitionHandlers[index].Display();
		}

		// Token: 0x040009F6 RID: 2550
		[SerializeField]
		private InterfaceReference<IUITabGroup> tabGroup;

		// Token: 0x040009F7 RID: 2551
		[SerializeField]
		private UICanvasHandler[] tabCanvasHandlers = Array.Empty<UICanvasHandler>();

		// Token: 0x040009F8 RID: 2552
		[SerializeField]
		private bool disableInactiveCanvases = true;

		// Token: 0x040009F9 RID: 2553
		[Header("Transitions")]
		[SerializeField]
		private bool enableTransitions = true;

		// Token: 0x040009FA RID: 2554
		[SerializeField]
		private UIDisplayAndHideHandler[] tabTransitionHandlers = Array.Empty<UIDisplayAndHideHandler>();

		// Token: 0x040009FB RID: 2555
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;
	}
}
