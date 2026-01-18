using System.Collections.Generic;
using UnityEngine;

namespace Endless.Gameplay.LevelEditing.Level;

[SerializeField]
public class EventEntry
{
	public string EmitterMemberName;

	public List<EventTargetEntry> TargetReceivers = new List<EventTargetEntry>();
}
