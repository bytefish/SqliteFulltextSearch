// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ElasticsearchFulltextExample.Api.Constants;
using ElasticsearchFulltextExample.Api.Infrastructure.Authentication;
using ElasticsearchFulltextExample.Api.Infrastructure.Exceptions;
using ElasticsearchFulltextExample.Database;
using ElasticsearchFulltextExample.Database.Model;
using ElasticsearchFulltextExample.Shared.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ElasticsearchFulltextExample.Api.Services
{
    public class UserService
    {
        private readonly ILogger<UserService> _logger;

        private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;

        public UserService(ILogger<UserService> logger, IDbContextFactory<ApplicationDbContext> dbContextFactory)
        {
            _logger = logger;
            _dbContextFactory = dbContextFactory;
        }

        public async Task<User> CreateUserAsync(User user, CurrentUser currentUser, CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();

            bool isAuthorized = currentUser.IsInRole(Roles.Administrator);

            if (!isAuthorized)
            {
                throw new AuthorizationFailedException("Insufficient Permissions to create a new user");
            }

            using var applicationDbContext = await _dbContextFactory
                .CreateDbContextAsync(cancellationToken)
                .ConfigureAwait(false);

            user.LastEditedBy = currentUser.UserId;

            await applicationDbContext
                .AddAsync(user, cancellationToken)
                .ConfigureAwait(false);

            await applicationDbContext
                .SaveChangesAsync(cancellationToken)
                .ConfigureAwait(false);

            return user;
        }

        public Task DeleteUserByUserIdAsync(int userId, CurrentUser currentUser, CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();

            throw new NotImplementedException();

        }

        public async Task<User> GetUserByEmailAsync(string email, CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();

            using var applicationDbContext = await _dbContextFactory
                .CreateDbContextAsync(cancellationToken)
                .ConfigureAwait(false);

            var user = await applicationDbContext.Users.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Email == email, cancellationToken)
                .ConfigureAwait(false);

            if (user == null)
            {
                throw new AuthenticationFailedException();
            }

            return user;
        }

        public async Task<List<Claim>> GetClaimsAsync(string email, string[] roles, CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();

            using var applicationDbContext = await _dbContextFactory
                .CreateDbContextAsync(cancellationToken)
                .ConfigureAwait(false);

            var user = await applicationDbContext.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Email == email, cancellationToken)
                .ConfigureAwait(false);

            if (user == null)
            {
                throw new AuthenticationFailedException();
            }

            // Build the Claims for the ClaimsPrincipal
            var claims = CreateClaims(user, roles);

            return claims;
        }

        private List<Claim> CreateClaims(User user, string[] roles)
        {
            _logger.TraceMethodEntry();

            var claims = new List<Claim>();

            claims.Add(new Claim(ClaimTypes.NameIdentifier, user.Email));
            claims.Add(new Claim(ClaimTypes.Email, user.Email));
            claims.Add(new Claim(ClaimTypes.Sid, Convert.ToString(user.Id)));
            claims.Add(new Claim(ClaimTypes.Name, Convert.ToString(user.PreferredName)));

            // Roles:
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            return claims;
        }
    }
}