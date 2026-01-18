using System;
using System.Collections.Generic;
using Endless.Shared.Debugging;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Shared.UI
{
	// Token: 0x02000251 RID: 593
	[DisallowMultipleComponent]
	public abstract class UIBaseScreenView : UIGameObject, IPoolableT, IBackable
	{
		// Token: 0x170002D3 RID: 723
		// (get) Token: 0x06000F04 RID: 3844 RVA: 0x00040A8E File Offset: 0x0003EC8E
		// (set) Token: 0x06000F05 RID: 3845 RVA: 0x00040A96 File Offset: 0x0003EC96
		protected bool VerboseLogging { get; set; }

		// Token: 0x170002D4 RID: 724
		// (get) Token: 0x06000F06 RID: 3846 RVA: 0x00040A9F File Offset: 0x0003EC9F
		// (set) Token: 0x06000F07 RID: 3847 RVA: 0x00040AA7 File Offset: 0x0003ECA7
		public MonoBehaviour Prefab { get; set; }

		// Token: 0x170002D5 RID: 725
		// (get) Token: 0x06000F08 RID: 3848 RVA: 0x000050D2 File Offset: 0x000032D2
		public bool IsUi
		{
			get
			{
				return true;
			}
		}

		// Token: 0x170002D6 RID: 726
		// (get) Token: 0x06000F09 RID: 3849 RVA: 0x00040AB0 File Offset: 0x0003ECB0
		public bool IsTweeningDisplay
		{
			get
			{
				return this.displayAndHideHandler.IsTweeningDisplay;
			}
		}

		// Token: 0x170002D7 RID: 727
		// (get) Token: 0x06000F0A RID: 3850 RVA: 0x00040ABD File Offset: 0x0003ECBD
		public bool IsTweeningClose
		{
			get
			{
				return this.displayAndHideHandler.IsTweeningHide;
			}
		}

		// Token: 0x06000F0B RID: 3851 RVA: 0x00040ACC File Offset: 0x0003ECCC
		protected virtual void Start()
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log("Start", this);
			}
			this.displayAndHideHandler.OnDisplayComplete.AddListener(new UnityAction(this.OnDisplayComplete));
			this.displayAndHideHandler.OnHideComplete.AddListener(new UnityAction(this.OnHideComplete));
		}

		// Token: 0x06000F0C RID: 3852 RVA: 0x00040B25 File Offset: 0x0003ED25
		public virtual void OnSpawn()
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log("OnSpawn", this);
			}
			if (!MonoBehaviourSingleton<BackManager>.Instance.HasContext(this))
			{
				MonoBehaviourSingleton<BackManager>.Instance.ClaimContext(this);
			}
		}

		// Token: 0x06000F0D RID: 3853 RVA: 0x00040B52 File Offset: 0x0003ED52
		public virtual void OnDespawn()
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log("OnDespawn", this);
			}
		}

		// Token: 0x06000F0E RID: 3854 RVA: 0x00040B67 File Offset: 0x0003ED67
		public virtual void OnBack()
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log("OnBack", this);
			}
		}

		// Token: 0x06000F0F RID: 3855 RVA: 0x00040B7C File Offset: 0x0003ED7C
		public virtual void Initialize(Dictionary<string, object> supplementalData)
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "Initialize", "supplementalData", (supplementalData == null) ? "null" : supplementalData.Count), this);
				if (supplementalData != null)
				{
					foreach (KeyValuePair<string, object> keyValuePair in supplementalData)
					{
						DebugUtility.Log(string.Format("{0}: {1}", keyValuePair.Key, keyValuePair.Value), this);
					}
				}
			}
			if (this.isFirstDisplay)
			{
				this.displayAndHideHandler.SetToDisplayStart(true);
			}
			Action<UIBaseScreenView> displayBegunAction = UIBaseScreenView.DisplayBegunAction;
			if (displayBegunAction != null)
			{
				displayBegunAction(this);
			}
			this.displayAndHideHandler.Display();
		}

		// Token: 0x06000F10 RID: 3856 RVA: 0x00040C4C File Offset: 0x0003EE4C
		public virtual void Close(Action onCloseTweenComplete)
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log("Close", this);
			}
			if (MonoBehaviourSingleton<BackManager>.Instance.HasContext(this))
			{
				MonoBehaviourSingleton<BackManager>.Instance.UnclaimContext(this);
			}
			this.displayAndHideHandler.Hide(onCloseTweenComplete);
			Action<UIBaseScreenView> closeBegunAction = UIBaseScreenView.CloseBegunAction;
			if (closeBegunAction == null)
			{
				return;
			}
			closeBegunAction(this);
		}

		// Token: 0x06000F11 RID: 3857 RVA: 0x00040CA0 File Offset: 0x0003EEA0
		public void CancelClose()
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log("CancelClose", this);
			}
			this.displayAndHideHandler.CancelHideTweens();
		}

		// Token: 0x06000F12 RID: 3858 RVA: 0x00040CC0 File Offset: 0x0003EEC0
		protected virtual void OnDisplayComplete()
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log("OnDisplayComplete", this);
			}
			Action<UIBaseScreenView> displayCompletedAction = UIBaseScreenView.DisplayCompletedAction;
			if (displayCompletedAction == null)
			{
				return;
			}
			displayCompletedAction(this);
		}

		// Token: 0x06000F13 RID: 3859 RVA: 0x00040CE5 File Offset: 0x0003EEE5
		private void OnHideComplete()
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log("OnHideComplete", this);
			}
			Action<UIBaseScreenView> closeCompletedAction = UIBaseScreenView.CloseCompletedAction;
			if (closeCompletedAction != null)
			{
				closeCompletedAction(this);
			}
			MonoBehaviourSingleton<PoolManagerT>.Instance.Despawn<UIBaseScreenView>(this);
		}

		// Token: 0x0400097E RID: 2430
		public static Action<UIBaseScreenView> DisplayBegunAction;

		// Token: 0x0400097F RID: 2431
		public static Action<UIBaseScreenView> DisplayCompletedAction;

		// Token: 0x04000980 RID: 2432
		public static Action<UIBaseScreenView> CloseBegunAction;

		// Token: 0x04000981 RID: 2433
		public static Action<UIBaseScreenView> CloseCompletedAction;

		// Token: 0x04000983 RID: 2435
		[SerializeField]
		private UIDisplayAndHideHandler displayAndHideHandler;

		// Token: 0x04000984 RID: 2436
		private readonly bool isFirstDisplay = true;
	}
}
