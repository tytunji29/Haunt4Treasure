using Haunt4Treasure.Models;
using Haunt4Treasure.RegistrationFlow;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Haunt4Treasure.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController(IAllService service) : ControllerBase
    {

        private readonly IAllService _allService = service;
        // POST: api/Payment/TopUp to call the service to top up the wallet
        [HttpPost("TopUp")]
        public async Task<ReturnObject> TopUp(decimal Amount)
        {
            //no userid is coming from token extract it from token
            var userId = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return new ReturnObject
                {
                    Status = false,
                    Message = "User not authenticated"
                };
            }

            if (Amount <= 0)
            {
                return new ReturnObject
                {
                    Status = false,
                    Message = "Minimium TopUp Amount"
                };
            }
            var result = await _allService.TopUpWallet(userId, Amount);
            return result;
        }

        // POST: api/Payment/CashOut to call the service to cash out the wallet
        [HttpPost("CashOut")]
        public async Task<ReturnObject> CashOut(GameCashOut GC)
        {
            //no userid is coming from token extract it from token
            var userId = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return new ReturnObject
                {
                    Status = false,
                    Message = "User not authenticated"
                };
            }
           
            var result = await _allService.UpdateGameSessionCashoutAsync(GC);
            return result;
        }
    }
}
