using BlueStacks.hyperDroid.Cloud.Services;
using BlueStacks.hyperDroid.Common;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;

internal class Downloader
{
    public delegate void UpdateProgressCallback(int percent);

    public delegate void DownloadCompletedCallback(string filePath);

    public delegate void ExceptionCallback(Exception e);

    private delegate void ProgressCallback();

    private class Range
    {
        private long m_From;

        private long m_To;

        public long From
        {
            get
            {
                return this.m_From;
            }
            set
            {
                this.m_From = value;
            }
        }

        public long To
        {
            get
            {
                return this.m_To;
            }
        }

        public long Length
        {
            get
            {
                return this.m_To - this.m_From + 1;
            }
        }

        public Range(long from, long to)
        {
            this.m_From = from;
            this.m_To = to;
        }
    }

    private class WorkerException : Exception
    {
        public WorkerException(string msg, Exception e)
            : base(msg, e)
        {
        }
    }

    private class Worker
    {
        private int m_Id;

        private string m_URL;

        private string m_PayloadName;

        private Range m_Range;

        private int m_PercentComplete;

        private ProgressCallback m_ProgressCallback;

        private Exception m_Exception;

        public int Id
        {
            get
            {
                return this.m_Id;
            }
        }

        public string PartFileName
        {
            get
            {
                return Downloader.MakePartFileName(this.m_PayloadName, this.m_Id);
            }
        }

        public Range Range
        {
            get
            {
                return this.m_Range;
            }
        }

        public string URL
        {
            get
            {
                return this.m_URL;
            }
        }

        public int PercentComplete
        {
            get
            {
                return this.m_PercentComplete;
            }
            set
            {
                this.m_PercentComplete = value;
                this.ProgressCallback();
            }
        }

        public ProgressCallback ProgressCallback
        {
            get
            {
                return this.m_ProgressCallback;
            }
            set
            {
                this.m_ProgressCallback = value;
            }
        }

        public Exception Exception
        {
            get
            {
                return this.m_Exception;
            }
            set
            {
                this.m_Exception = value;
            }
        }

        public Worker(int id, string url, string payloadName, Range range)
        {
            this.m_Id = id;
            this.m_URL = url;
            this.m_PayloadName = payloadName;
            this.m_Range = range;
        }
    }

    private class PayloadInfo
    {
        private bool m_SupportsRangeRequest;

        private long m_Size;

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

        public bool SupportsRangeRequest
        {
            get
            {
                return this.m_SupportsRangeRequest;
            }
        }

        public PayloadInfo(bool supportsRangeRequest, long size)
        {
            this.m_SupportsRangeRequest = supportsRangeRequest;
            this.m_Size = size;
        }
    }

    public static void Download(int nrWorkers, string url, string fileName, UpdateProgressCallback updateProgressCb, DownloadCompletedCallback downloadedCb, ExceptionCallback exceptionCb)
    {
        Logger.Info("Downloading {0} to: {1}", url, fileName);
        string filePath = fileName;
        try
        {
            string directoryName = Path.GetDirectoryName(Application.ExecutablePath);
            string text = Path.Combine(directoryName, Path.GetFileName(fileName));
            if (File.Exists(text))
            {
                Logger.Info("{0} already downloaded to {1}", url, text);
                filePath = text;
            }
            else
            {
                PayloadInfo remotePayloadInfo;
                try
                {
                    remotePayloadInfo = Downloader.GetRemotePayloadInfo(url);
                }
                catch (Exception)
                {
                    Logger.Error("Unable to send to " + url);
                    if (url.Contains(Service.Host))
                    {
                        url = url.Replace(Service.Host, Service.Host2);
                        Logger.Info("Trying " + url);
                        remotePayloadInfo = Downloader.GetRemotePayloadInfo(url);
                        goto end_IL_008d;
                    }
                    throw;
                end_IL_008d: ;
                }
                if (File.Exists(fileName))
                {
                    if (Downloader.IsPayloadOk(fileName, remotePayloadInfo.Size))
                    {
                        Logger.Info(url + " already downloaded");
                        goto IL_019a;
                    }
                    File.Delete(fileName);
                }
                if (!remotePayloadInfo.SupportsRangeRequest)
                {
                    nrWorkers = 1;
                }
                List<KeyValuePair<Thread, Worker>> workers = Downloader.MakeWorkers(nrWorkers, url, fileName, remotePayloadInfo.Size);
                int prevAverageTotalPercent = 0;
                Downloader.StartWorkers(workers, delegate
                {
                    int num = 0;
                    int num2 = 0;
                    foreach (KeyValuePair<Thread, Worker> item in workers)
                    {
                        num += item.Value.PercentComplete;
                    }
                    num2 = num / workers.Count;
                    if (num2 != prevAverageTotalPercent)
                    {
                        updateProgressCb(num2);
                    }
                    prevAverageTotalPercent = num2;
                });
                Downloader.WaitForWorkers(workers);
                Downloader.MakePayload(nrWorkers, fileName);
                if (!Downloader.IsPayloadOk(fileName, remotePayloadInfo.Size))
                {
                    string text2 = "Downloaded Prebundled not of the correct size";
                    Logger.Info(text2);
                    File.Delete(fileName);
                    throw new Exception(text2);
                }
                Logger.Info("File downloaded correctly");
                Downloader.DeletePayloadParts(nrWorkers, fileName);
            }
            goto IL_019a;
        IL_019a:
            downloadedCb(filePath);
        }
        catch (Exception ex2)
        {
            Logger.Error("Exception in Download. err: " + ex2.ToString());
            exceptionCb(ex2);
        }
    }

