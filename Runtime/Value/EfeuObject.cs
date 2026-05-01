using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Efeu.Runtime.Value
{
    public abstract class EfeuObject : IEquatable<EfeuValue>
    {
        public string TypeName => this.GetType().Name;

        public override string ToString()
        {
            return $"<{TypeName}>";
        }

        public virtual void WriteReference(IEfeuReferenceHasher hasher)
        {
            throw new NotImplementedException();
        }

        public virtual decimal AsDecimal()
        {
            throw new NotImplementedException();
        }

        public virtual double AsDouble()
        {
            throw new NotImplementedException();
        }

        public virtual bool AsBoolean()
        {
            throw new NotImplementedException();
        }

        public virtual long AsLong()
        {
            throw new NotImplementedException();
        }

        public virtual bool Equals(EfeuValue value)
        {
            return this.Equals((object)value.AsObject());
        }
    }
}
