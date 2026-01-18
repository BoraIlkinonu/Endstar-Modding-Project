using System;
using Endless.Shared.Debugging;
using Endless.Shared.Tweens;
using Endless.Shared.Validation;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Endless.Shared.UI
{
	// Token: 0x020001CB RID: 459
	public abstract class UIBaseModalView : UIGameObject, IPoolableT, IBackable, IValidatable
	{
		// Token: 0x17000225 RID: 549
		// (get) Token: 0x06000B67 RID: 2919 RVA: 0x00031355 File Offset: 0x0002F555
		// (set) Token: 0x06000B68 RID: 2920 RVA: 0x0003135D File Offset: 0x0002F55D
		protected bool VerboseLogging { get; set; }

		// Token: 0x17000226 RID: 550
		// (get) Token: 0x06000B69 RID: 2921 RVA: 0x00031366 File Offset: 0x0002F566
		// (set) Token: 0x06000B6A RID: 2922 RVA: 0x0003136E File Offset: 0x0002F56E
		public MonoBehaviour Prefab { get; set; }

		// Token: 0x17000227 RID: 551
		// (get) Token: 0x06000B6B RID: 2923 RVA: 0x000050D2 File Offset: 0x000032D2
		public bool IsUi
		{
			get
			{
				return true;
			}
		}

		// Token: 0x17000228 RID: 552
		// (get) Token: 0x06000B6C RID: 2924 RVA: 0x00031377 File Offset: 0x0002F577
		public UIModalTypes ModalSize
		{
			get
			{
				return this.modalSize;
			}
		}

		// Token: 0x06000B6D RID: 2925 RVA: 0x0003137F File Offset: 0x0002F57F
		[ContextMenu("Validate")]
		public virtual void Validate()
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log("Validate", this);
			}
			this.closeTweens.ValidateForNumberOfTweens(1);
			DebugUtility.DebugHasNullItem<GraphicRaycaster>(this.graphicRaycasters, "graphicRaycasters", this);
		}

		// Token: 0x06000B6E RID: 2926 RVA: 0x000313B2 File Offset: 0x0002F5B2
		protected virtual void Start()
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log("Start", this);
			}
			this.closeTweens.OnAllTweenCompleted.AddListener(new UnityAction(this.Despawn));
		}

		// Token: 0x06000B6F RID: 2927
		public abstract void OnBack();

		// Token: 0x06000B70 RID: 2928 RVA: 0x000313E3 File Offset: 0x0002F5E3
		public virtual void OnSpawn()
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log("OnSpawn", this);
			}
			this.displayTweens.Tween();
		}

		// Token: 0x06000B71 RID: 2929 RVA: 0x00031403 File Offset: 0x0002F603
		public virtual void OnDespawn()
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log("OnDespawn", this);
			}
		}

		// Token: 0x06000B72 RID: 2930 RVA: 0x00031418 File Offset: 0x0002F618
		public virtual void OnDisplay(params object[] modalData)
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "OnDisplay", "modalData", modalData.Length), this);
				for (int i = 0; i < modalData.Length; i++)
				{
					string text = ((modalData[i] == null) ? "null" : string.Format("({0}) {1}", modalData[i].GetType(), modalData[i]));
					DebugUtility.Log(string.Format("{0}[{1}]: {2}", "modalData", i, text), this);
				}
			}
			foreach (GraphicRaycaster graphicRaycaster in this.graphicRaycasters)
			{
				if (!graphicRaycaster)
				{
					Debug.LogError("There is a null item in graphicRaycasters on the modal " + base.gameObject.name, this);
				}
				else
				{
					graphicRaycaster.enabled = true;
				}
			}
			MonoBehaviourSingleton<BackManager>.Instance.ClaimContext(this);
		}

		// Token: 0x06000B73 RID: 2931 RVA: 0x000314F0 File Offset: 0x0002F6F0
		public virtual void Close()
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log("Close", this);
			}
			foreach (GraphicRaycaster graphicRaycaster in this.graphicRaycasters)
			{
				if (!graphicRaycaster)
				{
					Debug.LogError("There is a null item in graphicRaycasters on the modal " + base.gameObject.name, this);
				}
				else
				{
					graphicRaycaster.enabled = false;
				}
			}
			MonoBehaviourSingleton<BackManager>.Instance.UnclaimContext(this);
			this.closeTweens.Tween();
		}

		// Token: 0x06000B74 RID: 2932 RVA: 0x0003156C File Offset: 0x0002F76C
		protected object GetModalDataByType(Type targetType, params object[] modalData)
		{
			foreach (object obj in modalData)
			{
				if (obj.GetType() == targetType)
				{
					return obj;
				}
			}
			return null;
		}

		// Token: 0x06000B75 RID: 2933 RVA: 0x0003159E File Offset: 0x0002F79E
		private void Despawn()
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log("Despawn", this);
			}
			MonoBehaviourSingleton<PoolManagerT>.Instance.Despawn<UIBaseModalView>(this);
		}

		// Token: 0x04000750 RID: 1872
		[SerializeField]
		private UIModalTypes modalSize;

		// Token: 0x04000751 RID: 1873
		[SerializeField]
		private GraphicRaycaster[] graphicRaycasters = Array.Empty<GraphicRaycaster>();

		// Token: 0x04000752 RID: 1874
		[Header("Tweens")]
		[SerializeField]
		private TweenCollection displayTweens;

		// Token: 0x04000753 RID: 1875
		[SerializeField]
		private TweenCollection closeTweens;
	}
}
