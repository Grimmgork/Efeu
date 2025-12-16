using Efeu.Router.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Router.Script
{
    public class EfeuScriptScope
    {
        public readonly EfeuScriptScope? Parent;

        private readonly IDictionary<string, Func<EfeuValue>> Constants = new Dictionary<string, Func<EfeuValue>>();
        
        public EfeuScriptScope(EfeuScriptScope parent)
        {
            Parent = parent;
        }

        public EfeuScriptScope()
        {

        }

        public void Assign(string name, Func<EfeuValue> value)
        {
            Constants.Add(name, value);
        }

        public void Assign(string name, EfeuValue value)
        {
            Constants.Add(name, () => value);
        }

        public Func<EfeuValue> Get(string name)
        {
            EfeuScriptScope? scope = this;
            while (scope != null && !scope.Constants.ContainsKey(name))
            {
                scope = scope.Parent;
            }

            if (scope == null)
                return () => default;
            else
                return scope.Constants.GetValueOrDefault(name, () => default);
        }

    }
}
