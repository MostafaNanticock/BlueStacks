using BlueStacks.hyperDroid.Common;
using BlueStacks.hyperDroid.Common.HTTP;
using CodeTitans.JSon;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Resources;
using System.Threading;
using System.Windows.Forms;

namespace BlueStacks.hyperDroid.Locale
{
    public class Strings
    {
        public class English
        {
            public static string RestoreFactorySettings
            {
                get
                {
                    return Strings.GetEnglishString("RestoreFactorySettings");
                }
            }

            public static string StartBlueStacks
            {
                get
                {
                    return Strings.GetEnglishString("StartBlueStacks");
                }
            }

            public static string RestartBlueStacks
            {
                get
                {
                    return Strings.GetEnglishString("RestartBlueStacks");
                }
            }

            public static string QuitBlueStacks
            {
                get
                {
                    return Strings.GetEnglishString("QuitBlueStacks");
                }
            }

            public static string UploadDebugLogs
            {
                get
                {
                    return Strings.GetEnglishString("UploadDebugLogs");
                }
            }
        }

        public static ResourceManager s_Resource;

        public static CultureInfo s_Ci;

        public static string s_ResourceLocation;

        public static string s_ResourceBaseName = "i18n";

        private static string s_HostKeyPath = "Software\\BlueStacks\\Agent\\Cloud";

        private static CultureInfo s_en_US = new CultureInfo("en-US", false);

        public static string RestoreFactorySettings
        {
            get
            {
                return Strings.GetLocalizedString("RestoreFactorySettings");
            }
        }

        public static string StartBlueStacks
        {
            get
            {
                return Strings.GetLocalizedString("StartBlueStacks");
            }
        }

        public static string RestartBlueStacks
        {
            get
            {
                return Strings.GetLocalizedString("RestartBlueStacks");
            }
        }

        public static string QuitBlueStacks
        {
            get
            {
                return Strings.GetLocalizedString("QuitBlueStacks");
            }
        }

        public static string StopBlueStacks
        {
            get
            {
                return Strings.GetLocalizedString("StopBlueStacks");
            }
        }

        public static string RotatePortraitApps
        {
            get
            {
                return Strings.GetLocalizedString("RotatePortraitApps");
            }
        }

        public static string UploadDebugLogs
        {
            get
            {
                return Strings.GetLocalizedString("UploadDebugLogs");
            }
        }

        public static string FreeDuringBeta
        {
            get
            {
                return Strings.GetLocalizedString("FreeDuringBeta");
            }
        }

        public static string SMSSetupMenu
        {
            get
            {
                return Strings.GetLocalizedString("SMSSetupMenu");
            }
        }

        public static string ShowNotificationsMenu
        {
            get
            {
                return Strings.GetLocalizedString("FoneLink");
            }
        }

        public static string PauseSync
        {
            get
            {
                return Strings.GetLocalizedString("PauseSync");
            }
        }

        public static string ResumeSync
        {
            get
            {
                return Strings.GetLocalizedString("ResumeSync");
            }
        }

        public static string CheckForUpdates
        {
            get
            {
                return Strings.GetLocalizedString("CheckForUpdates");
            }
        }

        public static string DownloadingUpdates
        {
            get
            {
                return Strings.GetLocalizedString("DownloadingUpdates");
            }
        }

        public static string InstallUpdates
        {
            get
            {
                return Strings.GetLocalizedString("InstallUpdates");
            }
        }

        public static string Apps
        {
            get
            {
                return Strings.GetLocalizedString("Apps");
            }
        }

        public static string App
        {
            get
            {
                return Strings.GetLocalizedString("App");
            }
        }

        public static string Installed
        {
            get
            {
                return Strings.GetLocalizedString("Installed");
            }
        }

        public static string DiskUsageUnavailable
        {
            get
            {
                return Strings.GetLocalizedString("DiskUsageUnavailable");
            }
        }

        public static string Of
        {
            get
            {
                return Strings.GetLocalizedString("Of");
            }
        }

        public static string DiskUsed
        {
            get
            {
                return Strings.GetLocalizedString("DiskUsed");
            }
        }

        public static string BackButtonToolTip
        {
            get
            {
                return Strings.GetLocalizedString("BackButtonToolTip");
            }
        }

        public static string MenuButtonToolTip
        {
            get
            {
                return Strings.GetLocalizedString("MenuButtonToolTip");
            }
        }

        public static string CloseButtonToolTip
        {
            get
            {
                return Strings.GetLocalizedString("CloseButtonToolTip");
            }
        }

        public static string FullScreenButtonToolTip
        {
            get
            {
                return Strings.GetLocalizedString("FullScreenButtonToolTip");
            }
        }

