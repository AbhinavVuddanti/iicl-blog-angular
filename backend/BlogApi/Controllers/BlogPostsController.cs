using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BlogApi.Data;
using BlogApi.Models;

namespace BlogApi.Controllers
{
    [Route("api/blogs")]
    [ApiController]
    public class BlogPostsController : ControllerBase
    {
        private readonly BlogContext _context;

        public BlogPostsController(BlogContext context)
        {
            _context = context;
        }

        // GET: api/blogs?page=1&pageSize=10&author=John
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BlogPost>>> GetBlogPosts(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? author = null)
        {
            IQueryable<BlogPost> query = _context.BlogPosts;

            if (!string.IsNullOrEmpty(author))
            {
                query = query.Where(b => b.Author.Contains(author, StringComparison.OrdinalIgnoreCase));
            }

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            var blogPosts = await query
                .OrderByDescending(b => b.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            Response.Headers["X-Pagination-TotalCount"] = totalCount.ToString();
            Response.Headers["X-Pagination-PageSize"] = pageSize.ToString();
            Response.Headers["X-Pagination-CurrentPage"] = page.ToString();
            Response.Headers["X-Pagination-TotalPages"] = totalPages.ToString();

            return Ok(blogPosts);
        }

        // GET: api/blogs/5
        [HttpGet("{id}")]
        public async Task<ActionResult<BlogPost>> GetBlogPost(int id)
        {
            var blogPost = await _context.BlogPosts.FindAsync(id);
            if (blogPost == null) return NotFound();
            return Ok(blogPost);
        }

        // POST: api/blogs
        [HttpPost]
        public async Task<ActionResult<BlogPost>> PostBlogPost(BlogPost blogPost)
        {
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

            return CreatedAtAction(nameof(GetBlogPost), new { id = blogPost.Id }, blogPost);
        }

        // PUT: api/blogs/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutBlogPost(int id, BlogPost blogPost)
        {
            if (id != blogPost.Id) return BadRequest("ID mismatch");

            if (string.IsNullOrWhiteSpace(blogPost.Title) || 
                string.IsNullOrWhiteSpace(blogPost.Author) || 
                string.IsNullOrWhiteSpace(blogPost.Content))
            {
                return BadRequest("Title, Author, and Content are required.");
            }

            var existing = await _context.BlogPosts.FindAsync(id);
            if (existing == null) return NotFound();

            existing.Title = blogPost.Title;
            existing.Author = blogPost.Author;
            existing.Content = blogPost.Content;
            existing.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return Conflict();
            }

            return NoContent();
        }

        // DELETE: api/blogs/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBlogPost(int id)
        {
            var blogPost = await _context.BlogPosts.FindAsync(id);
            if (blogPost == null) return NotFound();

            _context.BlogPosts.Remove(blogPost);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}