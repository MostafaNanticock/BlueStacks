using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace BlueStacks.hyperDroid.Common.Interop
{
	public class SparseFile
	{
		[Flags]
		public enum FileSystemFeature : uint
		{
			FILE_CASE_SENSITIVE_SEARCH = 1u,
			FILE_CASE_PRESERVED_NAMES = 2u,
			FILE_UNICODE_ON_DISK = 4u,
			FILE_PERSISTENT_ACLS = 8u,
			FILE_FILE_COMPRESSION = 0x10,
			FILE_VOLUME_QUOTAS = 0x20,
			FILE_SUPPORTS_SPARSE_FILES = 0x40,
			FILE_SUPPORTS_REPARSE_POINTS = 0x80,
			FILE_VOLUME_IS_COMPRESSED = 0x8000,
			FILE_SUPPORTS_OBJECT_IDS = 0x10000,
			FILE_SUPPORTS_ENCRYPTION = 0x20000,
			FILE_NAMED_STREAMS = 0x40000,
			FILE_READONLY_VOLUME = 0x80000,
			FILE_SEQUENTIAL_WRITE_ONCE = 0x100000,
			FILE_SUPPORTS_TRANSACTIONS = 0x200000,
			FILE_SUPPORTS_HARD_LINKS = 0x400000,
			FILE_SUPPORTS_EXTENDED_ATTRIBUTES = 0x800000,
			FILE_SUPPORTS_OPEN_BY_FILE_ID = 0x1000000,
			FILE_SUPPORTS_USN_JOURNAL = 0x2000000
		}

		private const uint FILE_ATTRIBUTE_NORMAL = 128u;

		private const uint FILE_END = 2u;

		private const short INVALID_HANDLE_VALUE = -1;

		private const uint GENERIC_READ = 2147483648u;

		private const uint GENERIC_WRITE = 1073741824u;

		private const uint CREATE_NEW = 1u;

		private const uint CREATE_ALWAYS = 2u;

		private const uint OPEN_EXISTING = 3u;

		private const uint FSCTL_SET_SPARSE = 590020u;

		[DllImport("kernel32.dll")]
		private static extern bool GetVolumeInformation(string rootPathName, StringBuilder volumeNameBuffer, int volumeNameSize, out uint volumeSerialNumber, out uint maximumComponentLength, out FileSystemFeature fileSystemFlags, StringBuilder fileSystemNameBuffer, int nFileSystemNameSize);

		[DllImport("kernel32.dll")]
		private static extern IntPtr CreateFile(string lpFileName, uint dwDesiredAccess, uint dwShareMode, IntPtr lpSecurityAttributes, uint dwCreationDisposition, uint dwFlagsAndAttributes, IntPtr hTemplateFile);

		[DllImport("kernel32.dll")]
		private static extern bool DeviceIoControl(IntPtr hDevice, uint dwIoControlCode, IntPtr lpInBuffer, uint nInBufferSize, IntPtr lpOutBuffer, uint nOutBufferSize, out uint lpBytesReturned, IntPtr lpOverlapped);

		[DllImport("kernel32.dll")]
		private static extern bool SetFilePointerEx(IntPtr hFile, long liDistanceToMove, IntPtr lpNewFilePointer, uint dwMoveMethod);

		[DllImport("kernel32.dll")]
		private static extern bool SetEndOfFile(IntPtr hFile);

		[DllImport("kernel32.dll")]
		private static extern bool CloseHandle(IntPtr hObject);

		public static bool SupportsSparseFiles(string fileName)
		{
			string pathRoot = Path.GetPathRoot(fileName);
			StringBuilder stringBuilder = new StringBuilder(261);
			StringBuilder stringBuilder2 = new StringBuilder(261);
			uint num = default(uint);
			uint num2 = default(uint);
			FileSystemFeature fileSystemFeature = default(FileSystemFeature);
			SparseFile.GetVolumeInformation(pathRoot, stringBuilder, stringBuilder.Capacity, out num, out num2, out fileSystemFeature, stringBuilder2, stringBuilder2.Capacity);
			if ((fileSystemFeature & FileSystemFeature.FILE_SUPPORTS_SPARSE_FILES) == FileSystemFeature.FILE_SUPPORTS_SPARSE_FILES)
			{
				return true;
			}
			return false;
		}

		public unsafe static void CreateSparse(string fileNamePath, long sz)
		{
			if (File.Exists(fileNamePath))
			{
				File.Delete(fileNamePath);
			}
			IntPtr intPtr = SparseFile.CreateFile(fileNamePath, 3221225472u, 0u, IntPtr.Zero, 1u, 128u, IntPtr.Zero);
			int lastWin32Error = Marshal.GetLastWin32Error();
			if (intPtr.ToInt32() == -1)
			{
				throw new SystemException("CreateFile failed: " + lastWin32Error, new Win32Exception(lastWin32Error));
			}
			uint num = default(uint);
			if (!SparseFile.DeviceIoControl(intPtr, 590020u, IntPtr.Zero, 0u, IntPtr.Zero, 0u, out num, IntPtr.Zero))
			{
				throw new SystemException("DeviceIoControl failed: ", new Win32Exception(Marshal.GetLastWin32Error()));
			}
			ulong num2 = 0uL;
			IntPtr lpNewFilePointer = new IntPtr(&num2);
			if (!SparseFile.SetFilePointerEx(intPtr, sz, lpNewFilePointer, 2u))
			{
				throw new SystemException("SetFilePointerEx failed: ", new Win32Exception(Marshal.GetLastWin32Error()));
			}
			SparseFile.SetEndOfFile(intPtr);
			SparseFile.CloseHandle(intPtr);
		}

		public unsafe static void CreateNonSparse(string fileNamePath, long sz)
		{
			IntPtr intPtr = SparseFile.CreateFile(fileNamePath, 3221225472u, 0u, IntPtr.Zero, 1u, 128u, IntPtr.Zero);
			int lastWin32Error = Marshal.GetLastWin32Error();
			if (intPtr.ToInt32() == -1)
			{
				throw new SystemException("CreateFile failed: " + lastWin32Error, new Win32Exception(lastWin32Error));
			}
			ulong num = 0uL;
			IntPtr lpNewFilePointer = new IntPtr(&num);
			if (!SparseFile.SetFilePointerEx(intPtr, sz, lpNewFilePointer, 2u))
			{
				throw new SystemException("SetFilePointerEx failed: ", new Win32Exception(Marshal.GetLastWin32Error()));
			}
			SparseFile.SetEndOfFile(intPtr);
			SparseFile.CloseHandle(intPtr);
		}
	}
}
