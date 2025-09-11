namespace LuciferCore.Attributes
{
    public class HttpHeadAttribute : RouteAttribute
    {
        public HttpHeadAttribute(string path) : base("HEAD", path) { }
    }
}
