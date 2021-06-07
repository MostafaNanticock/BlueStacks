using BlueStacks.hyperDroid.Device;
using CodeTitans.JSon;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Web;

namespace BlueStacks.hyperDroid.Common.HTTP
{
	internal class Client
	{
		public const int TIMEOUT_10SECONDS = 10000;

		public static string Encode(Dictionary<string, string> data)
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (KeyValuePair<string, string> datum in data)
			{
				stringBuilder.AppendFormat("{0}={1}&", datum.Key, HttpUtility.UrlEncode(datum.Value));
			}
			char[] trimChars = new char[1]
			{
				'&'
			};
			return stringBuilder.ToString().TrimEnd(trimChars);
		}

		public static string Get(string url, Dictionary<string, string> headers, bool gzip)
		{
			return Client.Get(url, headers, gzip, 0);
		}

		public static string Get(string url, Dictionary<string, string> headers, bool gzip, int timeout)
		{
			HttpWebRequest httpWebRequest = WebRequest.Create(url) as HttpWebRequest;
			httpWebRequest.Method = "GET";
			if (timeout != 0)
			{
				httpWebRequest.Timeout = timeout;
			}
			if (gzip)
			{
				httpWebRequest.AutomaticDecompression = DecompressionMethods.GZip;
				httpWebRequest.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip");
			}
			if (headers != null)
			{
				foreach (KeyValuePair<string, string> header in headers)
				{
					httpWebRequest.Headers.Set(header.Key, header.Value);
				}
			}
			httpWebRequest.Headers.Set("x_oem", Profile.OEM);
			httpWebRequest.UserAgent = BlueStacks.hyperDroid.Common.Utils.UserAgent(User.GUID);
			Uri uri = new Uri(url);
			if (!uri.Host.Contains("localhost") && !uri.Host.Contains("127.0.0.1"))
			{
				IWebProxy systemWebProxy = WebRequest.GetSystemWebProxy();
				systemWebProxy.Credentials = CredentialCache.DefaultCredentials;
				httpWebRequest.Proxy = systemWebProxy;
				Logger.Debug("URI of proxy = " + httpWebRequest.Proxy.GetProxy(uri));
			}
			string text = null;
			using (HttpWebResponse httpWebResponse = httpWebRequest.GetResponse() as HttpWebResponse)
			{
				using (Stream stream = httpWebResponse.GetResponseStream())
				{
					using (StreamReader streamReader = new StreamReader(stream, Encoding.UTF8))
					{
						return streamReader.ReadToEnd();
					}
				}
			}
		}

		public static string Post(string url, Dictionary<string, string> data, Dictionary<string, string> headers, bool gzip)
		{
			return Client.Post(url, data, headers, gzip, 0);
		}

		public static string Post(string url, Dictionary<string, string> data, Dictionary<string, string> headers, bool gzip, int timeout)
		{
			HttpWebRequest httpWebRequest = WebRequest.Create(url) as HttpWebRequest;
			httpWebRequest.Method = "POST";
			if (timeout != 0)
			{
				httpWebRequest.Timeout = timeout;
			}
			if (gzip)
			{
				httpWebRequest.AutomaticDecompression = DecompressionMethods.GZip;
				httpWebRequest.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip");
			}
			if (headers != null)
			{
				foreach (KeyValuePair<string, string> header in headers)
				{
					httpWebRequest.Headers.Set(header.Key, header.Value);
				}
			}
			httpWebRequest.Headers.Set("x_oem", Profile.OEM);
			if (data == null)
			{
				data = new Dictionary<string, string>();
			}
			byte[] bytes = Encoding.UTF8.GetBytes(Client.Encode(data));
			httpWebRequest.ContentType = "application/x-www-form-urlencoded";
			httpWebRequest.ContentLength = bytes.Length;
			httpWebRequest.UserAgent = BlueStacks.hyperDroid.Common.Utils.UserAgent(User.GUID);
			Uri uri = new Uri(url);
			if (!uri.Host.Contains("localhost") && !uri.Host.Contains("127.0.0.1"))
			{
				IWebProxy systemWebProxy = WebRequest.GetSystemWebProxy();
				systemWebProxy.Credentials = CredentialCache.DefaultCredentials;
				httpWebRequest.Proxy = systemWebProxy;
				Logger.Debug("URI of proxy = " + httpWebRequest.Proxy.GetProxy(uri));
			}
			string text = null;
			using (Stream stream = httpWebRequest.GetRequestStream())
			{
				stream.Write(bytes, 0, bytes.Length);
				using (HttpWebResponse httpWebResponse = httpWebRequest.GetResponse() as HttpWebResponse)
				{
					using (Stream stream2 = httpWebResponse.GetResponseStream())
					{
						using (StreamReader streamReader = new StreamReader(stream2, Encoding.UTF8))
						{
							return streamReader.ReadToEnd();
						}
					}
				}
			}
		}

		public static string PostWithRetries(string url, Dictionary<string, string> data, Dictionary<string, string> headers, bool gzip, int retries, int sleepTimeMSecs)
		{
			return Client.PostWithRetries(url, data, headers, gzip, retries, sleepTimeMSecs, 0);
		}

