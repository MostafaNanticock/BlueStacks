using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace BlueStacks.hyperDroid.Common
{
	public class HTTPUtils
	{
		public static RequestData ParseRequest(HttpListenerRequest req)
		{
			Logger.Info("In ParseRequest Wrapper");
			return HTTPUtils.ParseRequest(req, true);
		}

		public static RequestData ParseRequest(HttpListenerRequest req, bool printData)
		{
			Logger.Info("In ParseRequest");
			RequestData requestData = new RequestData();
			bool flag = false;
			string text = null;
			requestData.headers = req.Headers;
			string[] allKeys = requestData.headers.AllKeys;
			foreach (string name in allKeys)
			{
				if (requestData.headers[name].Contains("multipart"))
				{
					text = "--" + requestData.headers[name].Substring(requestData.headers[name].LastIndexOf("=") + 1);
					Logger.Debug("boundary: {0}", text);
					flag = true;
				}
			}
			requestData.queryString = req.QueryString;
			if (!req.HasEntityBody)
			{
				Logger.Info("no body data");
				return requestData;
			}
			Stream inputStream = req.InputStream;
			byte[] array = new byte[16384];
			MemoryStream memoryStream = new MemoryStream();
			int count;
			while ((count = inputStream.Read(array, 0, array.Length)) > 0)
			{
				memoryStream.Write(array, 0, count);
			}
			byte[] array2 = memoryStream.ToArray();
			memoryStream.Close();
			inputStream.Close();
			Logger.Debug("byte array size {0}", array2.Length);
			string @string = Encoding.UTF8.GetString(array2);
			if (!flag)
			{
				requestData.data = HttpUtility.ParseQueryString(@string);
				return requestData;
			}
			byte[] bytes = Encoding.UTF8.GetBytes(text);
			List<int> list = HTTPUtils.IndexOf(array2, bytes);
			for (int j = 0; j < list.Count - 1; j++)
			{
				Logger.Info("Creating part");
				int num = list[j];
				int num2 = list[j + 1];
				int num3 = num2 - num;
				byte[] array3 = new byte[num3];
				Logger.Debug("Start: {0}, End: {1}, Length: {2}", num, num2, num3);
				Logger.Debug("byteData length: {0}", array2.Length);
				Buffer.BlockCopy(array2, num, array3, 0, num3);
				Logger.Debug("bytePart length: {0}", array3.Length);
				string string2 = Encoding.UTF8.GetString(array3);
				Regex regex = new Regex("(?<=Content\\-Type:)(.*?)(?=\\r\\n)");
				Match match = regex.Match(string2);
				regex = new Regex("(?<=filename\\=\\\")(.*?)(?=\\\")");
				Match match2 = regex.Match(string2);
				regex = new Regex("(?<=name\\=\\\")(.*?)(?=\\\")");
				Match match3 = regex.Match(string2);
				string text2 = match3.Value.Trim();
				Logger.Info("Got name: {0}", text2);
				if (match.Success && match2.Success)
				{
					Logger.Debug("Found file");
					string text3 = match.Value.Trim();
					Logger.Debug("Got contenttype: {0}", text3);
					string text4 = match2.Value.Trim();
					Logger.Info("Got filename: {0}", text4);
					int num4 = string2.IndexOf("\r\n\r\n") + "\r\n\r\n".Length;
					Encoding.UTF8.GetBytes("\r\n" + text);
					int num5 = num3 - num4;
					byte[] array4 = new byte[num5];
					Logger.Debug("startindex: {0}, contentlength: {1}", num4, num5);
					Buffer.BlockCopy(array3, num4, array4, 0, num5);
					string text5 = Path.Combine(Strings.BstUserDataDir, text4);
					Stream stream = File.OpenWrite(text5);
					stream.Write(array4, 0, num5);
					stream.Close();
					requestData.files.Add(text2, text5);
				}
				else
				{
					Logger.Info("No file in this part");
					int num6 = string2.LastIndexOf("\r\n\r\n");
					string text6 = string2.Substring(num6, string2.Length - num6);
					text6 = text6.Trim();
					if (printData)
					{
						Logger.Info("Got value: {0}", text6);
					}
					else
					{
						Logger.Info("Value hidden");
					}
					requestData.data.Add(text2, text6);
				}
			}
			return requestData;
		}

		private static List<int> IndexOf(byte[] searchWithin, byte[] searchFor)
		{
			List<int> list = new List<int>();
			int startIndex = 0;
			int num = Array.IndexOf(searchWithin, searchFor[0], startIndex);
			Logger.Debug("boundary size = {0}", searchFor.Length);
			do
			{
				int num2 = 0;
				while (num + num2 < searchWithin.Length && searchWithin[num + num2] == searchFor[num2])
				{
					num2++;
					if (num2 == searchFor.Length)
					{
						list.Add(num);
						Logger.Debug("Got boundary postion: {0}", num);
						break;
					}
				}
				if (num + num2 > searchWithin.Length)
				{
					break;
				}
				num = Array.IndexOf(searchWithin, searchFor[0], num + num2);
			}
			while (num != -1);
			return list;
		}
	}
}
