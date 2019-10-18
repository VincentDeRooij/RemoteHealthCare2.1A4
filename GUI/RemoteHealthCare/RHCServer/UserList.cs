using Newtonsoft.Json;
using RHCCore.Networking.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RHCServer
{
    public class UserList
    {
        private static string USER_FILE = "ACCOUNTS.json";
        public List<RHCCore.Networking.Models.Person> users { get; set; }

        public UserList()
        {
            users = new List<Person>();
            if (!File.Exists(USER_FILE))
            {
                FileStream fs = File.Create(USER_FILE);
                dynamic userList = new
                {
                    Users = users
                };

                byte[] data = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(userList));
                fs.Write(data, 0, data.Length);
                fs.Close();

                AddUser("test", RHCCore.Security.Hashing.EncryptSHA256("test"), RHCCore.Security.Hashing.EncryptSHA256("test"), false);
                AddUser("DOCTOR", RHCCore.Security.Hashing.EncryptSHA256("Admin"), RHCCore.Security.Hashing.EncryptSHA256("Hallo"), true);
            }
        }

        public static void AddUser(string name, string username, string password, bool isDoctor = false)
        {
            UserList list = JsonConvert.DeserializeObject<UserList>(File.ReadAllText(USER_FILE));
            list.users.Add(new Person(name, username, password, isDoctor));
            File.WriteAllText(USER_FILE, JsonConvert.SerializeObject(list));
        }

        public static bool UserExists(string username, string password, bool isDoctor = false)
        {
            UserList list = JsonConvert.DeserializeObject<UserList>(File.ReadAllText(USER_FILE));
            return list.users.Where(x => x.Username == username && x.Password == password && x.IsDoctor == isDoctor).Count() > 0;
        }

        public static List<Person> GetPersons()
        {
            UserList list = JsonConvert.DeserializeObject<UserList>(File.ReadAllText(USER_FILE));
            return list.users;
        }

        public static Person GetUser(string username)
        {
            UserList list = JsonConvert.DeserializeObject<UserList>(File.ReadAllText(USER_FILE));
            return list.users.Where(x => x.Username == username).FirstOrDefault();
        }
    }
}
