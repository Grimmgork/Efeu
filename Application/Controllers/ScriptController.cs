using Efeu.Runtime.Value;
using Efeu.Runtime.Script;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Efeu.Application.Controllers
{
    [Route("Script")]
    public class ScriptController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [Route("Run")]
        public EfeuValue Run(string script)
        {
            return EfeuScript.Run(script, new EfeuScriptScope());
        }
    }
}
