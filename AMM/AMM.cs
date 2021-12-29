using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;

namespace AMM
{
    public class AMM
    {
        public MsSqlManager MSSql = null; //AMM
        public bool bConnection = false;

        public string Connect()
        {
            if (MSSql != null)
            {
                ReturnLogSave("SqlManager is null");
                return "NG";
            }

            string strConnetion = string.Format("server=10.135.200.35;database=ATK4-AMM-DBv1;user id=amm;password=amm@123"); //AMM SERVER

            MSSql = new MsSqlManager(strConnetion);

            if (MSSql.OpenTest() == false)
            {
                bConnection = false;
                ReturnLogSave("OpenTest Fail");
                return "NG";
            }
            else
                bConnection = true;

            ReturnLogSave("Connect OK");
            return "OK";

        }

        public int Check_Exist_EqidCheck(string strLinecode, string strEquipid)
        {
            string query;

            query = string.Format("IF EXISTS (SELECT EQUIP_ID FROM TB_STATUS WHERE LINE_CODE='{0}' and EQUIP_ID='{1}') BEGIN SELECT 99 CNT END ELSE BEGIN SELECT 55 CNT END", strLinecode, strEquipid);
            DataTable dt = MSSql.GetData(query);

            if (dt.Rows.Count == 0)
            {
                return -1;
            }

            if (dt.Rows[0]["CNT"].ToString() == "99")
                return 1; //있다
            else
                return 0; //없다
        }

        public string SetEqStart(string strLinecode, string strEquipid)
        {
            string strSendtime = string.Format("{0}{1:00}{2:00}{3:00}{4:00}{5:00}", DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);

            if (strLinecode == "")
            {
                ReturnLogSave("SetEqStart EMPTY LINECODE");
                return "EMPTY LINECODE";
            }

            if (strEquipid == "")
            {
                ReturnLogSave("SetEqStart EMPTY EQUIP");
                return "EMPTY EQUIPID";
            }


            string query = "";
            int nReturn = 0;
            int nCheck = Check_Exist_EqidCheck(strLinecode, strEquipid);

            if (nCheck == 1)
            {
                //Update
                query = string.Format(@"UPDATE TB_STATUS SET DATETIME = '{0}', STATUS = '{1}', TYPE = '{2}' WHERE LINE_CODE = '{3}' and EQUIP_ID='{4}'",
                    strSendtime, "START", "START", strLinecode, strEquipid);

                nReturn = MSSql.SetData(query);

                if (nReturn == 0)
                {
                    ReturnLogSave(string.Format("SetEqStart TB_STATUS UPDATE FAIL LINECODE : {0}, EQUIPID : {1}, nCheck : {2}", strLinecode, strEquipid, nCheck));
                    return "TB_STATUS UPDATE FAIL";
                }
            }
            else if (nCheck == 0)
            {
                query = string.Format(@"INSERT INTO TB_STATUS (DATETIME,LINE_CODE,EQUIP_ID,STATUS,TYPE) VALUES ('{0}','{1}','{2}','{3}','{4}')",
                strSendtime, strLinecode, strEquipid, "START", "START");

                nReturn = MSSql.SetData(query);

                if (nReturn == 0)
                {
                    ReturnLogSave(string.Format("SetEqStart TB_STATUS INSERT FAIL LINECODE : {0}, EQUIPID : {1}, nCheck : {2}", strLinecode, strEquipid, nCheck));
                    return "TB_STATUS INSERT FAIL";
                }
            }
            else
            {
                ReturnLogSave(string.Format("SetEqStart EQUIPID CHECK FAIL LINECODE : {0}, EQUIPID : {1}, nCheck : {2}", strLinecode, strEquipid, nCheck));
                return "EQUIPID CHECK FAIL";
            }

            ///////Log 저장
            query = string.Format(@"INSERT INTO TB_STATUS_HISTORY (DATETIME,LINE_CODE,EQUIP_ID,STATUS,TYPE) VALUES ('{0}','{1}','{2}','{3}','{4}')",
                strSendtime, strLinecode, strEquipid, "START", "START");

            nReturn = MSSql.SetData(query);

            if (nReturn == 0)
            {
                ReturnLogSave(string.Format("SetEqStart TB_STATUS_HISTORY INSERT FAIL LINECODE : {0}, EQUIPID : {1}", strLinecode, strEquipid));
                return "TB_STATUS_HISTORY INSERT FAIL";
            }

            ////Skynet////
            if (bConnection)
            {
                Skynet_PM_Start(strLinecode, "1760", strEquipid);
            }
            /////////////////////////////////////////////
            return "OK";
        }

        public string SetEqEnd(string strLinecode, string strEquipid)
        {
            string strSendtime = string.Format("{0}{1:00}{2:00}{3:00}{4:00}{5:00}", DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);

            if (strLinecode == "")
            {
                ReturnLogSave("SetEqEnd EMPTY LINECODE");
                return "EMPTY LINECODE";
            }

            if (strEquipid == "")
            {
                ReturnLogSave("SetEqEnd EMPTY EQUIP");
                return "EMPTY EQUIPID";
            }

            string query = "";
            int nReturn = 0;
            int nCheck = Check_Exist_EqidCheck(strLinecode, strEquipid);

            if (nCheck == 1)
            {
                //Update
                query = string.Format(@"UPDATE TB_STATUS SET DATETIME = '{0}', STATUS = '{1}', TYPE = '{2}' WHERE LINE_CODE = '{3}' and EQUIP_ID='{4}'",
                    strSendtime, "END", "END", strLinecode, strEquipid);

                nReturn = MSSql.SetData(query);

                if (nReturn == 0)
                {
                    ReturnLogSave(string.Format("SetEqEnd TB_STATUS UPDATE FAIL LINECODE : {0}, EQUIPID : {1}, nCheck : {2}", strLinecode, strEquipid, nCheck));
                    return "TB_STATUS UPDATE FAIL";
                }
            }
            else if (nCheck == 0)
            {
                query = string.Format(@"INSERT INTO TB_STATUS (DATETIME,LINE_CODE,EQUIP_ID,STATUS,TYPE) VALUES ('{0}','{1}','{2}','{3}','{4}')",
                strSendtime, strLinecode, strEquipid, "END", "END");

                nReturn = MSSql.SetData(query);

                if (nReturn == 0)
                {
                    ReturnLogSave(string.Format("SetEqEnd TB_STATUS INSERT FAIL LINECODE : {0}, EQUIPID : {1}, nCheck : {2}", strLinecode, strEquipid, nCheck));
                    return "TB_STATUS INSERT FAIL";
                }
            }
            else
            {
                ReturnLogSave(string.Format("SetEqEnd EQUIPID CHECK FAIL LINECODE : {0}, EQUIPID : {1}, nCheck : {2}", strLinecode, strEquipid, nCheck));
                return "EQUIPID CHECK FAIL";
            }

            ////////Log 저장
            query = string.Format(@"INSERT INTO TB_STATUS_HISTORY (DATETIME,LINE_CODE,EQUIP_ID,STATUS,TYPE) VALUES ('{0}','{1}','{2}','{3}','{4}')",
                strSendtime, strLinecode, strEquipid, "END", "END");

            nReturn = MSSql.SetData(query);

            if (nReturn == 0)
            {
                ReturnLogSave(string.Format("SetEqEnd TB_STATUS_HISTORY INSERT FAIL LINECODE : {0}, EQUIPID : {1}, nCheck : {2}", strLinecode, strEquipid, nCheck));
                return "TB_STATUS_HISTORY INSERT FAIL";
            }

            ////Skynet////
            if (bConnection)
            {
                Skynet_PM_End(strLinecode, "1760", strEquipid);
            }
            /////////////////////////////////////////////
            return "OK";
        }

        public string SetEqStatus(string strLinecode, string strEquipid, string strStatus, string strType)
        {
            string strSendtime = string.Format("{0}{1:00}{2:00}{3:00}{4:00}{5:00}", DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);

            strStatus = strStatus.ToUpper();
            strType = strType.ToUpper();

            if (strLinecode == "")
            {
                ReturnLogSave("SetEqStatus EMPTY LINECODE");
                return "EMPTY LINECODE";
            }

            if (strEquipid == "")
            {
                ReturnLogSave("SetEqStatus EMPTY EQUIP");
                return "EMPTY EQUIPID";
            }

            string query = "";
            int nReturn = 0;
            int nCheck = Check_Exist_EqidCheck(strLinecode, strEquipid);

            if (nCheck == 1)
            {
                //Update
                query = string.Format(@"UPDATE TB_STATUS SET DATETIME = '{0}', STATUS = '{1}', TYPE = '{2}' WHERE LINE_CODE = '{3}' and EQUIP_ID='{4}'",
                    strSendtime, strStatus, strType, strLinecode, strEquipid);

                nReturn = MSSql.SetData(query);

                if (nReturn == 0)
                {
                    ReturnLogSave(string.Format("SetEqStatus TB_STATUS UPDATE FAIL LINECODE : {0}, EQUIPID : {1}, nCheck : {2}", strLinecode, strEquipid, nCheck));
                    return "TB_STATUS UPDATE FAIL";
                }
            }
            else if (nCheck == 0)
            {
                query = string.Format(@"INSERT INTO TB_STATUS (DATETIME,LINE_CODE,EQUIP_ID,STATUS,TYPE) VALUES ('{0}','{1}','{2}','{3}','{4}')",
                    strSendtime, strLinecode, strEquipid, strStatus, strType);

                nReturn = MSSql.SetData(query);

                if (nReturn == 0)
                {
                    ReturnLogSave(string.Format("SetEqStatus TB_STATUS INSERT FAIL LINECODE : {0}, EQUIPID : {1}, nCheck : {2}", strLinecode, strEquipid, nCheck));
                    return "TB_STATUS INSERT FAIL";
                }
            }
            else
            {
                ReturnLogSave(string.Format("SetEqStatus EQUIPID CHECK FAIL LINECODE : {0}, EQUIPID : {1}, nCheck : {2}", strLinecode, strEquipid, nCheck));
                return "EQUIPID CHECK FAIL";
            }

            //////Log 저장
            query = string.Format(@"INSERT INTO TB_STATUS_HISTORY (DATETIME,LINE_CODE,EQUIP_ID,STATUS,TYPE) VALUES ('{0}','{1}','{2}','{3}','{4}')",
                strSendtime, strLinecode, strEquipid, strStatus, strType);

            nReturn = MSSql.SetData(query);

            if (nReturn == 0)
                return "NO DATA";

            ////Skynet////
            if (bConnection)
            {
                if (strStatus == "IDLE")
                {
                    Skynet_SM_Send_Idle(strLinecode, "1760", strEquipid, "");
                }
                else if (strStatus == "RUN")
                {
                    Skynet_SM_Send_Run(strLinecode, "1760", strEquipid, strType);
                }
                else if (strStatus == "ALARM" || strStatus == "STOP")
                {
                    Skynet_SM_Send_Alarm(strLinecode, "1760", strEquipid, strType);
                }
                else if (strStatus == "SETUP")
                {
                    Skynet_SM_Send_Setup(strLinecode, "1760", strEquipid, strType);
                }
                else if (strStatus == "COMPLETE" || strStatus == "READY" || strStatus == "COMPLETED")
                {
                    Skynet_SM_Send_Run(strLinecode, "1760", strEquipid, strStatus);
                }
            }
            /////////////////////////////////////////////
            return "OK";
        }

        public string SetEqStatus(string strLinecode, string strEquipid, string strStatus, string strType, string strDeparture, string strArrival) //MOVE 출발지, 도착지 
        {
            string strSendtime = string.Format("{0}{1:00}{2:00}{3:00}{4:00}{5:00}", DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);

            strStatus = strStatus.ToUpper();
            strType = strType.ToUpper();

            if (strLinecode == "")
                return "EMPTY LINECODE";

            if (strEquipid == "")
                return "EMPTY EQUIPID";

            string query = "";
            int nReturn = 0;
            int nCheck = Check_Exist_EqidCheck(strLinecode, strEquipid);

            if (nCheck == 1)
            {
                //Update
                query = string.Format(@"UPDATE TB_STATUS SET DATETIME='{0}', STATUS='{1}', TYPE='{2}', DEPARTURE='{3}', ARRIVAL='{4}' WHERE LINE_CODE = '{5}' and EQUIP_ID='{6}'",
                    strSendtime, strStatus, strType, strDeparture, strArrival, strLinecode, strEquipid);

                nReturn = MSSql.SetData(query);

                if (nReturn == 0)
                    return "NO DATA";
            }
            else if (nCheck == 0)
            {
                query = string.Format(@"INSERT INTO TB_STATUS (DATETIME,LINE_CODE,EQUIP_ID,STATUS,TYPE,DEPARTURE,ARRIVAL) VALUES ('{0}','{1}','{2}','{3}','{4}','{5}','{6}')",
                    strSendtime, strLinecode, strEquipid, strStatus, strType, strDeparture, strArrival);

                nReturn = MSSql.SetData(query);

                if (nReturn == 0)
                    return "NO DATA";

            }
            else
            {
                return "NO EQUIPID";
            }

            /////Log 저장
            query = string.Format(@"INSERT INTO TB_STATUS_HISTORY (DATETIME,LINE_CODE,EQUIP_ID,STATUS,TYPE,DEPARTURE,ARRIVAL) VALUES ('{0}','{1}','{2}','{3}','{4}','{5}','{6}')",
                strSendtime, strLinecode, strEquipid, strStatus, strType, strDeparture, strArrival);

            nReturn = MSSql.SetData(query);

            if (nReturn == 0)
                return "NO DATA";

            ////Skynet////
            if (bConnection)
            {
                if (strStatus == "IDLE")
                {
                    Skynet_SM_Send_Idle(strLinecode, "1760", strEquipid, "");
                }
                else if (strStatus == "RUN")
                {
                    Skynet_SM_Send_Run(strLinecode, "1760", strEquipid, strType, strDeparture, strArrival);
                }
                else if (strStatus == "ALARM")
                {
                    Skynet_SM_Send_Alarm(strLinecode, "1760", strEquipid, strType);
                }
                else if (strStatus == "SETUP")
                {
                    Skynet_SM_Send_Setup(strLinecode, "1760", strEquipid, strType);
                }
                else if (strStatus == "COMPLETE" || strStatus == "READY" || strStatus == "COMPLETED")
                {
                    Skynet_SM_Send_Run(strLinecode, "1760", strEquipid, strStatus);
                }
            }
            /////////////////////////////////////////////
            return "OK";
        }
        public DataTable GetStatus(string strLinecode, string strEquipid)
        {
            string query = "";

            query = string.Format(@"SELECT * FROM TB_STATUS WITH (NOLOCK) WHERE LINE_CODE='{0}' and EQUIP_ID='{1}'", strLinecode, strEquipid);

            DataTable dt = MSSql.GetData(query);

            return dt;
        }

