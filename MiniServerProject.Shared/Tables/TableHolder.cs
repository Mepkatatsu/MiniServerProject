namespace MiniServerProject.Shared.Tables
{
    public static class TableHolder
    {
        private static Dictionary<Type, ITable> _tables = new();

        public static TTable GetTable<TTable>() where TTable : class, ITable, new()
        {
            Type type = typeof(TTable);

            if (_tables.TryGetValue(type, out ITable? table))
                return (TTable)table;

            // TODO: 테이블 목록 읽어오고 초기화하는 방식 개선 필요
            table = new TTable();
            table.Initialize();
            _tables.Add(type, table);

            return (TTable)table;
        }
    }
}
