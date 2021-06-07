using System;
using System.IO;

namespace BlueStacks.hyperDroid.Common
{
	internal class Strings
	{
		private static string s_AppTitle;

		public static string AppTitle
		{
			get
			{
				if (Strings.s_AppTitle != null)
				{
					return Strings.s_AppTitle;
				}
				return Strings.DefaultWindowTitle;
			}
			set
			{
				Strings.s_AppTitle = value;
			}
		}

		public static string AndroidServiceName
		{
			get
			{
				return "bsthdandroidsvc";
			}
		}

		public static string ThinInstallerTitle
		{
			get
			{
				return "BlueStacks Download Manager";
			}
		}

		public static string ThinInstallerInitLbl
		{
			get
			{
				return "Installing BlueStacks";
			}
		}

		public static string ThinInstallerInstallStateTitle
		{
			get
			{
				return "Installing BlueStacks";
			}
		}

		public static string RuntimeDisplayName
		{
			get
			{
				return "BlueStacks App Player";
			}
		}

		public static string AgentDownloadCompleteEventName
		{
			get
			{
				return "BlueStacks_Agent_Download_Complete";
			}
		}

		public static string FrontendLockName
		{
			get
			{
				return "Global\\BlueStacks_Android_Frontend_Lock";
			}
		}

		public static string HDAgentLockName
		{
			get
			{
				return "Global\\BlueStacks_HDAgent_Lock";
			}
		}

		public static string HDApkInstallerLockName
		{
			get
			{
				return "Global\\BlueStacks_HDApkInstaller_Lock";
			}
		}

		public static string RestartLockName
		{
			get
			{
				return "Global\\BlueStacks_Restart_Lock";
			}
		}

		public static string LogRotateLockName
		{
			get
			{
				return "Global\\BlueStacks_LogRotate_Lock";
			}
		}

		public static string ApkThinInstallerLockName
		{
			get
			{
				return "Global\\BlueStacks_ApkThinInstaller_Lock";
			}
		}

		public static string ThinInstallerLockName
		{
			get
			{
				return "Global\\BlueStacks_ThinInstaller_Lock";
			}
		}

		public static string SpawnAppsLauncherLockName
		{
			get
			{
				return "Global\\BlueStacks_SpawnApps_Launcher_Lock";
			}
		}

		public static string AnotherInstanceRunning
		{
			get
			{
				return "Access is denied. You probably have another instance of BlueStacks running from another user account.";
			}
		}

		public static string RegBasePath
		{
			get
			{
				return "Software\\BlueStacks";
			}
		}

		public static string HKLMConfigRegKeyPath
		{
			get
			{
				return "Software\\BlueStacks\\Guests\\Android\\Config";
			}
		}

		public static string AndroidKeyBasePath
		{
			get
			{
				return "Software\\BlueStacks\\Guests\\Android";
			}
		}

		public static string HKCURegKeyPath
		{
			get
			{
				return "Software\\BlueStacks\\Agent\\Cloud";
			}
		}

		public static string CloudRegKeyPath
		{
			get
			{
				return "Software\\BlueStacks\\Agent\\Cloud";
			}
		}

		public static string AppSyncRegKeyPath
		{
			get
			{
				return "Software\\BlueStacks\\Agent\\AppSync";
			}
		}

		public static string GetDiskUsage
		{
			get
			{
				return "getdiskusage";
			}
		}

		public static string SystrayVisibilityUrl
		{
			get
			{
				return "systrayvisibility";
			}
		}

		public static string ShowNotificationsUrl
		{
			get
			{
				return "usernotifications";
			}
		}

		public static string SharePicUrl
		{
			get
			{
				return "sharepic";
			}
		}

		public static string FileDropUrl
		{
			get
			{
				return "filedrop";
			}
		}

		public static string HostOrientationUrl
		{
			get
			{
				return "hostorientation";
			}
		}

		public static string UploadCrashUrl
		{
			get
			{
				return "stats/uploadcrashreport";
			}
		}

		public static string UploadUsageUrl
		{
			get
			{
				return "stats/uploadusagestats";
			}
		}

		public static string UploadUsageCountUrl
		{
			get
			{
				return "stats/uploadusagecountstats";
			}
		}

		public static string AppClickStatsUrl
		{
			get
			{
				return "stats/appclickstats";
			}
		}

		public static string AppInstallStatsUrl
		{
			get
			{
				return "stats/appinstallstats";
			}
		}

