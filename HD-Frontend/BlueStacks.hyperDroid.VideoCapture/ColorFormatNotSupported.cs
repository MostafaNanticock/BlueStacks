using System;

namespace BlueStacks.hyperDroid.VideoCapture
{
	public class ColorFormatNotSupported : Exception
	{
		public ColorFormatNotSupported()
		{
		}

		public ColorFormatNotSupported(string message)
			: base(message)
		{
		}

		public ColorFormatNotSupported(string message, Exception inner)
			: base(message, inner)
		{
		}
	}
}
