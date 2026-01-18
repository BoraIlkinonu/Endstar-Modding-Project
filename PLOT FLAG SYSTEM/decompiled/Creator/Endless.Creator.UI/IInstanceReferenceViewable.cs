using System;
using Endless.Shared.DataTypes;

namespace Endless.Creator.UI;

public interface IInstanceReferenceViewable
{
	event Action<SerializableGuid> OnInstanceEyeDropped;
}
