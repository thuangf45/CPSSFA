namespace LuciferCore.Attributes
{
    public class HttpPostAttribute : RouteAttribute
    {
        public HttpPostAttribute(string path) : base("POST", path) { }
    }
}
