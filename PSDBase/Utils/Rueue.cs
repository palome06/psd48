using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSD.Base.Utils
{
    public class Rueue<T> : IEnumerable<T> 
    {
        private LinkedList<T> lilt;
        public Rueue()
        {
            lilt = new LinkedList<T>();
        }
        public Rueue(IEnumerable<T> collection)
        {
            lilt = new LinkedList<T>(collection);
        }

        public T Dequeue()
        {
            T obj = lilt.First.Value;
            lilt.RemoveFirst();
            return obj;
        }

        public T[] Dequeue(int count)
        {
            if (count > lilt.Count)
                count = lilt.Count;
            T[] result = new T[count];
            for (int i = 0; i < count; ++i)
            {
                T obj = lilt.First.Value;
                lilt.RemoveFirst();
                result[i] = obj;
            }
            return result;
        }

        public void Enqueue(T value)
        {
            lilt.AddLast(value);
        }

        public void Enqueue(IEnumerable<T> values)
        {
            foreach (T value in values)
                lilt.AddLast(value);
        }

        public void PushBack(T value)
        {
            lilt.AddFirst(value);
        }

        public void PushBack(IEnumerable<T> values)
        {
            LinkedList<T> ll = new LinkedList<T>(values);
            var ls = ll.Reverse();
            foreach (T value in ls)
                lilt.AddFirst(value);
        }

        public int Count { get { return lilt.Count; } }

        public T Watch() { return lilt.First.Value; }

        public T[] Watch(int count)
        {
            if (count > lilt.Count)
                count = lilt.Count;
            T[] result = new T[count];
            var node = lilt.First;
            for (int i = 0; i < count; ++i)
            {
                result[i] = node.Value;
                node = node.Next;
            }
            return result;
        }
        // TODO:DEBUG: Remove from piles should not be permitted...
        public void Remove(T value)
        {
            lilt.Remove(value);
        }
        public IEnumerable<T> Intersect(IEnumerable<T> card)
        {
            return lilt.Intersect(card);
        }
        //public override LinkedList<T>.Enumerator GetEnumerator()
        //{
        //    return lilt.GetEnumerator();
        //}

        public IEnumerator<T> GetEnumerator()
        {
            return lilt.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return lilt.GetEnumerator();
        }
    }
}
