using System;
using System.Runtime.InteropServices;

namespace BlueStacks.hyperDroid.Common.Interop
{
	public class UUID
	{
		public class EUUID : Exception
		{
			public EUUID()
			{
			}

			public EUUID(string reason)
				: base(reason)
			{
			}
		}

		public class EUUIDLocalOnly : EUUID
		{
		}

		public class EUUIDNoAddress : EUUID
		{
		}

		public enum UUIDTYPE
		{
			GLOBAL,
			LOCAL
		}

		private const long RPC_S_OK = 0L;

		private const long RPC_S_UUID_LOCAL_ONLY = 1824L;

		private const long RPC_S_UUID_NO_ADDRESS = 1739L;

		[DllImport("rpcrt4.dll")]
		private static extern int UuidCreateSequential(out Guid guid);

		public static Guid GenerateUUID(UUIDTYPE type)
		{
			Guid result = default(Guid);
			long num = UUID.UuidCreateSequential(out result);
			switch (num)
			{
			case 1739L:
				throw new EUUIDNoAddress();
			case 1824L:
				if (type == UUIDTYPE.GLOBAL)
				{
					throw new EUUIDLocalOnly();
				}
				goto default;
			default:
				throw new EUUID("UuidToString failed. rc = " + num);
			case 0L:
				return result;
			}
		}
	}
}
