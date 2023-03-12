namespace Movies.Api;

public static class Endpoints
{
    private const string ApiBase = "api";
    private const string Id = "{id:guid}";
    private const string IdOrSlug = "{idOrSlug}";
    
    public static class Movies
    {
        private const string Base = ApiBase + "/movies/";
        private const string ById = Base + Id;
        private const string ByIdOrSlug = Base + IdOrSlug;

        public const string Get = ByIdOrSlug;
        public const string GetAll = Base;
        public const string Create = Base;
        public const string Update = ById;
        public const string Delete = ById;
        
        public const string Rate = ById + "/ratings";
        public const string DeleteRating = ById + "/ratings";
    }
    
    public static class Ratings
    {
        private const string Base = ApiBase + "/ratings";

        public const string GetUserRatings = Base + "/me";
    }
}
