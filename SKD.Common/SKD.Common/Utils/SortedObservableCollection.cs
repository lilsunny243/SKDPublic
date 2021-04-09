using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using SKD.Common.Models;
using SKD.Common.ViewModels;
using Xamarin.Forms;

namespace SKD.Common.Utils
{
    public class SortedObservableCollection<TViewModel, TModel, TKey> : ObservableCollection<TViewModel> where TViewModel : IndexedCardViewModel<TModel> where TModel : BaseModel
    {
        private readonly Func<TViewModel, TKey> _keySelector;
        private readonly bool _descending;
        public SortedObservableCollection(Func<TViewModel, TKey> keySelector, bool descending = false) : base() => (_keySelector, _descending) = (keySelector, descending);

        public new void Add(TViewModel vm)
        {
            if (Count > 0)
                base.InsertItem(GetNewIndex(vm), vm);
            else
                base.Add(vm);
            UpdateIndices();
        }

        private int GetNewIndex(TViewModel vm)
        {
            var comparer = Comparer<TKey>.Default;
            for (int i = 0; i < Count; i++)
            {
                var comparison = comparer.Compare(_keySelector(vm), _keySelector(this[i]));
                if (comparison > 0 && _descending || comparison < 0 && !_descending) 
                    return i;
            }
            return Count;
        }

        public void UpdateItem(TViewModel vm, TModel newModel)
        {
            var comparer = EqualityComparer<TKey>.Default;
            var oldKey = _keySelector(vm);
            vm.Update(newModel);
            var newKey = _keySelector(vm);
            if (Count > 0 && !comparer.Equals(oldKey, newKey))
            {
                base.Remove(vm);
                Add(vm);
            }
        }

        new public void Remove(TViewModel vm)
        {
            base.Remove(vm);
            UpdateIndices();
        }

        private void UpdateIndices() => this.Iteri((vm, i) => (vm.Index, vm.NotLast) = (i, i < Count - 1));

    }
}
