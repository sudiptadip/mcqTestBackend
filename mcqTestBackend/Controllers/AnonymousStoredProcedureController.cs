using mcqTestBackend.Data;
using mcqTestBackend.Model;
using mcqTestBackend.Utility;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Filters;
using System.Data;
using System.Net;
using System.Text.Json;

namespace FitnessApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AnonymousStoredProcedureController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _config;

        public AnonymousStoredProcedureController(ApplicationDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        [HttpPost("execute/{spName}/{mode}")]
        //  [SwaggerOperation(Summary = "Accepts any JSON body")]
        // [SwaggerRequestExample(typeof(object), typeof(DynamicJsonExample))]
        public async Task<IActionResult> ExecuteStoredProcedure([FromRoute] string spName, [FromRoute] int mode, [FromBody] JsonElement data)
        {
            var response = new ApiResponse();

            try
            {
                var anonymousSp = await _context.AnonymousSpPermissions.FirstOrDefaultAsync(u => u.SpName == spName);

                if (anonymousSp == null)
                {
                    response.StatusCode = HttpStatusCode.BadRequest;
                    response.IsSuccess = false;
                    response.ErrorMessage.Add("Not Allowed To Use");
                    return StatusCode(400, response);
                }


                using var conn = _context.Database.GetDbConnection();
                await conn.OpenAsync();

                using var cmd = conn.CreateCommand();
                cmd.CommandText = spName;
                cmd.CommandType = CommandType.StoredProcedure;

                // Input parameters
                cmd.Parameters.Add(new SqlParameter("@JsonInput", SqlDbType.NVarChar)
                {
                    Value = data.ToString()
                });

                cmd.Parameters.Add(new SqlParameter("@Mode", SqlDbType.Int)
                {
                    Value = mode
                });

                // Output parameter
                var outputParam = new SqlParameter("@output", SqlDbType.NVarChar, -1)
                {
                    Direction = ParameterDirection.Output
                };
                cmd.Parameters.Add(outputParam);

                await cmd.ExecuteNonQueryAsync();

                var jsonOutput = outputParam.Value?.ToString();

                if (!string.IsNullOrWhiteSpace(jsonOutput))
                {
                    var jsonElement = JsonSerializer.Deserialize<JsonElement>(jsonOutput);

                    // Parse core fields
                    var id = jsonElement.GetProperty("ID").GetInt32();
                    var statusCode = jsonElement.GetProperty("statusCode").GetInt32();
                    var result = jsonElement.GetProperty("response");

                    response.StatusCode = (HttpStatusCode)statusCode;
                    response.IsSuccess = id == 1;
                    response.Result = result;

                    if (id == 0)
                    {
                        // Optionally extract message for errors
                        if (jsonElement.TryGetProperty("response", out var errorMessage))
                        {
                            response.ErrorMessage.Add(errorMessage.ToString());
                        }

                        return StatusCode(statusCode, response);
                    }

                    return StatusCode(statusCode, response);
                }
                else
                {
                    response.StatusCode = HttpStatusCode.InternalServerError;
                    response.IsSuccess = false;
                    response.ErrorMessage.Add("Stored procedure did not return any output.");
                    return StatusCode(500, response);
                }
            }
            catch (Exception ex)
            {
                response.StatusCode = HttpStatusCode.InternalServerError;
                response.IsSuccess = false;
                response.ErrorMessage.Add(ex.Message);
                return StatusCode(500, response);
            }
        }
    }
}