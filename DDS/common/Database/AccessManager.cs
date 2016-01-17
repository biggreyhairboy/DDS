using System;
using System.Collections.Generic;
using System.Text;
using System.Data.OleDb;
using System.Data;
using OMS.common.Utilities;

namespace OMS.common.Database
{
    public class AccessManager : IDisposable
    {
        protected const string CONNECTIONSTRING = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source={0};User ID=Admin;Persist Security Info=False";
        protected OleDbConnection conn;
        protected OleDbDataAdapter adapter;
        protected OleDbCommandBuilder commandBuilder;
        protected OleDbCommand command;
        protected OleDbTransaction transaction;
        protected bool isConnectionReady;
        protected bool isDisposed;
        protected bool hasTransaction;
        protected string mdbFolder;

        public AccessManager(string mdbFolder)
        {
            this.mdbFolder = mdbFolder;
            isConnectionReady = false;
            isDisposed = false;
            hasTransaction = false;
        }

        public AccessManager(string mdbFolder, bool hasTransaction)
            : this(mdbFolder)
        {
            this.hasTransaction = hasTransaction;
        }

        public bool IsConnectionReady { get { return isConnectionReady && !isDisposed; } }

        public bool IsDisposed { get { return isDisposed; } }

        public bool Update(DataTable table)
        {
            if (!IsConnectionReady) return false;
            if (table == null || table.TableName == null || table.TableName.Trim() == "") return false;
            try
            {
                DataRow[] updatedRows = table.Select("", "", DataViewRowState.ModifiedOriginal);
                DataRow[] addedRows = table.Select("", "", DataViewRowState.Added);
                List<DataColumn> keys = new List<DataColumn>();
                foreach (DataColumn column in table.DefaultView.Table.PrimaryKey)
                {
                    keys.Add(column);
                }

                foreach (DataRow row in updatedRows)
                {
                    StringBuilder sb = new StringBuilder();
                    StringBuilder conditions = new StringBuilder();

                    sb.Append(string.Format("Update {0} set ", table.TableName));
                    foreach (DataColumn column in table.Columns)
                    {
                        if (keys.Contains(column))
                        {
                            if (conditions.Length == 0)
                                conditions.Append(" where ");
                            conditions.Append(string.Format("{0}='{1}',", column.ColumnName, row[column]));
                            continue;
                        }
                        sb.Append(string.Format("{0}='{1}',", column.ColumnName, row[column]));
                    }
                    sb.Remove(sb.Length - 1, 1);
                    if (conditions.Length > 0)
                        conditions.Remove(conditions.Length - 1, 1);
                    sb.Append(conditions.ToString());
                    adapter.UpdateCommand = new OleDbCommand(sb.ToString());
                    adapter.Update(new DataRow[] { row });
                }

                foreach (DataRow row in addedRows)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append(string.Format("Insert into {0} values(", table.TableName));
                    foreach (DataColumn column in table.Columns)
                    {
                        sb.Append(string.Format("'{0}',", row[column]));
                    }
                    sb.Remove(sb.Length - 1, 1);
                    sb.Append(")");
                    adapter.InsertCommand = new OleDbCommand(sb.ToString());
                    adapter.Update(new DataRow[] { row });
                }

                return true;
            }
            catch (Exception ex)
            {
                TLog.DefaultInstance.WriteLog(ex.ToString());
            }
            return false;
        }

        public int Execute(string sql)
        {
            if (IsConnectionReady)
            {
                command.CommandText = sql;
                command.CommandType = CommandType.Text;
                return command.ExecuteNonQuery();
            }
            else return -1;
        }

        public object GetObject(string sql)
        {
            if (IsConnectionReady)
            {
                command.CommandType = CommandType.Text;
                command.CommandText = sql;

                return command.ExecuteScalar();
            }
            else return null;
        }

        public string GetValue(string sql)
        {
            if (IsConnectionReady)
            {
                command.CommandText = sql;
                command.CommandType = CommandType.Text;
                object obj = command.ExecuteScalar();
                if (obj == null || obj == System.DBNull.Value) return null;
                else return obj.ToString().Trim();
            }
            else return null;
        }

        public DataSet GetDataSet(string sql)
        {
            if (IsConnectionReady)
            {
                DataSet ds = new DataSet();
                command.CommandType = CommandType.Text;
                command.CommandText = sql;
                if (adapter == null) adapter = new OleDbDataAdapter();
                adapter.SelectCommand = command;
                adapter.Fill(ds);

                return ds;
            }
            else return null;
        }

        public DataSet GetDataSet(string sql, string tableName)
        {
            if (IsConnectionReady)
            {
                command.CommandText = sql;
                command.CommandType = CommandType.Text;
                if (adapter == null) adapter = new OleDbDataAdapter();
                adapter.SelectCommand = command;
                DataSet ds = new DataSet();
                adapter.Fill(ds, tableName);

                return ds;
            }
            else return null;
        }

        public void Open()
        {
            try
            {
                if (conn != null && conn.State == System.Data.ConnectionState.Open) return;
                if (mdbFolder == null || mdbFolder.Trim() == "") return;
                string test = System.IO.Path.Combine(mdbFolder, "DailyReport.mdb");
                if (!System.IO.File.Exists(test)) return;
                conn = new OleDbConnection();
                conn.ConnectionString = string.Format(CONNECTIONSTRING, test);
                conn.Open();
                command = new OleDbCommand();
                command.Connection = conn;
                command.CommandType = CommandType.Text;
                adapter = new OleDbDataAdapter(command);
                adapter.MissingSchemaAction = MissingSchemaAction.AddWithKey;
                commandBuilder = new OleDbCommandBuilder(adapter);
                if (hasTransaction)
                {
                    transaction = conn.BeginTransaction();
                    command.Transaction = transaction;
                }
                isConnectionReady = true;
            }
            catch (Exception ex)
            {
                TLog.DefaultInstance.WriteLog(ex.ToString());
            }
        }

        public void Close()
        {
            try
            {
                if (conn != null && conn.State != System.Data.ConnectionState.Closed)
                {
                    conn.Close();
                    conn = null;
                }

                if (command != null)
                {
                    command.Dispose();
                    command = null;
                }

                if (adapter != null)
                {
                    adapter.Dispose();
                    adapter = null;
                }

                if (commandBuilder != null)
                {
                    commandBuilder.Dispose();
                    commandBuilder = null;
                }

                if (transaction != null)
                {
                    transaction.Dispose();
                    transaction = null;
                }
            }
            catch (Exception ex)
            {
                TLog.DefaultInstance.WriteLog(ex.ToString());
            }
        }

        public void CommitAndClose()
        {
            CommitTransaction();
            Close();
        }

        public void RollbackAndClose()
        {
            RollbackTransaction();
            Close();
        }

        public void CommitTransaction()
        {
            if (transaction != null)
            {
                transaction.Commit();
                transaction = null;
            }
        }

        public void RollbackTransaction()
        {
            if (transaction != null)
            {
                transaction.Rollback();
                transaction = null;
            }
        }

        public void BeginTransaction()
        {
            if (transaction == null && conn != null)
            {
                transaction = conn.BeginTransaction();
            }
        }

        public static string SpecialChar(string value)
        {
            if (string.IsNullOrEmpty(value)) return string.Empty;
            else return value.Replace(@"'", @"''");
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                Close();
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
            isDisposed = true;
        }

        #endregion
    }
}
