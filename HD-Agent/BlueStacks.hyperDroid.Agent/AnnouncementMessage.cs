using CodeTitans.JSon;
using System.Drawing;

namespace BlueStacks.hyperDroid.Agent
{
	internal class AnnouncementMessage
	{
		private Image m_Image;

		private string m_Title;

		private string m_Msg;

		private string m_Action;

		private string m_PkgName;

		private string m_ActionURL;

		private string m_FileName;

		public Image Image
		{
			get
			{
				return this.m_Image;
			}
		}

		public string Title
		{
			get
			{
				return this.m_Title;
			}
		}

		public string Msg
		{
			get
			{
				return this.m_Msg;
			}
		}

		public string Action
		{
			get
			{
				return this.m_Action;
			}
		}

		public string PkgName
		{
			get
			{
				return this.m_PkgName;
			}
		}

		public string ActionURL
		{
			get
			{
				return this.m_ActionURL;
			}
		}

		public string FileName
		{
			get
			{
				return this.m_FileName;
			}
			set
			{
				this.m_FileName = value;
			}
		}

		public AnnouncementMessage(Image image, string title, string msg, string action, string pkgName, string actionURL, string fileName)
		{
			this.m_Image = image;
			this.m_Title = title;
			this.m_Msg = msg;
			this.m_Action = action;
			this.m_PkgName = pkgName;
			this.m_ActionURL = actionURL;
			this.m_FileName = fileName;
		}

		public AnnouncementMessage(string title, string msg, string action, string pkgName, string actionURL, string fileName)
			: this(CloudAnnouncement.ProductLogo, title, msg, action, pkgName, actionURL, fileName)
		{
		}

		public AnnouncementMessage(Image image, IJSonObject o)
			: this(image, o["title"].StringValue.Trim(), o["msg"].StringValue.Trim(), o["action"].StringValue.Trim(), o["pkgName"].StringValue.Trim(), o["actionUrl"].StringValue.Trim(), o["fileName"].StringValue.Trim())
		{
		}
	}
}
