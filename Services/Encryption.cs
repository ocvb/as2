using System.Security.Cryptography;
using System.Text;

namespace _234412H_AS2.Services
{
    public interface IEncryptionService
    {
        string EncryptData(string data);
        string DecryptData(string encryptedData);
    }

    public class EncryptionService : IEncryptionService
    {
        private readonly string _key = "b14ca5898a4e4133bbce2ea2315a1916";

        public string EncryptData(string data)
        {
            using Aes aes = Aes.Create();
            byte[] key = Encoding.UTF8.GetBytes(_key);
            aes.Key = key;
            aes.GenerateIV();

            ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

            using MemoryStream msEncrypt = new();
            msEncrypt.Write(aes.IV, 0, aes.IV.Length);

            using (CryptoStream csEncrypt = new(msEncrypt, encryptor, CryptoStreamMode.Write))
            using (StreamWriter swEncrypt = new(csEncrypt))
            {
                swEncrypt.Write(data);
            }

            return Convert.ToBase64String(msEncrypt.ToArray());
        }

        public string DecryptData(string encryptedData)
        {
            byte[] fullCipher = Convert.FromBase64String(encryptedData);

            using Aes aes = Aes.Create();
            byte[] key = Encoding.UTF8.GetBytes(_key);
            aes.Key = key;

            byte[] iv = new byte[aes.BlockSize / 8];
            Array.Copy(fullCipher, 0, iv, 0, iv.Length);
            aes.IV = iv;

            ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

            using (MemoryStream msDecrypt = new MemoryStream(fullCipher, iv.Length, fullCipher.Length - iv.Length))
            using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
            using (StreamReader srDecrypt = new StreamReader(csDecrypt))
            {
                return srDecrypt.ReadToEnd();
            }
        }
    }
}
