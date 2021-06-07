using BlueStacks.hyperDroid.Device;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace BlueStacks.hyperDroid.Common
{
	internal class GoogleAnalytics
	{
		public class Event
		{
			private string m_Category;

			private string m_Action;

			private string m_Label;

			private int m_Value;

			public string Category
			{
				get
				{
					return this.m_Category;
				}
			}

			public string Action
			{
				get
				{
					return this.m_Action;
				}
			}

			public string Label
			{
				get
				{
					return this.m_Label;
				}
			}

			public int Value
			{
				get
				{
					return this.m_Value;
				}
			}

			public Event(string category, string action, string label, int value)
			{
				this.m_Category = category;
				this.m_Action = action;
				this.m_Label = label;
				this.m_Value = value;
			}
		}

		private const int SM_CXSCREEN = 0;

		private const int SM_CYSCREEN = 1;

		private static string s_AccountName = "UA-32186883-1";

		private static string s_PageDomain = Strings.ChannelsUrl;

		private static string s_UserAgent = string.Format("Mozilla/5.0 (compatible; MSIE {0}; Windows NT {1}.{2})", "0.7.18.921", Environment.OSVersion.Version.Major, Environment.OSVersion.Version.Minor);

		private static string s_Locale = CultureInfo.CurrentCulture.Name;

		private static int s_ScreenWidth = -1;

		private static int s_ScreenHeight = -1;

		private static int ScreenWidth
		{
			get
			{
				if (GoogleAnalytics.s_ScreenWidth == -1)
				{
					GoogleAnalytics.s_ScreenWidth = GoogleAnalytics.GetSystemMetrics(0);
				}
				return GoogleAnalytics.s_ScreenWidth;
			}
		}

		private static int ScreenHeight
		{
			get
			{
				if (GoogleAnalytics.s_ScreenHeight == -1)
				{
					GoogleAnalytics.s_ScreenHeight = GoogleAnalytics.GetSystemMetrics(1);
				}
				return GoogleAnalytics.s_ScreenHeight;
			}
		}

		private static int DomainHash
		{
			get
			{
				int num = 1;
				int num2 = 0;
				num = 0;
				for (int num3 = GoogleAnalytics.s_PageDomain.Length - 1; num3 >= 0; num3--)
				{
					char c = char.Parse(GoogleAnalytics.s_PageDomain.Substring(num3, 1));
					int num4 = c;
					num = (num << 6 & 0xFFFFFFF) + num4 + (num4 << 14);
					num2 = (num & 0xFE00000);
					num = ((num2 != 0) ? (num ^ num2 >> 21) : num);
				}
				return num;
			}
		}

		private static string FakeUtmcCookieString
		{
			get
			{
				string text = "(direct)";
				string text2 = "(direct)";
				string text3 = "(none)";
				int num = 2;
				int num2 = GoogleAnalytics.ToUnixTimestampSecs(DateTime.Now);
				string arg = "{GoogleAnalytics.DomainHash}.{int.Parse(RandomGenerator.Next(1000000000).ToString())}.{num2}.{num2}.{num2}.{num}";
				string arg2 = string.Format("{0}.{1}.{2}.{3}.utmcsr={4}|utmccn={5}|utmcmd={6}", GoogleAnalytics.DomainHash, num2, "1", "1", text, text2, text3);
				return Uri.EscapeDataString("__utma={arg};+__utmz={arg2};");
			}
		}

		[DllImport("user32.dll")]
		private static extern int GetSystemMetrics(int which);

		private static string OSName()
		{
			if (Utils.IsOSWin8())
			{
				return "Win8";
			}
			if (Utils.IsOSWin7())
			{
				return "Win7";
			}
			if (Utils.IsOSWinXP())
			{
				return "WinXP";
			}
			if (Utils.IsOSVista())
			{
				return "Vista";
			}
			return "None";
		}

		public static int ToUnixTimestampSecs(DateTime value)
		{
			return (int)(value - new DateTime(1970, 1, 1, 0, 0, 0, 0).ToLocalTime()).TotalSeconds;
		}

		private static void SendTrackEvent(string pageTitle, string pageURL, Event evt, string accountName)
		{
			try
			{
				List<KeyValuePair<string, string>> list = new List<KeyValuePair<string, string>>();
				list.Add(new KeyValuePair<string, string>("utmwv", "5.2.4"));
				list.Add(new KeyValuePair<string, string>("utmn", RandomGenerator.Next(1000000000).ToString()));
				list.Add(new KeyValuePair<string, string>("utmhn", GoogleAnalytics.s_PageDomain));
				list.Add(new KeyValuePair<string, string>("utmcs", "UTF-8"));
				list.Add(new KeyValuePair<string, string>("utmul", GoogleAnalytics.s_Locale));
				list.Add(new KeyValuePair<string, string>("utmsr", "{GoogleAnalytics.ScreenWidth}x{GoogleAnalytics.ScreenHeight}"));
				list.Add(new KeyValuePair<string, string>("utmsc", Profile.OEM));
				list.Add(new KeyValuePair<string, string>("utmje", "0"));
				list.Add(new KeyValuePair<string, string>("utmfl", GoogleAnalytics.OSName()));
				list.Add(new KeyValuePair<string, string>("utmdt", Uri.EscapeDataString(pageTitle)));
				list.Add(new KeyValuePair<string, string>("utmhid", RandomGenerator.Next(1000000000).ToString()));
				list.Add(new KeyValuePair<string, string>("utmr", "-"));
				list.Add(new KeyValuePair<string, string>("utmp", pageURL));
				list.Add(new KeyValuePair<string, string>("utmac", accountName));
				list.Add(new KeyValuePair<string, string>("utmcc", GoogleAnalytics.FakeUtmcCookieString));
				list.Add(new KeyValuePair<string, string>("utmt", "event"));
				string stringToEscape = "5({evt.Category}*{evt.Action}*{evt.Label})({evt.Value})";
				list.Add(new KeyValuePair<string, string>("utme", Uri.EscapeDataString(stringToEscape)));
				StringBuilder stringBuilder = new StringBuilder();
				foreach (KeyValuePair<string, string> item in list)
				{
					stringBuilder.Append("{item.Key}={item.Value}&");
				}
				string text = stringBuilder.ToString();
				text = text.Substring(0, text.Length - 1);
				string text2 = "https://www.google-analytics.com/__utm.gif?{text}";
				HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(new Uri(text2));
				IWebProxy systemWebProxy = WebRequest.GetSystemWebProxy();
				systemWebProxy.Credentials = CredentialCache.DefaultCredentials;
				httpWebRequest.Proxy = systemWebProxy;
				httpWebRequest.UserAgent = GoogleAnalytics.s_UserAgent;
				Logger.Debug("Request utmGifUrl = " + text2);
				HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
				httpWebResponse.Close();
				Logger.Debug("Response utmGifUrl = " + text2);
			}
			catch (Exception ex)
			{
				Logger.Error(ex.ToString());
			}
		}

		private static void TrackEventAsync(string pageTitle, string pageURL, Event evt, string accountName)
		{
			Thread thread = new Thread((ThreadStart)delegate
			{
				GoogleAnalytics.SendTrackEvent(pageTitle, pageURL, evt, accountName);
			}, 1);
			thread.IsBackground = true;
			thread.Start();
		}

		public static void TrackEvent(string pageTitle, Event evt)
		{
			string pageURL = "/{pageTitle}";
			GoogleAnalytics.SendTrackEvent(pageTitle, pageURL, evt, GoogleAnalytics.s_AccountName);
		}

		public static void TrackEvent(string pageTitle, Event evt, string accountName)
		{
			string pageURL = "/{pageTitle}";
			GoogleAnalytics.SendTrackEvent(pageTitle, pageURL, evt, accountName);
		}

		public static void TrackEventAsync(string pageTitle, Event evt)
		{
			string pageURL = "/{pageTitle}";
			GoogleAnalytics.TrackEventAsync(pageTitle, pageURL, evt, GoogleAnalytics.s_AccountName);
		}

		public static void TrackEventAsync(string pageTitle, Event evt, string accountName)
		{
			string pageURL = "/{pageTitle}";
			GoogleAnalytics.TrackEventAsync(pageTitle, pageURL, evt, accountName);
		}

		public static void TrackEventAsync(Event evt)
		{
			string processName = Process.GetCurrentProcess().ProcessName;
			string pageURL = "/{processName}";
			GoogleAnalytics.TrackEventAsync(processName, pageURL, evt, GoogleAnalytics.s_AccountName);
		}

		public static void TrackEventAsync(Event evt, string accountName)
		{
			string processName = Process.GetCurrentProcess().ProcessName;
			string pageURL = "/{processName}";
			GoogleAnalytics.TrackEventAsync(processName, pageURL, evt, accountName);
		}

		public static void UpdateDefaultAccountName(string accountName)
		{
			GoogleAnalytics.s_AccountName = accountName;
		}
	}
}
