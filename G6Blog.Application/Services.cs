using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using G6Blog.Application.DTOs;
using G6Blog.Domain.Entities;
using G6Blog.Domain.Repositories;

namespace G6Blog.Application.Services
{
    // Moved Interfaces here to decouple from Infrastructure
    public interface IJwtService
    {
        string GenerateToken(string userId, string role);
        string ValidateToken(string token);
    }

    public interface IPasswordService
    {
        string HashPassword(string password);
        bool VerifyPassword(string password, string hash);
    }

    public interface IAuthService
    {
        Task<AuthResponse> RegisterAsync(RegisterRequest request);
        Task<AuthResponse> LoginAsync(LoginRequest request);
    }

    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepo;
        private readonly IPasswordService _passwordService;
        private readonly IJwtService _jwtService;

        public AuthService(IUserRepository userRepo, IPasswordService passwordService, IJwtService jwtService)
        {
            _userRepo = userRepo;
            _passwordService = passwordService;
            _jwtService = jwtService;
        }

        public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
        {
            var existing = await _userRepo.GetByEmailAsync(request.Email);
            if (existing != null) throw new Exception("User already exists");

            var user = new User
            {
                // Assign new ID if not handled by DB automatically, Mongo usually handles if unset but better explicit or let driver do it.
                // Id = ObjectId.GenerateNewId().ToString(), // Requires Mongo reference, skip for now
                Username = request.Username,
                Email = request.Email,
                Role = Role.User,
                CreatedAt = DateTime.UtcNow,
                Password = _passwordService.HashPassword(request.Password)
            };

            await _userRepo.CreateAsync(user);
            // Re-fetch or assume ID is set by reference if implementing successfully
            
            // Hack for now: validation might fail if ID is null. 
            // In Repository, if ID is null, driver generates it. BUT we need it for token.
            // Let's assume Repository handles ID generation or we generate it here?
            // Cleanest without dependency: let Repo return the User with ID.
            
            // The user object passed to CreateAsync will have its Id updated by the driver driver usually? No, only if BsonId is set.
            // Setup ID: 
            user.Id = Guid.NewGuid().ToString(); // Simple ID generation for speed

            var token = _jwtService.GenerateToken(user.Id, user.Role.ToString());

            return new AuthResponse { Token = token, Role = user.Role.ToString() };
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest request)
        {
            var user = await _userRepo.GetByEmailAsync(request.Email);
            if (user == null) throw new Exception("Invalid credentials");

            if (!_passwordService.VerifyPassword(request.Password, user.Password))
                throw new Exception("Invalid credentials");

            var token = _jwtService.GenerateToken(user.Id, user.Role.ToString());
            return new AuthResponse { Token = token, Role = user.Role.ToString() };
        }
    }

    public interface IBlogService
    {
        Task<Blog> CreateBlogAsync(string userId, CreateBlogRequest request);
        Task<Blog> GetBlogAsync(string id);
        Task<List<Blog>> GetBlogsAsync(int page, int limit);
        Task DeleteBlogAsync(string id, string userId, Role userRole);
        Task<Blog> UpdateBlogAsync(string id, string userId, UpdateBlogRequest request);
        Task ToggleLikeAsync(string id);
    }

    public class BlogService : IBlogService
    {
        private readonly IBlogRepository _blogRepo;

        public BlogService(IBlogRepository blogRepo)
        {
            _blogRepo = blogRepo;
        }

        public async Task<Blog> CreateBlogAsync(string userId, CreateBlogRequest request)
        {
            var blog = new Blog
            {
                Id = Guid.NewGuid().ToString(),
                Title = request.Title,
                Content = request.Content,
                AuthorId = userId,
                Tags = request.Tags ?? new List<string>(),
                CreatedAt = DateTime.UtcNow,
                Likes = 0,
                Views = 0
            };
            return await _blogRepo.CreateAsync(blog);
        }

        public async Task<Blog> GetBlogAsync(string id)
        {
            var blog = await _blogRepo.GetByIDAsync(id);
             // Increment view
            if (blog != null) {
                await _blogRepo.IncrementFieldAsync(id, "views", 1);
            }
            return blog;
        }

         public async Task<List<Blog>> GetBlogsAsync(int page, int limit)
        {
            var (blogs, _) = await _blogRepo.ListAsync(null, page, limit);
            return blogs;
        }

        public async Task DeleteBlogAsync(string id, string userId, Role userRole)
        {
            var blog = await _blogRepo.GetByIDAsync(id);
            if (blog == null) return;

            if (blog.AuthorId != userId && userRole != Role.Admin)
            {
                throw new Exception("Unauthorized");
            }

            await _blogRepo.DeleteAsync(id);
        }
        public async Task<Blog> UpdateBlogAsync(string id, string userId, UpdateBlogRequest request)
        {
            var blog = await _blogRepo.GetByIDAsync(id);
            if (blog == null) return null;
            if (blog.AuthorId != userId) throw new Exception("Unauthorized");

            var updates = new Dictionary<string, object>();
            if (!string.IsNullOrEmpty(request.Title)) updates.Add("title", request.Title);
            if (!string.IsNullOrEmpty(request.Content)) updates.Add("content", request.Content);

            return await _blogRepo.UpdateAsync(id, updates);
        }

        public async Task ToggleLikeAsync(string id)
        {
             // Simple increment for now, real social app would track *who* liked it to prevent duplicates
             await _blogRepo.IncrementFieldAsync(id, "likes", 1);
        }
    }
}
