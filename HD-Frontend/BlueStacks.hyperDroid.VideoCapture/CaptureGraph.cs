using BlueStacks.hyperDroid.Common;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Threading;

namespace BlueStacks.hyperDroid.VideoCapture
{
	public class CaptureGraph : ISampleGrabberCB, IDisposable
	{
		private int m_Unit;

		private int m_Width;

		private int m_Height;

		private int m_FrameRate;

		private int m_Stride;

		private int m_DroppedFrame;

		private IntPtr m_Buffer = IntPtr.Zero;

		private ManualResetEvent m_Evt;

		private bool m_bGraphRunning;

		private volatile bool m_bGrabFrame;

		private SupportedColorFormat m_color;

		private IFilterGraph2 m_FilterGraph;

		private IMediaControl m_mediaCtrl;

		public int Width
		{
			get
			{
				return this.m_Width;
			}
		}

		public int Height
		{
			get
			{
				return this.m_Height;
			}
		}

		public int Stride
		{
			get
			{
				return this.m_Stride;
			}
		}

		[DllImport("Kernel32.dll", EntryPoint = "RtlMoveMemory")]
		private static extern void CopyMemory(IntPtr Destination, IntPtr Source, int Length);

		public CaptureGraph(int unit, int width, int height, int framerate, SupportedColorFormat color)
		{
			this.m_Unit = unit;
			this.m_Width = width;
			this.m_Height = height;
			this.m_FrameRate = framerate;
			this.m_DroppedFrame = 0;
			this.m_color = color;
			this.m_Evt = new ManualResetEvent(false);
			this.m_bGraphRunning = false;
			Logger.Info("Building graph");
			try
			{
				this.BuildGraph();
			}
			catch (Exception ex)
			{
				Logger.Error("Failed to build graph. Exception: {0}", ex.ToString());
				this.Dispose();
				throw;
			}
		}

		~CaptureGraph()
		{
			if (this.m_Buffer != IntPtr.Zero)
			{
				Marshal.FreeCoTaskMem(this.m_Buffer);
				this.m_Buffer = IntPtr.Zero;
			}
			this.Dispose();
		}

		public void BuildGraph()
		{
			ICaptureGraphBuilder2 captureGraphBuilder = null;
			IBaseFilter baseFilter = null;
			ISampleGrabber sampleGrabber = null;
			List<DeviceEnumerator> list = null;
			DeviceEnumerator deviceEnumerator = null;
			try
			{
				Logger.Info("Creating List of devices");
				list = DeviceEnumerator.ListDevices(Guids.VideoInputDeviceCategory);
			}
			catch (Exception ex)
			{
				Logger.Error("No Video device found : {0}", ex.ToString());
			}
			if (list != null)
			{
				try
				{
					Logger.Info("found {0} Camera, Opening {1}", list.Count, this.m_Unit);
					deviceEnumerator = ((this.m_Unit >= list.Count) ? list[0] : list[this.m_Unit]);
					this.m_FilterGraph = (IFilterGraph2)new FilterGraph();
					this.m_mediaCtrl = (this.m_FilterGraph as IMediaControl);
					captureGraphBuilder = (ICaptureGraphBuilder2)new CaptureGraphBuilder2();
					sampleGrabber = (ISampleGrabber)new SampleGrabber();
					ErrorHandler errorHandler = (ErrorHandler)captureGraphBuilder.SetFiltergraph(this.m_FilterGraph);
					ErrorHandler errorHandler2 = (ErrorHandler)this.m_FilterGraph.AddSourceFilterForMoniker(deviceEnumerator.Moniker, (IBindCtx)null, "Video input", out baseFilter);
					AMMediaType aMMediaType = new AMMediaType();
					aMMediaType.majorType = Guids.MediaTypeVideo;
					if (this.m_color == SupportedColorFormat.YUV2)
					{
						aMMediaType.subType = Guids.MediaSubtypeYUY2;
						goto IL_0156;
					}
					if (this.m_color == SupportedColorFormat.RGB24)
					{
						aMMediaType.subType = Guids.MediaSubtypeRGB24;
						goto IL_0156;
					}
					throw new Exception("Unsupported color format");
					IL_0156:
					aMMediaType.formatType = Guids.FormatTypesVideoInfo;
					ErrorHandler errorHandler3 = (ErrorHandler)sampleGrabber.SetMediaType(aMMediaType);
					this.FreeAMMedia(aMMediaType);
					ErrorHandler errorHandler4 = (ErrorHandler)sampleGrabber.SetCallback(this, 1);
					IBaseFilter baseFilter2 = (IBaseFilter)sampleGrabber;
					ErrorHandler errorHandler5 = (ErrorHandler)this.m_FilterGraph.AddFilter(baseFilter2, "FrameGrabber");
					object obj = default(object);
					ErrorHandler errorHandler6 = (ErrorHandler)captureGraphBuilder.FindInterface(Guids.PinCategoryCapture, Guids.MediaTypeVideo, baseFilter, typeof(IAMStreamConfig).GUID, out obj);
					IAMStreamConfig iAMStreamConfig = obj as IAMStreamConfig;
					if (iAMStreamConfig == null)
					{
						throw new Exception("Stream config Error");
					}
					ErrorHandler errorHandler7 = (ErrorHandler)iAMStreamConfig.GetFormat(out aMMediaType);
					VideoInfoHeader videoInfoHeader = new VideoInfoHeader();
					Marshal.PtrToStructure(aMMediaType.pbFormat, videoInfoHeader);
					videoInfoHeader.AvgTimePerFrame = 10000000 / this.m_FrameRate;
					videoInfoHeader.BmiHeader.Width = this.m_Width;
					videoInfoHeader.BmiHeader.Height = this.m_Height;
					Marshal.StructureToPtr(videoInfoHeader, aMMediaType.pbFormat, false);
					ErrorHandler errorHandler8 = (ErrorHandler)iAMStreamConfig.SetFormat(aMMediaType);
					this.FreeAMMedia(aMMediaType);
					ErrorHandler errorHandler9 = (ErrorHandler)captureGraphBuilder.RenderStream(Guids.PinCategoryCapture, Guids.MediaTypeVideo, baseFilter, null, baseFilter2);
					aMMediaType = new AMMediaType();
					ErrorHandler errorHandler10 = (ErrorHandler)sampleGrabber.GetConnectedMediaType(aMMediaType);
					if (aMMediaType.formatType != Guids.FormatTypesVideoInfo)
					{
						throw new ColorFormatNotSupported("Not able to connect to Video Media");
					}
					if (aMMediaType.pbFormat == IntPtr.Zero)
					{
						throw new Exception("Format Array is null");
					}
					videoInfoHeader = (VideoInfoHeader)Marshal.PtrToStructure(aMMediaType.pbFormat, typeof(VideoInfoHeader));
					this.m_Width = videoInfoHeader.BmiHeader.Width;
					this.m_Height = videoInfoHeader.BmiHeader.Height;
					this.m_Stride = this.m_Width * (videoInfoHeader.BmiHeader.BitCount / 8);
					if (this.m_Buffer == IntPtr.Zero)
					{
						this.m_Buffer = Marshal.AllocCoTaskMem(this.m_Stride * this.m_Height);
					}
					this.FreeAMMedia(aMMediaType);
				}
				catch
				{
					throw;
				}
				finally
				{
					if (baseFilter != null)
					{
						Marshal.ReleaseComObject(baseFilter);
						baseFilter = null;
					}
					if (sampleGrabber != null)
					{
						Marshal.ReleaseComObject(sampleGrabber);
						sampleGrabber = null;
					}
					if (captureGraphBuilder != null)
					{
						Marshal.ReleaseComObject(captureGraphBuilder);
						captureGraphBuilder = null;
					}
				}
			}
		}

