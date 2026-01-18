using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Endless.Shared.Debugging;
using Endless.Shared.Validation;
using UnityEngine;

namespace Endless.Shared.UI
{
	// Token: 0x020001D1 RID: 465
	public class UIModalManager : UIMonoBehaviourSingleton<UIModalManager>, IValidatable
	{
		// Token: 0x17000229 RID: 553
		// (get) Token: 0x06000B7F RID: 2943 RVA: 0x00031735 File Offset: 0x0002F935
		// (set) Token: 0x06000B80 RID: 2944 RVA: 0x0003173D File Offset: 0x0002F93D
		public UIBaseModalView PreviousSpawnedModal { get; private set; }

		// Token: 0x1700022A RID: 554
		// (get) Token: 0x06000B81 RID: 2945 RVA: 0x00031746 File Offset: 0x0002F946
		public UIBaseModalView SpawnedModal
		{
			get
			{
				if (this.stack.Count <= 0)
				{
					return null;
				}
				return this.stack.Peek().SpawnedModal;
			}
		}

		// Token: 0x1700022B RID: 555
		// (get) Token: 0x06000B82 RID: 2946 RVA: 0x00031768 File Offset: 0x0002F968
		public bool ModalIsDisplaying
		{
			get
			{
				return this.stack.Count > 0;
			}
		}

		// Token: 0x1700022C RID: 556
		// (get) Token: 0x06000B83 RID: 2947 RVA: 0x00031778 File Offset: 0x0002F978
		public UIModalGenericViewAction DefaultGenericModalAction
		{
			get
			{
				return this.defaultGenericModalAction;
			}
		}

		// Token: 0x06000B84 RID: 2948 RVA: 0x00031780 File Offset: 0x0002F980
		protected override void Awake()
		{
			if (this.verboseLogging)
			{
				DebugUtility.Log("Awake", this);
			}
			base.Awake();
			this.displayAndHideHandler.SetToHideEnd(true);
			this.defaultGenericModalAction.OnClick = new Action(this.CloseAndClearStack);
			this.defaultGenericModalButtons[0] = this.defaultGenericModalAction;
			foreach (UIModalManager.UIModalSizeEntry uimodalSizeEntry in this.modalContainerList)
			{
				this.modalContainers.Add(uimodalSizeEntry.Key, uimodalSizeEntry.Value);
			}
		}

		// Token: 0x06000B85 RID: 2949 RVA: 0x0003180F File Offset: 0x0002FA0F
		protected override void OnDestroy()
		{
			base.OnDestroy();
			UIModalManager.OnModalClosedByUser = null;
			UIModalManager.OnModalClosed = null;
		}

