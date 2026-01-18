using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Endless.Shared.Debugging;
using UnityEngine;

namespace Endless.Shared.UI
{
	// Token: 0x02000234 RID: 564
	public class UIPoolableViewPresenterSpawner : UIMonoBehaviourSingleton<UIPoolableViewPresenterSpawner>
	{
		// Token: 0x06000E40 RID: 3648 RVA: 0x0003DD5C File Offset: 0x0003BF5C
		public UIBasePresenter<TModel> SpawnModelWithDefaultStyle<TModel>(TModel model, RectTransform parent)
		{
			if (this.verboseLogging)
			{
				DebugUtility.Log(string.Format("{0} <{1}: {2}> ( {3}: {4}, {5}: {6} )", new object[]
				{
					"SpawnModelWithDefaultStyle",
					"TModel",
					typeof(TModel),
					"model",
					model.DebugSafeJson(),
					"parent",
					parent.DebugSafeName(true)
				}), this);
			}
			Type typeFromHandle = typeof(TModel);
			IUIPresentable presenterWithDefaultStyle = this.presenterDictionary.GetPresenterWithDefaultStyle(typeFromHandle);
			UIBasePresenter<TModel> uibasePresenter = presenterWithDefaultStyle.SpawnPooledInstance(parent) as UIBasePresenter<TModel>;
			if (!uibasePresenter)
			{
				DebugUtility.LogException(new InvalidCastException("Could cast " + presenterWithDefaultStyle.RectTransform.name + " to UIBasePresenter!"), this);
				return null;
			}
			uibasePresenter.SetModel(model, false);
			return uibasePresenter;
		}

