using System;
using System.Collections.Generic;
using Endless.Shared.Debugging;
using Endless.Shared.Validation;
using UnityEngine;

namespace Endless.Shared.UI
{
	// Token: 0x02000129 RID: 297
	public class UIRectTransformDictionary : UIGameObject, IValidatable
	{
		// Token: 0x17000138 RID: 312
		public RectTransformValue this[string key]
		{
			get
			{
				if (!this.initiated)
				{
					this.Initiate();
				}
				return this.dictionary[key];
			}
		}

		// Token: 0x0600073F RID: 1855 RVA: 0x0001E7A0 File Offset: 0x0001C9A0
		[ContextMenu("Validate")]
		public void Validate()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Validate", Array.Empty<object>());
			}
			HashSet<string> hashSet = new HashSet<string>();
			for (int i = 0; i < this.entries.Length; i++)
			{
				string key = this.entries[i].Key;
				if (!hashSet.Add(key))
				{
					DebugUtility.LogError("You have duplicate entries with the same key of '" + key + "'! Ensure each key is unique!", this);
				}
			}
		}

		// Token: 0x06000740 RID: 1856 RVA: 0x0001E810 File Offset: 0x0001CA10
		public bool Contains(string key)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Contains", new object[] { key });
			}
			if (this.initiated)
			{
				return this.dictionary.ContainsKey(key);
			}
			for (int i = 0; i < this.entries.Length; i++)
			{
				if (this.entries[i].Key == key)
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x06000741 RID: 1857 RVA: 0x0001E880 File Offset: 0x0001CA80
		public void Apply(string key)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Apply", new object[] { key });
			}
			if (!this.initiated)
			{
				this.Initiate();
			}
			RectTransformValue rectTransformValue;
			if (!this.dictionary.TryGetValue(key, out rectTransformValue))
			{
				DebugUtility.LogException(new KeyNotFoundException("key: " + key), this);
				return;
			}
			rectTransformValue.ApplyTo(base.RectTransform);
		}

		// Token: 0x06000742 RID: 1858 RVA: 0x0001E8EC File Offset: 0x0001CAEC
		private void Initiate()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Initiate", Array.Empty<object>());
			}
			this.initiated = true;
			for (int i = 0; i < this.entries.Length; i++)
			{
				UIRectTransformDictionary.UIRectTransformDictionaryEntry uirectTransformDictionaryEntry = this.entries[i];
				if (!this.dictionary.ContainsKey(uirectTransformDictionaryEntry.Key))
				{
					this.dictionary.Add(uirectTransformDictionaryEntry.Key, uirectTransformDictionaryEntry.Value);
				}
			}
		}

		// Token: 0x0400043B RID: 1083
		[SerializeField]
		private UIRectTransformDictionary.UIRectTransformDictionaryEntry[] entries = Array.Empty<UIRectTransformDictionary.UIRectTransformDictionaryEntry>();

		// Token: 0x0400043C RID: 1084
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;

		// Token: 0x0400043D RID: 1085
		[NonSerialized]
		private readonly Dictionary<string, RectTransformValue> dictionary = new Dictionary<string, RectTransformValue>();

		// Token: 0x0400043E RID: 1086
		[NonSerialized]
		private bool initiated;

		// Token: 0x0200012A RID: 298
		[Serializable]
		private struct UIRectTransformDictionaryEntry
		{
			// Token: 0x0400043F RID: 1087
			public string Key;

			// Token: 0x04000440 RID: 1088
			public RectTransformValue Value;
		}
	}
}
