using System;
using System.CodeDom.Compiler;

namespace Unity.Cloud.UserReporting.Plugin.SimpleJson
{
	// Token: 0x0200002B RID: 43
	[GeneratedCode("simple-json", "1.0.0")]
	public interface IJsonSerializerStrategy
	{
		// Token: 0x06000155 RID: 341
		bool TrySerializeNonPrimitiveObject(object input, out object output);

		// Token: 0x06000156 RID: 342
		object DeserializeObject(object value, Type type);
	}
}
