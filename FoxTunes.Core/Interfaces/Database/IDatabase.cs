﻿using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace FoxTunes.Interfaces
{
    public interface IDatabase : IStandardComponent, IDisposable
    {
        IDatabaseSet<T> GetSet<T>() where T : class;

        IDatabaseQuery<T> GetQuery<T>() where T : class;

        void Interlocked(Action action);

        void Interlocked(Action action, TimeSpan timeout);

        int SaveChanges();
    }
}
