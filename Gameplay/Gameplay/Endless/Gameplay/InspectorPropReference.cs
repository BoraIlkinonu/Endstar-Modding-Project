using System;

namespace Endless.Gameplay
{
	// Token: 0x020000D1 RID: 209
	[Serializable]
	public abstract class InspectorPropReference : InspectorReference
	{
		// Token: 0x170000A5 RID: 165
		// (get) Token: 0x0600042C RID: 1068
		internal abstract ReferenceFilter Filter { get; }

		// Token: 0x0600042D RID: 1069 RVA: 0x00016AC2 File Offset: 0x00014CC2
		public override string ToString()
		{
			return string.Format("{0}, {1}: {2}", base.ToString(), "Filter", this.Filter);
		}
	}
}
