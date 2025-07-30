using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Efeu.Integration
{
    public interface IEfeuMigration
    {
        public int Version => int.Parse(Regex.Match(this.GetType().Name, "^_([0-9]+)_").Groups[1].Value);

        public Task Up();

        public Task Down();
    }
}
