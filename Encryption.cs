using Serilog;
using System;
using System.Security.Cryptography;
using System.Text;

namespace GLMS
{
    public static class EncryptionManager
    {
        private static RSAParameters privateKey;
        private static RSAParameters personalPublicKey;

        private static Dictionary<string, RSAParameters> publicKeys = new Dictionary<string, RSAParameters>();
        private static List<string> currentPublicKeysFile = new List<string>();
        public static void AddPublicKey(string user, RSAParameters key)
        {
            if (publicKeys.ContainsKey(user))
            {
                return;
            }

            currentPublicKeysFile.Add(user + '@' + GetStringFromKey(key));
            publicKeys.Add(user, key);
            Log.Information($"[EncryptionManager] Successfully added {user}");
        }

        public static List<string> GetAllPublicKeys()
        {
            return currentPublicKeysFile;
        }

        public static void ResetPublicKeys()
        {
            currentPublicKeysFile = new List<string>();
            publicKeys.Clear();
            Log.Information("[EncryptionManager] Public keys were reset.");
        }

        public static void RemovePublicKey(string user)
        {
            bool exists = publicKeys.Remove(user);
            if (!exists)
            {
                return;
            }

            List<string> newList = new();
            foreach (string s in currentPublicKeysFile)
            {
                if (!s.Contains(user))
                {
                    newList.Add(s);
                }
            }

            currentPublicKeysFile = newList;
            Log.Information($"[EncryptionManager] {user} was removed from list.");
        }

        public static void WritePublicKeysToFile()
        {
            try
            {
                File.WriteAllLines("keys/public-keys.glks", currentPublicKeysFile.ToArray());
            }
            catch (Exception ex2)
            {
                // TODO log exception
                Log.Error(ex2,$"Unable to save public keys to file.");
                return;
            }

            Log.Information($"Saved all public keys to file.");
        }

        public static void GeneratePersonalKeys()
        {
            var csp = new RSACryptoServiceProvider(2048);
            privateKey = csp.ExportParameters(true);
            personalPublicKey = csp.ExportParameters(false);
        }

        public static void SetPublicKey(string key)
        {
            personalPublicKey = GetKeyFromString(key);
        }

        public static void SetPrivateKey(string key)
        {
            privateKey = GetKeyFromString(key);
        }

        public static string GetKeyStringRepresentation(bool includePrivateParams)
        {
            RSAParameters key;

            if (includePrivateParams)
            {
                key = privateKey;
            }
            else
            {
                key = personalPublicKey;
            }

            string keyString;
            {
                var sw = new StringWriter();
                var xs = new System.Xml.Serialization.XmlSerializer(typeof(RSAParameters));
                xs.Serialize(sw, key);
                keyString = sw.ToString();
            }

            return keyString;
        }

        public static string GetStringFromKey(RSAParameters key)
        {

            string keyString;
            {
                var sw = new StringWriter();
                var xs = new System.Xml.Serialization.XmlSerializer(typeof(RSAParameters));
                xs.Serialize(sw, key);
                keyString = sw.ToString();
            }

            return keyString;
        }

        public static RSAParameters GetKeyFromString(string keyString)
        {
            var sr = new StringReader(keyString);
            var xs = new System.Xml.Serialization.XmlSerializer(typeof(RSAParameters));

            object? v = xs.Deserialize(sr);
            if(v is RSAParameters vv)
            {
                return vv;
            }

            throw new Exception("Deserialize/Cast failed");
        }

        public static string? EncryptDataByUser(string user, string data)
        {
            if (!publicKeys.TryGetValue(user, out var key)) { return null; }

            var csp = new RSACryptoServiceProvider();
            csp.ImportParameters(key);

            byte[] bytesData = System.Text.Encoding.ASCII.GetBytes(data);
            byte[] bytesEncryptedText = csp.Encrypt(bytesData, false);
            return Convert.ToBase64String(bytesEncryptedText);
        }

        public static string DecryptData(string data)
        {
            byte[] bytesEncryptedText = Convert.FromBase64String(data);
            var csp = new RSACryptoServiceProvider();
            csp.ImportParameters(privateKey);

            byte[] bytesData = csp.Decrypt(bytesEncryptedText, false);

            return Encoding.ASCII.GetString(bytesData);
        }

        public static string? EncryptDataAes(string user, string data)
        {
            Aes? key = SessionManager.GetSessionKey(user);
            byte[] encrypted = new byte[256];
            if(key == null)
            {
                return null;
            }

            ICryptoTransform encryptor = key.CreateEncryptor(key.Key, key.IV);

            using (MemoryStream msEncrypt = new MemoryStream())
            {
                using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                {
                    using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                    {
                        swEncrypt.Write(data);
                    }
                    encrypted = msEncrypt.ToArray();
                }
            }

            return Encoding.Unicode.GetString(encrypted);
        }

        public static string? DecryptDataAes(string user, string data)
        {
            Aes? key = SessionManager.GetSessionKey(user);
            if (key == null)
            {
                return null;
            }

            byte[] cipherText = Encoding.Unicode.GetBytes(data);
            string plainText;
            ICryptoTransform decryptor = key.CreateDecryptor(key.Key, key.IV);
            using (MemoryStream msDecrypt = new MemoryStream(cipherText))
            {
                using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                {
                    using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                    {
                        plainText = srDecrypt.ReadToEnd();
                    }
                }
            }

            return plainText;
        }

        public static class SessionManager
        {
            private static Dictionary<string, Aes> sessions = new Dictionary<string, Aes>();
            
            public static void AddSession(string user, Aes s)
            {
                //overwrite
                if(sessions.ContainsKey(user))
                {
                    sessions.Remove(user);
                    sessions.Add(user, s);
                }

                sessions.Add(user, s);
            }

            public static Aes AddSession(string user)
            {
                Aes aes = Aes.Create();

                if (sessions.ContainsKey(user))
                {
                    sessions.Remove(user);
                    sessions.Add(user, aes);
                }

                sessions.Add(user, aes);
                return aes;
            }

            public static Aes? GetSessionKey(string user)
            {
                Aes? key = null;
                sessions.TryGetValue(user, out key);
                return key;
            }

            public static string GetStringRepresentation(Aes key)
            {
                string base64key = Convert.ToBase64String(key.Key);
                string base64iv = Convert.ToBase64String(key.IV);

                return base64key + "||" + base64iv;
            }

            public static string GetStringRepresentation()
            {
                Aes key = Aes.Create();

                string base64key = Convert.ToBase64String(key.Key);
                string base64iv = Convert.ToBase64String(key.IV);

                return base64key + "||" + base64iv;
            }

            public static Aes ResolveStringAsAes(string keyString)
            {
                string[] keyIvPair = keyString.Split("||"); ;
                byte[] key = Convert.FromBase64String(keyIvPair[0]);
                byte[] iv = Convert.FromBase64String(keyIvPair[1]);

                Aes aes = Aes.Create();
                aes.Key = key;
                aes.IV = iv;

                return aes;
            }
        }
    }
}
