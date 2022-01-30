using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ConventionManagementService.Config
{
    /// <summary>
    /// Configuration parameters of the API in auth0
    /// </summary>
    public class Auth0Config
    {
        public string Domain { get; set; }

        public string Audience { get; set; }
    }
}
