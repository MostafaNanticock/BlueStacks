using BlueStacks.hyperDroid.Common;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace BlueStacks.hyperDroid.Frontend
{
	public class GamePad
	{
		private delegate void LoggerCallback(string msg);

		private delegate void AttachCallback(int identity, int vendor, int product);

		private delegate void DetachCallback(int identity);

		private delegate void UpdateCallback(int identity, ref InputMapper.GamePad gamepad);

		private InputMapper mInputMapper;

		private LoggerCallback mLoggerCallback;

		private AttachCallback mAttachCallback;

		private DetachCallback mDetachCallback;

		private UpdateCallback mUpdateCallback;

		[CompilerGenerated]
		private static LoggerCallback _003C_003E9__CachedAnonymousMethodDelegate4;

		[DllImport("HD-Frontend-Native.dll")]
		private static extern void GamePadSetup(LoggerCallback logger, AttachCallback attach, DetachCallback detach, UpdateCallback update, IntPtr windowHandle);

		public void Setup(InputMapper inputMapper, IntPtr windowHandle)
		{
			Logger.Info("GamePad.Setup()");
			this.mInputMapper = inputMapper;
			this.mLoggerCallback = delegate(string msg)
			{
				Logger.Info("GamePad: " + msg);
			};
			this.mAttachCallback = delegate(int identity, int vendor, int product)
			{
				this.mInputMapper.DispatchGamePadAttach(identity, vendor, product);
			};
			this.mDetachCallback = delegate(int identity)
			{
				this.mInputMapper.DispatchGamePadDetach(identity);
			};
			this.mUpdateCallback = delegate(int identity, ref InputMapper.GamePad gamepad)
			{
				this.mInputMapper.DispatchGamePadUpdate(identity, gamepad);
			};
			GamePad.GamePadSetup(this.mLoggerCallback, this.mAttachCallback, this.mDetachCallback, this.mUpdateCallback, windowHandle);
		}
	}
}
