using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Integration
{
    public interface IWorkflowDefinitionRepository
    {
        public void GetById();

        public void GetAll();
    }
}
