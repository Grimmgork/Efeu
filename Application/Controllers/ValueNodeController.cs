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
        string[] hashes = await valueNodeCommands.WriteAsync([value]);
        return Ok(hashes[0]);
    }

    [HttpPut]
    [Route("/Cleanup")]
    public async Task<ActionResult> Cleanup()
    {
        await valueNodeCommands.CleanupAsync();
        return Ok();
    }

    [HttpGet]
    [Route("{hash}")]
    public async Task<ActionResult> Get(string hash)
    {
        EfeuValue[] values = await valueNodeCommands.ReadAsync([hash]);
        JsonElement json = JsonSerializer.SerializeToElement(values[0], jsonOptions.JsonSerializerOptions);
        return Ok(json);
    }
}