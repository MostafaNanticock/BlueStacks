using BlueStacks.hyperDroid.Common;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Management;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace BlueStacks.hyperDroid.Device
{
	internal class Profile
	{
		private static Dictionary<string, string> s_Info;

		private static string s_glVendor = "";

		private static string s_glRenderer = "";

		private static string s_glVersion = "";

		public static string OEM
		{
			get
			{
				string hKLMConfigRegKeyPath = Strings.HKLMConfigRegKeyPath;
				using (RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(hKLMConfigRegKeyPath))
				{
					if (registryKey == null)
					{
						return "BlueStacks";
					}
					return (string)registryKey.GetValue("OEM", "BlueStacks");
				}
			}
			set
			{
				string hKLMConfigRegKeyPath = Strings.HKLMConfigRegKeyPath;
				using (RegistryKey registryKey = Registry.LocalMachine.CreateSubKey(hKLMConfigRegKeyPath))
				{
					registryKey.SetValue("OEM", value);
					registryKey.Flush();
				}
			}
		}

		public static string VirtType
		{
			get
			{
				string text = "legacy";
				string androidKeyBasePath = Strings.AndroidKeyBasePath;
				using (RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(androidKeyBasePath))
				{
					if (registryKey != null)
					{
						text = (string)registryKey.GetValue("VirtType", "legacy");
					}
				}
				Logger.Info(androidKeyBasePath + "\\VirtType = " + text);
				return text;
			}
		}

		public static string GlVendor
		{
			get
			{
				return Profile.s_glVendor;
			}
			set
			{
				Profile.s_glVendor = value;
			}
		}

		public static string GlRenderer
		{
			get
			{
				return Profile.s_glRenderer;
			}
			set
			{
				Profile.s_glRenderer = value;
			}
		}

		public static string GlVersion
		{
			get
			{
				return Profile.s_glVersion;
			}
			set
			{
				Profile.s_glVersion = value;
			}
		}

		public static Dictionary<string, string> Info()
		{
			if (Profile.s_Info != null)
			{
				return Profile.s_Info;
			}
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			dictionary.Add("Processor", Profile.GetSysInfo("Select Name from Win32_Processor"));
			dictionary.Add("NumberOfProcessors", Profile.GetSysInfo("Select NumberOfLogicalProcessors from Win32_Processor"));
			dictionary.Add("GPU", Profile.GetSysInfo("Select Caption from Win32_VideoController"));
			dictionary.Add("GPUDriver", Profile.GetSysInfo("Select DriverVersion from Win32_VideoController"));
			dictionary.Add("OS", Profile.GetSysInfo("Select Caption from Win32_OperatingSystem"));
			string value = "{Environment.OSVersion.Version.Major}.{Environment.OSVersion.Version.Minor}";
			dictionary.Add("OSVersion", value);
			int num = 0;
			try
			{
				string sysInfo = Profile.GetSysInfo("Select TotalPhysicalMemory from Win32_ComputerSystem");
				ulong num2 = Convert.ToUInt64(sysInfo);
				num = (int)(num2 / 1048576uL);
			}
			catch (Exception ex)
			{
				Logger.Error("Exception when finding ram");
				Logger.Error(ex.ToString());
			}
			dictionary.Add("RAM", num.ToString());
			string value2;
			RegistryKey registryKey;
			try
			{
				registryKey = Registry.LocalMachine.OpenSubKey("Software\\BlueStacks");
				value2 = (string)registryKey.GetValue("Version");
			}
			catch
			{
				value2 = "";
			}
			dictionary.Add("BlueStacksVersion", value2);
			int num3;
			try
			{
				string name = "SOFTWARE\\BlueStacks\\Guests\\Android\\Config";
				registryKey = Registry.LocalMachine.OpenSubKey(name);
				num3 = (int)registryKey.GetValue("GlMode");
			}
			catch
			{
				num3 = -1;
			}
			dictionary.Add("GlMode", num3.ToString());
			int num4;
			try
			{
				string name2 = "SOFTWARE\\BlueStacks\\Guests\\Android\\Config";
				registryKey = Registry.LocalMachine.OpenSubKey(name2);
				num4 = (int)registryKey.GetValue("GlRenderMode");
			}
			catch
			{
				num4 = -1;
			}
			dictionary.Add("GlRenderMode", num4.ToString());
			string value3 = "";
			try
			{
				registryKey = Registry.LocalMachine.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\OEMInformation");
				string arg = (string)registryKey.GetValue("Manufacturer", "");
				string arg2 = (string)registryKey.GetValue("Model", "");
				value3 = "{arg} {arg2}";
			}
			catch
			{
			}
			dictionary.Add("OEMInfo", value3);
			int num5 = Screen.PrimaryScreen.Bounds.Width;
			int num6 = Screen.PrimaryScreen.Bounds.Height;
			dictionary.Add("ScreenResolution", num5.ToString() + "x" + num6.ToString());
			try
			{
				registryKey = Registry.LocalMachine.OpenSubKey("Software\\BlueStacks\\Guests\\Android\\FrameBuffer\\0");
				num5 = (int)registryKey.GetValue("Width");
				num6 = (int)registryKey.GetValue("Height");
			}
			catch
			{
				num5 = 0;
				num6 = 0;
			}
			dictionary.Add("BlueStacksResolution", num5.ToString() + "x" + num6.ToString());
			registryKey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\NET Framework Setup\\NDP");
			string[] subKeyNames = registryKey.GetSubKeyNames();
			string value4 = "";
			string[] array = subKeyNames;
			foreach (string text in array)
			{
				if (text.StartsWith("v"))
				{
					RegistryKey registryKey2 = registryKey.OpenSubKey(text);
					if (registryKey2.GetValue("Install") != null && (int)registryKey2.GetValue("Install") == 1)
					{
						value4 = (string)registryKey2.GetValue("Version");
					}
					if (text == "v4")
					{
						RegistryKey registryKey3 = registryKey2.OpenSubKey("Client");
						if (registryKey3 != null && (int)registryKey3.GetValue("Install") == 1)
						{
							value4 = (string)registryKey3.GetValue("Version") + " Client";
						}
						registryKey3 = registryKey2.OpenSubKey("Full");
						if (registryKey3 != null && (int)registryKey3.GetValue("Install") == 1)
						{
							value4 = (string)registryKey3.GetValue("Version") + " Full";
						}
					}
				}
			}
			dictionary.Add("DotNetVersion", value4);
			Profile.s_Info = dictionary;
			return Profile.s_Info;
		}

		public static Dictionary<string, string> InfoForGraphicsDriverCheck()
		{
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			dictionary.Add("os_version", Profile.GetSysInfo("Select Caption from Win32_OperatingSystem"));
			dictionary.Add("os_arch", Profile.GetSysInfo("Select OSArchitecture from Win32_OperatingSystem"));
			dictionary.Add("processor_vendor", Profile.GetSysInfo("Select Manufacturer from Win32_Processor"));
			dictionary.Add("processor", Profile.GetSysInfo("Select Name from Win32_Processor"));
			string text = Profile.GetSysInfo("Select Caption from Win32_VideoController");
			string text2 = "";
			string[] array = text.Split(new string[3]
			{
				Environment.NewLine,
				"\r\n",
				"\n"
			}, StringSplitOptions.RemoveEmptyEntries);
			if (!string.IsNullOrEmpty(text))
			{
				string[] array2 = array;
				foreach (string text3 in array2)
				{
					text2 = text2 + text3.Substring(0, text3.IndexOf(" ")) + "\r\n";
				}
				text2 = text2.Trim();
			}
			string text4 = Profile.GetSysInfo("Select DriverVersion from Win32_VideoController");
			string text5 = Profile.GetSysInfo("Select DriverDate from Win32_VideoController");
			string[] array3 = text2.Split(new string[3]
			{
				Environment.NewLine,
				"\r\n",
				"\n"
			}, StringSplitOptions.RemoveEmptyEntries);
			string[] array4 = text4.Split(new string[3]
			{
				Environment.NewLine,
				"\r\n",
				"\n"
			}, StringSplitOptions.RemoveEmptyEntries);
			string[] array5 = text5.Split(new string[3]
			{
				Environment.NewLine,
				"\r\n",
				"\n"
			}, StringSplitOptions.RemoveEmptyEntries);
			int num = 0;
			while (num < array.Length)
			{
				if (!(array[num] == Profile.GlRenderer) && !Profile.GlVendor.Contains(array3[num]))
				{
					num++;
					continue;
				}
				text = array[num];
				text2 = array3[num];
				text4 = array4[num];
				text5 = array5[num];
				break;
			}
			dictionary.Add("gpu", text);
			dictionary.Add("gpu_vendor", text2);
			dictionary.Add("driver_version", text4);
			dictionary.Add("driver_date", text5);
			string value = "";
			RegistryKey registryKey;
			using (registryKey = Registry.LocalMachine.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\OEMInformation"))
			{
				if (registryKey != null)
				{
					value = (string)registryKey.GetValue("Manufacturer", "");
				}
			}
			dictionary.Add("oem_manufacturer", value);
			string value2 = "";
			using (registryKey = Registry.LocalMachine.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\OEMInformation"))
			{
				if (registryKey != null)
				{
					value2 = (string)registryKey.GetValue("Model", "");
				}
			}
			dictionary.Add("oem_model", value2);
			dictionary.Add("bst_oem", Profile.OEM);
			return dictionary;
		}

		public static bool IsSupportedApu()
		{
			bool result = false;
			string[] array = new string[17]
			{
				"1309",
				"130A",
				"130B",
				"130C",
				"130D",
				"130E",
				"1318",
				"9850",
				"9851",
				"990B",
				"990D",
				"990F",
				"9995",
				"9997",
				"9999",
				"999A",
				"999B"
			};
			string sysInfo = Profile.GetSysInfo("Select PNPDeviceID from Win32_VideoController");
			Logger.Info(sysInfo);
			string[] array2 = sysInfo.Split(new string[3]
			{
				Environment.NewLine,
				"\r\n",
				"\n"
			}, StringSplitOptions.RemoveEmptyEntries);
			string[] array3 = array2;
			foreach (string text in array3)
			{
				if (text.Contains("VEN_1002"))
				{
					Regex regex = new Regex(".*DEV_(\\w\\w\\w\\w).*");
					Match match = regex.Match(text);
					if (match.Success)
					{
						string value = match.Groups[1].Value.ToLowerInvariant();
						if (Array.IndexOf(array, value) >= 0)
						{
							result = true;
							break;
						}
					}
				}
			}
			return result;
		}

		public static string GetSysInfo(string query)
		{
			int num = 0;
			string text = "";
			try
			{
				ManagementObjectSearcher managementObjectSearcher = new ManagementObjectSearcher(query);
				foreach (ManagementObject item in managementObjectSearcher.Get())
				{
					num++;
					PropertyDataCollection properties = item.Properties;
					PropertyDataCollection.PropertyDataEnumerator enumerator = properties.GetEnumerator();
					try
					{
						while (enumerator.MoveNext())
						{
							PropertyData current = enumerator.Current;
							text = text + current.Value.ToString() + '\n';
						}
					}
					finally
					{
						IDisposable disposable = enumerator as IDisposable;
						disposable.Dispose();
					}
				}
			}
			catch (Exception ex)
			{
				Logger.Error(ex.ToString());
			}
			return text.Trim();
		}
	}
}
