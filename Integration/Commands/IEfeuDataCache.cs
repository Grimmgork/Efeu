using Efeu.Runtime.Value;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Integration.Commands
{
    public interface IEfeuDataCache
    {
        public Task<EfeuValue> FetchAsync(EfeuReference hash);

        public void Add(EfeuValue value);
        
        public void Rollback();

        public Task FlushAsync();
    }

    public class EfeuDataCache : IEfeuDataCache
    {
        public void Add(EfeuValue value)
        {
            throw new NotImplementedException();
        }

        public Task<EfeuValue> FetchAsync(EfeuReference hash)
        {
            throw new NotImplementedException();
        }

        public Task FlushAsync()
        {
            throw new NotImplementedException();
        }

        public void Rollback()
        {
            throw new NotImplementedException();
        }
    }
}
