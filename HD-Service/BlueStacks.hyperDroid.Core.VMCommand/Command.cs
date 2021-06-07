using BlueStacks.hyperDroid.Common;
using Microsoft.Win32.SafeHandles;
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;

namespace BlueStacks.hyperDroid.Core.VMCommand
{
	public class Command
	{
		public delegate void ChunkHandler(string chunk);

		public delegate void LineHandler(string line);

		private const string NATIVE_DLL = "HD-VMCommand-Native.dll";

		private Random random = new Random();

		private SafeFileHandle vmHandle;

		private uint unitId;

		private LineHandler userOutputHandler;

		private LineHandler userErrorHandler;

		private StringBuilder outputBuffer = new StringBuilder();

		private StringBuilder errorBuffer = new StringBuilder();

		[DllImport("HD-VMCommand-Native.dll", SetLastError = true)]
		private static extern SafeFileHandle CommandAttach(uint vmId, uint unitId);

		[DllImport("HD-VMCommand-Native.dll")]
		private static extern int CommandPing(SafeFileHandle vmHandle, uint unitId);

		[DllImport("HD-VMCommand-Native.dll")]
		private static extern int CommandRun(SafeFileHandle vmHandle, uint unitId, int argc, string[] argv, ChunkHandler outHandler, ChunkHandler errHandler, ref int exitCode);

		[DllImport("HD-VMCommand-Native.dll")]
		private static extern int CommandKill(SafeFileHandle vmHandle, uint unitId);

		public void Attach(string vmName)
		{
			uint vmId = MonitorLocator.Lookup(vmName);
			this.unitId = (uint)this.random.Next();
			this.vmHandle = Command.CommandAttach(vmId, this.unitId);
			if (!this.vmHandle.IsInvalid)
			{
				return;
			}
			throw new ApplicationException("Cannot attach to monitor: " + Marshal.GetLastWin32Error());
		}

		public void SetOutputHandler(LineHandler handler)
		{
			this.userOutputHandler = handler;
		}

		public void SetErrorHandler(LineHandler handler)
		{
			this.userErrorHandler = handler;
		}

		public int Run(string[] argv)
		{
			int result = 0;
			int num = Command.CommandPing(this.vmHandle, this.unitId);
			if (num != 0)
			{
				throw new ApplicationException("Cannot ping VM", new Win32Exception(num));
			}
			num = Command.CommandRun(this.vmHandle, this.unitId, argv.Length, argv, this.OutputHandler, this.ErrorHandler, ref result);
			if (num != 0)
			{
				throw new ApplicationException("Cannot run VM command", new Win32Exception(num));
			}
			if (this.outputBuffer.Length > 0 && this.userOutputHandler != null)
			{
				this.userOutputHandler(this.outputBuffer.ToString());
			}
			if (this.errorBuffer.Length > 0 && this.userErrorHandler != null)
			{
				this.userErrorHandler(this.errorBuffer.ToString());
			}
			return result;
		}

		private void OutputHandler(string chunk)
		{
			Command.CommonHandler(chunk, this.outputBuffer, this.userOutputHandler);
		}

		private void ErrorHandler(string chunk)
		{
			Command.CommonHandler(chunk, this.errorBuffer, this.userErrorHandler);
		}

		private static void CommonHandler(string chunk, StringBuilder sb, LineHandler handler)
		{
			sb.Append(chunk);
			string text = sb.ToString();
			string[] array = text.Split('\n');
			if (array.Length >= 2)
			{
				for (int i = 0; i < array.Length - 1; i++)
				{
					handler.Invoke(array[i]);
				}
				sb.Remove(0, sb.Length);
				sb.Append(array[array.Length - 1]);
			}
		}

		public void Kill()
		{
			Command.CommandKill(this.vmHandle, this.unitId);
		}
	}
}
