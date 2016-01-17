using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;
using System.Data;

namespace OMS.common.Utilities
{
    public class BulkInsertEventArgs : EventArgs
    {
        public string sourceFile;
        public string tableName;
        public int TotalCount;
        public int InsertedCount;

        public BulkInsertEventArgs(string srcfilename,string tablename,int totalcount,int InsertCount)
        {
            sourceFile = srcfilename;
            tableName = tablename;
            TotalCount = totalcount;
            InsertedCount = InsertCount;
        }
    }

    public class LogEventArgs : EventArgs
    {
        private string content;
        public LogEventArgs(string content)
        {
            this.content = content;
        }
        public string Content
        {
            get { return content; }
        }
    }

    public class BulkInsert
    {
        protected string connectionString;
        protected SqlConnection conn;
        protected SqlCommand Command;
        protected SqlDataAdapter Adapter;
        protected DataSet sourceData;
        protected Int32 DTotalCount;
        protected string sourceFilepath;
        protected string tableName;
        protected static BulkInsert instance;

        public Encoding encoding;
        public int BathSize;
        public int tablecount;
        public event EventHandler<BulkInsertEventArgs> OnBulkInsertStatusUpdate;
        public event EventHandler<LogEventArgs> OnLog;

        public BulkInsert(string connectionString)
        {
            DTotalCount = 0;
            if (BathSize.ToString().Trim()== ""||BathSize.ToString().Trim()=="0")
            {
                BathSize = 100;
            }
            ConnectionString = connectionString;
        }

        public static BulkInsert GetInstance()
        {
            return instance;
        }

