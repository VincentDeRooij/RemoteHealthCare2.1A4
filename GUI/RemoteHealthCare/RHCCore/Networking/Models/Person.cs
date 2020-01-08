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

    public class AstrandSession : Session
    {
        public int Age { get; set; }
        public bool IsMale { get; set; }
        public bool ReachedSteady { get; set; }

        public double Weight { get; set; }

        public AstrandSession(string name, double weight, DateTime startDate, int age, bool isMale = true)
            : base(name, startDate, 420)
        {
            this.Age = age;
            this.IsMale = isMale;
            this.ReachedSteady = false;
            this.Weight = weight;
        }

        public int GetHeartrate()
        {
            Dictionary<Range, int> table = new Dictionary<Range, int>();
            table.Add(new Range(15, 24), 210);
            table.Add(new Range(25, 34), 200);
            table.Add(new Range(35, 39), 190);
            table.Add(new Range(40, 44), 180);
            table.Add(new Range(45, 49), 170);
            table.Add(new Range(50, 54), 160);
            table.Add(new Range(55, int.MaxValue), 150);

            int selectedHr = 210;
            foreach (var item in table)
            {
                if (item.Key.IsInRange(this.Age))
                {
                    selectedHr = item.Value;
                    break;
                }
            }
            return selectedHr;
        }

        public double GetFactor()
        {
            Dictionary<Range, double> table = new Dictionary<Range, double>();
            table.Add(new Range(15, 24), 1.1);
            table.Add(new Range(25, 34), 1.0);
            table.Add(new Range(35, 39), 0.87);
            table.Add(new Range(40, 44), 0.83);
            table.Add(new Range(45, 49), 0.78);
            table.Add(new Range(50, 54), 0.75);
            table.Add(new Range(55, 59), 0.71);
            table.Add(new Range(60, 64), 0.68);
            table.Add(new Range(65, int.MaxValue), 0.65);

            double factor = 1.0;
            foreach (var item in table)
            {
                if (item.Key.IsInRange(this.Age))
                {
                    factor = item.Value;
                    break;
                }
            }
            return factor;
        }
    }

    public struct Range
    {
        public int min, max;

        public Range(int min, int max)
        {
            this.min = min;
            this.max = max;
        }

        public bool IsInRange(int current)
        {
            return current >= min && current <= max;
        }
    }
}