        public string GetReelQty(string strLinecode, string strEuipid, string strReelid, string strQty)
        {
            return strQty;
        }

        public string SetPickingID(string strLinecode, string strEquipid, string strPickingid, string strQty, string strRequestor)
        {
            string query1 = "";
            List<string> queryList1 = new List<string>();

            string strSendtime = string.Format("{0}{1:00}{2:00}{3:00}{4:00}{5:00}", DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);

            ////Picking ID 생성
            query1 = string.Format(@"INSERT INTO TB_PICK_ID_INFO (DATETIME,LINE_CODE,EQUIP_ID,PICKID,QTY,REQUESTOR) VALUES ('{0}','{1}','{2}','{3}','{4}','{5}')",
                strSendtime, strLinecode, strEquipid, strPickingid, strQty, strRequestor);

            queryList1.Add(query1);

            int nJudge = MSSql.SetData(queryList1); ///return 확인 해서 false 값 날려 야 함.

            if (nJudge == 0)
                return "PICK ID INSERT FAIL";

            string query2 = "";
            List<string> queryList2 = new List<string>();

            /////Log 저장
            query2 = string.Format(@"INSERT INTO TB_PICK_ID_HISTORY (DATETIME,LINE_CODE,EQUIP_ID,PICKID,QTY,STATUS,REQUESTOR) VALUES ('{0}','{1}','{2}','{3}','{4}','{5}','{6}')",
                strSendtime, strLinecode, strEquipid, strPickingid, strQty, "CREATE", strRequestor);

            queryList2.Add(query2);

            nJudge = MSSql.SetData(queryList2); ///return 확인 해서 false 값 날려 야 함.

            if (nJudge == 0)
                return "PICK HISTORY INSERT FAIL";

            return "OK";
        }

        public DataTable GetPickingID(string strLinecode, string strEquipid)
        {
            string query = "";

            query = string.Format(@"SELECT * FROM TB_PICK_ID_INFO with(nolock) WHERE LINE_CODE='{0}' and EQUIP_ID='{1}'", strLinecode, strEquipid);

            DataTable dt = MSSql.GetData(query);

            return dt;
        }

        public DataTable GetPickingID_ALL(string strLinecode)
        {
            string query = "";

            query = string.Format(@"SELECT * FROM TB_PICK_ID_INFO WHERE LINE_CODE='{0}'", strLinecode);

            DataTable dt = MSSql.GetData(query);

            return dt;
        }

        public DataTable GetPickingID_Requestor(string strRequestor)
        {
            string query = "";

            query = string.Format(@"SELECT * FROM TB_PICK_ID_INFO WHERE REQUESTOR='{0}'", strRequestor);

            DataTable dt = MSSql.GetData(query);

            return dt;
        }

        public string GetPickingID_Pickid(string strPickID)
        {
            string query = "";

            query = string.Format(@"SELECT * FROM TB_PICK_ID_INFO WHERE PICKID='{0}'", strPickID);

            DataTable dt = MSSql.GetData(query);

            if (dt.Rows.Count < 1)
                return "";

            string strRequestor = dt.Rows[0]["REQUESTOR"].ToString();
            strRequestor = strRequestor.Trim();

            return strRequestor;
        }

        public string SetUnloadStart(string strLinecode, string strEquipid, string strPickingid)
        {
            string query1 = "", query2 = "";

            /////Pick id 맞는 자재 정보 가져 오기
            query1 = string.Format(@"SELECT * FROM TB_PICK_LIST_INFO WHERE LINE_CODE='{0}' and EQUIP_ID='{1}' and PICKID='{2}'", strLinecode, strEquipid, strPickingid);

            DataTable dt = MSSql.GetData(query1);

            int nCount = dt.Rows.Count;

            if (nCount == 0)
            {
                return "0";
            }

            string strcount = nCount.ToString();
            string strReturnValue = "";

            int nNewcount = 0;
            for (int n = 0; n < nCount; n++)
            {
                string strInfo = dt.Rows[n]["UID"].ToString();
                strInfo = strInfo.Trim();
                string strStatus = dt.Rows[n]["STATUS"].ToString();
                strStatus = strStatus.Trim();

                if (strStatus == "READY")
                {
                    nNewcount++;
                    strReturnValue = strReturnValue + ";" + strInfo;
                }
                else
                {
                    // Delete_Picklistinfo_Reelid(strLinecode, strEquipid, strInfo);
                }
            }
            strcount = nNewcount.ToString();

            strReturnValue = strcount + strReturnValue;
            string strRequestor = dt.Rows[0]["REQUESTOR"].ToString();

            //////Log 저장 Unload 시작
            List<string> queryList = new List<string>();
            string strSendtime = string.Format("{0}{1:00}{2:00}{3:00}{4:00}{5:00}", DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);

            query2 = string.Format(@"INSERT INTO TB_PICK_ID_HISTORY (DATETIME,LINE_CODE,EQUIP_ID,PICKID,QTY,STATUS,REQUESTOR) VALUES ('{0}','{1}','{2}','{3}','{4}','{5}','{6}')",
                strSendtime, strLinecode, strEquipid, strPickingid, nCount, "START", strRequestor);

            queryList.Add(query2);

            int nJudge = MSSql.SetData(queryList); ///return 확인 해서 false 값 날려 야 함.

            if (nJudge == 0)
            {
                return "PICK HISTORY INSERT FAIL";
            }

            return strReturnValue;
        }

