using System;
using Endless.Gameplay.LuaEnums;
using Newtonsoft.Json;

namespace Endless.Gameplay;

[Serializable]
public abstract class NpcClassCustomizationData
{
	[JsonIgnore]
	public string ClassName => NpcClass.ToString();

	[JsonProperty]
	public virtual NpcClass NpcClass { get; }
}
