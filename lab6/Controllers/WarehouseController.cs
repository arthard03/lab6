using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using lab6.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace Task6.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WarehouseController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public WarehouseController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost]
        public async Task<IActionResult> AddProductToWarehouse([FromBody] ProductWarehouseRequest request)
        {
            try
            {
                if (request.Amount <= 0)
                {
                    return BadRequest("Amount must be greater that 0");
                }

                using var connection = new SqlConnection(_configuration["ConnectionStrings:DefaultConnection"]);
                await connection.OpenAsync();

                var productExistsQuery = "SELECT COUNT(1) FROM Product WHERE IdProduct = @IdProduct";
                int productCount;
                using (var command = new SqlCommand(productExistsQuery, connection))
                {
                    command.Parameters.AddWithValue("@IdProduct", request.ProductId);
                    productCount = (int)await command.ExecuteScalarAsync();
                }
                if (productCount == 0)
                {
                    return NotFound($"There is no Product with that id {request.ProductId} ");
                }

                var warehouseExistsQuery = "SELECT COUNT(1) FROM Warehouse WHERE IdWarehouse = @IdWarehouse";
                int warehouseCount;
                using (var command = new SqlCommand(warehouseExistsQuery, connection))
                {
                    command.Parameters.AddWithValue("@IdWarehouse", request.WarehouseId);
                    warehouseCount = (int)await command.ExecuteScalarAsync();
                }
                if (warehouseCount == 0)
                {
                    return NotFound($"There is no Warehouse for that id {request.WarehouseId} ");
                }

                var getOrderQuery = "SELECT IdOrder FROM [Order] WHERE IdProduct = @ProductId";
                int orderId;
                using (var command = new SqlCommand(getOrderQuery, connection))
                {
                    command.Parameters.AddWithValue("@ProductId", request.ProductId);
                    var result = await command.ExecuteScalarAsync();
                    orderId = result != null ? Convert.ToInt32(result) : 0;
                }
                if (orderId == 0)
                {
                    return NotFound($"There are no Order for that Product: {request.ProductId} ");
                }

                var addProductToWarehouseQuery = @"
                    INSERT INTO Product_Warehouse (IdWarehouse, IdProduct, IdOrder, Amount, CreatedAt, Price)
                    VALUES (@WarehouseId, @ProductId, @IdOrder, @Amount, @CreatedAt, @Price)";
                using (var command = new SqlCommand(addProductToWarehouseQuery, connection))
                {
                    command.Parameters.AddWithValue("@WarehouseId", request.WarehouseId);
                    command.Parameters.AddWithValue("@ProductId", request.ProductId);
                    command.Parameters.AddWithValue("@Amount", request.Amount);
                    command.Parameters.AddWithValue("@CreatedAt", DateTime.Now);
                    var getPriceQuery = "SELECT Price FROM Product WHERE IdProduct = @ProductId";
                    using (var getPriceCommand = new SqlCommand(getPriceQuery, connection))
                    {
                        getPriceCommand.Parameters.AddWithValue("@ProductId", request.ProductId);
                        var price = Convert.ToDecimal(await getPriceCommand.ExecuteScalarAsync());
                        command.Parameters.AddWithValue("@Price", price * request.Amount);
                    }
                    command.Parameters.AddWithValue("@IdOrder", orderId);
                    await command.ExecuteNonQueryAsync();
                }

                return Ok("inserted successfully!");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }
    }
}
