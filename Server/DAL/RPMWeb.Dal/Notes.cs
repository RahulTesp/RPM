using Azure.Storage.Blobs;
using RPMWeb.Data.Common;
using System.Data;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.Data.SqlClient;

using Document = iTextSharp.text.Document;
using PdfWriter = iTextSharp.text.pdf.PdfWriter;
using Paragraph = iTextSharp.text.Paragraph;
using iTextSharp.text.pdf;
using iTextSharp.text;
using Font = iTextSharp.text.Font;

namespace RPMWeb.Dal
{
    class Notes
    {
        public int AddNotes(NoteInfo info, string ConnectionString)
        {

            try
            {
                DataTable custDT = new DataTable();
                DataRow dr = custDT.NewRow();
                DataColumn col = null;
                col = new DataColumn("VitalId");
                custDT.Columns.Add(col);
                col = new DataColumn("Notes");
                custDT.Columns.Add(col);
                if (info.Notes.Count > 0)
                {
                    foreach (var Data in info.Notes)
                    {
                        dr = custDT.NewRow(); // have new row on each iteration
                        dr["VitalId"] = Data.VitalId;
                        dr["Notes"] = Data.Notes;
                        custDT.Rows.Add(dr);
                    }
                }
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    SqlParameter param = new SqlParameter();
                    SqlCommand command = new SqlCommand("usp_InsPatientNotes", connection);
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.Add("@patientnoteslist", SqlDbType.Structured).Value = custDT;
                    command.Parameters.Add("@CreatedBy", SqlDbType.NVarChar).Value = info.CreatedBy;
                    command.Parameters.Add("@IsCallNote", SqlDbType.Bit).Value = info.IsCallNote;
                    command.Parameters.Add("@IsEstablishedCall", SqlDbType.Bit).Value = info.IsEstablishedCall;
                    command.Parameters.Add("@CareteamMemberUserId", SqlDbType.Int).Value = info.CareteamMemberUserId;
                    command.Parameters.Add("@Duration", SqlDbType.Int).Value = info.Duration;
                    command.Parameters.Add("@NoteTypeId", SqlDbType.SmallInt).Value = info.NoteTypeId;
                    command.Parameters.Add("@PatientId ", SqlDbType.Int).Value = info.PatientId;
                    command.Parameters.Add("@PatientProgramId", SqlDbType.Int).Value = info.PatientProgramId;
                    SqlParameter returnParameter = command.Parameters.Add("RetVal", SqlDbType.Int);
                    returnParameter.Direction = ParameterDirection.ReturnValue;
                    connection.Open();
                    command.ExecuteNonQuery();
                    int id = (int)returnParameter.Value;
                    connection.Close();
                    if (id.Equals(0))
                    {
                        return 0;
                    }
                    else
                    {
                        return id;
                    }

                }
            }
            catch
            {
                throw;
            }

        }
        public bool UpdateNotes(NoteInfo info, string ConnectionString)
        {
            bool ret = true;
            try
            {
                DataTable custDT = new DataTable();
                DataRow dr = custDT.NewRow();
                DataColumn col = null;
                col = new DataColumn("VitalId");
                custDT.Columns.Add(col);
                col = new DataColumn("Notes");
                custDT.Columns.Add(col);
                if (info.Notes.Count > 0)
                {
                    foreach (var Data in info.Notes)
                    {
                        dr = custDT.NewRow();
                        dr["VitalId"] = Data.VitalId;
                        dr["Notes"] = Data.Notes;
                        custDT.Rows.Add(dr);
                    }
                }
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    SqlCommand command = new SqlCommand("usp_UpdPatientNotes", connection);
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add("@Id", SqlDbType.Int).Value = info.Id;
                    command.Parameters.Add("@NoteTypeId", SqlDbType.Int).Value = info.NoteTypeId;
                    command.Parameters.Add("@patientnoteslist", SqlDbType.Structured).Value = custDT;
                    command.Parameters.Add("@IsEstablishedCall", SqlDbType.Bit).Value = info.IsEstablishedCall;
                    command.Parameters.Add("@Duration", SqlDbType.Int).Value = info.Duration;
                    command.Parameters.Add("@ModifiedBy", SqlDbType.NVarChar).Value = info.CreatedBy;
                    SqlParameter returnParameter = command.Parameters.Add("RetVal", SqlDbType.Int);
                    returnParameter.Direction = ParameterDirection.ReturnValue;
                    connection.Open();
                    command.ExecuteNonQuery();
                    int id = (int)returnParameter.Value;
                    connection.Close();
                    if (id.Equals(0))
                    {
                        ret = false;
                    }
                }
            }
            catch
            {
                throw;
            }
            return ret;
        }
        public List<GetNotes> GetPatientCallNotes(int PatientId, DateTime StartDate, DateTime EndDate, string CreatedBy, string ConnectionString)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(ConnectionString))
                {
                    con.Open();

                    using (SqlCommand command = new SqlCommand("usp_GetPatientCallNotes", con))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@CreatedBy", CreatedBy);
                        command.Parameters.AddWithValue("@PatientId", PatientId);
                        command.Parameters.AddWithValue("@StartDate", StartDate);
                        command.Parameters.AddWithValue("@EndDate", EndDate);
                        SqlParameter returnParameter = command.Parameters.Add("RetVal", SqlDbType.Int);
                        returnParameter.Direction = ParameterDirection.ReturnValue;
                        SqlDataReader reader = command.ExecuteReader();
                        List<GetNotes> ListNotes = new List<GetNotes>();
                        while (reader.Read())
                        {
                            GetNotes ret = new GetNotes();
                            ret.Id = (int)reader["Id"];
                            ret.NoteTypeId = (!DBNull.Value.Equals(reader["NoteTypeId"])) ? Convert.ToInt32(reader["NoteTypeId"]) : 0;
                            ret.NoteType = reader["NoteType"].ToString();
                            ret.PatientId = (!DBNull.Value.Equals(reader["PatientId"])) ? Convert.ToInt32(reader["PatientId"]) : 0;
                            ret.CareteamMemberUserId = (!DBNull.Value.Equals(reader["CareteamMemberUserId"])) ? Convert.ToInt32(reader["CareteamMemberUserId"]) : 0;
                            ret.CompletedBy = reader["CompletedBy"].ToString();
                            ret.IsCallNote = (!DBNull.Value.Equals(reader["IsCallNote"])) ? Convert.ToBoolean(reader["IsCallNote"]) : false;
                            ret.IsEstablishedCall = (!DBNull.Value.Equals(reader["IsEstablishedCall"])) ? Convert.ToBoolean(reader["IsEstablishedCall"]) : false;
                            ret.Duration = (!DBNull.Value.Equals(reader["Duration"])) ? Convert.ToInt32(reader["Duration"]) : 0;
                            ret.CreatedOn= (!DBNull.Value.Equals(reader["CreatedOn"])) ? Convert.ToDateTime(reader["CreatedOn"]) : DateTime.MinValue;
                            ListNotes.Add(ret);
                        }
                        if (reader.FieldCount == 0)
                        {
                            return null;
                        }
                        return ListNotes;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public class ProgramTypes
        {
            public ProgramTypes(string _pname, string _ptype)
            {
                ProgramName = _pname;
                Type = _ptype;
            }
            public string ProgramName { get; set; }
            public string Type { get; set; }
        }
        public class ProgramCategory
        {
            public ProgramCategory(string _pname, int _mCategoryId,int _pId)
            {
                Name = _pname;
                MasterCategoryId = _mCategoryId;
                ParentId = _pId;
            }
            public string Name { get; set; }
            public int MasterCategoryId { get; set; }
            public int ParentId { get; set; }
        }
        public GetPatientNotesQA GetPatientNotes(string ProgramName,string Type,int PatientNoteId,   string CreatedBy, string connectionString)
        {
            DataSet ds;
            GetPatientNotesQA patientNotesQA = new GetPatientNotesQA();
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();

                    using (SqlCommand command = new SqlCommand("usp_GetPatientNoteswithQuestionsAnswer", con))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@PatientNoteId", PatientNoteId);
                        command.Parameters.AddWithValue("@ProgramName", ProgramName);
                        command.Parameters.AddWithValue("@Type", Type);
                        command.Parameters.AddWithValue("@CreatedBy", CreatedBy);

                        using (SqlDataAdapter sda = new SqlDataAdapter())
                        {
                            sda.SelectCommand = command;
                            using (ds = new DataSet())
                            {
                                sda.Fill(ds);
                                var NotecatId = 0;
                                var noteparentCatid = 0;
                                List<GetMainQuestion> questions = new List<GetMainQuestion>();
                                                                
                                foreach (DataRow dr3 in ds.Tables[0].Rows)
                                {
                                    if (!DBNull.Value.Equals(dr3["PatientId"]) &&(patientNotesQA.PatientId==0) )
                                    {
                                        patientNotesQA.PatientId = Convert.ToInt32(dr3["PatientId"]);
                                        patientNotesQA.PatientProgramId = Convert.ToInt32(dr3["PatientProgramId"]);
                                        patientNotesQA.NoteTypeId = Convert.ToInt32(dr3["NoteTypeId"]);
                                        patientNotesQA.IsEstablishedCall = Convert.ToBoolean(dr3["IsEstablishedCall"]);
                                        patientNotesQA.IsCareGiver = Convert.ToBoolean(dr3["IsCareGiver"]);
                                        patientNotesQA.IsCallNote = Convert.ToBoolean(dr3["IsCallNote"]);
                                        patientNotesQA.Duration = Convert.ToInt32(dr3["Duration"]);
                                        patientNotesQA.Notes = dr3["Notes"].ToString();
                                        patientNotesQA.CompletedByUserId = Convert.ToInt32(dr3["CompletedByUserId"]);
                                        //patientNotesQA.CallType = (!DBNull.Value.Equals(dr3["CallType"])) ? dr3["CallType"].ToString() : null;

                                    }
                                    
                                    if (
                                        (NotecatId != Convert.ToInt32(dr3["NotecategoryId"])) &&
                                        ((noteparentCatid != Convert.ToInt32(dr3["NoteCategoryParentId"])) ||
                                        (Convert.ToInt32(dr3["NoteCategoryParentId"]) == 0)) 
                                        )
                                    {
                                        NotecatId = Convert.ToInt32(dr3["NotecategoryId"]);
                                        GetMainQuestion question = new GetMainQuestion();

                                        List<GetSubQuestion> subQuestions = new List<GetSubQuestion>();
                                        int questionCategoryId = 0;
                                        if (Convert.ToInt32(dr3["NoteCategoryParentId"]) == 0)
                                        {
                                            question.QuestionId = Convert.ToInt32(dr3["NotecategoryId"]);
                                            question.Question = (dr3["NoteCategoryName"]).ToString();                                         
                                            question.IsMandatory= Convert.ToBoolean(dr3["IsMandatory"]);
                                            question.IsMultipleChoice = Convert.ToBoolean(dr3["IsMultipleChoise"]);
                                            question.Notes = (from DataRow dr in ds.Tables[1].Rows
                                                                       where (int)dr["NotecategoryId"] == question.QuestionId
                                                                       select (string)dr["Notes"]).FirstOrDefault();
                                            List<AnswerType> lstanstype = new List<AnswerType>();
                                            foreach (DataRow dr2 in ds.Tables[0].Rows)
                                            {
                                                if (
                                                (NotecatId == Convert.ToInt32(dr2["NotecategoryId"]))
                                                )
                                                {
                                                    //if (Convert.ToBoolean(dr2["checked"]) == true)
                                                    //{
                                                    //    question.Notes = dr2["HeaderNotes"].ToString();
                                                    //}
                                                     
                                                    AnswerType answerType = new AnswerType(
                                                        Convert.ToInt32(dr2["NoteTemplateId"]),
                                                        dr2["NoteTemplateName"].ToString(), 
                                                        Convert.ToBoolean(dr2["checked"]));
                                                    lstanstype.Add(answerType);

                                                }
                                            }
                                            question.AnswerTypes = lstanstype;
                                            question.SubQuestions = subQuestions;
                                            questions.Add(question);
                                        }
                                        else
                                        {

                                            foreach (DataRow drCaterory in ds.Tables[1].Rows)
                                            {
                                                if (Convert.ToInt32(dr3["NoteCategoryParentId"]) == Convert.ToInt32(drCaterory["NotecategoryId"])
                                                    && questionCategoryId != Convert.ToInt32(drCaterory["NotecategoryId"]))
                                                {
                                                    question.QuestionId = Convert.ToInt32(drCaterory["NotecategoryId"]);
                                                    question.Question = drCaterory["NoteCategoryName"].ToString();
                                                    questionCategoryId = question.QuestionId;
                                                    noteparentCatid = questionCategoryId;
                                                }
                                            }
                                            int subqnId = 0;
                                            foreach (DataRow dr4 in ds.Tables[0].Rows)
                                            {
                                                GetSubQuestion subQuestion = new GetSubQuestion();
                                                if ((questionCategoryId == Convert.ToInt32(dr4["NoteCategoryParentId"])) &&
                                                   subqnId != Convert.ToInt32(dr4["NotecategoryId"]))
                                                {
                                                    subqnId = Convert.ToInt32(dr4["NotecategoryId"]);
                                                    subQuestion.Question = dr4["NoteCategoryName"].ToString();
                                                    subQuestion.QuestionId = Convert.ToInt32(dr4["NotecategoryId"]);
                                                    
                                                    subQuestion.IsMandatory = Convert.ToBoolean(dr4["IsMandatory"]);
                                                    subQuestion.IsMultipleChoice = Convert.ToBoolean(dr4["IsMultipleChoise"]);
                                                    subQuestion.Notes = (from DataRow dr in ds.Tables[1].Rows
                                                                      where (int)dr["NotecategoryId"] == subQuestion.QuestionId
                                                                         select (string)dr["Notes"]).FirstOrDefault();
                                                    int subqntcategoryid = subQuestion.QuestionId;
                                                    List<AnswerType> lstanstype = new List<AnswerType>();
                                                    foreach (DataRow dr2 in ds.Tables[0].Rows)
                                                    {
                                                        if ((subqntcategoryid == Convert.ToInt32(dr2["NotecategoryId"])) &&
                                                       (questionCategoryId == Convert.ToInt32(dr2["NoteCategoryParentId"])))
                                                        {
                                                            //if (Convert.ToBoolean(dr2["checked"]) == true)
                                                            //{
                                                            //    question.Notes = dr2["HeaderNotes"].ToString();
                                                            //}
                                                              
                                                            AnswerType answerType = new AnswerType(
                                                            Convert.ToInt32(dr2["NoteTemplateId"]),
                                                            dr2["NoteTemplateName"].ToString(),
                                                             Convert.ToBoolean(dr2["checked"]));
                                                            lstanstype.Add(answerType);
                                                        }

                                                    }
                                                    subQuestion.AnswerTypes = lstanstype;
                                                    
                                                    subQuestions.Add(subQuestion);
                                                  
                                                }

                                            }
                                            question.AnswerTypes = new List<AnswerType>();
                                            question.SubQuestions = subQuestions;
                                            questions.Add(question);
                                        }
                                    }
                                    patientNotesQA.MainQuestions = questions;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {

                throw;
            }
            return patientNotesQA;
        }
        public NotesTypeMasterData GetMasterDataNotes(string ProgramName,string Type,  string CreatedBy,string connectionString)
        {
            DataSet ds;
           
            NotesTypeMasterData notesType = new NotesTypeMasterData();
            try
            {
              
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();

                    using (SqlCommand command = new SqlCommand("usp_GetMasterDataNotesQuestionsAnswer", con))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@ProgramName", ProgramName);
                        command.Parameters.AddWithValue("@Type", Type);
                        command.Parameters.AddWithValue("@CreatedBy", CreatedBy);

                        using (SqlDataAdapter sda = new SqlDataAdapter())
                        {
                            sda.SelectCommand = command;
                            using (ds = new DataSet())
                            {
                                sda.Fill(ds);
                               
                                            List<GetMainQuestion> questions = new List<GetMainQuestion>();
                                            var NotecatId = 0;
                                            var noteparentCatid = 0;
                                            foreach (DataRow dr3 in ds.Tables[1].Rows)
                                            {
                                                if (notesType.Type==null)
                                                notesType.Type = (dr3["NOTESMASTERCATEGORYTYPE"]).ToString();
                                                if (
                                                    
                                                    (NotecatId != Convert.ToInt32(dr3["NOTECATEGORYID"]))&&
                                                    ((noteparentCatid != Convert.ToInt32(dr3["NOTECATEGORYPARENTID"]))||
                                                    (Convert.ToInt32(dr3["NOTECATEGORYPARENTID"])==0)) 
                                                                                                 
                                                    )
                                                    {
                                                    NotecatId = Convert.ToInt32(dr3["NOTECATEGORYID"]);
                                                    GetMainQuestion question = new GetMainQuestion();
                                                    
                                                    List<GetSubQuestion> subQuestions = new List<GetSubQuestion>();
                                                    int questionCategoryId = 0;
                                                    if ((Convert.ToInt32(dr3["NOTECATEGORYPARENTID"]) == 0))
                                                    {
                                                        question.QuestionId = Convert.ToInt32(dr3["NOTECATEGORYID"]);
                                                        question.Question = (dr3["NOTECATEGORYNAME"]).ToString();
                                                        question.IsMandatory= Convert.ToBoolean(dr3["IsMandatory"]);
                                                        question.IsMultipleChoice= Convert.ToBoolean(dr3["IsMultipleChoise"]);
                                                        List<AnswerType> lstanstype = new List<AnswerType>();
                                                        foreach (DataRow dr2 in ds.Tables[1].Rows)
                                                        {
                                                            if (
                                                            (NotecatId == Convert.ToInt32(dr2["NOTECATEGORYID"]))
                                                            )
                                                            {
                                                                AnswerType answerType = new AnswerType(
                                                                    Convert.ToInt32(dr2["NoteTemplateId"]),
                                                                    dr2["NoteTemplateName"].ToString(),false);
                                                                lstanstype.Add(answerType);

                                                            }
                                                        }
                                                    question.AnswerTypes = lstanstype;
                                                    question.SubQuestions = subQuestions;
                                                    questions.Add(question);
                                                    }
                                                    else
                                                    {
                                                        
                                                        foreach (DataRow drCaterory in ds.Tables[2].Rows)
                                                        {
                                                            if(Convert.ToInt32(dr3["NOTECATEGORYPARENTID"])== Convert.ToInt32(drCaterory["NOTECATEGORYID"])
                                                                && questionCategoryId!= Convert.ToInt32(drCaterory["NOTECATEGORYID"]))
                                                            {
                                                                question.QuestionId= Convert.ToInt32(drCaterory["NOTECATEGORYID"]);
                                                                question.Question= drCaterory["NOTECATEGORYNAME"].ToString();
                                                                questionCategoryId = question.QuestionId;
                                                                noteparentCatid = questionCategoryId;
                                                            }
                                                        }
                                                        int subqnId = 0;
                                                        foreach(DataRow dr4 in ds.Tables[1].Rows)
                                                        {
                                                            GetSubQuestion subQuestion = new GetSubQuestion();
                                                            if (
                                                               (questionCategoryId == Convert.ToInt32(dr4["NOTECATEGORYPARENTID"]))&&
                                                               subqnId!= Convert.ToInt32(dr4["NOTECATEGORYID"]))
                                                            {
                                                                subqnId = Convert.ToInt32(dr4["NOTECATEGORYID"]);
                                                            subQuestion.Question = dr4["NOTECATEGORYNAME"].ToString();
                                                            subQuestion.QuestionId = Convert.ToInt32(dr4["NOTECATEGORYID"]);
                                                            subQuestion.IsMandatory = Convert.ToBoolean(dr4["IsMandatory"]);
                                                            subQuestion.IsMultipleChoice = Convert.ToBoolean(dr4["IsMultipleChoise"]);
                                                            int subqntcategoryid = subQuestion.QuestionId;
                                                            List<AnswerType> lstanstype = new List<AnswerType>();
                                                            foreach (DataRow dr2 in ds.Tables[1].Rows)
                                                            {
                                                                    if ((subqntcategoryid == Convert.ToInt32(dr2["NOTECATEGORYID"])) &&
                                                                   (questionCategoryId == Convert.ToInt32(dr2["NOTECATEGORYPARENTID"])))
                                                                    {
                                                                        AnswerType answerType = new AnswerType(
                                                                        Convert.ToInt32(dr2["NoteTemplateId"]),
                                                                        dr2["NoteTemplateName"].ToString(),false);
                                                                        lstanstype.Add(answerType);
                                                                    }

                                                             }
                                                                subQuestion.AnswerTypes = lstanstype;
                                                                subQuestions.Add(subQuestion);
                                                            }
                                                            
                                                        }
                                                        question.AnswerTypes = new List<AnswerType>();
                                                        question.SubQuestions = subQuestions;
                                                        questions.Add(question);
                                                    }
                                                }
                                            }
                                            notesType.MainQuestions = questions;
                                            
                            }
                        }
                    }                                          
                }
                return notesType;
            }
            catch (Exception ex)
            {
                //Console.WriteLine("Exception occured - class Name :" + this.GetType().Name + ",  Method Name : " + MethodBase.GetCurrentMethod().Name + ", Error Message :" + ex.Message + "");
                return null;
            }



        }
        public List<GetNotes> GetPatientReviewNotes(int PatientId, DateTime StartDate, DateTime EndDate, string CreatedBy, string ConnectionString)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(ConnectionString))
                {
                    con.Open();

                    using (SqlCommand command = new SqlCommand("usp_GetPatientReviewNotes", con))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@CreatedBy", CreatedBy);
                        command.Parameters.AddWithValue("@PatientId", PatientId);
                        command.Parameters.AddWithValue("@StartDate", StartDate);
                        command.Parameters.AddWithValue("@EndDate", EndDate);
                        SqlParameter returnParameter = command.Parameters.Add("RetVal", SqlDbType.Int);
                        returnParameter.Direction = ParameterDirection.ReturnValue;
                        SqlDataReader reader = command.ExecuteReader();
                        List<GetNotes> ListNotes = new List<GetNotes>();
                        while (reader.Read())
                        {
                            GetNotes ret = new GetNotes();
                            ret.Id = (int)reader["Id"];
                            ret.NoteTypeId = (!DBNull.Value.Equals(reader["NoteTypeId"])) ? Convert.ToInt32(reader["NoteTypeId"]) : 0;
                            ret.NoteType = reader["NoteType"].ToString();
                            ret.PatientId = (!DBNull.Value.Equals(reader["PatientId"])) ? Convert.ToInt32(reader["PatientId"]) : 0;
                            ret.CareteamMemberUserId = (!DBNull.Value.Equals(reader["CareteamMemberUserId"])) ? Convert.ToInt32(reader["CareteamMemberUserId"]) : 0;
                            ret.CompletedBy = reader["CompletedBy"].ToString();
                            ret.IsCallNote = (!DBNull.Value.Equals(reader["IsCallNote"])) ? Convert.ToBoolean(reader["IsCallNote"]) : false;
                            ret.Duration = (!DBNull.Value.Equals(reader["Duration"])) ? Convert.ToInt32(reader["Duration"]) : 0;
                            ret.CreatedOn = (!DBNull.Value.Equals(reader["CreatedOn"])) ? Convert.ToDateTime(reader["CreatedOn"]) : DateTime.MinValue;
                            ListNotes.Add(ret);
                        }


                        if (reader.FieldCount == 0)
                        {
                            return null;
                        }
                        return ListNotes;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public List<GetAllNotes> GetPatientNotes(int PatientId,int PatientProgramId, string NoteType, DateTime StartDate, DateTime EndDate, string CreatedBy, string ConnectionString)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(ConnectionString))
                {
                    con.Open();

                    using (SqlCommand command = new SqlCommand("usp_GetPatientNotes", con))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@CreatedBy", CreatedBy);
                        command.Parameters.AddWithValue("@PatientId", PatientId);
                        command.Parameters.AddWithValue("@PatientProgramId", PatientProgramId);
                        command.Parameters.AddWithValue("@NoteType", NoteType);                       
                        command.Parameters.AddWithValue("@StartDate", StartDate);                        
                        command.Parameters.AddWithValue("@EndDate", EndDate);
                        SqlParameter returnParameter = command.Parameters.Add("RetVal", SqlDbType.Int);
                        returnParameter.Direction = ParameterDirection.ReturnValue;
                        SqlDataReader reader = command.ExecuteReader();
                        List<GetAllNotes> ListNotes = new List<GetAllNotes>();
                        while (reader.Read())
                        {
                            GetAllNotes ret = new GetAllNotes();       
                            ret.Id= (!DBNull.Value.Equals(reader["Id"])) ? Convert.ToInt32(reader["Id"]) : 0;
                            ret.NoteTypeId = (!DBNull.Value.Equals(reader["NoteTypeId"])) ? Convert.ToInt32(reader["NoteTypeId"]) : 0;
                            ret.NoteType = reader["NoteType"].ToString();
                            ret.PatientId = (!DBNull.Value.Equals(reader["PatientId"])) ? Convert.ToInt32(reader["PatientId"]) : 0;
                            ret.PatientProgramId = (!DBNull.Value.Equals(reader["PatientProgramId"])) ? Convert.ToInt32(reader["PatientProgramId"]) : 0;
                            ret.CareteamMemberUserId = (!DBNull.Value.Equals(reader["CareteamMemberUserId"])) ? Convert.ToInt32(reader["CareteamMemberUserId"]) : 0;
                            ret.CompletedBy = reader["CompletedBy"].ToString();
                            ret.IsCareGiver = (!DBNull.Value.Equals(reader["IsCareGiver"])) ? Convert.ToBoolean(reader["IsCareGiver"]) : false;
                            ret.IsEstablished = (!DBNull.Value.Equals(reader["IsEstablishedCall"])) ? Convert.ToBoolean(reader["IsEstablishedCall"]) : false;
                            ret.IsPhoneCall = (!DBNull.Value.Equals(reader["IsCallNote"])) ? Convert.ToBoolean(reader["IsCallNote"]) : false;
                            ret.Duration = (!DBNull.Value.Equals(reader["Duration"])) ? Convert.ToInt32(reader["Duration"]) : 0;
                            ret.CreatedOn = (!DBNull.Value.Equals(reader["CreatedOn"])) ? Convert.ToDateTime(reader["CreatedOn"]) : DateTime.MinValue;
                            ret.CallType = (!DBNull.Value.Equals(reader["CallType"])) ? reader["CallType"].ToString() : null;
                            ListNotes.Add(ret);
                        }


                        if (reader.FieldCount == 0)
                        {
                            return null;
                        }
                        return ListNotes;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public NoteDetails GetPatientReviewNotesDetails(int PatientNoteId, string CreatedBy, string ConnectionString)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(ConnectionString))
                {
                    con.Open();

                    using (SqlCommand command = new SqlCommand("usp_GetPatientReviewNotesDetails", con))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@CreatedBy", CreatedBy);
                        command.Parameters.AddWithValue("@PatientNoteId", PatientNoteId);
                        SqlParameter returnParameter = command.Parameters.Add("RetVal", SqlDbType.Int);
                        returnParameter.Direction = ParameterDirection.ReturnValue;
                        SqlDataReader reader = command.ExecuteReader();
                        NoteDetails record = new NoteDetails();
                        record.Notes = new List<VitalNotes>();

                        DataTable dt = new DataTable();
                        while (!reader.IsClosed)
                        {

                            dt.Load(reader);

                        }
                        if (dt.Rows.Count == 0)
                        {
                            return null;
                        }
                        record.NoteTypeId = dt.Rows[0].Field<Int16>("NoteTypeId");
                        record.IsCallNote = Convert.ToInt32(dt.Rows[0].Field<bool>("IsCallNote"));
                        record.Duration = dt.Rows[0].Field<Int32>("Duration").ToString();
                        foreach (DataRow row in dt.Rows)
                        {
                            VitalNotes vitalNotes = new VitalNotes();
                            vitalNotes.VitalId = Convert.ToInt32(row["VitalId"]);
                            vitalNotes.Notes = row["Notes"].ToString();
                            record.Notes.Add(vitalNotes);
                        }
                        return record;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public NoteDetails GetPatientCallNotesDetails(int PatientNoteId, string CreatedBy, string ConnectionString)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(ConnectionString))
                {
                    con.Open();

                    using (SqlCommand command = new SqlCommand("usp_GetPatientCallNotesDetails", con))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@CreatedBy", CreatedBy);
                        command.Parameters.AddWithValue("@PatientNoteId", PatientNoteId);
                        SqlParameter returnParameter = command.Parameters.Add("RetVal", SqlDbType.Int);
                        returnParameter.Direction = ParameterDirection.ReturnValue;
                        SqlDataReader reader = command.ExecuteReader();
                        NoteDetails record = new NoteDetails();
                        record.Notes = new List<VitalNotes>();

                        DataTable dt = new DataTable();
                        while (!reader.IsClosed)
                        {

                            dt.Load(reader);

                        }
                        if (dt.Rows.Count == 0)
                        {
                            return null;
                        }
                        record.NoteTypeId = dt.Rows[0].Field<Int16>("NoteTypeId");
                        record.IsCallNote = Convert.ToInt32(dt.Rows[0].Field<bool>("IsCallNote"));
                        record.IsEstablishedCall = Convert.ToInt32(dt.Rows[0].Field<bool>("IsEstablishedCall"));
                        record.Duration = dt.Rows[0].Field<Int32>("Duration").ToString();
                        foreach (DataRow row in dt.Rows)
                        {
                            VitalNotes vitalNotes = new VitalNotes();
                            vitalNotes.VitalId = Convert.ToInt32(row["VitalId"]);
                            vitalNotes.Notes = row["Notes"].ToString();
                            record.Notes.Add(vitalNotes);
                        }
                        return record;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public int AddNotesDetails_V1(NoteInfo_V1 info, string ConnectionString)
        {

            try
            {
                string NoteHeader = "Insert into #NoteHeader([NotecategoryId],[Notes])values";
                string NoteHeaderinserts = string.Empty;
                string NoteHeaderInput = string.Empty;
                string NoteTemplateInput = string.Empty;
                string NoteTemplate = "Insert into #Notes([NotecategoryId],[NoteTemplateId],[IsActive])values";
                string NoteTemplateinserts = string.Empty;
                bool haveAns = false;

                List<MainQuestion> mainQuestions = info.MainQuestions;
                if (mainQuestions.Count > 0)
                {
                    foreach (MainQuestion mainQuestion in mainQuestions)
                    {
                        string notes = "";
                        if (!String.IsNullOrEmpty(mainQuestion.Notes))
                        {
                            notes = mainQuestion.Notes.Replace("'", "''");
                        }
                        string Headerinsertvalues = "('" + mainQuestion.QuestionId + "','" + notes + "'),";

                        NoteHeaderinserts = NoteHeaderinserts + Headerinsertvalues;
                        foreach (int Id in mainQuestion.AnswersIds)
                        {
                            haveAns = true;
                            string Templateinsertvalues = "(" + mainQuestion.QuestionId + "," + Id + ",1),";
                            NoteTemplateinserts = NoteTemplateinserts + Templateinsertvalues;
                        }

                    }
                    NoteHeader = NoteHeader + NoteHeaderinserts;
                    NoteHeaderInput = NoteHeader.Substring(0, NoteHeader.Length - 1);
                    NoteTemplate = NoteTemplate + NoteTemplateinserts;
                    NoteTemplateInput = NoteTemplate.Substring(0, NoteTemplate.Length - 1);
                }
                if (haveAns == false)
                {
                    NoteTemplateInput = string.Empty;
                }
                
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    SqlParameter param = new SqlParameter();
                    SqlCommand command = new SqlCommand("[usp_InsPatientNotesDetails_V1]", connection);
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add("@PatientId ", SqlDbType.Int).Value = info.PatientId;
                    command.Parameters.Add("@PatientProgramId", SqlDbType.Int).Value = info.PatientProgramId;
                    command.Parameters.Add("@NoteTypeId", SqlDbType.SmallInt).Value = info.NoteTypeId;
                    command.Parameters.Add("@NoteType", SqlDbType.NVarChar).Value = info.NoteType;
                    command.Parameters.Add("@IsEstablishedCall", SqlDbType.Bit).Value = info.IsEstablishedCall;
                    command.Parameters.Add("@IsCareGiver", SqlDbType.Bit).Value = info.IsCareGiver;
                    command.Parameters.Add("@IsCallNote", SqlDbType.Bit).Value = info.IsCallNote;
                    command.Parameters.Add("@Duration", SqlDbType.Int).Value = info.Duration;
                    command.Parameters.Add("@Notes", SqlDbType.NVarChar).Value = info.Notes;
                    command.Parameters.Add("@CompletedByUserId", SqlDbType.Int).Value = info.CompletedByUserId;
                    command.Parameters.Add("@NotesHeaderlist", SqlDbType.NVarChar).Value = NoteHeaderInput;
                    command.Parameters.Add("@PatientTemplateList", SqlDbType.NVarChar).Value = NoteTemplateInput;
                    command.Parameters.Add("@CreatedBy", SqlDbType.NVarChar).Value = info.CreatedBy;
                    if (info.calltype == null&&!info.IsCallNote)
                    {
                        command.Parameters.Add("@CallType", SqlDbType.NVarChar).Value = DBNull.Value;
                    }
                    else if(info.calltype!= null&&info.IsCallNote) 
                    { 
                        command.Parameters.Add("@CallType", SqlDbType.NVarChar).Value = info.calltype;
                    }
                    else
                    {
                        return 0;
                    }
                    
                    SqlParameter returnParameter = command.Parameters.Add("RetVal", SqlDbType.Int);
                    returnParameter.Direction = ParameterDirection.ReturnValue;
                    connection.Open();
                    command.ExecuteNonQuery();
                    int id = (int)returnParameter.Value;
                    connection.Close();
                    if (id.Equals(0))
                    {
                        return 0;
                    }
                    else
                    {
                        return id;
                    }

                }
            }
            catch (Exception ex)
            {
                throw;
            }

        }
        public int UpdateNotesDetails_V1(NoteInfo_V1 info, string ConnectionString)
        {

            try
            {
                string NoteHeader = "Insert into #NoteHeader([NotecategoryId],[Notes])values";
                string NoteHeaderinserts = string.Empty;
                string NoteTemplate = "Insert into #Notes([NotecategoryId],[NoteTemplateId],[IsActive])values";
                string NoteTemplateinserts = string.Empty;
                string NoteHeaderInput = string.Empty;
                string NoteTemplateInput = string.Empty;
                bool haveAns = false;
                List<MainQuestion> mainQuestions = info.MainQuestions;
                if (mainQuestions.Count > 0)
                {
                    foreach (MainQuestion mainQuestion in mainQuestions)
                    {
                        string notes = "";
                        if (!String.IsNullOrEmpty(mainQuestion.Notes))
                        {
                            notes = mainQuestion.Notes.Replace("'", "''");
                        }
                        string Headerinsertvalues = "('" + mainQuestion.QuestionId + "','" + notes + "'),";
                        NoteHeaderinserts = NoteHeaderinserts + Headerinsertvalues;
                        foreach (int Id in mainQuestion.AnswersIds)
                        {
                            haveAns = true;
                            string Templateinsertvalues = "(" + mainQuestion.QuestionId + "," + Id + ",1),";
                            NoteTemplateinserts = NoteTemplateinserts + Templateinsertvalues;
                        }

                    }
                    NoteHeader = NoteHeader + NoteHeaderinserts;
                    NoteHeaderInput = NoteHeader.Substring(0, NoteHeader.Length - 1);
                    NoteTemplate = NoteTemplate + NoteTemplateinserts;
                    NoteTemplateInput = NoteTemplate.Substring(0, NoteTemplate.Length - 1);
                }
                if (haveAns == false)
                {
                    NoteTemplateInput= string.Empty;
                }
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    SqlParameter param = new SqlParameter();
                    SqlCommand command = new SqlCommand("[usp_UpdPatientNotesDetails_V1]", connection);
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add("@Id ", SqlDbType.Int).Value = info.Id;
                    command.Parameters.Add("@NoteTypeId", SqlDbType.SmallInt).Value = info.NoteTypeId;
                    command.Parameters.Add("@NoteType", SqlDbType.NVarChar).Value = info.NoteType;
                    command.Parameters.Add("@IsEstablishedCall", SqlDbType.Bit).Value = info.IsEstablishedCall;
                    command.Parameters.Add("@IsCareGiver", SqlDbType.Bit).Value = info.IsCareGiver;
                    command.Parameters.Add("@IsCallNote", SqlDbType.Bit).Value = info.IsCallNote;
                    command.Parameters.Add("@Duration", SqlDbType.Int).Value = info.Duration;
                    command.Parameters.Add("@Notes", SqlDbType.NVarChar).Value = info.Notes;
                    command.Parameters.Add("@CompletedByUserId", SqlDbType.Int).Value = info.CompletedByUserId;
                    command.Parameters.Add("@NotesHeaderlist", SqlDbType.NVarChar).Value = NoteHeaderInput;
                    command.Parameters.Add("@PatientTemplateList", SqlDbType.NVarChar).Value = NoteTemplateInput;
                    command.Parameters.Add("@CreatedBy", SqlDbType.NVarChar).Value = info.CreatedBy;
                    SqlParameter returnParameter = command.Parameters.Add("RetVal", SqlDbType.Int);
                    returnParameter.Direction = ParameterDirection.ReturnValue;
                    connection.Open();
                    command.ExecuteNonQuery();
                    int id = (int)returnParameter.Value;
                    connection.Close();
                    if (id.Equals(0))
                    {
                        return 0;
                    }
                    else
                    {
                        return id;
                    }

                }
            }
            catch
            {
                throw;
            }

        }

        public bool DeletePatientNotes(int noteid, string ConnectionString)
        {
            bool response = false;
            using (SqlConnection con = new SqlConnection(ConnectionString))
            {
                SqlCommand command = new SqlCommand("usp_DelPatientNote", con);
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@noteid", noteid);
                con.Open();
                SqlParameter returnParameter = command.Parameters.Add("RetVal", SqlDbType.Int);
                returnParameter.Direction = ParameterDirection.ReturnValue;
                command.ExecuteNonQuery();
                int r = (int)returnParameter.Value;
                if (!r.Equals(0))
                { response = true; }
                con.Close();
                return response;
            }
        }

        public string CallLogByCareTeam(string Blob_Conn_String, string ContainerName, string ConnectionString, DateTime startDate, DateTime endDate, int? userId,string createdBy)
        {
            string Uri = "";
            try
            {
                BlobContainerClient containerClient = new BlobContainerClient(Blob_Conn_String, ContainerName);
                //string Filename = "Invoice" + DateTime.Now + "." + "pdf";
                string Filename = "CallLog" + DateTime.Now.Day+DateTime.Now.Month+DateTime.Now.Year+DateTime.Now.Hour+DateTime.Now.Minute+DateTime.Now.Second + "." + "pdf";
                var filePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\" + "rpmfolder" + Path.GetExtension(Filename);
                if (!Directory.Exists(filePath))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                }
                
                
                bool dataExist = GenerateReport(filePath, ConnectionString, startDate, endDate, userId,createdBy);
                if(!dataExist)
                {
                    return null;
                }
                if (File.Exists(filePath))
                {
                    byte[] values = File.ReadAllBytes(filePath);
                    using (MemoryStream stream = new MemoryStream(values))
                    {
                        var cli = containerClient.UploadBlob(Filename, stream);
                        var uri = containerClient.Uri;
                        Uri = uri + "/" + Filename;
                    }
                }
            }
            catch
            {
                throw;
            }
            return Uri;
        }
        public bool GenerateReport(String strPdfPath, string ConnectionString,DateTime startDate,DateTime endDate,int? userId,string createdBy)
        {
            try
            {
                DataSet ds;
                DataTable table1 = new DataTable();
                using (SqlConnection con = new SqlConnection(ConnectionString))
                {
                    con.Open();

                    using (SqlCommand command = new SqlCommand("usp_GetCallLogbyUser", con))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@CreatedBy", createdBy);
                        command.Parameters.AddWithValue("@StartDate", startDate);
                        command.Parameters.AddWithValue("@EndDate", endDate);
                        if(userId != null)
                        command.Parameters.AddWithValue("@UserId", userId);
                        using (SqlDataAdapter sda = new SqlDataAdapter())
                        {
                            //command.Connection = con;
                            sda.SelectCommand = command;
                            using (ds = new DataSet())
                            {
                                sda.Fill(ds);
                                ds.Tables[0].TableName = "CallLogs";
                               
                            }
                            if (ds.Tables[0].Rows.Count == 0)
                            {
                                return false;
                            }
                            DataSet ds2 = ds.Clone();
                            

                            table1.Columns.Add("Date");
                            table1.Columns.Add("AssignedMember");
                            table1.Columns.Add("EstablishedCalls");
                            table1.Columns.Add("Non-EstablishedCalls");
                            table1.Columns.Add("IncomingCalls");
                            var users = ds.Tables[0].AsEnumerable().Select(r => r.Field<string>("CreatedBy").ToString()).Distinct();
                            foreach(string name in users)
                            {
                                var dates = ds.Tables[0].AsEnumerable().Where(r => r.Field<string>("CreatedBy") == name).Select(r => r.Field<DateTime>("CreatedOn").Date).Distinct();
                                int? IncomingCount = 0;
                                int? EstablishedCall = 0;
                                int? NonEstablishedCall = 0;
                                foreach (DateTime date in dates)
                                {
                                     IncomingCount = ds.Tables[0].AsEnumerable().Where(p => p.Field<DateTime>("CreatedOn")==date && p.Field<string>("NoteType")=="Incoming Call"&&p.Field<string>("CreatedBy")==name)?.Sum(x => x.Field<int>("Count"));
                                     EstablishedCall = ds.Tables[0].AsEnumerable().Where(p => p.Field<DateTime>("CreatedOn")==date && p.Field<string>("NoteType")=="Established Call" &&p.Field<string>("CreatedBy")==name)?.Sum(x => x.Field<int>("Count"));
                                     NonEstablishedCall = ds.Tables[0].AsEnumerable().Where(p => p.Field<DateTime>("CreatedOn")==date && p.Field<string>("NoteType")=="Non-Established Call"&&p.Field<string>("CreatedBy")==name)?.Sum(x => x.Field<int>("Count"));
                                     DataRow rows = table1.NewRow();
                                    rows["Date"] = date.ToShortDateString();
                                    rows["AssignedMember"] = name;
                                    rows["EstablishedCalls"] =EstablishedCall;
                                    rows["Non-EstablishedCalls"] = NonEstablishedCall;
                                    rows["IncomingCalls"] = IncomingCount;
                                    table1.Rows.Add(rows);
                                }
                            }
                           
                        }
                    }
                }

                PdfPTable table = new PdfPTable(table1.Columns.Count);
                System.IO.FileStream fs = new FileStream(strPdfPath, FileMode.Create, FileAccess.Write, FileShare.None);
                Document document = new Document();
                var pgSize = new iTextSharp.text.Rectangle(1400, 1000);
                document.SetPageSize(pgSize);
                PdfWriter writer = PdfWriter.GetInstance(document, fs);
                document.Open();

                //Report Header
                string strHeader = "CALL LOG REPORT";
                BaseFont bfntHead = BaseFont.CreateFont(BaseFont.TIMES_ROMAN, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
                Font fntHead = new Font(bfntHead, 16, 1, BaseColor.BLACK);
                Paragraph prgHeading = new Paragraph();
                prgHeading.Alignment = Element.ALIGN_CENTER;
                prgHeading.Add(new Chunk(strHeader.ToUpper(), fntHead));
                document.Add(prgHeading);


                //Author
                Paragraph prgAuthor = new Paragraph();
                BaseFont btnAuthor = BaseFont.CreateFont(BaseFont.TIMES_ROMAN, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
                Font fntAuthor = new Font(btnAuthor, 12, 2, BaseColor.BLACK);
                //prgAuthor.Alignment = Element.ALIGN_RIGHT;
                prgAuthor.Alignment = Element.ALIGN_JUSTIFIED;
                prgAuthor.Add(new Chunk("\nDuration      : " + ""+startDate.ToShortDateString()+" To "+endDate.ToShortDateString()+"", fntAuthor));
                document.Add(prgAuthor);

                //Add a line seperation
                Paragraph pg = new Paragraph(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(0.0F, 100.0F, BaseColor.BLACK, Element.ALIGN_LEFT, 1)));
                document.Add(pg);

                //Add line break
                document.Add(new Chunk("\n", fntHead));

                //Write the table

                //Table header
                BaseFont btnColumnHeader = BaseFont.CreateFont(BaseFont.TIMES_ROMAN, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
                Font fntColumnHeader = new Font(btnColumnHeader, 10, 1, BaseColor.WHITE);

                for (int i = 0; i < table1.Columns.Count; i++)
                {
                    var cells = new PdfPCell() { HorizontalAlignment = Element.ALIGN_CENTER };
                    cells.BackgroundColor = BaseColor.GRAY;
                    cells.MinimumHeight=50;
                    cells.Padding = 10;
                    string coulmn = table1.Columns[i].ToString();

                    cells.Phrase = new Phrase(coulmn);
                    //  cells.AddElement(new Chunk(dtblTable.Columns[i].ColumnName.ToUpper(), fntColumnHeader));
                    table.AddCell(cells);
                }
                //table Data
                var cell = new PdfPCell() { HorizontalAlignment = Element.ALIGN_CENTER };
                for (int i = 0; i < table1.Rows.Count; i++)
                {
                    for (int j = 0; j < table1.Columns.Count; j++)
                    {
                        cell.BackgroundColor = i % 2 == 0
                            
                    ? BaseColor.LIGHT_GRAY : BaseColor.WHITE;
                        cell.MinimumHeight=50;
                        cell.Padding = 10;
                        string row = table1.Rows[i][j].ToString();

                        cell.Phrase = new Phrase(row);
                        table.AddCell(cell);
                    }
                }
               

                document.Add(table);
                document.Close();
                writer.Close();
                fs.Close();
                return true;
               
            }
            catch
            {
                throw;
            }
        }

        //NonestablishedCallReport begins

        public string NonEstablishedCallReport(string Blob_Conn_String, string ContainerName, string ConnectionString, DateTime startDate, DateTime endDate,int RoleId,string Createdby)
        {
            string Uri = "";
            try
            {
                BlobContainerClient containerClient = new BlobContainerClient(Blob_Conn_String, ContainerName);
                string Filename = "NonEstablishedCall" +DateTime.Now.Day+DateTime.Now.Month+DateTime.Now.Year+DateTime.Now.Hour+DateTime.Now.Minute+DateTime.Now.Second + "." + "pdf";
                var filePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\" + "rpmfolder" + Path.GetExtension(Filename);
                if (!Directory.Exists(filePath))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                }
                bool dataExist = GenerateNonEstablishedCallReport(filePath, ConnectionString, startDate, endDate,RoleId,Createdby);
                if (!dataExist)
                {
                    return null;
                }
                if (File.Exists(filePath))
                {
                    byte[] values = File.ReadAllBytes(filePath);
                    using (MemoryStream stream = new MemoryStream(values))
                    {
                        var cli = containerClient.UploadBlob(Filename, stream);
                        var uri = containerClient.Uri;
                        Uri = uri + "/" + Filename;
                    }
                }
            }
            catch
            {
                throw;
            }
            return Uri;
        }
        public bool GenerateNonEstablishedCallReport(String strPdfPath, string ConnectionString, DateTime startDate, DateTime endDate,int RoleId,string CreatedBy)
        {
            try
            {
                DataTable table;
                using (SqlConnection con = new SqlConnection(ConnectionString))
                {
                    con.Open();
                    using (SqlCommand command = new SqlCommand("usp_GetPatientNonEstablishedCallReport", con))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@StartDate", startDate);
                        command.Parameters.AddWithValue("@EndDate", endDate);
                        command.Parameters.AddWithValue("@RoleId", RoleId);
                        command.Parameters.AddWithValue("@CreatedBy", CreatedBy);
                        using (SqlDataAdapter sda = new SqlDataAdapter())
                        {
                            //command.Connection = con;
                            sda.SelectCommand = command;

                            using (table=new DataTable())
                            {
                                sda.Fill(table);
                                //ds.Tables[0].TableName = "NonEstablishedCallReport";

                            }
                            if (table.Rows.Count == 0)
                            {
                                return false;
                            }
                        }


                        PdfPTable pdftable = new PdfPTable(table.Columns.Count);
                        System.IO.FileStream fs = new FileStream(strPdfPath, FileMode.Create, FileAccess.Write, FileShare.None);
                        Document document = new Document();
                        var pgSize = new iTextSharp.text.Rectangle(1400, 1000);
                        document.SetPageSize(pgSize);
                        PdfWriter writer = PdfWriter.GetInstance(document, fs);
                        document.Open();

                        //Report Header
                        string strHeader = "Non Established Call Report";
                        BaseFont bfntHead = BaseFont.CreateFont(BaseFont.TIMES_ROMAN, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
                        Font fntHead = new Font(bfntHead, 16, 1, BaseColor.BLACK);
                        Paragraph prgHeading = new Paragraph();
                        prgHeading.Alignment = Element.ALIGN_CENTER;
                        prgHeading.Add(new Chunk(strHeader.ToUpper(), fntHead));
                        document.Add(prgHeading);


                        //Author
                        Paragraph prgAuthor = new Paragraph();
                        BaseFont btnAuthor = BaseFont.CreateFont(BaseFont.TIMES_ROMAN, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
                        Font fntAuthor = new Font(btnAuthor, 12, 2, BaseColor.BLACK);
                        //prgAuthor.Alignment = Element.ALIGN_RIGHT;
                        prgAuthor.Alignment = Element.ALIGN_JUSTIFIED;
                        prgAuthor.Add(new Chunk("\nDuration      : " + "" + startDate.ToShortDateString() + " To " + endDate.ToShortDateString() + "", fntAuthor));
                        document.Add(prgAuthor);

                        //Add a line seperation
                        Paragraph pg = new Paragraph(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(0.0F, 100.0F, BaseColor.BLACK, Element.ALIGN_LEFT, 1)));
                        document.Add(pg);

                        //Add line break
                        document.Add(new Chunk("\n", fntHead));

                        //Write the table

                        //Table header
                        BaseFont btnColumnHeader = BaseFont.CreateFont(BaseFont.TIMES_ROMAN, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
                        Font fntColumnHeader = new Font(btnColumnHeader, 10, 1, BaseColor.WHITE);

                        for (int i = 0; i < table.Columns.Count; i++)
                        {
                            var cells = new PdfPCell() { HorizontalAlignment = Element.ALIGN_CENTER };
                            cells.BackgroundColor = BaseColor.GRAY;
                            cells.MinimumHeight = 50;
                            cells.Padding = 10;
                            string coulmn = table.Columns[i].ToString();

                            cells.Phrase = new Phrase(coulmn);
                            //  cells.AddElement(new Chunk(dtblTable.Columns[i].ColumnName.ToUpper(), fntColumnHeader));
                            pdftable.AddCell(cells);
                        }
                        //table Data
                        var cell = new PdfPCell() { HorizontalAlignment = Element.ALIGN_CENTER };
                        for (int i = 0; i < table.Rows.Count; i++)
                        {
                            for (int j = 0; j <table.Columns.Count; j++)
                            {
                                cell.BackgroundColor = i % 2 == 0

                            ? BaseColor.LIGHT_GRAY : BaseColor.WHITE;
                                cell.MinimumHeight = 50;
                                cell.Padding = 10;
                                string row = table.Rows[i][j].ToString();

                                cell.Phrase = new Phrase(row);
                                pdftable.AddCell(cell);
                            }
                        }


                        document.Add(pdftable);
                        document.Close();
                        writer.Close();
                        fs.Close();
                        return true;

                    }
                }
            }
            catch
            {
                throw;
            }
        }
    }
}
