using System;
using System.Collections.Generic;
using System.Data;

namespace BlueStacks.hyperDroid.Common
{
    internal class GraphicsDriverData
    {
        public enum Column
        {
            driver_url,
            driver_version,
            gpu,
            gpu_vendor,
            os_arch,
            os_version,
            processor_family,
            processor_vendor
        }

        private DataTable graphicsDriverData;

        public GraphicsDriverData()
        {
            this.graphicsDriverData = new DataTable();
            this.graphicsDriverData.Columns.Add("driver_url", typeof(string));
            this.graphicsDriverData.Columns.Add("driver_version", typeof(string));
            this.graphicsDriverData.Columns.Add("gpu", typeof(string));
            this.graphicsDriverData.Columns.Add("gpu_vendor", typeof(string));
            this.graphicsDriverData.Columns.Add("os_arch", typeof(string));
            this.graphicsDriverData.Columns.Add("os_version", typeof(string));
            this.graphicsDriverData.Columns.Add("processor_family", typeof(string));
            this.graphicsDriverData.Columns.Add("processor_vendor", typeof(string));
            this.graphicsDriverData.Rows.Add("https://downloadcenter.intel.com/confirm.aspx?httpDown=http://downloadmirror.intel.com/22521/a08/Win7Vista_64_152258.exe&lang=eng&Dwnldid=22521&ProductID=3231&ProductFamily=Graphics&ProductLine=Laptop+graphics+drivers&ProductProduct=IntelAE+Core84C2AE+HD+Graphics", "8.15.10.2993", "hd graphics", "intel(r)", "64-bit", "windows 7", "core", "genuineintel");
            this.graphicsDriverData.Rows.Add("https://downloadcenter.intel.com/confirm.aspx?httpDown=http://downloadmirror.intel.com/23106/a08/Win64_153117.exe&lang=eng&Dwnldid=23106&ProductID=3711&ProductFamily=Graphics&ProductLine=Laptop+graphics+drivers&ProductProduct=3rd+Generation+IntelAE+Core84C2AE+HD+Graphics+2500", "9.17.10.2000", "hd graphics 5000", "intel(r)", "64-bit", "windows 8", "core", "genuineintel");
            this.graphicsDriverData.Rows.Add("https://downloadcenter.intel.com/confirm.aspx?httpDown=http://downloadmirror.intel.com/23106/a08/Win64_153117.exe&lang=eng&Dwnldid=23106&ProductID=3711&ProductFamily=Graphics&ProductLine=Laptop+graphics+drivers&ProductProduct=3rd+Generation+IntelAE+Core84C2AE+HD+Graphics+2500", "9.17.10.2000", "hd graphics 4400", "intel(r)", "64-bit", "windows 7", "core", "genuineintel");
            this.graphicsDriverData.Rows.Add("https://downloadcenter.intel.com/confirm.aspx?httpDown=http://downloadmirror.intel.com/23106/a08/Win64_153117.exe&lang=eng&Dwnldid=23106&ProductID=3711&ProductFamily=Graphics&ProductLine=Laptop+graphics+drivers&ProductProduct=3rd+Generation+IntelAE+Core84C2AE+HD+Graphics+2500", "9.17.10.2000", "hd graphics 4000", "intel(r)", "64-bit", "windows 7", "core", "genuineintel");
            this.graphicsDriverData.Rows.Add("https://downloadcenter.intel.com/confirm.aspx?httpDown=http://downloadmirror.intel.com/23102/a08/Win64_152818.exe&lang=eng&Dwnldid=23102&DownloadType=Drivers&ProductID=3319&ProductFamily=Graphics&ProductLine=Laptop+graphics+drivers&ProductProduct=2nd+Generation+IntelAE+Core84C22f2000", "9.17.10.2000", "hd graphics 2000", "intel(r)", "64-bit", "windows 8", "core", "genuineintel");
            this.graphicsDriverData.Rows.Add("https://downloadcenter.intel.com/confirm.aspx?httpDown=http://downloadmirror.intel.com/23105/a08/Win32_153117.exe&lang=eng&Dwnldid=23105&ProductID=3711&ProductFamily=Graphics&ProductLine=Laptop+graphics+drivers&ProductProduct=3rd+Generation+IntelAE+Core84C2AE+HD+Graphics+2500", "9.17.10.2000", "hd graphics 5100", "intel(r)", "32-bit", "windows 7", "core", "genuineintel");
            this.graphicsDriverData.Rows.Add("https://downloadcenter.intel.com/confirm.aspx?httpDown=http://downloadmirror.intel.com/23105/a08/Win32_153117.exe&lang=eng&Dwnldid=23105&ProductID=3711&ProductFamily=Graphics&ProductLine=Laptop+graphics+drivers&ProductProduct=3rd+Generation+IntelAE+Core84C2AE+HD+Graphics+2500", "9.17.10.2000", "hd graphics 5200", "intel(r)", "32-bit", "windows 8", "core", "genuineintel");
            this.graphicsDriverData.Rows.Add("https://downloadcenter.intel.com/confirm.aspx?httpDown=http://downloadmirror.intel.com/23106/a08/Win64_153117.exe&lang=eng&Dwnldid=23106&ProductID=3711&ProductFamily=Graphics&ProductLine=Laptop+graphics+drivers&ProductProduct=3rd+Generation+IntelAE+Core84C2AE+HD+Graphics+2500", "9.17.10.2000", "hd graphics 4200", "intel(r)", "64-bit", "windows 7", "core", "genuineintel");
            this.graphicsDriverData.Rows.Add("https://downloadcenter.intel.com/confirm.aspx?httpDown=http://downloadmirror.intel.com/23105/a08/Win32_153117.exe&lang=eng&Dwnldid=23105&ProductID=3711&ProductFamily=Graphics&ProductLine=Laptop+graphics+drivers&ProductProduct=3rd+Generation+IntelAE+Core84C2AE+HD+Graphics+2500", "9.17.10.2000", "hd graphics 4600", "intel(r)", "32-bit", "windows 7", "core", "genuineintel");
            this.graphicsDriverData.Rows.Add("https://downloadcenter.intel.com/confirm.aspx?httpDown=http://downloadmirror.intel.com/23102/a08/Win64_152818.exe&lang=eng&Dwnldid=23102&DownloadType=Drivers&ProductID=3319&ProductFamily=Graphics&ProductLine=Laptop+graphics+drivers&ProductProduct=2nd+Generation+IntelAE+Core84C22f2000", "9.17.10.2000", "hd graphics 3000", "intel(r)", "64-bit", "windows 8", "core", "genuineintel");
            this.graphicsDriverData.Rows.Add("https://downloadcenter.intel.com/confirm.aspx?httpDown=http://downloadmirror.intel.com/23101/a08/Win32_152818.exe&lang=eng&Dwnldid=23101&DownloadType=Drivers&ProductID=3319&ProductFamily=Graphics&ProductLine=Laptop+graphics+drivers&ProductProduct=2nd+Generation+IntelAE+Core84C22f2000", "9.17.10.2000", "hd graphics 3000", "intel(r)", "32-bit", "windows 8", "core", "genuineintel");
            this.graphicsDriverData.Rows.Add("https://downloadcenter.intel.com/confirm.aspx?httpDown=http://downloadmirror.intel.com/23106/a08/Win64_153117.exe&lang=eng&Dwnldid=23106&ProductID=3711&ProductFamily=Graphics&ProductLine=Laptop+graphics+drivers&ProductProduct=3rd+Generation+IntelAE+Core84C2AE+HD+Graphics+2500", "9.17.10.2000", "hd graphics 4400", "intel(r)", "64-bit", "windows 8", "core", "genuineintel");
            this.graphicsDriverData.Rows.Add("https://downloadcenter.intel.com/confirm.aspx?httpDown=http://downloadmirror.intel.com/23106/a08/Win64_153117.exe&lang=eng&Dwnldid=23106&ProductID=3711&ProductFamily=Graphics&ProductLine=Laptop+graphics+drivers&ProductProduct=3rd+Generation+IntelAE+Core84C2AE+HD+Graphics+2500", "9.17.10.2000", "hd graphics 4200", "intel(r)", "64-bit", "windows 8", "core", "genuineintel");
            this.graphicsDriverData.Rows.Add("https://downloadcenter.intel.com/confirm.aspx?httpDown=http://downloadmirror.intel.com/23105/a08/Win32_153117.exe&lang=eng&Dwnldid=23105&ProductID=3711&ProductFamily=Graphics&ProductLine=Laptop+graphics+drivers&ProductProduct=3rd+Generation+IntelAE+Core84C2AE+HD+Graphics+2500", "9.17.10.2000", "hd graphics 2500", "intel(r)", "32-bit", "windows 8", "core", "genuineintel");
            this.graphicsDriverData.Rows.Add("https://downloadcenter.intel.com/confirm.aspx?httpDown=http://downloadmirror.intel.com/23106/a08/Win64_153117.exe&lang=eng&Dwnldid=23106&ProductID=3711&ProductFamily=Graphics&ProductLine=Laptop+graphics+drivers&ProductProduct=3rd+Generation+IntelAE+Core84C2AE+HD+Graphics+2500", "9.17.10.2000", "hd graphics 4600", "intel(r)", "64-bit", "windows 7", "core", "genuineintel");
            this.graphicsDriverData.Rows.Add("https://downloadcenter.intel.com/confirm.aspx?httpDown=http://downloadmirror.intel.com/23105/a08/Win32_153117.exe&lang=eng&Dwnldid=23105&ProductID=3711&ProductFamily=Graphics&ProductLine=Laptop+graphics+drivers&ProductProduct=3rd+Generation+IntelAE+Core84C2AE+HD+Graphics+2500", "9.17.10.2000", "hd graphics 5000", "intel(r)", "32-bit", "windows 8", "core", "genuineintel");
            this.graphicsDriverData.Rows.Add("https://downloadcenter.intel.com/confirm.aspx?httpDown=http://downloadmirror.intel.com/23106/a08/Win64_153117.exe&lang=eng&Dwnldid=23106&ProductID=3711&ProductFamily=Graphics&ProductLine=Laptop+graphics+drivers&ProductProduct=3rd+Generation+IntelAE+Core84C2AE+HD+Graphics+2500", "9.17.10.2000", "hd graphics 4000", "intel(r)", "64-bit", "windows 8", "core", "genuineintel");
            this.graphicsDriverData.Rows.Add("https://downloadcenter.intel.com/confirm.aspx?httpDown=http://downloadmirror.intel.com/23106/a08/Win64_153117.exe&lang=eng&Dwnldid=23106&ProductID=3711&ProductFamily=Graphics&ProductLine=Laptop+graphics+drivers&ProductProduct=3rd+Generation+IntelAE+Core84C2AE+HD+Graphics+2500", "9.17.10.2000", "hd graphics 5200", "intel(r)", "64-bit", "windows 8", "core", "genuineintel");
            this.graphicsDriverData.Rows.Add("https://downloadcenter.intel.com/confirm.aspx?httpDown=http://downloadmirror.intel.com/23101/a08/Win32_152818.exe&lang=eng&Dwnldid=23101&DownloadType=Drivers&ProductID=3319&ProductFamily=Graphics&ProductLine=Laptop+graphics+drivers&ProductProduct=2nd+Generation+IntelAE+Core84C22f2000", "9.17.10.2000", "hd graphics 2000", "intel(r)", "32-bit", "windows 7", "core", "genuineintel");
            this.graphicsDriverData.Rows.Add("https://downloadcenter.intel.com/confirm.aspx?httpDown=http://downloadmirror.intel.com/23105/a08/Win32_153117.exe&lang=eng&Dwnldid=23105&ProductID=3711&ProductFamily=Graphics&ProductLine=Laptop+graphics+drivers&ProductProduct=3rd+Generation+IntelAE+Core84C2AE+HD+Graphics+2500", "9.17.10.2000", "hd graphics 4600", "intel(r)", "32-bit", "windows 8", "core", "genuineintel");
            this.graphicsDriverData.Rows.Add("https://downloadcenter.intel.com/confirm.aspx?httpDown=http://downloadmirror.intel.com/23105/a08/Win32_153117.exe&lang=eng&Dwnldid=23105&ProductID=3711&ProductFamily=Graphics&ProductLine=Laptop+graphics+drivers&ProductProduct=3rd+Generation+IntelAE+Core84C2AE+HD+Graphics+2500", "9.17.10.2000", "hd graphics 5000", "intel(r)", "32-bit", "windows 7", "core", "genuineintel");
            this.graphicsDriverData.Rows.Add("https://downloadcenter.intel.com/confirm.aspx?httpDown=http://downloadmirror.intel.com/23101/a08/Win32_152818.exe&lang=eng&Dwnldid=23101&DownloadType=Drivers&ProductID=3319&ProductFamily=Graphics&ProductLine=Laptop+graphics+drivers&ProductProduct=2nd+Generation+IntelAE+Core84C22f2000", "9.17.10.2000", "hd graphics 2000", "intel(r)", "32-bit", "windows 8", "core", "genuineintel");
            this.graphicsDriverData.Rows.Add("https://downloadcenter.intel.com/confirm.aspx?httpDown=http://downloadmirror.intel.com/23105/a08/Win32_153117.exe&lang=eng&Dwnldid=23105&ProductID=3711&ProductFamily=Graphics&ProductLine=Laptop+graphics+drivers&ProductProduct=3rd+Generation+IntelAE+Core84C2AE+HD+Graphics+2500", "9.17.10.2000", "hd graphics 4400", "intel(r)", "32-bit", "windows 7", "core", "genuineintel");
            this.graphicsDriverData.Rows.Add("https://downloadcenter.intel.com/confirm.aspx?httpDown=http://downloadmirror.intel.com/23101/a08/Win32_152818.exe&lang=eng&Dwnldid=23101&DownloadType=Drivers&ProductID=3319&ProductFamily=Graphics&ProductLine=Laptop+graphics+drivers&ProductProduct=2nd+Generation+IntelAE+Core84C22f2000", "9.17.10.2000", "hd graphics 3000", "intel(r)", "32-bit", "windows 7", "core", "genuineintel");
            this.graphicsDriverData.Rows.Add("https://downloadcenter.intel.com/confirm.aspx?httpDown=http://downloadmirror.intel.com/23106/a08/Win64_153117.exe&lang=eng&Dwnldid=23106&ProductID=3711&ProductFamily=Graphics&ProductLine=Laptop+graphics+drivers&ProductProduct=3rd+Generation+IntelAE+Core84C2AE+HD+Graphics+2500", "9.17.10.2000", "hd graphics 2500", "intel(r)", "64-bit", "windows 8", "core", "genuineintel");
            this.graphicsDriverData.Rows.Add("https://downloadcenter.intel.com/confirm.aspx?httpDown=http://downloadmirror.intel.com/23106/a08/Win64_153117.exe&lang=eng&Dwnldid=23106&ProductID=3711&ProductFamily=Graphics&ProductLine=Laptop+graphics+drivers&ProductProduct=3rd+Generation+IntelAE+Core84C2AE+HD+Graphics+2500", "9.17.10.2000", "hd graphics 5200", "intel(r)", "64-bit", "windows 7", "core", "genuineintel");
            this.graphicsDriverData.Rows.Add("https://downloadcenter.intel.com/confirm.aspx?httpDown=http://downloadmirror.intel.com/23106/a08/Win64_153117.exe&lang=eng&Dwnldid=23106&ProductID=3711&ProductFamily=Graphics&ProductLine=Laptop+graphics+drivers&ProductProduct=3rd+Generation+IntelAE+Core84C2AE+HD+Graphics+2500", "9.17.10.2000", "hd graphics 5000", "intel(r)", "64-bit", "windows 7", "core", "genuineintel");
            this.graphicsDriverData.Rows.Add("https://downloadcenter.intel.com/confirm.aspx?httpDown=http://downloadmirror.intel.com/22519/a08/Win7Vista_152258.exe&lang=eng&Dwnldid=22519&ProductID=3231&ProductFamily=Graphics&ProductLine=Laptop+graphics+drivers&ProductProduct=IntelAE+Core84C2AE+HD+Graphics", "8.15.10.2993", "hd graphics", "intel(r)", "32-bit", "windows 7", "core", "genuineintel");
            this.graphicsDriverData.Rows.Add("https://downloadcenter.intel.com/confirm.aspx?httpDown=http://downloadmirror.intel.com/23106/a08/Win64_153117.exe&lang=eng&Dwnldid=23106&ProductID=3711&ProductFamily=Graphics&ProductLine=Laptop+graphics+drivers&ProductProduct=3rd+Generation+IntelAE+Core84C2AE+HD+Graphics+2500", "9.17.10.2000", "hd graphics 5100", "intel(r)", "64-bit", "windows 7", "core", "genuineintel");
            this.graphicsDriverData.Rows.Add("https://downloadcenter.intel.com/confirm.aspx?httpDown=http://downloadmirror.intel.com/23105/a08/Win32_153117.exe&lang=eng&Dwnldid=23105&ProductID=3711&ProductFamily=Graphics&ProductLine=Laptop+graphics+drivers&ProductProduct=3rd+Generation+IntelAE+Core84C2AE+HD+Graphics+2500", "9.17.10.2000", "hd graphics 4000", "intel(r)", "32-bit", "windows 8", "core", "genuineintel");
            this.graphicsDriverData.Rows.Add("https://downloadcenter.intel.com/confirm.aspx?httpDown=http://downloadmirror.intel.com/23105/a08/Win32_153117.exe&lang=eng&Dwnldid=23105&ProductID=3711&ProductFamily=Graphics&ProductLine=Laptop+graphics+drivers&ProductProduct=3rd+Generation+IntelAE+Core84C2AE+HD+Graphics+2500", "9.17.10.2000", "hd graphics 4000", "intel(r)", "32-bit", "windows 7", "core", "genuineintel");
            this.graphicsDriverData.Rows.Add("https://downloadcenter.intel.com/confirm.aspx?httpDown=http://downloadmirror.intel.com/23105/a08/Win32_153117.exe&lang=eng&Dwnldid=23105&ProductID=3711&ProductFamily=Graphics&ProductLine=Laptop+graphics+drivers&ProductProduct=3rd+Generation+IntelAE+Core84C2AE+HD+Graphics+2500", "9.17.10.2000", "hd graphics 4400", "intel(r)", "32-bit", "windows 8", "core", "genuineintel");
            this.graphicsDriverData.Rows.Add("https://downloadcenter.intel.com/confirm.aspx?httpDown=http://downloadmirror.intel.com/23102/a08/Win64_152818.exe&lang=eng&Dwnldid=23102&DownloadType=Drivers&ProductID=3319&ProductFamily=Graphics&ProductLine=Laptop+graphics+drivers&ProductProduct=2nd+Generation+IntelAE+Core84C22f2000", "9.17.10.2000", "hd graphics 3000", "intel(r)", "64-bit", "windows 7", "core", "genuineintel");
            this.graphicsDriverData.Rows.Add("https://downloadcenter.intel.com/confirm.aspx?httpDown=http://downloadmirror.intel.com/23105/a08/Win32_153117.exe&lang=eng&Dwnldid=23105&ProductID=3711&ProductFamily=Graphics&ProductLine=Laptop+graphics+drivers&ProductProduct=3rd+Generation+IntelAE+Core84C2AE+HD+Graphics+2500", "9.17.10.2000", "hd graphics 5200", "intel(r)", "32-bit", "windows 7", "core", "genuineintel");
            this.graphicsDriverData.Rows.Add("https://downloadcenter.intel.com/confirm.aspx?httpDown=http://downloadmirror.intel.com/23105/a08/Win32_153117.exe&lang=eng&Dwnldid=23105&ProductID=3711&ProductFamily=Graphics&ProductLine=Laptop+graphics+drivers&ProductProduct=3rd+Generation+IntelAE+Core84C2AE+HD+Graphics+2500", "9.17.10.2000", "hd graphics 2500", "intel(r)", "32-bit", "windows 7", "core", "genuineintel");
            this.graphicsDriverData.Rows.Add("https://downloadcenter.intel.com/confirm.aspx?httpDown=http://downloadmirror.intel.com/23106/a08/Win64_153117.exe&lang=eng&Dwnldid=23106&ProductID=3711&ProductFamily=Graphics&ProductLine=Laptop+graphics+drivers&ProductProduct=3rd+Generation+IntelAE+Core84C2AE+HD+Graphics+2500", "9.17.10.2000", "hd graphics 2500", "intel(r)", "64-bit", "windows 7", "core", "genuineintel");
            this.graphicsDriverData.Rows.Add("https://downloadcenter.intel.com/confirm.aspx?httpDown=http://downloadmirror.intel.com/23105/a08/Win32_153117.exe&lang=eng&Dwnldid=23105&ProductID=3711&ProductFamily=Graphics&ProductLine=Laptop+graphics+drivers&ProductProduct=3rd+Generation+IntelAE+Core84C2AE+HD+Graphics+2500", "9.17.10.2000", "hd graphics 5100", "intel(r)", "32-bit", "windows 8", "core", "genuineintel");
            this.graphicsDriverData.Rows.Add("https://downloadcenter.intel.com/confirm.aspx?httpDown=http://downloadmirror.intel.com/23106/a08/Win64_153117.exe&lang=eng&Dwnldid=23106&ProductID=3711&ProductFamily=Graphics&ProductLine=Laptop+graphics+drivers&ProductProduct=3rd+Generation+IntelAE+Core84C2AE+HD+Graphics+2500", "9.17.10.2000", "hd graphics 4600", "intel(r)", "64-bit", "windows 8", "core", "genuineintel");
            this.graphicsDriverData.Rows.Add("https://downloadcenter.intel.com/confirm.aspx?httpDown=http://downloadmirror.intel.com/23105/a08/Win32_153117.exe&lang=eng&Dwnldid=23105&ProductID=3711&ProductFamily=Graphics&ProductLine=Laptop+graphics+drivers&ProductProduct=3rd+Generation+IntelAE+Core84C2AE+HD+Graphics+2500", "9.17.10.2000", "hd graphics 4200", "intel(r)", "32-bit", "windows 7", "core", "genuineintel");
            this.graphicsDriverData.Rows.Add("https://downloadcenter.intel.com/confirm.aspx?httpDown=http://downloadmirror.intel.com/23102/a08/Win64_152818.exe&lang=eng&Dwnldid=23102&DownloadType=Drivers&ProductID=3319&ProductFamily=Graphics&ProductLine=Laptop+graphics+drivers&ProductProduct=2nd+Generation+IntelAE+Core84C22f2000", "9.17.10.2000", "hd graphics 2000", "intel(r)", "64-bit", "windows 7", "core", "genuineintel");
            this.graphicsDriverData.Rows.Add("https://downloadcenter.intel.com/confirm.aspx?httpDown=http://downloadmirror.intel.com/23105/a08/Win32_153117.exe&lang=eng&Dwnldid=23105&ProductID=3711&ProductFamily=Graphics&ProductLine=Laptop+graphics+drivers&ProductProduct=3rd+Generation+IntelAE+Core84C2AE+HD+Graphics+2500", "9.17.10.2000", "hd graphics 4200", "intel(r)", "32-bit", "windows 8", "core", "genuineintel");
            this.graphicsDriverData.Rows.Add("https://downloadcenter.intel.com/confirm.aspx?httpDown=http://downloadmirror.intel.com/23106/a08/Win64_153117.exe&lang=eng&Dwnldid=23106&ProductID=3711&ProductFamily=Graphics&ProductLine=Laptop+graphics+drivers&ProductProduct=3rd+Generation+IntelAE+Core84C2AE+HD+Graphics+2500", "9.17.10.2000", "hd graphics 5100", "intel(r)", "64-bit", "windows 8", "core", "genuineintel");
        }

