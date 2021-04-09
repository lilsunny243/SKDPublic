using SKD.Common.Models;

namespace SKD.Common.ViewModels
{
    public abstract class CardViewModel<TModel> : BaseViewModel where TModel : BaseModel
    {
        private TModel source;

        public TModel Source { get => source; set => SetProperty(ref source, value); }
        public string UID { get; }

        protected CardViewModel(TModel model) => (source, UID) = (model, model.UID);

        public virtual void Update(TModel model) => Source = model;
    }

    public abstract class IndexedCardViewModel<TModel> : CardViewModel<TModel> where TModel : BaseModel
    {
        private int index;
        private bool notLast;

        public int Index { get => index; set => SetProperty(ref index, value); }
        public bool NotLast { get => notLast; set => SetProperty(ref notLast, value); }

        protected IndexedCardViewModel(TModel model) : base(model) { }
    }

}
