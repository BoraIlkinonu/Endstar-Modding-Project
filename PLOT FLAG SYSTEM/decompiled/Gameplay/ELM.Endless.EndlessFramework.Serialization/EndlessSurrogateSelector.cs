using System;
using System.Runtime.Serialization;

namespace ELM.Endless.EndlessFramework.Serialization;

public class EndlessSurrogateSelector : SurrogateSelector, IDisposable
{
	private EndlessSerializationSurrogate _serializationSurrogate;

	public void Dispose()
	{
		if (_serializationSurrogate != null)
		{
			_serializationSurrogate = null;
		}
	}

	public override ISerializationSurrogate GetSurrogate(Type type, StreamingContext context, out ISurrogateSelector selector)
	{
		if (EndlessSerializationSurrogate.DefinedTypesContains(type))
		{
			selector = this;
			return GetEndlessSerializationSurrogate();
		}
		return base.GetSurrogate(type, context, out selector);
	}

	private EndlessSerializationSurrogate GetEndlessSerializationSurrogate()
	{
		return _serializationSurrogate ?? (_serializationSurrogate = new EndlessSerializationSurrogate());
	}
}
