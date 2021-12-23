using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Threading;
using System.IO;


namespace AMM
{
    public class MsSqlManager
    {
        private string sqlConnectionString;
        int GetRetryMax = 5;
        int RetryMax = 5;

        //private string ParameterServerName;
        public string ParameterServerIP { get; set; }

        //private string ParameterDatabaseName;
        public string ParameterDatabaseName { get; set; }

        public string ParameterUserId { get; set; }

        public string ParameterPassword { get; set; }

        public string DatabasePath { get; set; }
        
        // Database가 존재 하지 않을 경우 처리를 위하여 Datasource를 파싱 및 재조합
        public MsSqlManager(string sqlConnectionString)
        {
            ParameterServerIP = sqlConnectionString.Split(';')[0].Split('=')[1].ToString();
            ParameterDatabaseName = sqlConnectionString.Split(';')[1].Split('=')[1].ToString();
            ParameterUserId = sqlConnectionString.Split(';')[2].Split('=')[1].ToString();
            ParameterPassword = sqlConnectionString.Split(';')[3].Split('=')[1].ToString();

            //this.sqlConnectionString = string.Format("server={0};user id={1};password={2}"
            //                                                            , ParameterServerIP
            //                                                            , ParameterUserId
            //                                                            , ParameterPassword);

            this.sqlConnectionString = sqlConnectionString;
        }

        public bool OpenTest(bool first = true)
        {
            try
            {
                // 입력한 Database가 존재하는지 확인
                bool bExistsDatabase = ExistsDatabase(ParameterDatabaseName);

                if (bExistsDatabase == false)
                {
                    CreateDatabase(DatabasePath, ParameterDatabaseName);

                    // Database가 존재할 경우 해당 Database를 사용
                    //UseDatabase(ParameterDatabaseName);
                }

                using (SqlConnection c = new SqlConnection(sqlConnectionString))
                {
                    c.Open();
                }

                return true;
            }
            //catch (SqlException sqlEx)
            //{
            //    if (first == false)
            //        return false;

            //    if (sqlEx.Number == 4060)
            //    {
            //        CreateDatabase(DatabasePath, ParameterDatabaseName);

            //        // Database가 존재할 경우 해당 Database를 사용
            //        UseDatabase(ParameterDatabaseName);

            //        return OpenTest(false);
            //    }
            //    //sqlEx.Number
            //    Log.WriteLog(Log4net.EnumLogLevel.ERROR, sqlEx.ToString());
            //}
            catch (Exception ex)
            {
                ex.ToString();
                //Log.WriteLog(Log4net.EnumLogLevel.ERROR, ex.ToString());

            }

            return false;
        }

