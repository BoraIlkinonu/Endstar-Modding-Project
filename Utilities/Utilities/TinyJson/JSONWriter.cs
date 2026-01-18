using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;

namespace TinyJson
{
	// Token: 0x02000004 RID: 4
	public static class JSONWriter
	{
		// Token: 0x0600000A RID: 10 RVA: 0x00002D9C File Offset: 0x00000F9C
		public static string ToJson(this object item)
		{
			StringBuilder stringBuilder = new StringBuilder();
			JSONWriter.AppendValue(stringBuilder, item);
			return stringBuilder.ToString();
		}

		// Token: 0x0600000B RID: 11 RVA: 0x00002DC4 File Offset: 0x00000FC4
		private static void AppendValue(StringBuilder stringBuilder, object item)
		{
			bool flag = item == null;
			if (flag)
			{
				stringBuilder.Append("null");
			}
			else
			{
				Type type = item.GetType();
				bool flag2 = type == typeof(string) || type == typeof(char);
				if (flag2)
				{
					stringBuilder.Append('"');
					string text = item.ToString();
					for (int i = 0; i < text.Length; i++)
					{
						bool flag3 = text[i] < ' ' || text[i] == '"' || text[i] == '\\';
						if (flag3)
						{
							stringBuilder.Append('\\');
							int num = "\"\\\n\r\t\b\f".IndexOf(text[i]);
							bool flag4 = num >= 0;
							if (flag4)
							{
								stringBuilder.Append("\"\\nrtbf"[num]);
							}
							else
							{
								stringBuilder.AppendFormat("u{0:X4}", (uint)text[i]);
							}
						}
						else
						{
							stringBuilder.Append(text[i]);
						}
					}
					stringBuilder.Append('"');
				}
				else
				{
					bool flag5 = type == typeof(byte) || type == typeof(sbyte);
					if (flag5)
					{
						stringBuilder.Append(item.ToString());
					}
					else
					{
						bool flag6 = type == typeof(short) || type == typeof(ushort);
						if (flag6)
						{
							stringBuilder.Append(item.ToString());
						}
						else
						{
							bool flag7 = type == typeof(int) || type == typeof(uint);
							if (flag7)
							{
								stringBuilder.Append(item.ToString());
							}
							else
							{
								bool flag8 = type == typeof(long) || type == typeof(ulong);
								if (flag8)
								{
									stringBuilder.Append(item.ToString());
								}
								else
								{
									bool flag9 = type == typeof(float);
									if (flag9)
									{
										stringBuilder.Append(((float)item).ToString(CultureInfo.InvariantCulture));
									}
									else
									{
										bool flag10 = type == typeof(double);
										if (flag10)
										{
											stringBuilder.Append(((double)item).ToString(CultureInfo.InvariantCulture));
										}
										else
										{
											bool flag11 = type == typeof(decimal);
											if (flag11)
											{
												stringBuilder.Append(((decimal)item).ToString(CultureInfo.InvariantCulture));
											}
											else
											{
												bool flag12 = type == typeof(bool);
												if (flag12)
												{
													stringBuilder.Append(((bool)item) ? "true" : "false");
												}
												else
												{
													bool flag13 = type == typeof(DateTime);
													if (flag13)
													{
														stringBuilder.Append('"');
														stringBuilder.Append(((DateTime)item).ToString(CultureInfo.InvariantCulture));
														stringBuilder.Append('"');
													}
													else
													{
														bool isEnum = type.IsEnum;
														if (isEnum)
														{
															stringBuilder.Append('"');
															stringBuilder.Append(item.ToString());
															stringBuilder.Append('"');
														}
														else
														{
															bool flag14 = item is IList;
															if (flag14)
															{
																stringBuilder.Append('[');
																bool flag15 = true;
																IList list = item as IList;
																for (int j = 0; j < list.Count; j++)
																{
																	bool flag16 = flag15;
																	if (flag16)
																	{
																		flag15 = false;
																	}
																	else
																	{
																		stringBuilder.Append(',');
																	}
																	JSONWriter.AppendValue(stringBuilder, list[j]);
																}
																stringBuilder.Append(']');
															}
															else
															{
																bool flag17 = type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<, >);
																if (flag17)
																{
																	Type type2 = type.GetGenericArguments()[0];
																	bool flag18 = type2 != typeof(string);
																	if (flag18)
																	{
																		stringBuilder.Append("{}");
																	}
																	else
																	{
																		stringBuilder.Append('{');
																		IDictionary dictionary = item as IDictionary;
																		bool flag19 = true;
																		foreach (object obj in dictionary.Keys)
																		{
																			bool flag20 = flag19;
																			if (flag20)
																			{
																				flag19 = false;
																			}
																			else
																			{
																				stringBuilder.Append(',');
																			}
																			stringBuilder.Append('"');
																			stringBuilder.Append((string)obj);
																			stringBuilder.Append("\":");
																			JSONWriter.AppendValue(stringBuilder, dictionary[obj]);
																		}
																		stringBuilder.Append('}');
																	}
																}
																else
																{
																	stringBuilder.Append('{');
																	bool flag21 = true;
																	FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy);
																	for (int k = 0; k < fields.Length; k++)
																	{
																		bool flag22 = fields[k].IsDefined(typeof(IgnoreDataMemberAttribute), true);
																		if (!flag22)
																		{
																			object value = fields[k].GetValue(item);
																			bool flag23 = value != null;
																			if (flag23)
																			{
																				bool flag24 = flag21;
																				if (flag24)
																				{
																					flag21 = false;
																				}
																				else
																				{
																					stringBuilder.Append(',');
																				}
																				stringBuilder.Append('"');
																				stringBuilder.Append(JSONWriter.GetMemberName(fields[k]));
																				stringBuilder.Append("\":");
																				JSONWriter.AppendValue(stringBuilder, value);
																			}
																		}
																	}
																	PropertyInfo[] properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy);
																	for (int l = 0; l < properties.Length; l++)
																	{
																		bool flag25 = !properties[l].CanRead || properties[l].IsDefined(typeof(IgnoreDataMemberAttribute), true);
																		if (!flag25)
																		{
																			object value2 = properties[l].GetValue(item, null);
																			bool flag26 = value2 != null;
																			if (flag26)
																			{
																				bool flag27 = flag21;
																				if (flag27)
																				{
																					flag21 = false;
																				}
																				else
																				{
																					stringBuilder.Append(',');
																				}
																				stringBuilder.Append('"');
																				stringBuilder.Append(JSONWriter.GetMemberName(properties[l]));
																				stringBuilder.Append("\":");
																				JSONWriter.AppendValue(stringBuilder, value2);
																			}
																		}
																	}
																	stringBuilder.Append('}');
																}
															}
														}
													}
												}
											}
										}
									}
								}
							}
						}
					}
				}
			}
		}

		// Token: 0x0600000C RID: 12 RVA: 0x00003454 File Offset: 0x00001654
		private static string GetMemberName(MemberInfo member)
		{
			bool flag = member.IsDefined(typeof(DataMemberAttribute), true);
			if (flag)
			{
				DataMemberAttribute dataMemberAttribute = (DataMemberAttribute)Attribute.GetCustomAttribute(member, typeof(DataMemberAttribute), true);
				bool flag2 = !string.IsNullOrEmpty(dataMemberAttribute.Name);
				if (flag2)
				{
					return dataMemberAttribute.Name;
				}
			}
			return member.Name;
		}
	}
}
