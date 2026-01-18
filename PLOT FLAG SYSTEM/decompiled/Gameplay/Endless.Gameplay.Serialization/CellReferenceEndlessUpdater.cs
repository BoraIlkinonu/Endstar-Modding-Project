using Newtonsoft.Json;

namespace Endless.Gameplay.Serialization;

public class CellReferenceEndlessUpdater : EndlessTypeJsonSerializer
{
	protected override JsonConverter Converter => new CellReferenceJsonConverter();
}
