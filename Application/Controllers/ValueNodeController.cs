using System.Text.Json;
using System.Threading.Tasks;
using Efeu.Integration.Commands;
using Efeu.Runtime.Value;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Efeu.Application.Controllers;

[Route("ValueNode")]
public class ValueNodeController : Controller
{
    private readonly ValueNodeCommands valueNodeCommands;
    private readonly JsonOptions jsonOptions;
    
    public ValueNodeController(ValueNodeCommands valueNodeCommands, IOptions<JsonOptions> jsonOptions)
    {
        this.valueNodeCommands = valueNodeCommands;
        this.jsonOptions = jsonOptions.Value;
    }

    [HttpPut]
    public async Task<ActionResult> Add([FromBody] JsonElement json)
    {
        EfeuValue value = JsonSerializer.Deserialize<EfeuValue>(json, jsonOptions.JsonSerializerOptions)!;
        string hash = await valueNodeCommands.WriteAsync(value);
        return Ok(hash);
    }
}