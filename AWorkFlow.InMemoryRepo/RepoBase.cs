using System.Collections.Generic;

namespace AWorkFlow.InMemoryRepo
{
    public class RepoBase<T>
    {
        public List<T> Data = new List<T>();

        public void Insert(T data)
        {
            Data.Add(data);
        }
    }
}
