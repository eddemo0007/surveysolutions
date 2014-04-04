﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace Main.DenormalizerStorage
{
    public class InMemoryReadSideRepositoryAccessor<TView> : IQueryableReadSideRepositoryReader<TView>, IQueryableReadSideRepositoryWriter<TView>
        where TView : class, IView
    {
        private readonly Dictionary<string, TView> repository;
        private object locker = new object();
        public InMemoryReadSideRepositoryAccessor()
        {
            this.repository = new Dictionary<string, TView>();
        }

        public int Count()
        {
            return this.repository.Count;
        }

        public TView GetById(string id)
        {
            if (!this.repository.ContainsKey(id))
            {
                return null;
            }

            return this.repository[id];
        }

        public TResult Query<TResult>(Func<IQueryable<TView>, TResult> query)
        {
            return query.Invoke(this.repository.Values.AsQueryable());
        }

        public IEnumerable<TView> QueryAll(Expression<Func<TView, bool>> query)
        {
            return 
            repository.Values.Where(query.Compile());
        }

        public void Remove(string id)
        {
            lock (locker)
            {
                this.repository.Remove(id);
            }
        }
        
        public void Store(TView view, string id)
        {
            lock (locker)
            {
                if (!this.repository.ContainsKey(id))
                {
                    this.repository.Add(id, view);
                }
                else
                {
                    this.repository[id] = view;
                }
            }
        }

        public void Clear()
        {
            this.repository.Clear();
        }
    }
}