namespace Movies.Api;

public static class Endpoints
{
    private const string ApiBase = "api";
    private const string Id = "{id:guid}";
    
    public static class Movies
    {
        private const string Base = ApiBase + "/movies/";
        private const string ById = Base + Id;

        public const string Create = Base;
        public const string GetAll = Base;
        public const string Get = ById;
        public const string Update = ById;
        public const string Delete = ById;
    }
}
