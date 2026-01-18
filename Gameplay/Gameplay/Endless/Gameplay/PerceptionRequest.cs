using System;
using System.Collections.Generic;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x02000262 RID: 610
	public struct PerceptionRequest
	{
		// Token: 0x06000CA2 RID: 3234 RVA: 0x000444D0 File Offset: 0x000426D0
		public TargeterDatum GetTargeterDatum()
		{
			return new TargeterDatum
			{
				Position = this.Position,
				LookVector = this.LookVector,
				ProximityDistance = this.ProximityDistance,
				MaxDistance = this.MaxDistance,
				VerticalViewAngle = this.VerticalValue,
				HorizontalViewAngle = this.HorizontalValue,
				UseXray = this.UseXray
			};
		}

		// Token: 0x04000BA5 RID: 2981
		public bool IsBoxcast;

		// Token: 0x04000BA6 RID: 2982
		public Vector3 Position;

		// Token: 0x04000BA7 RID: 2983
		public Vector3 LookVector;

		// Token: 0x04000BA8 RID: 2984
		public float ProximityDistance;

		// Token: 0x04000BA9 RID: 2985
		public float MaxDistance;

		// Token: 0x04000BAA RID: 2986
		public float VerticalValue;

		// Token: 0x04000BAB RID: 2987
		public float HorizontalValue;

		// Token: 0x04000BAC RID: 2988
		public bool UseXray;

		// Token: 0x04000BAD RID: 2989
		public List<PerceptionResult> PerceptionResults;

		// Token: 0x04000BAE RID: 2990
		public Action PerceptionUpdatedCallback;
	}
}
