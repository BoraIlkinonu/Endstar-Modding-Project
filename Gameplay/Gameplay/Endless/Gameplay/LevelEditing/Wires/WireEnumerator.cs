using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Endless.Gameplay.LevelEditing.Level;

namespace Endless.Gameplay.LevelEditing.Wires
{
	// Token: 0x02000502 RID: 1282
	public class WireEnumerator : IEnumerable<EventTargetEntry>, IEnumerable
	{
		// Token: 0x06001F26 RID: 7974 RVA: 0x00089EF9 File Offset: 0x000880F9
		public WireEnumerator(IEnumerable<WiringEntry> wiringEntries)
		{
			this.entries = wiringEntries;
		}

		// Token: 0x06001F27 RID: 7975 RVA: 0x00089F08 File Offset: 0x00088108
		public IEnumerator<EventTargetEntry> GetEnumerator()
		{
			int num;
			for (int i = 0; i < this.entries.Count<WiringEntry>(); i = num + 1)
			{
				WiringEntry wiringEntry = this.entries.ElementAt(i);
				for (int j = 0; j < wiringEntry.WiredComponents.Count; j = num + 1)
				{
					WiredComponentEntry wiredComponentEntry = wiringEntry.WiredComponents[j];
					for (int k = 0; k < wiredComponentEntry.EventEntries.Count; k = num + 1)
					{
						EventEntry eventEntry = wiredComponentEntry.EventEntries[k];
						for (int l = 0; l < eventEntry.TargetReceivers.Count; l = num + 1)
						{
							yield return eventEntry.TargetReceivers[l];
							num = l;
						}
						eventEntry = null;
						num = k;
					}
					wiredComponentEntry = null;
					num = j;
				}
				wiringEntry = null;
				num = i;
			}
			yield break;
		}

		// Token: 0x06001F28 RID: 7976 RVA: 0x00089F17 File Offset: 0x00088117
		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}

		// Token: 0x04001874 RID: 6260
		private IEnumerable<WiringEntry> entries;
	}
}
