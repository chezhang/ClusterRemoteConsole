using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Microsoft.ComputeCluster.Admin
{
    internal class NativeMethods
    {
        /// <summary>
        /// Error codes from credential management functions
        /// </summary>
        public const int CredentialNotFoundErrorCode = 1168;
        public const int CredUiCancelledErrorCode = 1223;

        #region Enums

        /// <summary>
        /// Options for flags passed into CredUIPromptForCredentials
        /// Can be |'s to select mulitple
        /// </summary>
        [Flags]
        public enum CREDUI_FLAGS
        {
            INCORRECT_PASSWORD = 0x1,
            DO_NOT_PERSIST = 0x2,
            REQUEST_ADMINISTRATOR = 0x4,
            EXCLUDE_CERTIFICATES = 0x8,
            REQUIRE_CERTIFICATE = 0x10,
            SHOW_SAVE_CHECK_BOX = 0x40,
            ALWAYS_SHOW_UI = 0x80,
            REQUIRE_SMARTCARD = 0x100,
            PASSWORD_ONLY_OK = 0x200,
            VALIDATE_USERNAME = 0x400,
            COMPLETE_USERNAME = 0x800,
            PERSIST = 0x1000,
            SERVER_CREDENTIAL = 0x4000,
            EXPECT_CONFIRMATION = 0x20000,
            GENERIC_CREDENTIALS = 0x40000,
            USERNAME_TARGET_CREDENTIALS = 0x80000,
            KEEP_USERNAME = 0x100000,
        }

        /// <summary>
        /// Options for flags passed into CredUIPromptForWindowsCredentials
        /// Can be |'s to select mulitple
        /// </summary>
        [Flags]
        public enum CREDUIWIN_FLAGS
        {
            CREDUIWIN_GENERIC = 0x1,
            CREDUIWIN_CHECKBOX = 0x2,
            CREDUIWIN_AUTHPACKAGE_ONLY = 0x10,
            CREDUIWIN_IN_CRED_ONLY = 0x20,
            CREDUIWIN_ENUMERATE_ADMINS = 0x100,
            CREDUIWIN_ENUMERATE_CURRENT_USER = 0x200,
            CREDUIWIN_SECURE_PROMPT = 0x1000,
            CREDUIWIN_PACK_32_WOW = 0x10000000,
        }

        /// <summary>
        /// Options for flags passed into CredUnPackAuthenticationBuffer
        /// Can be |'s to select mulitple
        /// </summary>
        [Flags]
        public enum CREDPACK_FLAGS
        {
            CRED_PACK_PROTECTED_CREDENTIALS = 0x1,
            CRED_PACK_WOW_BUFFER = 0x2,
            CRED_PACK_GENERIC_CREDENTIALS = 0x4
        }

        /// <summary>
        /// Options for CREDENTIAL.Flags
        /// Can be |'s to select mulitple
        /// </summary>
        [Flags]
        public enum CREDENTIAL_FLAGS
        {
            CRED_FLAGS_PROMPT_NOW = 0x2,
            CRED_FLAGS_USERNAME_TARGET = 0x4
        }

        /// <summary>
        /// Options for the CREDENTIAL.Persist
        /// Only one can be selected
        /// </summary>
        [Flags]
        public enum CREDENTIAL_PERSIST
        {
            CRED_PERSIST_SESSION = 1,
            CRED_PERSIST_LOCAL_MACHINE = 2,
            CRED_PERSIST_ENTERPRISE = 3
        }

        /// <summary>
        /// Options for CREDENTIAL.Type
        /// Only one can be selected
        /// </summary>
        [Flags]
        public enum CREDENTIAL_TYPE
        {
            CRED_TYPE_GENERIC = 1,
            CRED_TYPE_DOMAIN_PASSWORD = 2,
            CRED_TYPE_DOMAIN_CERTIFICATE = 3,
            CRED_TYPE_DOMAIN_VISIBLE_PASSWORD = 4
        }

        #endregion

        #region Structs

        /// <summary>
        /// Info passed into CredUIPromptForCredentials
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct CREDUI_INFO
        {
            public int cbSize;
            public IntPtr hwndParent;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string pszMessageText;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string pszCaptionText;
            public IntPtr hbmBanner;
        }

        /// <summary>
        /// Used to represent a credential in CredWrite, CredRead and CredFree
        /// </summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct CREDENTIAL
        {
            public UInt32 Flags;
            public CREDENTIAL_TYPE Type;
            public IntPtr TargetName;
            public IntPtr Comment;
            public System.Runtime.InteropServices.ComTypes.FILETIME LastWritten;
            public UInt32 CredentialBlobSize;
            public IntPtr CredentialBlob;
            public UInt32 Persist;
            public UInt32 AttributeCount;
            public IntPtr Attributes;
            public IntPtr TargetAlias;
            public IntPtr UserName;
        }

        /// <summary>
        /// Used for CREDENTIAL.Attributes
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct CREDENTIAL_ATTRIBUTE
        {
            [MarshalAs(UnmanagedType.LPWStr)]
            public string Keyword;
            public int Flags;
            public int ValueSize;
            [MarshalAs(UnmanagedType.LPArray)]
            byte[] Value;
        }

        #endregion

        #region Methods

        /// <summary>
        /// The CredUIPromptForCredentials function creates and displays a configurable dialog box that accepts credentials 
        /// information from a user.
        /// </summary>
        /// <param name="creditUR">A pointer to a CREDUI_INFO structure that contains information for customizing 
        /// the appearance of the dialog box.</param>
        /// <param name="targetName">A string that contains the name of the target for the credentials, typically a server name</param>
        /// <param name="reserved1">This parameter is reserved for future use. It must be NULL.</param>
        /// <param name="iError">Specifies why the credential dialog box is needed. A caller can pass this Windows error parameter, 
        /// returned by another authentication call, to allow the dialog box to accommodate certain errors. 
        /// For example, if the password expired status code is passed, the dialog box prompts the user 
        /// to change the password on the account.</param>
        /// <param name="userName">A string that contains the user name for the credentials. 
        /// If a nonzero-length string is passed, the UserName option of the dialog box is prefilled with the string.</param>
        /// <param name="maxUserName">The maximum number of characters that can be copied to the username 
        /// including the terminating null character</param>
        /// <param name="password">A string that contains the password for the credentials. 
        /// If a nonzero-length string is specified for pszPassword, the password option of the dialog box will be prefilled with the string.</param>
        /// <param name="maxPassword">The maximum number of characters that can be copied to the password including the terminating null character.</param>
        /// <param name="iSave">Specifies the initial state of the Save check box and 
        /// receives the state of the Save check box after the user has responded to the dialog box. </param>
        /// <param name="flags">Options specified in CREDENTIAL_FLAGS enum</param>
        /// <returns>Returns 0 on success, error code on failure.</returns>
        [DllImport("credui", EntryPoint = "CredUIPromptForCredentialsW", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern int CredUIPromptForCredentials(ref CREDUI_INFO creditUR,
            string targetName,
            IntPtr reserved1,
            int iError,
            StringBuilder userName,
            int maxUserName,
            StringBuilder password,
            int maxPassword,
            ref int iSave,
            CREDUI_FLAGS flags);

        /// <summary>
        /// The CredWrite function creates a new credential or modifies an existing 
        /// credential in the user's credential set. The new credential is associated 
        /// with the logon session of the current token. The token must not have the 
        /// user's security identifier (SID) disabled.
        /// </summary>
        /// <param name="Credential">The CREDENTIAL structure to be written.</param>
        /// <param name="Flags">Flags that control the function's operation.</param>
        /// <returns>If the function succeeds, the function returns TRUE.
        /// If the function fails, it returns FALSE. Call the GetLastError function to get a more specific status code.
        /// </returns>
        [DllImport("Advapi32", EntryPoint = "CredWriteW", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern bool CredWrite(ref CREDENTIAL Credential,
            int Flags);

        /// <summary>
        /// The CredRead function reads a credential from the user's credential set. 
        /// The credential set used is the one associated with the logon session of the current token. 
        /// The token must not have the user's SID disabled.
        /// </summary>
        /// <param name="TargetName">The name of the credential to read.</param>
        /// <param name="Type">Type of the credential to read. Type must be one of the CRED_TYPE_* defined types.</param>
        /// <param name="Flags">Currently reserved and must be zero.</param>
        /// <param name="Credential">Pointer to a single allocated block buffer to return the credential. Any pointers contained within the buffer are pointers to locations within this single allocated block. The single returned buffer must be freed by calling CredFree.</param>
        /// <returns>The function returns TRUE on success and FALSE on failure. 
        /// The GetLastError function can be called to get a more specific status code.
        /// </returns>
        [DllImport("Advapi32", EntryPoint = "CredReadW", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern bool CredRead(string TargetName,
            int Type,
            int Flags,
            out IntPtr Credential);

        /// <summary>
        /// The CredFree function frees a buffer returned by any of the credentials management functions.
        /// </summary>
        /// <param name="Credential">Pointer to the buffer to be freed.</param>
        [DllImport("Advapi32", EntryPoint = "CredFree", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern void CredFree(IntPtr Credential);

        /// <summary>
        /// The CredUnPackAuthenticationBuffer function converts an authentication buffer returned by a call to the
        /// CredUIPromptForWindowsCredentials function into a string user name and password.
        /// </summary>
        /// <param name="flags">Flags - see MSDN.</param>
        /// <param name="pAuthBuffer">A pointer to the authentication buffer to be converted.</param>
        /// <param name="cbAuthBuffer">The size, in bytes, of the pAuthBuffer buffer.</param>
        /// <param name="pszUserName">A pointer to a null-terminated string that receives the user name..</param>
        /// <param name="pcchMaxUserName">A pointer to a DWORD value that specifies the size, in characters, of the pszUserName buffer.</param>
        /// <param name="pszDomainName">A pointer to a null-terminated string that receives the name of the user's domain.</param>
        /// <param name="pcchMaxDomainame">A pointer to a DWORD value that specifies the size, in characters, of the pszDomainName buffer.</param>
        /// <param name="pszPassword">A pointer to a null-terminated string that receives the password.</param>
        /// <param name="pcchMaxPassword">A pointer to a DWORD value that specifies the size, in characters, of the pszPassword buffer.</param>
        /// <returns>Returns true on success, false on failure.</returns>
        [DllImport("CredUI", EntryPoint = "CredUnPackAuthenticationBuffer", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern bool CredUnPackAuthenticationBuffer(CREDPACK_FLAGS flags,
            IntPtr pAuthBuffer,
            uint   cbAuthBuffer,
            StringBuilder pszUserName,
            ref int pcchMaxUserName,
            StringBuilder pszDomainName,
            ref int pcchMaxDomainame,
            StringBuilder pszPassword,
            ref int pcchMaxPassword);

        /// <summary>
        /// The CredUIPromptForWindowsCredentials function creates and displays a configurable dialog box that accepts credentials 
        /// information from a user.  
        /// </summary>
        /// <param name="pUiInfo">A pointer to a CREDUI_INFO structure that contains information for customizing 
        /// the appearance of the dialog box.</param>
        /// <param name="dwAuthError">A Windows error code, defined in Winerror.h, that is displayed in the dialog box.</param>
        /// <param name="pulAuthPackage">The authorization package for which the credentials in the pvInAuthBuffer buffer are serialized.</param>
        /// <param name="pvInAuthBuffer">A pointer to a credential BLOB that is used to populate the credential fields in the dialog box.</param>
        /// <param name="ulInAuthBufferSize">The size, in bytes, of the pvInAuthBuffer buffer.</param>
        /// <param name="ppvOutAuthBuffer">The address of a pointer that, on output, specifies the credential BLOB.</param>
        /// <param name="pulOutAuthBufferSize">The size, in bytes, of the ppvOutAuthBuffer buffer.</param>
        /// <param name="pfSave">Specifies the initial state of the Save check box and 
        /// receives the state of the Save check box after the user has responded to the dialog box. </param>
        /// <param name="dwFlags">Options specified in CREDUIWIN_FLAGS enum</param>
        /// <returns>Returns 0 on success, error code on failure.</returns>
        [DllImport("CredUI", EntryPoint = "CredUIPromptForWindowsCredentials", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern int CredUIPromptForWindowsCredentials(ref CREDUI_INFO pUiInfo,
            int dwAuthError,
            ref uint pulAuthPackage,
            IntPtr pvInAuthBuffer,
            int ulInAuthBufferSize,
            out IntPtr ppvOutAuthBuffer,
            out uint pulOutAuthBufferSize,
            ref int pfSave,
            CREDUIWIN_FLAGS dwFlags);

        /// <summary>
        /// The CoTaskMemFree function frees a buffer returned by the CredUIPromptForWindowsCredentials function.
        /// </summary>
        /// <param name="pv">Pointer to the buffer to be freed.</param>
        [DllImport("Ole32", EntryPoint = "CoTaskMemFree", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern void CoTaskMemFree(IntPtr pv);

        #endregion
    }
}
