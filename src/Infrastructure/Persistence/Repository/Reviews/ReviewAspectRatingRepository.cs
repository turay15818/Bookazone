using Bookazone.Application.Interfaces.Repository.Reviews;
using Bookazone.Domain.Entities.Reviews;
using Bookazone.Infrastructure.Persistence.DbContext;
using Bookazone.Infrastructure.Persistence.Repository;

namespace Bookazone.Infrastructure.Persistence.Repository.Reviews;

public class ReviewAspectRatingRepository(
    BookazoneDbContext context,
    ILogger<ReviewAspectRatingRepository> logger)
    : BaseRepository<ReviewAspectRating>(context, logger), IReviewAspectRatingRepository
{
}
