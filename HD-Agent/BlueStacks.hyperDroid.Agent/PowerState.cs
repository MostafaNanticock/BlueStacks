using System;
using System.Runtime.InteropServices;

namespace BlueStacks.hyperDroid.Agent
{
	[StructLayout(LayoutKind.Sequential)]
	public class PowerState
	{
		private const uint ERROR_SUCCESS = 0u;

		public ACLineStatus ACLineStatus;

		public BatteryFlag BatteryFlag;

		public byte BatteryLifePercent;

		public byte Reserved1;

		public int BatteryLifeTime;

		public int BatteryFullLifeTime;

		private PowerState()
		{
		}

		public static PowerState GetPowerState()
		{
			PowerState powerState = new PowerState();
			if (PowerState.GetSystemPowerStatusRef(powerState))
			{
				return powerState;
			}
			throw new ApplicationException("Unable to get power state");
		}

		public static Guid GetPowerActiveScheme()
		{
			IntPtr intPtr = Marshal.AllocHGlobal(16);
			Guid result = Guid.Empty;
			if (PowerState.PowerGetActiveScheme(IntPtr.Zero, ref intPtr) == 0)
			{
				result = (Guid)Marshal.PtrToStructure(intPtr, typeof(Guid));
			}
			Marshal.FreeHGlobal(intPtr);
			return result;
		}

		[DllImport("kernel32", EntryPoint = "GetSystemPowerStatus")]
		private static extern bool GetSystemPowerStatusRef(PowerState sps);

		[DllImport("powrprof.dll", CharSet = CharSet.Auto, SetLastError = true)]
		[return: MarshalAs(UnmanagedType.U4)]
		private static extern uint PowerGetActiveScheme(IntPtr UserRootPowerKey, ref IntPtr ActivePolicyGuid);
	}
}
