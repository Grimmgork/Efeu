using Efeu.Runtime.Data;
using Microsoft.AspNetCore.Mvc;
using System;

namespace Efeu.Application.Controllers
{
    [Route("WorkflowDefinitionSession")]
    public class WorkflowDefinitionSessionController : Controller
    {
        [HttpPost]
        [Route("{workflowDefinitionId}")]
        public IActionResult Open(int workflowDefinitionId)
        {
            // create session if none exists
            return View();
        }

        [HttpDelete]
        [Route("{sessionId}")]
        public IActionResult Close(string sessionsId)
        {
            // delete open session
            return View();
        }

        public int AddActionNode(string name, int x, int y)
        {
            throw new NotImplementedException();
        }

        public void AddInputNode(int id, string field, DataTraversal traversal)
        {

        }

        public void AddInputNode(int id, int index, DataTraversal traversal)
        {

        }

        public void RemoveInputNode()
        {

        }

        public void UpdateInputNode()
        {

        }

        public void DeleteInputNode()
        {

        }
    }
}
