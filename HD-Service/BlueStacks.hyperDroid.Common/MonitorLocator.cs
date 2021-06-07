using Microsoft.Win32;

namespace BlueStacks.hyperDroid.Common
{
	public class MonitorLocator
	{
		private const string REG_PATH = "Software\\BlueStacks\\Monitors";

		public static void Publish(string vmName, uint vmId)
		{
			RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("Software\\BlueStacks\\Monitors", true);
			string[] valueNames = registryKey.GetValueNames();
			foreach (string name in valueNames)
			{
				RegistryValueKind valueKind = registryKey.GetValueKind(name);
				if (valueKind == RegistryValueKind.DWord)
				{
					uint num = (uint)(int)registryKey.GetValue(name, 0);
					if (vmId == num)
					{
						registryKey.DeleteValue(name);
					}
				}
			}
			registryKey.SetValue(vmName, vmId, RegistryValueKind.DWord);
		}

		public static uint Lookup(string vmName)
		{
			RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("Software\\BlueStacks\\Monitors");
			return (uint)(int)registryKey.GetValue(vmName, 0);
		}
	}
}
