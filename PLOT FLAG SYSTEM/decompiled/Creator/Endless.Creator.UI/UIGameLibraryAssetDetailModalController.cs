using System;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI;

public class UIGameLibraryAssetDetailModalController : UIGameObject
{
	[SerializeField]
	private UIButton editButton;

	[SerializeField]
	private UIButton duplicateButton;

	[SerializeField]
	private UIGameAssetDetailView gameAssetDetail;

	[Header("Debugging")]
	[SerializeField]
	private bool verboseLogging;

	private void Start()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Start");
		}
		editButton.onClick.AddListener(Edit);
		duplicateButton.onClick.AddListener(Duplicate);
	}

	private void Edit()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Edit");
		}
		gameAssetDetail.SetWriteable(!gameAssetDetail.Writeable);
	}

	private void Duplicate()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Duplicate");
		}
		throw new NotImplementedException();
	}
}
