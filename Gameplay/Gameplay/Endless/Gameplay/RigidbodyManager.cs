using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Gameplay
{
	// Token: 0x0200011A RID: 282
	public class RigidbodyManager : EndlessBehaviourSingleton<RigidbodyManager>
	{
		// Token: 0x0600065A RID: 1626 RVA: 0x0001CC65 File Offset: 0x0001AE65
		private void OnEnable()
		{
			Physics.simulationMode = SimulationMode.Script;
		}

		// Token: 0x0600065B RID: 1627 RVA: 0x0001F422 File Offset: 0x0001D622
		public void AddListener(UnityAction beforeCall, UnityAction afterCall)
		{
			this.BeforeSimulationEvent.AddListener(beforeCall);
			this.AfterSimulationEvent.AddListener(afterCall);
		}

		// Token: 0x0600065C RID: 1628 RVA: 0x0001F43C File Offset: 0x0001D63C
		public void RemoveListener(UnityAction beforeCall, UnityAction afterCall)
		{
			this.BeforeSimulationEvent.RemoveListener(beforeCall);
			this.AfterSimulationEvent.RemoveListener(afterCall);
		}

		// Token: 0x0600065D RID: 1629 RVA: 0x0001F456 File Offset: 0x0001D656
		public void AddOffline(OfflineRigidbodyController controller)
		{
			this.offlineControllers.Add(controller);
		}

		// Token: 0x0600065E RID: 1630 RVA: 0x0001F464 File Offset: 0x0001D664
		public void RemoveOffline(OfflineRigidbodyController controller)
		{
			this.offlineControllers.RemoveSwapBack(controller);
		}

		// Token: 0x0600065F RID: 1631 RVA: 0x0001F474 File Offset: 0x0001D674
		public void BeforeSimulation()
		{
			this.BeforeSimulationEvent.Invoke();
			foreach (OfflineRigidbodyController offlineRigidbodyController in this.offlineControllers)
			{
				offlineRigidbodyController.HandleRigidbodySimulationStart();
			}
		}

		// Token: 0x06000660 RID: 1632 RVA: 0x0001F4D0 File Offset: 0x0001D6D0
		public void AfterSimulation()
		{
			this.AfterSimulationEvent.Invoke();
			foreach (OfflineRigidbodyController offlineRigidbodyController in this.offlineControllers)
			{
				offlineRigidbodyController.HandleRigidbodySimulationEnd();
			}
		}

		// Token: 0x040004D0 RID: 1232
		private UnityEvent BeforeSimulationEvent = new UnityEvent();

		// Token: 0x040004D1 RID: 1233
		private UnityEvent AfterSimulationEvent = new UnityEvent();

		// Token: 0x040004D2 RID: 1234
		private List<OfflineRigidbodyController> offlineControllers = new List<OfflineRigidbodyController>(100);
	}
}
