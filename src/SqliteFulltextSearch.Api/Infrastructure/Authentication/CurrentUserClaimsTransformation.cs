// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ElasticsearchFulltextExample.Api.Services;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

namespace ElasticsearchFulltextExample.Api.Infrastructure.Authentication
{
    public class CurrentUserClaimsTransformation : IClaimsTransformation
    {
        private readonly CurrentUser _currentUser;
        private readonly UserService _userService;

        public CurrentUserClaimsTransformation(CurrentUser currentUser, UserService userService)
        {
            _currentUser = currentUser;
            _userService = userService;
        }

        public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
        {
            _currentUser.Principal = principal;

            if (principal.FindFirstValue(ClaimTypes.NameIdentifier) is { Length: > 0 } name)
            {
                _currentUser.User = await _userService.GetUserByEmailAsync(name, default); // Where do we get the CancellationToken from?
            }

            return principal;
        }
    }
}
