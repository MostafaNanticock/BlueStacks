using BlueStacks.hyperDroid.Common;
using BlueStacks.hyperDroid.Frontend.Interop;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

namespace BlueStacks.hyperDroid.Frontend
{
	public class InputMapper
	{
		public struct TouchPoint
		{
			public float X;

			public float Y;

			public bool Down;
		}

		public enum Direction
		{
			None,
			Up,
			Down,
			Left,
			Right
		}

		public enum GamepadEvent
		{
			None,
			Attach,
			Detach,
			GuidancePress,
			GuidanceRelease
		}

		public struct GamePad
		{
			private int X;

			private int Y;

			private int Z;

			private int Rx;

			private int Ry;

			private int Rz;

			private int Hat;

			private uint Mask;
		}

		public delegate void ModeHandlerNet(string mode);

		private delegate void KeyHandler(IntPtr context, byte code);

		private delegate void TouchHandler(IntPtr context, IntPtr list, int count, int offset);

		private delegate void TiltHandler(IntPtr context, float x, float y, float z);

		private delegate void ModeHandler(IntPtr context, string mode);

		private delegate void MoveHandler(IntPtr context, int identity, int x, int y);

		private delegate void ClickHandler(IntPtr context, int identity, int down);

		private delegate void SpecialHandler(IntPtr context, string cmd);

		private delegate void GamepadHandler(IntPtr context, int identity, GamepadEvent evt, string layout);

		private delegate void LogHandler(string msg);

		public const int CURSOR_SLOTS = 4;

		private const int GAMEPAD_AXIS_MAX = 1000;

		private const int CURSOR_MOVE_FACTOR = 10;

		private const string TEMPLATE = "TEMPLATE.cfg";

		private const string DEFAULT = "DEFAULT.cfg";

		private const int MAX_X = 32767;

		private const int MAX_Y = 32767;

		private const string INPUT_MAPPER_DLL = "HD-InputMapper-Native.dll";

		private static InputMapper sInstance = new InputMapper();

		private Console mConsole;

		private string mFolder;

		private string mUserFolder;

		private BlueStacks.hyperDroid.Frontend.Interop.Monitor mMonitor;

		private BlueStacks.hyperDroid.Frontend.Interop.Monitor.TouchPoint[] mTouchPoints;

		private bool mEmulatedPortraitMode;

		private bool mRotateGuest180;

		private string mCurrentPackage;

		private bool mEmulatedGestureInProgress;

		private SerialWorkQueue mSerialQueue;

		private SensorDevice mSensor;

		private Cursor mCursor;

		private IControlHandler mControlHandler;

		private ModeHandlerNet mModeHandlerNet;

		private Timer mCursorTimer;

		private object mCursorLock = new object();

		private Point[] mCursorDeltas;

		private KeyHandler mKeyHandler;

		private TouchHandler mTouchHandler;

		private TiltHandler mTiltHandler;

		private ModeHandler mModeHandler;

		private MoveHandler mMoveHandler;

		private ClickHandler mClickHandler;

		private SpecialHandler mSpecialHandler;

		private GamepadHandler mGamepadHandler;

		private LogHandler mLogger;

		private float mSoftControlBarHeightLandscape;

		private float mSoftControlBarHeightPortrait;

		[CompilerGenerated]
		private static LogHandler _003C_003E9__CachedAnonymousMethodDelegate1;

		[CompilerGenerated]
		private static SerialWorkQueue.Work _003C_003E9__CachedAnonymousMethodDelegate24;

		[CompilerGenerated]
		private static SerialWorkQueue.Work _003C_003E9__CachedAnonymousMethodDelegate2c;

		[DllImport("HD-InputMapper-Native.dll")]
		private static extern void InputMapperInit(KeyHandler keyHandler, IntPtr keyContext, TouchHandler touchHandler, IntPtr touchContext, TiltHandler tiltHandler, IntPtr tiltContext, ModeHandler modeHandler, IntPtr modeContext, MoveHandler moveHandler, IntPtr moveContext, ClickHandler clickHandler, IntPtr clickContext, SpecialHandler specialHandler, IntPtr specialContext, GamepadHandler gamepadHandler, IntPtr gamepadContext, string defaultConfig, LogHandler logger, int verbose);

		[DllImport("HD-InputMapper-Native.dll")]
		private static extern void InputMapperSetLocale(string locale);