        public static void SetInstance(string connectionString)
        {
            instance = new BulkInsert(connectionString);
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
                        conn = new SqlConnection(connectionString);
                        conn.Open();
                    }
                    catch (Exception ex)
                    {
                        TLog.DefaultInstance.WriteLog("Failed to connect database:"+connectionString +" "+ex.ToString(), LogType.ERROR);
                    }
                }
            }
        }

        public bool InsertDataToDB(string tablename, DataSet data)
        {
            try
            {
                DTotalCount = data.Tables[0].Rows.Count;
                if (OnBulkInsertStatusUpdate != null)
                    OnBulkInsertStatusUpdate(this, new BulkInsertEventArgs("", tableName, DTotalCount, 0));
                using (System.Data.SqlClient.SqlBulkCopy bcp = new System.Data.SqlClient.SqlBulkCopy(connectionString, SqlBulkCopyOptions.UseInternalTransaction))
                {
                    bcp.SqlRowsCopied += new System.Data.SqlClient.SqlRowsCopiedEventHandler(bcp_SqlRowsCopied);
                    bcp.BatchSize = BathSize;
                    bcp.NotifyAfter = BathSize;

                    tablecount = 0;
                    bcp.DestinationTableName = tablename;
                    DataTable dbtable = data.Tables[0].Clone();
                    for (int i = 0; i < data.Tables[0].Rows.Count; i++)
                    {
                        dbtable.ImportRow(data.Tables[0].Rows[i]);
                        if (i % BathSize == 0 && i > 0)
                        {
                            tablecount = i / BathSize;
                            bcp.BatchSize = BathSize;
                            bcp.NotifyAfter = BathSize;
                            InsertSqlBulk(dbtable, bcp);
                            dbtable.Clear();
                        }
                    }
                    if (dbtable.Rows.Count > 0)
                    {
                        tablecount += 1;
                        bcp.BatchSize = dbtable.Rows.Count;
                        bcp.NotifyAfter = dbtable.Rows.Count;
                        InsertSqlBulk(dbtable, bcp);
                        dbtable.Clear();
                    }
                    bcp.Close();
                    return true;
                }
            }
            catch (Exception ex)
            {
                TLog.DefaultInstance.WriteLog("Insert table error:" + tablename + " " + ex.ToString(), LogType.ERROR);
                return false;
            }
        }


        public bool InsertFileToDB(string tablename, string filePath)
        {
            try
            {
                string fileName = System.IO.Path.GetFileName(filePath);
                sourceFilepath = filePath;
                tableName = System.IO.Path.GetFileNameWithoutExtension(filePath);
                if (OnBulkInsertStatusUpdate != null)
                    OnBulkInsertStatusUpdate(this, new BulkInsertEventArgs(sourceFilepath, tableName, 0, 0));
                TransferData(tablename, filePath);
                DTotalCount = sourceData.Tables[0].Rows.Count;
                if (OnBulkInsertStatusUpdate != null)
                    OnBulkInsertStatusUpdate(this, new BulkInsertEventArgs(sourceFilepath, tableName, DTotalCount, 0));
                using (System.Data.SqlClient.SqlBulkCopy bcp = new System.Data.SqlClient.SqlBulkCopy(connectionString, SqlBulkCopyOptions.UseInternalTransaction))
                {
                    bcp.SqlRowsCopied += new System.Data.SqlClient.SqlRowsCopiedEventHandler(bcp_SqlRowsCopied);
                    bcp.BatchSize = BathSize;
                    bcp.NotifyAfter = BathSize;

                    tablecount = 0;
                    bcp.DestinationTableName = tablename;
                    DataTable dbtable = sourceData.Tables[0].Clone();
                    for (int i = 0; i < sourceData.Tables[0].Rows.Count; i++)
                    {
                        dbtable.ImportRow(sourceData.Tables[0].Rows[i]);
                        if (i % BathSize == 0&&i>0)
                        {
                            tablecount  = i / BathSize;
                            bcp.BatchSize = BathSize;
                            bcp.NotifyAfter = BathSize;
                            InsertSqlBulk(dbtable, bcp);
                            dbtable.Clear();
                        }  
                    }
                    if (dbtable.Rows.Count > 0)
                    {
                        tablecount += 1;
                        bcp.BatchSize = dbtable.Rows.Count;
                        bcp.NotifyAfter = dbtable.Rows.Count;
                        InsertSqlBulk(dbtable, bcp);
                        dbtable.Clear();
                    }
                    bcp.Close();
                        return true;                    
                }
                
            }
            catch (Exception ex)
            {
                TLog.DefaultInstance.WriteLog("Insert table error:"+tablename +" filepath: " +filePath +ex.ToString(), LogType.ERROR);
                return false;
            }
            
        }

        public void InsertSqlBulk(DataTable dt, SqlBulkCopy sqlBulk)
        {
            string s = "";
            try
            {
                sqlBulk.WriteToServer(dt);
            }
            catch (Exception e)
            {
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    s += dt.Rows[0][i].ToString()+"|";
                }
                if (dt.Rows.Count == 1)
                {
                    //TLog.DefaultInstance.WriteLog("Dumplicate record: " + s+":\r\n"+e.Message , LogType.ERROR);
                    string content = e.Message + " Duplicate record: " + s;
                    if (null != OnLog)
                    {
                        OnLog(this, new LogEventArgs(content));
                    }
                    return;
                }
                int middle = dt.Rows.Count / 2;
                DataTable table = dt.Clone();
                for (int i = 0; i < middle; i++)
                    table.ImportRow(dt.Rows[i]);


                InsertSqlBulk(table, sqlBulk);

                table.Clear();
                for (int i = middle; i < dt.Rows.Count; i++)
                    table.ImportRow(dt.Rows[i]);
                InsertSqlBulk(table, sqlBulk);
            }
            
        }


        private void TransferData(string tablename, string filePath)
        {
            string cmdStr = "select * from " + tablename;
            if (Command == null)
                Command = new SqlCommand();
            if(Adapter==null)
                Adapter=new SqlDataAdapter();
            if (sourceData == null)
                sourceData = new DataSet();
            Command.CommandText = cmdStr;
            Command.Connection = conn;
            sourceData.Clear();
            sourceData.Tables.Clear();
            Adapter.SelectCommand = Command;
            Adapter.FillSchema(sourceData, SchemaType.Mapped);
            sourceData.Tables[0].Clear();
            System.IO.StreamReader reader;
            if (encoding != null)
            {
                reader = new System.IO.StreamReader(filePath, encoding);
            }
            else
            {
                reader = new System.IO.StreamReader(filePath, System.Text.Encoding.GetEncoding("BIG5"));
            }

            reader.Peek();
            while (reader.Peek() > 0)
            {
                string str = reader.ReadLine();
                if (str.Trim().Length > 0)
                {
                    string[] split = str.Split('|');
                    DataRow dr = sourceData.Tables[0].NewRow();
                    try
                    {
                        TransferDataRow(dr, split);
                        sourceData.Tables[0].Rows.Add(dr);
                    }
                    catch (Exception e)
                    {
                        TLog.DefaultInstance.WriteLog("Transfer data error,table name:"+tablename +" File path"+ filePath +" "+ e.ToString(), LogType.ERROR);
                    }
                }
            }
        }

        private void TransferDataRow(DataRow aRow, string[] srcArray)
        {
            int ColumnCount = sourceData.Tables[0].Columns.Count;
            string tmpSrcValue = "";
            string srcarraystr = "";
            for (int i = 0; i < ColumnCount; i++)
            {
                if (i < srcArray.Length)
                {
                    tmpSrcValue = srcArray[i].Trim();
                }
                else
                {
                    tmpSrcValue = "";
                }
                try
                {
                    if (tmpSrcValue == "NULL")
                    {
                        aRow[i] = DBNull.Value;
                    }
                    else if (sourceData.Tables[0].Columns[i].DataType == typeof(System.String))
                    {
                        int ColMaxLen = sourceData.Tables[0].Columns[i].MaxLength;
                        aRow[i] = Getlengthstring(tmpSrcValue, ColMaxLen);                      
                    }
                    else if (sourceData.Tables[0].Columns[i].DataType == typeof(System.Int16))
                    {
                        if (tmpSrcValue.Length > 0)
                        {
                            aRow[i] = Int16.Parse(tmpSrcValue);
                        }
                        else
                        {
                            aRow[i] = 0;
                        }
                    }
                    else if (sourceData.Tables[0].Columns[i].DataType == typeof(System.Int32))
                    {
                        if (tmpSrcValue.Length > 0)
                        {
                            aRow[i] = Int32.Parse(tmpSrcValue);
                        }
                        else
                        {
                            aRow[i] = 0;
                        }

                    }
                    else if (sourceData.Tables[0].Columns[i].DataType == typeof(System.Int64))
                    {
                        if (tmpSrcValue.Length > 0)
                        {
                            aRow[i] = Int64.Parse(tmpSrcValue);
                        }
                        else
                        {
                            aRow[i] = 0;
                        }
                    }
                    else if (sourceData.Tables[0].Columns[i].DataType == typeof(System.Double))
                    {
                        if (tmpSrcValue.Length > 0)
                        {
                            aRow[i] = Double.Parse(tmpSrcValue);
                        }
                        else
                        {
                            aRow[i] = 0;
                        }
                    }
                    else if (sourceData.Tables[0].Columns[i].DataType == typeof(System.Decimal))
                    {
                        if (tmpSrcValue.Length > 0)
                        {
                            aRow[i] = Decimal.Parse(tmpSrcValue);
                            // aRow[i] = Decimal.Round(Decimal.Parse(tmpSrcValue));
                        }
                        else
                        {
                            aRow[i] = 0;
                        }
                    }
                    else if (sourceData.Tables[0].Columns[i].DataType == typeof(System.DateTime))
                    {
                        if (tmpSrcValue.Length > 0)
                        {
                            aRow[i] = DateTime.Parse(tmpSrcValue);
                        }
                        else
                        {
                            aRow[i] = DateTime.Now;
                        }
                    }
                    else if (sourceData.Tables[0].Columns[i].DataType == typeof(System.Boolean))
                    {
                        if (tmpSrcValue.Length > 0)
                        {
                            aRow[i] = tmpSrcValue == "1" ? true : false;
                        }
                        else
                        {
                            aRow[i] = false;
                        }
                    }
                    else
                    {
                        aRow[i] = DBNull.Value;
                    }

                }
                catch(Exception e)
                {
                    if (tmpSrcValue == "NULL")
                    {
                        aRow[i] = DBNull.Value;
                    }
                    else if (sourceData.Tables[0].Columns[i].DataType == typeof(System.String))
                    {
                        aRow[i] = "";
                    }
                    else if (sourceData.Tables[0].Columns[i].DataType == typeof(System.Int16))
                    {
                        aRow[i] = 0;
                    }
                    else if (sourceData.Tables[0].Columns[i].DataType == typeof(System.Int32))
                    {
                        aRow[i] = 0;
                    }
                    else if (sourceData.Tables[0].Columns[i].DataType == typeof(System.Double))
                    {
                        aRow[i] = 0;
                    }
                    else if (sourceData.Tables[0].Columns[i].DataType == typeof(System.Decimal))
                    {
                        aRow[i] = 0;
                    }
                    else if (sourceData.Tables[0].Columns[i].DataType == typeof(System.DateTime))
                    {
                        aRow[i] = DateTime.Now;
                    }
                    else if (sourceData.Tables[0].Columns[i].DataType == typeof(System.Boolean))
                    {
                        aRow[i] = false;

                    }
                    else if (sourceData.Tables[0].Columns[i].DataType == typeof(System.Char))
                    {
                        aRow[i] = "";
                    }
                    else
                    {
                        aRow[i] = DBNull.Value;
                    }
                    for (int j = 0; j < srcArray.Length; j++)
                    {
                        srcarraystr += srcArray[j] + "|";
                    }
                    TLog.DefaultInstance.WriteLog(e.Message + "[Index\\Value:" + i + "\\" + srcArray[i]  + "]:" + srcarraystr, LogType.ERROR);
                    srcarraystr = "";
                }
            }
        }

        private string Getlengthstring(string str, int length)
        {
            string temp = str;
            int j = 0, k = 0;
            CharEnumerator ce = str.GetEnumerator();
            while (ce.MoveNext())
            {
                j += (ce.Current > 0 && ce.Current < 255) ? 1 : 2;
                if (j <= length)
                {
                    k++;
                }
                else
                {
                    temp = str.Substring(0, k);
                    break;
                }
            }
            return temp.Trim();
        }

        public void bcp_SqlRowsCopied(object sender, System.Data.SqlClient.SqlRowsCopiedEventArgs e)
        {
            int count=0;
            count = (tablecount-1) * BathSize + Int32.Parse(e.RowsCopied.ToString());
            if ((e != null) && (OnBulkInsertStatusUpdate!=null))
            { 
                OnBulkInsertStatusUpdate(this, new BulkInsertEventArgs(sourceFilepath, tableName, DTotalCount, count ));
            }
        }

    }
}
