using System;
using System.Collections.ObjectModel;
using System.Linq;
using Plugin.CloudFirestore;
using SKD.Common.Models;
using SKD.Common.Utils;
using SKD.Common.ViewModels;
using Xamarin.Forms.Internals;

namespace SKD.Common.Services
{
    public static class FirestoreCollectionService
    {
        public static IListenerRegistration Subscribe<TModel, TViewModel>(IQuery query, 
            ObservableCollection<TViewModel> viewModels, Func<TModel,TViewModel> getViewModel, Action? afterSnapshotProcessed = null, Predicate<TViewModel>? oneTimeFilter = null)
            where TModel : BaseModel
            where TViewModel : CardViewModel<TModel>
        {
            viewModels.Clear();
            return query.AddSnapshotListener((snapshot, ex) =>
            {
                if (snapshot is null)
                    viewModels.Clear();
                else if (viewModels.Any())
                {
                    foreach (var change in snapshot.DocumentChanges)
                    {
                        var doc = change.Document;
                        if (change.Type == DocumentChangeType.Added)
                        {
                            var vm = getViewModel(doc.ToObject<TModel>()!);
                            if(oneTimeFilter?.Invoke(vm) ?? true)
                                viewModels.Add(vm);
                        }
                        else
                        {
                            var vm = viewModels.SingleOrDefault(x => x.UID == doc.Id);
                            if (vm is null) return;
                            if (change.Type == DocumentChangeType.Modified)
                                vm.Update(change.Document.ToObject<TModel>()!);
                            else
                                viewModels.Remove(vm);
                        }
                    }
                }
                else if (!snapshot.IsEmpty)
                    snapshot.Documents.Select(x => getViewModel(x.ToObject<TModel>()!))
                    .Where(x => oneTimeFilter?.Invoke(x) ?? true)
                    .ForEach(viewModels.Add);
                afterSnapshotProcessed?.Invoke();
            });
        }

        public static IListenerRegistration Subscribe<TModel, TViewModel, TKey>(IQuery query, 
            SortedObservableCollection<TViewModel, TModel, TKey> viewModels,
            Func<TModel, TViewModel> getViewModel,
            Action? afterSnapshotProcessed = null)
            where TModel : BaseModel
            where TViewModel : IndexedCardViewModel<TModel>
        {
            viewModels.Clear();
            return query.AddSnapshotListener((snapshot, ex) =>
            {
                if (snapshot is null)
                    viewModels.Clear();
                else if (viewModels.Any())
                {
                    foreach (var change in snapshot.DocumentChanges)
                    {
                        var doc = change.Document;
                        if (change.Type == DocumentChangeType.Added)
                            viewModels.Add(getViewModel(doc.ToObject<TModel>()!));
                        else
                        {
                            var vm = viewModels.SingleOrDefault(x => x.UID == doc.Id);
                            if (vm is null) return;
                            if (change.Type == DocumentChangeType.Modified)
                                viewModels.UpdateItem(vm, change.Document.ToObject<TModel>()!);
                            else
                                viewModels.Remove(vm);
                        }
                    }
                }
                else if (!snapshot.IsEmpty)
                    snapshot.Documents.ForEach(x => viewModels.Add(getViewModel(x.ToObject<TModel>()!)));
                afterSnapshotProcessed?.Invoke();
            });
        }

        public static IListenerRegistration Subscribe<TModel, TViewModel, TKey>(IQuery query,
            GroupedObservableCollection<TKey, TViewModel> viewModels, 
            Func<TModel, TViewModel> getViewModel,
            Action? afterSnapshotProcessed = null)
           where TModel : BaseModel
           where TViewModel : CardViewModel<TModel>
        {
            viewModels.Clear();
            return query.AddSnapshotListener((snapshot, ex) =>
            {
                if (snapshot is null)
                    viewModels.Clear();
                else if (viewModels.Elements.Any())
                {
                    foreach (var change in snapshot.DocumentChanges)
                    {
                        var doc = change.Document;
                        if (change.Type == DocumentChangeType.Added)
                            viewModels.Add(getViewModel(doc.ToObject<TModel>()!));
                        else
                        {
                            var vm = viewModels.Elements.SingleOrDefault(x => x.UID == doc.Id);
                            if (vm is null) return;
                            if (change.Type == DocumentChangeType.Modified)
                            {
                                var oldKey = viewModels.GroupKeySelector(vm);
                                var oldPriority = viewModels.PrioritySelector(vm);
                                vm.Update(change.Document.ToObject<TModel>()!);
                                var newKey = viewModels.GroupKeySelector(vm)!;
                                if (!newKey.Equals(oldKey))
                                    viewModels.MoveGroup(vm, oldKey);
                                else if (oldPriority != viewModels.PrioritySelector(vm))
                                    viewModels.UpdatePriority(vm);
                            }
                            else
                                viewModels.Remove(vm);
                        }
                    }
                }
                else if (!snapshot.IsEmpty)
                    snapshot.Documents.ForEach(x => viewModels.Add(getViewModel(x.ToObject<TModel>()!)));
                afterSnapshotProcessed?.Invoke();
            });
        }

    }



}
