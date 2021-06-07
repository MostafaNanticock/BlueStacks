using Microsoft.Win32;

namespace BlueStacks.hyperDroid.Updater
{
	public static class Manifest
	{
		private const string REG_PATH = "Software\\BlueStacks\\Updater\\Manifest";

		private static string s_Version;

		private static string s_MD5;

		private static string s_SHA1;

		private static string s_Size;

		private static string s_URL;

		public static string Version
		{
			get
			{
				RegistryKey registryKey = Registry.LocalMachine.CreateSubKey("Software\\BlueStacks\\Updater\\Manifest");
				Manifest.s_Version = (string)registryKey.GetValue("Version");
				return Manifest.s_Version;
			}
			set
			{
				Manifest.s_Version = value;
				RegistryKey registryKey = Registry.LocalMachine.CreateSubKey("Software\\BlueStacks\\Updater\\Manifest");
				registryKey.SetValue("Version", Manifest.s_Version, RegistryValueKind.String);
			}
		}

		public static string MD5
		{
			get
			{
				RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("Software\\BlueStacks\\Updater\\Manifest");
				Manifest.s_MD5 = (string)registryKey.GetValue("MD5");
				return Manifest.s_MD5;
			}
			set
			{
				Manifest.s_MD5 = value;
				RegistryKey registryKey = Registry.LocalMachine.CreateSubKey("Software\\BlueStacks\\Updater\\Manifest");
				registryKey.SetValue("MD5", Manifest.s_MD5, RegistryValueKind.String);
			}
		}

		public static string SHA1
		{
			get
			{
				RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("Software\\BlueStacks\\Updater\\Manifest");
				Manifest.s_SHA1 = (string)registryKey.GetValue("SHA1");
				return Manifest.s_SHA1;
			}
			set
			{
				Manifest.s_SHA1 = value;
				RegistryKey registryKey = Registry.LocalMachine.CreateSubKey("Software\\BlueStacks\\Updater\\Manifest");
				registryKey.SetValue("SHA1", Manifest.s_SHA1, RegistryValueKind.String);
			}
		}

		public static string Size
		{
			get
			{
				RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("Software\\BlueStacks\\Updater\\Manifest");
				Manifest.s_Size = (string)registryKey.GetValue("Size");
				return Manifest.s_Size;
			}
			set
			{
				Manifest.s_Size = value;
				RegistryKey registryKey = Registry.LocalMachine.CreateSubKey("Software\\BlueStacks\\Updater\\Manifest");
				registryKey.SetValue("Size", Manifest.s_Size, RegistryValueKind.String);
			}
		}

		public static string URL
		{
			get
			{
				RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("Software\\BlueStacks\\Updater\\Manifest");
				Manifest.s_URL = (string)registryKey.GetValue("URL");
				return Manifest.s_URL;
			}
			set
			{
				Manifest.s_URL = value;
				RegistryKey registryKey = Registry.LocalMachine.CreateSubKey("Software\\BlueStacks\\Updater\\Manifest");
				registryKey.SetValue("URL", Manifest.s_URL, RegistryValueKind.String);
			}
		}
	}
}
