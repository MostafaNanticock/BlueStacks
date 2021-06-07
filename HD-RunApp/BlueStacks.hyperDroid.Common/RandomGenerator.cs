using System;
using System.Security.Cryptography;

namespace BlueStacks.hyperDroid.Common
{
	public static class RandomGenerator
	{
		private static RNGCryptoServiceProvider s_RandomProvider = new RNGCryptoServiceProvider();

		[ThreadStatic]
		private static Random s_RandomPerThread;

		public static int Next(int maxValue)
		{
			if (RandomGenerator.s_RandomPerThread == null)
			{
				byte[] array = new byte[4];
				RandomGenerator.s_RandomProvider.GetBytes(array);
				RandomGenerator.s_RandomPerThread = new Random(BitConverter.ToInt32(array, 0));
			}
			return RandomGenerator.s_RandomPerThread.Next(maxValue);
		}
	}
}
