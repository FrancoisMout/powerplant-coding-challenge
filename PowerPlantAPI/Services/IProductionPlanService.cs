using PowerPlantAPI.Contracts;

namespace PowerPlantAPI.Services
{
    public interface IProductionPlanService
    {
        IReadOnlyCollection<PowerPlantDispatch> CreateProductionPlan(ProductionPlanRequest request);
    }
}
