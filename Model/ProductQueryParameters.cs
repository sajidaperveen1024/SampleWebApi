namespace SampleWebApi.Model
{
    public class ProductQueryParameters
    {
        private const int MaxPageSize = 50;
        private int _pageSize = 10;

        public int Page { get; set; } = 1;

        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = (value > MaxPageSize) ? MaxPageSize : value;
        }

        public string? Name { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public string? SortBy { get; set; } = "Name";
        public bool Desc { get; set; } = false;
    }
}