        public string SetUnloadOut(string strLinecode, string strEquipid, string strReelid, bool bWebservice) ///3/9 다시 디버깅
        {
            string query1 = "", query2 = "";

            ///////Pick 자재상태 업데이트
            List<string> queryList1 = new List<string>();
            string strSendtime = string.Format("{0}{1:00}{2:00}{3:00}{4:00}{5:00}", DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);

            //queryList1.Add(Delete_Picklistinfo_Reelid(strLinecode, strEquipid, strReelid));
            //query1 = string.Format(@"INSERT INTO TB_PICK_LIST_INFO (LINE_CODE,EQUIP_ID,PICKID,UID,STATUS,REQUESTOR) VALUES ('{0}','{1}','{2}','{3}','{4}','{5}')",
            //    strLinecode, strEquipid, strPickingid, strReelid, "OUT", strRequestor);

            query1 = string.Format(@"UPDATE TB_PICK_LIST_INFO SET STATUS='{0}' WHERE UID='{1}'", "OUT", strReelid);

            queryList1.Add(query1);

            int nJudge = MSSql.SetData(queryList1); ///return 확인 해서 false 값 날려 야 함.

            if (nJudge == 0)
                return string.Format("PICK LIST({0}) UPDATE FAIL", strReelid);

            ////자재 삭제          
            string strJudge = Delete_MTL_Info(strReelid);

            if (strJudge == "NG")
                return string.Format("{0} DELETE FAIL", strReelid);

            ///////////자재 정보 가져 오기 //TB_PICK_LIST_INFO
            string query = "";

            query = string.Format(@"SELECT * FROM TB_PICK_LIST_INFO WHERE LINE_CODE='{0}' and EQUIP_ID='{1}' and UID='{2}'", strLinecode, strEquipid, strReelid);
            DataTable dt = MSSql.GetData(query);

            //////////로그 저장 ///TB_PICK_INOUT_HISTORY
            List<string> queryList2 = new List<string>();
            query2 = string.Format(@"INSERT INTO TB_PICK_INOUT_HISTORY (DATETIME,LINE_CODE,EQUIP_ID,PICKID,UID,STATUS,REQUESTOR,TOWER_NO,SID,LOTID,QTY,MANUFACTURER,PRODUCTION_DATE,INCH_INFO,INPUT_TYPE,ORDER_TYPE) VALUES ('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}','{11}','{12}','{13}','{14}','{15}')",
                strSendtime, strLinecode, strEquipid, dt.Rows[0]["PICKID"].ToString(), strReelid, "OUT", dt.Rows[0]["REQUESTOR"].ToString(), dt.Rows[0]["TOWER_NO"].ToString(), dt.Rows[0]["SID"].ToString(), dt.Rows[0]["LOTID"].ToString(),
                dt.Rows[0]["QTY"].ToString(), dt.Rows[0]["MANUFACTURER"].ToString(), dt.Rows[0]["PRODUCTION_DATE"].ToString(), dt.Rows[0]["INCH_INFO"].ToString(), dt.Rows[0]["INPUT_TYPE"].ToString(), dt.Rows[0]["ORDER_TYPE"].ToString());

            queryList2.Add(query2);

            nJudge = MSSql.SetData(queryList2); ///return 확인 해서 false 값 날려 야 함.

            if (nJudge == 0)
                return "NG";

            //////////////IT Webservice////////////
            /////모든 MNBR을 넣어 줘야 함.
            string strMnbr = "", strResut = "", strTwrno = "", strGroup = "";
            strTwrno = dt.Rows[0]["TOWER_NO"].ToString();
            strGroup = strTwrno.Substring(2, 1);

            if (strTwrno == "T0101") strMnbr = "34118";
            else if (strTwrno == "T0102") strMnbr = "34117";
            else if (strTwrno == "T0103") strMnbr = "34119";
            else if (strTwrno == "T0104") strMnbr = "34120";
            else if (strTwrno == "T0201") strMnbr = "34121";
            else if (strTwrno == "T0202") strMnbr = "34122";
            else if (strTwrno == "T0203") strMnbr = "34123";
            else if (strTwrno == "T0204") strMnbr = "34124";
            else if (strTwrno == "T0301") strMnbr = "34125";
            else if (strTwrno == "T0302") strMnbr = "34126";
            else if (strTwrno == "T0303") strMnbr = "34127";
            else if (strTwrno == "T0304") strMnbr = "34128";
            else if (strTwrno == "T0401") strMnbr = "34861";
            else if (strTwrno == "T0402") strMnbr = "34858";
            else if (strTwrno == "T0403") strMnbr = "34854";
            else if (strTwrno == "T0404") strMnbr = "34853";
            else if (strTwrno == "T0501") strMnbr = "34862";
            else if (strTwrno == "T0502") strMnbr = "34852";
            else if (strTwrno == "T0503") strMnbr = "34857";
            else if (strTwrno == "T0504") strMnbr = "34863";
            else if (strTwrno == "T0601") strMnbr = "34859";
            else if (strTwrno == "T0602") strMnbr = "34860";
            else if (strTwrno == "T0603") strMnbr = "34855";
            else if (strTwrno == "T0604") strMnbr = "34856";
            //[210907_Sangik.choi_7번그룹 추가
            else if (strTwrno == "T0701") strMnbr = "6417";
            else if (strTwrno == "T0702") strMnbr = "6420";
            else if (strTwrno == "T0703") strMnbr = "6418";
            else if (strTwrno == "T0704") strMnbr = "6419";
            //]210907_Sangik.choi_7번그룹 추가

            if (strMnbr != "")
            {
                if (bWebservice)
                {
                    try
                    {
                        var taskResut = Fnc_InoutTransaction(strMnbr, dt.Rows[0]["REQUESTOR"].ToString(), "CMS_OUT", strReelid, "", dt.Rows[0]["SID"].ToString(), dt.Rows[0]["MANUFACTURER"].ToString(), dt.Rows[0]["LOTID"].ToString(), "", dt.Rows[0]["QTY"].ToString(), "EA");
                        strResut = taskResut.Result;

                        if (strResut.Contains("Success") != true && strResut.Contains("Same Status") != true
                            && strResut.Contains("Enhance Location") != true && strResut.Contains("Already exist") != true)
                        {
                            Skynet_Set_Webservice_Faileddata(strMnbr, dt.Rows[0]["REQUESTOR"].ToString(), "CMS_OUT", strReelid, "", dt.Rows[0]["SID"].ToString(), dt.Rows[0]["MANUFACTURER"].ToString(), dt.Rows[0]["LOTID"].ToString(), "", dt.Rows[0]["QTY"].ToString(), "EA", strGroup);
                            return "FAILED_WEBSERVICE";
                        }

                        string strReturn = SetFailedWebservicedata(strEquipid);
                        return strReturn;
                    }
                    catch (Exception ex)
                    {
                        Skynet_Set_Webservice_Faileddata(strMnbr, dt.Rows[0]["REQUESTOR"].ToString(), "CMS_OUT", strReelid, "", dt.Rows[0]["SID"].ToString(), dt.Rows[0]["MANUFACTURER"].ToString(), dt.Rows[0]["LOTID"].ToString(), "", dt.Rows[0]["QTY"].ToString(), "EA", strGroup);
                        string strex = ex.ToString();
                        return "FAILED_WEBSERVICE";
                    }
                }
                else
                {
                    Skynet_Set_Webservice_Faileddata(strMnbr, dt.Rows[0]["REQUESTOR"].ToString(), "CMS_OUT", strReelid, "", dt.Rows[0]["SID"].ToString(), dt.Rows[0]["MANUFACTURER"].ToString(), dt.Rows[0]["LOTID"].ToString(), "", dt.Rows[0]["QTY"].ToString(), "EA", strGroup);
                }
            }

            return "OK";
        }
        public string SetUnloadOut_Manual(string strLinecode, string strEquipid, string strReelid, bool bWebservice) ///3/14
        {
            ///////Pick 자재상태 업데이트
            List<string> queryList = new List<string>();
            string strSendtime = string.Format("{0}{1:00}{2:00}{3:00}{4:00}{5:00}", DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);

            ///////////자재 정보 가져 오기 //TB_PICK_LIST_INFO
            string query = "", query2 = "";

            query = string.Format(@"SELECT * FROM TB_MTL_INFO WHERE LINE_CODE='{0}' and EQUIP_ID='{1}' and UID='{2}'", strLinecode, strEquipid, strReelid);
            DataTable dt = MSSql.GetData(query);

            //////////로그 저장 ///TB_PICK_INOUT_HISTORY
            query2 = string.Format(@"INSERT INTO TB_PICK_INOUT_HISTORY (DATETIME,LINE_CODE,EQUIP_ID,PICKID,UID,STATUS,REQUESTOR,TOWER_NO,SID,LOTID,QTY,MANUFACTURER,PRODUCTION_DATE,INCH_INFO,INPUT_TYPE) VALUES ('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}','{11}','{12}','{13}','{14}')",
                strSendtime, strLinecode, strEquipid, "-", strReelid, "OUT-MANUAL", "-", dt.Rows[0]["TOWER_NO"].ToString(), dt.Rows[0]["SID"].ToString(), dt.Rows[0]["LOTID"].ToString(),
                dt.Rows[0]["QTY"].ToString(), dt.Rows[0]["MANUFACTURER"].ToString(), dt.Rows[0]["PRODUCTION_DATE"].ToString(), dt.Rows[0]["INCH_INFO"].ToString(), dt.Rows[0]["INPUT_TYPE"].ToString(), "MANUAL");

            queryList.Add(query2);

            int nJudge = MSSql.SetData(queryList); ///return 확인 해서 false 값 날려 야 함.

            if (nJudge == 0)
                return "NG";

            ////자재 삭제          
            string strJudge = Delete_MTL_Info(strReelid);

            if (strJudge == "NG")
                return "NG";

            //////////////IT Webservice////////////
            /////모든 MNBR을 넣어 줘야 함.
            string strMnbr = "", strResut = "", strTwrno = "", strGroup = "";
            strTwrno = dt.Rows[0]["TOWER_NO"].ToString();
            strGroup = strTwrno.Substring(2, 1);

            if (strTwrno == "T0101") strMnbr = "34118";
            else if (strTwrno == "T0102") strMnbr = "34117";
            else if (strTwrno == "T0103") strMnbr = "34119";
            else if (strTwrno == "T0104") strMnbr = "34120";
            else if (strTwrno == "T0201") strMnbr = "34121";
            else if (strTwrno == "T0202") strMnbr = "34122";
            else if (strTwrno == "T0203") strMnbr = "34123";
            else if (strTwrno == "T0204") strMnbr = "34124";
            else if (strTwrno == "T0301") strMnbr = "34125";
            else if (strTwrno == "T0302") strMnbr = "34126";
            else if (strTwrno == "T0303") strMnbr = "34127";
            else if (strTwrno == "T0304") strMnbr = "34128";
            else if (strTwrno == "T0401") strMnbr = "34861";
            else if (strTwrno == "T0402") strMnbr = "34858";
            else if (strTwrno == "T0403") strMnbr = "34854";
            else if (strTwrno == "T0404") strMnbr = "34853";
            else if (strTwrno == "T0501") strMnbr = "34862";
            else if (strTwrno == "T0502") strMnbr = "34852";
            else if (strTwrno == "T0503") strMnbr = "34857";
            else if (strTwrno == "T0504") strMnbr = "34863";
            else if (strTwrno == "T0601") strMnbr = "34859";
            else if (strTwrno == "T0602") strMnbr = "34860";
            else if (strTwrno == "T0603") strMnbr = "34855";
            else if (strTwrno == "T0604") strMnbr = "34856";
            //[210907_Sangik.choi_7번그룹 추가
            else if (strTwrno == "T0701") strMnbr = "6417";
            else if (strTwrno == "T0702") strMnbr = "6420";
            else if (strTwrno == "T0703") strMnbr = "6418";
            else if (strTwrno == "T0704") strMnbr = "6419";
            //]210907_Sangik.choi_7번그룹 추가

            if (strMnbr != "")
            {
                if (bWebservice)
                {
                    try
                    {
                        var taskResut = Fnc_InoutTransaction(strMnbr, "", "CMS_OUT", strReelid, "", dt.Rows[0]["SID"].ToString(), dt.Rows[0]["MANUFACTURER"].ToString(), dt.Rows[0]["LOTID"].ToString(), "", dt.Rows[0]["QTY"].ToString(), "EA");
                        strResut = taskResut.Result;

                        if (strResut.Contains("Success") != true && strResut.Contains("Same Status") != true
                            && strResut.Contains("Enhance Location") != true && strResut.Contains("Already exist") != true)
                        {
                            Skynet_Set_Webservice_Faileddata(strMnbr, "", "CMS_OUT", strReelid, "", dt.Rows[0]["SID"].ToString(), dt.Rows[0]["MANUFACTURER"].ToString(), dt.Rows[0]["LOTID"].ToString(), "", dt.Rows[0]["QTY"].ToString(), "EA", strGroup);
                            return "FAILED_WEBSERVICE";
                        }

                        string strReturn = SetFailedWebservicedata(strEquipid);
                        return strReturn;
                    }
                    catch (Exception ex)
                    {
                        Skynet_Set_Webservice_Faileddata(strMnbr, "", "CMS_OUT", strReelid, "", dt.Rows[0]["SID"].ToString(), dt.Rows[0]["MANUFACTURER"].ToString(), dt.Rows[0]["LOTID"].ToString(), "", dt.Rows[0]["QTY"].ToString(), "EA", strGroup);

                        string strex = ex.ToString();
                        return "FAILED_WEBSERVICE";
                    }
                }
                else
                {
                    Skynet_Set_Webservice_Faileddata(strMnbr, "", "CMS_OUT", strReelid, "", dt.Rows[0]["SID"].ToString(), dt.Rows[0]["MANUFACTURER"].ToString(), dt.Rows[0]["LOTID"].ToString(), "", dt.Rows[0]["QTY"].ToString(), "EA", strGroup);
                }
            }

            return "OK";
        }
        public string SetFailedWebservicedata(string strEquipid)
        {
            string strGroup = strEquipid.Substring(strEquipid.Length - 1, 1);

            DataTable dtWebservice = Skynet_Get_Webservice_Faileddata(strGroup);

            int nWebCount = dtWebservice.Rows.Count;

            if (nWebCount > 0)
            {
                string[] strWebdata = new string[12];

                for (int k = 0; k < nWebCount; k++)
                {
                    strWebdata[0] = dtWebservice.Rows[k]["MNBR"].ToString(); strWebdata[0] = strWebdata[0].Trim();
                    strWebdata[1] = dtWebservice.Rows[k]["BADGE"].ToString(); strWebdata[1] = strWebdata[1].Trim();
                    strWebdata[2] = dtWebservice.Rows[k]["ACTION"].ToString(); strWebdata[2] = strWebdata[2].Trim();
                    strWebdata[3] = dtWebservice.Rows[k]["REEL_ID"].ToString(); strWebdata[3] = strWebdata[3].Trim();
                    strWebdata[4] = dtWebservice.Rows[k]["MTL_TYPE"].ToString(); strWebdata[4] = strWebdata[4].Trim();
                    strWebdata[5] = dtWebservice.Rows[k]["SID"].ToString(); strWebdata[5] = strWebdata[5].Trim();
                    strWebdata[6] = dtWebservice.Rows[k]["VENDOR"].ToString(); strWebdata[6] = strWebdata[6].Trim();
                    strWebdata[7] = dtWebservice.Rows[k]["BATCH"].ToString(); strWebdata[7] = strWebdata[7].Trim();
                    strWebdata[8] = dtWebservice.Rows[k]["EXPIRED_DATE"].ToString(); strWebdata[8] = strWebdata[8].Trim();
                    strWebdata[9] = dtWebservice.Rows[k]["QTY"].ToString(); strWebdata[9] = strWebdata[9].Trim();
                    strWebdata[10] = dtWebservice.Rows[k]["UNIT"].ToString(); strWebdata[10] = strWebdata[10].Trim();

                    var taskWebResut = Fnc_InoutTransaction(strWebdata[0], strWebdata[1], strWebdata[2], strWebdata[3], strWebdata[4], strWebdata[5],
                        strWebdata[6], strWebdata[7], strWebdata[8], strWebdata[9], strWebdata[10]);
                    string strwebResut = taskWebResut.Result;

                    if (strwebResut.Contains("Success") != true && strwebResut.Contains("Same Status") != true
                        && strwebResut.Contains("Enhance Location") != true && strwebResut.Contains("Already exist") != true)
                    {
                        k = nWebCount;
                        return "FAILED_WEBSERVICE";
                    }
                    else
                    {
                        Skynet_Webservice_Faileddata_Delete(strWebdata[3]);
                    }
                }
            }

            return "OK";
        }

        public string SetUnloadEnd(string strLinecode, string strEquipid, string strPickingid)
        {
            string query1 = "", query2 = "";

            ///Pick id 정보 가져 오기
            query1 = string.Format(@"SELECT * FROM TB_PICK_ID_INFO WHERE LINE_CODE='{0}' and EQUIP_ID='{1}' and PICKID='{2}'", strLinecode, strEquipid, strPickingid);

            DataTable dt = MSSql.GetData(query1);

            int nCount = dt.Rows.Count;

            if (nCount == 0)
            {
                return "OK";
            }

            string strGet_Qty = dt.Rows[0]["QTY"].ToString();
            string strGet_Requestor = dt.Rows[0]["REQUESTOR"].ToString();

            ///Pick id Unload End 정보 저장
            List<string> queryList1 = new List<string>();
            string strSendtime = string.Format("{0}{1:00}{2:00}{3:00}{4:00}{5:00}", DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);

            query2 = string.Format(@"INSERT INTO TB_PICK_ID_HISTORY (DATETIME,LINE_CODE,EQUIP_ID,PICKID,QTY,STATUS,REQUESTOR) VALUES ('{0}','{1}','{2}','{3}','{4}','{5}','{6}')",
                strSendtime, strLinecode, strEquipid, strPickingid, strGet_Qty, "END", strGet_Requestor);

            queryList1.Add(query2);


            int nJudge = MSSql.SetData(queryList1); ///return 확인 해서 false 값 날려 야 함.

            if (nJudge == 0)
            {
                return "NG";
            }

            /////PickID delete
            List<string> queryList2 = new List<string>();
            queryList2.Add(Delete_Pickidinfo(strLinecode, strEquipid, strPickingid));

            nJudge = MSSql.SetData(queryList2);

            if (nJudge == 0)
            {
                return "NG";
            }

            /////PickList delete
            List<string> queryList3 = new List<string>();
            queryList3.Add(Delete_Picklistinfo_Pickid(strLinecode, strEquipid, strPickingid));

            nJudge = MSSql.SetData(queryList3);

            if (nJudge == 0)
            {
                return "NG";
            }

            return "OK";
        }

