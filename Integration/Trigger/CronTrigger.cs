using Efeu.Integration.Commands;
using Efeu.Integration.Logic;
using Efeu.Runtime.Data;
using Efeu.Runtime.Model;
using Efeu.Runtime.Trigger;
using Quartz;
using System;
using System.Threading.Tasks;

namespace Efeu.Integration.Trigger
{

    internal class WorkflowCronElapsedJob : IJob
    {
        private readonly IWorkflowTriggerCommands triggerCommands;
        public WorkflowCronElapsedJob(IWorkflowTriggerCommands triggerCommands)
        {
            this.triggerCommands = triggerCommands;
        }

        public Task Execute(IJobExecutionContext context)
        {
            triggerCommands.SendSignal(new ChronElapsedSignal()
            {
                Id = context.MergedJobDataMap.GetString("")
            });
        }
    }

    public class CronTrigger : IWorkflowTrigger
    {
        private readonly IScheduler scheduler;

        public CronTrigger(IScheduler scheduler)
        { 
            this.scheduler = scheduler;
        }

        public async Task AttachAsync(WorkflowTriggerContext context)
        {
            Guid guid = Guid.NewGuid();
            context.Data = SomeData.String(guid.ToString());
            context.Hash = new CHrn

            IJobDetail job = JobBuilder.Create<WorkflowCronElapsedJob>()
                .UsingJobData("TaskName", "Beta")
                .Build();

            ITrigger trigger = TriggerBuilder.Create()
                .WithIdentity(guid.ToString())
                .StartNow()
                .WithSimpleSchedule(x => x.WithIntervalInSeconds(15).RepeatForever())
                .Build();

            await scheduler.ScheduleJob(job, trigger);
        }

        public async Task DetachAsync(WorkflowTriggerContext context)
        {
            Guid guid = new Guid(context.Data.ToString());

            await scheduler.UnscheduleJob(new TriggerKey(guid.ToString()));
        }

        public async Task OnStartupAsync(WorkflowTriggerContext context)
        {
            Guid guid = new Guid(context.Data.ToString());

            IJobDetail job = JobBuilder.Create<WorkflowCronElapsedJob>()
                .UsingJobData("Hash", guid)
                .Build();

            ITrigger trigger = TriggerBuilder.Create()
                .WithIdentity("trigger2")
                .StartNow()
                .WithSimpleSchedule(x => x.WithIntervalInSeconds(15).RepeatForever())
                .Build();

            await scheduler.ScheduleJob(job, trigger);
        }
    }
}
