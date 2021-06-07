using System;

namespace BlueStacks.hyperDroid.Cloud.Services
{
	public class EService : Exception
	{
		public EService(string reason)
			: base(reason)
		{
		}
	}
}
