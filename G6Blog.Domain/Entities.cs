using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace G6Blog.Domain.Entities
{
    public enum Role
    {
        User,
        Admin
    }

    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("username")]
        public string Username { get; set; }

        [BsonElement("email")]
        public string Email { get; set; }

        [BsonElement("password")]
        public string Password { get; set; }

        [BsonElement("role")]
        [BsonRepresentation(BsonType.String)]
        public Role Role { get; set; }

        [BsonElement("created_at")]
        public DateTime CreatedAt { get; set; }
    }

    public class Blog
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("title")]
        public string Title { get; set; }

        [BsonElement("content")]
        public string Content { get; set; }

        [BsonElement("author_id")]
        public string AuthorId { get; set; }

        [BsonElement("author")]
        // Optional in Go code, included here
        public string Author { get; set; }

        [BsonElement("tags")]
        public List<string> Tags { get; set; } = new List<string>();

        [BsonElement("views")]
        public long Views { get; set; }

        [BsonElement("likes")]
        public long Likes { get; set; }

        [BsonElement("created_at")]
        public DateTime CreatedAt { get; set; }
    }

    public class Profile
    {
        [BsonElement("user_id")]
        public string UserId { get; set; }

        [BsonElement("bio")]
        public string Bio { get; set; }

        [BsonElement("avatar")]
        public string Avatar { get; set; }
    }

    public class Token
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("user_id")]
        public string UserId { get; set; }

        [BsonElement("token")]
        public string AccessToken { get; set; }

        [BsonElement("expires_at")]
        public DateTime ExpiresAt { get; set; }
    }
}
