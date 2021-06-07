using System;
using System.Security.Cryptography;
using System.Text;

namespace BlueStacks.hyperDroid.Common
{
	internal class SecureUserData
	{
		public class ESecure : Exception
		{
			public ESecure(string reason)
				: base(reason)
			{
			}
		}

		private static byte[] s_Entropy = new byte[8]
		{
			122,
			105,
			110,
			103,
			109,
			112,
			101,
			103
		};

		public static byte[] Encrypt(string data)
		{
			if (data == null)
			{
				throw new ESecure("Cannot encrypt null string");
			}
			if (data.Length == 0)
			{
				throw new ESecure("Cannot encrypt empty string");
			}
			byte[] bytes = Encoding.UTF8.GetBytes(data);
			return ProtectedData.Protect(bytes, SecureUserData.s_Entropy, DataProtectionScope.CurrentUser);
		}

		public static string Decrypt(byte[] data)
		{
			byte[] bytes = ProtectedData.Unprotect(data, SecureUserData.s_Entropy, DataProtectionScope.CurrentUser);
			return Encoding.UTF8.GetString(bytes);
		}
	}
}
