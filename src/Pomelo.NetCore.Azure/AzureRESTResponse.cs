using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Pomelo.NetCore.Azure
{
    internal class AzureRESTResponse
    {
        public HttpStatusCode StatusCode { get; set; }
        public string Content { get; set; }
    }
}
