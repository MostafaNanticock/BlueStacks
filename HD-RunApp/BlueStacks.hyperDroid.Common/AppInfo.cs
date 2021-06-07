using CodeTitans.JSon;

namespace BlueStacks.hyperDroid.Common
{
	public class AppInfo
	{
		public string name;

		public string img;

		public string package;

		public string activity;

		public string system;

		public string url;

		public string appstore;

		public AppInfo(IJSonObject app)
		{
			this.name = app["KeyName"].StringValue;
			this.img = app["img"].StringValue;
			this.package = app["package"].StringValue;
			this.activity = app["activity"].StringValue;
			this.system = app["system"].StringValue;
			try
			{
				this.url = app["url"].StringValue;
			}
			catch
			{
				this.url = null;
			}
			try
			{
				this.appstore = app["appstore"].StringValue;
			}
			catch
			{
				this.appstore = "Unknown";
			}
		}

		public AppInfo(string InName, string InImage, string InPackage, string InActivity, string InSystem, string InAppStore)
		{
			this.name = InName;
			this.img = InImage;
			this.package = InPackage;
			this.activity = InActivity;
			this.system = InSystem;
			this.url = null;
			this.appstore = InAppStore;
		}
	}
}
