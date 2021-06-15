using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ADPasswordSecureCache.Encryption
{
    /// <summary>
    /// Class used as Symetric encryptor/decryptor initialized with Key and IV providing the methods for 
    /// encrypt SecureString to Stream and decrypt Stream to SecureString
    /// </summary>
    public class StringEncryptor
    {
        private static readonly string protectedFile = "protected.dat";
        private static readonly string vectorFile = "vector.dat";

        private readonly AesCryptoServiceProvider cryptic = null;

        private static readonly byte[] s_additionalEntropy = { 167, 45, 26, 44, 179, 247, 215, 41, 79, 136, 41, 58, 234, 59, 71, 194, 132, 116, 117, 156, 227, 9, 225, 85, 197, 14, 171, 123, 226, 211, 200, 58, 186, 27, 174, 118, 141, 206, 247, 37, 37, 214, 185, 136, 0, 64, 122, 138, 233, 27, 65, 247, 106, 183, 170, 196, 176, 62, 9, 105, 243, 35, 234, 192, 118, 50, 187, 98, 92, 144, 72, 72, 126, 138, 215, 8, 53, 173, 86, 132, 226, 245, 177, 73, 69, 253, 207, 220, 246, 24, 193, 169, 121, 112, 123, 225, 187, 74, 121, 55, 0, 53, 4, 26, 251, 93, 217, 125, 79, 157, 85, 142, 45, 251, 68, 203, 245, 246, 0, 226, 140, 214, 239, 85, 155, 27, 196, 29 };

        private static readonly IsolatedStorageFile isoStore = IsolatedStorageFile.GetStore(IsolatedStorageScope.Assembly | IsolatedStorageScope.User, null, null);


        private static IsolatedStorageFileStream GetStorageStreamForWriting(string dtaFile)
        {
            return new IsolatedStorageFileStream(dtaFile, FileMode.Create, FileAccess.Write, isoStore);
        }

        private static IsolatedStorageFileStream GetStorageStreamForReading(string dtaFile)
        {
            return new IsolatedStorageFileStream(dtaFile, FileMode.OpenOrCreate, FileAccess.Read, isoStore);
        }



        private static byte[] Protect(byte[] data)
        {
            try
            {
                // Encrypt the data using DataProtectionScope.CurrentUser. The result can be decrypted
                // only by the same current user.
                return ProtectedData.Protect(data, s_additionalEntropy, DataProtectionScope.CurrentUser);
            }
            catch (CryptographicException e)
            {
                throw new Exception("Protected data was not encrypted. An error occurred.",e);
            }
        }

        private static byte[] Unprotect(byte[] data)
        {
            try
            {
                //Decrypt the data using DataProtectionScope.CurrentUser.
                return ProtectedData.Unprotect(data, s_additionalEntropy, DataProtectionScope.CurrentUser);
            }
            catch (CryptographicException e)
            {
                throw new Exception("Protected data was not decrypted. An error occurred.",e);
            }
        }


        public StringEncryptor()
        {
            using (RijndaelManaged myRijndael = new RijndaelManaged())
            {

                using (IsolatedStorageFileStream protectedStream = GetStorageStreamForReading(protectedFile))
                {
                    if (protectedStream.CanRead && protectedStream.Length > 0)
                    {
                        byte[] protectedData = new byte[protectedStream.Length];
                        protectedStream.Read(protectedData, 0, (int)protectedStream.Length);
                        myRijndael.Key = Unprotect(protectedData);
                        protectedStream.Close();
                    }
                    else
                    {
                        myRijndael.GenerateKey();
                    }
                }

                using (IsolatedStorageFileStream protectedStream = GetStorageStreamForWriting(protectedFile))
                {
                    byte[] protectedByte = Protect(myRijndael.Key);
                    if (protectedStream.CanWrite)
                    {
                        protectedStream.Write(protectedByte, 0, protectedByte.Length);
                        protectedStream.Flush();
                        protectedStream.Close();
                    }
                }

                using (IsolatedStorageFileStream vectorStream = GetStorageStreamForReading(vectorFile))
                {
                    if (vectorStream.CanRead && vectorStream.Length > 0)
                    {
                        byte[] vectorData = new byte[vectorStream.Length];
                        vectorStream.Read(vectorData, 0, (int)vectorStream.Length);
                        myRijndael.IV = Unprotect(vectorData);
                        vectorStream.Close();
                    }
                    else
                    {
                        myRijndael.GenerateIV();
                    }
                }

                using (IsolatedStorageFileStream vectorStream = GetStorageStreamForWriting(vectorFile))
                {
                    byte[] protectedByte = Protect(myRijndael.IV);
                    if (vectorStream.CanWrite)
                    {
                        vectorStream.Write(protectedByte, 0, protectedByte.Length);
                        vectorStream.Flush();
                        vectorStream.Close();
                    }
                }

                cryptic = new AesCryptoServiceProvider
                {
                    KeySize = 256,
                    Key = myRijndael.Key,
                    IV = myRijndael.IV,
                    Padding = PaddingMode.Zeros
                };
            }
        }

        public StringEncryptor(SecureString key, SecureString IV)
        {
            cryptic = new AesCryptoServiceProvider
            {
                KeySize = 256,
                Key = ASCIIEncoding.ASCII.GetBytes(key.ConvertToString()),
                IV = ASCIIEncoding.ASCII.GetBytes(IV.ConvertToString()),
                Padding = PaddingMode.Zeros
            };
        }


        public Stream Encrypt(SecureString value)
        {
            MemoryStream stream = new MemoryStream(2*value.Length);

            MemoryStream outStream = new MemoryStream(2 * value.Length);

            using (CryptoStream crStream = new CryptoStream(stream,
               cryptic.CreateEncryptor(), CryptoStreamMode.Write))
            {

                byte[] data = ASCIIEncoding.UTF8.GetBytes(value.ConvertToString());

                crStream.Write(data, 0, data.Length);
                crStream.FlushFinalBlock();


                stream.WriteTo(outStream);

                outStream.Seek(0, SeekOrigin.Begin);

                crStream.Close();

            }

            return outStream;
        }

        public SecureString Decrypt(Stream value)
        {

            CryptoStream crStream = new CryptoStream(value,
                cryptic.CreateDecryptor(), CryptoStreamMode.Read);

            StreamReader reader = new StreamReader(crStream);

            SecureString data = reader.ReadToEnd().ToSecureString();

            reader.Close();
            value.Close();

            return data;

        }
    }
}
