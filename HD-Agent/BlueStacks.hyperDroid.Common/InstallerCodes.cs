namespace BlueStacks.hyperDroid.Common
{
	internal enum InstallerCodes
	{
		SUCCESS_CODE,
		Success = 0,
		INVALID_CODE = -1,
		PROCESS_ALREADY_RUNNING = -2,
		INSTALL_FAILED_SERVER_ERROR = -3,
		USER_EXITED = -4,
		INSTALL_FAILED_ALREADY_EXISTS = 1,
		INSTALL_PARSE_FAILED_UNEXPECTED_EXCEPTION,
		INSTALL_FAILED_CONFLICTING_PROVIDER,
		INSTALL_FAILED_CONTAINER_ERROR,
		INSTALL_FAILED_CPU_ABI_INCOMPATIBLE,
		INSTALL_FAILED_DEXOPT,
		INSTALL_FAILED_DUPLICATE_PACKAGE,
		INSTALL_FAILED_INSUFFICIENT_STORAGE,
		INSTALL_FAILED_INTERNAL_ERROR,
		INSTALL_FAILED_INVALID_APK,
		INSTALL_FAILED_INVALID_INSTALL_LOCATION,
		INSTALL_FAILED_INVALID_URI,
		INSTALL_FAILED_MEDIA_UNAVAILABLE,
		INSTALL_FAILED_MISSING_FEATURE,
		INSTALL_FAILED_MISSING_SHARED_LIBRARY,
		INSTALL_FAILED_NEWER_SDK,
		INSTALL_FAILED_NO_SHARED_USER,
		INSTALL_FAILED_OLDER_SDK,
		INSTALL_FAILED_REPLACE_COULDNT_DELETE,
		INSTALL_FAILED_SHARED_USER_INCOMPATIBLE,
		INSTALL_FAILED_TEST_ONLY,
		INSTALL_FAILED_UPDATE_INCOMPATIBLE,
		INSTALL_PARSE_FAILED_BAD_MANIFEST,
		INSTALL_PARSE_FAILED_BAD_PACKAGE_NAME,
		INSTALL_PARSE_FAILED_BAD_SHARED_USER_ID,
		INSTALL_PARSE_FAILED_CERTIFICATE_ENCODING,
		INSTALL_PARSE_FAILED_INCONSISTENT_CERTIFICATES,
		INSTALL_PARSE_FAILED_MANIFEST_EMPTY,
		INSTALL_PARSE_FAILED_MANIFEST_MALFORMED,
		INSTALL_PARSE_FAILED_NO_CERTIFICATES,
		INSTALL_PARSE_FAILED_NOT_APK
	}
}
