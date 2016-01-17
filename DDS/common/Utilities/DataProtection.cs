using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;
using System.IO;
using System.Runtime.InteropServices;

namespace OMS.common.Utilities
{
    public sealed class DataProtection
    {
        /// <summary>
        /// use local machine or user to encrypt and decrypt the data
        /// </summary>
        public enum Store
        {
            Machine,
            User
        }

        private class Consts
        {
            // specify an entropy so other DPAPI applications can't see the data
            public readonly static byte[] EntropyData = ASCIIEncoding.ASCII.GetBytes("B0D125B7-967E-4f94-9305-A6F9AF56A19A");
            public readonly static byte[] KeyData = Encoding.ASCII.GetBytes("init key");
            public readonly static byte[] IVData = Encoding.ASCII.GetBytes("init vec");
        }

        private DataProtection() { }

        private static byte[] GetByteData(char paddingChar, string seed, int size)
        {
            if (seed == null) seed = "";
            if (seed.Length > size) seed = seed.Substring(0, size);
            return Encoding.ASCII.GetBytes(seed.PadRight(size, paddingChar));
        }

        public static string Encrypt(string plainText, string seed)
        {
            string cipherText = "";
            try
            {
                byte[] bPlainText = Encoding.ASCII.GetBytes(plainText);
                RijndaelManaged rijndael = new RijndaelManaged();
                byte[] key = GetByteData('X', seed, 32);
                byte[] iv = GetByteData('Y', seed, 16);
                ICryptoTransform transform = rijndael.CreateEncryptor(key, iv);
                MemoryStream ms = new MemoryStream();
                CryptoStream cs = new CryptoStream(ms, transform, CryptoStreamMode.Write);
                cs.Write(bPlainText, 0, bPlainText.Length);
                cs.FlushFinalBlock();

                cipherText = Convert.ToBase64String(ms.ToArray());
                ms.Close();
                cs.Close();
            }
            catch { }
            return cipherText;
        }

