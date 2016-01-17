using System;
using System.Data;
using System.Data.Odbc;
using System.Data.SqlClient;
using System.Collections.Generic;
using OMS.common.IO;
using OMS.common.Utilities;

namespace OMS.common.Database
{
    public class OmsDatabaseManager : IDisposable
    {
        private static volatile object syncRoot = new object();
        private static OmsDatabaseManager instance;

        private Dictionary<string, OdbcConnection> odbcList;
        private Dictionary<string, SqlConnection> sqlList;
        private Dictionary<string, string> redirectMap;

        private OmsDatabaseManager()
        {
            odbcList = new Dictionary<string, OdbcConnection>();
            sqlList = new Dictionary<string, SqlConnection>();
            redirectMap = new Dictionary<string, string>();
        }

        public OdbcConnection DBMatrix(string alias)
        {
            if (alias == null || alias.Trim() == "") return null;
            alias = alias.ToUpper();
            if (odbcList.ContainsKey(alias))
                return odbcList[alias];

            string aliasSection = "";
            if (redirectMap.ContainsKey(alias))
            {
                aliasSection = redirectMap[alias];
            }
            else return null;

            alias = OmsIni.Instance[aliasSection, "DSN_Source"];
            if (alias == "") return null;
            AddDataMap(alias, aliasSection);
            string dsnUser = OmsIni.Instance[aliasSection, "DSN_User"];
            string dsnPassword = OmsIni.Instance[aliasSection, "DSN_Password"];
            try
            {
                OdbcConnection conn = new OdbcConnection(string.Format("dsn={0};UID={1};PWD={2}", alias, dsnUser, dsnPassword));
                conn.Open();
                odbcList[alias.ToUpper()] = conn;
                return conn;
            }
            catch (Exception ex)
            {
                TLog.DefaultInstance.WriteLog(ex.ToString(), LogType.ERROR);
            }
            return null;
        }

        public OdbcConnection DBMatrix(string alias, string dsnSource, string dsnUser, string dsnPassword)
        {
            if (alias == null || alias.Trim() == "") return null;
            if (dsnSource == null || dsnSource.Trim() == "") return null;
            if (dsnUser == null || dsnUser.Trim() == "") return null;
            alias = alias.ToUpper();
            if (odbcList.ContainsKey(alias)) return odbcList[alias];
            try
            {
                OdbcConnection conn = new OdbcConnection(string.Format("dsn={0};UID={1};PWD={2}", dsnSource, dsnUser, dsnPassword));
                conn.Open();
                odbcList[alias] = conn;
                return conn;
            }
            catch (Exception ex)
            {
                TLog.DefaultInstance.WriteLog(ex.ToString(), LogType.ERROR);
            }
            return null;
        }

        public SqlConnection SqlMatrix(string alias, string connectionString)
        {
            if (alias == null || alias.Trim() == "") return null;
            alias = alias.ToUpper();
            if (sqlList.ContainsKey(alias)) return sqlList[alias];
            try
            {
                SqlConnection conn = new SqlConnection(connectionString);
                conn.Open();
                sqlList[alias] = conn;
                return conn;
            }
            catch (Exception ex)
            {
                TLog.DefaultInstance.WriteLog(ex.ToString(), LogType.ERROR);
            }
            return null;
        }

        public DataSet GetDataViaOdbc(string alias, string selectSql)
        {
            return GetOdbcData(alias, selectSql, CommandType.Text, null);
        }

        public DataSet GetDataViaSqlClient(string alias, string selectSql)
        {
            return GetSqlClientData(alias, selectSql, CommandType.Text, null);
        }
        /// <summary>
        /// Get data via ODBC stored procedure
        /// </summary>
        /// <param name="alias">Database alias</param>
        /// <param name="spName">
        /// Stored procedure name,please pass in this way <code>{CALL StoredProcName(?,?,?)}</code>, here "?" represents a stored procedure parameter placeholder
        /// <example>
        /// <para>example:</para>
        /// <para>OdbcParameter p = new OdbcParameter("@userid", "103");</para>
        /// <para>p.Direction = ParameterDirection.Input;</para>
        /// <para>DataSet ds = OmsDatabaseManager.Instance.GetDataViaStoredProc("omsdata", "{CALL sp_oms_queryUserFile_id(?)}", new OdbcParameter[] { p });</para>
        /// </example>
        /// </param>
        /// <param name="e">Stored procedure parameters</param>
        /// <returns>DataSet of the return value</returns>
        public DataSet GetDataViaOdbcStoredProc(string alias, string spName, OdbcParameter[] e)
        {
            return GetOdbcData(alias, spName, CommandType.StoredProcedure, e);
        }
        /// <summary>
        /// Gets data via ODBC or SqlClient, first will try ODBC, if fail then try SqlClient, if fail again then return NULL
        /// </summary>
        /// <param name="alias">Database alias</param>
        /// <param name="selectSql">selection SQL statement</param>
        /// <returns>Dataset of the result data, if NULL then means action failed</returns>
        public DataSet GetDataAmbiguous(string alias, string selectSql)
        {
            DataSet ds = GetDataViaOdbc(alias, selectSql);
            if (ds == null)
                ds = GetDataViaSqlClient(alias, selectSql);
            return ds;
        }

