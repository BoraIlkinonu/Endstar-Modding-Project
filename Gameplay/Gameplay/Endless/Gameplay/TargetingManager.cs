using System;
using System.Collections.Generic;
using Endless.Shared;

namespace Endless.Gameplay
{
	// Token: 0x020002C2 RID: 706
	public class TargetingManager : MonoBehaviourSingleton<TargetingManager>
	{
		// Token: 0x1700032B RID: 811
		// (get) Token: 0x06001018 RID: 4120 RVA: 0x000520E9 File Offset: 0x000502E9
		public Dictionary<WorldObject, HittableComponent> TargetableMap { get; } = new Dictionary<WorldObject, HittableComponent>();

		// Token: 0x1700032C RID: 812
		// (get) Token: 0x06001019 RID: 4121 RVA: 0x000520F1 File Offset: 0x000502F1
		public List<HittableComponent> Targetables { get; } = new List<HittableComponent>();

		// Token: 0x0600101A RID: 4122 RVA: 0x000520F9 File Offset: 0x000502F9
		public void AddTargetable(HittableComponent targetable)
		{
			this.TargetableMap.Add(targetable.WorldObject, targetable);
			this.Targetables.Add(targetable);
		}

		// Token: 0x0600101B RID: 4123 RVA: 0x00052119 File Offset: 0x00050319
		public void RemoveTargetable(HittableComponent targetable)
		{
			this.TargetableMap.Remove(targetable.WorldObject);
			this.Targetables.Remove(targetable);
		}
	}
}
