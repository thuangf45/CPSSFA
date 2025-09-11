namespace LuciferCore.Attributes
{
    public class HttpOptionsAttribute : RouteAttribute
    {
        public HttpOptionsAttribute(string path) : base("OPTIONS", path) { }
    }
}
