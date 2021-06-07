using BlueStacks.hyperDroid.Common;
using BlueStacks.hyperDroid.Frontend.Interop;
using NAudio.Wave;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace BlueStacks.hyperDroid.Audio
{
	public class Manager
	{
		private delegate void PlaybackStreamCallback(IntPtr buff, int size);

		private delegate void PlaybackConfigCallback(int rate, int bits, int channel);

		private delegate void CaptureConfigCallback(int rate, int bits, int channel);

		private delegate void CaptureStartCallback();

		private delegate void CaptureStopCallback();

		private const string NATIVE_DLL = "HD-Audio-Native.dll";

		private const int BST_CO_MSG_MAX_SIZE = 65536;

		private static PlaybackStreamCallback s_PlaybackStreamCallback;

		private static PlaybackConfigCallback s_PlaybackConfigCallback;

		private static CaptureConfigCallback s_CaptureConfigCallback;

		private static CaptureStartCallback s_CaptureStartCallback;

		private static CaptureStopCallback s_CaptureStopCallback;

		private static BlueStacks.hyperDroid.Frontend.Interop.Monitor s_Monitor;

		private static WaveIn s_WaveIn;

		private static int s_CaptureRate = 44100;

		private static int s_CaptureBitsPerSample = 16;

		private static int s_CaptureNrChannels = 1;

		private static bool s_IsRecording = false;

		private static bool s_CaptureEnabled = true;

		private static IWavePlayer s_WaveOut;

		private static WaveFormat s_WaveOutFormat;

		private static BufferedWaveProvider s_WaveProvider;

		private static int s_PlaybackRate = 44100;

		private static int s_PlaybackBitsPerSample = 16;

		private static int s_PlaybackNrChannels = 2;

		private static IntPtr s_IoHandle = IntPtr.Zero;

		private static object s_IoHandleLock = new object();

		private static bool s_AudioMuted = false;

		private static byte[] s_Samples = new byte[65536];

		[CompilerGenerated]
		private static ThreadStart _003C_003E9__CachedAnonymousMethodDelegate1;

		public static BlueStacks.hyperDroid.Frontend.Interop.Monitor Monitor
		{
			get
			{
				return Manager.s_Monitor;
			}
			set
			{
				Manager.s_Monitor = value;
			}
		}

		[DllImport("kernel32.dll")]
		private static extern bool CloseHandle(IntPtr handle);

		[DllImport("HD-Audio-Native.dll")]
		private static extern void SetPlaybackStreamCallback(PlaybackStreamCallback func);

		[DllImport("HD-Audio-Native.dll")]
		private static extern void SetPlaybackConfigCallback(PlaybackConfigCallback func);

		[DllImport("HD-Audio-Native.dll")]
		private static extern void SetCaptureConfigCallback(CaptureConfigCallback func);

		[DllImport("HD-Audio-Native.dll")]
		private static extern void SetCaptureStartCallback(CaptureStartCallback func);

		[DllImport("HD-Audio-Native.dll")]
		private static extern void SetCaptureStopCallback(CaptureStopCallback func);

		[DllImport("HD-Audio-Native.dll", SetLastError = true)]
		private static extern IntPtr AudioIoAttach(uint vmId);

		[DllImport("HD-Audio-Native.dll")]
		private static extern int AudioIoProcessMessages(IntPtr ioHandle);

		private static void BstPlaybackConfigCallback(int rate, int bits, int channels)
		{
			if (Manager.s_PlaybackRate == rate && Manager.s_PlaybackBitsPerSample == bits && Manager.s_PlaybackNrChannels == channels)
			{
				return;
			}
			Manager.s_PlaybackRate = rate;
			Manager.s_PlaybackBitsPerSample = bits;
			Manager.s_PlaybackNrChannels = channels;
			Manager.PlaybackDeviceFini();
			Manager.PlaybackDeviceInit(rate, bits, channels);
		}

		private static void BstPlaybackStreamCallback(IntPtr buff, int size)
		{
			if (!Manager.s_AudioMuted)
			{
				Marshal.Copy(buff, Manager.s_Samples, 0, size);
				Manager.s_WaveProvider.AddSamples(Manager.s_Samples, 0, size);
			}
		}

		private static int PlaybackDeviceInit(int rate, int bits, int channels)
		{
			Manager.s_WaveOut = new DirectSoundOut(92);
			Manager.s_WaveOutFormat = new WaveFormat(rate, bits, channels);
			Manager.s_WaveProvider = new BufferedWaveProvider(Manager.s_WaveOutFormat);
			Manager.s_WaveProvider.DiscardOnBufferOverflow = true;
			Manager.s_WaveProvider.BufferLength = 5242880;
			Manager.s_WaveOut.Init(Manager.s_WaveProvider);
			Manager.s_WaveOut.Play();
			return 0;
		}

		private static void PlaybackDeviceFini()
		{
			Manager.s_WaveOut.Stop();
			Manager.s_WaveOut.Dispose();
		}

		private static void BstCaptureConfigCallback(int rate, int bits, int channels)
		{
			if (Manager.s_CaptureRate == rate && Manager.s_CaptureNrChannels == channels && bits == Manager.s_CaptureBitsPerSample)
			{
				return;
			}
			Manager.s_CaptureRate = rate;
			Manager.s_CaptureBitsPerSample = bits;
			Manager.s_CaptureNrChannels = channels;
			Manager.s_WaveIn.WaveFormat = new WaveFormat(rate, bits, channels);
		}

		private static void BstSendCaptureSamples(object sender, WaveInEventArgs samples)
		{
			if (Manager.s_IsRecording)
			{
				Manager.Monitor.SendAudioCaptureStream(samples.Buffer, samples.BytesRecorded);
			}
		}

		private static int CaptureDeviceInit(int rate, int bits, int channels)
		{
			Manager.s_WaveIn = new WaveIn(WaveCallbackInfo.FunctionCallback());
			Manager.s_WaveIn.WaveFormat = new WaveFormat(rate, bits, channels);
			Manager.s_WaveIn.BufferMilliseconds = 75;
			Manager.s_WaveIn.DataAvailable += Manager.BstSendCaptureSamples;
			return 0;
		}

		private static void CaptureDeviceFini()
		{
			if (Manager.s_IsRecording)
			{
				Manager.s_IsRecording = false;
				Manager.s_WaveIn.StopRecording();
			}
			if (Manager.s_WaveIn != null)
			{
				Manager.s_WaveIn.Dispose();
			}
		}

		private static void BstCaptureStartCallback()
		{
			if (Manager.s_CaptureEnabled && !Manager.s_AudioMuted)
			{
				try
				{
					Manager.s_WaveIn.StartRecording();
					Manager.s_IsRecording = true;
				}
				catch (Exception ex)
				{
					Logger.Error("Audio: Excetpion during recording: {0}.\n", ex.Message);
					Manager.s_CaptureEnabled = false;
				}
			}
		}

		private static void BstCaptureStopCallback()
		{
			if (Manager.s_IsRecording)
			{
				Manager.s_IsRecording = false;
				Manager.s_WaveIn.StopRecording();
			}
		}

		public static void Mute()
		{
			Logger.Debug("Audio: volume muted");
			Manager.s_AudioMuted = true;
		}

		public static void Unmute()
		{
			Logger.Debug("Audio: volume unmuted");
			Manager.s_AudioMuted = false;
		}

		public static void Main(string[] args)
		{
			if (args.Length != 1)
			{
				Manager.Usage();
			}
			string vmName = args[0];
			uint num = MonitorLocator.Lookup(vmName);
			lock (Manager.s_IoHandleLock)
			{
				if (Manager.s_IoHandle != IntPtr.Zero)
				{
					throw new SystemException("I/O handle is already open");
				}
				Logger.Debug("Attaching to monitor ID {0}", num);
				Manager.s_IoHandle = Manager.AudioIoAttach(num);
				if (Manager.s_IoHandle == IntPtr.Zero)
				{
					throw new SystemException("Cannot attach for I/O", new Win32Exception(Marshal.GetLastWin32Error()));
				}
			}
			int num2 = Manager.PlaybackDeviceInit(Manager.s_PlaybackRate, Manager.s_PlaybackBitsPerSample, Manager.s_PlaybackNrChannels);
			if (num2 != 0)
			{
				throw new SystemException("Failed to init playback device.", new Win32Exception(num2));
			}
			num2 = Manager.CaptureDeviceInit(Manager.s_CaptureRate, Manager.s_CaptureBitsPerSample, Manager.s_CaptureNrChannels);
			if (num2 != 0)
			{
				throw new SystemException("Failed to init capture device.", new Win32Exception(num2));
			}
			Manager.s_PlaybackStreamCallback = Manager.BstPlaybackStreamCallback;
			Manager.s_PlaybackConfigCallback = Manager.BstPlaybackConfigCallback;
			Manager.s_CaptureConfigCallback = Manager.BstCaptureConfigCallback;
			Manager.s_CaptureStartCallback = Manager.BstCaptureStartCallback;
			Manager.s_CaptureStopCallback = Manager.BstCaptureStopCallback;
			Manager.SetPlaybackStreamCallback(Manager.s_PlaybackStreamCallback);
			Manager.SetPlaybackConfigCallback(Manager.s_PlaybackConfigCallback);
			Manager.SetCaptureConfigCallback(Manager.s_CaptureConfigCallback);
			Manager.SetCaptureStartCallback(Manager.s_CaptureStartCallback);
			Manager.SetCaptureStopCallback(Manager.s_CaptureStopCallback);
			Logger.Debug("Waiting for Audio messages...");
			Thread thread = new Thread((ThreadStart)delegate
			{
				try
				{
					int num3;
					do
					{
						num3 = Manager.AudioIoProcessMessages(Manager.s_IoHandle);
					}
					while (num3 == 0);
					throw new SystemException("Cannot process VM messages", new Win32Exception(num3));
				}
				catch (Exception ex)
				{
					Logger.Error(ex.ToString());
					Logger.Error("Audio: Exiting thread.");
				}
			});
			thread.IsBackground = true;
			thread.Start();
			Application.Run();
		}

		private static void Usage()
		{
			string processName = Process.GetCurrentProcess().ProcessName;
			Environment.Exit(1);
		}

		public static void Shutdown()
		{
			Manager.PlaybackDeviceFini();
			Manager.CaptureDeviceFini();
			Thread.Sleep(500);
			lock (Manager.s_IoHandleLock)
			{
				if (Manager.s_IoHandle != IntPtr.Zero)
				{
					Manager.CloseHandle(Manager.s_IoHandle);
					Manager.s_IoHandle = IntPtr.Zero;
				}
			}
		}
	}
}
