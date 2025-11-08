namespace EVCharging.Repositories.NganVHH.ModelExtensions
{
    public class SearchRequest
    {
        public int? CurrentPage { get; set; }
        public int? PageSize { get; set; }

    }

    public class SearchRequestDto : SearchRequest
    {
        public string? ModelName { get; set; }

        public string? Material { get; set; }

        //public decimal? Amount { get; set; }
    }
}
