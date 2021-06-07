using BlueStacks.hyperDroid.Common;
using CodeTitans.JSon;
using Microsoft.Win32;
using System;
using System.ComponentModel;

namespace BlueStacks.hyperDroid.Cloud.Services
{
	internal class Service
	{
		public delegate void OnSuccess(IJSonObject o);

		public delegate void OnFailed(Exception exc);

		public static string Host
		{
			get
			{
				using (RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("Software\\BlueStacks\\Agent\\Cloud"))
				{
					if (registryKey == null)
					{
						return Strings.ChannelsUrl;
					}
					return (string)registryKey.GetValue("Host", "http://127.0.0.1:8080");
				}
			}
		}

		public static string Host2
		{
			get
			{
				using (RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("Software\\BlueStacks\\Agent\\Cloud"))
				{
					if (registryKey == null)
					{
						return "https://23.23.194.123";
					}
					return (string)registryKey.GetValue("Host2", "http://127.0.0.1:8080");
				}
			}
		}

		public static bool Success(IJSonObject o)
		{
			if (o.Contains("success"))
			{
				return o["success"].IsTrue;
			}
			return false;
		}

		public static string ErrorReason(IJSonObject o)
		{
			if (o.Contains("reason"))
			{
				return o["reason"].StringValue;
			}
			return "";
		}

		public static BackgroundWorker CreateWorkerAsync(OnSuccess success, OnFailed failed)
		{
			BackgroundWorker backgroundWorker = new BackgroundWorker();
			backgroundWorker.RunWorkerCompleted += delegate(object o, RunWorkerCompletedEventArgs args)
			{
				if (args.Error != null)
				{
					if (failed != null)
					{
						failed(args.Error);
					}
				}
				else
				{
					IJSonObject o2 = (IJSonObject)args.Result;
					if (Service.Success(o2))
					{
						success(o2);
					}
					else
					{
						EService exc = new EService(Service.ErrorReason(o2));
						failed(exc);
					}
				}
			};
			return backgroundWorker;
		}
	}
}
