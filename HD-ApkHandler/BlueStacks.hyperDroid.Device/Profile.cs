using BlueStacks.hyperDroid.Common;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Management;
using System.Windows.Forms;

namespace BlueStacks.hyperDroid.Device
{
	internal class Profile
	{
		private static Dictionary<string, string> s_Info;

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
			string value;
			RegistryKey registryKey;
			try
			{
				registryKey = Registry.LocalMachine.OpenSubKey("Software\\BlueStacks");
				value = (string)registryKey.GetValue("Version");
			}
			catch
			{
				value = "";
			}
			dictionary.Add("BlueStacksVersion", value);
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
			string value2 = "";
			try
			{
				registryKey = Registry.LocalMachine.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\OEMInformation");
				string arg = (string)registryKey.GetValue("Manufacturer", "");
				string arg2 = (string)registryKey.GetValue("Model", "");
				value2 = arg+" "+arg2;
			}
			catch
			{
			}
			dictionary.Add("OEMInfo", value2);
			int num4 = Screen.PrimaryScreen.Bounds.Width;
			int num5 = Screen.PrimaryScreen.Bounds.Height;
			dictionary.Add("ScreenResolution", num4.ToString() + "x" + num5.ToString());
			try
			{
				registryKey = Registry.LocalMachine.OpenSubKey("Software\\BlueStacks\\Guests\\Android\\FrameBuffer\\0");
				num4 = (int)registryKey.GetValue("Width");
				num5 = (int)registryKey.GetValue("Height");
			}
			catch
			{
				num4 = 0;
				num5 = 0;
			}
			dictionary.Add("BlueStacksResolution", num4.ToString() + "x" + num5.ToString());
			registryKey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\NET Framework Setup\\NDP");
			string[] subKeyNames = registryKey.GetSubKeyNames();
			string value3 = "";
			string[] array = subKeyNames;
			foreach (string text in array)
			{
				if (text.StartsWith("v"))
				{
					RegistryKey registryKey2 = registryKey.OpenSubKey(text);
					if (registryKey2.GetValue("Install") != null && (int)registryKey2.GetValue("Install") == 1)
					{
						value3 = (string)registryKey2.GetValue("Version");
					}
					if (text == "v4")
					{
						RegistryKey registryKey3 = registryKey2.OpenSubKey("Client");
						if (registryKey3 != null && (int)registryKey3.GetValue("Install") == 1)
						{
							value3 = (string)registryKey3.GetValue("Version") + " Client";
						}
						registryKey3 = registryKey2.OpenSubKey("Full");
						if (registryKey3 != null && (int)registryKey3.GetValue("Install") == 1)
						{
							value3 = (string)registryKey3.GetValue("Version") + " Full";
						}
					}
				}
			}
			dictionary.Add("DotNetVersion", value3);
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
			string sysInfo = Profile.GetSysInfo("Select Caption from Win32_VideoController");
			dictionary.Add("gpu", sysInfo);
			string value = "";
			if (!string.IsNullOrEmpty(sysInfo))
			{
				value = sysInfo.Substring(0, sysInfo.IndexOf(" "));
			}
			dictionary.Add("gpu_vendor", value);
			dictionary.Add("driver_version", Profile.GetSysInfo("Select DriverVersion from Win32_VideoController"));
			dictionary.Add("driver_date", Profile.GetSysInfo("Select DriverDate from Win32_VideoController"));
			return dictionary;
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
