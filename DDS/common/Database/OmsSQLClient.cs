using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;
using OMS.common.Utilities;
using System.Data;
using System.Threading;

namespace OMS.common.Database
{
    public class OmsSQLClient : IDisposable
    {
        public const string ALIAS = "OMSDATA";
        private static OmsSQLClient instance;

        private volatile object syncRoot = new object();
        protected string connectionString;
        protected int reconnectInterval;
        protected bool autoReconnect;
        protected bool timerRunning;
        protected Exception lastError;
        protected SqlConnection conn;
        protected SqlCommand changeCommand;
        protected SqlCommand selectCommand;
        protected SqlTransaction changeTransaction;
        protected SqlDataAdapter selectAdapter;
        protected Timer reconnectTimer;
        protected int sqltimeout;
        protected string myAlias;

        public OmsSQLClient()
        {
            autoReconnect = false;
            timerRunning = false;
            reconnectInterval = 10000;
            reconnectTimer = new Timer(new TimerCallback(DoReconnect));
            reconnectTimer.Change(Timeout.Infinite, reconnectInterval);
            sqltimeout = 0;
        }

        public OmsSQLClient(string connectionString)
            : this()
        {
            ConnectionString = connectionString;
        }

        public OmsSQLClient(string connectionString, bool autoReconnect)
            : this(connectionString)
        {
            this.autoReconnect = autoReconnect;
        }

        public OmsSQLClient(string connectionString, bool autoReconnect, int reconnectInterval)
            : this(connectionString, autoReconnect)
        {
            this.reconnectInterval = reconnectInterval;
        }

        public string SQLAlias
        {
            get
            {
                if (myAlias == null || myAlias.Trim() == "") return ALIAS;
                return myAlias;
            }
            set
            {
                myAlias = value;
            }
        }

        public int SqlTimeout { get { return sqltimeout; } set { sqltimeout = value; } }
        /// <summary>
        /// Gets or sets the reconnect interval, in milliseconds, default 10,000
        /// </summary>
        public int ReconnectInterval
        {
            get { return reconnectInterval; }
            set
            {
                if (reconnectInterval != value)
                {
                    if (value > 0)
                    {
                        reconnectInterval = value;
                        if (timerRunning) reconnectTimer.Change(0, reconnectInterval);
                    }
                }
            }
        }
        /// <summary>
        /// Gets or sets the value to indicate whether or not auto reconnect database once connection failed, default FALSE
        /// </summary>
        public bool AutoReconnect
        {
            get { return autoReconnect; }
            set { autoReconnect = value; }
        }
        /// <summary>
        /// Gets the exception occurred last time
        /// </summary>
        public Exception LastError { get { return lastError; } }
        /// <summary>
        /// Gets the SQL connection establish using the Connection String, for <see cref="UpdateDataset"/> Insert/Delete/Update command only
        /// </summary>
        public SqlConnection Connection { get { return conn; } }

        protected void DoReconnect(object state)
        {
            try
            {
                if (conn == null)
                {
                    TLog.DefaultInstance.WriteLogWithDebugConsole("Null connection detected, reconnect failed");
                    StopReconnect();
                    return;
                }
                if (Reconnect() == 0)
                {
                    TLog.DefaultInstance.WriteLogWithDebugConsole("Reconnect to SqlClient successfully!");
                    StopReconnect();
                }
                else
                {
                    TLog.DefaultInstance.WriteLogWithDebugConsole("Reconnect to SqlClient failed");
                }
            }
            catch (Exception ex)
            {
                TLog.DefaultInstance.WriteLog(ex.ToString(), LogType.ERROR);
                lastError = ex;
            }
        }

        public static OmsSQLClient GetInstance()
        {
            return instance;
        }

        public static void SetInstance(string connectionString)
        {
            instance = new OmsSQLClient(connectionString);
        }

        public string ConnectionString
        {
            get { return connectionString; }
            set
            {
                if (connectionString != value)
                {
                    connectionString = value;
                    try
                    {
                        DisposeInternal();
                        //conn = new SqlConnection(connectionString);
                        //conn.Open();
                        conn = OmsDatabaseManager.Instance.SqlMatrix(SQLAlias, connectionString);
                        lastError = null;
                    }
                    catch (Exception ex)
                    {
                        TLog.DefaultInstance.WriteLog(ex.ToString(), LogType.ERROR);
                        lastError = ex;
                    }
                }
            }
        }

