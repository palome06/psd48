using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using Mono.Data.Sqlite;

namespace PSD.Base.Utils
{
    public class ReadonlySQL
    {
        private string dbConnection;
        /// <summary>
        /// Single Param Constructor for specifying the DB file.
        /// </summary>
        /// <param name="table">The File containing the DB</param>
        public ReadonlySQL(string table)
        {
            dbConnection = string.Format("Data Source={0}", table);
        }
        /// <summary>
        /// Allows programmer to run a query against the Database.
        /// </summary>
        /// <param name="sql">The SQL to run</param>
        /// <returns>A DataTable containing the result set.</returns>
        public DataTable GetDataTable(string sql)
        {
            DataTable dt = new DataTable();
            SqliteConnection cnn = new SqliteConnection(dbConnection);
            cnn.Open();
            SqliteCommand cmd = new SqliteCommand(cnn) { CommandText = sql };
            SqliteDataReader reader = cmd.ExecuteReader();
            dt.Load(reader);
            reader.Close();
            cnn.Close();
            return dt;
        }
        private int ExecuteNonQuery(string sql)
        {
            SqliteConnection cnn = new SqliteConnection(dbConnection);
            cnn.Open();
            SqliteCommand cmd = new SqliteCommand(cnn) { CommandText = sql };
            int rowUpdated = cmd.ExecuteNonQuery();
            cnn.Clone();
            return rowUpdated;
        }
        /// <summary>
        /// Allows the programmer to retrieve single items from the DB.
        /// </summary>
        /// <param name="sql">The query to run.</param>
        /// <returns>A string.</returns>
        public string ExecuteScalar(string sql)
        {
            SqliteConnection cnn = new SqliteConnection(dbConnection);
            cnn.Open();
            SqliteCommand cmd = new SqliteCommand(cnn) { CommandText = sql };
            object value = cmd.ExecuteScalar();
            if (value != null)
                return value.ToString();
            else
                return "";
        }

        public DataRowCollection Query(List<string> queries, string table)
        {
            string query = "select " + string.Join(", ", queries
                .Select(p => p + " \"" + p + "\"")) + "from " + table + ";";
            DataTable recipe = GetDataTable(query);
            return recipe.Rows;
        }
        // query with condition
        public DataRowCollection Query(IEnumerable<string> queries, string table, string cond)
        {
            string query = "select " + string.Join(", ", queries.Select(p => p + " \"" +
                p + "\"")) + "from " + table + " where " + cond + ";";
            return GetDataTable(query).Rows;
        }

        public void Insert(Dictionary<string, int> iData,
            Dictionary<string, string> sData, string table)
        {
            string columns = "", values = "";
            foreach (var pair in iData)
            {
                columns += string.Format(" {0},", pair.Key.ToString());
                values += string.Format(" {0},", pair.Value);
            }
            foreach (var pair in sData)
            {
                columns += string.Format(" {0},", pair.Key.ToString());
                values += string.Format(" '{0}',", pair.Value);
            }
            columns = columns.Substring(0, columns.Length - 1);
            values = values.Substring(0, values.Length - 1);
            ExecuteNonQuery(string.Format("INSERT INTO {0}({1}) VALUES({2});", table, columns, values));
        }
    }
}
