using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ATTSystems.SFA.DAL
{
    public class EncryptionDecryptionSHA256
    {
        public EncryptionDecryptionSHA256() { }

        public static string _key = "dnbGZmMKlGtsHqz8nSac2UCBnPhquFxJZWx41KecyT4=";
        public static string _iv = "D2Ktk/1AwZ9UJ6BuMrnaAA==";
        // Encrypt using SHA-256
        public static string Encrypt(string data)
        {
            using (Aes aesAlg = Aes.Create())
            {
                byte[] key = Convert.FromBase64String(_key);
                byte[] iv = Convert.FromBase64String(_iv);

                aesAlg.Key = SHA256.Create().ComputeHash(key);
                aesAlg.IV = iv;

                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(data);
                        }
                    }
                    return Convert.ToBase64String(msEncrypt.ToArray());
                }
            }
        }

        // Decrypt using SHA-256
        public static string Decrypt(string encData)
        {
            using (Aes aesAlg = Aes.Create())
            {
                byte[] key = Convert.FromBase64String(_key);
                byte[] iv = Convert.FromBase64String(_iv);

                aesAlg.Key = SHA256.Create().ComputeHash(key);
                aesAlg.IV = iv;

                byte[] encryptedData = Convert.FromBase64String(encData);

                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msDecrypt = new MemoryStream(encryptedData))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {
                            return srDecrypt.ReadToEnd();
                        }
                    }
                }
            }
        }

        // Helper method to generate random bytes for key and IV
        public byte[] GenerateRandomBytes(int length)
        {
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                byte[] bytes = new byte[length];
                rng.GetBytes(bytes);
                return bytes;
            }
        }
    }
}
