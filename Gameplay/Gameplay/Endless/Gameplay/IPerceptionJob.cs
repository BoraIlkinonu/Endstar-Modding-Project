using System;

namespace Endless.Gameplay
{
	// Token: 0x0200025D RID: 605
	public interface IPerceptionJob
	{
		// Token: 0x17000240 RID: 576
		// (get) Token: 0x06000C89 RID: 3209
		bool IsComplete { get; }

		// Token: 0x06000C8A RID: 3210
		void RespondToRequests();
	}
}