        protected void StartReconnect()
        {
            timerRunning = true;
            reconnectTimer.Change(0, reconnectInterval);
            TLog.DefaultInstance.WriteLogWithDebugConsole("Start reconnect to SqlClient");
        }

        protected void StopReconnect()
        {
            timerRunning = false;
            reconnectTimer.Change(Timeout.Infinite, reconnectInterval);
            TLog.DefaultInstance.WriteLogWithDebugConsole("Stop reconnect to SqlClient");
        }
        /// <summary>
        /// Gets the database connection status
        /// </summary>
        public ConnectionState DatabaseState
        {
            get
            {
                if (conn == null) return ConnectionState.Closed;
                try
                {
                    ConnectionState state = conn.State;
                    lastError = null;
                    return state;
                }
                catch (Exception ex)
                {
                    TLog.DefaultInstance.WriteLog(ex.ToString(), LogType.ERROR);
                    lastError = ex;
                }
                return ConnectionState.Closed;
            }
        }
        /// <summary>
        /// Reconnect to the specified database
        /// </summary>
        /// <returns>0 if no error, else return -1</returns>
        public int Reconnect()
        {
            try
            {
                if (conn != null)
                {
                    //If connection's OK, no need to reconnect
                    if ((conn.State != ConnectionState.Broken) && (conn.State != ConnectionState.Closed)) return 0;
                }
                else
                {
                    //conn = new SqlConnection(connectionString);
                    conn = OmsDatabaseManager.Instance.SqlMatrix(ALIAS, connectionString);
                }
                //conn.Open();
                lastError = null;
                return 0;
            }
            catch (Exception ex)
            {
                TLog.DefaultInstance.WriteLog(ex.ToString(), LogType.ERROR);
                lastError = ex;
            }
            return -1;
        }
        /// <summary>
        /// Update database using the specified SQL statement
        /// </summary>
        /// <param name="sql">SQL statement to update database</param>
        /// <returns>OmsSQLError</returns>
        public OmsSQLError UpdateInfo(string sql)
        {
            try
            {
                if ((DatabaseState & ConnectionState.Open) == 0)
                {
                    if (autoReconnect)
                    {
                        if (!timerRunning)
                        {
                            StartReconnect();
                        }
                    }
                    return OmsSQLError.seConnectionFailed;
                }
                if (changeCommand == null)
                    changeCommand = new SqlCommand();
                changeCommand.Connection = conn;
                changeCommand.CommandText = sql;
                changeCommand.CommandType = System.Data.CommandType.Text;
                if (sqltimeout > 0)
                {
                    changeCommand.CommandTimeout = sqltimeout;
                }
                changeTransaction = conn.BeginTransaction();                
                changeCommand.Transaction = changeTransaction;
                changeCommand.ExecuteNonQuery();
                changeCommand.Transaction.Commit();            
                lastError = null;
                return OmsSQLError.seOK;
            }
            catch (Exception ex)
            {
                if (changeCommand != null)
                {
                    if (changeCommand.Transaction != null)
                        changeCommand.Transaction.Rollback();
                }
                TLog.DefaultInstance.WriteLog(ex.ToString(), LogType.ERROR);
                lastError = ex;
                if ((DatabaseState & ConnectionState.Open) == 0)
                {
                    if (autoReconnect)
                    {
                        if (!timerRunning)
                        {
                            StartReconnect();
                        }
                    }
                    return OmsSQLError.seConnectionFailed;
                }
            }
            return OmsSQLError.seGenericFailed;
        }
        /// <summary>
        /// Query database using the specified SQL statement
        /// </summary>
        /// <param name="sql">SQL statement to query database</param>
        /// <param name="ds">User defined dataset for result</param>
        /// <returns>OmsSQLError</returns>
        public OmsSQLError Query(string sql, ref DataSet ds)
        {
            try
            {
                if ((DatabaseState & ConnectionState.Open) == 0)
                {
                    if (autoReconnect)
                    {
                        if (!timerRunning)
                        {
                            StartReconnect();
                        }
                    }
                    return OmsSQLError.seConnectionFailed;
                }
                if (selectCommand == null)
                    selectCommand = new SqlCommand();
                selectCommand.CommandText = sql;
                selectCommand.CommandType = CommandType.Text;
                selectCommand.Connection = conn;
                if (selectAdapter == null)
                    selectAdapter = new SqlDataAdapter();
                selectAdapter.SelectCommand = selectCommand;
                selectAdapter.Fill(ds);
                lastError = null;
                return OmsSQLError.seOK;
            }
            catch (Exception ex)
            {
                TLog.DefaultInstance.WriteLog(ex.ToString(), LogType.ERROR);
                lastError = ex;
                if ((DatabaseState & ConnectionState.Open) == 0)
                {
                    if (autoReconnect)
                    {
                        if (!timerRunning)
                        {
                            StartReconnect();
                        }
                    }
                    return OmsSQLError.seConnectionFailed;
                }
            }
            return OmsSQLError.seGenericFailed;
        }

