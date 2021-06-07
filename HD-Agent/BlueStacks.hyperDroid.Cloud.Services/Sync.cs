using BlueStacks.hyperDroid.Common;
using BlueStacks.hyperDroid.Common.HTTP;
using CodeTitans.JSon;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;

namespace BlueStacks.hyperDroid.Cloud.Services
{
	internal class Sync : Service
	{
		public static string API_URL
		{
			get
			{
				return string.Format("{0}/api/{1}", Service.Host, "sync");
			}
		}

		public static string API_V2_URL
		{
			get
			{
				return string.Format("{0}/api/v2/{1}", Service.Host, "sync");
			}
		}

		public static string Route
		{
			get
			{
				return new Uri(Sync.API_URL).PathAndQuery;
			}
		}

		public static string RouteV2
		{
			get
			{
				return new Uri(Sync.API_V2_URL).PathAndQuery;
			}
		}

		public static IJSonObject Echo(string param1, string param2, string key, string secret)
		{
			Dictionary<string, string> dictionary = Auth.CreateHeaders(key);
			string[] paramsOrder = new string[2]
			{
				"param1",
				"param2"
			};
			Dictionary<string, string> dictionary2 = new Dictionary<string, string>();
			dictionary2["param1"] = param1;
			dictionary2["param2"] = param2;
			string value = Auth.Sign("POST", Sync.Route + "/echo", dictionary2, dictionary, paramsOrder, secret);
			dictionary.Add("X-Bst-Auth-Sign", value);
			bool gzip = true;
			string input = Client.Post(Sync.API_URL + "/echo", dictionary2, dictionary, gzip);
			IJSonReader iJSonReader = new JSonReader();
			return iJSonReader.ReadAsJSonObject(input);
		}

		public static void EchoAsync(string param1, string param2, string key, string secret, OnSuccess success, OnFailed failed)
		{
			BackgroundWorker backgroundWorker = Service.CreateWorkerAsync(success, failed);
			backgroundWorker.DoWork += delegate(object o, DoWorkEventArgs args)
			{
				args.Result = Sync.Echo(param1, param2, key, secret);
			};
			backgroundWorker.RunWorkerAsync();
		}

		public static IJSonObject AppInfo(string key, string secret)
		{
			throw new NotImplementedException();
		}

		public static void AppInfoAsync()
		{
		}

		public static IJSonObject AppList(string key, string secret)
		{
			Dictionary<string, string> dictionary = Auth.CreateHeaders(key);
			string[] paramsOrder = new string[0];
			Dictionary<string, string> data = new Dictionary<string, string>();
			string value = Auth.Sign("POST", Sync.Route + "/app/list", data, dictionary, paramsOrder, secret);
			dictionary.Add("X-Bst-Auth-Sign", value);
			bool gzip = true;
			string input = Client.Post(Sync.API_URL + "/app/list", data, dictionary, gzip);
			IJSonReader iJSonReader = new JSonReader();
			return iJSonReader.ReadAsJSonObject(input);
		}

		public static IJSonObject AppList2(string key, string secret)
		{
			Dictionary<string, string> dictionary = Auth.CreateHeaders(key);
			string[] paramsOrder = new string[0];
			Dictionary<string, string> data = new Dictionary<string, string>();
			string value = Auth.Sign("GET", Sync.RouteV2 + "/app/list", data, dictionary, paramsOrder, secret);
			dictionary.Add("X-Bst-Auth-Sign", value);
			bool gzip = true;
			string input = Client.Get(Sync.API_V2_URL + "/app/list", dictionary, gzip);
			IJSonReader iJSonReader = new JSonReader();
			return iJSonReader.ReadAsJSonObject(input);
		}

		public static void AppListAsync()
		{
		}

		public static IJSonObject UploadApp()
		{
			throw new NotImplementedException();
		}

		public static void UploadAppAsync()
		{
		}

		public static IJSonObject DestroyApp()
		{
			throw new NotImplementedException();
		}

		public static void DestroyAppAsync()
		{
		}

		public static void DownloadApp(string srcUrl, string dest, string key, string secret)
		{
			Dictionary<string, string> dictionary = Auth.CreateHeaders(key);
			string[] paramsOrder = new string[0];
			Dictionary<string, string> data = new Dictionary<string, string>();
			string pathAndQuery = new Uri(srcUrl).PathAndQuery;
			string value = Auth.Sign("GET", pathAndQuery, data, dictionary, paramsOrder, secret);
			dictionary.Add("X-Bst-Auth-Sign", value);
			using (WebClient webClient = new WebClient())
			{
				Logger.Debug("URI of proxy = " + webClient.Proxy.GetProxy(new Uri(Service.Host)));
				foreach (KeyValuePair<string, string> item in dictionary)
				{
					webClient.Headers.Set(item.Key, item.Value);
				}
				webClient.Headers.Add("User-Agent", BlueStacks.hyperDroid.Common.Utils.UserAgent(User.GUID));
				webClient.DownloadFile(srcUrl, dest);
			}
		}

		public static void DownloadAppAsync()
		{
		}

		public static IJSonObject UploadAppIcon()
		{
			throw new NotImplementedException();
		}

		public static void UploadAppIconAsync()
		{
		}

		public static IJSonObject DownloadAppIcon()
		{
			throw new NotImplementedException();
		}

		public static void DownloadAppIconAsync()
		{
		}
	}
}
