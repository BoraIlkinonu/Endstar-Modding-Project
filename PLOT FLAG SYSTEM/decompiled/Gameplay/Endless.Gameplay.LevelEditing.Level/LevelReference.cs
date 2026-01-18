using System;
using Endless.Assets;
using Newtonsoft.Json;

namespace Endless.Gameplay.LevelEditing.Level;

[Serializable]
public class LevelReference : AssetReference
{
	public LevelReference()
	{
		UpdateParentVersion = true;
	}

	public virtual object GetAnonymousObject()
	{
		return JsonConvert.DeserializeObject(JsonConvert.SerializeObject(this));
	}
}
