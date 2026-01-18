using System;
using System.Collections;
using System.Collections.Generic;
using Endless.Shared.Debugging;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Endless.Shared.UI
{
	// Token: 0x02000206 RID: 518
	public class UINamedViewPresenter : UIPoolableGameObject, IClearable
	{
		// Token: 0x06000D7A RID: 3450 RVA: 0x0003B3E0 File Offset: 0x000395E0
		public void Clear()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log("Clear", null);
			}
			if (this.presenterCache != null)
			{
				this.presenterCache.ReturnToPool();
				this.presenterCache = null;
			}
			if (this.classMemberInfoHandlerCache != null)
			{
				this.classMemberInfoHandlerCache.Clear();
				this.classMemberInfoHandlerCache = null;
			}
			this.typeStyleOverrideDictionary.Clear();
		}

		// Token: 0x06000D7B RID: 3451 RVA: 0x0003B43F File Offset: 0x0003963F
		public override void OnDespawn()
		{
			base.OnDespawn();
			this.Clear();
		}

		// Token: 0x06000D7C RID: 3452 RVA: 0x0003B450 File Offset: 0x00039650
		public UIBasePresenter<TModel> SpawnModelWithDefaultStyle<TModel>(TModel model, string name)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} <{1}: {2}> ( {3}: {4}, {5}: {6} )", new object[]
				{
					"SpawnModelWithDefaultStyle",
					"TModel",
					typeof(TModel),
					"model",
					model.DebugSafeJson(),
					"name",
					name
				}), this);
			}
			UIBasePresenter<TModel> uibasePresenter = MonoBehaviourSingleton<UIPoolableViewPresenterSpawner>.Instance.SpawnModelWithDefaultStyle<TModel>(model, this.container);
			this.SetPresenterLayoutNameAndAlignment(name, uibasePresenter, model);
			return uibasePresenter;
		}

		// Token: 0x06000D7D RID: 3453 RVA: 0x0003B4DC File Offset: 0x000396DC
		public UIBasePresenter<TModel> SpawnModelWithStyle<TModel, TView, TViewStyle>(TModel model, TViewStyle style, string name) where TView : IUIViewStylable<TViewStyle> where TViewStyle : Enum
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} <{1}: {2}, {3}: {4}, {5}: {6}> ( {7}: {8}, {9}: {10}, {11}: {12} )", new object[]
				{
					"SpawnModelWithStyle",
					"TModel",
					typeof(TModel),
					"TView",
					typeof(TView),
					"TViewStyle",
					typeof(TViewStyle),
					"model",
					model.DebugSafeJson(),
					"style",
					style,
					"name",
					name
				}), this);
			}
			UIBasePresenter<TModel> uibasePresenter = MonoBehaviourSingleton<UIPoolableViewPresenterSpawner>.Instance.SpawnModelWithStyle<TModel, TView, TViewStyle>(model, style, this.container);
			this.SetPresenterLayoutNameAndAlignment(name, uibasePresenter, model);
			return uibasePresenter;
		}

		// Token: 0x06000D7E RID: 3454 RVA: 0x0003B5AC File Offset: 0x000397AC
		public UIEnumPresenter SpawnEnumWithDefaultStyle(Enum model, string name)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Concat(new string[]
				{
					"SpawnEnumWithDefaultStyle ( model: ",
					model.DebugSafeJson(),
					", name: ",
					name,
					" )"
				}), this);
			}
			UIEnumPresenter uienumPresenter = MonoBehaviourSingleton<UIPoolableViewPresenterSpawner>.Instance.SpawnEnumWithDefaultStyle(model, this.container);
			this.SetPresenterLayoutNameAndAlignment(name, uienumPresenter, model);
			return uienumPresenter;
		}

		// Token: 0x06000D7F RID: 3455 RVA: 0x0003B614 File Offset: 0x00039814
		public UIEnumPresenter SpawnEnumWithStyle(Enum model, UIBaseEnumView.Styles style, string name)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2}, {3}: {4}, {5}: {6} )", new object[]
				{
					"SpawnEnumWithStyle",
					"model",
					model.DebugSafeJson(),
					"style",
					style,
					"name",
					name
				}), this);
			}
			UIEnumPresenter uienumPresenter = MonoBehaviourSingleton<UIPoolableViewPresenterSpawner>.Instance.SpawnEnumWithStyle(model, style, this.container);
			this.SetPresenterLayoutNameAndAlignment(name, uienumPresenter, model);
			return uienumPresenter;
		}

		// Token: 0x06000D80 RID: 3456 RVA: 0x0003B694 File Offset: 0x00039894
		public UIIEnumerablePresenter SpawnEnumerableWithDefaultStyle(IEnumerable model, string name)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Concat(new string[]
				{
					"SpawnEnumerableWithDefaultStyle ( model: ",
					model.DebugSafeJson(),
					", name: ",
					name,
					" )"
				}), this);
			}
			UIIEnumerablePresenter uiienumerablePresenter = MonoBehaviourSingleton<UIPoolableViewPresenterSpawner>.Instance.SpawnEnumerableWithDefaultStyle(model, this.container, this.typeStyleOverrideDictionary);
			this.SetPresenterLayoutNameAndAlignment(name, uiienumerablePresenter, model);
			return uiienumerablePresenter;
		}

		// Token: 0x06000D81 RID: 3457 RVA: 0x0003B704 File Offset: 0x00039904
		public UIIEnumerablePresenter SpawnEnumerableWithStyle(IEnumerable model, UIBaseIEnumerableView.ArrangementStyle style, string name)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2}, {3}: {4}, {5}: {6} )", new object[]
				{
					"SpawnEnumerableWithStyle",
					"model",
					model.DebugSafeJson(),
					"style",
					style,
					"name",
					name
				}), this);
			}
			UIIEnumerablePresenter uiienumerablePresenter = MonoBehaviourSingleton<UIPoolableViewPresenterSpawner>.Instance.SpawnEnumerableWithStyle(model, style, this.container, this.typeStyleOverrideDictionary);
			this.SetPresenterLayoutNameAndAlignment(name, uiienumerablePresenter, model);
			return uiienumerablePresenter;
		}

		// Token: 0x06000D82 RID: 3458 RVA: 0x0003B788 File Offset: 0x00039988
		public IUIPresentable SpawnObjectModelWithDefaultStyle(object model, string name)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Concat(new string[]
				{
					"SpawnObjectModelWithDefaultStyle ( model: ",
					model.DebugSafeJson(),
					", name: ",
					name,
					" )"
				}), this);
			}
			IUIPresentable iuipresentable = MonoBehaviourSingleton<UIPoolableViewPresenterSpawner>.Instance.SpawnObjectModelWithDefaultStyle(model, this.container, this.typeStyleOverrideDictionary);
			this.SetPresenterLayoutNameAndAlignment(name, iuipresentable, model);
			return iuipresentable;
		}

		// Token: 0x06000D83 RID: 3459 RVA: 0x0003B7F8 File Offset: 0x000399F8
		public IUIPresentable SpawnObjectModelWithStyle(object model, Enum style, string name)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2}, {3}: {4}, {5}: {6}, {7}: {8} )", new object[]
				{
					"SpawnObjectModelWithStyle",
					"model",
					model.DebugSafeJson(),
					"style",
					style,
					"name",
					name,
					"typeStyleOverrideDictionary",
					this.typeStyleOverrideDictionary.Count
				}), this);
			}
			IUIPresentable iuipresentable = MonoBehaviourSingleton<UIPoolableViewPresenterSpawner>.Instance.SpawnObjectModelWithStyle(model, style, this.container, this.typeStyleOverrideDictionary);
			this.SetPresenterLayoutNameAndAlignment(name, iuipresentable, model);
			return iuipresentable;
		}

		// Token: 0x06000D84 RID: 3460 RVA: 0x0003B894 File Offset: 0x00039A94
		public UIClassMemberInfoHandler<TModel> SpawnForEachFieldInClass<TModel>(TModel target, string name) where TModel : class
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0}<{1}: {2}> ( {3}: {4}, {5}: {6} )", new object[]
				{
					"SpawnForEachFieldInClass",
					"TModel",
					typeof(TModel),
					"target",
					target.DebugSafeJson(),
					"name",
					name
				}), null);
			}
			this.nameText.text = name;
			UIClassMemberInfoHandler<TModel> uiclassMemberInfoHandler = MonoBehaviourSingleton<UIPoolableViewPresenterSpawner>.Instance.SpawnForEachFieldInClass<TModel>(target, this.container, false);
			this.classMemberInfoHandlerCache = uiclassMemberInfoHandler;
			this.nameText.alignment = ((this.layoutElement.preferredHeight > 75f) ? TextAlignmentOptions.TopLeft : TextAlignmentOptions.Left);
			return uiclassMemberInfoHandler;
		}

		// Token: 0x06000D85 RID: 3461 RVA: 0x0003B950 File Offset: 0x00039B50
		private void SetPresenterLayoutNameAndAlignment(string name, IUIPresentable presenter, object model)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2}, {3}: {4}, {5}: {6} )", new object[] { "SetPresenterLayoutNameAndAlignment", "name", name, "name", name, "model", model }), null);
			}
			this.nameText.text = name;
			this.presenterCache = presenter;
			this.layoutElement.preferredHeight = presenter.Viewable.GetPreferredHeight(model);
			this.nameText.alignment = ((this.layoutElement.preferredHeight > 75f) ? TextAlignmentOptions.TopLeft : TextAlignmentOptions.Left);
			presenter.RectTransform.SetAsLastSibling();
		}

		// Token: 0x06000D86 RID: 3462 RVA: 0x0003BA05 File Offset: 0x00039C05
		public void SetTypeStyleOverrideDictionary(Dictionary<Type, Enum> typeStyleOverrideDictionary)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log("SetTypeStyleOverrideDictionary ( typeStyleOverrideDictionary: " + typeStyleOverrideDictionary.DebugSafeCount<KeyValuePair<Type, Enum>>() + " )", null);
			}
			this.typeStyleOverrideDictionary = typeStyleOverrideDictionary;
		}

		// Token: 0x040008BB RID: 2235
		private const int ALIGN_NAME_TEXT_TO_TOP_LEFT_IF_PREFERRED_HEIGHT_IS_GREATER_THAN = 75;

		// Token: 0x040008BC RID: 2236
		[Header("UINamedViewPresenter")]
		[SerializeField]
		private TextMeshProUGUI nameText;

		// Token: 0x040008BD RID: 2237
		[SerializeField]
		private RectTransform container;

		// Token: 0x040008BE RID: 2238
		[SerializeField]
		private LayoutElement layoutElement;

		// Token: 0x040008BF RID: 2239
		private IClearable classMemberInfoHandlerCache;

		// Token: 0x040008C0 RID: 2240
		private IUIPresentable presenterCache;

		// Token: 0x040008C1 RID: 2241
		private Dictionary<Type, Enum> typeStyleOverrideDictionary = new Dictionary<Type, Enum>();
	}
}
