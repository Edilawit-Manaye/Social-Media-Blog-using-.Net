using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using G6Blog.Application.Services;
using G6Blog.Application.DTOs;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using G6Blog.Domain.Entities;

namespace G6Blog.Api.Controllers
{
    [ApiController]
    [Route("api")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequest request)
        {
            try
            {
                var response = await _authService.RegisterAsync(request);
                return Ok(response);
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest request)
        {
             try
            {
                var response = await _authService.LoginAsync(request);
                return Ok(response);
            }
            catch (System.Exception ex)
            {
                return Unauthorized(new { error = ex.Message });
            }
        }
    }

    [ApiController]
    [Route("api/blogs")]
    public class BlogController : ControllerBase
    {
        private readonly IBlogService _blogService;

        public BlogController(IBlogService blogService)
        {
            _blogService = blogService;
        }

        [HttpGet]
        public async Task<IActionResult> List([FromQuery] int page = 1)
        {
            var blogs = await _blogService.GetBlogsAsync(page, 20);
            return Ok(blogs);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(string id)
        {
            var blog = await _blogService.GetBlogAsync(id);
            if (blog == null) return NotFound();
            return Ok(blog);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create(CreateBlogRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var blog = await _blogService.CreateBlogAsync(userId, request);
            return CreatedAtAction(nameof(Get), new { id = blog.Id }, blog);
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var roleStr = User.FindFirst("role")?.Value;
            var role = roleStr == "Admin" ? Role.Admin : Role.User;

            try {
                await _blogService.DeleteBlogAsync(id, userId, role);
                return Ok(new { status = "deleted" });
            } catch {
                return Forbid();
            }
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, UpdateBlogRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            try {
                var blog = await _blogService.UpdateBlogAsync(id, userId, request);
                if (blog == null) return NotFound();
                return Ok(blog);
            } catch {
                return Forbid();
            }
        }

        [Authorize]
        [HttpPost("{id}/like")]
        public async Task<IActionResult> Like(string id)
        {
            await _blogService.ToggleLikeAsync(id);
            return Ok(new { status = "liked" });
        }
    }
    [ApiController]
    [Route("api/ai")]
    public class AIController : ControllerBase
    {
        private readonly IAIService _aiService;

        public AIController(IAIService aiService)
        {
            _aiService = aiService;
        }

        [Authorize]
        [HttpPost("generate")]
        public async Task<IActionResult> Generate([FromBody] string prompt)
        {
            if (string.IsNullOrEmpty(prompt)) return BadRequest("Prompt required");
            var result = await _aiService.GenerateContent(prompt);
            return Ok(new { content = result });
        }
    }
}