		[DllImport("HD-InputMapper-Native.dll")]
		private static extern void InputMapperSetEmulatedSwipeKnobs(float length, int duration);

		[DllImport("HD-InputMapper-Native.dll")]
		private static extern void InputMapperSetEmulatedPinchKnobs(float split, float lengthIn, float lengthOut, int duration);

		[DllImport("HD-InputMapper-Native.dll", CharSet = CharSet.Unicode)]
		private static extern int InputMapperLoadConfig([MarshalAs(UnmanagedType.LPWStr)] string path, [MarshalAs(UnmanagedType.LPWStr)] string userPath, [MarshalAs(UnmanagedType.LPStr)] string name);

		[DllImport("HD-InputMapper-Native.dll")]
		private static extern int InputMapperHandleKey(uint code, int down);

		[DllImport("HD-InputMapper-Native.dll")]
		private static extern int InputMapperHandleController(int identity, uint button, int down);

		[DllImport("HD-InputMapper-Native.dll", CharSet = CharSet.Unicode)]
		private static extern int InputMapperHandleGamePadAttach(int identity, int vendor, int product, string dbPath);

		[DllImport("HD-InputMapper-Native.dll")]
		private static extern int InputMapperHandleGamePadDetach(int identity);

		[DllImport("HD-InputMapper-Native.dll")]
		private static extern int InputMapperHandleGamePadUpdate(int identity, ref GamePad gamepad);

		[DllImport("HD-InputMapper-Native.dll")]
		private static extern int InputMapperHandleCharacter(char ch);

		[DllImport("HD-InputMapper-Native.dll")]
		private static extern int InputMapperEmulateSwipe(float x, float y, Direction direction);

		[DllImport("HD-InputMapper-Native.dll")]
		private static extern int InputMapperEmulatePinch(float x, float y, int zoomIn);

		[DllImport("HD-InputMapper-Native.dll")]
		private static extern void InputMapperScrollBegin(float x, float y);

		[DllImport("HD-InputMapper-Native.dll")]
		private static extern void InputMapperScrollUpdate(float dx, float dy);

		[DllImport("HD-InputMapper-Native.dll")]
		private static extern void InputMapperScrollEnd();

		[DllImport("HD-InputMapper-Native.dll")]
		private static extern void InputMapperZoomBegin(float x, float y);

		[DllImport("HD-InputMapper-Native.dll")]
		private static extern void InputMapperZoomUpdate(float dz);

		[DllImport("HD-InputMapper-Native.dll")]
		private static extern void InputMapperZoomEnd();

		[DllImport("User32.dll")]
		private static extern uint MapVirtualKey(uint code, uint mapType);

		public static InputMapper Instance()
		{
			return InputMapper.sInstance;
		}

		private InputMapper()
		{
		}

		public void Init(string folder, bool verbose, SensorDevice sensor, Cursor cursor)
		{
			this.mFolder = folder;
			this.mUserFolder = Path.Combine(this.mFolder, "UserFiles");
			this.mSensor = sensor;
			this.mCursor = cursor;
			this.mTouchPoints = new BlueStacks.hyperDroid.Frontend.Interop.Monitor.TouchPoint[16];
			for (int i = 0; i < this.mTouchPoints.Length; i++)
			{
				this.mTouchPoints[i] = new BlueStacks.hyperDroid.Frontend.Interop.Monitor.TouchPoint(65535, 65535);
			}
			this.mCursorDeltas = new Point[4];
			this.mKeyHandler = this.KeyHandlerImpl;
			this.mTouchHandler = this.TouchHandlerImpl;
			this.mTiltHandler = this.TiltHandlerImpl;
			this.mModeHandler = this.ModeHandlerImpl;
			this.mMoveHandler = this.MoveHandlerImpl;
			this.mClickHandler = this.ClickHandlerImpl;
			this.mSpecialHandler = this.SpecialHandlerImpl;
			this.mGamepadHandler = this.GamepadHandlerImpl;
			this.mLogger = delegate(string msg)
			{
				Logger.Info("InputMapper: " + msg);
			};
			InputMapper.InputMapperInit(this.mKeyHandler, IntPtr.Zero, this.mTouchHandler, IntPtr.Zero, this.mTiltHandler, IntPtr.Zero, this.mModeHandler, IntPtr.Zero, this.mMoveHandler, IntPtr.Zero, this.mClickHandler, IntPtr.Zero, this.mSpecialHandler, IntPtr.Zero, this.mGamepadHandler, IntPtr.Zero, this.mFolder + "\\DEFAULT.cfg", this.mLogger, verbose ? 1 : 0);
			InputMapper.InputMapperSetLocale(CultureInfo.CurrentCulture.Name);
			this.mSerialQueue = new SerialWorkQueue();
			this.mSerialQueue.Start();
			this.mCursorTimer = new Timer(this.MoveHandlerTick, null, 0, 15);
		}

