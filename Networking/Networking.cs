using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using Serilog;

namespace GLMS.Networking
{
    public static class NetworkHandler
    {
        public static bool messageReceived = false;
        public static bool waiting = false;
        public static bool ready = false;
        private static bool isFirestarting = false;
        private static Thread fireStarter;

        //dont know if this is good practise have fun with all the fires
        //thread that starts the listener only if its not waiting for a connection
        private static void FireStarterThreadProc()
        {
            TcpListener server = new TcpListener(IPAddress.Any, 6777);
            server.Start();
            int i;
            byte[] bytes;
            string data;

            while (isFirestarting)
            {
                if (server.Pending())
                {
                    TcpClient client = server.AcceptTcpClient();
                    NetworkStream stream = client.GetStream();
                    bytes = new byte[4098];
                    data = String.Empty;
                    while ((i = stream.Read(bytes, 0, bytes.Length)) != 0) {
                        data = Encoding.Unicode.GetString(bytes,0,i);
                    }

                    Log.Debug("Received: {0}",data);
                    string[] ipPortPair = client.Client.RemoteEndPoint.ToString().Split(':');
                    ProcessData(data, ipPortPair[0]); //may be null how?
                    client.Close();
                }
                Thread.Sleep(50);
            }

            server.Stop();
        }

        //starts the thread
        public static void StartFirestarter()
        {
            isFirestarting = true;
            fireStarter = new Thread(new ThreadStart(FireStarterThreadProc));
            fireStarter.Start();
        }

        //stops thread
        public static void StopFirestarter()
        {
            isFirestarting = false;
        }


        //called by thread to process incoming data
        public static void ProcessData(string receiveString, string ip)
        { 
            Message msg;
            //tries to get message, fails if wrongly formatted or corrupted
            try
            {

                msg = MessageSerializer.GetClassFromString(receiveString);
            } catch (Exception ex)
            {
                if(!UserList.ContainsIPAddress(ip))
                {
                    Log.Error(ex, $"[Networking] Error deserializing received message.");
                    return;
                }
                Log.Error($"[Networking] Error receiving message from known source, start a session or try again.");
                Log.Debug(ex,"Stack trace:");
                return;
            }

            //if handshake, it processes the handshake then replys if it has to
            if(msg.IsHandshake)
            {
                EncryptionManager.AddPublicKey(msg.Username, EncryptionManager.GetKeyFromString(msg.Text));
                UserList.AddUser(msg.Username, ip);
                Log.Information("[Networking] Received handshake.");
                if (msg.toReply)
                {
                    Log.Information($"[Networking] Replying to {ip} ...");
                    NetworkHandler.SendHandshake(ip, false);
                }

                return;
            }

            //if session key, saves it
            if(msg.IsSessionStarter)
            {
                string decryptedKey = EncryptionManager.DecryptData(msg.Text);
                Aes key = EncryptionManager.SessionManager.ResolveStringAsAes(decryptedKey);
                EncryptionManager.SessionManager.AddSession(msg.Username,key);
                Log.Information($"[Networking] Session started by {msg.Username}");
                return;
            }

            //prints and decrypts message using session key
            Log.Information($"[Message] {msg.Username}: {EncryptionManager.DecryptDataAes(msg.Username,msg.Text)}");
        }

        public static TcpClient? OpenSender(IPAddress ip)
        {
            IPEndPoint endPoint = new IPEndPoint(ip, 6777);
            TcpClient client = new TcpClient();
            var result = client.BeginConnect(endPoint.Address, endPoint.Port, null, null);
            var success = result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(2.5d));

            if (!success)
            {
                return null;
            }

            try
            {
                client.EndConnect(result);
            } catch
            {
                return null;
            }

            return client;
        }

        //sends and encrypts data
        public static void SendMessage(string user, string text)
        {
            IPAddress parsedIp = UserList.GetIPAddressByUser(user);
            if(parsedIp == IPAddress.None) { return; }

            TcpClient? client = OpenSender(parsedIp);
            if(client == null)
            {
                Log.Error("[Networking] Send timed out.");
                return;
            }

            string? encryptedText = EncryptionManager.EncryptDataAes(user, text);
            if(encryptedText == null)
            {
                Log.Error("[Networking] Unable to encrypt text for specified user. Consider using the handshake command and starting a session.");
                client.Close();
                return;
            }

            Message msg = new Message
            {
                IsHandshake = false,
                Username = SettingsManager.GetSettings().username,
                toReply = false,
                Text = encryptedText
            };
            try
            {
                client.GetStream().Write(Encoding.Unicode.GetBytes(MessageSerializer.GetStringFromClass(msg)));
                Thread.Sleep(250);
                Log.Debug("Sent: {0}", MessageSerializer.GetStringFromClass(msg));
                Log.Information("[Networking] Message sent.");
            } catch(Exception ex)
            {
                Log.Error(ex,$"[Networking] Failed to send message, stack trace:");
                client.Close();
                return;
            }

            client.Close();
        }