    public static string MakePartFileName(string fileName, int id)
    {
        return fileName + "_part_" + id;
    }

    private static long GetSizeFromContentRange(HttpWebResponse res)
    {
        string text = ((NameValueCollection)res.Headers)["Content-Range"];
        char[] separator = new char[1]
		{
			'/'
		};
        string[] array = text.Split(separator);
        return Convert.ToInt64(array[array.Length - 1]);
    }

    private static PayloadInfo GetRemotePayloadInfo(string url)
    {
        HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
        httpWebRequest.Method = "Head";
        HttpWebResponse httpWebResponse = null;
        string text = null;
        PayloadInfo result = null;
        try
        {
            Downloader.Add64BitRange(httpWebRequest, 0L, 0L);
            httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            text = Downloader.GetHTTPResponseHeaders(httpWebResponse);
            Logger.Warning(text);
            if (httpWebResponse.StatusCode == HttpStatusCode.PartialContent)
            {
                long sizeFromContentRange = Downloader.GetSizeFromContentRange(httpWebResponse);
                result = new PayloadInfo(true, sizeFromContentRange);
            }
            else if (httpWebResponse.StatusCode == HttpStatusCode.OK)
            {
                result = new PayloadInfo(false, httpWebResponse.ContentLength);
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex.ToString());
            throw;
        }
        httpWebResponse.Close();
        return result;
    }

    private static List<KeyValuePair<Thread, Worker>> MakeWorkers(int nrWorkers, string url, string payloadFileName, long payloadSize)
    {
        long num = payloadSize / nrWorkers;
        List<KeyValuePair<Thread, Worker>> list = new List<KeyValuePair<Thread, Worker>>();
        for (int i = 0; i < nrWorkers; i++)
        {
            long from = i * num;
            long num2 = -1L;
            num2 = ((i != nrWorkers - 1) ? ((i + 1) * num - 1) : ((i + 1) * num + payloadSize % nrWorkers - 1));
            Thread thread = new Thread(Downloader.DoWork);
            thread.IsBackground = true;
            Worker value = new Worker(i, url, payloadFileName, new Range(from, num2));
            KeyValuePair<Thread, Worker> item = new KeyValuePair<Thread, Worker>(thread, value);
            list.Add(item);
        }
        return list;
    }

    private static void StartWorkers(List<KeyValuePair<Thread, Worker>> workers, ProgressCallback progressCallback)
    {
        foreach (KeyValuePair<Thread, Worker> worker in workers)
        {
            worker.Value.ProgressCallback = progressCallback;
            worker.Key.Start(worker.Value);
        }
    }

    private static void MakePayload(int nrWorkers, string payloadName)
    {
        Stream stream = new FileStream(payloadName, FileMode.Create, FileAccess.Write, FileShare.None);
        int num = 16384;
        byte[] buffer = new byte[num];
        int num2 = 0;
        for (int i = 0; i < nrWorkers; i++)
        {
            string path = Downloader.MakePartFileName(payloadName, i);
            Stream stream2 = new FileStream(path, FileMode.Open, FileAccess.Read);
            while ((num2 = stream2.Read(buffer, 0, num)) > 0)
            {
                stream.Write(buffer, 0, num2);
            }
            stream2.Close();
        }
        stream.Flush();
        stream.Close();
    }

