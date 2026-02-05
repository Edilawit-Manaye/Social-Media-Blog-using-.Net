using System;
using System.Collections.Generic;

namespace G6Blog.Application.DTOs
{
    public class RegisterRequest
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class LoginRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class AuthResponse
    {
        public string Token { get; set; }
        public string RefreshToken { get; set; }
        public string Role { get; set; }
    }

    public class CreateBlogRequest
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public List<string> Tags { get; set; }
    }
    
    public class UpdateBlogRequest
    {
        public string Title { get; set; }
        public string Content { get; set; }
    }

    public class UserProfileDto
    {
         public string Id { get; set; }
         public string Username { get; set; }
         public string BIO { get; set; }
         public string Avatar { get; set; }
    }

    public class UpdateProfileRequest
    {
        public string Bio { get; set; }
        public string Avatar { get; set; }
    }
}
