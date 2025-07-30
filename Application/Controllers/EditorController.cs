using Efeu.Runtime;
using Efeu.Runtime.Data;
using Efeu.Runtime.Json;
using Efeu.Runtime.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Converters;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Efeu.Application.Controllers
{
    public class EditorController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [Route("ImportFile")]
        public IActionResult ImportFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return this.BadRequest();

            JsonSerializerOptions options = new JsonSerializerOptions();
            options.Converters.Add(new SomeDataJsonConverter());
            options.Converters.Add(new JsonStringEnumConverter());

            try
            {
                WorkflowDefinition definition = JsonSerializer.Deserialize<WorkflowDefinition>(file.OpenReadStream(), options);

                return PartialView("Editor", definition);
            }
            catch (Exception)
            {
                return BadRequest();
            }
        }
    }
}
