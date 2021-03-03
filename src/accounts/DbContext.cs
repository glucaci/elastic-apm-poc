using Elastic.Apm.Mongo;
using MongoDB.Extensions.Context;

namespace Demo.Accounts
{
    public class DbContext : MongoDbContext
    {
        public DbContext(MongoOptions mongoOptions) : base(mongoOptions)
        {
        }

        protected override void OnConfiguring(IMongoDatabaseBuilder mongoDatabaseBuilder)
        {
            mongoDatabaseBuilder.RegisterDefaultConventionPack();
            mongoDatabaseBuilder.ConfigureConnection(s =>
                s.ClusterConfigurator = builder => builder.Subscribe(new MongoEventSubscriber()));
        }
    }
}