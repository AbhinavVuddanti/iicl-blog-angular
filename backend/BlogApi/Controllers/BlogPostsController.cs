using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BlogApi.Data;
using BlogApi.Models;

namespace BlogApi.Controllers
{
    [Route("api/BlogPosts")]
    [ApiController]
    public class BlogPostsController : ControllerBase
    {
        private readonly BlogContext _context;

        public BlogPostsController(BlogContext context)
        {
            _context = context;
        }

        // GET: api/BlogPosts?page=1&pageSize=10&author=John
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BlogPost>>> GetBlogPosts(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? author = null)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100; // prevent abuse

            IQueryable<BlogPost> query = _context.BlogPosts.AsQueryable();

            // Filter by author (case-insensitive partial match)
            if (!string.IsNullOrWhiteSpace(author))
            {
                query = query.Where(b => b.Author.Contains(author, StringComparison.OrdinalIgnoreCase));
            }

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var blogPosts = await query
                .OrderByDescending(b => b.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Pagination headers (optional but useful for frontend)
            Response.Headers["X-Pagination-TotalCount"] = totalCount.ToString();
            Response.Headers["X-Pagination-PageSize"]   = pageSize.ToString();
            Response.Headers["X-Pagination-CurrentPage"] = page.ToString();
            Response.Headers["X-Pagination-TotalPages"] = totalPages.ToString();

            return Ok(blogPosts);
        }

        // GET: api/BlogPosts/5
        [HttpGet("{id}")]
        public async Task<ActionResult<BlogPost>> GetBlogPost(int id)
        {
            var blogPost = await _context.BlogPosts.FindAsync(id);

            if (blogPost == null)
                return NotFound();

            return Ok(blogPost);
        }

        // POST: api/BlogPosts
        [HttpPost]
        public async Task<ActionResult<BlogPost>> PostBlogPost([FromBody] BlogPost blogPost)
        {
            if (blogPost == null)
                return BadRequest("Request body is required.");

            if (string.IsNullOrWhiteSpace(blogPost.Title) ||
                string.IsNullOrWhiteSpace(blogPost.Author) ||
                string.IsNullOrWhiteSpace(blogPost.Content))
            {
                return BadRequest("Title, Author, and Content are required.");
            }

            blogPost.CreatedAt = DateTime.UtcNow;
            blogPost.UpdatedAt = DateTime.UtcNow;

            _context.BlogPosts.Add(blogPost);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetBlogPost),
                new { id = blogPost.Id },
                blogPost);
        }

        // PUT: api/BlogPosts/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutBlogPost(int id, [FromBody] BlogPost blogPost)
        {
            if (blogPost == null)
                return BadRequest("Request body is required.");

            if (id != blogPost.Id)
                return BadRequest("ID in URL must match ID in body.");

            if (string.IsNullOrWhiteSpace(blogPost.Title) ||
                string.IsNullOrWhiteSpace(blogPost.Author) ||
                string.IsNullOrWhiteSpace(blogPost.Content))
            {
                return BadRequest("Title, Author, and Content are required.");
            }

            var existing = await _context.BlogPosts.FindAsync(id);
            if (existing == null)
                return NotFound();

            // Update fields
            existing.Title    = blogPost.Title;
            existing.Author   = blogPost.Author;
            existing.Content  = blogPost.Content;
            existing.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return Conflict("The post was modified by another user. Please reload and try again.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }

            return NoContent();
        }

        // DELETE: api/BlogPosts/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBlogPost(int id)
        {
            var blogPost = await _context.BlogPosts.FindAsync(id);
            if (blogPost == null)
                return NotFound();

            _context.BlogPosts.Remove(blogPost);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}