        #region SQL Helper

        public int ExecuteNonQuery(CommandType commandType, string commandText)
        {
            return ExecuteNonQuery(commandType, commandText, (SqlParameter[])null);
        }

        public int ExecuteNonQuery(CommandType commandType, string commandText, params SqlParameter[] commandParameters)
        {
            lock (syncRoot)
                return SqlHelper.ExecuteNonQuery(conn, commandType, commandText, commandParameters);
        }

        public int ExecuteNonQuery(string spName, params object[] parameterValues)
        {
            lock (syncRoot)
                return SqlHelper.ExecuteNonQuery(conn, spName, parameterValues);
        }

        public DataSet ExecuteDataset(CommandType commandType, string commandText)
        {
            return ExecuteDataset(commandType, commandText, (SqlParameter[])null);
        }

        public DataSet ExecuteDataset(CommandType commandType, string commandText, params SqlParameter[] commandParameters)
        {
            lock (syncRoot)
                return SqlHelper.ExecuteDataset(conn, commandType, commandText, commandParameters);
        }

        public DataSet ExecuteDataset(string spName, params object[] parameterValues)
        {
            lock (syncRoot)
                return SqlHelper.ExecuteDataset(conn, spName, parameterValues);
        }

        public SqlDataReader ExecuteReader(CommandType commandType, string commandText)
        {
            return ExecuteReader(commandType, commandText, (SqlParameter[])null);
        }

        public SqlDataReader ExecuteReader(CommandType commandType, string commandText, params SqlParameter[] commandParameters)
        {
            lock (syncRoot)
                return SqlHelper.ExecuteReader(conn, commandType, commandText, commandParameters);
        }

        public SqlDataReader ExecuteReader(string spName, params object[] parameterValues)
        {
            lock (syncRoot)
                return SqlHelper.ExecuteReader(conn, spName, parameterValues);
        }

        public object ExecuteScalar(CommandType commandType, string commandText)
        {
            return ExecuteScalar(commandType, commandText, (SqlParameter[])null);
        }

        public object ExecuteScalar(CommandType commandType, string commandText, params SqlParameter[] commandParameters)
        {
            lock (syncRoot)
                return SqlHelper.ExecuteScalar(conn, commandType, commandText, commandParameters);
        }

        public object ExecuteScalar(string spName, params object[] parameterValues)
        {
            lock (syncRoot)
                return SqlHelper.ExecuteScalar(conn, spName, parameterValues);
        }

        public System.Xml.XmlReader ExecuteXmlReader(CommandType commandType, string commandText)
        {
            return ExecuteXmlReader(commandType, commandText, (SqlParameter[])null);
        }

        public System.Xml.XmlReader ExecuteXmlReader(CommandType commandType, string commandText, params SqlParameter[] commandParameters)
        {
            lock (syncRoot)
                return SqlHelper.ExecuteXmlReader(conn, commandType, commandText, commandParameters);
        }

        public System.Xml.XmlReader ExecuteXmlReader(string spName, params object[] parameterValues)
        {
            lock (syncRoot)
                return SqlHelper.ExecuteXmlReader(conn, spName, parameterValues);
        }

