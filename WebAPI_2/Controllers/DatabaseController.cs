using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Text.Json;

namespace WebAPI_2.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DatabaseController : ControllerBase
    {
        private readonly string _appSettingsPath = "appsettings.json";
        private readonly IConfiguration _configuration;
        private readonly Dictionary<int, string> _dbOptions = new()
        {
            { 1, "LocalDb" },
            { 2, "DockerDb" }
        };

        public DatabaseController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet("options")]
        public IActionResult GetDbOptions()
        {
            var active = _configuration.GetSection("ConnectionStrings")?["ActiveConnection"] ?? "LocalDb";
            var result = _dbOptions.Select(x => new
            {
                Number = x.Key,
                Name = x.Value,
                ConnectionString = _configuration.GetConnectionString(x.Value),
                IsActive = x.Value == active
            });
            return Ok(result);
        }

        [HttpPost("switch")]
        public async Task<IActionResult> SwitchDatabase([FromQuery] int number)
        {
            if (!_dbOptions.TryGetValue(number, out var connectionName))
                return BadRequest("Specify a valid database number (1 or 2). Use GET /api/database/options to see available options.");

            // Read appsettings.json
            var json = await System.IO.File.ReadAllTextAsync(_appSettingsPath);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement.Clone();

            if (!root.TryGetProperty("ConnectionStrings", out var connStrings) ||
                !connStrings.TryGetProperty(connectionName, out _))
            {
                return NotFound($"Connection string '{connectionName}' not found.");
            }

            // Replace ActiveConnection
            using var ms = new MemoryStream();
            using (var writer = new Utf8JsonWriter(ms, new JsonWriterOptions { Indented = true }))
            {
                writer.WriteStartObject();
                foreach (var prop in root.EnumerateObject())
                {
                    if (prop.Name == "ConnectionStrings")
                    {
                        writer.WritePropertyName("ConnectionStrings");
                        writer.WriteStartObject();
                        foreach (var cs in prop.Value.EnumerateObject())
                        {
                            if (cs.Name == "ActiveConnection")
                                writer.WriteString("ActiveConnection", connectionName);
                            else
                                cs.WriteTo(writer);
                        }
                        writer.WriteEndObject();
                    }
                    else
                    {
                        prop.WriteTo(writer);
                    }
                }
                writer.WriteEndObject();
            }
            await System.IO.File.WriteAllBytesAsync(_appSettingsPath, ms.ToArray());

            return Ok($"ActiveConnection switched to '{connectionName}'. Please restart the application for changes to take effect.");
        }
    }
}
