using BlueStacks.hyperDroid.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.CompilerServices;

internal class SplitDownloader
{
	public delegate void ProgressCb(int percent);

	public delegate void CompletedCb(string filePath);

	public delegate void ExceptionCb(Exception e);

	public delegate void DownloadFileProgressCb(long downloaded, long size);

	public delegate void DownloadFileCompletedCb(string filePath);

	public delegate void DownloadFileExceptionCb(Exception e);

	public class FilePart
	{
		private string m_Name;

		private long m_Size;

		private string m_SHA1;

		private string m_Path;

		private long m_DownloadedSize;

		public string Name
		{
			get
			{
				return this.m_Name;
			}
			set
			{
				this.m_Name = value;
			}
		}

		public long Size
		{
			get
			{
				return this.m_Size;
			}
			set
			{
				this.m_Size = value;
			}
		}

		public string SHA1
		{
			get
			{
				return this.m_SHA1;
			}
			set
			{
				this.m_SHA1 = value;
			}
		}

		public string Path
		{
			get
			{
				return this.m_Path;
			}
		}

		public long DownloadedSize
		{
			get
			{
				return this.m_DownloadedSize;
			}
			set
			{
				this.m_DownloadedSize = value;
			}
		}

		public FilePart(string name, long size, string sha1, string path)
		{
			this.m_Name = name;
			this.m_Size = size;
			this.m_SHA1 = sha1;
			this.m_Path = path;
			this.m_DownloadedSize = 0L;
		}

		public string URL(string manifestURL)
		{
			string str = manifestURL.Substring(0, manifestURL.LastIndexOf('/') + 1);
			return str + this.Name;
		}

		public bool Check()
		{
			bool result = false;
			if (!File.Exists(this.Path))
			{
				return false;
			}
			using (Stream stream = File.OpenRead(this.Path))
			{
				if (stream.Length != this.Size)
				{
					return false;
				}
				string a = SplitFile.CheckSum(stream);
				if (a == this.SHA1)
				{
					this.DownloadedSize = this.Size;
					return true;
				}
				return result;
			}
		}
	}

	public class Manifest
	{
		public class CheckFailed : Exception
		{
		}

		private List<FilePart> m_FileParts;

		private string m_FilePath;

		private long m_FileSize;

		public long Count
		{
			get
			{
				return this.m_FileParts.Count;
			}
		}

		public FilePart this[int i]
		{
			get
			{
				return this.m_FileParts[i];
			}
		}

		public long DownloadedSize
		{
			get
			{
				long num = 0L;
				foreach (FilePart filePart in this.m_FileParts)
				{
					num += filePart.DownloadedSize;
				}
				return num;
			}
		}

		public long FileSize
		{
			get
			{
				return this.m_FileSize;
			}
		}

		public Manifest(string filePath)
		{
			this.m_FileParts = new List<FilePart>();
			this.m_FilePath = filePath;
		}

		public bool Check()
		{
			foreach (FilePart filePart in this.m_FileParts)
			{
				if (!filePart.Check())
				{
					return false;
				}
			}
			return true;
		}

		public void Build()
		{
			using (StreamReader streamReader = new StreamReader(File.OpenRead(this.m_FilePath)))
			{
				string text;
				while ((text = streamReader.ReadLine()) != null)
				{
					string[] array = text.Split(' ');
					string text2 = array[0];
					long num = Convert.ToInt64(array[1]);
					string sha = array[2];
					string path = Path.Combine(Path.GetDirectoryName(this.m_FilePath), text2);
					FilePart filePart = new FilePart(text2, num, sha, path);
					if (filePart.Check())
					{
						filePart.DownloadedSize = filePart.Size;
					}
					this.m_FileParts.Add(filePart);
					this.m_FileSize += num;
				}
			}
		}

		public void Dump()
		{
			foreach (FilePart filePart in this.m_FileParts)
			{
				Logger.Info("{0} {1} {2}", filePart.Name, filePart.Size, filePart.SHA1);
			}
		}