		// Token: 0x06000E41 RID: 3649 RVA: 0x0003DE2C File Offset: 0x0003C02C
		public UIBasePresenter<TModel> SpawnModelWithStyle<TModel, TView, TViewStyle>(TModel model, TViewStyle style, RectTransform parent) where TView : IUIViewStylable<TViewStyle> where TViewStyle : Enum
		{
			if (this.verboseLogging)
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
					"parent",
					parent.DebugSafeName(true)
				}), this);
			}
			Type typeFromHandle = typeof(TModel);
			IUIPresentable presenterWithStyle = this.presenterDictionary.GetPresenterWithStyle(typeFromHandle, style);
			UIBasePresenter<TModel> uibasePresenter = presenterWithStyle.SpawnPooledInstance(parent) as UIBasePresenter<TModel>;
			if (!uibasePresenter)
			{
				DebugUtility.LogException(new InvalidCastException("Could cast " + presenterWithStyle.RectTransform.name + " to UIBasePresenter!"), this);
				return null;
			}
			uibasePresenter.SetModel(model, false);
			return uibasePresenter;
		}

		// Token: 0x06000E42 RID: 3650 RVA: 0x0003DF44 File Offset: 0x0003C144
		public UIEnumPresenter SpawnEnumWithDefaultStyle(Enum model, RectTransform parent)
		{
			if (this.verboseLogging)
			{
				DebugUtility.Log(string.Concat(new string[]
				{
					"SpawnEnumWithDefaultStyle ( model: ",
					model.DebugSafeJson(),
					", parent: ",
					parent.DebugSafeName(true),
					" )"
				}), this);
			}
			Type typeFromHandle = typeof(Enum);
			IUIPresentable presenterWithDefaultStyle = this.presenterDictionary.GetPresenterWithDefaultStyle(typeFromHandle);
			UIEnumPresenter uienumPresenter = presenterWithDefaultStyle.SpawnPooledInstance(parent) as UIEnumPresenter;
			if (!uienumPresenter)
			{
				DebugUtility.LogException(new InvalidCastException("Could cast " + presenterWithDefaultStyle.RectTransform.name + " to UIEnumPresenter!"), this);
				return null;
			}
			uienumPresenter.SetModel(model, false);
			return uienumPresenter;
		}

		// Token: 0x06000E43 RID: 3651 RVA: 0x0003DFF4 File Offset: 0x0003C1F4
		public UIEnumPresenter SpawnEnumWithStyle(Enum model, UIBaseEnumView.Styles style, RectTransform parent)
		{
			if (this.verboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2}, {3}: {4}, {5}: {6} )", new object[]
				{
					"SpawnEnumWithStyle",
					"model",
					model.DebugSafeJson(),
					"style",
					style,
					"parent",
					parent.DebugSafeName(true)
				}), this);
			}
			Type typeFromHandle = typeof(Enum);
			IUIPresentable presenterWithStyle = this.presenterDictionary.GetPresenterWithStyle(typeFromHandle, style);
			UIEnumPresenter uienumPresenter = presenterWithStyle.SpawnPooledInstance(parent) as UIEnumPresenter;
			if (!uienumPresenter)
			{
				DebugUtility.LogException(new InvalidCastException("Could cast " + presenterWithStyle.RectTransform.name + " to UIEnumPresenter!"), this);
				return null;
			}
			uienumPresenter.SetModel(model, false);
			return uienumPresenter;
		}

		// Token: 0x06000E44 RID: 3652 RVA: 0x0003E0C0 File Offset: 0x0003C2C0
		public UIIEnumerablePresenter SpawnEnumerableWithDefaultStyle(IEnumerable model, RectTransform parent, Dictionary<Type, Enum> typeStyleOverrideDictionary)
		{
			if (this.verboseLogging)
			{
				DebugUtility.Log(string.Concat(new string[]
				{
					"SpawnEnumerableWithDefaultStyle ( model: ",
					model.DebugSafeJson(),
					", parent: ",
					parent.DebugSafeName(true),
					", typeStyleOverrideDictionary: ",
					typeStyleOverrideDictionary.DebugSafeCount<KeyValuePair<Type, Enum>>(),
					" )"
				}), this);
			}
			Type typeFromHandle = typeof(IEnumerable);
			IUIPresentable presenterWithDefaultStyle = this.presenterDictionary.GetPresenterWithDefaultStyle(typeFromHandle);
			UIIEnumerablePresenter uiienumerablePresenter = presenterWithDefaultStyle.SpawnPooledInstance(parent) as UIIEnumerablePresenter;
			if (!uiienumerablePresenter)
			{
				DebugUtility.LogException(new InvalidCastException("Could cast " + presenterWithDefaultStyle.RectTransform.name + " to UIIEnumerablePresenter!"), this);
				return null;
			}
			IUITypeStyleOverridable iuitypeStyleOverridable = uiienumerablePresenter.Viewable as IUITypeStyleOverridable;
			if (iuitypeStyleOverridable != null && typeStyleOverrideDictionary != null)
			{
				iuitypeStyleOverridable.SetTypeStyleOverrideDictionary(typeStyleOverrideDictionary);
			}
			uiienumerablePresenter.SetModel(model, false);
			return uiienumerablePresenter;
		}

		// Token: 0x06000E45 RID: 3653 RVA: 0x0003E198 File Offset: 0x0003C398
		public UIIEnumerablePresenter SpawnEnumerableWithStyle(IEnumerable model, UIBaseIEnumerableView.ArrangementStyle style, RectTransform parent, Dictionary<Type, Enum> typeStyleOverrideDictionary)
		{
			if (this.verboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2}, {3}: {4}, {5}: {6}, {7}: {8} )", new object[]
				{
					"SpawnEnumerableWithStyle",
					"model",
					model.DebugSafeJson(),
					"style",
					style,
					"parent",
					parent.DebugSafeName(true),
					"typeStyleOverrideDictionary",
					typeStyleOverrideDictionary.DebugSafeCount<KeyValuePair<Type, Enum>>()
				}), this);
			}
			Type typeFromHandle = typeof(IEnumerable);
			IUIPresentable presenterWithStyle = this.presenterDictionary.GetPresenterWithStyle(typeFromHandle, style);
			UIIEnumerablePresenter uiienumerablePresenter = presenterWithStyle.SpawnPooledInstance(parent) as UIIEnumerablePresenter;
			if (!uiienumerablePresenter)
			{
				DebugUtility.LogException(new InvalidCastException("Could cast " + presenterWithStyle.RectTransform.name + " to UIIEnumerablePresenter!"), this);
				return null;
			}
			IUITypeStyleOverridable iuitypeStyleOverridable = uiienumerablePresenter.Viewable as IUITypeStyleOverridable;
			if (iuitypeStyleOverridable != null && typeStyleOverrideDictionary != null)
			{
				iuitypeStyleOverridable.SetTypeStyleOverrideDictionary(typeStyleOverrideDictionary);
			}
			uiienumerablePresenter.SetModel(model, false);
			return uiienumerablePresenter;
		}

		// Token: 0x06000E46 RID: 3654 RVA: 0x0003E290 File Offset: 0x0003C490
		public IUIPresentable SpawnObjectModelWithDefaultStyle(object model, RectTransform parent, Dictionary<Type, Enum> typeStyleOverrideDictionary)
		{
			if (this.verboseLogging)
			{
				DebugUtility.Log(string.Concat(new string[]
				{
					"SpawnObjectModelWithDefaultStyle ( model: ",
					model.DebugSafeJson(),
					", parent: ",
					parent.DebugSafeName(true),
					", typeStyleOverrideDictionary: ",
					typeStyleOverrideDictionary.DebugSafeCount<KeyValuePair<Type, Enum>>(),
					" )"
				}), this);
			}
			Type type = UIPresenterModelTypeUtility.SanitizeType(model.GetType());
			IUIPresentable presenterWithDefaultStyle = this.presenterDictionary.GetPresenterWithDefaultStyle(type);
			if (presenterWithDefaultStyle == null)
			{
				return null;
			}
			IUIPresentable iuipresentable = presenterWithDefaultStyle.SpawnPooledInstance(parent);
			if (iuipresentable != null)
			{
				IUITypeStyleOverridable iuitypeStyleOverridable = iuipresentable.Viewable as IUITypeStyleOverridable;
				if (iuitypeStyleOverridable != null)
				{
					iuitypeStyleOverridable.SetTypeStyleOverrideDictionary(typeStyleOverrideDictionary);
				}
				iuipresentable.SetModelAsObject(model, false);
				return iuipresentable;
			}
			DebugUtility.LogException(new InvalidCastException("Could not cast " + presenterWithDefaultStyle.RectTransform.name + " to IUIPresentable!"), this);
			return null;
		}

		// Token: 0x06000E47 RID: 3655 RVA: 0x0003E364 File Offset: 0x0003C564
		public IUIPresentable SpawnObjectModelWithStyle(object model, Enum style, RectTransform parent, Dictionary<Type, Enum> typeStyleOverrideDictionary)
		{
			if (this.verboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2}, {3}: {4}, {5}: {6}, {7}: {8} )", new object[]
				{
					"SpawnObjectModelWithStyle",
					"model",
					model.DebugSafeJson(),
					"style",
					style,
					"parent",
					parent.DebugSafeName(true),
					"typeStyleOverrideDictionary",
					typeStyleOverrideDictionary.DebugSafeCount<KeyValuePair<Type, Enum>>()
				}), this);
			}
			Type type = UIPresenterModelTypeUtility.SanitizeType(model.GetType());
			IUIPresentable iuipresentable;
			if (this.presenterDictionary.HasStyleForType(type, style))
			{
				iuipresentable = this.presenterDictionary.GetPresenterWithStyle(type, style);
			}
			else
			{
				DebugUtility.LogWarning(string.Format("{0} ( {1}: {2}, {3}: {4}, {5}: {6} ) | Could not find {7}!", new object[]
				{
					"SpawnObjectModelWithStyle",
					"model",
					model.DebugSafeJson(),
					"style",
					style,
					"parent",
					parent.DebugSafeName(true),
					"style"
				}), this);
				iuipresentable = this.presenterDictionary.GetPresenterWithDefaultStyle(type);
			}
			if (iuipresentable == null)
			{
				return null;
			}
			IUIPresentable iuipresentable2 = iuipresentable.SpawnPooledInstance(parent);
			if (iuipresentable2 != null)
			{
				IUITypeStyleOverridable iuitypeStyleOverridable = iuipresentable2.Viewable as IUITypeStyleOverridable;
				if (iuitypeStyleOverridable != null)
				{
					iuitypeStyleOverridable.SetTypeStyleOverrideDictionary(typeStyleOverrideDictionary);
				}
				iuipresentable2.SetModelAsObject(model, false);
				return iuipresentable2;
			}
			DebugUtility.LogException(new InvalidCastException("Could not cast " + iuipresentable.RectTransform.name + " to IUIPresentable!"), this);
			return null;
		}

		// Token: 0x06000E48 RID: 3656 RVA: 0x0003E4C0 File Offset: 0x0003C6C0
		public UIClassMemberInfoHandler<TModel> SpawnForEachFieldInClass<TModel>(TModel target, RectTransform parent, bool verboseLoggingFlag = false) where TModel : class
		{
			if (this.verboseLogging)
			{
				DebugUtility.Log(string.Format("{0}<{1}: {2}> ( {3}: {4}, {5}: {6} )", new object[]
				{
					"SpawnForEachFieldInClass",
					"TModel",
					typeof(TModel),
					"target",
					target.DebugSafeJson(),
					"parent",
					parent.DebugSafeName(true)
				}), null);
			}
			if (target == null)
			{
				DebugUtility.LogException(new ArgumentNullException("target"), this);
				return null;
			}
			return new UIClassMemberInfoHandler<TModel>(target, parent, BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy, verboseLoggingFlag);
		}

		// Token: 0x06000E49 RID: 3657 RVA: 0x0003E554 File Offset: 0x0003C754
		public UINamedViewPresenter SpawnNamedViewPresenter(RectTransform parent, Dictionary<Type, Enum> typeStyleOverrideDictionary)
		{
			if (this.verboseLogging)
			{
				DebugUtility.Log(string.Concat(new string[]
				{
					"SpawnNamedViewPresenter ( parent: ",
					parent.DebugSafeName(true),
					", typeStyleOverrideDictionary: ",
					typeStyleOverrideDictionary.DebugSafeCount<KeyValuePair<Type, Enum>>(),
					" )"
				}), this);
			}
			UINamedViewPresenter uinamedViewPresenter = MonoBehaviourSingleton<PoolManagerT>.Instance.Spawn<UINamedViewPresenter>(this.namedViewPresenterSource, default(Vector3), default(Quaternion), parent);
			uinamedViewPresenter.SetTypeStyleOverrideDictionary(typeStyleOverrideDictionary);
			return uinamedViewPresenter;
		}

		// Token: 0x04000920 RID: 2336
		[SerializeField]
		private UIPresenterDictionary presenterDictionary;

		// Token: 0x04000921 RID: 2337
		[SerializeField]
		private UINamedViewPresenter namedViewPresenterSource;

		// Token: 0x04000922 RID: 2338
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;
	}
}