		public static string AVGInstallStatsUrl
		{
			get
			{
				return "stats/avginstallstats";
			}
		}

		public static string SystemInfoStatsUrl
		{
			get
			{
				return "stats/systeminfostats";
			}
		}

		public static string BootStatsUrl
		{
			get
			{
				return "stats/bootstats";
			}
		}

		public static string BinaryCrashStatsUrl
		{
			get
			{
				return "stats/bincrashstats";
			}
		}

		public static string BsInstallStatsUrl
		{
			get
			{
				return "stats/bsinstallstats";
			}
		}

		public static string RegisterEmailUrl
		{
			get
			{
				return "api/auth/registeremail";
			}
		}

		public static string SignUpUrl
		{
			get
			{
				return "api/auth/signup";
			}
		}

		public static string LoginUrl
		{
			get
			{
				return "api/auth/login";
			}
		}

		public static string ForgotPasswordUrl
		{
			get
			{
				return "forgotpassword";
			}
		}

		public static string UploadDebugLogsUrl
		{
			get
			{
				return "uploaddebuglogs";
			}
		}

		public static string SlideoutMetricsUrl
		{
			get
			{
				return "UpdateSlideOutMetrics";
			}
		}

		public static string ShowFeNotificationUrl
		{
			get
			{
				return "showfenotification";
			}
		}

		public static string AppDataFEUrl
		{
			get
			{
				return "appdatafeurl";
			}
		}

		public static string QuitFrontend
		{
			get
			{
				return "quitfrontend";
			}
		}

		public static string SwitchToLauncherUrl
		{
			get
			{
				return "switchtolauncher";
			}
		}

		public static string SwitchToWindowsUrl
		{
			get
			{
				return "switchtowindows";
			}
		}

		public static string LocaleResourceUrl
		{
			get
			{
				return "downloadlocale";
			}
		}

		public static string AppInstallUrl
		{
			get
			{
				return "amzinstall";
			}
		}

		public static string CheckGAUrl
		{
			get
			{
				return "gaurl";
			}
		}

		public static string FoneLinkUrl
		{
			get
			{
				return "fonelink/home";
			}
		}

		public static string CheckGraphicsDriverUrl
		{
			get
			{
				return "checkgraphicsdriver";
			}
		}

		public static string UserDataDir
		{
			get
			{
				return "UserData";
			}
		}

		public static string MyAppsDir
		{
			get
			{
				return "My Apps";
			}
		}

		public static string StoreAppsDir
		{
			get
			{
				return "App Stores";
			}
		}

		public static string IconsDir
		{
			get
			{
				return "Icons";
			}
		}

		public static string ChannelsUrl
		{
			get
			{
				return Strings.ChannelsProdUrl;
			}
		}

		public static string ChannelsProdUrl
		{
			get
			{
				return "https://bluestacks-cloud.appspot.com";
			}
		}

		public static string ChannelsQaUrl
		{
			get
			{
				return "https://bluestacks-cloud-qa.appspot.com";
			}
		}

		public static string ChannelsDevUrl
		{
			get
			{
				return "https://bluestacks-cloud-dev.appspot.com";
			}
		}

		public static string CDNDownloadUrl
		{
			get
			{
				return "http://cdn.bluestacks.com/downloads";
			}
		}

		public static string GLUnsupportedError
		{
			get
			{
				return "BlueStacks currently doesn't recognize your graphics card.\nIt is possible your Graphics Drivers may need to be updated. Please update them and try installing again.";
			}
		}

		public static string GLUnsupportedErrorForApkToExe
		{
			get
			{
				return "Your graphics hardware or drivers do not support apps that need high performance graphics.\nYou may update the graphics drivers and re-install the app to try to resolve this limitation.";
			}
		}

		public static string BitdefenderFoundError
		{
			get
			{
				return "You seem to have Bitdefender antivirus installed. BlueStacks is currently not compatible with Bitdefender. Installation will now abort.";
			}
		}

		public static string UninstallMessage
		{
			get
			{
				return "Do you want to keep all your apps and data?\n\nNote: This might take some space on your system depending upon app data size you currently have.";
			}
		}

		public static string UninstallDependentAppsMessage
		{
			get
			{
				return "Some programs require Notification Center to work properly. If you uninstall it, you will not be able to use those dependent programs.\n\n Are you sure want to uninstall Notification Center?";
			}
		}

