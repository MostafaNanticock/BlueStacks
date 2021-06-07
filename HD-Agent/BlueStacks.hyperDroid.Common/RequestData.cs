using System.Collections.Specialized;

namespace BlueStacks.hyperDroid.Common
{
	public class RequestData
	{
		public NameValueCollection headers;

		public NameValueCollection queryString;

		public NameValueCollection data;

		public NameValueCollection files;

		public RequestData()
		{
			this.headers = new NameValueCollection();
			this.queryString = new NameValueCollection();
			this.data = new NameValueCollection();
			this.files = new NameValueCollection();
		}
	}
}
