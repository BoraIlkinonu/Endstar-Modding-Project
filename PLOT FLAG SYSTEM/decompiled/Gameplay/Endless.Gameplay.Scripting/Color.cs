using UnityEngine;

namespace Endless.Gameplay.Scripting;

public class Color
{
	private static Color instance;

	internal static Color Instance
	{
		get
		{
			if (instance == null)
			{
				instance = new Color();
			}
			return instance;
		}
	}

	public UnityEngine.Color Red => UnityEngine.Color.red;

	public UnityEngine.Color Black => UnityEngine.Color.black;

	public UnityEngine.Color Blue => UnityEngine.Color.blue;

	public UnityEngine.Color Green => UnityEngine.Color.green;

	public UnityEngine.Color Clear => UnityEngine.Color.clear;

	public UnityEngine.Color Cyan => UnityEngine.Color.cyan;

	public UnityEngine.Color Gray => UnityEngine.Color.gray;

	public UnityEngine.Color Magenta => UnityEngine.Color.magenta;

	public UnityEngine.Color White => UnityEngine.Color.white;

	public UnityEngine.Color Yellow => UnityEngine.Color.yellow;

	public UnityEngine.Color Create(float red, float green, float blue, float alpha)
	{
		return new UnityEngine.Color(red, green, blue, alpha);
	}
}