		public void SetSoftControlBarHeight(float landscape, float portrait)
		{
			Logger.Info("SetSoftControlBarHeight({0}, {1})", landscape, portrait);
			this.mSoftControlBarHeightLandscape = landscape;
			this.mSoftControlBarHeightPortrait = portrait;
		}

		public void OverrideLocale(string locale)
		{
			InputMapper.InputMapperSetLocale(locale);
		}

		public void SetConsole(Console console)
		{
			this.mConsole = console;
		}

		public void SetControlHandler(IControlHandler handler)
		{
			this.mControlHandler = handler;
		}

		public void SetModeHandler(ModeHandlerNet handler)
		{
			this.mModeHandlerNet = handler;
		}

		public void SetEmulatedSwipeKnobs(float length, int duration)
		{
			InputMapper.InputMapperSetEmulatedSwipeKnobs(length, duration);
		}

		public void SetEmulatedPinchKnobs(float split, float lengthIn, float lengthOut, int duration)
		{
			InputMapper.InputMapperSetEmulatedPinchKnobs(split, lengthIn, lengthOut, duration);
		}

		public void SetMonitor(BlueStacks.hyperDroid.Frontend.Interop.Monitor monitor)
		{
			this.mMonitor = monitor;
		}

		public void SetDisplay(bool emulatedPortraitMode, bool rotateGuest180)
		{
			this.mEmulatedPortraitMode = emulatedPortraitMode;
			this.mRotateGuest180 = rotateGuest180;
		}

		public void SetPackage(string package)
		{
			this.mCurrentPackage = package;
			string path2 = package + ".cfg";
			string path = Path.Combine(this.mFolder, path2);
			string userPath = Path.Combine(this.mUserFolder, path2);
			this.mSerialQueue.Enqueue(delegate
			{
				InputMapper.InputMapperLoadConfig(path, userPath, package);
			});
		}

		public void ShowConfigDialog()
		{
			string package = this.mCurrentPackage;
			InputMapperForm inputMapperForm = new InputMapperForm(package, this.EditHandler, this.ManageHandler);
			inputMapperForm.ShowDialog();
		}

		private void EditHandler(string package)
		{
			string path = package + ".cfg";
			string sourceFileName = Path.Combine(this.mFolder, "TEMPLATE.cfg");
			string text = Path.Combine(this.mFolder, path);
			string text2 = Path.Combine(this.mUserFolder, path);
			if (!File.Exists(text2) && File.Exists(text))
			{
				File.Copy(text, text2);
			}
			text = text2;
			Logger.Info("Editing input mapper Prebundled '{0}'", text);
			try
			{
				if (!File.Exists(text))
				{
					File.Copy(sourceFileName, text);
				}
				Process process = new Process();
				process.StartInfo.FileName = "notepad.exe";
				process.StartInfo.Arguments = "\"" + text + "\"";
				process.Start();
			}
			catch (Exception ex)
			{
				Logger.Error("Cannot edit input mapper Prebundled: " + ex.ToString());
			}
		}

		private void ManageHandler(string package)
		{
			string path = package + ".cfg";
			string path2 = Path.Combine(this.mUserFolder, path);
			string fileName = this.mFolder;
			if (File.Exists(path2))
			{
				fileName = this.mUserFolder;
			}
			try
			{
				Process process = new Process();
				process.StartInfo.FileName = fileName;
				process.Start();
			}
			catch (Exception ex)
			{
				Logger.Error("Cannot open input mapper folder: " + ex.ToString());
			}
		}

		public void DispatchKeyboardEvent(uint code, bool down)
		{
			this.mSerialQueue.Enqueue(delegate
			{
				InputMapper.InputMapperHandleKey(code, down ? 1 : 0);
			});
		}

		public void DispatchCharacter(char ch)
		{
			this.mSerialQueue.Enqueue(delegate
			{
				InputMapper.InputMapperHandleCharacter(ch);
			});
		}

