namespace LuciferCore.Attributes
{
    public class HttpPutAttribute : RouteAttribute
    {
        public HttpPutAttribute(string path) : base("PUT", path) { }
    }
}