		public static string PostWithRetries(string url, Dictionary<string, string> data, Dictionary<string, string> headers, bool gzip, int retries, int sleepTimeMSecs, int timeout)
		{
			string result = null;
			int num = retries;
			while (num > 0)
			{
				try
				{
					result = Client.Post(url, data, null, false, timeout);
					return result;
				}
				catch (Exception ex)
				{
					if (num == retries)
					{
						RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("Software\\BlueStacks");
						string path = (string)registryKey.GetValue("InstallDir");
						if (!BlueStacks.hyperDroid.Common.Utils.IsGlHotAttach())
						{
							BlueStacks.hyperDroid.Common.Utils.StartHiddenFrontend();
						}
						Process.Start(Path.Combine(path, "HD-Agent.exe"));
					}
					Logger.Error("Exception when posting");
					Logger.Error(ex.Message);
				}
				num--;
				Thread.Sleep(sleepTimeMSecs);
			}
			return result;
		}

		public static string HTTPGaeFileUploader(string url, Dictionary<string, string> data, Dictionary<string, string> headers, string filepath, string contentType, bool gzip)
		{
			if (filepath == null)
			{
				return Client.Post(url, data, headers, gzip);
			}
			string input = Client.Get(url, null, false);
			JSonReader jSonReader = new JSonReader();
			IJSonObject iJSonObject = jSonReader.ReadAsJSonObject(input);
			string url2 = null;
			if (iJSonObject["success"].BooleanValue)
			{
				url2 = iJSonObject["url"].StringValue;
			}
			return Client.HttpUploadFile(url2, filepath, "file", contentType, headers, data);
		}

		public static string HttpUploadFile(string url, string file, string paramName, string contentType, Dictionary<string, string> headers, Dictionary<string, string> data)
		{
			Logger.Info("Uploading {file} to {url}");
			string str = "---------------------------" + DateTime.Now.Ticks.ToString("x");
			byte[] bytes = Encoding.ASCII.GetBytes("\r\n--" + str + "\r\n");
			Uri uri = new Uri(url);
			Logger.Info("Resolving proxy");
			HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
			httpWebRequest.ContentType = "multipart/form-data; boundary=" + str;
			httpWebRequest.Method = "POST";
			httpWebRequest.KeepAlive = true;
			httpWebRequest.UserAgent = BlueStacks.hyperDroid.Common.Utils.UserAgent(User.GUID);
			if (!uri.Host.Contains("localhost") && !uri.Host.Contains("127.0.0.1"))
			{
				IWebProxy systemWebProxy = WebRequest.GetSystemWebProxy();
				systemWebProxy.Credentials = CredentialCache.DefaultCredentials;
				httpWebRequest.Proxy = systemWebProxy;
				Logger.Debug("URI of proxy = " + httpWebRequest.Proxy.GetProxy(uri));
			}
			if (headers != null)
			{
				foreach (KeyValuePair<string, string> header in headers)
				{
					httpWebRequest.Headers.Set(header.Key, header.Value);
				}
			}
			httpWebRequest.Headers.Set("x_oem", Profile.OEM);
			if (data == null)
			{
				data = new Dictionary<string, string>();
			}
			Logger.Info("Making request");
			Stream requestStream = httpWebRequest.GetRequestStream();
			string format = "Content-Disposition: form-data; KeyName=\"{0}\"\r\n\r\n{1}";
			Logger.Info("Reading data");
			foreach (KeyValuePair<string, string> datum in data)
			{
				requestStream.Write(bytes, 0, bytes.Length);
				string s = string.Format(format, datum.Key, datum.Value);
				byte[] bytes2 = Encoding.UTF8.GetBytes(s);
				requestStream.Write(bytes2, 0, bytes2.Length);
			}
			requestStream.Write(bytes, 0, bytes.Length);
			Logger.Info("Data read");
			string format2 = "Content-Disposition: form-data; KeyName=\"{0}\"; filename=\"{1}\"\r\nContent-Type: {2}\r\n\r\n";
			string s2 = string.Format(format2, paramName, file, contentType);
			byte[] bytes3 = Encoding.UTF8.GetBytes(s2);
			requestStream.Write(bytes3, 0, bytes3.Length);
			Logger.Info("Reading file");
			string path = Environment.ExpandEnvironmentVariables("%TEMP%");
			path = Path.Combine(path, Path.GetFileName(file)) + "_bst";
			File.Copy(file, path);
			FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
			byte[] array = new byte[4096];
			int num = 0;
			while ((num = fileStream.Read(array, 0, array.Length)) != 0)
			{
				requestStream.Write(array, 0, num);
			}
			fileStream.Close();
			File.Delete(path);
			Logger.Info("File read");
			byte[] bytes4 = Encoding.ASCII.GetBytes("\r\n--" + str + "--\r\n");
			requestStream.Write(bytes4, 0, bytes4.Length);
			requestStream.Close();
			string text = null;
			WebResponse webResponse = null;
			try
			{
				Logger.Info("Sending request");
				webResponse = httpWebRequest.GetResponse();
				Stream responseStream = webResponse.GetResponseStream();
				StreamReader streamReader = new StreamReader(responseStream);
				text = streamReader.ReadToEnd();
				Logger.Info("File uploaded, server response is: {text}");
				return text;
			}
			catch (Exception ex)
			{
				Logger.Error("Error uploading file", ex);
				if (webResponse != null)
				{
					webResponse.Close();
					webResponse = null;
					return text;
				}
				return text;
			}
			finally
			{
				httpWebRequest = null;
			}
		}

		public static void LogHeaders(WebHeaderCollection h)
		{
			for (int i = 0; i < h.Count; i++)
			{
				Logger.Info("{0} = {1}", h.Keys[i], ((NameValueCollection)h)[i]);
			}
		}
	}
}
