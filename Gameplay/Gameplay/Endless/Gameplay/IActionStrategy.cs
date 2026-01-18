using System;

namespace Endless.Gameplay
{
	// Token: 0x0200017C RID: 380
	public interface IActionStrategy
	{
		// Token: 0x1700019F RID: 415
		// (get) Token: 0x06000892 RID: 2194
		Func<float> GetCost { get; }

		// Token: 0x170001A0 RID: 416
		// (get) Token: 0x06000893 RID: 2195
		GoapAction.Status Status { get; }

		// Token: 0x06000894 RID: 2196 RVA: 0x00002DB0 File Offset: 0x00000FB0
		void Start()
		{
		}

		// Token: 0x06000895 RID: 2197 RVA: 0x00002DB0 File Offset: 0x00000FB0
		void Update(float deltaTime)
		{
		}

		// Token: 0x06000896 RID: 2198 RVA: 0x00002DB0 File Offset: 0x00000FB0
		void Tick(uint frame)
		{
		}

		// Token: 0x06000897 RID: 2199 RVA: 0x00002DB0 File Offset: 0x00000FB0
		void Stop()
		{
		}
	}
}
