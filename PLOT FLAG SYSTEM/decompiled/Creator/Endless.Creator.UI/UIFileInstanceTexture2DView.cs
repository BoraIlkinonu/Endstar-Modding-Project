using System;
using Endless.FileManagement;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Endless.Creator.UI;

public class UIFileInstanceTexture2DView : UIGameObject, IUILoadingSpinnerViewCompatible
{
	private enum States
	{
		Empty,
		Loading,
		Loaded
	}

	[SerializeField]
	private RawImage rawImage;

	[Header("Debugging")]
	[SerializeField]
	private bool verboseLogging;

	private States state;

	public int FileInstanceId { get; private set; } = -1;

	public UnityEvent OnLoadingStarted { get; } = new UnityEvent();

	public UnityEvent OnLoadingEnded { get; } = new UnityEvent();

	public void View(Sprite sprite)
	{
		if (verboseLogging)
		{
			DebugUtility.Log("View ( sprite: " + sprite.DebugSafeName() + " )", this);
		}
		States states = state;
		if (states == States.Loading || states == States.Loaded)
		{
			Clear();
		}
		state = States.Loaded;
		if (sprite != null)
		{
			rawImage.texture = sprite.texture;
			rawImage.enabled = true;
		}
	}

	public void View(int fileInstanceId)
	{
		if (verboseLogging)
		{
			DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "View", "fileInstanceId", fileInstanceId), this);
		}
		States states = state;
		if (states == States.Loading || states == States.Loaded)
		{
			Clear();
		}
		FileInstanceId = fileInstanceId;
		if (FileInstanceId <= 0)
		{
			return;
		}
		state = States.Loading;
		OnLoadingStarted.Invoke();
		try
		{
			MonoBehaviourSingleton<LoadedFileManager>.Instance.GetTexture2D(this, fileInstanceId, "png", OnGetTexture2dCompleted);
		}
		catch (Exception exception)
		{
			Clear();
			DebugUtility.LogException(exception, this);
		}
	}

	public void Clear()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethodWithAppension(this, "Clear", state.ToString());
		}
		OnLoadingEnded.Invoke();
		rawImage.enabled = false;
		rawImage.texture = null;
		if (state != States.Empty)
		{
			MonoBehaviourSingleton<LoadedFileManager>.Instance.ReleaseAccess(this, FileInstanceId);
			state = States.Empty;
		}
		FileInstanceId = -1;
	}

	private void OnGetTexture2dCompleted(int fileInstanceId, Texture2D texture2D)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "OnGetTexture2dCompleted", fileInstanceId, texture2D.DebugSafeName());
		}
		OnLoadingEnded.Invoke();
		rawImage.texture = texture2D;
		rawImage.enabled = true;
	}
}