        public static string ZoomOutButtonToolTip
        {
            get
            {
                return Strings.GetLocalizedString("ZoomOutButtonToolTip");
            }
        }

        public static string ZoomInButtonToolTip
        {
            get
            {
                return Strings.GetLocalizedString("ZoomInButtonToolTip");
            }
        }

        public static string SettingsButtonToolTip
        {
            get
            {
                return Strings.GetLocalizedString("SettingsButtonToolTip");
            }
        }

        public static string HomeButtonToolTip
        {
            get
            {
                return Strings.GetLocalizedString("HomeButtonToolTip");
            }
        }

        public static string ShareButtonToolTip
        {
            get
            {
                return Strings.GetLocalizedString("ShareButtonToolTip");
            }
        }

        public static string ResizeMessageBoxCaption
        {
            get
            {
                return Strings.GetLocalizedString("ResizeMessageBoxCaption");
            }
        }

        public static string ResizeMessageBoxText
        {
            get
            {
                return Strings.GetLocalizedString("ResizeMessageBoxText");
            }
        }

        public static string Initializing
        {
            get
            {
                return Strings.GetLocalizedString("Initializing");
            }
        }

        public static string InitializingGame
        {
            get
            {
                return Strings.GetLocalizedString("InitializingGame");
            }
        }

        public static string DownloadingGameData
        {
            get
            {
                return Strings.GetLocalizedString("DownloadingGameData");
            }
        }

        public static string CanNotStart
        {
            get
            {
                return Strings.GetLocalizedString("CanNotStart");
            }
        }

        public static string NetworkAvailableIconText
        {
            get
            {
                return Strings.GetLocalizedString("NetworkAvailableIconText");
            }
        }

        public static string NetworkUnavailableIconText
        {
            get
            {
                return Strings.GetLocalizedString("NetworkUnavailableIconText");
            }
        }

        public static string PostToWallLink
        {
            get
            {
                return Strings.GetLocalizedString("PostToWallLink");
            }
        }

        public static string PostToWallPicture
        {
            get
            {
                return Strings.GetLocalizedString("PostToWallPicture");
            }
        }

        public static string PostToWallName
        {
            get
            {
                return Strings.GetLocalizedString("PostToWallName");
            }
        }

        public static string PostToWallCaption
        {
            get
            {
                return Strings.GetLocalizedString("PostToWallCaption");
            }
        }

        public static string PostToWallDescription
        {
            get
            {
                return Strings.GetLocalizedString("PostToWallDescription");
            }
        }

        public static string FacebookWindowTitle
        {
            get
            {
                return Strings.GetLocalizedString("FacebookWindowTitle");
            }
        }

        public static string FacebookLoginWindowText
        {
            get
            {
                return Strings.GetLocalizedString("FacebookLoginWindowText");
            }
        }

        public static string PostToFacebookWindowText
        {
            get
            {
                return Strings.GetLocalizedString("PostToFacebookWindowText");
            }
        }

        public static string NoInternetDuringFBConnect
        {
            get
            {
                return Strings.GetLocalizedString("NoInternetDuringFBConnect");
            }
        }

        public static string BalloonTitle
        {
            get
            {
                return Strings.GetLocalizedString("BalloonTitle");
            }
        }

        public static string MessageBoxTitle
        {
            get
            {
                return Strings.GetLocalizedString("MessageBoxTitle");
            }
        }

        public static string MessageBoxText
        {
            get
            {
                return Strings.GetLocalizedString("MessageBoxText");
            }
        }

        public static string OKButtonText
        {
            get
            {
                return Strings.GetLocalizedString("OKButtonText");
            }
        }

        public static string CancelButtonText
        {
            get
            {
                return Strings.GetLocalizedString("CancelButtonText");
            }
        }

        public static string UninstallWindowTitle
        {
            get
            {
                return Strings.GetLocalizedString("UninstallWindowTitle");
            }
        }

        public static string InstallSuccess
        {
            get
            {
                return Strings.GetLocalizedString("InstallSuccess");
            }
        }

        public static string UninstallSuccess
        {
            get
            {
                return Strings.GetLocalizedString("UninstallSuccess");
            }
        }

        public static string UninstallFailed
        {
            get
            {
                return Strings.GetLocalizedString("UninstallFailed");
            }
        }

        public static string UninstallingWait
        {
            get
            {
                return Strings.GetLocalizedString("UninstallingWait");
            }
        }

        public static string GpsWindowTitle
        {
            get
            {
                return Strings.GetLocalizedString("GpsWindowTitle");
            }
        }

