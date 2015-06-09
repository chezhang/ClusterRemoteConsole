using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;

namespace Microsoft.ComputeCluster.Admin
{
    /// <summary>
    /// Helper class which handles getting and saving username/password credentials
    /// </summary>
    [System.Security.Permissions.PermissionSetAttribute(System.Security.Permissions.SecurityAction.Demand, Name="FullTrust")]
    internal class GetCredential
    {
        #region Private Fields
        
        /// <summary>
        /// Maximum length of a username
        /// </summary>
        private const int CRED_MAX_USERNAME_LENGTH = (256 + 1 + 256);

        /// <summary>
        /// Maximum length of a password
        /// </summary>
        private const int CRED_MAX_PASSWORD_LENGTH = 256;

        /// <summary>
        /// Maximum length of a domain
        /// </summary>
        private const int CRED_MAX_DOMAIN_LENGTH = 256;

        /// <summary>
        /// The current password
        /// </summary>
        private SecureString password;

        /// <summary>
        /// The current user name
        /// </summary>
        private string username;

        #endregion

        #region Constructor
        #endregion

        #region Properties

        /// <summary>
        /// The name used by this class to identify credentials stored
        /// in the windows credential manager
        /// </summary>
        private string TargetName
        {
            get
            {
                return "Microsoft_HPC_ClusterRemoteConsole_AllNodes";
            }
        }

        /// <summary>
        /// The password.
        /// Call Get() to fill.
        /// </summary>
        public SecureString Password
        {
            get
            {
                return password;
            }
        }

