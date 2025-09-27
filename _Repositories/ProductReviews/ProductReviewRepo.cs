using Microsoft.EntityFrameworkCore;
using TheLightStore.Datas;
using TheLightStore.Dtos.Paging;
using TheLightStore.Interfaces.Repository;
using TheLightStore.Models.ProductReviews;

namespace TheLightStore.Repositories.ProductReviews;

public class ProductReviewRepo : IProductReviewRepo
{
    private readonly DBContext _context;

    public ProductReviewRepo(DBContext context)
    {
        _context = context;
    }

    #region Basic CRUD

    public async Task<ProductReview?> GetByIdAsync(int id, bool includeRelated = false)
    {
        var query = _context.ProductReviews.AsQueryable();

        if (includeRelated)
        {
            query = query
                .Include(r => r.Product)
                .Include(r => r.User)
                .Include(r => r.Order)
                .Include(r => r.ReviewHelpfulVotes);
        }

        return await query.FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<PagedResult<ProductReview>> GetByProductIdAsync(int productId, PagedRequest pagedRequest)
    {
        var query = _context.ProductReviews
            .Where(r => r.ProductId == productId && r.Status == "approved")
            .Include(r => r.User)
            .Include(r => r.ReviewHelpfulVotes)
            .AsQueryable();

        // Apply search if provided
        if (!string.IsNullOrEmpty(pagedRequest.Search))
        {
            query = query.Where(r => r.Comment.Contains(pagedRequest.Search) ||
                                   (r.Title != null && r.Title.Contains(pagedRequest.Search)) ||
                                   r.CustomerName.Contains(pagedRequest.Search));
        }

        // Apply sorting
        if (!string.IsNullOrEmpty(pagedRequest.Sort))
        {
            switch (pagedRequest.Sort.ToLower())
            {
                case "rating_desc":
                    query = query.OrderByDescending(r => r.Rating);
                    break;
                case "rating_asc":
                    query = query.OrderBy(r => r.Rating);
                    break;
                case "helpful_desc":
                    query = query.OrderByDescending(r => r.HelpfulCount);
                    break;
                case "oldest":
                    query = query.OrderBy(r => r.CreatedAt);
                    break;
                case "newest":
                default:
                    query = query.OrderByDescending(r => r.CreatedAt);
                    break;
            }
        }
        else
        {
            query = query.OrderByDescending(r => r.CreatedAt);
        }

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((pagedRequest.Page - 1) * pagedRequest.Size)
            .Take(pagedRequest.Size)
            .ToListAsync();

        return new PagedResult<ProductReview>
        {
            Items = items,
            TotalCount = totalCount,
            Page = pagedRequest.Page,
            PageSize = pagedRequest.Size
        };
    }

    public async Task<ProductReview> AddAsync(ProductReview review)
    {
        _context.ProductReviews.Add(review);
        await _context.SaveChangesAsync();
        return review;
    }

    public async Task<ProductReview> UpdateAsync(ProductReview review)
    {
        _context.ProductReviews.Update(review);
        await _context.SaveChangesAsync();
        return review;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var review = await _context.ProductReviews.FindAsync(id);
        if (review == null) return false;

        _context.ProductReviews.Remove(review);
        return await _context.SaveChangesAsync() > 0;
    }

    #endregion

    #region Review Queries

    public async Task<PagedResult<ProductReview>> GetUserReviewsAsync(int userId, PagedRequest pagedRequest)
    {
        var query = _context.ProductReviews
            .Where(r => r.UserId == userId)
            .Include(r => r.Product)
            .Include(r => r.ReviewHelpfulVotes)
            .AsQueryable();

        // Apply search if provided
        if (!string.IsNullOrEmpty(pagedRequest.Search))
        {
            query = query.Where(r => r.Comment.Contains(pagedRequest.Search) ||
                                   (r.Title != null && r.Title.Contains(pagedRequest.Search)) ||
                                   r.Product.Name.Contains(pagedRequest.Search));
        }

        // Apply sorting
        if (!string.IsNullOrEmpty(pagedRequest.Sort))
        {
            switch (pagedRequest.Sort.ToLower())
            {
                case "rating_desc":
                    query = query.OrderByDescending(r => r.Rating);
                    break;
                case "rating_asc":
                    query = query.OrderBy(r => r.Rating);
                    break;
                case "product_name":
                    query = query.OrderBy(r => r.Product.Name);
                    break;
                case "oldest":
                    query = query.OrderBy(r => r.CreatedAt);
                    break;
                case "newest":
                default:
                    query = query.OrderByDescending(r => r.CreatedAt);
                    break;
            }
        }
        else
        {
            query = query.OrderByDescending(r => r.CreatedAt);
        }

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((pagedRequest.Page - 1) * pagedRequest.Size)
            .Take(pagedRequest.Size)
            .ToListAsync();

        return new PagedResult<ProductReview>
        {
            Items = items,
            TotalCount = totalCount,
            Page = pagedRequest.Page,
            PageSize = pagedRequest.Size
        };
    }

    public async Task<PagedResult<ProductReview>> GetPendingReviewsAsync(PagedRequest pagedRequest)
    {
        var query = _context.ProductReviews
            .Where(r => r.Status == "pending")
            .Include(r => r.Product)
            .Include(r => r.User)
            .AsQueryable();

        // Apply search if provided
        if (!string.IsNullOrEmpty(pagedRequest.Search))
        {
            query = query.Where(r => r.Comment.Contains(pagedRequest.Search) ||
                                   (r.Title != null && r.Title.Contains(pagedRequest.Search)) ||
                                   r.Product.Name.Contains(pagedRequest.Search) ||
                                   r.CustomerName.Contains(pagedRequest.Search));
        }

        // Apply sorting
        if (!string.IsNullOrEmpty(pagedRequest.Sort))
        {
            switch (pagedRequest.Sort.ToLower())
            {
                case "rating_desc":
                    query = query.OrderByDescending(r => r.Rating);
                    break;
                case "product_name":
                    query = query.OrderBy(r => r.Product.Name);
                    break;
                case "customer_name":
                    query = query.OrderBy(r => r.CustomerName);
                    break;
                case "oldest":
                    query = query.OrderBy(r => r.CreatedAt);
                    break;
                case "newest":
                default:
                    query = query.OrderByDescending(r => r.CreatedAt);
                    break;
            }
        }
        else
        {
            query = query.OrderByDescending(r => r.CreatedAt);
        }

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((pagedRequest.Page - 1) * pagedRequest.Size)
            .Take(pagedRequest.Size)
            .ToListAsync();

        return new PagedResult<ProductReview>
        {
            Items = items,
            TotalCount = totalCount,
            Page = pagedRequest.Page,
            PageSize = pagedRequest.Size
        };
    }

    public async Task<bool> HasUserReviewedProductAsync(int userId, int productId)
    {
        return await _context.ProductReviews
            .AnyAsync(r => r.UserId == userId && r.ProductId == productId);
    }

    public async Task<bool> HasUserPurchasedProductAsync(int userId, int productId)
    {
        return await _context.OrderItems
            .Where(oi => oi.Order.UserId == userId && 
                        oi.ProductId == productId && 
                        oi.Order.OrderStatus == "completed")
            .AnyAsync();
    }

    #endregion

    #region Statistics

    public async Task<decimal> GetAverageRatingAsync(int productId)
    {
        var reviews = await _context.ProductReviews
            .Where(r => r.ProductId == productId && r.Status == "approved")
            .Select(r => r.Rating)
            .ToListAsync();

        if (!reviews.Any()) return 0;

        return (decimal)reviews.Average();
    }

    public async Task<int> GetTotalReviewsAsync(int productId)
    {
        return await _context.ProductReviews
            .CountAsync(r => r.ProductId == productId && r.Status == "approved");
    }

    public async Task<Dictionary<int, int>> GetRatingDistributionAsync(int productId)
    {
        var distribution = await _context.ProductReviews
            .Where(r => r.ProductId == productId && r.Status == "approved")
            .GroupBy(r => r.Rating)
            .Select(g => new { Rating = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Rating, x => x.Count);

        // Ensure all ratings 1-5 are present
        for (int i = 1; i <= 5; i++)
        {
            if (!distribution.ContainsKey(i))
                distribution[i] = 0;
        }

        return distribution;
    }

    #endregion

    #region Vote Management

    public async Task<ReviewHelpfulVote?> GetVoteAsync(int reviewId, int? userId, string? ipAddress)
    {
        var query = _context.ReviewHelpfulVotes
            .Where(v => v.ReviewId == reviewId);

        if (userId.HasValue)
        {
            query = query.Where(v => v.UserId == userId);
        }
        else if (!string.IsNullOrEmpty(ipAddress))
        {
            query = query.Where(v => v.IpAddress == ipAddress && v.UserId == null);
        }
        else
        {
            return null;
        }

        return await query.FirstOrDefaultAsync();
    }

    public async Task<ReviewHelpfulVote> AddVoteAsync(ReviewHelpfulVote vote)
    {
        _context.ReviewHelpfulVotes.Add(vote);
        await _context.SaveChangesAsync();
        await UpdateHelpfulCountAsync(vote.ReviewId);
        return vote;
    }

    public async Task<bool> RemoveVoteAsync(int reviewId, int? userId, string? ipAddress)
    {
        var vote = await GetVoteAsync(reviewId, userId, ipAddress);
        if (vote == null) return false;

        _context.ReviewHelpfulVotes.Remove(vote);
        var result = await _context.SaveChangesAsync() > 0;
        if (result)
        {
            await UpdateHelpfulCountAsync(reviewId);
        }
        return result;
    }

    public async Task<bool> UpdateHelpfulCountAsync(int reviewId)
    {
        var helpfulCount = await _context.ReviewHelpfulVotes
            .CountAsync(v => v.ReviewId == reviewId && v.IsHelpful);

        var review = await _context.ProductReviews.FindAsync(reviewId);
        if (review == null) return false;

        review.HelpfulCount = helpfulCount;
        return await _context.SaveChangesAsync() > 0;
    }

    #endregion
}