using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Efeu.Router.Data
{
    public class EfeuObject
    {
        public string TypeName => this.GetType().Name;

        public virtual EfeuValue Call(string field)
        {
            return EfeuValue.Nil();
        }

        public virtual EfeuValue Call(string field, EfeuValue value)
        {
            throw new NotImplementedException();
        }

        public virtual EfeuValue Call(int index, EfeuValue value)
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

        public virtual int Length()
        {
            throw new NotImplementedException();
        }

        public virtual EfeuValue Last()
        {
            throw new NotImplementedException();
        }

        public virtual EfeuValue First()
        {
            throw new NotImplementedException();
        }

        public virtual IEnumerable<EfeuValue> Each()
        {
            throw new NotImplementedException();
        }

        public virtual IEnumerable<EfeuValue> Times()
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

        public virtual string ToString()
        {
            return $"<{TypeName}>";
        }

        public virtual EfeuValue ToValue()
        {
            return new EfeuValue(this);
        }

        public virtual IEnumerable<KeyValuePair<string, EfeuValue>> Fields()
        {
            return [];
        }

        public virtual EfeuValue Push(params EfeuValue[] items)
        {
            return new EfeuArray([this, ..items]);
        }

        public virtual EfeuValue Add(EfeuValue value)
        {
            throw new NotImplementedException();
        }

        public virtual EfeuValue Subtract(EfeuValue value)
        {
            throw new NotImplementedException();
        }

        public virtual EfeuValue Multiply(EfeuValue value)
        {
            throw new NotImplementedException();
        }

        public virtual EfeuValue Divide(EfeuValue value)
        {
            throw new NotImplementedException();
        }

        public virtual EfeuValue Modulo(EfeuValue value)
        {
            throw new NotImplementedException();
        }

        public virtual EfeuValue GreaterThan(EfeuValue value)
        {
            throw new NotImplementedException();
        }

        public virtual EfeuValue LessThan(EfeuValue value)
        {
            throw new NotImplementedException();
        }

        public virtual EfeuValue Eval()
        {
            throw new NotImplementedException();
        }
    }
}