        public static string Decrypt(string cipherText, string seed)
        {
            string plainText = "";
            try
            {
                RijndaelManaged rijndael = new RijndaelManaged();
                byte[] key = GetByteData('X', seed, 32);
                byte[] iv = GetByteData('Y', seed, 16);
                ICryptoTransform transform = rijndael.CreateDecryptor(key, iv);
                byte[] bCipherText = Convert.FromBase64String(cipherText);
                MemoryStream ms = new MemoryStream(bCipherText);
                CryptoStream cs = new CryptoStream(ms, transform, CryptoStreamMode.Read);
                byte[] bPlainText = new byte[bCipherText.Length];
                cs.Read(bPlainText, 0, bPlainText.Length);
                plainText = Encoding.ASCII.GetString(bPlainText);
                plainText = plainText.Trim('\0');
            }
            catch { }
            return plainText;
        }
        /// <summary>
        /// encrypt the data using DPAPI, returns a base64-encoded encrypted string
        /// </summary>
        /// <param name="data"></param>
        /// <param name="store"></param>
        /// <returns></returns>
        public static string Encrypt(string data, Store store)
        {
            // holds the result string
            string result = "";

            // blobs used in the CryptProtectData call
            Crypt32.DATA_BLOB inBlob = new Crypt32.DATA_BLOB();
            Crypt32.DATA_BLOB entropyBlob = new Crypt32.DATA_BLOB();
            Crypt32.DATA_BLOB outBlob = new Crypt32.DATA_BLOB();

            try
            {
                // setup flags passed to the CryptProtectData call
                int flags = Crypt32.CRYPTPROTECT_UI_FORBIDDEN |
                    (int)((store == Store.Machine) ? Crypt32.CRYPTPROTECT_LOCAL_MACHINE : 0);

                // setup input blobs, the data to be encrypted and entropy blob
                SetBlobData(ref inBlob, ASCIIEncoding.ASCII.GetBytes(data));
                SetBlobData(ref entropyBlob, Consts.EntropyData);

                // call the DPAPI function, returns true if successful and fills in the outBlob
                if (Crypt32.CryptProtectData(ref inBlob, "", ref entropyBlob, IntPtr.Zero, IntPtr.Zero, flags, ref outBlob))
                {
                    byte[] resultBits = GetBlobData(ref outBlob);
                    if (resultBits != null)
                        result = Convert.ToBase64String(resultBits);
                }
            }
            catch (Exception ex)
            {
                TLog.DefaultInstance.WriteLog(ex.ToString(), LogType.ERROR);
            }
            finally
            {
                if (inBlob.pbData.ToInt32() != 0)
                    Marshal.FreeHGlobal(inBlob.pbData);

                if (entropyBlob.pbData.ToInt32() != 0)
                    Marshal.FreeHGlobal(entropyBlob.pbData);
            }

            return result;
        }
        /// <summary>
        /// decrypt the data using DPAPI, data is a base64-encoded encrypted string
        /// </summary>
        /// <param name="data"></param>
        /// <param name="store"></param>
        /// <returns></returns>
        public static string Decrypt(string data, Store store)
        {
            // holds the result string
            string result = "";

            // blobs used in the CryptUnprotectData call
            Crypt32.DATA_BLOB inBlob = new Crypt32.DATA_BLOB();
            Crypt32.DATA_BLOB entropyBlob = new Crypt32.DATA_BLOB();
            Crypt32.DATA_BLOB outBlob = new Crypt32.DATA_BLOB();

            try
            {
                // setup flags passed to the CryptUnprotectData call
                int flags = Crypt32.CRYPTPROTECT_UI_FORBIDDEN |
                    (int)((store == Store.Machine) ? Crypt32.CRYPTPROTECT_LOCAL_MACHINE : 0);

                // the CryptUnprotectData works with a byte array, convert string data
                byte[] bits = Convert.FromBase64String(data);

                // setup input blobs, the data to be decrypted and entropy blob
                SetBlobData(ref inBlob, bits);
                SetBlobData(ref entropyBlob, Consts.EntropyData);

                // call the DPAPI function, returns true if successful and fills in the outBlob
                if (Crypt32.CryptUnprotectData(ref inBlob, null, ref entropyBlob, IntPtr.Zero, IntPtr.Zero, flags, ref outBlob))
                {
                    byte[] resultBits = GetBlobData(ref outBlob);
                    if (resultBits != null)
                        result = ASCIIEncoding.ASCII.GetString(resultBits);
                }
            }
            catch (Exception ex)
            {
                TLog.DefaultInstance.WriteLog(ex.ToString(), LogType.ERROR);
            }
            finally
            {
                if (inBlob.pbData.ToInt32() != 0)
                    Marshal.FreeHGlobal(inBlob.pbData);

                if (entropyBlob.pbData.ToInt32() != 0)
                    Marshal.FreeHGlobal(entropyBlob.pbData);
            }

            return result;
        }
        /// <summary>
        ///helper method that fills in a DATA_BLOB, copies  
        ///data from managed to unmanaged memory
        /// </summary>
        /// <param name="blob"></param>
        /// <param name="bits"></param>
        private static void SetBlobData(ref Crypt32.DATA_BLOB blob, byte[] bits)
        {
            blob.cbData = bits.Length;
            blob.pbData = Marshal.AllocHGlobal(bits.Length);
            Marshal.Copy(bits, 0, blob.pbData, bits.Length);
        }
        /// <summary>
        ///helper method that gets data from a DATA_BLOB,
        ///copies data from unmanaged memory to managed
        /// </summary>
        /// <param name="blob"></param>
        /// <returns></returns>
        private static byte[] GetBlobData(ref Crypt32.DATA_BLOB blob)
        {
            // return an empty string if the blob is empty
            if (blob.pbData.ToInt32() == 0)
                return null;

            // copy information from the blob
            byte[] data = new byte[blob.cbData];
            Marshal.Copy(blob.pbData, data, 0, blob.cbData);
            Kernel32.LocalFree(blob.pbData);

            return data;
        }
    }
}
