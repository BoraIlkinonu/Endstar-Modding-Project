using System;
using System.Collections.Generic;
using Endless.Gameplay.UI;
using Endless.Shared;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x02000155 RID: 341
	public class TargetingDebugger : MonoBehaviourSingleton<TargetingDebugger>
	{
		// Token: 0x17000180 RID: 384
		// (get) Token: 0x06000804 RID: 2052 RVA: 0x00025B54 File Offset: 0x00023D54
		// (set) Token: 0x06000805 RID: 2053 RVA: 0x00025B5C File Offset: 0x00023D5C
		public TargeterComponent TargeterComponent
		{
			get
			{
				return this.targeterComponent;
			}
			private set
			{
				if (this.targeterComponent == value)
				{
					return;
				}
				if (this.targeterComponent)
				{
					this.targeterText.Close();
					this.targeterComponent.OnScoresUpdated -= this.TargeterComponentOnScoresUpdated;
				}
				foreach (UISimpleText uisimpleText in this.targetTexts.Values)
				{
					uisimpleText.Close();
				}
				this.targetTexts.Clear();
				this.targeterComponent = value;
				if (this.targeterComponent != null)
				{
					string text = (this.targeterComponent.Target ? this.TargeterComponent.Target.WorldObject.gameObject.name : "");
					this.targeterText = UISimpleText.CreateInstance(this.simpleTextSource, this.targeterComponent.transform, MonoBehaviourSingleton<UIGameplayReferenceManager>.Instance.AnchorContainer, text, new Vector3?(this.targeterOffset));
					this.targeterComponent.OnScoresUpdated += this.TargeterComponentOnScoresUpdated;
				}
			}
		}

		// Token: 0x06000806 RID: 2054 RVA: 0x00025C8C File Offset: 0x00023E8C
		private void TargeterComponentOnScoresUpdated(PerceptionDebuggingData data)
		{
			string text = (this.targeterComponent.Target ? this.TargeterComponent.Target.WorldObject.gameObject.name : "");
			this.targeterText.UpdateText(text);
			Dictionary<HittableComponent, PerceptionDebuggingDatum> filteredData = data.GetFilteredData();
			foreach (HittableComponent hittableComponent in filteredData.Keys)
			{
				PerceptionDebuggingDatum perceptionDebuggingDatum = filteredData[hittableComponent];
				string text2 = string.Format("Awareness this frame: {0}", perceptionDebuggingDatum.AwarenessThisFrame) + Environment.NewLine;
				text2 = text2 + string.Format("Current Awareness: {0}", perceptionDebuggingDatum.CurrentAwareness) + Environment.NewLine;
				text2 += string.Format("Final Score: {0}", perceptionDebuggingDatum.FinalScore);
				UISimpleText uisimpleText;
				if (this.targetTexts.TryGetValue(hittableComponent, out uisimpleText))
				{
					uisimpleText.UpdateText(text2);
				}
				else
				{
					this.targetTexts.Add(hittableComponent, UISimpleText.CreateInstance(this.simpleTextSource, hittableComponent.WorldObject.transform, MonoBehaviourSingleton<UIGameplayReferenceManager>.Instance.AnchorContainer, text2, new Vector3?(this.targeterOffset)));
				}
			}
		}

		// Token: 0x06000807 RID: 2055 RVA: 0x00025DE8 File Offset: 0x00023FE8
		public void DebugTargeting(TargeterComponent targeter)
		{
			this.TargeterComponent = targeter;
		}

		// Token: 0x04000665 RID: 1637
		[SerializeField]
		private Vector3 targeterOffset;

		// Token: 0x04000666 RID: 1638
		[SerializeField]
		private UISimpleText simpleTextSource;

		// Token: 0x04000667 RID: 1639
		private UISimpleText targeterText;

		// Token: 0x04000668 RID: 1640
		private readonly Dictionary<HittableComponent, UISimpleText> targetTexts = new Dictionary<HittableComponent, UISimpleText>();

		// Token: 0x04000669 RID: 1641
		private TargeterComponent targeterComponent;
	}
}
