namespace BlueStacks.hyperDroid.VideoCapture
{
	public class ErrorHandler
	{
		private int hr;

		public ErrorHandler(int hr)
		{
			this.hr = hr;
			CameraError.GetCameraErrorString(hr);
		}

		public ErrorHandler(ErrorHandler err)
		{
			CameraError.ThrowCameraError(err.hr);
		}

		public static implicit operator ErrorHandler(int hr)
		{
			return new ErrorHandler(hr);
		}
	}
}
