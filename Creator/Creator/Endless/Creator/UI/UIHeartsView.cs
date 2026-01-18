using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Creator.UI
{
	// Token: 0x020001F4 RID: 500
	public class UIHeartsView : UIBaseIntView
	{
		// Token: 0x060007D6 RID: 2006 RVA: 0x0002683C File Offset: 0x00024A3C
		public override void View(int model)
		{
			base.View(model);
			this.DisableSurplus();
			base.NumericFieldViews[0].SetValue((float)model, false);
			if (model > this.halfHeartCount)
			{
				this.SpawnHeartsUpTo(model);
			}
			else if (model < this.halfHeartCount)
			{
				this.DespawnHeartsDownTo(model);
			}
			this.ViewSurplus(model);
		}

		// Token: 0x060007D7 RID: 2007 RVA: 0x00026894 File Offset: 0x00024A94
		public override void Clear()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "Clear", Array.Empty<object>());
			}
			foreach (UIPoolableGameObject uipoolableGameObject in this.poolableObjects)
			{
				uipoolableGameObject.ReturnToPool();
			}
			this.poolableObjects.Clear();
			this.halfHeartCount = 0;
		}

		// Token: 0x060007D8 RID: 2008 RVA: 0x00026910 File Offset: 0x00024B10
		protected override void ApplyNumericFieldViewValuesToModel(float fieldModel)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "ApplyNumericFieldViewValuesToModel", new object[] { fieldModel });
			}
			UnityEvent<int> onValueChanged = base.OnValueChanged;
			if (onValueChanged == null)
			{
				return;
			}
			onValueChanged.Invoke((int)fieldModel);
		}

		// Token: 0x060007D9 RID: 2009 RVA: 0x00026948 File Offset: 0x00024B48
		private void SpawnHeartsUpTo(int model)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "SpawnHeartsUpTo", new object[] { model });
			}
			int num = model - this.halfHeartCount;
			int maximumAmountOfVisibleHalfHearts = this.GetMaximumAmountOfVisibleHalfHearts();
			if (this.halfHeartCount + num > maximumAmountOfVisibleHalfHearts)
			{
				num = maximumAmountOfVisibleHalfHearts - this.halfHeartCount;
			}
			for (int i = 0; i < num; i++)
			{
				bool flag = this.halfHeartCount == 0;
				bool flag2 = (this.halfHeartCount - 1) % 2 == 0;
				Transform transform;
				if (!flag && !flag2)
				{
					PoolManagerT instance = MonoBehaviourSingleton<PoolManagerT>.Instance;
					UIHeartHalfSpaceView uiheartHalfSpaceView = this.heartHalfSpaceSource;
					transform = this.heartHalfContainer;
					UIHeartHalfSpaceView uiheartHalfSpaceView2 = instance.Spawn<UIHeartHalfSpaceView>(uiheartHalfSpaceView, default(Vector3), default(Quaternion), transform);
					this.poolableObjects.Add(uiheartHalfSpaceView2);
				}
				PoolManagerT instance2 = MonoBehaviourSingleton<PoolManagerT>.Instance;
				UIHeartHalfView uiheartHalfView = this.heartHalfSource;
				transform = this.heartHalfContainer;
				UIHeartHalfView uiheartHalfView2 = instance2.Spawn<UIHeartHalfView>(uiheartHalfView, default(Vector3), default(Quaternion), transform);
				this.poolableObjects.Add(uiheartHalfView2);
				uiheartHalfView2.View(this.halfHeartCount, i);
				this.halfHeartCount++;
			}
		}

		// Token: 0x060007DA RID: 2010 RVA: 0x00026A68 File Offset: 0x00024C68
		private void DespawnHeartsDownTo(int model)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "DespawnHeartsDownTo", new object[] { model });
			}
			int num = this.halfHeartCount - model;
			int maximumAmountOfVisibleHalfHearts = this.GetMaximumAmountOfVisibleHalfHearts();
			num = Mathf.Clamp(num, 0, maximumAmountOfVisibleHalfHearts);
			while (num > 0 && this.poolableObjects.Count > 0)
			{
				this.<DespawnHeartsDownTo>g__DespawnLastItemIfSpace|9_0();
				List<UIPoolableGameObject> list = this.poolableObjects;
				UIHeartHalfView uiheartHalfView = list[list.Count - 1] as UIHeartHalfView;
				if (uiheartHalfView != null)
				{
					uiheartHalfView.HideAndDespawnOnComplete();
				}
				else
				{
					DebugUtility.LogException(new InvalidCastException("Could not cast UIPoolableGameObject to UIHeartHalfView"), this);
				}
				this.poolableObjects.RemoveAt(this.poolableObjects.Count - 1);
				this.halfHeartCount--;
				num--;
				this.<DespawnHeartsDownTo>g__DespawnLastItemIfSpace|9_0();
			}
		}

		// Token: 0x060007DB RID: 2011 RVA: 0x00026B30 File Offset: 0x00024D30
		private void DisableSurplus()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "DisableSurplus", Array.Empty<object>());
			}
			if (this.poolableObjects.Count <= 0)
			{
				return;
			}
			List<UIPoolableGameObject> list = this.poolableObjects;
			UIHeartHalfView uiheartHalfView = list[list.Count - 1] as UIHeartHalfView;
			if (uiheartHalfView != null)
			{
				uiheartHalfView.DisableSurplus();
				return;
			}
			DebugUtility.LogException(new InvalidCastException("Could not cast UIPoolableGameObject to UIHeartHalfView!"), this);
		}

		// Token: 0x060007DC RID: 2012 RVA: 0x00026B98 File Offset: 0x00024D98
		private void ViewSurplus(int model)
		{
			UIHeartsView.<>c__DisplayClass11_0 CS$<>8__locals1;
			CS$<>8__locals1.<>4__this = this;
			CS$<>8__locals1.model = model;
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "ViewSurplus", new object[] { CS$<>8__locals1.model });
			}
			int num = this.<ViewSurplus>g__GetCountOfSurplusOfHalfHearts|11_0(ref CS$<>8__locals1);
			if (num <= 0)
			{
				return;
			}
			List<UIPoolableGameObject> list = this.poolableObjects;
			UIHeartHalfView uiheartHalfView = list[list.Count - 1] as UIHeartHalfView;
			if (uiheartHalfView != null)
			{
				uiheartHalfView.ViewSurplus(num);
				return;
			}
			DebugUtility.LogException(new InvalidCastException("Could not cast UIPoolableGameObject to UIHeartHalfView"), this);
		}

		// Token: 0x060007DD RID: 2013 RVA: 0x00026C20 File Offset: 0x00024E20
		private int GetMaximumAmountOfVisibleHalfHearts()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "GetMaximumAmountOfVisibleHalfHearts", Array.Empty<object>());
			}
			int num = 0;
			Canvas.ForceUpdateCanvases();
			float num2 = this.heartHalfContainer.rect.width;
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0}: {1}", "area", num2), this.heartHalfContainer);
			}
			while (num2 > 0f)
			{
				num2 -= this.heartHalfSource.RectTransform.rect.width;
				bool flag = num == 0;
				bool flag2 = num % 2 == 0;
				if (!flag && !flag2)
				{
					num2 -= this.heartHalfSpaceSource.RectTransform.rect.width;
				}
				num++;
			}
			if (num <= 0)
			{
				num = 1;
			}
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0}: {1}", "maximumAmountOfVisibleHalfHearts", num), this);
			}
			return num;
		}

		// Token: 0x060007DF RID: 2015 RVA: 0x00026D1C File Offset: 0x00024F1C
		[CompilerGenerated]
		private void <DespawnHeartsDownTo>g__DespawnLastItemIfSpace|9_0()
		{
			if (this.poolableObjects.Count == 0)
			{
				return;
			}
			List<UIPoolableGameObject> list = this.poolableObjects;
			UIHeartHalfSpaceView uiheartHalfSpaceView = list[list.Count - 1] as UIHeartHalfSpaceView;
			if (uiheartHalfSpaceView == null)
			{
				return;
			}
			uiheartHalfSpaceView.HideAndDespawnOnComplete();
			this.poolableObjects.RemoveAt(this.poolableObjects.Count - 1);
		}

		// Token: 0x060007E0 RID: 2016 RVA: 0x00026D74 File Offset: 0x00024F74
		[CompilerGenerated]
		private int <ViewSurplus>g__GetCountOfSurplusOfHalfHearts|11_0(ref UIHeartsView.<>c__DisplayClass11_0 A_1)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log("GetCountOfSurplusOfHalfHearts", this);
			}
			int maximumAmountOfVisibleHalfHearts = this.GetMaximumAmountOfVisibleHalfHearts();
			int num = 0;
			if (A_1.model > maximumAmountOfVisibleHalfHearts)
			{
				num = A_1.model - maximumAmountOfVisibleHalfHearts;
			}
			return num;
		}

		// Token: 0x040006F4 RID: 1780
		[Header("UIHeartsView")]
		[SerializeField]
		private RectTransform heartHalfContainer;

		// Token: 0x040006F5 RID: 1781
		[SerializeField]
		private UIHeartHalfView heartHalfSource;

		// Token: 0x040006F6 RID: 1782
		[SerializeField]
		private UIHeartHalfSpaceView heartHalfSpaceSource;

		// Token: 0x040006F7 RID: 1783
		private readonly List<UIPoolableGameObject> poolableObjects = new List<UIPoolableGameObject>();

		// Token: 0x040006F8 RID: 1784
		private int halfHeartCount;
	}
}
