using Microsoft.AspNetCore.Mvc;
using PowerPlantAPI.Contracts;
using PowerPlantAPI.Services;

namespace PowerPlantAPI.Controllers
{
    [ApiController]
    [Route("api/productionplan")]
    public class ProductionPlanController : ControllerBase
    {
        private readonly IProductionPlanService _service;

        public ProductionPlanController(IProductionPlanService service)
        {
            _service = service;
        }

        [HttpPost]
        public IReadOnlyCollection<PowerPlantDispatch> CreateProductionPlan([FromBody] ProductionPlanRequest request)
        {
            return _service.CreateProductionPlan(request);
        }
    }
}