        public static string CloudConnectTitle
        {
            get
            {
                return Strings.GetLocalizedString("CloudConnectTitle");
            }
        }

        public static string CloudConnectedMsg
        {
            get
            {
                return Strings.GetLocalizedString("CloudConnectedMsg");
            }
        }

        public static string CloudDisconnectedMsg
        {
            get
            {
                return Strings.GetLocalizedString("CloudDisconnectedMsg");
            }
        }

        public static string InsufficientStorageMessage
        {
            get
            {
                return Strings.GetLocalizedString("InsufficientStorageMessage");
            }
        }

        public static string InstallFail
        {
            get
            {
                return Strings.GetLocalizedString("InstallFail");
            }
        }

        public static string UserWaitText
        {
            get
            {
                return Strings.GetLocalizedString("UserWaitText");
            }
        }

        public static string FullScreenToastText
        {
            get
            {
                return Strings.GetLocalizedString("FullScreenToastText");
            }
        }

        public static string SnapshotErrorToastText
        {
            get
            {
                return Strings.GetLocalizedString("SnapshotErrorToastText");
            }
        }

        public static string GraphicsDriverOutdatedError
        {
            get
            {
                return Strings.GetLocalizedString("GraphicsDriverOutdatedError");
            }
        }

        public static string GraphicsDriverOutdatedWarning
        {
            get
            {
                return Strings.GetLocalizedString("GraphicsDriverOutdatedWarning");
            }
        }

        public static string GraphicsDriverUpdatedMessage
        {
            get
            {
                return Strings.GetLocalizedString("GraphicsDriverUpdatedMessage");
            }
        }

        public static string NC_ONLY_FORM_TEXT
        {
            get
            {
                return Strings.GetLocalizedString("NC_ONLY_FORM_TEXT");
            }
        }

        public static string FORM_TEXT
        {
            get
            {
                return Strings.GetLocalizedString("FORM_TEXT");
            }
        }

        public static string BUTTON_TEXT
        {
            get
            {
                return Strings.GetLocalizedString("BUTTON_TEXT");
            }
        }

        public static string EMAIL_LABEL
        {
            get
            {
                return Strings.GetLocalizedString("EMAIL_LABEL");
            }
        }

        public static string ZENDESK_ID_TEXT
        {
            get
            {
                return Strings.GetLocalizedString("ZENDESK_ID_TEXT");
            }
        }

        public static string DESCRIPTION_LABEL
        {
            get
            {
                return Strings.GetLocalizedString("DESCRIPTION_LABEL");
            }
        }

        public static string STATUS_INITIAL
        {
            get
            {
                return Strings.GetLocalizedString("STATUS_INITIAL");
            }
        }

        public static string STATUS_SENDING
        {
            get
            {
                return Strings.GetLocalizedString("STATUS_SENDING");
            }
        }

        public static string STATUS_COLLECTING_PRODUCT
        {
            get
            {
                return Strings.GetLocalizedString("STATUS_COLLECTING_PRODUCT");
            }
        }

        public static string STATUS_COLLECTING_HOST
        {
            get
            {
                return Strings.GetLocalizedString("STATUS_COLLECTING_HOST");
            }
        }

        public static string STATUS_COLLECTING_GUEST
        {
            get
            {
                return Strings.GetLocalizedString("STATUS_COLLECTING_GUEST");
            }
        }

        public static string STATUS_ARCHIVING
        {
            get
            {
                return Strings.GetLocalizedString("STATUS_ARCHIVING");
            }
        }

        public static string APP_NAME
        {
            get
            {
                return Strings.GetLocalizedString("APP_NAME");
            }
        }

        public static string FINISH_CAPT
        {
            get
            {
                return Strings.GetLocalizedString("FINISH_CAPT");
            }
        }

        public static string FINISH_TEXT
        {
            get
            {
                return Strings.GetLocalizedString("FINISH_TEXT");
            }
        }

        public static string PROMPT_TEXT
        {
            get
            {
                return Strings.GetLocalizedString("PROMPT_TEXT");
            }
        }

        public static string DESC_MISSING_TEXT
        {
            get
            {
                return Strings.GetLocalizedString("DESC_MISSING_TEXT");
            }
        }

        public static string ZENDESK_INVALID_ID_TEXT
        {
            get
            {
                return Strings.GetLocalizedString("ZENDESK_INVALID_ID_TEXT");
            }
        }

        public static string SELECT_CATEGORY_TEXT
        {
            get
            {
                return Strings.GetLocalizedString("SELECT_CATEGORY_TEXT");
            }
        }

        public static string EMAIL_MISSING_TEXT
        {
            get
            {
                return Strings.GetLocalizedString("EMAIL_MISSING_TEXT");
            }
        }

