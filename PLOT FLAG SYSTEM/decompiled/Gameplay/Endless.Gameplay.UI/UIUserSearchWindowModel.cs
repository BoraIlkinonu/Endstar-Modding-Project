using System;
using System.Collections.Generic;
using Endless.Shared.DataTypes;
using Endless.Shared.UI;

namespace Endless.Gameplay.UI;

public class UIUserSearchWindowModel
{
	private readonly List<User> usersToHide;

	public string WindowTitle { get; private set; }

	public IReadOnlyList<User> UsersToHide => usersToHide;

	public SelectionType SelectionType { get; private set; }

	public Action<List<object>> OnSelectionConfirmed { get; private set; }

	public UIUserSearchWindowModel(string windowTitle, List<User> usersToHide, SelectionType selectionType, Action<List<object>> onSelectionConfirmed)
	{
		WindowTitle = windowTitle;
		this.usersToHide = usersToHide;
		SelectionType = selectionType;
		OnSelectionConfirmed = onSelectionConfirmed;
	}

	public override string ToString()
	{
		return string.Format("{{ {0}: {1}, {2}: {3}, {4}: {5}, {6}: {7} }}", "WindowTitle", WindowTitle, "UsersToHide", UsersToHide.Count, "SelectionType", SelectionType, "OnSelectionConfirmed", (OnSelectionConfirmed == null) ? "null" : "not null");
	}
}
