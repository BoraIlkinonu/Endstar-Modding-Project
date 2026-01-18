using System.IO;
using UnityEngine;

namespace Endless.Gameplay;

public class NpcLogger
{
	private readonly StreamWriter streamWriter;

	private readonly string key;

	public NpcLogger(string key)
	{
		this.key = key;
		FileStream stream = new FileStream(Application.persistentDataPath + "/AiLog_" + key + ".txt", FileMode.Create, FileAccess.ReadWrite);
		streamWriter = new StreamWriter(stream);
		streamWriter.AutoFlush = true;
	}

	public void LogMessage(string message)
	{
		if (streamWriter.BaseStream != null)
		{
			streamWriter.WriteLine("Ai: " + key + ", " + message);
		}
	}

	public void Close()
	{
		streamWriter.Close();
	}
}
