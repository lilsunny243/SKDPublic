using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Plugin.CloudFirestore;
using Plugin.FirebaseAuth;
using Plugin.FirebaseStorage;
using Xamarin.Forms;
using Xamarin.Forms.Internals;

namespace SKD.Common.ViewModels
{
    public class BaseViewModel : INotifyPropertyChanged
    {
        private static IAuth? _auth;
        protected static IAuth Auth => _auth ??= CrossFirebaseAuth.Current.Instance;

        private static IFirestore? _firestore;
        protected static IFirestore Firestore => _firestore ??= CrossCloudFirestore.Current.Instance;

        private static IStorage? _storage;
        protected static IStorage Storage => _storage ??= CrossFirebaseStorage.Current.Instance;

        private static INavigation? _navigation;
        protected static INavigation Navigation => _navigation ??= Shell.Current.Navigation;


        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        protected Action RaisePropertyChanged(params string[] names) => () => names.ForEach(x => OnPropertyChanged(x));

        protected bool SetProperty<T>(ref T backingStore, T value, Action? onChanged = null,
            Predicate<T>? onlyIf = null, [CallerMemberName] string propertyName = "")
        {
            if (EqualityComparer<T>.Default.Equals(backingStore, value))
                return false;

            if (onlyIf?.Invoke(value) ?? true)
            {
                backingStore = value;
                onChanged?.Invoke();
            }
            OnPropertyChanged(propertyName);
            return true;
        }

    }

    public class PageViewModel : BaseViewModel
    {

        private string title = string.Empty;
        public string Title { get => title; set => SetProperty(ref title, value); }
    }
}
