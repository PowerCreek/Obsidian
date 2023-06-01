namespace Obsidian.Stripped.Utilities.EventSystem;

public interface IDispatch<T>
{
    public void Dispatch(T args);
}
