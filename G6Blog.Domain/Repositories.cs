using System.Collections.Generic;
using System.Threading.Tasks;
using G6Blog.Domain.Entities;

namespace G6Blog.Domain.Repositories
{
    public interface IUserRepository
    {
        Task<User> CreateAsync(User user);
        Task<User> GetByEmailAsync(string email);
        Task<User> GetByIDAsync(string id);
        Task UpdateRoleAsync(string id, Role role);
    }

    public interface IBlogRepository
    {
        Task<Blog> CreateAsync(Blog blog);
        Task<Blog> GetByIDAsync(string id);
        Task<Blog> UpdateAsync(string id, Dictionary<string, object> updates);
        Task DeleteAsync(string id);
        Task<(List<Blog>, long)> ListAsync(Dictionary<string, object> filter, int page, int limit);
        Task IncrementFieldAsync(string id, string field, long delta);
    }

    public interface IProfileRepository
    {
        Task UpsertAsync(Profile profile);
        Task<Profile> GetByUserIDAsync(string userId);
    }

    public interface ITokenRepository
    {
        Task SaveTokenAsync(Token token);
    }
}
