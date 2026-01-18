using Newtonsoft.Json;

namespace Endless.Gameplay.Serialization;

public class CharacterVisualsEndlessUpdater : EndlessTypeJsonSerializer
{
	protected override JsonConverter Converter => new CharacterVisualsReferenceJsonConverter();
}
