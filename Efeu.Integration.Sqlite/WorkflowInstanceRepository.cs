﻿using Efeu.Integration.Data;
using Efeu.Integration.Model;
using LinqToDB;
using LinqToDB.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Integration.Sqlite
{
    internal class WorkflowInstanceRepository : IWorkflowInstanceRepository
    {
        private readonly DataConnection connection;

        public WorkflowInstanceRepository(DataConnection connection)
        {
            this.connection = connection;
        }

        public Task<int> Add(WorkflowInstanceEntity instance)
        {
            return connection.InsertWithInt32IdentityAsync(instance);
        }

        public async Task<IEnumerable<WorkflowInstanceEntity>> GetAllActive()
        {
             return await connection.GetTable<WorkflowInstanceEntity>()
                .Where(i => i.State != Runtime.WorkflowInstanceState.Done)
                .ToArrayAsync();
        }

        public Task<WorkflowInstanceEntity> GetById(int id)
        {
            return connection.GetTable<WorkflowInstanceEntity>()
                .FirstAsync(i => i.Id == id);
        }
    }
}