        /// <summary>
        /// The user name.
        /// Call Get() to fill.
        /// </summary>
        public string Username
        {
            get
            {
                return username;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Fills the Username and Password properties.
        /// Gets the values from the windows credential manager if they were previously stored there
        /// Otherwise, prompts the user and saves to the windows credman if the user asked to save them.
        /// </summary>
        /// <param name="clusterName"></param>
        /// <param name="formHandle"></param>
        /// <param name="alwaysAsk">Whether to ask the user for cred info if a stored cred is found in cred man.</param>
        public void Get(string clusterName, IntPtr formHandle, bool alwaysAsk)
        {
            string caption = Resources.CredentialDialogTitle;
            string message = Resources.CredentialDialogMessage;
            bool save = false;

            if (alwaysAsk || !RetrieveCredential())
            {
                //
                //  Prompt for credentials, using OS version specific function
                //
                System.OperatingSystem osInfo = System.Environment.OSVersion;
                if (osInfo.Version.Major < 6) // For versions prior to Vista
                {
                    PromptForCredentialsLegacy(ref this.username, ref this.password, ref save, formHandle, caption, message);
                }
                else
                {
                    PromptForCredentials(ref this.username, ref this.password, ref save, formHandle, caption, message);
                }

                if (save)
                {
                    SaveCredential();
                }
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Helper method that attempts to retrive credentials previously stored in the windows credential manager
        /// Populates the Username and Password properties if a stored credential is found.
        /// </summary>
        /// <returns></returns>
        private bool RetrieveCredential()
        {
            IntPtr credPtr;

            bool success = NativeMethods.CredRead(this.TargetName,
                (int)NativeMethods.CREDENTIAL_TYPE.CRED_TYPE_GENERIC,
                0,
                out credPtr);
            int error = Marshal.GetLastWin32Error();

            if (success)
            {
                try
                {
                    NativeMethods.CREDENTIAL cred = (NativeMethods.CREDENTIAL)Marshal.PtrToStructure(credPtr, typeof(NativeMethods.CREDENTIAL));
                    this.username = Marshal.PtrToStringUni(cred.UserName);
                    this.password = LoadPassword(cred.CredentialBlob, (int)cred.CredentialBlobSize);
                    ZeroPassword(cred.CredentialBlob, (int)cred.CredentialBlobSize);
                }
                finally
                {                    
                    NativeMethods.CredFree(credPtr);
                }
            }
            else if (error == NativeMethods.CredentialNotFoundErrorCode) 
            {
                // no credential found, clear the username/password to avoid confusion
                this.username = null;
                this.password = null;
            }
            else
            {
                throw new System.ComponentModel.Win32Exception(error);
            }

            if (String.IsNullOrEmpty(this.username) || this.password == null || this.password.Length == 0)
            {
                // Incomplete credential found, clear the username/password to avoid confusion
                this.username = null;
                this.password = null;
                return false;
            }

            return true;
        }

        
        /// <summary>
        /// Zero out the memory corresponding to the password (but do not free it).
        /// </summary>
        private static void ZeroPassword(IntPtr credBlob, int credBlobSize)
        {
            if (credBlob.Equals(IntPtr.Zero) || credBlobSize == 0)
            {
                return;
            }
            byte[] zeros = new byte[credBlobSize];
            Array.Clear (zeros, 0, zeros.Length);
            Marshal.Copy(zeros, 0, credBlob, credBlobSize);
        }
        
        /// <summary>
        /// Helper method to load a password from an unmanaged buffer into a secure string.
        /// </summary>
        private static SecureString LoadPassword(IntPtr credBlob, int credBlobSize)
        {
            //
            // Avoid creating strings containing the password, as they can
            // persist in memory for an unspecified amount of time.
            //
            System.Diagnostics.Debug.Assert((credBlobSize % sizeof(char)) == 0);
            char[] passChars = new char[credBlobSize / sizeof(char)];
            Marshal.Copy(credBlob, passChars, 0, passChars.Length);
            
            SecureString password = new SecureString();
            for (int i=0; i<passChars.Length; i++)
            {
                password.AppendChar(passChars[i]);                
            } 
            password.MakeReadOnly();

            // Overwrite cleartext password
            Array.Clear(passChars, 0, passChars.Length);

            return password;
        }

        /// <summary>
        /// Helper method which saves the credential info in the Username and Password properties
        /// into the windows credential manager
        /// </summary>        
        private void SaveCredential()
        {
            NativeMethods.CREDENTIAL cred = new NativeMethods.CREDENTIAL();
            string comment = "Credential used by Microsoft HPC pack's ClusterRemoteConsole tool to connect to all nodes";

            cred.Flags   = 0;
            cred.Type    = NativeMethods.CREDENTIAL_TYPE.CRED_TYPE_GENERIC;
            cred.Persist = (UInt32)NativeMethods.CREDENTIAL_PERSIST.CRED_PERSIST_LOCAL_MACHINE;
            cred.AttributeCount = 0;
            cred.Attributes     = IntPtr.Zero;
            cred.TargetAlias    = IntPtr.Zero;

            cred.TargetName     = IntPtr.Zero;
            cred.Comment        = IntPtr.Zero;
            cred.UserName       = IntPtr.Zero;            
            cred.CredentialBlob = IntPtr.Zero;
            cred.CredentialBlobSize = 0;
            try
            {
                cred.TargetName = Marshal.StringToCoTaskMemUni(this.TargetName);
                cred.Comment = Marshal.StringToCoTaskMemUni(comment);                
                cred.UserName = Marshal.StringToCoTaskMemUni(this.username);
                cred.CredentialBlob = Marshal.SecureStringToCoTaskMemUnicode(this.Password);
                cred.CredentialBlobSize = (UInt32)(this.Password.Length * sizeof(char));

                bool success = NativeMethods.CredWrite(ref cred, 0);
                int error = Marshal.GetLastWin32Error();
                if (!success)
                {
                    throw new System.ComponentModel.Win32Exception(error);
                }
            }
            finally
            {
                Marshal.ZeroFreeCoTaskMemUnicode(cred.TargetName);
                Marshal.ZeroFreeCoTaskMemUnicode(cred.Comment);
                Marshal.ZeroFreeCoTaskMemUnicode(cred.UserName);
                Marshal.ZeroFreeCoTaskMemUnicode(cred.CredentialBlob);
            }                
        }
         
        /// <summary>
        /// Helper method which prompts the user for credential info using the Win32 api
        /// This method is only for XP and Windows Server 2003 as it uses a deprecated API
        /// </summary>
        /// <param name="username">The username</param>
        /// <param name="password">The password</param>
        /// <param name="save">Whether the user requested that the credential be saved</param>
        /// <param name="hwndParent">Handle for the parent window of the dialog box</param>
        /// <param name="caption">Title of the dialog box</param>
        /// <param name="message">Message to display in the dialog box</param>
        private void PromptForCredentialsLegacy(ref string username, ref SecureString password, ref bool save,
            IntPtr hwndParent, string caption, string message)
        {
            NativeMethods.CREDUI_FLAGS flags = NativeMethods.CREDUI_FLAGS.GENERIC_CREDENTIALS |
                                                     NativeMethods.CREDUI_FLAGS.EXCLUDE_CERTIFICATES |
                                                     NativeMethods.CREDUI_FLAGS.DO_NOT_PERSIST |
                                                     NativeMethods.CREDUI_FLAGS.SHOW_SAVE_CHECK_BOX |
                                                     NativeMethods.CREDUI_FLAGS.ALWAYS_SHOW_UI |
                                                     NativeMethods.CREDUI_FLAGS.COMPLETE_USERNAME;

            NativeMethods.CREDUI_INFO info = new NativeMethods.CREDUI_INFO();

            StringBuilder user = new StringBuilder(username, CRED_MAX_USERNAME_LENGTH);
            StringBuilder pwd = new StringBuilder(CRED_MAX_PASSWORD_LENGTH);
            info.cbSize = Marshal.SizeOf(info);
            info.hwndParent = hwndParent;
            info.pszCaptionText = caption;
            info.pszMessageText = message;

            int saveCredentials = save ? 1 : 0;
            int result = NativeMethods.CredUIPromptForCredentials(
                                                ref info,
                                                String.Empty,
                                                IntPtr.Zero,
                                                0,
                                                user,
                                                CRED_MAX_USERNAME_LENGTH,
                                                pwd,
                                                CRED_MAX_PASSWORD_LENGTH,
                                                ref saveCredentials,
                                                flags);

            if (result != 0)
            {
                if (result == NativeMethods.CredUiCancelledErrorCode)
                {
                    username = null;
                    password = null;
                    save = false;
                    return;
                }
                throw new System.ComponentModel.Win32Exception(result);
            }

            username = user.ToString();
            password = new SecureString();
            for (int i = 0; i < pwd.Length; i++)
            {
                password.AppendChar(pwd[i]);
            }
            password.MakeReadOnly();

            // overwrite the cleartext password
            pwd.Remove(0, pwd.Length);

            save = (saveCredentials != 0);
        }

        /// <summary>
        /// Helper method which prompts the user for credential info using the Win32 api
        /// </summary>
        /// <param name="username">The username</param>
        /// <param name="password">The password</param>
        /// <param name="save">Whether the user requested that the credential be saved</param>
        /// <param name="hwndParent">Handle for the parent window of the dialog box</param>
        /// <param name="caption">Title of the dialog box</param>
        /// <param name="message">Message to display in the dialog box</param>
        private void PromptForCredentials(ref string username, ref SecureString password, ref bool save,
            IntPtr hwndParent, string caption, string message)
        {
            NativeMethods.CREDUIWIN_FLAGS flags = NativeMethods.CREDUIWIN_FLAGS.CREDUIWIN_AUTHPACKAGE_ONLY |
                                                  NativeMethods.CREDUIWIN_FLAGS.CREDUIWIN_CHECKBOX;

            NativeMethods.CREDUI_INFO info = new NativeMethods.CREDUI_INFO();

            StringBuilder user = new StringBuilder(username, CRED_MAX_USERNAME_LENGTH);
            StringBuilder pwd = new StringBuilder(CRED_MAX_PASSWORD_LENGTH);
            StringBuilder domain = new StringBuilder(CRED_MAX_DOMAIN_LENGTH);

            info.cbSize = Marshal.SizeOf(info);
            info.hwndParent = hwndParent;
            info.pszCaptionText = caption;
            info.pszMessageText = message;

            int saveCredentials = save ? 1 : 0;

            //  Prompt for user credentials
            IntPtr credBuffer;
            uint credBufferSize;
            uint authPackage = 0;

            int result = NativeMethods.CredUIPromptForWindowsCredentials(ref info,
                                                0,
                                                ref authPackage,
                                                IntPtr.Zero,
                                                0,
                                                out credBuffer,
                                                out credBufferSize,
                                                ref saveCredentials,
                                                flags);

            if (result != 0)
            {
                if (result == NativeMethods.CredUiCancelledErrorCode)
                {
                    username = null;
                    password = null;
                    save = false;
                    return;
                }
                throw new System.ComponentModel.Win32Exception(result);
            }

            int userLength = CRED_MAX_USERNAME_LENGTH;
            int pwdLength = CRED_MAX_PASSWORD_LENGTH;
            int domainLength = CRED_MAX_DOMAIN_LENGTH;

            NativeMethods.CredUnPackAuthenticationBuffer(NativeMethods.CREDPACK_FLAGS.CRED_PACK_PROTECTED_CREDENTIALS,
                                                credBuffer,
                                                credBufferSize,
                                                user,
                                                ref userLength,
                                                domain,
                                                ref domainLength,
                                                pwd,
                                                ref pwdLength);

            username = user.ToString();
            password = new SecureString();
            for (int i = 0; i < pwd.Length; i++)
            {
                password.AppendChar(pwd[i]);
            }
            password.MakeReadOnly();

            // Overwrite the cleartext password
            pwd.Remove(0, pwd.Length);

            // Free buffer
            ZeroPassword(credBuffer, (int)credBufferSize);
            NativeMethods.CoTaskMemFree(credBuffer);

            save = (saveCredentials != 0);
        }

        /// <summary>
        /// Helper method that takes a SecureString and returns a corresponding string
        /// </summary>
        /// <param name="secureString">A secure string to unsecure</param>
        /// <returns>An unsecure string</returns>
        private static string UnsecureString(SecureString secureString)
        {
            if (secureString == null)
            {
                return null;
            }
            string unsecureString;

            IntPtr ptr = Marshal.SecureStringToGlobalAllocUnicode(secureString);
            try
            {
                unsecureString = Marshal.PtrToStringUni(ptr);
            }
            finally
            {
                Marshal.ZeroFreeGlobalAllocUnicode(ptr);
            }
            return unsecureString;
        }

        #endregion
    }
}
