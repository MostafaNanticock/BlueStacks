using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace BlueStacks.hyperDroid.VideoCapture
{
    //[ComImport]
	[Guid("C1F400A0-3F08-11d3-9F0B-006008039E37")]
	public class SampleGrabber
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		public extern SampleGrabber();
	}
}
