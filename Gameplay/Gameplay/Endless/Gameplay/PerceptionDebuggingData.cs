using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Endless.Gameplay
{
	// Token: 0x02000156 RID: 342
	public readonly struct PerceptionDebuggingData
	{
		// Token: 0x06000809 RID: 2057 RVA: 0x00025E04 File Offset: 0x00024004
		public PerceptionDebuggingData(List<PerceptionResult> perception, Dictionary<HittableComponent, float> awarenessValues, [TupleElementNames(new string[] { "score", "hittableComponent" })] List<ValueTuple<float, HittableComponent>> scores)
		{
			this.perceptionResults = perception;
			this.currentAwarenessValues = awarenessValues;
			this.finalScores = scores;
		}

		// Token: 0x0600080A RID: 2058 RVA: 0x00025E1C File Offset: 0x0002401C
		public Dictionary<HittableComponent, PerceptionDebuggingDatum> GetFilteredData()
		{
			Dictionary<HittableComponent, PerceptionDebuggingDatum> dictionary = new Dictionary<HittableComponent, PerceptionDebuggingDatum>();
			using (Dictionary<HittableComponent, float>.KeyCollection.Enumerator enumerator = this.currentAwarenessValues.Keys.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					HittableComponent hittableComponent = enumerator.Current;
					float num = -1f;
					float num2 = -1f;
					float num3 = this.currentAwarenessValues[hittableComponent];
					IEnumerable<PerceptionResult> enumerable = this.perceptionResults;
					Func<PerceptionResult, bool> func;
					Func<PerceptionResult, bool> <>9__0;
					if ((func = <>9__0) == null)
					{
						func = (<>9__0 = (PerceptionResult result) => result.HittableComponent == hittableComponent);
					}
					foreach (PerceptionResult perceptionResult in enumerable.Where(func))
					{
						num = perceptionResult.Awareness;
					}
					IEnumerable<ValueTuple<float, HittableComponent>> enumerable2 = this.finalScores;
					Func<ValueTuple<float, HittableComponent>, bool> func2;
					Func<ValueTuple<float, HittableComponent>, bool> <>9__1;
					if ((func2 = <>9__1) == null)
					{
						func2 = (<>9__1 = ([TupleElementNames(new string[] { "score", "hittableComponent" })] ValueTuple<float, HittableComponent> score) => score.Item2 == hittableComponent);
					}
					foreach (ValueTuple<float, HittableComponent> valueTuple in enumerable2.Where(func2))
					{
						num2 = valueTuple.Item1;
					}
					dictionary.Add(hittableComponent, new PerceptionDebuggingDatum
					{
						AwarenessThisFrame = num,
						CurrentAwareness = num3,
						FinalScore = num2
					});
				}
			}
			return dictionary;
		}

		// Token: 0x0400066A RID: 1642
		private readonly List<PerceptionResult> perceptionResults;

		// Token: 0x0400066B RID: 1643
		private readonly Dictionary<HittableComponent, float> currentAwarenessValues;

		// Token: 0x0400066C RID: 1644
		[TupleElementNames(new string[] { "score", "hittableComponent" })]
		private readonly List<ValueTuple<float, HittableComponent>> finalScores;
	}
}
