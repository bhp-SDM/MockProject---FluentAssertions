using System;
using System.Collections.Generic;
using System.Text;

namespace MockProject.Core.Interfaces
{
    public interface IRepository<K, T>
    {
        int Count { get; }
        void Add(T item);
        void Remove(T item);
        T GetByID(K id);
        List<T> GetAll();
        void Update(T item);
    }
}