		// Token: 0x06000B86 RID: 2950 RVA: 0x00031824 File Offset: 0x0002FA24
		[ContextMenu("Validate")]
		public void Validate()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Validate", Array.Empty<object>());
			}
			if (this.defaultGenericModalButtons.Length == 0)
			{
				DebugUtility.LogException(new Exception("defaultGenericModalButtons needs at least 1 entry!"), this);
			}
			HashSet<UIModalTypes> hashSet = new HashSet<UIModalTypes>();
			foreach (UIModalManager.UIModalSizeEntry uimodalSizeEntry in this.modalContainerList)
			{
				if (!hashSet.Add(uimodalSizeEntry.Key))
				{
					DebugUtility.LogError("You have more than 1 entry of `Key' in modalSizes! Ensure each key is unique!", this);
				}
			}
		}

		// Token: 0x06000B87 RID: 2951 RVA: 0x0003189E File Offset: 0x0002FA9E
		public void DisplayErrorModal(string text)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "DisplayErrorModal", new object[] { text });
			}
			this.Display(this.errorModalSource, UIModalManagerStackActions.PopStack, new object[] { text });
		}

		// Token: 0x06000B88 RID: 2952 RVA: 0x000318D4 File Offset: 0x0002FAD4
		public async Task<bool> Confirm(string body, UIModalManagerStackActions stackAction = UIModalManagerStackActions.MaintainStack)
		{
			UIModalManager.<>c__DisplayClass28_0 CS$<>8__locals1 = new UIModalManager.<>c__DisplayClass28_0();
			CS$<>8__locals1.completionSource = new TaskCompletionSource<bool>();
			this.Confirm(body, new Action(CS$<>8__locals1.<Confirm>g__OnConfirm|0), new Action(CS$<>8__locals1.<Confirm>g__OnCancel|1), stackAction);
			while (!CS$<>8__locals1.completionSource.Task.IsCompleted)
			{
				await Task.Yield();
			}
			return CS$<>8__locals1.completionSource.Task.Result;
		}

		// Token: 0x06000B89 RID: 2953 RVA: 0x00031928 File Offset: 0x0002FB28
		public async Task DisplayGenericModalAsync(string title, Sprite titleIconSprite, string body, UIModalManagerStackActions stackAction = UIModalManagerStackActions.ClearStack, params UIModalGenericViewAction[] buttons)
		{
			TaskCompletionSource<UIModalGenericViewAction> completionSource = new TaskCompletionSource<UIModalGenericViewAction>();
			if (buttons == null || buttons.Length == 0)
			{
				buttons = this.defaultGenericModalButtons;
			}
			UIModalGenericViewAction[] array = new UIModalGenericViewAction[buttons.Length];
			for (int i = 0; i < buttons.Length; i++)
			{
				UIModalGenericViewAction originalButton = buttons[i];
				array[i] = new UIModalGenericViewAction(originalButton.Color, originalButton.Text, null);
				Action originalAction = originalButton.OnClick;
				array[i].OnClick = delegate
				{
					Action originalAction2 = originalAction;
					if (originalAction2 != null)
					{
						originalAction2();
					}
					completionSource.SetResult(originalButton);
				};
			}
			this.DisplayGenericModal(title, titleIconSprite, body, stackAction, array);
			while (!completionSource.Task.IsCompleted)
			{
				await Task.Yield();
			}
		}

		// Token: 0x06000B8A RID: 2954 RVA: 0x00031998 File Offset: 0x0002FB98
		public void Confirm(string body, Action onConfirm, Action onCancel = null, UIModalManagerStackActions stackAction = UIModalManagerStackActions.MaintainStack)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Confirm", new object[]
				{
					body,
					onConfirm.DebugIsNull(),
					onCancel.DebugIsNull(),
					stackAction
				});
			}
			UIModalGenericViewAction[] array = new UIModalGenericViewAction[2];
			array[0] = this.defaultConfirmAction;
			array[0].OnClick = onConfirm;
			array[1] = this.defaultCancelAction;
			array[1].OnClick = onCancel;
			this.DisplayGenericModal("Confirmation", null, body, stackAction, array);
		}

		// Token: 0x06000B8B RID: 2955 RVA: 0x00031A2C File Offset: 0x0002FC2C
		public void DisplayGenericModal(string title, Sprite titleIconSprite, string body, UIModalManagerStackActions stackAction = UIModalManagerStackActions.ClearStack, params UIModalGenericViewAction[] buttons)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "DisplayGenericModal", new object[]
				{
					title,
					titleIconSprite.DebugSafeName(true),
					body,
					stackAction,
					buttons.Length
				});
			}
			if (buttons == null || buttons.Length == 0)
			{
				buttons = this.defaultGenericModalButtons;
				if (this.verboseLogging)
				{
					Debug.Log("No buttons were sent to they are defaulting to defaultGenericModalButtons", this);
				}
			}
			this.Display(this.genericModalSource, stackAction, new object[] { title, titleIconSprite, body, buttons });
		}

		// Token: 0x06000B8C RID: 2956 RVA: 0x00031AC4 File Offset: 0x0002FCC4
		public void Display(UIBaseModalView modalSource, UIModalManagerStackActions stackAction, params object[] modalData)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Display", new object[]
				{
					modalSource.GetType().Name,
					stackAction,
					modalData.Length
				});
			}
			if (this.displayAndHideHandler.IsTweeningDisplayOrHide)
			{
				this.displayAndHideHandler.CancelAnyTweens();
			}
			if (this.ModalIsDisplaying)
			{
				this.SpawnedModal.Close();
				if (stackAction == UIModalManagerStackActions.PopStack)
				{
					if (this.stackVerboseLogging)
					{
						Debug.Log("stack Pop: " + this.stack.Peek().ModalSource.GetType().Name);
					}
					this.stack.Pop();
				}
			}
			this.DispayModal(false, modalSource, stackAction, modalData);
		}

		// Token: 0x06000B8D RID: 2957 RVA: 0x00031B82 File Offset: 0x0002FD82
		public void CloseAndClearStack()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "CloseAndClearStack", Array.Empty<object>());
			}
			this.Close(true, Array.Empty<object>());
		}

		// Token: 0x06000B8E RID: 2958 RVA: 0x00031BA8 File Offset: 0x0002FDA8
		public void CloseWithoutClearingStack()
		{
			if (this.verboseLogging)
			{
				Debug.Log("CloseWithoutClearingStack", this);
			}
			this.CloseWithoutClearingStack(new object[0]);
		}

		// Token: 0x06000B8F RID: 2959 RVA: 0x00031BC9 File Offset: 0x0002FDC9
		public void CloseWithoutClearingStack(params object[] modalData)
		{
			if (this.verboseLogging)
			{
				Debug.Log(string.Format("{0} ( {1}: {2} )", "CloseWithoutClearingStack", "modalData", modalData.Length), this);
			}
			this.Close(false, modalData);
		}

		// Token: 0x06000B90 RID: 2960 RVA: 0x00031C00 File Offset: 0x0002FE00
		private void DispayModal(bool isFromBack, UIBaseModalView modalSource, UIModalManagerStackActions stackAction, params object[] modalData)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "DispayModal", new object[]
				{
					isFromBack,
					modalSource.GetType().Name,
					stackAction,
					modalData.Length
				});
			}
			if (stackAction == UIModalManagerStackActions.ClearStack)
			{
				if (this.stackVerboseLogging)
				{
					Debug.Log(string.Format("{0} Clear: {1}", "stack", this.stack.Count));
				}
				this.stack.Clear();
			}
			UIModalTypes uimodalTypes = modalSource.ModalSize;
			if (modalData != null)
			{
				foreach (object obj in modalData)
				{
					if (obj != null && obj.GetType() == typeof(UIModalTypes))
					{
						uimodalTypes = (UIModalTypes)obj;
						break;
					}
				}
			}
			Transform transform = this.modalContainers[(uimodalTypes == UIModalTypes.CenterSameSize) ? UIModalTypes.CenterSmall : uimodalTypes];
			PoolManagerT instance = MonoBehaviourSingleton<PoolManagerT>.Instance;
			Transform transform2 = transform;
			UIBaseModalView uibaseModalView = instance.Spawn<UIBaseModalView>(modalSource, default(Vector3), default(Quaternion), transform2);
			if (uimodalTypes != UIModalTypes.CenterSameSize)
			{
				uibaseModalView.RectTransform.SetAnchor(AnchorPresets.StretchAll, 0f, 0f, 0f, 0f);
			}
			this.stack.Push(new UIModalManager.UIModalHistoryEntry(modalSource, uibaseModalView, modalData));
			if (this.stackVerboseLogging)
			{
				Debug.Log("stack Push: " + modalSource.GetType().Name);
			}
			uibaseModalView.OnDisplay(modalData);
			this.displayAndHideHandler.Display();
		}

		// Token: 0x06000B91 RID: 2961 RVA: 0x00031D80 File Offset: 0x0002FF80
		private void Close(bool clearStack, params object[] onCloseModalData)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Close", new object[] { clearStack, onCloseModalData.Length });
			}
			if (this.ModalIsDisplaying)
			{
				this.PreviousSpawnedModal = this.SpawnedModal;
				this.SpawnedModal.Close();
				Action<UIBaseModalView> onModalClosed = UIModalManager.OnModalClosed;
				if (onModalClosed != null)
				{
					onModalClosed(this.SpawnedModal);
				}
				if (this.stackVerboseLogging)
				{
					Debug.Log("stack Pop: " + this.stack.Peek().ModalSource.GetType().Name);
				}
				this.stack.Pop();
			}
			if (clearStack)
			{
				if (this.stackVerboseLogging)
				{
					Debug.Log(string.Format("{0} Clear: {1}", "stack", this.stack.Count));
				}
				this.stack.Clear();
			}
			if (this.stack.Count == 0)
			{
				this.CloseContainer();
				return;
			}
			UIModalManager.UIModalHistoryEntry uimodalHistoryEntry = this.stack.Pop();
			object[] array = uimodalHistoryEntry.ModalData;
			if (onCloseModalData != null && onCloseModalData.Length != 0)
			{
				array = this.CombineModalData(array, onCloseModalData);
			}
			this.DispayModal(true, uimodalHistoryEntry.ModalSource, UIModalManagerStackActions.MaintainStack, array);
		}

		// Token: 0x06000B92 RID: 2962 RVA: 0x00031EAC File Offset: 0x000300AC
		private void CloseContainer()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "CloseContainer", Array.Empty<object>());
			}
			if (this.displayAndHideHandler.IsTweeningDisplayOrHide)
			{
				this.displayAndHideHandler.CancelAnyTweens();
			}
			this.displayAndHideHandler.Hide();
		}

		// Token: 0x06000B93 RID: 2963 RVA: 0x00031EEC File Offset: 0x000300EC
		private object[] CombineModalData(object[] modalDataA, object[] modalDataB)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "CombineModalData", new object[] { modalDataA.Length, modalDataB.Length });
			}
			object[] array = new object[modalDataA.Length + modalDataB.Length];
			int num = 0;
			for (int i = 0; i < modalDataA.Length; i++)
			{
				array[i] = modalDataA[i];
				num++;
			}
			for (int j = 0; j < modalDataB.Length; j++)
			{
				array[num] = modalDataB[j];
				num++;
			}
			return array;
		}

		// Token: 0x04000765 RID: 1893
		public static Action<UIBaseModalView> OnModalClosedByUser;

		// Token: 0x04000766 RID: 1894
		public static Action<UIBaseModalView> OnModalClosed;

		// Token: 0x04000767 RID: 1895
		[SerializeField]
		private UIGenericModalView genericModalSource;

		// Token: 0x04000768 RID: 1896
		[SerializeField]
		private UIErrorModalView errorModalSource;

		// Token: 0x04000769 RID: 1897
		[SerializeField]
		private UIModalGenericViewAction defaultGenericModalAction = new UIModalGenericViewAction(Color.red, "OK", null);

		// Token: 0x0400076A RID: 1898
		[SerializeField]
		private UIModalManager.UIModalSizeEntry[] modalContainerList = Array.Empty<UIModalManager.UIModalSizeEntry>();

		// Token: 0x0400076B RID: 1899
		[SerializeField]
		private UIDisplayAndHideHandler displayAndHideHandler;

		// Token: 0x0400076C RID: 1900
		[Header("Confirmation")]
		[SerializeField]
		private UIModalGenericViewAction defaultConfirmAction = new UIModalGenericViewAction(Color.blue, "Confirm", null);

		// Token: 0x0400076D RID: 1901
		[SerializeField]
		private UIModalGenericViewAction defaultCancelAction = new UIModalGenericViewAction(Color.red, "Cancel", null);

		// Token: 0x0400076E RID: 1902
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;

		// Token: 0x0400076F RID: 1903
		[SerializeField]
		private bool stackVerboseLogging;

		// Token: 0x04000770 RID: 1904
		private readonly UIModalGenericViewAction[] defaultGenericModalButtons = new UIModalGenericViewAction[1];

		// Token: 0x04000771 RID: 1905
		private readonly Stack<UIModalManager.UIModalHistoryEntry> stack = new Stack<UIModalManager.UIModalHistoryEntry>();

		// Token: 0x04000772 RID: 1906
		private readonly Dictionary<UIModalTypes, RectTransform> modalContainers = new Dictionary<UIModalTypes, RectTransform>();

		// Token: 0x020001D2 RID: 466
		[Serializable]
		private struct UIModalSizeEntry
		{
			// Token: 0x04000774 RID: 1908
			public UIModalTypes Key;

			// Token: 0x04000775 RID: 1909
			public RectTransform Value;
		}

		// Token: 0x020001D3 RID: 467
		private struct UIModalHistoryEntry
		{
			// Token: 0x06000B95 RID: 2965 RVA: 0x00031FEA File Offset: 0x000301EA
			public UIModalHistoryEntry(UIBaseModalView modalSource, UIBaseModalView spawnedModal, object[] modalData)
			{
				this.ModalSource = modalSource;
				this.SpawnedModal = spawnedModal;
				this.ModalData = modalData;
			}

			// Token: 0x04000776 RID: 1910
			public readonly UIBaseModalView ModalSource;

			// Token: 0x04000777 RID: 1911
			public readonly UIBaseModalView SpawnedModal;

			// Token: 0x04000778 RID: 1912
			public readonly object[] ModalData;
		}
	}
}
