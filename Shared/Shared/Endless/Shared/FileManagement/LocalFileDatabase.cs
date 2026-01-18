using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Endless.Matchmaking;
using Mono.Data.Sqlite;
using UnityEngine;

namespace Endless.Shared.FileManagement
{
	// Token: 0x020000FE RID: 254
	public class LocalFileDatabase : MonoBehaviourSingleton<LocalFileDatabase>
	{
		// Token: 0x17000100 RID: 256
		// (get) Token: 0x0600060A RID: 1546 RVA: 0x00019384 File Offset: 0x00017584
		private string LOCAL_DATABASE_PATH_KEY
		{
			get
			{
				switch (MatchmakingClientController.Instance.NetworkEnvironment)
				{
				case NetworkEnvironment.DEV:
					return "DEV_USER_LOCAL_DATABASE_PATH";
				case NetworkEnvironment.STAGING:
					return "STAGING_USER_LOCAL_DATABASE_PATH";
				case NetworkEnvironment.PROD:
					return "USER_LOCAL_DATABASE_PATH";
				default:
					throw new ArgumentOutOfRangeException("NetworkEnvironment");
				}
			}
		}

		// Token: 0x17000101 RID: 257
		// (get) Token: 0x0600060B RID: 1547 RVA: 0x000193CC File Offset: 0x000175CC
		private string DatabaseLocation
		{
			get
			{
				return Path.Combine(this.coreLocation, "FileCache/");
			}
		}

		// Token: 0x17000102 RID: 258
		// (get) Token: 0x0600060C RID: 1548 RVA: 0x000193DE File Offset: 0x000175DE
		private string FileLocation
		{
			get
			{
				return Path.Combine(this.coreLocation, "FileCache/", "Files/");
			}
		}

		// Token: 0x0600060D RID: 1549 RVA: 0x000193F5 File Offset: 0x000175F5
		public void Setup()
		{
			this.LoadCoreLocation();
			this.Initialize();
		}

		// Token: 0x0600060E RID: 1550 RVA: 0x00019404 File Offset: 0x00017604
		private void LoadCoreLocation()
		{
			this.coreLocation = PlayerPrefs.GetString(this.LOCAL_DATABASE_PATH_KEY, null);
			if (string.IsNullOrEmpty(this.coreLocation))
			{
				this.coreLocation = Application.persistentDataPath;
				string text = string.Empty;
				text = "_STANDALONE";
				switch (MatchmakingClientController.Instance.NetworkEnvironment)
				{
				case NetworkEnvironment.DEV:
					this.coreLocation = Path.Combine(this.coreLocation, "DEV_" + text);
					break;
				case NetworkEnvironment.STAGING:
					this.coreLocation = Path.Combine(this.coreLocation, "STAGING_" + text);
					break;
				case NetworkEnvironment.PROD:
					this.coreLocation = Path.Combine(this.coreLocation, text);
					break;
				default:
					throw new ArgumentOutOfRangeException();
				}
				PlayerPrefs.SetString(this.LOCAL_DATABASE_PATH_KEY, this.coreLocation);
				PlayerPrefs.Save();
			}
		}

		// Token: 0x0600060F RID: 1551 RVA: 0x000194D5 File Offset: 0x000176D5
		protected override void OnDestroy()
		{
			base.OnDestroy();
			this.SQLiteClose();
		}

		// Token: 0x06000610 RID: 1552 RVA: 0x000194E4 File Offset: 0x000176E4
		private void RunTests()
		{
			int num = new global::System.Random().Next(0, int.MaxValue);
			this.CreateNewFileEntry(num, ".png", new byte[25]);
			this.CreateNewFileEntry(new global::System.Random().Next(0, int.MaxValue), ".png", new byte[50]);
			Debug.Log(this.GetFilePathForImmediateLoad(num));
			Debug.Log(string.Format("Does file exist (expected true): {0}", this.DoesFileExist(num)));
			Debug.Log(string.Format("Cache Size: {0}", this.GetCacheSize()));
			Debug.Log("Verify check: " + (this.VerifyDatabase() == 0).ToString());
			this.DeleteFile(num);
			Debug.Log(string.Format("Does file exist (expected false): {0}", this.DoesFileExist(num)));
			Debug.Log(this.GetFilePathForImmediateLoad(num));
			Debug.Log(string.Format("Cache Size: {0}", this.GetCacheSize()));
			foreach (ValueTuple<int, string, long, float> valueTuple in this.ScoreExistingFiles())
			{
				Debug.Log(string.Format("Score: {0}| {1}, {2}", valueTuple.Item4, valueTuple.Item1, valueTuple.Item2));
			}
		}

