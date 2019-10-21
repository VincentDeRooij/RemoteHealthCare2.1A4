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
    public class AccountStorage
    {
        private List<Person> accountList;
        public List<Person> AccountList => accountList;

        private const string FILE_NAME = "ACCOUNTS.json";

        private object fileLock;

        private AccountStorage()
        {
            fileLock = new object();
            lock (fileLock)
            {
                if (!File.Exists(FILE_NAME))
                {
                    accountList = new List<Person>();
                    accountList.Add(new Person("henk", RHCCore.Security.Hashing.EncryptSHA256("henk"), RHCCore.Security.Hashing.EncryptSHA256("henk"), false));
                    accountList.Add(new Person("test", RHCCore.Security.Hashing.EncryptSHA256("test"), RHCCore.Security.Hashing.EncryptSHA256("test"), true));

                    using (FileStream fs = File.Create(FILE_NAME))
                    {
                        byte[] buffer = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(accountList));
                        fs.Write(buffer, 0, buffer.Length);
                    }
                }
                else
                    SyncFromFile();
            }
        }

        private static AccountStorage _instance;
        public static AccountStorage Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new AccountStorage();

                return _instance;
            }
        }

        public void SyncToFile()
        {
            lock (fileLock)
            {
                File.WriteAllText(FILE_NAME, JsonConvert.SerializeObject(accountList));
            }
        }

        private void SyncFromFile()
        {
            lock (fileLock)
            {
                accountList = JsonConvert.DeserializeObject<List<Person>>(File.ReadAllText(FILE_NAME));
            }
        }

        public bool AddPerson(Person person)
        {
            if (!accountList.Any(x => x.Username == person.Username))
            {
                accountList.Add(person);
                return true;
            }
            return false;
        }

        public Person GetPerson(string username, bool isDoctor = false)
        {
            if (accountList.Any(x => x.Username == username && x.IsDoctor == isDoctor))
            {
                return accountList.Where(x => x.Username == username && x.IsDoctor == isDoctor).First();
            }
            return null;
        }

        public bool UserExists(string username, string password, bool isDoctor = false)
        {
            return accountList.Any(x => x.Username == username && x.Password == password && x.IsDoctor == !(isDoctor == false));
        }
    }
}
