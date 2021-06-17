using Microsoft.AspNetCore.Http;
using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace trlingensecwebapi.GraphApi
{
    public class GraphClient
    {
        private readonly OnBehalfOfMsGraphAuthenticationProvider _msGraphAuthenticationProvider = null;

        public GraphClient(OnBehalfOfMsGraphAuthenticationProvider msGraphAuthenticationProvider)
        {
            _msGraphAuthenticationProvider = msGraphAuthenticationProvider;
        }

        public async Task<User> GetUserProfileAsync()
        {
            var client = new GraphServiceClient(_msGraphAuthenticationProvider);
            var currentUser = await client.Me.Request().GetAsync();
            return currentUser;
        }
    }
}
