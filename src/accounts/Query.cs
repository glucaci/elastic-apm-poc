using System.Collections.Generic;
using System.Threading.Tasks;
using HotChocolate;

namespace Demo.Accounts
{
    public class Query
    {
        public Task<IEnumerable<User>> GetUsers([Service] UserRepository repository) =>
            repository.GetUsers();

        public Task<User> GetUser(int id, [Service] UserRepository repository) => 
            repository.GetUser(id);
    }
}