		// Token: 0x06000611 RID: 1553 RVA: 0x00019644 File Offset: 0x00017844
		protected void Initialize()
		{
			Directory.CreateDirectory(this.DatabaseLocation);
			string text = "URI=file:" + this.DatabaseLocation + "FileCache.db";
			Debug.Log("databaseLocation " + text);
			if (this.debug)
			{
				Debug.Log("databaseLocation " + text);
			}
			this._connection = new SqliteConnection(text);
			this._command = this._connection.CreateCommand();
			this._connection.Open();
			this._command.CommandText = "PRAGMA journal_mode = WAL;";
			this._command.ExecuteNonQuery();
			this._command.CommandText = "PRAGMA journal_mode";
			this._reader = this._command.ExecuteReader();
			if (this.debug && this._reader.Read())
			{
				Debug.Log("SQLiter - WAL value is: " + this._reader.GetString(0));
			}
			this._reader.Close();
			this._command.CommandText = "PRAGMA synchronous = OFF";
			this._command.ExecuteNonQuery();
			this._command.CommandText = "PRAGMA synchronous";
			this._reader = this._command.ExecuteReader();
			if (this.debug && this._reader.Read())
			{
				Debug.Log("SQLiter - synchronous value is: " + this._reader.GetInt32(0).ToString());
			}
			this._reader.Close();
			this._command.CommandText = "SELECT name FROM sqlite_master WHERE name='FileEntries'";
			this._reader = this._command.ExecuteReader();
			bool flag = false;
			if (!this._reader.Read())
			{
				Debug.Log("SQLiter - Could not find SQLite table FileEntries");
				flag = true;
			}
			this._reader.Close();
			if (flag)
			{
				Debug.Log("SQLiter - Dropping old SQLite table if Exists: FileEntries");
				this._command.CommandText = "DROP TABLE IF EXISTS FileEntries";
				this._command.ExecuteNonQuery();
				Debug.Log("SQLiter - Creating new SQLite table: FileEntries");
				this._sqlString = "CREATE TABLE IF NOT EXISTS FileEntries (FileInstanceId TEXT NOT NULL, Location TEXT, FileSize INTEGER DEFAULT 0, DownloadTime INTEGER DEFAULT 0, AccessTime INTEGER DEFAULT 0, AccessCount INTEGER DEFAULT 0, PRIMARY KEY (FileInstanceId))";
				if (this.debug)
				{
					Debug.Log("Creation string: " + this._sqlString);
				}
				this._command.CommandText = this._sqlString;
				this._command.ExecuteNonQuery();
				return;
			}
			if (this.debug)
			{
				Debug.Log("SQLiter - SQLite table FileEntries was found");
			}
		}

		// Token: 0x06000612 RID: 1554 RVA: 0x00019890 File Offset: 0x00017A90
		private void SQLiteClose()
		{
			if (this._reader != null && !this._reader.IsClosed)
			{
				this._reader.Close();
			}
			this._reader = null;
			if (this._command != null)
			{
				this._command.Dispose();
			}
			this._command = null;
			if (this._connection != null && this._connection.State != ConnectionState.Closed)
			{
				this._connection.Close();
			}
			this._connection = null;
		}

		// Token: 0x06000613 RID: 1555 RVA: 0x00019908 File Offset: 0x00017B08
		public void CreateNewFileEntry(int fileInstanceId, string fileExtension, byte[] bytes)
		{
			string text = this.GenerateFilePath(fileInstanceId, fileExtension);
			using (FileStream fileStream = File.Create(text))
			{
				fileStream.Write(bytes);
			}
			this.CreateNewFileEntry(fileInstanceId, text, bytes.Length);
			this.HandlePotentialCacheOverFlow();
		}

		// Token: 0x06000614 RID: 1556 RVA: 0x00019960 File Offset: 0x00017B60
		private void CreateNewFileEntry(int fileInstanceId, string filePath, int fileSize)
		{
			long ticks = DateTime.UtcNow.Ticks;
			string text = "INSERT OR REPLACE INTO FileEntries (FileInstanceId, Location, FileSize, DownloadTime, AccessTime, AccessCount)" + string.Format("VALUES('{0}', '{1}', {2}, {3}, {4}, {5})", new object[] { fileInstanceId, filePath, fileSize, ticks, ticks, 1 });
			if (this.debug)
			{
				Debug.Log("CreateNewFileEntry: " + text);
			}
			this.ExecuteNonQuery(text);
		}