		public static string GraphicsDriverOutdatedError
		{
			get
			{
				return "BlueStacks could not be installed. Your Graphics Drivers seem to be out-of-date. We recommend you update your drivers and try installing again. Update now?";
			}
		}

		public static string GraphicsDriverOutdated
		{
			get
			{
				return "Your Graphics Drivers seem to be out-of-date. We recommend you update your drivers for optimal performance. Update now?";
			}
		}

		public static string GAUserAccountDefault
		{
			get
			{
				return "UA-32186883-1";
			}
		}

		public static string GAUserAccountAppClicks
		{
			get
			{
				return "UA-30694866-1";
			}
		}

		public static string GAUserAccountInstaller
		{
			get
			{
				return "UA-30705638-1";
			}
		}

		public static string GAUserAccountSuggestedApps
		{
			get
			{
				return "UA-30698067-1";
			}
		}

		public static string GACategorySuggestedApps
		{
			get
			{
				return "suggestedapps";
			}
		}

		public static string GACategoryInstaller
		{
			get
			{
				return "installer";
			}
		}

		public static string GACategoryAppClicks
		{
			get
			{
				return "appclicks";
			}
		}

		public static string DefaultWindowTitle
		{
			get
			{
				return "App Player";
			}
		}

		public static string LibraryName
		{
			get
			{
				return "Apps";
			}
		}

		public static string StartLauncherShortcutName
		{
			get
			{
				return "Start BlueStacks";
			}
		}

		public static string BlueStacksNotFound
		{
			get
			{
				return "No BlueStacks installation found. Please download and install BlueStacks runtime from www.bluestacks.com";
			}
		}

		public static string VMXError
		{
			get
			{
				return "Unfortunately, BlueStacks (beta-1) cannot run on a PC where a virtual machine (VM) is in use. You may either uninstall the VM, reboot and re-run BlueStacks, or wait for a future release of BlueStacks in which this condition is relaxed.";
			}
		}

		public static string LenovoVMXError
		{
			get
			{
				return "Unfortunately, Lenovo App Player cannot run on a PC where a virtual machine (VM) is in use. You may either uninstall the VM, reboot and re-run the app player, or wait for a future release of the Lenovo App Player in which this condition is relaxed.";
			}
		}

		public static string SystemUpgradedError
		{
			get
			{
				return "It seems that your system has been upgraded. Please click OK to configure and run BlueStacks App Player.";
			}
		}

		public static string CompanyName
		{
			get
			{
				return "BlueStack Systems, Inc.";
			}
		}

		public static string UninstallKey
		{
			get
			{
				return "Software\\Microsoft\\Windows\\CurrentVersion\\Uninstall";
			}
		}

		public static string BstPrefix
		{
			get
			{
				return "Bst-";
			}
		}

		public static string UninstallKeyPrefix
		{
			get
			{
				return Strings.UninstallKey + "\\" + Strings.BstPrefix;
			}
		}

		public static string CheckForUpdates
		{
			get
			{
				return "Check For Updates";
			}
		}

		public static string DownloadingUpdates
		{
			get
			{
				return "Downloading Updates";
			}
		}

		public static string InstallUpdates
		{
			get
			{
				return "Install Updates";
			}
		}

		public static string CommonAppData
		{
			get
			{
				return Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
			}
		}

		public static string BstCommonAppData
		{
			get
			{
				return Path.Combine(Strings.CommonAppData, "BlueStacks");
			}
		}

		public static string BstUserDataDir
		{
			get
			{
				return Path.Combine(Strings.BstCommonAppData, Strings.UserDataDir);
			}
		}

		public static string GadgetDir
		{
			get
			{
				return Path.Combine(Strings.BstUserDataDir, "Gadget");
			}
		}

		public static string LibraryDir
		{
			get
			{
				return Path.Combine(Strings.BstUserDataDir, "Library");
			}
		}

		public static string SharedFolderDir
		{
			get
			{
				return Path.Combine(Strings.BstUserDataDir, "SharedFolder");
			}
		}

		public static string SharedFolderName
		{
			get
			{
				return "BstSharedFolder";
			}
		}

		public static string InputMapperFolderName
		{
			get
			{
				return "InputMapper";
			}
		}

		public static string InputMapperFolder
		{
			get
			{
				return Path.Combine(Strings.BstUserDataDir, Strings.InputMapperFolderName);
			}
		}
	}
}
