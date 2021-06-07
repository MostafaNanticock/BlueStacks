using System;
using System.Runtime.InteropServices;

namespace BlueStacks.hyperDroid.Frontend.Interop
{
	public class DWM
	{
		private const uint DWM_EC_DISABLECOMPOSITION = 0u;

		private const uint DWM_EC_ENABLECOMPOSITION = 1u;

		public static bool CompositionEnabled
		{
			get
			{
				bool result = false;
				uint num = DWM.DwmIsCompositionEnabled(ref result);
				if (num != 0)
				{
					throw new SystemException("Cannot check if DWM composition is enabled: 0x" + num.ToString("x"));
				}
				return result;
			}
		}

		[DllImport("dwmapi.dll")]
		private static extern uint DwmIsCompositionEnabled(ref bool enabled);

		[DllImport("dwmapi.dll")]
		private static extern uint DwmEnableComposition(uint action);

		public static void DisableComposition()
		{
			if (DWM.CompositionEnabled)
			{
				uint num = DWM.DwmEnableComposition(0u);
				if (num == 0)
				{
					return;
				}
				throw new SystemException("Cannot disable DWM composition: 0x" + num.ToString("x"));
			}
		}
	}
}
