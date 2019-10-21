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
        public static SessionStorage Instance { get { if (_instance == null) _instance = new SessionStorage(); return _instance; } }

        private SessionStorage()
        {
            if (!Directory.Exists("Sessions"))
                Directory.CreateDirectory("Sessions");
        }

        public Session RetrieveSession(string sessionId)
        {
            if (!SessionExists(sessionId))
                return null;

            return JsonConvert.DeserializeObject<Session>(File.ReadAllText($"Sessions/{sessionId}.data"));
        }

        public bool CreateSession(Session session)
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

        public bool SyncSession(Session session)
        {
            try
            {
                using (FileStream fs = new FileStream($"Sessions/{session.SessionId}.data", FileMode.Truncate, FileAccess.ReadWrite))
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

        public bool SessionExists(string sessionId)
        {
            return File.Exists(Path.Combine("Sessions", sessionId + ".data"));
        }
    }
}
