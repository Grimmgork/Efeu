using Efeu.Router.Value;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Integration.Foreign
{
    public class EfeuTriggerContext
    {
        public readonly Guid Id;

        public readonly EfeuValue Input;

        public readonly DateTimeOffset CreatedAt;

        public readonly bool IsStartup;

        public EfeuValue Data;
    }
}
