using System;
using UnityEngine;

namespace Endless.Creator.UI;

[Serializable]
public struct UIRolesDescription
{
	[TextArea]
	public string Game;

	[TextArea]
	public string Level;
}
