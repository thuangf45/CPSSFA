namespace LuciferCore.Attributes
{
    public class HttpDeleteAttribute : RouteAttribute
    {
        public HttpDeleteAttribute(string path) : base("DELETE", path) { }
    }
}
