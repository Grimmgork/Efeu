using Efeu.Integration.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Integration.Data
{
    public  interface IWorkflowInstanceRepository
    {
        public Task GetById();

        public Task GetAllActive();

        public Task<int> Add(WorkflowInstanceEntity instance);
    }
}
