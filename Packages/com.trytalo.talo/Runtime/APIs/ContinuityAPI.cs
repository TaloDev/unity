using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TaloGameServices
{
    public class ContinuityAPI : BaseAPI
    {
        public ContinuityAPI() : base("") { }

        public async Task Replay(
            Uri uri,
            string method,
            string content,
            List<HttpHeader> headers
        )
        {
            await Call(uri, method, content, headers, true);
        }
    }
}
