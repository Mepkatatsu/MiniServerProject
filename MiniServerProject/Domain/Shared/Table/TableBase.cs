using MiniServerProject.Domain.Table;

namespace MiniServerProject.Domain.Shared.Table
{
    public abstract class TableBase<TKey, TData> : ITable<TKey, TData> where TKey : notnull
    {
        protected readonly Dictionary<TKey, TData> datas = new();

        public abstract void Initialize();

        public TData? Get(TKey key)
        {
            if (!datas.TryGetValue(key, out TData? data))
                return default(TData);

            return data;
        }
    }
}
