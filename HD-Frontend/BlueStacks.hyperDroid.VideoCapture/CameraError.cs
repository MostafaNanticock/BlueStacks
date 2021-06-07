using System.Runtime.InteropServices;
using System.Security;
using System.Text;

namespace BlueStacks.hyperDroid.VideoCapture
{
	public static class CameraError
	{
		[DllImport("quartz.dll", CharSet = CharSet.Unicode, EntryPoint = "AMGetErrorTextW", ExactSpelling = true)]
		[SuppressUnmanagedCodeSecurity]
		public static extern int AMGetErrorText(int hr, StringBuilder buf, int max);

		public static string GetCameraErrorString(int hr)
		{
			StringBuilder stringBuilder = new StringBuilder(256, 256);
			if (CameraError.AMGetErrorText(hr, stringBuilder, 256) > 0)
			{
				return stringBuilder.ToString();
			}
			return null;
		}

		public static void ThrowCameraError(int hr)
		{
			if (hr < 0)
			{
				string cameraErrorString = CameraError.GetCameraErrorString(hr);
				if (cameraErrorString != null)
				{
					throw new COMException(cameraErrorString, hr);
				}
				Marshal.ThrowExceptionForHR(hr);
			}
		}
	}
}
