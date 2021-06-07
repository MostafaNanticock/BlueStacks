using BlueStacks.hyperDroid.Common;
using BlueStacks.hyperDroid.Common.HTTP;
using CodeTitans.JSon;
using System;
using System.Collections.Generic;

namespace BlueStacks.hyperDroid.Cloud.Services
{
	internal class SMS : Service
	{
		public static string API_URL
		{
			get
			{
				return string.Format("{0}/api/{1}", Service.Host, "sms");
			}
		}

		public static string Route
		{
			get
			{
				return new Uri(SMS.API_URL).PathAndQuery;
			}
		}

		public static IJSonObject ReadSMS(string key, string secret)
		{
			Dictionary<string, string> dictionary = Auth.CreateHeaders(key);
			string[] paramsOrder = new string[0];
			Dictionary<string, string> data = new Dictionary<string, string>();
			string value = Auth.Sign("GET", SMS.Route + "/readsms", data, dictionary, paramsOrder, secret);
			dictionary.Add("X-Bst-Auth-Sign", value);
			bool gzip = true;
			Logger.Debug("SMS: Host " + SMS.API_URL + "/readsms");
			string input = Client.Get(SMS.API_URL + "/readsms", dictionary, gzip);
			IJSonReader iJSonReader = new JSonReader();
			return iJSonReader.ReadAsJSonObject(input);
		}
	}
}