        public void PrintGraphicsDriverData()
        {
            foreach (DataRow row in this.graphicsDriverData.Rows)
            {
                object[] itemArray = row.ItemArray;
                for (int i = 0; i < itemArray.Length; i++)
                {
                    string msg = (string)itemArray[i];
                    Logger.Info(msg);
                }
            }
        }

        public bool FindDriver(Dictionary<string, string> deviceInfo, out string updateUrl)
        {
            bool flag = false;
            updateUrl = "";
            string text = deviceInfo["os_version"].ToLower();
            string arg = deviceInfo["os_arch"].ToLower();
            string arg2 = deviceInfo["processor_vendor"].ToLower();
            string text2 = deviceInfo["processor"].ToLower();
            string[] array = deviceInfo["gpu_vendor"].ToLower().Split(new string[3]
			{
				Environment.NewLine,
				"\r\n",
				"\n"
			}, StringSplitOptions.RemoveEmptyEntries);
            string[] array2 = deviceInfo["gpu"].ToLower().Split(new string[3]
			{
				Environment.NewLine,
				"\r\n",
				"\n"
			}, StringSplitOptions.RemoveEmptyEntries);
            string[] array3 = deviceInfo["driver_version"].ToLower().Split(new string[3]
			{
				Environment.NewLine,
				"\r\n",
				"\n"
			}, StringSplitOptions.RemoveEmptyEntries);
            int num = 0;
            bool flag2 = false;
            string text3 = "";
            string text4 = "";
            string text5 = "";
            for (int i = 0; i < array.Length; i++)
            {
                string filterExpression = "os_arch = '" + arg + "' AND processor_vendor = '" + arg2 + "' AND gpu_vendor = '" + array[i] + "'";
                DataRow[] array4 = this.graphicsDriverData.Select(filterExpression);
                foreach (DataRow row in array4)
                {
                    if (array2[i].Contains(this.GetRowData(row, Column.gpu)) && text.Contains(this.GetRowData(row, Column.os_version)) && text2.Contains(this.GetRowData(row, Column.processor_family)))
                    {
                        Logger.Info("Found match");
                        flag2 = true;
                        System.Version v = new System.Version(array3[i]);
                        System.Version v2 = new System.Version(this.GetRowData(row, Column.driver_version));
                        if (v2 > v)
                        {
                            Logger.Info("Found a match");
                            flag = true;
                            int num2 = this.GetRowData(row, Column.gpu).Split(new char[1]
							{
								' '
							}, StringSplitOptions.RemoveEmptyEntries).Length;
                            if (num2 > num)
                            {
                                num = num2;
                                text3 = this.GetRowData(row, Column.driver_version);
                                string text6 = array[i];
                                text4 = array2[i];
                                text5 = this.GetRowData(row, Column.driver_url);
                            }
                        }
                        else if (v2 == v)
                        {
                            text3 = this.GetRowData(row, Column.driver_version);
                            string text7 = array[i];
                            text4 = array2[i];
                        }
                    }
                    else
                    {
                        Logger.Info("No match");
                    }
                }
            }
            if (flag)
            {
                Logger.Info("Found new driver");
                Logger.Info("GPU:{0}, Driver version: {1}, Driver URL: {2}", text4, text3, text5);
                updateUrl = text5;
            }
            else
            {
                Logger.Info("Did not find new driver");
            }
            if (!flag2)
            {
                Logger.Info("We do not have information about this gpu. Not tracking this request.");
                return flag;
            }
            return flag;
        }

        private string GetRowData(DataRow row, Column column)
        {
            return (string)row[(int)column];
        }
    }
}
