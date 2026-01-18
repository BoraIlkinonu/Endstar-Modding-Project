using System;
using System.Collections.Generic;
using Endless.Shared.Debugging;
using UnityEngine;

namespace Endless.Shared.UI
{
	// Token: 0x02000252 RID: 594
	[DefaultExecutionOrder(-2147483648)]
	public class UIScreenManager : UIMonoBehaviourSingleton<UIScreenManager>
	{
		// Token: 0x170002D8 RID: 728
		// (get) Token: 0x06000F15 RID: 3861 RVA: 0x00040D25 File Offset: 0x0003EF25
		public bool IsDisplaying
		{
			get
			{
				return this.history.Count > 0;
			}
		}

		// Token: 0x170002D9 RID: 729
		// (get) Token: 0x06000F16 RID: 3862 RVA: 0x00040D35 File Offset: 0x0003EF35
		public Type DisplayedScreenType
		{
			get
			{
				if (!this.IsDisplaying)
				{
					return null;
				}
				return this.history.Peek().Key;
			}
		}

		// Token: 0x06000F17 RID: 3863 RVA: 0x00040D54 File Offset: 0x0003EF54
		protected override void Awake()
		{
			if (this.verboseLogging)
			{
				DebugUtility.Log("Awake", this);
			}
			base.Awake();
			foreach (object obj in this.sources)
			{
				UIBaseScreenView uibaseScreenView = (UIBaseScreenView)obj;
				Type type = uibaseScreenView.GetType();
				if (this.sourceDictionary.ContainsKey(type))
				{
					Debug.LogException(new Exception(string.Format("There is a duplicate entry of {0}!", uibaseScreenView.GetType())), this);
				}
				else
				{
					this.sourceDictionary.Add(uibaseScreenView.GetType(), uibaseScreenView);
				}
			}
			UIBaseScreenView.CloseBegunAction = (Action<UIBaseScreenView>)Delegate.Combine(UIBaseScreenView.CloseBegunAction, new Action<UIBaseScreenView>(this.OnScreenCloseBegun));
			UIBaseScreenView.CloseCompletedAction = (Action<UIBaseScreenView>)Delegate.Combine(UIBaseScreenView.CloseCompletedAction, new Action<UIBaseScreenView>(this.OnScreenCloseComplete));
		}

		// Token: 0x06000F18 RID: 3864 RVA: 0x00040E44 File Offset: 0x0003F044
		public UIBaseScreenView Display<T>(UIScreenManager.DisplayStackActions displayStackAction, Dictionary<string, object> supplementalData = null) where T : UIBaseScreenView
		{
			Type typeFromHandle = typeof(T);
			if (this.verboseLogging)
			{
				Debug.Log(string.Format("{0}<{1}> ( {2}: {3}, {4}: {5} )", new object[]
				{
					"Display",
					typeFromHandle.Name,
					"displayStackAction",
					displayStackAction,
					"supplementalData",
					(supplementalData == null) ? "null" : supplementalData.Count
				}), this);
			}
			if (!this.sourceDictionary.ContainsKey(typeFromHandle))
			{
				Debug.LogException(new Exception("No source with a type of " + typeFromHandle.Name + " is in sourceDictionary!"), this);
			}
			UIBaseScreenView uibaseScreenView = this.sourceDictionary[typeFromHandle];
			return this.Display(uibaseScreenView, displayStackAction, supplementalData);
		}