        public DataTable GetData(string queryString, int retry = 1)
        {
            DataTable dt = new DataTable();

            try
            {
                using (SqlConnection c = new SqlConnection(sqlConnectionString))
                {
                    c.Open();

                    using (SqlCommand cmd = new SqlCommand(queryString, c))
                    {
                        using (SqlDataAdapter adt = new SqlDataAdapter(cmd))
                        {
                            adt.Fill(dt);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (retry == GetRetryMax)
                {
                    ex.ToString();
                    //Log.WriteLog(Log4net.EnumLogLevel.ERROR, queryString);
                    //Log.WriteLog(Log4net.EnumLogLevel.ERROR, ex.ToString());
                }
                else
                {
                    System.Threading.Thread.Sleep(100);
                   // Log.WriteLog(Log4net.EnumLogLevel.ERROR, "MSSQL Get Retry:" + retry);
                    return GetData(queryString, ++retry);
                }
            }
            return dt;
        }

        public int SetData(string queryString, SqlParameter[] sqlParam = null, CommandType cmdType = CommandType.Text, int retry = 1)
        {
            int ret = 0;
            try
            {
                using (SqlConnection c = new SqlConnection(sqlConnectionString))
                {
                    c.Open();
                    using (SqlCommand cmd = new SqlCommand(queryString, c))
                    {
                        if (sqlParam != null)
                            cmd.Parameters.AddRange(sqlParam);

                        cmd.CommandType = cmdType;
                        cmd.CommandTimeout = 300;

                        ret = cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                if (retry == RetryMax)
                {
                    //Log.WriteLog(Log4net.EnumLogLevel.ERROR, queryString);
                    //Log.WriteLog(Log4net.EnumLogLevel.ERROR, sqlEx.ToString());
                    return ret;
                }
                //else if (sqlEx.Number == 1205) //SQL_DEADLOCK_ERROR_CODE
                {
                    System.Threading.Thread.Sleep(100);
                    //Log.WriteLog(Log4net.EnumLogLevel.ERROR, "MSSQL Retry:" + retry);
                    return SetData(queryString, sqlParam, cmdType, ++retry);
                }
            }
            return ret;
        }

        public int SetData(List<string> queryList, CommandType cmdType = CommandType.Text, int retry = 1)
        {
            if (queryList.Count == 0)
                return 0;

            int ret = 0;
            try
            {
                using (SqlConnection c = new SqlConnection(sqlConnectionString))
                {
                    //Log.WriteLog(Log4net.EnumLogLevel.INFO, "MSSQL SetData List Start");
                    c.Open();
                    SqlTransaction transaction = c.BeginTransaction();

                    foreach (var query in queryList)
                    {
                        using (SqlCommand cmd = new SqlCommand(query, c))
                        {
                            cmd.CommandType = cmdType;
                            cmd.Transaction = transaction;
                            ret = cmd.ExecuteNonQuery();
                        }
                    }
                    transaction.Commit();

                    //Log.WriteLog(Log4net.EnumLogLevel.INFO, "MSSQL SetData List End");
                }
            }
            catch (SqlException sqlEx)
            {
                if (retry == RetryMax)
                {                   
                    //Log.WriteLog(Log4net.EnumLogLevel.ERROR, queryList.Count.ToString() + "/" + queryList[0]);
                    //Log.WriteLog(Log4net.EnumLogLevel.ERROR, sqlEx.ToString());
                    return ret;
                }
                //else if (sqlEx.Number == 1205) //SQL_DEADLOCK_ERROR_CODE
                {
                    System.Threading.Thread.Sleep(100);
                    //Log.WriteLog(Log4net.EnumLogLevel.ERROR, "MSSQL Retry:" + retry);
                    return SetData(queryList, cmdType, ++retry);
                }
            }
            return ret;
        }

        public void DropTable(string tableName)
        {
            try
            {
                string query = "DROP TABLE " + tableName;
                SetData(query);
            }
            catch (Exception ex)
            {
                ex.ToString();
                //Log.WriteLog(Log4net.EnumLogLevel.ERROR, ex.ToString());
            }
        }

        public int GetColumnsCount(string tableName)
        {
            try
            {
                string query = "SELECT COUNT(*) FROM information_schema.COLUMNS WHERE TABLE_NAME = '" + tableName + "'";
                DataTable dt = GetData(query);
                if (dt.Rows.Count > 0)
                    return Convert.ToInt16(dt.Rows[0][0].ToString());
            }
            catch (Exception ex)
            {
                ex.ToString();
                //Log.WriteLog(Log4net.EnumLogLevel.ERROR, ex.ToString());
            }

            return 0;
        }

        public bool ExistsTable(string tableName)
        {
            try
            {
                string query = "SELECT TABLE_NAME FROM information_schema.TABLES WHERE TABLE_NAME = '" + tableName + "'";
                DataTable dt = GetData(query);
                if (dt.Rows.Count > 0)
                    return true;
            }
            catch (Exception ex)
            {
                ex.ToString();
                //Log.WriteLog(Log4net.EnumLogLevel.ERROR, ex.ToString());
            }

            return false;
        }

        public bool ExistsDatabase(string databaseName)
        {
            try
            {
                string tempConnectionString = sqlConnectionString;

                this.sqlConnectionString = GetConnectionStringNoDatabase();

                string query = "SELECT name FROM sys.databases WHERE name = '" + databaseName + "'";
                DataTable dt = GetData(query);

                sqlConnectionString = tempConnectionString;

                if (dt.Rows.Count > 0)
                    return true;
            }
            catch (Exception ex)
            {
                ex.ToString();
                //Log.WriteLog(Log4net.EnumLogLevel.ERROR, ex.ToString());
            }

            return false;
        }

        public string GetConnectionStringNoDatabase()
        {
            return string.Format("server={0};user id={1};password={2}"
                                                              , ParameterServerIP
                                                              , ParameterUserId
                                                              , ParameterPassword);
        }

        public bool CreateDatabase(string databasePath, string databaseName)
        {
            try
            {
                // Database 생성 전에 생성할 폴더가 존재하는지 확인 후 없을 경우 생성
                //CommonFunc.DirectoryInit(databasePath);

                string query = string.Format("CREATE DATABASE [{0}] " +
                                             "CONTAINMENT = NONE "+
                                             "ON  PRIMARY " + 
                                             "( NAME = N'{0}', FILENAME = N'{1}\\{0}.mdf' , SIZE = 5120KB , MAXSIZE = UNLIMITED, FILEGROWTH = 1024KB ) " +
                                             "LOG ON " + 
                                             "( NAME = N'{0}_log', FILENAME = N'{1}\\{0}_log.ldf' , SIZE = 2048KB , MAXSIZE = 2048GB , FILEGROWTH = 10%)"
                                             , databaseName, databasePath);

                string tempConnectionString = sqlConnectionString;
                this.sqlConnectionString = GetConnectionStringNoDatabase();

                SetData(query);

                sqlConnectionString = tempConnectionString;


                return true;
            }
            catch (Exception ex)
            {
                ex.ToString();
                //Log.WriteLog(Log4net.EnumLogLevel.ERROR, ex.ToString());
            }

            return false;
        }

        public bool UseDatabase(string databaseName)
        {
            try
            {
                // Database가 존재할 경우 해당 Database를 사용
                string query = "USE " + databaseName;
                SetData(query);

                return true;
            }
            catch (Exception ex)
            {
                ex.ToString();
                //Log.WriteLog(Log4net.EnumLogLevel.ERROR, ex.ToString());
            }

            return false;
        }
    }
}

