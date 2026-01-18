using Endless.Gameplay.LevelEditing.Level;

namespace Endless.Creator.UI;

public class UIStoredParameterPackage
{
	public string Name { get; private set; }

	public StoredParameter StoredParameter { get; private set; }

	public UIStoredParameterPackage(string name, StoredParameter storedParameter)
	{
		Name = name;
		StoredParameter = storedParameter;
	}
}
