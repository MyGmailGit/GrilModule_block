using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Collections.Generic;
using UnityEngine;

namespace Utility
{

    public static class OpenSSLCryptoHelper
    {
        // AES-256-CBC 参数
        private const int KeySizeBytes = 32;      // AES-256 密钥 32字节
        private const int IvSizeBytes = 16;       // CBC IV 16字节
        private const int SaltSizeBytes = 8;      // OpenSSL 使用 8 字节 salt
        private const int Iterations = 10000;     // PBKDF2 迭代次数
        private static readonly byte[] MagicHeader = Encoding.ASCII.GetBytes("Salted__"); // OpenSSL 固定头

        /// <summary>
        /// 加密字节数组（OpenSSL salted 格式）
        /// </summary>
        /// <param name="plainData">要加密的原始数据</param>
        /// <param name="password">密码</param>
        /// <returns>加密后的数据（格式：Salted__ + salt(8) + 密文）</returns>
        public static byte[] EncryptBytes(byte[] plainData, string password)
        {
            if (plainData == null || plainData.Length == 0)
                throw new ArgumentException("plainData cannot be null or empty", nameof(plainData));

            if (string.IsNullOrEmpty(password))
                throw new ArgumentException("password cannot be null or empty", nameof(password));

            try
            {
                // 1. 生成随机 salt
                byte[] salt = GenerateRandomSalt();

                // 2. 从密码和 salt 派生 key 和 iv
                byte[] key = new byte[KeySizeBytes];
                byte[] iv = new byte[IvSizeBytes];
                DeriveKeyAndIv(password, salt, key, iv);

                // 3. AES 加密
                byte[] encryptedData;
                using (var aes = Aes.Create())
                {
                    aes.KeySize = 256;
                    aes.BlockSize = 128;
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;
                    aes.Key = key;
                    aes.IV = iv;

                    using (var encryptor = aes.CreateEncryptor())
                    {
                        encryptedData = encryptor.TransformFinalBlock(plainData, 0, plainData.Length);
                    }
                }

                // 4. 组装 OpenSSL 格式: "Salted__" (8字节) + salt (8字节) + 加密数据
                byte[] result = new byte[MagicHeader.Length + SaltSizeBytes + encryptedData.Length];
                Array.Copy(MagicHeader, 0, result, 0, MagicHeader.Length);
                Array.Copy(salt, 0, result, MagicHeader.Length, SaltSizeBytes);
                Array.Copy(encryptedData, 0, result, MagicHeader.Length + SaltSizeBytes, encryptedData.Length);

                return result;
            }
            catch (Exception e)
            {
                Debug.LogError($"EncryptBytes failed: {e.Message}");
                throw;
            }
        }

        /// <summary>
        /// 解密字节数组（OpenSSL salted 格式）
        /// </summary>
        /// <param name="encryptedData">加密的数据（格式：Salted__ + salt(8) + 密文）</param>
        /// <param name="password">密码</param>
        /// <returns>解密后的原始数据</returns>
        public static byte[] DecryptBytes(byte[] encryptedData, string password)
        {
            if (encryptedData == null || encryptedData.Length == 0)
                throw new ArgumentException("encryptedData cannot be null or empty", nameof(encryptedData));

            if (string.IsNullOrEmpty(password))
                throw new ArgumentException("password cannot be null or empty", nameof(password));

            // 1. 验证 OpenSSL 格式头
            if (encryptedData.Length < MagicHeader.Length + SaltSizeBytes)
                throw new Exception("Encrypted data too short, missing OpenSSL header");

            // 检查 "Salted__" 头
            for (int i = 0; i < MagicHeader.Length; i++)
            {
                if (encryptedData[i] != MagicHeader[i])
                    throw new Exception("Not OpenSSL salted AES format");
            }

            // 2. 提取 salt
            byte[] salt = new byte[SaltSizeBytes];
            Array.Copy(encryptedData, MagicHeader.Length, salt, 0, SaltSizeBytes);

            // 3. 提取密文
            int cipherOffset = MagicHeader.Length + SaltSizeBytes;
            int cipherLength = encryptedData.Length - cipherOffset;

            if (cipherLength <= 0)
                throw new Exception("No cipher data found");

            // 4. 从密码和 salt 派生 key 和 iv
            byte[] key = new byte[KeySizeBytes];
            byte[] iv = new byte[IvSizeBytes];
            DeriveKeyAndIv(password, salt, key, iv);

            // 5. AES 解密
            try
            {
                using (var aes = Aes.Create())
                {
                    aes.KeySize = 256;
                    aes.BlockSize = 128;
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;
                    aes.Key = key;
                    aes.IV = iv;

                    using (var decryptor = aes.CreateDecryptor())
                    {
                        return decryptor.TransformFinalBlock(encryptedData, cipherOffset, cipherLength);
                    }
                }
            }
            catch (CryptographicException e)
            {
                Debug.LogError($"Decrypt failed, possibly wrong password: {e.Message}");
                throw new Exception("Decryption failed, please check password", e);
            }
        }

        /// <summary>
        /// 加密字符串（返回 Base64）
        /// </summary>
        public static string EncryptString(string plainText, string password)
        {
            if (string.IsNullOrEmpty(plainText))
                throw new ArgumentException("plainText cannot be null or empty", nameof(plainText));

            byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
            byte[] encryptedData = EncryptBytes(plainBytes, password);
            return Convert.ToBase64String(encryptedData);
        }

