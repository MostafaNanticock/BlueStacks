using BlueStacks.hyperDroid.Common;
using BlueStacks.hyperDroid.Common.HTTP;
using CodeTitans.JSon;
using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Security.Cryptography;
using System.Text;

namespace BlueStacks.hyperDroid.Cloud.Services
{
	internal class Auth : Service
	{
		public class ELogin : EService
		{
			public ELogin(string reason)
				: base(reason)
			{
			}
		}

		public class ESignUp : EService
		{
			public ESignUp(string reason)
				: base(reason)
			{
			}
		}

		public class Token
		{
			public class EMalformed : Exception
			{
				public EMalformed(string reason)
					: base(reason)
				{
				}
			}

			private const string REG_PATH = "Software\\BlueStacks\\Agent\\Cloud";

			public static string Key
			{
				get
				{
					return Token.GetUnSecureRegValue("Key");
				}
				set
				{
					Token.PutSecureRegValue("Key", value);
				}
			}

			public static string Secret
			{
				get
				{
					return Token.GetUnSecureRegValue("Secret");
				}
				set
				{
					Token.PutSecureRegValue("Secret", value);
				}
			}

			private static string GetUnSecureRegValue(string key)
			{
				string text = null;
				using (RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("Software\\BlueStacks\\Agent\\Cloud"))
				{
					string text2 = (string)registryKey.GetValue(key, "");
					if (text2 == "")
					{
						throw new EMalformed("Empty");
					}
					try
					{
						byte[] data = Convert.FromBase64String(text2);
						return SecureUserData.Decrypt(data);
					}
					catch (FormatException ex)
					{
						throw new EMalformed(ex.ToString());
					}
					catch (CryptographicException ex2)
					{
						throw new EMalformed(ex2.ToString());
					}
				}
			}

			private static void PutSecureRegValue(string key, string value)
			{
				byte[] inArray = SecureUserData.Encrypt(value);
				string value2 = Convert.ToBase64String(inArray);
				using (RegistryKey registryKey = Registry.LocalMachine.CreateSubKey("Software\\BlueStacks\\Agent\\Cloud"))
				{
					registryKey.SetValue(key, value2, RegistryValueKind.String);
					registryKey.Flush();
				}
			}
		}

		public const string X_BST_AUTH_KEY = "X-Bst-Auth-Key";

		public const string X_BST_AUTH_TIMESTAMP = "X-Bst-Auth-Timestamp";

		public const string X_BST_AUTH_SIGN = "X-Bst-Auth-Sign";

		private static string s_GuidSecret = "3921330286be3e2cb90a";

		public static string API_URL
		{
			get
			{
				return string.Format("{0}/api/{1}", Service.Host, "auth");
			}
		}

		public static string Route
		{
			get
			{
				return new Uri(Auth.API_URL).PathAndQuery;
			}
		}

		public static string GuidSecret(string guid)
		{
			return Auth.HMACSign(Encoding.UTF8.GetBytes(Auth.s_GuidSecret), Encoding.UTF8.GetBytes(guid));
		}

		public static Dictionary<string, string> CreateHeaders(string key)
		{
			long value = (DateTime.UtcNow.Ticks - 621355968000000000L) / 10000;
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			dictionary["X-Bst-Auth-Key"] = key;
			dictionary["X-Bst-Auth-Timestamp"] = Convert.ToString(value);
			return dictionary;
		}

		public static string Sign(string verb, string route, Dictionary<string, string> data, Dictionary<string, string> headers, string[] paramsOrder, string secret)
		{
			ArrayList arrayList = new ArrayList();
			arrayList.Add(verb);
			arrayList.Add(route);
			arrayList.Add(string.Format("{0}:{1}", "X-Bst-Auth-Key", headers["X-Bst-Auth-Key"]));
			arrayList.Add(string.Format("{0}:{1}", "X-Bst-Auth-Timestamp", headers["X-Bst-Auth-Timestamp"]));
			foreach (string text in paramsOrder)
			{
				arrayList.Add("{text}={data[text]}");
			}
			string str = string.Join("\n", arrayList.ToArray(typeof(string)) as string[]);
			str += "\n";
			Encoding.UTF8.GetBytes(str);
			return Auth.HMACSign(Encoding.UTF8.GetBytes(secret), Encoding.UTF8.GetBytes(str));
		}

		public static string HMACSign(byte[] secret, byte[] data)
		{
			byte[] array = null;
			using (HMACSHA1 hMACSHA = new HMACSHA1(secret))
			{
				array = hMACSHA.ComputeHash(data);
			}
			string text = "";
			string text2 = "";
			byte[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				byte b = array2[i];
				text = b.ToString("X").ToLower();
				text2 = text2 + ((text.Length == 1) ? "0" : "") + text;
			}
			return text2;
		}

		public static IJSonObject Login(string email, string password)
		{
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			dictionary.Add("email", email);
			dictionary.Add("password", password);
			bool gzip = true;
			string input = Client.Post(Auth.API_URL + "/login", dictionary, null, gzip);
			IJSonReader iJSonReader = new JSonReader();
			return iJSonReader.ReadAsJSonObject(input);
		}

		public static IJSonObject SignUp(string name, string email, string password)
		{
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			dictionary.Add("name", name);
			dictionary.Add("email", email);
			dictionary.Add("password", password);
			bool gzip = true;
			string input = Client.Post(Auth.API_URL + "/signup", dictionary, null, gzip);
			IJSonReader iJSonReader = new JSonReader();
			return iJSonReader.ReadAsJSonObject(input);
		}

		public static void LoginAsync(string email, string password, OnSuccess success, OnFailed failed)
		{
			BackgroundWorker backgroundWorker = Service.CreateWorkerAsync(success, failed);
			backgroundWorker.DoWork += delegate(object o, DoWorkEventArgs args)
			{
				args.Result = Auth.Login(email, password);
			};
			backgroundWorker.RunWorkerAsync();
		}

		public static void SignUpAsync(string name, string email, string password, OnSuccess success, OnFailed failed)
		{
			BackgroundWorker backgroundWorker = Service.CreateWorkerAsync(success, failed);
			backgroundWorker.DoWork += delegate(object o, DoWorkEventArgs args)
			{
				args.Result = Auth.SignUp(name, email, password);
			};
			backgroundWorker.RunWorkerAsync();
		}

		public static IJSonObject CCPC(string pcGUID)
		{
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			dictionary.Add("pc_guid", pcGUID);
			bool gzip = true;
			string input = Client.Post(Auth.API_URL + "/cc/pc", dictionary, null, gzip);
			IJSonReader iJSonReader = new JSonReader();
			return iJSonReader.ReadAsJSonObject(input);
		}

		public static IJSonObject CCPCNoCache(string pcGUID)
		{
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			dictionary.Add("pc_guid", pcGUID);
			dictionary.Add("cache", "no");
			bool gzip = true;
			string input = Client.Post(Auth.API_URL + "/cc/pc", dictionary, null, gzip);
			IJSonReader iJSonReader = new JSonReader();
			return iJSonReader.ReadAsJSonObject(input);
		}

		public static IJSonObject CCPCAdd(string email, string pcGUID)
		{
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			dictionary.Add("email", email);
			dictionary.Add("pc_guid", pcGUID);
			bool gzip = true;
			string input = Client.Post(Auth.API_URL + "/cc/pc/add", dictionary, null, gzip);
			IJSonReader iJSonReader = new JSonReader();
			return iJSonReader.ReadAsJSonObject(input);
		}
	}
}
