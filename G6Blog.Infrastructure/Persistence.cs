using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using G6Blog.Domain.Entities;
using G6Blog.Domain.Repositories;
using MongoDB.Bson;
using MongoDB.Driver;

namespace G6Blog.Infrastructure.Persistence
{
    public class MongoDbContext
    {
        private readonly IMongoDatabase _database;

        public MongoDbContext(string connectionString, string databaseName)
        {
            var client = new MongoClient(connectionString);
            _database = client.GetDatabase(databaseName);
        }

        public IMongoCollection<User> Users => _database.GetCollection<User>("users");
        public IMongoCollection<Blog> Blogs => _database.GetCollection<Blog>("blogs");
        public IMongoCollection<Profile> Profiles => _database.GetCollection<Profile>("profiles");
        public IMongoCollection<Token> Tokens => _database.GetCollection<Token>("tokens");
    }

    public class UserRepository : IUserRepository
    {
        private readonly IMongoCollection<User> _users;

        public UserRepository(MongoDbContext context)
        {
            _users = context.Users;
            // Create unique index on email and username
            var indexKeys = Builders<User>.IndexKeys.Ascending(u => u.Email);
            var indexOptions = new CreateIndexOptions { Unique = true };
            _users.Indexes.CreateOne(new CreateIndexModel<User>(indexKeys, indexOptions));
            
            var usernameKeys = Builders<User>.IndexKeys.Ascending(u => u.Username);
            _users.Indexes.CreateOne(new CreateIndexModel<User>(usernameKeys, new CreateIndexOptions { Unique = true }));
        }

        public async Task<User> CreateAsync(User user)
        {
            await _users.InsertOneAsync(user);
            return user;
        }

        public async Task<User> GetByEmailAsync(string email)
        {
            return await _users.Find(u => u.Email == email).FirstOrDefaultAsync();
        }

        public async Task<User> GetByIDAsync(string id)
        {
           return await _users.Find(u => u.Id == id).FirstOrDefaultAsync();
        }

        public async Task UpdateRoleAsync(string id, Role role)
        {
            var update = Builders<User>.Update.Set(u => u.Role, role);
            await _users.UpdateOneAsync(u => u.Id == id, update);
        }
    }

    public class BlogRepository : IBlogRepository
    {
        private readonly IMongoCollection<Blog> _blogs;

        public BlogRepository(MongoDbContext context)
        {
            _blogs = context.Blogs;
             // Text index for search
            var indexKeys = Builders<Blog>.IndexKeys.Text(b => b.Title).Text(b => b.Content);
            _blogs.Indexes.CreateOne(new CreateIndexModel<Blog>(indexKeys));
        }

        public async Task<Blog> CreateAsync(Blog blog)
        {
            await _blogs.InsertOneAsync(blog);
            return blog;
        }

        public async Task<Blog> GetByIDAsync(string id)
        {
            return await _blogs.Find(b => b.Id == id).FirstOrDefaultAsync();
        }

        public async Task<Blog> UpdateAsync(string id, Dictionary<string, object> updates)
        {
            var updateDefinitions = new List<UpdateDefinition<Blog>>();
            foreach (var update in updates)
            {
                updateDefinitions.Add(Builders<Blog>.Update.Set(update.Key, update.Value));
            }
            
            if (updateDefinitions.Count == 0) return await GetByIDAsync(id);

            var combinedUpdate = Builders<Blog>.Update.Combine(updateDefinitions);
            await _blogs.UpdateOneAsync(b => b.Id == id, combinedUpdate);
            return await GetByIDAsync(id);
        }

        public async Task DeleteAsync(string id)
        {
            await _blogs.DeleteOneAsync(b => b.Id == id);
        }

        public async Task<(List<Blog>, long)> ListAsync(Dictionary<string, object> filter, int page, int limit)
        {
            var builder = Builders<Blog>.Filter;
            var mongoFilter = builder.Empty;

            if (filter != null)
            {
               foreach(var kvp in filter)
               {
                   if (kvp.Key == "$text")
                   {
                       mongoFilter &= builder.Text(kvp.Value.ToString());
                   }
                   else if (kvp.Key == "author_id")
                   {
                        mongoFilter &= builder.Eq(b => b.AuthorId, kvp.Value.ToString());
                   }
                   // Add other filters as needed
               }
            }

            var total = await _blogs.CountDocumentsAsync(mongoFilter);
            var blogs = await _blogs.Find(mongoFilter)
                .Skip((page - 1) * limit)
                .Limit(limit)
                .ToListAsync();

            return (blogs, total);
        }

        public async Task IncrementFieldAsync(string id, string field, long delta)
        {
             var update = Builders<Blog>.Update.Inc(field, delta);
             await _blogs.UpdateOneAsync(b => b.Id == id, update);
        }
    }

    public class ProfileRepository : IProfileRepository
    {
        private readonly IMongoCollection<Profile> _profiles;

        public ProfileRepository(MongoDbContext context)
        {
            _profiles = context.Profiles;
        }

        public async Task UpsertAsync(Profile profile)
        {
            var filter = Builders<Profile>.Filter.Eq(p => p.UserId, profile.UserId);
            var update = Builders<Profile>.Update
                .Set(p => p.Bio, profile.Bio)
                .Set(p => p.Avatar, profile.Avatar);
            
            await _profiles.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = true });
        }

        public async Task<Profile> GetByUserIDAsync(string userId)
        {
            return await _profiles.Find(p => p.UserId == userId).FirstOrDefaultAsync();
        }
    }

    public class TokenRepository : ITokenRepository
    {
        private readonly IMongoCollection<Token> _tokens;

        public TokenRepository(MongoDbContext context)
        {
            _tokens = context.Tokens;
        }

        public async Task SaveTokenAsync(Token token)
        {
            await _tokens.InsertOneAsync(token);
        }
    }
}
