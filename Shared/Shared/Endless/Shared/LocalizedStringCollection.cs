using System;
using Unity.Netcode;

namespace Endless.Shared
{
	// Token: 0x0200006E RID: 110
	[Serializable]
	public struct LocalizedStringCollection : INetworkSerializable
	{
		// Token: 0x17000090 RID: 144
		// (get) Token: 0x06000376 RID: 886 RVA: 0x000101DA File Offset: 0x0000E3DA
		public int Length
		{
			get
			{
				LocalizedString[] array = this.stringPairs;
				if (array == null)
				{
					return 0;
				}
				return array.Length;
			}
		}

		// Token: 0x06000377 RID: 887 RVA: 0x000101EA File Offset: 0x0000E3EA
		public static implicit operator LocalizedString[](LocalizedStringCollection collection)
		{
			return collection.stringPairs;
		}

		// Token: 0x06000378 RID: 888 RVA: 0x000101F4 File Offset: 0x0000E3F4
		public static implicit operator LocalizedStringCollection(LocalizedString[] localizedStrings)
		{
			return new LocalizedStringCollection
			{
				stringPairs = localizedStrings
			};
		}

		// Token: 0x06000379 RID: 889 RVA: 0x00010214 File Offset: 0x0000E414
		public static string[] GetStrings(LocalizedString[] localizedStrings, Language language)
		{
			string[] array = new string[localizedStrings.Length];
			for (int i = 0; i < localizedStrings.Length; i++)
			{
				array[i] = localizedStrings[i].GetString(language);
			}
			return array;
		}

		// Token: 0x0600037A RID: 890 RVA: 0x00010248 File Offset: 0x0000E448
		public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
		{
			int length = this.Length;
			serializer.SerializeValue<int>(ref length, default(FastBufferWriter.ForPrimitives));
			if (serializer.IsReader)
			{
				this.stringPairs = new LocalizedString[length];
				for (int i = 0; i < this.stringPairs.Length; i++)
				{
					this.stringPairs[i] = new LocalizedString();
				}
			}
			for (int j = 0; j < length; j++)
			{
				serializer.SerializeValue<LocalizedString>(ref this.stringPairs[j], default(FastBufferWriter.ForNetworkSerializable));
			}
		}

		// Token: 0x040001AD RID: 429
		public LocalizedString[] stringPairs;
	}
}
