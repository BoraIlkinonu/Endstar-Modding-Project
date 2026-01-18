using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Endless.Gameplay.LevelEditing.Level;

namespace Endless.Gameplay.LevelEditing.Wires;

public class WireEnumerator : IEnumerable<EventTargetEntry>, IEnumerable
{
	private IEnumerable<WiringEntry> entries;

	public WireEnumerator(IEnumerable<WiringEntry> wiringEntries)
	{
		entries = wiringEntries;
	}

	public IEnumerator<EventTargetEntry> GetEnumerator()
	{
		for (int i = 0; i < entries.Count(); i++)
		{
			WiringEntry wiringEntry = entries.ElementAt(i);
			for (int j = 0; j < wiringEntry.WiredComponents.Count; j++)
			{
				WiredComponentEntry wiredComponentEntry = wiringEntry.WiredComponents[j];
				for (int k = 0; k < wiredComponentEntry.EventEntries.Count; k++)
				{
					EventEntry eventEntry = wiredComponentEntry.EventEntries[k];
					for (int l = 0; l < eventEntry.TargetReceivers.Count; l++)
					{
						yield return eventEntry.TargetReceivers[l];
					}
				}
			}
		}
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}
