namespace BlueStacks.hyperDroid.Agent
{
	public enum BatteryFlag : byte
	{
		High = 1,
		Low,
		Critical = 4,
		Charging = 8,
		NoSystemBattery = 0x80,
		Unknown = 0xFF
	}
}
