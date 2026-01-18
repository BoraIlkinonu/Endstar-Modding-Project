using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;

namespace Endless
{
	// Token: 0x02000006 RID: 6
	public static class ConfigReader
	{
		// Token: 0x06000013 RID: 19 RVA: 0x00003688 File Offset: 0x00001888
		public static bool ReadFromTextFile(string filePath, Type configType)
		{
			DirectoryInfo directoryInfo = new DirectoryInfo(Directory.GetCurrentDirectory());
			string text = ((directoryInfo != null) ? directoryInfo.FullName : null) ?? "";
			filePath = Path.Combine(text, filePath) + ".txt";
			bool flag = !File.Exists(filePath);
			bool flag2;
			if (flag)
			{
				Logger.Log(null, filePath + " file could not be found!", true);
				flag2 = false;
			}
			else
			{
				string text2 = File.ReadAllText(filePath);
				CultureInfo invariantCulture = CultureInfo.InvariantCulture;
				Dictionary<string, string> dictionary = ConfigReader.ReadNameValuePairs(text2);
				foreach (KeyValuePair<string, string> keyValuePair in dictionary)
				{
					PropertyInfo property = configType.GetProperty(keyValuePair.Key.Trim(), BindingFlags.Static | BindingFlags.Public);
					bool flag3 = property == null || property.SetMethod == null;
					if (flag3)
					{
						Logger.Log(null, string.Concat(new string[] { "No property for ", keyValuePair.Key, " = ", keyValuePair.Value, " in ", configType.Name }), true);
					}
					else
					{
						string text3 = keyValuePair.Value;
						object obj = null;
						try
						{
							bool flag4 = property.PropertyType == typeof(int);
							if (flag4)
							{
								obj = Convert.ToInt32(text3, invariantCulture);
							}
							bool flag5 = property.PropertyType == typeof(int?);
							if (flag5)
							{
								obj = Convert.ToInt32(text3, invariantCulture);
							}
							bool flag6 = property.PropertyType == typeof(float);
							if (flag6)
							{
								obj = (float)Convert.ToDouble(text3, invariantCulture);
							}
							bool flag7 = property.PropertyType == typeof(bool);
							if (flag7)
							{
								bool flag8 = text3 == "1";
								if (flag8)
								{
									text3 = "true";
								}
								bool flag9 = text3 == "0";
								if (flag9)
								{
									text3 = "false";
								}
								obj = Convert.ToBoolean(text3, invariantCulture);
							}
							bool flag10 = property.PropertyType == typeof(string);
							if (flag10)
							{
								obj = Convert.ToString(text3);
							}
							bool isEnum = property.PropertyType.IsEnum;
							if (isEnum)
							{
								obj = Enum.Parse(property.PropertyType, text3);
							}
							bool flag11 = property.PropertyType == typeof(int[]);
							if (flag11)
							{
								string[] array = text3.Trim().Split(',', StringSplitOptions.None);
								List<int> list = new List<int>();
								foreach (string text4 in array)
								{
									int num = Convert.ToInt32(text4.Trim());
									list.Add(num);
								}
								obj = list.ToArray();
							}
							bool flag12 = property.PropertyType == typeof(float[]);
							if (flag12)
							{
								string[] array3 = text3.Trim().Split(',', StringSplitOptions.None);
								List<float> list2 = new List<float>();
								foreach (string text5 in array3)
								{
									float num2 = Convert.ToSingle(text5.Trim());
									list2.Add(num2);
								}
								obj = list2.ToArray();
							}
							bool flag13 = property.PropertyType == typeof(bool[]);
							if (flag13)
							{
								string[] array5 = text3.Trim().Split(',', StringSplitOptions.None);
								List<bool> list3 = new List<bool>();
								foreach (string text6 in array5)
								{
									bool flag14 = Convert.ToBoolean(text6.Trim());
									list3.Add(flag14);
								}
								obj = list3.ToArray();
							}
						}
						catch (Exception ex)
						{
							Logger.Log(null, ex.ToString(), true);
							return false;
						}
						bool flag15 = obj == null;
						if (!flag15)
						{
							property.SetValue(null, obj);
						}
					}
				}
				flag2 = true;
			}
			return flag2;
		}

		// Token: 0x06000014 RID: 20 RVA: 0x00003ADC File Offset: 0x00001CDC
		private static Dictionary<string, string> ReadNameValuePairs(string _fileContents)
		{
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			string[] array = _fileContents.Split(new char[] { '\n', '\r' });
			foreach (string text in array)
			{
				string text2 = text.Trim();
				bool flag = text2.StartsWith("#") || text2.StartsWith("//");
				if (!flag)
				{
					string[] array3 = text2.Split('=', StringSplitOptions.None);
					bool flag2 = array3.Length != 2;
					if (!flag2)
					{
						bool flag3 = string.IsNullOrEmpty(array3[0]) || string.IsNullOrEmpty(array3[1]);
						if (!flag3)
						{
							dictionary.Add(array3[0].Trim(), array3[1].Trim());
						}
					}
				}
			}
			return dictionary;
		}
	}
}
