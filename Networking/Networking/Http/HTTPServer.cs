using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;

namespace Endless.Networking.Http
{
	// Token: 0x02000023 RID: 35
	public class HTTPServer : IDisposable
	{
		// Token: 0x17000036 RID: 54
		// (get) Token: 0x06000126 RID: 294 RVA: 0x00005A04 File Offset: 0x00003C04
		public string LogFileName
		{
			get
			{
				return HTTPConfig.LogFileName;
			}
		}

		// Token: 0x17000037 RID: 55
		// (get) Token: 0x06000127 RID: 295 RVA: 0x00005A0B File Offset: 0x00003C0B
		public string ServerPrefix
		{
			get
			{
				return HTTPConfig.ServerPrefix;
			}
		}

		// Token: 0x17000038 RID: 56
		// (get) Token: 0x06000128 RID: 296 RVA: 0x00005A12 File Offset: 0x00003C12
		public int ServerPort
		{
			get
			{
				return HTTPConfig.ServerPort;
			}
		}

		// Token: 0x17000039 RID: 57
		// (get) Token: 0x06000129 RID: 297 RVA: 0x00005A19 File Offset: 0x00003C19
		public bool DirectoryPermission
		{
			get
			{
				return HTTPConfig.DirectoryPermission;
			}
		}

		// Token: 0x0600012A RID: 298 RVA: 0x00005A20 File Offset: 0x00003C20
		public HTTPServer(string path)
		{
			this.rootDirectory = path;
			this.serverThread = new Thread(new ThreadStart(this.Listen));
			this.serverThread.Start();
		}

		// Token: 0x0600012B RID: 299 RVA: 0x00005A90 File Offset: 0x00003C90
		private void Listen()
		{
			this.listener = new HttpListener();
			this.listener.Prefixes.Add(this.ServerPrefix + ":" + this.ServerPort.ToString() + "/");
			this.listener.Start();
			for (;;)
			{
				try
				{
					HttpListenerContext context = this.listener.GetContext();
					this.httpRequests.Add(context);
				}
				catch (Exception ex)
				{
					Logger.Log(this.LogFileName, ex.ToString(), true);
				}
			}
		}

		// Token: 0x0600012C RID: 300 RVA: 0x00005B38 File Offset: 0x00003D38
		public void RegisterGetHandler(string uri, Func<byte[]> handler)
		{
			bool flag = uri == null || handler == null;
			if (flag)
			{
				Logger.Log(this.LogFileName, "Get handler is invalid!", true);
			}
			else
			{
				this.getUriHandlers[uri] = handler;
			}
		}

		// Token: 0x0600012D RID: 301 RVA: 0x00005B78 File Offset: 0x00003D78
		public void RemoveGetHandler(string uri)
		{
			Func<byte[]> func;
			bool flag = !this.getUriHandlers.TryGetValue(uri, out func);
			if (!flag)
			{
				this.getUriHandlers.Remove(uri);
			}
		}

		// Token: 0x0600012E RID: 302 RVA: 0x00005BAC File Offset: 0x00003DAC
		public void RegisterPostHandler(string uri, Action<byte[]> handler)
		{
			bool flag = uri == null || handler == null;
			if (flag)
			{
				Logger.Log(this.LogFileName, "Post handler is invalid!", true);
			}
			else
			{
				this.postUriHandlers[uri] = handler;
			}
		}

		// Token: 0x0600012F RID: 303 RVA: 0x00005BEC File Offset: 0x00003DEC
		public void RemovePostHandler(string uri)
		{
			Action<byte[]> action;
			bool flag = !this.postUriHandlers.TryGetValue(uri, out action);
			if (!flag)
			{
				this.postUriHandlers.Remove(uri);
			}
		}

		// Token: 0x06000130 RID: 304 RVA: 0x00005C20 File Offset: 0x00003E20
		public void Update()
		{
			for (;;)
			{
				HttpListenerContext httpListenerContext;
				bool flag = this.httpRequests.TryTake(out httpListenerContext);
				if (!flag)
				{
					break;
				}
				this.Process(httpListenerContext);
			}
		}

