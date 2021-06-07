using BlueStacks.hyperDroid.Common;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Threading;

namespace BlueStacks.hyperDroid.Frontend
{
    public class UsageTime
    {
        private Stopwatch mWatch;

        private TimeSpan mTime;

        private bool mComputing;

        public UsageTime()
        {
            this.mWatch = Stopwatch.StartNew();
            this.mComputing = true;
        }

        public void Update()
        {
            if (this.mComputing)
            {
                this.mWatch.Stop();
                TimeSpan elapsed = this.mWatch.Elapsed;
                string currentDate = DateTime.Now.ToString("dd-MM-yyyy");
                RegistryKey registryKey = Registry.LocalMachine.CreateSubKey(Strings.HKLMConfigRegKeyPath);
                Logger.Info("Updating usage time...");
                try
                {
                    string todaysUsage = (string)registryKey.GetValue("BstUsageTime");
                    if (todaysUsage.StartsWith(currentDate))
                    {
                        Logger.Info("Updating usage time for: " + currentDate);
                        string[] array = todaysUsage.Split('#', ':');
                        this.mTime = new TimeSpan(Convert.ToInt32(array[1]), Convert.ToInt32(array[2]), Convert.ToInt32(array[3]));
                    }
                    else
                    {
                        Logger.Info("Found entries for upload on: " + currentDate);
                        Thread thread = new Thread((ThreadStart)delegate
                        {
                            Stats.UpdatePendingStatsQueue(todaysUsage, currentDate);
                        });
                        thread.IsBackground = true;
                        thread.Start();
                        this.mTime = new TimeSpan(0, 0, 0);
                    }
                }
                catch (Exception)
                {
                    this.mTime = new TimeSpan(0, 0, 0);
                }
                this.mTime = this.mTime.Add(elapsed);
                string value = currentDate + "#" + this.mTime.Hours + ":" + this.mTime.Minutes + ":" + this.mTime.Seconds;//Modified
                registryKey.SetValue("BstUsageTime", value);
                this.mComputing = false;
                Logger.Info("Updated usage time...");
            }
        }
    }
}
