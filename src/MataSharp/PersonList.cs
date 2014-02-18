using System.Linq;
using System.Collections.Generic;

namespace MataSharp
{
    /// <summary>
    /// Custom made list to store MagisterPerson instances.
    /// </summary>
    public class PersonList : IList<MagisterPerson>
    {
        private Mata Mata;
        private List<MagisterPerson> List;

        public PersonList(Mata mata)
        {
            this.List = new List<MagisterPerson>();
            this.Mata = mata;
        }

        public PersonList(int startSize, Mata mata)
        {
            this.List = new List<MagisterPerson>(startSize);
            this.Mata = mata;
        }

        public PersonList(IEnumerable<MagisterPerson> collection, Mata mata)
        {
            this.List = new List<MagisterPerson>(collection);
            this.Mata = mata;
        }

        public PersonList(IEnumerable<string> collection, Mata mata)
        {
            this.List = new List<MagisterPerson>(collection.Count());
            this.Mata = mata;
            this.AddRange(collection);
        }

        internal PersonList(Mata mata, IEnumerable<MagisterStylePerson> collection, bool readOnly, bool download)
        {
            this.IsReadOnly = readOnly;
            this.List = new List<MagisterPerson>(collection.Count());
            this.Mata = mata;
            this.AddRange(collection, download);
        }

        public void Add(MagisterPerson item)
        {
            if (this.IsReadOnly) ThrowException();
            this.List.Add(item);
        }

        public void Add(string name)
        {
            if (this.IsReadOnly) ThrowException();
            this.List.Add(this.Mata.GetPersons(name)[0]);
        }

        internal void Add(MagisterStylePerson item, bool download)
        {
            this.List.Add(item.ToPerson(download));
        }

        public void AddRange(IEnumerable<MagisterPerson> collection)
        {
            if (this.IsReadOnly) ThrowException();
            this.List.AddRange(collection);
        }

        public void AddRange(IEnumerable<string> collection)
        {
            if (this.IsReadOnly) ThrowException();
            this.List.AddRange(collection.ConvertAll(x => this.Mata.GetPersons(x)[0]));
        }

        public void AddRange(string name)
        {
            if (this.IsReadOnly) ThrowException();
            this.List.AddRange(this.Mata.GetPersons(name));
        }

        internal void AddRange(IEnumerable<MagisterStylePerson> collection, bool download)
        {
            this.List.AddRange(collection.ConvertAll(p => p.ToPerson(download)));
        }

        public void Clear()
        {
            this.List.Clear();
        }

        public bool Contains(MagisterPerson item)
        {
            return this.List.Contains(item);
        }

        public void CopyTo(MagisterPerson[] array, int arrayIndex)
        {
            this.List.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return this.List.Count; }
        }

        public void InsertRange(int index, IEnumerable<MagisterPerson> collection)
        {
            if (this.IsReadOnly) ThrowException();
            this.List.InsertRange(index, collection);
        }

        public void InsertRange(int index, IEnumerable<string> collection)
        {
            if (this.IsReadOnly) ThrowException();
            this.List.InsertRange(index, collection.ConvertAll(x => (MagisterPerson)this.Mata.GetPersons(x)[0]));
        }

        public void InsertRange(int index, string name)
        {
            if (this.IsReadOnly) ThrowException();
            this.List.InsertRange(index, (IEnumerable<MagisterPerson>)this.Mata.GetPersons(name));
        }

        internal void InsertRange(int index, IEnumerable<MagisterStylePerson> collection, bool download)
        {
            this.List.InsertRange(index, collection.ConvertAll(p => (MagisterPerson)p.ToPerson(download)));
        }

        public bool IsReadOnly { get; private set; }

        public bool Remove(MagisterPerson item)
        {
            if (this.IsReadOnly) ThrowException();
            return this.List.Remove(item);
        }

        public void RemoveRange(int index, int count)
        {
            if (this.IsReadOnly) ThrowException();
            this.List.RemoveRange(index, count);
        }

        public void RemoveAt(int index)
        {
            if (this.IsReadOnly) ThrowException();
            this.List.RemoveAt(index);
        }

        public int RemoveAll(System.Predicate<MagisterPerson> predicate)
        {
            if (this.IsReadOnly) ThrowException();
            return this.List.RemoveAll(predicate);
        }

        public void TrimExcess()
        {
            this.List.TrimExcess();
        }

        public bool TrueForAll(System.Predicate<MagisterPerson> predicate)
        {
            return this.List.TrueForAll(predicate);
        }

        public IEnumerator<MagisterPerson> GetEnumerator()
        {
            return this.List.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public int IndexOf(MagisterPerson item)
        {
            return this.List.IndexOf(item);
        }

        public void Insert(int index, MagisterPerson item)
        {
            if (this.IsReadOnly) ThrowException();
            this.List.Insert(index, item);
        }

        public MagisterPerson this[int index]
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

        private static void ThrowException() { throw new System.Data.ReadOnlyException("This list is ReadOnly!"); }
    }
}