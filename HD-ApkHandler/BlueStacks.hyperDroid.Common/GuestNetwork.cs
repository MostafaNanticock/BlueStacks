using Microsoft.Win32;

namespace BlueStacks.hyperDroid.Common
{
	public class GuestNetwork
	{
		public static int GetHostPort(bool isUdp, int guestPort)
		{
			string name = string.Format("{0}/{1}", isUdp ? "udp" : "tcp", guestPort);
			RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("Software\\BlueStacks\\Guests\\Android\\Network\\Redirect");
			return (int)registryKey.GetValue(name, -1);
		}
	}
}
