﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Integration.Data
{
    public interface IUnitOfWork
    {
        public Task ExecuteAsync(Func<Task> action);
    }
}
