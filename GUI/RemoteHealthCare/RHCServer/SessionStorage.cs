using Newtonsoft.Json;
using RHCCore.Networking.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RHCServer
{
    public class SessionStorage
    {
        private static SessionStorage _instance;
        private static object writeLock = new object();
        public static SessionStorage Instance { get { if (_instance == null) _instance = new SessionStorage(); return _instance; } }

        private SessionStorage()
        {
            if (!Directory.Exists("Sessions"))
                Directory.CreateDirectory("Sessions");
        }

        public Session RetrieveSession(string sessionId)
        {
            lock (writeLock)
            {
                if (!SessionExists(sessionId))
                    return null;

                return JsonConvert.DeserializeObject<Session>(File.ReadAllText($"Sessions/{sessionId}.data"));
            }
        }

        public AstrandSession RetrieveAstrandSession(string sessionId)
        {
            lock (writeLock)
            {
                if (!SessionExists(sessionId))
                    return null;

                return JsonConvert.DeserializeObject<AstrandSession>(File.ReadAllText($"Sessions/{sessionId}.data"));
            }
        }

        public bool CreateSession(Session session)
        {
            lock (writeLock)
            {
                try
                {
                    using (FileStream fs = new FileStream($"Sessions/{session.SessionId}.data", FileMode.Create, FileAccess.ReadWrite))
                    {
                        byte[] sessionBuffer = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(session));
                        fs.Write(sessionBuffer, 0, sessionBuffer.Length);
                    }
                    return true;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    return false;
                }
            }
        }

        public bool SyncSession(Session session)
        {
            lock (writeLock)
            {
                try
                {
                    using (FileStream fs = new FileStream($"Sessions/{session.SessionId}.data", FileMode.Truncate, FileAccess.ReadWrite))
                    {
                        string jsonObject = JsonConvert.SerializeObject(session);
                        byte[] sessionBuffer = Encoding.ASCII.GetBytes(jsonObject);
                        fs.Write(sessionBuffer, 0, sessionBuffer.Length);
                    }
                    return true;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    return false;
                }
            }
        }

        public bool SessionExists(string sessionId)
        {
            return File.Exists(Path.Combine("Sessions", sessionId + ".data"));
        }
    }
}
