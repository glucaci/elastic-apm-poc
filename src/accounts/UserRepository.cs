using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace Demo.Accounts
{
    public class UserRepository
    {
        private readonly IMongoCollection<User> _usersCollection;
        private readonly Lazy<Task> _initialize;
        private readonly IReadOnlyList<User> _users = new[]
        {
            new User(1, "Ada Lovelace", new DateTime(1815, 12, 10), "@ada"),
            new User(2, "Alan Turing", new DateTime(1912, 06, 23), "@complete")
        };

        public UserRepository(DbContext dbContext)
        {
            _usersCollection = dbContext.CreateCollection<User>();

            _initialize = new Lazy<Task>(async () =>
            {
                var options = new ReplaceOptions {IsUpsert = true};
                foreach (var user in _users)
                {
                    await _usersCollection.ReplaceOneAsync($"{{_id:{user.Id}}}", user, options);
                }
            });
        }

        public async Task<User> GetUser(int id)
        {
            await _initialize.Value;
            return await _usersCollection.Find($"{{_id:{id}}}").FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<User>> GetUsers() 
        {
            await _initialize.Value;
            return await _usersCollection.Find(string.Empty).ToListAsync();
        }
    }
}