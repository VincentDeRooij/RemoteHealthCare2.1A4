using System;
using System.Collections.Generic;
using System.Text;

namespace RHCCore.Networking.Models
{
    public class Person
    {
        public string Name { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public bool IsDoctor { get; set; }
        public List<string> Sessions { get; set; }
        public List<ChatMessage> Messages { get; set; }

        public Person(string Name, string Username, string Password, bool IsDoctor = false)
        {
            this.Name = Name;
            this.Username = Username;
            this.Password = Password;
            this.IsDoctor = IsDoctor;
            Sessions = new List<string>();
            Messages = new List<ChatMessage>();
        }
    }

    public class ChatMessage
    {
        public string Message { get; set; }
        public bool IsDoctor { get; set; }

        public ChatMessage(string message, bool isDoctor)
        {
            this.Message = message;
            this.IsDoctor = isDoctor;
        }
    }

    public class Session
    {
        public List<dynamic> BikeData { get; set; }
        public DateTime StartDate { get; set; }
        public int SessionDuration { get; set; }
        public string SessionId { get; set; }
        public bool IsArchived { get; set; }
        public string Name { get; set; }

        public Session(string name, DateTime startDate, int duration)
        {
            BikeData = new List<dynamic>();
            this.StartDate = startDate;
            this.SessionDuration = duration;
            IsArchived = false;
            this.SessionId = Guid.NewGuid().ToString();
            this.Name = name;
        }
    }
}