        public void FillDataset(CommandType commandType, string commandText, DataSet dataSet, string[] tableNames)
        {
            FillDataset(commandType, commandText, dataSet, tableNames, (SqlParameter[])null);
        }

        public void FillDataset(CommandType commandType, string commandText, DataSet dataSet, string[] tableNames, params SqlParameter[] commandParameters)
        {
            lock (syncRoot)
                SqlHelper.FillDataset(conn, commandType, commandText, dataSet, tableNames, commandParameters);
        }

        public void FillDataset(string spName, DataSet dataSet, string[] tableNames, params object[] parameterValues)
        {
            lock (syncRoot)
                SqlHelper.FillDataset(conn, spName, dataSet, tableNames, parameterValues);
        }

        public void UpdateDataset(SqlCommand insertCommand, SqlCommand deleteCommand, SqlCommand updateCommand, DataSet dataSet, string tableName)
        {
            lock (syncRoot)
                SqlHelper.UpdateDataset(insertCommand, deleteCommand, updateCommand, dataSet, tableName);
        }

        public SqlCommand CreateCommand(string spName, params string[] sourceColumns)
        {
            lock (syncRoot)
                return SqlHelper.CreateCommand(conn, spName, sourceColumns);
        }

        #endregion

        #region Bulk Copy

        public SqlBulkCopy GetBulkCopy()
        {
            return new SqlBulkCopy(conn);
        }

        public SqlBulkCopy GetBulkCopy(int batchSize)
        {
            SqlBulkCopy bulk = new SqlBulkCopy(conn);
            bulk.BatchSize = batchSize;
            return bulk;
        }

        public SqlBulkCopy GetBulkCopy(int batchSize, int timeout)
        {
            SqlBulkCopy bulk = new SqlBulkCopy(conn);
            bulk.BatchSize = batchSize;
            bulk.BulkCopyTimeout = timeout;
            return bulk;
        }

        public SqlBulkCopy GetBulkCopy(int batchSize, int timeout, string table)
        {
            SqlBulkCopy bulk = new SqlBulkCopy(conn);
            bulk.BatchSize = batchSize;
            bulk.BulkCopyTimeout = timeout;
            bulk.DestinationTableName = table;
            return bulk;
        }

        public void BulkUpdate(SqlBulkCopy bulk, bool closeAfterDone, DataRow[] rows)
        {
            if (bulk == null) return;
            if (rows.Length == 0) return;
            bulk.WriteToServer(rows);
            if (closeAfterDone) bulk.Close();
        }

        public void BulkUpdate(SqlBulkCopy bulk, bool closeAfterDone, DataTable table)
        {
            if (bulk == null || table == null) return;
            bulk.WriteToServer(table);
            if (closeAfterDone) bulk.Close();
        }

        public void BulkUpdate(SqlBulkCopy bulk, bool closeAfterDone, DataTable table, DataRowState state)
        {
            if (bulk == null || table == null) return;
            bulk.WriteToServer(table, state);
            if (closeAfterDone) bulk.Close();
        }

        #endregion

        private void DisposeInternal()
        {
            try
            {
                if (conn != null)
                {
                    if ((conn.State & ConnectionState.Open) != 0)
                    {
                        conn.Close();
                    }
                    conn.Dispose();
                }
                if (changeCommand != null)
                {
                    changeCommand.Dispose();
                    changeCommand = null;
                }
                if (selectCommand != null)
                {
                    selectCommand.Dispose();
                    selectCommand = null;
                }
                if (selectAdapter != null)
                {
                    selectAdapter.Dispose();
                    selectAdapter = null;
                }
                if (changeTransaction != null)
                {
                    changeTransaction.Dispose();
                    changeTransaction = null;
                }
                lastError = null;
            }
            catch (Exception ex)
            {
                TLog.DefaultInstance.WriteLog(ex.ToString(), LogType.ERROR);
                lastError = ex;
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            try
            {
                DisposeInternal();
            }
            catch (Exception ex)
            {
                TLog.DefaultInstance.WriteLog(ex.ToString(), LogType.ERROR);
                lastError = ex;
            }
        }

        #endregion
    }
}
