using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Endless.Shared.Debugging;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Shared.UI
{
	// Token: 0x0200022D RID: 557
	public class UIClassMemberInfoHandler<TModel> : IClearable where TModel : class
	{
		// Token: 0x06000E27 RID: 3623 RVA: 0x0003D4A4 File Offset: 0x0003B6A4
		public UIClassMemberInfoHandler(TModel model, RectTransform parent, BindingFlags bindingFlags = BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy, bool verboseLogging = false)
		{
			UIClassMemberInfoHandler<TModel> <>4__this = this;
			if (verboseLogging)
			{
				DebugUtility.Log(string.Concat(new string[]
				{
					"new UIClassMemberInfoHandler ( model: ",
					model.DebugSafeJson(),
					", parent: ",
					parent.DebugSafeName(true),
					", bindingFlags: ",
					bindingFlags.DebugSafeJson(),
					" )"
				}), parent);
			}
			this.verboseLogging = verboseLogging;
			Type type = model.GetType();
			FieldInfo[] array = type.GetFields(bindingFlags).ToArray<FieldInfo>();
			PropertyInfo[] array2 = type.GetProperties(bindingFlags).ToArray<PropertyInfo>();
			List<ValueTuple<string, RectTransform>> list = new List<ValueTuple<string, RectTransform>>();
			if (verboseLogging)
			{
				DebugUtility.Log(string.Format("{0}: {1}", "fieldInfos", array.Length), parent);
				DebugUtility.Log(string.Format("{0}: {1}", "propertyInfos", array2.Length), parent);
			}
			FieldInfo[] array3 = array;
			for (int i = 0; i < array3.Length; i++)
			{
				FieldInfo fieldInfo = array3[i];
				object value = fieldInfo.GetValue(model);
				string text = fieldInfo.Name;
				text = this.CleanName(text);
				if (verboseLogging)
				{
					DebugUtility.Log(string.Format("{0}: {1}", text, value), parent);
				}
				UINamedViewPresenter uinamedViewPresenter = MonoBehaviourSingleton<UIPoolableViewPresenterSpawner>.Instance.SpawnNamedViewPresenter(parent, this.typeStyleDictionary);
				IUIPresentable presenter = this.GetPresenter(value, text, uinamedViewPresenter);
				if (presenter == null)
				{
					DebugUtility.LogWarning("Skipping presenter for the field " + text + " due to no prefab for it!", parent);
					MonoBehaviourSingleton<PoolManagerT>.Instance.Despawn<UINamedViewPresenter>(uinamedViewPresenter);
				}
				else
				{
					list.Add(new ValueTuple<string, RectTransform>(fieldInfo.Name, presenter.RectTransform));
					presenter.SetModelAsObject(value, false);
					this.namedViewPresenters.Add(uinamedViewPresenter);
					presenter.OnModelChanged += delegate(object newFieldInfoValue)
					{
						fieldInfo.SetValue(model, newFieldInfoValue);
						<>4__this.OnModelChanged.Invoke(model);
					};
				}
			}
			PropertyInfo[] array4 = array2;
			for (int i = 0; i < array4.Length; i++)
			{
				PropertyInfo propertyInfo = array4[i];
				object value2 = propertyInfo.GetValue(model);
				string propertyInfoName = propertyInfo.Name;
				propertyInfoName = this.CleanName(propertyInfoName);
				if (verboseLogging)
				{
					DebugUtility.Log(string.Format("{0}: {1}", propertyInfoName, value2), parent);
				}
				UINamedViewPresenter uinamedViewPresenter2 = MonoBehaviourSingleton<UIPoolableViewPresenterSpawner>.Instance.SpawnNamedViewPresenter(parent, this.typeStyleDictionary);
				IUIPresentable presenter2 = this.GetPresenter(value2, propertyInfoName, uinamedViewPresenter2);
				if (presenter2 == null)
				{
					DebugUtility.LogWarning("Skipping presenter for the property " + propertyInfoName + " due to no prefab for it!", parent);
					MonoBehaviourSingleton<PoolManagerT>.Instance.Despawn<UINamedViewPresenter>(uinamedViewPresenter2);
				}
				else
				{
					list.Add(new ValueTuple<string, RectTransform>(propertyInfo.Name, presenter2.RectTransform));
					presenter2.SetModelAsObject(value2, false);
					this.namedViewPresenters.Add(uinamedViewPresenter2);
					presenter2.OnModelChanged += delegate(object newPropertyInfoValue)
					{
						if (propertyInfo.CanWrite)
						{
							propertyInfo.SetValue(model, newPropertyInfoValue);
							<>4__this.OnModelChanged.Invoke(model);
							return;
						}
						<>4__this.OnReadOnlyPropertyChanged.Invoke(model, propertyInfoName, newPropertyInfoValue);
					};
				}
			}
			list = list.OrderBy(([TupleElementNames(new string[] { "name", "rectTransform" })] ValueTuple<string, RectTransform> item) => item.Item1).ToList<ValueTuple<string, RectTransform>>();
			foreach (ValueTuple<string, RectTransform> valueTuple in list)
			{
				valueTuple.Item2.SetAsFirstSibling();
			}
		}

		// Token: 0x170002A0 RID: 672
		// (get) Token: 0x06000E28 RID: 3624 RVA: 0x0003D88C File Offset: 0x0003BA8C
		public UnityEvent<TModel> OnModelChanged { get; } = new UnityEvent<TModel>();

		// Token: 0x170002A1 RID: 673
		// (get) Token: 0x06000E29 RID: 3625 RVA: 0x0003D894 File Offset: 0x0003BA94
		public UnityEvent<TModel, string, object> OnReadOnlyPropertyChanged { get; } = new UnityEvent<TModel, string, object>();

		// Token: 0x06000E2A RID: 3626 RVA: 0x0003D89C File Offset: 0x0003BA9C
		public void Clear()
		{
			if (this.verboseLogging)
			{
				DebugUtility.Log("Clear", null);
			}
			foreach (UINamedViewPresenter uinamedViewPresenter in this.namedViewPresenters)
			{
				MonoBehaviourSingleton<PoolManagerT>.Instance.Despawn<UINamedViewPresenter>(uinamedViewPresenter);
			}
			this.namedViewPresenters.Clear();
			this.typeStyleDictionary.Clear();
			this.OnModelChanged.RemoveAllListeners();
			this.OnReadOnlyPropertyChanged.RemoveAllListeners();
		}

		// Token: 0x06000E2B RID: 3627 RVA: 0x0003D934 File Offset: 0x0003BB34
		public void SetTypeStyleDictionary(Dictionary<Type, Enum> value)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "SetTypeStyleDictionary", new object[] { value.Count });
			}
			this.typeStyleDictionary = value;
		}

		// Token: 0x06000E2C RID: 3628 RVA: 0x0003D964 File Offset: 0x0003BB64
		private IUIPresentable GetPresenter(object model, string name, UINamedViewPresenter namedViewPresenter)
		{
			if (this.verboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2}, {3}: {4}, {5}: {6} )", new object[]
				{
					"GetPresenter",
					"model",
					model,
					"name",
					name,
					"namedViewPresenter",
					namedViewPresenter.DebugSafeName(true)
				}), null);
			}
			Type type = model.GetType();
			Enum @enum;
			if (!this.typeStyleDictionary.TryGetValue(type, out @enum))
			{
				return namedViewPresenter.SpawnObjectModelWithDefaultStyle(model, name);
			}
			return namedViewPresenter.SpawnObjectModelWithStyle(model, @enum, name);
		}

		// Token: 0x06000E2D RID: 3629 RVA: 0x0003D9EC File Offset: 0x0003BBEC
		private string CleanName(string name)
		{
			if (this.verboseLogging)
			{
				DebugUtility.Log("CleanName ( name: " + name + " )", null);
			}
			if (!name.Contains("k__BackingField"))
			{
				return name;
			}
			name = name.Replace("k__BackingField", string.Empty);
			name = name.Replace("<", string.Empty);
			name = name.Replace(">", string.Empty);
			return name;
		}

		// Token: 0x04000908 RID: 2312
		private const BindingFlags defaultBindingFlags = BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy;

		// Token: 0x04000909 RID: 2313
		private readonly List<UINamedViewPresenter> namedViewPresenters = new List<UINamedViewPresenter>();

		// Token: 0x0400090A RID: 2314
		private readonly bool verboseLogging;

		// Token: 0x0400090B RID: 2315
		private Dictionary<Type, Enum> typeStyleDictionary = new Dictionary<Type, Enum>();
	}
}
