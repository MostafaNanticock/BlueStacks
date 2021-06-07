using CodeTitans.JSon;
using System.IO;

namespace BlueStacks.hyperDroid.Common
{
	public class JsonParser
	{
		private static string s_appsDotJsonFile = Path.Combine(Strings.GadgetDir, "apps.json");

		public static AppInfo[] s_originalJson = null;

		public static AppInfo[] GetAppList()
		{
			StreamReader streamReader = new StreamReader(JsonParser.s_appsDotJsonFile);
			string input = streamReader.ReadToEnd();
			streamReader.Close();
			JSonReader jSonReader = new JSonReader();
			JsonParser.GetOriginalJson(jSonReader.ReadAsJSonObject(input));
			return JsonParser.s_originalJson;
		}

		private static void GetOriginalJson(IJSonObject input)
		{
			JsonParser.s_originalJson = new AppInfo[input.Length];
			for (int i = 0; i < input.Length; i++)
			{
				JsonParser.s_originalJson[i] = new AppInfo(input[i]);
			}
		}

		public static int GetInstalledAppCount()
		{
			JsonParser.GetAppList();
			int num = 0;
			for (int i = 0; i < JsonParser.s_originalJson.Length; i++)
			{
				if (string.Compare(JsonParser.s_originalJson[i].activity, ".Main", true) != 0 && string.Compare(JsonParser.s_originalJson[i].appstore, "yes", true) != 0)
				{
					num++;
				}
			}
			return num;
		}

		public static bool GetAppInfoFromAppName(string appName, out string packageName, out string imageName, out string activityName)
		{
			packageName = null;
			imageName = null;
			activityName = null;
			JsonParser.GetAppList();
			for (int i = 0; i < JsonParser.s_originalJson.Length; i++)
			{
				if (JsonParser.s_originalJson[i].name == appName)
				{
					packageName = JsonParser.s_originalJson[i].package;
					imageName = JsonParser.s_originalJson[i].img;
					activityName = JsonParser.s_originalJson[i].activity;
					return true;
				}
			}
			return false;
		}

		public static bool GetAppInfoFromPackageName(string packageName, out string appName, out string imageName, out string activityName, out string appstore)
		{
			appName = "";
			imageName = "";
			activityName = "";
			appstore = "";
			JsonParser.GetAppList();
			for (int i = 0; i < JsonParser.s_originalJson.Length; i++)
			{
				if (JsonParser.s_originalJson[i].package == packageName)
				{
					appName = JsonParser.s_originalJson[i].name;
					imageName = JsonParser.s_originalJson[i].img;
					activityName = JsonParser.s_originalJson[i].activity;
					appstore = JsonParser.s_originalJson[i].appstore;
					return true;
				}
			}
			return false;
		}

		public static string GetAppNameFromPackageActivity(string packageName, string activityName)
		{
			JsonParser.GetAppList();
			for (int i = 0; i < JsonParser.s_originalJson.Length; i++)
			{
				if (JsonParser.s_originalJson[i].package == packageName && JsonParser.s_originalJson[i].activity == activityName)
				{
					return JsonParser.s_originalJson[i].name;
				}
			}
			return string.Empty;
		}

		public static string GetPackageNameFromActivityName(string activityName)
		{
			JsonParser.GetAppList();
			for (int i = 0; i < JsonParser.s_originalJson.Length; i++)
			{
				if (JsonParser.s_originalJson[i].activity == activityName)
				{
					return JsonParser.s_originalJson[i].package;
				}
			}
			return string.Empty;
		}

		public static string GetActivityNameFromPackageName(string packageName)
		{
			JsonParser.GetAppList();
			for (int i = 0; i < JsonParser.s_originalJson.Length; i++)
			{
				if (JsonParser.s_originalJson[i].package == packageName)
				{
					return JsonParser.s_originalJson[i].activity;
				}
			}
			return string.Empty;
		}

		public static bool IsPackageNameSystemApp(string packageName)
		{
			JsonParser.GetAppList();
			for (int i = 0; i < JsonParser.s_originalJson.Length; i++)
			{
				if (JsonParser.s_originalJson[i].package == packageName)
				{
					if (JsonParser.s_originalJson[i].system == "1")
					{
						return true;
					}
					return false;
				}
			}
			return false;
		}

		public static bool IsAppNameSystemApp(string appName)
		{
			JsonParser.GetAppList();
			for (int i = 0; i < JsonParser.s_originalJson.Length; i++)
			{
				if (JsonParser.s_originalJson[i].name == appName)
				{
					if (JsonParser.s_originalJson[i].system == "1")
					{
						return true;
					}
					return false;
				}
			}
			return false;
		}

		public static bool IsAppInstalled(string packageName, string activityName)
		{
			JsonParser.GetAppList();
			for (int i = 0; i < JsonParser.s_originalJson.Length; i++)
			{
				if (JsonParser.s_originalJson[i].package == packageName && JsonParser.s_originalJson[i].activity == activityName)
				{
					return true;
				}
			}
			return false;
		}

		public static bool GetAppData(string package, string activity, out string name, out string img)
		{
			JsonParser.GetAppList();
			name = "";
			img = "";
			for (int i = 0; i < JsonParser.s_originalJson.Length; i++)
			{
				if (JsonParser.s_originalJson[i].package == package && JsonParser.s_originalJson[i].activity == activity)
				{
					name = JsonParser.s_originalJson[i].name;
					img = JsonParser.s_originalJson[i].img;
					Logger.Info("Got AppName: {0} and AppIcon: {1}", name, img);
					return true;
				}
			}
			return false;
		}

		public static void WriteJson(AppInfo[] json)
		{
			JSonWriter jSonWriter = new JSonWriter();
			Logger.Info("JsonParser: Writing json object array to json writer");
			jSonWriter.WriteArrayBegin();
			for (int i = 0; i < json.Length; i++)
			{
				jSonWriter.WriteObjectBegin();
				jSonWriter.WriteMember("img", json[i].img);
				jSonWriter.WriteMember("KeyName", json[i].name);
				jSonWriter.WriteMember("system", json[i].system);
				jSonWriter.WriteMember("package", json[i].package);
				jSonWriter.WriteMember("appstore", json[i].appstore);
				jSonWriter.WriteMember("activity", json[i].activity);
				if (json[i].url != null)
				{
					jSonWriter.WriteMember("url", json[i].url);
				}
				jSonWriter.WriteObjectEnd();
			}
			jSonWriter.WriteArrayEnd();
			StreamWriter streamWriter = new StreamWriter(JsonParser.s_appsDotJsonFile + ".tmp");
			streamWriter.Write(jSonWriter.ToString());
			streamWriter.Close();
			File.Copy(JsonParser.s_appsDotJsonFile + ".tmp", JsonParser.s_appsDotJsonFile + ".bak", true);
			File.Delete(JsonParser.s_appsDotJsonFile);
			File.Move(JsonParser.s_appsDotJsonFile + ".tmp", JsonParser.s_appsDotJsonFile);
		}

		public static int AddToJson(AppInfo json)
		{
			JsonParser.GetAppList();
			Logger.Info("Adding to Json");
			AppInfo[] array = new AppInfo[JsonParser.s_originalJson.Length + 1];
			int i;
			for (i = 0; i < JsonParser.s_originalJson.Length; i++)
			{
				array[i] = JsonParser.s_originalJson[i];
			}
			array[i] = json;
			JsonParser.WriteJson(array);
			return JsonParser.s_originalJson.Length;
		}
	}
}
