namespace Rebel.Framework
{
    public interface IQueue<T>
    {
        void Enqueue(T item);
        bool TryDequeue(out T ret);
    }
}