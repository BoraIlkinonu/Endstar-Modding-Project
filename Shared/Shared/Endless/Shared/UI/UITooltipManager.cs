using System;
using Endless.Shared.Debugging;
using TMPro;
using UnityEngine;

namespace Endless.Shared.UI
{
	// Token: 0x02000274 RID: 628
	public class UITooltipManager : UIMonoBehaviourSingleton<UITooltipManager>
	{
		// Token: 0x06000FC7 RID: 4039 RVA: 0x0004398A File Offset: 0x00041B8A
		private void Start()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			this.displayAndHideHandler.SetToHideEnd(true);
		}

		// Token: 0x06000FC8 RID: 4040 RVA: 0x000439B0 File Offset: 0x00041BB0
		private void Update()
		{
			Vector2 vector = Input.mousePosition;
			if (this.ShouldHide(vector))
			{
				this.Hide();
				return;
			}
			vector += this.offset;
			Vector2 vector2;
			RectTransformUtility.ScreenPointToLocalPointInRectangle(this.canvas.transform as RectTransform, vector, this.canvas.worldCamera, out vector2);
			base.transform.position = this.canvas.transform.TransformPoint(vector2);
		}

		// Token: 0x06000FC9 RID: 4041 RVA: 0x00043A2C File Offset: 0x00041C2C
		public void Display(string text, RectTransform tooltipArea, UITooltip source = null)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Display", new object[]
				{
					text,
					tooltipArea.DebugSafeName(true)
				});
			}
			if (tooltipArea == null)
			{
				return;
			}
			this.tooltipArea = tooltipArea;
			this.currentTooltipSource = source;
			Vector3 mousePosition = Input.mousePosition;
			float num = (float)Screen.width;
			float num2 = (float)Screen.height;
			if (mousePosition.x < num / 2f && mousePosition.y < num2 / 2f)
			{
				base.RectTransform.SetAnchor(AnchorPresets.BottomLeft, 0f, 0f, 0f, 0f);
				base.RectTransform.SetPivot(PivotPresets.BottomLeft);
			}
			else if (mousePosition.x >= num / 2f && mousePosition.y < num2 / 2f)
			{
				base.RectTransform.SetAnchor(AnchorPresets.BottomRight, 0f, 0f, 0f, 0f);
				base.RectTransform.SetPivot(PivotPresets.BottomRight);
			}
			else if (mousePosition.x < num / 2f && mousePosition.y >= num2 / 2f)
			{
				base.RectTransform.SetAnchor(AnchorPresets.TopLeft, 0f, 0f, 0f, 0f);
				base.RectTransform.SetPivot(PivotPresets.TopLeft);
			}
			else
			{
				base.RectTransform.SetAnchor(AnchorPresets.TopRight, 0f, 0f, 0f, 0f);
				base.RectTransform.SetPivot(PivotPresets.TopRight);
			}
			this.tooltipText.text = text;
			this.displayAndHideHandler.Display();
		}

		// Token: 0x06000FCA RID: 4042 RVA: 0x00043BB6 File Offset: 0x00041DB6
		public void Hide()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Hide", Array.Empty<object>());
			}
			this.currentTooltipSource = null;
			this.displayAndHideHandler.Hide();
		}

		// Token: 0x06000FCB RID: 4043 RVA: 0x00043BE4 File Offset: 0x00041DE4
		public void UpdateText(string text, UITooltip source)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "UpdateText", new object[]
				{
					text,
					source.DebugSafeName(true)
				});
			}
			if (this.currentTooltipSource != source)
			{
				return;
			}
			this.tooltipText.text = text;
		}

		// Token: 0x06000FCC RID: 4044 RVA: 0x00043C34 File Offset: 0x00041E34
		private bool ShouldHide(Vector2 mousePosition)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "ShouldHide", new object[] { mousePosition });
			}
			return !this.displayAndHideHandler.IsTweeningHide && (this.tooltipArea == null || !this.tooltipArea.gameObject.activeInHierarchy || !RectTransformUtility.RectangleContainsScreenPoint(this.tooltipArea, mousePosition, this.canvas.worldCamera));
		}

		// Token: 0x04000A11 RID: 2577
		[SerializeField]
		private TextMeshProUGUI tooltipText;

		// Token: 0x04000A12 RID: 2578
		[SerializeField]
		private Vector2 offset = Vector2.up;

		// Token: 0x04000A13 RID: 2579
		[SerializeField]
		private Canvas canvas;

		// Token: 0x04000A14 RID: 2580
		[SerializeField]
		private UIDisplayAndHideHandler displayAndHideHandler;

		// Token: 0x04000A15 RID: 2581
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;

		// Token: 0x04000A16 RID: 2582
		private RectTransform tooltipArea;

		// Token: 0x04000A17 RID: 2583
		private UITooltip currentTooltipSource;
	}
}
