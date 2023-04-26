using FluentAssertions;
using Moq;
using PowerPlantAPI.Contracts;
using PowerPlantAPI.Controllers;
using PowerPlantAPI.Services;
using Xunit;

namespace PowerPlantAPI.Test.Controllers
{
    public class ProductionPlanControllerTest
    {
        private readonly Mock<IProductionPlanService> _productionPlanServiceMock = new();

        [Fact]
        public void CreateProductionPlan_ShouldCallService()
        {
            // Arrange
            var request = new ProductionPlanRequest();
            var response = new List<PowerPlantDispatch>();
            var controller = new ProductionPlanController(_productionPlanServiceMock.Object);

            _productionPlanServiceMock
                .Setup(x => x.CreateProductionPlan(It.IsAny<ProductionPlanRequest>()))
                .Returns(response);

            // Act
            var result = controller.CreateProductionPlan(request);

            // Assert
            result.Should().NotBeNull();
            _productionPlanServiceMock.Verify(x => x.CreateProductionPlan(request), Times.Once);
        }
    }
}