		public string MakeFile()
		{
			int num = 16384;
			byte[] buffer = new byte[num];
			int num2 = 0;
			string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(this.m_FilePath);
			string directoryName = Path.GetDirectoryName(this.m_FilePath);
			string text = Path.Combine(directoryName, fileNameWithoutExtension);
			using (Stream stream2 = new FileStream(text, FileMode.Create, FileAccess.Write, FileShare.None))
			{
				foreach (FilePart filePart in this.m_FileParts)
				{
					using (Stream stream = new FileStream(filePart.Path, FileMode.Open, FileAccess.Read))
					{
						while ((num2 = stream.Read(buffer, 0, num)) > 0)
						{
							stream2.Write(buffer, 0, num2);
						}
					}
				}
				return text;
			}
		}

		public void DeleteFileParts()
		{
			foreach (FilePart filePart in this.m_FileParts)
			{
				File.Delete(filePart.Path);
			}
		}

		public int PercentDownloaded()
		{
			return Convert.ToInt32((double)this.DownloadedSize * 100.0 / (double)this.FileSize);
		}
	}

	private string m_ManifestURL;

	private string m_DirPath;

	private string m_UserGUID;

	private string m_UserAgent;

	private ProgressCb m_ProgressCb;

	private CompletedCb m_CompletedCb;

	private ExceptionCb m_ExceptionCb;

	private int m_NrWorkers;

	private SerialWorkQueue[] m_Workers;

	private bool m_WorkersStarted;

	private Manifest m_Manifest;

	private int m_PercentDownloaded;

	[CompilerGenerated]
	private static DownloadFileProgressCb _003C_003E9__CachedAnonymousMethodDelegate6;

	public SplitDownloader(string manifestURL, string dirPath, string userGUID, int nrWorkers)
	{
		this.m_ManifestURL = manifestURL;
		this.m_DirPath = dirPath;
		this.m_UserGUID = userGUID;
		this.m_UserAgent = string.Format("SplitDownloader {0}/{1}/{2}", "BlueStacks", "0.9.4.4078", this.m_UserGUID);
		this.m_NrWorkers = nrWorkers;
		this.m_Workers = new SerialWorkQueue[nrWorkers];
		for (int i = 0; i < this.m_NrWorkers; i++)
		{
			this.m_Workers[i] = new SerialWorkQueue();
		}
		this.m_WorkersStarted = false;
	}

	public void Download(ProgressCb progressCb, CompletedCb completedCb, ExceptionCb exceptionCb)
	{
		this.m_ProgressCb = progressCb;
		this.m_CompletedCb = completedCb;
		this.m_ExceptionCb = exceptionCb;
		try
		{
			this.m_Manifest = this.GetManifest();
			this.GetManifestFilePath();
			FilePart filePart = null;
			this.StartWorkers();
			this.m_ProgressCb(this.m_Manifest.PercentDownloaded());
			for (int i = 0; i < this.m_Manifest.Count; i++)
			{
				filePart = this.m_Manifest[i];
				SerialWorkQueue.Work work = this.MakeWork(filePart);
				this.m_Workers[i % this.m_NrWorkers].Enqueue(work);
			}
			this.StopAndWaitWorkers();
			if (!this.m_Manifest.Check())
			{
				throw new Manifest.CheckFailed();
			}
			string filePath = this.m_Manifest.MakeFile();
			this.m_Manifest.DeleteFileParts();
			this.m_CompletedCb(filePath);
		}
		catch (Exception ex)
		{
			Logger.Error(ex.ToString());
			this.m_ExceptionCb(ex);
		}
		finally
		{
			if (this.m_WorkersStarted)
			{
				this.StopAndWaitWorkers();
			}
		}
	}

	private void StartWorkers()
	{
		for (int i = 0; i < this.m_NrWorkers; i++)
		{
			this.m_Workers[i].Start();
		}
		this.m_WorkersStarted = true;
	}

	private void StopAndWaitWorkers()
	{
		for (int i = 0; i < this.m_NrWorkers; i++)
		{
			this.m_Workers[i].Stop();
		}
		for (int j = 0; j < this.m_NrWorkers; j++)
		{
			this.m_Workers[j].Join();
		}
		this.m_WorkersStarted = false;
	}

	private SerialWorkQueue.Work MakeWork(FilePart filePart)
	{
		return delegate
		{
			try
			{
				if (filePart.Check())
				{
					Logger.Info(filePart.Path + " is already downloaded");
				}
				else
				{
					this.DownloadFilePart(filePart);
				}
			}
			catch (Exception ex)
			{
				Logger.Error(ex.ToString());
			}
		};
	}

