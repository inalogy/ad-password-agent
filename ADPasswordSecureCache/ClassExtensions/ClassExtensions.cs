using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Runtime.InteropServices;

namespace ADPasswordSecureCache
{
    public static class ClassExtensions
    {

        /// <summary>
        /// Extension for use with FileInfo for chacking if the file is currently used and locked
        /// </summary>
        #region FileInfo Extension

        public static bool IsFileLocked(this FileInfo fi)
        {
            bool lockStatus = false;
            try
            {
                using (FileStream fileStream = File.Open(fi.FullName, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
                {
                    // File/Stream manipulating code here

                    lockStatus = !fileStream.CanWrite;

                }
            }
            catch
            {
                //check here why it failed and ask user to retry if the file is in use.
                lockStatus = true;
            }
            return lockStatus;
        }

        #endregion

        /// <summary>
        /// Extension for use with SecureString and String objects for simplying secure conversion
        /// of Strings to a secure form and back
        /// </summary>
        #region SecureString Extension


        /// <summary>
        /// Original String value is automatically zeroed out leaving only the secure encrypted object in one step.
        /// Call: SecureString s = myString.toSecureString();
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static SecureString ToSecureString(this String value, bool leaveOriginal = false, bool makeReadOnly = true)
        {
            value.CheckNullRef();
            SecureString secureString;

            unsafe
            {
                fixed (char* chars = value)
                {
                    //create encrypted secure string object
                    secureString = new SecureString(chars, value.Length);
                    if (makeReadOnly)
                        secureString.MakeReadOnly(); //.AddChar and InsertAt methods won't work if true

                    if (!leaveOriginal)
                        value.SecureClear();
                }
            }

            return secureString;
        }



        /// <summary>
        /// Decrypts the SecureString converting it to a String for convenience and 
        /// systematic method of doing on the object
        /// Call: String s = secureObject.ConvertToString();
        /// </summary>
        /// <param name="SecureString">An allocated SecureString object</param> 
        /// <returns>The decrypted String</returns>
        public static String ConvertToString(this SecureString value)
        {
            value.CheckNullRef();

            IntPtr stringPointer = IntPtr.Zero;
            string result = String.Empty;

            try
            {
                stringPointer = Marshal.SecureStringToGlobalAllocUnicode(value);
                result = Marshal.PtrToStringUni(stringPointer);
            }
            finally
            {
                if (stringPointer != IntPtr.Zero)
                {
                    Marshal.ZeroFreeGlobalAllocUnicode(stringPointer);
                }
            }

            return result;
        }


        /// <summary>
        /// Facilitates comparison of two SecureString objects, decrypting each briefly then zeroing
        /// out the cleartext once the comparison is performed.
        /// Call: SecureString.SecureCompare(SecureString object);
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool SecureCompare(this SecureString left, SecureString right)
        {
            bool result = false;

            //temporarily decrypt both SecureString objects
            String leftValue = ConvertToString(left);
            String rightValue = ConvertToString(right);

            result = leftValue == rightValue;

            //clear memory 
            leftValue.SecureClear();
            rightValue.SecureClear();

            return result;
        }



        /// <summary>
        /// Offers existing String type a safe zeroing out method without returning a SecureString object while taking care
        /// not to zero out string literals or values that share the same location as a string literal
        /// Call: myString.SecureClear();
        /// </summary>
        /// <param name="value"></param>
        public static void SecureClear(this String value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                object checkInterned = String.IsInterned(value);
                if (checkInterned == null)
                {
                    unsafe
                    {
                        fixed (char* chars = value)
                        {
                            //zero out original
                            for (int i = 0; i < value.Length; i++)
                                chars[i] = '\0';
                        }
                    }
                }
            }
        }



        /// <summary>
        /// Create a hash of the original data within a securestring e.g. of a password or other value
        /// Call: String hash = mySecureString.HashValue
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static String SHA256HashValue(this SecureString value)
        {
            String original = value.ConvertToString();
            byte[] bytes = Encoding.UTF8.GetBytes(original);

            var sha2 = new SHA256Managed();
            byte[] hashBytes = sha2.ComputeHash(bytes);

            //cleanup original
            original.SecureClear();

            return HexStringFromBytes(hashBytes);
        }

        /// <summary>
        /// Convert an array of bytes to a string of hex digits
        /// </summary>
        /// <param name="bytes">array of bytes</param>
        /// <returns>String of hex digits</returns>
        private static string HexStringFromBytes(byte[] bytes)
        {
            var sb = new StringBuilder();
            foreach (byte b in bytes)
            {
                var hex = b.ToString("x2");
                sb.Append(hex);
            }
            return sb.ToString();
        }


        private static void CheckNullRef(this object value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }
        }


        #endregion


    }
}
