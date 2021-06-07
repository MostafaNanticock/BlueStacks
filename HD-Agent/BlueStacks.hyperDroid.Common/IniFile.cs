using System.Runtime.InteropServices;
using System.Text;

namespace BlueStacks.hyperDroid.Common
{
	public class IniFile
	{
		private const int VALUE_LEN_MAX = 255;

		private string m_Path;

		public IniFile(string path)
		{
			this.m_Path = path;
		}

		public string GetValue(string section, string key)
		{
			StringBuilder stringBuilder = new StringBuilder(255);
			IniFile.GetPrivateProfileString(section, key, "", stringBuilder, 255, this.m_Path);
			return stringBuilder.ToString();
		}

		public void SetValue(string section, string key, string value)
		{
			IniFile.WritePrivateProfileString(section, key, value, this.m_Path);
		}

		[DllImport("kernel32", SetLastError = true)]
		private static extern int GetPrivateProfileString(string section, string key, string defaultValue, StringBuilder result, int size, string fileName);

		[DllImport("kernel32", SetLastError = true)]
		private static extern long WritePrivateProfileString(string section, string key, string val, string path);
	}
}