	private string GetManifestFilePath()
	{
		string fileName = Path.GetFileName(new Uri(this.m_ManifestURL).AbsolutePath);
		return Path.Combine(this.m_DirPath, fileName);
	}

	private Manifest GetManifest()
	{
		string manifestFilePath = this.GetManifestFilePath();
		Logger.Info("Downloading " + this.m_ManifestURL + " to " + manifestFilePath);
		bool downloaded = false;
		Exception capturedException = null;
		SplitDownloader.DownloadFile(this.m_ManifestURL, manifestFilePath, this.m_UserAgent, delegate(long downloadedSize, long totalSize)
		{
			Logger.Info("Downloaded (" + downloadedSize + " bytes) out of " + totalSize);
		}, delegate(string filePath)
		{
			downloaded = true;
			Logger.Info("Downloaded " + this.m_ManifestURL + " to " + filePath);
		}, delegate(Exception e)
		{
			downloaded = false;
			capturedException = e;
			Logger.Error(e.ToString());
		});
		if (!downloaded)
		{
			throw capturedException;
		}
		Manifest manifest = new Manifest(manifestFilePath);
		manifest.Build();
		return manifest;
	}

	private void DownloadFilePart(FilePart filePart)
	{
		string filePartURL = filePart.URL(this.m_ManifestURL);
		Logger.Info("Downloading " + filePartURL + " to " + filePart.Path);
		bool downloaded = false;
		Exception capturedException = null;
		SplitDownloader.DownloadFile(filePartURL, filePart.Path, this.m_UserAgent, delegate(long downloadedSize, long totalSize)
		{
			filePart.DownloadedSize = downloadedSize;
			if (this.m_PercentDownloaded != this.m_Manifest.PercentDownloaded())
			{
				this.m_ProgressCb(this.m_Manifest.PercentDownloaded());
			}
			this.m_PercentDownloaded = this.m_Manifest.PercentDownloaded();
		}, delegate
		{
			downloaded = true;
			Logger.Info("Downloaded " + filePartURL + " to " + filePart.Path);
		}, delegate(Exception e)
		{
			downloaded = false;
			capturedException = e;
			Logger.Error(e.ToString());
		});
		if (downloaded)
		{
			return;
		}
		throw capturedException;
	}

	private static void DownloadFile(string url, string filePath, string userAgent, DownloadFileProgressCb progressCb, DownloadFileCompletedCb completedCb, DownloadFileExceptionCb exceptionCb)
	{
		FileStream fileStream = null;
		HttpWebRequest httpWebRequest = null;
		HttpWebResponse httpWebResponse = null;
		Stream stream = null;
		bool flag = false;
		try
		{
			if (File.Exists(filePath))
			{
				File.Delete(filePath);
			}
			httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
			httpWebRequest.UserAgent = userAgent;
			httpWebRequest.KeepAlive = false;
			httpWebRequest.ReadWriteTimeout = 60000;
			httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
			long contentLength = httpWebResponse.ContentLength;
			stream = httpWebResponse.GetResponseStream();
			Logger.Warning("HTTP Response Header\nStatusCode: "+(int)httpWebResponse.StatusCode+"\n"+httpWebResponse.Headers);
			int num = 4096;
			byte[] buffer = new byte[num];
			int num2 = 0;
			long num3 = 0L;
			fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None);
			while ((num2 = stream.Read(buffer, 0, num)) > 0)
			{
				fileStream.Write(buffer, 0, num2);
				num3 += num2;
				progressCb(num3, contentLength);
			}
			if (contentLength != num3)
			{
				string message = "totalContentRead("+num3+") != contentLength("+contentLength+")";
				throw new Exception(message);
			}
			flag = true;
		}
		catch (Exception ex)
		{
			Logger.Error(ex.ToString());
			exceptionCb(ex);
		}
		finally
		{
			stream.Close();
			httpWebResponse.Close();
			if (fileStream != null)
			{
				fileStream.Flush();
				fileStream.Close();
			}
		}
		if (flag)
		{
			completedCb(filePath);
		}
	}
}
