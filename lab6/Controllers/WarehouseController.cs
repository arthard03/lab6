using System.Data.SqlClient;
using lab6.Extensions;
using lab6.Model;
using Microsoft.AspNetCore.Mvc;

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
        public async Task<IActionResult> AddProductToWarehouse(ProductWarehouseRequest request)
        {
            
            if (request.Amount <= 0)
            {
                throw new AmountException();
            }

            using var connection = new SqlConnection(_configuration["ConnectionStrings:DefaultConnection"]);
            await connection.OpenAsync();

            var productExistsQuery = "SELECT COUNT(1) FROM Product WHERE IdProduct = @IdProduct";
            int productCount;
            using (var cmd = new SqlCommand(productExistsQuery, connection))
            {
                cmd.Parameters.AddWithValue("@IdProduct", request.ProductId);
                productCount = (int)await cmd.ExecuteScalarAsync();
            }

            if (productCount == 0)
            {
                throw new ProductIdException();
            }

            var warehouseExistsQuery = "SELECT COUNT(1) FROM Warehouse WHERE IdWarehouse = @IdWarehouse";
            int warehouseCount;
            using (var cmd = new SqlCommand(warehouseExistsQuery, connection))
            {
                cmd.Parameters.AddWithValue("@IdWarehouse", request.WarehouseId);
                warehouseCount = (int)await cmd.ExecuteScalarAsync();
            }

            if (warehouseCount == 0)
            {
                throw new WarehouseIdException();
            }

            var getOrderQuery = @"
            SELECT IdOrder, CreatedAt
            FROM [Order]
            WHERE IdProduct = @ProductId
            AND CreatedAt < @RequestCreatedAt
            AND NOT EXISTS (
                SELECT 1 FROM Product_Warehouse WHERE IdOrder = [Order].IdOrder
            )";
            int orderId;
            DateTime orderCreatedAt;
            using (var cmd = new SqlCommand(getOrderQuery, connection))
            {
                cmd.Parameters.AddWithValue("@ProductId", request.ProductId);
                cmd.Parameters.AddWithValue("@RequestCreatedAt", request.CreatedAt);
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (reader.Read())
                    {
                        orderId = reader.GetInt32(0);
                        orderCreatedAt = reader.GetDateTime(1);
                    }
                    else
                    {
                        throw new NoSuitableOrderforProductException();
                    }
                }
            }

            var updateOrderQuery = "UPDATE [Order] SET FulfilledAt = @CurrentTime WHERE IdOrder = @OrderId";
            using (var cmd = new SqlCommand(updateOrderQuery, connection))
            {
                cmd.Parameters.AddWithValue("@CurrentTime", DateTime.Now);
                cmd.Parameters.AddWithValue("@OrderId", orderId);
                await cmd.ExecuteNonQueryAsync();
            }

            try
            {
                var addProductToWarehouseQuery = @"
        INSERT INTO Product_Warehouse (IdWarehouse, IdProduct, IdOrder, Amount, CreatedAt, Price)
        VALUES (@WarehouseId, @ProductId, @IdOrder, @Amount, @CreatedAt, @Price);
        SELECT SCOPE_IDENTITY();";
                decimal price;
                using (var cmd = new SqlCommand(addProductToWarehouseQuery, connection))
                {
                    cmd.Parameters.AddWithValue("@WarehouseId", request.WarehouseId);
                    cmd.Parameters.AddWithValue("@ProductId", request.ProductId);
                    cmd.Parameters.AddWithValue("@Amount", request.Amount);
                    cmd.Parameters.AddWithValue("@CreatedAt", DateTime.Now);
                    var getPriceQuery = "SELECT Price FROM Product WHERE IdProduct = @ProductId";
                    using (var getPriceCommand = new SqlCommand(getPriceQuery, connection))
                    {
                        getPriceCommand.Parameters.AddWithValue("@ProductId", request.ProductId);
                        price = Convert.ToDecimal(await getPriceCommand.ExecuteScalarAsync());
                        cmd.Parameters.AddWithValue("@Price", price * request.Amount);
                    }

                    cmd.Parameters.AddWithValue("@IdOrder", orderId);
                    var insertedPrimaryKey = await cmd.ExecuteScalarAsync();
                    return Ok(
                        $"Inserted successfully! Primary Key: {insertedPrimaryKey}, Order Created At: {orderCreatedAt}");
                }
            }
            catch(Exception)
            {
                throw new InsertException();
            }

        }
    }
}
