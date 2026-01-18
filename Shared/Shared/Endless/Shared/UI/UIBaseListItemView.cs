using System;
using Endless.Shared.Debugging;
using Endless.Shared.Tweens;
using Endless.Shared.Validation;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Endless.Shared.UI
{
	// Token: 0x02000179 RID: 377
	[RequireComponent(typeof(LayoutElement))]
	public abstract class UIBaseListItemView<T> : UIGameObject, IPoolableT, IValidatable
	{
		// Token: 0x17000190 RID: 400
		// (get) Token: 0x06000940 RID: 2368 RVA: 0x00028182 File Offset: 0x00026382
		// (set) Token: 0x06000941 RID: 2369 RVA: 0x0002818A File Offset: 0x0002638A
		public LayoutElement LayoutElement { get; private set; }

		// Token: 0x17000191 RID: 401
		// (get) Token: 0x06000942 RID: 2370 RVA: 0x00028193 File Offset: 0x00026393
		// (set) Token: 0x06000943 RID: 2371 RVA: 0x0002819B File Offset: 0x0002639B
		public UIBaseListItemView<T>.SizeTypes SizeType { get; private set; }

		// Token: 0x17000192 RID: 402
		// (get) Token: 0x06000944 RID: 2372 RVA: 0x000281A4 File Offset: 0x000263A4
		// (set) Token: 0x06000945 RID: 2373 RVA: 0x000281AC File Offset: 0x000263AC
		public float ExplicitSize { get; private set; } = 53f;

		// Token: 0x17000193 RID: 403
		// (get) Token: 0x06000946 RID: 2374 RVA: 0x000281B5 File Offset: 0x000263B5
		// (set) Token: 0x06000947 RID: 2375 RVA: 0x000281BD File Offset: 0x000263BD
		public UIBaseListView<T> ListView { get; private set; }

		// Token: 0x17000194 RID: 404
		// (get) Token: 0x06000948 RID: 2376 RVA: 0x000281C6 File Offset: 0x000263C6
		// (set) Token: 0x06000949 RID: 2377 RVA: 0x000281CE File Offset: 0x000263CE
		public int DataIndex { get; private set; }

		// Token: 0x17000195 RID: 405
		// (get) Token: 0x0600094A RID: 2378 RVA: 0x000281D7 File Offset: 0x000263D7
		// (set) Token: 0x0600094B RID: 2379 RVA: 0x000281DF File Offset: 0x000263DF
		public int ViewIndex { get; private set; }

		// Token: 0x17000196 RID: 406
		// (get) Token: 0x0600094C RID: 2380 RVA: 0x000281E8 File Offset: 0x000263E8
		// (set) Token: 0x0600094D RID: 2381 RVA: 0x000281F0 File Offset: 0x000263F0
		protected bool VerboseLogging { get; set; }

		// Token: 0x17000197 RID: 407
		// (get) Token: 0x0600094E RID: 2382
		public abstract bool IsRow { get; }

		// Token: 0x17000198 RID: 408
		// (get) Token: 0x0600094F RID: 2383 RVA: 0x000281F9 File Offset: 0x000263F9
		public UnityEvent SpawnedUnityEvent { get; } = new UnityEvent();

		// Token: 0x17000199 RID: 409
		// (get) Token: 0x06000950 RID: 2384 RVA: 0x00028201 File Offset: 0x00026401
		public UnityEvent InitializedUnityEvent { get; } = new UnityEvent();

		// Token: 0x1700019A RID: 410
		// (get) Token: 0x06000951 RID: 2385 RVA: 0x00028209 File Offset: 0x00026409
		public UnityEvent ViewUnityEvent { get; } = new UnityEvent();

		// Token: 0x1700019B RID: 411
		// (get) Token: 0x06000952 RID: 2386 RVA: 0x00028211 File Offset: 0x00026411
		public UnityEvent DespawnedUnityEvent { get; } = new UnityEvent();

		// Token: 0x1700019C RID: 412
		// (get) Token: 0x06000953 RID: 2387 RVA: 0x00028219 File Offset: 0x00026419
		// (set) Token: 0x06000954 RID: 2388 RVA: 0x00028221 File Offset: 0x00026421
		public MonoBehaviour Prefab { get; set; }

		// Token: 0x1700019D RID: 413
		// (get) Token: 0x06000955 RID: 2389 RVA: 0x000050D2 File Offset: 0x000032D2
		public bool IsUi
		{
			get
			{
				return true;
			}
		}

		// Token: 0x1700019E RID: 414
		// (get) Token: 0x06000956 RID: 2390 RVA: 0x0002822A File Offset: 0x0002642A
		public UIBaseListModel<T> ListModel
		{
			get
			{
				return this.ListView.Model;
			}
		}

		// Token: 0x1700019F RID: 415
		// (get) Token: 0x06000957 RID: 2391 RVA: 0x00028237 File Offset: 0x00026437
		public T Model
		{
			get
			{
				return this.ListModel[this.DataIndex];
			}
		}

		// Token: 0x170001A0 RID: 416
		// (get) Token: 0x06000958 RID: 2392 RVA: 0x0002824A File Offset: 0x0002644A
		public object ModelAsObject
		{
			get
			{
				return this.Model;
			}
		}

		// Token: 0x170001A1 RID: 417
		// (get) Token: 0x06000959 RID: 2393 RVA: 0x00028257 File Offset: 0x00026457
		public bool IsSelected
		{
			get
			{
				return this.ListModel.IsSelected(this.DataIndex);
			}
		}

		// Token: 0x0600095A RID: 2394 RVA: 0x0002826C File Offset: 0x0002646C
		[ContextMenu("Validate")]
		public virtual void Validate()
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log("Validate", this);
			}
			if (!this.LayoutElement)
			{
				DebugUtility.LogError("LayoutElement is required!", this);
			}
			if (!this.onInitializeTweenCollection)
			{
				DebugUtility.LogError("onInitializeTweenCollection is required!", this);
			}
			if (!this.onViewTweenCollection)
			{
				DebugUtility.LogError("onViewTweenCollection is required!", this);
			}
			foreach (MaskableGraphic maskableGraphic in base.gameObject.GetComponentsInChildren<MaskableGraphic>(true))
			{
				if (!maskableGraphic.maskable)
				{
					DebugUtility.LogWarning(string.Concat(new string[]
					{
						"The MaskableGraphic field on the game object '",
						maskableGraphic.gameObject.name,
						"' is not set to TRUE. This cell, '",
						base.gameObject.name,
						"', will not clip unless that is fixed!"
					}), this);
				}
			}
		}

		// Token: 0x0600095B RID: 2395 RVA: 0x00028340 File Offset: 0x00026540
		public void SetDataIndex(int value)
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "SetDataIndex", "value", value), this);
			}
			this.DataIndex = value;
		}

		// Token: 0x0600095C RID: 2396 RVA: 0x00028371 File Offset: 0x00026571
		public virtual void OnSpawn()
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log("OnSpawn", this);
			}
			this.SpawnedUnityEvent.Invoke();
		}

		// Token: 0x0600095D RID: 2397 RVA: 0x00028391 File Offset: 0x00026591
		public virtual void OnDespawn()
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log("OnDespawn", this);
			}
			this.DespawnedUnityEvent.Invoke();
		}

		// Token: 0x0600095E RID: 2398 RVA: 0x000283B4 File Offset: 0x000265B4
		public virtual void Initialize(int dataIndex, int viewIndex)
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log("Initialize ( " + string.Format("{0}: {1}, ", "dataIndex", dataIndex) + string.Format("{0}: {1} )", "viewIndex", viewIndex), this);
			}
			this.DataIndex = dataIndex;
			this.ViewIndex = viewIndex;
			this.onInitializeTweenCollection.Tween();
			this.InitializedUnityEvent.Invoke();
		}

		// Token: 0x0600095F RID: 2399 RVA: 0x00028428 File Offset: 0x00026628
		public virtual void View(UIBaseListView<T> listView, int dataIndex)
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log(string.Concat(new string[]
				{
					"View ( listView: ",
					listView.DebugSafeName(true),
					", ",
					string.Format("{0}: {1} ) | ", "dataIndex", dataIndex),
					string.Format("Value: {0} ) | ", listView.Model[dataIndex]),
					string.Format("{0}: {1}", "Count", listView.Model.Count)
				}), this);
			}
			this.ListView = listView;
			this.DataIndex = dataIndex;
			this.onViewTweenCollection.Tween();
			this.ViewUnityEvent.Invoke();
		}

		// Token: 0x06000960 RID: 2400 RVA: 0x000284E7 File Offset: 0x000266E7
		public void SetOnViewTweenCollectionDelay(float delay)
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log("SetOnViewTweenCollectionDelay ( " + string.Format("{0}: {1} )", "delay", delay), this);
			}
			this.onViewTweenCollection.SetDelay(delay);
		}

		// Token: 0x06000961 RID: 2401 RVA: 0x00028522 File Offset: 0x00026722
		public override string ToString()
		{
			return string.Format("[ {0}: {1}, {2}: {3} ]", new object[] { "DataIndex", this.DataIndex, "ViewIndex", this.ViewIndex });
		}

		// Token: 0x040005E3 RID: 1507
		[SerializeField]
		private TweenCollection onInitializeTweenCollection;

		// Token: 0x040005E4 RID: 1508
		[SerializeField]
		private TweenCollection onViewTweenCollection;

		// Token: 0x0200017A RID: 378
		public enum SizeTypes
		{
			// Token: 0x040005EC RID: 1516
			Explicit,
			// Token: 0x040005ED RID: 1517
			ListWidth,
			// Token: 0x040005EE RID: 1518
			ListHeight
		}
	}
}
