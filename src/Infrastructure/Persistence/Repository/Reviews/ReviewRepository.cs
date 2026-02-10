using Bookazone.Application.Interfaces.Repository.Reviews;
using Bookazone.Domain.Entities.Reviews;
using Bookazone.Infrastructure.Persistence.DbContext;
using Bookazone.Infrastructure.Persistence.Repository;

namespace Bookazone.Infrastructure.Persistence.Repository.Reviews;

public class ReviewRepository(
    BookazoneDbContext context,
    ILogger<ReviewRepository> logger)
    : BaseRepository<Review>(context, logger), IReviewRepository
{
}
