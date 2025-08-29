namespace LuciferCore.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class RouteAttribute : Attribute
    {
        public string Method { get; }
        public string Path { get; }

        public RouteAttribute(string method, string path)
        {
            Method = method.ToUpper();
            Path = path.StartsWith("/") ? path : "/" + path;
        }
    }
}
