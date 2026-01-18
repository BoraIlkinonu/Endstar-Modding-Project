using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace Endless.Shared
{
	// Token: 0x0200006C RID: 108
	[Serializable]
	public class LocalizedString : INetworkSerializable, IEquatable<LocalizedString>
	{
		// Token: 0x1700008D RID: 141
		// (get) Token: 0x06000363 RID: 867 RVA: 0x000043C6 File Offset: 0x000025C6
		public static Language ActiveLanguage
		{
			get
			{
				return Language.English;
			}
		}

		// Token: 0x06000364 RID: 868 RVA: 0x0000FDA4 File Offset: 0x0000DFA4
		public override string ToString()
		{
			return string.Format("{0}: {1}, ", "ActiveLanguage", LocalizedString.ActiveLanguage) + string.Format("{0}: {1}, ", "originalLanguage", this.originalLanguage) + "localizedStrings: " + StringUtility.CommaSeparateClasses<LocalizedString.LanguageStringPair>(this.localizedStrings);
		}

		// Token: 0x1700008E RID: 142
		// (get) Token: 0x06000365 RID: 869 RVA: 0x0000FDFC File Offset: 0x0000DFFC
		private Dictionary<Language, LocalizedString.LanguageStringPair> LocalizedStringMap
		{
			get
			{
				if (this.localizedStringMap == null)
				{
					this.localizedStringMap = new Dictionary<Language, LocalizedString.LanguageStringPair>();
					foreach (LocalizedString.LanguageStringPair languageStringPair in this.localizedStrings)
					{
						this.localizedStringMap.Add(languageStringPair.Language, languageStringPair);
					}
				}
				return this.localizedStringMap;
			}
		}

		// Token: 0x06000366 RID: 870 RVA: 0x0000FE74 File Offset: 0x0000E074
		public LocalizedString()
		{
		}

		// Token: 0x06000367 RID: 871 RVA: 0x0000FE87 File Offset: 0x0000E087
		public LocalizedString(string defaultString)
		{
			this.SetStringValue(defaultString, Language.English);
		}

		// Token: 0x06000368 RID: 872 RVA: 0x0000FEA2 File Offset: 0x0000E0A2
		public LocalizedString(Language language, string defaultString)
		{
			this.SetStringValue(defaultString, language);
		}

		// Token: 0x06000369 RID: 873 RVA: 0x0000FEC0 File Offset: 0x0000E0C0
		public void SetStringValue(string newValue, Language language)
		{
			LocalizedString.LanguageStringPair languageStringPair;
			if (this.LocalizedStringMap.TryGetValue(language, out languageStringPair))
			{
				languageStringPair.String = newValue;
				Action<Language> onTextChanged = this.OnTextChanged;
				if (onTextChanged == null)
				{
					return;
				}
				onTextChanged(language);
				return;
			}
			else
			{
				LocalizedString.LanguageStringPair languageStringPair2 = new LocalizedString.LanguageStringPair
				{
					Language = language,
					String = newValue
				};
				this.localizedStrings.Add(languageStringPair2);
				this.LocalizedStringMap.Add(language, languageStringPair2);
				Action<Language> onTextChanged2 = this.OnTextChanged;
				if (onTextChanged2 == null)
				{
					return;
				}
				onTextChanged2(language);
				return;
			}
		}

		// Token: 0x0600036A RID: 874 RVA: 0x0000FF34 File Offset: 0x0000E134
		public string GetLocalizedString()
		{
			return this.GetString(LocalizedString.ActiveLanguage);
		}

		// Token: 0x0600036B RID: 875 RVA: 0x0000FF41 File Offset: 0x0000E141
		public string GetOriginalString()
		{
			return this.GetString(this.originalLanguage);
		}

		// Token: 0x0600036C RID: 876 RVA: 0x0000FF50 File Offset: 0x0000E150
		public string GetString(Language language)
		{
			LocalizedString.LanguageStringPair languageStringPair;
			if (this.LocalizedStringMap.TryGetValue(language, out languageStringPair))
			{
				return languageStringPair.String;
			}
			foreach (Language language2 in Enum.GetValues(typeof(Language)) as Language[])
			{
				LocalizedString.LanguageStringPair languageStringPair2;
				if (this.localizedStringMap.TryGetValue(language2, out languageStringPair2))
				{
					return languageStringPair2.String;
				}
			}
			return string.Empty;
		}

		// Token: 0x0600036D RID: 877 RVA: 0x0000FFB8 File Offset: 0x0000E1B8
		public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
		{
			serializer.SerializeValue<Language>(ref this.originalLanguage, default(FastBufferWriter.ForEnums));
			if (this.localizedStrings == null)
			{
				this.localizedStrings = new List<LocalizedString.LanguageStringPair>();
			}
			int num = (serializer.IsWriter ? this.localizedStrings.Count : 0);
			serializer.SerializeValue<int>(ref num, default(FastBufferWriter.ForPrimitives));
			if (serializer.IsWriter)
			{
				for (int i = 0; i < num; i++)
				{
					LocalizedString.LanguageStringPair languageStringPair = this.localizedStrings[i];
					serializer.SerializeValue<LocalizedString.LanguageStringPair>(ref languageStringPair, default(FastBufferWriter.ForNetworkSerializable));
				}
				return;
			}
			this.localizedStrings.Clear();
			this.localizedStringMap = null;
			for (int j = 0; j < num; j++)
			{
				LocalizedString.LanguageStringPair languageStringPair2 = new LocalizedString.LanguageStringPair();
				serializer.SerializeValue<LocalizedString.LanguageStringPair>(ref languageStringPair2, default(FastBufferWriter.ForNetworkSerializable));
				this.localizedStrings.Add(languageStringPair2);
			}
		}

		// Token: 0x0600036E RID: 878 RVA: 0x00010098 File Offset: 0x0000E298
		public bool Equals(LocalizedString other)
		{
			return other != null && (this == other || (object.Equals(this.OnTextChanged, other.OnTextChanged) && object.Equals(this.localizedStrings, other.localizedStrings) && this.originalLanguage == other.originalLanguage && object.Equals(this.localizedStringMap, other.localizedStringMap)));
		}

		// Token: 0x0600036F RID: 879 RVA: 0x000100F7 File Offset: 0x0000E2F7
		public override bool Equals(object obj)
		{
			return obj != null && (this == obj || (!(obj.GetType() != base.GetType()) && this.Equals((LocalizedString)obj)));
		}

		// Token: 0x06000370 RID: 880 RVA: 0x00010125 File Offset: 0x0000E325
		public override int GetHashCode()
		{
			return HashCode.Combine<Action<Language>, List<LocalizedString.LanguageStringPair>, int, Dictionary<Language, LocalizedString.LanguageStringPair>>(this.OnTextChanged, this.localizedStrings, (int)this.originalLanguage, this.localizedStringMap);
		}

		// Token: 0x040001A7 RID: 423
		public Action<Language> OnTextChanged;

		// Token: 0x040001A8 RID: 424
		[SerializeField]
		private List<LocalizedString.LanguageStringPair> localizedStrings = new List<LocalizedString.LanguageStringPair>();

		// Token: 0x040001A9 RID: 425
		[SerializeField]
		private Language originalLanguage;

		// Token: 0x040001AA RID: 426
		private Dictionary<Language, LocalizedString.LanguageStringPair> localizedStringMap;

		// Token: 0x0200006D RID: 109
		[Serializable]
		private class LanguageStringPair : INetworkSerializable
		{
			// Token: 0x1700008F RID: 143
			// (get) Token: 0x06000371 RID: 881 RVA: 0x00010144 File Offset: 0x0000E344
			// (set) Token: 0x06000372 RID: 882 RVA: 0x00010157 File Offset: 0x0000E357
			public string String
			{
				get
				{
					return this.stringInfo.ToString();
				}
				set
				{
					this.stringInfo = value;
				}
			}

			// Token: 0x06000373 RID: 883 RVA: 0x00010168 File Offset: 0x0000E368
			public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
			{
				serializer.SerializeValue<FixedString4096Bytes>(ref this.stringInfo, default(FastBufferWriter.ForFixedStrings));
				serializer.SerializeValue<Language>(ref this.Language, default(FastBufferWriter.ForEnums));
			}

			// Token: 0x06000374 RID: 884 RVA: 0x000101A1 File Offset: 0x0000E3A1
			public override string ToString()
			{
				return string.Format("{0}: {1}, {2}: {3}", new object[] { "Language", this.Language, "String", this.String });
			}

			// Token: 0x040001AB RID: 427
			public Language Language;

			// Token: 0x040001AC RID: 428
			private FixedString4096Bytes stringInfo;
		}
	}
}