		// Token: 0x06000615 RID: 1557 RVA: 0x000199E8 File Offset: 0x00017BE8
		private string GenerateFilePath(int fileInstanceId, string fileExtension)
		{
			string text = string.Format("{0}{1}{2}", this.FileLocation, fileInstanceId, fileExtension);
			if (this.debug)
			{
				Debug.Log("Generated file path: " + text);
			}
			Directory.CreateDirectory(Path.GetDirectoryName(text));
			return text;
		}

		// Token: 0x06000616 RID: 1558 RVA: 0x00019A32 File Offset: 0x00017C32
		private void ExecuteNonQuery(string commandText)
		{
			this._command.CommandText = commandText;
			this._command.ExecuteNonQuery();
		}

		// Token: 0x06000617 RID: 1559 RVA: 0x00019A4C File Offset: 0x00017C4C
		private static string GetWhereClause(int fileInstanceId)
		{
			return string.Format("WHERE {0} ='{1}'", "FileInstanceId", fileInstanceId);
		}

		// Token: 0x06000618 RID: 1560 RVA: 0x00019A64 File Offset: 0x00017C64
		public long GetCacheSize()
		{
			this._command.CommandText = "SELECT FileSize FROM FileEntries WHERE Location IS NOT NULL";
			this._reader = this._command.ExecuteReader();
			long num = 0L;
			while (this._reader.Read())
			{
				num += this._reader.GetInt64(0);
			}
			this._reader.Close();
			return num;
		}

		// Token: 0x06000619 RID: 1561 RVA: 0x00019AC0 File Offset: 0x00017CC0
		public void DeleteFile(int fileInstanceId)
		{
			string text = null;
			this._command.CommandText = "SELECT Location FROM FileEntries " + LocalFileDatabase.GetWhereClause(fileInstanceId);
			this._reader = this._command.ExecuteReader();
			if (this._reader.Read())
			{
				text = this._reader.GetString(0);
			}
			this._reader.Close();
			this.DeleteFile(fileInstanceId, text);
		}

		// Token: 0x0600061A RID: 1562 RVA: 0x00019B28 File Offset: 0x00017D28
		private void DeleteFile(int fileInstanceId, string filePath = null)
		{
			if (this.debug)
			{
				Debug.Log("Deleting file at " + filePath);
			}
			if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
			{
				File.Delete(filePath);
			}
			string text = "UPDATE FileEntries SET Location = NULL, FileSize = 0 " + LocalFileDatabase.GetWhereClause(fileInstanceId);
			this.ExecuteNonQuery(text);
		}

		// Token: 0x0600061B RID: 1563 RVA: 0x00019B7C File Offset: 0x00017D7C
		public bool DoesFileExist(int fileInstanceId)
		{
			bool flag = false;
			this._command.CommandText = "SELECT Location FROM FileEntries " + LocalFileDatabase.GetWhereClause(fileInstanceId);
			this._reader = this._command.ExecuteReader();
			if (this._reader.Read())
			{
				flag = this._reader.GetValue(0) != null;
			}
			this._reader.Close();
			return flag;
		}

		// Token: 0x0600061C RID: 1564 RVA: 0x00019BE0 File Offset: 0x00017DE0
		public string GetFilePathForImmediateLoad(int fileInstanceId)
		{
			string text = null;
			int num = 0;
			long ticks = DateTime.UtcNow.Ticks;
			this._command.CommandText = "SELECT Location, AccessCount FROM FileEntries " + LocalFileDatabase.GetWhereClause(fileInstanceId) + " AND Location IS NOT NULL";
			if (this.debug)
			{
				Debug.Log("GetFilePathForImmediateLoad: " + this._command.CommandText);
			}
			this._reader = this._command.ExecuteReader();
			if (this._reader.Read())
			{
				text = this._reader.GetString(0);
				num = this._reader.GetInt32(1) + 1;
				if (this.debug)
				{
					Debug.Log(string.Format("New access count: {0}", num));
				}
			}
			this._reader.Close();
			if (!File.Exists(text))
			{
				text = null;
			}
			if (text != null)
			{
				string text2 = string.Format("UPDATE {0} SET {1} = {2}, {3} = {4} {5}", new object[]
				{
					"FileEntries",
					"AccessCount",
					num,
					"AccessTime",
					ticks,
					LocalFileDatabase.GetWhereClause(fileInstanceId)
				});
				this.ExecuteNonQuery(text2);
			}
			return text;
		}

