using System.Net;
using System.Text;

namespace BlueStacks.hyperDroid.Common.HTTP
{
	public class Utils
	{
		public static void Write(StringBuilder sb, HttpListenerResponse res)
		{
			Utils.Write(sb.ToString(), res);
		}

		public static void Write(string s, HttpListenerResponse res)
		{
			byte[] bytes = Encoding.UTF8.GetBytes(s);
			res.ContentLength64 = bytes.Length;
			res.OutputStream.Write(bytes, 0, bytes.Length);
			res.OutputStream.Flush();
		}
	}
}
