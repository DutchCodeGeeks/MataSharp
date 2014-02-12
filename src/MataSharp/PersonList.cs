﻿using System.Collections.Generic;

namespace MataSharp
{
    public class PersonList<T> : IList<T> where T : MagisterPerson
    {
        private Mata Mata;
        private List<T> List;

        public PersonList()
        {
            this.List = new List<T>();
            this.Mata = _Session.Mata;
        }

        public PersonList(Mata mata)
        {
            this.List = new List<T>();
            this.Mata = mata;
        }

        public PersonList(Mata mata, int startSize)
        {
            this.List = new List<T>(startSize);
            this.Mata = mata;
        }

        public PersonList(Mata mata, IEnumerable<T> collection)
        {
            this.List = new List<T>(collection);
            this.Mata = mata;
        }

        public PersonList(Mata mata, IEnumerable<string> collection)
        {
            this.List = new List<T>();
            this.Mata = mata;
            this.AddRange(collection);
        }

        internal PersonList(Mata mata, IEnumerable<MagisterStylePerson> collection, bool download)
        {
            this.List = new List<T>();
            this.Mata = mata;
            this.AddRange(collection, download);
        }

        public void Add(T item)
        {
            this.List.Add(item);
        }

        public void Add(string name)
        {
            this.List.Add((T)this.Mata.GetPersons(name)[0]);
        }

        internal void Add(MagisterStylePerson item, bool download)
        {
            this.List.Add((T)item.ToPerson(download));
        }

        public void AddRange(IEnumerable<T> collection)
        {
            this.List.AddRange(collection);
        }

        public void AddRange(IEnumerable<string> collection)
        {
            this.List.AddRange(collection.ConvertAll(x => (T)this.Mata.GetPersons(x)[0]));
        }

        internal void AddRange(IEnumerable<MagisterStylePerson> collection, bool download)
        {
            this.List.AddRange(collection.ConvertAll(p => (T)p.ToPerson(download)));
        }

        public void Clear()
        {
            this.List.Clear();
        }

        public bool Contains(T item)
        {
            return this.List.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            this.List.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return this.List.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(T item)
        {
            return this.List.Remove(item);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return this.List.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public int IndexOf(T item)
        {
            return this.List.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            this.List.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            this.List.RemoveAt(index);
        }

        public T this[int index]
        {
            get
            {
                return this.List[index];
            }
            set
            {
                this.List[index] = value;
            }
        }

        public void Sort()
        {
            this.List.Sort();
        }
    }
}