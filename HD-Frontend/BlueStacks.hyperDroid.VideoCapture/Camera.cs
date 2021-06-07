using BlueStacks.hyperDroid.Common;
using System;
using System.Threading;

namespace BlueStacks.hyperDroid.VideoCapture
{
	public class Camera
	{
		public delegate void getFrameCB(IntPtr ip, int width, int height, int stride);

		public IntPtr pFrame = IntPtr.Zero;

		protected Thread previewThread;

		private volatile bool m_bStop;

		private CaptureGraph VidCapture;

		private static getFrameCB s_sendFrame;

		private int m_Unit;

		private int m_Width;

		private int m_Height;

		private int m_Framerate;

		private int m_Quality;

		private SupportedColorFormat m_color;

		public bool registerFrameCB(getFrameCB cb)
		{
			if (cb == null)
			{
				return false;
			}
			Camera.s_sendFrame = cb.Invoke;
			return true;
		}

		public Camera(int unit, int width, int height, int framerate, int quality, SupportedColorFormat color)
		{
			this.m_bStop = true;
			this.m_Unit = unit;
			this.m_Width = width;
			this.m_Height = height;
			this.m_Framerate = framerate;
			this.m_Quality = quality;
			this.m_color = color;
			this.VidCapture = new CaptureGraph(this.m_Unit, this.m_Width, this.m_Height, this.m_Framerate, this.m_color);
		}

		public void StartCamera()
		{
            //new ThreadStart(this.Run);
			this.previewThread = new Thread(this.Run);
			this.previewThread.Start();
		}

		public void StopCamera()
		{
			if (this.previewThread != null)
			{
				this.m_bStop = true;
				this.previewThread.Join();
				if (this.VidCapture != null)
				{
					this.VidCapture.Dispose();
					this.VidCapture = null;
				}
				if (Camera.s_sendFrame != null)
				{
					Camera.s_sendFrame = null;
				}
			}
			this.previewThread = null;
		}

		protected void Run()
		{
			this.m_bStop = false;
			try
			{
				this.VidCapture.Run();
				do
				{
					try
					{
						this.pFrame = IntPtr.Zero;
						this.pFrame = this.VidCapture.getSignleFrame();
						if (Camera.s_sendFrame != null && this.pFrame != IntPtr.Zero)
						{
							Camera.s_sendFrame(this.pFrame, this.VidCapture.Width, this.VidCapture.Height, this.VidCapture.Stride);
						}
						if (this.m_bStop)
						{
							goto IL_00a7;
						}
					}
					catch (Exception ex)
					{
						Logger.Error("Failed in send frame callback. Exception: {0}", ex.ToString());
						throw;
					}
                    //goto IL_0015;
					IL_00a7:
					this.VidCapture.Pause();
				}
				while (!this.m_bStop);
			}
			catch (Exception ex2)
			{
				Logger.Error("Failed in Graph Run. Exception {0}", ex2.ToString());
			}
			finally
			{
				if (this.VidCapture != null)
				{
					this.VidCapture.Dispose();
					this.VidCapture = null;
				}
			}
		}
	}
}
