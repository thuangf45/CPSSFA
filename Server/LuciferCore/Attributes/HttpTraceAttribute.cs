namespace LuciferCore.Attributes
{
    public class HttpTraceAttribute : RouteAttribute
    {
        public HttpTraceAttribute(string path) : base("TRACE", path) { }
    }
}
