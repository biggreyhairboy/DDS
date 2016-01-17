using System;
using System.Text;
using System.Collections;
using System.IO;
using System.Data;
using System.Text.RegularExpressions;
using System.Diagnostics;
using OMS.common.Utilities;

namespace OMS.common.IO
{
    /// <summary>
    /// Read the CSV class, read the file of CSV, and put the data to dataTable
    /// </summary>
    public class CsvStreamReader
    {
        private ArrayList rowAL;         //Row table
        private string fileName;        //File Name

        private Encoding encoding;      //Encoding

        public CsvStreamReader()
        {
            this.rowAL = new ArrayList();
            this.fileName = "";
            this.encoding = Encoding.Default;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName">FileName, include the file path</param>
        public CsvStreamReader(string fileName)
        {
            this.rowAL = new ArrayList();
            this.fileName = fileName;
            this.encoding = Encoding.Default;
            LoadCsvFile();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName">FileName, include the file path</param>
        /// <param name="encoding">Encoding</param>
        public CsvStreamReader(string fileName, Encoding encoding)
        {
            this.rowAL = new ArrayList();
            this.fileName = fileName;
            this.encoding = encoding;
            LoadCsvFile();
        }
        /// <summary>
        /// FileName, include the file path
        /// </summary>
        public string FileName
        {
            set
            {
                this.fileName = value;
                LoadCsvFile();
            }
        }
        /// <summary>
        /// Encoding
        /// </summary>
        public Encoding FileEncoding
        {
            set
            {
                this.encoding = value;
            }
        }
        /// <summary>
        /// Get the row count
        /// </summary>
        public int RowCount
        {
            get
            {
                return this.rowAL.Count;
            }
        }
        /// <summary>
        /// Get the column count
        /// </summary>
        public int ColCount
        {
            get
            {
                int maxCol;
                maxCol = 0;
                for (int i = 0; i < this.rowAL.Count; i++)
                {
                    ArrayList colAL = (ArrayList)this.rowAL[i];
                    maxCol = (maxCol > colAL.Count) ? maxCol : colAL.Count;
                }

                return maxCol;
            }
        }
        /// <summary>
        /// Get the data for the row and the column
        /// row:row = 1 the first row
        /// col:列,col = 1 the first column 
        /// </summary>
        public string this[int row, int col]
        {
            get
            {
                //chedk the input data
                //CheckRowValid(row);
                //CheckColValid(col);
                ArrayList colAL = (ArrayList)this.rowAL[row - 1];
                if (colAL.Count < col)
                {
                    return "";
                }
                return colAL[col - 1].ToString();
            }
        }
        /// <summary>
        /// Get the datatable data base on the minrow,maxrow,minCol,MaxCol
        /// MinRow=1: the first row
        /// minCol=1: the first column
        /// maxrow= -1: the max row
        /// maxcol=-1: the max column
        /// </summary>
        public DataTable this[int minRow, int maxRow, int minCol, int maxCol]
        {
            get
            {
                CheckRowValid(minRow);
                CheckMaxRowValid(maxRow);
                CheckColValid(minCol);
                CheckMaxColValid(maxCol);
                if (maxRow == -1)
                {
                    maxRow = RowCount;
                }
                if (maxCol == -1)
                {
                    maxCol = ColCount;
                }
                if (maxRow < minRow)
                {
                    TLog.DefaultInstance.WriteLog("The max row can not be less than the min row");
                }
                if (maxCol < minCol)
                {
                    TLog.DefaultInstance.WriteLog("The Max column can not be less than the min column");
                }
                DataTable csvDT = new DataTable();
                int i;
                int col;
                int row;

                //Add column
                for (i = minCol; i <= maxCol; i++)
                {
                    csvDT.Columns.Add(i.ToString());
                }
                for (row = minRow; row <= maxRow; row++)
                {
                    DataRow csvDR = csvDT.NewRow();

                    i = 0;
                    for (col = minCol; col <= maxCol; col++)
                    {
                        csvDR[i] = this[row, col];
                        i++;
                    }
                    csvDT.Rows.Add(csvDR);
                }

                return csvDT;
            }
        }
        /// <summary>
        /// Check the row is valid or not
        /// </summary>
        /// <param name="col"></param>  
        private void CheckRowValid(int row)
        {
            if (row <= 0)
            {
                TLog.DefaultInstance.WriteLog("The row can not less than 0");
            }
            if (row > RowCount)
            {
                TLog.DefaultInstance.WriteLog("There is no the row data");
            }
        }
        /// <summary>
        /// Check the max row
        /// </summary>
        /// <param name="col"></param>  
        private void CheckMaxRowValid(int maxRow)
        {
            if (maxRow <= 0 && maxRow != -1)
            {
                TLog.DefaultInstance.WriteLog("The row can not be 0 or less than 0");
            }
            if (maxRow > RowCount)
            {
                TLog.DefaultInstance.WriteLog("There is no the data for the row");
            }
        }

        /// <summary>
        /// Check the column data
        /// </summary>
        /// <param name="col"></param>  
        private void CheckColValid(int col)
        {
            if (col <= 0)
            {
                TLog.DefaultInstance.WriteLog("The column can not be 0 or less than 0");
            }
            if (col > ColCount)
            {
                TLog.DefaultInstance.WriteLog("There is no the data for the column");
            }
        }

        /// <summary>
        /// Check the max column
        /// </summary>
        /// <param name="col"></param>  
        private void CheckMaxColValid(int maxCol)
        {
            if (maxCol <= 0 && maxCol != -1)
            {
                TLog.DefaultInstance.WriteLog("The column can not be 0 or less than 0");
            }
            if (maxCol > ColCount)
            {
                TLog.DefaultInstance.WriteLog("There is no the data for the column");
            }
        }

        /// <summary>
        /// Read the CSV　file 
        /// </summary>
        private void LoadCsvFile()
        {
            if (this.fileName == null)
            {
                TLog.DefaultInstance.WriteLog("There is no CSV file");
            }
            else if (!File.Exists(this.fileName))
            {
                TLog.DefaultInstance.WriteLog("The CSV file is not existed");
            }
            else
            {
            }
            if (this.encoding == null)
            {
                this.encoding = Encoding.Default;
            }
            StreamReader sr = new StreamReader(this.fileName, this.encoding);
            string csvDataLine;
            csvDataLine = "";
            while (true)
            {
                string fileDataLine;
                fileDataLine = sr.ReadLine();
                if (fileDataLine == null)
                {
                    break;
                }
                if (csvDataLine == "")
                {
                    csvDataLine = fileDataLine;//GetDeleteQuotaDataLine(fileDataLine);
                }
                else
                {
                    csvDataLine += "\r\n" + fileDataLine;//GetDeleteQuotaDataLine(fileDataLine);
                }
                if (!IfOddQuota(csvDataLine))
                {
                    AddNewDataLine(csvDataLine);
                    csvDataLine = "";
                }
            }
            sr.Close();
            if (csvDataLine.Length > 0)
            {
                TLog.DefaultInstance.WriteLog("The CSV file format is not right");
            }
        }

        private string GetDeleteQuotaDataLine(string fileDataLine)
        {
            return fileDataLine.Replace("\"\"", "\"");
        }

        private bool IfOddQuota(string dataLine)
        {
            int quotaCount;
            bool oddQuota;
            quotaCount = 0;
            for (int i = 0; i < dataLine.Length; i++)
            {
                if (dataLine[i] == '\"')
                {
                    quotaCount++;
                }
            }
            oddQuota = false;
            if (quotaCount % 2 == 1)
            {
                oddQuota = true;
            }
            return oddQuota;
        }

        private bool IfOddStartQuota(string dataCell)
        {
            int quotaCount;
            bool oddQuota;
            quotaCount = 0;
            for (int i = 0; i < dataCell.Length; i++)
            {
                if (dataCell[i] == '\"')
                {
                    quotaCount++;
                }
                else
                {
                    break;
                }
            }
            oddQuota = false;
            if (quotaCount % 2 == 1)
            {
                oddQuota = true;
            }
            return oddQuota;
        }

        private bool IfOddEndQuota(string dataCell)
        {
            int quotaCount;
            bool oddQuota;

            quotaCount = 0;
            for (int i = dataCell.Length - 1; i >= 0; i--)
            {
                if (dataCell[i] == '\"')
                {
                    quotaCount++;
                }
                else
                {
                    break;
                }
            }

            oddQuota = false;
            if (quotaCount % 2 == 1)
            {
                oddQuota = true;
            }
            return oddQuota;
        }

        /// <summary>
        /// Add the new line
        /// </summary>
        /// <param name="newDataLine">new data line</param>
        private void AddNewDataLine(string newDataLine)
        {
            Debug.WriteLine("NewLine:" + newDataLine);

            ArrayList colAL = new ArrayList();
            string[] dataArray = newDataLine.Split(',');
            bool oddStartQuota;
            string cellData;
            oddStartQuota = false;
            cellData = "";
            for (int i = 0; i < dataArray.Length; i++)
            {
                if (oddStartQuota)
                {
                    cellData += "," + dataArray[i];
                    if (IfOddEndQuota(dataArray[i]))
                    {
                        colAL.Add(GetHandleData(cellData));
                        oddStartQuota = false;
                        continue;
                    }
                }
                else
                {
                    if (IfOddStartQuota(dataArray[i]))
                    {
                        if (IfOddEndQuota(dataArray[i]) && dataArray[i].Length > 2 && !IfOddQuota(dataArray[i]))
                        {
                            colAL.Add(GetHandleData(dataArray[i]));
                            oddStartQuota = false;
                            continue;
                        }
                        else
                        {
                            oddStartQuota = true;
                            cellData = dataArray[i];
                            continue;
                        }
                    }
                    else
                    {
                        colAL.Add(GetHandleData(dataArray[i]));
                    }
                }
            }
            if (oddStartQuota)
            {
                TLog.DefaultInstance.WriteLog("The CSV file format is not right");
            }
            this.rowAL.Add(colAL);
        }

        private string GetHandleData(string fileCellData)
        {
            if (fileCellData == "")
            {
                return "";
            }
            if (IfOddStartQuota(fileCellData))
            {
                if (IfOddEndQuota(fileCellData))
                {
                    return fileCellData.Substring(1, fileCellData.Length - 2).Replace("\"\"", "\"");
                }
                else
                {
                    TLog.DefaultInstance.WriteLog("The CSV file format is not right");
                }
            }
            else
            {
                //the data ""    """"      """"""    
                if (fileCellData.Length > 2 && fileCellData[0] == '\"')
                {
                    fileCellData = fileCellData.Substring(1, fileCellData.Length - 2).Replace("\"\"", "\"");
                }
            }
            return fileCellData;
        }
    }
}


