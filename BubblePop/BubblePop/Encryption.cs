using System.Text;
using System.Security.Cryptography;
using System.IO;
using System.Xml;
using System;


public class Encryption64
{
    //private byte[] key = {};
    //private byte[] IV = {10, 20, 30, 40, 50, 60, 70, 80}; // it can be any byte value

    public static string Decrypt(string stringToDecrypt, string sEncryptionKey)
    {

        byte[] key = { };
        byte[] IV = { 10, 20, 30, 40, 50, 60, 70, 80, 10, 20, 30, 40, 50, 60, 70, 80 };
        byte[] inputByteArray = new byte[stringToDecrypt.Length];

        try
        {
            key = Encoding.UTF8.GetBytes(sEncryptionKey.Substring(0, 16));
            AesManaged aes = new AesManaged();
            aes.KeySize = 128;
            //DESCryptoServiceProvider des = new DESCryptoServiceProvider();
            inputByteArray = Convert.FromBase64String(stringToDecrypt);

            MemoryStream ms = new MemoryStream();
            CryptoStream cs = new CryptoStream(ms, aes.CreateDecryptor(key, IV), CryptoStreamMode.Write);
            cs.Write(inputByteArray, 0, inputByteArray.Length);
            cs.FlushFinalBlock();

            Encoding encoding = Encoding.UTF8;
            return encoding.GetString(ms.ToArray(), 0, (int)ms.Length);
        }
        catch (System.Exception ex)
        {
            throw ex;
        }
    }

    public static string Encrypt(string stringToEncrypt, string sEncryptionKey)
    {
        byte[] key = { };
        byte[] IV = { 10, 20, 30, 40, 50, 60, 70, 80, 10, 20, 30, 40, 50, 60, 70, 80 };
        byte[] inputByteArray; //Convert.ToByte(stringToEncrypt.Length)

        try
        {
            key = Encoding.UTF8.GetBytes(sEncryptionKey.Substring(0, 16));
            AesManaged aes = new AesManaged();
            aes.KeySize = 128;
            //DESCryptoServiceProvider des = new DESCryptoServiceProvider();
            inputByteArray = Encoding.UTF8.GetBytes(stringToEncrypt);
            MemoryStream ms = new MemoryStream();
            CryptoStream cs = new CryptoStream(ms, aes.CreateEncryptor(key, IV), CryptoStreamMode.Write);
            cs.Write(inputByteArray, 0, inputByteArray.Length);
            cs.FlushFinalBlock();

            return Convert.ToBase64String(ms.ToArray());
        }
        catch (System.Exception ex)
        {
            throw ex;
        }
    }

}