		// Token: 0x0600061D RID: 1565 RVA: 0x00019D00 File Offset: 0x00017F00
		public void ClearDatabase()
		{
			this._command.CommandText = "SELECT FileInstanceId, Location FROM FileEntries WHERE Location IS NOT NULL";
			this._reader = this._command.ExecuteReader();
			List<ValueTuple<int, string>> list = new List<ValueTuple<int, string>>();
			while (this._reader.Read())
			{
				list.Add(new ValueTuple<int, string>(int.Parse(this._reader.GetString(0)), this._reader.GetString(1)));
			}
			this._reader.Close();
			foreach (ValueTuple<int, string> valueTuple in list)
			{
				this.DeleteFile(valueTuple.Item1, valueTuple.Item2);
			}
		}

		// Token: 0x0600061E RID: 1566 RVA: 0x00019DC4 File Offset: 0x00017FC4
		public int VerifyDatabase()
		{
			int num = 0;
			List<ValueTuple<int, string>> list = new List<ValueTuple<int, string>>();
			this._command.CommandText = "SELECT FileInstanceId, Location FROM FileEntries WHERE Location IS NOT NULL";
			this._reader = this._command.ExecuteReader();
			while (this._reader.Read())
			{
				list.Add(new ValueTuple<int, string>(int.Parse(this._reader.GetString(0)), this._reader.GetString(1)));
			}
			this._reader.Close();
			List<string> list2 = Directory.GetFiles(this.FileLocation).ToList<string>();
			foreach (ValueTuple<int, string> valueTuple in list)
			{
				if (list2.Contains(valueTuple.Item2))
				{
					list2.Remove(valueTuple.Item2);
				}
				else
				{
					Debug.Log("Missing expected file: " + valueTuple.Item2);
					this.DeleteFile(valueTuple.Item1);
					num++;
				}
			}
			foreach (string text in list2)
			{
				Debug.Log("Deleting untracked file: " + text);
				File.Delete(text);
				num++;
			}
			return num;
		}

		// Token: 0x0600061F RID: 1567 RVA: 0x00019F24 File Offset: 0x00018124
		public void SetCacheMaxSize(long newMaxSize)
		{
			this.maxCacheSize = newMaxSize;
			this.HandlePotentialCacheOverFlow();
		}

		// Token: 0x06000620 RID: 1568 RVA: 0x00019F33 File Offset: 0x00018133
		public long GetCacheMaxSize()
		{
			return this.maxCacheSize;
		}

		// Token: 0x06000621 RID: 1569 RVA: 0x00019F3C File Offset: 0x0001813C
		public void ClearOldFiles()
		{
			Queue<ValueTuple<int, string, long, float>> queue = new Queue<ValueTuple<int, string, long, float>>(this.ScoreExistingFiles().ToArray<ValueTuple<int, string, long, float>>());
			while (queue.Count > 0 && queue.Peek().Item4 > 0.5f)
			{
				ValueTuple<int, string, long, float> valueTuple = queue.Dequeue();
				this.DeleteFile(valueTuple.Item1, valueTuple.Item2);
			}
		}

		// Token: 0x06000622 RID: 1570 RVA: 0x00019F90 File Offset: 0x00018190
		[return: TupleElementNames(new string[] { "fileInstanceId", "filePath", "fileSize", "score" })]
		private IOrderedEnumerable<ValueTuple<int, string, long, float>> ScoreExistingFiles()
		{
			List<ValueTuple<int, string, long, int, long>> list = new List<ValueTuple<int, string, long, int, long>>();
			this._command.CommandText = "SELECT FileInstanceId, Location, AccessTime, AccessCount, FileSize FROM FileEntries WHERE Location IS NOT NULL";
			this._reader = this._command.ExecuteReader();
			while (this._reader.Read())
			{
				list.Add(new ValueTuple<int, string, long, int, long>(this._reader.GetInt32(0), this._reader.GetString(1), this._reader.GetInt64(2), this._reader.GetInt32(3), this._reader.GetInt64(4)));
			}
			this._reader.Close();
			return from e in list
				select new ValueTuple<int, string, long, float>(e.Item1, e.Item2, e.Item5, this.GetScore(e.Item5, e.Item4, e.Item3)) into score
				orderby score.Item4 descending
				select score;
		}