        public string SetLoadComplete(string strLinecode, string strEquipid, string strBcrinfo, bool bWebservice)
        {
            //1. Barcode parsing 
            //2. 자재 확인, DB에 해당 자재 가 있느냐?
            //3. 없으면 저장
            //4. 로그 저장
            //5. Web service 

            ////1.바코드 파싱
            ///

            var strReturn = "";

            string[] strInfo = strBcrinfo.Split(';');
            int ncount = strInfo.Length;

            if (ncount != 9)
            {
                strReturn = "BARCODE_ERROR";
                return strReturn;
            }

            ////////2. 자재 확인
            string query = "", query2 = "", query3 = "";

            query = string.Format(@"SELECT * FROM TB_MTL_INFO WHERE UID='{0}'", strInfo[1]);
            DataTable dt = MSSql.GetData(query);

            int nCount = dt.Rows.Count;

            if (nCount > 0)
                return "DUPLICATE";

            //////3. DB 저장
            string strSendtime = string.Format("{0}{1:00}{2:00}{3:00}{4:00}{5:00}", DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);

            List<string> queryList1 = new List<string>();
            query2 = string.Format(@"INSERT INTO TB_MTL_INFO (DATETIME,LINE_CODE,EQUIP_ID,TOWER_NO,UID,SID,LOTID,QTY,MANUFACTURER,PRODUCTION_DATE,INCH_INFO,INPUT_TYPE) VALUES ('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}','{11}')",
                strSendtime, strLinecode, strEquipid, strInfo[0], strInfo[1], strInfo[2], strInfo[3], strInfo[4], strInfo[5], strInfo[6], strInfo[7], strInfo[8]);

            queryList1.Add(query2);

            int nJudge = MSSql.SetData(queryList1); ///return 확인 해서 false 값 날려 야 함.

            if (nJudge == 0)
                return "NG";

            //////////로그 저장 ///TB_PICK_INOUT_HISTORY
            List<string> queryList2 = new List<string>();
            query3 = string.Format(@"INSERT INTO TB_PICK_INOUT_HISTORY (DATETIME,LINE_CODE,EQUIP_ID,PICKID,UID,STATUS,REQUESTOR,TOWER_NO,SID,LOTID,QTY,MANUFACTURER,PRODUCTION_DATE,INCH_INFO,INPUT_TYPE) VALUES ('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}','{11}','{12}','{13}','{14}')",
                strSendtime, strLinecode, strEquipid, "", strInfo[1], "IN", "", strInfo[0], strInfo[2], strInfo[3], strInfo[4], strInfo[5], strInfo[6], strInfo[7], strInfo[8]);

            queryList2.Add(query3);

            nJudge = MSSql.SetData(queryList2); ///return 확인 해서 false 값 날려 야 함.

            if (nJudge == 0)
                return "NG";

            ///////////IT Webservice////////////
            /////모든 MNBR을 넣어 줘야 함.
            string strMnbr = "", strResut = "", strGroup = "";
            strGroup = strInfo[0].Substring(2, 1);

            if (strInfo[0] == "T0101") strMnbr = "34118";
            else if (strInfo[0] == "T0102") strMnbr = "34117";
            else if (strInfo[0] == "T0103") strMnbr = "34119";
            else if (strInfo[0] == "T0104") strMnbr = "34120";
            else if (strInfo[0] == "T0201") strMnbr = "34121";
            else if (strInfo[0] == "T0202") strMnbr = "34122";
            else if (strInfo[0] == "T0203") strMnbr = "34123";
            else if (strInfo[0] == "T0204") strMnbr = "34124";
            else if (strInfo[0] == "T0301") strMnbr = "34125";
            else if (strInfo[0] == "T0302") strMnbr = "34126";
            else if (strInfo[0] == "T0303") strMnbr = "34127";
            else if (strInfo[0] == "T0304") strMnbr = "34128";
            else if (strInfo[0] == "T0401") strMnbr = "34861";
            else if (strInfo[0] == "T0402") strMnbr = "34858";
            else if (strInfo[0] == "T0403") strMnbr = "34854";
            else if (strInfo[0] == "T0404") strMnbr = "34853";
            else if (strInfo[0] == "T0501") strMnbr = "34862";
            else if (strInfo[0] == "T0502") strMnbr = "34852";
            else if (strInfo[0] == "T0503") strMnbr = "34857";
            else if (strInfo[0] == "T0504") strMnbr = "34863";
            else if (strInfo[0] == "T0601") strMnbr = "34859";
            else if (strInfo[0] == "T0602") strMnbr = "34860";
            else if (strInfo[0] == "T0603") strMnbr = "34855";
            else if (strInfo[0] == "T0604") strMnbr = "34856";
            //[210907_Sangik.choi_7번그룹 추가
            else if (strInfo[0] == "T0701") strMnbr = "6417";
            else if (strInfo[0] == "T0702") strMnbr = "6420";
            else if (strInfo[0] == "T0703") strMnbr = "6418";
            else if (strInfo[0] == "T0704") strMnbr = "6419";
            //]210907_Sangik.choi_7번그룹 추가

            if (strMnbr != "")
            {
                if (bWebservice)
                {
                    try
                    {
                        var taskResut = Task.Run(async () =>
                        {
                            return await Fnc_InoutTransaction(strMnbr, "", "CMS_IN", strInfo[1], "", strInfo[2], strInfo[5], strInfo[3], "", strInfo[4], "EA");
                        });

                        strResut = taskResut.Result;
                        //Fnc_WebServiceLog(strResut, taskResut.Result);

                        if (strResut.Contains("Success") != true && strResut.Contains("Same Status") != true
                            && strResut.Contains("Enhance Location") != true && strResut.Contains("Already exist") != true)
                        {
                            Skynet_Set_Webservice_Faileddata(strMnbr, "", "CMS_IN", strInfo[1], "", strInfo[2], strInfo[5], strInfo[3], "", strInfo[4], "EA", strGroup);
                            strReturn = "FAILED_WEBSERVICE";
                            return strReturn;
                        }

                        string str = SetFailedWebservicedata(strEquipid);
                        strReturn = str;

                        return strReturn;
                    }
                    catch (Exception ex)
                    {
                        Skynet_Set_Webservice_Faileddata(strMnbr, "", "CMS_IN", strInfo[1], "", strInfo[2], strInfo[5], strInfo[3], "", strInfo[4], "EA", strGroup);
                        string strex = ex.ToString();
                        return "FAILED_WEBSERVICE";
                    }
                }
                else
                {
                    int nJudge2 = Skynet_Set_Webservice_Faileddata(strMnbr, "", "CMS_IN", strInfo[1], "", strInfo[2], strInfo[5], strInfo[3], "", strInfo[4], "EA", strGroup);

                    if (nJudge2 == 0)
                    {
                        strReturn = "NG";
                        return strReturn;
                    }
                }
            }

            strReturn = "OK";
            return strReturn;
        }
        public string SetSortComplete(string strLinecode, string strEquipid, string strBcrinfo, bool bWebservice)
        {
            //1. Barcode parsing 
            //2. 자재 확인, DB에 해당 자재 가 있느냐?
            //3. 없으면 저장
            //4. 로그 저장
            //5. Web service 

            ////1.바코드 파싱

            var strReturn = "";

            string[] strInfo = strBcrinfo.Split(';');
            int ncount = strInfo.Length;

            if (ncount != 9)
            {
                strReturn = "BARCODE_ERROR";
                return strReturn;
            }

            string query = "";

            //////3. DB 저장
            string strSendtime = string.Format("{0}{1:00}{2:00}{3:00}{4:00}{5:00}", DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);

            //////////로그 저장 ///TB_PICK_INOUT_HISTORY
            query = string.Format(@"INSERT INTO TB_PICK_INOUT_HISTORY (DATETIME,LINE_CODE,EQUIP_ID,PICKID,UID,STATUS,REQUESTOR,TOWER_NO,SID,LOTID,QTY,MANUFACTURER,PRODUCTION_DATE,INCH_INFO,INPUT_TYPE) VALUES ('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}','{11}','{12}','{13}','{14}')",
                strSendtime, strLinecode, strEquipid, "", strInfo[1], "IN", "", strInfo[0], strInfo[2], strInfo[3], strInfo[4], strInfo[5], strInfo[6], strInfo[7], strInfo[8]);

            int nJudge = MSSql.SetData(query); ///return 확인 해서 false 값 날려 야 함.

            if (nJudge == 0)
                return "NG";

            ///////////IT Webservice////////////
            /////모든 MNBR을 넣어 줘야 함.
            string strMnbr = "", strResut = "", strGroup = "";

            strMnbr = strEquipid;
            strGroup = strMnbr;

            if (strMnbr != "")
            {
                if (bWebservice)
                {
                    try
                    {
                        var taskResut = Task.Run(async () =>
                        {
                            return await Fnc_InoutTransaction(strMnbr, "", "CMS_IN", strInfo[1], "", strInfo[2], strInfo[5], strInfo[3], "", strInfo[4], "EA");
                        });

                        strResut = taskResut.Result;
                        //Fnc_WebServiceLog(strResut, taskResut.Result);

                        if (strResut.Contains("Success") != true && strResut.Contains("Same Status") != true
                            && strResut.Contains("Enhance Location") != true && strResut.Contains("Already exist") != true)
                        {
                            Skynet_Set_Webservice_Faileddata(strMnbr, "", "CMS_IN", strInfo[1], "", strInfo[2], strInfo[5], strInfo[3], "", strInfo[4], "EA", strGroup);
                            strReturn = "FAILED_WEBSERVICE";
                            return strReturn;
                        }

                        string str = SetFailedWebservicedata(strEquipid);
                        strReturn = str;

                        return strReturn;
                    }
                    catch (Exception ex)
                    {
                        Skynet_Set_Webservice_Faileddata(strMnbr, "", "CMS_IN", strInfo[1], "", strInfo[2], strInfo[5], strInfo[3], "", strInfo[4], "EA", strGroup);
                        string strex = ex.ToString();
                        return "FAILED_WEBSERVICE";
                    }
                }
                else
                {
                    int nJudge2 = Skynet_Set_Webservice_Faileddata(strMnbr, "", "CMS_IN", strInfo[1], "", strInfo[2], strInfo[5], strInfo[3], "", strInfo[4], "EA", strGroup);

                    if (nJudge2 == 0)
                    {
                        strReturn = "NG";
                        return strReturn;
                    }
                }
            }

            strReturn = "OK";
            return strReturn;
        }

        public string SetSortComplete(Reelinfo info)
        {
            //1. Barcode parsing 
            //2. 자재 확인, DB에 해당 자재 가 있느냐?
            //3. 없으면 저장
            //4. 로그 저장
            //5. Web service 

            ////1.바코드 파싱
            var strReturn = "";

            if (info.manufacturer == string.Empty)
            {
                info.manufacturer = "";
            }

            if (info.production_date == string.Empty)
            {
                info.production_date = "";
            }

            string strBcrinfo = string.Format("{0};{1};{2};{3};{4};{5};{6};{7};{8};", info.portNo, info.uid, info.sid, info.lotid, info.qty,
                info.manufacturer, info.production_date, info.inch_info, info.destination);
            string[] strInfo = strBcrinfo.Split(';');
            int ncount = strInfo.Length;

            if (info.uid.Length < 3)
            {
                strReturn = "BARCODE_ERROR";
                return strReturn;
            }

            string query = "";

            //////3. DB 저장
            string strSendtime = string.Format("{0}{1:00}{2:00}{3:00}{4:00}{5:00}", DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);

            //////////로그 저장 ///TB_PICK_INOUT_HISTORY
            query = string.Format(@"INSERT INTO TB_PICK_INOUT_HISTORY (DATETIME,LINE_CODE,EQUIP_ID,PICKID,UID,STATUS,REQUESTOR,TOWER_NO,SID,LOTID,QTY,MANUFACTURER,PRODUCTION_DATE,INCH_INFO,INPUT_TYPE) VALUES ('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}','{11}','{12}','{13}','{14}')",
                strSendtime, info.linecode, info.equipid, "", strInfo[1], "IN", info.createby, strInfo[0], strInfo[2], strInfo[3], strInfo[4], strInfo[5], strInfo[6], strInfo[7], strInfo[8]);

            int nJudge = MSSql.SetData(query); ///return 확인 해서 false 값 날려 야 함.

            if (nJudge == 0)
                return "NG";

            ///////////IT Webservice////////////
            /////모든 MNBR을 넣어 줘야 함.
            string strMnbr = "", strResut = "", strGroup = "", strCreator = "";

            strMnbr = info.equipid;
            strCreator = info.createby;
            strGroup = strMnbr;

            if (strMnbr != "")
            {
                if (info.bwebservice)
                {
                    try
                    {
                        var taskResut = Task.Run(async () =>
                        {
                            return await Fnc_InoutTransaction(strMnbr, strCreator, "CMS_IN", strInfo[1], "", strInfo[2], strInfo[5], strInfo[3], "", strInfo[4], "EA");
                        });

                        strResut = taskResut.Result;
                        //Fnc_WebServiceLog(strResut, taskResut.Result);

                        if (strResut.Contains("Success") != true && strResut.Contains("Same Status") != true
                            && strResut.Contains("Enhance Location") != true && strResut.Contains("Already exist") != true)
                        {
                            Skynet_Set_Webservice_Faileddata(strMnbr, strCreator, "CMS_IN", strInfo[1], "", strInfo[2], strInfo[5], strInfo[3], "", strInfo[4], "EA", strGroup);
                            strReturn = "FAILED_WEBSERVICE";
                            return strReturn;
                        }

                        string str = SetFailedWebservicedata(info.equipid);
                        strReturn = str;

                        return strReturn;
                    }
                    catch (Exception ex)
                    {
                        Skynet_Set_Webservice_Faileddata(strMnbr, strCreator, "CMS_IN", strInfo[1], "", strInfo[2], strInfo[5], strInfo[3], "", strInfo[4], "EA", strGroup);
                        string strex = ex.ToString();
                        return "FAILED_WEBSERVICE";
                    }
                }
                else
                {
                    int nJudge2 = Skynet_Set_Webservice_Faileddata(strMnbr, strCreator, "CMS_IN", strInfo[1], "", strInfo[2], strInfo[5], strInfo[3], "", strInfo[4], "EA", strGroup);

                    if (nJudge2 == 0)
                    {
                        strReturn = "NG";
                        return strReturn;
                    }
                }
            }

            strReturn = "OK";
            return strReturn;
        }

