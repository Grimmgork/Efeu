using Efeu.Integration.Persistence;
using Efeu.Runtime;
using Efeu.Runtime.Value;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Integration.Foreign
{
    public interface IEfeuEngine
    {
        public Task SendMessageAsync(EfeuMessage message);
    }
}
