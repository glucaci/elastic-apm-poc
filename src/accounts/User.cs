using System;

namespace Demo.Accounts
{
    public class User
    {
        public int Id { get; }
        public string Name { get; }
        public DateTime Birthdate { get; }
        public string Username { get; }

        public User(int id, string name, DateTime birthdate, string username)
        {
            Id = id;
            Name = name;
            Birthdate = birthdate;
            Username = username;
        }
    }
    
}