		public void Dispose()
		{
			this.TearDownCom();
			if (this.m_Evt != null)
			{
				this.m_Evt.Close();
				this.m_Evt = null;
			}
		}

		private void FreeAMMedia(AMMediaType m)
		{
			if (m != null)
			{
				if (m.cbFormat != 0)
				{
					Marshal.FreeCoTaskMem(m.pbFormat);
					m.cbFormat = 0;
					m.pbFormat = IntPtr.Zero;
				}
				if (m.pUnk != IntPtr.Zero)
				{
					Marshal.Release(m.pUnk);
					m.pUnk = IntPtr.Zero;
				}
			}
			m = null;
		}

		int ISampleGrabberCB.SampleCB(double time, IMediaSample pSample)
		{
			Marshal.ReleaseComObject(pSample);
			return 0;
		}

		int ISampleGrabberCB.BufferCB(double time, IntPtr pBuffer, int len)
		{
			if (this.m_bGrabFrame)
			{
				if (len <= this.m_Stride * this.m_Height)
				{
					CaptureGraph.CopyMemory(this.m_Buffer, pBuffer, len);
				}
				this.m_bGrabFrame = false;
				this.m_Evt.Set();
			}
			else
			{
				this.m_DroppedFrame++;
			}
			return 0;
		}

		public void Run()
		{
			if (!this.m_bGraphRunning)
			{
				ErrorHandler errorHandler = (ErrorHandler)this.m_mediaCtrl.Run();
				this.m_bGraphRunning = true;
			}
		}

		public void Pause()
		{
			if (this.m_bGraphRunning)
			{
				ErrorHandler errorHandler = (ErrorHandler)this.m_mediaCtrl.Pause();
				this.m_bGraphRunning = false;
			}
		}

		public IntPtr getSignleFrame()
		{
			try
			{
				this.m_Evt.Reset();
				this.m_bGrabFrame = true;
				this.Run();
				if (!this.m_Evt.WaitOne(5000, false))
				{
					Logger.Info("GetSingleFrame Timed out");
				}
			}
			catch (Exception ex)
			{
				Logger.Error(ex.ToString());
				Marshal.FreeCoTaskMem(this.m_Buffer);
				this.m_Buffer = IntPtr.Zero;
			}
			return this.m_Buffer;
		}

		private void TearDownCom()
		{
			try
			{
				if (this.m_mediaCtrl != null && this.m_bGraphRunning)
				{
					ErrorHandler errorHandler = (ErrorHandler)this.m_mediaCtrl.Stop();
					this.m_bGraphRunning = false;
				}
				if (this.m_mediaCtrl != null)
				{
					Marshal.ReleaseComObject(this.m_mediaCtrl);
					this.m_mediaCtrl = null;
				}
			}
			catch (Exception ex)
			{
				Logger.Error("Failed to Stop Graph, Exception: {0}", ex.ToString());
			}
			if (this.m_FilterGraph != null)
			{
				Marshal.ReleaseComObject(this.m_FilterGraph);
				this.m_FilterGraph = null;
			}
		}
	}
}