		// Token: 0x06000F19 RID: 3865 RVA: 0x00040F00 File Offset: 0x0003F100
		public void Close(UIScreenManager.CloseStackActions closeStackAction, Action onCloseTweenComplete, bool displayNextHistoricalRecord = true, bool isInProcessOfDisplaying = false)
		{
			if (this.verboseLogging)
			{
				string text = "Close";
				UIBaseScreenView uibaseScreenView = this.activeInstance;
				DebugUtility.LogMethodWithAppension(this, text, (uibaseScreenView != null) ? uibaseScreenView.DebugSafeName(true) : null, new object[] { closeStackAction, onCloseTweenComplete, displayNextHistoricalRecord, isInProcessOfDisplaying });
			}
			if (!this.activeInstance)
			{
				DebugUtility.LogException(new NullReferenceException("activeInstance is null!"), this);
				return;
			}
			UIBaseScreenView uibaseScreenView2 = this.activeInstance;
			this.activeInstance = null;
			uibaseScreenView2.Close(onCloseTweenComplete);
			switch (closeStackAction)
			{
			case UIScreenManager.CloseStackActions.Pop:
				this.history.Pop();
				if (this.history.Count > 0)
				{
					UIScreenManager.HistoricalRecord historicalRecord = this.history.Peek();
					if (displayNextHistoricalRecord)
					{
						if (!this.sourceDictionary.ContainsKey(historicalRecord.Key))
						{
							Debug.LogException(new Exception("No source with a type of " + historicalRecord.Key.Name + " is in sourceDictionary!"), this);
						}
						UIBaseScreenView uibaseScreenView3 = this.sourceDictionary[historicalRecord.Key];
						this.Display(uibaseScreenView3, UIScreenManager.DisplayStackActions.Nothing, historicalRecord.SupplementalData);
					}
				}
				break;
			case UIScreenManager.CloseStackActions.Clear:
				this.history.Clear();
				break;
			case UIScreenManager.CloseStackActions.Nothing:
				break;
			default:
				DebugUtility.LogNoEnumSupportError<UIScreenManager.CloseStackActions>(this, "Close", closeStackAction, new object[] { closeStackAction });
				break;
			}
			if (this.history.Count == 0 && closeStackAction != UIScreenManager.CloseStackActions.Nothing && !isInProcessOfDisplaying)
			{
				Action onScreenSystemClose = UIScreenManager.OnScreenSystemClose;
				if (onScreenSystemClose == null)
				{
					return;
				}
				onScreenSystemClose();
			}
		}

		// Token: 0x06000F1A RID: 3866 RVA: 0x00041078 File Offset: 0x0003F278
		private UIBaseScreenView Display(UIBaseScreenView source, UIScreenManager.DisplayStackActions displayStackAction, Dictionary<string, object> supplementalData = null)
		{
			if (this.verboseLogging)
			{
				Debug.Log(string.Format("{0} ( {1}: {2}, {3}: {4}, {5}: {6} )", new object[]
				{
					"Display",
					"source",
					source.GetType().Name,
					"displayStackAction",
					displayStackAction,
					"supplementalData",
					(supplementalData == null) ? "null" : supplementalData.Count
				}), this);
			}
			bool flag = !this.IsDisplaying;
			Type type = source.GetType();
			UIScreenManager.HistoricalRecord historicalRecord = new UIScreenManager.HistoricalRecord(type, supplementalData);
			switch (displayStackAction)
			{
			case UIScreenManager.DisplayStackActions.Push:
				if (this.IsDisplaying)
				{
					this.Close(UIScreenManager.CloseStackActions.Nothing, null, false, true);
				}
				this.history.Push(historicalRecord);
				break;
			case UIScreenManager.DisplayStackActions.Pop:
				if (this.IsDisplaying)
				{
					this.Close(UIScreenManager.CloseStackActions.Pop, null, false, true);
				}
				break;
			case UIScreenManager.DisplayStackActions.PopAndPush:
				if (this.IsDisplaying)
				{
					this.Close(UIScreenManager.CloseStackActions.Pop, null, false, true);
				}
				this.history.Push(historicalRecord);
				break;
			case UIScreenManager.DisplayStackActions.ClearAndPush:
				if (this.IsDisplaying)
				{
					this.Close(UIScreenManager.CloseStackActions.Clear, null, false, true);
				}
				this.history.Push(historicalRecord);
				break;
			case UIScreenManager.DisplayStackActions.Nothing:
				break;
			default:
				DebugUtility.LogNoEnumSupportError<UIScreenManager.DisplayStackActions>(this, "Display", displayStackAction, new object[]
				{
					source,
					displayStackAction,
					(supplementalData == null) ? "null" : supplementalData.Count
				});
				break;
			}
			if (this.closingDictionary.ContainsKey(type))
			{
				this.activeInstance = this.closingDictionary[type];
				this.closingDictionary.Remove(type);
				this.activeInstance.CancelClose();
			}
			else
			{
				PoolManagerT instance = MonoBehaviourSingleton<PoolManagerT>.Instance;
				Transform transform = base.transform;
				this.activeInstance = instance.Spawn<UIBaseScreenView>(source, default(Vector3), default(Quaternion), transform);
			}
			this.activeInstance.Initialize(supplementalData);
			RectTransformValue rectTransformValue = new RectTransformValue(source.RectTransform);
			rectTransformValue.ApplyTo(this.activeInstance.RectTransform);
			if (flag)
			{
				Action onScreenSystemOpen = UIScreenManager.OnScreenSystemOpen;
				if (onScreenSystemOpen != null)
				{
					onScreenSystemOpen();
				}
			}
			return this.activeInstance;
		}