		public void DispatchControllerEvent(int identity, uint button, int down)
		{
			this.mSerialQueue.Enqueue(delegate
			{
				InputMapper.InputMapperHandleController(identity, button, down);
			});
		}

		public void DispatchGamePadAttach(int identity, int vendor, int product)
		{
			this.mSerialQueue.Enqueue(delegate
			{
				InputMapper.InputMapperHandleGamePadAttach(identity, vendor, product, Path.Combine(this.mFolder, "GamePads.db"));
			});
		}

		public void DispatchGamePadDetach(int identity)
		{
			this.mSerialQueue.Enqueue(delegate
			{
				InputMapper.InputMapperHandleGamePadDetach(identity);
			});
		}

		public void DispatchGamePadUpdate(int identity, GamePad gamepad)
		{
			this.mSerialQueue.Enqueue(delegate
			{
				InputMapper.InputMapperHandleGamePadUpdate(identity, ref gamepad);
			});
		}

		public void EmulateSwipe(float x, float y, Direction direction)
		{
			if (!this.mEmulatedGestureInProgress)
			{
				this.mEmulatedGestureInProgress = true;
				this.mSerialQueue.Enqueue(delegate
				{
					InputMapper.InputMapperEmulateSwipe(x, y, direction);
					this.mEmulatedGestureInProgress = false;
				});
			}
		}

		public void EmulatePinch(float x, float y, bool zoomIn)
		{
			if (!this.mEmulatedGestureInProgress)
			{
				this.mEmulatedGestureInProgress = true;
				this.mSerialQueue.Enqueue(delegate
				{
					InputMapper.InputMapperEmulatePinch(x, y, zoomIn ? 1 : 0);
					this.mEmulatedGestureInProgress = false;
				});
			}
		}

		public void ScrollBegin(float x, float y)
		{
			this.mSerialQueue.Enqueue(delegate
			{
				InputMapper.InputMapperScrollBegin(x, y);
			});
		}

		public void ScrollUpdate(float dx, float dy)
		{
			this.mSerialQueue.Enqueue(delegate
			{
				InputMapper.InputMapperScrollUpdate(dx, dy);
			});
		}

		public void ScrollEnd()
		{
			this.mSerialQueue.Enqueue(delegate
			{
				InputMapper.InputMapperScrollEnd();
			});
		}

		public void ZoomBegin(float x, float y)
		{
			this.mSerialQueue.Enqueue(delegate
			{
				InputMapper.InputMapperZoomBegin(x, y);
			});
		}

		public void ZoomUpdate(float dz)
		{
			this.mSerialQueue.Enqueue(delegate
			{
				InputMapper.InputMapperZoomUpdate(dz);
			});
		}

		public void ZoomEnd()
		{
			this.mSerialQueue.Enqueue(delegate
			{
				InputMapper.InputMapperZoomEnd();
			});
		}

		private void KeyHandlerImpl(IntPtr context, byte code)
		{
			try
			{
				this.mMonitor.SendScanCode(code);
			}
			catch (Exception ex)
			{
				Logger.Error("Cannot send keyboard scan code: " + ex.ToString());
			}
		}

		private void TouchHandlerImpl(IntPtr context, IntPtr array, int count, int offset)
		{
			this.TouchHandlerImpl(array, count, offset, true);
		}

		public void TouchHandlerImpl(IntPtr array, int count, int offset)
		{
			this.TouchHandlerImpl(array, count, offset, true);
		}

		public void TouchHandlerImpl(IntPtr array, int count, int offset, bool adjustForControlBar)
		{
			try
			{
				this.TouchHandlerImplInternal(array, count, offset, adjustForControlBar);
			}
			catch (Exception ex)
			{
				Logger.Error("Cannot send mapped touch points: " + ex.ToString());
			}
		}

		public void TouchHandlerImplInternal(IntPtr array, int count, int offset, bool adjustForControlBar)
		{
			TouchPoint[] array2 = new TouchPoint[count];
			int num = Marshal.SizeOf(typeof(TouchPoint));
			for (int i = 0; i < count; i++)
			{
				IntPtr ptr = new IntPtr(array.ToInt64() + i * num);
				array2[i] = (TouchPoint)Marshal.PtrToStructure(ptr, typeof(TouchPoint));
			}
			this.TouchHandlerImpl(array2, offset, adjustForControlBar);
		}

