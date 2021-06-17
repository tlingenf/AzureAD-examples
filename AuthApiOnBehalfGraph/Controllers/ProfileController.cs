using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using trlingensecwebapi.GraphApi;

namespace trlingensecwebapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProfileController : ControllerBase
    {
        protected readonly GraphClient _graphClient = null;

        public ProfileController(GraphClient graphClient)
        {
            _graphClient = graphClient;
        }

        [HttpGet]
        public async Task<User> GetUpn()
        {
            var currentUser = await _graphClient.GetUserProfileAsync();
            return currentUser;
        }
    }
}
