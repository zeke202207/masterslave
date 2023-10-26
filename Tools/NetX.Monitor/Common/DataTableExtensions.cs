using NetX.MasterSDK;
using System.Data;
using System.Reflection;

namespace NetX.Monitor;

public static class DataTableExtensions
{
    public static DataTable ToDataTable<T>(this IEnumerable<T> items)
    {
        DataTable dataTable = new DataTable();

        PropertyInfo[] properties = typeof(T).GetProperties();
        foreach (PropertyInfo property in properties)
        {
            DataColumn column = new DataColumn();
            ColumnNameAttribute columnNameAttribute = property.GetCustomAttribute<ColumnNameAttribute>();
            if (columnNameAttribute != null)
            {
                column.Caption = property.Name;
                column.ColumnName = columnNameAttribute.Name;
            }
            else
            {
                //不配置，忽略丢弃
                column.ColumnName = property.Name;
                continue;
            }
            column.DataType = property.PropertyType;
            dataTable.Columns.Add(column);
        }

        foreach (T item in items)
        {
            DataRow row = dataTable.NewRow();
            foreach (DataColumn column in dataTable.Columns)
            {
                PropertyInfo property = typeof(T).GetProperty(column.Caption);
                row[column.ColumnName] = property.GetValue(item);
            }
            dataTable.Rows.Add(row);
        }

        return dataTable;
    }
}
