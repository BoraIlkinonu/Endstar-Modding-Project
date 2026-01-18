using System;
using Endless.Gameplay.Scripting;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x02000345 RID: 837
	public class WireTesting : EndlessBehaviour, IStartSubscriber, IBaseType, IComponentBase
	{
		// Token: 0x0600147C RID: 5244 RVA: 0x000624B3 File Offset: 0x000606B3
		public void EndlessStart()
		{
			this.BasicEvent.Invoke(this.Context);
			this.IntEvent.Invoke(this.Context, 5);
			this.Int2Event.Invoke(this.Context, 6, 7);
		}

		// Token: 0x0600147D RID: 5245 RVA: 0x000624EB File Offset: 0x000606EB
		public void ReceiverBasic(Context context)
		{
			Debug.Log("ReceiverBasic called.");
		}

		// Token: 0x0600147E RID: 5246 RVA: 0x000624F7 File Offset: 0x000606F7
		public void ReceiverInt1(Context context, int value)
		{
			Debug.Log(string.Format("{0} received a value of {1}", "ReceiverInt1", value));
		}

		// Token: 0x0600147F RID: 5247 RVA: 0x00062513 File Offset: 0x00060713
		public void ReceiverInt2(Context context, int value1, int value2)
		{
			Debug.Log(string.Format("{0} received a value of {1} and {2}", "ReceiverInt2", value1, value2));
		}

		// Token: 0x17000433 RID: 1075
		// (get) Token: 0x06001480 RID: 5248 RVA: 0x00062535 File Offset: 0x00060735
		// (set) Token: 0x06001481 RID: 5249 RVA: 0x0006253D File Offset: 0x0006073D
		public WorldObject WorldObject { get; private set; }

		// Token: 0x17000434 RID: 1076
		// (get) Token: 0x06001482 RID: 5250 RVA: 0x00062548 File Offset: 0x00060748
		public Context Context
		{
			get
			{
				Context context;
				if ((context = this.context) == null)
				{
					context = (this.context = new Context(this.WorldObject));
				}
				return context;
			}
		}

		// Token: 0x06001483 RID: 5251 RVA: 0x00062573 File Offset: 0x00060773
		public void PrefabInitialize(WorldObject worldObject)
		{
			this.WorldObject = worldObject;
		}

		// Token: 0x040010FF RID: 4351
		public EndlessEvent BasicEvent = new EndlessEvent();

		// Token: 0x04001100 RID: 4352
		public EndlessEvent<int> IntEvent = new EndlessEvent<int>();

		// Token: 0x04001101 RID: 4353
		public EndlessEvent<int, int> Int2Event = new EndlessEvent<int, int>();

		// Token: 0x04001102 RID: 4354
		private Context context;
	}
}
