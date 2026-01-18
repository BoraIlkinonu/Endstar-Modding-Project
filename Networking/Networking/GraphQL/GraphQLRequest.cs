using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Endless.Networking.Http;
using Newtonsoft.Json;

namespace Endless.Networking.GraphQL
{
	// Token: 0x02000025 RID: 37
	public static class GraphQLRequest
	{
		// Token: 0x06000136 RID: 310 RVA: 0x0000662C File Offset: 0x0000482C
		public static async Task<GraphQLResult> Request(QueryType type, string callName, object inputArgs = null, string returnArgs = null, string authToken = null, bool debugQuery = false)
		{
			string queryTypeString;
			switch (type)
			{
			case QueryType.Query:
				queryTypeString = "query";
				goto IL_0074;
			case QueryType.Subscription:
				queryTypeString = "subscription";
				Logger.Log(null, "GraphQL subscription query type is not yet supported!", true);
				return null;
			}
			queryTypeString = "mutation";
			IL_0074:
			string parsedArgs = "";
			bool flag = inputArgs != null;
			if (flag)
			{
				var input = new
				{
					input = inputArgs
				};
				string jsonArgs = JsonConvert.SerializeObject(input);
				parsedArgs = "(" + GraphQLRequest.JsonToArgument(jsonArgs) + ")";
				input = null;
				jsonArgs = null;
			}
			string parsedReturnArgs = "";
			bool flag2 = returnArgs != null;
			if (flag2)
			{
				parsedReturnArgs = "{" + returnArgs + "}";
			}
			string query = string.Concat(new string[] { queryTypeString, "{", callName, parsedArgs, parsedReturnArgs, "}" });
			if (debugQuery)
			{
				Logger.Log(null, query, true);
			}
			Task<GraphQLResult> request = GraphQLRequest.PostAsync("https://dev.graphql-api.endlessstudios.com/graphql", query, authToken);
			await request;
			return request.Result;
		}

		// Token: 0x06000137 RID: 311 RVA: 0x00006698 File Offset: 0x00004898
		public static async Task<GraphQLResult> PostAsync(string url, string details, string authToken = null)
		{
			GraphQLResult graphQLResult;
			try
			{
				string jsonData = JsonConvert.SerializeObject(new
				{
					query = details
				});
				Logger.Log(null, "Client request [" + authToken + "]: " + jsonData, true);
				byte[] postData = Encoding.ASCII.GetBytes(jsonData);
				HttpClient httpClient = HTTPClient.Client;
				bool flag = !string.IsNullOrWhiteSpace(authToken);
				if (flag)
				{
					httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);
				}
				Task<HttpResponseMessage> tsk = httpClient.PostAsync(url, new ByteArrayContent(postData)
				{
					Headers = 
					{
						ContentType = new MediaTypeHeaderValue("application/json")
					}
				}, CancellationToken.None);
				await tsk;
				if (tsk.IsCompletedSuccessfully)
				{
					HttpResponseMessage responseMessage = tsk.Result;
					if (responseMessage != null && responseMessage.IsSuccessStatusCode)
					{
						Task<string> responseContent = responseMessage.Content.ReadAsStringAsync();
						await responseContent;
						string text = responseContent.Result;
						Logger.Log(null, "Server response: " + text, true);
						GraphQLResult result = JsonConvert.DeserializeObject<GraphQLResult>(text);
						if (result.HasErrors)
						{
							Logger.Log(null, "GraphQL error: \n" + GraphQLRequest.FormatJson(text), true);
						}
						result.RawResult = text;
						graphQLResult = result;
					}
					else
					{
						Logger.Log(null, "GraphQL PostAsync error: " + responseMessage.StatusCode.ToString(), true);
						graphQLResult = null;
					}
				}
				else
				{
					string text2 = null;
					string text3 = "GraphQL PostAsync error: ";
					AggregateException exception = tsk.Exception;
					Logger.Log(text2, text3 + ((exception != null) ? exception.Message : null), true);
					graphQLResult = null;
				}
			}
			catch (Exception e)
			{
				Logger.Log(null, e.ToString(), true);
				graphQLResult = null;
			}
			return graphQLResult;
		}

		// Token: 0x06000138 RID: 312 RVA: 0x000066EC File Offset: 0x000048EC
		public static string FormatJson(string json)
		{
			object obj = JsonConvert.DeserializeObject(json);
			return JsonConvert.SerializeObject(obj, Formatting.Indented);
		}

		// Token: 0x06000139 RID: 313 RVA: 0x0000670C File Offset: 0x0000490C
		private static string JsonToArgument(string jsonInput)
		{
			char[] array = jsonInput.ToCharArray();
			List<int> list = new List<int>();
			array[0] = ' ';
			char[] array2 = array;
			array2[array2.Length - 1] = ' ';
			for (int i = 0; i < array.Length; i++)
			{
				bool flag = array[i] == '"';
				if (flag)
				{
					bool flag2 = list.Count == 2;
					if (flag2)
					{
						list = new List<int>();
					}
					list.Add(i);
				}
				bool flag3 = array[i] == ':';
				if (flag3)
				{
					array[list[0]] = ' ';
					array[list[1]] = ' ';
				}
			}
			return new string(array);
		}

		// Token: 0x04000094 RID: 148
		private const string url = "https://dev.graphql-api.endlessstudios.com/graphql";
	}
}
