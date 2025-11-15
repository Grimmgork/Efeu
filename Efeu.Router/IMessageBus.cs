using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Router
{
    internal interface IMessageBus
    {
        public Task OutboxAsync(object message);
    }
}