        public static string RPC_FORM_TEXT
        {
            get
            {
                return Strings.GetLocalizedString("RPC_FORM_TEXT");
            }
        }

        public static string RPC_WORK_DONE_TEXT
        {
            get
            {
                return Strings.GetLocalizedString("RPC_WORK_DONE_TEXT");
            }
        }

        public static string RPC_PROGRESS_TEXT
        {
            get
            {
                return Strings.GetLocalizedString("RPC_PROGRESS_TEXT");
            }
        }

        public static string RPC_TROUBLESHOOTER_TEXT
        {
            get
            {
                return Strings.GetLocalizedString("RPC_TROUBLESHOOTER_TEXT");
            }
        }

        public static string LOGCOLLECTOR_RUNNING_TEXT
        {
            get
            {
                return Strings.GetLocalizedString("LOGCOLLECTOR_RUNNING_TEXT");
            }
        }

        public static string LOGCOLLECTOR_PROBLEMS
        {
            get
            {
                return Strings.GetLocalizedString("LOGCOLLECTOR_PROBLEMS");
            }
        }

        public static string APP_MISSING_TEXT
        {
            get
            {
                return Strings.GetLocalizedString("APP_MISSING_TEXT");
            }
        }

        public static string ZIP_NAME
        {
            get
            {
                return Strings.GetLocalizedString("ZIP_NAME");
            }
        }

        public static void InitLocalization()
        {
            Strings.s_ResourceLocation = Path.Combine(BlueStacks.hyperDroid.Common.Strings.BstCommonAppData, "Locales");
            Strings.s_Resource = ResourceManager.CreateFileBasedResourceManager(Strings.s_ResourceBaseName, Strings.s_ResourceLocation, null);
            Strings.s_Ci = Thread.CurrentThread.CurrentCulture;
        }

        public static string GetLocalizedString(string id)
        {
            string @string = Strings.s_Resource.GetString(id, Strings.s_Ci);
            if (string.IsNullOrEmpty(@string))
            {
                return id;
            }
            return @string;
        }

        public static string GetEnglishString(string id)
        {
            string @string = Strings.s_Resource.GetString(id, Strings.s_en_US);
            if (string.IsNullOrEmpty(@string))
            {
                return id;
            }
            return @string;
        }

        public static void CheckForLocaleUpdates()
        {
            string name = CultureInfo.CurrentCulture.Name;
            if (!name.ToLower().StartsWith("en"))
            {
                string path = Strings.s_ResourceBaseName + "." + name + ".Resources";
                string resourceFileName = Path.Combine(Strings.s_ResourceLocation, path);
                if (!File.Exists(resourceFileName))
                {
                    Thread thread = new Thread((ThreadStart)delegate
                    {
                        try
                        {
                            Strings.DownloadResourceFile(resourceFileName);
                        }
                        catch (Exception ex)
                        {
                            Logger.Error("Failed to download resource file: {0}. err: {1}", resourceFileName, ex.ToString());
                        }
                    });
                    thread.IsBackground = true;
                    thread.Start();
                }
            }
        }

        public static void DownloadResourceFile(string resourceFileName)
        {
            RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(Strings.s_HostKeyPath);
            string arg = (string)registryKey.GetValue("Host");
            string url = arg + "/" + BlueStacks.hyperDroid.Common.Strings.LocaleResourceUrl;
            string text = null;
            string text2 = null;
            string text3 = null;
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            dictionary.Add("x_culture_name", CultureInfo.CurrentCulture.Name.ToLower());
            text = Client.Get(url, dictionary, false);
            if (text == null)
            {
                Logger.Error("Failed to get language resource. No resp from server.");
            }
            else
            {
                string input = text;
                JSonReader jSonReader = new JSonReader();
                IJSonObject iJSonObject = jSonReader.ReadAsJSonObject(input);
                text2 = iJSonObject["success"].StringValue.Trim();
                text3 = iJSonObject["reason"].StringValue.Trim();
                if (string.Compare(text2, "false", true) == 0)
                {
                    Logger.Info("Failed to get language resource: " + text3);
                }
                else
                {
                    string text4 = iJSonObject["url"].StringValue.Trim();
                    Logger.Info("Downloading locale file from: " + text4);
                    WebClient webClient = new WebClient();
                    webClient.DownloadFile(text4, resourceFileName);
                    MessageBox.Show("Support for your current language: '" + CultureInfo.CurrentCulture.NativeName + "' is available. Please restart BlueStacks to apply language pack changes.", "BlueStacks language support available");
                }
            }
        }
    }
}