        //sends a handshake to an ip
        // handshaking exchanges public keys
        public static void SendHandshake(string ip, bool reply)
        {
            IPAddress parsedIp;
            try
            {
                parsedIp = IPAddress.Parse(ip);
            }
            catch
            {
                Log.Error("[Networking] Invalid IP inserted");
                return;
            }

            TcpClient? client = OpenSender(parsedIp);
            if (client == null)
            {
                Log.Error("[Networking] Handshake timed out.");
                return;
            }

            Message hs = new Message
            {
                IsHandshake = true,
                Username = SettingsManager.GetSettings().username,
                toReply = reply,
                Text = EncryptionManager.GetKeyStringRepresentation(false)
            };
            try
            {
                client.GetStream().Write(Encoding.Unicode.GetBytes(MessageSerializer.GetStringFromClass(hs)));
                Thread.Sleep(1000);
                Log.Debug("Sent: {0}", MessageSerializer.GetStringFromClass(hs));
                Log.Information($"[Networking] Sent handshake to {ip}");
            }
            catch (Exception ex)
            {
                Log.Error(ex,$"[Networking] Failed to send message, stack trace:");
                client.Close();
                return;
            }

            client.Close();
        }

        public static void SendSessionStart(string user)
        {
            IPAddress parsedIp = UserList.GetIPAddressByUser(user);
            if(parsedIp == IPAddress.None)
            {
                return;
            }

            TcpClient? client = OpenSender(parsedIp);
            if (client == null)
            {
                Log.Error("[Networking] Timed out.");
                return;
            }

            string aesKey = EncryptionManager.SessionManager.GetStringRepresentation();
            string? encryptedKey = EncryptionManager.EncryptDataByUser(user, aesKey);
            if (encryptedKey == null)
            {
                Log.Warning("[Networking] Unable to encrypt key for specified user. Consider using the handshake command and then starting a session.");
                client.Close();
                return;
            }

            Message hs = new Message
            {
                IsSessionStarter = true,
                Username = SettingsManager.GetSettings().username,
                Text = encryptedKey
            };

            try
            {
                client.GetStream().Write(Encoding.Unicode.GetBytes(MessageSerializer.GetStringFromClass(hs)));
                Thread.Sleep(1000);
                Log.Debug("Sent: {0}", MessageSerializer.GetStringFromClass(hs));
                Log.Information($"[Networking] Sent session to {parsedIp.ToString()}");
            }
            catch (Exception ex)
            {
                Log.Error(ex,$"[Networking] Failed to send message, stack trace:");
                client.Close();
                return;
            }

            Aes aes = EncryptionManager.SessionManager.ResolveStringAsAes(aesKey);
            EncryptionManager.SessionManager.AddSession(user, aes);
            client.Close();
        }
    }

    public static class UserList
    {
        private static Dictionary<string, string> userIPList = new Dictionary<string, string>();
        private static List<string> currentUserListFile = new List<string>();

        public static void AddUser(string user, string ip)
        {
            if(userIPList.ContainsKey(user))
            {
                RemoveUser(user); 
            }
            if(userIPList.ContainsValue(ip))
            {
                Log.Information($"[UserList] IP:{ip} already exists.");
                return;
            }

            currentUserListFile.Add(user + '@' + ip);
            userIPList.Add(user, ip);
            Log.Information($"[UserList] Successfully added {user}");
        }

        public static bool ContainsIPAddress(string ip)
        {
            return userIPList.ContainsValue(ip);
        } 

        public static void RemoveUser(string user)
        {
            if(userIPList.Remove(user))
            {
                Log.Information($"[UserList] Successfully removed {user}");

                List<string> newList = new();
                foreach (string s in currentUserListFile)
                {
                    if (!s.Contains(user))
                    {
                        newList.Add(s);
                    }
                }

                currentUserListFile = newList;

                return;
            }

            Log.Information($"[UserList] {user} isn't registered.");
        }

        public static void EditUser(string user, string newIp)
        {
            RemoveUser(user);
            AddUser(user, newIp);
        }

        public static string? GetIPByUser(string user)
        {
            string? ip = null;
            if(!userIPList.TryGetValue(user, out ip))
            {
                Log.Information($"[UserList] {user} isn't registered.");
            }

            return ip;
        }

        public static IPAddress GetIPAddressByUser(string user)
        {
            string? ip = null;
            if (!userIPList.TryGetValue(user, out ip))
            {
                Log.Information($"[UserList] {user} isn't registered.");
                return IPAddress.None;
            }

            IPAddress parsed;
            try
            {
                parsed = IPAddress.Parse(ip);
            } catch
            {
                return IPAddress.None;
            }

            return parsed;
        }

        public static List<string> GetUserListAsList()
        {
            return currentUserListFile;
        }

        public static void ResetUserList()
        {
            currentUserListFile = new List<string>();
            userIPList.Clear();
            Log.Information("Reset userlist.");
        }
        public static void SaveToFile()
        {
            try
            {
                File.WriteAllLines("settings/user-ips.dat", currentUserListFile.ToArray());
            }
            catch (Exception ex2)
            {
                Log.Error(ex2,$"Unable to save user IPs to file.");
                return;
            }

            Log.Information($"Saved all user IPs to file.");
        }
    }

}