		public void TouchHandlerImpl(TouchPoint[] points, int offset, bool adjustForControlBar)
		{
			for (int i = 0; i + offset < this.mTouchPoints.Length && i < points.Length; i++)
			{
				TouchPoint touchPoint = points[i];
				if (!touchPoint.Down)
				{
					this.mTouchPoints[i + offset].PosX = 65535;
					this.mTouchPoints[i + offset].PosY = 65535;
				}
				else
				{
					ushort num = (ushort)(touchPoint.X * 32767f);
					ushort num2 = (ushort)(touchPoint.Y * 32767f);
					float num3 = 0f;
					if (!this.mEmulatedPortraitMode)
					{
						if (adjustForControlBar)
						{
							num3 = this.mSoftControlBarHeightLandscape;
						}
						num2 = (ushort)((float)(int)num2 * (1f - num3));
						if (!this.mRotateGuest180)
						{
							this.mTouchPoints[i + offset].PosX = num;
							this.mTouchPoints[i + offset].PosY = num2;
						}
						else
						{
							this.mTouchPoints[i + offset].PosX = (ushort)(32767 - num);
							this.mTouchPoints[i + offset].PosY = (ushort)(32767 - num2);
						}
					}
					else
					{
						if (adjustForControlBar)
						{
							num3 = this.mSoftControlBarHeightPortrait;
						}
						num2 = (ushort)((float)(int)num2 * (1f - num3));
						if (!this.mRotateGuest180)
						{
							this.mTouchPoints[i + offset].PosX = (ushort)(32767 - num2);
							this.mTouchPoints[i + offset].PosY = num;
						}
						else
						{
							this.mTouchPoints[i + offset].PosX = num2;
							this.mTouchPoints[i + offset].PosY = (ushort)(32767 - num);
						}
					}
				}
			}
			if (this.mMonitor != null)
			{
				this.mMonitor.SendTouchState(this.mTouchPoints);
			}
		}

		private void TiltHandlerImpl(IntPtr context, float x, float y, float z)
		{
			this.mSensor.SetAccelerometerVector(x, y, z);
		}

		private void ModeHandlerImpl(IntPtr context, string mode)
		{
			this.mModeHandlerNet(mode);
		}

		private void MoveHandlerImpl(IntPtr context, int identity, int x, int y)
		{
			if (identity >= 0 && identity < 4)
			{
				lock (this.mCursorLock)
				{
					this.mCursorDeltas[identity].X = x;
					this.mCursorDeltas[identity].Y = y;
				}
			}
		}

		private void MoveHandlerTick(object obj)
		{
			Point[] array = new Point[4];
			lock (this.mCursorLock)
			{
				for (int i = 0; i < 4; i++)
				{
					array[i] = this.mCursorDeltas[i];
				}
			}
			for (int i = 0; i < 4; i++)
			{
				if (array[i].X != 0 || array[i].Y != 0)
				{
					float x = (float)array[i].X / 1000f * 10f;
					float y = (float)array[i].Y / 1000f * 10f;
					this.mCursor.Move(i, x, y, false);
				}
			}
		}

		private void ClickHandlerImpl(IntPtr context, int identity, int down)
		{
			this.mCursor.Click(identity, down != 0);
		}

		private void SpecialHandlerImpl(IntPtr context, string cmd)
		{
			if (cmd == "Back")
			{
				this.mControlHandler.Back();
			}
			else if (cmd == "Menu")
			{
				this.mControlHandler.Menu();
			}
			else if (cmd == "Home")
			{
				this.mControlHandler.Home();
			}
		}

		private void GamepadHandlerImpl(IntPtr context, int identity, GamepadEvent evt, string layout)
		{
			Logger.Info("InputMapper.GamepadHandlerImpl {0} {1} {2}", identity, evt, layout);
			switch (evt)
			{
			case GamepadEvent.Attach:
				this.mCursor.Attach(identity);
				this.mConsole.HandleControllerAttach(true, identity, layout);
				break;
			case GamepadEvent.Detach:
				this.mCursor.Detach(identity);
				this.mConsole.HandleControllerAttach(false, identity, layout);
				break;
			case GamepadEvent.GuidancePress:
				this.mConsole.HandleControllerGuidance(true, identity, layout);
				break;
			case GamepadEvent.GuidanceRelease:
				this.mConsole.HandleControllerGuidance(false, identity, layout);
				break;
			}
		}
	}
}
