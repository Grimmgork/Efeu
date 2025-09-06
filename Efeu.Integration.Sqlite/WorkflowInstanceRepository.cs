using Efeu.Integration.Persistence;
using Efeu.Integration.Entities;
using Efeu.Integration.Model;
using LinqToDB;
using LinqToDB.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Efeu.Integration.Sqlite
{
    internal class WorkflowInstanceRepository : IWorkflowInstanceRepository
    {
        private readonly SqliteDataConnection connection;

        public WorkflowInstanceRepository(SqliteDataConnection connection)
        {
            this.connection = connection;
        }

        public Task<int> Add(WorkflowInstanceEntity instance)
        {
            return connection.InsertWithInt32IdentityAsync(instance);
        }

        public async Task<IEnumerable<WorkflowInstanceEntity>> GetAllActiveAsync()
        {
             return await connection.GetTable<WorkflowInstanceEntity>()
                .Where(i => i.State != Runtime.WorkflowRuntimeState.Done)
                .ToArrayAsync();
        }

        public async Task<WorkflowInstanceEntity?> GetForProcessing(int id, WorkflowExecutionState[] allowedExecutionStates, WorkflowExecutionState newExecutionState)
        {
            int numberOfUpdateRows = await connection.GetTable<WorkflowInstanceEntity>()
                .Where(i => allowedExecutionStates.Contains(i.ExecutionState) && i.IsProcessing == false)
                .Set(p => p.ExecutionState, newExecutionState)
                .Set(p => p.IsProcessing, true)
                .UpdateAsync();

            if (numberOfUpdateRows == 0)
            {
                return null;
            }
            else
            {
                return await connection.GetTable<WorkflowInstanceEntity>()
                    .FirstAsync(i => i.Id == id);
            }
        }

        public Task<WorkflowInstanceEntity> GetByIdAsync(int id)
        {
            return connection.GetTable<WorkflowInstanceEntity>()
                .FirstAsync(i => i.Id == id);
        }

        public Task Update(WorkflowInstanceEntity instance)
        {
            return connection.UpdateAsync(instance);
        }

        public async Task<bool> Delete(int id, WorkflowExecutionState[] allowedExecutionStates)
        {
            int numberOfUpdatedRows = await connection.GetTable<WorkflowInstanceEntity>()
                .Where(i => i.Id == id && allowedExecutionStates.Contains(i.ExecutionState))
                .DeleteAsync();

            return numberOfUpdatedRows != 0;
        }
    }
}
