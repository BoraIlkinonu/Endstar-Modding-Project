using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Endless.Shared;
using Endless.Shared.EndlessQualitySettings;
using Endless.Shared.FileManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Endless.Core.Test
{
	// Token: 0x020000D8 RID: 216
	public class FpsTestManager : MonoBehaviour
	{
		// Token: 0x17000098 RID: 152
		// (get) Token: 0x060004DE RID: 1246 RVA: 0x00017A7C File Offset: 0x00015C7C
		public string[] SceneLabels
		{
			get
			{
				return this.results.Keys.ToArray<string>();
			}
		}

		// Token: 0x060004DF RID: 1247 RVA: 0x00017A90 File Offset: 0x00015C90
		private void Start()
		{
			this.sceneLabel = (string.IsNullOrWhiteSpace(this.sceneLabel) ? SceneManager.GetActiveScene().name : this.sceneLabel);
			this.results.Add(this.sceneLabel, new List<FpsInfo>());
			if (this.executeTestsOnStart)
			{
				this.ExecuteTests();
			}
		}

		// Token: 0x060004E0 RID: 1248 RVA: 0x00017AE9 File Offset: 0x00015CE9
		public FpsInfo[] GetFpsInfo(string sceneLabel)
		{
			return this.results[sceneLabel].ToArray();
		}

		// Token: 0x060004E1 RID: 1249 RVA: 0x00017AFC File Offset: 0x00015CFC
		public void ExecuteTests()
		{
			base.StartCoroutine(this.RunTests());
		}

		// Token: 0x060004E2 RID: 1250 RVA: 0x00017B0B File Offset: 0x00015D0B
		private IEnumerator RunTests()
		{
			if (this.clearCacheOnTest)
			{
				MonoBehaviourSingleton<LocalFileDatabase>.Instance.ClearDatabase();
			}
			if (this.forcePreset)
			{
				this.qualityMenu.SwitchQualityPreset(this.presetToUse.DisplayName, true);
			}
			int num;
			for (int testIndex = 0; testIndex < this.tests.Length; testIndex = num + 1)
			{
				BaseFpsInfo test = this.tests[testIndex];
				test.StartTest();
				while (!test.IsDone)
				{
					test.ProcessFrame();
					yield return null;
				}
				test.StopTest();
				if (test.FpsInfo.TestType != FpsTestType.Display)
				{
					this.results[this.sceneLabel].Add(test.FpsInfo);
				}
				Debug.Log(string.Format("FpsTesting: Completed test: {0}. records count: {1}", test.FpsInfo.SectionName, this.results[this.sceneLabel].Count));
				yield return null;
				test = null;
				num = testIndex;
			}
			if (this.showResultsOnCompletion)
			{
				this.display.Display();
			}
			if (this.destroyOnCompletion)
			{
				global::UnityEngine.Object.Destroy(base.gameObject);
			}
			yield break;
		}

		// Token: 0x060004E3 RID: 1251 RVA: 0x00017B1C File Offset: 0x00015D1C
		public static void NextScene()
		{
			int buildIndex = SceneManager.GetActiveScene().buildIndex;
			if (buildIndex < SceneManager.sceneCountInBuildSettings - 1)
			{
				SceneManager.LoadScene(buildIndex + 1);
			}
		}

		// Token: 0x060004E4 RID: 1252 RVA: 0x00017B4C File Offset: 0x00015D4C
		public void ClearData()
		{
			int count = this.results[this.sceneLabel].Count;
			this.results[this.sceneLabel].Clear();
			Debug.Log(string.Format("FpsTesting: Cleared results. {0}->{1}", count, this.results[this.sceneLabel].Count));
		}

		// Token: 0x04000340 RID: 832
		[SerializeField]
		private string sceneLabel = "";

		// Token: 0x04000341 RID: 833
		[SerializeField]
		private BaseFpsInfo[] tests;

		// Token: 0x04000342 RID: 834
		[SerializeField]
		private FpsTestResultsDisplay display;

		// Token: 0x04000343 RID: 835
		[SerializeField]
		private bool executeTestsOnStart;

		// Token: 0x04000344 RID: 836
		[SerializeField]
		private bool clearCacheOnTest;

		// Token: 0x04000345 RID: 837
		[SerializeField]
		private bool showResultsOnCompletion = true;

		// Token: 0x04000346 RID: 838
		[SerializeField]
		private bool destroyOnCompletion;

		// Token: 0x04000347 RID: 839
		[SerializeField]
		private bool forcePreset;

		// Token: 0x04000348 RID: 840
		[SerializeField]
		private QualityMenu qualityMenu;

		// Token: 0x04000349 RID: 841
		[SerializeField]
		private QualityPreset presetToUse;

		// Token: 0x0400034A RID: 842
		private Dictionary<string, List<FpsInfo>> results = new Dictionary<string, List<FpsInfo>>();
	}
}
