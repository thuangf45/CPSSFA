namespace LuciferCore.Interface
{
    public interface ICommand<T>
    {
        T Handle();
    }
}
