using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace WaterAssessment.ViewModel
{
    public abstract partial class PagedViewModelBase<TItem> : ObservableObject
    {
        private List<TItem> _allItems = new();

        protected PagedViewModelBase(int pageSize = 10)
        {
            PageSize = pageSize;
        }

        public ObservableCollection<TItem> PagedItems { get; } = new();

        public int PageSize { get; }

        [ObservableProperty]
        private int _totalItems;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CanGoNext))]
        [NotifyPropertyChangedFor(nameof(CanGoPrevious))]
        private int _totalPages = 1;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CanGoNext))]
        [NotifyPropertyChangedFor(nameof(CanGoPrevious))]
        private int _currentPage = 1;

        public bool CanGoPrevious => CurrentPage > 1;
        public bool CanGoNext => CurrentPage < TotalPages;

        [RelayCommand]
        private void PageChanged(int newPage)
        {
            if (newPage > 0 && newPage <= TotalPages)
            {
                CurrentPage = newPage;
                UpdatePagedItems();
            }
        }

        protected void SetItems(IEnumerable<TItem> items)
        {
            _allItems = items.ToList();
            TotalItems = _allItems.Count;
            TotalPages = Math.Max(1, (int)Math.Ceiling(TotalItems / (double)PageSize));

            if (CurrentPage > TotalPages)
            {
                CurrentPage = TotalPages;
            }

            UpdatePagedItems();
        }

        protected IReadOnlyList<TItem> GetAllItems() => _allItems;

        protected void UpdatePagedItems()
        {
            var pageItems = _allItems
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize);

            PagedItems.Clear();
            foreach (var item in pageItems)
            {
                PagedItems.Add(item);
            }
        }
    }
}
