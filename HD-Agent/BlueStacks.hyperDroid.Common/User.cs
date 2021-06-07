using Microsoft.Win32;
using System;
using System.Security.Principal;

namespace BlueStacks.hyperDroid.Common
{
	public class User
	{
		public const string FIRST_TIME_LAUNCH_URL = "http://updates.bluestacks.com/check";

		private const string REG_PATH = "Software\\BlueStacks";

		private static string s_GUID;

		public static string GUID
		{
			get
			{
				RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("Software\\BlueStacks");
				if (registryKey == null)
				{
					return "";
				}
				User.s_GUID = (string)registryKey.GetValue("USER_GUID", "");
				return User.s_GUID;
			}
			set
			{
				RegistryKey registryKey = Registry.LocalMachine.CreateSubKey("Software\\BlueStacks");
				registryKey.SetValue("USER_GUID", value, RegistryValueKind.String);
				User.s_GUID = value;
			}
		}

		public static bool IsFirstTimeLaunch()
		{
			RegistryKey registryKey = Registry.LocalMachine.CreateSubKey("Software\\BlueStacks");
			string a = (string)registryKey.GetValue("FirstTimeLaunch", "");
			if (a == "")
			{
				registryKey.SetValue("FirstTimeLaunch", DateTime.Now.ToString());
			}
			return a == "";
		}

		public static bool IsAdministrator()
		{
			bool result = false;
			try
			{
				WindowsIdentity current = WindowsIdentity.GetCurrent();
				if (current == null)
				{
					return false;
				}
				WindowsPrincipal windowsPrincipal = new WindowsPrincipal(current);
				result = windowsPrincipal.IsInRole(WindowsBuiltInRole.Administrator);
				return result;
			}
			catch (UnauthorizedAccessException)
			{
				return result;
			}
			catch (Exception)
			{
				return result;
			}
		}
	}
}
