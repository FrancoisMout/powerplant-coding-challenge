using PowerPlantAPI.Contracts;
using PowerPlantAPI.Models;
using PowerPlantAPI.Utils;

namespace PowerPlantAPI.Services
{
    public class ProductionPlanService : IProductionPlanService
    {
        private const decimal CarbonFactor = 0.3m;

        private readonly ILogger<ProductionPlanService> _logger;

        public ProductionPlanService(ILogger<ProductionPlanService> logger)
        {
            _logger = logger;
        }

        public IReadOnlyCollection<PowerPlantDispatch> CreateProductionPlan(ProductionPlanRequest request)
        {
            if (request == null)
            {
                _logger.LogError("The request cannot be null.");
                throw new ArgumentNullException(nameof(request), "The request cannot be null.");
            }

            var powerPlants = CreatePowerPlants(request);

            var usableWindTurbines = powerPlants
                .Where(x => x.Type == PlantType.WindTurbine && x.PMax != 0)
                .ToList();

            var minWindPower = usableWindTurbines.Count > 0 ? usableWindTurbines.Min(x => x.PMax) : decimal.MaxValue;

            var minTurbinePower = powerPlants
                .Where(x => x.Type != PlantType.WindTurbine)
                .Min(x => x.PMin);

            var minPowerProduced = Math.Min(minWindPower, minTurbinePower);

            if (request.Load < minPowerProduced)
            {
                _logger.LogError($"All power plants produce more than requested load: {request.Load} MWh.");
                throw new InvalidOperationException(
                    $"All power plants produce more than requested load: {request.Load} MWh.");
            }

            var maxPowerProduced = powerPlants.Sum(x => x.PMax);

            if (request.Load > maxPowerProduced)
            {
                _logger.LogError($"Cannot generate load: {request.Load} MWh with available plants.");
                throw new InvalidOperationException(
                    $"Cannot generate load: {request.Load} MWh with available plants.");
            }

            return CreateProductionPlan(powerPlants, request.Load);
        }

        private static IReadOnlyCollection<PowerPlantDispatch> CreateProductionPlan(List<PowerPlant> powerPlants, decimal loadToMatch)
        {
            var windTurbines = powerPlants
                            .Where(x => x.Type == PlantType.WindTurbine)
                            .ToList();

            var totalWindPower = windTurbines.Sum(x => x.PMax);

            if (totalWindPower == loadToMatch)
            {
                foreach (var powerPlant in powerPlants.Where(x => x.Type == PlantType.WindTurbine))
                {
                    powerPlant.Load = powerPlant.PMax;
                }
            }
            else if (totalWindPower > loadToMatch)
            {
                var windTurbinePowers = windTurbines
                    .Select(x => x.PMax)
                    .ToArray();

                (var loadReached, var solution) = KnapsackProblemSolver.SolveKnapsackProblem(windTurbinePowers, loadToMatch);

                if (loadReached == loadToMatch)
                {
                    // We found a wind turbines combination exactly matching the load.
                    windTurbines
                        .Where((o, i) => solution[i])
                        .ToList()
                        .ForEach(x => x.Load = x.PMax);
                }
                else
                {
                    // The wind turbines give some of the power but not enough.
                    // We have to involve other turbines.
                    var selectedWindTurbines = windTurbines
                        .Where((o, i) => solution[i])
                        .OrderBy(x => x.PMax)
                        .ToList();

                    var loadLeft = loadToMatch - loadReached;

                    TryUsingAllTurbines(selectedWindTurbines, powerPlants, loadLeft);
                }
            }
            else
            {
                TryUsingAllTurbines(windTurbines, powerPlants, loadToMatch - totalWindPower);
            }

            var response = powerPlants
                .Select(x => new PowerPlantDispatch
                {
                    Name = x.Name,
                    Load = x.Load,
                })
                .ToList();

            return response;
        }

