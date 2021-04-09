namespace SKD.Common.ViewModels
{
    public class SwitchButtonViewModel<T> : BaseViewModel
    {
        private bool isSelected;

        public SwitchButtonViewModel(string text, T param, bool isSelected = false)
            => (Text, Param, IsSelected) = (text, param, isSelected);

        public string Text { get; }
        public T Param { get; }
        public bool IsSelected { get => isSelected; set => SetProperty(ref isSelected, value); }
    }
}