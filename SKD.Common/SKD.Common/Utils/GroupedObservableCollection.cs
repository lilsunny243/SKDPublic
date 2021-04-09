using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Xamarin.Forms.Internals;

namespace SKD.Common.Utils
{
    // Inspired by https://github.com/mikegoatly/GroupedObservableCollection
    public class GroupedObservableCollection<TKey, TElement> : List<Grouping<TKey, TElement>>
    {
        public readonly Func<TElement, TKey> GroupKeySelector;
        public readonly Predicate<TElement> PrioritySelector;

        private readonly List<TElement> filterRejects = new List<TElement>();
        private readonly Predicate<TElement> _filter;

        public void RefreshFilter()
        {
            var itemsToRetreive = filterRejects.Where(x => _filter?.Invoke(x) ?? true);
            var itemsToReject = Elements.Where(x => !(_filter?.Invoke(x) ?? true));
            foreach (TElement item in itemsToRetreive.ToArray())
            {
                filterRejects.Remove(item);
                Add(item, false);
            }
            foreach (TElement item in itemsToReject.ToArray())
            {
                Remove(item, false);
                filterRejects.Add(item);
            }
        }

        public GroupedObservableCollection(Func<TElement, TKey> groupKeySelector,
            Predicate<TElement> filter,
            Predicate<TElement> prioritySelector,
            params (TKey key, string name)[] keysAndNames)
        {
            GroupKeySelector = groupKeySelector;
            _filter = filter;
            PrioritySelector = prioritySelector;
            keysAndNames.ForEach(x => Add(new Grouping<TKey, TElement>(x.key, x.name)));
        }

        public void Add(TElement item, bool filter = true)
        {
            if ((filter && (_filter?.Invoke(item) ?? true)) || !filter)
            {
                var key = GroupKeySelector(item);
                var group = FindGroup(key);
                if (PrioritySelector(item))
                {
                    var priorityCount = group.Count(x => PrioritySelector(x));
                    group.Insert(priorityCount, item);
                }
                else
                    FindGroup(key).Add(item);
            }
            else
                filterRejects.Add(item);
        }

        public bool Remove(TElement item, bool filter = true)
        {
            if ((filter && (_filter?.Invoke(item) ?? true)) || !filter)
            {
                var key = GroupKeySelector(item);
                var group = FindGroup(key);
                return group.Remove(item);
            }
            else
                return filterRejects.Remove(item);
        }

        public void UpdatePriority(TElement item)
        {
            if (_filter?.Invoke(item) ?? true)
            {
                var key = GroupKeySelector(item);
                var group = FindGroup(key);
                var priority = PrioritySelector(item);
                var itemIndex = group.IndexOf(item);
                group.Move(itemIndex, priority ? 0 : group.Count - 1);
            }
        }

        public void MoveGroup(TElement item, TKey oldKey)
        {
            if (_filter?.Invoke(item) ?? true)
            {
                var oldGroup = FindGroup(oldKey);
                oldGroup.Remove(item);
                Add(item);
            }
        }

        new public void Clear()
        {
            ForEach(x => x.Clear());
            filterRejects.Clear();
        }

        private Grouping<TKey, TElement> FindGroup(TKey key) => this.SingleOrDefault(x => x.Key.Equals(key));

        public IEnumerable<TKey> Keys => this.Select(x => x.Key);
        public IEnumerable<TElement> Elements => this.SelectMany(x => x);

    }

    public class Grouping<TKey, TElement> : ObservableCollection<TElement>, IGrouping<TKey, TElement>
    {
        public Grouping(TKey key, string name) => (Key, Name) = (key, name);

        public TKey Key { get; }
        public string Name { get; }
    }
}
