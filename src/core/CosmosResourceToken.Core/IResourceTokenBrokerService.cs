using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace CosmosResourceToken.Core
{
    public interface IResourceTokenBrokerService
    {
        Task<IActionResult> Get(string token);

        Task<IActionResult> Get(string token, string userId, string userPermissionId);
    }
}