		// Token: 0x06000131 RID: 305 RVA: 0x00005C4C File Offset: 0x00003E4C
		private void Process(HttpListenerContext context)
		{
			string text = null;
			try
			{
				text = context.Request.Url.AbsolutePath;
				text = text.Substring(1);
			}
			catch (Exception ex)
			{
				Logger.Log(this.LogFileName, ex.ToString(), true);
				context.Response.StatusCode = 400;
				context.Response.OutputStream.Close();
				return;
			}
			bool flag = string.IsNullOrWhiteSpace(text);
			if (flag)
			{
				text = "";
			}
			string httpMethod = context.Request.HttpMethod;
			string text2 = httpMethod;
			if (!(text2 == "GET"))
			{
				if (!(text2 == "POST"))
				{
					context.Response.StatusCode = 404;
				}
				else
				{
					Action<byte[]> action;
					bool flag2 = this.postUriHandlers.TryGetValue(text, out action);
					if (flag2)
					{
						try
						{
							Stream inputStream = context.Request.InputStream;
							bool flag3 = inputStream == null;
							if (flag3)
							{
								context.Response.StatusCode = 422;
							}
							else
							{
								int num = 200;
								byte[] array = HTTPServer.ReadFully(inputStream, num);
								DataBuffer dataBuffer = DataBuffer.FromBytes(array);
								int num2 = dataBuffer.ReadInteger(true);
								byte[] array2 = dataBuffer.ReadBytes(num2, true);
								if (action != null)
								{
									action(array2);
								}
								context.Response.StatusCode = 200;
							}
						}
						catch (Exception ex2)
						{
							Logger.Log(this.LogFileName, ex2.ToString(), true);
							context.Response.StatusCode = 500;
						}
					}
				}
			}
			else
			{
				Func<byte[]> func;
				bool flag4 = this.getUriHandlers.TryGetValue(text, out func);
				if (flag4)
				{
					try
					{
						byte[] array3 = ((func != null) ? func() : null);
						bool flag5 = array3 == null;
						if (flag5)
						{
							context.Response.StatusCode = 404;
						}
						else
						{
							using (MemoryStream memoryStream = new MemoryStream())
							{
								using (BinaryWriter binaryWriter = new BinaryWriter(memoryStream, Encoding.UTF8))
								{
									binaryWriter.Write(array3);
									memoryStream.Seek(0L, SeekOrigin.Begin);
									this.SetContextResponseContent(context, "application/octet-stream", memoryStream);
								}
							}
						}
					}
					catch (Exception ex3)
					{
						Logger.Log(this.LogFileName, ex3.ToString(), true);
						context.Response.StatusCode = 500;
					}
				}
				else
				{
					bool directoryPermission = this.DirectoryPermission;
					if (directoryPermission)
					{
						text = Path.Combine(this.rootDirectory, text);
						bool flag6 = File.Exists(text);
						if (flag6)
						{
							object obj = HTTPServer.fileLock;
							lock (obj)
							{
								try
								{
									Stream stream = new FileStream(text, FileMode.Open);
									string text3;
									this.SetContextResponseContent(context, HTTPServer.mimeTypeMappings.TryGetValue(Path.GetExtension(text), out text3) ? text3 : "application/octet-stream", stream);
									stream.Close();
								}
								catch (Exception ex4)
								{
									Logger.Log(this.LogFileName, ex4.ToString(), true);
									context.Response.StatusCode = 500;
								}
							}
						}
						else
						{
							context.Response.StatusCode = 404;
						}
					}
				}
			}
			context.Response.OutputStream.Close();
		}

		// Token: 0x06000132 RID: 306 RVA: 0x00005FF0 File Offset: 0x000041F0
		private void SetContextResponseContent(HttpListenerContext context, string contentType, Stream content)
		{
			bool flag = string.IsNullOrWhiteSpace(contentType);
			if (flag)
			{
				throw new InvalidDataException(string.Format("Http response content data invalid. {0}, {1}", contentType, content.Length));
			}
			context.Response.ContentType = contentType;
			context.Response.ContentLength64 = content.Length;
			context.Response.ContentEncoding = Encoding.UTF8;
			context.Response.AddHeader("Date", DateTime.Now.ToString("r"));
			context.Response.AddHeader("Last-Modified", DateTime.Now.ToString("r"));
			int num;
			while ((num = content.Read(this.responseBuffer, 0, this.responseBuffer.Length)) > 0)
			{
				context.Response.OutputStream.Write(this.responseBuffer, 0, num);
			}
			context.Response.StatusCode = 200;
			context.Response.OutputStream.Flush();
		}

		// Token: 0x06000133 RID: 307 RVA: 0x000060F8 File Offset: 0x000042F8
		public static byte[] ReadFully(Stream stream, int initialLength)
		{
			bool flag = initialLength < 1;
			if (flag)
			{
				initialLength = 32768;
			}
			byte[] array = new byte[initialLength];
			int num = 0;
			int num2;
			while ((num2 = stream.Read(array, num, array.Length - num)) > 0)
			{
				num += num2;
				bool flag2 = num == array.Length;
				if (flag2)
				{
					int num3 = stream.ReadByte();
					bool flag3 = num3 == -1;
					if (flag3)
					{
						return array;
					}
					byte[] array2 = new byte[array.Length * 2];
					Array.Copy(array, array2, array.Length);
					array2[num] = (byte)num3;
					array = array2;
					num++;
				}
			}
			byte[] array3 = new byte[num];
			Array.Copy(array, array3, num);
			return array3;
		}