        private DataSet GetOdbcData(string alias, string sql, CommandType cmdType, OdbcParameter[] e)
        {
            try
            {
                lock (syncRoot)
                {
                    OdbcConnection conn = DBMatrix(alias);
                    if (conn != null)
                    {
                        OdbcCommand cmd = new OdbcCommand();
                        cmd.Connection = conn;
                        cmd.CommandText = sql;
                        cmd.CommandType = cmdType;

                        if (e != null)
                        {
                            for (int i = 0; i < e.Length; i++)
                            {
                                OdbcParameter p = cmd.Parameters.Add(e[i].ParameterName, e[i].OdbcType, e[i].Size);
                                p.Direction = e[i].Direction;
                                p.Value = e[i].Value;
                            }
                        }

                        OdbcDataAdapter da = new OdbcDataAdapter();
                        da.SelectCommand = cmd;
                        DataSet ds = new DataSet();
                        da.Fill(ds);
                        return ds;
                    }
                }
            }
            catch (Exception ex)
            {
                TLog.DefaultInstance.WriteLog(ex.ToString(), LogType.ERROR);
            }

            return null;
        }

        private DataSet GetSqlClientData(string alias, string sql, CommandType cmdType, SqlParameter[] e)
        {
            try
            {
                lock (syncRoot)
                {
                    SqlConnection conn = sqlList[alias.ToUpper()];
                    if (conn != null)
                    {
                        SqlCommand cmd = new SqlCommand();
                        cmd.Connection = conn;
                        cmd.CommandText = sql;
                        cmd.CommandType = cmdType;

                        if (e != null)
                        {
                            for (int i = 0; i < e.Length; i++)
                            {
                                SqlParameter p = cmd.Parameters.Add(e[i].ParameterName, e[i].SqlDbType, e[i].Size);
                                p.Direction = e[i].Direction;
                                p.Value = e[i].Value;
                            }
                        }

                        SqlDataAdapter da = new SqlDataAdapter();
                        da.SelectCommand = cmd;
                        DataSet ds = new DataSet();
                        da.Fill(ds);
                        return ds;
                    }
                }
            }
            catch (Exception ex)
            {
                TLog.DefaultInstance.WriteLog(ex.ToString(), LogType.ERROR);
            }

            return null;
        }

        public int UpdateDataViaOdbcStoredProc(string alias, string spName, OdbcParameter[] e)
        {
            return UpdateOdbcData(alias, spName, CommandType.StoredProcedure, e);
        }

        public int UpdateDataViaSqlClientStoredProc(string alias, string spName, SqlParameter[] e)
        {
            return UpdateSqlClientData(alias, spName, CommandType.StoredProcedure, e);
        }

        public int UpdateDataViaOdbc(string alias, string updateSql)
        {
            return UpdateOdbcData(alias, updateSql, CommandType.Text, null);
        }

        public int UpdateDataViaSqlClient(string alias, string selectSql)
        {
            return UpdateSqlClientData(alias, selectSql, CommandType.Text, null);
        }

        private int UpdateOdbcData(string alias, string sql, CommandType cmdType, OdbcParameter[] e)
        {
            try
            {
                lock (syncRoot)
                {
                    OdbcConnection conn = DBMatrix(alias);
                    if (conn != null)
                    {
                        OdbcCommand cmd = new OdbcCommand();
                        cmd.Connection = conn;
                        cmd.CommandType = cmdType;
                        cmd.CommandText = sql;

                        if (e != null)
                        {
                            for (int i = 0; i < e.Length; i++)
                            {
                                OdbcParameter p = cmd.Parameters.Add(e[i].ParameterName, e[i].OdbcType, e[i].Size);
                                p.Direction = e[i].Direction;
                                p.Value = e[i].Value;
                            }
                        }

                        return cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                TLog.DefaultInstance.WriteLog(ex.ToString(), LogType.ERROR);
            }
            return -1;
        }

        private int UpdateSqlClientData(string alias, string sql, CommandType cmdType, SqlParameter[] e)
        {
            try
            {
                lock (syncRoot)
                {
                    SqlConnection conn = sqlList[alias.ToUpper()];
                    if (conn != null)
                    {
                        SqlCommand cmd = new SqlCommand();
                        cmd.Connection = conn;
                        cmd.CommandType = cmdType;
                        cmd.CommandText = sql;

                        if (e != null)
                        {
                            for (int i = 0; i < e.Length; i++)
                            {
                                SqlParameter p = cmd.Parameters.Add(e[i].ParameterName, e[i].SqlDbType, e[i].Size);
                                p.Direction = e[i].Direction;
                                p.Value = e[i].Value;
                            }
                        }

                        return cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                TLog.DefaultInstance.WriteLog(ex.ToString(), LogType.ERROR);
            }
            return -1;
        }

        public void AddDataMap(string alias, string section)
        {
            if (alias == null || alias.Trim() == "") return;
            if (section == null || section.Trim() == "") return;
            redirectMap[alias.ToUpper()] = section;
        }

        public static OmsDatabaseManager Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                        {
                            instance = new OmsDatabaseManager();
                        }
                    }
                }
                return instance;
            }
        }
        #region IDisposable Members

        public void Dispose()
        {
            try
            {
                foreach (OdbcConnection conn in odbcList.Values)
                {
                    conn.Close();
                    conn.Dispose();
                }
                foreach (SqlConnection sqlConn in sqlList.Values)
                {
                    sqlConn.Close();
                    sqlConn.Dispose();
                }
            }
            catch (Exception ex)
            {
                TLog.DefaultInstance.WriteLog(ex.ToString(), LogType.ERROR);
            }
        }

        #endregion
    }
}