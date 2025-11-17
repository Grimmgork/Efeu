using Efeu.Integration.Persistence;
using Efeu.Integration.Entities;
using Efeu.Runtime.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Efeu.Router;

namespace Efeu.Integration.Commands
{
    internal class BehaviourDefinitionCommands : IBehaviourDefinitionCommands
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IBehaviourDefinitionRepository definitionRepository;

        public BehaviourDefinitionCommands(IUnitOfWork unitOfWork, IBehaviourDefinitionRepository repository)
        {
            this.unitOfWork = unitOfWork;
            this.definitionRepository = repository;
        }

        public Task<int> CreateAsync(BehaviourDefinitionEntity definition)
        {
            return definitionRepository.CreateAsync(definition);
        }

        public Task Delete(int id)
        {
            return definitionRepository.DeleteAsync(id);
        }
    }
}
