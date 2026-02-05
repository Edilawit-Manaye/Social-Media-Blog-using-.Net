using System;
using System.Threading.Tasks;
using G6Blog.Application.DTOs;
using G6Blog.Domain.Entities;
using G6Blog.Domain.Repositories;

namespace G6Blog.Application.Services
{
     public interface IAIService
    {
        Task<string> GenerateContent(string prompt);
    }

    // Move to Infrastructure usually, but wrapper here
    public interface IOpenAIClient
    {
        Task<string> Generate(string prompt);
    }
    
    // Infrastructure implementation of IOpenAIClient will be injected into AIService
    // Or simpler: Interface in Domain/Application, Impl in Infra.
    // Let's assume Infra implemented a service we can use.


    public class ProfileService 
    {
        private readonly IProfileRepository _profileRepo;

        public ProfileService(IProfileRepository profileRepo)
        {
            _profileRepo = profileRepo;
        }

        public async Task<UserProfileDto> GetProfileAsync(string userId, string currentUsername)
        {
            var profile = await _profileRepo.GetByUserIDAsync(userId);
            return new UserProfileDto
            {
                Id = userId,
                Username = currentUsername, // Fetched from User Repo if needed
                BIO = profile?.Bio ?? "",
                Avatar = profile?.Avatar ?? ""
            };
        }

        public async Task UpdateProfileAsync(string userId, UpdateProfileRequest request)
        {
            var profile = new Profile
            {
                UserId = userId,
                Bio = request.Bio,
                Avatar = request.Avatar
            };
            await _profileRepo.UpsertAsync(profile);
        }
    }
}
