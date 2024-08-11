
using SqliteFulltextSearch.Database.Infrastructure;
using SqliteFulltextSearch.Shared.Infrastructure;

namespace SqliteFulltextSearch.Api.Hosting
{
    public class DatabaseMigrationBackgroundService : BackgroundService
    {
        private readonly ILogger<DatabaseMigrationBackgroundService> _logger;

        private readonly DatabaseManagement _databaseManagement;

        public DatabaseMigrationBackgroundService(ILogger<DatabaseMigrationBackgroundService> logger, DatabaseManagement databaseManagement)
        {
            _logger = logger;
            _databaseManagement = databaseManagement;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();

            // Create Database ...
            await _databaseManagement.CreateDatabase(cancellationToken);

            // Apply Migrations ...
        }
    }
}
