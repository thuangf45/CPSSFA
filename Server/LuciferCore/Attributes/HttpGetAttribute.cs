namespace LuciferCore.Attributes
{
    public class HttpGetAttribute : RouteAttribute
    {
        public HttpGetAttribute(string path) : base("GET", path) { }
    }
}
