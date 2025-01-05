using System;
using System.Threading.Tasks;
using UnityEngine;

namespace TaloGameServices
{
    public class SocketTicketsAPI : BaseAPI
    {
        public SocketTicketsAPI() : base("v1/socket-tickets") { }

        public async Task<string> CreateTicket()
        {
            var uri = new Uri(baseUrl);
            var json = await Call(uri, "POST");

            var res = JsonUtility.FromJson<SocketTicketsCreateResponse>(json);
            return res.ticket;
        }
    }
}