    private static void DeletePayloadParts(int nrParts, string payloadName)
    {
        for (int i = 0; i < nrParts; i++)
        {
            string path = Downloader.MakePartFileName(payloadName, i);
            File.Delete(path);
        }
    }

    private static string GetHTTPResponseHeaders(HttpWebResponse res)
    {
        string str = "HTTP Response Headers\n";
        str += "StatusCode: " + (int)res.StatusCode + "\n";
        return str + res.Headers;
    }

    public static void DoWork(object data)
    {
        Worker worker = (Worker)data;
        Range range = worker.Range;
        Stream stream = null;
        HttpWebRequest httpWebRequest = null;
        HttpWebResponse httpWebResponse = null;
        Stream stream2 = null;
        try
        {
            Logger.Info("WorkerId {0} range.From = {1}, range.To = {2}", worker.Id, range.From, range.To);
            httpWebRequest = (HttpWebRequest)WebRequest.Create(worker.URL);
            httpWebRequest.KeepAlive = false;
            if (File.Exists(worker.PartFileName))
            {
                stream = new FileStream(worker.PartFileName, FileMode.Append, FileAccess.Write, FileShare.None);
                if (stream.Length == range.Length)
                {
                    worker.PercentComplete = 100;
                    Logger.Info("WorkerId {0} already downloaded", worker.Id);
                    return;
                }
                worker.PercentComplete = (int)(stream.Length * 100 / range.Length);
                Logger.Info("WorkerId {0} Resuming from range.From = {1}, range.To = {2}", worker.Id, range.From + stream.Length, range.To);
                Downloader.Add64BitRange(httpWebRequest, range.From + stream.Length, range.To);
            }
            else
            {
                worker.PercentComplete = 0;
                stream = new FileStream(worker.PartFileName, FileMode.Create, FileAccess.Write, FileShare.None);
                Downloader.Add64BitRange(httpWebRequest, range.From, range.To);
            }
            httpWebRequest.ReadWriteTimeout = 60000;
            httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            long contentLength = httpWebResponse.ContentLength;
            stream2 = httpWebResponse.GetResponseStream();
            int num = 4096;
            byte[] buffer = new byte[num];
            int num2 = 0;
            long num3 = 0L;
            string str = "WorkerId " + worker.Id + "\n";
            str += Downloader.GetHTTPResponseHeaders(httpWebResponse);
            Logger.Warning(str);
            while ((num2 = stream2.Read(buffer, 0, num)) > 0)
            {
                stream.Write(buffer, 0, num2);
                num3 += num2;
                worker.PercentComplete = (int)(stream.Length * 100 / range.Length);
            }
            if (contentLength != num3)
            {
                string message = "totalContentRead(" + num3 + ") != contentLength(" + contentLength + ")";
                throw new Exception(message);
            }
        }
        catch (Exception ex)
        {
            Exception ex3 = worker.Exception = ex;
            Logger.Error(ex3.ToString());
            return;
        }
        finally
        {
            stream2.Close();
            httpWebResponse.Close();
            if (stream != null)
            {
                stream.Flush();
                stream.Close();
            }
        }
        Logger.Info("WorkerId {0} Finished", worker.Id);
    }

    private static bool IsPayloadOk(string payloadFileName, long remoteSize)
    {
        long length = new FileInfo(payloadFileName).Length;
        Logger.Info("payloadSize = " + length + " remoteSize = " + remoteSize);
        return length == remoteSize;
    }

    private static void WaitForWorkers(List<KeyValuePair<Thread, Worker>> workers)
    {
        foreach (KeyValuePair<Thread, Worker> worker in workers)
        {
            worker.Key.Join();
        }
        foreach (KeyValuePair<Thread, Worker> worker2 in workers)
        {
            if (worker2.Value.Exception != null)
            {
                throw new WorkerException(worker2.Value.Exception.Message, worker2.Value.Exception);
            }
        }
    }

    private static void Add64BitRange(HttpWebRequest req, long start, long end)
    {
        MethodInfo method = typeof(WebHeaderCollection).GetMethod("AddWithoutValidate", BindingFlags.Instance | BindingFlags.NonPublic);
        string text = "Range";
        string text2 = "bytes=" + start + "-" + end;
        method.Invoke(req.Headers, new object[2]
		{
			text,
			text2
		});
    }
}
