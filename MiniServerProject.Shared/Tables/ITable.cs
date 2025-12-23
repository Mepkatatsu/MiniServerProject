namespace MiniServerProject.Shared.Tables
{
    public interface ITable
    {
        public void Initialize();
    }

    public interface ITable<TKey, TData> : ITable where TKey : notnull
    {
        TData? Get(TKey key);
    }
}
