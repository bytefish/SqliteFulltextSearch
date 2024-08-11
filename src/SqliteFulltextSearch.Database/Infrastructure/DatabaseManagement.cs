using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace SqliteFulltextSearch.Database.Infrastructure
{
    public class DatabaseManagement
    {
        private readonly ILogger<DatabaseManagement> _logger;
        private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;

        public DatabaseManagement(ILogger<DatabaseManagement> logger, IDbContextFactory<ApplicationDbContext> dbContextFactory)
        {
            _logger = logger;
            _dbContextFactory = dbContextFactory;
        }

        public async Task CreateDatabase(CancellationToken cancellationToken)
        {
            using var applicationDbContext = _dbContextFactory.CreateDbContext();

            var file = Path.Combine(AppContext.BaseDirectory, "sql/fts.sql");

            var sql = File.ReadAllText(file);

            await applicationDbContext.Database.ExecuteSqlRawAsync(sql, cancellationToken);
        }
    }
}