		// Token: 0x06000F1B RID: 3867 RVA: 0x00041286 File Offset: 0x0003F486
		private void OnScreenCloseBegun(UIBaseScreenView screen)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnScreenCloseBegun", new object[] { screen.GetType().Name });
			}
			this.closingDictionary.Add(screen.GetType(), screen);
		}

		// Token: 0x06000F1C RID: 3868 RVA: 0x000412C1 File Offset: 0x0003F4C1
		private void OnScreenCloseComplete(UIBaseScreenView screen)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnScreenCloseComplete", new object[] { screen.GetType().Name });
			}
			this.closingDictionary.Remove(screen.GetType());
		}

		// Token: 0x04000986 RID: 2438
		public static Action OnScreenSystemOpen;

		// Token: 0x04000987 RID: 2439
		public static Action OnScreenSystemClose;

		// Token: 0x04000988 RID: 2440
		[SerializeField]
		private UIScreenScriptableObjectArray sources;

		// Token: 0x04000989 RID: 2441
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;

		// Token: 0x0400098A RID: 2442
		private Dictionary<Type, UIBaseScreenView> sourceDictionary = new Dictionary<Type, UIBaseScreenView>();

		// Token: 0x0400098B RID: 2443
		private readonly Stack<UIScreenManager.HistoricalRecord> history = new Stack<UIScreenManager.HistoricalRecord>();

		// Token: 0x0400098C RID: 2444
		private UIBaseScreenView activeInstance;

		// Token: 0x0400098D RID: 2445
		private readonly Dictionary<Type, UIBaseScreenView> closingDictionary = new Dictionary<Type, UIBaseScreenView>();

		// Token: 0x02000253 RID: 595
		public enum DisplayStackActions
		{
			// Token: 0x0400098F RID: 2447
			Push,
			// Token: 0x04000990 RID: 2448
			Pop,
			// Token: 0x04000991 RID: 2449
			PopAndPush,
			// Token: 0x04000992 RID: 2450
			ClearAndPush,
			// Token: 0x04000993 RID: 2451
			Nothing
		}

		// Token: 0x02000254 RID: 596
		public enum CloseStackActions
		{
			// Token: 0x04000995 RID: 2453
			Pop,
			// Token: 0x04000996 RID: 2454
			Clear,
			// Token: 0x04000997 RID: 2455
			Nothing
		}

		// Token: 0x02000255 RID: 597
		private struct HistoricalRecord
		{
			// Token: 0x06000F1E RID: 3870 RVA: 0x00041325 File Offset: 0x0003F525
			public HistoricalRecord(Type key, Dictionary<string, object> supplementalData)
			{
				this.Key = key;
				this.SupplementalData = supplementalData;
			}

			// Token: 0x06000F1F RID: 3871 RVA: 0x00041338 File Offset: 0x0003F538
			public override string ToString()
			{
				return string.Format("{0}: {1}, {2}: {3}", new object[]
				{
					"Key",
					this.Key.Name,
					"SupplementalData",
					(this.SupplementalData == null) ? "null" : this.SupplementalData.Count
				});
			}

			// Token: 0x04000998 RID: 2456
			public Type Key;

			// Token: 0x04000999 RID: 2457
			public Dictionary<string, object> SupplementalData;
		}
	}
}
