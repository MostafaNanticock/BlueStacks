using System;
using System.Management;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;

namespace BlueStacks.hyperDroid.Service
{
	internal class Id
	{
		public static string GenerateID()
		{
			string str = "";
			try
			{
				ManagementClass managementClass = new ManagementClass("Win32_BaseBoard");
				ManagementObjectCollection instances = managementClass.GetInstances();
				foreach (ManagementObject item in instances)
				{
					str = ((ManagementBaseObject)item)["SerialNumber"].ToString();
				}
			}
			catch
			{
			}
			string str2 = "";
			try
			{
				NTAccount nTAccount = new NTAccount(Environment.UserName);
				SecurityIdentifier securityIdentifier = (SecurityIdentifier)nTAccount.Translate(typeof(SecurityIdentifier));
				str2 = securityIdentifier.ToString();
			}
			catch
			{
			}
			string text = str2 + str;
			text = text.Replace("-", "");
			try
			{
				SHA1 sHA = SHA1.Create();
				byte[] value = sHA.ComputeHash(Encoding.Default.GetBytes(text));
				text = BitConverter.ToString(value).Replace("-", "");
				return text;
			}
			catch
			{
				return text;
			}
		}
	}
}