		// Token: 0x06000623 RID: 1571 RVA: 0x0001A05C File Offset: 0x0001825C
		private float GetScore(long fileSize, int accessCount, long accessTime)
		{
			long num = (DateTime.UtcNow.Ticks - accessTime) / 864000000000L;
			long num2 = fileSize / 1000L;
			float num3 = 1f - 1f / (1f + (float)num / 5f);
			float num4 = 1f / (1f + (float)accessCount / 2f);
			float num5 = 1f / (1f + (float)num2 / 75000f);
			return Mathf.Clamp01((num3 + num3 + num4) / 3f);
		}

		// Token: 0x06000624 RID: 1572 RVA: 0x0001A0E0 File Offset: 0x000182E0
		private void HandlePotentialCacheOverFlow()
		{
			long num = this.GetCacheSize();
			if (num > this.maxCacheSize)
			{
				if (this.debug)
				{
					Debug.LogWarning("Cache size exceeded!");
				}
				Queue<ValueTuple<int, string, long, float>> queue = new Queue<ValueTuple<int, string, long, float>>(this.ScoreExistingFiles().ToArray<ValueTuple<int, string, long, float>>());
				while (num > this.maxCacheSize)
				{
					ValueTuple<int, string, long, float> valueTuple = queue.Dequeue();
					num -= valueTuple.Item3;
					this.DeleteFile(valueTuple.Item1, valueTuple.Item2);
				}
			}
		}

		// Token: 0x06000625 RID: 1573 RVA: 0x0001A14D File Offset: 0x0001834D
		public string GetCoreFilePath()
		{
			return this.coreLocation;
		}

		// Token: 0x06000626 RID: 1574 RVA: 0x0001A158 File Offset: 0x00018358
		public void MoveDatabase(string newFilePath)
		{
			string databaseLocation = this.DatabaseLocation;
			this.ClearDatabase();
			this.coreLocation = newFilePath;
			File.Copy(databaseLocation + "FileCache.db", this.DatabaseLocation + "FileCache.db", true);
			PlayerPrefs.SetString(this.LOCAL_DATABASE_PATH_KEY, this.coreLocation);
			PlayerPrefs.Save();
			this.Initialize();
		}

		// Token: 0x04000354 RID: 852
		[SerializeField]
		private bool debug;

		// Token: 0x04000355 RID: 853
		private const string CACHE_PATH = "FileCache/";

		// Token: 0x04000356 RID: 854
		private const string DATABASE_FILE_NAME = "FileCache.db";

		// Token: 0x04000357 RID: 855
		private const string FILES_PATH = "Files/";

		// Token: 0x04000358 RID: 856
		private const string SQL_TABLE_NAME = "FileEntries";

		// Token: 0x04000359 RID: 857
		private const string COLUMN_FILE_INSTANCE_ID = "FileInstanceId";

		// Token: 0x0400035A RID: 858
		private const string COLUMN_LOCATION = "Location";

		// Token: 0x0400035B RID: 859
		private const string COLUMN_DOWNLOAD_TIME = "DownloadTime";

		// Token: 0x0400035C RID: 860
		private const string COLUMN_ACCESS_TIME = "AccessTime";

		// Token: 0x0400035D RID: 861
		private const string COLUMN_ACCESS_COUNT = "AccessCount";

		// Token: 0x0400035E RID: 862
		private const string COLUMN_FILE_SIZE = "FileSize";

		// Token: 0x0400035F RID: 863
		private const string LOCAL_DATABASE_PATH_KEY_BASE = "USER_LOCAL_DATABASE_PATH";

		// Token: 0x04000360 RID: 864
		private string coreLocation;

		// Token: 0x04000361 RID: 865
		private IDbConnection _connection;

		// Token: 0x04000362 RID: 866
		private IDbCommand _command;

		// Token: 0x04000363 RID: 867
		private IDataReader _reader;

		// Token: 0x04000364 RID: 868
		private string _sqlString;

		// Token: 0x04000365 RID: 869
		private long maxCacheSize = 5000000000L;
	}
}
