using Efeu.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Integration.Commands;

public interface ITriggerCommands
{
    public Task DeleteStaticAsync(Guid behaviourVersionId);

    public Task DeleteAsync(Guid[] ids);
}
