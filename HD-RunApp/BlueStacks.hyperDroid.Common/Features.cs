using Microsoft.Win32;
using System;

namespace BlueStacks.hyperDroid.Common
{
	public class Features
	{
		public const uint BROADCAST_MESSAGES = 1u;

		public const uint INSTALL_NOTIFICATIONS = 2u;

		public const uint UNINSTALL_NOTIFICATIONS = 4u;

		public const uint CREATE_APP_SHORTCUTS = 8u;

		public const uint LAUNCH_SETUP_APP = 16u;

		public const uint SHOW_USAGE_STATS = 32u;

		public const uint SYS_TRAY_SUPPORT = 64u;

		public const uint SUGGESTED_APPS_SUPPORT = 128u;

		public const uint OTA_SUPPORT = 256u;

		public const uint SHOW_RESTART = 512u;

		public const uint ANDROID_NOTIFICATIONS = 1024u;

		public const uint GAMING_EDITION = 251658240u;

		public const uint ALL_FEATURES = 268435455u;

		private static string s_ConfigPath = "Software\\BlueStacks\\Guests\\Android\\Config";

		public static uint GetEnabledFeatures()
		{
			RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(Features.s_ConfigPath);
			return Convert.ToUInt32(registryKey.GetValue("Features", 0));
		}

		public static bool IsFeatureEnabled(uint featureMask)
		{
			RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(Features.s_ConfigPath);
			uint num = Convert.ToUInt32(registryKey.GetValue("Features", 0));
			if ((num & featureMask) != 0)
			{
				return true;
			}
			return false;
		}

		public static void DisableFeature(uint featureMask)
		{
			RegistryKey registryKey = Registry.LocalMachine.CreateSubKey(Features.s_ConfigPath);
			uint num = Convert.ToUInt32(registryKey.GetValue("Features", 0));
			if ((num & featureMask) != 0)
			{
				uint num2 = num & ~featureMask;
				registryKey.SetValue("Features", num2);
				registryKey.Flush();
				registryKey.Close();
			}
		}

		public static void EnableFeature(uint featureMask)
		{
			RegistryKey registryKey = Registry.LocalMachine.CreateSubKey(Features.s_ConfigPath);
			uint num = Convert.ToUInt32(registryKey.GetValue("Features", 0));
			if ((num & featureMask) == 0)
			{
				uint num2 = num | featureMask;
				registryKey.SetValue("Features", num2);
				registryKey.Flush();
				registryKey.Close();
			}
		}

		public static void EnableAllFeatures()
		{
			RegistryKey registryKey = Registry.LocalMachine.CreateSubKey(Features.s_ConfigPath);
			registryKey.SetValue("Features", 268435455u, RegistryValueKind.DWord);
			registryKey.Flush();
			registryKey.Close();
		}

		public static bool IsFullScreenToggleEnabled()
		{
			if (Utils.IsOEM("Lenovo"))
			{
				return false;
			}
			return true;
		}

		public static bool IsHomeButtonEnabled()
		{
			if (!Utils.IsOEMBlueStacks() && !Utils.IsOEM("Acer") && !Utils.IsOEM("AMD") && !Utils.IsOEM("Bstk") && !Utils.IsOEM("Bstkm") && !Utils.IsOEM("MSI") && !Utils.IsOEM("China") && !Utils.IsOEM("Lenovo") && !Utils.IsOEM("yifang"))
			{
				return false;
			}
			return true;
		}

		public static bool IsShareButtonEnabled()
		{
			if (!Utils.IsOEM("Lenovo") && !Utils.IsOEM("360"))
			{
				return true;
			}
			return false;
		}

		public static bool IsSettingsButtonEnabled()
		{
			if (Utils.IsOEM("360"))
			{
				return false;
			}
			return true;
		}
	}
}
