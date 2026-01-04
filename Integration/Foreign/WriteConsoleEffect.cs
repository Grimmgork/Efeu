using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;

namespace Efeu.Integration.Foreign
{
    internal class WriteConsoleEffect : IEffect
    {
        private readonly IEfeuEngine efeu;

        public WriteConsoleEffect(IEfeuEngine efeu)
        {
            this.efeu = efeu;
        }

        public async Task RunAsync(EffectExecutionContext context, CancellationToken token)
        {
            await using DbTransaction transaction = await efeu.UnitOfWork.Connection.BeginTransactionAsync();
            Console.WriteLine(context.Input);

            await efeu.UnitOfWork.DoAsync(transaction, () => 
                efeu.SendSignalAsync(new Router.EfeuMessage(), DateTime.Now, "asdf"));

            await context.CompleteAsync();
            await transaction.CommitAsync();
        }
    }
}
