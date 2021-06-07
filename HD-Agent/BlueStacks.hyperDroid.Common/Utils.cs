using BlueStacks.hyperDroid.Common.HTTP;
using BlueStacks.hyperDroid.Device;
using CodeTitans.JSon;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.ServiceProcess;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace BlueStacks.hyperDroid.Common
{
    internal class Utils
    {
        public class CmdRes
        {
            public string StdOut = "";

            public string StdErr = "";
        }

        private const int SM_TABLETPC = 86;

        private static string s_BstHKLMPath = "SOFTWARE\\BlueStacks";

        private static string s_BstHKCUCloudPath = "Software\\BlueStacks\\Agent\\Cloud";

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
        }

        public static int GlMode
        {
            get
            {
                string hKLMConfigRegKeyPath = Strings.HKLMConfigRegKeyPath;
                using (RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(hKLMConfigRegKeyPath))
                {
                    if (registryKey == null)
                    {
                        return 0;
                    }
                    return (int)registryKey.GetValue("GlMode", 0);
                }
            }
        }

        public static int ForceVMLegacyMode
        {
            get
            {
                string hKLMConfigRegKeyPath = Strings.HKLMConfigRegKeyPath;
                using (RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(hKLMConfigRegKeyPath))
                {
                    if (registryKey == null)
                    {
                        return 0;
                    }
                    return (int)registryKey.GetValue("ForceVMLegacyMode", 0);
                }
            }
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool IsWow64Process(IntPtr proc, ref bool isWow);

        [DllImport("urlmon.dll", CharSet = CharSet.Auto)]
        private static extern uint FindMimeFromData(uint pBC, [MarshalAs(UnmanagedType.LPStr)] string pwzUrl, [MarshalAs(UnmanagedType.LPArray)] byte[] pBuffer, uint cbSize, [MarshalAs(UnmanagedType.LPStr)] string pwzMimeProposed, uint dwMimeFlags, out uint ppwzMimeOut, uint dwReserverd);

        [DllImport("user32.dll")]
        private static extern int GetSystemMetrics(int smIndex);

        public static bool IsDesktopPC()
        {
            return Utils.GetSystemMetrics(86) == 0;
        }

        public static bool IsOSWinXP()
        {
            return Environment.OSVersion.Version.Major == 5;
        }

        public static bool IsOSVista()
        {
            if (Environment.OSVersion.Version.Major == 6)
            {
                return Environment.OSVersion.Version.Minor == 0;
            }
            return false;
        }

        public static bool IsOSWin7()
        {
            if (Environment.OSVersion.Version.Major == 6)
            {
                return Environment.OSVersion.Version.Minor == 1;
            }
            return false;
        }

        public static bool IsOSWin8()
        {
            if (Environment.OSVersion.Version.Major == 6)
            {
                return Environment.OSVersion.Version.Minor == 2;
            }
            return false;
        }

        public static bool IsOSWin81()
        {
            if (Environment.OSVersion.Version.Major == 6)
            {
                return Environment.OSVersion.Version.Minor == 3;
            }
            return false;
        }

        public static bool IsProxyEnabled(out string proxy)
        {
            Uri uri = new Uri("http://www.bluestacks.com/");
            IWebProxy systemWebProxy = WebRequest.GetSystemWebProxy();
            bool flag = systemWebProxy.IsBypassed(uri);
            Uri proxy2 = systemWebProxy.GetProxy(uri);
            proxy = proxy2.ToString();
            return !flag;
        }

        public static bool GetOSInfo(out string osName, out string servicePack, out string osArch)
        {
            osName = "";
            servicePack = "";
            osArch = "";
            OperatingSystem oSVersion = Environment.OSVersion;
            System.Version version = oSVersion.Version;
            if (oSVersion.Platform == PlatformID.Win32Windows)
            {
                switch (version.Minor)
                {
                    case 0:
                        osName = "95";
                        break;
                    case 10:
                        if (version.Revision.ToString() == "2222A")
                        {
                            osName = "98SE";
                        }
                        else
                        {
                            osName = "98";
                        }
                        break;
                    case 90:
                        osName = "Me";
                        break;
                }
            }
            else if (oSVersion.Platform == PlatformID.Win32NT)
            {
                switch (version.Major)
                {
                    case 3:
                        osName = "NT 3.51";
                        break;
                    case 4:
                        osName = "NT 4.0";
                        break;
                    case 5:
                        if (version.Minor == 0)
                        {
                            osName = "2000";
                        }
                        else
                        {
                            osName = "XP";
                        }
                        break;
                    case 6:
                        if (version.Minor == 0)
                        {
                            osName = "Vista";
                        }
                        else
                        {
                            osName = "7";
                        }
                        break;
                }
            }
            string text = osName;
            if (text != "")
            {
                text = "Windows " + text;
                if (oSVersion.ServicePack != "")
                {
                    servicePack = oSVersion.ServicePack.Substring(oSVersion.ServicePack.LastIndexOf(' ') + 1);
                    text = text + " " + oSVersion.ServicePack;
                }
                osArch = Utils.getOSArchitecture().ToString() + "-bit";
                text = text + " " + osArch;
                Logger.Info("Operating system details: " + text);
                return true;
            }
            return false;
        }

        public static bool IsOs64Bit()
        {
            Process currentProcess = Process.GetCurrentProcess();
            bool result = false;
            if (!Utils.IsWow64Process(currentProcess.Handle, ref result))
            {
                throw new Exception("Could not get os arch info.");
            }
            return result;
        }

        public static int getOSArchitecture()
        {
            string environmentVariable = Environment.GetEnvironmentVariable("PROCESSOR_ARCHITECTURE");
            if (!string.IsNullOrEmpty(environmentVariable) && string.Compare(environmentVariable, 0, "x86", 0, 3, true) != 0)
            {
                return 64;
            }
            return 32;
        }

        public static Dictionary<string, string> GetUserData()
        {
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            string text = "";
            try
            {
                using (RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(Utils.s_BstHKLMPath))
                {
                    text = (string)registryKey.GetValue("Version", "");
                }
            }
            catch (Exception)
            {
            }
            string text2 = "";
            try
            {
                using (RegistryKey registryKey2 = Registry.LocalMachine.OpenSubKey(Utils.s_BstHKCUCloudPath))
                {
                    text2 = (string)registryKey2.GetValue("Email", "");
                }
            }
            catch (Exception)
            {
            }
            if (text != "")
            {
                dictionary.Add("upgrade_ver", text);
            }
            if (text2 != "")
            {
                dictionary.Add("email", text2);
            }
            long num = DateTime.UtcNow.Ticks - 621355968000000000L;
            num /= 10000000;
            string value = num.ToString();
            dictionary.Add("user_time", value);
            return dictionary;
        }

        public static bool FindProcessByName(string name)
        {
            Process[] processesByName = Process.GetProcessesByName(name);
            return processesByName.Length != 0;
        }

        public static void KillProcessByName(string name)
        {
            Process[] processesByName = Process.GetProcessesByName(name);
            Process[] array = processesByName;
            foreach (Process process in array)
            {
                Logger.Info("Killing PID " + process.Id + " -> " + process.ProcessName);
                try
                {
                    process.Kill();
                }
                catch (Exception ex)
                {
                    Logger.Error(ex.ToString());
                    continue;
                }
                if (!process.WaitForExit(5000))
                {
                    Logger.Info("Timeout waiting for process to die");
                }
            }
        }

        public static void KillProcessesByName(string[] nameList)
        {
            foreach (string name in nameList)
            {
                Utils.KillProcessByName(name);
            }
        }

        public static bool StartServiceIfNeeded()
        {
            string name = "bsthdandroidsvc";
            ServiceController serviceController = new ServiceController(name);
            if (serviceController.Status != ServiceControllerStatus.Stopped && serviceController.Status != ServiceControllerStatus.StopPending)
            {
                return true;
            }
            string name2 = "Software\\BlueStacks\\Guests\\Android\\Config";
            using (RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(name2, true))
            {
                registryKey.SetValue("ServiceStoppedGracefully", 1, RegistryValueKind.DWord);
                registryKey.Flush();
            }
            Logger.Info("Utils: Starting service");
            Utils.EnableService(name);
            serviceController.Start();
            serviceController.WaitForStatus(ServiceControllerStatus.Running);
            if (!Utils.IsGlHotAttach())
            {
                Utils.StartHiddenFrontend();
            }
            return false;
        }

        public static void EnableService(string name)
        {
            string prog = "sc";
            string args = "config " + name + " start= auto";
            Utils.RunCmd(prog, args, null);
        }

        public static void StopServiceNoWait(string name)
        {
            Utils.StopServiceByName(name, true);
        }

        public static void StopService(string name)
        {
            Utils.StopServiceByName(name, false);
        }

        public static void StopServiceByName(string name, bool noWait)
        {
            try
            {
                ServiceController serviceController = new ServiceController(name);
                if (serviceController.Status == ServiceControllerStatus.Running)
                {
                    Logger.Info("Stopping " + name);
                    serviceController.Stop();
                    if (!noWait)
                    {
                        serviceController.WaitForStatus(ServiceControllerStatus.Stopped, new TimeSpan(0, 0, 10));
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to stop service {0}", name);
                Logger.Error(ex.ToString());
            }
        }

        public static bool IsServiceRunning(string svcName)
        {
            ServiceController serviceController = new ServiceController(svcName);
            if (serviceController.Status != ServiceControllerStatus.Running && serviceController.Status != ServiceControllerStatus.StartPending)
            {
                return false;
            }
            return true;
        }

        public static bool IsAlreadyRunning(string name, out Mutex lck)
        {
            bool flag = false;
            try
            {
                lck = new Mutex(true, name, out flag);
            }
            catch (UnauthorizedAccessException ex)
            {
                lck = null;
                Logger.Error(ex.Message);
                return true;
            }
            if (!flag)
            {
                lck.Close();
                lck = null;
            }
            return !flag;
        }

        public static string UserAgent(string tag)
        {
            string str = string.Format("{0}/{1}/{2}", "BlueStacks", "0.9.4.4078", tag);
            if (Features.IsFeatureEnabled(251658240u))
            {
                str += "/GE";
            }
            str += " gzip";
            Logger.Debug("UserAgent = " + str);
            return str;
        }

        public static void StartHiddenFrontend()
        {
            RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("Software\\BlueStacks");
            string path = (string)registryKey.GetValue("InstallDir");
            string fileName = Path.Combine(path, "HD-RunApp.exe");
            Process process = new Process();
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.FileName = fileName;
            process.StartInfo.Arguments = string.Format("-h");
            Logger.Info("Utils: Starting hidden Frontend");
            process.Start();
        }

        public static string GetMD5HashFromFile(string fileName)
        {
            FileStream fileStream = new FileStream(fileName, FileMode.Open);
            MD5 mD = new MD5CryptoServiceProvider();
            byte[] array = mD.ComputeHash(fileStream);
            fileStream.Close();
            StringBuilder stringBuilder = new StringBuilder();
            for (int i = 0; i < array.Length; i++)
            {
                stringBuilder.Append(array[i].ToString("x2"));
            }
            return stringBuilder.ToString();
        }

        public static string GetSystemFontName()
        {
            try
            {
                new Font("Arial", 8f, FontStyle.Regular, GraphicsUnit.Point, 0);
                return "Arial";
            }
            catch (Exception)
            {
                Label label = new Label();
                try
                {
                    new Font(label.Font.Name, 8f, FontStyle.Regular, GraphicsUnit.Point, 0);
                }
                catch (Exception)
                {
                    MessageBox.Show("Failed to load Font set.", "BlueStacks instance failed.", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                    Environment.Exit(-1);
                }
                return label.Font.Name;
            }
        }

        public static long GetContentSize(string downloadURL)
        {
            HttpWebRequest httpWebRequest = WebRequest.Create(downloadURL) as HttpWebRequest;
            httpWebRequest.Method = "HEAD";
            HttpWebResponse httpWebResponse = httpWebRequest.GetResponse() as HttpWebResponse;
            long contentLength = httpWebResponse.ContentLength;
            httpWebResponse.Close();
            return contentLength;
        }

        public static bool IsResumeSupported(string downloadURL)
        {
            HttpWebRequest httpWebRequest = WebRequest.Create(downloadURL) as HttpWebRequest;
            httpWebRequest.AddRange(0);
            httpWebRequest.Method = "HEAD";
            HttpWebResponse httpWebResponse = httpWebRequest.GetResponse() as HttpWebResponse;
            HttpStatusCode statusCode = httpWebResponse.StatusCode;
            httpWebResponse.Close();
            if (statusCode == HttpStatusCode.PartialContent)
            {
                return true;
            }
            return false;
        }

        public static bool IsBlueStacksInstalled()
        {
            try
            {
                Logger.Info("Checking for existing BlueStacks installation...");
                using (RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("Software\\BlueStacks"))
                {
                    string value = (string)registryKey.GetValue("Version", "");
                    if (!string.IsNullOrEmpty(value))
                    {
                        return true;
                    }
                }
            }
            catch (Exception)
            {
            }
            return false;
        }

        public static bool IsOEMBlueStacks()
        {
            return string.Compare(Utils.OEM, "bluestacks", true) == 0;
        }

        public static bool IsOEM(string oem)
        {
            return string.Compare(Utils.OEM, oem, true) == 0;
        }

        public static void UpdateOEM(string newOEM)
        {
            RegistryKey registryKey = Registry.LocalMachine.CreateSubKey(Strings.AndroidKeyBasePath);
            string input = (string)registryKey.GetValue("BootParameters");
            string replacement = "OEM=" + newOEM;
            Regex regex = new Regex("OEM=\\w+");
            string value = regex.Replace(input, replacement);
            registryKey.SetValue("BootParameters", value);
            registryKey.Flush();
            registryKey.Close();
            RegistryKey registryKey2 = Registry.LocalMachine.CreateSubKey(Strings.HKLMConfigRegKeyPath);
            registryKey2.SetValue("OEM", newOEM);
            registryKey2.Flush();
            registryKey2.Close();
        }

        public static void AddUploadTextToImage(string inputImage, string outputImage)
        {
            Image image = Image.FromFile(inputImage);
            int width = image.Width;
            int height = image.Height + 100;
            Bitmap bitmap = new Bitmap(width, height);
            Graphics graphics = Graphics.FromImage(bitmap);
            graphics.DrawImage(image, new Rectangle(0, 0, image.Width, image.Height), new Rectangle(0, 0, image.Width, image.Height), GraphicsUnit.Pixel);
            RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(Strings.RegBasePath);
            string path = (string)registryKey.GetValue("InstallDir");
            string filename = Path.Combine(path, "bluestacks.ico");
            Image image2 = Image.FromFile(filename);
            graphics.DrawImage(image2, new Rectangle(65, image.Height, 40, 40), new Rectangle(80, 0, image2.Width, 40), GraphicsUnit.Pixel);
            SolidBrush brush = new SolidBrush(Color.White);
            float width2 = (float)image.Width;
            float height2 = 80f;
            RectangleF layoutRectangle = new RectangleF(120f, (float)(image.Height + 7), width2, height2);
            Pen pen = new Pen(Color.Black);
            graphics.DrawRectangle(pen, 120f, (float)image.Height, width2, height2);
            graphics.DrawString("shared via BlueStacks App Player (www.bluestacks.com)", new Font("Arial", 14f), brush, layoutRectangle);
            graphics.Save();
            image.Dispose();
            bitmap.Save(outputImage, ImageFormat.Jpeg);
        }

        public static void KillAnotherFrontendInstance()
        {
            int id = Process.GetCurrentProcess().Id;
            Process[] processesByName = Process.GetProcessesByName("Hd-Frontend");
            Process[] array = processesByName;
            foreach (Process process in array)
            {
                if (process.Id != id)
                {
                    Utils.KillProcessByName(process.ProcessName);
                }
            }
        }

        public static bool IsP2DMEnabled()
        {
            try
            {
                RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(Strings.AndroidKeyBasePath);
                string text = (string)registryKey.GetValue("BootParameters");
                if (text.Contains("P2DM=1"))
                {
                    return true;
                }
            }
            catch (Exception)
            {
            }
            return false;
        }

        public static CmdRes RunCmd(string prog, string args, string outPath)
        {
            try
            {
                return Utils.RunCmdInternal(prog, args, outPath, true);
            }
            catch (Exception ex)
            {
                Logger.Error(ex.ToString());
            }
            return new CmdRes();
        }

        public static CmdRes RunCmdNoLog(string prog, string args, string outPath)
        {
            try
            {
                return Utils.RunCmdInternal(prog, args, outPath, false);
            }
            catch (Exception ex)
            {
                Logger.Error(ex.ToString());
            }
            return new CmdRes();
        }

        public static CmdRes RunCmdInternal(string prog, string args, string outPath, bool enableLog)
        {
            StreamWriter writer = null;
            Process proc = new Process();
            Logger.Info("Running Command");
            Logger.Info("    prog: " + prog);
            Logger.Info("    args: " + args);
            Logger.Info("    out:  " + outPath);
            CmdRes res = new CmdRes();
            proc.StartInfo.FileName = prog;
            proc.StartInfo.Arguments = args;
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.CreateNoWindow = true;
            if (outPath != null)
            {
                writer = new StreamWriter(outPath);
            }
            proc.StartInfo.RedirectStandardInput = true;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.StartInfo.RedirectStandardError = true;
            proc.OutputDataReceived += delegate(object obj, DataReceivedEventArgs line)
            {
                if (outPath != null)
                {
                    writer.WriteLine(line.Data);
                }
                string data = line.Data;
                if (data != null && (data = data.Trim()) != string.Empty)
                {
                    if (enableLog)
                    {
                        Logger.Info(proc.Id + " OUT: " + data);
                    }
                    CmdRes cmdRes2 = res;
                    cmdRes2.StdOut = cmdRes2.StdOut + data + "\n";
                }
            };
            proc.ErrorDataReceived += delegate(object obj, DataReceivedEventArgs line)
            {
                if (outPath != null)
                {
                    writer.WriteLine(line.Data);
                }
                if (enableLog)
                {
                    Logger.Error(proc.Id + " ERR: " + line.Data);
                }
                CmdRes cmdRes = res;
                cmdRes.StdErr = cmdRes.StdErr + line.Data + "\n";
            };
            proc.Start();
            proc.BeginOutputReadLine();
            proc.BeginErrorReadLine();
            proc.WaitForExit();
            if (outPath != null)
            {
                writer.Close();
            }
            return res;
        }

        public static void RunCmdAsync(string prog, string args)
        {
            try
            {
                Utils.RunCmdAsyncInternal(prog, args);
            }
            catch (Exception ex)
            {
                Logger.Error(ex.ToString());
            }
        }

        public static void RunCmdAsyncInternal(string prog, string args)
        {
            Process process = new Process();
            Logger.Info("Running Command Async");
            Logger.Info("    prog: " + prog);
            Logger.Info("    args: " + args);
            process.StartInfo.FileName = prog;
            process.StartInfo.Arguments = args;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;
            process.Start();
        }

        public static int Unzip(string filePath, string targetPath)
        {
            RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("Software\\BlueStacks");
            string arg = (string)registryKey.GetValue("InstallDir");
            Process dumpInfoProc = new Process();
            dumpInfoProc.StartInfo.UseShellExecute = false;
            dumpInfoProc.StartInfo.CreateNoWindow = true;
            dumpInfoProc.StartInfo.RedirectStandardOutput = true;
            dumpInfoProc.StartInfo.RedirectStandardError = true;
            dumpInfoProc.StartInfo.FileName = "\"" + arg + "HD-unzip.exe\"";
            dumpInfoProc.StartInfo.Arguments = "-ov \"" + filePath + "\" -d \"" + targetPath + "\"";
            dumpInfoProc.OutputDataReceived += delegate(object sender, DataReceivedEventArgs line)
            {
                Logger.Info(dumpInfoProc.Id + " Unzip info OUT: " + line.Data);
            };
            dumpInfoProc.ErrorDataReceived += delegate(object sender, DataReceivedEventArgs line)
            {
                Logger.Error(dumpInfoProc.Id + " Unzip info ERR: " + line.Data);
            };
            Logger.Info("Starting unzip: " + dumpInfoProc.StartInfo.FileName + " " + dumpInfoProc.StartInfo.Arguments);
            dumpInfoProc.Start();
            dumpInfoProc.BeginOutputReadLine();
            dumpInfoProc.BeginErrorReadLine();
            dumpInfoProc.WaitForExit();
            Logger.Info(dumpInfoProc.StartInfo.FileName + " " + dumpInfoProc.StartInfo.Arguments + " ExitCode = " + dumpInfoProc.ExitCode);
            Process unzipProc = new Process();
            unzipProc.StartInfo.UseShellExecute = false;
            unzipProc.StartInfo.CreateNoWindow = true;
            unzipProc.StartInfo.RedirectStandardOutput = true;
            unzipProc.StartInfo.RedirectStandardError = true;
            unzipProc.StartInfo.FileName = "\"" + arg + "HD-unzip.exe\"";
            unzipProc.StartInfo.Arguments = "-o \"" + filePath + "\" -d \"" + targetPath + "\"";
            Logger.Info("Starting unzip: " + unzipProc.StartInfo.FileName + " " + unzipProc.StartInfo.Arguments);
            unzipProc.OutputDataReceived += delegate(object sender, DataReceivedEventArgs line)
            {
                Logger.Info(unzipProc.Id + " Unzip extract OUT: " + line.Data);
            };
            unzipProc.ErrorDataReceived += delegate(object sender, DataReceivedEventArgs line)
            {
                Logger.Error(unzipProc.Id + " Unzip extract ERR: " + line.Data);
            };
            Logger.Info("Starting unzip: " + unzipProc.StartInfo.FileName + " " + unzipProc.StartInfo.Arguments);
            unzipProc.Start();
            unzipProc.BeginOutputReadLine();
            unzipProc.BeginErrorReadLine();
            unzipProc.WaitForExit();
            unzipProc.StartInfo.Arguments = "-o \"" + filePath + "\" -d \"" + targetPath + "\"";
            Logger.Info(unzipProc.StartInfo.FileName + " " + unzipProc.StartInfo.Arguments + " ExitCode = " + unzipProc.ExitCode);
            return unzipProc.ExitCode;
        }

        public static void RestartBlueStacks()
        {
            RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("Software\\BlueStacks");
            string str = (string)registryKey.GetValue("InstallDir");
            ProcessStartInfo processStartInfo = new ProcessStartInfo();
            processStartInfo.FileName = str + "HD-Restart.exe";
            processStartInfo.Arguments = "Android";
            Process.Start(processStartInfo);
        }

        public static void GetWindowWidthAndHeight(int sWidth, int sHeight, out int width, out int height)
        {
            if (sWidth >= 1920 && sHeight >= 1080)
            {
                width = 1440;
                height = 900;
            }
            else if (sWidth > 1366 && sHeight > 768)
            {
                width = 1152;
                height = 720;
            }
            else
            {
                width = 960;
                height = 600;
            }
        }

        public static void GetGuestWidthAndHeight(int sWidth, int sHeight, out int width, out int height)
        {
            if (sWidth >= 1920 && sHeight >= 1080)
            {
                width = 1440;
                height = 900;
            }
            else
            {
                width = 1152;
                height = 720;
            }
        }

        public static string GetURLSafeBase64String(string originalString)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(originalString));
        }

        public static bool IsSignedByBlueStacks(string filePath)
        {
            Logger.Info("Checking if {0} is signed", filePath);
            Logger.Info("MD5 of file: " + Utils.GetMD5HashFromFile(filePath));
            bool result = false;
            try
            {
                X509Certificate certificate = X509Certificate.CreateFromSignedFile(filePath);
                X509Certificate2 x509Certificate = new X509Certificate2(certificate);
                string name = x509Certificate.IssuerName.Name;
                Logger.Debug("Certificate issuer name is: " + name);
                string name2 = x509Certificate.SubjectName.Name;
                Logger.Debug("Certificate issued by: " + name2);
                if (name2.Contains("Bluestack Systems, Inc."))
                {
                    Logger.Info("File signed by BlueStacks");
                    if (name.Contains("VeriSign, Inc."))
                    {
                        Logger.Info("Certficate issued by Verisign.");
                        if (x509Certificate.Verify())
                        {
                            Logger.Info("Certificate verified");
                            result = true;
                            return result;
                        }
                        Logger.Info("Certificate not verified");
                        return result;
                    }
                    Logger.Info("Certificate not issued by VeriSign");
                    return result;
                }
                Logger.Info("File not signed by BlueStacks");
                return result;
            }
            catch (Exception ex)
            {
                Logger.Error("File not signed");
                Logger.Error(ex.ToString());
                return result;
            }
        }

        public static bool IsValidEmail(string email)
        {
            string pattern = "^(([^<>()[\\]\\\\.,;:\\s@\\\"]+(\\.[^<>()[\\]\\\\.,;:\\s@\\\"]+)*)|(\\\".+\\\"))@((\\[[0-9]{1,3}\\.[0-9]{1,3}\\.[0-9]{1,3}\\.[0-9]{1,3}\\])|(([a-zA-Z\\-0-9]+\\.)+[a-zA-Z]{2,}))$";
            Regex regex = new Regex(pattern);
            return regex.IsMatch(email);
        }

        public static string GetMimeFromFile(string filename)
        {
            string result = "";
            if (!File.Exists(filename))
            {
                return result;
            }
            byte[] array = new byte[256];
            using (FileStream fileStream = new FileStream(filename, FileMode.Open))
            {
                if (fileStream.Length >= 256)
                {
                    fileStream.Read(array, 0, 256);
                }
                else
                {
                    fileStream.Read(array, 0, (int)fileStream.Length);
                }
            }
            try
            {
                uint num = default(uint);
                Utils.FindMimeFromData(0u, (string)null, array, 256u, (string)null, 0u, out num, 0u);
                IntPtr ptr = new IntPtr(num);
                result = Marshal.PtrToStringUni(ptr);
                Marshal.FreeCoTaskMem(ptr);
                return result;
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to get mime type. err: " + ex.Message);
                return result;
            }
        }

        public static bool IsSharedFolderEnabled()
        {
            RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(Strings.HKLMConfigRegKeyPath);
            if ((int)registryKey.GetValue("FileSystem", 0) == 0)
            {
                Logger.Info("Shared folders disabled");
                return false;
            }
            return true;
        }

        public static bool IsInstallTypeNCOnly()
        {
            RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(Strings.RegBasePath);
            string strA = (string)registryKey.GetValue("InstallType");
            if (string.Compare(strA, "nconly", true) == 0)
            {
                return true;
            }
            return false;
        }

        public static bool IsGlHotAttach()
        {
            RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("Software\\BlueStacks\\Guests\\Android\\Config");
            int num = (int)registryKey.GetValue("GlHotAttach", 0);
            Logger.Debug("GlHotAttach = {0}", num);
            registryKey.Close();
            return num != 0;
        }

        public static bool IsProcessAlive(int pid)
        {
            bool result = false;
            try
            {
                Process.GetProcessById(pid);
                result = true;
                return result;
            }
            catch (ArgumentException)
            {
                return result;
            }
        }

        public static bool IsProcessAlive(string lockName)
        {
            Mutex mutex = default(Mutex);
            if (Utils.IsAlreadyRunning(lockName, out mutex))
            {
                Logger.Info(lockName + " running.");
                return true;
            }
            mutex.Close();
            return false;
        }

        public static bool IsFileNullOrMissing(string file)
        {
            if (!File.Exists(file))
            {
                Logger.Info(file + " does not exist");
                return true;
            }
            FileInfo fileInfo = new FileInfo(file);
            if (fileInfo.Length == 0)
            {
                Logger.Info(file + " is null");
                return true;
            }
            return false;
        }

        public static bool IsDirectoryEmpty(string dir)
        {
            bool result = true;
            if (!Directory.Exists(dir))
            {
                Logger.Info(dir + " does not exist");
                return result;
            }
            string[] files = Directory.GetFiles(dir);
            if (files.Length == 0)
            {
                Logger.Info(dir + " is empty");
            }
            else
            {
                result = false;
            }
            string[] directories = Directory.GetDirectories(dir);
            string[] array = directories;
            foreach (string text in array)
            {
                files = Directory.GetFiles(text);
                if (!Utils.IsDirectoryEmpty(text))
                {
                    result = false;
                }
            }
            return result;
        }

        public static bool IsWebInstallation()
        {
            RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(Strings.RegBasePath);
            string strA = (string)registryKey.GetValue("InstallPlatform", "Desktop");
            if (string.Compare(strA, "web", true) == 0)
            {
                Logger.Info("Web Installation");
                return true;
            }
            return false;
        }

        public static bool IsRunningInSpawner()
        {
            string environmentVariable = Environment.GetEnvironmentVariable("SPAWNAPPS_APP_NAME");
            Logger.Info("SPAWNAPPS_APP_NAME = " + environmentVariable);
            if (!string.IsNullOrEmpty(environmentVariable))
            {
                return true;
            }
            return false;
        }

        public static bool IsGraphicsDriverUptodate(out string updateUrl, out string msgType, string guid)
        {
            Logger.Info("In IsGraphicsDriverUptodate");
            updateUrl = null;
            msgType = null;
            string text = "Software\\BlueStacks\\Guests\\Android\\Config";
            int num = 0;
            using (RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(text, true))
            {
                if (registryKey != null)
                {
                    num = (int)registryKey.GetValue("SkipGraphicsDriverCheck", 0);
                }
            }
            switch (num)
            {
                case 1:
                    Logger.Info("Skipping graphics driver version check");
                    return true;
                default:
                    {
                        string text2 = Strings.ChannelsProdUrl + "/" + Strings.CheckGraphicsDriverUrl;
                        Dictionary<string, string> dictionary;
                        try
                        {
                            dictionary = Profile.InfoForGraphicsDriverCheck();
                        }
                        catch (Exception ex)
                        {
                            Logger.Error("Error in InfoForGraphicsDriverCheck");
                            Logger.Error(ex.ToString());
                            return true;
                        }
                        if (guid != null)
                        {
                            dictionary.Add("guid", guid);
                        }
                        Logger.Info("data being posted: ");
                        foreach (KeyValuePair<string, string> item in dictionary)
                        {
                            Logger.Info("Key: " + item.Key + " Value: " + item.Value);
                        }
                        try
                        {
                            Logger.Info("Sending post request to " + text2);
                            string text3 = Client.Post(text2, dictionary, null, false);
                            Logger.Info("IsGraphicsDriverUptodate response: " + text3);
                            IJSonReader iJSonReader = new JSonReader();
                            IJSonObject iJSonObject = iJSonReader.ReadAsJSonObject(text3);
                            if (iJSonObject["result"].StringValue == "false")
                            {
                                Logger.Info("Driver out-of-date");
                                updateUrl = iJSonObject["url"].StringValue;
                                msgType = iJSonObject["msgtype"].StringValue;
                                using (RegistryKey registryKey2 = Registry.LocalMachine.CreateSubKey(text))
                                {
                                    registryKey2.SetValue("DriverUrl", updateUrl);
                                    registryKey2.SetValue("MsgType", msgType);
                                }
                                return false;
                            }
                            using (RegistryKey registryKey3 = Registry.LocalMachine.CreateSubKey(text))
                            {
                                registryKey3.DeleteValue("DriverUrl", false);
                                registryKey3.DeleteValue("MsgType", false);
                            }
                        }
                        catch (Exception ex2)
                        {
                            Logger.Error("Request failed");
                            using (RegistryKey registryKey4 = Registry.LocalMachine.OpenSubKey(text))
                            {
                                if (registryKey4.GetValue("DriverUrl") != null && registryKey4.GetValue("MsgType") != null)
                                {
                                    Logger.Info("Driver out-of-date");
                                    updateUrl = (string)registryKey4.GetValue("DriverUrl");
                                    msgType = (string)registryKey4.GetValue("MsgType");
                                    return false;
                                }
                            }
                            Logger.Error(ex2.ToString());
                            Logger.Info("Checking local data for newer driver");
                            if (!Utils.CheckLocalGraphicsDriverData(dictionary, out updateUrl, out msgType))
                            {
                                goto end_IL_0217;
                            }
                            Logger.Info("Driver out-of-date");
                            return false;
                        end_IL_0217: ;
                        }
                        Logger.Info("Could not find a newer driver");
                        return true;
                    }
            }
        }

        public static bool SecurePackagesInstalled(out string securePackages)
        {
            securePackages = "";
            try
            {
                string name = "bsthdandroidsvc";
                ServiceController serviceController = new ServiceController(name);
                if (serviceController.Status == ServiceControllerStatus.Stopped || serviceController.Status == ServiceControllerStatus.StopPending)
                {
                    int num = 0;
                    string name2 = "Software\\BlueStacks\\Guests\\Android\\Config";
                    using (RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(name2, true))
                    {
                        num = (int)registryKey.GetValue("ServiceStoppedGracefully", 0);
                        registryKey.Flush();
                    }
                    if (num == 0)
                    {
                        Logger.Info("Service not stopped gracefully. Can't show app list to user. Will upgrade.");
                        return false;
                    }
                    Logger.Info("ThinInstaller: Starting service");
                    serviceController.Start();
                    serviceController.WaitForStatus(ServiceControllerStatus.Running);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex.ToString());
                Logger.Info("Error when trying to start service. Will upgrade.");
                return false;
            }
            if (!Utils.WaitForBootComplete())
            {
                Logger.Info("Taking too long to boot. Will upgrade.");
                return false;
            }
            securePackages = Utils.GetSecurePackages();
            if (securePackages.Trim() == "")
            {
                Logger.Info("No secure packages. Will upgrade");
                return false;
            }
            return true;
        }

        public static string GetSecurePackages()
        {
            int num;
            try
            {
                num = GuestNetwork.GetHostPort(false, 5555);
            }
            catch (Exception ex)
            {
                Logger.Error(ex.ToString());
                num = 5555;
            }
            string arg = "localhost:" + num;
            string path = default(string);
            using (RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("Software\\BlueStacks"))
            {
                path = (string)registryKey.GetValue("InstallDir");
            }
            string prog = Path.Combine(path, "HD-Adb.exe");
            Utils.RunCmdAsync(prog, "start-server");
            Thread.Sleep(3000);
            Utils.RunCmdAsync(prog, "connect " + arg);
            Thread.Sleep(250);
            string args = "-s " + arg + " shell pm list packages -f | grep -i mnt | cut -d = -f 2";
            string text = "";
            try
            {
                CmdRes cmdRes = Utils.RunCmd(prog, args, null);
                return cmdRes.StdOut;
            }
            catch (Exception ex2)
            {
                Logger.Info(ex2.ToString());
                throw ex2;
            }
        }

        public static bool WaitForBootComplete()
        {
            string url = "http://127.0.0.1:" + VmCmdHandler.s_ServerPort + "/" + VmCmdHandler.s_PingPath;
            int num = 120;
            while (num > 0)
            {
                num--;
                try
                {
                    Client.Get(url, null, false, 1000);
                    Logger.Info("Guest finished booting");
                    Thread.Sleep(3000);
                    return true;
                }
                catch (Exception)
                {
                    Logger.Info("Guest not booted yet.");
                    Thread.Sleep(1000);
                }
            }
            return false;
        }

        private static bool CheckLocalGraphicsDriverData(Dictionary<string, string> deviceInfo, out string updateUrl, out string msgType)
        {
            bool result = false;
            updateUrl = null;
            msgType = null;
            try
            {
                GraphicsDriverData graphicsDriverData = new GraphicsDriverData();
                result = graphicsDriverData.FindDriver(deviceInfo, out updateUrl, out msgType);
                return result;
            }
            catch (Exception ex)
            {
                Logger.Error(ex.ToString());
                return result;
            }
        }

        public static void ExtractImages(string targetDir, string resourceName)
        {
            try
            {
                Directory.Delete(targetDir, true);
            }
            catch (Exception)
            {
            }
            if (!Directory.Exists(targetDir))
            {
                Directory.CreateDirectory(targetDir);
            }
            ResourceManager resourceManager;
            try
            {
                resourceManager = new ResourceManager(resourceName, Assembly.GetExecutingAssembly());
            }
            catch (Exception ex2)
            {
                Logger.Error("Failed to extract resources. err: " + ex2.ToString());
                return;
            }
            Image image = (Image)resourceManager.GetObject("bg");
            image.Save(Path.Combine(targetDir, "bg.jpg"), ImageFormat.Jpeg);
            bool flag = true;
            try
            {
                image = (Image)resourceManager.GetObject("HomeScreen");
                image.Save(Path.Combine(targetDir, "HomeScreen.jpg"), ImageFormat.Jpeg);
            }
            catch (Exception)
            {
                flag = false;
            }
            try
            {
                image = (Image)resourceManager.GetObject("ThankYouImage");
                image.Save(Path.Combine(targetDir, "ThankYouImage.jpg"), ImageFormat.Jpeg);
            }
            catch (Exception)
            {
            }
            int num = 0;
            try
            {
                while (true)
                {
                    num++;
                    image = (Image)resourceManager.GetObject("SetupImage" + Convert.ToString(num));
                    image.Save(Path.Combine(targetDir, "SetupImage" + Convert.ToString(num) + ".jpg"), ImageFormat.Jpeg);
                    if (!flag && num == 1)
                    {
                        image.Save(Path.Combine(targetDir, "HomeScreen.jpg"), ImageFormat.Jpeg);
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        public static string DownloadIcon(string iconPath, string packageName)
        {
            try
            {
                string text = "http://opasanet.appspot.com/op/appinfo?id=" + packageName;
                Logger.Info("Downloading app info from url: " + text);
                string input = Client.Get(text, null, true);
                JSonReader jSonReader = new JSonReader();
                IJSonObject iJSonObject = jSonReader.ReadAsJSonObject(input);
                string input2 = iJSonObject["json"].ToString();
                jSonReader = new JSonReader();
                IJSonObject iJSonObject2 = jSonReader.ReadAsJSonObject(input2);
                string text2 = iJSonObject2["icon_url"].StringValue.Trim();
                Logger.Info("Downloaded app icon from url: " + text2);
                WebClient webClient = new WebClient();
                webClient.DownloadFile(text2, iconPath);
                Logger.Info("Downloaded app icon at: " + iconPath);
                return iconPath;
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to download icon from web. Err: " + ex.ToString());
                iconPath = null;
                return iconPath;
            }
        }

        public static void ExitAgent()
        {
            RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(Strings.HKLMConfigRegKeyPath);
            int num = (int)registryKey.GetValue("AgentServerPort", 2861);
            string url = "http://127.0.0.1:" + num + "/" + Strings.ExitAgentUrl;
            try
            {
                Client.Get(url, null, false, 3000);
            }
            catch (Exception ex)
            {
                Logger.Error("Exception in ExitAgent");
                Logger.Error(ex.ToString());
                Utils.KillProcessByName("HD-Agent");
            }
        }
    }
}
