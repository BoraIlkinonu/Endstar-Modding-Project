using System;
using System.Runtime.Serialization;

namespace ELM.Endless.EndlessFramework.Serialization
{
	// Token: 0x02000035 RID: 53
	public class EndlessSurrogateSelector : SurrogateSelector, IDisposable
	{
		// Token: 0x060000EE RID: 238 RVA: 0x00005684 File Offset: 0x00003884
		public void Dispose()
		{
			if (this._serializationSurrogate != null)
			{
				this._serializationSurrogate = null;
			}
		}

		// Token: 0x060000EF RID: 239 RVA: 0x00005695 File Offset: 0x00003895
		public override ISerializationSurrogate GetSurrogate(Type type, StreamingContext context, out ISurrogateSelector selector)
		{
			if (EndlessSerializationSurrogate.DefinedTypesContains(type))
			{
				selector = this;
				return this.GetEndlessSerializationSurrogate();
			}
			return base.GetSurrogate(type, context, out selector);
		}

		// Token: 0x060000F0 RID: 240 RVA: 0x000056B4 File Offset: 0x000038B4
		private EndlessSerializationSurrogate GetEndlessSerializationSurrogate()
		{
			EndlessSerializationSurrogate endlessSerializationSurrogate;
			if ((endlessSerializationSurrogate = this._serializationSurrogate) == null)
			{
				endlessSerializationSurrogate = (this._serializationSurrogate = new EndlessSerializationSurrogate());
			}
			return endlessSerializationSurrogate;
		}

		// Token: 0x040000A5 RID: 165
		private EndlessSerializationSurrogate _serializationSurrogate;
	}
}
