using System;
using System.Collections.Generic;
using Endless.Shared.Debugging;
using UnityEngine;

namespace Endless.Shared.UI
{
	// Token: 0x02000235 RID: 565
	[CreateAssetMenu(menuName = "ScriptableObject/UI/Shared/Dictionaries/Presenter Dictionary", fileName = "Presenter Dictionary")]
	public class UIPresenterDictionary : ScriptableObject
	{
		// Token: 0x170002A4 RID: 676
		// (get) Token: 0x06000E4B RID: 3659 RVA: 0x0003E5D9 File Offset: 0x0003C7D9
		public IReadOnlyList<InterfaceReference<IUIPresentable>> Presenters
		{
			get
			{
				return this.presenters.Value;
			}
		}

		// Token: 0x06000E4C RID: 3660 RVA: 0x0003E5E6 File Offset: 0x0003C7E6
		private void OnEnable()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnEnable", Array.Empty<object>());
			}
			if (!this.dictionaryPopulated)
			{
				this.PopulateDictionary();
			}
		}

		// Token: 0x06000E4D RID: 3661 RVA: 0x0003E610 File Offset: 0x0003C810
		public IUIPresentable GetPresenterWithDefaultStyle(Type type)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "GetPresenterWithDefaultStyle", new object[] { type.Name });
			}
			this.typeArray[0] = type;
			this.typeArray[1] = type.BaseType;
			if (this.verboseLogging)
			{
				DebugUtility.Log("typeArray[0]: " + this.typeArray[0].Name, this);
				DebugUtility.Log("typeArray[1]: " + this.typeArray[1].Name, this);
			}
			foreach (Type type2 in this.typeArray)
			{
				Dictionary<Enum, IUIPresentable> dictionary;
				if (!(type2 == null) && this.dictionary.TryGetValue(type2, out dictionary))
				{
					Enum @enum = this.defaultStyleMap[type2];
					IUIPresentable iuipresentable;
					if (dictionary.TryGetValue(@enum, out iuipresentable))
					{
						return iuipresentable;
					}
					using (Dictionary<Enum, IUIPresentable>.ValueCollection.Enumerator enumerator = dictionary.Values.GetEnumerator())
					{
						if (enumerator.MoveNext())
						{
							return enumerator.Current;
						}
					}
				}
			}
			DebugUtility.LogException(new KeyNotFoundException(string.Format("No item with {0} of {1}", "type", type)), this);
			return null;
		}

		// Token: 0x06000E4E RID: 3662 RVA: 0x0003E748 File Offset: 0x0003C948
		public bool HasStyleForType(Type type, Enum style)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "HasStyleForType", new object[] { type, style });
			}
			Dictionary<Enum, IUIPresentable> dictionary;
			return this.dictionary.TryGetValue(type, out dictionary) && dictionary.ContainsKey(style);
		}

		// Token: 0x06000E4F RID: 3663 RVA: 0x0003E790 File Offset: 0x0003C990
		public IUIPresentable GetPresenterWithStyle(Type type, Enum style)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "GetPresenterWithStyle", new object[] { type, style });
			}
			Dictionary<Enum, IUIPresentable> dictionary;
			if (!this.dictionary.TryGetValue(type, out dictionary))
			{
				DebugUtility.LogException(new KeyNotFoundException(string.Format("No item with {0} of {1}", "type", type)), this);
				return null;
			}
			IUIPresentable iuipresentable;
			if (!dictionary.TryGetValue(style, out iuipresentable))
			{
				DebugUtility.LogException(new KeyNotFoundException(string.Format("No item with {0} of {1}", "style", style)), this);
				return null;
			}
			return iuipresentable;
		}

		// Token: 0x06000E50 RID: 3664 RVA: 0x0003E814 File Offset: 0x0003CA14
		public float GetSize(object model, LayoutReferenceType dimension, IReadOnlyDictionary<Type, Enum> typeStyleOverrideDictionary)
		{
			if (this.verboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2}, {3}: {4}, {5}: {6} )", new object[]
				{
					"GetSize",
					"model",
					model,
					"dimension",
					dimension,
					"typeStyleOverrideDictionary",
					(typeStyleOverrideDictionary == null) ? "null" : typeStyleOverrideDictionary.Count
				}), this);
			}
			Type type = model.GetType();
			return this.GetSize(model, type, dimension, typeStyleOverrideDictionary);
		}

		// Token: 0x06000E51 RID: 3665 RVA: 0x0003E898 File Offset: 0x0003CA98
		public float GetSize(Type type, LayoutReferenceType dimension, IReadOnlyDictionary<Type, Enum> typeStyleOverrideDictionary, IUIDynamicTypeFactory dynamicTypeFactory)
		{
			if (this.verboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2}, {3}: {4}, {5}: {6} )", new object[]
				{
					"GetSize",
					"type",
					type.Name,
					"dimension",
					dimension,
					"typeStyleOverrideDictionary",
					(typeStyleOverrideDictionary == null) ? "null" : typeStyleOverrideDictionary.Count
				}), this);
			}
			object obj;
			object obj2;
			if (UIPresenterDictionary.defaultModelDictionary.TryGetValue(type, out obj))
			{
				obj2 = obj;
			}
			else
			{
				obj2 = dynamicTypeFactory.Create(type);
				UIPresenterDictionary.defaultModelDictionary.Add(type, obj2);
			}
			return this.GetSize(obj2, type, dimension, typeStyleOverrideDictionary);
		}

		// Token: 0x06000E52 RID: 3666 RVA: 0x0003E940 File Offset: 0x0003CB40
		public float GetSize(object model, Type type, LayoutReferenceType dimension, IReadOnlyDictionary<Type, Enum> typeStyleOverrideDictionary)
		{
			if (this.verboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2}, {3}: {4}, {5}: {6}, {7}: {8} )", new object[]
				{
					"GetSize",
					"model",
					model,
					"type",
					type,
					"dimension",
					dimension,
					"typeStyleOverrideDictionary",
					(typeStyleOverrideDictionary == null) ? "null" : typeStyleOverrideDictionary.Count
				}), this);
			}
			float num = 0f;
			try
			{
				type = UIPresenterModelTypeUtility.SanitizeType(type);
				IUIPresentable iuipresentable;
				if (typeStyleOverrideDictionary == null)
				{
					iuipresentable = this.GetPresenterWithDefaultStyle(type);
				}
				else
				{
					Enum @enum;
					iuipresentable = (typeStyleOverrideDictionary.TryGetValue(type, out @enum) ? this.GetPresenterWithStyle(type, @enum) : this.GetPresenterWithDefaultStyle(type));
				}
				IUIViewable viewable = iuipresentable.Viewable;
				num = ((dimension == LayoutReferenceType.Height) ? viewable.GetPreferredHeight(model) : viewable.GetPreferredWidth(model));
				if (this.verboseLogging)
				{
					DebugUtility.Log(string.Format("{0}: {1}\n{2}: {3}", new object[] { "type", type.Name, "itemSize", num }), iuipresentable.RectTransform);
				}
			}
			catch (Exception ex)
			{
				DebugUtility.LogException(ex, this);
			}
			return num;
		}

		// Token: 0x06000E53 RID: 3667 RVA: 0x0003EA78 File Offset: 0x0003CC78
		private void PopulateDictionary()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "PopulateDictionary", Array.Empty<object>());
			}
			if (this.dictionaryPopulated)
			{
				return;
			}
			foreach (object obj in this.presenters)
			{
				IUIPresentable @interface = ((InterfaceReference<IUIPresentable>)obj).Interface;
				Type modelType = @interface.ModelType;
				if (!this.dictionary.ContainsKey(modelType))
				{
					this.dictionary[modelType] = new Dictionary<Enum, IUIPresentable>();
				}
				Dictionary<Enum, IUIPresentable> dictionary = this.dictionary[modelType];
				Enum style = @interface.Style;
				if (dictionary.TryAdd(style, @interface))
				{
					if (this.verboseLogging)
					{
						DebugUtility.Log(string.Format("Added {0}: {1} with {2} of {3}", new object[] { "modelType", modelType, "style", style }), this);
					}
				}
				else
				{
					DebugUtility.LogError(string.Format("A {0} of {1} for a {2} of {3} has already been added to {4}!", new object[] { "style", style, "modelType", modelType.Name, "styleDictionary" }), this);
				}
			}
			foreach (KeyValuePair<Type, Dictionary<Enum, IUIPresentable>> keyValuePair in this.dictionary)
			{
				Type key = keyValuePair.Key;
				Dictionary<Enum, IUIPresentable> value = keyValuePair.Value;
				Enum @enum = null;
				foreach (Enum enum2 in value.Keys)
				{
					if (Convert.ToInt32(enum2) == 0)
					{
						@enum = enum2;
						break;
					}
				}
				if (@enum == null)
				{
					using (Dictionary<Enum, IUIPresentable>.KeyCollection.Enumerator enumerator4 = value.Keys.GetEnumerator())
					{
						if (enumerator4.MoveNext())
						{
							@enum = enumerator4.Current;
						}
					}
				}
				this.defaultStyleMap[key] = @enum;
			}
			this.dictionaryPopulated = true;
		}

		// Token: 0x04000923 RID: 2339
		private static readonly Dictionary<Type, object> defaultModelDictionary = new Dictionary<Type, object>();

		// Token: 0x04000924 RID: 2340
		[SerializeField]
		private UIPresenterScriptableObjectArray presenters;

		// Token: 0x04000925 RID: 2341
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;

		// Token: 0x04000926 RID: 2342
		private readonly Dictionary<Type, Enum> defaultStyleMap = new Dictionary<Type, Enum>();

		// Token: 0x04000927 RID: 2343
		private readonly Dictionary<Type, Dictionary<Enum, IUIPresentable>> dictionary = new Dictionary<Type, Dictionary<Enum, IUIPresentable>>();

		// Token: 0x04000928 RID: 2344
		private readonly Type[] typeArray = new Type[2];

		// Token: 0x04000929 RID: 2345
		[NonSerialized]
		private bool dictionaryPopulated;
	}
}
