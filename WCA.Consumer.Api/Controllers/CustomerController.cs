// using Microsoft.AspNetCore.Authorization;
// using Microsoft.AspNetCore.Mvc;
// using WCA.Consumer.Api.Services.Contracts;

// namespace WCA.Consumer.Api.Controllers
// {
//     [ApiController]
//     [ApiExplorerSettings(IgnoreApi = true)]
//     [Route("[controller]s")]
//     public class CustomerController : BaseController
//     {
//         readonly ICustomerService _service;

//         public CustomerController(ICustomerService service)
//         {
//             this._service = service;
//         }

//         [AllowAnonymous]
//         [HttpGet("grpc/{id}")]
//         public async System.Threading.Tasks.Task<IActionResult> GetCustomerById(int id)
//         {
//             return Ok(await _service.GetCustomerById(id));
//         }

//         [AllowAnonymous]
//         [HttpGet("http/{id}")]
//         public async System.Threading.Tasks.Task<IActionResult> GetCustomerById2(int id)
//         {
//             return Ok(await _service.GetCustomerById2(id));
//         }
//     }
// }
