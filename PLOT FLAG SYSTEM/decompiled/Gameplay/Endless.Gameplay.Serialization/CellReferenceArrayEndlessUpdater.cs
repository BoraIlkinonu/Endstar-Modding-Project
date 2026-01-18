using Newtonsoft.Json;

namespace Endless.Gameplay.Serialization;

public class CellReferenceArrayEndlessUpdater : EndlessTypeJsonSerializer
{
	protected override JsonConverter Converter => new CellReferenceArrayJsonConverter();
}
