using System;
using System.Runtime.InteropServices;

namespace BlueStacks.hyperDroid.Frontend.Interop
{
	public class Video
	{
		public class Mode
		{
			public int width;

			public int height;

			public int depth;

			public int Width
			{
				get
				{
					return this.width;
				}
			}

			public int Height
			{
				get
				{
					return this.height;
				}
			}

			public int Depth
			{
				get
				{
					return this.depth;
				}
			}

			public Mode(int width, int height, int depth)
			{
				this.width = width;
				this.height = height;
				this.depth = depth;
			}
		}

		private const uint OFFSET_MAGIC = 0u;

		private const uint OFFSET_LENGTH = 4u;

		private const uint OFFSET_OFFSET = 8u;

		private const uint OFFSET_MODE = 12u;

		private const uint OFFSET_STRIDE = 16u;

		private const uint OFFSET_DIRTY = 20u;

		private IntPtr addr;

		private unsafe byte* raw;

		[DllImport("HD-Frontend-Native.dll")]
		private static extern bool VideoCheckMagic(IntPtr addr, ref uint magic);

		[DllImport("HD-Frontend-Native.dll")]
		private static extern void VideoGetMode(IntPtr addr, ref uint width, ref uint height, ref uint depth);

		[DllImport("HD-Frontend-Native.dll")]
		private static extern bool VideoGetAndClearDirty(IntPtr addr);

		public unsafe Video(IntPtr addr)
		{
			this.addr = addr;
			this.raw = (byte*)(void*)addr;
		}

		public void CheckMagic()
		{
			uint num = 0u;
			if (Video.VideoCheckMagic(this.addr, ref num))
			{
				return;
			}
			throw new SystemException("Bad magic 0x" + num.ToString("x"));
		}

		public Mode GetMode()
		{
			uint width = 0u;
			uint height = 0u;
			uint depth = 0u;
			Video.VideoGetMode(this.addr, ref width, ref height, ref depth);
			return new Mode((int)width, (int)height, (int)depth);
		}

		public bool GetAndClearDirty()
		{
			return Video.VideoGetAndClearDirty(this.addr);
		}

		public unsafe uint GetStride()
		{
			ushort* ptr = (ushort*)(this.raw + 16u);
			return *ptr;
		}

		public unsafe IntPtr GetBufferAddr()
		{
			uint* ptr = (uint*)(this.raw + 8u);
			uint* value = (uint*)(this.raw + (int)(*ptr));
			return (IntPtr)(void*)value;
		}

		public unsafe IntPtr GetBufferEnd()
		{
			uint* ptr = (uint*)(this.raw + 4u);
			uint* value = (uint*)(this.raw + (int)(*ptr));
			return (IntPtr)(void*)value;
		}

		public uint GetBufferSize()
		{
			int num = (int)this.GetBufferEnd() - (int)this.GetBufferAddr();
			if (num < 0)
			{
				throw new SystemException("Buffer size is negative");
			}
			return (uint)num;
		}
	}
}
