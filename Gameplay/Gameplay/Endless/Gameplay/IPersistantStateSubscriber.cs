using System;

namespace Endless.Gameplay
{
	// Token: 0x02000099 RID: 153
	public interface IPersistantStateSubscriber
	{
		// Token: 0x17000081 RID: 129
		// (get) Token: 0x060002B3 RID: 691
		bool ShouldSaveAndLoad { get; }

		// Token: 0x060002B4 RID: 692
		object GetSaveState();

		// Token: 0x060002B5 RID: 693
		void LoadState(object loadedState);
	}
}
