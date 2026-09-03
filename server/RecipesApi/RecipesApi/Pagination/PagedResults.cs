namespace RecipesApi.Pagination
{
    public class PagedResults<T>
    {
        public IEnumerable<T> Results { get;}
        public int TotalCount { get; }
        public int CurrentPage { get; }
        public int PageSize { get; }
        public int TotalPages { get; }

        public PagedResults(IEnumerable<T> results, int totalCount, int currentPage, int pageSize)
        {
            Results = results;
            TotalCount = totalCount;
            CurrentPage = currentPage;
            PageSize = pageSize;
            TotalPages = totalCount > 0 && pageSize > 0 ?
                (int)Math.Ceiling(totalCount / (double)pageSize) : 1;
        }
    }
}