        private static void TryUsingAllTurbines(List<PowerPlant> selectedWindTurbines, List<PowerPlant> powerPlants, decimal loadLeft)
        {
            // We will try to use all the wind turbines in combination with some of the other
            // turbines and remove wind turbines one after the other until a solution is found.
            while (true)
            {
                var turbines = powerPlants
                    .Where(x => x.Type != PlantType.WindTurbine && x.PMin < loadLeft)
                    .OrderBy(x => x.CostPerMWh);

                var turbinesMinPower = turbines
                    .Select(x => x.PMin)
                    .ToArray();

                (var turbineMinLoad, var turbineSolution) = KnapsackProblemSolver.SolveKnapsackProblem(turbinesMinPower, loadLeft);

                var selectedTurbines = turbines
                    .Where((x, i) => turbineSolution[i])
                    .ToList();

                var maxLoad = selectedTurbines
                    .Sum(x => x.PMax);

                if (maxLoad >= loadLeft)
                {
                    // Possible solution with the selectedTurbines
                    selectedWindTurbines.ForEach(x => x.Load = x.PMax);
                    BalanceLoadForSelectedTurbines(selectedTurbines, turbineMinLoad, loadLeft);
                    break;
                }
                else if (selectedWindTurbines.Count > 0)
                {
                    // No solution so remove the min wind turbine
                    loadLeft += selectedWindTurbines[0].PMax;
                    selectedWindTurbines = selectedWindTurbines.Skip(1).ToList();
                }
                else
                {
                    // We couldn't find a solution with the turbines
                    break;
                }
            }
        }

        private static void BalanceLoadForSelectedTurbines(List<PowerPlant> selectedTurbines, decimal turbineMinLoad, decimal loadLeft)
        {
            // 1. Fill every turbine with PMin
            selectedTurbines.ForEach(x => x.Load = x.PMin);

            // 2. Fill turbines with PMax starting from first
            loadLeft -= turbineMinLoad;
            foreach (var turbine in selectedTurbines)
            {
                if (loadLeft == 0)
                {
                    break;
                }

                var diff = turbine.PMax - turbine.PMin;
                if (loadLeft >= diff)
                {
                    turbine.Load = turbine.PMax;
                    loadLeft -= diff;
                }
                else
                {
                    turbine.Load += loadLeft;
                    loadLeft = 0;
                }
            }

            // 3. Shift power from last turbine to first
            var n = selectedTurbines.Count;
            for (int i = 0; i < n - 1; i++)
            {
                var index = n - i - 1;
                var loadToShift = selectedTurbines[index].Load;

                if (loadToShift == 0)
                {
                    continue;
                }

                var firstTurbines = selectedTurbines
                    .Where((o, i) => i < index)
                    .ToList();

                var success = TryInjectLoadToFirstTurbines(loadToShift, firstTurbines);
                if (success)
                {
                    selectedTurbines[index].Load = 0;
                }
            }
        }

        private static bool TryInjectLoadToFirstTurbines(decimal loadToShift, List<PowerPlant> selectedTurbines)
        {
            if (selectedTurbines.Count == 0)
            {
                return false;
            }

            var loadAvailable = selectedTurbines
                .Sum(x => x.PMax - x.Load);

            if (loadAvailable < loadToShift)
            {
                return false;
            }

            for (int i = 0; i < selectedTurbines.Count; i++)
            {
                if (loadToShift == 0)
                {
                    break;
                }

                var diff = selectedTurbines[i].PMax - selectedTurbines[i].Load;
                if (loadToShift >= diff)
                {
                    selectedTurbines[i].Load = selectedTurbines[i].PMax;
                    loadToShift -= diff;
                }
                else
                {
                    selectedTurbines[i].Load += loadToShift;
                    loadToShift = 0;
                }
            }

            return true;
        }

        private static List<PowerPlant> CreatePowerPlants(ProductionPlanRequest request)
        {
            return request.PowerPlants
                .Select(x => new PowerPlant
                {
                    Name = x.Name,
                    Type = x.Type,
                    PMin = x.PMin,
                    PMax = GetMaxPower(x, request.Fuels),
                    CostPerMWh = GetCostOfOperationPerMWh(x, request.Fuels),
                    Load = 0
                })
                .ToList();
        }

        private static decimal GetCostOfOperationPerMWh(PowerPlantDto powerPlant, EnergySources energySources)
        {
            var pricePerMWh = powerPlant.Type switch
            {
                PlantType.WindTurbine => 0,
                PlantType.GasFired => powerPlant.Efficiency == 0 ? decimal.MaxValue :
                    energySources.GasCost / powerPlant.Efficiency + CarbonFactor * energySources.CarbonCost,
                PlantType.TurboJet => energySources.KerosineCost / powerPlant.Efficiency,
                _ => throw new ArgumentOutOfRangeException(nameof(powerPlant),
                    $"{powerPlant.Type} is an unknown power plant type")
            };

            return pricePerMWh;
        }

        private static decimal GetMaxPower(PowerPlantDto powerPlant, EnergySources energySources)
        {
            return powerPlant.Type == PlantType.WindTurbine ?
                Math.Round(powerPlant.PMax * energySources.WindPercent / 100, 1) :
                powerPlant.PMax;
        }
    }
}