		// Token: 0x06000134 RID: 308 RVA: 0x000061A6 File Offset: 0x000043A6
		public void Dispose()
		{
			this.serverThread.Abort();
			this.listener.Stop();
		}

		// Token: 0x04000085 RID: 133
		private const string GET_METHOD = "GET";

		// Token: 0x04000086 RID: 134
		private const string POST_METHOD = "POST";

		// Token: 0x04000087 RID: 135
		private static readonly object fileLock = new object();

		// Token: 0x04000088 RID: 136
		private static readonly IDictionary<string, string> mimeTypeMappings = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase)
		{
			{ ".asf", "video/x-ms-asf" },
			{ ".asx", "video/x-ms-asf" },
			{ ".avi", "video/x-msvideo" },
			{ ".bin", "application/octet-stream" },
			{ ".cco", "application/x-cocoa" },
			{ ".crt", "application/x-x509-ca-cert" },
			{ ".css", "text/css" },
			{ ".deb", "application/octet-stream" },
			{ ".der", "application/x-x509-ca-cert" },
			{ ".dll", "application/octet-stream" },
			{ ".dmg", "application/octet-stream" },
			{ ".ear", "application/java-archive" },
			{ ".eot", "application/octet-stream" },
			{ ".exe", "application/octet-stream" },
			{ ".flv", "video/x-flv" },
			{ ".gif", "image/gif" },
			{ ".hqx", "application/mac-binhex40" },
			{ ".htc", "text/x-component" },
			{ ".htm", "text/html" },
			{ ".html", "text/html" },
			{ ".ico", "image/x-icon" },
			{ ".img", "application/octet-stream" },
			{ ".iso", "application/octet-stream" },
			{ ".jar", "application/java-archive" },
			{ ".jardiff", "application/x-java-archive-diff" },
			{ ".jng", "image/x-jng" },
			{ ".jnlp", "application/x-java-jnlp-file" },
			{ ".jpeg", "image/jpeg" },
			{ ".jpg", "image/jpeg" },
			{ ".js", "application/x-javascript" },
			{ ".mml", "text/mathml" },
			{ ".mng", "video/x-mng" },
			{ ".mov", "video/quicktime" },
			{ ".mp3", "audio/mpeg" },
			{ ".mpeg", "video/mpeg" },
			{ ".mpg", "video/mpeg" },
			{ ".msi", "application/octet-stream" },
			{ ".msm", "application/octet-stream" },
			{ ".msp", "application/octet-stream" },
			{ ".pdb", "application/x-pilot" },
			{ ".pdf", "application/pdf" },
			{ ".pem", "application/x-x509-ca-cert" },
			{ ".pl", "application/x-perl" },
			{ ".pm", "application/x-perl" },
			{ ".png", "image/png" },
			{ ".prc", "application/x-pilot" },
			{ ".ra", "audio/x-realaudio" },
			{ ".rar", "application/x-rar-compressed" },
			{ ".rpm", "application/x-redhat-package-manager" },
			{ ".rss", "text/xml" },
			{ ".run", "application/x-makeself" },
			{ ".sea", "application/x-sea" },
			{ ".shtml", "text/html" },
			{ ".sit", "application/x-stuffit" },
			{ ".swf", "application/x-shockwave-flash" },
			{ ".tcl", "application/x-tcl" },
			{ ".tk", "application/x-tcl" },
			{ ".txt", "text/plain" },
			{ ".war", "application/java-archive" },
			{ ".wbmp", "image/vnd.wap.wbmp" },
			{ ".wmv", "video/x-ms-wmv" },
			{ ".xml", "text/xml" },
			{ ".xpi", "application/x-xpinstall" },
			{ ".zip", "application/zip" }
		};

		// Token: 0x04000089 RID: 137
		private readonly Thread serverThread;

		// Token: 0x0400008A RID: 138
		private readonly string rootDirectory;

		// Token: 0x0400008B RID: 139
		private readonly BlockingCollection<HttpListenerContext> httpRequests = new BlockingCollection<HttpListenerContext>();

		// Token: 0x0400008C RID: 140
		private readonly Dictionary<string, Func<byte[]>> getUriHandlers = new Dictionary<string, Func<byte[]>>();

		// Token: 0x0400008D RID: 141
		private readonly Dictionary<string, Action<byte[]>> postUriHandlers = new Dictionary<string, Action<byte[]>>();

		// Token: 0x0400008E RID: 142
		private readonly byte[] responseBuffer = new byte[16384];

		// Token: 0x0400008F RID: 143
		private HttpListener listener;
	}
}
