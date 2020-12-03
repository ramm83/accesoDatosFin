using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace AccesoDatos.Negocio.Crypto
{
    public static class Crypto
    {
        private const string SecretKey = "$G1Int3rnet*.**";

        public static string OpenSSLEncrypt(string plainText)
        {
            byte[] numArray = new byte[8];
            new RNGCryptoServiceProvider().GetNonZeroBytes(numArray);
            byte[] key;
            byte[] iv;
            DeriveKeyAndIV(numArray, out key, out iv);
            byte[] bytesAes = EncryptStringToBytesAes(plainText, key, iv);
            byte[] inArray = new byte[numArray.Length + bytesAes.Length + 8];
            Buffer.BlockCopy(Encoding.ASCII.GetBytes("Salted__"), 0, inArray, 0, 8);
            Buffer.BlockCopy(numArray, 0, inArray, 8, numArray.Length);
            Buffer.BlockCopy(bytesAes, 0, inArray, numArray.Length + 8, bytesAes.Length);
            return Convert.ToBase64String(inArray);
        }

        public static string OpenSSLDecrypt(string encrypted)
        {
            byte[] numArray = Convert.FromBase64String(encrypted);
            byte[] salt = new byte[8];
            byte[] cipherText = new byte[numArray.Length - salt.Length - 8];
            Buffer.BlockCopy(numArray, 8, salt, 0, salt.Length);
            Buffer.BlockCopy(numArray, salt.Length + 8, cipherText, 0, cipherText.Length);
            byte[] key;
            byte[] iv;
            DeriveKeyAndIV(salt, out key, out iv);
            return DecryptStringFromBytesAes(cipherText, key, iv);
        }

        private static void DeriveKeyAndIV(byte[] salt, out byte[] key, out byte[] iv)
        {
            List<byte> byteList = new List<byte>(48);
            byte[] bytes = Encoding.UTF8.GetBytes("$G1Int3rnet*.**");
            byte[] numArray = new byte[0];
            MD5 md5 = MD5.Create();
            bool flag = false;
            while (!flag)
            {
                byte[] buffer = new byte[numArray.Length + bytes.Length + salt.Length];
                Buffer.BlockCopy(numArray, 0, buffer, 0, numArray.Length);
                Buffer.BlockCopy(bytes, 0, buffer, numArray.Length, bytes.Length);
                Buffer.BlockCopy(salt, 0, buffer, numArray.Length + bytes.Length, salt.Length);
                numArray = md5.ComputeHash(buffer);
                byteList.AddRange(numArray);
                if (byteList.Count >= 48)
                    flag = true;
            }
            key = new byte[32];
            iv = new byte[16];
            byteList.CopyTo(0, key, 0, 32);
            byteList.CopyTo(32, iv, 0, 16);
            md5.Clear();
        }

        private static byte[] EncryptStringToBytesAes(string plainText, byte[] key, byte[] iv)
        {
            if (plainText == null || plainText.Length <= 0)
                throw new ArgumentNullException(nameof(plainText));
            if (key == null || key.Length == 0)
                throw new ArgumentNullException(nameof(key));
            if (iv == null || iv.Length == 0)
                throw new ArgumentNullException(nameof(iv));
            RijndaelManaged rijndaelManaged1 = (RijndaelManaged)null;
            MemoryStream memoryStream;
            try
            {
                RijndaelManaged rijndaelManaged2 = new RijndaelManaged();
                rijndaelManaged2.Mode = CipherMode.CBC;
                rijndaelManaged2.KeySize = 256;
                rijndaelManaged2.BlockSize = 128;
                rijndaelManaged2.Key = key;
                rijndaelManaged2.IV = iv;
                rijndaelManaged1 = rijndaelManaged2;
                ICryptoTransform encryptor = rijndaelManaged1.CreateEncryptor(rijndaelManaged1.Key, rijndaelManaged1.IV);
                memoryStream = new MemoryStream();
                using (CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                {
                    using (StreamWriter streamWriter = new StreamWriter(cryptoStream))
                    {
                        streamWriter.Write(plainText);
                        streamWriter.Flush();
                        streamWriter.Close();
                    }
                }
            }
            finally
            {
                rijndaelManaged1?.Clear();
            }
            return memoryStream.ToArray();
        }

        private static string DecryptStringFromBytesAes(byte[] cipherText, byte[] key, byte[] iv)
        {
            if (cipherText == null || cipherText.Length == 0)
                throw new ArgumentNullException(nameof(cipherText));
            if (key == null || key.Length == 0)
                throw new ArgumentNullException(nameof(key));
            if (iv == null || iv.Length == 0)
                throw new ArgumentNullException(nameof(iv));
            RijndaelManaged rijndaelManaged1 = (RijndaelManaged)null;
            string end;
            try
            {
                RijndaelManaged rijndaelManaged2 = new RijndaelManaged();
                rijndaelManaged2.Mode = CipherMode.CBC;
                rijndaelManaged2.KeySize = 256;
                rijndaelManaged2.BlockSize = 128;
                rijndaelManaged2.Key = key;
                rijndaelManaged2.IV = iv;
                rijndaelManaged1 = rijndaelManaged2;
                ICryptoTransform decryptor = rijndaelManaged1.CreateDecryptor(rijndaelManaged1.Key, rijndaelManaged1.IV);
                using (MemoryStream memoryStream = new MemoryStream(cipherText))
                {
                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader streamReader = new StreamReader(cryptoStream))
                        {
                            end = streamReader.ReadToEnd();
                            streamReader.Close();
                        }
                    }
                }
            }
            finally
            {
                rijndaelManaged1?.Clear();
            }
            return end;
        }
    }
}
