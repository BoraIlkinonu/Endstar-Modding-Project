using System;
using System.Collections.Generic;
using UnityEngine;

namespace Endless.Gameplay.LevelEditing.Level
{
	// Token: 0x02000551 RID: 1361
	[SerializeField]
	public class EventEntry
	{
		// Token: 0x04001A21 RID: 6689
		public string EmitterMemberName;

		// Token: 0x04001A22 RID: 6690
		public List<EventTargetEntry> TargetReceivers = new List<EventTargetEntry>();
	}
}
