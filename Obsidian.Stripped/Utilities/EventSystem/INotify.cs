namespace Obsidian.Stripped.Utilities.EventSystem;

public interface INotify<T>
{
    public void AddListener(Action<T> listener);
}