        /// <summary>
        /// 解密字符串（从 Base64）
        /// </summary>
        public static string DecryptString(string encryptedBase64, string password)
        {
            if (string.IsNullOrEmpty(encryptedBase64))
                throw new ArgumentException("encryptedBase64 cannot be null or empty", nameof(encryptedBase64));

            byte[] encryptedData = Convert.FromBase64String(encryptedBase64);
            byte[] plainBytes = DecryptBytes(encryptedData, password);
            return Encoding.UTF8.GetString(plainBytes);
        }

        /// <summary>
        /// 加密文件
        /// </summary>
        /// <param name="inputFile">输入文件路径</param>
        /// <param name="outputFile">输出文件路径</param>
        /// <param name="password">密码</param>
        public static void EncryptFile(string inputFile, string outputFile, string password)
        {
            byte[] plainData = File.ReadAllBytes(inputFile);
            byte[] encryptedData = EncryptBytes(plainData, password);
            File.WriteAllBytes(outputFile, encryptedData);
        }

        /// <summary>
        /// 解密文件
        /// </summary>
        /// <param name="inputFile">输入文件路径</param>
        /// <param name="outputFile">输出文件路径</param>
        /// <param name="password">密码</param>
        public static void DecryptFile(string inputFile, string outputFile, string password)
        {
            byte[] encryptedData = File.ReadAllBytes(inputFile);
            byte[] plainData = DecryptBytes(encryptedData, password);
            File.WriteAllBytes(outputFile, plainData);
        }

        /// <summary>
        /// 流式加密大文件（避免内存溢出）
        /// </summary>
        public static void EncryptFileStream(string inputFile, string outputFile, string password)
        {
            if (!File.Exists(inputFile))
                throw new FileNotFoundException($"Input file not found: {inputFile}");

            byte[] salt = GenerateRandomSalt();
            byte[] key = new byte[KeySizeBytes];
            byte[] iv = new byte[IvSizeBytes];
            DeriveKeyAndIv(password, salt, key, iv);

            using (var inputStream = File.OpenRead(inputFile))
            using (var outputStream = File.Create(outputFile))
            {
                // 写入 OpenSSL 头
                outputStream.Write(MagicHeader, 0, MagicHeader.Length);
                outputStream.Write(salt, 0, salt.Length);

                // AES 加密流式写入
                using (var aes = Aes.Create())
                {
                    aes.KeySize = 256;
                    aes.BlockSize = 128;
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;
                    aes.Key = key;
                    aes.IV = iv;

                    using (var encryptor = aes.CreateEncryptor())
                    using (var cryptoStream = new CryptoStream(outputStream, encryptor, CryptoStreamMode.Write))
                    {
                        inputStream.CopyTo(cryptoStream);
                        cryptoStream.FlushFinalBlock();
                    }
                }
            }
        }

        /// <summary>
        /// 流式解密大文件（避免内存溢出）
        /// </summary>
        public static void DecryptFileStream(string inputFile, string outputFile, string password)
        {
            if (!File.Exists(inputFile))
                throw new FileNotFoundException($"Input file not found: {inputFile}");

            using (var inputStream = File.OpenRead(inputFile))
            {
                // 读取并验证 OpenSSL 头
                byte[] header = new byte[MagicHeader.Length];
                if (inputStream.Read(header, 0, header.Length) != header.Length)
                    throw new Exception("Invalid OpenSSL encrypted file");

                for (int i = 0; i < MagicHeader.Length; i++)
                {
                    if (header[i] != MagicHeader[i])
                        throw new Exception("Not OpenSSL salted AES format");
                }

                // 读取 salt
                byte[] salt = new byte[SaltSizeBytes];
                if (inputStream.Read(salt, 0, salt.Length) != salt.Length)
                    throw new Exception("Failed to read salt");

                // 派生 key 和 iv
                byte[] key = new byte[KeySizeBytes];
                byte[] iv = new byte[IvSizeBytes];
                DeriveKeyAndIv(password, salt, key, iv);

                // AES 解密流式写入
                using (var aes = Aes.Create())
                {
                    aes.KeySize = 256;
                    aes.BlockSize = 128;
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;
                    aes.Key = key;
                    aes.IV = iv;

                    using (var decryptor = aes.CreateDecryptor())
                    using (var cryptoStream = new CryptoStream(inputStream, decryptor, CryptoStreamMode.Read))
                    using (var outputStream = File.Create(outputFile))
                    {
                        cryptoStream.CopyTo(outputStream);
                    }
                }
            }
        }

        /// <summary>
        /// 派生 Key 和 IV（与 OpenSSL 算法一致）
        /// </summary>
        private static void DeriveKeyAndIv(string password, byte[] salt, byte[] outKey, byte[] outIv)
        {
            if (outKey.Length != KeySizeBytes)
                throw new ArgumentException($"outKey must be {KeySizeBytes} bytes", nameof(outKey));

            if (outIv.Length != IvSizeBytes)
                throw new ArgumentException($"outIv must be {IvSizeBytes} bytes", nameof(outIv));

            using (var pbkdf2 = new Rfc2898DeriveBytes(
                password,
                salt,
                Iterations,
                HashAlgorithmName.SHA256
            ))
            {
                byte[] keyIv = pbkdf2.GetBytes(KeySizeBytes + IvSizeBytes);
                Array.Copy(keyIv, 0, outKey, 0, KeySizeBytes);
                Array.Copy(keyIv, KeySizeBytes, outIv, 0, IvSizeBytes);
            }
        }

        /// <summary>
        /// 生成随机 Salt
        /// </summary>
        private static byte[] GenerateRandomSalt()
        {
            byte[] salt = new byte[SaltSizeBytes];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }
            return salt;
        }
    }
}