        public string Get_Sid_Location(string sid)
        {
            string query = "";

            query = string.Format(@"SELECT TOWER_NO FROM dbo.TB_MTL_INFO WHERE SID='{0}'", sid);

            DataTable dt = MSSql.GetData(query);

            if (dt.Rows.Count < 1)
                return "NO_DATA";

            List<int> list = new List<int>();

            for (int n = 0; n < dt.Rows.Count; n++)
            {
                string strGetTowerNo = dt.Rows[n]["TOWER_NO"].ToString(); strGetTowerNo = strGetTowerNo.Trim();
                strGetTowerNo = strGetTowerNo.Substring(1, 2);
                list.Add(Int32.Parse(strGetTowerNo));
            }

            list.Sort();

            int nNo_buf = 0, nNocount = 0;
            string strNo = "";
            for (int m = 0; m < list.Count; m++)
            {
                int nNo = list[m];
                if (nNo_buf != nNo)
                {
                    string strInfo = string.Format("{0}", nNo);

                    strNo = strNo + strInfo + ",";
                    nNocount = 0;
                    nNo_buf = nNo;
                }
                else
                    nNocount++;
            }

            strNo = strNo.Substring(0, strNo.Length - 1);
            return strNo;
        }

        public DataTable Get_Sid_info(string linecode, string group, string sid)
        {
            string query = "";

            string strEquipid = string.Format("TWR{0}", group);
            query = string.Format(@"SELECT * FROM dbo.TB_MTL_INFO WHERE LINE_CODE='{0}' and EQUIP_ID='{1}' and SID='{2}'", linecode, strEquipid, sid);

            DataTable dt = MSSql.GetData(query);

            return dt;
        }

        public async Task<string> Fnc_InoutTransaction(string strMnbr, string strBadge, string strAction, string strReelID, string strMtlType, string strSID, string strVendor, string strBatch, string strExpireddate, string strQty, string strUnit)
        {
            string strurl = "";

            if (strMnbr == "" || strReelID == "")
                return "";

            //Fnc_WebServiceLog("Start", "");

            strurl = string.Format("http://cim_service.amkor.co.kr:8080/ysj/material/txn_cms?mnbr={0}&badge={1}&action_type={2}&matl_id={3}&matl_type={4}&matl_sid={5}&matl_vendorlot={6}&matl_vendorname={7}&matl_batch={8}&matl_expired_date={9}&matl_qty={10}&matl_qty_unit={11}",
                strMnbr, strBadge, strAction, strReelID, strMtlType, strSID, strBatch, strVendor, strBatch, strExpireddate, strQty, strUnit);

            var res_ = await Fnc_RunAsync(strurl);

            Fnc_WebServiceLog(strurl, res_);

            return res_;
        }

        public async Task<string> Wbs_Get_Stripmarking_mtlinfo(string strLinecode, string strSM)
        {
            string strurl = "";

            if (strSM == "")
                return "";

            strurl = string.Format("http://tms.amkor.co.kr:8080/TMSWebService/rps_info/get_component_by_sm?line_code={0}&strip_mark={1}", strLinecode, strSM);

            var res_ = await Fnc_RunAsync(strurl);

            Fnc_WebServiceLog(strurl, res_);

            return res_;
        }

