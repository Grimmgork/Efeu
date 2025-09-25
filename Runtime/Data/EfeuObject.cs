using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Runtime.Data
{
    public abstract class EfeuObject
    {
        public string Name => this.GetType().Name;

        public virtual EfeuValue Call(string field)
        {
            return EfeuValue.Nil();
        }

        public virtual void Call(string field, EfeuValue value)
        {
            throw new NotImplementedException();
        }

        public virtual void Call(int index, EfeuValue value)
        {
            throw new NotImplementedException();
        }

        public virtual EfeuValue Call(int index)
        {
            return EfeuValue.Nil();
        }

        public virtual EfeuValue Increment()
        {
            return ToLong() + 1;
        }

        public virtual EfeuValue Decrement()
        {
            return ToLong() - 1;
        }

        public virtual EfeuValue Last()
        {
            throw new NotImplementedException();
        }

        public virtual EfeuValue First()
        {
            throw new NotImplementedException();
        }

        public virtual void Push(EfeuValue item)
        {
            throw new NotImplementedException();
        }

        public virtual EfeuValue Shift()
        {
            throw new NotImplementedException();
        }

        public virtual void Unshift()
        {
            throw new NotImplementedException();
        }

        public virtual void Unshift(params EfeuValue[] values)
        {
            throw new NotImplementedException();
        }

        public virtual EfeuValue Pop()
        {
            throw new NotImplementedException();
        }

        public virtual long ToLong()
        {
            throw new NotImplementedException();
        }

        public virtual bool ToBoolean()
        {
            return ToLong() == 0;
        }

        public virtual decimal ToDecimal()
        {
            return ToLong();
        }

        public virtual double ToDouble()
        {
            return ToLong();
        }

        public virtual IEnumerable<KeyValuePair<string, EfeuValue>> Fields()
        {
            return [];
        }

        public virtual EfeuValue Clone()
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return $"<{Name}>";
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public EfeuValue Eval()
        {
            throw new NotImplementedException();
        }
    }
}