        public void Fnc_WebServiceLog(string strMessage, string strResult)
        {
            System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(@"C:\Log");
            if (!di.Exists) { di.Create(); }

            string strPath = "C:\\LOG\\";
            string strToday = string.Format("{0}{1:00}{2:00}", DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
            string strHead = string.Format(",{0:00}:{1:00}:{2:00}", DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);
            strPath = strPath + strToday + "WebService.txt";
            strHead = strToday + strHead;

            string strSave;
            strSave = strHead + ',' + strMessage + ',' + strResult;
            Fnc_WriteFile(strPath, strSave);
        }

        public void ReturnLogSave(string msg)
        {
            System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(@"C:\Log\ReturnLog");
            if (!di.Exists) { di.Create(); }

            string strPath = "C:\Log\ReturnLog\\";
            string strToday = string.Format("{0}{1:00}{2:00}", DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
            string strHead = string.Format(",{0:00}:{1:00}:{2:00}", DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);
            strPath = strPath + strToday + "ReturnLog.txt";
            strHead = strToday + strHead;

            string strSave;
            strSave = strHead + ',' + msg;
            Fnc_WriteFile(strPath, strSave);
        }

        public void Fnc_WriteFile(string strFileName, string strLine)
        {
            using (System.IO.StreamWriter file =
           new System.IO.StreamWriter(strFileName, true))
            {
                file.WriteLine(strLine);
            }
        }

        public async Task<string> Fnc_RunAsync(string strKey)
        {
            var str = "";

            //Fnc_WebServiceLog(strKey, "");

            using (var client = new HttpClient())
            {
                //Fnc_WebServiceLog("N", "");

                client.BaseAddress = new Uri(strKey);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/HY"));

                //Fnc_WebServiceLog("M", "");

                HttpResponseMessage response = client.GetAsync("").Result;
                if (response.IsSuccessStatusCode)
                {
                    var contents = await response.Content.ReadAsStringAsync();
                    str = contents;
                }
            }

            return str;
        }

        public string SetPickIDNo(string strLinecode, string strEquipid, string strPrefix, string strNumber)
        {
            string query1 = "";
            List<string> queryList1 = new List<string>();

            string strSendtime = string.Format("{0}{1:00}{2:00}{3:00}{4:00}{5:00}", DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);

            ////Picking ID 생성
            query1 = string.Format(@"INSERT INTO TB_IDNUNMER_INFO (DATETIME,LINE_CODE,EQUIP_ID,PICK_PREFIX,PICK_NUM) VALUES ('{0}','{1}','{2}','{3}','{4}')",
                strSendtime, strLinecode, strEquipid, strPrefix, strNumber);

            queryList1.Add(query1);

            int nJudge = MSSql.SetData(queryList1); ///return 확인 해서 false 값 날려 야 함.

            if (nJudge == 0)
                return "NG";

            return "OK";
        }

        public string SetPickIDNo(string strLinecode, string strEquipid, string strNumber)
        {
            string query = "";

            string strSendtime = string.Format("{0}{1:00}{2:00}{3:00}{4:00}{5:00}", DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);

            ////Picking ID 생성
            query = string.Format(@"UPDATE dbo.TB_IDNUNMER_INFO SET PICK_NUM='{0}' WHERE LINE_CODE='{1}' AND EQUIP_ID='{2}'",
                strNumber, strLinecode, strEquipid);

            int nJudge = MSSql.SetData(query); ///return 확인 해서 false 값 날려 야 함.

            if (nJudge == 0)
                return "NG";

            return "OK";
        }

        public DataTable GetPickIDNo(string strLinecode, string strEquipid)
        {
            string query = "";

            query = string.Format(@"SELECT * FROM TB_IDNUNMER_INFO WHERE LINE_CODE='{0}' and EQUIP_ID='{1}'", strLinecode, strEquipid);

            DataTable dt = MSSql.GetData(query);

            return dt;
        }
        public DataTable GetInouthistroy(string strLinecode, string strEquipid, double dStart, double dEnd)
        {
            string query = "";

            query = string.Format(@"SELECT * FROM TB_PICK_INOUT_HISTORY WHERE LINE_CODE='{0}' and EQUIP_ID='{1}' and DATETIME>'{2}' and DATETIME<='{3}' ", strLinecode, strEquipid, dStart, dEnd);

            DataTable dt = MSSql.GetData(query);

            return dt;
        }

        public DataTable GetInouthistroy_Sid(string strLinecode, string strSid, double dStart, double dEnd)
        {
            string query = "";

            query = string.Format(@"SELECT * FROM TB_PICK_INOUT_HISTORY WHERE LINE_CODE='{0}' and SID='{1}' and DATETIME>'{2}' and DATETIME<='{3}' ", strLinecode, strSid, dStart, dEnd);

            DataTable dt = MSSql.GetData(query);

            return dt;
        }

        public DataTable GetInouthistroy_Sid2(string strLinecode, string strEquipid, string strSid, double dStart, double dEnd)
        {
            string query = "";

            query = string.Format(@"SELECT * FROM TB_PICK_INOUT_HISTORY WHERE LINE_CODE='{0}' and EQUIP_ID='{1}' and SID='{2}' and DATETIME>'{3}' and DATETIME<='{4}' ", strLinecode, strEquipid, strSid, dStart, dEnd);

            DataTable dt = MSSql.GetData(query);

            return dt;
        }

        public DataTable GetMaterial_Tracking(string strUid)
        {
            string query = "";

            query = string.Format(@"SELECT * FROM TB_PICK_INOUT_HISTORY WHERE UID='{0}' ", strUid);

            DataTable dt = MSSql.GetData(query);

            return dt;
        }

        public DataTable GetMTLInfo(string strLinecode)
        {
            string query = "";

            query = string.Format(@"SELECT * FROM TB_MTL_INFO WHERE LINE_CODE='{0}'", strLinecode);

            DataTable dt = MSSql.GetData(query);

            return dt;
        }

        public DataTable GetMTLInfo(string strLinecode, string strEquipid)
        {
            string query = "";

            query = string.Format(@"SELECT * FROM TB_MTL_INFO WHERE LINE_CODE='{0}' and EQUIP_ID='{1}'", strLinecode, strEquipid);

            DataTable dt = MSSql.GetData(query);

            return dt;
        }

        public DataTable GetMTLInfo(string strLinecode, string strEquipid, string strTwrNo)
        {
            string query = "";

            query = string.Format(@"SELECT * FROM TB_MTL_INFO WHERE LINE_CODE='{0}' and EQUIP_ID='{1}' and TOWER_NO='{2}'", strLinecode, strEquipid, strTwrNo);

            DataTable dt = MSSql.GetData(query);

            return dt;
        }

        public DataTable GetMTLInfo_SID(string strLinecode, string strEquipid, string strSID)
        {
            string query = "";

            query = string.Format(@"SELECT * FROM TB_MTL_INFO WHERE LINE_CODE='{0}' and EQUIP_ID='{1}' and SID='{2}'", strLinecode, strEquipid, strSID);

            DataTable dt = MSSql.GetData(query);

            return dt;
        }

        public DataTable GetMTLInfo_UID(string strLinecode, string strEquipid, string strUID)
        {
            string query = "";

            query = string.Format(@"SELECT * FROM TB_MTL_INFO WHERE LINE_CODE='{0}' and EQUIP_ID='{1}' and UID='{2}'", strLinecode, strEquipid, strUID);

            DataTable dt = MSSql.GetData(query);

            return dt;
        }

        public string SetEqEvent(string strLinecode, string strEquipid, string strErrorcode, string strErrortype, string strErrorname, string strErrordescript, string strErrorAction)
        {
            string query1 = "";
            List<string> queryList1 = new List<string>();

            string strSendtime = string.Format("{0}{1:00}{2:00}{3:00}{4:00}{5:00}", DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);

            /////Log 저장
            query1 = string.Format(@"INSERT INTO TB_EVENT_HISTORY (DATETIME,LINE_CODE,EQUIP_ID,ERROR_CODE,ERROR_TYPE,ERROR_NAME,ERROR_DESCRIPT,ERROR_ACTION) VALUES ('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}')",
                strSendtime, strLinecode, strEquipid, strErrorcode, strErrortype, strErrorname, strErrordescript, strErrorAction);

            queryList1.Add(query1);

            int nJudge = MSSql.SetData(queryList1); ///return 확인 해서 false 값 날려 야 함.

            if (nJudge == 0)
                return "NG";

            ////Skynet////
            if (bConnection)
            {
                Skynet_EM_DataSend(strLinecode, "1760", strEquipid, strErrorcode, strErrortype, strErrorname, strErrordescript, strErrorAction);
            }
            /////////////////////////////////////////////

            return "OK";
        }

        public DataTable GetEqEvent(string strLinecode, string strEquipid)
        {
            string query = "";

            query = string.Format(@"SELECT * FROM TB_EVENT_HISTORY WHERE LINE_CODE='{0}' and EQUIP_ID='{1}'", strLinecode, strEquipid);

            DataTable dt = MSSql.GetData(query);

            return dt;
        }

        public string SetEquipmentInfo(string strLinecode, string strEquipid, string strIndex)
        {
            string strSendtime = string.Format("{0}{1:00}{2:00}{3:00}{4:00}{5:00}", DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);

            List<string> queryList = new List<string>();
            queryList.Add(Delete_EquipmentInfo(strLinecode, strEquipid));

            string query = "";

            query = string.Format(@"INSERT INTO TB_SET_EQUIP (DATETIME,LINE_CODE,EQUIP_ID,INDEX_NO) VALUES ('{0}','{1}','{2}','{3}')",
                strSendtime, strLinecode, strEquipid, strIndex);

            queryList.Add(query);
            int nJudge = MSSql.SetData(queryList); ///return 확인 해서 false 값 날려 야 함.

            if (nJudge == 0)
                return "NG";

            return "OK";
        }

        public DataTable GetEquipmentInfo_All()
        {
            string query = "";

            query = string.Format(@"SELECT * FROM TB_SET_EQUIP");

            DataTable dt = MSSql.GetData(query);

            return dt;
        }

        public DataTable GetEquipmentInfo(string strLinecode)
        {
            string query = "";

            query = string.Format(@"SELECT * FROM TB_SET_EQUIP WHERE LINE_CODE='{0}'", strLinecode);

            DataTable dt = MSSql.GetData(query);

            return dt;
        }

        public string SetPicking_Readyinfo(string strLinecode, string strEquipid, string strPickid, string strUid, string strRequestor, string strTwrno, string strSid, string strLotid, string strQty,
            string strManufacturer, string strProductiondate, string strInchinfo, string strInputtype, string strOrdertype)
        {
            ///1. 자재 중복 체크 TB_PICK_LIST_INFO 내 자재
            ///2. 자재 저장 TB_PICK_READY_INFO
            ///

            if (GetPickingListinfo(strUid) == "NG")
                return "Duplicate";

            //if (GetPickingReadyinfo(strUid) == "NG")
            //    return "Duplicate";

            List<string> queryList = new List<string>();

            string query = "";

            query = string.Format(@"INSERT INTO TB_PICK_READY_INFO (LINE_CODE,EQUIP_ID,PICKID,UID,REQUESTOR,TOWER_NO,SID,LOTID,QTY,MANUFACTURER,PRODUCTION_DATE,INCH_INFO,INPUT_TYPE, ORDER_TYPE) VALUES ('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}','{11}','{12}','{13}')",
                strLinecode, strEquipid, strPickid, strUid, strRequestor, strTwrno, strSid, strLotid, strQty, strManufacturer, strProductiondate, strInchinfo, strInputtype, strOrdertype);

            queryList.Add(query);
            int nJudge = MSSql.SetData(queryList); ///return 확인 해서 false 값 날려 야 함.

            if (nJudge == 0)
                return "NG";

            return "OK";
        }

        public string SetPicking_Listinfo(string strLinecode, string strEquipid, string strPickid, string strUid, string strRequestor, string strTwrno, string strSid, string strLotid, string strQty,
            string strManufacturer, string strProductiondate, string strInchinfo, string strInputtype, string strOrdertype)
        {
            ///1. 자재 중복 체크 TB_PICK_LIST_INFO 내 자재
            ///2. 자재 저장 TB_PICK_READY_INFO
            ///

            if (GetPickingListinfo(strUid) == "NG")
                return "Duplicate";

            //if (GetPickingReadyinfo(strUid) == "NG")
            //    return "Duplicate";

            List<string> queryList = new List<string>();

            string query = "";

            query = string.Format(@"INSERT INTO TB_PICK_LIST_INFO (LINE_CODE,EQUIP_ID,PICKID,UID,STATUS,REQUESTOR,TOWER_NO,SID,LOTID,QTY,MANUFACTURER,PRODUCTION_DATE,INCH_INFO,INPUT_TYPE,ORDER_TYPE) VALUES ('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}','{11}','{12}','{13}','{14}')",
                strLinecode, strEquipid, strPickid, strUid, "READY", strRequestor, strTwrno, strSid, strLotid, strQty, strManufacturer, strProductiondate, strInchinfo, strInputtype, strOrdertype);

            queryList.Add(query);
            int nJudge = MSSql.SetData(queryList); ///return 확인 해서 false 값 날려 야 함.

            if (nJudge == 0)
                return "NG";

            return "OK";
        }

        public string SetPickingList_Cancel(string strLinecode, string strEquipid, string strPickingid)
        {
            string strJudge = "";

            strJudge = Delete_Pickidinfo2(strLinecode, strEquipid, strPickingid);
            if (strJudge == "OK")
            {
                strJudge = Delete_Picklistinfo_Pickid2(strLinecode, strEquipid, strPickingid);
            }

            return strJudge;
        }

        public DataTable GetPickingListinfo(string strLinecode, string strEquipid, string strPickingid)
        {
            string query1 = "";
            query1 = string.Format(@"SELECT * FROM TB_PICK_LIST_INFO WHERE LINE_CODE='{0}' and EQUIP_ID='{1}' and PICKID='{2}'", strLinecode, strEquipid, strPickingid);

            DataTable dt = MSSql.GetData(query1);

            return dt;
        }

        public DataTable GetPickingListinfo(string strLinecode, string strPickingid)
        {
            string query1 = "";
            query1 = string.Format(@"SELECT * FROM TB_PICK_LIST_INFO WHERE LINE_CODE='{0}' and PICKID='{1}'", strLinecode, strPickingid);

            DataTable dt = MSSql.GetData(query1);

            return dt;
        }

        public DataTable GetPickingMtlinfo(string strLinecode, string strUid)
        {
            string query1 = "";
            query1 = string.Format(@"SELECT * FROM TB_PICK_LIST_INFO WHERE LINE_CODE='{0}' and UID='{1}'", strLinecode, strUid);

            DataTable dt = MSSql.GetData(query1);

            return dt;
        }

        public string GetPickingListinfo(string uid)
        {
            string query = "";

            query = string.Format("IF EXISTS (SELECT UID FROM TB_PICK_LIST_INFO WHERE UID='{0}') BEGIN SELECT 99 CNT END ELSE BEGIN SELECT 55 CNT END", uid);
            DataTable dt = MSSql.GetData(query);

            if (dt.Rows.Count == 0)
            {
                return "ERROR";
            }

            if (dt.Rows[0]["CNT"].ToString() == "99")
                return "NG";
            else
                return "OK";
        }

        public string GetPickingReadyinfo(string uid)
        {
            string query = "";

            query = string.Format("IF EXISTS (SELECT UID FROM TB_PICK_READY_INFO WHERE UID='{0}') BEGIN SELECT 99 CNT END ELSE BEGIN SELECT 55 CNT END", uid);
            DataTable dt = MSSql.GetData(query);

            if (dt.Rows.Count == 0)
            {
                return "ERROR";
            }

            if (dt.Rows[0]["CNT"].ToString() == "99")
                return "NG";
            else
                return "OK";
        }

        public DataTable GetPickingReadyinfo_ID(string strPickingid)
        {
            string query = "";

            query = string.Format(@"SELECT * FROM TB_PICK_READY_INFO WHERE PICKID='{0}'", strPickingid);

            DataTable dt = MSSql.GetData(query);

            return dt;
        }

        public DataTable GetPickingIDinfo_Stripmark(string strSM)
        {
            string query = "";

            query = string.Format(@"SELECT PICKID FROM TB_PICK_READY_INFO WHERE ORDER_TYPE='{0}'", strSM);

            DataTable dt = MSSql.GetData(query);

            return dt;
        }

        public string SetUserInfo(string sid, string name, string shift)
        {
            List<string> queryList = new List<string>();

            string str = Delete_UserInfo(sid);

            if (str == "NG")
                return str;

            string query = "";

            query = string.Format(@"INSERT INTO TB_USER_INFO (SID,NAME,SHIFT) VALUES ('{0}','{1}','{2}')", sid, name, shift);

            queryList.Add(query);
            int nJudge = MSSql.SetData(queryList); ///return 확인 해서 false 값 날려 야 함.

            if (nJudge == 0)
                return "NG";

            return "OK";
        }

        public DataTable GetUserInfo(string sid, int nType) //nType 0 : SID, 1:Name
        {
            string query = "";

            if (nType == 0)
                query = string.Format(@"SELECT * FROM TB_USER_INFO WHERE SID='{0}'", sid);
            else
                query = string.Format(@"SELECT * FROM TB_USER_INFO WHERE NAME='{0}'", sid);

            DataTable dt = MSSql.GetData(query);

            int nCount = dt.Rows.Count;

            return dt;
        }

        public string Delete_UserInfo(string sid)
        {
            string query = "";

            query = string.Format("DELETE FROM TB_USER_INFO WHERE SID='{0}'", sid);

            int nJudge = MSSql.SetData(query);

            if (nJudge == 0)
                return "NG";

            return "OK";
        }

        public string SetUserRequest(string sid, string name)
        {
            string strSendtime = string.Format("{0}{1:00}{2:00}{3:00}{4:00}{5:00}", DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);

            List<string> queryList = new List<string>();

            DataTable dt = GetUserRequest();
            if (dt.Rows.Count != 0)
            {
                for (int n = 0; n < dt.Rows.Count; n++)
                {
                    string strGetInfo = dt.Rows[n]["USER_SID"].ToString();
                    strGetInfo = strGetInfo.Trim();
                    if (sid == strGetInfo)
                    {
                        string str = Delete_UserRequest(sid);

                        if (str == "NG")
                            return "DELETE ERROR";
                    }
                }

            }
            string query = "";

            query = string.Format(@"INSERT INTO TB_USER_REQ (DATETIME,USER_SID,USER_NAME) VALUES ('{0}','{1}','{2}')", strSendtime, sid, name);

            queryList.Add(query);
            int nJudge = MSSql.SetData(queryList);

            if (nJudge == 0)
                return "SET";

            return "OK";
        }
        public DataTable GetUserRequest()
        {
            string query = "";

            query = string.Format(@"SELECT * FROM TB_USER_REQ");

            DataTable dt = MSSql.GetData(query);

            return dt;
        }
        public string Delete_UserRequest(string sid)
        {
            string query = "";

            query = string.Format("DELETE FROM TB_USER_REQ WHERE USER_SID='{0}'", sid);

            int nJudge = MSSql.SetData(query);

            if (nJudge == 0)
                return "NG";

            return "OK";
        }

        public string Delete_EquipmentInfo(string strLinecode, string strEquipid)
        {
            string query = "";

            query = string.Format("DELETE FROM TB_SET_EQUIP WHERE LINE_CODE='{0}' and EQUIP_ID='{1}'", strLinecode, strEquipid);

            return query;
        }
        public string Delete_EquipmentInfo2(string strLinecode, string strEquipid)
        {
            string query = "";

            query = string.Format("DELETE FROM TB_SET_EQUIP WHERE LINE_CODE='{0}' and EQUIP_ID='{1}'", strLinecode, strEquipid);

            int nJudge = MSSql.SetData(query);

            if (nJudge == 0)
                return "NG";

            return "OK";
        }

        public string Delete_PickReadyinfo(string strLinecode, string strPickid)
        {
            string query = "";

            query = string.Format("DELETE FROM TB_PICK_READY_INFO WHERE LINE_CODE='{0}' and PICKID ='{1}'", strLinecode, strPickid);

            int nJudge = MSSql.SetData(query);

            if (nJudge == 0)
                return "NG";

            return "OK";
        }

        public string Delete_PickReadyinfo_ReelID(string strLinecode, string strReelid)
        {
            string query = "";

            query = string.Format("DELETE FROM TB_PICK_READY_INFO WHERE LINE_CODE='{0}' and UID='{1}'", strLinecode, strReelid);

            int nJudge = MSSql.SetData(query);

            if (nJudge == 0)
                return "NG";

            return "OK";
        }

        public string Delete_Picklistinfo_Reelid(string strLinecode, string strEquipid, string strReelid)
        {
            string query = "";

            query = string.Format("DELETE FROM TB_PICK_LIST_INFO WHERE LINE_CODE='{0}' and EQUIP_ID='{1}' and UID='{2}'", strLinecode, strEquipid, strReelid);

            int nJudge = MSSql.SetData(query);

            if (nJudge == 0)
                return "NG";

            return "OK";
        }

        public string Delete_Picklistinfo_Pickid(string strLinecode, string strEquipid, string strPickid)
        {
            string query = "";

            query = string.Format("DELETE FROM TB_PICK_LIST_INFO WHERE LINE_CODE='{0}' and EQUIP_ID='{1}' and PICKID='{2}'", strLinecode, strEquipid, strPickid);

            return query;
        }

        public string Delete_Picklistinfo_Pickid2(string strLinecode, string strEquipid, string strPickid)
        {
            string query = "";

            query = string.Format("DELETE FROM TB_PICK_LIST_INFO WHERE LINE_CODE='{0}' and EQUIP_ID='{1}' and PICKID='{2}'", strLinecode, strEquipid, strPickid);

            int nJudge = MSSql.SetData(query);

            if (nJudge == 0)
                return "NG";

            return "OK";
        }

        public string Delete_Pickidinfo(string strLinecode, string strEquipid, string strPickid)
        {
            string query = "";

            query = string.Format("DELETE FROM TB_PICK_ID_INFO WHERE LINE_CODE='{0}' and EQUIP_ID='{1}' and PICKID='{2}'", strLinecode, strEquipid, strPickid);

            return query;
        }

        public string Delete_PickIDNo(string strLinecode, string strEquipid)
        {
            string query = "";

            query = string.Format("DELETE FROM TB_IDNUNMER_INFO WHERE LINE_CODE='{0}' and EQUIP_ID='{1}'", strLinecode, strEquipid);

            int nJudge = MSSql.SetData(query);

            if (nJudge == 0)
                return "NG";

            return "OK";
        }

        public string Delete_Pickidinfo2(string strLinecode, string strEquipid, string strPickid)
        {
            string query = "";

            query = string.Format("DELETE FROM TB_PICK_ID_INFO WHERE LINE_CODE='{0}' and EQUIP_ID='{1}' and PICKID='{2}'", strLinecode, strEquipid, strPickid);

            int nJudge = MSSql.SetData(query);

            if (nJudge == 0)
                return "NG";

            string query2 = "";
            List<string> queryList2 = new List<string>();

            string strSendtime = string.Format("{0}{1:00}{2:00}{3:00}{4:00}{5:00}", DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);

            /////Log 저장
            query2 = string.Format(@"INSERT INTO TB_PICK_ID_HISTORY (DATETIME,LINE_CODE,EQUIP_ID,PICKID,QTY,STATUS,REQUESTOR) VALUES ('{0}','{1}','{2}','{3}','{4}','{5}','{6}')",
                strSendtime, strLinecode, strEquipid, strPickid, "", "CANCEL", "");

            queryList2.Add(query2);

            nJudge = MSSql.SetData(queryList2); ///return 확인 해서 false 값 날려 야 함.

            if (nJudge == 0)
                return "NG";

            return "OK";
        }

        public string Delete_EqStatus(string strLinecode, string strEquipid)
        {
            string query = "";

            query = string.Format("DELETE FROM TB_STATUS WHERE LINE_CODE='{0}' and EQUIP_ID='{1}'", strLinecode, strEquipid);

            return query;
        }

        public string Delete_MTL_Info(string strReelid)
        {
            string query = "";

            query = string.Format("DELETE FROM TB_MTL_INFO WHERE UID='{0}'", strReelid);

            int nJudge = MSSql.SetData(query);

            if (nJudge == 0)
                return "NG";

            return "OK";
        }

        public string Delete_MTL_Tower(string strEqid)
        {
            string query = "";

            query = string.Format("DELETE FROM TB_MTL_INFO WHERE EQUIP_ID='{0}'", strEqid);

            int nJudge = MSSql.SetData(query);

            if (nJudge == 0)
                return "NG";

            return "OK";
        }

        ///User 확인
        public string User_Register(string sid, string name)
        {
            string query1 = "";
            List<string> queryList1 = new List<string>();

            string strSendtime = string.Format("{0}{1:00}{2:00}{3:00}{4:00}{5:00}", DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);

            string check = User_check(sid);

            if (check != "NO_INFO")
            {
                User_Delete(sid);
            }

            query1 = string.Format(@"INSERT INTO TB_USER_INFO (DATETIME,SID,NAME) VALUES ('{0}','{1}','{2}')", strSendtime, sid, name);

            queryList1.Add(query1);

            int nJudge = MSSql.SetData(queryList1);

            if (nJudge == 0)
                return "NG";

            return "OK";
        }
        public string User_Delete(string sid)
        {
            string query = "";

            query = string.Format("DELETE FROM TB_USER_INFO WHERE SID='{0}'", sid);

            int nJudge = MSSql.SetData(query);

            if (nJudge == 0)
                return "NG";

            return "OK";
        }
        public string User_check(string strSid)
        {
            string query = "";

            query = string.Format(@"SELECT * FROM TB_USER_INFO WHERE SID='{0}'", strSid);

            DataTable dt = MSSql.GetData(query);

            int nCount = dt.Rows.Count;

            if (nCount == 0)
            {
                return "NO_INFO";
            }

            string strName = dt.Rows[0]["NAME"].ToString();

            return strName;
        }
        public string Set_Twr_Use(string strTower, string strUse)
        {
            string query1 = "";
            List<string> queryList1 = new List<string>();

            //string strSendtime = string.Format("{0}{1:00}{2:00}{3:00}{4:00}{5:00}", DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);

            Delete_Twr_Use(strTower);

            query1 = string.Format(@"INSERT INTO TB_TOWER_USE (TWR_NAME,TWR_USE) VALUES ('{0}','{1}')", strTower, strUse);

            queryList1.Add(query1);

            int nJudge = MSSql.SetData(queryList1);

            if (nJudge == 0)
                return "NG";

            return "OK";
        }
        public string Delete_Twr_Use(string strTower)
        {
            string query = "";

            query = string.Format("DELETE FROM TB_TOWER_USE WHERE TWR_NAME='{0}'", strTower);

            int nJudge = MSSql.SetData(query);

            if (nJudge == 0)
                return "NG";

            return "OK";
        }
        public string Get_Twr_Use(string strTower)
        {
            string query = "";

            query = string.Format(@"SELECT * FROM TB_TOWER_USE WHERE TWR_NAME='{0}'", strTower);

            DataTable dt = MSSql.GetData(query);

            int nCount = dt.Rows.Count;

            if (nCount == 0)
            {
                return "USE";
            }

            string strUse = dt.Rows[0]["TWR_USE"].ToString();

            return strUse;
        }

        //[210817_Sangik.choi_장기보관자재 검색 가능 인원 검사

        //[210818_Sangik.choi_인치별 capa 조회

        public DataTable Get_Capa_inch()
        {
            string query = "";
            query = string.Format(@"select A.*, B.INCH_7_CAPA, B.INCH_13_CAPA
, convert(decimal(10, 2), ((A.INCH_7_CNT / B.INCH_7_CAPA) * 100)) as INCH_7_LOAD_RATE
, convert(decimal(10, 2), ((A.INCH_13_CNT / B.INCH_13_CAPA) * 100)) as INCH_13_LOAD_RATE
from
(
select EQUIP_ID
, sum(INCH_7) as INCH_7_CNT, sum(INCH_13) as INCH_13_CNT
from
(
select *
, case when INCH_INFO = '7' then 1 else 0 end as INCH_7
, case when INCH_INFO = '13' then 1 else 0 end as INCH_13
from TB_MTL_INFO with(nolock)
where 1=1
--and EQUIP_ID = 'TWR1'
) T
group by EQUIP_ID
) A 
left outer join TB_TOWER_CAPA B with(nolock)
on A.EQUIP_ID = B.EQUIP_ID
order by EQUIP_ID");

            DataTable dt = MSSql.GetData(query);

            return dt;
        }
        //]210818_Sangik.choi_인치별 capa 조회



        public string Check_LT_User(string SID)
        {
            string badge = SID;
            string query = "";

            query = string.Format(@"SELECT * FROM TB_USER_INFO_LT WHERE SID='{0}'", badge);
            DataTable dt = MSSql.GetData(query);

            int nCount = dt.Rows.Count;

            if (nCount == 0)
            {
                return "NG";
            }

            return "OK";

        }

        //]210817_Sangik.choi_장기보관자재 검색 가능 인원 검사





        ///2021.1.26 배출 리스트 있을 시 타워내 릴 감지 되있는 경우 알림 팝업 하여 작업지 인지
        public string Set_Twr_State(string strLinecode, string strEqid, string strReelexist, string strState)
        {
            string query1 = "";

            string strSendtime = string.Format("{0}{1:00}{2:00}{3:00}{4:00}{5:00}", DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);

            Delete_Twr_State(strLinecode, strEqid);

            query1 = string.Format(@"INSERT INTO TB_TOWER_STATE (DATETIME,LINE_CODE,EQUIP_ID,REEL,STATE) VALUES ('{0}','{1}','{2}','{3}','{4}')",
                strSendtime, strLinecode, strEqid, strReelexist, strState);

            int nJudge = MSSql.SetData(query1);

            if (nJudge == 0)
                return "NG";

            return "OK";
        }

        public string Delete_Twr_State(string strLinecode, string strEqid)
        {
            string query = "";

            query = string.Format("DELETE FROM TB_TOWER_STATE WHERE LINE_CODE='{0}' and EQUIP_ID='{1}'", strLinecode, strEqid);

            int nJudge = MSSql.SetData(query);

            if (nJudge == 0)
                return "NG";

            return "OK";
        }
        public string Get_Twr_State(string strLinecode, string strEqid)
        {
            string query = "";

            query = string.Format(@"SELECT * FROM TB_TOWER_STATE WHERE LINE_CODE='{0}' and EQUIP_ID='{1}'", strLinecode, strEqid);

            DataTable dt = MSSql.GetData(query);

            int nCount = dt.Rows.Count;

            if (nCount == 0)
            {
                return "";
            }

            string strReel = dt.Rows[0]["REEL"].ToString();
            string strState = dt.Rows[0]["STATE"].ToString();

            if (strState == "RUN" && strReel == "ON")
                return "PICK_FAIL";

            return strState;
        }
        public string Get_Twr_State_Reel(string strLinecode, string strEqid)
        {
            string query = "";

            query = string.Format(@"SELECT * FROM TB_TOWER_STATE WHERE LINE_CODE='{0}' and EQUIP_ID='{1}'", strLinecode, strEqid);

            DataTable dt = MSSql.GetData(query);

            int nCount = dt.Rows.Count;

            if (nCount == 0)
            {
                return "";
            }

            string strReel = dt.Rows[0]["REEL"].ToString();

            return strReel;
        }

        public string Get_Twr_State_Job(string strLinecode, string strEqid)
        {
            string query = "";

            query = string.Format(@"SELECT * FROM TB_TOWER_STATE WHERE LINE_CODE='{0}' and EQUIP_ID='{1}'", strLinecode, strEqid);

            DataTable dt = MSSql.GetData(query);

            int nCount = dt.Rows.Count;

            if (nCount == 0)
            {
                return "";
            }

            string strJob = dt.Rows[0]["STATE"].ToString();

            return strJob;
        }

        public int Skynet_Exist_EqidCheck(string strLinecode, string strEquipid, int nAlive)
        {
            string query = "";
            query = string.Format(@"UPDATE TB_STATUS SET ALIVE='{0}' WHERE LINE_CODE='{1}' and EQUIP_ID='{2}'", nAlive, strLinecode, strEquipid);
            //GetEqEvent

            int nJudge = MSSql.SetData(query);

            ////Skynet////
            if (bConnection)
            {
                Skynet_SM_Alive(strLinecode, "1760", strEquipid, nAlive);
            }

            return nJudge;
        }

        ////Skynet
        public int Skynet_Exist_EqidCheck(string strLinecode, string strEquipid)
        {
            string query;

            query = string.Format("IF EXISTS (SELECT EQUIP_ID FROM Skynet.dbo.TB_STATUS WHERE LINE_CODE='{0}' and EQUIP_ID='{1}') BEGIN SELECT 99 CNT END ELSE BEGIN SELECT 55 CNT END", strLinecode, strEquipid);
            DataTable dt = MSSql.GetData(query);

            if (dt.Rows.Count == 0)
            {
                return -1;
            }

            if (dt.Rows[0]["CNT"].ToString() == "99")
                return 1; //있다
            else
                return 0; //없다
        }

        public int Skynet_EM_DataSend(string strLinecode, string strProcesscode, string strEquipid, string strErrorcode, string strType, string strErrorName, string strErrorDescript, string strErrorAction)
        {
            string strSendtime = string.Format("{0}{1:00}{2:00}{3:00}{4:00}{5:00}", DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);

            List<string> queryList = new List<string>();
            //queryList.Add(Skynet_EM_Delete(strLinecode, strProcesscode, strEquipid));

            string query = "";

            query = string.Format(@"INSERT INTO Skynet.dbo.TB_EVENT (DATETIME,LINE_CODE,PROCESS_CODE,EQUIP_ID,ERROR_CODE,TYPE,ERROR_NAME,ERROR_DESCRIPT,ERROR_ACTION) VALUES ('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}')",
                strSendtime, strLinecode, "0000", strEquipid, strErrorcode, strType, strErrorName, strErrorDescript, strErrorAction);

            queryList.Add(query);

            int nJudge = MSSql.SetData(queryList); ///return 확인 해서 false 값 날려 야 함.

            return nJudge; // 1: OK else: fail
        }

        public int Skynet_SM_Send_Run(string strLinecode, string strProcesscode, string strEquipid, string strRemote)
        {
            string strSendtime = string.Format("{0}{1:00}{2:00}{3:00}{4:00}{5:00}", DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);

            if (strLinecode == "" || strEquipid == "")
                return 0;

            string query = "";
            int nReturn = 0;

            int nCheck = Skynet_Exist_EqidCheck(strLinecode, strEquipid);

            if (nCheck == 1) //있음
            {
                //Update
                query = string.Format(@"UPDATE Skynet.dbo.TB_STATUS SET DATETIME = '{0}', STATUS = '{1}', TYPE = '{2}' WHERE LINE_CODE = '{3}' and EQUIP_ID='{4}'", strSendtime, "RUN", strRemote, strLinecode, strEquipid);

                nReturn = MSSql.SetData(query);

                if (nReturn == 0)
                    return 0;
            }
            else if (nCheck == 0) // 없음
            {
                //Insert
                query = string.Format(@"INSERT INTO Skynet.dbo.TB_STATUS (DATETIME,LINE_CODE,PROCESS_CODE,EQUIP_ID,STATUS,TYPE) VALUES ('{0}','{1}','{2}','{3}','{4}','{5}')",
                strSendtime, strLinecode, "0000", strEquipid, "RUN", strRemote);

                nReturn = MSSql.SetData(query);

                if (nReturn == 0)
                    return 0;
            }
            else
            {
                //Error
                return 0;
            }

            query = string.Format(@"INSERT INTO Skynet.dbo.TB_STATUS_HISTORY (DATETIME,LINE_CODE,PROCESS_CODE,EQUIP_ID,STATUS,TYPE) VALUES ('{0}','{1}','{2}','{3}','{4}','{5}')",
                strSendtime, strLinecode, "0000", strEquipid, "RUN", strRemote);

            nReturn = MSSql.SetData(query);

            if (nReturn == 0)
                return 0;

            return 1;
        }

        public int Skynet_SM_Send_Run(string strLinecode, string strProcesscode, string strEquipid, string strRemote, string strDeparture, string strArrival)
        {
            string strSendtime = string.Format("{0}{1:00}{2:00}{3:00}{4:00}{5:00}", DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);

            if (strLinecode == "" || strEquipid == "")
                return 0;

            string query = "";
            int nReturn = 0;

            int nCheck = Skynet_Exist_EqidCheck(strLinecode, strEquipid);

            if (nCheck == 1) //있음
            {
                query = string.Format(@"UPDATE Skynet.dbo.TB_STATUS SET DATETIME = '{0}', STATUS = '{1}', TYPE = '{2}', DEPARTURE = '{3}', ARRIVAL = '{4}' WHERE LINE_CODE = '{5}' and EQUIP_ID='{6}'", strSendtime, "RUN", strRemote, strDeparture, strArrival, strLinecode, strEquipid);

                nReturn = MSSql.SetData(query);

                if (nReturn == 0)
                    return 0;
            }
            else if (nCheck == 0) //없음
            {
                query = string.Format(@"INSERT INTO Skynet.dbo.TB_STATUS (DATETIME,LINE_CODE,PROCESS_CODE,EQUIP_ID,STATUS,TYPE,DEPARTURE,ARRIVAL) VALUES ('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}')",
                strSendtime, strLinecode, "0000", strEquipid, "Run", strRemote, strDeparture, strArrival);

                nReturn = MSSql.SetData(query);

                if (nReturn == 0)
                    return 0;
            }
            else
            {
                return 0;
            }

            /////Log 저장
            query = string.Format(@"INSERT INTO Skynet.dbo.TB_STATUS_HISTORY (DATETIME,LINE_CODE,PROCESS_CODE,EQUIP_ID,STATUS,TYPE,DEPARTURE,ARRIVAL) VALUES ('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}')",
                strSendtime, strLinecode, "0000", strEquipid, "Run", strRemote, strDeparture, strArrival);

            nReturn = MSSql.SetData(query);

            if (nReturn == 0)
                return 0;

            return 1;
        }

        public int Skynet_SM_Send_Idle(string strLinecode, string strProcesscode, string strEquipid, string strRemote)
        {
            string strSendtime = string.Format("{0}{1:00}{2:00}{3:00}{4:00}{5:00}", DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);

            if (strLinecode == "" || strEquipid == "")
                return 0;

            string query = "";
            int nReturn = 0;

            int nCheck = Skynet_Exist_EqidCheck(strLinecode, strEquipid);

            if (nCheck == 1) //있음
            {
                query = string.Format(@"UPDATE Skynet.dbo.TB_STATUS SET DATETIME = '{0}', STATUS = '{1}', TYPE = '{2}' WHERE LINE_CODE = '{3}' and EQUIP_ID='{4}'", strSendtime, "IDLE", strRemote, strLinecode, strEquipid);

                nReturn = MSSql.SetData(query);

                if (nReturn == 0)
                    return 0;
            }
            else if (nCheck == 0) //없음
            {
                query = string.Format(@"INSERT INTO Skynet.dbo.TB_STATUS (DATETIME,LINE_CODE,PROCESS_CODE,EQUIP_ID,STATUS,TYPE) VALUES ('{0}','{1}','{2}','{3}','{4}','{5}')",
                strSendtime, strLinecode, "0000", strEquipid, "IDLE", strRemote);

                nReturn = MSSql.SetData(query);

                if (nReturn == 0)
                    return 0;
            }
            else
            {
                return 0;
            }

            /////Log 저장
            query = string.Format(@"INSERT INTO Skynet.dbo.TB_STATUS_HISTORY (DATETIME,LINE_CODE,PROCESS_CODE,EQUIP_ID,STATUS,TYPE) VALUES ('{0}','{1}','{2}','{3}','{4}','{5}')",
                strSendtime, strLinecode, "0000", strEquipid, "IDLE", strRemote);

            nReturn = MSSql.SetData(query);

            if (nReturn == 0)
                return 0;

            return 1;
        }

        public int Skynet_SM_Send_Alarm(string strLinecode, string strProcesscode, string strEquipid, string strRemote)
        {
            string strSendtime = string.Format("{0}{1:00}{2:00}{3:00}{4:00}{5:00}", DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);

            if (strLinecode == "" || strEquipid == "")
                return 0;

            string query = "";
            int nReturn = 0;

            int nCheck = Skynet_Exist_EqidCheck(strLinecode, strEquipid);

            if (nCheck == 1) //있음
            {
                query = string.Format(@"UPDATE Skynet.dbo.TB_STATUS SET DATETIME = '{0}', STATUS = '{1}', TYPE = '{2}' WHERE LINE_CODE = '{3}' and EQUIP_ID='{4}'", strSendtime, "ALARM", strRemote, strLinecode, strEquipid);

                nReturn = MSSql.SetData(query);

                if (nReturn == 0)
                    return 0;
            }
            else if (nCheck == 0) //없음
            {
                query = string.Format(@"INSERT INTO Skynet.dbo.TB_STATUS (DATETIME,LINE_CODE,PROCESS_CODE,EQUIP_ID,STATUS,TYPE) VALUES ('{0}','{1}','{2}','{3}','{4}','{5}')",
                strSendtime, strLinecode, "0000", strEquipid, "ALARM", strRemote);

                nReturn = MSSql.SetData(query);

                if (nReturn == 0)
                    return 0;
            }
            else
            {
                return 0;
            }

            /////Log 저장
            query = string.Format(@"INSERT INTO Skynet.dbo.TB_STATUS_HISTORY (DATETIME,LINE_CODE,PROCESS_CODE,EQUIP_ID,STATUS,TYPE) VALUES ('{0}','{1}','{2}','{3}','{4}','{5}')",
                strSendtime, strLinecode, "0000", strEquipid, "ALARM", strRemote);

            nReturn = MSSql.SetData(query);

            if (nReturn == 0)
                return 0;

            return 1;
        }

        public int Skynet_SM_Send_Setup(string strLinecode, string strProcesscode, string strEquipid, string strRemote)
        {
            string strSendtime = string.Format("{0}{1:00}{2:00}{3:00}{4:00}{5:00}", DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);

            if (strLinecode == "" || strEquipid == "")
                return 0;

            string query = "";
            int nReturn = 0;

            int nCheck = Skynet_Exist_EqidCheck(strLinecode, strEquipid);

            if (nCheck == 1) //있음
            {
                query = string.Format(@"UPDATE Skynet.dbo.TB_STATUS SET DATETIME = '{0}', STATUS = '{1}', TYPE = '{2}' WHERE LINE_CODE = '{3}' and EQUIP_ID='{4}'", strSendtime, "SETUP", strRemote, strLinecode, strEquipid);

                nReturn = MSSql.SetData(query);

                if (nReturn == 0)
                    return 0;
            }
            else if (nCheck == 0) //없음
            {
                query = string.Format(@"INSERT INTO Skynet.dbo.TB_STATUS (DATETIME,LINE_CODE,PROCESS_CODE,EQUIP_ID,STATUS,TYPE) VALUES ('{0}','{1}','{2}','{3}','{4}','{5}')",
                strSendtime, strLinecode, "0000", strEquipid, "SETUP", strRemote);

                nReturn = MSSql.SetData(query);

                if (nReturn == 0)
                    return 0;
            }
            else
            {
                return 0;
            }

            /////Log 저장
            query = string.Format(@"INSERT INTO Skynet.dbo.TB_STATUS_HISTORY (DATETIME,LINE_CODE,PROCESS_CODE,EQUIP_ID,STATUS,TYPE) VALUES ('{0}','{1}','{2}','{3}','{4}','{5}')",
                strSendtime, strLinecode, "0000", strEquipid, "SETUP", strRemote);

            nReturn = MSSql.SetData(query);

            if (nReturn == 0)
                return 0;

            return 1;
        }

        public int Skynet_SM_Alive(string strLinecode, string strProcesscode, string strEquipid, int nAlive)
        {
            string query = "";
            query = string.Format(@"UPDATE Skynet.dbo.TB_STATUS SET ALIVE='{0}' WHERE LINE_CODE='{1}' and EQUIP_ID='{2}'", nAlive, strLinecode, strEquipid);

            int nJudge = MSSql.SetData(query);

            return nJudge;
        }

        public string Skynet_SM_Delete(string strLinecode, string strProcesscode, string strEquipid)
        {
            string query = "";

            query = string.Format("DELETE FROM Skynet.dbo.TB_STATUS WHERE LINE_CODE='{0}' and EQUIP_ID='{1}'", strLinecode, strEquipid);

            return query;
        }

        public int Skynet_PM_Start(string strLinecode, string strProcesscode, string strEquipid)
        {
            string strSendtime = string.Format("{0}{1:00}{2:00}{3:00}{4:00}{5:00}", DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);

            if (strLinecode == "" || strEquipid == "")
                return 0;

            string query = "";
            int nReturn = 0;

            int nCheck = Skynet_Exist_EqidCheck(strLinecode, strEquipid);

            if (nCheck != 1)
                return 0;

            if (nCheck == 1) //있음
            {
                query = string.Format(@"UPDATE Skynet.dbo.TB_STATUS SET DATETIME = '{0}', STATUS = '{1}', TYPE = '{2}' WHERE LINE_CODE = '{3}' and EQUIP_ID='{4}'", strSendtime, "START", "START", strLinecode, strEquipid);

                nReturn = MSSql.SetData(query);

                if (nReturn == 0)
                    return 0;
            }
            else if (nCheck == 0) //없음
            {
                query = string.Format(@"INSERT INTO Skynet.dbo.TB_STATUS (DATETIME,LINE_CODE,PROCESS_CODE,EQUIP_ID,STATUS,TYPE) VALUES ('{0}','{1}','{2}','{3}','{4}','{5}')",
                strSendtime, strLinecode, "0000", strEquipid, "START", "START");

                nReturn = MSSql.SetData(query);

                if (nReturn == 0)
                    return 0;
            }
            else
            {
                return 0;
            }
            /////Log 저장
            query = string.Format(@"INSERT INTO Skynet.dbo.TB_STATUS_HISTORY (DATETIME,LINE_CODE,PROCESS_CODE,EQUIP_ID,STATUS,TYPE) VALUES ('{0}','{1}','{2}','{3}','{4}','{5}')",
                strSendtime, strLinecode, "0000", strEquipid, "START", "START");

            nReturn = MSSql.SetData(query);

            if (nReturn == 0)
                return 0;

            return 1;
        }

        public int Skynet_PM_End(string strLinecode, string strProcesscode, string strEquipid)
        {
            string strSendtime = string.Format("{0}{1:00}{2:00}{3:00}{4:00}{5:00}", DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);

            if (strLinecode == "" || strEquipid == "")
                return 0;

            string query = "";
            int nReturn = 0;

            int nCheck = Skynet_Exist_EqidCheck(strLinecode, strEquipid);

            if (nCheck != 1)
                return 0;

            if (nCheck == 1) //있음
            {
                query = string.Format(@"UPDATE Skynet.dbo.TB_STATUS SET DATETIME = '{0}', STATUS = '{1}', TYPE = '{2}' WHERE LINE_CODE = '{3}' and EQUIP_ID='{4}'", strSendtime, "END", "END", strLinecode, strEquipid);

                nReturn = MSSql.SetData(query);

                if (nReturn == 0)
                    return 0;
            }
            else if (nCheck == 0) //없음
            {
                query = string.Format(@"INSERT INTO Skynet.dbo.TB_STATUS (DATETIME,LINE_CODE,PROCESS_CODE,EQUIP_ID,STATUS,TYPE) VALUES ('{0}','{1}','{2}','{3}','{4}','{5}')",
                strSendtime, strLinecode, "0000", strEquipid, "END", "END");

                nReturn = MSSql.SetData(query);

                if (nReturn == 0)
                    return 0;
            }
            else
            {
                return 0;
            }
            /////Log 저장
            query = string.Format(@"INSERT INTO Skynet.dbo.TB_STATUS_HISTORY (DATETIME,LINE_CODE,PROCESS_CODE,EQUIP_ID,STATUS,TYPE) VALUES ('{0}','{1}','{2}','{3}','{4}','{5}')",
                strSendtime, strLinecode, "0000", strEquipid, "END", "END");

            nReturn = MSSql.SetData(query);

            if (nReturn == 0)
                return 0;

            return 1;
        }
        public int Skynet_Set_Webservice_Faileddata(string strMnbr, string strBadge, string strAction, string strReelID, string strMtlType, string strSID, string strVendor, string strBatch, string strExpireddate, string strQty, string strUnit, string strLocation)
        {
            string strSendtime = string.Format("{0}{1:00}{2:00}{3:00}{4:00}{5:00}", DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);

            List<string> queryList = new List<string>();

            string query = "";

            query = string.Format(@"INSERT INTO Skynet.dbo.TB_WEBSERVICE_STB (DATETIME,MNBR,BADGE,ACTION,REEL_ID,MTL_TYPE,SID,VENDOR,BATCH,EXPIRED_DATE,QTY,UNIT,LOCATION) VALUES ('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}','{11}','{12}')",
                strSendtime, strMnbr, strBadge, strAction, strReelID, strMtlType, strSID, strVendor, strBatch, strExpireddate, strQty, strUnit, strLocation);

            queryList.Add(query);

            int nJudge = MSSql.SetData(queryList);

            return nJudge;
        }
        public DataTable Skynet_Get_Webservice_Faileddata(string strLocation)
        {
            string query = "";

            query = string.Format(@"SELECT * FROM Skynet.dbo.TB_WEBSERVICE_STB WHERE LOCATION={0}", strLocation);

            DataTable dt = MSSql.GetData(query);

            return dt;
        }

        public int Skynet_Webservice_Faileddata_Delete(string strReelid)
        {
            string query = "";

            query = string.Format("DELETE FROM Skynet.dbo.TB_WEBSERVICE_STB WHERE REEL_ID='{0}'", strReelid);

            int nJudge = MSSql.SetData(query);

            return nJudge;
        }
    }
    public struct Reelinfo
    {
        public string linecode;
        public string equipid;
        public bool bwebservice;

        public string portNo;
        public string uid;
        public string sid;
        public string lotid;
        public string qty;
        public string manufacturer;
        public string production_date;
        public string inch_info;
        public string destination;
        public string createby;
    }
}
