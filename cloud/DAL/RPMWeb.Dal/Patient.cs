using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using iTextSharp.text;
using iTextSharp.text.pdf;
using RPMWeb.Data.Common;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Reflection;
using DataTable = System.Data.DataTable;
using Font = iTextSharp.text.Font;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Spreadsheet;
using Workbook = DocumentFormat.OpenXml.Spreadsheet.Workbook;
using Worksheet = DocumentFormat.OpenXml.Spreadsheet.Worksheet;
using Sheets = DocumentFormat.OpenXml.Spreadsheet.Sheets;
using Values = RPMWeb.Data.Common.Values;
using Border = DocumentFormat.OpenXml.Spreadsheet.Border;
using Borders = DocumentFormat.OpenXml.Spreadsheet.Borders;
using CellFormat = DocumentFormat.OpenXml.Spreadsheet.CellFormat;
using System.Globalization;
using iText.Forms.Form.Element;
using Microsoft.AspNetCore.Http;

namespace RPMWeb.Dal
{
    public sealed class Patient
    {
        public enum CPTCodes
        {
            CPT99453,
            CPT99454,
            CPT99457,
            CPT99458,

        };
        public List<DraftPatient> GetDraftPatients(string CreatedBy, string ConnectionString)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(ConnectionString))
                {
                    con.Open();

                    using (SqlCommand command = new SqlCommand("usp_GetDraftedPatientList", con))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@CreatedBy", CreatedBy);
                        SqlParameter returnParameter = command.Parameters.Add("RetVal", SqlDbType.Int);
                        returnParameter.Direction = ParameterDirection.ReturnValue;
                        SqlDataReader reader = command.ExecuteReader();
                        List<DraftPatient> list = new List<DraftPatient>();
                        while (reader.Read())
                        {
                            DraftPatient ret = new DraftPatient();
                            ret.PatientId = Convert.ToInt32(reader["PatientId"]);
                            ret.PatientName = reader["PatientName"].ToString();
                            ret.CreatedOn = Convert.ToDateTime(reader["CreatedOn"]);
                            list.Add(ret);
                        }
                        if (reader.FieldCount == 0)
                        {
                            return null;
                        }
                        return list;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public bool UploadProfilePicture(int PatientId, IFormFile httpPostedFile, string filename, string Blob_Conn_String, string ContainerName, string UserName, string ConnectionString)
        {
            var docfiles = new List<string>();
            bool ret = false;
            if (string.IsNullOrEmpty(httpPostedFile.FileName))
            {
                return ret;
            }
            string ext = Path.GetExtension(httpPostedFile.FileName);
            ext = ext.ToLower();
            if (!string.IsNullOrEmpty(ext) || ext.Equals(".jpeg") ||
                ext.Equals(".jpg") || ext.Equals(".png"))
            {
                BlobContainerClient containerClient = new BlobContainerClient(Blob_Conn_String, ContainerName);
                //var filePath = HttpContext.Current.Server.MapPath("~/" +  postedFile.FileName);
                var filePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\" + "rpmfolder" + Path.GetExtension(httpPostedFile.FileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    httpPostedFile.CopyTo(fileStream);
                }
                docfiles.Add(filePath);
                if (File.Exists(filePath))
                {
                    byte[] values = File.ReadAllBytes(filePath);
                    if (values != null && values.Length > 100000) // 100KB
                    {
                        File.Delete(filePath); // not acceptable size
                        return ret;
                    }
                    using (MemoryStream stream = new MemoryStream(values))
                    {
                        string dtstring = DateTime.UtcNow.ToString("dd / MM / yyyy HH: mm:ss");
                        dtstring = dtstring.Replace(".", "");
                        dtstring = dtstring.Replace(" ", "");
                        dtstring = dtstring.Replace("-", "");
                        dtstring = dtstring.Replace("/", "");
                        dtstring = dtstring.Replace(":", "");
                        var cli = containerClient.UploadBlob(dtstring + "_" + filename, stream);
                        var uri = containerClient.Uri;
                        string Uri = uri + "/" + dtstring + "_" + filename;

                        using (SqlConnection connection = new SqlConnection(ConnectionString))
                        {
                            SqlCommand command = new SqlCommand("usp_UpdPatientProfilePicture", connection);
                            command.CommandType = CommandType.StoredProcedure;
                            command.Parameters.AddWithValue("@PatientId", PatientId);
                            command.Parameters.AddWithValue("@Modifiedby", UserName);
                            command.Parameters.AddWithValue("@Picture", Uri);
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
                            File.Delete(filePath);
                            ret = true;
                        }
                    }
                }
                if (docfiles.Count <= 0)
                {
                    ret = false;
                }
            }
            return ret;
        }
        /* public bool UploadPatientDocument(UploadPatientDocument doc, HttpPostedFile httpPostedFile, string ConnectionString)
         {
             try
             {
                 var docfiles = new List<string>();
                 bool ret = false;
                 if (string.IsNullOrEmpty(httpPostedFile.FileName))
                 {
                     return ret;
                 }
                 //string ext = Path.GetExtension(httpPostedFile.FileName);
                 //ext = ext.ToLower();
                 //if (!string.IsNullOrEmpty(ext) || ext.Equals(".jpeg") ||
                 //    ext.Equals(".jpg") || ext.Equals(".png"))
                 //{
                 BlobContainerClient containerClient = new BlobContainerClient(doc.Blob_Conn_String, doc.ContainerName);
                 //var filePath = HttpContext.Current.Server.MapPath("~/" +  postedFile.FileName);
                 var filePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\" + "rpmfolder" + Path.GetExtension(httpPostedFile.FileName);
                 httpPostedFile.SaveAs(filePath);
                 docfiles.Add(filePath);
                 if (File.Exists(filePath))
                 {
                     byte[] values = File.ReadAllBytes(filePath);
                     //if (values != null && values.Length > 100000) // 100KB
                     //{
                     //    File.Delete(filePath); // not acceptable size
                     //    return ret;
                     //}
                     using (MemoryStream stream = new MemoryStream(values))
                     {
                         var cli = containerClient.UploadBlob(doc.DocumentName + ' ' + DateTime.UtcNow, stream);
                         var uri = containerClient.Uri;
                         string Uri = uri + "/" + doc.DocumentName + ' ' + DateTime.UtcNow;
                         using (SqlConnection connection = new SqlConnection(ConnectionString))
                         {
                             SqlCommand command = new SqlCommand("usp_InsPatientProgramDocuments", connection);
                             command.CommandType = CommandType.StoredProcedure;
                             command.Parameters.AddWithValue("@PatientId", doc.PatientId);
                             command.Parameters.AddWithValue("@PatientProgramId", doc.PatientProgramId);
                             command.Parameters.AddWithValue("@DocumentType", doc.DocumentType);
                             command.Parameters.AddWithValue("@DocumentName", doc.DocumentName + ' ' + DateTime.UtcNow);
                             command.Parameters.AddWithValue("@DocumentUNC", Uri);
                             command.Parameters.AddWithValue("@CreatedBy", doc.CreatedBy);
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

                             ret = true;
                             File.Delete(filePath);
                         }
                     }
                     // }
                     if (docfiles.Count <= 0)
                     {
                         ret = false;
                     }
                 }
                 return ret;
             }
             catch (Exception)
             {

                 throw;
             }

         }*/

        public bool UploadPatientDocument(UploadPatientDocument doc, IFormFile httpPostedFile, string ConnectionString)
        {
            try
            {
                var docfiles = new List<string>();
                bool ret = true;
                if (string.IsNullOrEmpty(httpPostedFile.FileName))
                {
                    return ret;
                }
                //string ext = Path.GetExtension(httpPostedFile.FileName);
                //ext = ext.ToLower();
                //if (!string.IsNullOrEmpty(ext) || ext.Equals(".jpeg") ||
                //    ext.Equals(".jpg") || ext.Equals(".png"))
                //{

                BlobContainerClient containerClient = new BlobContainerClient(doc.Blob_Conn_String, doc.ContainerName);

                //BlobContainerClient containerClient = new BlobContainerClient(doc.Blob_Conn_String, doc.ContainerName);

                containerClient.CreateIfNotExists(PublicAccessType.Blob);
                //var filePath = HttpContext.Current.Server.MapPath("~/" +  postedFile.FileName);
                string fileName = httpPostedFile.FileName;
                string extension = Path.GetExtension(httpPostedFile.FileName);
                string name = Path.GetFileNameWithoutExtension(httpPostedFile.FileName);
                var filePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\" + "rpmfolder\\" + name;
                var fullfilepath = filePath + extension;
                bool folderExists = Directory.Exists(filePath);
                if (!folderExists)
                    Directory.CreateDirectory(filePath);
                using (var fullfileStream = new FileStream(fullfilepath, FileMode.Create))
                {
                    httpPostedFile.CopyTo(fullfileStream);
                }
                docfiles.Add(fullfilepath);
                if (File.Exists(fullfilepath))
                {
                    byte[] values = File.ReadAllBytes(fullfilepath);
                    //if (values != null && values.Length > 100000) // 100KB
                    //{
                    //    File.Delete(filePath); // not acceptable size
                    //    return ret;
                    //}
                    using (MemoryStream stream = new MemoryStream(values))
                    {
                        string dtstring = DateTime.UtcNow.ToString("dd / MM / yyyy HH: mm:ss");
                        dtstring = dtstring.Replace(".", "");
                        dtstring = dtstring.Replace(" ", "");
                        dtstring = dtstring.Replace("-", "");
                        dtstring = dtstring.Replace("/", "");
                        dtstring = dtstring.Replace(":", "");

                        var cli = containerClient.UploadBlob(doc.DocumentName.Replace(" ", "") + "_" + dtstring + extension, stream);
                        var uri = containerClient.Uri;

                        string Uri = uri + "/" + doc.DocumentName.Replace(" ", "") + "_" + dtstring + extension;

                        using (SqlConnection connection = new SqlConnection(ConnectionString))
                        {
                            SqlCommand command = new SqlCommand("usp_InsPatientProgramDocuments", connection);
                            command.CommandType = CommandType.StoredProcedure;
                            command.Parameters.AddWithValue("@PatientId", doc.PatientId);
                            command.Parameters.AddWithValue("@PatientProgramId", doc.PatientProgramId);
                            command.Parameters.AddWithValue("@DocumentType", doc.DocumentType);

                            command.Parameters.AddWithValue("@DocumentName", doc.DocumentName.Replace(" ", "") + "_" + dtstring + extension);

                            command.Parameters.AddWithValue("@DocumentUNC", Uri);
                            command.Parameters.AddWithValue("@CreatedBy", doc.CreatedBy);
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

                            File.Delete(fullfilepath);
                        }
                    }
                    // }
                    if (docfiles.Count <= 0)
                    {
                        ret = false;
                    }
                }
                return ret;
            }
            catch (Exception)
            {

                throw;
            }

        }
        public List<PatientDocuments> PatientDocuments(int PatientId, string CreatedBy, string ConnectionString)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(ConnectionString))
                {
                    con.Open();

                    using (SqlCommand command = new SqlCommand("usp_GetPatientDocuments", con))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@PatientId", PatientId);
                        command.Parameters.AddWithValue("@CreatedBy", CreatedBy);
                        SqlParameter returnParameter = command.Parameters.Add("RetVal", SqlDbType.Int);
                        returnParameter.Direction = ParameterDirection.ReturnValue;
                        SqlDataReader reader = command.ExecuteReader();
                        List<PatientDocuments> list = new List<PatientDocuments>();
                        while (reader.Read())
                        {
                            PatientDocuments ret = new PatientDocuments();
                            ret.Id = Convert.ToInt32(reader["Id"]);
                            ret.DocumentType = reader["DocumentType"].ToString();
                            ret.DocumentName = reader["DocumentName"].ToString();
                            ret.DocumentUNC = reader["DocumentUNC"].ToString();
                            ret.CreatedOn = Convert.ToDateTime(reader["CreatedOn"]);
                            list.Add(ret);
                        }
                        if (reader.FieldCount == 0)
                        {
                            return null;
                        }
                        return list;

                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public PatientDetails GetDraftPatientDetails(int PatientId, string CreatedBy, string ConnectionString)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(ConnectionString))
                {
                    con.Open();

                    using (SqlCommand command = new SqlCommand("usp_GetPatientBasicInfoDrafted", con))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@PatientId", PatientId);
                        command.Parameters.AddWithValue("@CreatedBy", CreatedBy);
                        SqlParameter returnParameter = command.Parameters.Add("RetVal", SqlDbType.Int);
                        returnParameter.Direction = ParameterDirection.ReturnValue;
                        SqlDataReader reader = command.ExecuteReader();
                        PatientDetails patientDetails = new PatientDetails();
                        while (reader.Read())
                        {
                            patientDetails.PatientId = Convert.ToInt32(reader["PatientId"]);
                            patientDetails.UserName = reader["UserName"].ToString();
                            patientDetails.FirstName = reader["FirstName"].ToString();
                            patientDetails.MiddleName = reader["MiddleName"].ToString();
                            patientDetails.LastName = reader["LastName"].ToString();
                            patientDetails.DOB = Convert.ToDateTime(reader["DOB"]);
                            patientDetails.Gender = reader["Gender"].ToString();
                            patientDetails.Height = Convert.ToSingle(reader["Height"]);
                            patientDetails.Weight = Convert.ToSingle(reader["Weight"]);
                            patientDetails.MobileNo = reader["MobileNo"].ToString();
                            patientDetails.AlternateMobNo = reader["AlternateMobNo"].ToString();
                            patientDetails.Email = reader["Email"].ToString();
                            patientDetails.Address1 = reader["Address1"].ToString();
                            patientDetails.Address2 = reader["Address2"].ToString();
                            patientDetails.CityID = Convert.ToInt32(reader["CityID"]);
                            patientDetails.StateId = Convert.ToInt32(reader["StateId"]);
                            patientDetails.CountryId = Convert.ToInt32(reader["CountryId"]);
                            patientDetails.ZipCode = reader["ZipCode"].ToString();
                            patientDetails.Language = reader["Language"].ToString();
                            patientDetails.Contact1Name = reader["Contact1Name"].ToString();
                            patientDetails.Contact1RelationName = reader["Contact1RelationName"].ToString();
                            patientDetails.Contact1Phone = reader["Contact1Phone"].ToString();
                            patientDetails.Contact2Name = reader["Contact2Name"].ToString();
                            patientDetails.Contact2RelationName = reader["Contact2RelationName"].ToString();
                            patientDetails.Contact2Phone = reader["Contact2Phone"].ToString();
                            patientDetails.Notes = reader["Notes"].ToString();
                            patientDetails.TimeZoneID = Convert.ToInt32(reader["TimeZoneID"]);
                            patientDetails.Picture = reader["Picture"].ToString();
                            patientDetails.CallTime = reader["CallTime"].ToString();
                            patientDetails.Preference1 = reader["Preference1"].ToString();
                            patientDetails.Preference2 = reader["Preference2"].ToString();
                            patientDetails.Preference3 = reader["Preference3"].ToString();
                            patientDetails.OrganizationID = Convert.ToInt32(reader["OrganizationID"]);
                            patientDetails.ClinicName = reader["ClinicName"].ToString();
                            patientDetails.ClinicCode = reader["ClinicCode"].ToString();
                            patientDetails.Status = reader["Status"].ToString();
                        }
                        if (reader.FieldCount == 0)
                        {
                            return null;
                        }
                        return patientDetails;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public PatientInfoForProgramInsert GetPatientInfoForProgramInsert(int PatientId, string CreatedBy, string ConnectionString)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(ConnectionString))
                {
                    con.Open();

                    using (SqlCommand command = new SqlCommand("usp_GetPatientInfoForProgramInsert", con))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@PatientId", PatientId);
                        command.Parameters.AddWithValue("@CreatedBy", CreatedBy);
                        SqlParameter returnParameter = command.Parameters.Add("RetVal", SqlDbType.Int);
                        returnParameter.Direction = ParameterDirection.ReturnValue;
                        SqlDataReader reader = command.ExecuteReader();
                        PatientInfoForProgramInsert patientDetails = new PatientInfoForProgramInsert();
                        while (reader.Read())
                        {
                            patientDetails.OrganizationId = (!DBNull.Value.Equals(reader["OrganizationId"])) ? Convert.ToInt32(reader["OrganizationId"]) : 0;
                            patientDetails.ConsultationDate = Convert.ToDateTime(reader["CreatedOn"]);
                        }
                        if (reader.FieldCount == 0)
                        {
                            return null;
                        }
                        return patientDetails;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public NewPatientCredential SavePatient(PatientDetails Info, string ConnectionString)
        {
            NewPatientCredential newPatientCredential = new NewPatientCredential();
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                SqlCommand command = new SqlCommand("usp_InsPatient", connection);
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@Contact1Name", Info.Contact1Name);
                command.Parameters.AddWithValue("@Contact1RelationName", Info.Contact1RelationName);
                command.Parameters.AddWithValue("@Contact1Phone", Info.Contact1Phone);
                command.Parameters.AddWithValue("@Contact2Name", Info.Contact2Name);
                command.Parameters.AddWithValue("@Contact2RelationName", Info.Contact2RelationName);
                command.Parameters.AddWithValue("@Contact2Phone", Info.Contact2Phone);
                command.Parameters.AddWithValue("@Status", Info.Status);
                command.Parameters.AddWithValue("@Height", Info.Height);
                command.Parameters.AddWithValue("@Weight", Info.Weight);
                command.Parameters.AddWithValue("@CallTime", Info.CallTime);
                command.Parameters.AddWithValue("@Preference1", Info.Preference1);
                command.Parameters.AddWithValue("@Preference2", Info.Preference2);
                command.Parameters.AddWithValue("@Preference3", Info.Preference3);
                command.Parameters.AddWithValue("@Notes", Info.Notes);
                command.Parameters.AddWithValue("@CreatedBy", Info.UserName);
                command.Parameters.AddWithValue("@OrganizationID", Info.OrganizationID);
                command.Parameters.AddWithValue("@MobileNo", Info.MobileNo);
                command.Parameters.AddWithValue("@AlternateMobNo", Info.AlternateMobNo);
                command.Parameters.AddWithValue("@Email", Info.Email);
                command.Parameters.AddWithValue("@UserStatus", Info.Status);
                command.Parameters.AddWithValue("@FirstName", Info.FirstName);
                command.Parameters.AddWithValue("@MiddleName", Info.MiddleName);
                command.Parameters.AddWithValue("@LastName", Info.LastName);
                command.Parameters.AddWithValue("@DisplayName", Info.FirstName);
                command.Parameters.AddWithValue("@DOB", Info.DOB);
                command.Parameters.AddWithValue("@Gender", Info.Gender);
                command.Parameters.AddWithValue("@Address1", Info.Address1);
                command.Parameters.AddWithValue("@Address2", Info.Address2);
                command.Parameters.AddWithValue("@CityID", Info.CityID);
                command.Parameters.AddWithValue("@StateId", Info.StateId);
                command.Parameters.AddWithValue("@CountryId", Info.CountryId);
                command.Parameters.AddWithValue("@ZipCode", Info.ZipCode);
                command.Parameters.AddWithValue("@TimeZoneID", Info.TimeZoneID);
                command.Parameters.AddWithValue("@Language", Info.Language);



                SqlParameter returnParameter = command.Parameters.Add("RetVal", SqlDbType.Int);
                returnParameter.Direction = ParameterDirection.ReturnValue;
                connection.Open();
                SqlDataReader reader = command.ExecuteReader();


                while (reader.Read())
                {

                    newPatientCredential.PatientId = Convert.ToInt32(reader["patientId"]);
                    newPatientCredential.password = Convert.ToString(reader["RandomPassword"]);



                }
                if (reader.FieldCount == 0)
                {
                    return null;
                }
                connection.Close();


            }
            return newPatientCredential;
        }
        public bool UpdatePatient(PatientDetails Info, string ConnectionString)
        {

            bool ret = true;
            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    SqlCommand command = new SqlCommand("usp_UpdPatient", connection);
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@PatientId", Info.PatientId);
                    command.Parameters.AddWithValue("@Contact1Name", Info.Contact1Name);
                    command.Parameters.AddWithValue("@Contact1RelationName", Info.Contact1RelationName);
                    command.Parameters.AddWithValue("@Contact1Phone", Info.Contact1Phone);
                    command.Parameters.AddWithValue("@Contact2Name", Info.Contact2Name);
                    command.Parameters.AddWithValue("@Contact2RelationName", Info.Contact2RelationName);
                    command.Parameters.AddWithValue("@Contact2Phone", Info.Contact2Phone);
                    command.Parameters.AddWithValue("@Height", Info.Height);
                    command.Parameters.AddWithValue("@Weight", Info.Weight);
                    command.Parameters.AddWithValue("@CityID", Info.CityID);
                    command.Parameters.AddWithValue("@CallTime", Info.CallTime);
                    command.Parameters.AddWithValue("@Preference1", Info.Preference1);
                    command.Parameters.AddWithValue("@Preference2", Info.Preference2);
                    command.Parameters.AddWithValue("@Preference3", Info.Preference3);
                    command.Parameters.AddWithValue("@Notes", Info.Notes);
                    command.Parameters.AddWithValue("@Language", Info.Language);
                    command.Parameters.AddWithValue("@ModifiedBy", Info.UserName);
                    command.Parameters.AddWithValue("@MobileNo", Info.MobileNo);
                    command.Parameters.AddWithValue("@AlternateMobNo", Info.AlternateMobNo);
                    command.Parameters.AddWithValue("@Email", Info.Email);
                    command.Parameters.AddWithValue("@FirstName", Info.FirstName);
                    command.Parameters.AddWithValue("@MiddleName", Info.MiddleName);
                    command.Parameters.AddWithValue("@LastName", Info.LastName);
                    command.Parameters.AddWithValue("@DOB", Info.DOB);
                    command.Parameters.AddWithValue("@Gender", Info.Gender);
                    command.Parameters.AddWithValue("@Address1", Info.Address1);
                    command.Parameters.AddWithValue("@Address2", Info.Address2);
                    command.Parameters.AddWithValue("@StateId", Info.StateId);
                    command.Parameters.AddWithValue("@CountryId", Info.CountryId);
                    command.Parameters.AddWithValue("@ZipCode", Info.ZipCode);
                    command.Parameters.AddWithValue("@TimeZoneID", Info.TimeZoneID);
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
            catch (Exception ex)
            {
                throw ex;
            }
            return ret;
        }
        public ProgramDetailsMasterDataAddPatient GetProgramDetailsMasterData(int RoleId, string UserName, string ConnectionString)
        {
            DataSet ds;
            try
            {
                ProgramDetailsMasterDataAddPatient programDetailsMasterDataAddPatient = new ProgramDetailsMasterDataAddPatient();

                using (SqlConnection con = new SqlConnection(ConnectionString))
                {
                    con.Open();
                    using (SqlCommand command = new SqlCommand("usp_GetMasterDataForAddPatient", con))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@RoleId", RoleId);
                        command.Parameters.AddWithValue("@CreatedBy", UserName);
                        using (SqlDataAdapter sda = new SqlDataAdapter())
                        {
                            sda.SelectCommand = command;
                            using (ds = new DataSet())
                            {
                                sda.Fill(ds);

                                List<ProgramDetailsMasterDataProgram> lstpDMD = new List<ProgramDetailsMasterDataProgram>();
                                string[] ProgramNames = ds.Tables[0].AsEnumerable().Select(s => s.Field<string>("ProgramName")).Distinct().ToArray<string>();
                                string[] VitalNames = ds.Tables[1].AsEnumerable().Select(s => s.Field<string>("VitalName")).Distinct().ToArray<string>();

                                foreach (string ProgramName in ProgramNames)
                                {
                                    ProgramDetailsMasterDataProgram programDetailsMD = new ProgramDetailsMasterDataProgram();
                                    foreach (DataRow drM in ds.Tables[0].Rows)
                                    {
                                        if (ProgramName == drM["ProgramName"].ToString())
                                        {

                                            List<GoalDetails> lstGoalD = new List<GoalDetails>();
                                            foreach (DataRow dr in ds.Tables[0].Rows)
                                            {
                                                if (ProgramName == dr["ProgramName"].ToString())
                                                {

                                                    programDetailsMD.ProgramId = Convert.ToInt32(dr["ProgramId"]);
                                                    programDetailsMD.ProgramName = dr["ProgramName"].ToString();
                                                    programDetailsMD.Duration = Convert.ToInt32(dr["Duration"]);
                                                    GoalDetails gd = new GoalDetails(Convert.ToInt32(dr["GoalId"]),
                                                                                    dr["Goal"].ToString(),
                                                                                    dr["GoalDescription"].ToString());
                                                    if (!lstGoalD.Any(item => item.Id == gd.Id))
                                                    {
                                                        lstGoalD.Add(gd);
                                                    }

                                                }
                                            }
                                            programDetailsMD.goalDetails = lstGoalD;
                                            List<ProgramVitals> lstV = new List<ProgramVitals>();
                                            foreach (string VitalName in VitalNames)
                                            {
                                                foreach (DataRow dr in ds.Tables[1].Rows)
                                                {
                                                    if (VitalName == dr["VitalName"].ToString() && ProgramName == dr["ProgramName"].ToString())
                                                    {
                                                        ProgramVitals pv = new ProgramVitals();
                                                        pv.VitalId = Convert.ToInt32(dr["VitalId"]);
                                                        pv.VitalName = dr["VitalName"].ToString();
                                                        List<ProgramVitalDignostics> lstVD = new List<ProgramVitalDignostics>();
                                                        /*foreach (DataRow dr2 in ds.Tables[1].Rows)
                                                        {
                                                            if (pv.VitalId == Convert.ToInt32(dr2["VitalId"]))
                                                            {
                                                                ProgramVitalDignostics pd = new ProgramVitalDignostics();
                                                                pd.DiagnosisCode = dr2["DiagnosisCode"].ToString();
                                                                pd.DiagnosisName = dr2["DiagnosisName"].ToString();
                                                                if (!lstVD.Any(item => item.DiagnosisCode == pd.DiagnosisCode))
                                                                {
                                                                    lstVD.Add(pd);
                                                                }
                                                            }

                                                        }*/
                                                        pv.Dignostics = lstVD;
                                                        if (!lstV.Any(item => item.VitalId == pv.VitalId))
                                                        {
                                                            lstV.Add(pv);
                                                        }
                                                    }
                                                }

                                            }
                                            programDetailsMD.Vitals = lstV;

                                        }
                                    }
                                    if (!lstpDMD.Any(item => item.ProgramId == programDetailsMD.ProgramId))
                                    {
                                        lstpDMD.Add(programDetailsMD);
                                    }
                                }

                                programDetailsMasterDataAddPatient.ProgramDetailsMasterData = lstpDMD;

                                //Clinic details
                                List<ClinicDetails> lstclinicDetails = new List<ClinicDetails>();
                                foreach (DataRow dr in ds.Tables[2].Rows)
                                {
                                    ClinicDetails clinicDetails = new ClinicDetails();
                                    clinicDetails.Id = Convert.ToInt32(dr["Id"]);
                                    clinicDetails.ClinicName = dr["ClinicName"].ToString();
                                    clinicDetails.ClinicCode = dr["ClinicCode"].ToString();
                                    lstclinicDetails.Add(clinicDetails);
                                }
                                programDetailsMasterDataAddPatient.ClinicDetails = lstclinicDetails;
                                //physicianDetails
                                List<PhysicianDetails> lstphysicianDetails = new List<PhysicianDetails>();
                                foreach (DataRow dr in ds.Tables[3].Rows)
                                {
                                    PhysicianDetails physicianDetails = new PhysicianDetails();
                                    physicianDetails.UserId = Convert.ToInt32(dr["UserId"]);
                                    physicianDetails.ClinicID = Convert.ToInt32(dr["ClinicID"]);
                                    physicianDetails.PhysicianName = dr["PhysicianName"].ToString();
                                    lstphysicianDetails.Add(physicianDetails);
                                }
                                programDetailsMasterDataAddPatient.PhysicianDetails = lstphysicianDetails;

                                //Cities
                                //List<Cities> lstcities = new List<Cities>();
                                //foreach (DataRow dr in ds.Tables[3].Rows)
                                //{
                                //    Cities cities = new Cities();
                                //    cities.Id = Convert.ToInt32(dr["Id"]);
                                //    cities.Name = dr["Name"].ToString();
                                //    cities.StateId = (!DBNull.Value.Equals(dr["StateId"]) ? Convert.ToInt32(dr["StateId"]) : 0); 
                                //    cities.State = dr["State"].ToString();
                                //    cities.StateCode = dr["StateCode"].ToString();
                                //    cities.TimeZoneId = (!DBNull.Value.Equals(dr["TimeZoneId"])?(Convert.ToInt32(dr["TimeZoneId"])):0);
                                //    cities.Zipcode = dr["Zipcode"].ToString();
                                //    lstcities.Add(cities);
                                //}
                                //programDetailsMasterDataAddPatient.Cities = lstcities;

                                //CareTeamMembers
                                List<CareTeamMembers> lstcareTeamMembers = new List<CareTeamMembers>();
                                foreach (DataRow dr in ds.Tables[4].Rows)
                                {
                                    CareTeamMembers careTeamMembers = new CareTeamMembers();
                                    careTeamMembers.UserId = Convert.ToInt32(dr["UserId"]);
                                    careTeamMembers.CareTeamId = Convert.ToInt32(dr["CareTeamId"]);
                                    careTeamMembers.CareTeamMemberName = dr["CareTeamMemberName"].ToString();
                                    lstcareTeamMembers.Add(careTeamMembers);
                                }
                                programDetailsMasterDataAddPatient.CareTeamMembers = lstcareTeamMembers;

                                //PatientsStatus
                                List<PatientsStatus> lstpatientsStatuses = new List<PatientsStatus>();
                                foreach (DataRow dr in ds.Tables[5].Rows)
                                {
                                    PatientsStatus patientsStatus = new PatientsStatus();
                                    patientsStatus.Id = Convert.ToInt32(dr["Id"]);
                                    patientsStatus.PatientStatus = dr["PatientStatus"].ToString();
                                    lstpatientsStatuses.Add(patientsStatus);
                                }
                                programDetailsMasterDataAddPatient.PatientStatuses = lstpatientsStatuses;

                                //Schedules
                                List<ScheduleList> lstSchedule = new List<ScheduleList>();
                                foreach (DataRow dr in ds.Tables[6].Rows)
                                {
                                    ScheduleList scheduleList = new ScheduleList();
                                    scheduleList.Id = Convert.ToInt32(dr["Id"]);
                                    scheduleList.Name = dr["Name"].ToString();
                                    lstSchedule.Add(scheduleList);
                                }
                                programDetailsMasterDataAddPatient.ScheduleLists = lstSchedule;
                                //VitalSchedules
                                List<VitalScheduleList> lstVitalSchedule = new List<VitalScheduleList>();
                                foreach (DataRow dr in ds.Tables[7].Rows)
                                {
                                    VitalScheduleList vitalScheduleList = new VitalScheduleList();
                                    vitalScheduleList.Id = Convert.ToInt32(dr["Id"]);
                                    vitalScheduleList.Name = dr["Name"].ToString();
                                    vitalScheduleList.VitalId = Convert.ToInt32(dr["VitalId"]);
                                    lstVitalSchedule.Add(vitalScheduleList);
                                }
                                programDetailsMasterDataAddPatient.VitalScheduleLists = lstVitalSchedule;
                                //BillingCodes
                                List<BillingCodes> lstBilling = new List<BillingCodes>();
                                foreach (DataRow dr in ds.Tables[8].Rows)
                                {
                                    BillingCodes billingCodes = new BillingCodes();
                                    billingCodes.Id = Convert.ToInt32(dr["Id"]);
                                    billingCodes.BillingCode = dr["BillingCode"].ToString();
                                    billingCodes.FrequencyPeriod = dr["FrequencyPeriod"].ToString();
                                    billingCodes.Id = Convert.ToInt32(dr["TargetReadings"]);
                                    billingCodes.TargetDuration = dr["TargetDuration"].ToString();
                                    billingCodes.Description = dr["Description"].ToString();
                                    lstBilling.Add(billingCodes);
                                }
                                programDetailsMasterDataAddPatient.BillingCodes = lstBilling;
                                //Device Communication Type
                                List<DeviceCommunicationType> lstDevComm = new List<DeviceCommunicationType>();
                                foreach (DataRow dr in ds.Tables[9].Rows)
                                {
                                    DeviceCommunicationType DevComm = new DeviceCommunicationType();
                                    DevComm.Id = Convert.ToInt32(dr["Id"]);
                                    DevComm.DeviceCommunicationTypeName = dr["DeviceCommunicationTypeName"].ToString();
                                    lstDevComm.Add(DevComm);
                                }
                                programDetailsMasterDataAddPatient.DeviceCommunicationTypes = lstDevComm;
                                //Device Details
                                List<DeviceDetails> lstDevDetail = new List<DeviceDetails>();
                                foreach (DataRow dr in ds.Tables[10].Rows)
                                {
                                    DeviceDetails DevDetail = new DeviceDetails();
                                    DevDetail.VitalId = Convert.ToInt32(dr["VitalId"]);
                                    DevDetail.DeviceName = dr["DeviceName"].ToString();
                                    DevDetail.DeviceVendorId = Convert.ToInt32(dr["DeviceVendorId"]);
                                    DevDetail.DeviceNumber = dr["DeviceNumber"].ToString();
                                    DevDetail.DeviceModel = dr["DeviceModel"].ToString();
                                    DevDetail.DeviceStatus = dr["DeviceStatus"].ToString();
                                    DevDetail.DeviceCommunicationTypeId = Convert.ToInt32(dr["DeviceCommunicationTypeId"]);
                                    lstDevDetail.Add(DevDetail);
                                }
                                programDetailsMasterDataAddPatient.DeviceDetails = lstDevDetail;
                                //Insurence Details
                                List<InsurenceDetails> lstInsDetail = new List<InsurenceDetails>();
                                foreach (DataRow dr in ds.Tables[11].Rows)
                                {
                                    InsurenceDetails InsDetail = new InsurenceDetails();
                                    InsDetail.VendorId = Convert.ToInt32(dr["VendorId"]);
                                    InsDetail.VendorName = dr["VendorName"].ToString();
                                    lstInsDetail.Add(InsDetail);
                                }
                                programDetailsMasterDataAddPatient.InsurenceDetails = lstInsDetail;
                            }
                        }

                        return programDetailsMasterDataAddPatient;
                    }
                }
            }
            catch (Exception ex)
            {

                throw;
            }
        }
        public List<DeviceDetails> GetDeviceDetails(int VitalId, string CreatedBy, string ConnectionString)
        {
            List<DeviceDetails> lstDevDetail = new List<DeviceDetails>();
            try
            {
                using (SqlConnection con = new SqlConnection(ConnectionString))
                {
                    con.Open();
                    using (SqlCommand command = new SqlCommand("usp_GetAvailableDevices", con))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@VitalId", VitalId);
                        command.Parameters.AddWithValue("@CreatedBy", CreatedBy);
                        SqlParameter returnParameter = command.Parameters.Add("RetVal", SqlDbType.Int);
                        returnParameter.Direction = ParameterDirection.ReturnValue;
                        SqlDataReader reader = command.ExecuteReader();
                        DeviceDetails DeviceDetails = new DeviceDetails();
                        while (reader.Read())
                        {

                            DeviceDetails DevDetail = new DeviceDetails();
                            DevDetail.VitalId = (!DBNull.Value.Equals(reader["VitalId"])) ? Convert.ToInt32(reader["VitalId"]) : 0;
                            DevDetail.DeviceName = reader["DeviceName"].ToString();
                            DevDetail.DeviceVendorId = (!DBNull.Value.Equals(reader["DeviceVendorId"])) ? Convert.ToInt32(reader["DeviceVendorId"]) : 0;
                            DevDetail.DeviceNumber = reader["DeviceNumber"].ToString();
                            DevDetail.DeviceModel = reader["DeviceModel"].ToString();
                            DevDetail.DeviceStatus = reader["DeviceStatus"].ToString();
                            DevDetail.DeviceCommunicationTypeId = (!DBNull.Value.Equals(reader["DeviceCommunicationTypeId"])) ? Convert.ToInt32(reader["DeviceCommunicationTypeId"]) : 0;
                            lstDevDetail.Add(DevDetail);
                        }
                        if (reader.FieldCount == 0)
                        {
                            return null;
                        }

                    }
                    return lstDevDetail;
                }
            }
            catch (Exception)
            {

                throw;
            }
        }
        public PatientNProgramDetails GetPatient(int PatientId, int PatientProgramId, string CreatedBy, string ConnectionString)
        {
            DataSet ds;
            try
            {
                using (SqlConnection con = new SqlConnection(ConnectionString))
                {
                    con.Open();
                    PatientNProgramDetails patientNProgramDetails = new PatientNProgramDetails();
                    GetPatientDetails patientDetails = new GetPatientDetails();
                    GetPatientProgramdetails patientProgramDetails = new GetPatientProgramdetails();
                    GetPatientProgramGoals getPatientProgramGoals = new GetPatientProgramGoals();
                    GetPatientPrescribtionDetails getPatientPrescribtionDetails = new GetPatientPrescribtionDetails();
                    GetPatientEnrolledDetails getPatientEnrolledDetails = new GetPatientEnrolledDetails();
                    GetPatientDevicesDetails getPatientDevicesDetails = new GetPatientDevicesDetails();
                    GetPatientVitalDetails getPatientVitalDetails = new GetPatientVitalDetails();
                    GetPatientInsurenceDetails getPatientInsurenceDetails = new GetPatientInsurenceDetails();
                    GetPatientDocumentDetails getPatientDocumentDetails = new GetPatientDocumentDetails();
                    using (SqlCommand command = new SqlCommand("usp_GetEditPatientInfo", con))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@PatientId", PatientId);
                        command.Parameters.AddWithValue("@PatientProgramId", PatientProgramId);
                        command.Parameters.AddWithValue("@CreatedBy", CreatedBy);
                        using (SqlDataAdapter sda = new SqlDataAdapter())
                        {
                            sda.SelectCommand = command;
                            using (ds = new DataSet())
                            {
                                sda.Fill(ds);
                                if (ds.Tables.Count > 9)
                                {
                                    //DataSet is empty

                                    //Patient  details
                                    patientDetails.UserName = ds.Tables[0].Rows[0]["UserName"].ToString();
                                    patientDetails.UserId = (!DBNull.Value.Equals(ds.Tables[0].Rows[0]["UserId"]) ? (Convert.ToInt32(ds.Tables[0].Rows[0]["UserId"])) : 0);
                                    patientDetails.OrganizationID = Convert.ToInt32(ds.Tables[0].Rows[0]["OrganizationID"]);
                                    patientDetails.MobileNo = ds.Tables[0].Rows[0]["MobileNo"].ToString();
                                    patientDetails.Email = ds.Tables[0].Rows[0]["Email"].ToString();
                                    patientDetails.Status = ds.Tables[0].Rows[0]["Status"].ToString();
                                    patientDetails.FirstName = ds.Tables[0].Rows[0]["FirstName"].ToString();
                                    patientDetails.MiddleName = ds.Tables[0].Rows[0]["MiddleName"].ToString();
                                    patientDetails.LastName = ds.Tables[0].Rows[0]["LastName"].ToString();
                                    patientDetails.DOB = Convert.ToDateTime(ds.Tables[0].Rows[0]["DOB"]);
                                    var genter = ds.Tables[0].Rows[0]["Gender"].ToString();
                                    if (genter == "M")
                                    {
                                        patientDetails.Gender = "Male";
                                    }
                                    else
                                    {
                                        patientDetails.Gender = "Female";
                                    }
                                    patientDetails.Height = Convert.ToSingle(ds.Tables[0].Rows[0]["Height"]);
                                    patientDetails.Weight = Convert.ToSingle(ds.Tables[0].Rows[0]["Weight"]);
                                    patientDetails.AlternateMobNo = ds.Tables[0].Rows[0]["AlternateMobNo"].ToString();
                                    patientDetails.Address1 = ds.Tables[0].Rows[0]["Address1"].ToString();
                                    patientDetails.Address2 = ds.Tables[0].Rows[0]["Address2"].ToString();
                                    patientDetails.CityId = (!DBNull.Value.Equals(ds.Tables[0].Rows[0]["CityId"]) ? (Convert.ToInt32(ds.Tables[0].Rows[0]["CityId"])) : 0);
                                    patientDetails.CityName = ds.Tables[0].Rows[0]["CityName"].ToString();
                                    patientDetails.StateId = (!DBNull.Value.Equals(ds.Tables[0].Rows[0]["StateId"]) ? (Convert.ToInt32(ds.Tables[0].Rows[0]["StateId"])) : 0);
                                    patientDetails.State = ds.Tables[0].Rows[0]["State"].ToString();
                                    patientDetails.CountryId = (!DBNull.Value.Equals(ds.Tables[0].Rows[0]["CountryId"]) ? (Convert.ToInt32(ds.Tables[0].Rows[0]["CountryId"])) : 0);
                                    //patientDetails.Picture = (byte[])ds.Tables[0].Rows[0]["Picture"];
                                    patientDetails.ZipCode = ds.Tables[0].Rows[0]["ZipCode"].ToString();
                                    patientDetails.TimeZoneID = (!DBNull.Value.Equals(ds.Tables[0].Rows[0]["TimeZoneID"]) ? (Convert.ToInt32(ds.Tables[0].Rows[0]["TimeZoneID"])) : 0);
                                    patientDetails.TimeZone= ds.Tables[0].Rows[0]["TimeZone"].ToString();
                                    patientDetails.UTCDifference = (!DBNull.Value.Equals(ds.Tables[0].Rows[0]["UTCDifference"]) ? (Convert.ToInt32(ds.Tables[0].Rows[0]["UTCDifference"])) : 0);
                                    patientDetails.Contact1Name = ds.Tables[0].Rows[0]["Contact1Name"].ToString();
                                    patientDetails.Contact1Phone = ds.Tables[0].Rows[0]["Contact1Phone"].ToString();
                                    patientDetails.Contact1RelationName = ds.Tables[0].Rows[0]["Contact1RelationName"].ToString();
                                    patientDetails.Contact2Name = ds.Tables[0].Rows[0]["Contact2Name"].ToString();
                                    patientDetails.Contact2Phone = ds.Tables[0].Rows[0]["Contact2Phone"].ToString();
                                    patientDetails.Contact2RelationName = ds.Tables[0].Rows[0]["Contact2RelationName"].ToString();
                                    patientDetails.CallTime = ds.Tables[0].Rows[0]["CallTime"].ToString();
                                    patientDetails.Preference1 = ds.Tables[0].Rows[0]["Preference1"].ToString();
                                    patientDetails.Preference2 = ds.Tables[0].Rows[0]["Preference2"].ToString();
                                    patientDetails.Preference3 = ds.Tables[0].Rows[0]["Preference3"].ToString();
                                    patientDetails.Notes = ds.Tables[0].Rows[0]["Notes"].ToString();
                                    patientDetails.Language = ds.Tables[0].Rows[0]["Language"].ToString();
                                    patientDetails.Picture = ds.Tables[0].Rows[0]["Picture"].ToString();
                                    //Patient program details
                                    if (ds.Tables[1].Rows.Count > 0)
                                    {
                                        patientProgramDetails.PatientProgramId = Convert.ToInt32(ds.Tables[1].Rows[0]["PatientProgramId"]);
                                        patientProgramDetails.ProgramId = Convert.ToInt32(ds.Tables[1].Rows[0]["ProgramId"]);
                                        patientProgramDetails.ProgramName = ds.Tables[1].Rows[0]["ProgramName"].ToString();
                                        patientProgramDetails.CareTeamUserId = Convert.ToInt32(ds.Tables[1].Rows[0]["CareTeamUserId"]);
                                        patientProgramDetails.AssignedMember = ds.Tables[1].Rows[0]["AssignedMember"].ToString();
                                        patientProgramDetails.ManagerId = Convert.ToInt32(ds.Tables[1].Rows[0]["ManagerId"]);
                                        patientProgramDetails.Manager = ds.Tables[1].Rows[0]["Manager"].ToString();
                                        patientProgramDetails.StartDate = Convert.ToDateTime(ds.Tables[1].Rows[0]["StartDate"]);
                                        patientProgramDetails.EndDate = Convert.ToDateTime(ds.Tables[1].Rows[0]["EndDate"]);
                                        patientProgramDetails.Duration = Convert.ToInt32(ds.Tables[1].Rows[0]["Duration"]);
                                        patientProgramDetails.ProgramStatus = ds.Tables[1].Rows[0]["ProgramStatus"].ToString();
                                        patientProgramDetails.Status = ds.Tables[1].Rows[0]["Status"].ToString();
                                        patientProgramDetails.TargetReadings = Convert.ToInt32(ds.Tables[1].Rows[0]["TargetReadings"]);
                                        List<GetPatientVitalInfo> lstV = new List<GetPatientVitalInfo>();
                                        foreach (DataRow dr in ds.Tables[1].Rows)
                                        {
                                            GetPatientVitalInfo patientVitalInfo = new GetPatientVitalInfo(
                                                dr["Vital"].ToString(),
                                                Convert.ToInt32(dr["VitalId"]),
                                                Convert.ToBoolean(dr["Selected"])//ccm change
                                                );
                                            lstV.Add(patientVitalInfo);
                                        }
                                        patientProgramDetails.PatientVitalInfos = lstV;
                                    }
                                    else
                                    {
                                        return null;
                                    }
                                    //Patient goal details
                                    List<GoalDetails> lstGD = new List<GoalDetails>();
                                    foreach (DataRow dr in ds.Tables[2].Rows)
                                    {
                                        GoalDetails patientpgmgoaldetail = new GoalDetails(
                                            Convert.ToInt32(dr["Id"]),
                                            dr["ProgramGoal"].ToString(),
                                            dr["ProgramDescription"].ToString()
                                            );
                                        lstGD.Add(patientpgmgoaldetail);
                                    }
                                    getPatientProgramGoals.goalDetails = lstGD;

                                    //Patient prescribtion details
                                    if (ds.Tables[3].Rows.Count > 0)
                                    {
                                        getPatientPrescribtionDetails.Status = ds.Tables[3].Rows[0]["Status"].ToString();
                                        getPatientPrescribtionDetails.PrescribedDate = (DateTime)ds.Tables[3].Rows[0]["PrescribedDate"];
                                        getPatientPrescribtionDetails.PhysicianId = (!DBNull.Value.Equals(ds.Tables[3].Rows[0]["PhysicianId"])) ? Convert.ToInt32(ds.Tables[3].Rows[0]["PhysicianId"]) : 0;
                                        getPatientPrescribtionDetails.Physician = ds.Tables[3].Rows[0]["Physician"].ToString();
                                        getPatientPrescribtionDetails.ConsultationDate = ds.Tables[3].Rows[0]["ConsultationDate"].ToString(); //fix
                                                                                                                                              // getPatientPrescribtionDetails.ConsultationDate = (DateTime)ds.Tables[3].Rows[0]["ConsultationDate"];
                                                                                                                                              // getPatientPrescribtionDetails.ConsultationDate= (!DBNull.Value.Equals(ds.Tables[0].Rows[0]["ConsultationDate"]) ? (DateTime)ds.Tables[3].Rows[0]["ConsultationDate"] : (DateTime)ds.Tables[3].Rows[0]["PrescribedDate"]);
                                        getPatientPrescribtionDetails.ClinicCode = ds.Tables[3].Rows[0]["ClinicCode"].ToString();
                                        getPatientPrescribtionDetails.Clinic= ds.Tables[3].Rows[0]["Clinic"].ToString();
                                        getPatientPrescribtionDetails.Branch = ds.Tables[3].Rows[0]["Branch"].ToString();
                                        List<PatientDiagnosisInfo> lstDGI = new List<PatientDiagnosisInfo>();
                                        foreach (DataRow dr in ds.Tables[3].Rows)
                                        {
                                            PatientDiagnosisInfo patientDiagnosticsInfo = new PatientDiagnosisInfo(
                                                Convert.ToInt32(dr["Id"]),
                                                dr["DiagnosticsName"].ToString(),
                                                dr["DiagnosticsCode"].ToString()
                                                );
                                            lstDGI.Add(patientDiagnosticsInfo);
                                        }
                                        getPatientPrescribtionDetails.PatientDiagnosisInfos = lstDGI;
                                    }

                                    //Patient enrollment details

                                    List<PatientEnrolledInfo> lstED = new List<PatientEnrolledInfo>();
                                    foreach (DataRow dr in ds.Tables[4].Rows)
                                    {
                                        getPatientEnrolledDetails.Status = ds.Tables[4].Rows[0]["Status"].ToString();
                                        getPatientEnrolledDetails.AssignedDate = Convert.ToDateTime(dr["CreatedOn"]);
                                        PatientEnrolledInfo patientenrolldetail = new PatientEnrolledInfo(
                                            dr["UserName"].ToString()
                                            );
                                        lstED.Add(patientenrolldetail);
                                    }
                                    getPatientEnrolledDetails.patientEnrolledInfos = lstED;

                                    //patient Active details
                                    PatientStatusInfo ActivePatientInfo = new PatientStatusInfo();
                                    foreach (DataRow dr in ds.Tables[5].Rows)
                                    {
                                        ActivePatientInfo.Status = dr["Status"].ToString();
                                        ActivePatientInfo.AssigneeId = Convert.ToInt32(dr["AssigneeId"]);
                                        ActivePatientInfo.AssigneeName = dr["AssigneeName"].ToString();
                                        ActivePatientInfo.ManagerId = Convert.ToInt32(dr["ManagerId"]);
                                        ActivePatientInfo.ManagerName = dr["ManagerName"].ToString();
                                        ActivePatientInfo.AssignedDate = Convert.ToDateTime(dr["CreatedOn"]);
                                    }
                                    patientNProgramDetails.ActivePatientDetails = ActivePatientInfo;
                                    //patient ReadyForDischarge details
                                    PatientStatusInfo ReadyForDischargePatientInfo = new PatientStatusInfo();
                                    foreach (DataRow dr in ds.Tables[6].Rows)
                                    {
                                        ReadyForDischargePatientInfo.Status = dr["Status"].ToString();
                                        ReadyForDischargePatientInfo.AssigneeId = Convert.ToInt32(dr["AssigneeId"]);
                                        ReadyForDischargePatientInfo.AssigneeName = dr["AssigneeName"].ToString();
                                        ReadyForDischargePatientInfo.ManagerId = Convert.ToInt32(dr["AssigneeId"]);
                                        ReadyForDischargePatientInfo.ManagerName = dr["ManagerName"].ToString();
                                        ReadyForDischargePatientInfo.AssignedDate = Convert.ToDateTime(dr["CreatedOn"]);
                                    }
                                    patientNProgramDetails.ReadyForDischargePatientDetails = ReadyForDischargePatientInfo;

                                    //patient OnHold details
                                    PatientStatusInfo OnHoldPatientInfo = new PatientStatusInfo();
                                    foreach (DataRow dr in ds.Tables[7].Rows)
                                    {
                                        OnHoldPatientInfo.Status = dr["Status"].ToString();
                                        OnHoldPatientInfo.AssigneeId = Convert.ToInt32(dr["AssigneeId"]);
                                        OnHoldPatientInfo.AssigneeName = dr["AssigneeName"].ToString();
                                        OnHoldPatientInfo.ManagerId = Convert.ToInt32(dr["AssigneeId"]);
                                        OnHoldPatientInfo.ManagerName = dr["ManagerName"].ToString();
                                        OnHoldPatientInfo.AssignedDate = Convert.ToDateTime(dr["CreatedOn"]);
                                    }
                                    patientNProgramDetails.OnHoldPatientDetais = OnHoldPatientInfo;
                                    //patient InActive details
                                    PatientStatusInfo InActivePatientInfo = new PatientStatusInfo();
                                    foreach (DataRow dr in ds.Tables[8].Rows)
                                    {
                                        InActivePatientInfo.Status = dr["Status"].ToString();
                                        InActivePatientInfo.AssigneeId = Convert.ToInt32(dr["AssigneeId"]);
                                        InActivePatientInfo.AssigneeName = dr["AssigneeName"].ToString();
                                        InActivePatientInfo.ManagerId = Convert.ToInt32(dr["AssigneeId"]);
                                        InActivePatientInfo.ManagerName = dr["ManagerName"].ToString();
                                        InActivePatientInfo.AssignedDate = Convert.ToDateTime(dr["CreatedOn"]);
                                    }
                                    patientNProgramDetails.InActivePatientDetais = InActivePatientInfo;
                                    //patient Discharged details
                                    PatientStatusInfo DischargedPatientInfo = new PatientStatusInfo();
                                    foreach (DataRow dr in ds.Tables[9].Rows)
                                    {
                                        DischargedPatientInfo.Status = dr["Status"].ToString();
                                        DischargedPatientInfo.AssigneeId = Convert.ToInt32(dr["AssigneeId"]);
                                        DischargedPatientInfo.AssigneeName = dr["AssigneeName"].ToString();
                                        DischargedPatientInfo.ManagerId = Convert.ToInt32(dr["AssigneeId"]);
                                        DischargedPatientInfo.ManagerName = dr["ManagerName"].ToString();
                                        DischargedPatientInfo.AssignedDate = Convert.ToDateTime(dr["CreatedOn"]);
                                    }
                                    patientNProgramDetails.DischargedPatientDetails = DischargedPatientInfo;

                                    //Patient devices details
                                    List<PatientDeviceInfo> lstDI = new List<PatientDeviceInfo>();
                                    foreach (DataRow dr in ds.Tables[10].Rows)
                                    {
                                        PatientDeviceInfo patientDeviceInfo = new PatientDeviceInfo(
                                            dr["VitalName"].ToString(),
                                            Convert.ToInt32(dr["VitalId"]),
                                            dr["DeviceName"].ToString(),
                                            Convert.ToInt32(dr["DeviceVendorId"]),
                                            dr["DeviceNumber"].ToString(),
                                            dr["DeviceStatus"].ToString(),
                                            dr["DeviceCommunicationType"].ToString(),
                                            dr["DeviceModel"].ToString(),
                                            dr["DeviceVendorUserId"].ToString(),
                                            dr["DeviceVendorUserName"].ToString()
                                            );
                                        lstDI.Add(patientDeviceInfo);
                                    }
                                    getPatientDevicesDetails.PatientDeviceInfos = lstDI;

                                    //Patient vital details
                                    List<PatientVitalInfo> IstVI = new List<PatientVitalInfo>();
                                    string vname = "";
                                    foreach (DataRow dr in ds.Tables[11].Rows)
                                    {
                                        if (vname != dr["VitalName"].ToString())
                                        {
                                            PatientVitalInfo patientVitalInfo = new PatientVitalInfo();
                                            patientVitalInfo.VitalName = dr["VitalName"].ToString();
                                            patientVitalInfo.VitalId = Convert.ToInt32(dr["VitalId"]);
                                            vname = patientVitalInfo.VitalName;
                                            patientVitalInfo.ScheduleId = Convert.ToInt32(dr["ScheduleId"]);
                                            patientVitalInfo.Schedule = dr["Schedule"].ToString();
                                            patientVitalInfo.ScheduleName = dr["ScheduleName"].ToString();
                                            patientVitalInfo.VitalScheduleId = Convert.ToInt32(dr["VitalScheduleId"]);
                                            patientVitalInfo.VitalScheduleName = dr["VitalScheduleName"].ToString();
                                            patientVitalInfo.Morning = Convert.ToBoolean(dr["Morning"]);
                                            patientVitalInfo.Afternoon = Convert.ToBoolean(dr["Afternoon"]);
                                            patientVitalInfo.Evening = Convert.ToBoolean(dr["Evening"]);
                                            patientVitalInfo.Night = Convert.ToBoolean(dr["Night"]);
                                            List<VitalMeasureInfo> vitalMeasureInfos = new List<VitalMeasureInfo>();
                                            foreach (DataRow dr1 in ds.Tables[11].Rows)
                                            {
                                                if (patientVitalInfo.VitalName == dr1["VitalName"].ToString())
                                                {
                                                    VitalMeasureInfo vitalMeasureInfo = new VitalMeasureInfo();
                                                    vitalMeasureInfo.Id = Convert.ToInt32(dr1["Id"]);
                                                    vitalMeasureInfo.DeviceVitalMeasuresId = Convert.ToInt32(dr1["DeviceVitalMeasuresId"]);
                                                    vitalMeasureInfo.MeasureName = dr1["MeasureName"].ToString();
                                                    vitalMeasureInfo.UnitName = dr1["UnitName"].ToString();
                                                    vitalMeasureInfo.MeasureOrder = Convert.ToInt32(dr1["MeasureOrder"]);
                                                    vitalMeasureInfo.CriticallMinimum = Convert.ToSingle(dr1["CriticallMinimum"]);
                                                    vitalMeasureInfo.CautiousMinimum = Convert.ToSingle(dr1["CautiousMinimum"]);
                                                    vitalMeasureInfo.NormalMinimum = Convert.ToSingle(dr1["NormalMinimum"]);
                                                    vitalMeasureInfo.NormalMaximum = Convert.ToSingle(dr1["NormalMaximum"]);
                                                    vitalMeasureInfo.CautiousMaximum = Convert.ToSingle(dr1["CautiousMaximum"]);
                                                    vitalMeasureInfo.CriticalMaximum = Convert.ToSingle(dr1["CriticalMaximum"]);
                                                    vitalMeasureInfos.Add(vitalMeasureInfo);
                                                }
                                                patientVitalInfo.VitalMeasureInfos = vitalMeasureInfos;

                                            }
                                            IstVI.Add(patientVitalInfo);
                                        }

                                    }
                                    getPatientVitalDetails.PatientVitalInfos = IstVI;

                                    //Patient insurence details
                                    List<PatientInsurenceInfo> IstII = new List<PatientInsurenceInfo>();
                                    foreach (DataRow dr in ds.Tables[12].Rows)
                                    {
                                        PatientInsurenceInfo patientInsurenceInfo = new PatientInsurenceInfo(
                                            Convert.ToInt32(dr["Id"]),
                                            Convert.ToInt32(dr["InsuranceVendorId"]),
                                            dr["InsuranceVendorName"].ToString(),
                                            Convert.ToBoolean(dr["IsPrimary"]));
                                        IstII.Add(patientInsurenceInfo);
                                    }
                                    getPatientInsurenceDetails.PatientInsurenceInfos = IstII;

                                    //Patient document details
                                    List<PatientDocumentinfo> IstDocI = new List<PatientDocumentinfo>();
                                    foreach (DataRow dr in ds.Tables[13].Rows)
                                    {
                                        PatientDocumentinfo patientDocumentinfo = new PatientDocumentinfo(
                                            Convert.ToInt32(dr["Id"]),
                                            dr["DocumentType"].ToString(),
                                            dr["DocumentName"].ToString(),
                                            Convert.ToDateTime(dr["CreatedOn"]),
                                            dr["DocumentUNC"].ToString());
                                        IstDocI.Add(patientDocumentinfo);
                                    }
                                    getPatientDocumentDetails.PatientDocumentinfos = IstDocI;
                                }
                                else
                                {
                                    return null;
                                }
                            }
                        }
                    }

                    patientNProgramDetails.PatientDetails = patientDetails;
                    patientNProgramDetails.PatientProgramdetails = patientProgramDetails;
                    patientNProgramDetails.PatientProgramGoals = getPatientProgramGoals;
                    patientNProgramDetails.PatientPrescribtionDetails = getPatientPrescribtionDetails;
                    patientNProgramDetails.PatientEnrolledDetails = getPatientEnrolledDetails;
                    patientNProgramDetails.PatientDevicesDetails = getPatientDevicesDetails;
                    patientNProgramDetails.PatientVitalDetails = getPatientVitalDetails;
                    patientNProgramDetails.PatientInsurenceDetails = getPatientInsurenceDetails;
                    patientNProgramDetails.PatientDocumentDetails = getPatientDocumentDetails;
                    return patientNProgramDetails;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public DataSet GetAllPatient(int Days, string Vitals, string PatientType, int RoleId, string CreatedBy, string ConnectionString)
        {
            DataSet ds;
            try
            {
                using (SqlConnection con = new SqlConnection(ConnectionString))
                {
                    con.Open();

                    using (SqlCommand command = new SqlCommand("usp_getVitalSummary", con))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@Days", Days);
                        command.Parameters.AddWithValue("@PatientType", PatientType);
                        command.Parameters.AddWithValue("@Vitals", Vitals);
                        command.Parameters.AddWithValue("@RoleId", RoleId);
                        command.Parameters.AddWithValue("@CreatedBy", CreatedBy);
                        using (SqlDataAdapter sda = new SqlDataAdapter())
                        {
                            //command.Connection = con;
                            sda.SelectCommand = command;
                            using (ds = new DataSet())
                            {
                                sda.Fill(ds);
                                //ds.Tables[0].TableName= "TotalPatients";
                                ds.Tables[0].TableName = "PatientList";
                                ds.Tables[1].TableName = "VitalCount";
                            }
                        }
                    }
                }
                return ds;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public List<GetAllPatientInfo> GetAllPatients(string PatientType, string Vitals, int RoleId, string CreatedBy, string ConnectionString)
        {
            List<GetAllPatientInfo> list = new List<GetAllPatientInfo>();
            try
            {
                using (SqlConnection con = new SqlConnection(ConnectionString))
                {
                    con.Open();

                    using (SqlCommand command = new SqlCommand("usp_GetAllPatients", con))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.Add("@PatientType", SqlDbType.NVarChar).Value = PatientType;
                        command.Parameters.Add("@Vitals", SqlDbType.NVarChar).Value = Vitals;
                        command.Parameters.Add("@RoleId", SqlDbType.NVarChar).Value = RoleId;
                        command.Parameters.Add("@CreatedBy", SqlDbType.NVarChar).Value = CreatedBy;
                        SqlDataReader reader = command.ExecuteReader();
                        while (reader.Read())
                        {
                            GetAllPatientInfo info = new GetAllPatientInfo();
                            info.PatientId = (!DBNull.Value.Equals(reader["PatientId"])) ? Convert.ToInt32(reader["PatientId"]) : 0;
                            info.PatientProgramId = (!DBNull.Value.Equals(reader["PatientProgramId"])) ? Convert.ToInt32(reader["PatientProgramId"]) : 0;
                            info.AssignedMemberId = (!DBNull.Value.Equals(reader["AssignedMemberId"])) ? Convert.ToInt32(reader["AssignedMemberId"]) : 0;
                            info.ClinicId = (!DBNull.Value.Equals(reader["ClinicId"])) ? Convert.ToInt32(reader["ClinicId"]) : 0;
                            info.ClinicName = reader["ClinicName"].ToString();
                            info.PatientNumber = reader["PatientNumber"].ToString();
                            info.PatientName = reader["PatientName"].ToString();
                            info.ProgramName = reader["ProgramName"].ToString();
                            info.Program= reader["Program"].ToString();
                            info.EnrolledDate = reader["EnrolledDate"].ToString();
                            info.PhysicianName = reader["PhysicianName"].ToString();
                            info.AssignedMember = reader["AssignedMember"].ToString();
                            info.PatientType = reader["PatientType"].ToString();
                            info.Vital = reader["Vital"].ToString();
                            info.Priority = reader["Priority"].ToString();
                            list.Add(info);
                        }
                    }
                }
                return list;
            }
            catch (Exception)
            {

                throw;
            }

        }
        public List<GetAllPatientInfo> GetAllPatientsList(DateTime ToDate, int UtcOffset, int Days, int RoleId, string CreatedBy, string ConnectionString)
        {
            List<GetAllPatientInfo> list = new List<GetAllPatientInfo>();
            try
            {
                using (SqlConnection con = new SqlConnection(ConnectionString))
                {
                    con.Open();

                    using (SqlCommand command = new SqlCommand("usp_GetAllPatientsList", con))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.Add("@ToDate", SqlDbType.DateTime).Value = ToDate;
                        command.Parameters.Add("@UtcOffset", SqlDbType.Int).Value = UtcOffset;
                        command.Parameters.Add("@RoleId", SqlDbType.NVarChar).Value = RoleId;
                        command.Parameters.Add("@Days", SqlDbType.NVarChar).Value = Days;
                        command.Parameters.Add("@CreatedBy", SqlDbType.NVarChar).Value = CreatedBy;
                        SqlDataReader reader = command.ExecuteReader();
                        while (reader.Read())
                        {
                            GetAllPatientInfo info = new GetAllPatientInfo();
                            info.PatientId = (!DBNull.Value.Equals(reader["PatientId"])) ? Convert.ToInt32(reader["PatientId"]) : 0;
                            info.PatientProgramId = (!DBNull.Value.Equals(reader["PatientProgramId"])) ? Convert.ToInt32(reader["PatientProgramId"]) : 0;
                            info.AssignedMemberId = (!DBNull.Value.Equals(reader["AssignedMemberId"])) ? Convert.ToInt32(reader["AssignedMemberId"]) : 0;
                            info.ClinicId = (!DBNull.Value.Equals(reader["ClinicId"])) ? Convert.ToInt32(reader["ClinicId"]) : 0;
                            info.ClinicName = reader["ClinicName"].ToString();
                            info.PatientNumber = reader["PatientNumber"].ToString();
                            info.PatientName = reader["PatientName"].ToString();
                            info.ProgramName = reader["ProgramName"].ToString();
                            info.Program = reader["Program"].ToString();
                            info.EnrolledDate = reader["EnrolledDate"].ToString();
                            info.PhysicianName = reader["PhysicianName"].ToString();
                            info.AssignedMember = reader["AssignedMember"].ToString();
                            info.PatientType = reader["PatientType"].ToString();
                            info.Vital = reader["Vital"].ToString();
                            info.Priority = reader["Priority"].ToString();

                            list.Add(info);
                        }
                    }
                }
                return list;
            }
            catch (Exception)
            {

                throw;
            }

        }

        public List<GetAllPatientSmsInfo> GetAllPatientsSmsList(int RoleId, string CreatedBy, string ConnectionString)
        {
            List<GetAllPatientSmsInfo> list = new List<GetAllPatientSmsInfo>();
            try
            {
                using (SqlConnection con = new SqlConnection(ConnectionString))
                {
                    con.Open();

                    using (SqlCommand command = new SqlCommand("usp_GetAllPatientsSmsList", con))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.Add("@RoleId", SqlDbType.NVarChar).Value = RoleId;
                        command.Parameters.Add("@CreatedBy", SqlDbType.NVarChar).Value = CreatedBy;
                        SqlDataReader reader = command.ExecuteReader();
                        while (reader.Read())
                        {
                            GetAllPatientSmsInfo info = new GetAllPatientSmsInfo();
                            info.PatientId = (!DBNull.Value.Equals(reader["PatientId"])) ? Convert.ToInt32(reader["PatientId"]) : 0;
                            info.PatientProgramId = (!DBNull.Value.Equals(reader["PatientProgramId"])) ? Convert.ToInt32(reader["PatientProgramId"]) : 0;
                            info.PatientName = reader["PatientName"].ToString();
                            info.ProgramName = reader["ProgramName"].ToString();
                            info.Senddate = reader["SentDate"].ToString();
                            DateTime parsedDate;
                            DateTime.TryParse(info.Senddate, out parsedDate);
                            string formattedDate = parsedDate.ToString("yyyy-MM-dd HH:mm:ss");
                            info.Senddate = formattedDate;
                            info.Senddate = info.Senddate.ToString();
                            info.FromNumber = reader["FromNo"].ToString();
                            info.Id = Convert.ToInt16(reader["Id"]);
                            info.Message = reader["SmsBody"].ToString();


                            list.Add(info);
                        }
                    }
                }
                return list;
            }
            catch (Exception)
            {

                throw;
            }

        }
        public string GetPatientLastPgmStatus(int PatientId, int PatientProgramId, string Createdby, string ConnectionString)
        {
            string Status = string.Empty;
            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    SqlCommand command = new SqlCommand("usp_GetPatientLastProgramStatus", connection);
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add("@PatientId", SqlDbType.Int).Value = PatientId;
                    command.Parameters.Add("@PatientProgramId", SqlDbType.Int).Value = PatientProgramId;
                    command.Parameters.Add("@CreatedBy", SqlDbType.NVarChar).Value = Createdby;
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        Status = !reader.IsDBNull(reader.GetOrdinal("Status")) ? reader["Status"].ToString() : string.Empty;
                    }
                }
            }
            catch
            {
                throw;
            }
            return Status;
        }
        public VitalReadings GetPatientVitalReadings(int PatientId, int PatientProgramId, DateTime StartDate, DateTime EndDate, string CreatedBy, string ConnectionString)
        {
            DataSet ds;
            try
            {
                VitalReadings vitalReadings = new VitalReadings();
                List<BloodPressure> bloodPressureList = new List<BloodPressure>();
                List<BloodGlucose> bloodGlucoseList = new List<BloodGlucose>();
                List<Weight> weigthList = new List<Weight>();
                List<BloodOxygen> bloodOxygenList = new List<BloodOxygen>();
                using (SqlConnection con = new SqlConnection(ConnectionString))
                {
                    con.Open();
                    using (SqlCommand command = new SqlCommand("usp_GetPatientVitalReadings", con))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.Add("@PatientId", SqlDbType.Int).Value = PatientId;
                        command.Parameters.Add("@PatientProgramId", SqlDbType.Int).Value = PatientProgramId;
                        command.Parameters.Add("@StartDate", SqlDbType.SmallDateTime).Value = StartDate;
                        command.Parameters.Add("@EndDate", SqlDbType.SmallDateTime).Value = EndDate;
                        command.Parameters.Add("@CreatedBy", SqlDbType.NVarChar).Value = CreatedBy;
                        using (SqlDataAdapter sda = new SqlDataAdapter())
                        {
                            sda.SelectCommand = command;
                            using (ds = new DataSet())
                            {
                                sda.Fill(ds);

                                List<ProgramDetailsMasterDataProgram> lstpDMD = new List<ProgramDetailsMasterDataProgram>();
                                List<DateTime> CreatedOn = new List<DateTime>();
                                foreach (DataRow dr1 in ds.Tables[0].Rows)
                                {
                                    if (!CreatedOn.Contains(Convert.ToDateTime(dr1["CreatedOn"]).Date))
                                    {
                                        CreatedOn.Add(Convert.ToDateTime(dr1["CreatedOn"]).Date);
                                    }
                                }

                                foreach (DateTime d in CreatedOn)
                                {
                                    BloodGlucose bloodGlucose = new BloodGlucose();
                                    foreach (DataRow dr1 in ds.Tables[0].Rows)
                                    {
                                        if (d == Convert.ToDateTime(dr1["CreatedOn"]).Date)
                                        {
                                            List<BloodGlucoseReading> readinglistBG = new List<BloodGlucoseReading>();
                                            bloodGlucose.ReadingDate = Convert.ToDateTime(dr1["CreatedOn"]).Date;
                                            foreach (DataRow dr2 in ds.Tables[0].Rows)
                                            {
                                                if (bloodGlucose.ReadingDate == Convert.ToDateTime(dr2["CreatedOn"]).Date)
                                                {
                                                    BloodGlucoseReading reading = new BloodGlucoseReading();
                                                    reading.ReadingTime = Convert.ToDateTime(dr2["CreatedOn"]);
                                                    reading.Schedule = dr2["MeasureName"].ToString();
                                                    reading.BGmgdl = Convert.ToInt32(dr2["MeasureValue"]);
                                                    reading.Status = dr2["Remark"].ToString();
                                                    reading.Remarks = dr2["Remark"].ToString();
                                                    readinglistBG.Add(reading);
                                                }
                                            }
                                            bloodGlucose.BloodGlucoseReadings = readinglistBG;
                                        }

                                    }
                                    bloodGlucoseList.Add(bloodGlucose);
                                }


                                List<DateTime> CreatedOnBP = new List<DateTime>();
                                foreach (DataRow dr1 in ds.Tables[1].Rows)
                                {
                                    if (!CreatedOnBP.Contains(Convert.ToDateTime(dr1["CreatedOn"]).Date))
                                    {
                                        CreatedOnBP.Add(Convert.ToDateTime(dr1["CreatedOn"]).Date);
                                    }
                                }

                                foreach (DateTime d in CreatedOnBP)
                                {
                                    BloodPressure bloodPressure = new BloodPressure();
                                    foreach (DataRow dr1 in ds.Tables[1].Rows)
                                    {
                                        if (d == Convert.ToDateTime(dr1["CreatedOn"]).Date)
                                        {
                                            bloodPressure.ReadingDate = Convert.ToDateTime(dr1["CreatedOn"]).Date;
                                            List<BloodPressureReading> BPreading = new List<BloodPressureReading>();
                                            foreach (DataRow dr2 in ds.Tables[1].Rows)
                                            {
                                                if (bloodPressure.ReadingDate == Convert.ToDateTime(dr2["CreatedOn"]).Date)
                                                {
                                                    BloodPressureReading reading = new BloodPressureReading();
                                                    reading.ReadingTime = Convert.ToDateTime(dr2["CreatedOn"]);
                                                    reading.Systolic = Convert.ToInt32(dr2["SystolicValue"]);
                                                    reading.Diastolic = Convert.ToInt32(dr2["DiastolicValue"]);
                                                    reading.pulse = Convert.ToInt32(dr2["PulseValue"]);
                                                    reading.SystolicStatus = dr2["systRemark"].ToString();
                                                    reading.DiastolicStatus = dr2["diasRemark"].ToString();
                                                    reading.pulseStatus = dr2["pulsRemark"].ToString();
                                                    reading.Remarks = dr2["TotalRemark"].ToString();
                                                    reading.Status = dr2["TotalRemark"].ToString();
                                                    BPreading.Add(reading);
                                                }
                                            }
                                            bloodPressure.BloodPressureReadings = BPreading;
                                        }
                                    }
                                    bloodPressureList.Add(bloodPressure);
                                }
                                List<DateTime> CreatedOnBW = new List<DateTime>();
                                foreach (DataRow dr1 in ds.Tables[2].Rows)
                                {
                                    if (!CreatedOnBW.Contains(Convert.ToDateTime(dr1["CreatedOn"]).Date))
                                    {
                                        CreatedOnBW.Add(Convert.ToDateTime(dr1["CreatedOn"]).Date);
                                    }
                                }
                                foreach (DateTime d in CreatedOnBW)
                                {
                                    Weight weight = new Weight();
                                    foreach (DataRow dr1 in ds.Tables[2].Rows)
                                    {
                                        if (d == Convert.ToDateTime(dr1["CreatedOn"]).Date)
                                        {
                                            List<WeightReading> readinglistBW = new List<WeightReading>();
                                            weight.ReadingDate = Convert.ToDateTime(dr1["CreatedOn"]).Date;
                                            foreach (DataRow dr2 in ds.Tables[2].Rows)
                                            {
                                                if (weight.ReadingDate == Convert.ToDateTime(dr2["CreatedOn"]).Date)
                                                {
                                                    WeightReading reading = new WeightReading();
                                                    reading.ReadingTime = Convert.ToDateTime(dr2["CreatedOn"]);
                                                    reading.Schedule = dr2["MeasureName"].ToString();
                                                    reading.BWlbs = Convert.ToSingle(dr2["MeasureValue"]);
                                                    reading.Status = dr2["Remark"].ToString();
                                                    reading.Remarks = dr2["Remark"].ToString();
                                                    readinglistBW.Add(reading);
                                                }
                                            }
                                            weight.WeightReadings = readinglistBW;
                                        }

                                    }
                                    weigthList.Add(weight);
                                }

                                //for bloodoxygen
                                List<DateTime> CreatedOnBO = new List<DateTime>();
                                foreach (DataRow dr1 in ds.Tables[3].Rows)
                                {
                                    if (!CreatedOnBO.Contains(Convert.ToDateTime(dr1["CreatedOn"]).Date))
                                    {
                                        CreatedOnBO.Add(Convert.ToDateTime(dr1["CreatedOn"]).Date);
                                    }
                                }

                                foreach (DateTime d in CreatedOnBO)
                                {
                                    BloodOxygen bloodOxygen = new BloodOxygen();
                                    foreach (DataRow dr1 in ds.Tables[3].Rows)
                                    {
                                        if (d == Convert.ToDateTime(dr1["CreatedOn"]).Date)
                                        {
                                            bloodOxygen.ReadingDate = Convert.ToDateTime(dr1["CreatedOn"]).Date;
                                            List<BloodOxygenReading> BOreading = new List<BloodOxygenReading>();
                                            foreach (DataRow dr2 in ds.Tables[3].Rows)
                                            {
                                                if (bloodOxygen.ReadingDate == Convert.ToDateTime(dr2["CreatedOn"]).Date)
                                                {
                                                    BloodOxygenReading reading = new BloodOxygenReading();
                                                    reading.ReadingTime = Convert.ToDateTime(dr2["CreatedOn"]);
                                                    reading.Pulse = Convert.ToInt32(dr2["pulseValue"]);
                                                    reading.Oxygen = Convert.ToInt32(dr2["oxygenValue"]);
                                                    reading.PulseStatus = dr2["pulseRemark"].ToString();
                                                    reading.OxygenStatus = dr2["oxygenRemark"].ToString();
                                                    reading.Remarks = dr2["TotalRemark"].ToString();
                                                    reading.Status = dr2["TotalRemark"].ToString();
                                                    BOreading.Add(reading);
                                                }
                                            }
                                            bloodOxygen.BloodOxygenReadings = BOreading;
                                        }
                                    }
                                    bloodOxygenList.Add(bloodOxygen);
                                }
                            }
                        }
                    }
                }

                vitalReadings.BloodGlucose = bloodGlucoseList;
                vitalReadings.BloodPressure = bloodPressureList;
                vitalReadings.Weight = weigthList;
                vitalReadings.BloodOxygen = bloodOxygenList;
                return vitalReadings;
            }
            catch
            {

                throw;
            }
        }
        //Api for patient vital readings
        public PatientVitalReadings GetPatientVitalReadingswithDateTime(int PatientId, int PatientProgramId, DateTime StartDate, DateTime EndDate, string CreatedBy, string ConnectionString)
        {
            DataSet ds;
            try
            {
                PatientVitalReadings vitalReadings = new PatientVitalReadings();
                List<BloodPressureReading> bloodPressureList = new List<BloodPressureReading>();
                List<BloodGlucoseReading> bloodGlucoseList = new List<BloodGlucoseReading>();
                List<WeightReading> weigthList = new List<WeightReading>();
                List<BloodOxygenReading> bloodOxygenList = new List<BloodOxygenReading>();
                using (SqlConnection con = new SqlConnection(ConnectionString))
                {
                    con.Open();
                    using (SqlCommand command = new SqlCommand("usp_GetPatientVitalReadings_V1", con))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.Add("@PatientId", SqlDbType.Int).Value = PatientId;
                        command.Parameters.Add("@PatientProgramId", SqlDbType.Int).Value = PatientProgramId;
                        command.Parameters.Add("@StartDate", SqlDbType.SmallDateTime).Value = StartDate;
                        command.Parameters.Add("@EndDate", SqlDbType.SmallDateTime).Value = EndDate;
                        command.Parameters.Add("@CreatedBy", SqlDbType.NVarChar).Value = CreatedBy;
                        using (SqlDataAdapter sda = new SqlDataAdapter())
                        {
                            sda.SelectCommand = command;
                            using (ds = new DataSet())
                            {
                                sda.Fill(ds);

                                List<BloodGlucoseReading> readinglistBG = new List<BloodGlucoseReading>();

                                foreach (DataRow dr2 in ds.Tables[0].Rows)
                                {
                                    BloodGlucoseReading reading = new BloodGlucoseReading();
                                    reading.ReadingTime = Convert.ToDateTime(dr2["CreatedOn"]);
                                    reading.Schedule = dr2["MeasureName"].ToString();
                                    reading.BGmgdl = Convert.ToInt32(dr2["MeasureValue"]);
                                    reading.Status = dr2["Remark"].ToString();
                                    reading.Remarks = dr2["Remark"].ToString();
                                    readinglistBG.Add(reading);
                                }
                                vitalReadings.BloodGlucose = readinglistBG;
                                List<BloodPressureReading> BPreading = new List<BloodPressureReading>();
                                foreach (DataRow dr2 in ds.Tables[1].Rows)
                                {
                                    BloodPressureReading reading = new BloodPressureReading();
                                    reading.ReadingTime = Convert.ToDateTime(dr2["CreatedOn"]);
                                    reading.Systolic = Convert.ToInt32(dr2["SystolicValue"]);
                                    reading.Diastolic = Convert.ToInt32(dr2["DiastolicValue"]);
                                    reading.pulse = Convert.ToInt32(dr2["PulseValue"]);
                                    reading.SystolicStatus = dr2["systRemark"].ToString();
                                    reading.DiastolicStatus = dr2["diasRemark"].ToString();
                                    reading.pulseStatus = dr2["pulsRemark"].ToString();
                                    reading.Remarks = dr2["TotalRemark"].ToString();
                                    reading.Status = dr2["TotalRemark"].ToString();
                                    BPreading.Add(reading);
                                }
                                vitalReadings.BloodPressure = BPreading;
                                List<WeightReading> readinglistBW = new List<WeightReading>();

                                foreach (DataRow dr2 in ds.Tables[2].Rows)
                                {
                                    WeightReading reading = new WeightReading();
                                    reading.ReadingTime = Convert.ToDateTime(dr2["CreatedOn"]);
                                    reading.Schedule = dr2["MeasureName"].ToString();
                                    reading.BWlbs = Convert.ToSingle(dr2["MeasureValue"]);
                                    reading.Status = dr2["Remark"].ToString();
                                    reading.Remarks = dr2["Remark"].ToString();
                                    readinglistBW.Add(reading);
                                }
                                vitalReadings.Weight = readinglistBW;
                                List<BloodOxygenReading> BOreading = new List<BloodOxygenReading>();
                                foreach (DataRow dr2 in ds.Tables[3].Rows)
                                {
                                    BloodOxygenReading reading = new BloodOxygenReading();
                                    reading.ReadingTime = Convert.ToDateTime(dr2["CreatedOn"]);
                                    reading.Pulse = Convert.ToInt32(dr2["pulseValue"]);
                                    reading.Oxygen = Convert.ToInt32(dr2["oxygenValue"]);
                                    reading.PulseStatus = dr2["pulseRemark"].ToString();
                                    reading.OxygenStatus = dr2["oxygenRemark"].ToString();
                                    reading.Remarks = dr2["TotalRemark"].ToString();
                                    reading.Status = dr2["TotalRemark"].ToString();
                                    BOreading.Add(reading);
                                }
                                vitalReadings.BloodOxygen = BOreading;
                                return vitalReadings;
                            }

                        }

                    }
                }
            }
            catch
            {

                throw;
            }
        }
        public HealthTrends GetPatientHealthTrends(string username, int PatientId, int PatientProgramId, DateTime StartDate, DateTime EndDate, string CreatedBy, string ConnectionString)
        {
            try
            {
                HealthTrends ret = new HealthTrends();
                ret.Time = new List<DateTime>();
                ret.Values = new List<Values>();
                using (SqlConnection con = new SqlConnection(ConnectionString))
                {
                    con.Open();

                    using (SqlCommand command = new SqlCommand("usp_GetpatientHealthTrends", con))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.Add("@PatientId", SqlDbType.Int).Value = PatientId;
                        command.Parameters.Add("@PatientProgramId", SqlDbType.Int).Value = PatientProgramId;
                        //command.Parameters.Add("@StartDate", SqlDbType.SmallDateTime).Value = StartDate;
                        //command.Parameters.Add("@EndDate", SqlDbType.SmallDateTime).Value = EndDate;

                        command.Parameters.AddWithValue("@StartDate", StartDate);
                        command.Parameters.AddWithValue("@EndDate", EndDate);

                        command.Parameters.Add("@CreatedBy", SqlDbType.NVarChar).Value = CreatedBy;
                        SqlParameter returnParameter = command.Parameters.Add("RetVal", SqlDbType.Int);
                        returnParameter.Direction = ParameterDirection.ReturnValue;
                        SqlDataReader reader = command.ExecuteReader();
                        HealthTrends list = new HealthTrends();
                        DataTable dt = new DataTable();
                        while (!reader.IsClosed)
                        {
                            dt.Load(reader);
                        }
                        if (dt.Rows.Count > 0)
                        {
                            List<DateTime> xAxisInfo = new List<DateTime>();
                            List<DateTime> xDatesInDb = dt.AsEnumerable().Select(s => s.Field<DateTime>("CreatedOn")).Distinct().ToList<DateTime>();

                            for (DateTime sDate = StartDate; sDate.Date <= EndDate.Date; sDate = sDate.AddDays(1))
                            {
                                List<DateTime> tempTime = xDatesInDb.FindAll(x => x.Date == sDate.Date);
                                if (tempTime.Count > 0)
                                {
                                    xAxisInfo.AddRange(tempTime);
                                }
                                else
                                {
                                    if (sDate.Date == EndDate.Date) { sDate=sDate.AddSeconds(-1); }
                                    xAxisInfo.Add(sDate);
                                }
                            }
                            string sVitalName = string.Empty;
                            int nVitalId = 0;
                            Dictionary<string, List<VitalMesureData>> values = new Dictionary<string, List<VitalMesureData>>();
                            foreach (DataRow row in dt.Rows)
                            {
                                if (string.IsNullOrEmpty(sVitalName))
                                {
                                    sVitalName = row["VitalName"].ToString();
                                    nVitalId = Convert.ToInt32(row["VitalId"]);
                                }
                                string sMeasurementName = row["MeasureName"].ToString();
                                int nMeasureValue = Convert.ToInt32(row["MeasureValue"]);
                                DateTime dtCreatedOn = Convert.ToDateTime(row["CreatedOn"]);
                                if (!values.ContainsKey(sMeasurementName))
                                {
                                    values.Add(sMeasurementName, new List<VitalMesureData>());
                                }
                                VitalMesureData vi = new VitalMesureData();
                                vi.ValueDate = dtCreatedOn;
                                vi.Value = nMeasureValue.ToString();
                                values[sMeasurementName].Add(vi);
                            }
                            ret.VitalName = sVitalName;
                            VitalMeasure vitaldata = new VitalMeasure();
                            vitaldata = GetLatestReading(username, 7, ConnectionString, sVitalName);
                            ret.LatestVitalMeasure = vitaldata;
                            ret.VitalId = nVitalId;
                            ret.Time.AddRange(xAxisInfo);
                            foreach (KeyValuePair<string, List<VitalMesureData>> kv in values)
                            {
                                Values v = new Values();
                                v.label = kv.Key;
                                v.data = new List<string>();
                                foreach (DateTime sDate in ret.Time)
                                {
                                    VitalMesureData vm = kv.Value.Find(x => x.ValueDate == sDate);
                                    if (vm == null)
                                        v.data.Add(null);
                                    else
                                        v.data.Add(vm.Value);
                                }
                                ret.Values.Add(v);
                            }
                        }
                        else
                        {
                            VitalMeasure vitaldata = new VitalMeasure();
                            vitaldata = GetLatestReading(username, 7, ConnectionString, null);
                            ret.LatestVitalMeasure = vitaldata;
                        }

                    }
                }
                return ret;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public List<GetSchedules> GetPatientSchedule(int PatientId, DateTime StartDate, DateTime EndDate, string CreatedBy, string ConnectionString)
        {
            List<GetSchedules> getSchedules = new List<GetSchedules>();
            DataSet ds;
            try
            {
                using (SqlConnection con = new SqlConnection(ConnectionString))
                {
                    con.Open();

                    using (SqlCommand command = new SqlCommand("usp_GetPatientSchedule", con))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.Add("@PatientId", SqlDbType.Int).Value = PatientId;
                        command.Parameters.Add("@StartDate", SqlDbType.SmallDateTime).Value = StartDate;
                        command.Parameters.Add("@EndDate", SqlDbType.SmallDateTime).Value = EndDate;
                        command.Parameters.AddWithValue("@CreatedBy", CreatedBy);

                        using (SqlDataAdapter sda = new SqlDataAdapter())
                        {
                            sda.SelectCommand = command;
                            using (ds = new DataSet())
                            {
                                sda.Fill(ds);
                                DateTime[] dates = ds.Tables[0].AsEnumerable().Select(s => s.Field<DateTime>("Date")).Distinct().ToArray<DateTime>();
                                foreach (DateTime dt in dates)
                                {
                                    GetSchedules schedules = new GetSchedules();
                                    List<SchedueInfo> schedueInfos = new List<SchedueInfo>();
                                    schedules.ScheduleDate = dt;
                                    foreach (DataRow dr in ds.Tables[0].Rows)
                                    {
                                        if (dt == Convert.ToDateTime(dr["Date"]).Date)
                                        {
                                            SchedueInfo schedueInfo = new SchedueInfo();
                                            schedueInfo.Id = (!DBNull.Value.Equals(dr["Id"])) ? Convert.ToInt32(dr["Id"]) : 0;
                                            schedueInfo.CurrentScheduleId = (!DBNull.Value.Equals(dr["CurrentScheduleId"])) ? Convert.ToInt32(dr["CurrentScheduleId"]) : 0;
                                            schedueInfo.PatientId = (!DBNull.Value.Equals(dr["PatientId"])) ? Convert.ToInt32(dr["PatientId"]) : 0;
                                            schedueInfo.PatientProgramId = (!DBNull.Value.Equals(dr["PatientProgramId"])) ? Convert.ToInt32(dr["PatientProgramId"]) : 0;
                                            schedueInfo.ScheduleTime = dr["Time"].ToString();
                                            schedueInfo.ScheduleType = dr["ScheduleType"].ToString();
                                            schedueInfo.Description = dr["Description"].ToString();
                                            schedueInfo.ContactName = dr["ContactName"].ToString();
                                            schedueInfo.AssignedBy = (!DBNull.Value.Equals(dr["AssignedBy"])) ? Convert.ToInt32(dr["AssignedBy"]) : 0;
                                            schedueInfo.AssignedByName = dr["AssignedByName"].ToString();
                                            schedueInfo.IsCompleted = (!DBNull.Value.Equals(dr["IsCompleted"])) ? Convert.ToBoolean(dr["IsCompleted"]) : false;
                                            schedueInfos.Add(schedueInfo);
                                        }
                                    }
                                    schedules.SchedueInfos = schedueInfos;
                                    getSchedules.Add(schedules);
                                }
                            }
                        }
                    }
                }
                return getSchedules;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public List<GetAlertsAndTasks> GetPatientAlertAndTask(int PatientId, int PatientProgramId, DateTime StartDate, DateTime EndDate, string CreatedBy, string ConnectionString)
        {
            List<GetAlertsAndTasks> list = new List<GetAlertsAndTasks>();
            try
            {
                using (SqlConnection con = new SqlConnection(ConnectionString))
                {
                    con.Open();

                    using (SqlCommand command = new SqlCommand("usp_GetPatientAlertAndTask", con))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.Add("@PatientId", SqlDbType.Int).Value = PatientId;
                        command.Parameters.Add("@PatientProgramId", SqlDbType.Int).Value = PatientProgramId;
                        command.Parameters.Add("@StartDate", SqlDbType.DateTime).Value = StartDate;
                        command.Parameters.Add("@EndDate", SqlDbType.DateTime).Value = EndDate;
                        command.Parameters.Add("@CreatedBy", SqlDbType.NVarChar).Value = CreatedBy;
                        SqlDataReader reader = command.ExecuteReader();
                        while (reader.Read())
                        {
                            GetAlertsAndTasks info = new GetAlertsAndTasks();
                            info.Id = (!DBNull.Value.Equals(reader["Id"])) ? Convert.ToInt32(reader["Id"]) : 0;
                            info.PatientId = (!DBNull.Value.Equals(reader["PatientId"])) ? Convert.ToInt32(reader["PatientId"]) : 0;
                            info.PatientName = reader["PatientName"].ToString();
                            info.PatientProgramId = (!DBNull.Value.Equals(reader["PatientProgramId"])) ? Convert.ToInt32(reader["PatientProgramId"]) : 0;
                            info.Type = reader["Type"].ToString();
                            info.Priority = reader["Priority"].ToString();
                            info.CreatedBy = reader["CreatedBy"].ToString();
                            info.AssignedToId = (!DBNull.Value.Equals(reader["AssignedToId"])) ? Convert.ToInt32(reader["AssignedToId"]) : 0;
                            info.AssignedMember = reader["AssignedMember"].ToString();
                            info.DueDate = (!DBNull.Value.Equals(reader["DueDate"])) ? Convert.ToDateTime(reader["DueDate"]) : DateTime.MinValue;
                            info.Status = reader["Status"].ToString();
                            info.TaskOrAlert = reader["TaskOrAlert"].ToString();
                            list.Add(info);
                        }
                    }
                }
                return list;
            }
            catch (Exception)
            {

                throw;
            }

        }
        public List<DashboardAlerts> GetPatientCriticalAlerts(int PatientId, string CreatedBy, string ConnectionString)
        {
            List<DashboardAlerts> dashboardAlerts = new List<DashboardAlerts>();
            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    SqlCommand command = new SqlCommand("usp_GetPatientCriticalAlerts", connection);
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add("@PatientId", SqlDbType.Int).Value = PatientId;
                    command.Parameters.Add("@CreatedBy", SqlDbType.NVarChar).Value = CreatedBy;
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {

                        DashboardAlerts info = new DashboardAlerts();
                        info.Id = (!DBNull.Value.Equals(reader["Id"])) ? Convert.ToInt32(reader["Id"]) : 0;
                        info.PatientId = (!DBNull.Value.Equals(reader["PatientId"])) ? Convert.ToInt32(reader["PatientId"]) : 0;
                        info.PatientName = reader["PatientName"].ToString();
                        info.PatientProgramId = (!DBNull.Value.Equals(reader["PatientProgramId"])) ? Convert.ToInt32(reader["PatientProgramId"]) : 0;
                        info.Priority = reader["Priority"].ToString();
                        info.VitalAlert = reader["VitalAlert"].ToString();
                        info.AssignedToCareTeamUserId = (!DBNull.Value.Equals(reader["AssignedToCareTeamUserId"])) ? Convert.ToInt32(reader["AssignedToCareTeamUserId"]) : 0;
                        info.Time = (!DBNull.Value.Equals(reader["Time"])) ? Convert.ToDateTime(reader["Time"]) : DateTime.MinValue;
                        dashboardAlerts.Add(info);
                    }
                }
            }
            catch
            {
                throw;
            }
            return dashboardAlerts;

        }
        public int GetPatientInteractionTime(int PatientId, string CreatedBy, string ConnectionString)
        {
            int interactionTime = 0;
            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    SqlCommand command = new SqlCommand("usp_GetPatientInteractionTime", connection);
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add("@PatientId", SqlDbType.Int).Value = PatientId;
                    command.Parameters.Add("@CreatedBy", SqlDbType.NVarChar).Value = CreatedBy;
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        interactionTime = (!DBNull.Value.Equals(reader["InterationTime"])) ? Convert.ToInt32(reader["InterationTime"]) : 0;
                    }
                }
            }
            catch
            {
                throw;
            }
            return interactionTime;
        }
        public NewPatientCredential UpdatePatientPassword(ResetPatientPW Info, string ConnectionString)
        {
            NewPatientCredential newPatientCredential = new NewPatientCredential();
            bool ret = true;
            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    SqlCommand command = new SqlCommand("usp_UpdPatientPassword", connection);
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@PatientId", Info.PatientId);
                    command.Parameters.AddWithValue("@CreatedBy", Info.CreatedBy);
                    SqlParameter returnParameter = command.Parameters.Add("RetVal", SqlDbType.Int);
                    returnParameter.Direction = ParameterDirection.ReturnValue;
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        newPatientCredential.PatientId = Convert.ToInt32(reader["patientId"]);
                        newPatientCredential.password = Convert.ToString(reader["RandomPassword"]);
                    }
                    if (reader.FieldCount == 0)
                    {
                        return null;
                    }
                    connection.Close();
                }
                return newPatientCredential;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        public SearchPatient SearchPatient(string PatientNumber, string CreatedBy, string ConnectionString)
        {
            SearchPatient searchPatient = new SearchPatient();
            try
            {

                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    SqlCommand command = new SqlCommand("usp_GetPatientAndProgramId", connection);
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add("@PatientNumber", SqlDbType.NVarChar).Value = PatientNumber;
                    command.Parameters.Add("@CreatedBy", SqlDbType.NVarChar).Value = CreatedBy;
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        searchPatient.PatientId = (!DBNull.Value.Equals(reader["PatientId"])) ? Convert.ToInt32(reader["PatientId"]) : 0;
                        searchPatient.PatientProgramId = (!DBNull.Value.Equals(reader["PatientProgramId"])) ? Convert.ToInt32(reader["PatientProgramId"]) : 0;
                    }
                }
            }
            catch
            {
                throw;
            }
            return searchPatient;
        }
        public List<PatientSummary> GetPatientVitalSummary(string Username, DateTime StartDate, DateTime EndDate, string ConnectionString)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(ConnectionString))
                {
                    con.Open();
                    //DateTime startDateNew = DateTime.Today.AddDays(-dayCount);
                    //DateTime enddateNew = DateTime.Today;
                    using (SqlCommand command = new SqlCommand("usp_GetpatientHealthTrendsbyPatient", con))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@CreatedBy", Username);
                        command.Parameters.AddWithValue("@StartDate", StartDate);
                        command.Parameters.AddWithValue("@EndDate", EndDate);
                        SqlParameter returnParameter = command.Parameters.Add("RetVal", SqlDbType.Int);
                        returnParameter.Direction = ParameterDirection.ReturnValue;
                        SqlDataReader reader = command.ExecuteReader();
                        DataTable dt = new DataTable();
                        while (!reader.IsClosed)
                        {
                            dt.Load(reader);
                        }
                        List<PatientSummary> lstSummery = new List<PatientSummary>();
                        if (dt.Rows.Count == 0)
                        {
                            PatientSummary summary = null;
                            summary = new PatientSummary();
                            summary.VitalName = "No vital Data";
                            lstSummery.Add(summary);
                            PatientMeasureCollection collection = null;

                            collection = new PatientMeasureCollection();
                            collection.Measurement = null;
                            summary.Measurements.Add(collection);

                            for (DateTime sDateNew = StartDate; sDateNew < EndDate; sDateNew = sDateNew.AddDays(1))
                            {

                                collection.Time.Add(sDateNew);
                                collection.Value.Add("null");
                            }
                        }

                        else
                        {
                            foreach (DataRow dr in dt.Rows)
                            {
                                string sVitalName = dr["VitalName"].ToString();
                                PatientSummary summary = null;
                                if (!string.IsNullOrEmpty(sVitalName) &&
                                    (summary = lstSummery.Find(x => x.VitalName == sVitalName)) == null)
                                {
                                    summary = new PatientSummary();
                                    summary.VitalName = sVitalName;
                                    lstSummery.Add(summary);
                                }
                                string sMeaurementName = dr["MeasureName"].ToString();
                                PatientMeasureCollection collection = summary.Measurements.Find(x => x.Measurement.Equals(sMeaurementName));
                                if (collection == null)
                                {
                                    collection = new PatientMeasureCollection();
                                    collection.Measurement = sMeaurementName;
                                    summary.Measurements.Add(collection);
                                }
                                float measurevalue = Convert.ToSingle(dr["MeasureValue"]);
                                DateTime createdOn = (DateTime)dr["CreatedOn"];
                                collection.Time.Add(createdOn);
                                collection.Value.Add(Convert.ToString(measurevalue));
                            }
                            List<DateTime> dateTimeAll = new List<DateTime>();

                            for (DateTime sDateNew = StartDate; sDateNew < EndDate; sDateNew = sDateNew.AddDays(1))
                            {
                                dateTimeAll.Add(sDateNew);
                            }

                            foreach (var data in lstSummery)
                            {

                                foreach (var vitals in data.Measurements)
                                {
                                    List<DateTime> DateTimeAdded = vitals.Time;
                                    List<DateTime> newDateList = dateTimeAll.Except(DateTimeAdded).ToList();
                                    foreach (DateTime datee in dateTimeAll)
                                    {
                                        if (!DateTimeAdded.Any(existingDate => existingDate.Date == datee.Date))
                                        {
                                            vitals.Time.Add(datee.Date);  // Add only the date part
                                            vitals.Value.Add("null");
                                        }
                                    }
                                }

                                /* int i = 0;
                                 List<DateTime> DateTimeAdded = data.Measurements[i].Time;

                                 List<DateTime> newDateList = dateTimeAll.Except(DateTimeAdded).ToList();
                                 /* foreach (DateTime datee in dateTimeAll)
                                  {

                                      bool alreadyExists = DateTimeAdded.Any(x => x.Date==datee.Date);
                                      if (!alreadyExists)
                                      {
                                          foreach(var vitals in data.Measurements)
                                          {
                                              data.Measurements[i].Time.Add(datee);
                                              data.Measurements[i].Value.Add("null");

                                          }


                                      }


                                  }
                                  i++;
                                 foreach (DateTime datee in newDateList)
                                 {
                                     vitals.Time.Add(datee);
                                     vitals.Value.Add("null");
                                 }*/

                            }

                        }

                        /*DateTime[] dates = dt.AsEnumerable().Select(s => s.Field<DateTime>("CreatedOn").Date).Distinct().ToArray<DateTime>();
                        string[] measureValues = dt.AsEnumerable().Select(s => s.Field<string>("MeasureName")).Distinct().ToArray<string>();
                        foreach (string measureName in measureValues)
                        {
                            PatientSummary ret = new PatientSummary();
                            ret.Vital = measureName;
                            ret.Time = new List<DateTime>();
                            ret.Values = new List<Values>();

                            Values vv = new Values();
                            vv.data = new List<string>();
                            vv.label = measureName.ToString();
                            foreach (DateTime date in dates)
                            {
                                var result = dt.AsEnumerable().Where(myRow => myRow.Field<string>("MeasureName") == measureName.ToString() && myRow.Field<DateTime>("CreatedOn") == date)?.FirstOrDefault();
                                if (result == null)
                                {
                                    vv.data.Add("Nil");
                                }
                                else
                                {
                                    vv.data.Add(result["MeasureValue"].ToString());
                                }
                            }
                            ret.Values.Add(vv);
                            ret.Time.AddRange(dates);
                            lstSummery.Add(ret);
                        }*/


                        return lstSummery;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public VitalSummary GetVitalSummaryDetails(string Username, DateTime startDate, DateTime endDate, string ConnectionString)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(ConnectionString))
                {
                    con.Open();

                    using (SqlCommand command = new SqlCommand("usp_GetVitalSummaryDetails", con))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@username", Username);
                        command.Parameters.AddWithValue("@startDate", startDate);
                        command.Parameters.AddWithValue("@endDate", endDate);
                        SqlParameter returnParameter = command.Parameters.Add("RetVal", SqlDbType.Int);
                        returnParameter.Direction = ParameterDirection.ReturnValue;
                        SqlDataReader reader = command.ExecuteReader();
                        VitalSummary list = new VitalSummary();
                        list.vitals = new List<Vitalslist>();
                        DataTable dt = new DataTable();
                        while (!reader.IsClosed)
                        {

                            dt.Load(reader);

                        }
                        if (dt.Rows.Count == 0)
                        {
                            return null;
                        }

                        string[] VitalNames = dt.AsEnumerable().Select(s => s.Field<string>("Name")).Distinct().ToArray<string>();
                        DateTime[] Dates = dt.AsEnumerable().Select(s => s.Field<DateTime>("CreatedOn")).Distinct().OrderByDescending(s => s).ToArray<DateTime>();
                        Vitalslist vitalslist = new Vitalslist();
                        Vitalslist vitalslist1 = new Vitalslist();
                        vitalslist.VitalDetails = new List<VitalDetails>();
                        bool pulseAdded = false;
                        Vitalslist vitals = new Vitalslist();
                        vitals.VitalDetails = new List<VitalDetails>();
                        foreach (string vital in VitalNames)
                        {
                            vitalslist.VitalName = vital;
                            DataTable dt2 = dt.Clone();
                            dt.AsEnumerable().Where(myRow => myRow.Field<string>("Name") == vital.ToString()).ToList().ForEach(myRow => dt2.ImportRow(myRow));
                            string[] Names = dt2.AsEnumerable().Select(s => s.Field<string>("Name")).Distinct().ToArray<string>();
                            TimeSpan[] Time = dt2.AsEnumerable().Where(p => p.Field<string>("MeasureName") != "PULSE").Select(s => s.Field<TimeSpan>("time")).Distinct().OrderByDescending(x => x).ToArray<TimeSpan>();
                            TimeSpan[] PulseTime = dt2.AsEnumerable().Where(p => p.Field<string>("MeasureName") == "PULSE").Select(s => s.Field<TimeSpan>("time")).Distinct().OrderByDescending(x => x).ToArray<TimeSpan>();
                            string[] MeasureNames = dt2.AsEnumerable().Select(s => s.Field<string>("MeasureName")).Distinct().ToArray<string>();
                            foreach (string Name in Names)
                            {
                                if (Name == "Blood Pressure")
                                {
                                    VitalDetails vitalDetails = new VitalDetails();
                                    //vitalDetails.Date = date;
                                    vitals.VitalName = Name;
                                    vitalDetails.Vitaldata = new List<vitalData>();

                                    foreach (TimeSpan time in Time)
                                    {
                                        vitalData vitalData = new vitalData();
                                        DataTable tb = dt2.Clone();
                                        string value1 = string.Empty;
                                        string value2 = string.Empty;
                                        tb = dt2.AsEnumerable().Where(s => s.Field<TimeSpan>("time") == time).ToList().CopyToDataTable();
                                        tb.DefaultView.Sort = ("time desc");
                                        if (tb.Rows.Count > 0)
                                        {
                                            DateTime dateOnly = Convert.ToDateTime(tb.Rows[0]["CreatedOn"]).Date;
                                            DataRow drow = tb.AsEnumerable().Where(p => p.Field<string>("MeasureName") == MeasureNames[0].ToString()).FirstOrDefault();
                                            //drow[0].ToString();
                                            if (MeasureNames.Count() > 1)
                                            {
                                                DataRow drow1 = tb.AsEnumerable().Where(p => p.Field<string>("MeasureName") == MeasureNames[1].ToString()).FirstOrDefault();
                                                //vitalData.MeasureName = MeasureNames[0].ToString() + "/" + MeasureNames[1].ToString();
                                                if (drow == null && drow1 != null)
                                                {
                                                    vitalData.MeasureName = MeasureNames[1].ToString();
                                                    vitalData.Value = drow1[0].ToString();
                                                }
                                                else if (drow != null && drow1 == null)
                                                {
                                                    vitalData.MeasureName = MeasureNames[0].ToString();
                                                    vitalData.Value = drow[0].ToString();
                                                }
                                                else if (drow != null && drow1 != null)
                                                {
                                                    if (MeasureNames[1] == "Diastolic")
                                                    {
                                                        vitalData.MeasureName = MeasureNames[0].ToString() + "/" + MeasureNames[1].ToString();
                                                        vitalData.Value = drow[0].ToString() + "/" + drow1[0].ToString();
                                                    }
                                                    else
                                                    {
                                                        DataRow drow2 = tb.AsEnumerable().Where(p => p.Field<string>("MeasureName") == MeasureNames[2].ToString()).FirstOrDefault();
                                                        vitalData.MeasureName = MeasureNames[0].ToString() + "/" + MeasureNames[2].ToString();
                                                        vitalData.Value = drow[0].ToString() + "/" + drow2[0].ToString();
                                                    }

                                                }
                                                //vitalData.Value = drow[0].ToString() + "/" + drow1[0].ToString();
                                                vitalData.time = dateOnly.ToString("yyyy/MM/dd") + "T" + tb.Rows[0]["time"].ToString();
                                                vitalData.unit = tb.Rows[0]["UnitName"].ToString();
                                                vitalDetails.Vitaldata.Add(vitalData);
                                            }
                                            else
                                            {
                                                // DataRow drow1 = tb.AsEnumerable().Where(p => p.Field<string>("MeasureName") == MeasureNames[1].ToString()).FirstOrDefault();
                                                vitalData.MeasureName = MeasureNames[0].ToString();
                                                vitalData.Value = drow[0].ToString();
                                                vitalData.time = tb.Rows[0]["time"].ToString();
                                                vitalData.unit = tb.Rows[0]["UnitName"].ToString();
                                                vitalDetails.Vitaldata.Add(vitalData);
                                            }
                                        }
                                        else
                                        {
                                            DateTime dateOnly = Convert.ToDateTime(tb.Rows[0]["CreatedOn"]).Date;
                                            var Value1result = tb.AsEnumerable().Where(p => p.Field<string>("MeasureName") == MeasureNames[0].ToString()).Select(s => s.Field<TimeSpan>("time")).Distinct().OrderByDescending(x => x).ToArray<TimeSpan>();
                                            vitalData.MeasureName = MeasureNames[0].ToString();
                                            vitalData.Value = Value1result.ToString();
                                            vitalData.time = dateOnly.ToString("yyyy/MM/dd") + "T" + tb.Rows[0]["time"].ToString();
                                            vitalData.unit = tb.Rows[0]["UnitName"].ToString();
                                            vitalDetails.Vitaldata.Add(vitalData);
                                        }
                                    }
                                    vitalslist.VitalDetails.Add(vitalDetails);
                                }
                                else
                                {
                                    VitalDetails vitalDetails = new VitalDetails();
                                    //vitalDetails.Date = date;
                                    vitalDetails.Vitaldata = new List<vitalData>();
                                    foreach (DataRow row in dt2.Rows)
                                    {
                                        DateTime dateOnly = Convert.ToDateTime(row["CreatedOn"]).Date;
                                        vitalData vitalData = new vitalData();

                                        vitalData.MeasureName = row["MeasureName"].ToString();

                                        vitalData.Value = row["MeasureValue"].ToString();
                                        vitalData.time = dateOnly.ToString("yyyy/MM/dd") + "T" + row["time"].ToString();
                                        vitalData.unit = row["UnitName"].ToString();
                                        vitalDetails.Vitaldata.Add(vitalData);
                                    }
                                    vitalslist.VitalDetails.Add(vitalDetails);
                                }
                                list.vitals.Add(vitalslist);
                            }

                            foreach (string Name in Names)
                            {
                                if (MeasureNames.Contains("Pulse") && pulseAdded == false)
                                {
                                    VitalDetails vitalDetails = new VitalDetails();
                                    Vitalslist vitalslistpulse = new Vitalslist();
                                    vitalDetails.Vitaldata = new List<vitalData>();
                                    vitalslistpulse.VitalDetails = new List<VitalDetails>();
                                    DataTable tbpulse = dt2.Clone();

                                    VitalDetails vitalDetailspulse = new VitalDetails();
                                    vitalDetailspulse.Vitaldata = new List<vitalData>();
                                    var tab = dt.AsEnumerable().Where(s => s.Field<string>("MeasureName") == "Pulse").ToList();

                                    if (tab.Any())
                                    {
                                        tbpulse = dt.AsEnumerable().Where(s => s.Field<string>("MeasureName") == "Pulse").ToList().CopyToDataTable();
                                    }
                                    else
                                    {
                                        continue;
                                    }
                                    tbpulse.DefaultView.Sort = "time desc";
                                    tbpulse = tbpulse.DefaultView.ToTable();
                                    if (tbpulse.Rows.Count > 0)
                                    {
                                        foreach (DataRow row in tbpulse.Rows)
                                        {
                                            DateTime dateOnly = Convert.ToDateTime(row["CreatedOn"]).Date;
                                            vitalData vitalData = new vitalData();
                                            vitalData.MeasureName = row["MeasureName"].ToString();
                                            vitalData.Value = row["MeasureValue"].ToString(); ;
                                            vitalData.time = dateOnly.ToString("yyyy/MM/dd") + "T" + row["time"].ToString();
                                            vitalData.unit = row["UnitName"].ToString();
                                            vitalDetailspulse.Vitaldata.Add(vitalData);
                                        }
                                        //vitalDetailspulse.Date = datetime;
                                        vitalslistpulse.VitalDetails.Add(vitalDetailspulse);
                                    }


                                    pulseAdded = true;
                                    vitalslistpulse.VitalName = "Pulse";
                                    //vitalslistpulse.VitalDetails.Add(vitalDetails);
                                    list.vitals.Add(vitalslistpulse);

                                }
                                // list.vitals.Add(vitalslist);
                            }

                        }

                        return list;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public List<GetPatientInfo> GetPatientInfo(string UserName, string ConnectionString)
        {

            try
            {
                using (SqlConnection con = new SqlConnection(ConnectionString))
                {
                    con.Open();

                    using (SqlCommand command = new SqlCommand("usp_GetPatientinfo", con))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        command.Parameters.AddWithValue("@UserName", UserName);

                        SqlParameter returnParameter = command.Parameters.Add("RetVal", SqlDbType.Int);
                        returnParameter.Direction = ParameterDirection.ReturnValue;
                        SqlDataReader reader = command.ExecuteReader();
                        List<GetPatientInfo> ListPinfo = new List<GetPatientInfo>();
                        while (reader.Read())
                        {
                            GetPatientInfo ret = new GetPatientInfo();
                            //ret.Id = (int)reader["Id"];
                            ret.FirstName = reader["FirstName"].ToString();
                            ret.MiddleName = reader["MiddleName"].ToString();
                            ret.LastName = reader["LastName"].ToString();
                            ret.DOB = reader["DOB"].ToString();
                            ret.Gender = reader["Gender"].ToString();
                            ret.Height = Convert.ToInt32(reader["Height"]);
                            ret.Weight = Convert.ToInt32(reader["Weight"]);
                            ret.Email = reader["Email"].ToString();
                            ret.PhoneNo = reader["PhoneNo"].ToString();
                            ret.AlternateMobNo = reader["AlternateMobNo"].ToString();
                            ret.Address1 = reader["Address1"].ToString();
                            ret.Address2 = reader["Address2"].ToString();
                            ret.ZipCode = reader["ZipCode"].ToString();
                            ret.City = reader["City"].ToString();
                            ret.State = reader["State"].ToString();
                            ret.TimeZoneId = Convert.ToInt32(reader["TimeZoneId"]);
                            ret.TimeZone = reader["TimeZone"].ToString();
                            ret.EmergencyContact1 = reader["EmergencyContact1"].ToString();
                            ret.EmergencyContactNumber1 = reader["EmergencyContactNumber1"].ToString();
                            ret.EmergencyContact1Relation = reader["EmergencyContact1Relation"].ToString();
                            ret.EmergencyContact2 = reader["EmergencyContact2"].ToString();
                            ret.EmergencyContactNumber2 = reader["EmergencyContactNumber2"].ToString();
                            ret.EmergencyContact2Relation = reader["EmergencyContact2Relation"].ToString();
                            ret.CallTime = reader["CallTime"].ToString();
                            ret.Language = reader["Language"].ToString();
                            ret.Preference1 = reader["Preference1"].ToString();
                            ret.Preference2 = reader["Preference2"].ToString();
                            ret.Preference3 = reader["Preference3"].ToString();
                            ret.Notes = reader["Notes"].ToString();

                            ListPinfo.Add(ret);
                        }
                        if (reader.FieldCount == 0)
                        {
                            return null;
                        }
                        return ListPinfo;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public string GetPatientBillingReport(DateTime startDate, DateTime endDate, int? patientId, string clinic, string cptCode, int isMonth, string CreatedBy, string Format, string ConnectionString, string Blob_Conn_String, string ContainerName)
        {
            bool ispatient = false;
            bool isClinic = false;
            bool isCptCode = false;
            string Uri = "";
            //List<PatientBillReport> list = new List<PatientBillReport>();
            List<PatientBillReport> Reportlist = new List<PatientBillReport>();
            try
            {
                using (SqlConnection con = new SqlConnection(ConnectionString))
                {
                    con.Open();

                    using (SqlCommand command = new SqlCommand("usp_GetPatientBillingReport", con))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.Add("@startDate", SqlDbType.DateTime).Value = startDate;
                        command.Parameters.Add("@endDate", SqlDbType.DateTime).Value = endDate;
                        if (!String.IsNullOrEmpty(patientId?.ToString()))
                        {
                            command.Parameters.Add("@patientId", SqlDbType.Int).Value = patientId;
                            ispatient = true;
                        }
                        else
                        {
                            command.Parameters.Add("@patientId", SqlDbType.Int).Value = DBNull.Value;
                        }

                        // command.Parameters.Add("@CreatedBy", SqlDbType.NVarChar).Value = CreatedBy;
                        SqlDataReader reader = command.ExecuteReader();
                        while (reader.Read())
                        {
                            PatientBillReport info = new PatientBillReport();
                            info.PatientId = reader["PatientId"].ToString();
                            info.BillingCode = reader["BillingCode"].ToString();
                            info.PatientNumber = reader["PatientNumber"].ToString();
                            info.FirstName = reader["FirstName"].ToString();
                            info.LastName = reader["LastName"].ToString();
                            DateTime dob = Convert.ToDateTime(reader["DOB"]);
                            info.Dob = dob.ToString("yyyy-MM-dd");
                            info.clinic = reader["ClinicId"].ToString();
                            info.ClinicName = reader["ClinicName"].ToString();
                            info.ProgramName = reader["ProgramName"].ToString();
                            DateTime ped = Convert.ToDateTime(reader["EnrolledDate"]);
                            info.ProgramEnrolledDate = ped.ToString("yyyy-MM-dd");
                            DateTime dateTime = Convert.ToDateTime(reader["BilledDate"]);
                            info.ReadyToBillDate = dateTime.ToString("yyyy-MM-dd");
                            info.Physician = reader["Physician"].ToString();
                            info.DiagnosisCode = reader["DiagnosticsCode"].ToString();
                            string str = info.DiagnosisCode;
                            List<string> uniques = str.Split(',').Reverse().Distinct().Take(2).Reverse().ToList();
                            string newStr = string.Join(",", uniques);
                            info.DiagnosisCode = newStr;

                            info.DeviceName = reader["DeviceNames"].ToString();
                            if (info.DeviceName==null || info.DeviceName==String.Empty)
                                info.DeviceName="Not Available";
                            info.DeviceActivateDate = (!DBNull.Value.Equals(reader["DeviceActivatedDateTime"])) ? reader["DeviceActivatedDateTime"].ToString() : String.Empty;
                            if (info.DeviceActivateDate==null || info.DeviceActivateDate==String.Empty)
                                info.DeviceActivateDate="Not Available";
                            info.LastReadyToBillDate = (!DBNull.Value.Equals(reader["LastBilledDate"])) ? reader["LastBilledDate"].ToString() : String.Empty;
                            if (info.LastReadyToBillDate != String.Empty)
                                info.LastReadyToBillDate = Convert.ToDateTime(info.LastReadyToBillDate).ToString("yyyy-MM-dd");
                            Reportlist.Add(info);
                            //bug fix R12 - added back code for showing 99458 billed records as more than one if the duration is above 40 min
                            int tgReadings = Convert.ToInt32(reader["TargetReadings"]);
                            tgReadings = tgReadings * 60; // in secs
                            int totReadings = Convert.ToInt32(reader["TotalReadings"]);
                            if (info.BillingCode.Equals("99458") & totReadings >= tgReadings)
                            {
                                Reportlist.Add(info);
                            }
                            if (info.BillingCode.Equals("99439") & totReadings >= tgReadings)
                            {
                                int duration = totReadings-tgReadings;
                                int intervals = duration / tgReadings;
                                for (int i = 0; i < intervals; i++)
                                {
                                    Reportlist.Add(info);
                                }
                            }
                            if (info.BillingCode.Equals("99437") & totReadings >= tgReadings)
                            {
                                int duration = totReadings-tgReadings;
                                int intervals = duration / tgReadings;
                                for (int i = 0; i < intervals; i++)
                                {
                                    Reportlist.Add(info);
                                }
                            }
                            if (info.BillingCode.Equals("99425") & totReadings >= tgReadings)
                            {
                                int duration = totReadings-tgReadings;
                                int intervals = duration / tgReadings;
                                for (int i = 0; i < intervals; i++)
                                {
                                    Reportlist.Add(info);
                                }
                            }
                            if (info.BillingCode.Equals("99427") & totReadings >= tgReadings)
                            {
                                int duration = totReadings-tgReadings;
                                int intervals = duration / tgReadings;
                                for (int i = 0; i < intervals; i++)
                                {
                                    Reportlist.Add(info);
                                }
                            }
                            if (info.BillingCode.Equals("99489") & totReadings >= tgReadings)
                            {
                                int duration = totReadings-tgReadings;
                                int intervals = duration / tgReadings;
                                for (int i = 0; i < intervals; i++)
                                {
                                    Reportlist.Add(info);
                                }
                            }

                        }
                        reader.Close();
                        /*if (list.Count > 0)
                        {

                            foreach (var item in list)
                            {
                                using (SqlCommand commands = new SqlCommand("usp_GetReportData", con))
                                {

                                    commands.CommandType = CommandType.StoredProcedure;
                                    commands.Parameters.Add("@patientId", SqlDbType.Int).Value = item.PatientId;
                                    commands.Parameters.Add("@billingCode", SqlDbType.NVarChar).Value = item.BillingCode.ToString();

                                    // command.Parameters.Add("@CreatedBy", SqlDbType.NVarChar).Value = CreatedBy;
                                    SqlDataReader readers = commands.ExecuteReader();

                                    while (readers.Read())
                                    {

                                        item.DiagnosisCode = readers["DiagnosisCode"].ToString();
                                        item.DeviceName = readers["DeviceNames"].ToString();
                                        DateTime? lastDate = (readers["LastBilledDate"] as DateTime?) ?? null;
                                        if (lastDate == null)
                                        {
                                            item.LastReadyToBillDate = null;
                                        }
                                        else
                                        {
                                            DateTime dt = Convert.ToDateTime(readers["LastBilledDate"]);
                                            item.LastReadyToBillDate = dt.ToString("yyyy-MM-dd");

                                        }
                                        item.DeviceActivateDate = readers["DeviceActivationdate"].ToString();

                                        Reportlist.Add(item);

                                    }
                                    readers.Close();

                                }

                            }
                            list.Clear();
                        }*/
                    }
                }
                if (!string.IsNullOrEmpty(clinic) && !string.IsNullOrEmpty(cptCode))
                {
                    Reportlist = (List<PatientBillReport>)Reportlist.Where(s => s.clinic == clinic && s.BillingCode == cptCode).ToList();
                    isClinic = true;
                    isCptCode = true;
                    if (Reportlist.Count > 0)
                        clinic = Reportlist[0].ClinicName.ToString();
                }
                else if (!string.IsNullOrEmpty(clinic) && string.IsNullOrEmpty(cptCode))
                {
                    Reportlist = (List<PatientBillReport>)Reportlist.Where(s => s.clinic == clinic).ToList();
                    isClinic = true;
                    if (Reportlist.Count > 0)
                        clinic = Reportlist[0].ClinicName.ToString();
                }
                else if (!string.IsNullOrEmpty(cptCode) && string.IsNullOrEmpty(clinic))
                {
                    Reportlist = (List<PatientBillReport>)Reportlist.Where(s => s.BillingCode == cptCode).ToList();
                    isCptCode = true;
                }

                if (Reportlist.Count > 0)
                {
                    Uri = genaratePdf(Reportlist, patientId, clinic, cptCode, isMonth, startDate, endDate, Blob_Conn_String, ContainerName, Format);
                }

                return Uri;
            }
            catch (Exception)
            {

                throw;
            }

        }

        public string GetPatientBillingReportDetailsCCM(DateTime startDate, DateTime endDate, int? patientId, string clinic, int isMonth, string CreatedBy, string Format, string ConnectionString, string Blob_Conn_String, string ContainerName)
        {
            bool ispatient = false;
            bool isClinic = false;
            bool isCptCode = false;
            string Uri = "";
            //List<PatientBillReport> list = new List<PatientBillReport>();
            List<PatientBillReportDetailsCCM> Reportlist = new List<PatientBillReportDetailsCCM>();
            try
            {
                using (SqlConnection con = new SqlConnection(ConnectionString))
                {
                    con.Open();

                    using (SqlCommand command = new SqlCommand("usp_GetPatientBillingReportDetailsCCM", con))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.Add("@startDate", SqlDbType.DateTime).Value = startDate;
                        command.Parameters.Add("@endDate", SqlDbType.DateTime).Value = endDate;
                        if (!String.IsNullOrEmpty(patientId?.ToString()))
                        {
                            command.Parameters.Add("@patientId", SqlDbType.Int).Value = patientId;
                            ispatient = true;
                        }
                        else
                        {
                            command.Parameters.Add("@patientId", SqlDbType.Int).Value = DBNull.Value;
                        }

                        // command.Parameters.Add("@CreatedBy", SqlDbType.NVarChar).Value = CreatedBy;
                        SqlDataReader reader = command.ExecuteReader();
                        while (reader.Read())
                        {
                            PatientBillReportDetailsCCM info = new PatientBillReportDetailsCCM();
                            info.PatientName = reader["PATIENT"].ToString();
                            info.PatientNumber = reader["PatientNumber"].ToString();
                            info.DOS = Convert.ToDateTime(reader["DOS"]).ToShortDateString();
                            info.DOB = Convert.ToDateTime(reader["DOB"]).ToShortDateString();
                            string codes = reader["ApplicableCodes"].ToString();
                            string[] ApplicableCodes = codes.Split(',');
                            string[] ApplicableCPTCodes = ApplicableCodes.Distinct().ToArray();
                            info.CPT99490 = "NA";
                            info.CPT99439 = "NA";
                            info.CPT99491 = "NA";
                            info.CPT99437 = "NA";
                            info.CPT99487 = "NA";
                            info.CPT99489 = "NA";
                            if (ApplicableCPTCodes.Contains("99490"))
                            {
                                info.CPT99490 = reader["99490"].ToString();
                                info.CPT99490 =  Convert.ToString(ConvertSecondsToMinutes(Convert.ToInt32(info.CPT99490)));
                            }
                            if (ApplicableCPTCodes.Contains("99439"))
                            {
                                info.CPT99439 = reader["99439"].ToString();
                                info.CPT99439 = Convert.ToString(ConvertSecondsToMinutes(Convert.ToInt32(info.CPT99439)));
                            }
                            if (ApplicableCPTCodes.Contains("99491"))
                            {
                                info.CPT99491 = reader["99491"].ToString();
                                info.CPT99491 = Convert.ToString(ConvertSecondsToMinutes(Convert.ToInt32(info.CPT99491)));
                            }
                            if (ApplicableCPTCodes.Contains("99437"))
                            {
                                info.CPT99437 = reader["99437"].ToString();
                                info.CPT99437 = Convert.ToString(ConvertSecondsToMinutes(Convert.ToInt32(info.CPT99437)));
                            }
                            if (ApplicableCPTCodes.Contains("99487"))
                            {
                                info.CPT99487 = reader["99487"].ToString();
                                info.CPT99487 = Convert.ToString(ConvertSecondsToMinutes(Convert.ToInt32(info.CPT99487)));
                            }
                            if (ApplicableCPTCodes.Contains("99489"))
                            {
                                info.CPT99489 = reader["99489"].ToString();
                                info.CPT99489 = Convert.ToString(ConvertSecondsToMinutes(Convert.ToInt32(info.CPT99489)));
                            }







                            info.EnrolledDate = Convert.ToDateTime(reader["ENROLL DATE"]);
                            info.clinic = reader["CLINIC"].ToString();
                            info.clinicname = reader["clinicname"].ToString();
                            Reportlist.Add(info);

                        }
                        reader.Close();

                    }
                }
                if (!string.IsNullOrEmpty(clinic))
                {
                    Reportlist = (List<PatientBillReportDetailsCCM>)Reportlist.Where(s => s.clinic == clinic).ToList();
                    isClinic = true;
                    isCptCode = true;
                    if (Reportlist.Count>0)
                        clinic = Reportlist[0].clinicname.ToString();
                }

                if (Reportlist.Count > 0)
                {
                    Uri = genaratePdfForDetailsCCM(Reportlist, patientId, clinic, isMonth, startDate, endDate, Blob_Conn_String, ContainerName, Format);
                }

                return Uri;
            }
            catch (Exception)
            {

                throw;
            }

        }
        public string GetPatientMissingBillingReportDetailsCCM(DateTime startDate, DateTime endDate, int? patientId, string clinic, int isMonth, string CreatedBy, string Format, string ConnectionString, string Blob_Conn_String, string ContainerName)
        {
            bool ispatient = false;
            bool isClinic = false;
            bool isCptCode = false;
            string Uri = "";
            //List<PatientBillReport> list = new List<PatientBillReport>();
            List<PatientBillReportDetailsCCM> Reportlist = new List<PatientBillReportDetailsCCM>();
            try
            {
                using (SqlConnection con = new SqlConnection(ConnectionString))
                {
                    con.Open();

                    using (SqlCommand command = new SqlCommand("usp_GetPatientMissingBillingReportDetailsCCM", con))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.Add("@startDate", SqlDbType.DateTime).Value = startDate;
                        command.Parameters.Add("@endDate", SqlDbType.DateTime).Value = endDate;
                        if (!String.IsNullOrEmpty(patientId?.ToString()))
                        {
                            command.Parameters.Add("@patientId", SqlDbType.Int).Value = patientId;
                            ispatient = true;
                        }
                        else
                        {
                            command.Parameters.Add("@patientId", SqlDbType.Int).Value = DBNull.Value;
                        }

                        // command.Parameters.Add("@CreatedBy", SqlDbType.NVarChar).Value = CreatedBy;
                        SqlDataReader reader = command.ExecuteReader();
                        while (reader.Read())
                        {
                            PatientBillReportDetailsCCM info = new PatientBillReportDetailsCCM();
                            info.PatientName = reader["PATIENT"].ToString();
                            info.PatientNumber = reader["PatientNumber"].ToString();
                            info.DOS = Convert.ToDateTime(reader["DOS"]).ToShortDateString();
                            info.DOB = Convert.ToDateTime(reader["DOB"]).ToShortDateString();
                            string codes = reader["ApplicableCodes"].ToString();
                            string[] ApplicableCodes = codes.Split(',');
                            string[] ApplicableCPTCodes = ApplicableCodes.Distinct().ToArray();
                            info.CPT99490 = "NA";
                            info.CPT99439 = "NA";
                            info.CPT99491 = "NA";
                            info.CPT99437 = "NA";
                            info.CPT99487 = "NA";
                            info.CPT99489 = "NA";
                            if (ApplicableCPTCodes.Contains("99490"))
                            {
                                info.CPT99490 = reader["99490"].ToString();
                                info.CPT99490 =  Convert.ToString(ConvertSecondsToMinutes(Convert.ToInt32(info.CPT99490)));
                            }
                            if (ApplicableCPTCodes.Contains("99439"))
                            {
                                info.CPT99439 = reader["99439"].ToString();
                                info.CPT99439 = Convert.ToString(ConvertSecondsToMinutes(Convert.ToInt32(info.CPT99439)));
                            }
                            if (ApplicableCPTCodes.Contains("99491"))
                            {
                                info.CPT99491 = reader["99491"].ToString();
                                info.CPT99491 = Convert.ToString(ConvertSecondsToMinutes(Convert.ToInt32(info.CPT99491)));
                            }
                            if (ApplicableCPTCodes.Contains("99437"))
                            {
                                info.CPT99437 = reader["99437"].ToString();
                                info.CPT99437 = Convert.ToString(ConvertSecondsToMinutes(Convert.ToInt32(info.CPT99437)));
                            }
                            if (ApplicableCPTCodes.Contains("99487"))
                            {
                                info.CPT99487 = reader["99487"].ToString();
                                info.CPT99487 = Convert.ToString(ConvertSecondsToMinutes(Convert.ToInt32(info.CPT99487)));
                            }
                            if (ApplicableCPTCodes.Contains("99489"))
                            {
                                info.CPT99489 = reader["99489"].ToString();
                                info.CPT99489 = Convert.ToString(ConvertSecondsToMinutes(Convert.ToInt32(info.CPT99489)));
                            }







                            info.EnrolledDate = Convert.ToDateTime(reader["ENROLL DATE"]);
                            info.clinic = reader["CLINIC"].ToString();
                            info.clinicname = reader["clinicname"].ToString();
                            Reportlist.Add(info);

                        }
                        reader.Close();

                    }
                }
                if (!string.IsNullOrEmpty(clinic))
                {
                    Reportlist = (List<PatientBillReportDetailsCCM>)Reportlist.Where(s => s.clinic == clinic).ToList();
                    isClinic = true;
                    isCptCode = true;
                    if (Reportlist.Count>0)
                        clinic = Reportlist[0].clinicname.ToString();
                }

                if (Reportlist.Count > 0)
                {
                    Uri = genaratePdfForDetailsCCM(Reportlist, patientId, clinic, isMonth, startDate, endDate, Blob_Conn_String, ContainerName, Format);
                }

                return Uri;
            }
            catch (Exception)
            {

                throw;
            }

        }

        public string GetPatientBillingReportDetailsPCM(DateTime startDate, DateTime endDate, int? patientId, string clinic, int isMonth, string CreatedBy, string Format, string ConnectionString, string Blob_Conn_String, string ContainerName)
        {
            bool ispatient = false;
            bool isClinic = false;
            bool isCptCode = false;
            string Uri = "";
            //List<PatientBillReport> list = new List<PatientBillReport>();
            List<PatientBillReportDetailsPCM> Reportlist = new List<PatientBillReportDetailsPCM>();
            try
            {
                using (SqlConnection con = new SqlConnection(ConnectionString))
                {
                    con.Open();

                    using (SqlCommand command = new SqlCommand("usp_GetPatientBillingReportDetailsPCM", con))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.Add("@startDate", SqlDbType.DateTime).Value = startDate;
                        command.Parameters.Add("@endDate", SqlDbType.DateTime).Value = endDate;
                        if (!String.IsNullOrEmpty(patientId?.ToString()))
                        {
                            command.Parameters.Add("@patientId", SqlDbType.Int).Value = patientId;
                            ispatient = true;
                        }
                        else
                        {
                            command.Parameters.Add("@patientId", SqlDbType.Int).Value = DBNull.Value;
                        }

                        // command.Parameters.Add("@CreatedBy", SqlDbType.NVarChar).Value = CreatedBy;
                        SqlDataReader reader = command.ExecuteReader();
                        while (reader.Read())
                        {
                            PatientBillReportDetailsPCM info = new PatientBillReportDetailsPCM();
                            info.PatientName = reader["PATIENT"].ToString();
                            info.PatientNumber = reader["PatientNumber"].ToString();
                            info.DOS = Convert.ToDateTime(reader["DOS"]).ToShortDateString();
                            info.DOB = Convert.ToDateTime(reader["DOB"]).ToShortDateString();
                            string codes = reader["ApplicableCodes"].ToString();
                            string[] ApplicableCodes = codes.Split(',');
                            string[] ApplicableCPTCodes = ApplicableCodes.Distinct().ToArray();
                            info.CPT99424 = "NA";
                            info.CPT99425 = "NA";
                            info.CPT99426 = "NA";
                            info.CPT99427 = "NA";
                            if (ApplicableCPTCodes.Contains("99424"))
                            {
                                info.CPT99424 = reader["99424"].ToString();
                                info.CPT99424 =  Convert.ToString(ConvertSecondsToMinutes(Convert.ToInt32(info.CPT99424)));
                            }
                            if (ApplicableCPTCodes.Contains("99425"))
                            {
                                info.CPT99425 = reader["99425"].ToString();
                                info.CPT99425 =  Convert.ToString(ConvertSecondsToMinutes(Convert.ToInt32(info.CPT99425)));
                            }
                            if (ApplicableCPTCodes.Contains("99426"))
                            {
                                info.CPT99426 = reader["99426"].ToString();
                                info.CPT99426 =  Convert.ToString(ConvertSecondsToMinutes(Convert.ToInt32(info.CPT99426)));
                            }
                            if (ApplicableCPTCodes.Contains("99427"))
                            {
                                info.CPT99427 = reader["99427"].ToString();
                                info.CPT99427 =  Convert.ToString(ConvertSecondsToMinutes(Convert.ToInt32(info.CPT99427)));
                            }




                            info.EnrolledDate = Convert.ToDateTime(reader["ENROLL DATE"]);
                            info.clinic = reader["CLINIC"].ToString();
                            info.clinicname = reader["clinicname"].ToString();
                            Reportlist.Add(info);

                        }
                        reader.Close();

                    }
                }
                if (!string.IsNullOrEmpty(clinic))
                {
                    Reportlist = (List<PatientBillReportDetailsPCM>)Reportlist.Where(s => s.clinic == clinic).ToList();
                    isClinic = true;
                    isCptCode = true;
                    if (Reportlist.Count>0)
                        clinic = Reportlist[0].clinicname.ToString();
                }

                if (Reportlist.Count > 0)
                {
                    Uri = genaratePdfForDetailsPCM(Reportlist, patientId, clinic, isMonth, startDate, endDate, Blob_Conn_String, ContainerName, Format);
                }

                return Uri;
            }
            catch (Exception)
            {

                throw;
            }

        }

        public string GetPatientMissingBillingReportDetailsPCM(DateTime startDate, DateTime endDate, int? patientId, string clinic, int isMonth, string CreatedBy, string Format, string ConnectionString, string Blob_Conn_String, string ContainerName)
        {
            bool ispatient = false;
            bool isClinic = false;
            bool isCptCode = false;
            string Uri = "";
            //List<PatientBillReport> list = new List<PatientBillReport>();
            List<PatientBillReportDetailsPCM> Reportlist = new List<PatientBillReportDetailsPCM>();
            try
            {
                using (SqlConnection con = new SqlConnection(ConnectionString))
                {
                    con.Open();

                    using (SqlCommand command = new SqlCommand("usp_GetPatientMissingBillingReportDetailsPCM", con))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.Add("@startDate", SqlDbType.DateTime).Value = startDate;
                        command.Parameters.Add("@endDate", SqlDbType.DateTime).Value = endDate;
                        if (!String.IsNullOrEmpty(patientId?.ToString()))
                        {
                            command.Parameters.Add("@patientId", SqlDbType.Int).Value = patientId;
                            ispatient = true;
                        }
                        else
                        {
                            command.Parameters.Add("@patientId", SqlDbType.Int).Value = DBNull.Value;
                        }

                        // command.Parameters.Add("@CreatedBy", SqlDbType.NVarChar).Value = CreatedBy;
                        SqlDataReader reader = command.ExecuteReader();
                        while (reader.Read())
                        {
                            PatientBillReportDetailsPCM info = new PatientBillReportDetailsPCM();
                            info.PatientName = reader["PATIENT"].ToString();
                            info.PatientNumber = reader["PatientNumber"].ToString();
                            info.DOS = Convert.ToDateTime(reader["DOS"]).ToShortDateString();
                            info.DOB = Convert.ToDateTime(reader["DOB"]).ToShortDateString();
                            string codes = reader["ApplicableCodes"].ToString();
                            string[] ApplicableCodes = codes.Split(',');
                            string[] ApplicableCPTCodes = ApplicableCodes.Distinct().ToArray();
                            info.CPT99424 = "NA";
                            info.CPT99425 = "NA";
                            info.CPT99426 = "NA";
                            info.CPT99427 = "NA";
                            if (ApplicableCPTCodes.Contains("99424"))
                            {
                                info.CPT99424 = reader["99424"].ToString();
                                info.CPT99424 =  Convert.ToString(ConvertSecondsToMinutes(Convert.ToInt32(info.CPT99424)));
                            }
                            if (ApplicableCPTCodes.Contains("99425"))
                            {
                                info.CPT99425 = reader["99425"].ToString();
                                info.CPT99425 =  Convert.ToString(ConvertSecondsToMinutes(Convert.ToInt32(info.CPT99425)));
                            }
                            if (ApplicableCPTCodes.Contains("99426"))
                            {
                                info.CPT99426 = reader["99426"].ToString();
                                info.CPT99426 =  Convert.ToString(ConvertSecondsToMinutes(Convert.ToInt32(info.CPT99426)));
                            }
                            if (ApplicableCPTCodes.Contains("99427"))
                            {
                                info.CPT99427 = reader["99427"].ToString();
                                info.CPT99427 =  Convert.ToString(ConvertSecondsToMinutes(Convert.ToInt32(info.CPT99427)));
                            }




                            info.EnrolledDate = Convert.ToDateTime(reader["ENROLL DATE"]);
                            info.clinic = reader["CLINIC"].ToString();
                            info.clinicname = reader["clinicname"].ToString();
                            Reportlist.Add(info);

                        }
                        reader.Close();

                    }
                }
                if (!string.IsNullOrEmpty(clinic))
                {
                    Reportlist = (List<PatientBillReportDetailsPCM>)Reportlist.Where(s => s.clinic == clinic).ToList();
                    isClinic = true;
                    isCptCode = true;
                    if (Reportlist.Count>0)
                        clinic = Reportlist[0].clinicname.ToString();
                }

                if (Reportlist.Count > 0)
                {
                    Uri = genaratePdfForDetailsPCM(Reportlist, patientId, clinic, isMonth, startDate, endDate, Blob_Conn_String, ContainerName, Format);
                }

                return Uri;
            }
            catch (Exception)
            {

                throw;
            }

        }

        public string genaratePdfForDetailsCCM(List<PatientBillReportDetailsCCM> data, int? Patientid, string clinic, int isMonth, DateTime startDate, DateTime endDate, string Blob_Conn_String, string ContainerName, string format)
        {
            string Uri = "";
            DataTable dataTable = new DataTable(typeof(PatientBillReportDetailsCCM).Name);

            //Get all the properties
            PropertyInfo[] Props = typeof(PatientBillReportDetailsCCM).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo prop in Props)
            {
                //Defining type of data column gives proper data table 
                var type = (prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>) ? Nullable.GetUnderlyingType(prop.PropertyType) : prop.PropertyType);
                //Setting column names as Property names
                dataTable.Columns.Add(prop.Name, type);
            }
            foreach (PatientBillReportDetailsCCM item in data.OrderBy(x => Convert.ToDateTime(x.DOS)).ToList())
            {
                var values = new object[Props.Length];
                for (int i = 0; i < Props.Length; i++)
                {
                    //inserting property values to datatable rows
                    values[i] = Props[i].GetValue(item, null);
                }
                dataTable.Rows.Add(values);
            }

            // dataTable.Columns.Remove("patientId");
            dataTable.Columns.Remove("clinic");
            dataTable.Columns.Remove("clinicname");
            try
            {
                BlobContainerClient containerClient = new BlobContainerClient(Blob_Conn_String, ContainerName);
                string Filename = "PatientBillingReportDetails_"  + DateTime.Now.Day+DateTime.Now.Month+DateTime.Now.Year+DateTime.Now.Hour+DateTime.Now.Minute+DateTime.Now.Second + "." + format;
                var filePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\" + "rpmfolder" + Path.GetExtension(Filename);
                if (!Directory.Exists(filePath))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                }

                if (format == "xlsx")
                {
                    WriteExcelFile(filePath, dataTable);
                }
                else
                {
                    ExportDataTableToPdfDetails(dataTable, Patientid, clinic, isMonth, startDate, endDate, filePath);
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
        public string genaratePdfForDetailsPCM(List<PatientBillReportDetailsPCM> data, int? Patientid, string clinic, int isMonth, DateTime startDate, DateTime endDate, string Blob_Conn_String, string ContainerName, string format)
        {
            string Uri = "";
            DataTable dataTable = new DataTable(typeof(PatientBillReportDetailsPCM).Name);

            //Get all the properties
            PropertyInfo[] Props = typeof(PatientBillReportDetailsPCM).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo prop in Props)
            {
                //Defining type of data column gives proper data table 
                var type = (prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>) ? Nullable.GetUnderlyingType(prop.PropertyType) : prop.PropertyType);
                //Setting column names as Property names
                dataTable.Columns.Add(prop.Name, type);
            }
            foreach (PatientBillReportDetailsPCM item in data.OrderBy(x => Convert.ToDateTime(x.DOS)).ToList())
            {
                var values = new object[Props.Length];
                for (int i = 0; i < Props.Length; i++)
                {
                    //inserting property values to datatable rows
                    values[i] = Props[i].GetValue(item, null);
                }
                dataTable.Rows.Add(values);
            }

            // dataTable.Columns.Remove("patientId");
            dataTable.Columns.Remove("clinic");
            dataTable.Columns.Remove("clinicname");
            try
            {
                BlobContainerClient containerClient = new BlobContainerClient(Blob_Conn_String, ContainerName);
                string Filename = "PatientBillingReportDetails_"  + DateTime.Now.Day+DateTime.Now.Month+DateTime.Now.Year+DateTime.Now.Hour+DateTime.Now.Minute+DateTime.Now.Second + "." + format;
                var filePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\" + "rpmfolder" + Path.GetExtension(Filename);
                if (!Directory.Exists(filePath))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                }

                if (format == "xlsx")
                {
                    WriteExcelFile(filePath, dataTable);
                }
                else
                {
                    ExportDataTableToPdfDetails(dataTable, Patientid, clinic, isMonth, startDate, endDate, filePath);
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
        public string genaratePdf(List<PatientBillReport> data, int? Patientid, string clinic, string cptCoode, int isMonth, DateTime startDate, DateTime endDate, string Blob_Conn_String, string ContainerName, string format)
        {
            string Uri = "";
            DataTable dataTable = new DataTable(typeof(PatientBillReport).Name);

            //Get all the properties
            PropertyInfo[] Props = typeof(PatientBillReport).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo prop in Props)
            {
                //Defining type of data column gives proper data table 
                var type = (prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>) ? Nullable.GetUnderlyingType(prop.PropertyType) : prop.PropertyType);
                //Setting column names as Property names
                dataTable.Columns.Add(prop.Name, type);
            }
            foreach (PatientBillReport item in data.OrderBy(x => x.PatientNumber).ToList())
            {
                var values = new object[Props.Length];
                for (int i = 0; i < Props.Length; i++)
                {
                    //inserting property values to datatable rows
                    values[i] = Props[i].GetValue(item, null);
                }
                dataTable.Rows.Add(values);
            }

            dataTable.Columns.Remove("patientId");
            dataTable.Columns.Remove("clinic");
            dataTable.Columns.Remove("Clinicname");
            try
            {
                BlobContainerClient containerClient = new BlobContainerClient(Blob_Conn_String, ContainerName);
                string Filename = "PatientBillingReport" +DateTime.Now.Day+DateTime.Now.Month+DateTime.Now.Year+DateTime.Now.Hour+DateTime.Now.Minute+DateTime.Now.Second + "." + format;
                var filePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\" + "rpmfolder" + Path.GetExtension(Filename);
                if (!Directory.Exists(filePath))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                }

                if (format == "xlsx")
                {
                    WriteExcelFile(filePath, dataTable);
                }
                else
                {
                    ExportDataTableToPdf(dataTable, Patientid, clinic, cptCoode, isMonth, startDate, endDate, filePath);
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



        private static WorkbookStylesPart AddStyleSheet(SpreadsheetDocument spreadsheet)
        {
            WorkbookStylesPart stylesheet = spreadsheet.WorkbookPart.AddNewPart<WorkbookStylesPart>();

            Stylesheet workbookstylesheet = new Stylesheet();
            DocumentFormat.OpenXml.Spreadsheet.Font font0 = new DocumentFormat.OpenXml.Spreadsheet.Font();
            //Font font0 = new Font();         // Default font

            DocumentFormat.OpenXml.Spreadsheet.Font font1 = new DocumentFormat.OpenXml.Spreadsheet.Font();         // Bold font
            Bold bold = new Bold();
            font1.Append(bold);

            Fonts fonts = new Fonts();      // <APENDING Fonts>
            fonts.Append(font0);
            fonts.Append(font1);

            // <Fills>
            //Fill fill0 = new Fill();        // Default fill
            //Fill fill1 = new Fill(new DocumentFormat.OpenXml.Spreadsheet.Color() { Rgb = "8be3d9" });

            //Fills fills = new Fills();   
            //// <APENDING Fills>
            //fills.Append(fill0);
            //fills.Append(fill1);

            Fills fills = new Fills(
                new Fill(new PatternFill() { PatternType = PatternValues.None }), // Index 0 - default
                new Fill(new PatternFill() { PatternType = PatternValues.Gray125 }), // Index 1 - default
                new Fill(new PatternFill(new ForegroundColor { Rgb = "8be3d9" })
                { PatternType = PatternValues.Solid }) // Index 2 - header
            );

            // <Borders>
            Border border0 = new Border();
            Border border1 = new Border( // index 1 black border
                    new LeftBorder(new DocumentFormat.OpenXml.Spreadsheet.Color() { Rgb = "8be3d9" }) { Style = BorderStyleValues.Thick },
                    new RightBorder(new DocumentFormat.OpenXml.Spreadsheet.Color() { Rgb = "8be3d9" }) { Style = BorderStyleValues.Thick },
                    new TopBorder(new DocumentFormat.OpenXml.Spreadsheet.Color() { Rgb = "8be3d9" }) { Style = BorderStyleValues.Thick },
                    new BottomBorder(new DocumentFormat.OpenXml.Spreadsheet.Color() { Rgb = "8be3d9" }) { Style = BorderStyleValues.Thick },
                    new DiagonalBorder());// Defualt border

            Borders borders = new Borders();
            // BorderStyle borderStyle = new BorderStyle();


            // <APENDING Borders>


            // <APENDING Borders>
            borders.Append(border0);
            borders.Append(border1);

            // <CellFormats>
            CellFormat cellformat0 = new CellFormat() { FontId = 0, FillId = 0, BorderId = 0 }; // Default style : Mandatory | Style ID =0

            CellFormat cellformat1 = new CellFormat() { FontId=1, BorderId=1, FillId=2 };  // Style with Bold text ; Style ID = 1

            //CellFormat cellformat2 = new CellFormat() { BorderId = 1 };  // Style with Bold text ; Style ID = 2


            // <APENDING CellFormats>
            CellFormats cellformats = new CellFormats();
            cellformats.Append(cellformat0);
            cellformats.Append(cellformat1);
            // cellformats.Append(cellformat2);


            // Append FONTS, FILLS , BORDERS & CellFormats to stylesheet <Preserve the ORDER>
            workbookstylesheet.Append(fonts);
            workbookstylesheet.Append(fills);
            workbookstylesheet.Append(borders);
            workbookstylesheet.Append(cellformats);

            // Finalize
            stylesheet.Stylesheet = workbookstylesheet;
            stylesheet.Stylesheet.Save();

            return stylesheet;
        }

        private static void WriteExcelFile(string outputPath, DataTable table)
        {
            using (SpreadsheetDocument document = SpreadsheetDocument.Create(outputPath, SpreadsheetDocumentType.Workbook))
            {
                WorkbookPart workbookPart = document.AddWorkbookPart();
                workbookPart.Workbook = new Workbook();
                WorksheetPart worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
                var sheetData = new SheetData();
                worksheetPart.Worksheet = new Worksheet(sheetData);



                Sheets sheets = workbookPart.Workbook.AppendChild(new Sheets());
                Sheet sheet = new Sheet() { Id = workbookPart.GetIdOfPart(worksheetPart), SheetId = 1, Name = "Sheet1" };
                AddStyleSheet(document);
                sheets.Append(sheet);



                Row headerRow = new Row();



                List<String> columns = new List<string>();
                foreach (System.Data.DataColumn column in table.Columns)
                {
                    columns.Add(column.ColumnName);



                    Cell cell = new Cell() { StyleIndex = Convert.ToUInt32(1) };
                    cell.DataType = CellValues.String;
                    cell.CellValue = new CellValue(column.ColumnName);

                    headerRow.AppendChild(cell);
                }



                sheetData.AppendChild(headerRow);



                foreach (DataRow dsrow in table.Rows)
                {
                    Row newRow = new Row();
                    foreach (String col in columns)
                    {
                        Cell cell = new Cell();
                        cell.DataType = CellValues.String;
                        cell.CellValue = new CellValue(dsrow[col].ToString());
                        newRow.AppendChild(cell);
                    }



                    sheetData.AppendChild(newRow);
                }



                workbookPart.Workbook.Save();
            }
        }

        void ExportDataTableToPdf(DataTable dtblTable, int? Patientid, string clinic, string cptCoode, int isMonth, DateTime startDate, DateTime endDate, String strPdfPath)
        {
            try
            {
                PdfPTable table = new PdfPTable(dtblTable.Columns.Count);
                System.IO.FileStream fs = new FileStream(strPdfPath, FileMode.Create, FileAccess.Write, FileShare.None);
                Document document = new Document();
                var pgSize = new iTextSharp.text.Rectangle(1400, 1000);
                document.SetPageSize(pgSize);
                PdfWriter writer = PdfWriter.GetInstance(document, fs);
                document.Open();

                //Report Header
                string strHeader = "PATIENTS BILL REPORT";
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
                prgAuthor.Add(new Chunk("\nReport Date      : " + DateTime.Now.ToShortDateString(), fntAuthor));
                if (!string.IsNullOrEmpty(Patientid.ToString()))
                {
                    string patientNumber = dtblTable.Rows[0][1].ToString();
                    prgAuthor.Add(new Chunk("\nPatient Id    : " + patientNumber, fntAuthor));
                }
                if (!string.IsNullOrEmpty(clinic) && !string.IsNullOrEmpty(cptCoode))
                {
                    prgAuthor.Add(new Chunk("\nCPT Code   : " + cptCoode, fntAuthor));
                    prgAuthor.Add(new Chunk("\nClinic        : " + clinic, fntAuthor));
                }
                else if (!string.IsNullOrEmpty(clinic) && string.IsNullOrEmpty(cptCoode))
                {
                    prgAuthor.Add(new Chunk("\nClinic       : " + clinic, fntAuthor));
                }
                else if (!string.IsNullOrEmpty(cptCoode) && string.IsNullOrEmpty(clinic))
                {
                    prgAuthor.Add(new Chunk("\nCPT Code   : " + cptCoode, fntAuthor));
                }
                if (isMonth != 1)
                {
                    prgAuthor.Add(new Chunk("\nStart Date    : " + startDate.ToShortDateString(), fntAuthor));
                    prgAuthor.Add(new Chunk("\nEnd Date      : " + endDate.ToShortDateString(), fntAuthor));
                }
                else
                {
                    prgAuthor.Add(new Chunk("\nMonth       : " + startDate.ToString("MMMM") + " " + startDate.Year, fntAuthor));
                }

                document.Add(prgAuthor);

                //Add a line seperation
                Paragraph p = new Paragraph(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(0.0F, 100.0F, BaseColor.BLACK, Element.ALIGN_LEFT, 1)));
                document.Add(p);

                //Add line break
                document.Add(new Chunk("\n", fntHead));

                //Write the table

                //Table header
                BaseFont btnColumnHeader = BaseFont.CreateFont(BaseFont.TIMES_ROMAN, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
                Font fntColumnHeader = new Font(btnColumnHeader, 10, 1, BaseColor.WHITE);

                for (int i = 0; i < dtblTable.Columns.Count; i++)
                {
                    var cells = new PdfPCell() { HorizontalAlignment = Element.ALIGN_CENTER };
                    cells.BackgroundColor = BaseColor.GRAY;
                    string coulmn = dtblTable.Columns[i].ToString();

                    cells.Phrase = new Phrase(coulmn);
                    //  cells.AddElement(new Chunk(dtblTable.Columns[i].ColumnName.ToUpper(), fntColumnHeader));
                    table.AddCell(cells);
                }
                //table Data
                var cell = new PdfPCell() { HorizontalAlignment = Element.ALIGN_CENTER };
                for (int i = 0; i < dtblTable.Rows.Count; i++)
                {
                    for (int j = 0; j < dtblTable.Columns.Count; j++)
                    {
                        cell.BackgroundColor = i % 2 == 0
                    ? BaseColor.LIGHT_GRAY : BaseColor.WHITE;
                        string row = dtblTable.Rows[i][j].ToString();

                        cell.Phrase = new Phrase(row);
                        table.AddCell(cell);
                    }
                }
                //for (int i = 0; i < 200; i++)
                //{
                //    for (int j = 0; j < dtblTable.Columns.Count; j++)
                //    {
                //        cell.BackgroundColor = i % 2 == 0
                //    ? BaseColor.LIGHT_GRAY : BaseColor.WHITE;
                //        string row = "sample text".ToString();

                //        cell.Phrase = new Phrase(row);
                //        table.AddCell(cell);
                //    }
                //}

                document.Add(table);
                document.Close();
                writer.Close();
                fs.Close();
            }
            catch
            {
                throw;
            }
        }
        void GenerateInvoice(String strPdfPath)
        {
            try
            {

                System.IO.FileStream fs = new FileStream(strPdfPath, FileMode.Create, FileAccess.Write, FileShare.None);
                Document document = new Document();
                var pgSize = new iTextSharp.text.Rectangle(1400, 1000);
                document.SetPageSize(pgSize);
                PdfWriter writer = PdfWriter.GetInstance(document, fs);
                document.Open();

                //Report Header
                string strHeader = "INVOICE";
                BaseFont bfntHead = BaseFont.CreateFont(BaseFont.HELVETICA_BOLD, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
                Font fntHead = new Font(bfntHead, 16, 1, BaseColor.DARK_GRAY);
                Paragraph prgHeading = new Paragraph();
                prgHeading.Alignment = Element.ALIGN_RIGHT;
                prgHeading.Add(new Chunk(strHeader.ToUpper(), fntHead));
                document.Add(prgHeading);
                string logopath = "C:\\Users\\AshwinKannoth\\Desktop\\tesp.jpg";
                iTextSharp.text.Image img = iTextSharp.text.Image.GetInstance(logopath);
                img.SetAbsolutePosition(1500f, 1500f);
                img.ScaleAbsolute(1600f, 15f);
                img.ScalePercent(0.5f * 100);
                document.Add(img);

                document.Close();
                writer.Close();
                fs.Close();
            }
            catch
            {
                throw;
            }
        }
        public List<BillingInfo> GetPatientBillingInfos(DateTime startDate, DateTime endDate, string CreatedBy, string ConnectionString)
        {
            List<BillingInfo> list = new List<BillingInfo>();
            try
            {
                using (SqlConnection con = new SqlConnection(ConnectionString))
                {
                    con.Open();

                    using (SqlCommand command = new SqlCommand("usp_GetBillingInfo", con))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.Add("@startDate", SqlDbType.Date).Value = startDate;
                        command.Parameters.Add("@endDate", SqlDbType.Date).Value = endDate;
                        SqlDataReader reader = command.ExecuteReader();
                        while (reader.Read())
                        {
                            BillingInfo info = new BillingInfo();

                            info.BillingCode = reader["BillingCode"].ToString();
                            info.Total = Convert.ToInt32(reader["total"]);
                            info.TargetMet = Convert.ToInt32(reader["TargetMetCount"]);
                            info.ReadyToBill = Convert.ToInt32(reader["ReadyToBillCount"]);
                            info.MissingInfo = 0;//business logic not yet decided
                            info.OnHold = 0;//business logic not yet decided

                            list.Add(info);
                        }
                    }
                }
                return list;
            }
            catch (Exception)
            {

                throw;
            }

        }
        public List<PatientBilldata> GetPatientBillingData(int patientid, string CreatedBy, string connectionString)
        {
            try
            {
                DateTime Enddate = DateTime.Today;
                var stOfMonth = new DateTime(Enddate.Year, Enddate.Month, 1);
                DateTime endMonth = stOfMonth.AddMonths(1).AddDays(-1);
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();

                    using (SqlCommand command = new SqlCommand("usp_GetPatientBillData", con))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.Add("@patientId", SqlDbType.Int).Value = patientid;
                        command.Parameters.Add("@startDate", SqlDbType.Date).Value = stOfMonth;
                        command.Parameters.Add("@endDate", SqlDbType.Date).Value = endMonth;
                        SqlParameter returnParameter = command.Parameters.Add("RetVal", SqlDbType.Int);
                        returnParameter.Direction = ParameterDirection.ReturnValue;
                        SqlDataReader reader = command.ExecuteReader();

                        List<PatientBilldata> PatientBilldateList = new List<PatientBilldata>();
                        List<int> roleids = new List<int>();
                        while (reader.Read())
                        {
                            PatientBilldata PatientBilldata = new PatientBilldata();
                            PatientBilldata.CPTCode = reader["BillingCode"].ToString();
                            PatientBilldata.Completed = Convert.ToInt32(reader["TotalReadings"].ToString());
                            PatientBilldata.Total = Convert.ToInt32(reader["TargetReadings"].ToString());
                            PatientBilldata.ReadyTobill = Convert.ToInt32(reader["ReadyToBill"]);
                            PatientBilldateList.Add(PatientBilldata);
                        }

                        if (reader.FieldCount == 0)
                        {
                            con.Close();
                            return null;
                        }
                        con.Close();
                        return PatientBilldateList;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception occured - class Name :" + this.GetType().Name + ",  Method Name : " + MethodBase.GetCurrentMethod().Name + ", Error Message :" + ex.Message + "");
                return null;
            }
        }
        public VitalSummaryMeasures GetRecentPatientVitalSummary(string Username, int dayCount, string ConnectionString)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(ConnectionString))
                {
                    con.Open();

                    using (SqlCommand command = new SqlCommand("usp_GetpatientHealthTrendsbyPatient", con))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@CreatedBy", Username);
                        command.Parameters.AddWithValue("@StartDate", DateTime.Today.AddDays(-dayCount));
                        command.Parameters.AddWithValue("@EndDate", DateTime.Today);
                        SqlParameter returnParameter = command.Parameters.Add("RetVal", SqlDbType.Int);
                        returnParameter.Direction = ParameterDirection.ReturnValue;
                        SqlDataReader reader = command.ExecuteReader();
                        VitalSummaryMeasures ret1 = new VitalSummaryMeasures();
                        ret1.PatientSummary = new List<PatientLatestSummary>();
                        PatientLatestSummary ret = new PatientLatestSummary();
                        ret.Values = new List<Values>();
                        ret.Time = new List<DateTime>();
                        // ret1.LatestVitalMeasure = new List<VitalMeasure>();
                        DataTable dt = new DataTable();
                        while (!reader.IsClosed)
                        {
                            dt.Load(reader);
                        }
                        DateTime startDateNew = DateTime.Today.AddDays(-dayCount);
                        DateTime enddateNew = DateTime.Today;
                        if (dt.Rows.Count > 0)
                        {
                            DateTime[] datesnew = new DateTime[7];
                            DateTime[] dates = dt.AsEnumerable().Select(s => s.Field<DateTime>("CreatedOn")).Distinct().ToArray<DateTime>();
                            string[] measureValues = dt.AsEnumerable().Select(s => s.Field<string>("MeasureName")).Distinct().ToArray<string>();
                            string[] vitals = dt.AsEnumerable().Select(s => s.Field<string>("VitalName")).Distinct().ToArray<string>();
                            foreach (string vital in vitals)
                            {
                                VitalMeasure vitaldata = new VitalMeasure();
                                vitaldata = GetLatestReading(Username, dayCount, ConnectionString, vital);

                                foreach (string measureName in measureValues)
                                {
                                    Values vv = new Values();
                                    vv.data = new List<string>();
                                    vv.label = measureName.ToString();
                                    int i = 0;

                                    for (DateTime sDateNew = startDateNew.Date; sDateNew < enddateNew.Date; sDateNew = sDateNew.AddDays(1))
                                    {



                                        var result = dt.AsEnumerable().Where(myRow => myRow.Field<string>("MeasureName") == measureName.ToString() && myRow.Field<DateTime>("CreatedOn").Date == sDateNew.Date)?.FirstOrDefault();
                                        if (result == null)
                                        {
                                            datesnew[i] = sDateNew;
                                            vv.data.Add("null");
                                        }
                                        else
                                        {
                                            datesnew[i] = Convert.ToDateTime(result["CreatedOn"]);
                                            vv.data.Add(result["MeasureValue"].ToString());
                                        }
                                        //
                                        //List<DateTime> tempTime = xDatesInDb.FindAll(x => x.Date == sDate.Date);
                                        //if (tempTime.Count > 0)
                                        //{
                                        //    xAxisInfo.AddRange(tempTime);
                                        //}
                                        //else
                                        //{
                                        //    xAxisInfo.Add(sDate);
                                        //}
                                        i++;
                                    }
                                    //foreach (DateTime date in dates)
                                    //{
                                    //    var result = dt.AsEnumerable().Where(myRow => myRow.Field<string>("MeasureName") == measureName.ToString() && myRow.Field<DateTime>("CreatedOn") == date)?.FirstOrDefault();
                                    //    if (result == null)
                                    //    {
                                    //        vv.data.Add("Nil");
                                    //    }
                                    //    else
                                    //    {
                                    //        vv.data.Add(result["MeasureValue"].ToString());
                                    //    }
                                    //}
                                    ret.Values.Add(vv);
                                }
                                ret.LatestVitalMeasure = vitaldata;
                            }
                            ret.Time.AddRange(datesnew);
                        }
                        else
                        {
                            DateTime[] datesnew = new DateTime[8];
                            Values vv = new Values();
                            vv.data = new List<string>();
                            vv.label = "No Vital Data";
                            int i = 0;
                            for (DateTime sDateNew = startDateNew.Date; sDateNew <= enddateNew.Date; sDateNew = sDateNew.AddDays(1))
                            {
                                datesnew[i] = sDateNew;
                                vv.data.Add("null");
                                i++;
                            }
                            ret.Values.Add(vv);
                            ret.Time.AddRange(datesnew);
                            ret.LatestVitalMeasure = null;

                        }
                        ret1.PatientSummary.Add(ret);
                        return ret1;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public VitalMeasure GetLatestReading(string Username, int dayCount, string ConnectionString, string vitalname)
        {
            try
            {

                using (SqlConnection con = new SqlConnection(ConnectionString))
                {
                    con.Open();

                    using (SqlCommand command = new SqlCommand("usp_GetLatestVitalReading", con))
                    {
                        VitalMeasure ret = new VitalMeasure();
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@username", Username);
                        command.Parameters.AddWithValue("@StartDate", DateTime.Today.AddDays(-130));
                        command.Parameters.AddWithValue("@EndDate", DateTime.Today);
                        if (vitalname == null)
                        {
                            command.Parameters.AddWithValue("@vitalName", DBNull.Value);
                        }
                        else
                        {
                            command.Parameters.AddWithValue("@vitalName", vitalname);
                        }
                        SqlParameter returnParameter = command.Parameters.Add("RetVal", SqlDbType.Int);
                        returnParameter.Direction = ParameterDirection.ReturnValue;
                        SqlDataReader reader = command.ExecuteReader();


                        DataTable dt = new DataTable();
                        while (!reader.IsClosed)
                        {

                            dt.Load(reader);

                        }
                        if (dt.Rows.Count == 0)
                        {
                            return null;
                        }

                        var result = dt.AsEnumerable().OrderByDescending(y => y.Field<DateTime>("CreatedOn")).FirstOrDefault();

                        if (result != null)
                        {
                            var pulse = dt.AsEnumerable().Where(p => p.Field<string>("MeasureName") == "Pulse").OrderByDescending(y => y.Field<DateTime>("CreatedOn")).FirstOrDefault();
                            if (vitalname== null)
                            {
                                vitalname = result["Name"].ToString();
                            }
                            if (vitalname == "Blood Pressure")
                            {
                                string[] MeasureNames = dt.AsEnumerable().Where(p => p.Field<string>("MeasureName") != "Pulse").Select(s => s.Field<string>("MeasureName")).Distinct().ToArray<string>();
                                List<string> Values = new List<string>();
                                DataRow latestReading1;
                                foreach (var measure in MeasureNames)//systoli,diastolic,pulse
                                {

                                    latestReading1 = dt.AsEnumerable().Where(s => s.Field<string>("MeasureName") == measure).OrderByDescending(m => m.Field<DateTime>("CreatedOn")).FirstOrDefault();
                                    Values.Add(latestReading1["MeasureValue"].ToString());
                                    ret.Date = (DateTime)latestReading1["CreatedOn"];
                                    ret.unit = latestReading1["UnitName"].ToString();
                                    ret.time = latestReading1["time"].ToString();
                                    ret.VitalName = latestReading1["Name"].ToString();
                                    string datePart = ret.Date.ToString("yyyy-MM-dd");
                                    string newDate = datePart + " " + ret.time;
                                    DateTime date = DateTime.ParseExact(datePart, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                                    DateTime combinedDateTime = date.Add(TimeSpan.Parse(ret.time));
                                    ret.Date = combinedDateTime;


                                }
                                string[] myString = Values.ToArray();
                                ret.Value = string.Join(" / ", myString);

                            }
                            else
                            {
                                ret.Value = result["MeasureValue"].ToString();
                                ret.Date = (DateTime)result["CreatedOn"];
                                ret.unit = result["UnitName"].ToString();
                                ret.time = result["time"].ToString();
                                ret.VitalName = result["Name"].ToString();
                                string datePart = ret.Date.ToString("yyyy-MM-dd");
                                string newDate = datePart + " " + ret.time;
                                DateTime date = DateTime.ParseExact(datePart, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                                DateTime combinedDateTime = date.Add(TimeSpan.Parse(ret.time));
                                ret.Date = combinedDateTime;

                            }
                            if (pulse != null)
                            {
                                ret.PulseValue = pulse["MeasureValue"].ToString();
                                ret.PulseUnit = pulse["UnitName"].ToString();

                            }


                        }
                        return ret;

                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public List<BillingInfo> GetPatientBillingInfos(int isPast, int isFuture, int isToday, int isCurrentMonth, int isLastMonth, string CreatedBy, string ConnectionString)
        {
            List<BillingInfo> list = new List<BillingInfo>();
            DataTable table = new DataTable();

            DataTable tblFiltered = new DataTable();
            table = GetPatientBillingDetails(ConnectionString);
            bool dayCode = false;
            bool monthCode = false;
            if (isToday == 1)
            {
                dayCode = true;
                tblFiltered = table.AsEnumerable()
                         .Where(r => r.Field<bool>("isToday") == Convert.ToBoolean(isToday))
                         .CopyToDataTable();
            }
            else if (isFuture == 1)
            {
                dayCode = true;
                tblFiltered = table.AsEnumerable()
                         .Where(r => r.Field<bool>("isFuture") == Convert.ToBoolean(isFuture))
                         .CopyToDataTable();
            }
            else if (isPast == 1)
            {
                dayCode = true;
                tblFiltered = table.AsEnumerable()
                         .Where(r => r.Field<bool>("isPast") == Convert.ToBoolean(isPast))
                         .CopyToDataTable();
            }
            else if (isCurrentMonth == 1)
            {
                monthCode = true;
                tblFiltered = table.AsEnumerable()
                         .Where(r => r.Field<bool>("isCurrentMonth") == Convert.ToBoolean(isCurrentMonth))
                         .CopyToDataTable();
            }
            else if (isLastMonth == 1)
            {
                monthCode = true;
                tblFiltered = table.AsEnumerable()
                         .Where(r => r.Field<bool>("isLastMonth") == Convert.ToBoolean(isLastMonth))
                         .CopyToDataTable();
            }
            foreach (var val in Enum.GetValues(typeof(CPTCodes)))
            {
                DataTable tblFilteredCode = new DataTable();
                if (val.ToString() == "CPT99453" && dayCode)
                {
                    tblFilteredCode = tblFiltered.AsEnumerable()
                         .Where(r => r.Field<int>("BillingCodeId") == 1)
                         .CopyToDataTable();

                    BillingInfo billingInfo = new BillingInfo();
                    billingInfo.BillingCode = "99453";
                    billingInfo.Total = tblFilteredCode.Rows.Count;
                    billingInfo.ReadyToBill = tblFilteredCode.AsEnumerable().Where(x => Convert.ToInt16(x["ReadyToBill"]) == 1).ToList().Count;
                    billingInfo.TargetMet = tblFilteredCode.AsEnumerable().Where(x => Convert.ToInt16(x["TargetMet"]) == 1).ToList().Count;
                    billingInfo.OnHold = tblFilteredCode.AsEnumerable().Where(x => Convert.ToInt16(x["OnHold"]) == 1).ToList().Count;
                    billingInfo.MissingInfo = tblFilteredCode.AsEnumerable().Where(x => Convert.ToInt16(x["MissingInfo"]) == 1).ToList().Count;
                    list.Add(billingInfo);
                }
                else if (val.ToString() == "CPT99454" && dayCode)
                {
                    tblFilteredCode = tblFiltered.AsEnumerable()
                             .Where(r => r.Field<int>("BillingCodeId") == 2)
                             .CopyToDataTable();

                    BillingInfo billingInfo = new BillingInfo();
                    billingInfo.BillingCode = "99454";
                    billingInfo.Total = tblFilteredCode.Rows.Count;
                    billingInfo.ReadyToBill = tblFilteredCode.AsEnumerable().Where(x => Convert.ToInt16(x["ReadyToBill"]) == 1).ToList().Count;
                    billingInfo.TargetMet = tblFilteredCode.AsEnumerable().Where(x => Convert.ToInt16(x["TargetMet"]) == 1).ToList().Count;
                    billingInfo.OnHold = tblFilteredCode.AsEnumerable().Where(x => Convert.ToInt16(x["OnHold"]) == 1).ToList().Count;
                    billingInfo.MissingInfo = tblFilteredCode.AsEnumerable().Where(x => Convert.ToInt16(x["MissingInfo"]) == 1).ToList().Count;
                    list.Add(billingInfo);
                }
                else if (val.ToString() == "CPT99457" && monthCode)
                {
                    tblFilteredCode = tblFiltered.AsEnumerable()
                             .Where(r => r.Field<int>("BillingCodeId") == 3)
                             .CopyToDataTable();

                    BillingInfo billingInfo = new BillingInfo();
                    billingInfo.BillingCode = "99457";
                    billingInfo.Total = tblFilteredCode.Rows.Count;
                    billingInfo.ReadyToBill = tblFilteredCode.AsEnumerable().Where(x => Convert.ToInt16(x["ReadyToBill"]) == 1).ToList().Count;
                    billingInfo.TargetMet = tblFilteredCode.AsEnumerable().Where(x => Convert.ToInt16(x["TargetMet"]) == 1).ToList().Count;
                    billingInfo.OnHold = tblFilteredCode.AsEnumerable().Where(x => Convert.ToInt16(x["OnHold"]) == 1).ToList().Count;
                    billingInfo.MissingInfo = tblFilteredCode.AsEnumerable().Where(x => Convert.ToInt16(x["MissingInfo"]) == 1).ToList().Count;
                    list.Add(billingInfo);
                }
                else if (val.ToString() == "CPT99458" && monthCode)
                {
                    tblFilteredCode = tblFiltered.AsEnumerable()
                             .Where(r => r.Field<int>("BillingCodeId") == 4)
                             .CopyToDataTable();

                    BillingInfo billingInfo = new BillingInfo();
                    billingInfo.BillingCode = "99458";
                    billingInfo.Total = tblFilteredCode.Rows.Count;
                    billingInfo.ReadyToBill = tblFilteredCode.AsEnumerable().Where(x => Convert.ToInt16(x["ReadyToBill"]) == 1).ToList().Count;
                    billingInfo.TargetMet = tblFilteredCode.AsEnumerable().Where(x => Convert.ToInt16(x["TargetMet"]) == 1).ToList().Count;
                    billingInfo.OnHold = tblFilteredCode.AsEnumerable().Where(x => Convert.ToInt16(x["OnHold"]) == 1).ToList().Count;
                    billingInfo.MissingInfo = tblFilteredCode.AsEnumerable().Where(x => Convert.ToInt16(x["MissingInfo"]) == 1).ToList().Count;
                    list.Add(billingInfo);
                }


            }


            return list;

        }
        public DataTable GetPatientBillingDetails(string ConnectionString)
        {
            List<BillingInfo> list = new List<BillingInfo>();
            try
            {
                DataTable table = new DataTable();
                using (SqlConnection con = new SqlConnection(ConnectionString))
                {
                    con.Open();

                    using (SqlCommand command = new SqlCommand("usp_GetpatientReportDetails", con))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        SqlDataAdapter da = new SqlDataAdapter(command);
                        // this will query your database and return the result to your datatable
                        da.Fill(table);
                        con.Close();
                        da.Dispose();
                    }
                }
                return table;
            }
            catch (Exception)
            {

                throw;

            }
        }
        public List<PatientBilldata> GetBillingDataByPatientId(int patientid, int patientProgramid, string CreatedBy, string connectionString)
        {
            try
            {

                List<BillingCodesDetails> billingCodes = GetBillingCodeDetails(connectionString);
                List<PatientBilldata> PatientBilldateList = new List<PatientBilldata>();
                PatientProgramData patientProgramData = new PatientProgramData();
                patientProgramData = patientprogramInfo(patientid, patientProgramid, connectionString);
                List<SystemConfigInfo> lstConfig = DalCommon.GetSystemConfig(connectionString, "Billing", "User");
                SystemConfigInfo provider = lstConfig.Find(x => x.Name.Equals("BillingType"));
                if (patientProgramData == null)
                {
                    return null;
                }
                string programName = GetProgramName(connectionString, patientProgramid);
                if (programName.ToUpper() == "RPM")
                {
                    foreach (var code in billingCodes)
                    {
                        switch (code.BillingCode)
                        {
                            case "99453":
                                PatientBilldata PatientBilldata = new PatientBilldata();
                                if (patientProgramData.status == "Active" ||
                                   patientProgramData.status == "ReadyToDischarge" ||
                                   patientProgramData.status == "OnHold"||
                                   patientProgramData.status == "InActive")
                                {
                                    if (lstConfig.Count > 0)
                                    {
                                        if (provider.Value == "30days")
                                        {
                                            PatientBilldata = ProcessCPT99453_DaysBasedBilling(patientid, patientProgramid, code.TargetReadings, code, connectionString);

                                        }
                                        else if (provider.Value == "cycle")
                                        {
                                            PatientBilldata = ProcessCPT99453_CycleBasesBilling(patientid, patientProgramid, code.TargetReadings, code, connectionString);
                                        }
                                        else
                                        {
                                            PatientBilldata = ProcessCPT99453_CycleBasesBilling(patientid, patientProgramid, code.TargetReadings, code, connectionString);
                                        }
                                    }
                                    else
                                    {
                                        PatientBilldata = ProcessCPT99453_CycleBasesBilling(patientid, patientProgramid, code.TargetReadings, code, connectionString);
                                    }

                                }
                                else
                                {
                                    PatientBilldata.IsTargetMet = false;
                                    PatientBilldata.ReadyTobill = 0;
                                    PatientBilldata.Total = code.TargetReadings;
                                    PatientBilldata.Completed = 0;
                                    PatientBilldata.CPTCode = code.BillingCode;
                                }
                                PatientBilldata.ProgramName="RPM";
                                PatientBilldateList.Add(PatientBilldata);
                                break;
                            case "99454":
                                PatientBilldata PatientBilldata54 = new PatientBilldata();
                                if (patientProgramData.status == "Active" ||
                                   patientProgramData.status == "ReadyToDischarge" ||
                                   patientProgramData.status == "OnHold"||
                                   patientProgramData.status == "InActive")
                                {
                                    if (lstConfig.Count > 0)
                                    {
                                        if (provider.Value == "30days")
                                        {
                                            PatientBilldata54 = ProcessCPT99454_DaysBasedBilling(patientid, patientProgramid, code, connectionString);
                                        }
                                        else if (provider.Value == "cycle")
                                        {
                                            PatientBilldata54 = ProcessCPT99454_CycleBasedBilling(patientid, patientProgramid, code, connectionString);
                                        }
                                        else
                                        {
                                            PatientBilldata54 = ProcessCPT99454_CycleBasedBilling(patientid, patientProgramid, code, connectionString);
                                        }
                                    }
                                    else
                                    {
                                        PatientBilldata54 = ProcessCPT99454_CycleBasedBilling(patientid, patientProgramid, code, connectionString);
                                    }


                                }
                                else
                                {
                                    PatientBilldata54.IsTargetMet = false;
                                    PatientBilldata54.ReadyTobill = 0;
                                    PatientBilldata54.Total = code.TargetReadings;
                                    PatientBilldata54.Completed = 0;
                                    PatientBilldata54.CPTCode = code.BillingCode;
                                }
                                PatientBilldata54.ProgramName="RPM";
                                PatientBilldateList.Add(PatientBilldata54);
                                break;
                            case "99457":
                                PatientBilldata PatientBilldata57 = new PatientBilldata();
                                if (patientProgramData.status == "Active" ||
                                   patientProgramData.status == "ReadyToDischarge" ||
                                   patientProgramData.status == "OnHold"||
                                   patientProgramData.status == "InActive")
                                {
                                    if (lstConfig.Count > 0)
                                    {
                                        if (provider.Value == "30days")
                                        {
                                            PatientBilldata57 = ProcessCPT99457_DaysBasedBilling(patientid, patientProgramid, code, connectionString);
                                        }
                                        else if (provider.Value == "cycle")
                                        {
                                            PatientBilldata57 = ProcessCPT99457_CycleBasedBilling(patientid, patientProgramid, code, connectionString);
                                        }
                                        else
                                        {
                                            PatientBilldata57 = ProcessCPT99457_CycleBasedBilling(patientid, patientProgramid, code, connectionString);
                                        }
                                    }
                                    else
                                    {
                                        PatientBilldata57 = ProcessCPT99457_CycleBasedBilling(patientid, patientProgramid, code, connectionString);
                                    }
                                }
                                else
                                {
                                    PatientBilldata57.IsTargetMet = false;
                                    PatientBilldata57.Total = code.TargetReadings*60;
                                    PatientBilldata57.ReadyTobill = 0;
                                    PatientBilldata57.Completed = 0;
                                    PatientBilldata57.CPTCode = code.BillingCode;
                                }
                                PatientBilldata57.ProgramName="RPM";
                                PatientBilldateList.Add(PatientBilldata57);

                                break;
                            case "99458":
                                PatientBilldata PatientBilldata58 = new PatientBilldata();
                                if (patientProgramData.status == "Active" ||
                                   patientProgramData.status == "ReadyToDischarge" ||
                                   patientProgramData.status == "OnHold"||
                                   patientProgramData.status == "InActive")
                                {
                                    if (lstConfig.Count > 0)
                                    {
                                        if (provider.Value == "30days")
                                        {
                                            BillingCodesDetails billingCodesDetails457 = billingCodes.Find(x => x.BillingCode == "99457");
                                            PatientBilldata58 = ProcessCPT99458_DaysBasedBilling(patientid, patientProgramid, code, billingCodesDetails457, connectionString);

                                        }
                                        else if (provider.Value == "cycle")
                                        {
                                            BillingCodesDetails billingCodesDetails457 = billingCodes.Find(x => x.BillingCode == "99457");
                                            PatientBilldata58 = ProcessCPT99458_CycleBasedBilling(patientid, patientProgramid, code, billingCodesDetails457, connectionString);
                                        }
                                        else
                                        {
                                            BillingCodesDetails billingCodesDetails457 = billingCodes.Find(x => x.BillingCode == "99457");
                                            PatientBilldata58 = ProcessCPT99458_CycleBasedBilling(patientid, patientProgramid, code, billingCodesDetails457, connectionString);
                                        }
                                    }
                                    else
                                    {
                                        BillingCodesDetails billingCodesDetails457 = billingCodes.Find(x => x.BillingCode == "99457");
                                        PatientBilldata58 = ProcessCPT99458_CycleBasedBilling(patientid, patientProgramid, code, billingCodesDetails457, connectionString);
                                    }

                                }
                                else
                                {
                                    PatientBilldata58.IsTargetMet = false;
                                    PatientBilldata58.Total = code.TargetReadings*60;
                                    PatientBilldata58.ReadyTobill = 0;
                                    PatientBilldata58.Completed = 0;
                                    PatientBilldata58.CPTCode = code.BillingCode;
                                }
                                PatientBilldata58.ProgramName="RPM";
                                PatientBilldateList.Add(PatientBilldata58);

                                break;
                        }
                    }

                }
                else if (programName.ToUpper() == "CCM-C")
                {
                    foreach (var code in billingCodes)
                    {
                        switch (code.BillingCode)
                        {

                            case "99490":
                                PatientBilldata PatientBilldata490 = new PatientBilldata();
                                if (patientProgramData.status == "Active" ||
                                   patientProgramData.status == "ReadyToDischarge" ||
                                   patientProgramData.status == "OnHold"||
                                   patientProgramData.status == "InActive")
                                {
                                    if (lstConfig.Count > 0)
                                    {
                                        if (provider.Value == "cycle")
                                        {
                                            PatientBilldata490 = ProcessCPT99490_CycleBasedBilling(patientid, patientProgramid, code, connectionString);
                                        }

                                        else if (provider.Value == "30days")
                                        {
                                            PatientBilldata490 = ProcessCPT99490_DaysBasedBilling(patientid, patientProgramid, code, connectionString);
                                        }
                                        else
                                        {
                                            PatientBilldata490 = ProcessCPT99490_CycleBasedBilling(patientid, patientProgramid, code, connectionString);
                                        }

                                        //if (provider.Value == "CycleBasedBilling")
                                        //{
                                        //    PatientBilldata490 = ProcessCPT99490_CycleBasedBilling(patientid, patientProgramid, code, connectionString);
                                        //}

                                        //else
                                        //{
                                        //    break;
                                        //}
                                    }
                                    else
                                    {
                                        break;
                                    }


                                }
                                else
                                {
                                    PatientBilldata490.IsTargetMet = false;
                                    PatientBilldata490.ReadyTobill = 0;
                                    PatientBilldata490.Total = code.TargetReadings*60;
                                    PatientBilldata490.Completed = 0;
                                    PatientBilldata490.CPTCode = code.BillingCode;
                                }
                                PatientBilldata490.ProgramName="CCM-C";
                                PatientBilldateList.Add(PatientBilldata490);
                                break;
                            case "99439":
                                PatientBilldata PatientBilldata439 = new PatientBilldata();
                                if (patientProgramData.status == "Active" ||
                                   patientProgramData.status == "ReadyToDischarge" ||
                                   patientProgramData.status == "OnHold"||
                                   patientProgramData.status == "InActive")
                                {
                                    if (lstConfig.Count > 0)
                                    {

                                        if (provider.Value == "cycle")
                                        {
                                            BillingCodesDetails billingCodesDetails490 = billingCodes.Find(x => x.BillingCode == "99490");
                                            PatientBilldata439 = ProcessCPT99439_CycleBasedBilling(patientid, patientProgramid, code, billingCodesDetails490, connectionString);
                                            PatientBilldata439.Total=null;
                                        }

                                        else if (provider.Value=="30days")
                                        {
                                            BillingCodesDetails billingCodesDetails490 = billingCodes.Find(x => x.BillingCode == "99490");
                                            PatientBilldata439 = ProcessCPT99439_DaysBasedBilling(patientid, patientProgramid, code, billingCodesDetails490, connectionString);
                                            PatientBilldata439.Total=null;
                                        }
                                        else
                                        {
                                            BillingCodesDetails billingCodesDetails490 = billingCodes.Find(x => x.BillingCode == "99490");
                                            PatientBilldata439 = ProcessCPT99439_CycleBasedBilling(patientid, patientProgramid, code, billingCodesDetails490, connectionString);
                                            PatientBilldata439.Total=null;
                                        }
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                                else
                                {
                                    PatientBilldata439.IsTargetMet = false;
                                    PatientBilldata439.Total = null;
                                    PatientBilldata439.ReadyTobill = 0;
                                    PatientBilldata439.Completed = 0;
                                    PatientBilldata439.CPTCode = code.BillingCode;
                                }
                                PatientBilldata439.ProgramName="CCM-C";
                                PatientBilldateList.Add(PatientBilldata439);

                                break;

                        }
                    }
                }
                else if (programName.ToUpper() == "CCM-P")
                {
                    foreach (var code in billingCodes)
                    {
                        switch (code.BillingCode)
                        {

                            case "99491":
                                PatientBilldata PatientBilldata491 = new PatientBilldata();
                                if (patientProgramData.status == "Active" ||
                                   patientProgramData.status == "ReadyToDischarge" ||
                                   patientProgramData.status == "OnHold"||
                                   patientProgramData.status == "InActive")
                                {
                                    if (lstConfig.Count > 0)
                                    {

                                        if (provider.Value == "cycle")
                                        {

                                            PatientBilldata491 = ProcessCPT99491_CycleBasedBilling(patientid, patientProgramid, code, connectionString);

                                        }

                                        else if (provider.Value=="30days")
                                        {

                                            PatientBilldata491 = ProcessCPT99491_DaysBasedBilling(patientid, patientProgramid, code, connectionString);
                                        }
                                        else
                                        {
                                            PatientBilldata491 = ProcessCPT99491_CycleBasedBilling(patientid, patientProgramid, code, connectionString);
                                        }
                                    }
                                    else
                                    {
                                        break;
                                    }

                                }
                                else
                                {
                                    PatientBilldata491.IsTargetMet = false;
                                    PatientBilldata491.Total = code.TargetReadings*60;
                                    PatientBilldata491.ReadyTobill = 0;
                                    PatientBilldata491.Completed = 0;
                                    PatientBilldata491.CPTCode = code.BillingCode;
                                }
                                PatientBilldata491.ProgramName="CCM-P";
                                PatientBilldateList.Add(PatientBilldata491);

                                break;
                            case "99437":
                                PatientBilldata PatientBilldata437 = new PatientBilldata();
                                if (patientProgramData.status == "Active" ||
                                   patientProgramData.status == "ReadyToDischarge" ||
                                   patientProgramData.status == "OnHold"||
                                   patientProgramData.status == "InActive")
                                {
                                    if (lstConfig.Count > 0)
                                    {

                                        if (provider.Value == "cycle")
                                        {
                                            BillingCodesDetails billingCodesDetails491 = billingCodes.Find(x => x.BillingCode == "99491");
                                            PatientBilldata437 = ProcessCPT99437_CycleBasedBilling(patientid, patientProgramid, code, billingCodesDetails491, connectionString);
                                            PatientBilldata437.Total=null;

                                        }

                                        else if (provider.Value == "30days")
                                        {

                                            BillingCodesDetails billingCodesDetails491 = billingCodes.Find(x => x.BillingCode == "99491");
                                            PatientBilldata437 = ProcessCPT99437_DaysBasedBilling(patientid, patientProgramid, code, billingCodesDetails491, connectionString);
                                            PatientBilldata437.Total=null;
                                        }
                                        else
                                        {
                                            BillingCodesDetails billingCodesDetails491 = billingCodes.Find(x => x.BillingCode == "99491");
                                            PatientBilldata437 = ProcessCPT99437_CycleBasedBilling(patientid, patientProgramid, code, billingCodesDetails491, connectionString);
                                            PatientBilldata437.Total=null;
                                        }
                                    }
                                    else
                                    {
                                        break;
                                    }

                                }
                                else
                                {
                                    PatientBilldata437.IsTargetMet = false;
                                    PatientBilldata437.Total = null;
                                    PatientBilldata437.ReadyTobill = 0;
                                    PatientBilldata437.Completed = 0;
                                    PatientBilldata437.CPTCode = code.BillingCode;
                                }
                                PatientBilldata437.ProgramName="CCM-P";
                                PatientBilldateList.Add(PatientBilldata437);

                                break;

                        }
                    }
                }
                else if (programName.ToUpper() == "C-CCM")
                {
                    foreach (var code in billingCodes)
                    {
                        switch (code.BillingCode)
                        {
                            case "G0506":
                                PatientBilldata PatientBilldata = new PatientBilldata();
                                if (patientProgramData.status == "Active" ||
                                   patientProgramData.status == "ReadyToDischarge" ||
                                   patientProgramData.status == "OnHold"||
                                   patientProgramData.status == "InActive")
                                {
                                    if (lstConfig.Count > 0)
                                    {

                                        if (provider.Value == "cycle")
                                        {
                                            PatientBilldata = ProcessCPTG0506_CycleBasedBilling(patientid, patientProgramid, code.TargetReadings, code, connectionString);

                                        }

                                        else if (provider.Value == "30days")
                                        {

                                            PatientBilldata = ProcessCPTG0506_DaysBasedBilling(patientid, patientProgramid, code.TargetReadings, code, connectionString);
                                        }
                                        else
                                        {
                                            PatientBilldata = ProcessCPTG0506_CycleBasedBilling(patientid, patientProgramid, code.TargetReadings, code, connectionString);
                                        }

                                    }
                                    else
                                    {
                                        break;
                                    }


                                }
                                else
                                {
                                    PatientBilldata.IsTargetMet = false;
                                    PatientBilldata.ReadyTobill = 0;
                                    PatientBilldata.Total = code.TargetReadings*60;
                                    PatientBilldata.Completed = 0;
                                    PatientBilldata.CPTCode = code.BillingCode;
                                }
                                PatientBilldata.ProgramName="C-CCM";
                                PatientBilldateList.Add(PatientBilldata);
                                break;

                            case "99487":
                                PatientBilldata PatientBilldata487 = new PatientBilldata();
                                if (patientProgramData.status == "Active" ||
                                   patientProgramData.status == "ReadyToDischarge" ||
                                   patientProgramData.status == "OnHold"||
                                   patientProgramData.status == "InActive")
                                {
                                    if (lstConfig.Count > 0)
                                    {

                                        if (provider.Value == "cycle")
                                        {
                                            PatientBilldata487 = ProcessCPT99487_CycleBasedBilling(patientid, patientProgramid, code, connectionString);

                                        }

                                        else if (provider.Value == "30days")
                                        {

                                            PatientBilldata487 = ProcessCPT99487_DaysBasedBilling(patientid, patientProgramid, code, connectionString);
                                        }
                                        else
                                        {
                                            PatientBilldata487 = ProcessCPT99487_CycleBasedBilling(patientid, patientProgramid, code, connectionString);
                                        }
                                    }
                                    else
                                    {
                                        break;
                                    }

                                }
                                else
                                {
                                    PatientBilldata487.IsTargetMet = false;
                                    PatientBilldata487.Total = code.TargetReadings*60;
                                    PatientBilldata487.ReadyTobill = 0;
                                    PatientBilldata487.Completed = 0;
                                    PatientBilldata487.CPTCode = code.BillingCode;
                                }
                                PatientBilldata487.ProgramName="C-CCM";
                                PatientBilldateList.Add(PatientBilldata487);

                                break;
                            case "99489":
                                PatientBilldata PatientBilldata489 = new PatientBilldata();
                                if (patientProgramData.status == "Active" ||
                                   patientProgramData.status == "ReadyToDischarge" ||
                                   patientProgramData.status == "OnHold"||
                                   patientProgramData.status == "InActive")
                                {
                                    if (lstConfig.Count > 0)
                                    {

                                        if (provider.Value == "cycle")
                                        {
                                            BillingCodesDetails billingCodesDetails487 = billingCodes.Find(x => x.BillingCode == "99487");
                                            PatientBilldata489 = ProcessCPT99489_CycleBasedBilling(patientid, patientProgramid, code, billingCodesDetails487, connectionString);
                                            PatientBilldata489.Total=null;

                                        }

                                        else if (provider.Value == "30days")
                                        {

                                            BillingCodesDetails billingCodesDetails487 = billingCodes.Find(x => x.BillingCode == "99487");
                                            PatientBilldata489 = ProcessCPT99489_DaysBasedBilling(patientid, patientProgramid, code, billingCodesDetails487, connectionString);
                                            PatientBilldata489.Total=null;
                                        }
                                        else
                                        {
                                            BillingCodesDetails billingCodesDetails487 = billingCodes.Find(x => x.BillingCode == "99487");
                                            PatientBilldata489 = ProcessCPT99489_CycleBasedBilling(patientid, patientProgramid, code, billingCodesDetails487, connectionString);
                                            PatientBilldata489.Total=null;
                                        }
                                    }
                                    else
                                    {
                                        break;
                                    }

                                }
                                else
                                {
                                    PatientBilldata489.IsTargetMet = false;
                                    PatientBilldata489.Total = null;
                                    PatientBilldata489.ReadyTobill = 0;
                                    PatientBilldata489.Completed = 0;
                                    PatientBilldata489.CPTCode = code.BillingCode;
                                }
                                PatientBilldata489.ProgramName="C-CCM";
                                PatientBilldateList.Add(PatientBilldata489);

                                break;
                        }
                    }
                }
                else if (programName.ToUpper()=="PCM-P")
                {
                    foreach (var code in billingCodes)
                    {
                        switch (code.BillingCode)
                        {

                            case "99424":
                                PatientBilldata PatientBilldata424 = new PatientBilldata();
                                if (patientProgramData.status == "Active" ||
                                   patientProgramData.status == "ReadyToDischarge" ||
                                   patientProgramData.status == "OnHold"||
                                   patientProgramData.status == "InActive")
                                {
                                    if (lstConfig.Count > 0)
                                    {

                                        if (provider.Value == "cycle")
                                        {
                                            PatientBilldata424 = ProcessCPT99424_CycleBasedBilling(patientid, patientProgramid, code, connectionString);

                                        }

                                        else if (provider.Value == "30days")
                                        {

                                            PatientBilldata424 = ProcessCPT99424_DaysBasedBilling(patientid, patientProgramid, code, connectionString);
                                        }
                                        else
                                        {
                                            PatientBilldata424 = ProcessCPT99424_CycleBasedBilling(patientid, patientProgramid, code, connectionString);
                                        }
                                    }
                                    else
                                    {
                                        break;
                                    }


                                }
                                else
                                {
                                    PatientBilldata424.IsTargetMet = false;
                                    PatientBilldata424.ReadyTobill = 0;
                                    PatientBilldata424.Total = code.TargetReadings*60;
                                    PatientBilldata424.Completed = 0;
                                    PatientBilldata424.CPTCode = code.BillingCode;
                                }
                                PatientBilldata424.ProgramName="PCM-P";
                                PatientBilldateList.Add(PatientBilldata424);
                                break;
                            case "99425":
                                PatientBilldata PatientBilldata425 = new PatientBilldata();
                                if (patientProgramData.status == "Active" ||
                                   patientProgramData.status == "ReadyToDischarge" ||
                                   patientProgramData.status == "OnHold"||
                                   patientProgramData.status == "InActive")
                                {
                                    if (lstConfig.Count > 0)
                                    {

                                        if (provider.Value == "cycle")
                                        {
                                            BillingCodesDetails billingCodesDetails424 = billingCodes.Find(x => x.BillingCode == "99424");
                                            PatientBilldata425 = ProcessCPT99425_CycleBasedBilling(patientid, patientProgramid, code, billingCodesDetails424, connectionString);
                                            PatientBilldata425.Total=null;

                                        }

                                        else if (provider.Value == "30days")
                                        {

                                            BillingCodesDetails billingCodesDetails424 = billingCodes.Find(x => x.BillingCode == "99424");
                                            PatientBilldata425 = ProcessCPT99425_DaysBasedBilling(patientid, patientProgramid, code, billingCodesDetails424, connectionString);
                                            PatientBilldata425.Total=null;
                                        }
                                        else
                                        {
                                            BillingCodesDetails billingCodesDetails424 = billingCodes.Find(x => x.BillingCode == "99424");
                                            PatientBilldata425 = ProcessCPT99425_CycleBasedBilling(patientid, patientProgramid, code, billingCodesDetails424, connectionString);
                                            PatientBilldata425.Total=null;
                                        }
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                                else
                                {
                                    PatientBilldata425.IsTargetMet = false;
                                    PatientBilldata425.Total = null;
                                    PatientBilldata425.ReadyTobill = 0;
                                    PatientBilldata425.Completed = 0;
                                    PatientBilldata425.CPTCode = code.BillingCode;
                                }
                                PatientBilldata425.ProgramName="PCM-P";
                                PatientBilldateList.Add(PatientBilldata425);

                                break;



                        }
                    }
                }
                else if (programName.ToUpper()=="PCM-C")
                {
                    foreach (var code in billingCodes)
                    {
                        switch (code.BillingCode)
                        {


                            case "99426":
                                PatientBilldata PatientBilldata426 = new PatientBilldata();
                                if (patientProgramData.status == "Active" ||
                                   patientProgramData.status == "ReadyToDischarge" ||
                                   patientProgramData.status == "OnHold"||
                                   patientProgramData.status == "InActive")
                                {
                                    if (lstConfig.Count > 0)
                                    {

                                        if (provider.Value == "cycle")
                                        {
                                            PatientBilldata426 = ProcessCPT99426_CycleBasedBilling(patientid, patientProgramid, code, connectionString);

                                        }

                                        else if (provider.Value == "30days")
                                        {

                                            PatientBilldata426 = ProcessCPT99426_DaysBasedBilling(patientid, patientProgramid, code, connectionString);
                                        }
                                        else
                                        {
                                            PatientBilldata426 = ProcessCPT99426_CycleBasedBilling(patientid, patientProgramid, code, connectionString);
                                        }
                                    }
                                    else
                                    {
                                        break;
                                    }

                                }
                                else
                                {
                                    PatientBilldata426.IsTargetMet = false;
                                    PatientBilldata426.Total = code.TargetReadings*60;
                                    PatientBilldata426.ReadyTobill = 0;
                                    PatientBilldata426.Completed = 0;
                                    PatientBilldata426.CPTCode = code.BillingCode;
                                }
                                PatientBilldata426.ProgramName="PCM-P";
                                PatientBilldateList.Add(PatientBilldata426);

                                break;
                            case "99427":
                                PatientBilldata PatientBilldata427 = new PatientBilldata();
                                if (patientProgramData.status == "Active" ||
                                   patientProgramData.status == "ReadyToDischarge" ||
                                   patientProgramData.status == "OnHold"||
                                   patientProgramData.status == "InActive")
                                {
                                    if (lstConfig.Count > 0)
                                    {

                                        if (provider.Value == "cycle")
                                        {
                                            BillingCodesDetails billingCodesDetails426 = billingCodes.Find(x => x.BillingCode == "99426");
                                            PatientBilldata427 = ProcessCPT99427_CycleBasedBilling(patientid, patientProgramid, code, billingCodesDetails426, connectionString);
                                            PatientBilldata427.Total=null;

                                        }

                                        else if (provider.Value == "30days")
                                        {

                                            BillingCodesDetails billingCodesDetails426 = billingCodes.Find(x => x.BillingCode == "99426");
                                            PatientBilldata427 = ProcessCPT99427_DaysBasedBilling(patientid, patientProgramid, code, billingCodesDetails426, connectionString);
                                            PatientBilldata427.Total=null;
                                        }
                                        else
                                        {
                                            BillingCodesDetails billingCodesDetails426 = billingCodes.Find(x => x.BillingCode == "99426");
                                            PatientBilldata427 = ProcessCPT99427_CycleBasedBilling(patientid, patientProgramid, code, billingCodesDetails426, connectionString);
                                            PatientBilldata427.Total=null;
                                        }
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                                else
                                {
                                    PatientBilldata427.IsTargetMet = false;
                                    PatientBilldata427.Total = null;
                                    PatientBilldata427.ReadyTobill = 0;
                                    PatientBilldata427.Completed = 0;
                                    PatientBilldata427.CPTCode = code.BillingCode;
                                }
                                PatientBilldata427.ProgramName="PCM-P";
                                PatientBilldateList.Add(PatientBilldata427);

                                break;


                        }
                    }
                }
                PatientBilldateList.RemoveAll(item => item == null);
                return PatientBilldateList;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception occured - class Name :" + this.GetType().Name + ",  Method Name : " + MethodBase.GetCurrentMethod().Name + ", Error Message :" + ex.Message + "");
                return null;
            }
        }
        public PatientProgramData patientprogramInfo(int patientid, int patientProgramid, string connectionString)
        {

            try
            {
                // string ConnectionString = ConfigurationManager.AppSettings["RPM"].ToString();
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();

                    using (SqlCommand command = new SqlCommand("usp_GetPatientProgramInfoById", con))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@PatientProgramId", patientProgramid);
                        command.Parameters.AddWithValue("@patientid", patientid);
                        SqlParameter returnParameter = command.Parameters.Add("RetVal", SqlDbType.Int);
                        returnParameter.Direction = ParameterDirection.ReturnValue;
                        SqlDataReader reader = command.ExecuteReader();

                        PatientProgramData patientData = new PatientProgramData();
                        List<int> roleids = new List<int>();
                        while (reader.Read())
                        {

                            patientData.PatienttId = Convert.ToInt32(reader["PatientId"]);
                            patientData.PatientProgramid = Convert.ToInt32(reader["ID"]);
                            patientData.TargetReading = Convert.ToInt32(reader["TargetReadings"]);
                            patientData.status = reader["Status"].ToString();
                        }

                        if (reader.FieldCount == 0)
                        {
                            con.Close();
                            return null;
                        }
                        con.Close();
                        return patientData;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception occured - class Name :" + this.GetType().Name + ",  Method Name : " + MethodBase.GetCurrentMethod().Name + ", Error Message :" + ex.Message + "");
                return null;
            }
        }
        public PatientBilldata ProcessCPT99453_DaysBasedBilling(int patientId, int patientProgramId, int targetReading, BillingCodesDetails code, string connectionString)
        {
            try
            {
                PatientBilldata PatientBilldata = new PatientBilldata();
                PatientStartDate patientStartdate = GetBillingStartDateEx(patientId, patientProgramId, code.BillingCodeID, connectionString);
                //Startdate is null means already billed in the patient billing table for 53 code.
                if (patientStartdate.Status == "BilledDate")
                {

                    PatientBilldata.BillingStartDate = (DateTime)patientStartdate.StartDate;
                    PatientBilldata.CPTCode = code.BillingCode;
                    PatientBilldata.Completed = targetReading;
                    PatientBilldata.Total = targetReading;
                    PatientBilldata.IsTargetMet = true;
                    PatientBilldata.ReadyTobill = 1;
                    return PatientBilldata;
                }

                List<PatientDailyBillingData> lstDailyData = GetPatientBillingCounts(patientId, patientProgramId, connectionString);
                if (lstDailyData == null || lstDailyData.Count == 0)
                {

                    PatientBilldata = null;
                }

                foreach (PatientDailyBillingData pdb in lstDailyData)
                {
                    if (pdb.BillingCodeId == code.BillingCodeID)
                    {
                        DateTime createdOn = pdb.CreatedOn;
                        DateTime createdontemp = GetUTCFromLocalTime((DateTime)createdOn.Date, connectionString);
                        bool flag = false;


                        DateTime createdEnd = GetUTCFromLocalTime((DateTime)createdOn.Date.AddDays(1).Date.AddSeconds(-1), connectionString);
                        if (createdOn < createdontemp)
                        {
                            flag = true;
                            createdontemp = GetUTCFromLocalTime((DateTime)createdOn.Date.AddDays(-1).Date, connectionString);
                            createdEnd = GetUTCFromLocalTime((DateTime)createdOn.Date.AddSeconds(-1), connectionString);
                        }
                        List<VitalReading> VitalReadings = GetVitalReadingsLocal(patientProgramId, connectionString, createdontemp, createdOn).ToList();
                        if (VitalReadings.Count == 0)
                        {
                            List<VitalReading> VitalReading = GetVitalReadingsLocal(patientProgramId, connectionString, createdOn, createdEnd).ToList();
                            if (VitalReading.Count != 0)
                            {
                                pdb.TotalVitalCount = pdb.TotalVitalCount + 1;
                            }
                            //List<VitalReading> VitalReadingg = GetVitalReadingsLocal(patientProgramId, connectionString, pdb.CreatedOn, GetUTCFromLocalTime((DateTime)pdb.CreatedOn.AddDays(1).Date.AddSeconds(-1), connectionString)).ToList();
                            //if (VitalReading.Count!=0)
                            //{
                            //    pdb.TotalVitalCount= pdb.TotalVitalCount +1;
                            //}
                        }
                        else
                        {
                            if (flag)
                            {
                                createdontemp = GetUTCFromLocalTime((DateTime)createdOn.Date, connectionString);
                                createdEnd = GetUTCFromLocalTime((DateTime)createdOn.Date.AddDays(1).AddSeconds(-1), connectionString);
                                List<VitalReading> VitalReading = GetVitalReadingsLocal(patientProgramId, connectionString, pdb.CreatedOn, GetUTCFromLocalTime((DateTime)pdb.CreatedOn.AddDays(1).Date.AddSeconds(-1), connectionString)).ToList();
                                if (VitalReading.Count != 0)
                                {
                                    pdb.TotalVitalCount = pdb.TotalVitalCount + 1;
                                }
                            }

                        }
                        PatientBilldata = new PatientBilldata();
                        PatientBilldata.BillingStartDate = (DateTime)pdb.StartDate;
                        PatientBilldata.CPTCode = code.BillingCode;
                        PatientBilldata.Completed = pdb.TotalVitalCount;
                        PatientBilldata.Total = code.TargetReadings;
                        PatientBilldata.IsTargetMet = pdb.TotalVitalCount >= code.TargetReadings ? true : false; ;
                        PatientBilldata.ReadyTobill = (pdb.TotalVitalCount >= code.TargetReadings && pdb.DaysCompleted >= code.BillingThreshold) ? 1 : 0;
                        return PatientBilldata;




                    }
                    else
                    {
                        PatientBilldata = null;
                    }

                }

                if (PatientBilldata == null)
                {
                    int DaysCompleted = 0;
                    PatientStartDate PatientStartDate = new PatientStartDate();
                    PatientStartDate = GetPatientBillingStartDateEx(patientId, patientProgramId, code.BillingCodeID, connectionString);
                    DateTime StartDateTemp = GetLocalTimeFromUTC((DateTime)PatientStartDate.StartDate, connectionString);
                    DateTime Today = GetLocalTimeFromUTC(DateTime.UtcNow.Date, connectionString);
                    if (PatientStartDate == null || PatientStartDate.Status.ToLower() == "invalid")
                    {
                        PatientBilldata = new PatientBilldata();
                        PatientBilldata.CPTCode = code.BillingCode;
                        PatientBilldata.Completed = 0;
                        PatientBilldata.Total = code.TargetReadings;
                        PatientBilldata.IsTargetMet = false;
                        PatientBilldata.ReadyTobill = 0;
                        return PatientBilldata;
                    }
                    else if (StartDateTemp > Today && PatientStartDate.Status.ToLower() == "active")
                    {
                        PatientBilldata = new PatientBilldata();
                        PatientBilldata.CPTCode = code.BillingCode;
                        PatientBilldata.Completed = 0;
                        PatientBilldata.Total = code.TargetReadings;
                        PatientBilldata.IsTargetMet = false;
                        PatientBilldata.ReadyTobill = 0;
                        return PatientBilldata;
                    }
                    else
                    {
                        DateTime stDate = Convert.ToDateTime(PatientStartDate.StartDate).AddDays(1);
                        DateTime endDate = DateTime.UtcNow.Date;
                        DateTime stDatetemp = GetLocalTimeFromUTC((DateTime)stDate, connectionString);
                        DateTime today = DateTime.UtcNow;
                        DateTime endDatetemp = GetLocalTimeFromUTC((DateTime)today, connectionString);

                        // endDate = GetLocalTimeFromUTC((DateTime)endDate, connectionString);
                        //Note: This will happen in the last day of billing.. In the last day, billed date will set in 
                        //the patientbilling table and +1 day will come as next start day.. So we should   
                        // calcualte the start date with respect to the billing threshold
                        if (stDate.Date > endDatetemp)
                        {
                            //stDate = stDate.AddDays(-1 * code.BillingThreshold);
                            BilledDates bd = GetPatientLastBilledPeriods(patientId, patientProgramId, code.BillingCodeID, connectionString);
                            if (bd == null)
                            {
                                PatientBilldata = new PatientBilldata();
                                PatientBilldata.CPTCode = code.BillingCode;
                                PatientBilldata.Completed = 0;
                                PatientBilldata.Total = code.TargetReadings;
                                PatientBilldata.IsTargetMet = false;
                                PatientBilldata.ReadyTobill = 0;
                                return PatientBilldata;
                            }
                            stDatetemp = bd.StartDate;//GetLocalTimeFromUTC((DateTime)bd.StartDate, connectionString);
                            endDatetemp = bd.EndDate;// GetLocalTimeFromUTC((DateTime)bd.EndDate, connectionString);
                        }
                        DateTime stDateLoc = stDatetemp;
                        DateTime endDateLoc = endDatetemp;
                        //DateTime Enddate1 = endDate;
                        var DateDiff = Convert.ToDateTime(stDatetemp).Date - endDatetemp.Date;
                        DaysCompleted = Math.Abs(DateDiff.Days) + 1;
                        List<Dates> Dates = new List<Dates>();
                        List<Dates> DatesNew = new List<Dates>();
                        for (int i = 0; i <= DaysCompleted; i++)
                        {
                            if (i == 0)
                            {
                                endDatetemp = stDateLoc.AddDays(1).Date.AddSeconds(-1);

                            }
                            else if (i == DaysCompleted)
                            {
                                stDatetemp = stDateLoc.AddDays(i).Date;
                                endDatetemp = endDateLoc;
                            }
                            else
                            {
                                stDatetemp = stDateLoc.AddDays(i).Date;
                                endDatetemp = stDatetemp.AddDays(1).Date.AddSeconds(-1);

                            }

                            stDatetemp = GetUTCFromLocalTime((DateTime)stDatetemp, connectionString);
                            endDatetemp = GetUTCFromLocalTime((DateTime)endDatetemp, connectionString);


                            Dates.Add(new Dates() { StartDate = (DateTime)stDatetemp, EndDate = endDatetemp, Totalreading = 0 });

                        }
                        if (Dates.Count > 0)
                        {

                            foreach (Dates date in Dates)
                            {

                                List<VitalReading> VitalReadings = GetVitalReadingsLocal(patientProgramId, connectionString, date.StartDate, date.EndDate).ToList();

                                if (VitalReadings.Count > 0)
                                {

                                    DatesNew.Add(new Dates() { StartDate = (DateTime)date.StartDate, EndDate = date.EndDate, Totalreading = 1 });

                                }
                                else
                                {
                                    DatesNew.Add(new Dates() { StartDate = (DateTime)date.StartDate, EndDate = date.EndDate, Totalreading = 0 });
                                }

                            }


                        }

                        int VitalCount = DatesNew.Where(s => s.Totalreading == 1).Count();

                        //int nVitalCount = 0;
                        // List<VitalReading> VitalReadings = GetVitalReadings(patientProgramId, connectionString).Where(s => s.ReadingDate >= stDate && s.ReadingDate <= endDate).ToList();
                        //if (VitalReadings != null)
                        //{
                        //    nVitalCount = VitalReadings.Count;
                        //}
                        PatientBilldata = new PatientBilldata();
                        PatientBilldata.BillingStartDate = stDate;
                        PatientBilldata.CPTCode = code.BillingCode;
                        PatientBilldata.Completed = VitalCount;
                        PatientBilldata.Total = code.TargetReadings;
                        PatientBilldata.IsTargetMet = (VitalCount >= code.TargetReadings) ? true : false;
                        PatientBilldata.ReadyTobill = (VitalCount >= code.TargetReadings && DaysCompleted >= code.BillingThreshold) ? 1 : 0;
                        return PatientBilldata;
                    }
                }

                return PatientBilldata;

















                //DateTime Enddate = DateTime.Today;
                //var Enddatetemp = Enddate.AddDays(1);
                //var DateDiff = Convert.ToDateTime(patientStartdate.StartDate) - Enddatetemp;
                ////if (Math.Abs(DateDiff.Days) > code.BillingThreshold)
                ////{
                ////    Startdate = Enddatetemp.AddDays(-code.BillingThreshold);
                ////}
                ////GetVitalReadings takes one reading/day, so days completed will be covered in the below method
                //List<VitalReading> VitalReadings = GetVitalReadings(patientProgramId, connectionString).Where(s => s.ReadingDate >= patientStartdate.StartDate && s.ReadingDate <= Enddate /*&& s.programId == patientProgramId*/).ToList();
                //if (VitalReadings == null)
                //{
                //    return null;
                //}
                //if (VitalReadings.Count != 0 && VitalReadings.Count >= targetReading)
                //{
                //    PatientBilldata PatientBilldata = new PatientBilldata();
                //    PatientBilldata.BillingStartDate = (DateTime)patientStartdate.StartDate;
                //    PatientBilldata.CPTCode = code.BillingCode;
                //    PatientBilldata.Completed = VitalReadings.Count;
                //    PatientBilldata.Total = targetReading;
                //    PatientBilldata.IsTargetMet = true;
                //    PatientBilldata.ReadyTobill = 1;
                //    return PatientBilldata;
                //}
                //else
                //{
                //    PatientBilldata PatientBilldata = new PatientBilldata();
                //    PatientBilldata.BillingStartDate = (DateTime)patientStartdate.StartDate;
                //    PatientBilldata.CPTCode = code.BillingCode;
                //    PatientBilldata.Completed = VitalReadings.Count;
                //    PatientBilldata.Total = targetReading;
                //    PatientBilldata.IsTargetMet = false;
                //    PatientBilldata.ReadyTobill = 0;
                //    return PatientBilldata;
                //}


            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception occured - class Name :" + this.GetType().Name + ",  Method Name : " + MethodBase.GetCurrentMethod().Name + ", Error Message :" + ex.Message + "");
                return null;
            }
        }
        public PatientBilldata ProcessCPT99453_CycleBasesBilling(int patientId, int patientProgramId, int targetReading, BillingCodesDetails code, string connectionString)
        {
            try
            {
                PatientBilldata PatientBilldatas = new PatientBilldata();
                List<PatientDailyBillingData> lstDailyData = GetPatientBillingCounts(patientId, patientProgramId, connectionString);
                if (lstDailyData == null || lstDailyData.Count == 0)
                {

                    PatientBilldatas.CPTCode = code.BillingCode;
                    PatientBilldatas.Completed = 0;
                    PatientBilldatas.Total = code.TargetReadings;
                    PatientBilldatas.IsTargetMet = false;
                    PatientBilldatas.ReadyTobill = 0;
                    return PatientBilldatas;
                }

                foreach (PatientDailyBillingData pdb in lstDailyData)
                {
                    if (pdb.BillingCodeId == code.BillingCodeID)
                    {

                        if (pdb.TotalVitalCount >= code.TargetReadings && pdb.DaysCompleted >= code.BillingThreshold)
                        {
                            PatientBilldatas = new PatientBilldata();
                            PatientBilldatas.BillingStartDate = (DateTime)pdb.StartDate;//(DateTime)patientStartdate.StartDate;
                            PatientBilldatas.CPTCode = code.BillingCode;
                            //PatientBilldata.Completed = VitalReadings.Count;
                            PatientBilldatas.Completed = pdb.DaysCompleted;
                            //PatientBilldata.Total = code.TargetReadings;
                            PatientBilldatas.Total = code.BillingThreshold;
                            PatientBilldatas.IsTargetMet = true;
                            PatientBilldatas.ReadyTobill = 1;
                            return PatientBilldatas;
                        }

                        else
                        {
                            PatientBilldatas = new PatientBilldata();
                            PatientBilldatas.BillingStartDate = (DateTime)pdb.StartDate;//(DateTime)patientStartdate.StartDate;
                            PatientBilldatas.CPTCode = code.BillingCode;
                            //PatientBilldata.Completed = VitalReadings.Count;
                            PatientBilldatas.Completed = pdb.DaysCompleted;
                            //PatientBilldata.Total = code.TargetReadings;
                            PatientBilldatas.Total = code.BillingThreshold;
                            PatientBilldatas.IsTargetMet = false;
                            PatientBilldatas.ReadyTobill = 0;
                            return PatientBilldatas;
                        }

                    }
                    else
                    {
                        PatientBilldatas = null;
                    }
                }
                if (PatientBilldatas == null)
                {
                    PatientStartDate patientStartdate = GetBillingStartDateEx(patientId, patientProgramId,
                                                              targetReading, connectionString);
                    if (patientStartdate == null || patientStartdate.Status.ToLower() == "invalid")
                    {
                        PatientBilldatas = new PatientBilldata();
                        PatientBilldatas.CPTCode = code.BillingCode;
                        PatientBilldatas.Completed = 0;
                        PatientBilldatas.Total = code.TargetReadings;
                        PatientBilldatas.IsTargetMet = false;
                        PatientBilldatas.ReadyTobill = 0;
                        return PatientBilldatas;
                    }
                    /*else if (Startdate.Status.ToLower() == "invalid")
                    {
                        SetData(patientProgramData, 0, null, billingCode.BillingCodeID, DaysCompleted, false, null);
                    }*/
                    else
                    {
                        var Startdate = patientStartdate.StartDate;
                        DateTime StartdateTemp = GetLocalTimeFromUTC((DateTime)Startdate, connectionString);
                        DateTime startDateLoc = StartdateTemp;
                        DateTime Enddate = DateTime.UtcNow;
                        DateTime EndDateTemp = GetLocalTimeFromUTC((DateTime)Enddate, connectionString);
                        DateTime endDateLoc = EndDateTemp;

                        //var EnddateTemp = Enddate.AddDays(1);
                        var DateDiff = Convert.ToDateTime(StartdateTemp).Date - EndDateTemp.Date;
                        var DaysCompleted = Math.Abs(DateDiff.Days);
                        // Counting the current day also for day of completion
                        // No need to compate with threashold as its yearly data 


                        List<Dates> Dates = new List<Dates>();
                        List<Dates> DatesNew = new List<Dates>();

                        for (int i = 0; i <= DaysCompleted; i++)
                        {
                            if (i == 0)
                            {
                                EndDateTemp = startDateLoc.AddDays(1).Date.AddSeconds(-1);

                            }
                            else if (i == DaysCompleted)
                            {
                                StartdateTemp = startDateLoc.AddDays(i).Date;
                                EndDateTemp = endDateLoc;
                            }
                            else
                            {
                                StartdateTemp = startDateLoc.AddDays(i).Date;
                                EndDateTemp = StartdateTemp.AddDays(1).Date.AddSeconds(-1);

                            }

                            StartdateTemp = GetUTCFromLocalTime((DateTime)StartdateTemp, connectionString);
                            EndDateTemp = GetUTCFromLocalTime((DateTime)EndDateTemp, connectionString);


                            Dates.Add(new Dates() { StartDate = (DateTime)StartdateTemp, EndDate = EndDateTemp, Totalreading = 0 });




                        }

                        if (Dates.Count > 0)
                        {

                            foreach (Dates date in Dates)
                            {
                                List<VitalReading> VitalReadings = GetVitalReadingsLocal(patientProgramId, connectionString, date.StartDate, date.EndDate).ToList();


                                if (VitalReadings.Count > 0)
                                {

                                    DatesNew.Add(new Dates() { StartDate = (DateTime)date.StartDate, EndDate = date.EndDate, Totalreading = 1 });

                                }
                                else
                                {
                                    DatesNew.Add(new Dates() { StartDate = (DateTime)date.StartDate, EndDate = date.EndDate, Totalreading = 0 });
                                }

                            }


                        }

                        int VitalCount = DatesNew.Where(s => s.Totalreading == 1).Count();









                        if (VitalCount == 0)
                        {
                            PatientBilldatas = new PatientBilldata();
                            PatientBilldatas.BillingStartDate = (DateTime)Startdate;//(DateTime)patientStartdate.StartDate;
                            PatientBilldatas.CPTCode = code.BillingCode;
                            PatientBilldatas.Completed = 0;
                            //PatientBilldata.Total = code.TargetReadings;
                            PatientBilldatas.Total = code.BillingThreshold;
                            PatientBilldatas.IsTargetMet = false;
                            PatientBilldatas.ReadyTobill = 0;
                            return PatientBilldatas;
                        }
                        if (VitalCount >= code.TargetReadings && DaysCompleted >= code.BillingThreshold)
                        {
                            PatientBilldatas = new PatientBilldata();
                            PatientBilldatas.BillingStartDate = (DateTime)Startdate;//(DateTime)patientStartdate.StartDate;
                            PatientBilldatas.CPTCode = code.BillingCode;
                            //PatientBilldata.Completed = VitalReadings.Count;
                            PatientBilldatas.Completed = DaysCompleted;
                            //PatientBilldata.Total = code.TargetReadings;
                            PatientBilldatas.Total = code.BillingThreshold;
                            PatientBilldatas.IsTargetMet = true;
                            PatientBilldatas.ReadyTobill = 1;
                            return PatientBilldatas;
                        }
                        else
                        {
                            PatientBilldatas = new PatientBilldata();
                            PatientBilldatas.BillingStartDate = (DateTime)Startdate;//(DateTime)patientStartdate.StartDate;
                            PatientBilldatas.CPTCode = code.BillingCode;
                            //PatientBilldata.Completed = VitalReadings.Count;
                            PatientBilldatas.Completed = DaysCompleted;
                            //PatientBilldata.Total = code.TargetReadings;
                            PatientBilldatas.Total = code.BillingThreshold;
                            PatientBilldatas.IsTargetMet = false;
                            PatientBilldatas.ReadyTobill = 0;
                            return PatientBilldatas;
                        }
                    }
                }
                return PatientBilldatas;













                //PatientStartDate patientStartdate = GetBillingStartDateEx(patientId, patientProgramId,
                //                                          targetReading, connectionString);
                //if (patientStartdate == null || patientStartdate.Status.ToLower() == "invalid")
                //{
                //    PatientBilldata PatientBilldata = new PatientBilldata();
                //    PatientBilldata.CPTCode = code.BillingCode;
                //    PatientBilldata.Completed = 0;
                //    PatientBilldata.Total = code.TargetReadings;
                //    PatientBilldata.IsTargetMet = false;
                //    PatientBilldata.ReadyTobill = 0;
                //    return PatientBilldata;
                //}
                ///*else if (Startdate.Status.ToLower() == "invalid")
                //{
                //    SetData(patientProgramData, 0, null, billingCode.BillingCodeID, DaysCompleted, false, null);
                //}*/
                //else
                //{
                //    var Startdate = patientStartdate.StartDate;
                //    DateTime StartdateTemp = GetLocalTimeFromUTC((DateTime)Startdate, connectionString);
                //    DateTime startDateLoc = StartdateTemp;
                //    DateTime Enddate = DateTime.UtcNow;
                //    DateTime EndDateTemp = GetLocalTimeFromUTC((DateTime)Enddate, connectionString);
                //    DateTime endDateLoc = EndDateTemp;

                //    //var EnddateTemp = Enddate.AddDays(1);
                //    var DateDiff = Convert.ToDateTime(StartdateTemp).Date - EndDateTemp.Date;
                //    var DaysCompleted = Math.Abs(DateDiff.Days);
                //    // Counting the current day also for day of completion
                //    // No need to compate with threashold as its yearly data 


                //    List<Dates> Dates = new List<Dates>();
                //    List<Dates> DatesNew = new List<Dates>();

                //    for (int i = 0; i<=DaysCompleted; i++)
                //    {
                //        if (i==0)
                //        {
                //            EndDateTemp = startDateLoc.AddDays(1).Date.AddSeconds(-1);

                //        }
                //        else if (i==DaysCompleted)
                //        {
                //            StartdateTemp = startDateLoc.AddDays(i).Date;
                //            EndDateTemp = endDateLoc ;
                //        }
                //        else
                //        {
                //            StartdateTemp = startDateLoc.AddDays(i).Date;
                //            EndDateTemp = StartdateTemp.AddDays(1).Date.AddSeconds(-1);

                //        }

                //        StartdateTemp = GetUTCFromLocalTime((DateTime)StartdateTemp, connectionString);
                //        EndDateTemp = GetUTCFromLocalTime((DateTime)EndDateTemp, connectionString);


                //        Dates.Add(new Dates() { StartDate = (DateTime)StartdateTemp, EndDate = EndDateTemp, Totalreading=0 });




                //    }

                //    if (Dates.Count>0)
                //    {

                //        foreach (Dates date in Dates)
                //        {
                //            List<VitalReading> VitalReadings = GetVitalReadingsLocal(patientProgramId, connectionString,date.StartDate,date.EndDate).ToList();


                //            if (VitalReadings.Count>0)
                //            {

                //                DatesNew.Add(new Dates() { StartDate = (DateTime)date.StartDate, EndDate = date.EndDate, Totalreading=1 });

                //            }
                //            else
                //            {
                //                DatesNew.Add(new Dates() { StartDate = (DateTime)date.StartDate, EndDate = date.EndDate, Totalreading=0 });
                //            }

                //        }


                //    }

                //    int VitalCount = DatesNew.Where(s => s.Totalreading == 1).Count();









                //    if (VitalCount == 0)
                //    {
                //        PatientBilldata PatientBilldata = new PatientBilldata();
                //        PatientBilldata.BillingStartDate = (DateTime)Startdate;//(DateTime)patientStartdate.StartDate;
                //        PatientBilldata.CPTCode = code.BillingCode;
                //        PatientBilldata.Completed = 0;
                //        //PatientBilldata.Total = code.TargetReadings;
                //        PatientBilldata.Total = code.BillingThreshold;
                //        PatientBilldata.IsTargetMet = false;
                //        PatientBilldata.ReadyTobill = 0;
                //        return PatientBilldata;
                //    }
                //    if (VitalCount >= code.TargetReadings && DaysCompleted>=code.BillingThreshold)
                //    {
                //        PatientBilldata PatientBilldata = new PatientBilldata();
                //        PatientBilldata.BillingStartDate = (DateTime)Startdate;//(DateTime)patientStartdate.StartDate;
                //        PatientBilldata.CPTCode = code.BillingCode;
                //        //PatientBilldata.Completed = VitalReadings.Count;
                //        PatientBilldata.Completed = DaysCompleted;
                //        //PatientBilldata.Total = code.TargetReadings;
                //        PatientBilldata.Total = code.BillingThreshold;
                //        PatientBilldata.IsTargetMet = true;
                //        PatientBilldata.ReadyTobill = 1;
                //        return PatientBilldata;
                //    }
                //    else
                //    {
                //        PatientBilldata PatientBilldata = new PatientBilldata();
                //        PatientBilldata.BillingStartDate = (DateTime)Startdate;//(DateTime)patientStartdate.StartDate;
                //        PatientBilldata.CPTCode = code.BillingCode;
                //        //PatientBilldata.Completed = VitalReadings.Count;
                //        PatientBilldata.Completed = DaysCompleted;
                //        //PatientBilldata.Total = code.TargetReadings;
                //        PatientBilldata.Total = code.BillingThreshold;
                //        PatientBilldata.IsTargetMet = false;
                //        PatientBilldata.ReadyTobill = 0;
                //        return PatientBilldata;
                //    }
                //}
            }
            catch (Exception ex)
            {
                //Console.WriteLine("Exception occured - class Name :" + this.GetType().Name + ",  Method Name : " + MethodBase.GetCurrentMethod().Name + ", Error Message :" + ex.Message + "");
                throw ex;
            }

        }
        public PatientBilldata ProcessCPT99454_DaysBasedBilling(int patientId, int patientProgramId, BillingCodesDetails code, string connectionString)
        {
            try
            {
                PatientBilldata PatientBilldatas = new PatientBilldata();
                List<PatientDailyBillingData> lstDailyData = GetPatientBillingCounts(patientId, patientProgramId, connectionString);
                if (lstDailyData == null || lstDailyData.Count == 0)
                {

                    PatientBilldatas = null;
                }

                foreach (PatientDailyBillingData pdb in lstDailyData)
                {
                    if (pdb.BillingCodeId == code.BillingCodeID)
                    {
                        DateTime createdOn = pdb.CreatedOn;
                        DateTime createdontemp = GetUTCFromLocalTime((DateTime)createdOn.Date, connectionString);
                        bool flag = false;


                        DateTime createdEnd = GetUTCFromLocalTime((DateTime)createdOn.Date.AddDays(1).Date.AddSeconds(-1), connectionString);
                        if (createdOn < createdontemp)
                        {
                            flag = true;
                            createdontemp = GetUTCFromLocalTime((DateTime)createdOn.Date.AddDays(-1).Date, connectionString);
                            createdEnd = GetUTCFromLocalTime((DateTime)createdOn.Date.AddSeconds(-1), connectionString);
                        }
                        List<VitalReading> VitalReadings = GetVitalReadingsLocal(patientProgramId, connectionString, createdontemp, createdOn).ToList();
                        if (VitalReadings.Count == 0)
                        {
                            List<VitalReading> VitalReading = GetVitalReadingsLocal(patientProgramId, connectionString, createdOn, createdEnd).ToList();
                            if (VitalReading.Count != 0)
                            {
                                pdb.TotalVitalCount = pdb.TotalVitalCount + 1;
                            }
                            //List<VitalReading> VitalReadingg = GetVitalReadingsLocal(patientProgramId, connectionString, pdb.CreatedOn, GetUTCFromLocalTime((DateTime)pdb.CreatedOn.AddDays(1).Date.AddSeconds(-1), connectionString)).ToList();
                            //if (VitalReading.Count!=0)
                            //{
                            //    pdb.TotalVitalCount= pdb.TotalVitalCount +1;
                            //}
                        }
                        else
                        {
                            if (flag)
                            {
                                createdontemp = GetUTCFromLocalTime((DateTime)createdOn.Date, connectionString);
                                createdEnd = GetUTCFromLocalTime((DateTime)createdOn.Date.AddDays(1).AddSeconds(-1), connectionString);
                                List<VitalReading> VitalReading = GetVitalReadingsLocal(patientProgramId, connectionString, pdb.CreatedOn, GetUTCFromLocalTime((DateTime)pdb.CreatedOn.AddDays(1).Date.AddSeconds(-1), connectionString)).ToList();
                                if (VitalReading.Count != 0)
                                {
                                    pdb.TotalVitalCount = pdb.TotalVitalCount + 1;
                                }
                            }

                        }
                        PatientBilldatas = new PatientBilldata();
                        PatientBilldatas.BillingStartDate = (DateTime)pdb.StartDate;
                        PatientBilldatas.CPTCode = code.BillingCode;
                        PatientBilldatas.Completed = pdb.TotalVitalCount;
                        PatientBilldatas.Total = code.TargetReadings;
                        PatientBilldatas.IsTargetMet = pdb.TotalVitalCount >= code.TargetReadings ? true : false;
                        PatientBilldatas.ReadyTobill = (pdb.TotalVitalCount >= code.TargetReadings && pdb.DaysCompleted >= code.BillingThreshold) ? 1 : 0;
                        return PatientBilldatas;




                    }
                    else
                    {
                        PatientBilldatas = null;
                    }

                }

                if (PatientBilldatas == null)
                {
                    int DaysCompleted = 0;
                    PatientStartDate PatientStartDate = new PatientStartDate();
                    PatientStartDate = GetPatientBillingStartDateEx(patientId, patientProgramId, code.BillingCodeID, connectionString);
                    DateTime StartDateTemp = GetLocalTimeFromUTC((DateTime)PatientStartDate.StartDate, connectionString);
                    DateTime Today = GetLocalTimeFromUTC(DateTime.UtcNow.Date, connectionString);
                    if (PatientStartDate == null || PatientStartDate.Status.ToLower() == "invalid")
                    {
                        PatientBilldatas = new PatientBilldata();
                        PatientBilldatas.CPTCode = code.BillingCode;
                        PatientBilldatas.Completed = 0;
                        PatientBilldatas.Total = code.TargetReadings;
                        PatientBilldatas.IsTargetMet = false;
                        PatientBilldatas.ReadyTobill = 0;
                        return PatientBilldatas;
                    }
                    else if (StartDateTemp > Today && PatientStartDate.Status.ToLower() == "active")
                    {
                        PatientBilldatas = new PatientBilldata();
                        PatientBilldatas.CPTCode = code.BillingCode;
                        PatientBilldatas.Completed = 0;
                        PatientBilldatas.Total = code.TargetReadings;
                        PatientBilldatas.IsTargetMet = false;
                        PatientBilldatas.ReadyTobill = 0;
                        return PatientBilldatas;
                    }
                    else
                    {
                        //DateTime stDate = Convert.ToDateTime(PatientStartDate.StartDate).AddDays(1);
                        DateTime stDate = DateTime.MinValue;
                        if (PatientStartDate.Status.ToLower() == "active")
                        {
                            stDate = Convert.ToDateTime(PatientStartDate.StartDate).AddDays(1);
                        }
                        else
                        {
                            stDate = Convert.ToDateTime(PatientStartDate.StartDate);
                        }
                        DateTime endDate = DateTime.UtcNow.Date;
                        DateTime stDatetemp = GetLocalTimeFromUTC((DateTime)stDate, connectionString);

                        DateTime endDatetemp = GetLocalTimeFromUTC((DateTime)endDate, connectionString);

                        // endDate = GetLocalTimeFromUTC((DateTime)endDate, connectionString);
                        //Note: This will happen in the last day of billing.. In the last day, billed date will set in 
                        //the patientbilling table and +1 day will come as next start day.. So we should   
                        // calcualte the start date with respect to the billing threshold
                        DateTime today = DateTime.UtcNow;
                        DateTime endDateTempay = GetLocalTimeFromUTC((DateTime)today, connectionString);
                        if (stDatetemp.Date > endDateTempay)
                        {
                            //stDate = stDate.AddDays(-1 * code.BillingThreshold);
                            BilledDates bd = GetPatientLastBilledPeriods(patientId, patientProgramId, code.BillingCodeID, connectionString);
                            if (bd== null)
                            {
                                PatientBilldata PatientBilldata = new PatientBilldata();
                                PatientBilldata.CPTCode = code.BillingCode;
                                PatientBilldata.Completed = 0;
                                PatientBilldata.Total = code.TargetReadings;
                                PatientBilldata.IsTargetMet = false;
                                PatientBilldata.ReadyTobill = 0;
                                return PatientBilldata;
                            }
                            stDatetemp = bd.StartDate;//GetLocalTimeFromUTC((DateTime)bd.StartDate, connectionString);
                            endDatetemp = bd.EndDate;// GetLocalTimeFromUTC((DateTime)bd.EndDate, connectionString);
                        }
                        DateTime stDateLoc = stDatetemp;
                        DateTime endDateLoc = endDatetemp;
                        //DateTime Enddate1 = endDate;
                        var DateDiff = Convert.ToDateTime(stDatetemp).Date - endDatetemp.Date;
                        DaysCompleted = Math.Abs(DateDiff.Days) + 1;
                        List<Dates> Dates = new List<Dates>();
                        List<Dates> DatesNew = new List<Dates>();
                        for (int i = 0; i <= DaysCompleted; i++)
                        {
                            if (i == 0)
                            {
                                endDatetemp = stDateLoc.AddDays(1).Date.AddSeconds(-1);

                            }
                            else if (i == DaysCompleted)
                            {
                                stDatetemp = stDateLoc.AddDays(i).Date;
                                endDatetemp = endDateLoc;
                            }
                            else
                            {
                                stDatetemp = stDateLoc.AddDays(i).Date;
                                endDatetemp = stDatetemp.AddDays(1).Date.AddSeconds(-1);

                            }

                            stDatetemp = GetUTCFromLocalTime((DateTime)stDatetemp, connectionString);
                            endDatetemp = GetUTCFromLocalTime((DateTime)endDatetemp, connectionString);


                            Dates.Add(new Dates() { StartDate = (DateTime)stDatetemp, EndDate = endDatetemp, Totalreading = 0 });

                        }
                        if (Dates.Count > 0)
                        {

                            foreach (Dates date in Dates)
                            {

                                List<VitalReading> VitalReadings = GetVitalReadingsLocal(patientProgramId, connectionString, date.StartDate, date.EndDate).ToList();

                                if (VitalReadings.Count > 0)
                                {

                                    DatesNew.Add(new Dates() { StartDate = (DateTime)date.StartDate, EndDate = date.EndDate, Totalreading = 1 });

                                }
                                else
                                {
                                    DatesNew.Add(new Dates() { StartDate = (DateTime)date.StartDate, EndDate = date.EndDate, Totalreading = 0 });
                                }

                            }


                        }

                        int VitalCount = DatesNew.Where(s => s.Totalreading == 1).Count();

                        //int nVitalCount = 0;
                        // List<VitalReading> VitalReadings = GetVitalReadings(patientProgramId, connectionString).Where(s => s.ReadingDate >= stDate && s.ReadingDate <= endDate).ToList();
                        //if (VitalReadings != null)
                        //{
                        //    nVitalCount = VitalReadings.Count;
                        //}
                        PatientBilldatas = new PatientBilldata();
                        PatientBilldatas.BillingStartDate = stDate;
                        PatientBilldatas.CPTCode = code.BillingCode;
                        PatientBilldatas.Completed = VitalCount;
                        PatientBilldatas.Total = code.TargetReadings;
                        PatientBilldatas.IsTargetMet = (VitalCount >= code.TargetReadings) ? true : false;
                        PatientBilldatas.ReadyTobill = (VitalCount >= code.TargetReadings && DaysCompleted >= code.BillingThreshold) ? 1 : 0;
                        return PatientBilldatas;
                    }
                }
                return PatientBilldatas;





                //int DaysCompleted = 0;
                //PatientStartDate PatientStartDate = new PatientStartDate();
                //PatientStartDate = GetPatientBillingStartDateEx(patientId, patientProgramId, code.BillingCodeID, connectionString);
                //DateTime StartDateTemp = GetLocalTimeFromUTC((DateTime)PatientStartDate.StartDate, connectionString);
                //DateTime Today = GetLocalTimeFromUTC(DateTime.UtcNow.Date, connectionString); 
                //if (PatientStartDate == null || PatientStartDate.Status.ToLower() == "invalid")
                //{
                //    PatientBilldata PatientBilldata = new PatientBilldata();
                //    PatientBilldata.CPTCode = code.BillingCode;
                //    PatientBilldata.Completed = 0;
                //    PatientBilldata.Total = code.TargetReadings;
                //    PatientBilldata.IsTargetMet = false;
                //    PatientBilldata.ReadyTobill = 0;
                //    return PatientBilldata;
                //}
                //else if (StartDateTemp > Today && PatientStartDate.Status.ToLower() == "active")
                //{
                //    PatientBilldata PatientBilldata = new PatientBilldata();
                //    PatientBilldata.CPTCode = code.BillingCode;
                //    PatientBilldata.Completed = 0;
                //    PatientBilldata.Total = code.TargetReadings;
                //    PatientBilldata.IsTargetMet = false;
                //    PatientBilldata.ReadyTobill = 0;
                //    return PatientBilldata;
                //}
                //else
                //{
                //    DateTime stDate = Convert.ToDateTime(PatientStartDate.StartDate);
                //    DateTime endDate = DateTime.UtcNow.Date;
                //    DateTime stDatetemp = GetLocalTimeFromUTC((DateTime)stDate, connectionString);

                //    DateTime endDatetemp = GetLocalTimeFromUTC((DateTime)endDate, connectionString);

                //   // endDate = GetLocalTimeFromUTC((DateTime)endDate, connectionString);
                //    //Note: This will happen in the last day of billing.. In the last day, billed date will set in 
                //    //the patientbilling table and +1 day will come as next start day.. So we should   
                //    // calcualte the start date with respect to the billing threshold
                //    if (stDatetemp > endDatetemp)
                //    {
                //        //stDate = stDate.AddDays(-1 * code.BillingThreshold);
                //        BilledDates bd = GetPatientLastBilledPeriods(patientId, patientProgramId, code.BillingCodeID, connectionString);

                //        stDatetemp = GetLocalTimeFromUTC((DateTime)bd.StartDate, connectionString);
                //        stDatetemp = GetLocalTimeFromUTC((DateTime)bd.EndDate, connectionString); ;
                //    }
                //    DateTime stDateLoc = stDatetemp;
                //    DateTime endDateLoc = endDatetemp;
                //    //DateTime Enddate1 = endDate;
                //    var DateDiff = Convert.ToDateTime(stDatetemp).Date - endDatetemp.Date;
                //    DaysCompleted = Math.Abs(DateDiff.Days)+1;
                //    List<Dates> Dates = new List<Dates>();
                //    List<Dates> DatesNew = new List<Dates>();
                //    for (int i = 0; i<=DaysCompleted; i++)
                //    {
                //        if (i==0)
                //        {
                //            endDatetemp = stDateLoc.AddDays(1).Date.AddSeconds(-1);

                //        }
                //        else if (i==DaysCompleted)
                //        {
                //            stDatetemp = stDateLoc.AddDays(i).Date;
                //            endDatetemp = endDateLoc;
                //        }
                //        else
                //        {
                //            stDatetemp = stDateLoc.AddDays(i).Date;
                //            endDatetemp = stDatetemp.AddDays(1).Date.AddSeconds(-1);

                //        }

                //        stDatetemp = GetUTCFromLocalTime((DateTime)stDatetemp, connectionString);
                //        endDatetemp = GetUTCFromLocalTime((DateTime)endDatetemp, connectionString);


                //        Dates.Add(new Dates() { StartDate = (DateTime)stDatetemp, EndDate = endDatetemp, Totalreading=0 });

                //    }
                //    if (Dates.Count>0)
                //    {

                //        foreach (Dates date in Dates)
                //        {

                //            List<VitalReading> VitalReadings = GetVitalReadingsLocal(patientProgramId, connectionString, date.StartDate, date.EndDate).ToList();

                //            if (VitalReadings.Count>0)
                //            {

                //                DatesNew.Add(new Dates() { StartDate = (DateTime)date.StartDate, EndDate = date.EndDate, Totalreading=1 });

                //            }
                //            else
                //            {
                //                DatesNew.Add(new Dates() { StartDate = (DateTime)date.StartDate, EndDate = date.EndDate, Totalreading=0 });
                //            }

                //        }


                //    }

                //    int VitalCount = DatesNew.Where(s => s.Totalreading == 1).Count();

                //    //int nVitalCount = 0;
                //   // List<VitalReading> VitalReadings = GetVitalReadings(patientProgramId, connectionString).Where(s => s.ReadingDate >= stDate && s.ReadingDate <= endDate).ToList();
                //    //if (VitalReadings != null)
                //    //{
                //    //    nVitalCount = VitalReadings.Count;
                //    //}
                //    PatientBilldata PatientBilldata = new PatientBilldata();
                //    PatientBilldata.BillingStartDate = stDate;
                //    PatientBilldata.CPTCode = code.BillingCode;
                //    PatientBilldata.Completed = VitalCount;
                //    PatientBilldata.Total = code.TargetReadings;
                //    PatientBilldata.IsTargetMet = (VitalCount >= code.TargetReadings) ? true : false;
                //    PatientBilldata.ReadyTobill = (VitalCount >= code.TargetReadings && DaysCompleted >= code.BillingThreshold) ? 1 : 0;
                //    return PatientBilldata;
                //}
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public PatientBilldata ProcessCPT99454_CycleBasedBilling(int patientId, int patientProgramId, BillingCodesDetails code, string connectionString)
        {
            try
            {
                PatientBilldata PatientBilldatas = new PatientBilldata();
                List<PatientDailyBillingData> lstDailyData = GetPatientBillingCounts(patientId, patientProgramId, connectionString);
                if (lstDailyData == null || lstDailyData.Count == 0)
                {

                    PatientBilldatas = null;
                }

                foreach (PatientDailyBillingData pdb in lstDailyData)
                {
                    if (pdb.BillingCodeId == code.BillingCodeID)
                    {
                        DateTime createdOn = pdb.CreatedOn;
                        DateTime createdontemp = GetUTCFromLocalTime((DateTime)createdOn.Date, connectionString);
                        bool flag = false;


                        DateTime createdEnd = GetUTCFromLocalTime((DateTime)createdOn.Date.AddDays(1).Date.AddSeconds(-1), connectionString);
                        if (createdOn < createdontemp)
                        {
                            flag = true;
                            createdontemp = GetUTCFromLocalTime((DateTime)createdOn.Date.AddDays(-1).Date, connectionString);
                            createdEnd = GetUTCFromLocalTime((DateTime)createdOn.Date.AddSeconds(-1), connectionString);
                        }
                        List<VitalReading> VitalReadings = GetVitalReadingsLocal(patientProgramId, connectionString, createdontemp, createdOn).ToList();
                        if (VitalReadings.Count == 0)
                        {
                            List<VitalReading> VitalReading = GetVitalReadingsLocal(patientProgramId, connectionString, createdOn, createdEnd).ToList();
                            if (VitalReading.Count != 0)
                            {
                                pdb.TotalVitalCount = pdb.TotalVitalCount + 1;
                            }
                            //List<VitalReading> VitalReadingg = GetVitalReadingsLocal(patientProgramId, connectionString, pdb.CreatedOn, GetUTCFromLocalTime((DateTime)pdb.CreatedOn.AddDays(1).Date.AddSeconds(-1), connectionString)).ToList();
                            //if (VitalReading.Count!=0)
                            //{
                            //    pdb.TotalVitalCount= pdb.TotalVitalCount +1;
                            //}
                        }
                        else
                        {
                            if (flag)
                            {
                                createdontemp = GetUTCFromLocalTime((DateTime)createdOn.Date, connectionString);
                                createdEnd = GetUTCFromLocalTime((DateTime)createdOn.Date.AddDays(1).AddSeconds(-1), connectionString);
                                List<VitalReading> VitalReading = GetVitalReadingsLocal(patientProgramId, connectionString, pdb.CreatedOn, GetUTCFromLocalTime((DateTime)pdb.CreatedOn.AddDays(1).Date.AddSeconds(-1), connectionString)).ToList();
                                if (VitalReading.Count != 0)
                                {
                                    pdb.TotalVitalCount = pdb.TotalVitalCount + 1;
                                }
                            }

                        }
                        PatientBilldatas = new PatientBilldata();
                        PatientBilldatas.BillingStartDate = (DateTime)pdb.StartDate;
                        PatientBilldatas.CPTCode = code.BillingCode;
                        PatientBilldatas.Completed = pdb.TotalVitalCount;
                        PatientBilldatas.Total = code.TargetReadings;
                        PatientBilldatas.IsTargetMet = pdb.TotalVitalCount >= code.TargetReadings ? true : false;
                        PatientBilldatas.ReadyTobill = (pdb.TotalVitalCount >= code.TargetReadings && pdb.DaysCompleted >= code.BillingThreshold) ? 1 : 0;
                        return PatientBilldatas;




                    }
                    else
                    {
                        PatientBilldatas = null;
                    }

                }

                if (PatientBilldatas == null)
                {
                    int DaysCompleted = 0;
                    PatientStartDate PatientStartDate = new PatientStartDate();
                    PatientStartDate = GetPatientBillingStartDateEx(patientId, patientProgramId, code.BillingCodeID, connectionString);
                    DateTime StartDateTemp = GetLocalTimeFromUTC((DateTime)PatientStartDate.StartDate, connectionString);
                    DateTime Today = GetLocalTimeFromUTC(DateTime.UtcNow.Date, connectionString);
                    if (PatientStartDate == null || PatientStartDate.Status.ToLower() == "invalid")
                    {
                        PatientBilldatas = new PatientBilldata();
                        PatientBilldatas.CPTCode = code.BillingCode;
                        PatientBilldatas.Completed = 0;
                        PatientBilldatas.Total = code.TargetReadings;
                        PatientBilldatas.IsTargetMet = false;
                        PatientBilldatas.ReadyTobill = 0;
                        return PatientBilldatas;
                    }
                    else if (PatientStartDate.StartDate > DateTime.UtcNow && PatientStartDate.Status.ToLower() == "active")
                    {
                        PatientBilldatas = new PatientBilldata();
                        PatientBilldatas.CPTCode = code.BillingCode;
                        PatientBilldatas.Completed = 0;
                        PatientBilldatas.Total = code.TargetReadings;
                        PatientBilldatas.IsTargetMet = false;
                        PatientBilldatas.ReadyTobill = 0;
                        return PatientBilldatas;
                    }
                    else
                    {
                        DateTime stDate = Convert.ToDateTime(PatientStartDate.StartDate);
                        DateTime endDate = DateTime.UtcNow.Date;
                        DateTime stDatetemp = GetLocalTimeFromUTC((DateTime)stDate, connectionString);
                        DateTime Todayy = DateTime.UtcNow;
                        DateTime endDatetemp = GetLocalTimeFromUTC((DateTime)Todayy, connectionString);

                        // endDate = GetLocalTimeFromUTC((DateTime)endDate, connectionString);
                        //Note: This will happen in the last day of billing.. In the last day, billed date will set in 
                        //the patientbilling table and +1 day will come as next start day.. So we should   
                        // calcualte the start date with respect to the billing threshold
                        if (stDate.Date > endDatetemp)
                        {
                            //stDate = stDate.AddDays(-1 * code.BillingThreshold);
                            BilledDates bd = GetPatientLastBilledPeriods(patientId, patientProgramId, code.BillingCodeID, connectionString);
                            if (bd == null)
                            {
                                PatientBilldatas = new PatientBilldata();
                                PatientBilldatas.CPTCode = code.BillingCode;
                                PatientBilldatas.Completed = 0;
                                PatientBilldatas.Total = code.TargetReadings;
                                PatientBilldatas.IsTargetMet = false;
                                PatientBilldatas.ReadyTobill = 0;
                                return PatientBilldatas;
                            }
                            stDatetemp = bd.StartDate;//GetLocalTimeFromUTC((DateTime)bd.StartDate, connectionString);
                            endDatetemp = bd.EndDate;// GetLocalTimeFromUTC((DateTime)bd.EndDate, connectionString);
                        }
                        DateTime stDateLoc = stDatetemp;
                        DateTime endDateLoc = endDatetemp;
                        //DateTime Enddate1 = endDate;
                        var DateDiff = Convert.ToDateTime(stDatetemp).Date - endDatetemp.Date;
                        DaysCompleted = Math.Abs(DateDiff.Days) + 1;
                        List<Dates> Dates = new List<Dates>();
                        List<Dates> DatesNew = new List<Dates>();
                        for (int i = 0; i <= DaysCompleted; i++)
                        {
                            if (i == 0)
                            {
                                endDatetemp = stDateLoc.AddDays(1).Date.AddSeconds(-1);

                            }
                            else if (i == DaysCompleted)
                            {
                                stDatetemp = stDateLoc.AddDays(i).Date;
                                endDatetemp = endDateLoc;
                            }
                            else
                            {
                                stDatetemp = stDateLoc.AddDays(i).Date;
                                endDatetemp = stDatetemp.AddDays(1).Date.AddSeconds(-1);

                            }

                            stDatetemp = GetUTCFromLocalTime((DateTime)stDatetemp, connectionString);
                            endDatetemp = GetUTCFromLocalTime((DateTime)endDatetemp, connectionString);


                            Dates.Add(new Dates() { StartDate = (DateTime)stDatetemp, EndDate = endDatetemp, Totalreading = 0 });

                        }
                        if (Dates.Count > 0)
                        {

                            foreach (Dates date in Dates)
                            {

                                List<VitalReading> VitalReadings = GetVitalReadingsLocal(patientProgramId, connectionString, date.StartDate, date.EndDate).ToList();

                                if (VitalReadings.Count > 0)
                                {

                                    DatesNew.Add(new Dates() { StartDate = (DateTime)date.StartDate, EndDate = date.EndDate, Totalreading = 1 });

                                }
                                else
                                {
                                    DatesNew.Add(new Dates() { StartDate = (DateTime)date.StartDate, EndDate = date.EndDate, Totalreading = 0 });
                                }

                            }


                        }

                        int VitalCount = DatesNew.Where(s => s.Totalreading == 1).Count();

                        //int nVitalCount = 0;
                        // List<VitalReading> VitalReadings = GetVitalReadings(patientProgramId, connectionString).Where(s => s.ReadingDate >= stDate && s.ReadingDate <= endDate).ToList();
                        //if (VitalReadings != null)
                        //{
                        //    nVitalCount = VitalReadings.Count;
                        //}
                        PatientBilldatas = new PatientBilldata();
                        PatientBilldatas.BillingStartDate = stDate;
                        PatientBilldatas.CPTCode = code.BillingCode;
                        PatientBilldatas.Completed = VitalCount;
                        PatientBilldatas.Total = code.TargetReadings;
                        PatientBilldatas.IsTargetMet = (VitalCount >= code.TargetReadings) ? true : false;
                        PatientBilldatas.ReadyTobill = (VitalCount >= code.TargetReadings && DaysCompleted >= code.BillingThreshold) ? 1 : 0;
                        return PatientBilldatas;
                    }
                }
                return PatientBilldatas;





                //int DaysCompleted = 0;
                //PatientStartDate PatientStartDate = new PatientStartDate();
                //PatientStartDate = GetPatientBillingStartDateEx(patientId, patientProgramId, code.BillingCodeID, connectionString);
                //DateTime StartDateTemp = GetLocalTimeFromUTC((DateTime)PatientStartDate.StartDate, connectionString);
                //DateTime Today = GetLocalTimeFromUTC(DateTime.UtcNow.Date, connectionString); 
                //if (PatientStartDate == null || PatientStartDate.Status.ToLower() == "invalid")
                //{
                //    PatientBilldata PatientBilldata = new PatientBilldata();
                //    PatientBilldata.CPTCode = code.BillingCode;
                //    PatientBilldata.Completed = 0;
                //    PatientBilldata.Total = code.TargetReadings;
                //    PatientBilldata.IsTargetMet = false;
                //    PatientBilldata.ReadyTobill = 0;
                //    return PatientBilldata;
                //}
                //else if (StartDateTemp > Today && PatientStartDate.Status.ToLower() == "active")
                //{
                //    PatientBilldata PatientBilldata = new PatientBilldata();
                //    PatientBilldata.CPTCode = code.BillingCode;
                //    PatientBilldata.Completed = 0;
                //    PatientBilldata.Total = code.TargetReadings;
                //    PatientBilldata.IsTargetMet = false;
                //    PatientBilldata.ReadyTobill = 0;
                //    return PatientBilldata;
                //}
                //else
                //{
                //    DateTime stDate = Convert.ToDateTime(PatientStartDate.StartDate);
                //    DateTime endDate = DateTime.UtcNow.Date;
                //    DateTime stDatetemp = GetLocalTimeFromUTC((DateTime)stDate, connectionString);

                //    DateTime endDatetemp = GetLocalTimeFromUTC((DateTime)endDate, connectionString);

                //   // endDate = GetLocalTimeFromUTC((DateTime)endDate, connectionString);
                //    //Note: This will happen in the last day of billing.. In the last day, billed date will set in 
                //    //the patientbilling table and +1 day will come as next start day.. So we should   
                //    // calcualte the start date with respect to the billing threshold
                //    if (stDatetemp > endDatetemp)
                //    {
                //        //stDate = stDate.AddDays(-1 * code.BillingThreshold);
                //        BilledDates bd = GetPatientLastBilledPeriods(patientId, patientProgramId, code.BillingCodeID, connectionString);

                //        stDatetemp = GetLocalTimeFromUTC((DateTime)bd.StartDate, connectionString);
                //        stDatetemp = GetLocalTimeFromUTC((DateTime)bd.EndDate, connectionString); ;
                //    }
                //    DateTime stDateLoc = stDatetemp;
                //    DateTime endDateLoc = endDatetemp;
                //    //DateTime Enddate1 = endDate;
                //    var DateDiff = Convert.ToDateTime(stDatetemp).Date - endDatetemp.Date;
                //    DaysCompleted = Math.Abs(DateDiff.Days)+1;
                //    List<Dates> Dates = new List<Dates>();
                //    List<Dates> DatesNew = new List<Dates>();
                //    for (int i = 0; i<=DaysCompleted; i++)
                //    {
                //        if (i==0)
                //        {
                //            endDatetemp = stDateLoc.AddDays(1).Date.AddSeconds(-1);

                //        }
                //        else if (i==DaysCompleted)
                //        {
                //            stDatetemp = stDateLoc.AddDays(i).Date;
                //            endDatetemp = endDateLoc;
                //        }
                //        else
                //        {
                //            stDatetemp = stDateLoc.AddDays(i).Date;
                //            endDatetemp = stDatetemp.AddDays(1).Date.AddSeconds(-1);

                //        }

                //        stDatetemp = GetUTCFromLocalTime((DateTime)stDatetemp, connectionString);
                //        endDatetemp = GetUTCFromLocalTime((DateTime)endDatetemp, connectionString);


                //        Dates.Add(new Dates() { StartDate = (DateTime)stDatetemp, EndDate = endDatetemp, Totalreading=0 });

                //    }
                //    if (Dates.Count>0)
                //    {

                //        foreach (Dates date in Dates)
                //        {

                //            List<VitalReading> VitalReadings = GetVitalReadingsLocal(patientProgramId, connectionString, date.StartDate, date.EndDate).ToList();

                //            if (VitalReadings.Count>0)
                //            {

                //                DatesNew.Add(new Dates() { StartDate = (DateTime)date.StartDate, EndDate = date.EndDate, Totalreading=1 });

                //            }
                //            else
                //            {
                //                DatesNew.Add(new Dates() { StartDate = (DateTime)date.StartDate, EndDate = date.EndDate, Totalreading=0 });
                //            }

                //        }


                //    }

                //    int VitalCount = DatesNew.Where(s => s.Totalreading == 1).Count();

                //    //int nVitalCount = 0;
                //   // List<VitalReading> VitalReadings = GetVitalReadings(patientProgramId, connectionString).Where(s => s.ReadingDate >= stDate && s.ReadingDate <= endDate).ToList();
                //    //if (VitalReadings != null)
                //    //{
                //    //    nVitalCount = VitalReadings.Count;
                //    //}
                //    PatientBilldata PatientBilldata = new PatientBilldata();
                //    PatientBilldata.BillingStartDate = stDate;
                //    PatientBilldata.CPTCode = code.BillingCode;
                //    PatientBilldata.Completed = VitalCount;
                //    PatientBilldata.Total = code.TargetReadings;
                //    PatientBilldata.IsTargetMet = (VitalCount >= code.TargetReadings) ? true : false;
                //    PatientBilldata.ReadyTobill = (VitalCount >= code.TargetReadings && DaysCompleted >= code.BillingThreshold) ? 1 : 0;
                //    return PatientBilldata;
                //}
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public PatientBilldata ProcessCPT99457_DaysBasedBilling(int patientId, int patientProgramId, BillingCodesDetails code, string connectionString)
        {
            PatientBilldata pbd = new PatientBilldata();
            try
            {
                int DaysCompleted = 0; int TotalReading = 0;
                PatientStartDate PatientStartDate = new PatientStartDate();
                PatientStartDate = GetPatientBillingStartDateEx(patientId, patientProgramId, code.BillingCodeID, connectionString);
                if (PatientStartDate == null ||
                    PatientStartDate.Status.ToLower() == "invalid")
                {
                    pbd.CPTCode = code.BillingCode;
                    pbd.Completed = 0;
                    pbd.Total = code.TargetReadings*60;
                    pbd.IsTargetMet = false;
                    pbd.ReadyTobill = 0;
                    return pbd;
                }
                else if (PatientStartDate.StartDate > DateTime.UtcNow.Date && PatientStartDate.Status.ToLower() == "active")
                {
                    PatientBilldata PatientBilldata = new PatientBilldata();
                    PatientBilldata.CPTCode = code.BillingCode;
                    PatientBilldata.Completed = 0;
                    PatientBilldata.Total = code.TargetReadings*60;
                    PatientBilldata.IsTargetMet = false;
                    PatientBilldata.ReadyTobill = 0;
                    return PatientBilldata;
                }
                else
                {

                    //DateTime stDate = Convert.ToDateTime(PatientStartDate.StartDate).AddDays(1);
                    DateTime stDate = DateTime.MinValue;
                    if (PatientStartDate.Status.ToLower() == "active")
                    {
                        stDate = Convert.ToDateTime(PatientStartDate.StartDate).AddDays(1);
                    }
                    else
                    {
                        stDate = Convert.ToDateTime(PatientStartDate.StartDate);
                    }
                    DateTime endDate = DateTime.UtcNow;
                    DateTime today = DateTime.UtcNow;
                    DateTime endDateTempay = GetLocalTimeFromUTC((DateTime)today, connectionString);
                    //Note: This will happen in the last day of billing.. In the last day, billed date will set in 
                    //the patientbilling table and +1 day will come as next start day.. So we should   
                    // calcualte the start date with respect to the billing threshold
                    if (stDate.Date > endDateTempay)
                    {
                        //stDate = stDate.AddDays(-1 * code.BillingThreshold);
                        BilledDates bd = GetPatientLastBilledPeriods(patientId, patientProgramId, code.BillingCodeID, connectionString);
                        if (bd== null)
                        {
                            PatientBilldata PatientBilldata = new PatientBilldata();
                            PatientBilldata.CPTCode = code.BillingCode;
                            PatientBilldata.Completed = 0;
                            PatientBilldata.Total = code.TargetReadings*60;
                            PatientBilldata.IsTargetMet = false;
                            PatientBilldata.ReadyTobill = 0;
                            return PatientBilldata;
                        }
                        stDate = bd.StartDate;

                    }



                    if (PatientStartDate.Status.ToLower() == "billeddate")
                    {
                        stDate = GetUTCFromLocalTime((DateTime)stDate, connectionString);
                        endDate =DateTime.UtcNow;

                    }








                    List<PatientInteraction> PatientInteractiontim = GetPatientInteractiontime(patientId, patientProgramId, connectionString, stDate, endDate).ToList();
                    if (PatientInteractiontim == null)
                    {
                        pbd.CPTCode = code.BillingCode;
                        pbd.Completed = 0;
                        pbd.Total = code.TargetReadings*60;
                        pbd.IsTargetMet = false;
                        pbd.ReadyTobill = 0;
                        pbd.BillingStartDate = stDate;
                        return pbd;
                    }
                    var isEstablishedExist = PatientInteractiontim.Where(s => s.IsCallNote == 1).ToList();
                    if (isEstablishedExist.Count() > 0)
                    {
                        isEstablishedExist = isEstablishedExist.Where(s => s.IsEstablishedCall == 1).ToList();
                        if (isEstablishedExist != null && isEstablishedExist.Count() == 0)
                        {
                            PatientInteractiontim.Clear();
                        }
                    }
                    if (PatientInteractiontim.Count == 0 || PatientInteractiontim == null || isEstablishedExist.Count == 0)
                    {
                        pbd.CPTCode = code.BillingCode;
                        pbd.Completed = 0;
                        pbd.Total = code.TargetReadings*60;
                        pbd.IsTargetMet = false;
                        pbd.ReadyTobill = 0;
                        pbd.BillingStartDate = stDate;
                        return pbd;
                    }
                    TotalReading = PatientInteractiontim.Sum(s => s.Duration);

                    if (TotalReading >= code.TargetReadings * 60)
                    {
                        TotalReading = code.TargetReadings * 60;
                    }
                    pbd.BillingStartDate = stDate;
                    pbd.CPTCode = code.BillingCode;
                    pbd.Completed = TotalReading;
                    pbd.Total = code.TargetReadings * 60;
                    pbd.IsTargetMet = (TotalReading >= code.TargetReadings * 60) ? true : false;
                    pbd.ReadyTobill = (TotalReading >= (code.TargetReadings * 60) && DaysCompleted >= code.BillingThreshold) ? 1 : 0;
                    return pbd;
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            //PatientBilldata pbd = new PatientBilldata();
            //try
            //{
            //    PatientBilldata PatientBilldatas = new PatientBilldata();
            //    List<PatientDailyBillingData> lstDailyData = GetPatientBillingCounts(patientId, patientProgramId, connectionString);
            //    if (lstDailyData == null || lstDailyData.Count == 0)
            //    {

            //        //PatientBilldatas.CPTCode = code.BillingCode;
            //        //PatientBilldatas.Completed = 0;
            //        //PatientBilldatas.Total = code.TargetReadings;
            //        //PatientBilldatas.IsTargetMet = false;
            //        //PatientBilldatas.ReadyTobill = 0;
            //        //return PatientBilldatas;
            //        PatientBilldatas=null;
            //    }

            //    foreach (PatientDailyBillingData pdb in lstDailyData)
            //    {
            //        if (pdb.BillingCodeId == code.BillingCodeID)
            //        {
            //            DateTime createdOn = pdb.CreatedOn;
            //            DateTime createdOnEnd = DateTime.UtcNow;
            //            List<PatientInteraction> PatientInteractiontim = GetPatientInteractiontime(patientId, patientProgramId, connectionString, createdOn, createdOnEnd).ToList();
            //            if (PatientInteractiontim != null && PatientInteractiontim.Count > 0)
            //            {
            //                var isEstablishedExist = PatientInteractiontim.Where(s => s.IsCallNote == 1).ToList();
            //                if (isEstablishedExist.Count() > 0)
            //                {
            //                    isEstablishedExist = isEstablishedExist.Where(s => s.IsEstablishedCall == 1).ToList();
            //                    if (isEstablishedExist != null && isEstablishedExist.Count() == 0)
            //                    {
            //                        PatientInteractiontim.Clear();
            //                    }
            //                }
            //                if (PatientInteractiontim != null && PatientInteractiontim.Count > 0)
            //                {
            //                    int TotalReading = PatientInteractiontim.Sum(s => s.Duration);
            //                    pdb.TotalDuration = pdb.TotalDuration + TotalReading;
            //                }

            //                //if (PatientInteractiontim.Count==0 || PatientInteractiontim==null || isEstablishedExist.Count==0)
            //                //{
            //                //    TotalReading = 0;
            //                //}

            //            }
            //            PatientBilldatas = new PatientBilldata();
            //            PatientBilldatas.BillingStartDate = (DateTime)pdb.StartDate;
            //            PatientBilldatas.CPTCode = code.BillingCode;
            //            PatientBilldatas.Completed = pdb.TotalDuration;
            //            PatientBilldatas.Total = code.TargetReadings;
            //            PatientBilldatas.IsTargetMet = (pdb.TotalDuration >= code.TargetReadings * 60) ? true : false;
            //            PatientBilldatas.ReadyTobill = (pdb.TotalDuration >= (code.TargetReadings * 60) && pdb.DaysCompleted >= code.BillingThreshold) ? 1 : 0;
            //            return PatientBilldatas;

            //        }
            //        else
            //        {
            //            PatientBilldatas = null;
            //        }
            //    }

            //    if (PatientBilldatas == null)
            //    {
            //        int DaysCompleted = 0; int TotalReading = 0;
            //        PatientStartDate PatientStartDate = new PatientStartDate();
            //        PatientStartDate = GetPatientBillingStartDateEx(patientId, patientProgramId, code.BillingCodeID, connectionString);
            //        if (PatientStartDate == null ||
            //            PatientStartDate.Status.ToLower() == "invalid")
            //        {
            //            pbd.CPTCode = code.BillingCode;
            //            pbd.Completed = 0;
            //            pbd.Total = code.TargetReadings;
            //            pbd.IsTargetMet = false;
            //            pbd.ReadyTobill = 0;
            //            return pbd;
            //        }
            //        else if (PatientStartDate.StartDate > DateTime.UtcNow.Date && PatientStartDate.Status.ToLower() == "active")
            //        {
            //            PatientBilldatas = new PatientBilldata();
            //            PatientBilldatas.CPTCode = code.BillingCode;
            //            PatientBilldatas.Completed = 0;
            //            PatientBilldatas.Total = code.TargetReadings;
            //            PatientBilldatas.IsTargetMet = false;
            //            PatientBilldatas.ReadyTobill = 0;
            //            return PatientBilldatas;
            //        }
            //        else
            //        {
            //            DateTime stDate = Convert.ToDateTime(PatientStartDate.StartDate);
            //            DateTime endDate = DateTime.UtcNow.Date;
            //            DateTime startDatetemp = GetLocalTimeFromUTC((DateTime)stDate, connectionString);
            //            DateTime endDatetemp = GetLocalTimeFromUTC((DateTime)endDate, connectionString);
            //            //Note: This will happen in the last day of billing.. In the last day, billed date will set in 
            //            //the patientbilling table and +1 day will come as next start day.. So we should   
            //            // calcualte the start date with respect to the billing threshold
            //            if (stDate > DateTime.UtcNow.Date)
            //            {
            //                //stDate = stDate.AddDays(-1 * code.BillingThreshold);
            //                BilledDates bd = GetPatientLastBilledPeriods(patientId, patientProgramId, code.BillingCodeID, connectionString);
            //                startDatetemp = bd.StartDate;
            //                endDatetemp = bd.EndDate;
            //            }


            //            DateTime startDateLoc = startDatetemp;
            //            DateTime enddateLoc = endDatetemp;
            //            int nTotDays = Math.Abs((startDatetemp - endDatetemp).Days) + 1;
            //            DaysCompleted = nTotDays;
            //            List<Dates> Dates = new List<Dates>();
            //            for (int i = 0; i <= DaysCompleted; i++)
            //            {
            //                if (i == 0)
            //                {
            //                    endDatetemp = startDateLoc.AddDays(1).Date.AddSeconds(-1);

            //                }
            //                else if (i == DaysCompleted)
            //                {
            //                    startDatetemp = startDateLoc.AddDays(i).Date;
            //                    endDatetemp = startDatetemp.AddDays(1).Date.AddSeconds(-1);
            //                }
            //                else
            //                {
            //                    startDatetemp = startDateLoc.AddDays(i).Date;
            //                    endDatetemp = startDatetemp.AddDays(1).Date.AddSeconds(-1);

            //                }

            //                startDatetemp = GetUTCFromLocalTime((DateTime)startDatetemp, connectionString);
            //                endDatetemp = GetUTCFromLocalTime((DateTime)endDatetemp, connectionString);


            //                Dates.Add(new Dates() { StartDate = (DateTime)startDatetemp, EndDate = endDatetemp, Totalreading = 0 });

            //            }
            //            List<PatientInteraction> PatientInteractiontimList = new List<PatientInteraction>();
            //            if (Dates.Count > 0)
            //            {

            //                foreach (Dates date in Dates)
            //                {

            //                    List<PatientInteraction> PatientInteractiontims = GetPatientInteractiontime(patientId, patientProgramId, connectionString, date.StartDate, date.EndDate).ToList();
            //                    //List<VitalReading> VitalReadings = billing.GetVitalReadings(patientProgramData, con).Where(s => s.ReadingDate >= date.StartDate && s.ReadingDate <= date.EndDate).ToList();

            //                    if (PatientInteractiontims != null && PatientInteractiontims.Count > 0)
            //                    {
            //                        foreach (var list in PatientInteractiontims)
            //                        {
            //                            PatientInteractiontimList.Add(list);
            //                        }

            //                    }

            //                }


            //            }









            //            // List<PatientInteraction> PatientInteractiontim = GetPatientInteractiontime(patientId, patientProgramId, connectionString).Where(s => s.Date >= stDate && s.Date <= endDate).ToList();
            //            if (PatientInteractiontimList == null)
            //            {
            //                PatientBilldatas = new PatientBilldata();
            //                PatientBilldatas.CPTCode = code.BillingCode;
            //                PatientBilldatas.Completed = 0;
            //                PatientBilldatas.Total = code.TargetReadings;
            //                PatientBilldatas.IsTargetMet = false;
            //                PatientBilldatas.ReadyTobill = 0;
            //                return PatientBilldatas;
            //            }
            //            var isEstablishedExist = PatientInteractiontimList.Where(s => s.IsCallNote == 1).ToList();
            //            if (isEstablishedExist.Count() > 0)
            //            {
            //                isEstablishedExist = isEstablishedExist.Where(s => s.IsEstablishedCall == 1).ToList();
            //                if (isEstablishedExist != null && isEstablishedExist.Count() == 0)
            //                {
            //                    PatientInteractiontimList.Clear();
            //                }
            //            }
            //            if (PatientInteractiontimList.Count == 0 || PatientInteractiontimList == null || isEstablishedExist.Count == 0)
            //            {
            //                PatientBilldatas = new PatientBilldata();
            //                PatientBilldatas.CPTCode = code.BillingCode;
            //                PatientBilldatas.Completed = 0;
            //                PatientBilldatas.Total = code.TargetReadings;
            //                PatientBilldatas.IsTargetMet = false;
            //                PatientBilldatas.ReadyTobill = 0;
            //                return PatientBilldatas;
            //            }
            //            TotalReading = PatientInteractiontimList.Sum(s => s.Duration);

            //            if (TotalReading >= code.TargetReadings * 60)
            //            {
            //                TotalReading = code.TargetReadings * 60;
            //            }
            //            PatientBilldatas = new PatientBilldata();
            //            PatientBilldatas.BillingStartDate = stDate;
            //            PatientBilldatas.CPTCode = code.BillingCode;
            //            PatientBilldatas.Completed = TotalReading;
            //            PatientBilldatas.Total = code.TargetReadings * 60;
            //            PatientBilldatas.IsTargetMet = (TotalReading >= code.TargetReadings * 60) ? true : false;
            //            PatientBilldatas.ReadyTobill = (TotalReading >= (code.TargetReadings * 60) && DaysCompleted >= code.BillingThreshold) ? 1 : 0;
            //            return PatientBilldatas;
            //        }
            //    }

            //    return PatientBilldatas;





            //int DaysCompleted = 0; int TotalReading = 0;
            //PatientStartDate PatientStartDate = new PatientStartDate();
            //PatientStartDate = GetPatientBillingStartDateEx(patientId, patientProgramId, code.BillingCodeID, connectionString);
            //if (PatientStartDate == null ||
            //    PatientStartDate.Status.ToLower() == "invalid")
            //{
            //    pbd.CPTCode = code.BillingCode;
            //    pbd.Completed = 0;
            //    pbd.Total = code.TargetReadings;
            //    pbd.IsTargetMet = false;
            //    pbd.ReadyTobill = 0;
            //    return pbd;
            //}
            //else if (PatientStartDate.StartDate > DateTime.UtcNow.Date && PatientStartDate.Status.ToLower() == "active")
            //{
            //    PatientBilldata PatientBilldata = new PatientBilldata();
            //    PatientBilldata.CPTCode = code.BillingCode;
            //    PatientBilldata.Completed = 0;
            //    PatientBilldata.Total = code.TargetReadings;
            //    PatientBilldata.IsTargetMet = false;
            //    PatientBilldata.ReadyTobill = 0;
            //    return PatientBilldata;
            //}
            //else
            //{
            //    DateTime stDate = Convert.ToDateTime(PatientStartDate.StartDate);
            //    DateTime endDate = DateTime.UtcNow.Date;

            //    //Note: This will happen in the last day of billing.. In the last day, billed date will set in 
            //    //the patientbilling table and +1 day will come as next start day.. So we should   
            //    // calcualte the start date with respect to the billing threshold
            //    if (stDate > DateTime.UtcNow.Date)
            //    {
            //        //stDate = stDate.AddDays(-1 * code.BillingThreshold);
            //        BilledDates bd = GetPatientLastBilledPeriods(patientId, patientProgramId, code.BillingCodeID, connectionString);
            //        stDate = bd.StartDate;
            //        endDate = bd.EndDate;
            //    }
            //    List<PatientInteraction> PatientInteractiontim = GetPatientInteractiontime(patientId, patientProgramId, connectionString).Where(s => s.Date >= stDate && s.Date <= endDate).ToList();
            //    if (PatientInteractiontim == null)
            //    {
            //        pbd.CPTCode = code.BillingCode;
            //        pbd.Completed = 0;
            //        pbd.Total = code.TargetReadings;
            //        pbd.IsTargetMet = false;
            //        pbd.ReadyTobill = 0;
            //        return pbd;
            //    }
            //    var isEstablishedExist = PatientInteractiontim.Where(s => s.IsCallNote == 1).ToList();
            //    if (isEstablishedExist.Count() > 0)
            //    {
            //        isEstablishedExist = isEstablishedExist.Where(s => s.IsEstablishedCall == 1).ToList();
            //        if (isEstablishedExist != null && isEstablishedExist.Count() == 0)
            //        {
            //            PatientInteractiontim.Clear();
            //        }
            //    }
            //    if (PatientInteractiontim.Count == 0 || PatientInteractiontim == null || isEstablishedExist.Count==0)
            //    {
            //        pbd.CPTCode = code.BillingCode;
            //        pbd.Completed = 0;
            //        pbd.Total = code.TargetReadings;
            //        pbd.IsTargetMet = false;
            //        pbd.ReadyTobill = 0;
            //        return pbd;
            //    }
            //    TotalReading = PatientInteractiontim.Sum(s => s.Duration);

            //    if (TotalReading>= code.TargetReadings * 60)
            //    {
            //        TotalReading = code.TargetReadings * 60;
            //    }
            //    pbd.BillingStartDate = stDate;
            //    pbd.CPTCode = code.BillingCode;
            //    pbd.Completed = TotalReading;
            //    pbd.Total = code.TargetReadings * 60;
            //    pbd.IsTargetMet = (TotalReading >= code.TargetReadings*60) ? true : false;
            //    pbd.ReadyTobill = (TotalReading >= (code.TargetReadings*60) && DaysCompleted >= code.BillingThreshold) ? 1 : 0;
            //    return pbd;
            //}
            //}
            //    catch (Exception ex)
            //    {
            //        throw;
            //    }
        }
        public PatientBilldata ProcessCPT99457_CycleBasedBilling(int patientId, int patientProgramId, BillingCodesDetails code, string connectionString)
        {
            PatientBilldata pbd = new PatientBilldata();
            try
            {
                int DaysCompleted = 0; int TotalReading = 0;
                PatientStartDate PatientStartDate = new PatientStartDate();
                PatientStartDate = GetPatientBillingStartDateEx(patientId, patientProgramId, code.BillingCodeID, connectionString);
                if (PatientStartDate == null ||
                    PatientStartDate.Status.ToLower() == "invalid")
                {
                    pbd.CPTCode = code.BillingCode;
                    pbd.Completed = 0;
                    pbd.Total = code.TargetReadings*60;
                    pbd.IsTargetMet = false;
                    pbd.ReadyTobill = 0;
                    return pbd;
                }
                else if (PatientStartDate.StartDate > DateTime.UtcNow && PatientStartDate.Status.ToLower() == "active")
                {
                    PatientBilldata PatientBilldata = new PatientBilldata();
                    PatientBilldata.CPTCode = code.BillingCode;
                    PatientBilldata.Completed = 0;
                    PatientBilldata.Total = code.TargetReadings*60;
                    PatientBilldata.IsTargetMet = false;
                    PatientBilldata.ReadyTobill = 0;
                    return PatientBilldata;
                }
                else
                {

                    DateTime stDate = Convert.ToDateTime(PatientStartDate.StartDate);
                    DateTime endDate = DateTime.UtcNow;//.AddMinutes(10);

                    //Note: This will happen in the last day of billing.. In the last day, billed date will set in 
                    //the patientbilling table and +1 day will come as next start day.. So we should   
                    // calcualte the start date with respect to the billing threshold
                    DateTime today = DateTime.UtcNow;
                    DateTime endDateTempay = GetLocalTimeFromUTC((DateTime)today, connectionString);
                    if (stDate.Date > endDateTempay)
                    {
                        //stDate = stDate.AddDays(-1 * code.BillingThreshold);
                        BilledDates bd = GetPatientLastBilledPeriods(patientId, patientProgramId, code.BillingCodeID, connectionString);
                        if (bd == null)
                        {
                            PatientBilldata PatientBilldata = new PatientBilldata();
                            PatientBilldata.CPTCode = code.BillingCode;
                            PatientBilldata.Completed = 0;
                            PatientBilldata.Total = code.TargetReadings*60;
                            PatientBilldata.IsTargetMet = false;
                            PatientBilldata.ReadyTobill = 0;
                            return PatientBilldata;
                        }
                        stDate = bd.StartDate;

                    }



                    if (PatientStartDate.Status.ToLower() == "billeddate")
                    {
                        stDate = GetUTCFromLocalTime((DateTime)stDate, connectionString);
                        endDate = DateTime.UtcNow;//.AddMinutes(10);

                    }







                    int patientSmsCount = 0;
                    List<PatientInteraction> PatientInteractiontim = GetPatientInteractiontime(patientId, patientProgramId, connectionString, stDate, endDate).ToList();
                    patientSmsCount =GetPatientSmsCount(patientId, connectionString, stDate, endDate);
                    if (PatientInteractiontim == null)
                    {
                        pbd.CPTCode = code.BillingCode;
                        pbd.Completed = 0;
                        pbd.Total = code.TargetReadings*60;
                        pbd.IsTargetMet = false;
                        pbd.ReadyTobill = 0;
                        pbd.BillingStartDate = stDate;
                        return pbd;
                    }
                    var isEstablishedExist = PatientInteractiontim.Where(s => s.IsCallNote == 1).ToList();
                    if (isEstablishedExist.Count() > 0)
                    {
                        isEstablishedExist = isEstablishedExist.Where(s => s.IsEstablishedCall == 1).ToList();
                        if (isEstablishedExist != null && isEstablishedExist.Count() == 0)
                        {
                            if (patientSmsCount <= 0)
                            {
                                PatientInteractiontim.Clear();
                            }

                        }
                    }
                    if (PatientInteractiontim.Count == 0 || PatientInteractiontim == null || isEstablishedExist.Count == 0)
                    {
                        if (patientSmsCount <= 0)
                        {
                            pbd.CPTCode = code.BillingCode;
                            pbd.Completed = 0;
                            pbd.Total = code.TargetReadings*60;
                            pbd.IsTargetMet = false;
                            pbd.ReadyTobill = 0;
                            pbd.BillingStartDate = stDate;
                            return pbd;
                        }

                    }
                    TotalReading = PatientInteractiontim.Sum(s => s.Duration);

                    if (TotalReading >= code.TargetReadings * 60)
                    {
                        TotalReading = code.TargetReadings * 60;
                    }
                    pbd.BillingStartDate = stDate;
                    pbd.CPTCode = code.BillingCode;
                    pbd.Completed = TotalReading;
                    pbd.Total = code.TargetReadings * 60;
                    pbd.IsTargetMet = (TotalReading >= code.TargetReadings * 60) ? true : false;
                    pbd.ReadyTobill = (TotalReading >= (code.TargetReadings * 60) && DaysCompleted >= code.BillingThreshold) ? 1 : 0;
                    return pbd;
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            //PatientBilldata pbd = new PatientBilldata();
            //try
            //{
            //    PatientBilldata PatientBilldatas = new PatientBilldata();
            //    List<PatientDailyBillingData> lstDailyData = GetPatientBillingCounts(patientId, patientProgramId, connectionString);
            //    if (lstDailyData == null || lstDailyData.Count == 0)
            //    {

            //        //PatientBilldatas.CPTCode = code.BillingCode;
            //        //PatientBilldatas.Completed = 0;
            //        //PatientBilldatas.Total = code.TargetReadings;
            //        //PatientBilldatas.IsTargetMet = false;
            //        //PatientBilldatas.ReadyTobill = 0;
            //        //return PatientBilldatas;
            //        PatientBilldatas=null;
            //    }

            //    foreach (PatientDailyBillingData pdb in lstDailyData)
            //    {
            //        if (pdb.BillingCodeId == code.BillingCodeID)
            //        {
            //            DateTime createdOn = pdb.CreatedOn;
            //            DateTime createdOnEnd = DateTime.UtcNow;
            //            List<PatientInteraction> PatientInteractiontim = GetPatientInteractiontime(patientId, patientProgramId, connectionString, createdOn, createdOnEnd).ToList();
            //            if (PatientInteractiontim != null && PatientInteractiontim.Count > 0)
            //            {
            //                var isEstablishedExist = PatientInteractiontim.Where(s => s.IsCallNote == 1).ToList();
            //                if (isEstablishedExist.Count() > 0)
            //                {
            //                    isEstablishedExist = isEstablishedExist.Where(s => s.IsEstablishedCall == 1).ToList();
            //                    if (isEstablishedExist != null && isEstablishedExist.Count() == 0)
            //                    {
            //                        PatientInteractiontim.Clear();
            //                    }
            //                }
            //                if (PatientInteractiontim != null && PatientInteractiontim.Count > 0)
            //                {
            //                    int TotalReading = PatientInteractiontim.Sum(s => s.Duration);
            //                    pdb.TotalDuration = pdb.TotalDuration + TotalReading;
            //                }

            //                //if (PatientInteractiontim.Count==0 || PatientInteractiontim==null || isEstablishedExist.Count==0)
            //                //{
            //                //    TotalReading = 0;
            //                //}

            //            }
            //            PatientBilldatas = new PatientBilldata();
            //            PatientBilldatas.BillingStartDate = (DateTime)pdb.StartDate;
            //            PatientBilldatas.CPTCode = code.BillingCode;
            //            PatientBilldatas.Completed = pdb.TotalDuration;
            //            PatientBilldatas.Total = code.TargetReadings;
            //            PatientBilldatas.IsTargetMet = (pdb.TotalDuration >= code.TargetReadings * 60) ? true : false;
            //            PatientBilldatas.ReadyTobill = (pdb.TotalDuration >= (code.TargetReadings * 60) && pdb.DaysCompleted >= code.BillingThreshold) ? 1 : 0;
            //            return PatientBilldatas;

            //        }
            //        else
            //        {
            //            PatientBilldatas = null;
            //        }
            //    }

            //    if (PatientBilldatas == null)
            //    {
            //        int DaysCompleted = 0; int TotalReading = 0;
            //        PatientStartDate PatientStartDate = new PatientStartDate();
            //        PatientStartDate = GetPatientBillingStartDateEx(patientId, patientProgramId, code.BillingCodeID, connectionString);
            //        if (PatientStartDate == null ||
            //            PatientStartDate.Status.ToLower() == "invalid")
            //        {
            //            pbd.CPTCode = code.BillingCode;
            //            pbd.Completed = 0;
            //            pbd.Total = code.TargetReadings;
            //            pbd.IsTargetMet = false;
            //            pbd.ReadyTobill = 0;
            //            return pbd;
            //        }
            //        else if (PatientStartDate.StartDate > DateTime.UtcNow.Date && PatientStartDate.Status.ToLower() == "active")
            //        {
            //            PatientBilldatas = new PatientBilldata();
            //            PatientBilldatas.CPTCode = code.BillingCode;
            //            PatientBilldatas.Completed = 0;
            //            PatientBilldatas.Total = code.TargetReadings;
            //            PatientBilldatas.IsTargetMet = false;
            //            PatientBilldatas.ReadyTobill = 0;
            //            return PatientBilldatas;
            //        }
            //        else
            //        {
            //            DateTime stDate = Convert.ToDateTime(PatientStartDate.StartDate);
            //            DateTime endDate = DateTime.UtcNow.Date;
            //            DateTime startDatetemp = GetLocalTimeFromUTC((DateTime)stDate, connectionString);
            //            DateTime endDatetemp = GetLocalTimeFromUTC((DateTime)endDate, connectionString);
            //            //Note: This will happen in the last day of billing.. In the last day, billed date will set in 
            //            //the patientbilling table and +1 day will come as next start day.. So we should   
            //            // calcualte the start date with respect to the billing threshold
            //            if (stDate > DateTime.UtcNow.Date)
            //            {
            //                //stDate = stDate.AddDays(-1 * code.BillingThreshold);
            //                BilledDates bd = GetPatientLastBilledPeriods(patientId, patientProgramId, code.BillingCodeID, connectionString);
            //                startDatetemp = bd.StartDate;
            //                endDatetemp = bd.EndDate;
            //            }


            //            DateTime startDateLoc = startDatetemp;
            //            DateTime enddateLoc = endDatetemp;
            //            int nTotDays = Math.Abs((startDatetemp - endDatetemp).Days) + 1;
            //            DaysCompleted = nTotDays;
            //            List<Dates> Dates = new List<Dates>();
            //            for (int i = 0; i <= DaysCompleted; i++)
            //            {
            //                if (i == 0)
            //                {
            //                    endDatetemp = startDateLoc.AddDays(1).Date.AddSeconds(-1);

            //                }
            //                else if (i == DaysCompleted)
            //                {
            //                    startDatetemp = startDateLoc.AddDays(i).Date;
            //                    endDatetemp = startDatetemp.AddDays(1).Date.AddSeconds(-1);
            //                }
            //                else
            //                {
            //                    startDatetemp = startDateLoc.AddDays(i).Date;
            //                    endDatetemp = startDatetemp.AddDays(1).Date.AddSeconds(-1);

            //                }

            //                startDatetemp = GetUTCFromLocalTime((DateTime)startDatetemp, connectionString);
            //                endDatetemp = GetUTCFromLocalTime((DateTime)endDatetemp, connectionString);


            //                Dates.Add(new Dates() { StartDate = (DateTime)startDatetemp, EndDate = endDatetemp, Totalreading = 0 });

            //            }
            //            List<PatientInteraction> PatientInteractiontimList = new List<PatientInteraction>();
            //            if (Dates.Count > 0)
            //            {

            //                foreach (Dates date in Dates)
            //                {

            //                    List<PatientInteraction> PatientInteractiontims = GetPatientInteractiontime(patientId, patientProgramId, connectionString, date.StartDate, date.EndDate).ToList();
            //                    //List<VitalReading> VitalReadings = billing.GetVitalReadings(patientProgramData, con).Where(s => s.ReadingDate >= date.StartDate && s.ReadingDate <= date.EndDate).ToList();

            //                    if (PatientInteractiontims != null && PatientInteractiontims.Count > 0)
            //                    {
            //                        foreach (var list in PatientInteractiontims)
            //                        {
            //                            PatientInteractiontimList.Add(list);
            //                        }

            //                    }

            //                }


            //            }









            //            // List<PatientInteraction> PatientInteractiontim = GetPatientInteractiontime(patientId, patientProgramId, connectionString).Where(s => s.Date >= stDate && s.Date <= endDate).ToList();
            //            if (PatientInteractiontimList == null)
            //            {
            //                PatientBilldatas = new PatientBilldata();
            //                PatientBilldatas.CPTCode = code.BillingCode;
            //                PatientBilldatas.Completed = 0;
            //                PatientBilldatas.Total = code.TargetReadings;
            //                PatientBilldatas.IsTargetMet = false;
            //                PatientBilldatas.ReadyTobill = 0;
            //                return PatientBilldatas;
            //            }
            //            var isEstablishedExist = PatientInteractiontimList.Where(s => s.IsCallNote == 1).ToList();
            //            if (isEstablishedExist.Count() > 0)
            //            {
            //                isEstablishedExist = isEstablishedExist.Where(s => s.IsEstablishedCall == 1).ToList();
            //                if (isEstablishedExist != null && isEstablishedExist.Count() == 0)
            //                {
            //                    PatientInteractiontimList.Clear();
            //                }
            //            }
            //            if (PatientInteractiontimList.Count == 0 || PatientInteractiontimList == null || isEstablishedExist.Count == 0)
            //            {
            //                PatientBilldatas = new PatientBilldata();
            //                PatientBilldatas.CPTCode = code.BillingCode;
            //                PatientBilldatas.Completed = 0;
            //                PatientBilldatas.Total = code.TargetReadings;
            //                PatientBilldatas.IsTargetMet = false;
            //                PatientBilldatas.ReadyTobill = 0;
            //                return PatientBilldatas;
            //            }
            //            TotalReading = PatientInteractiontimList.Sum(s => s.Duration);

            //            if (TotalReading >= code.TargetReadings * 60)
            //            {
            //                TotalReading = code.TargetReadings * 60;
            //            }
            //            PatientBilldatas = new PatientBilldata();
            //            PatientBilldatas.BillingStartDate = stDate;
            //            PatientBilldatas.CPTCode = code.BillingCode;
            //            PatientBilldatas.Completed = TotalReading;
            //            PatientBilldatas.Total = code.TargetReadings * 60;
            //            PatientBilldatas.IsTargetMet = (TotalReading >= code.TargetReadings * 60) ? true : false;
            //            PatientBilldatas.ReadyTobill = (TotalReading >= (code.TargetReadings * 60) && DaysCompleted >= code.BillingThreshold) ? 1 : 0;
            //            return PatientBilldatas;
            //        }
            //    }

            //    return PatientBilldatas;





            //int DaysCompleted = 0; int TotalReading = 0;
            //PatientStartDate PatientStartDate = new PatientStartDate();
            //PatientStartDate = GetPatientBillingStartDateEx(patientId, patientProgramId, code.BillingCodeID, connectionString);
            //if (PatientStartDate == null ||
            //    PatientStartDate.Status.ToLower() == "invalid")
            //{
            //    pbd.CPTCode = code.BillingCode;
            //    pbd.Completed = 0;
            //    pbd.Total = code.TargetReadings;
            //    pbd.IsTargetMet = false;
            //    pbd.ReadyTobill = 0;
            //    return pbd;
            //}
            //else if (PatientStartDate.StartDate > DateTime.UtcNow.Date && PatientStartDate.Status.ToLower() == "active")
            //{
            //    PatientBilldata PatientBilldata = new PatientBilldata();
            //    PatientBilldata.CPTCode = code.BillingCode;
            //    PatientBilldata.Completed = 0;
            //    PatientBilldata.Total = code.TargetReadings;
            //    PatientBilldata.IsTargetMet = false;
            //    PatientBilldata.ReadyTobill = 0;
            //    return PatientBilldata;
            //}
            //else
            //{
            //    DateTime stDate = Convert.ToDateTime(PatientStartDate.StartDate);
            //    DateTime endDate = DateTime.UtcNow.Date;

            //    //Note: This will happen in the last day of billing.. In the last day, billed date will set in 
            //    //the patientbilling table and +1 day will come as next start day.. So we should   
            //    // calcualte the start date with respect to the billing threshold
            //    if (stDate > DateTime.UtcNow.Date)
            //    {
            //        //stDate = stDate.AddDays(-1 * code.BillingThreshold);
            //        BilledDates bd = GetPatientLastBilledPeriods(patientId, patientProgramId, code.BillingCodeID, connectionString);
            //        stDate = bd.StartDate;
            //        endDate = bd.EndDate;
            //    }
            //    List<PatientInteraction> PatientInteractiontim = GetPatientInteractiontime(patientId, patientProgramId, connectionString).Where(s => s.Date >= stDate && s.Date <= endDate).ToList();
            //    if (PatientInteractiontim == null)
            //    {
            //        pbd.CPTCode = code.BillingCode;
            //        pbd.Completed = 0;
            //        pbd.Total = code.TargetReadings;
            //        pbd.IsTargetMet = false;
            //        pbd.ReadyTobill = 0;
            //        return pbd;
            //    }
            //    var isEstablishedExist = PatientInteractiontim.Where(s => s.IsCallNote == 1).ToList();
            //    if (isEstablishedExist.Count() > 0)
            //    {
            //        isEstablishedExist = isEstablishedExist.Where(s => s.IsEstablishedCall == 1).ToList();
            //        if (isEstablishedExist != null && isEstablishedExist.Count() == 0)
            //        {
            //            PatientInteractiontim.Clear();
            //        }
            //    }
            //    if (PatientInteractiontim.Count == 0 || PatientInteractiontim == null || isEstablishedExist.Count==0)
            //    {
            //        pbd.CPTCode = code.BillingCode;
            //        pbd.Completed = 0;
            //        pbd.Total = code.TargetReadings;
            //        pbd.IsTargetMet = false;
            //        pbd.ReadyTobill = 0;
            //        return pbd;
            //    }
            //    TotalReading = PatientInteractiontim.Sum(s => s.Duration);

            //    if (TotalReading>= code.TargetReadings * 60)
            //    {
            //        TotalReading = code.TargetReadings * 60;
            //    }
            //    pbd.BillingStartDate = stDate;
            //    pbd.CPTCode = code.BillingCode;
            //    pbd.Completed = TotalReading;
            //    pbd.Total = code.TargetReadings * 60;
            //    pbd.IsTargetMet = (TotalReading >= code.TargetReadings*60) ? true : false;
            //    pbd.ReadyTobill = (TotalReading >= (code.TargetReadings*60) && DaysCompleted >= code.BillingThreshold) ? 1 : 0;
            //    return pbd;
            //}
            //}
            //    catch (Exception ex)
            //    {
            //        throw;
            //    }
        }
        public PatientBilldata ProcessCPT99458_DaysBasedBilling(int patientId, int patientProgramId, BillingCodesDetails code,
                                                     BillingCodesDetails code457, string connectionString)
        {
            PatientBilldata pbd = new PatientBilldata();
            try
            {
                int DaysCompleted = 0; int TotalReading = 0;
                PatientStartDate PatientStartDate = new PatientStartDate();
                PatientStartDate = GetPatientBillingStartDateEx(patientId, patientProgramId, code.BillingCodeID, connectionString);
                if (PatientStartDate == null ||
                    PatientStartDate.Status.ToLower() == "invalid")
                {
                    pbd.CPTCode = code.BillingCode;
                    pbd.Completed = 0;
                    pbd.Total = code.TargetReadings*60;
                    pbd.IsTargetMet = false;
                    pbd.ReadyTobill = 0;
                    return pbd;
                }
                else if (PatientStartDate.StartDate > DateTime.UtcNow.Date && PatientStartDate.Status.ToLower() == "active")
                {
                    PatientBilldata PatientBilldata = new PatientBilldata();
                    PatientBilldata.CPTCode = code.BillingCode;
                    PatientBilldata.Completed = 0;
                    PatientBilldata.Total = code.TargetReadings*60;
                    PatientBilldata.IsTargetMet = false;
                    PatientBilldata.ReadyTobill = 0;
                    return PatientBilldata;
                }
                else
                {
                    //DateTime stDate = Convert.ToDateTime(PatientStartDate.StartDate);
                    DateTime stDate = DateTime.MinValue;
                    if (PatientStartDate.Status.ToLower() == "active")
                    {
                        stDate = Convert.ToDateTime(PatientStartDate.StartDate).AddDays(1);
                    }
                    else
                    {
                        stDate = Convert.ToDateTime(PatientStartDate.StartDate);
                    }
                    DateTime endDate = DateTime.UtcNow;
                    //Note: This will happen in the last day of billing.. In the last day, billed date will set in 
                    //the patientbilling table and +1 day will come as next start day.. So we should   
                    // calcualte the start date with respect to the billing threshold
                    DateTime today = DateTime.UtcNow;
                    DateTime endDateTempay = GetLocalTimeFromUTC((DateTime)today, connectionString);
                    if (stDate.Date > endDateTempay)
                    {
                        //stDate = stDate.AddDays(-1 * code.BillingThreshold);
                        BilledDates bd = GetPatientLastBilledPeriods(patientId, patientProgramId, code.BillingCodeID, connectionString);
                        if (bd == null)
                        {
                            PatientBilldata PatientBilldata = new PatientBilldata();
                            PatientBilldata.CPTCode = code.BillingCode;
                            PatientBilldata.Completed = 0;
                            PatientBilldata.Total = code.TargetReadings*60;
                            PatientBilldata.IsTargetMet = false;
                            PatientBilldata.ReadyTobill = 0;
                            return PatientBilldata;
                        }

                        stDate = bd.StartDate;
                        endDate = bd.EndDate;
                    }

                    if (PatientStartDate.Status.ToLower() == "billeddate")
                    {
                        stDate = GetUTCFromLocalTime((DateTime)stDate, connectionString);
                        endDate =DateTime.UtcNow;
                    }
                    List<PatientInteraction> PatientInteractiontim = GetPatientInteractiontime(patientId, patientProgramId, connectionString, stDate, endDate).ToList();
                    if (PatientInteractiontim == null)
                    {
                        pbd.CPTCode = code.BillingCode;
                        pbd.Completed = 0;
                        pbd.Total = code.TargetReadings*60;
                        pbd.IsTargetMet = false;
                        pbd.ReadyTobill = 0;
                        pbd.BillingStartDate = stDate;
                        return pbd;
                    }
                    var isEstablishedExist = PatientInteractiontim.Where(s => s.IsCallNote == 1).ToList();
                    if (isEstablishedExist.Count() > 0)
                    {
                        isEstablishedExist = isEstablishedExist.Where(s => s.IsEstablishedCall == 1).ToList();
                        if (isEstablishedExist != null && isEstablishedExist.Count() == 0)
                        {
                            PatientInteractiontim.Clear();
                        }
                    }
                    if (PatientInteractiontim.Count == 0 || PatientInteractiontim == null || isEstablishedExist.Count == 0)
                    {
                        pbd.CPTCode = code.BillingCode;
                        pbd.Completed = 0;
                        pbd.Total = code.TargetReadings*60;
                        pbd.IsTargetMet = false;
                        pbd.ReadyTobill = 0;
                        pbd.BillingStartDate = stDate;
                        return pbd;
                    }
                    TotalReading = PatientInteractiontim.Sum(s => s.Duration);

                    if (TotalReading > code457.TargetReadings * 60)
                    {
                        TimeSpan t1 = TimeSpan.FromSeconds(TotalReading);
                        TotalReading = (int)(t1.TotalSeconds - (code457.TargetReadings * 60));

                        pbd.BillingStartDate = endDate;
                        pbd.CPTCode = code.BillingCode;
                        pbd.Completed = TotalReading;
                        if (TotalReading < code457.TargetReadings * 60)
                        {
                            // base 20
                            pbd.Total = code457.TargetReadings * 60;
                            pbd.ReadyTobill = (TotalReading >= (code457.TargetReadings * 60) && DaysCompleted >= code.BillingThreshold) ? 1 : 0;
                        }
                        else
                        {
                            //base 40
                            pbd.Total = code.TargetReadings * 60;
                            pbd.ReadyTobill = (TotalReading >= (code.TargetReadings * 60) && DaysCompleted >= code.BillingThreshold) ? 1 : 0;
                        }
                        pbd.IsTargetMet = (TotalReading >= code457.TargetReadings * 60) ? true : false;
                    }
                    else
                    {
                        //Target reading 20
                        pbd.BillingStartDate = endDate;
                        pbd.CPTCode = code.BillingCode;
                        pbd.Completed = 0;
                        pbd.Total = code457.TargetReadings * 60;
                        pbd.IsTargetMet = (TotalReading >= code457.TargetReadings * 60) ? true : false;
                        pbd.ReadyTobill = (TotalReading >= (code457.TargetReadings * 60) && DaysCompleted >= code.BillingThreshold) ? 1 : 0;

                    }
                    return pbd;
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            //PatientBilldata pbd = new PatientBilldata();
            //try
            //{

            //    PatientBilldata PatientBilldatas = new PatientBilldata();
            //    List<PatientDailyBillingData> lstDailyData = GetPatientBillingCounts(patientId, patientProgramId, connectionString);
            //    if (lstDailyData == null || lstDailyData.Count == 0)
            //    {

            //        //PatientBilldatas.CPTCode = code.BillingCode;
            //        //PatientBilldatas.Completed = 0;
            //        //PatientBilldatas.Total = code.TargetReadings;
            //        //PatientBilldatas.IsTargetMet = false;
            //        //PatientBilldatas.ReadyTobill = 0;
            //        //return PatientBilldatas;
            //        PatientBilldatas=null;
            //    }

            //    foreach (PatientDailyBillingData pdb in lstDailyData)
            //    {
            //        if (pdb.BillingCodeId == code.BillingCodeID)
            //        {
            //            DateTime createdOn = pdb.CreatedOn;
            //            DateTime createdOnEnd = DateTime.UtcNow;
            //            List<PatientInteraction> PatientInteractiontim = GetPatientInteractiontime(patientId, patientProgramId, connectionString, createdOn, createdOnEnd).ToList();
            //            if (PatientInteractiontim != null && PatientInteractiontim.Count > 0)
            //            {
            //                var isEstablishedExist = PatientInteractiontim.Where(s => s.IsCallNote == 1).ToList();
            //                if (isEstablishedExist.Count() > 0)
            //                {
            //                    isEstablishedExist = isEstablishedExist.Where(s => s.IsEstablishedCall == 1).ToList();
            //                    if (isEstablishedExist != null && isEstablishedExist.Count() == 0)
            //                    {
            //                        PatientInteractiontim.Clear();
            //                    }
            //                }
            //                if (PatientInteractiontim != null && PatientInteractiontim.Count > 0)
            //                {
            //                    int TotalReading = PatientInteractiontim.Sum(s => s.Duration);
            //                    pdb.TotalDuration = pdb.TotalDuration + TotalReading;
            //                }

            //                //if (PatientInteractiontim.Count==0 || PatientInteractiontim==null || isEstablishedExist.Count==0)
            //                //{
            //                //    TotalReading = 0;
            //                //}

            //            }
            //            PatientBilldatas = new PatientBilldata();
            //            PatientBilldatas.BillingStartDate = (DateTime)pdb.StartDate;
            //            PatientBilldatas.CPTCode = code.BillingCode;
            //            PatientBilldatas.Completed = pdb.TotalDuration;
            //            PatientBilldatas.Total = code.TargetReadings;
            //            PatientBilldatas.IsTargetMet = (pdb.TotalDuration >= code.TargetReadings * 60) ? true : false;
            //            PatientBilldatas.ReadyTobill = (pdb.TotalDuration >= (code.TargetReadings * 60) && pdb.DaysCompleted >= code.BillingThreshold) ? 1 : 0;
            //            return PatientBilldatas;

            //        }
            //        else
            //        {
            //            PatientBilldatas = null;
            //        }
            //    }
            //    if (PatientBilldatas == null)
            //    {
            //        int DaysCompleted = 0; int TotalReading = 0;
            //        PatientStartDate PatientStartDate = new PatientStartDate();
            //        PatientStartDate = GetPatientBillingStartDateEx(patientId, patientProgramId, code.BillingCodeID, connectionString);
            //        if (PatientStartDate == null ||
            //            PatientStartDate.Status.ToLower() == "invalid")
            //        {
            //            PatientBilldatas = new PatientBilldata();
            //            PatientBilldatas = new PatientBilldata();
            //            PatientBilldatas.CPTCode = code.BillingCode;
            //            PatientBilldatas.Completed = 0;
            //            PatientBilldatas.Total = code.TargetReadings;
            //            PatientBilldatas.IsTargetMet = false;
            //            PatientBilldatas.ReadyTobill = 0;
            //            return PatientBilldatas;
            //        }
            //        else if (PatientStartDate.StartDate > DateTime.UtcNow.Date && PatientStartDate.Status.ToLower() == "active")
            //        {
            //            PatientBilldatas = new PatientBilldata();
            //            PatientBilldatas.CPTCode = code.BillingCode;
            //            PatientBilldatas.Completed = 0;
            //            PatientBilldatas.Total = code.TargetReadings;
            //            PatientBilldatas.IsTargetMet = false;
            //            PatientBilldatas.ReadyTobill = 0;
            //            return PatientBilldatas;
            //        }
            //        else
            //        {
            //            DateTime stDate = Convert.ToDateTime(PatientStartDate.StartDate);
            //            DateTime endDate = DateTime.UtcNow.Date;
            //            DateTime startDatetemp = GetLocalTimeFromUTC((DateTime)stDate, connectionString);
            //            DateTime endDatetemp = GetLocalTimeFromUTC((DateTime)endDate, connectionString);
            //            //Note: This will happen in the last day of billing.. In the last day, billed date will set in 
            //            //the patientbilling table and +1 day will come as next start day.. So we should   
            //            // calcualte the start date with respect to the billing threshold
            //            if (stDate > DateTime.UtcNow.Date)
            //            {
            //                //stDate = stDate.AddDays(-1 * code.BillingThreshold);
            //                BilledDates bd = GetPatientLastBilledPeriods(patientId, patientProgramId, code.BillingCodeID, connectionString);
            //                startDatetemp = bd.StartDate;
            //                endDatetemp = bd.EndDate;
            //            }


            //            DateTime startDateLoc = startDatetemp;
            //            DateTime enddateLoc = endDatetemp;
            //            int nTotDays = Math.Abs((startDatetemp - endDatetemp).Days) + 1;
            //            DaysCompleted = nTotDays;
            //            List<Dates> Dates = new List<Dates>();
            //            for (int i = 0; i <= DaysCompleted; i++)
            //            {
            //                if (i == 0)
            //                {
            //                    endDatetemp = startDateLoc.AddDays(1).Date.AddSeconds(-1);

            //                }
            //                else if (i == DaysCompleted)
            //                {
            //                    startDatetemp = startDateLoc.AddDays(i).Date;
            //                    endDatetemp = startDatetemp.AddDays(1).Date.AddSeconds(-1);
            //                }
            //                else
            //                {
            //                    startDatetemp = startDateLoc.AddDays(i).Date;
            //                    endDatetemp = startDatetemp.AddDays(1).Date.AddSeconds(-1);

            //                }

            //                startDatetemp = GetUTCFromLocalTime((DateTime)startDatetemp, connectionString);
            //                endDatetemp = GetUTCFromLocalTime((DateTime)endDatetemp, connectionString);


            //                Dates.Add(new Dates() { StartDate = (DateTime)startDatetemp, EndDate = endDatetemp, Totalreading = 0 });

            //            }
            //            List<PatientInteraction> PatientInteractiontimList = new List<PatientInteraction>();
            //            if (Dates.Count > 0)
            //            {

            //                foreach (Dates date in Dates)
            //                {

            //                    List<PatientInteraction> PatientInteractiontims = GetPatientInteractiontime(patientId, patientProgramId, connectionString, date.StartDate, date.EndDate).ToList();
            //                    //List<VitalReading> VitalReadings = billing.GetVitalReadings(patientProgramData, con).Where(s => s.ReadingDate >= date.StartDate && s.ReadingDate <= date.EndDate).ToList();

            //                    if (PatientInteractiontims != null && PatientInteractiontims.Count > 0)
            //                    {
            //                        foreach (var list in PatientInteractiontims)
            //                        {
            //                            PatientInteractiontimList.Add(list);
            //                        }

            //                    }

            //                }


            //            }








            //            //List<PatientInteraction> PatientInteractiontim = GetPatientInteractiontime(patientId, patientProgramId, connectionString).Where(s => s.Date >= stDate && s.Date <= endDate).ToList();
            //            if (PatientInteractiontimList == null)
            //            {
            //                PatientBilldatas = new PatientBilldata();
            //                PatientBilldatas.CPTCode = code.BillingCode;
            //                PatientBilldatas.Completed = 0;
            //                PatientBilldatas.Total = code.TargetReadings;
            //                PatientBilldatas.IsTargetMet = false;
            //                PatientBilldatas.ReadyTobill = 0;
            //                return PatientBilldatas;
            //            }
            //            var isEstablishedExist = PatientInteractiontimList.Where(s => s.IsCallNote == 1).ToList();
            //            if (isEstablishedExist.Count() > 0)
            //            {
            //                isEstablishedExist = isEstablishedExist.Where(s => s.IsEstablishedCall == 1).ToList();
            //                if (isEstablishedExist != null && isEstablishedExist.Count() == 0)
            //                {
            //                    PatientInteractiontimList.Clear();
            //                }
            //            }
            //            if (PatientInteractiontimList.Count == 0 || PatientInteractiontimList == null || isEstablishedExist.Count == 0)
            //            {
            //                PatientBilldatas = new PatientBilldata();
            //                PatientBilldatas.CPTCode = code.BillingCode;
            //                PatientBilldatas.Completed = 0;
            //                PatientBilldatas.Total = code.TargetReadings;
            //                PatientBilldatas.IsTargetMet = false;
            //                PatientBilldatas.ReadyTobill = 0;
            //                return PatientBilldatas;
            //            }
            //            TotalReading = PatientInteractiontimList.Sum(s => s.Duration);

            //            if (TotalReading > code457.TargetReadings * 60)
            //            {
            //                TimeSpan t1 = TimeSpan.FromSeconds(TotalReading);
            //                TotalReading = (int)(t1.TotalSeconds - (code457.TargetReadings * 60));
            //                PatientBilldatas = new PatientBilldata();
            //                PatientBilldatas.BillingStartDate = endDate;
            //                PatientBilldatas.CPTCode = code.BillingCode;
            //                PatientBilldatas.Completed = TotalReading;
            //                if (TotalReading < code457.TargetReadings * 60)
            //                {
            //                    // base 20
            //                    PatientBilldatas.Total = code457.TargetReadings * 60;
            //                    PatientBilldatas.ReadyTobill = (TotalReading >= (code457.TargetReadings * 60) && DaysCompleted >= code.BillingThreshold) ? 1 : 0;
            //                }
            //                else
            //                {
            //                    //base 40
            //                    PatientBilldatas.Total = code.TargetReadings * 60;
            //                    PatientBilldatas.ReadyTobill = (TotalReading >= (code.TargetReadings * 60) && DaysCompleted >= code.BillingThreshold) ? 1 : 0;
            //                }
            //                PatientBilldatas.IsTargetMet = (TotalReading >= code457.TargetReadings * 60) ? true : false;
            //            }
            //            else
            //            {
            //                PatientBilldatas = new PatientBilldata();
            //                //Target reading 20
            //                PatientBilldatas.BillingStartDate = stDate;
            //                PatientBilldatas.CPTCode = code.BillingCode;
            //                PatientBilldatas.Completed = 0;
            //                PatientBilldatas.Total = code457.TargetReadings * 60;
            //                PatientBilldatas.IsTargetMet = (TotalReading >= code457.TargetReadings * 60) ? true : false;
            //                PatientBilldatas.ReadyTobill = (TotalReading >= (code457.TargetReadings * 60) && DaysCompleted >= code.BillingThreshold) ? 1 : 0;

            //            }
            //            return PatientBilldatas;
            //        }
            //    }
            //    return PatientBilldatas;
            //int DaysCompleted = 0; int TotalReading = 0;
            //PatientStartDate PatientStartDate = new PatientStartDate();
            //PatientStartDate = GetPatientBillingStartDateEx(patientId, patientProgramId, code.BillingCodeID, connectionString);
            //if (PatientStartDate == null ||
            //    PatientStartDate.Status.ToLower() == "invalid")
            //{
            //    pbd.CPTCode = code.BillingCode;
            //    pbd.Completed = 0;
            //    pbd.Total = code.TargetReadings;
            //    pbd.IsTargetMet = false;
            //    pbd.ReadyTobill = 0;
            //    return pbd;
            //}
            //else if (PatientStartDate.StartDate > DateTime.UtcNow.Date && PatientStartDate.Status.ToLower() == "active")
            //{
            //    PatientBilldata PatientBilldata = new PatientBilldata();
            //    PatientBilldata.CPTCode = code.BillingCode;
            //    PatientBilldata.Completed = 0;
            //    PatientBilldata.Total = code.TargetReadings;
            //    PatientBilldata.IsTargetMet = false;
            //    PatientBilldata.ReadyTobill = 0;
            //    return PatientBilldata;
            //}
            //else
            //{
            //    DateTime stDate = Convert.ToDateTime(PatientStartDate.StartDate);
            //    DateTime endDate = DateTime.UtcNow.Date;
            //    //Note: This will happen in the last day of billing.. In the last day, billed date will set in 
            //    //the patientbilling table and +1 day will come as next start day.. So we should   
            //    // calcualte the start date with respect to the billing threshold
            //    if (stDate > DateTime.UtcNow.Date)
            //    {
            //        //stDate = stDate.AddDays(-1 * code.BillingThreshold);
            //        BilledDates bd = GetPatientLastBilledPeriods(patientId, patientProgramId, code.BillingCodeID, connectionString);
            //        stDate = bd.StartDate;
            //        endDate = bd.EndDate;
            //    }
            //    List<PatientInteraction> PatientInteractiontim = GetPatientInteractiontime(patientId, patientProgramId, connectionString).Where(s => s.Date >= stDate && s.Date <= endDate).ToList();
            //    if (PatientInteractiontim == null)
            //    {
            //        pbd.CPTCode = code.BillingCode;
            //        pbd.Completed = 0;
            //        pbd.Total = code.TargetReadings;
            //        pbd.IsTargetMet = false;
            //        pbd.ReadyTobill = 0;
            //        return pbd;
            //    }
            //    var isEstablishedExist = PatientInteractiontim.Where(s => s.IsCallNote == 1).ToList();
            //    if (isEstablishedExist.Count() > 0)
            //    {
            //        isEstablishedExist = isEstablishedExist.Where(s => s.IsEstablishedCall == 1).ToList();
            //        if (isEstablishedExist != null && isEstablishedExist.Count() == 0)
            //        {
            //            PatientInteractiontim.Clear();
            //        }
            //    }
            //    if (PatientInteractiontim.Count == 0 || PatientInteractiontim ==null || isEstablishedExist.Count==0)
            //    {
            //        pbd.CPTCode = code.BillingCode;
            //        pbd.Completed = 0;
            //        pbd.Total = code.TargetReadings;
            //        pbd.IsTargetMet = false;
            //        pbd.ReadyTobill = 0;
            //        return pbd;
            //    }
            //    TotalReading = PatientInteractiontim.Sum(s => s.Duration);

            //    if (TotalReading > code457.TargetReadings*60)
            //    {
            //        TimeSpan t1 = TimeSpan.FromSeconds(TotalReading);
            //        TotalReading = (int)(t1.TotalSeconds - (code457.TargetReadings * 60));

            //        pbd.BillingStartDate = endDate;
            //        pbd.CPTCode = code.BillingCode;
            //        pbd.Completed = TotalReading;
            //        if (TotalReading<code457.TargetReadings*60)
            //        {
            //            // base 20
            //            pbd.Total = code457.TargetReadings * 60;
            //            pbd.ReadyTobill = (TotalReading >= (code457.TargetReadings * 60) && DaysCompleted >= code.BillingThreshold) ? 1 : 0;
            //        }
            //        else
            //        {
            //            //base 40
            //            pbd.Total = code.TargetReadings * 60;
            //            pbd.ReadyTobill = (TotalReading >= (code.TargetReadings * 60) && DaysCompleted >= code.BillingThreshold) ? 1 : 0;
            //        }
            //        pbd.IsTargetMet = (TotalReading >= code457.TargetReadings * 60) ? true : false;
            //    }
            //    else
            //    {
            //        //Target reading 20
            //        pbd.BillingStartDate = endDate;
            //        pbd.CPTCode = code.BillingCode;
            //        pbd.Completed = 0;
            //        pbd.Total = code457.TargetReadings * 60;
            //        pbd.IsTargetMet = (TotalReading >= code457.TargetReadings * 60) ? true : false;
            //        pbd.ReadyTobill = (TotalReading >= (code457.TargetReadings * 60) && DaysCompleted >= code.BillingThreshold) ? 1 : 0;

            //    }
            //    return pbd;
            //}
            //}
            //catch (Exception ex)
            //{
            //    throw;
            //}
        }
        public PatientBilldata ProcessCPT99458_CycleBasedBilling(int patientId, int patientProgramId, BillingCodesDetails code,
                                                     BillingCodesDetails code457, string connectionString)
        {
            PatientBilldata pbd = new PatientBilldata();
            try
            {
                int DaysCompleted = 0; int TotalReading = 0;
                PatientStartDate PatientStartDate = new PatientStartDate();
                PatientStartDate = GetPatientBillingStartDateEx(patientId, patientProgramId, code.BillingCodeID, connectionString);
                if (PatientStartDate == null ||
                    PatientStartDate.Status.ToLower() == "invalid")
                {
                    pbd.CPTCode = code.BillingCode;
                    pbd.Completed = 0;
                    pbd.Total = code.TargetReadings*60;
                    pbd.IsTargetMet = false;
                    pbd.ReadyTobill = 0;
                    return pbd;
                }
                else if (PatientStartDate.StartDate > DateTime.UtcNow && PatientStartDate.Status.ToLower() == "active")
                {
                    PatientBilldata PatientBilldata = new PatientBilldata();
                    PatientBilldata.CPTCode = code.BillingCode;
                    PatientBilldata.Completed = 0;
                    PatientBilldata.Total = code.TargetReadings*60;
                    PatientBilldata.IsTargetMet = false;
                    PatientBilldata.ReadyTobill = 0;
                    return PatientBilldata;
                }
                else
                {
                    DateTime stDate = Convert.ToDateTime(PatientStartDate.StartDate);
                    DateTime endDate = DateTime.UtcNow;//.AddMinutes(10);
                    //Note: This will happen in the last day of billing.. In the last day, billed date will set in 
                    //the patientbilling table and +1 day will come as next start day.. So we should   
                    // calcualte the start date with respect to the billing threshold
                    DateTime today = DateTime.UtcNow;
                    DateTime endDateTempay = GetLocalTimeFromUTC((DateTime)today, connectionString);
                    if (stDate.Date > endDateTempay)
                    {
                        //stDate = stDate.AddDays(-1 * code.BillingThreshold);
                        BilledDates bd = GetPatientLastBilledPeriods(patientId, patientProgramId, code.BillingCodeID, connectionString);
                        if (bd == null)
                        {
                            PatientBilldata PatientBilldata = new PatientBilldata();
                            PatientBilldata.CPTCode = code.BillingCode;
                            PatientBilldata.Completed = 0;
                            PatientBilldata.Total = code.TargetReadings*60;
                            PatientBilldata.IsTargetMet = false;
                            PatientBilldata.ReadyTobill = 0;
                            return PatientBilldata;
                        }
                        stDate = bd.StartDate;
                        endDate = bd.EndDate;
                    }

                    if (PatientStartDate.Status.ToLower() == "billeddate")
                    {
                        stDate = GetUTCFromLocalTime((DateTime)stDate, connectionString);
                        endDate = DateTime.UtcNow;//.AddMinutes(10);
                    }
                    int patientSmsCount = 0;
                    List<PatientInteraction> PatientInteractiontim = GetPatientInteractiontime(patientId, patientProgramId, connectionString, stDate, endDate).ToList();
                    patientSmsCount =GetPatientSmsCount(patientId, connectionString, stDate, endDate);
                    if (PatientInteractiontim == null)
                    {
                        pbd.CPTCode = code.BillingCode;
                        pbd.Completed = 0;
                        pbd.Total = code.TargetReadings*60;
                        pbd.IsTargetMet = false;
                        pbd.ReadyTobill = 0;
                        pbd.BillingStartDate = stDate;
                        return pbd;
                    }
                    var isEstablishedExist = PatientInteractiontim.Where(s => s.IsCallNote == 1).ToList();
                    if (isEstablishedExist.Count() > 0)
                    {
                        isEstablishedExist = isEstablishedExist.Where(s => s.IsEstablishedCall == 1).ToList();
                        if (isEstablishedExist != null && isEstablishedExist.Count() == 0)
                        {
                            if (patientSmsCount <= 0)
                            {
                                PatientInteractiontim.Clear();
                            }

                        }
                    }
                    if (PatientInteractiontim.Count == 0 || PatientInteractiontim == null || isEstablishedExist.Count == 0)
                    {
                        if (patientSmsCount <= 0)
                        {
                            pbd.CPTCode = code.BillingCode;
                            pbd.Completed = 0;
                            pbd.Total = code.TargetReadings*60;
                            pbd.IsTargetMet = false;
                            pbd.ReadyTobill = 0;
                            pbd.BillingStartDate = stDate;
                            return pbd;
                        }
                    }
                    TotalReading = PatientInteractiontim.Sum(s => s.Duration);

                    if (TotalReading > code457.TargetReadings * 60)
                    {
                        TimeSpan t1 = TimeSpan.FromSeconds(TotalReading);
                        TotalReading = (int)(t1.TotalSeconds - (code457.TargetReadings * 60));

                        pbd.BillingStartDate = endDate;
                        pbd.CPTCode = code.BillingCode;
                        pbd.Completed = TotalReading;
                        if (TotalReading < code457.TargetReadings * 60)
                        {
                            // base 20
                            pbd.Total = code457.TargetReadings * 60;
                            pbd.ReadyTobill = (TotalReading >= (code457.TargetReadings * 60) && DaysCompleted >= code.BillingThreshold) ? 1 : 0;
                        }
                        else
                        {
                            //base 40
                            pbd.Total = code.TargetReadings * 60;
                            pbd.ReadyTobill = (TotalReading >= (code.TargetReadings * 60) && DaysCompleted >= code.BillingThreshold) ? 1 : 0;
                        }
                        pbd.IsTargetMet = (TotalReading >= code457.TargetReadings * 60) ? true : false;
                    }
                    else
                    {
                        //Target reading 20
                        pbd.BillingStartDate = endDate;
                        pbd.CPTCode = code.BillingCode;
                        pbd.Completed = 0;
                        pbd.Total = code457.TargetReadings * 60;
                        pbd.IsTargetMet = (TotalReading >= code457.TargetReadings * 60) ? true : false;
                        pbd.ReadyTobill = (TotalReading >= (code457.TargetReadings * 60) && DaysCompleted >= code.BillingThreshold) ? 1 : 0;

                    }
                    return pbd;
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            //PatientBilldata pbd = new PatientBilldata();
            //try
            //{

            //    PatientBilldata PatientBilldatas = new PatientBilldata();
            //    List<PatientDailyBillingData> lstDailyData = GetPatientBillingCounts(patientId, patientProgramId, connectionString);
            //    if (lstDailyData == null || lstDailyData.Count == 0)
            //    {

            //        //PatientBilldatas.CPTCode = code.BillingCode;
            //        //PatientBilldatas.Completed = 0;
            //        //PatientBilldatas.Total = code.TargetReadings;
            //        //PatientBilldatas.IsTargetMet = false;
            //        //PatientBilldatas.ReadyTobill = 0;
            //        //return PatientBilldatas;
            //        PatientBilldatas=null;
            //    }

            //    foreach (PatientDailyBillingData pdb in lstDailyData)
            //    {
            //        if (pdb.BillingCodeId == code.BillingCodeID)
            //        {
            //            DateTime createdOn = pdb.CreatedOn;
            //            DateTime createdOnEnd = DateTime.UtcNow;
            //            List<PatientInteraction> PatientInteractiontim = GetPatientInteractiontime(patientId, patientProgramId, connectionString, createdOn, createdOnEnd).ToList();
            //            if (PatientInteractiontim != null && PatientInteractiontim.Count > 0)
            //            {
            //                var isEstablishedExist = PatientInteractiontim.Where(s => s.IsCallNote == 1).ToList();
            //                if (isEstablishedExist.Count() > 0)
            //                {
            //                    isEstablishedExist = isEstablishedExist.Where(s => s.IsEstablishedCall == 1).ToList();
            //                    if (isEstablishedExist != null && isEstablishedExist.Count() == 0)
            //                    {
            //                        PatientInteractiontim.Clear();
            //                    }
            //                }
            //                if (PatientInteractiontim != null && PatientInteractiontim.Count > 0)
            //                {
            //                    int TotalReading = PatientInteractiontim.Sum(s => s.Duration);
            //                    pdb.TotalDuration = pdb.TotalDuration + TotalReading;
            //                }

            //                //if (PatientInteractiontim.Count==0 || PatientInteractiontim==null || isEstablishedExist.Count==0)
            //                //{
            //                //    TotalReading = 0;
            //                //}

            //            }
            //            PatientBilldatas = new PatientBilldata();
            //            PatientBilldatas.BillingStartDate = (DateTime)pdb.StartDate;
            //            PatientBilldatas.CPTCode = code.BillingCode;
            //            PatientBilldatas.Completed = pdb.TotalDuration;
            //            PatientBilldatas.Total = code.TargetReadings;
            //            PatientBilldatas.IsTargetMet = (pdb.TotalDuration >= code.TargetReadings * 60) ? true : false;
            //            PatientBilldatas.ReadyTobill = (pdb.TotalDuration >= (code.TargetReadings * 60) && pdb.DaysCompleted >= code.BillingThreshold) ? 1 : 0;
            //            return PatientBilldatas;

            //        }
            //        else
            //        {
            //            PatientBilldatas = null;
            //        }
            //    }
            //    if (PatientBilldatas == null)
            //    {
            //        int DaysCompleted = 0; int TotalReading = 0;
            //        PatientStartDate PatientStartDate = new PatientStartDate();
            //        PatientStartDate = GetPatientBillingStartDateEx(patientId, patientProgramId, code.BillingCodeID, connectionString);
            //        if (PatientStartDate == null ||
            //            PatientStartDate.Status.ToLower() == "invalid")
            //        {
            //            PatientBilldatas = new PatientBilldata();
            //            PatientBilldatas = new PatientBilldata();
            //            PatientBilldatas.CPTCode = code.BillingCode;
            //            PatientBilldatas.Completed = 0;
            //            PatientBilldatas.Total = code.TargetReadings;
            //            PatientBilldatas.IsTargetMet = false;
            //            PatientBilldatas.ReadyTobill = 0;
            //            return PatientBilldatas;
            //        }
            //        else if (PatientStartDate.StartDate > DateTime.UtcNow.Date && PatientStartDate.Status.ToLower() == "active")
            //        {
            //            PatientBilldatas = new PatientBilldata();
            //            PatientBilldatas.CPTCode = code.BillingCode;
            //            PatientBilldatas.Completed = 0;
            //            PatientBilldatas.Total = code.TargetReadings;
            //            PatientBilldatas.IsTargetMet = false;
            //            PatientBilldatas.ReadyTobill = 0;
            //            return PatientBilldatas;
            //        }
            //        else
            //        {
            //            DateTime stDate = Convert.ToDateTime(PatientStartDate.StartDate);
            //            DateTime endDate = DateTime.UtcNow.Date;
            //            DateTime startDatetemp = GetLocalTimeFromUTC((DateTime)stDate, connectionString);
            //            DateTime endDatetemp = GetLocalTimeFromUTC((DateTime)endDate, connectionString);
            //            //Note: This will happen in the last day of billing.. In the last day, billed date will set in 
            //            //the patientbilling table and +1 day will come as next start day.. So we should   
            //            // calcualte the start date with respect to the billing threshold
            //            if (stDate > DateTime.UtcNow.Date)
            //            {
            //                //stDate = stDate.AddDays(-1 * code.BillingThreshold);
            //                BilledDates bd = GetPatientLastBilledPeriods(patientId, patientProgramId, code.BillingCodeID, connectionString);
            //                startDatetemp = bd.StartDate;
            //                endDatetemp = bd.EndDate;
            //            }


            //            DateTime startDateLoc = startDatetemp;
            //            DateTime enddateLoc = endDatetemp;
            //            int nTotDays = Math.Abs((startDatetemp - endDatetemp).Days) + 1;
            //            DaysCompleted = nTotDays;
            //            List<Dates> Dates = new List<Dates>();
            //            for (int i = 0; i <= DaysCompleted; i++)
            //            {
            //                if (i == 0)
            //                {
            //                    endDatetemp = startDateLoc.AddDays(1).Date.AddSeconds(-1);

            //                }
            //                else if (i == DaysCompleted)
            //                {
            //                    startDatetemp = startDateLoc.AddDays(i).Date;
            //                    endDatetemp = startDatetemp.AddDays(1).Date.AddSeconds(-1);
            //                }
            //                else
            //                {
            //                    startDatetemp = startDateLoc.AddDays(i).Date;
            //                    endDatetemp = startDatetemp.AddDays(1).Date.AddSeconds(-1);

            //                }

            //                startDatetemp = GetUTCFromLocalTime((DateTime)startDatetemp, connectionString);
            //                endDatetemp = GetUTCFromLocalTime((DateTime)endDatetemp, connectionString);


            //                Dates.Add(new Dates() { StartDate = (DateTime)startDatetemp, EndDate = endDatetemp, Totalreading = 0 });

            //            }
            //            List<PatientInteraction> PatientInteractiontimList = new List<PatientInteraction>();
            //            if (Dates.Count > 0)
            //            {

            //                foreach (Dates date in Dates)
            //                {

            //                    List<PatientInteraction> PatientInteractiontims = GetPatientInteractiontime(patientId, patientProgramId, connectionString, date.StartDate, date.EndDate).ToList();
            //                    //List<VitalReading> VitalReadings = billing.GetVitalReadings(patientProgramData, con).Where(s => s.ReadingDate >= date.StartDate && s.ReadingDate <= date.EndDate).ToList();

            //                    if (PatientInteractiontims != null && PatientInteractiontims.Count > 0)
            //                    {
            //                        foreach (var list in PatientInteractiontims)
            //                        {
            //                            PatientInteractiontimList.Add(list);
            //                        }

            //                    }

            //                }


            //            }








            //            //List<PatientInteraction> PatientInteractiontim = GetPatientInteractiontime(patientId, patientProgramId, connectionString).Where(s => s.Date >= stDate && s.Date <= endDate).ToList();
            //            if (PatientInteractiontimList == null)
            //            {
            //                PatientBilldatas = new PatientBilldata();
            //                PatientBilldatas.CPTCode = code.BillingCode;
            //                PatientBilldatas.Completed = 0;
            //                PatientBilldatas.Total = code.TargetReadings;
            //                PatientBilldatas.IsTargetMet = false;
            //                PatientBilldatas.ReadyTobill = 0;
            //                return PatientBilldatas;
            //            }
            //            var isEstablishedExist = PatientInteractiontimList.Where(s => s.IsCallNote == 1).ToList();
            //            if (isEstablishedExist.Count() > 0)
            //            {
            //                isEstablishedExist = isEstablishedExist.Where(s => s.IsEstablishedCall == 1).ToList();
            //                if (isEstablishedExist != null && isEstablishedExist.Count() == 0)
            //                {
            //                    PatientInteractiontimList.Clear();
            //                }
            //            }
            //            if (PatientInteractiontimList.Count == 0 || PatientInteractiontimList == null || isEstablishedExist.Count == 0)
            //            {
            //                PatientBilldatas = new PatientBilldata();
            //                PatientBilldatas.CPTCode = code.BillingCode;
            //                PatientBilldatas.Completed = 0;
            //                PatientBilldatas.Total = code.TargetReadings;
            //                PatientBilldatas.IsTargetMet = false;
            //                PatientBilldatas.ReadyTobill = 0;
            //                return PatientBilldatas;
            //            }
            //            TotalReading = PatientInteractiontimList.Sum(s => s.Duration);

            //            if (TotalReading > code457.TargetReadings * 60)
            //            {
            //                TimeSpan t1 = TimeSpan.FromSeconds(TotalReading);
            //                TotalReading = (int)(t1.TotalSeconds - (code457.TargetReadings * 60));
            //                PatientBilldatas = new PatientBilldata();
            //                PatientBilldatas.BillingStartDate = endDate;
            //                PatientBilldatas.CPTCode = code.BillingCode;
            //                PatientBilldatas.Completed = TotalReading;
            //                if (TotalReading < code457.TargetReadings * 60)
            //                {
            //                    // base 20
            //                    PatientBilldatas.Total = code457.TargetReadings * 60;
            //                    PatientBilldatas.ReadyTobill = (TotalReading >= (code457.TargetReadings * 60) && DaysCompleted >= code.BillingThreshold) ? 1 : 0;
            //                }
            //                else
            //                {
            //                    //base 40
            //                    PatientBilldatas.Total = code.TargetReadings * 60;
            //                    PatientBilldatas.ReadyTobill = (TotalReading >= (code.TargetReadings * 60) && DaysCompleted >= code.BillingThreshold) ? 1 : 0;
            //                }
            //                PatientBilldatas.IsTargetMet = (TotalReading >= code457.TargetReadings * 60) ? true : false;
            //            }
            //            else
            //            {
            //                PatientBilldatas = new PatientBilldata();
            //                //Target reading 20
            //                PatientBilldatas.BillingStartDate = stDate;
            //                PatientBilldatas.CPTCode = code.BillingCode;
            //                PatientBilldatas.Completed = 0;
            //                PatientBilldatas.Total = code457.TargetReadings * 60;
            //                PatientBilldatas.IsTargetMet = (TotalReading >= code457.TargetReadings * 60) ? true : false;
            //                PatientBilldatas.ReadyTobill = (TotalReading >= (code457.TargetReadings * 60) && DaysCompleted >= code.BillingThreshold) ? 1 : 0;

            //            }
            //            return PatientBilldatas;
            //        }
            //    }
            //    return PatientBilldatas;
            //int DaysCompleted = 0; int TotalReading = 0;
            //PatientStartDate PatientStartDate = new PatientStartDate();
            //PatientStartDate = GetPatientBillingStartDateEx(patientId, patientProgramId, code.BillingCodeID, connectionString);
            //if (PatientStartDate == null ||
            //    PatientStartDate.Status.ToLower() == "invalid")
            //{
            //    pbd.CPTCode = code.BillingCode;
            //    pbd.Completed = 0;
            //    pbd.Total = code.TargetReadings;
            //    pbd.IsTargetMet = false;
            //    pbd.ReadyTobill = 0;
            //    return pbd;
            //}
            //else if (PatientStartDate.StartDate > DateTime.UtcNow.Date && PatientStartDate.Status.ToLower() == "active")
            //{
            //    PatientBilldata PatientBilldata = new PatientBilldata();
            //    PatientBilldata.CPTCode = code.BillingCode;
            //    PatientBilldata.Completed = 0;
            //    PatientBilldata.Total = code.TargetReadings;
            //    PatientBilldata.IsTargetMet = false;
            //    PatientBilldata.ReadyTobill = 0;
            //    return PatientBilldata;
            //}
            //else
            //{
            //    DateTime stDate = Convert.ToDateTime(PatientStartDate.StartDate);
            //    DateTime endDate = DateTime.UtcNow.Date;
            //    //Note: This will happen in the last day of billing.. In the last day, billed date will set in 
            //    //the patientbilling table and +1 day will come as next start day.. So we should   
            //    // calcualte the start date with respect to the billing threshold
            //    if (stDate > DateTime.UtcNow.Date)
            //    {
            //        //stDate = stDate.AddDays(-1 * code.BillingThreshold);
            //        BilledDates bd = GetPatientLastBilledPeriods(patientId, patientProgramId, code.BillingCodeID, connectionString);
            //        stDate = bd.StartDate;
            //        endDate = bd.EndDate;
            //    }
            //    List<PatientInteraction> PatientInteractiontim = GetPatientInteractiontime(patientId, patientProgramId, connectionString).Where(s => s.Date >= stDate && s.Date <= endDate).ToList();
            //    if (PatientInteractiontim == null)
            //    {
            //        pbd.CPTCode = code.BillingCode;
            //        pbd.Completed = 0;
            //        pbd.Total = code.TargetReadings;
            //        pbd.IsTargetMet = false;
            //        pbd.ReadyTobill = 0;
            //        return pbd;
            //    }
            //    var isEstablishedExist = PatientInteractiontim.Where(s => s.IsCallNote == 1).ToList();
            //    if (isEstablishedExist.Count() > 0)
            //    {
            //        isEstablishedExist = isEstablishedExist.Where(s => s.IsEstablishedCall == 1).ToList();
            //        if (isEstablishedExist != null && isEstablishedExist.Count() == 0)
            //        {
            //            PatientInteractiontim.Clear();
            //        }
            //    }
            //    if (PatientInteractiontim.Count == 0 || PatientInteractiontim ==null || isEstablishedExist.Count==0)
            //    {
            //        pbd.CPTCode = code.BillingCode;
            //        pbd.Completed = 0;
            //        pbd.Total = code.TargetReadings;
            //        pbd.IsTargetMet = false;
            //        pbd.ReadyTobill = 0;
            //        return pbd;
            //    }
            //    TotalReading = PatientInteractiontim.Sum(s => s.Duration);

            //    if (TotalReading > code457.TargetReadings*60)
            //    {
            //        TimeSpan t1 = TimeSpan.FromSeconds(TotalReading);
            //        TotalReading = (int)(t1.TotalSeconds - (code457.TargetReadings * 60));

            //        pbd.BillingStartDate = endDate;
            //        pbd.CPTCode = code.BillingCode;
            //        pbd.Completed = TotalReading;
            //        if (TotalReading<code457.TargetReadings*60)
            //        {
            //            // base 20
            //            pbd.Total = code457.TargetReadings * 60;
            //            pbd.ReadyTobill = (TotalReading >= (code457.TargetReadings * 60) && DaysCompleted >= code.BillingThreshold) ? 1 : 0;
            //        }
            //        else
            //        {
            //            //base 40
            //            pbd.Total = code.TargetReadings * 60;
            //            pbd.ReadyTobill = (TotalReading >= (code.TargetReadings * 60) && DaysCompleted >= code.BillingThreshold) ? 1 : 0;
            //        }
            //        pbd.IsTargetMet = (TotalReading >= code457.TargetReadings * 60) ? true : false;
            //    }
            //    else
            //    {
            //        //Target reading 20
            //        pbd.BillingStartDate = endDate;
            //        pbd.CPTCode = code.BillingCode;
            //        pbd.Completed = 0;
            //        pbd.Total = code457.TargetReadings * 60;
            //        pbd.IsTargetMet = (TotalReading >= code457.TargetReadings * 60) ? true : false;
            //        pbd.ReadyTobill = (TotalReading >= (code457.TargetReadings * 60) && DaysCompleted >= code.BillingThreshold) ? 1 : 0;

            //    }
            //    return pbd;
            //}
            //}
            //catch (Exception ex)
            //{
            //    throw;
            //}
        }
        public List<PatientInteraction> GetPatientInteractiontime(int patientid, int patientprogramid, string connectionString)
        {

            try
            {
                //string ConnectionString = ConfigurationManager.ConnectionStrings[0].ToString();
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();

                    using (SqlCommand command = new SqlCommand("usp_GetPatientCallOrReviewTime", con))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@patientProgramId", patientprogramid);
                        command.Parameters.AddWithValue("@patientId", patientid);
                        SqlParameter returnParameter = command.Parameters.Add("RetVal", SqlDbType.Int);
                        returnParameter.Direction = ParameterDirection.ReturnValue;
                        SqlDataReader reader = command.ExecuteReader();

                        List<PatientInteraction> PatientInteractiontime = new List<PatientInteraction>();
                        List<int> roleids = new List<int>();
                        while (reader.Read())
                        {
                            PatientInteraction PatientInteractiontimes = new PatientInteraction();
                            PatientInteractiontimes.Duration = Convert.ToInt32(reader["Duration"]);
                            PatientInteractiontimes.Date = Convert.ToDateTime(reader["Date"]);
                            PatientInteractiontimes.IsCallNote = Convert.ToInt32(reader["IsCallNote"]);
                            PatientInteractiontimes.IsEstablishedCall = Convert.ToInt32(reader["IsEstablishedCall"]);
                            PatientInteractiontime.Add(PatientInteractiontimes);
                        }

                        if (reader.FieldCount == 0)
                        {
                            con.Close();
                            return null;
                        }
                        con.Close();
                        return PatientInteractiontime;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception occured - class Name :" + this.GetType().Name + ",  Method Name : " + MethodBase.GetCurrentMethod().Name + ", Error Message :" + ex.Message + "");
                return null;
            }



        }
        public DateTime? GetPatientBillingStartDate(int patientId, int patientProgramid, int billingCodeId, string connectionString)
        {
            try
            {
                //string ConnectionString = ConfigurationManager.ConnectionStrings["RPM"].ConnectionString;
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();

                    using (SqlCommand command = new SqlCommand("usp_GetPatientBillingStartDate", con))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@PatientId", patientId);
                        command.Parameters.AddWithValue("@PatientProgramId", patientProgramid);
                        command.Parameters.AddWithValue("@BillingCodeId", billingCodeId);
                        SqlParameter returnParameter = command.Parameters.Add("RetVal", SqlDbType.Int);
                        returnParameter.Direction = ParameterDirection.ReturnValue;
                        var result = command.ExecuteScalar();
                        con.Close();
                        return result as DateTime?;

                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception occured - class Name :" + this.GetType().Name + ",  Method Name : " + MethodBase.GetCurrentMethod().Name + ", Error Message :" + ex.Message + "");
                return null;
            }
        }
        public List<VitalReading> GetVitalReadings(int patientProgramId, string connectionString)
        {

            try
            {
                //string ConnectionString = ConfigurationManager.ConnectionStrings[0].ToString();
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();

                    using (SqlCommand command = new SqlCommand("usp_GetVitalReadings", con))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@patientProgramId", patientProgramId);
                        SqlParameter returnParameter = command.Parameters.Add("RetVal", SqlDbType.Int);
                        returnParameter.Direction = ParameterDirection.ReturnValue;
                        SqlDataReader reader = command.ExecuteReader();

                        List<VitalReading> VitalReadingList = new List<VitalReading>();
                        List<int> roleids = new List<int>();
                        while (reader.Read())
                        {
                            VitalReading VitalReading = new VitalReading();
                            VitalReading.ReadingDate = Convert.ToDateTime(reader["ReadingDate"]);
                            VitalReading.Totalreadings = Convert.ToInt32(reader["Totalreadings"]);

                            VitalReadingList.Add(VitalReading);
                        }

                        if (reader.FieldCount == 0)
                        {
                            con.Close();
                            return null;
                        }
                        con.Close();
                        return VitalReadingList;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception occured - class Name :" + this.GetType().Name + ",  Method Name : " + MethodBase.GetCurrentMethod().Name + ", Error Message :" + ex.Message + "");
                return null;
            }



        }
        public DateTime? GetBillingStartDate(int patientId, int patientprogramid, int billingCodeId, string connectionString)
        {
            try
            {
                //string ConnectionString = ConfigurationManager.ConnectionStrings["RPM"].ConnectionString;
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();

                    using (SqlCommand command = new SqlCommand("usp_GetBillingStartDates", con))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@PatientId", patientId);
                        command.Parameters.AddWithValue("@PatientProgramId", patientprogramid);
                        command.Parameters.AddWithValue("@BillingCodeId", billingCodeId);
                        SqlParameter returnParameter = command.Parameters.Add("RetVal", SqlDbType.Int);
                        returnParameter.Direction = ParameterDirection.ReturnValue;
                        var result = command.ExecuteScalar();
                        con.Close();
                        return result as DateTime?;

                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception occured - class Name :" + this.GetType().Name + ",  Method Name : " + MethodBase.GetCurrentMethod().Name + ", Error Message :" + ex.Message + "");
                return null;
            }
        }
        public PatientStartDate GetBillingStartDateEx(int patientId, int patientprogramid, int billingCodeId, string connectionString)
        {
            try
            {
                //string ConnectionString = ConfigurationManager.ConnectionStrings["RPM"].ConnectionString;
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();

                    using (SqlCommand command = new SqlCommand("usp_GetBillingStartDates", con))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@PatientId", patientId);
                        command.Parameters.AddWithValue("@PatientProgramId", patientprogramid);
                        command.Parameters.AddWithValue("@BillingCodeId", billingCodeId);
                        SqlParameter returnParameter = command.Parameters.Add("RetVal", SqlDbType.Int);
                        returnParameter.Direction = ParameterDirection.ReturnValue;
                        SqlDataReader reader = command.ExecuteReader();
                        PatientStartDate PatientStartDate = new PatientStartDate();
                        while (reader.Read())
                        {
                            if (DBNull.Value.Equals(reader["BillingStartDate"]))
                            {
                                PatientStartDate.StartDate = null;
                            }
                            else
                            {
                                PatientStartDate.StartDate = Convert.ToDateTime(reader["BillingStartDate"]);
                            }

                            PatientStartDate.Status = Convert.ToString(reader["Status"]);
                        }
                        con.Close();
                        return PatientStartDate;

                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception occured - class Name :" + this.GetType().Name + ",  Method Name : " + MethodBase.GetCurrentMethod().Name + ", Error Message :" + ex.Message + "");
                return null;
            }
        }
        public PatientStartDate GetPatientBillingStartDateEx(int patientId, int patientprogramid, int billingCodeId, string connectionString)
        {
            try
            {
                //string ConnectionString = ConfigurationManager.ConnectionStrings["RPM"].ConnectionString;
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();

                    using (SqlCommand command = new SqlCommand("usp_GetPatientBillingStartDate", con))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@PatientId", patientId);
                        command.Parameters.AddWithValue("@PatientProgramId", patientprogramid);
                        command.Parameters.AddWithValue("@BillingCodeId", billingCodeId);
                        SqlParameter returnParameter = command.Parameters.Add("RetVal", SqlDbType.Int);
                        returnParameter.Direction = ParameterDirection.ReturnValue;
                        SqlDataReader reader = command.ExecuteReader();
                        PatientStartDate PatientStartDate = new PatientStartDate();
                        while (reader.Read())
                        {
                            if (DBNull.Value.Equals(reader["BillingStartDate"]))
                            {
                                PatientStartDate.StartDate = null;
                            }
                            else
                            {
                                PatientStartDate.StartDate = Convert.ToDateTime(reader["BillingStartDate"]);
                            }

                            PatientStartDate.Status = Convert.ToString(reader["Status"]);
                        }

                        con.Close();
                        return PatientStartDate;

                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception occured - class Name :" + this.GetType().Name + ",  Method Name : " + MethodBase.GetCurrentMethod().Name + ", Error Message :" + ex.Message + "");
                return null;
            }
        }
        public List<BillingCodesDetails> GetBillingCodeDetails(string connectionString)
        {

            try
            {
                //string ConnectionString = ConfigurationManager.ConnectionStrings[0].ToString();
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();

                    using (SqlCommand command = new SqlCommand("usp_GetBillingCodeDetails", con))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        SqlParameter returnParameter = command.Parameters.Add("RetVal", SqlDbType.Int);
                        returnParameter.Direction = ParameterDirection.ReturnValue;
                        SqlDataReader reader = command.ExecuteReader();

                        List<BillingCodesDetails> billingCodesList = new List<BillingCodesDetails>();
                        List<int> roleids = new List<int>();
                        while (reader.Read())
                        {
                            BillingCodesDetails BillingCodes = new BillingCodesDetails();
                            BillingCodes.BillingCodeID = Convert.ToInt32(reader["Id"]);
                            BillingCodes.BillingCode = Convert.ToString(reader["BillingCode"]);
                            BillingCodes.Frequency = Convert.ToInt32(reader["Frequency"]);
                            BillingCodes.FrequencyPeriod = Convert.ToString(reader["FrequencyPeriod"]);
                            BillingCodes.BillingThreshold = Convert.ToInt32(reader["BillingThreshold"]);
                            BillingCodes.BillingPeriod = Convert.ToString(reader["BillingPeriod"]);
                            BillingCodes.TargetReadings = Convert.ToInt32(reader["TargetReadings"]);
                            billingCodesList.Add(BillingCodes);
                        }

                        if (reader.FieldCount == 0)
                        {
                            con.Close();
                            return null;
                        }
                        con.Close();
                        return billingCodesList;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception occured - class Name :" + this.GetType().Name + ",  Method Name : " + MethodBase.GetCurrentMethod().Name + ", Error Message :" + ex.Message + "");
                return null;
            }



        }
        public List<BillingCodesDetails> GetBillingCodeDetailsByProgram(string connectionString, string programName)
        {

            try
            {
                //string ConnectionString = ConfigurationManager.ConnectionStrings[0].ToString();
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();

                    using (SqlCommand command = new SqlCommand("usp_GetBillingCodeDetailsByProgram", con))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@ProgramName", programName);
                        SqlParameter returnParameter = command.Parameters.Add("RetVal", SqlDbType.Int);
                        returnParameter.Direction = ParameterDirection.ReturnValue;
                        SqlDataReader reader = command.ExecuteReader();

                        List<BillingCodesDetails> billingCodesList = new List<BillingCodesDetails>();
                        List<int> roleids = new List<int>();
                        while (reader.Read())
                        {
                            BillingCodesDetails BillingCodes = new BillingCodesDetails();
                            BillingCodes.BillingCodeID = Convert.ToInt32(reader["Id"]);
                            BillingCodes.BillingCode = Convert.ToString(reader["BillingCode"]);
                            BillingCodes.Frequency = Convert.ToInt32(reader["Frequency"]);
                            BillingCodes.FrequencyPeriod = Convert.ToString(reader["FrequencyPeriod"]);
                            BillingCodes.BillingThreshold = Convert.ToInt32(reader["BillingThreshold"]);
                            BillingCodes.BillingPeriod = Convert.ToString(reader["BillingPeriod"]);
                            BillingCodes.TargetReadings = Convert.ToInt32(reader["TargetReadings"]);
                            billingCodesList.Add(BillingCodes);
                        }

                        if (reader.FieldCount == 0)
                        {
                            con.Close();
                            return null;
                        }
                        con.Close();
                        return billingCodesList;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception occured - class Name :" + this.GetType().Name + ",  Method Name : " + MethodBase.GetCurrentMethod().Name + ", Error Message :" + ex.Message + "");
                return null;
            }



        }
        public List<searchPatient> SearchPatient(int RoleId, string UserName, string ConnectionString)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(ConnectionString))
                {
                    con.Open();

                    using (SqlCommand command = new SqlCommand("usp_GetTeamPatients", con))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@CreatedBy", UserName);
                        command.Parameters.AddWithValue("@RoleId", RoleId);
                        SqlParameter returnParameter = command.Parameters.Add("RetVal", SqlDbType.Int);
                        returnParameter.Direction = ParameterDirection.ReturnValue;
                        SqlDataReader reader = command.ExecuteReader();
                        List<searchPatient> list = new List<searchPatient>();
                        while (reader.Read())
                        {
                            searchPatient ret = new searchPatient();
                            ret.PatientId = Convert.ToInt32(reader["PatientId"]);
                            ret.PatientProgramId = Convert.ToInt32(reader["PatientProgramId"]);
                            ret.PatientNumber = reader["PatientNumber"].ToString();
                            ret.PatientName = reader["PatientName"].ToString();
                            ret.ProgramName = reader["ProgramName"].ToString();
                            list.Add(ret);
                        }
                        if (reader.FieldCount == 0)
                        {
                            return null;
                        }
                        return list;
                    }
                }
            }
            catch (Exception ex)
            {


                throw;
            }
        }
        public List<string> GetBillingCodes(string connectionString)
        {

            try
            {
                //string ConnectionString = ConfigurationManager.ConnectionStrings[0].ToString();
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();

                    using (SqlCommand command = new SqlCommand("usp_GetBillingCodeDetails", con))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        SqlParameter returnParameter = command.Parameters.Add("RetVal", SqlDbType.Int);
                        returnParameter.Direction = ParameterDirection.ReturnValue;
                        SqlDataReader reader = command.ExecuteReader();

                        List<BillingCodesDetails> billingCodesList = new List<BillingCodesDetails>();
                        List<string> BillingCodes = new List<string>();
                        while (reader.Read())
                        {

                            BillingCodes.Add(Convert.ToString(reader["BillingCode"]));

                        }
                        if (reader.FieldCount == 0)
                        {
                            con.Close();
                            return null;
                        }
                        con.Close();
                        return BillingCodes;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception occured - class Name :" + this.GetType().Name + ",  Method Name : " + MethodBase.GetCurrentMethod().Name + ", Error Message :" + ex.Message + "");
                return null;
            }



        }

        public List<Programs> GetAllPrograms(string connectionString)
        {

            try
            {
                //string ConnectionString = ConfigurationManager.ConnectionStrings[0].ToString();
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();

                    using (SqlCommand command = new SqlCommand("usp_GetAllPrograms", con))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        SqlParameter returnParameter = command.Parameters.Add("RetVal", SqlDbType.Int);
                        returnParameter.Direction = ParameterDirection.ReturnValue;
                        SqlDataReader reader = command.ExecuteReader();

                        List<Programs> programlist = new List<Programs>();


                        while (reader.Read())
                        {
                            Programs pg = new Programs();
                            pg.Name = reader["Name"].ToString();
                            pg.ProgramType=reader["ProgramName"].ToString();

                            programlist.Add(pg);


                        }

                        if (reader.FieldCount == 0)
                        {
                            con.Close();
                            return null;
                        }
                        con.Close();
                        return programlist;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception occured - class Name :" + this.GetType().Name + ",  Method Name : " + MethodBase.GetCurrentMethod().Name + ", Error Message :" + ex.Message + "");
                return null;
            }



        }

        public List<ProgramVitalDignostics> GetDiagnosisCodeByVitalId(string connectionString, vitalIdsList vitalId)
        {

            try
            {
                List<ProgramVitalDignostics> lstVD = new List<ProgramVitalDignostics>();

                if (vitalId.VitalIds.Count() > 0)
                {

                    foreach (int id in vitalId.VitalIds)
                    {

                        using (SqlConnection con = new SqlConnection(connectionString))
                        {
                            con.Open();

                            using (SqlCommand command = new SqlCommand("GetDiagnosisCodeByVitalId", con))
                            {
                                command.CommandType = CommandType.StoredProcedure;
                                command.Parameters.AddWithValue("@VitalId", id);
                                SqlParameter returnParameter = command.Parameters.Add("RetVal", SqlDbType.Int);
                                returnParameter.Direction = ParameterDirection.ReturnValue;
                                SqlDataReader reader = command.ExecuteReader();

                                List<Programs> programlist = new List<Programs>();

                                //DiagnosisList dl = new DiagnosisList();
                                //dl.DiagnosisCodes = new List<string>();
                                //List<ProgramVitalDignostics> lstVD = new List<ProgramVitalDignostics>();
                                //ProgramVitalDignostics codes = new ProgramVitalDignostics();
                                while (reader.Read())
                                {
                                    ProgramVitalDignostics codes = new ProgramVitalDignostics();
                                    codes.DiagnosisCode = reader["Code"].ToString();
                                    codes.DiagnosisName = reader["Name"].ToString();
                                    lstVD.Add(codes);


                                }



                                if (reader.FieldCount == 0)
                                {
                                    con.Close();
                                    return null;
                                }
                                con.Close();

                            }
                        }

                    }


                }
                //return lstVD;
                return lstVD = lstVD.GroupBy(x => x.DiagnosisCode).Select(x => x.First()).ToList();
                //string ConnectionString = ConfigurationManager.ConnectionStrings[0].ToString();

            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception occured - class Name :" + this.GetType().Name + ",  Method Name : " + MethodBase.GetCurrentMethod().Name + ", Error Message :" + ex.Message + "");
                return null;
            }
        }
        public PatientBilldataList GetPatientBillingDataList(string patientType, string patientFilter, string patientId, string patientName, string program, string assignedmember, int Index, string readingFilter, string interactionFilter, int RoleId, string Createdby, string ConnectionString, string ProgramType)
        {
            List<SystemConfigInfo> lstConfig = DalCommon.GetSystemConfig(ConnectionString, "Billing", "User");
            SystemConfigInfo provider = lstConfig.Find(x => x.Name.Equals("BillingType"));
            PatientBilldataList patientBilldataList = new PatientBilldataList();
            if (lstConfig.Count > 0)
            {
                if (provider.Value == "30days")
                {
                    if (ProgramType.ToUpper()=="RPM")
                    {
                        patientBilldataList = GetPatientBillingDataListDaysBasedBilling(patientType, patientFilter, patientId, patientName, program, assignedmember, Index, readingFilter, interactionFilter, RoleId, Createdby, ConnectionString);

                    }
                    else if (ProgramType.ToUpper()=="CCM")
                    {
                        patientBilldataList = GetPatientBillingDataListCCMDaysBasedBilling(patientType, patientFilter, patientId, patientName, program, assignedmember, Index, readingFilter, interactionFilter, RoleId, Createdby, ConnectionString);
                    }
                    else if (ProgramType.ToUpper()=="PCM")
                    {
                        patientBilldataList = GetPatientBillingDataListPCMDaysBasedBilling(patientType, patientFilter, patientId, patientName, program, assignedmember, Index, readingFilter, interactionFilter, RoleId, Createdby, ConnectionString);
                    }
                }
                else if (provider.Value == "cycle")
                {
                    if (ProgramType.ToUpper()=="RPM")
                    {
                        patientBilldataList = GetPatientBillingDataListCycleBasedBilling(patientType, patientFilter, patientId, patientName, program, assignedmember, Index, readingFilter, interactionFilter, RoleId, Createdby, ConnectionString);
                    }
                    else if (ProgramType.ToUpper()=="CCM")
                    {
                        patientBilldataList = GetPatientBillingDataListCCMCycleBasedBilling(patientType, patientFilter, patientId, patientName, program, assignedmember, Index, readingFilter, interactionFilter, RoleId, Createdby, ConnectionString);
                    }
                    else if (ProgramType.ToUpper()=="PCM")
                    {
                        patientBilldataList = GetPatientBillingDataListPCMCycleBasedBilling(patientType, patientFilter, patientId, patientName, program, assignedmember, Index, readingFilter, interactionFilter, RoleId, Createdby, ConnectionString);
                    }
                }
                else
                {
                    patientBilldataList = GetPatientBillingDataListCycleBasedBilling(patientType, patientFilter, patientId, patientName, program, assignedmember, Index, readingFilter, interactionFilter, RoleId, Createdby, ConnectionString);
                }
            }
            else
            {
                patientBilldataList = GetPatientBillingDataListCycleBasedBilling(patientType, patientFilter, patientId, patientName, program, assignedmember, Index, readingFilter, interactionFilter, RoleId, Createdby, ConnectionString);
            }
            return patientBilldataList;
        }

        public PatientBilldataList GetPatientBillingDataListCycleBasedBilling(string patientType, string patientFilter, string patientId, string patientName, string program, string assignedmember, int Index, string readingFilter, string interactionFilter, int RoleId, string Createdby, string ConnectionString)
        {
            try
            {

                using (SqlConnection con = new SqlConnection(ConnectionString))
                {
                    con.Open();

                    using (SqlCommand command = new SqlCommand("GetPatientBillingDataList_CycleBasedBilling", con))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.Add("@patientType", SqlDbType.NVarChar).Value = patientType;
                        command.Parameters.Add("@patientFilter", SqlDbType.NVarChar).Value = patientFilter ?? String.Empty;
                        command.Parameters.Add("@patientId", SqlDbType.NVarChar).Value = patientId ?? String.Empty;
                        command.Parameters.Add("@patientName", SqlDbType.NVarChar).Value = patientName ?? String.Empty;
                        command.Parameters.Add("@program", SqlDbType.NVarChar).Value = program;
                        command.Parameters.Add("@assignedmember", SqlDbType.NVarChar).Value = assignedmember ?? String.Empty;
                        command.Parameters.Add("@index", SqlDbType.Int).Value = Index;
                        command.Parameters.Add("@readingFilter", SqlDbType.NVarChar).Value = readingFilter;
                        command.Parameters.Add("@InteractionFilter", SqlDbType.NVarChar).Value = interactionFilter;
                        command.Parameters.Add("@Createdby", SqlDbType.NVarChar).Value = Createdby;
                        command.Parameters.Add("@RoleId", SqlDbType.Int).Value = RoleId;
                        SqlDataReader reader = command.ExecuteReader();

                        List<PatientBilldataList> PatientBilldateList = new List<PatientBilldataList>();
                        List<int> roleids = new List<int>();
                        TotalCounts counts = new TotalCounts();
                        PatientBilldataList PatientBilldata = new PatientBilldataList();
                        PatientBilldata.PatientBilldataRecords = new List<PatientBilldataRecords>();

                        while (reader.Read())
                        {
                            bool val = false;
                            for (int i = 0; i < reader.FieldCount; i++)
                            {

                                if (reader.GetName(i).Equals("PatientNumber", StringComparison.InvariantCultureIgnoreCase))
                                    val = true;
                            }
                            if (val)
                            {
                                PatientBilldataRecords patientBilldataRecords = new PatientBilldataRecords();


                                patientBilldataRecords.PatientNumber = reader["PatientNumber"].ToString();
                                patientBilldataRecords.patientId = Convert.ToInt32(reader["patientId"]);
                                patientBilldataRecords.PatientProgramId = Convert.ToInt32(reader["PatientProgramId"]);
                                patientBilldataRecords.PatientName = Convert.ToString(reader["FirstName"].ToString());
                                patientBilldataRecords.Program = Convert.ToString(reader["Name"].ToString());
                                patientBilldataRecords.AssignedMemeber = Convert.ToString(reader["Assignedmember"]) + ' ' + Convert.ToString(reader["AssignedLastName"]);
                                patientBilldataRecords.TortalVitalCount = Convert.ToInt32(reader["totalVitalCount"]);
                                patientBilldataRecords.interaction = Convert.ToInt32(reader["TotalDuration"]);
                                patientBilldataRecords.DaysCompleted = Convert.ToInt32(reader["DaysCompleted"]);
                                patientBilldataRecords.ClinicName = Convert.ToString(reader["ClinicName"]);
                                patientBilldataRecords.Code = Convert.ToString(reader["Code"]);
                                patientBilldataRecords.NextBillingDate = null;
                                counts.Total = Convert.ToInt32(reader["Total"]);
                                counts.TotalCount = Convert.ToInt32(reader["TotalCount"]);
                                counts.C1Total = Convert.ToInt32(reader["C1Total"]);
                                counts.C2Total = Convert.ToInt32(reader["C2Total"]);
                                counts.TodayCount = null;
                                counts.Next7DaysCount = null;

                                PatientBilldata.PatientBilldataRecords.Add(patientBilldataRecords);
                            }
                            else
                            {
                                counts.Total = Convert.ToInt32(reader["Total"]);
                                counts.TotalCount = Convert.ToInt32(reader["TotalCount"]);
                                counts.C1Total = Convert.ToInt32(reader["C1Total"]);
                                counts.C2Total = Convert.ToInt32(reader["C2Total"]);
                                counts.TodayCount = null;
                                counts.Next7DaysCount = null;
                            }

                        }
                        PatientBilldata.TotalCounts = counts;
                        // PatientBilldateList.Add(PatientBilldata);
                        //if (reader.FieldCount == 0)
                        //{
                        //    con.Close();
                        //    return null;
                        //}
                        con.Close();
                        return PatientBilldata;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception occured - class Name :" + this.GetType().Name + ",  Method Name : " + MethodBase.GetCurrentMethod().Name + ", Error Message :" + ex.Message + "");
                return null;
            }



        }

        public PatientBilldataList GetPatientBillingDataListCCMCycleBasedBilling(string patientType, string patientFilter, string patientId, string patientName, string program, string assignedmember, int Index, string readingFilter, string interactionFilter, int RoleId, string Createdby, string ConnectionString)
        {
            try
            {
                double startDuration;
                double endDuration;
                if (interactionFilter == "0-19")
                {
                    TimeSpan t1 = TimeSpan.FromMinutes(19.99);
                    startDuration = 0;
                    endDuration=t1.TotalSeconds;
                }
                else if (interactionFilter == "20-39")
                {
                    TimeSpan t1 = TimeSpan.FromMinutes(20);
                    TimeSpan t2 = TimeSpan.FromMinutes(39.99);
                    startDuration = t1.TotalSeconds;
                    endDuration=t2.TotalSeconds;
                }
                else if (interactionFilter == "40-59")
                {
                    TimeSpan t1 = TimeSpan.FromMinutes(40);
                    TimeSpan t2 = TimeSpan.FromMinutes(59.99);
                    startDuration = t1.TotalSeconds;
                    endDuration=t2.TotalSeconds;
                }
                else if (interactionFilter == "60")
                {
                    TimeSpan t1 = TimeSpan.FromMinutes(60);
                    startDuration = t1.TotalSeconds;
                    endDuration=0;
                }
                else
                {
                    startDuration = 0;
                    endDuration=0;
                }
                using (SqlConnection con = new SqlConnection(ConnectionString))
                {
                    con.Open();

                    using (SqlCommand command = new SqlCommand("GetPatientBillingDataListCCM_CycleBasedBilling", con))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.Add("@patientType", SqlDbType.NVarChar).Value = patientType;
                        command.Parameters.Add("@patientFilter", SqlDbType.NVarChar).Value = patientFilter ?? String.Empty;
                        command.Parameters.Add("@patientId", SqlDbType.NVarChar).Value = patientId ?? String.Empty;
                        command.Parameters.Add("@patientName", SqlDbType.NVarChar).Value = patientName ?? String.Empty;
                        command.Parameters.Add("@program", SqlDbType.NVarChar).Value = program;
                        command.Parameters.Add("@assignedmember", SqlDbType.NVarChar).Value = assignedmember ?? String.Empty;
                        command.Parameters.Add("@index", SqlDbType.Int).Value = Index;
                        command.Parameters.Add("@readingFilter", SqlDbType.NVarChar).Value = readingFilter;
                        command.Parameters.Add("@InteractionFilter", SqlDbType.NVarChar).Value = interactionFilter;
                        command.Parameters.Add("@Createdby", SqlDbType.NVarChar).Value = Createdby;
                        command.Parameters.Add("@RoleId", SqlDbType.Int).Value = RoleId;
                        command.Parameters.Add("@startDuration", SqlDbType.Int).Value = startDuration;
                        command.Parameters.Add("@endDuration", SqlDbType.Int).Value = endDuration;
                        SqlDataReader reader = command.ExecuteReader();

                        List<PatientBilldataList> PatientBilldateList = new List<PatientBilldataList>();
                        List<int> roleids = new List<int>();
                        TotalCounts counts = new TotalCounts();
                        PatientBilldataList PatientBilldata = new PatientBilldataList();
                        PatientBilldata.PatientBilldataRecords = new List<PatientBilldataRecords>();

                        while (reader.Read())
                        {
                            bool val = false;
                            for (int i = 0; i < reader.FieldCount; i++)
                            {

                                if (reader.GetName(i).Equals("PatientNumber", StringComparison.InvariantCultureIgnoreCase))
                                    val = true;
                            }
                            if (val)
                            {
                                PatientBilldataRecords patientBilldataRecords = new PatientBilldataRecords();


                                patientBilldataRecords.PatientNumber = reader["PatientNumber"].ToString();
                                patientBilldataRecords.patientId = Convert.ToInt32(reader["patientId"]);
                                patientBilldataRecords.PatientProgramId = Convert.ToInt32(reader["PatientProgramId"]);
                                patientBilldataRecords.PatientName = Convert.ToString(reader["FirstName"].ToString());
                                patientBilldataRecords.Program = Convert.ToString(reader["Name"].ToString());
                                patientBilldataRecords.AssignedMemeber = Convert.ToString(reader["Assignedmember"]) + ' ' + Convert.ToString(reader["AssignedLastName"]);
                                patientBilldataRecords.TortalVitalCount = Convert.ToInt32(reader["totalVitalCount"]);
                                patientBilldataRecords.interaction = Convert.ToInt32(reader["TotalDuration"]);
                                patientBilldataRecords.DaysCompleted = Convert.ToInt32(reader["DaysCompleted"]);
                                patientBilldataRecords.ClinicName = Convert.ToString(reader["ClinicName"]);
                                patientBilldataRecords.Code = Convert.ToString(reader["Code"]);
                                patientBilldataRecords.NextBillingDate = null;
                                counts.Total = Convert.ToInt32(reader["Total"]);
                                counts.TotalCount = Convert.ToInt32(reader["TotalCount"]);
                                counts.C1Total = Convert.ToInt32(reader["C1Total"]);
                                counts.C2Total = Convert.ToInt32(reader["C2Total"]);
                                counts.TodayCount = null;
                                counts.Next7DaysCount = null;

                                PatientBilldata.PatientBilldataRecords.Add(patientBilldataRecords);
                            }
                            else
                            {
                                counts.Total = Convert.ToInt32(reader["Total"]);
                                counts.TotalCount = Convert.ToInt32(reader["TotalCount"]);
                                counts.C1Total = Convert.ToInt32(reader["C1Total"]);
                                counts.C2Total = Convert.ToInt32(reader["C2Total"]);
                                counts.TodayCount = null;
                                counts.Next7DaysCount = null;
                            }

                        }
                        PatientBilldata.TotalCounts = counts;
                        // PatientBilldateList.Add(PatientBilldata);
                        //if (reader.FieldCount == 0)
                        //{
                        //    con.Close();
                        //    return null;
                        //}
                        con.Close();
                        return PatientBilldata;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception occured - class Name :" + this.GetType().Name + ",  Method Name : " + MethodBase.GetCurrentMethod().Name + ", Error Message :" + ex.Message + "");
                return null;
            }



        }
        public PatientBilldataList GetPatientBillingDataListCCMDaysBasedBilling(string patientType, string patientFilter, string patientId, string patientName, string program, string assignedmember, int Index, string readingFilter, string interactionFilter, int RoleId, string Createdby, string ConnectionString)
        {
            try
            {
                double startDuration;
                double endDuration;
                if (interactionFilter == "0-19")
                {
                    TimeSpan t1 = TimeSpan.FromMinutes(19.99);
                    startDuration = 0;
                    endDuration=t1.TotalSeconds;
                }
                else if (interactionFilter == "20-39")
                {
                    TimeSpan t1 = TimeSpan.FromMinutes(20);
                    TimeSpan t2 = TimeSpan.FromMinutes(39.99);
                    startDuration = t1.TotalSeconds;
                    endDuration=t2.TotalSeconds;
                }
                else if (interactionFilter == "40-59")
                {
                    TimeSpan t1 = TimeSpan.FromMinutes(40);
                    TimeSpan t2 = TimeSpan.FromMinutes(59.99);
                    startDuration = t1.TotalSeconds;
                    endDuration=t2.TotalSeconds;
                }
                else if (interactionFilter == "60")
                {
                    TimeSpan t1 = TimeSpan.FromMinutes(60);
                    startDuration = t1.TotalSeconds;
                    endDuration=0;
                }
                else
                {
                    startDuration = 0;
                    endDuration=0;
                }
                using (SqlConnection con = new SqlConnection(ConnectionString))
                {
                    con.Open();

                    using (SqlCommand command = new SqlCommand("GetPatientBillingDataListCCM_DaysBasedBilling", con))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.Add("@patientType", SqlDbType.NVarChar).Value = patientType;
                        command.Parameters.Add("@patientFilter", SqlDbType.NVarChar).Value = patientFilter ?? String.Empty;
                        command.Parameters.Add("@patientId", SqlDbType.NVarChar).Value = patientId ?? String.Empty;
                        command.Parameters.Add("@patientName", SqlDbType.NVarChar).Value = patientName ?? String.Empty;
                        command.Parameters.Add("@program", SqlDbType.NVarChar).Value = program;
                        command.Parameters.Add("@assignedmember", SqlDbType.NVarChar).Value = assignedmember ?? String.Empty;
                        command.Parameters.Add("@index", SqlDbType.Int).Value = Index;
                        command.Parameters.Add("@readingFilter", SqlDbType.NVarChar).Value = readingFilter;
                        command.Parameters.Add("@InteractionFilter", SqlDbType.NVarChar).Value = interactionFilter;
                        command.Parameters.Add("@Createdby", SqlDbType.NVarChar).Value = Createdby;
                        command.Parameters.Add("@RoleId", SqlDbType.Int).Value = RoleId;
                        command.Parameters.Add("@startDuration", SqlDbType.Int).Value = startDuration;
                        command.Parameters.Add("@endDuration", SqlDbType.Int).Value = endDuration;
                        SqlDataReader reader = command.ExecuteReader();

                        List<PatientBilldataList> PatientBilldateList = new List<PatientBilldataList>();
                        List<int> roleids = new List<int>();
                        TotalCounts counts = new TotalCounts();
                        PatientBilldataList PatientBilldata = new PatientBilldataList();
                        PatientBilldata.PatientBilldataRecords = new List<PatientBilldataRecords>();

                        while (reader.Read())
                        {
                            bool val = false;
                            for (int i = 0; i < reader.FieldCount; i++)
                            {

                                if (reader.GetName(i).Equals("PatientNumber", StringComparison.InvariantCultureIgnoreCase))
                                    val = true;
                            }
                            if (val)
                            {
                                PatientBilldataRecords patientBilldataRecords = new PatientBilldataRecords();
                                patientBilldataRecords.PatientNumber = reader["PatientNumber"].ToString();
                                patientBilldataRecords.patientId = Convert.ToInt32(reader["patientId"]);
                                patientBilldataRecords.PatientProgramId = Convert.ToInt32(reader["PatientProgramId"]);
                                patientBilldataRecords.PatientName = Convert.ToString(reader["FirstName"].ToString());
                                patientBilldataRecords.Program = Convert.ToString(reader["Name"].ToString());
                                patientBilldataRecords.AssignedMemeber = Convert.ToString(reader["Assignedmember"]) + ' ' + Convert.ToString(reader["AssignedLastName"]);
                                patientBilldataRecords.TortalVitalCount = Convert.ToInt32(reader["totalVitalCount"]);
                                patientBilldataRecords.interaction = Convert.ToInt32(reader["TotalDuration"]);
                                patientBilldataRecords.DaysCompleted = Convert.ToInt32(reader["DaysCompleted"]);
                                patientBilldataRecords.ClinicName = Convert.ToString(reader["ClinicName"]);
                                patientBilldataRecords.Code = Convert.ToString(reader["Code"]);
                                DateTime dateTime = Convert.ToDateTime(reader["NextBillingDate"]);
                                patientBilldataRecords.NextBillingDate = dateTime.ToString("yyyy-MM-dd");
                                counts.Total = Convert.ToInt32(reader["Total"]);
                                counts.TotalCount = Convert.ToInt32(reader["TotalCount"]);
                                counts.C1Total = null;
                                counts.C2Total = null;
                                counts.TodayCount = Convert.ToInt32(reader["TodayCount"]);
                                counts.Next7DaysCount = Convert.ToInt32(reader["Next7DaysCount"]);

                                PatientBilldata.PatientBilldataRecords.Add(patientBilldataRecords);
                            }
                            else
                            {
                                counts.Total = Convert.ToInt32(reader["Total"]);
                                counts.TotalCount = Convert.ToInt32(reader["TotalCount"]);
                                counts.C1Total = null;
                                counts.C2Total = null;
                                counts.TodayCount = Convert.ToInt32(reader["TodayCount"]);
                                counts.Next7DaysCount = Convert.ToInt32(reader["Next7DaysCount"]);
                            }

                        }
                        PatientBilldata.TotalCounts = counts;
                        // PatientBilldateList.Add(PatientBilldata);
                        //if (reader.FieldCount == 0)
                        //{
                        //    con.Close();
                        //    return null;
                        //}
                        con.Close();
                        return PatientBilldata;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception occured - class Name :" + this.GetType().Name + ",  Method Name : " + MethodBase.GetCurrentMethod().Name + ", Error Message :" + ex.Message + "");
                return null;
            }



        }
        public PatientBilldataList GetPatientBillingDataListPCMCycleBasedBilling(string patientType, string patientFilter, string patientId, string patientName, string program, string assignedmember, int Index, string readingFilter, string interactionFilter, int RoleId, string Createdby, string ConnectionString)
        {
            try
            {
                double startDuration;
                double endDuration;
                if (interactionFilter == "0-19")
                {
                    TimeSpan t1 = TimeSpan.FromMinutes(19.99);
                    startDuration = 0;
                    endDuration=t1.TotalSeconds;
                }
                else if (interactionFilter == "20-39")
                {
                    TimeSpan t1 = TimeSpan.FromMinutes(20);
                    TimeSpan t2 = TimeSpan.FromMinutes(39.99);
                    startDuration = t1.TotalSeconds;
                    endDuration=t2.TotalSeconds;
                }
                else if (interactionFilter == "40-59")
                {
                    TimeSpan t1 = TimeSpan.FromMinutes(40);
                    TimeSpan t2 = TimeSpan.FromMinutes(59.99);
                    startDuration = t1.TotalSeconds;
                    endDuration=t2.TotalSeconds;
                }
                else if (interactionFilter == "60")
                {
                    TimeSpan t1 = TimeSpan.FromMinutes(60);
                    startDuration = t1.TotalSeconds;
                    endDuration=0;
                }
                else
                {
                    startDuration = 0;
                    endDuration=0;
                }

                using (SqlConnection con = new SqlConnection(ConnectionString))
                {
                    con.Open();

                    using (SqlCommand command = new SqlCommand("GetPatientBillingDataListPCM_CycleBasedBilling", con))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.Add("@patientType", SqlDbType.NVarChar).Value = patientType;
                        command.Parameters.Add("@patientFilter", SqlDbType.NVarChar).Value = patientFilter ?? String.Empty;
                        command.Parameters.Add("@patientId", SqlDbType.NVarChar).Value = patientId ?? String.Empty;
                        command.Parameters.Add("@patientName", SqlDbType.NVarChar).Value = patientName ?? String.Empty;
                        command.Parameters.Add("@program", SqlDbType.NVarChar).Value = program;
                        command.Parameters.Add("@assignedmember", SqlDbType.NVarChar).Value = assignedmember ?? String.Empty;
                        command.Parameters.Add("@index", SqlDbType.Int).Value = Index;
                        command.Parameters.Add("@readingFilter", SqlDbType.NVarChar).Value = readingFilter;
                        command.Parameters.Add("@InteractionFilter", SqlDbType.NVarChar).Value = interactionFilter;
                        command.Parameters.Add("@Createdby", SqlDbType.NVarChar).Value = Createdby;
                        command.Parameters.Add("@RoleId", SqlDbType.Int).Value = RoleId;
                        command.Parameters.Add("@startDuration", SqlDbType.Int).Value = startDuration;
                        command.Parameters.Add("@endDuration", SqlDbType.Int).Value = endDuration;
                        SqlDataReader reader = command.ExecuteReader();

                        List<PatientBilldataList> PatientBilldateList = new List<PatientBilldataList>();
                        List<int> roleids = new List<int>();
                        TotalCounts counts = new TotalCounts();
                        PatientBilldataList PatientBilldata = new PatientBilldataList();
                        PatientBilldata.PatientBilldataRecords = new List<PatientBilldataRecords>();

                        while (reader.Read())
                        {
                            bool val = false;
                            for (int i = 0; i < reader.FieldCount; i++)
                            {

                                if (reader.GetName(i).Equals("PatientNumber", StringComparison.InvariantCultureIgnoreCase))
                                    val = true;
                            }
                            if (val)
                            {
                                PatientBilldataRecords patientBilldataRecords = new PatientBilldataRecords();


                                patientBilldataRecords.PatientNumber = reader["PatientNumber"].ToString();
                                patientBilldataRecords.patientId = Convert.ToInt32(reader["patientId"]);
                                patientBilldataRecords.PatientProgramId = Convert.ToInt32(reader["PatientProgramId"]);
                                patientBilldataRecords.PatientName = Convert.ToString(reader["FirstName"].ToString());
                                patientBilldataRecords.Program = Convert.ToString(reader["Name"].ToString());
                                patientBilldataRecords.AssignedMemeber = Convert.ToString(reader["Assignedmember"]) + ' ' + Convert.ToString(reader["AssignedLastName"]);
                                patientBilldataRecords.TortalVitalCount = Convert.ToInt32(reader["totalVitalCount"]);
                                patientBilldataRecords.interaction = Convert.ToInt32(reader["TotalDuration"]);
                                patientBilldataRecords.DaysCompleted = Convert.ToInt32(reader["DaysCompleted"]);
                                patientBilldataRecords.ClinicName = Convert.ToString(reader["ClinicName"]);
                                patientBilldataRecords.Code = Convert.ToString(reader["Code"]);
                                patientBilldataRecords.NextBillingDate = null;
                                counts.Total = Convert.ToInt32(reader["Total"]);
                                counts.TotalCount = Convert.ToInt32(reader["TotalCount"]);
                                counts.C1Total = Convert.ToInt32(reader["C1Total"]);
                                counts.C2Total = Convert.ToInt32(reader["C2Total"]);
                                counts.TodayCount = null;
                                counts.Next7DaysCount = null;

                                PatientBilldata.PatientBilldataRecords.Add(patientBilldataRecords);
                            }
                            else
                            {
                                counts.Total = Convert.ToInt32(reader["Total"]);
                                counts.TotalCount = Convert.ToInt32(reader["TotalCount"]);
                                counts.C1Total = Convert.ToInt32(reader["C1Total"]);
                                counts.C2Total = Convert.ToInt32(reader["C2Total"]);
                                counts.TodayCount = null;
                                counts.Next7DaysCount = null;
                            }

                        }
                        PatientBilldata.TotalCounts = counts;
                        // PatientBilldateList.Add(PatientBilldata);
                        //if (reader.FieldCount == 0)
                        //{
                        //    con.Close();
                        //    return null;
                        //}
                        con.Close();
                        return PatientBilldata;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception occured - class Name :" + this.GetType().Name + ",  Method Name : " + MethodBase.GetCurrentMethod().Name + ", Error Message :" + ex.Message + "");
                return null;
            }



        }
        public PatientBilldataList GetPatientBillingDataListPCMDaysBasedBilling(string patientType, string patientFilter, string patientId, string patientName, string program, string assignedmember, int Index, string readingFilter, string interactionFilter, int RoleId, string Createdby, string ConnectionString)
        {
            try
            {
                double startDuration;
                double endDuration;
                if (interactionFilter == "0-19")
                {
                    TimeSpan t1 = TimeSpan.FromMinutes(19.99);
                    startDuration = 0;
                    endDuration=t1.TotalSeconds;
                }
                else if (interactionFilter == "20-39")
                {
                    TimeSpan t1 = TimeSpan.FromMinutes(20);
                    TimeSpan t2 = TimeSpan.FromMinutes(39.99);
                    startDuration = t1.TotalSeconds;
                    endDuration=t2.TotalSeconds;
                }
                else if (interactionFilter == "40-59")
                {
                    TimeSpan t1 = TimeSpan.FromMinutes(40);
                    TimeSpan t2 = TimeSpan.FromMinutes(59.99);
                    startDuration = t1.TotalSeconds;
                    endDuration=t2.TotalSeconds;
                }
                else if (interactionFilter == "60")
                {
                    TimeSpan t1 = TimeSpan.FromMinutes(60);
                    startDuration = t1.TotalSeconds;
                    endDuration=0;
                }
                else
                {
                    startDuration = 0;
                    endDuration=0;
                }
                using (SqlConnection con = new SqlConnection(ConnectionString))
                {
                    con.Open();

                    using (SqlCommand command = new SqlCommand("GetPatientBillingDataListPCM_DaysBasedBilling", con))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.Add("@patientType", SqlDbType.NVarChar).Value = patientType;
                        command.Parameters.Add("@patientFilter", SqlDbType.NVarChar).Value = patientFilter ?? String.Empty;
                        command.Parameters.Add("@patientId", SqlDbType.NVarChar).Value = patientId ?? String.Empty;
                        command.Parameters.Add("@patientName", SqlDbType.NVarChar).Value = patientName ?? String.Empty;
                        command.Parameters.Add("@program", SqlDbType.NVarChar).Value = program;
                        command.Parameters.Add("@assignedmember", SqlDbType.NVarChar).Value = assignedmember ?? String.Empty;
                        command.Parameters.Add("@index", SqlDbType.Int).Value = Index;
                        command.Parameters.Add("@readingFilter", SqlDbType.NVarChar).Value = readingFilter;
                        command.Parameters.Add("@InteractionFilter", SqlDbType.NVarChar).Value = interactionFilter;
                        command.Parameters.Add("@Createdby", SqlDbType.NVarChar).Value = Createdby;
                        command.Parameters.Add("@RoleId", SqlDbType.Int).Value = RoleId;
                        command.Parameters.Add("@startDuration", SqlDbType.Int).Value = startDuration;
                        command.Parameters.Add("@endDuration", SqlDbType.Int).Value = endDuration;
                        SqlDataReader reader = command.ExecuteReader();

                        List<PatientBilldataList> PatientBilldateList = new List<PatientBilldataList>();
                        List<int> roleids = new List<int>();
                        TotalCounts counts = new TotalCounts();
                        PatientBilldataList PatientBilldata = new PatientBilldataList();
                        PatientBilldata.PatientBilldataRecords = new List<PatientBilldataRecords>();

                        while (reader.Read())
                        {
                            bool val = false;
                            for (int i = 0; i < reader.FieldCount; i++)
                            {

                                if (reader.GetName(i).Equals("PatientNumber", StringComparison.InvariantCultureIgnoreCase))
                                    val = true;
                            }
                            if (val)
                            {
                                PatientBilldataRecords patientBilldataRecords = new PatientBilldataRecords();
                                patientBilldataRecords.PatientNumber = reader["PatientNumber"].ToString();
                                patientBilldataRecords.patientId = Convert.ToInt32(reader["patientId"]);
                                patientBilldataRecords.PatientProgramId = Convert.ToInt32(reader["PatientProgramId"]);
                                patientBilldataRecords.PatientName = Convert.ToString(reader["FirstName"].ToString());
                                patientBilldataRecords.Program = Convert.ToString(reader["Name"].ToString());
                                patientBilldataRecords.AssignedMemeber = Convert.ToString(reader["Assignedmember"]) + ' ' + Convert.ToString(reader["AssignedLastName"]);
                                patientBilldataRecords.TortalVitalCount = Convert.ToInt32(reader["totalVitalCount"]);
                                patientBilldataRecords.interaction = Convert.ToInt32(reader["TotalDuration"]);
                                patientBilldataRecords.DaysCompleted = Convert.ToInt32(reader["DaysCompleted"]);
                                patientBilldataRecords.ClinicName = Convert.ToString(reader["ClinicName"]);
                                patientBilldataRecords.Code = Convert.ToString(reader["Code"]);
                                DateTime dateTime = Convert.ToDateTime(reader["NextBillingDate"]);
                                patientBilldataRecords.NextBillingDate = dateTime.ToString("yyyy-MM-dd");
                                counts.Total = Convert.ToInt32(reader["Total"]);
                                counts.TotalCount = Convert.ToInt32(reader["TotalCount"]);
                                counts.C1Total = null;
                                counts.C2Total = null;
                                counts.TodayCount = Convert.ToInt32(reader["TodayCount"]);
                                counts.Next7DaysCount = Convert.ToInt32(reader["Next7DaysCount"]);

                                PatientBilldata.PatientBilldataRecords.Add(patientBilldataRecords);
                            }
                            else
                            {
                                counts.Total = Convert.ToInt32(reader["Total"]);
                                counts.TotalCount = Convert.ToInt32(reader["TotalCount"]);
                                counts.C1Total = null;
                                counts.C2Total = null;
                                counts.TodayCount = Convert.ToInt32(reader["TodayCount"]);
                                counts.Next7DaysCount = Convert.ToInt32(reader["Next7DaysCount"]);
                            }

                        }
                        PatientBilldata.TotalCounts = counts;
                        // PatientBilldateList.Add(PatientBilldata);
                        //if (reader.FieldCount == 0)
                        //{
                        //    con.Close();
                        //    return null;
                        //}
                        con.Close();
                        return PatientBilldata;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception occured - class Name :" + this.GetType().Name + ",  Method Name : " + MethodBase.GetCurrentMethod().Name + ", Error Message :" + ex.Message + "");
                return null;
            }



        }
        public PatientBilldataList GetPatientBillingDataListDaysBasedBilling(string patientType, string patientFilter, string patientId, string patientName, string program, string assignedmember, int Index, string readingFilter, string interactionFilter, int RoleId, string Createdby, string ConnectionString)
        {
            try
            {

                using (SqlConnection con = new SqlConnection(ConnectionString))
                {
                    con.Open();

                    using (SqlCommand command = new SqlCommand("GetPatientBillingDataList_DaysBasedBilling", con))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.Add("@patientType", SqlDbType.NVarChar).Value = patientType;
                        command.Parameters.Add("@patientFilter", SqlDbType.NVarChar).Value = patientFilter ?? String.Empty;
                        command.Parameters.Add("@patientId", SqlDbType.NVarChar).Value = patientId ?? String.Empty;
                        command.Parameters.Add("@patientName", SqlDbType.NVarChar).Value = patientName ?? String.Empty;
                        command.Parameters.Add("@program", SqlDbType.NVarChar).Value = program;
                        command.Parameters.Add("@assignedmember", SqlDbType.NVarChar).Value = assignedmember ?? String.Empty;
                        command.Parameters.Add("@index", SqlDbType.Int).Value = Index;
                        command.Parameters.Add("@readingFilter", SqlDbType.NVarChar).Value = readingFilter;
                        command.Parameters.Add("@InteractionFilter", SqlDbType.NVarChar).Value = interactionFilter;
                        command.Parameters.Add("@Createdby", SqlDbType.NVarChar).Value = Createdby;
                        command.Parameters.Add("@RoleId", SqlDbType.Int).Value = RoleId;
                        SqlDataReader reader = command.ExecuteReader();

                        List<PatientBilldataList> PatientBilldateList = new List<PatientBilldataList>();
                        List<int> roleids = new List<int>();
                        TotalCounts counts = new TotalCounts();
                        PatientBilldataList PatientBilldata = new PatientBilldataList();
                        PatientBilldata.PatientBilldataRecords = new List<PatientBilldataRecords>();

                        while (reader.Read())
                        {
                            bool val = false;
                            for (int i = 0; i < reader.FieldCount; i++)
                            {

                                if (reader.GetName(i).Equals("PatientNumber", StringComparison.InvariantCultureIgnoreCase))
                                    val = true;
                            }
                            if (val)
                            {
                                PatientBilldataRecords patientBilldataRecords = new PatientBilldataRecords();
                                patientBilldataRecords.PatientNumber = reader["PatientNumber"].ToString();
                                patientBilldataRecords.patientId = Convert.ToInt32(reader["patientId"]);
                                patientBilldataRecords.PatientProgramId = Convert.ToInt32(reader["PatientProgramId"]);
                                patientBilldataRecords.PatientName = Convert.ToString(reader["FirstName"].ToString());
                                patientBilldataRecords.Program = Convert.ToString(reader["Name"].ToString());
                                patientBilldataRecords.AssignedMemeber = Convert.ToString(reader["Assignedmember"]) + ' ' + Convert.ToString(reader["AssignedLastName"]);
                                patientBilldataRecords.TortalVitalCount = Convert.ToInt32(reader["totalVitalCount"]);
                                patientBilldataRecords.interaction = Convert.ToInt32(reader["TotalDuration"]);
                                patientBilldataRecords.DaysCompleted = Convert.ToInt32(reader["DaysCompleted"]);
                                patientBilldataRecords.ClinicName = Convert.ToString(reader["ClinicName"]);
                                patientBilldataRecords.Code = Convert.ToString(reader["Code"]);
                                DateTime dateTime = Convert.ToDateTime(reader["NextBillingDate"]);
                                patientBilldataRecords.NextBillingDate = dateTime.ToString("yyyy-MM-dd");
                                counts.Total = Convert.ToInt32(reader["Total"]);
                                counts.TotalCount = Convert.ToInt32(reader["TotalCount"]);
                                counts.C1Total = null;
                                counts.C2Total = null;
                                counts.TodayCount = Convert.ToInt32(reader["TodayCount"]);
                                counts.Next7DaysCount = Convert.ToInt32(reader["Next7DaysCount"]);

                                PatientBilldata.PatientBilldataRecords.Add(patientBilldataRecords);
                            }
                            else
                            {
                                counts.Total = Convert.ToInt32(reader["Total"]);
                                counts.TotalCount = Convert.ToInt32(reader["TotalCount"]);
                                counts.C1Total = null;
                                counts.C2Total = null;
                                counts.TodayCount = Convert.ToInt32(reader["TodayCount"]);
                                counts.Next7DaysCount = Convert.ToInt32(reader["Next7DaysCount"]);
                            }

                        }
                        PatientBilldata.TotalCounts = counts;
                        // PatientBilldateList.Add(PatientBilldata);
                        //if (reader.FieldCount == 0)
                        //{
                        //    con.Close();
                        //    return null;
                        //}
                        con.Close();
                        return PatientBilldata;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception occured - class Name :" + this.GetType().Name + ",  Method Name : " + MethodBase.GetCurrentMethod().Name + ", Error Message :" + ex.Message + "");
                return null;
            }



        }
        public BilledDates GetPatientLastBilledPeriods(int patientId, int patientPgmId, int billingcode, string connectionString)
        {
            BilledDates ret = new BilledDates();
            try
            {
                //string ConnectionString = ConfigurationManager.ConnectionStrings[0].ToString();
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();

                    using (SqlCommand command = new SqlCommand("usp_GetLastBilledDates", con))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@PatientId", patientId);
                        command.Parameters.AddWithValue("@PatientProgramId", patientPgmId);
                        command.Parameters.AddWithValue("@BillingCodeId", billingcode);
                        SqlParameter returnParameter = command.Parameters.Add("RetVal", SqlDbType.Int);
                        returnParameter.Direction = ParameterDirection.ReturnValue;
                        SqlDataReader reader = command.ExecuteReader();

                        while (reader.Read())
                        {
                            ret.StartDate = Convert.ToDateTime(reader["LastBilledStartDate"]);
                            ret.EndDate = Convert.ToDateTime(reader["LastBilledEndDate"]);
                        }
                        con.Close();
                        return ret;

                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception occured - class Name :" + this.GetType().Name + ",  Method Name : " + MethodBase.GetCurrentMethod().Name + ", Error Message :" + ex.Message + "");
                return null;
            }



        }
        public List<LastBilleddata> GetPatientlastBilledCycle(int patientId, int patientProgramId, string status, string ConnectionString)
        {
            List<SystemConfigInfo> lstConfig = DalCommon.GetSystemConfig(ConnectionString, "Billing", "User");
            SystemConfigInfo provider = lstConfig.Find(x => x.Name.Equals("BillingType"));
            List<LastBilleddata> lastBilleddata = new List<LastBilleddata>();
            if (lstConfig.Count > 0)
            {
                if (provider.Value == "30days")
                {
                    lastBilleddata = GetPatientlastBilledCycleDaysBasedBilling(patientId, patientProgramId, status, ConnectionString);

                }
                else if (provider.Value == "cycle")
                {
                    lastBilleddata = GetPatientlastBilledCycleCycleBasedBilling(patientId, patientProgramId, status, ConnectionString);
                }
                else
                {
                    lastBilleddata = GetPatientlastBilledCycleCycleBasedBilling(patientId, patientProgramId, status, ConnectionString);
                }
            }
            else
            {
                lastBilleddata = GetPatientlastBilledCycleCycleBasedBilling(patientId, patientProgramId, status, ConnectionString);
            }
            return lastBilleddata;
        }
        public List<LastBilleddata> GetPatientlastBilledCycleByDate(int patientId, int patientProgramId, DateTime billeddate, string ConnectionString)
        {
            List<SystemConfigInfo> lstConfig = DalCommon.GetSystemConfig(ConnectionString, "Billing", "User");
            SystemConfigInfo provider = lstConfig.Find(x => x.Name.Equals("BillingType"));
            List<LastBilleddata> lastBilleddata = new List<LastBilleddata>();
            if (lstConfig.Count > 0)
            {
                if (provider.Value == "30days")
                {
                    lastBilleddata = GetPatientlastBilledCycleByDateDaysBasedBilling(patientId, patientProgramId, billeddate, ConnectionString);

                }
                else if (provider.Value == "cycle")
                {
                    lastBilleddata = GetPatientlastBilledCycleByDateCycleBasedBilling(patientId, patientProgramId, billeddate, ConnectionString);
                }
                else
                {
                    lastBilleddata = GetPatientlastBilledCycleByDateCycleBasedBilling(patientId, patientProgramId, billeddate, ConnectionString);
                }
            }
            else
            {
                lastBilleddata = GetPatientlastBilledCycleByDateCycleBasedBilling(patientId, patientProgramId, billeddate, ConnectionString);
            }
            return lastBilleddata;
        }
        public List<LastBilleddata> GetPatientlastBilledCycleByDateDaysBasedBilling(int patientId, int patientProgramId, DateTime billeddate, string ConnectionString)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(ConnectionString))
                {
                    con.Open();

                    using (SqlCommand command = new SqlCommand("usp_GetLastBilledCycleByDate", con))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@patientid", patientId);
                        command.Parameters.AddWithValue("@patientprogramid", patientProgramId);
                        command.Parameters.AddWithValue("@billeddate", billeddate);
                        SqlParameter returnParameter = command.Parameters.Add("RetVal", SqlDbType.Int);
                        returnParameter.Direction = ParameterDirection.ReturnValue;
                        SqlDataReader reader = command.ExecuteReader();
                        List<LastBilleddata> list = new List<LastBilleddata>();
                        string programName = GetProgramName(ConnectionString, patientProgramId);
                        List<BillingCodesDetails> billingCodes = GetBillingCodeDetailsByProgram(ConnectionString, programName);
                        while (reader.Read())
                        {
                            LastBilleddata ret = new LastBilleddata();
                            ret.CPTCode = Convert.ToString(reader["BillingCode"]);

                            int targetMet = Convert.ToInt32(reader["TargetMet"]);
                            var startdate = Convert.ToDateTime(reader["StartDate"]);
                            // (!DBNull.Value.Equals(Convert.ToDateTime(reader["Billeddate"]))) ? Convert.ToInt32(reader["OrganizationId"]) : 0;
                            string Billeddate = Convert.ToString(reader["Billeddate"]);
                            string jointString = string.Empty;
                            if (Billeddate != null && Billeddate != String.Empty)
                            {
                                string date = Convert.ToDateTime(Billeddate).ToString("MMM dd, yyyy");
                                jointString = " to " + date;
                            }
                            ret.Last_Billing_Cycle = startdate.ToString("MMM dd, yyyy") + jointString;
                            ret.reading = Convert.ToString(reader["Totalreadings"]);
                            ret.status = targetMet == 1 ? "Billed" : "Not Billed";
                            list.Add(ret);



                        }
                        List<LastBilleddata> list1 = new List<LastBilleddata>();
                        foreach (var code in billingCodes)
                        {
                            LastBilleddata ret = new LastBilleddata();
                            var result = list.Where(s => s.CPTCode == Convert.ToString(code.BillingCodeID)).FirstOrDefault();
                            if (result != null)
                            {
                                ret.CPTCode = code.BillingCode;
                                ret.Last_Billing_Cycle = result.Last_Billing_Cycle;
                                ret.reading = result.reading;
                                ret.status = result.status;
                            }
                            else
                            {
                                ret.CPTCode = code.BillingCode;
                                ret.Last_Billing_Cycle = null;
                                ret.reading = "0";
                                ret.status = "Not Billed";
                            }
                            list1.Add(ret);

                        }


                        if (reader.FieldCount == 0)
                        {
                            return null;
                        }
                        return list1;
                    }
                }
            }
            catch (Exception ex)
            {


                throw;
            }
        }

        public List<LastBilleddata> GetPatientlastBilledCycleByDateCycleBasedBilling(int patientId, int patientProgramId, DateTime billeddate, string ConnectionString)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(ConnectionString))
                {
                    con.Open();

                    using (SqlCommand command = new SqlCommand("usp_GetLastBilledCycleByDate", con))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@patientid", patientId);
                        command.Parameters.AddWithValue("@patientprogramid", patientProgramId);
                        command.Parameters.AddWithValue("@billeddate", billeddate);
                        SqlParameter returnParameter = command.Parameters.Add("RetVal", SqlDbType.Int);
                        returnParameter.Direction = ParameterDirection.ReturnValue;
                        SqlDataReader reader = command.ExecuteReader();
                        List<LastBilleddata> list = new List<LastBilleddata>();
                        string programName = GetProgramName(ConnectionString, patientProgramId);
                        List<BillingCodesDetails> billingCodes = GetBillingCodeDetailsByProgram(ConnectionString, programName);
                        while (reader.Read())
                        {
                            LastBilleddata ret = new LastBilleddata();
                            ret.CPTCode = Convert.ToString(reader["BillingCode"]);

                            int targetMet = Convert.ToInt32(reader["TargetMet"]);
                            var startdate = Convert.ToDateTime(reader["StartDate"]);
                            // (!DBNull.Value.Equals(Convert.ToDateTime(reader["Billeddate"]))) ? Convert.ToInt32(reader["OrganizationId"]) : 0;
                            string Billeddate = Convert.ToString(reader["Billeddate"]);
                            string jointString = string.Empty;
                            if (Billeddate != null && Billeddate != String.Empty)
                            {
                                string date = Convert.ToDateTime(Billeddate).ToString("MMM dd, yyyy");
                                jointString = " to " + date;
                            }
                            ret.Last_Billing_Cycle = startdate.ToString("MMM dd, yyyy") + jointString;
                            ret.reading = Convert.ToString(reader["Totalreadings"]);
                            ret.status = targetMet == 1 ? "Billed" : "Not Billed";
                            list.Add(ret);



                        }
                        List<LastBilleddata> list1 = new List<LastBilleddata>();
                        foreach (var code in billingCodes)
                        {
                            LastBilleddata ret = new LastBilleddata();
                            var result = list.Where(s => s.CPTCode == Convert.ToString(code.BillingCodeID)).FirstOrDefault();
                            if (result != null)
                            {
                                ret.CPTCode = code.BillingCode;
                                ret.Last_Billing_Cycle = result.Last_Billing_Cycle;
                                ret.reading = result.reading;
                                ret.status = result.status;
                            }
                            else
                            {
                                ret.CPTCode = code.BillingCode;
                                ret.Last_Billing_Cycle = null;
                                ret.reading = "0";
                                ret.status = "Not Billed";
                            }
                            list1.Add(ret);

                        }


                        if (reader.FieldCount == 0)
                        {
                            return null;
                        }
                        return list1;
                    }
                }
            }
            catch (Exception ex)
            {


                throw;
            }
        }
        public List<LastBilleddata> GetPatientlastBilledCycleDaysBasedBilling(int patientId, int patientProgramId, string status, string ConnectionString)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(ConnectionString))
                {
                    con.Open();

                    using (SqlCommand command = new SqlCommand("Usp_GetpatientLastBilledcycle_DaysBasedBilling", con))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@PatientId", patientId);
                        command.Parameters.AddWithValue("@PatientProgramId", patientProgramId);
                        command.Parameters.AddWithValue("@status", status);
                        SqlParameter returnParameter = command.Parameters.Add("RetVal", SqlDbType.Int);
                        returnParameter.Direction = ParameterDirection.ReturnValue;
                        SqlDataReader reader = command.ExecuteReader();
                        List<LastBilleddata> list = new List<LastBilleddata>();
                        string programName = GetProgramName(ConnectionString, patientProgramId);
                        List<BillingCodesDetails> billingCodes = GetBillingCodeDetailsByProgram(ConnectionString, programName);
                        while (reader.Read())
                        {
                            LastBilleddata ret = new LastBilleddata();
                            ret.CPTCode = Convert.ToString(reader["BillingCode"]);

                            int targetMet = Convert.ToInt32(reader["TargetMet"]);
                            var startdate = Convert.ToDateTime(reader["StartDate"]);
                            // (!DBNull.Value.Equals(Convert.ToDateTime(reader["Billeddate"]))) ? Convert.ToInt32(reader["OrganizationId"]) : 0;
                            string Billeddate = Convert.ToString(reader["Billeddate"]);
                            string jointString = string.Empty;
                            if (Billeddate != null && Billeddate != String.Empty)
                            {
                                string date = Convert.ToDateTime(Billeddate).ToString("MMM dd, yyyy");
                                jointString = " to " + date;
                            }
                            ret.Last_Billing_Cycle = startdate.ToString("MMM dd, yyyy") + jointString;
                            ret.reading = Convert.ToString(reader["Totalreadings"]);
                            ret.status = targetMet == 1 ? "Billed" : "Not Billed";
                            list.Add(ret);



                        }
                        List<LastBilleddata> list1 = new List<LastBilleddata>();
                        foreach (var code in billingCodes)
                        {
                            LastBilleddata ret = new LastBilleddata();
                            var result = list.Where(s => s.CPTCode == code.BillingCode).FirstOrDefault();
                            if (result != null)
                            {
                                ret.CPTCode = result.CPTCode;
                                ret.Last_Billing_Cycle = result.Last_Billing_Cycle;
                                ret.reading = result.reading;
                                ret.status = result.status;
                            }
                            else
                            {
                                ret.CPTCode = code.BillingCode;
                                ret.Last_Billing_Cycle = null;
                                ret.reading = "0";
                                ret.status = "Not Billed";
                            }
                            list1.Add(ret);

                        }


                        if (reader.FieldCount == 0)
                        {
                            return null;
                        }
                        return list1;
                    }
                }
            }
            catch (Exception ex)
            {


                throw;
            }
        }
        public string GetProgramName(string connectionString, int PatientProgramId)
        {
            try
            {
                // string ConnectionString = ConfigurationManager.AppSettings["RPM"].ToString();
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();

                    using (SqlCommand command = new SqlCommand("usp_GetProgramName", con))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@PatientProgramId", PatientProgramId);
                        SqlParameter returnParameter = command.Parameters.Add("RetVal", SqlDbType.Int);
                        returnParameter.Direction = ParameterDirection.ReturnValue;
                        SqlDataReader reader = command.ExecuteReader();
                        string ProgramName = string.Empty;

                        while (reader.Read())
                        {
                            ProgramName = Convert.ToString(reader["ProgramName"]);
                        }

                        if (ProgramName == string.Empty)
                        {
                            con.Close();
                            return null;
                        }
                        con.Close();
                        return ProgramName;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception occured - class Name :" + this.GetType().Name + ",  Method Name : " + MethodBase.GetCurrentMethod().Name + ", Error Message :" + ex.Message + "");
                return null;
            }



        }
        public string GetProgramNameByBillingCode(string connectionString, string BillingCode)
        {
            try
            {
                // string ConnectionString = ConfigurationManager.AppSettings["RPM"].ToString();
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();

                    using (SqlCommand command = new SqlCommand("usp_GetProgramNameByBillingCode", con))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@BillingCode", BillingCode);
                        SqlParameter returnParameter = command.Parameters.Add("RetVal", SqlDbType.Int);
                        returnParameter.Direction = ParameterDirection.ReturnValue;
                        SqlDataReader reader = command.ExecuteReader();
                        string ProgramName = string.Empty;

                        while (reader.Read())
                        {
                            ProgramName = Convert.ToString(reader["ProgramName"]);
                        }

                        if (ProgramName == string.Empty)
                        {
                            con.Close();
                            return null;
                        }
                        con.Close();
                        return ProgramName;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception occured - class Name :" + this.GetType().Name + ",  Method Name : " + MethodBase.GetCurrentMethod().Name + ", Error Message :" + ex.Message + "");
                return null;
            }



        }
        public List<LastBilleddata> GetPatientlastBilledCycleCycleBasedBilling(int patientId, int patientProgramId, string status, string ConnectionString)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(ConnectionString))
                {
                    con.Open();

                    using (SqlCommand command = new SqlCommand("Usp_GetpatientLastBilledcycle_CycleBasedBilling", con))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@PatientId", patientId);
                        command.Parameters.AddWithValue("@PatientProgramId", patientProgramId);
                        command.Parameters.AddWithValue("@status", status);
                        SqlParameter returnParameter = command.Parameters.Add("RetVal", SqlDbType.Int);
                        returnParameter.Direction = ParameterDirection.ReturnValue;
                        SqlDataReader reader = command.ExecuteReader();
                        List<LastBilleddata> list = new List<LastBilleddata>();
                        string programName = GetProgramName(ConnectionString, patientProgramId);
                        //List<BillingCodesDetails> billingCodes = GetBillingCodeDetails(ConnectionString);
                        List<BillingCodesDetails> billingCodes = GetBillingCodeDetailsByProgram(ConnectionString, programName);
                        while (reader.Read())
                        {
                            LastBilleddata ret = new LastBilleddata();
                            ret.CPTCode = Convert.ToString(reader["BillingCode"]);

                            int targetMet = Convert.ToInt32(reader["TargetMet"]);
                            var startdate = Convert.ToDateTime(reader["StartDate"]);
                            // (!DBNull.Value.Equals(Convert.ToDateTime(reader["Billeddate"]))) ? Convert.ToInt32(reader["OrganizationId"]) : 0;
                            string Billeddate = Convert.ToString(reader["Billeddate"]);
                            string jointString = string.Empty;
                            if (Billeddate != null && Billeddate != String.Empty)
                            {
                                string date = Convert.ToDateTime(Billeddate).ToString("MMM dd, yyyy");
                                jointString = " to " + date;
                            }
                            if (ret.CPTCode == "G0506")
                            {
                                // string billdate = Convert.ToString(reader["Billeddate"]);
                                if (Billeddate != null && Billeddate != String.Empty)
                                {
                                    ret.Last_Billing_Cycle = Convert.ToDateTime(Billeddate).ToString("MMM dd, yyyy");
                                }


                            }
                            else
                            {
                                ret.Last_Billing_Cycle = startdate.ToString("MMM dd, yyyy") + jointString;
                            }

                            ret.reading = Convert.ToString(reader["Totalreadings"]);
                            ret.status = targetMet == 1 ? "Billed" : "Not Billed";
                            list.Add(ret);



                        }
                        List<LastBilleddata> list1 = new List<LastBilleddata>();
                        foreach (var code in billingCodes)
                        {
                            LastBilleddata ret = new LastBilleddata();
                            var result = list.Where(s => s.CPTCode == code.BillingCode).FirstOrDefault();
                            if (result != null)
                            {
                                ret.CPTCode = result.CPTCode;
                                ret.Last_Billing_Cycle = result.Last_Billing_Cycle;
                                ret.reading = result.reading;
                                ret.status = result.status;
                            }
                            else
                            {
                                ret.CPTCode = code.BillingCode;
                                ret.Last_Billing_Cycle = null;
                                ret.reading = "0";
                                ret.status = "Not Billed";
                            }
                            list1.Add(ret);

                        }


                        if (reader.FieldCount == 0)
                        {
                            return null;
                        }
                        return list1;
                    }
                }
            }
            catch (Exception ex)
            {


                throw;
            }
        }
        public bool UpdateBillDatesMedIT(BillingDatesUpdates Info, string ConnectionString)
        {
            bool ret = true;
            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    SqlCommand command = new SqlCommand("Usp_UpdPatientBillingDates", connection);
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@PatientId", Info.PatientId);
                    command.Parameters.AddWithValue("@PatientProgramId", Info.PatientProgramId);
                    command.Parameters.AddWithValue("@StartDate", Info.StartDate);
                    command.Parameters.AddWithValue("@BilledDate", Info.BilledDate);
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
            catch (Exception ex)
            {
                throw ex;
            }
            return ret;
        }
        public BillingType GetBillingType(string connectionString)
        {
            BillingType billingType = new BillingType();
            List<SystemConfigInfo> lstConfig = DalCommon.GetSystemConfig(connectionString, "Billing", "User");
            if (lstConfig != null && lstConfig.Count > 0)
            {
                SystemConfigInfo provider = lstConfig.Find(x => x.Name.Equals("BillingType"));
                billingType.Provider = provider.Value;
            }

            return billingType;
        }

        public List<BillingInfoCounts> GetPatientBillingInfoCounts(string billingCode, string cycle, int RoleId, string CreatedBy, string ConnectionString)
        {
            List<BillingInfoCounts> billingInfoCounts = new List<BillingInfoCounts>();
            List<SystemConfigInfo> lstConfig = DalCommon.GetSystemConfig(ConnectionString, "Billing", "User");
            SystemConfigInfo provider = lstConfig.Find(x => x.Name.Equals("BillingType"));
            if (lstConfig.Count > 0)
            {
                if (provider.Value == "30days")
                {
                    billingInfoCounts = GetPatientBillingInfoCountsDaysBasedBilling(billingCode, cycle, RoleId, CreatedBy, ConnectionString);

                }
                else if (provider.Value == "cycle")
                {
                    billingInfoCounts = GetPatientBillingInfoCountsCycleBasedBilling(billingCode, cycle, RoleId, CreatedBy, ConnectionString);
                }
                else
                {
                    billingInfoCounts = GetPatientBillingInfoCountsCycleBasedBilling(billingCode, cycle, RoleId, CreatedBy, ConnectionString);
                }
            }
            else
            {
                billingInfoCounts = GetPatientBillingInfoCountsCycleBasedBilling(billingCode, cycle, RoleId, CreatedBy, ConnectionString);
            }
            return billingInfoCounts;
        }
        public List<BillingInfoCounts> GetPatientBillingInfoCountsCycleBasedBilling(string billingCode, string cycle, int RoleId, string CreatedBy, string ConnectionString)
        {
            List<BillingInfoCounts> list = new List<BillingInfoCounts>();
            try
            {
                using (SqlConnection con = new SqlConnection(ConnectionString))
                {
                    con.Open();
                    string ProgramName = GetProgramNameByBillingCode(ConnectionString, billingCode);
                    string procedureName = string.Empty;
                    if (ProgramName=="RPM")
                    {
                        procedureName = "usp_GetPatientBillingInfo_CycleBasedBilling";
                    }
                    else
                    {
                        //for ccm,pcm patients
                        procedureName = "usp_GetPatientBillingInfoCCM_CycleBasedBilling";

                    }
                    using (SqlCommand command = new SqlCommand(procedureName, con))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.Add("@BillingCode", SqlDbType.NVarChar).Value = billingCode;
                        command.Parameters.Add("@cycle", SqlDbType.NVarChar).Value = cycle;
                        command.Parameters.Add("@CreatedBy", SqlDbType.NVarChar).Value = CreatedBy;
                        command.Parameters.Add("@RoleId", SqlDbType.NVarChar).Value = RoleId;
                        SqlDataReader reader = command.ExecuteReader();
                        while (reader.Read())
                        {
                            BillingInfoCounts info = new BillingInfoCounts();


                            info.Total = Convert.ToInt32(reader["Total"]);
                            info.TargetMet = Convert.ToInt32(reader["TargerMet"]);
                            info.ReadyToBill = Convert.ToInt32(reader["ReadyToBill"]);
                            info.CycleDate = Convert.ToString(reader["CycleDate"]);
                            info.TargetNotMet = Convert.ToInt32(reader["TargetNotMet"]);


                            list.Add(info);
                        }
                    }
                }
                return list;
            }
            catch (Exception)
            {

                throw;
            }

        }
        public List<BillingInfoCounts> GetPatientBillingInfoCountsDaysBasedBilling(string billingCode, string cycle, int RoleId, string CreatedBy, string ConnectionString)
        {
            List<BillingInfoCounts> list = new List<BillingInfoCounts>();
            try
            {
                using (SqlConnection con = new SqlConnection(ConnectionString))
                {
                    con.Open();
                    string ProgramName = GetProgramNameByBillingCode(ConnectionString, billingCode);
                    string procedureName = string.Empty;
                    if (ProgramName=="RPM")
                    {
                        procedureName = "usp_GetPatientBillingInfo_DaysBasedBilling";
                    }
                    else
                    {
                        //for ccm,pcm patients
                        procedureName = "usp_GetPatientBillingInfoCCM_DaysBasedBilling";

                    }
                    using (SqlCommand command = new SqlCommand(procedureName, con))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.Add("@BillingCode", SqlDbType.NVarChar).Value = billingCode;
                        command.Parameters.Add("@Filter", SqlDbType.NVarChar).Value = cycle;
                        command.Parameters.Add("@CreatedBy", SqlDbType.NVarChar).Value = CreatedBy;
                        command.Parameters.Add("@RoleId", SqlDbType.NVarChar).Value = RoleId;
                        SqlDataReader reader = command.ExecuteReader();
                        while (reader.Read())
                        {
                            BillingInfoCounts info = new BillingInfoCounts();
                            info.Total = Convert.ToInt32(reader["Total"]);
                            info.TargetMet = Convert.ToInt32(reader["TargerMet"]);
                            info.ReadyToBill = Convert.ToInt32(reader["ReadyToBill"]);
                            info.CycleDate = Convert.ToString(reader["CycleDate"]);
                            info.TargetNotMet = Convert.ToInt32(reader["TargetNotMet"]);


                            list.Add(info);
                        }
                    }
                }
                return list;
            }
            catch (Exception)
            {

                throw;
            }

        }
        public List<PatientDailyBillingData> GetPatientBillingCounts(int patientId, int patientPgmId, string connectionString)
        {
            try
            {
                //string ConnectionString = ConfigurationManager.ConnectionStrings[0].ToString();
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();

                    using (SqlCommand command = new SqlCommand("usp_GetPatientBillingCountsBypatient", con))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@PatientId", patientId);
                        command.Parameters.AddWithValue("@PatientPrigramId", patientPgmId);
                        SqlParameter returnParameter = command.Parameters.Add("RetVal", SqlDbType.Int);
                        returnParameter.Direction = ParameterDirection.ReturnValue;
                        SqlDataReader reader = command.ExecuteReader();

                        List<PatientDailyBillingData> PatientDailyBillingDataList = new List<PatientDailyBillingData>();
                        List<int> roleids = new List<int>();
                        while (reader.Read())
                        {
                            PatientDailyBillingData PatientDailyBillingData = new PatientDailyBillingData();
                            PatientDailyBillingData.PatientId = Convert.ToInt32(reader["PatientId"]);
                            PatientDailyBillingData.PatientProgramId = Convert.ToInt32(reader["PatientProgramId"]);
                            PatientDailyBillingData.BillingCodeId = Convert.ToInt32(reader["BillingCodeId"]);
                            PatientDailyBillingData.Status = reader["Status"].ToString();
                            PatientDailyBillingData.TotalVitalCount = Convert.ToInt32(reader["TotalVitalCount"]);
                            PatientDailyBillingData.TotalDuration = Convert.ToInt32(reader["TotalDuration"]);
                            PatientDailyBillingData.DaysCompleted = Convert.ToInt32(reader["DaysCompleted"]);
                            if (DBNull.Value.Equals(reader["StartDate"]))
                            {
                                PatientDailyBillingData.StartDate = null;
                            }
                            else
                            {
                                PatientDailyBillingData.StartDate = Convert.ToDateTime(reader["StartDate"]);
                            }

                            if (DBNull.Value.Equals(reader["LastBilledDate"]))
                            {
                                PatientDailyBillingData.LastBilledDate = null;
                            }
                            else
                            {
                                PatientDailyBillingData.LastBilledDate = Convert.ToDateTime(reader["LastBilledDate"]);
                            }
                            //PatientDailyBillingData.LastBilledDate = (DBNull.Value.Equals(reader["LastBilledDate"])) ? null: Convert.ToDateTime(reader["LastBilledDate"]); ;
                            PatientDailyBillingData.CreatedOn = Convert.ToDateTime(reader["CreatedOn"]);
                            PatientDailyBillingDataList.Add(PatientDailyBillingData);
                        }

                        if (reader.FieldCount == 0)
                        {
                            con.Close();
                            return null;
                        }
                        con.Close();
                        return PatientDailyBillingDataList;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception occured - class Name :" + this.GetType().Name + ",  Method Name : " + MethodBase.GetCurrentMethod().Name + ", Error Message :" + ex.Message + "");
                return null;
            }
        }

        public List<PatientInteraction> GetPatientInteractiontime(int patientId, int patientprogramId, string connectionString, DateTime startDate, DateTime enddate)
        {
            try
            {
                //string ConnectionString = ConfigurationManager.ConnectionStrings[0].ToString();
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();

                    using (SqlCommand command = new SqlCommand("usp_GetPatientCallOrReviewTimeLocal", con))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@patientProgramId", patientprogramId);
                        command.Parameters.AddWithValue("@patientId", patientId);
                        command.Parameters.AddWithValue("@startdate", startDate);
                        command.Parameters.AddWithValue("@enddate", enddate);
                        SqlParameter returnParameter = command.Parameters.Add("RetVal", SqlDbType.Int);
                        returnParameter.Direction = ParameterDirection.ReturnValue;
                        SqlDataReader reader = command.ExecuteReader();

                        List<PatientInteraction> PatientInteractiontime = new List<PatientInteraction>();
                        List<int> roleids = new List<int>();
                        while (reader.Read())
                        {
                            PatientInteraction PatientInteractiontimes = new PatientInteraction();
                            PatientInteractiontimes.Duration = Convert.ToInt32(reader["Duration"]);
                            PatientInteractiontimes.Date = Convert.ToDateTime(reader["Date"]);
                            PatientInteractiontimes.IsCallNote = Convert.ToInt32(reader["IsCallNote"]);
                            PatientInteractiontimes.IsEstablishedCall = Convert.ToInt32(reader["IsEstablishedCall"]);
                            PatientInteractiontime.Add(PatientInteractiontimes);
                        }

                        if (reader.FieldCount == 0)
                        {
                            con.Close();
                            return null;
                        }
                        con.Close();
                        return PatientInteractiontime;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception occured - class Name :" + this.GetType().Name + ",  Method Name : " + MethodBase.GetCurrentMethod().Name + ", Error Message :" + ex.Message + "");
                return null;
            }



        }
        public int GetPatientSmsCount(int patientId, string connectionString, DateTime startDate, DateTime enddate)
        {
            try
            {
                int count;
                //string ConnectionString = ConfigurationManager.ConnectionStrings[0].ToString();
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();


                    using (SqlCommand command = new SqlCommand("usp_GetPatientSmsCount", con))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@patientId", patientId);
                        command.Parameters.AddWithValue("@startdate", startDate);
                        command.Parameters.AddWithValue("@enddate", enddate);
                        //count = (int)command.ExecuteScalar();
                        SqlParameter returnParameter = command.Parameters.Add("RetVal", SqlDbType.Int);
                        returnParameter.Direction = ParameterDirection.ReturnValue;
                        SqlDataReader reader = command.ExecuteReader();
                        while (reader.Read())
                        {

                            return 1;

                        }
                        con.Close();
                        return 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception occured - class Name :" + this.GetType().Name + ",  Method Name : " + MethodBase.GetCurrentMethod().Name + ", Error Message :" + ex.Message + "");
                return 0;
            }



        }

        public List<VitalReading> GetVitalReadingsLocal(int patientProgramId, string connectionString, DateTime startDate, DateTime endDate)
        {

            try
            {
                //string ConnectionString = ConfigurationManager.ConnectionStrings[0].ToString();
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();

                    using (SqlCommand command = new SqlCommand("usp_GetVitalReadingsLocal", con))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@patientProgramId", patientProgramId);
                        command.Parameters.AddWithValue("@startdate", startDate);
                        command.Parameters.AddWithValue("@enddate", endDate);
                        SqlParameter returnParameter = command.Parameters.Add("RetVal", SqlDbType.Int);
                        returnParameter.Direction = ParameterDirection.ReturnValue;
                        SqlDataReader reader = command.ExecuteReader();

                        List<VitalReading> VitalReadingList = new List<VitalReading>();
                        List<int> roleids = new List<int>();
                        while (reader.Read())
                        {
                            VitalReading VitalReading = new VitalReading();
                            VitalReading.ReadingDate = Convert.ToDateTime(reader["ReadingDate"]);
                            VitalReading.Totalreadings = Convert.ToInt32(reader["Totalreadings"]);

                            VitalReadingList.Add(VitalReading);
                        }

                        if (reader.FieldCount == 0)
                        {
                            con.Close();
                            return null;
                        }
                        con.Close();
                        return VitalReadingList;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception occured - class Name :" + this.GetType().Name + ",  Method Name : " + MethodBase.GetCurrentMethod().Name + ", Error Message :" + ex.Message + "");
                return null;
            }



        }
        public static DateTime GetLocalTimeFromUTC(DateTime dt, string con)
        {
            List<SystemConfigInfo> configData = GetSystemConfig(con, "Provider", String.Empty);
            if (configData == null || configData.Count <= 0)
            {
                throw new Exception("Provider time zone not set.");
            }
            SystemConfigInfo tz = configData.Find(x => x.Name == "TimeZone");
            if (tz == null) throw new Exception("Provider time zone not set.");

            TimeZoneInfo timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(tz.Value);
            DateTime newUtc = TimeZoneInfo.ConvertTimeFromUtc(dt, timeZoneInfo);
            return newUtc;
        }
        public static DateTime GetUTCFromLocalTime(DateTime dt, string con)
        {
            List<SystemConfigInfo> configData = GetSystemConfig(con, "Provider", String.Empty);
            if (configData == null || configData.Count <= 0)
            {
                throw new Exception("Provider time zone not set.");
            }
            SystemConfigInfo tz = configData.Find(x => x.Name == "TimeZone");
            if (tz == null) throw new Exception("Provider time zone not set.");

            TimeZoneInfo timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(tz.Value);
            DateTime newUtc = TimeZoneInfo.ConvertTimeToUtc(dt, timeZoneInfo);
            return newUtc;
        }

        public static List<SystemConfigInfo> GetSystemConfig(string cs, string category, string createdby)
        {
            List<SystemConfigInfo> ret = new List<SystemConfigInfo>();
            try
            {
                using (SqlConnection connection = new SqlConnection(cs))
                {
                    //string query = "select * from SystemConfigurations where Category='iGlucose'";
                    SqlCommand command = new SqlCommand("usp_GetSystemConfig", connection);
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add("@Category", SqlDbType.NVarChar).Value = category;
                    command.Parameters.Add("@CreatedBy", SqlDbType.NVarChar).Value = createdby;
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        SystemConfigInfo info = new SystemConfigInfo();
                        info.Name = !reader.IsDBNull(reader.GetOrdinal("name")) ? reader["name"].ToString() : string.Empty;
                        info.Value = !reader.IsDBNull(reader.GetOrdinal("Value")) ? reader["Value"].ToString() : string.Empty;
                        info.Descripiton = !reader.IsDBNull(reader.GetOrdinal("Description")) ? reader["Description"].ToString() : string.Empty;
                        ret.Add(info);
                    }
                }
            }
            catch
            {
                throw;
            }
            return ret;
        }
        public PatientBilldata ProcessCPTG0506_DaysBasedBilling(int patientId, int patientProgramId, int targetReading, BillingCodesDetails code, string connectionString)
        {
            try
            {
                PatientBilldata PatientBilldata = new PatientBilldata();
                PatientStartDate patientStartdate = GetBillingStartDateEx(patientId, patientProgramId, code.BillingCodeID, connectionString);
                //Startdate is null means already billed in the patient billing table for 53 code.
                if (patientStartdate.Status == "BilledDate")
                {

                    PatientBilldata.BillingStartDate = (DateTime)patientStartdate.StartDate;
                    PatientBilldata.CPTCode = code.BillingCode;
                    PatientBilldata.Completed = targetReading;
                    PatientBilldata.Total = targetReading;
                    PatientBilldata.IsTargetMet = true;
                    PatientBilldata.ReadyTobill = 1;
                    return PatientBilldata;
                }

                List<PatientDailyBillingData> lstDailyData = GetPatientBillingCounts(patientId, patientProgramId, connectionString);
                if (lstDailyData == null || lstDailyData.Count == 0)
                {

                    PatientBilldata = null;
                }

                foreach (PatientDailyBillingData pdb in lstDailyData)
                {
                    if (pdb.BillingCodeId == code.BillingCodeID)
                    {
                        PatientBilldata = new PatientBilldata();
                        PatientBilldata.BillingStartDate = (DateTime)pdb.StartDate;
                        PatientBilldata.CPTCode = code.BillingCode;
                        PatientBilldata.Completed = pdb.TotalDuration;
                        PatientBilldata.Total = code.TargetReadings*60;
                        PatientBilldata.IsTargetMet = pdb.TotalVitalCount >= code.TargetReadings ? true : false; ;
                        PatientBilldata.ReadyTobill = (pdb.TotalVitalCount >= code.TargetReadings && pdb.DaysCompleted >= code.BillingThreshold) ? 1 : 0;
                        return PatientBilldata;

                    }
                    else
                    {
                        PatientBilldata = null;
                    }

                }

                if (PatientBilldata == null)
                {

                    PatientBilldata = new PatientBilldata();
                    PatientBilldata.BillingStartDate = Convert.ToDateTime(patientStartdate.StartDate);
                    PatientBilldata.CPTCode = code.BillingCode;
                    int VitalCount = 0;
                    int DaysCompleted = 0;
                    PatientBilldata.Completed = VitalCount;
                    PatientBilldata.Total = code.TargetReadings*60;
                    PatientBilldata.IsTargetMet = (VitalCount >= code.TargetReadings) ? true : false;
                    PatientBilldata.ReadyTobill = (VitalCount >= code.TargetReadings && DaysCompleted >= code.BillingThreshold) ? 1 : 0;
                    return PatientBilldata;

                }

                return PatientBilldata;


            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception occured - class Name :" + this.GetType().Name + ",  Method Name : " + MethodBase.GetCurrentMethod().Name + ", Error Message :" + ex.Message + "");
                return null;
            }
        }

        public PatientBilldata ProcessCPT99490_DaysBasedBilling(int patientId, int patientProgramId, BillingCodesDetails code, string connectionString)
        {
            PatientBilldata pbd = new PatientBilldata();
            try
            {
                int DaysCompleted = 0; int TotalReading = 0;
                PatientStartDate PatientStartDate = new PatientStartDate();
                PatientStartDate = GetPatientBillingStartDateEx(patientId, patientProgramId, code.BillingCodeID, connectionString);
                if (PatientStartDate == null ||
                    PatientStartDate.Status.ToLower() == "invalid")
                {
                    pbd.CPTCode = code.BillingCode;
                    pbd.Completed = 0;
                    pbd.Total = code.TargetReadings*60;
                    pbd.IsTargetMet = false;
                    pbd.ReadyTobill = 0;
                    return pbd;
                }
                else if (PatientStartDate.StartDate > DateTime.UtcNow.Date && PatientStartDate.Status.ToLower() == "active")
                {
                    PatientBilldata PatientBilldata = new PatientBilldata();
                    PatientBilldata.CPTCode = code.BillingCode;
                    PatientBilldata.Completed = 0;
                    PatientBilldata.Total = code.TargetReadings*60;
                    PatientBilldata.IsTargetMet = false;
                    PatientBilldata.ReadyTobill = 0;
                    return PatientBilldata;
                }
                else
                {

                    DateTime stDate = DateTime.MinValue;
                    if (PatientStartDate.Status.ToLower() == "active")
                    {
                        stDate = Convert.ToDateTime(PatientStartDate.StartDate).AddDays(1);
                    }
                    else
                    {
                        stDate = Convert.ToDateTime(PatientStartDate.StartDate);
                        stDate = Convert.ToDateTime(PatientStartDate.StartDate);
                    }
                    DateTime endDate = DateTime.UtcNow;

                    //Note: This will happen in the last day of billing.. In the last day, billed date will set in 
                    //the patientbilling table and +1 day will come as next start day.. So we should   
                    // calcualte the start date with respect to the billing threshold
                    DateTime today = DateTime.UtcNow;
                    DateTime endDateTempay = GetLocalTimeFromUTC((DateTime)today, connectionString);
                    if (stDate.Date > endDateTempay)
                    {
                        //stDate = stDate.AddDays(-1 * code.BillingThreshold);
                        BilledDates bd = GetPatientLastBilledPeriods(patientId, patientProgramId, code.BillingCodeID, connectionString);

                        if (bd== null)
                        {
                            PatientBilldata PatientBilldata = new PatientBilldata();
                            PatientBilldata.CPTCode = code.BillingCode;
                            PatientBilldata.Completed = 0;
                            PatientBilldata.Total = code.TargetReadings*60;
                            PatientBilldata.IsTargetMet = false;
                            PatientBilldata.ReadyTobill = 0;
                            return PatientBilldata;
                        }
                        stDate = bd.StartDate;

                    }



                    if (PatientStartDate.Status.ToLower() == "billeddate")
                    {
                        stDate = GetUTCFromLocalTime((DateTime)stDate, connectionString);
                        endDate =DateTime.UtcNow;

                    }


                    List<PatientInteraction> PatientInteractiontim = GetPatientInteractiontime(patientId, patientProgramId, connectionString, stDate, endDate).ToList();
                    if (PatientInteractiontim == null)
                    {
                        pbd.CPTCode = code.BillingCode;
                        pbd.Completed = 0;
                        pbd.Total = code.TargetReadings*60;
                        pbd.IsTargetMet = false;
                        pbd.ReadyTobill = 0;
                        pbd.BillingStartDate = (DateTime)stDate;
                        return pbd;
                    }
                    //var isEstablishedExist = PatientInteractiontim.Where(s => s.IsCallNote == 1).ToList();
                    //if (isEstablishedExist.Count() > 0)
                    //{
                    //    isEstablishedExist = isEstablishedExist.Where(s => s.IsEstablishedCall == 1).ToList();
                    //    if (isEstablishedExist != null && isEstablishedExist.Count() == 0)
                    //    {
                    //        PatientInteractiontim.Clear();
                    //    }
                    //}
                    if (PatientInteractiontim.Count == 0 || PatientInteractiontim == null /*|| isEstablishedExist.Count == 0*/)
                    {
                        pbd.CPTCode = code.BillingCode;
                        pbd.Completed = 0;
                        pbd.Total = code.TargetReadings*60;
                        pbd.IsTargetMet = false;
                        pbd.ReadyTobill = 0;
                        pbd.BillingStartDate = (DateTime)stDate;
                        return pbd;
                    }
                    TotalReading = PatientInteractiontim.Sum(s => s.Duration);

                    if (TotalReading >= code.TargetReadings * 60)
                    {
                        TotalReading = code.TargetReadings * 60;
                    }
                    pbd.BillingStartDate = (DateTime)stDate;
                    pbd.CPTCode = code.BillingCode;
                    pbd.Completed = TotalReading;
                    pbd.Total = code.TargetReadings * 60;
                    pbd.IsTargetMet = (TotalReading >= code.TargetReadings * 60) ? true : false;
                    pbd.ReadyTobill = (TotalReading >= (code.TargetReadings * 60) && DaysCompleted >= code.BillingThreshold) ? 1 : 0;
                    return pbd;
                }
            }
            catch (Exception ex)
            {
                throw;
            }

        }
        public PatientBilldata ProcessCPT99439_DaysBasedBilling(int patientId, int patientProgramId, BillingCodesDetails code,
                                                     BillingCodesDetails code490, string connectionString)
        {
            PatientBilldata pbd = new PatientBilldata();
            try
            {
                int DaysCompleted = 0; int TotalReading = 0;
                PatientStartDate PatientStartDate = new PatientStartDate();
                PatientStartDate = GetPatientBillingStartDateEx(patientId, patientProgramId, code.BillingCodeID, connectionString);
                if (PatientStartDate == null ||
                    PatientStartDate.Status.ToLower() == "invalid")
                {
                    pbd.CPTCode = code.BillingCode;
                    pbd.Completed = 0;
                    pbd.Total = code.TargetReadings*60;
                    pbd.IsTargetMet = false;
                    pbd.ReadyTobill = 0;
                    return pbd;
                }
                else if (PatientStartDate.StartDate > DateTime.UtcNow.Date && PatientStartDate.Status.ToLower() == "active")
                {
                    PatientBilldata PatientBilldata = new PatientBilldata();
                    PatientBilldata.CPTCode = code.BillingCode;
                    PatientBilldata.Completed = 0;
                    PatientBilldata.Total = code.TargetReadings;
                    PatientBilldata.IsTargetMet = false;
                    PatientBilldata.ReadyTobill = 0;
                    return PatientBilldata;
                }
                else
                {
                    DateTime stDate = DateTime.MinValue;
                    if (PatientStartDate.Status.ToLower() == "active")
                    {
                        stDate = Convert.ToDateTime(PatientStartDate.StartDate).AddDays(1);
                    }
                    else
                    {
                        stDate = Convert.ToDateTime(PatientStartDate.StartDate);
                        stDate = Convert.ToDateTime(PatientStartDate.StartDate);
                    }
                    DateTime endDate = DateTime.UtcNow;
                    //Note: This will happen in the last day of billing.. In the last day, billed date will set in 
                    //the patientbilling table and +1 day will come as next start day.. So we should   
                    // calcualte the start date with respect to the billing threshold
                    DateTime today = DateTime.UtcNow;
                    DateTime endDateTempay = GetLocalTimeFromUTC((DateTime)today, connectionString);
                    if (stDate.Date > endDateTempay)
                    {
                        //stDate = stDate.AddDays(-1 * code.BillingThreshold);
                        BilledDates bd = GetPatientLastBilledPeriods(patientId, patientProgramId, code.BillingCodeID, connectionString);

                        //BilledDates bd = GetPatientLastBilledPeriods(patientId, patientProgramId, code.BillingCodeID, connectionString);

                        if (bd== null)
                        {
                            PatientBilldata PatientBilldata = new PatientBilldata();
                            PatientBilldata.CPTCode = code.BillingCode;
                            PatientBilldata.Completed = 0;
                            PatientBilldata.Total = code.TargetReadings*60;
                            PatientBilldata.IsTargetMet = false;
                            PatientBilldata.ReadyTobill = 0;
                            return PatientBilldata;
                        }
                        stDate = bd.StartDate; stDate = bd.StartDate;
                        endDate = bd.EndDate;
                    }

                    if (PatientStartDate.Status.ToLower() == "billeddate")
                    {
                        stDate = GetUTCFromLocalTime((DateTime)stDate, connectionString);
                        endDate =DateTime.UtcNow;
                    }
                    List<PatientInteraction> PatientInteractiontim = GetPatientInteractiontime(patientId, patientProgramId, connectionString, stDate, endDate).ToList();
                    if (PatientInteractiontim == null)
                    {
                        pbd.CPTCode = code.BillingCode;
                        pbd.Completed = 0;
                        pbd.Total = code.TargetReadings*60;
                        pbd.IsTargetMet = false;
                        pbd.ReadyTobill = 0;
                        pbd.BillingStartDate = (DateTime)stDate;
                        return pbd;
                    }
                    //var isEstablishedExist = PatientInteractiontim.Where(s => s.IsCallNote == 1).ToList();
                    //if (isEstablishedExist.Count() > 0)
                    //{
                    //    isEstablishedExist = isEstablishedExist.Where(s => s.IsEstablishedCall == 1).ToList();
                    //    if (isEstablishedExist != null && isEstablishedExist.Count() == 0)
                    //    {
                    //        PatientInteractiontim.Clear();
                    //    }
                    //}
                    if (PatientInteractiontim.Count == 0 || PatientInteractiontim == null /*|| isEstablishedExist.Count == 0*/)
                    {
                        pbd.CPTCode = code.BillingCode;
                        pbd.Completed = 0;
                        pbd.Total = code.TargetReadings*60;
                        pbd.IsTargetMet = false;
                        pbd.ReadyTobill = 0;
                        pbd.BillingStartDate = (DateTime)stDate;
                        return pbd;
                    }
                    TotalReading = PatientInteractiontim.Sum(s => s.Duration);

                    if (TotalReading > code490.TargetReadings * 60)
                    {
                        TimeSpan t1 = TimeSpan.FromSeconds(TotalReading);
                        TotalReading = (int)(t1.TotalSeconds - (code490.TargetReadings * 60));

                        pbd.BillingStartDate = (DateTime)stDate;
                        pbd.CPTCode = code.BillingCode;
                        pbd.Completed = TotalReading;
                        if (TotalReading < code490.TargetReadings * 60)
                        {
                            // base 20
                            pbd.Total = code490.TargetReadings * 60;
                            pbd.ReadyTobill = (TotalReading >= (code490.TargetReadings * 60) && DaysCompleted >= code.BillingThreshold) ? 1 : 0;
                        }
                        else
                        {
                            //base 40
                            pbd.Total = code.TargetReadings * 60;
                            pbd.ReadyTobill = (TotalReading >= (code.TargetReadings * 60) && DaysCompleted >= code.BillingThreshold) ? 1 : 0;
                        }
                        pbd.IsTargetMet = (TotalReading >= code490.TargetReadings * 60) ? true : false;
                    }
                    else
                    {
                        //Target reading 20
                        pbd.BillingStartDate = (DateTime)stDate;
                        pbd.CPTCode = code.BillingCode;
                        pbd.Completed = 0;
                        pbd.Total = code490.TargetReadings * 60;
                        pbd.IsTargetMet = (TotalReading >= code490.TargetReadings * 60) ? true : false;
                        pbd.ReadyTobill = (TotalReading >= (code490.TargetReadings * 60) && DaysCompleted >= code.BillingThreshold) ? 1 : 0;

                    }
                    return pbd;
                }
            }
            catch (Exception ex)
            {
                throw;
            }

        }


        public PatientBilldata ProcessCPT99491_DaysBasedBilling(int patientId, int patientProgramId, BillingCodesDetails code, string connectionString)
        {
            PatientBilldata pbd = new PatientBilldata();
            try
            {
                int DaysCompleted = 0; int TotalReading = 0;
                PatientStartDate PatientStartDate = new PatientStartDate();
                PatientStartDate = GetPatientBillingStartDateEx(patientId, patientProgramId, code.BillingCodeID, connectionString);
                if (PatientStartDate == null ||
                    PatientStartDate.Status.ToLower() == "invalid")
                {
                    pbd.CPTCode = code.BillingCode;
                    pbd.Completed = 0;
                    pbd.Total = code.TargetReadings*60;
                    pbd.IsTargetMet = false;
                    pbd.ReadyTobill = 0;
                    return pbd;
                }
                else if (PatientStartDate.StartDate > DateTime.UtcNow.Date && PatientStartDate.Status.ToLower() == "active")
                {
                    PatientBilldata PatientBilldata = new PatientBilldata();
                    PatientBilldata.CPTCode = code.BillingCode;
                    PatientBilldata.Completed = 0;
                    PatientBilldata.Total = code.TargetReadings*60;
                    PatientBilldata.IsTargetMet = false;
                    PatientBilldata.ReadyTobill = 0;
                    return PatientBilldata;
                }
                else
                {

                    DateTime stDate = DateTime.MinValue;
                    if (PatientStartDate.Status.ToLower() == "active")
                    {
                        stDate = Convert.ToDateTime(PatientStartDate.StartDate).AddDays(1);
                    }
                    else
                    {
                        stDate = Convert.ToDateTime(PatientStartDate.StartDate);
                    }
                    DateTime endDate = DateTime.UtcNow;

                    //Note: This will happen in the last day of billing.. In the last day, billed date will set in 
                    //the patientbilling table and +1 day will come as next start day.. So we should   
                    // calcualte the start date with respect to the billing threshold
                    DateTime today = DateTime.UtcNow;
                    DateTime endDateTempay = GetLocalTimeFromUTC((DateTime)today, connectionString);
                    if (stDate.Date > endDateTempay)
                    {
                        //stDate = stDate.AddDays(-1 * code.BillingThreshold);
                        BilledDates bd = GetPatientLastBilledPeriods(patientId, patientProgramId, code.BillingCodeID, connectionString);


                        if (bd== null)
                        {
                            PatientBilldata PatientBilldata = new PatientBilldata();
                            PatientBilldata.CPTCode = code.BillingCode;
                            PatientBilldata.Completed = 0;
                            PatientBilldata.Total = code.TargetReadings*60;
                            PatientBilldata.IsTargetMet = false;
                            PatientBilldata.ReadyTobill = 0;
                            return PatientBilldata;
                        }
                        stDate = bd.StartDate;
                        stDate = bd.StartDate;

                    }



                    if (PatientStartDate.Status.ToLower() == "billeddate")
                    {
                        stDate = GetUTCFromLocalTime((DateTime)stDate, connectionString);
                        endDate =DateTime.UtcNow;

                    }


                    List<PatientInteraction> PatientInteractiontim = GetPatientInteractiontime(patientId, patientProgramId, connectionString, stDate, endDate).ToList();
                    if (PatientInteractiontim == null)
                    {
                        pbd.CPTCode = code.BillingCode;
                        pbd.Completed = 0;
                        pbd.Total = code.TargetReadings*60;
                        pbd.IsTargetMet = false;
                        pbd.ReadyTobill = 0;
                        pbd.BillingStartDate = (DateTime)stDate;
                        return pbd;
                    }
                    //var isEstablishedExist = PatientInteractiontim.Where(s => s.IsCallNote == 1).ToList();
                    //if (isEstablishedExist.Count() > 0)
                    //{
                    //    isEstablishedExist = isEstablishedExist.Where(s => s.IsEstablishedCall == 1).ToList();
                    //    if (isEstablishedExist != null && isEstablishedExist.Count() == 0)
                    //    {
                    //        PatientInteractiontim.Clear();
                    //    }
                    //}
                    if (PatientInteractiontim.Count == 0 || PatientInteractiontim == null /*|| isEstablishedExist.Count == 0*/)
                    {
                        pbd.CPTCode = code.BillingCode;
                        pbd.Completed = 0;
                        pbd.Total = code.TargetReadings*60;
                        pbd.IsTargetMet = false;
                        pbd.ReadyTobill = 0;
                        pbd.BillingStartDate = (DateTime)stDate;
                        return pbd;
                    }
                    TotalReading = PatientInteractiontim.Sum(s => s.Duration);

                    if (TotalReading >= code.TargetReadings * 60)
                    {
                        TotalReading = code.TargetReadings * 60;
                    }
                    pbd.BillingStartDate = (DateTime)stDate;
                    pbd.CPTCode = code.BillingCode;
                    pbd.Completed = TotalReading;
                    pbd.Total = code.TargetReadings * 60;
                    pbd.IsTargetMet = (TotalReading >= code.TargetReadings * 60) ? true : false;
                    pbd.ReadyTobill = (TotalReading >= (code.TargetReadings * 60) && DaysCompleted >= code.BillingThreshold) ? 1 : 0;
                    return pbd;
                }
            }
            catch (Exception ex)
            {
                throw;
            }

        }

        public PatientBilldata ProcessCPT99437_DaysBasedBilling(int patientId, int patientProgramId, BillingCodesDetails code,
                                                    BillingCodesDetails code491, string connectionString)
        {
            PatientBilldata pbd = new PatientBilldata();
            try
            {
                int DaysCompleted = 0; int TotalReading = 0;
                PatientStartDate PatientStartDate = new PatientStartDate();
                PatientStartDate = GetPatientBillingStartDateEx(patientId, patientProgramId, code.BillingCodeID, connectionString);
                if (PatientStartDate == null ||
                    PatientStartDate.Status.ToLower() == "invalid")
                {
                    pbd.CPTCode = code.BillingCode;
                    pbd.Completed = 0;
                    pbd.Total = code.TargetReadings*60;
                    pbd.IsTargetMet = false;
                    pbd.ReadyTobill = 0;
                    return pbd;
                }
                else if (PatientStartDate.StartDate > DateTime.UtcNow.Date && PatientStartDate.Status.ToLower() == "active")
                {
                    PatientBilldata PatientBilldata = new PatientBilldata();
                    PatientBilldata.CPTCode = code.BillingCode;
                    PatientBilldata.Completed = 0;
                    PatientBilldata.Total = code.TargetReadings*60;
                    PatientBilldata.IsTargetMet = false;
                    PatientBilldata.ReadyTobill = 0;
                    return PatientBilldata;
                }
                else
                {
                    DateTime stDate = DateTime.MinValue;
                    if (PatientStartDate.Status.ToLower() == "active")
                    {
                        stDate = Convert.ToDateTime(PatientStartDate.StartDate).AddDays(1);
                    }
                    else
                    {
                        stDate = Convert.ToDateTime(PatientStartDate.StartDate);
                    }
                    DateTime endDate = DateTime.UtcNow;
                    //Note: This will happen in the last day of billing.. In the last day, billed date will set in 
                    //the patientbilling table and +1 day will come as next start day.. So we should   
                    // calcualte the start date with respect to the billing threshold
                    DateTime today = DateTime.UtcNow;
                    DateTime endDateTempay = GetLocalTimeFromUTC((DateTime)today, connectionString);
                    if (stDate.Date > endDateTempay)
                    {
                        //stDate = stDate.AddDays(-1 * code.BillingThreshold);
                        BilledDates bd = GetPatientLastBilledPeriods(patientId, patientProgramId, code.BillingCodeID, connectionString);


                        if (bd== null)
                        {
                            PatientBilldata PatientBilldata = new PatientBilldata();
                            PatientBilldata.CPTCode = code.BillingCode;
                            PatientBilldata.Completed = 0;
                            PatientBilldata.Total = code.TargetReadings*60;
                            PatientBilldata.IsTargetMet = false;
                            PatientBilldata.ReadyTobill = 0;
                            return PatientBilldata;
                        }
                        stDate = bd.StartDate;
                        stDate = bd.StartDate;
                        endDate = bd.EndDate;
                    }

                    if (PatientStartDate.Status.ToLower() == "billeddate")
                    {
                        stDate = GetUTCFromLocalTime((DateTime)stDate, connectionString);
                        endDate =DateTime.UtcNow;
                    }
                    List<PatientInteraction> PatientInteractiontim = GetPatientInteractiontime(patientId, patientProgramId, connectionString, stDate, endDate).ToList();
                    if (PatientInteractiontim == null)
                    {
                        pbd.CPTCode = code.BillingCode;
                        pbd.Completed = 0;
                        pbd.Total = code.TargetReadings*60;
                        pbd.IsTargetMet = false;
                        pbd.ReadyTobill = 0;
                        pbd.BillingStartDate = (DateTime)stDate;
                        return pbd;
                    }
                    //var isEstablishedExist = PatientInteractiontim.Where(s => s.IsCallNote == 1).ToList();
                    //if (isEstablishedExist.Count() > 0)
                    //{
                    //    isEstablishedExist = isEstablishedExist.Where(s => s.IsEstablishedCall == 1).ToList();
                    //    if (isEstablishedExist != null && isEstablishedExist.Count() == 0)
                    //    {
                    //        PatientInteractiontim.Clear();
                    //    }
                    //}
                    if (PatientInteractiontim.Count == 0 || PatientInteractiontim == null /*|| isEstablishedExist.Count == 0*/)
                    {
                        pbd.CPTCode = code.BillingCode;
                        pbd.Completed = 0;
                        pbd.Total = code.TargetReadings*60;
                        pbd.IsTargetMet = false;
                        pbd.ReadyTobill = 0;
                        pbd.BillingStartDate = (DateTime)stDate;
                        return pbd;
                    }
                    TotalReading = PatientInteractiontim.Sum(s => s.Duration);

                    if (TotalReading > code491.TargetReadings * 60)
                    {
                        TimeSpan t1 = TimeSpan.FromSeconds(TotalReading);
                        TotalReading = (int)(t1.TotalSeconds - (code491.TargetReadings * 60));

                        pbd.BillingStartDate = (DateTime)stDate;
                        pbd.CPTCode = code.BillingCode;
                        pbd.Completed = TotalReading;
                        if (TotalReading < code491.TargetReadings * 60)
                        {
                            // base 20
                            pbd.Total = code491.TargetReadings * 60;
                            pbd.ReadyTobill = (TotalReading >= (code491.TargetReadings * 60) && DaysCompleted >= code.BillingThreshold) ? 1 : 0;
                        }
                        else
                        {
                            //base 40
                            pbd.Total = code.TargetReadings * 60;
                            pbd.ReadyTobill = (TotalReading >= (code.TargetReadings * 60) && DaysCompleted >= code.BillingThreshold) ? 1 : 0;
                        }
                        pbd.IsTargetMet = (TotalReading >= code491.TargetReadings * 60) ? true : false;
                    }
                    else
                    {
                        //Target reading 20
                        pbd.BillingStartDate = (DateTime)stDate;
                        pbd.CPTCode = code.BillingCode;
                        pbd.Completed = 0;
                        pbd.Total = code491.TargetReadings * 60;
                        pbd.IsTargetMet = (TotalReading >= code491.TargetReadings * 60) ? true : false;
                        pbd.ReadyTobill = (TotalReading >= (code491.TargetReadings * 60) && DaysCompleted >= code.BillingThreshold) ? 1 : 0;

                    }
                    return pbd;
                }
            }
            catch (Exception ex)
            {
                throw;
            }

        }
        public PatientBilldata ProcessCPT99487_DaysBasedBilling(int patientId, int patientProgramId, BillingCodesDetails code, string connectionString)
        {
            PatientBilldata pbd = new PatientBilldata();
            try
            {
                int DaysCompleted = 0; int TotalReading = 0;
                PatientStartDate PatientStartDate = new PatientStartDate();
                PatientStartDate = GetPatientBillingStartDateEx(patientId, patientProgramId, code.BillingCodeID, connectionString);
                if (PatientStartDate == null ||
                    PatientStartDate.Status.ToLower() == "invalid")
                {
                    pbd.CPTCode = code.BillingCode;
                    pbd.Completed = 0;
                    pbd.Total = code.TargetReadings*60;
                    pbd.IsTargetMet = false;
                    pbd.ReadyTobill = 0;
                    return pbd;
                }
                else if (PatientStartDate.StartDate > DateTime.UtcNow.Date && PatientStartDate.Status.ToLower() == "active")
                {
                    PatientBilldata PatientBilldata = new PatientBilldata();
                    PatientBilldata.CPTCode = code.BillingCode;
                    PatientBilldata.Completed = 0;
                    PatientBilldata.Total = code.TargetReadings*60;
                    PatientBilldata.IsTargetMet = false;
                    PatientBilldata.ReadyTobill = 0;
                    return PatientBilldata;
                }
                else
                {

                    DateTime stDate = DateTime.MinValue;
                    if (PatientStartDate.Status.ToLower() == "active")
                    {
                        stDate = Convert.ToDateTime(PatientStartDate.StartDate).AddDays(1);
                    }
                    else
                    {
                        stDate = Convert.ToDateTime(PatientStartDate.StartDate);
                    }
                    DateTime endDate = DateTime.UtcNow;

                    //Note: This will happen in the last day of billing.. In the last day, billed date will set in 
                    //the patientbilling table and +1 day will come as next start day.. So we should   
                    // calcualte the start date with respect to the billing threshold
                    DateTime today = DateTime.UtcNow;
                    DateTime endDateTempay = GetLocalTimeFromUTC((DateTime)today, connectionString);
                    if (stDate.Date > endDateTempay)
                    {
                        //stDate = stDate.AddDays(-1 * code.BillingThreshold);
                        BilledDates bd = GetPatientLastBilledPeriods(patientId, patientProgramId, code.BillingCodeID, connectionString);


                        if (bd== null)
                        {
                            PatientBilldata PatientBilldata = new PatientBilldata();
                            PatientBilldata.CPTCode = code.BillingCode;
                            PatientBilldata.Completed = 0;
                            PatientBilldata.Total = code.TargetReadings*60;
                            PatientBilldata.IsTargetMet = false;
                            PatientBilldata.ReadyTobill = 0;
                            return PatientBilldata;
                        }
                        stDate = bd.StartDate;
                        stDate = bd.StartDate;

                    }



                    if (PatientStartDate.Status.ToLower() == "billeddate")
                    {
                        stDate = GetUTCFromLocalTime((DateTime)stDate, connectionString);
                        endDate =DateTime.UtcNow;

                    }


                    List<PatientInteraction> PatientInteractiontim = GetPatientInteractiontime(patientId, patientProgramId, connectionString, stDate, endDate).ToList();
                    if (PatientInteractiontim == null)
                    {
                        pbd.CPTCode = code.BillingCode;
                        pbd.Completed = 0;
                        pbd.Total = code.TargetReadings*60;
                        pbd.IsTargetMet = false;
                        pbd.ReadyTobill = 0;
                        pbd.BillingStartDate = (DateTime)stDate;
                        return pbd;
                    }
                    //var isEstablishedExist = PatientInteractiontim.Where(s => s.IsCallNote == 1).ToList();
                    //if (isEstablishedExist.Count() > 0)
                    //{
                    //    isEstablishedExist = isEstablishedExist.Where(s => s.IsEstablishedCall == 1).ToList();
                    //    if (isEstablishedExist != null && isEstablishedExist.Count() == 0)
                    //    {
                    //        PatientInteractiontim.Clear();
                    //    }
                    //}
                    if (PatientInteractiontim.Count == 0 || PatientInteractiontim == null /*|| isEstablishedExist.Count == 0*/)
                    {
                        pbd.CPTCode = code.BillingCode;
                        pbd.Completed = 0;
                        pbd.Total = code.TargetReadings*60;
                        pbd.IsTargetMet = false;
                        pbd.ReadyTobill = 0;
                        pbd.BillingStartDate = (DateTime)stDate;
                        return pbd;
                    }
                    TotalReading = PatientInteractiontim.Sum(s => s.Duration);

                    if (TotalReading >= code.TargetReadings * 60)
                    {
                        TotalReading = code.TargetReadings * 60;
                    }
                    pbd.BillingStartDate = (DateTime)stDate;
                    pbd.CPTCode = code.BillingCode;
                    pbd.Completed = TotalReading;
                    pbd.Total = code.TargetReadings * 60;
                    pbd.IsTargetMet = (TotalReading >= code.TargetReadings * 60) ? true : false;
                    pbd.ReadyTobill = (TotalReading >= (code.TargetReadings * 60) && DaysCompleted >= code.BillingThreshold) ? 1 : 0;
                    return pbd;
                }
            }
            catch (Exception ex)
            {
                throw;
            }

        }
        public PatientBilldata ProcessCPT99489_DaysBasedBilling(int patientId, int patientProgramId, BillingCodesDetails code,
                                                    BillingCodesDetails code487, string connectionString)
        {
            PatientBilldata pbd = new PatientBilldata();
            try
            {
                int DaysCompleted = 0; int TotalReading = 0;
                PatientStartDate PatientStartDate = new PatientStartDate();
                PatientStartDate = GetPatientBillingStartDateEx(patientId, patientProgramId, code.BillingCodeID, connectionString);
                if (PatientStartDate == null ||
                    PatientStartDate.Status.ToLower() == "invalid")
                {
                    pbd.CPTCode = code.BillingCode;
                    pbd.Completed = 0;
                    pbd.Total = code.TargetReadings*60;
                    pbd.IsTargetMet = false;
                    pbd.ReadyTobill = 0;
                    return pbd;
                }
                else if (PatientStartDate.StartDate > DateTime.UtcNow.Date && PatientStartDate.Status.ToLower() == "active")
                {
                    PatientBilldata PatientBilldata = new PatientBilldata();
                    PatientBilldata.CPTCode = code.BillingCode;
                    PatientBilldata.Completed = 0;
                    PatientBilldata.Total = code.TargetReadings*60;
                    PatientBilldata.IsTargetMet = false;
                    PatientBilldata.ReadyTobill = 0;
                    return PatientBilldata;
                }
                else
                {
                    DateTime stDate = DateTime.MinValue;
                    if (PatientStartDate.Status.ToLower() == "active")
                    {
                        stDate = Convert.ToDateTime(PatientStartDate.StartDate).AddDays(1);
                    }
                    else
                    {
                        stDate = Convert.ToDateTime(PatientStartDate.StartDate);
                    }
                    DateTime endDate = DateTime.UtcNow;
                    //Note: This will happen in the last day of billing.. In the last day, billed date will set in 
                    //the patientbilling table and +1 day will come as next start day.. So we should   
                    // calcualte the start date with respect to the billing threshold
                    DateTime today = DateTime.UtcNow;
                    DateTime endDateTempay = GetLocalTimeFromUTC((DateTime)today, connectionString);
                    if (stDate.Date > endDateTempay)
                    {
                        //stDate = stDate.AddDays(-1 * code.BillingThreshold);
                        BilledDates bd = GetPatientLastBilledPeriods(patientId, patientProgramId, code.BillingCodeID, connectionString);



                        if (bd== null)
                        {
                            PatientBilldata PatientBilldata = new PatientBilldata();
                            PatientBilldata.CPTCode = code.BillingCode;
                            PatientBilldata.Completed = 0;
                            PatientBilldata.Total = code.TargetReadings*60;
                            PatientBilldata.IsTargetMet = false;
                            PatientBilldata.ReadyTobill = 0;
                            return PatientBilldata;
                        }
                        stDate = bd.StartDate; stDate = bd.StartDate;
                        endDate = bd.EndDate;
                    }

                    if (PatientStartDate.Status.ToLower() == "billeddate")
                    {
                        stDate = GetUTCFromLocalTime((DateTime)stDate, connectionString);
                        endDate =DateTime.UtcNow;
                    }
                    List<PatientInteraction> PatientInteractiontim = GetPatientInteractiontime(patientId, patientProgramId, connectionString, stDate, endDate).ToList();
                    if (PatientInteractiontim == null)
                    {
                        pbd.CPTCode = code.BillingCode;
                        pbd.Completed = 0;
                        pbd.Total = code.TargetReadings*60;
                        pbd.IsTargetMet = false;
                        pbd.ReadyTobill = 0;
                        pbd.BillingStartDate = (DateTime)stDate;
                        return pbd;
                    }
                    //var isEstablishedExist = PatientInteractiontim.Where(s => s.IsCallNote == 1).ToList();
                    //if (isEstablishedExist.Count() > 0)
                    //{
                    //    isEstablishedExist = isEstablishedExist.Where(s => s.IsEstablishedCall == 1).ToList();
                    //    if (isEstablishedExist != null && isEstablishedExist.Count() == 0)
                    //    {
                    //        PatientInteractiontim.Clear();
                    //    }
                    //}
                    if (PatientInteractiontim.Count == 0 || PatientInteractiontim == null /*|| isEstablishedExist.Count == 0*/)
                    {
                        pbd.CPTCode = code.BillingCode;
                        pbd.Completed = 0;
                        pbd.Total = code.TargetReadings*60;
                        pbd.IsTargetMet = false;
                        pbd.ReadyTobill = 0;
                        pbd.BillingStartDate = (DateTime)stDate;
                        return pbd;
                    }
                    TotalReading = PatientInteractiontim.Sum(s => s.Duration);

                    if (TotalReading > code487.TargetReadings * 60)
                    {
                        TimeSpan t1 = TimeSpan.FromSeconds(TotalReading);
                        TotalReading = (int)(t1.TotalSeconds - (code487.TargetReadings * 60));

                        pbd.BillingStartDate = (DateTime)stDate;
                        pbd.CPTCode = code.BillingCode;
                        pbd.Completed = TotalReading;

                        //base 40
                        pbd.Total = code.TargetReadings * 60;
                        pbd.ReadyTobill = (TotalReading >= (code.TargetReadings * 60) && DaysCompleted >= code.BillingThreshold) ? 1 : 0;

                        pbd.IsTargetMet = (TotalReading >= code.TargetReadings * 60) ? true : false;
                    }
                    else
                    {
                        //Target reading 20
                        pbd.BillingStartDate = (DateTime)stDate;
                        pbd.CPTCode = code.BillingCode;
                        pbd.Completed = 0;
                        pbd.Total = code.TargetReadings * 60;
                        pbd.IsTargetMet = (TotalReading >= code.TargetReadings * 60) ? true : false;
                        pbd.ReadyTobill = (TotalReading >= (code.TargetReadings * 60) && DaysCompleted >= code.BillingThreshold) ? 1 : 0;

                    }
                    return pbd;
                }
            }
            catch (Exception ex)
            {
                throw;
            }

        }
        public PatientBilldata ProcessCPT99424_DaysBasedBilling(int patientId, int patientProgramId, BillingCodesDetails code, string connectionString)
        {
            PatientBilldata pbd = new PatientBilldata();
            try
            {
                int DaysCompleted = 0; int TotalReading = 0;
                PatientStartDate PatientStartDate = new PatientStartDate();
                PatientStartDate = GetPatientBillingStartDateEx(patientId, patientProgramId, code.BillingCodeID, connectionString);
                if (PatientStartDate == null ||
                    PatientStartDate.Status.ToLower() == "invalid")
                {
                    pbd.CPTCode = code.BillingCode;
                    pbd.Completed = 0;
                    pbd.Total = code.TargetReadings*60;
                    pbd.IsTargetMet = false;
                    pbd.ReadyTobill = 0;
                    return pbd;
                }
                else if (PatientStartDate.StartDate > DateTime.UtcNow.Date && PatientStartDate.Status.ToLower() == "active")
                {
                    PatientBilldata PatientBilldata = new PatientBilldata();
                    PatientBilldata.CPTCode = code.BillingCode;
                    PatientBilldata.Completed = 0;
                    PatientBilldata.Total = code.TargetReadings*60;
                    PatientBilldata.IsTargetMet = false;
                    PatientBilldata.ReadyTobill = 0;
                    return PatientBilldata;
                }
                else
                {

                    DateTime stDate = DateTime.MinValue;
                    if (PatientStartDate.Status.ToLower() == "active")
                    {
                        stDate = Convert.ToDateTime(PatientStartDate.StartDate).AddDays(1);
                    }
                    else
                    {
                        stDate = Convert.ToDateTime(PatientStartDate.StartDate);
                    }
                    DateTime endDate = DateTime.UtcNow;

                    //Note: This will happen in the last day of billing.. In the last day, billed date will set in 
                    //the patientbilling table and +1 day will come as next start day.. So we should   
                    // calcualte the start date with respect to the billing threshold
                    DateTime today = DateTime.UtcNow;
                    DateTime endDateTempay = GetLocalTimeFromUTC((DateTime)today, connectionString);
                    if (stDate.Date > endDateTempay)
                    {
                        //stDate = stDate.AddDays(-1 * code.BillingThreshold);
                        BilledDates bd = GetPatientLastBilledPeriods(patientId, patientProgramId, code.BillingCodeID, connectionString);

                        if (bd== null)
                        {
                            PatientBilldata PatientBilldata = new PatientBilldata();
                            PatientBilldata.CPTCode = code.BillingCode;
                            PatientBilldata.Completed = 0;
                            PatientBilldata.Total = code.TargetReadings*60;
                            PatientBilldata.IsTargetMet = false;
                            PatientBilldata.ReadyTobill = 0;
                            return PatientBilldata;
                        }
                        stDate = bd.StartDate;
                        stDate = bd.StartDate;

                    }



                    if (PatientStartDate.Status.ToLower() == "billeddate")
                    {
                        stDate = GetUTCFromLocalTime((DateTime)stDate, connectionString);
                        endDate =DateTime.UtcNow;

                    }


                    List<PatientInteraction> PatientInteractiontim = GetPatientInteractiontime(patientId, patientProgramId, connectionString, stDate, endDate).ToList();
                    if (PatientInteractiontim == null)
                    {
                        pbd.CPTCode = code.BillingCode;
                        pbd.Completed = 0;
                        pbd.Total = code.TargetReadings*60;
                        pbd.IsTargetMet = false;
                        pbd.ReadyTobill = 0;
                        pbd.BillingStartDate = (DateTime)stDate;
                        return pbd;
                    }
                    //var isEstablishedExist = PatientInteractiontim.Where(s => s.IsCallNote == 1).ToList();
                    //if (isEstablishedExist.Count() > 0)
                    //{
                    //    isEstablishedExist = isEstablishedExist.Where(s => s.IsEstablishedCall == 1).ToList();
                    //    if (isEstablishedExist != null && isEstablishedExist.Count() == 0)
                    //    {
                    //        PatientInteractiontim.Clear();
                    //    }
                    //}
                    if (PatientInteractiontim.Count == 0 || PatientInteractiontim == null /*|| isEstablishedExist.Count == 0*/)
                    {
                        pbd.CPTCode = code.BillingCode;
                        pbd.Completed = 0;
                        pbd.Total = code.TargetReadings*60;
                        pbd.IsTargetMet = false;
                        pbd.ReadyTobill = 0;
                        pbd.BillingStartDate = (DateTime)stDate;
                        return pbd;
                    }
                    TotalReading = PatientInteractiontim.Sum(s => s.Duration);

                    if (TotalReading >= code.TargetReadings * 60)
                    {
                        TotalReading = code.TargetReadings * 60;
                    }
                    pbd.BillingStartDate = (DateTime)stDate;
                    pbd.CPTCode = code.BillingCode;
                    pbd.Completed = TotalReading;
                    pbd.Total = code.TargetReadings * 60;
                    pbd.IsTargetMet = (TotalReading >= code.TargetReadings * 60) ? true : false;
                    pbd.ReadyTobill = (TotalReading >= (code.TargetReadings * 60) && DaysCompleted >= code.BillingThreshold) ? 1 : 0;
                    return pbd;
                }
            }
            catch (Exception ex)
            {
                throw;
            }

        }
        public PatientBilldata ProcessCPT99425_DaysBasedBilling(int patientId, int patientProgramId, BillingCodesDetails code,
                                                    BillingCodesDetails code424, string connectionString)
        {
            PatientBilldata pbd = new PatientBilldata();
            try
            {
                int DaysCompleted = 0; int TotalReading = 0;
                PatientStartDate PatientStartDate = new PatientStartDate();
                PatientStartDate = GetPatientBillingStartDateEx(patientId, patientProgramId, code.BillingCodeID, connectionString);
                if (PatientStartDate == null ||
                    PatientStartDate.Status.ToLower() == "invalid")
                {
                    pbd.CPTCode = code.BillingCode;
                    pbd.Completed = 0;
                    pbd.Total = code.TargetReadings*60;
                    pbd.IsTargetMet = false;
                    pbd.ReadyTobill = 0;
                    return pbd;
                }
                else if (PatientStartDate.StartDate > DateTime.UtcNow.Date && PatientStartDate.Status.ToLower() == "active")
                {
                    PatientBilldata PatientBilldata = new PatientBilldata();
                    PatientBilldata.CPTCode = code.BillingCode;
                    PatientBilldata.Completed = 0;
                    PatientBilldata.Total = code.TargetReadings*60;
                    PatientBilldata.IsTargetMet = false;
                    PatientBilldata.ReadyTobill = 0;
                    return PatientBilldata;
                }
                else
                {
                    DateTime stDate = DateTime.MinValue;
                    if (PatientStartDate.Status.ToLower() == "active")
                    {
                        stDate = Convert.ToDateTime(PatientStartDate.StartDate).AddDays(1);
                    }
                    else
                    {
                        stDate = Convert.ToDateTime(PatientStartDate.StartDate);
                    }
                    DateTime endDate = DateTime.UtcNow;
                    //Note: This will happen in the last day of billing.. In the last day, billed date will set in 
                    //the patientbilling table and +1 day will come as next start day.. So we should   
                    // calcualte the start date with respect to the billing threshold
                    DateTime today = DateTime.UtcNow;
                    DateTime endDateTempay = GetLocalTimeFromUTC((DateTime)today, connectionString);
                    if (stDate.Date > endDateTempay)
                    {
                        //stDate = stDate.AddDays(-1 * code.BillingThreshold);
                        BilledDates bd = GetPatientLastBilledPeriods(patientId, patientProgramId, code.BillingCodeID, connectionString);


                        if (bd== null)
                        {
                            PatientBilldata PatientBilldata = new PatientBilldata();
                            PatientBilldata.CPTCode = code.BillingCode;
                            PatientBilldata.Completed = 0;
                            PatientBilldata.Total = code.TargetReadings*60;
                            PatientBilldata.IsTargetMet = false;
                            PatientBilldata.ReadyTobill = 0;
                            return PatientBilldata;
                        }
                        stDate = bd.StartDate;
                        stDate = bd.StartDate;
                        endDate = bd.EndDate;
                    }

                    if (PatientStartDate.Status.ToLower() == "billeddate")
                    {
                        stDate = GetUTCFromLocalTime((DateTime)stDate, connectionString);
                        endDate =DateTime.UtcNow;
                    }
                    List<PatientInteraction> PatientInteractiontim = GetPatientInteractiontime(patientId, patientProgramId, connectionString, stDate, endDate).ToList();
                    if (PatientInteractiontim == null)
                    {
                        pbd.CPTCode = code.BillingCode;
                        pbd.Completed = 0;
                        pbd.Total = code.TargetReadings*60;
                        pbd.IsTargetMet = false;
                        pbd.ReadyTobill = 0;
                        pbd.BillingStartDate = (DateTime)stDate;
                        return pbd;
                    }
                    //var isEstablishedExist = PatientInteractiontim.Where(s => s.IsCallNote == 1).ToList();
                    //if (isEstablishedExist.Count() > 0)
                    //{
                    //    isEstablishedExist = isEstablishedExist.Where(s => s.IsEstablishedCall == 1).ToList();
                    //    if (isEstablishedExist != null && isEstablishedExist.Count() == 0)
                    //    {
                    //        PatientInteractiontim.Clear();
                    //    }
                    //}
                    if (PatientInteractiontim.Count == 0 || PatientInteractiontim == null /*|| isEstablishedExist.Count == 0*/)
                    {
                        pbd.CPTCode = code.BillingCode;
                        pbd.Completed = 0;
                        pbd.Total = code.TargetReadings*60;
                        pbd.IsTargetMet = false;
                        pbd.ReadyTobill = 0;
                        pbd.BillingStartDate = (DateTime)stDate;
                        return pbd;
                    }
                    TotalReading = PatientInteractiontim.Sum(s => s.Duration);

                    if (TotalReading > code424.TargetReadings * 60)
                    {
                        TimeSpan t1 = TimeSpan.FromSeconds(TotalReading);
                        TotalReading = (int)(t1.TotalSeconds - (code424.TargetReadings * 60));

                        pbd.BillingStartDate = (DateTime)stDate;
                        pbd.CPTCode = code.BillingCode;
                        pbd.Completed = TotalReading;
                        if (TotalReading < code424.TargetReadings * 60)
                        {
                            // base 20
                            pbd.Total = code424.TargetReadings * 60;
                            pbd.ReadyTobill = (TotalReading >= (code424.TargetReadings * 60) && DaysCompleted >= code.BillingThreshold) ? 1 : 0;
                        }
                        else
                        {
                            //base 40
                            pbd.Total = code.TargetReadings * 60;
                            pbd.ReadyTobill = (TotalReading >= (code.TargetReadings * 60) && DaysCompleted >= code.BillingThreshold) ? 1 : 0;
                        }
                        pbd.IsTargetMet = (TotalReading >= code424.TargetReadings * 60) ? true : false;
                    }
                    else
                    {
                        //Target reading 20
                        pbd.BillingStartDate = (DateTime)stDate;
                        pbd.CPTCode = code.BillingCode;
                        pbd.Completed = 0;
                        pbd.Total = code424.TargetReadings * 60;
                        pbd.IsTargetMet = (TotalReading >= code424.TargetReadings * 60) ? true : false;
                        pbd.ReadyTobill = (TotalReading >= (code424.TargetReadings * 60) && DaysCompleted >= code.BillingThreshold) ? 1 : 0;

                    }
                    return pbd;
                }
            }
            catch (Exception ex)
            {
                throw;
            }

        }
        public PatientBilldata ProcessCPT99426_DaysBasedBilling(int patientId, int patientProgramId, BillingCodesDetails code, string connectionString)
        {
            PatientBilldata pbd = new PatientBilldata();
            try
            {
                int DaysCompleted = 0; int TotalReading = 0;
                PatientStartDate PatientStartDate = new PatientStartDate();
                PatientStartDate = GetPatientBillingStartDateEx(patientId, patientProgramId, code.BillingCodeID, connectionString);
                if (PatientStartDate == null ||
                    PatientStartDate.Status.ToLower() == "invalid")
                {
                    pbd.CPTCode = code.BillingCode;
                    pbd.Completed = 0;
                    pbd.Total = code.TargetReadings*60;
                    pbd.IsTargetMet = false;
                    pbd.ReadyTobill = 0;
                    return pbd;
                }
                else if (PatientStartDate.StartDate > DateTime.UtcNow.Date && PatientStartDate.Status.ToLower() == "active")
                {
                    PatientBilldata PatientBilldata = new PatientBilldata();
                    PatientBilldata.CPTCode = code.BillingCode;
                    PatientBilldata.Completed = 0;
                    PatientBilldata.Total = code.TargetReadings*60;
                    PatientBilldata.IsTargetMet = false;
                    PatientBilldata.ReadyTobill = 0;
                    return PatientBilldata;
                }
                else
                {

                    DateTime stDate = DateTime.MinValue;
                    if (PatientStartDate.Status.ToLower() == "active")
                    {
                        stDate = Convert.ToDateTime(PatientStartDate.StartDate).AddDays(1);
                    }
                    else
                    {
                        stDate = Convert.ToDateTime(PatientStartDate.StartDate);
                    }
                    DateTime endDate = DateTime.UtcNow;

                    //Note: This will happen in the last day of billing.. In the last day, billed date will set in 
                    //the patientbilling table and +1 day will come as next start day.. So we should   
                    // calcualte the start date with respect to the billing threshold
                    DateTime today = DateTime.UtcNow;
                    DateTime endDateTempay = GetLocalTimeFromUTC((DateTime)today, connectionString);
                    if (stDate.Date > endDateTempay)
                    {
                        //stDate = stDate.AddDays(-1 * code.BillingThreshold);
                        BilledDates bd = GetPatientLastBilledPeriods(patientId, patientProgramId, code.BillingCodeID, connectionString);


                        if (bd== null)
                        {
                            PatientBilldata PatientBilldata = new PatientBilldata();
                            PatientBilldata.CPTCode = code.BillingCode;
                            PatientBilldata.Completed = 0;
                            PatientBilldata.Total = code.TargetReadings*60;
                            PatientBilldata.IsTargetMet = false;
                            PatientBilldata.ReadyTobill = 0;
                            return PatientBilldata;
                        }
                        stDate = bd.StartDate;
                        stDate = bd.StartDate;

                    }



                    if (PatientStartDate.Status.ToLower() == "billeddate")
                    {
                        stDate = GetUTCFromLocalTime((DateTime)stDate, connectionString);
                        endDate =DateTime.UtcNow;

                    }


                    List<PatientInteraction> PatientInteractiontim = GetPatientInteractiontime(patientId, patientProgramId, connectionString, stDate, endDate).ToList();
                    if (PatientInteractiontim == null)
                    {
                        pbd.CPTCode = code.BillingCode;
                        pbd.Completed = 0;
                        pbd.Total = code.TargetReadings*60;
                        pbd.IsTargetMet = false;
                        pbd.ReadyTobill = 0;
                        pbd.BillingStartDate = (DateTime)stDate;
                        return pbd;
                    }
                    //var isEstablishedExist = PatientInteractiontim.Where(s => s.IsCallNote == 1).ToList();
                    //if (isEstablishedExist.Count() > 0)
                    //{
                    //    isEstablishedExist = isEstablishedExist.Where(s => s.IsEstablishedCall == 1).ToList();
                    //    if (isEstablishedExist != null && isEstablishedExist.Count() == 0)
                    //    {
                    //        PatientInteractiontim.Clear();
                    //    }
                    //}
                    if (PatientInteractiontim.Count == 0 || PatientInteractiontim == null /*|| isEstablishedExist.Count == 0*/)
                    {
                        pbd.CPTCode = code.BillingCode;
                        pbd.Completed = 0;
                        pbd.Total = code.TargetReadings*60;
                        pbd.IsTargetMet = false;
                        pbd.ReadyTobill = 0;
                        pbd.BillingStartDate = (DateTime)stDate;
                        return pbd;
                    }
                    TotalReading = PatientInteractiontim.Sum(s => s.Duration);

                    if (TotalReading >= code.TargetReadings * 60)
                    {
                        TotalReading = code.TargetReadings * 60;
                    }
                    pbd.BillingStartDate = (DateTime)stDate;
                    pbd.CPTCode = code.BillingCode;
                    pbd.Completed = TotalReading;
                    pbd.Total = code.TargetReadings * 60;
                    pbd.IsTargetMet = (TotalReading >= code.TargetReadings * 60) ? true : false;
                    pbd.ReadyTobill = (TotalReading >= (code.TargetReadings * 60) && DaysCompleted >= code.BillingThreshold) ? 1 : 0;
                    return pbd;
                }
            }
            catch (Exception ex)
            {
                throw;
            }

        }
        public PatientBilldata ProcessCPT99427_DaysBasedBilling(int patientId, int patientProgramId, BillingCodesDetails code,
                                                    BillingCodesDetails code426, string connectionString)
        {
            PatientBilldata pbd = new PatientBilldata();
            try
            {
                int DaysCompleted = 0; int TotalReading = 0;
                PatientStartDate PatientStartDate = new PatientStartDate();
                PatientStartDate = GetPatientBillingStartDateEx(patientId, patientProgramId, code.BillingCodeID, connectionString);
                if (PatientStartDate == null ||
                    PatientStartDate.Status.ToLower() == "invalid")
                {
                    pbd.CPTCode = code.BillingCode;
                    pbd.Completed = 0;
                    pbd.Total = code.TargetReadings*60;
                    pbd.IsTargetMet = false;
                    pbd.ReadyTobill = 0;
                    return pbd;
                }
                else if (PatientStartDate.StartDate > DateTime.UtcNow.Date && PatientStartDate.Status.ToLower() == "active")
                {
                    PatientBilldata PatientBilldata = new PatientBilldata();
                    PatientBilldata.CPTCode = code.BillingCode;
                    PatientBilldata.Completed = 0;
                    PatientBilldata.Total = code.TargetReadings*60;
                    PatientBilldata.IsTargetMet = false;
                    PatientBilldata.ReadyTobill = 0;
                    return PatientBilldata;
                }
                else
                {
                    DateTime stDate = DateTime.MinValue;
                    if (PatientStartDate.Status.ToLower() == "active")
                    {
                        stDate = Convert.ToDateTime(PatientStartDate.StartDate).AddDays(1);
                    }
                    else
                    {
                        stDate = Convert.ToDateTime(PatientStartDate.StartDate);
                    }
                    DateTime endDate = DateTime.UtcNow;
                    //Note: This will happen in the last day of billing.. In the last day, billed date will set in 
                    //the patientbilling table and +1 day will come as next start day.. So we should   
                    // calcualte the start date with respect to the billing threshold
                    DateTime today = DateTime.UtcNow;
                    DateTime endDateTempay = GetLocalTimeFromUTC((DateTime)today, connectionString);
                    if (stDate.Date > endDateTempay)
                    {
                        //stDate = stDate.AddDays(-1 * code.BillingThreshold);
                        BilledDates bd = GetPatientLastBilledPeriods(patientId, patientProgramId, code.BillingCodeID, connectionString);


                        if (bd== null)
                        {
                            PatientBilldata PatientBilldata = new PatientBilldata();
                            PatientBilldata.CPTCode = code.BillingCode;
                            PatientBilldata.Completed = 0;
                            PatientBilldata.Total = code.TargetReadings*60;
                            PatientBilldata.IsTargetMet = false;
                            PatientBilldata.ReadyTobill = 0;
                            return PatientBilldata;
                        }
                        stDate = bd.StartDate;
                        stDate = bd.StartDate;
                        endDate = bd.EndDate;
                    }

                    if (PatientStartDate.Status.ToLower() == "billeddate")
                    {
                        stDate = GetUTCFromLocalTime((DateTime)stDate, connectionString);
                        endDate =DateTime.UtcNow;
                    }
                    List<PatientInteraction> PatientInteractiontim = GetPatientInteractiontime(patientId, patientProgramId, connectionString, stDate, endDate).ToList();
                    if (PatientInteractiontim == null)
                    {
                        pbd.CPTCode = code.BillingCode;
                        pbd.Completed = 0;
                        pbd.Total = code.TargetReadings*60;
                        pbd.IsTargetMet = false;
                        pbd.ReadyTobill = 0;
                        pbd.BillingStartDate = (DateTime)stDate;
                        return pbd;
                    }
                    //var isEstablishedExist = PatientInteractiontim.Where(s => s.IsCallNote == 1).ToList();
                    //if (isEstablishedExist.Count() > 0)
                    //{
                    //    isEstablishedExist = isEstablishedExist.Where(s => s.IsEstablishedCall == 1).ToList();
                    //    if (isEstablishedExist != null && isEstablishedExist.Count() == 0)
                    //    {
                    //        PatientInteractiontim.Clear();
                    //    }
                    //}
                    if (PatientInteractiontim.Count == 0 || PatientInteractiontim == null /*|| isEstablishedExist.Count == 0*/)
                    {
                        pbd.CPTCode = code.BillingCode;
                        pbd.Completed = 0;
                        pbd.Total = code.TargetReadings*60;
                        pbd.IsTargetMet = false;
                        pbd.ReadyTobill = 0;
                        pbd.BillingStartDate = (DateTime)stDate;
                        return pbd;
                    }
                    TotalReading = PatientInteractiontim.Sum(s => s.Duration);

                    if (TotalReading > code426.TargetReadings * 60)
                    {
                        TimeSpan t1 = TimeSpan.FromSeconds(TotalReading);
                        TotalReading = (int)(t1.TotalSeconds - (code426.TargetReadings * 60));

                        pbd.BillingStartDate = (DateTime)stDate;
                        pbd.CPTCode = code.BillingCode;
                        pbd.Completed = TotalReading;
                        if (TotalReading < code426.TargetReadings * 60)
                        {
                            // base 20
                            pbd.Total = code426.TargetReadings * 60;
                            pbd.ReadyTobill = (TotalReading >= (code426.TargetReadings * 60) && DaysCompleted >= code.BillingThreshold) ? 1 : 0;
                        }
                        else
                        {
                            //base 40
                            pbd.Total = code.TargetReadings * 60;
                            pbd.ReadyTobill = (TotalReading >= (code.TargetReadings * 60) && DaysCompleted >= code.BillingThreshold) ? 1 : 0;
                        }
                        pbd.IsTargetMet = (TotalReading >= code426.TargetReadings * 60) ? true : false;
                    }
                    else
                    {
                        //Target reading 20
                        pbd.BillingStartDate = (DateTime)stDate;
                        pbd.CPTCode = code.BillingCode;
                        pbd.Completed = 0;
                        pbd.Total = code426.TargetReadings * 60;
                        pbd.IsTargetMet = (TotalReading >= code426.TargetReadings * 60) ? true : false;
                        pbd.ReadyTobill = (TotalReading >= (code426.TargetReadings * 60) && DaysCompleted >= code.BillingThreshold) ? 1 : 0;

                    }
                    return pbd;
                }
            }
            catch (Exception ex)
            {
                throw;
            }

        }
        public PatientBilldata ProcessCPTG0506_CycleBasedBilling(int patientId, int patientProgramId, int targetReading, BillingCodesDetails code, string connectionString)
        {
            try
            {
                PatientBilldata PatientBilldata = new PatientBilldata();
                PatientStartDate patientStartdate = GetBillingStartDateEx(patientId, patientProgramId, code.BillingCodeID, connectionString);
                //Startdate is null means already billed in the patient billing table for 53 code.
                if (patientStartdate.Status == "BilledDate")
                {

                    PatientBilldata.BillingStartDate = (DateTime)patientStartdate.StartDate;
                    PatientBilldata.CPTCode = code.BillingCode;
                    PatientBilldata.Completed = targetReading;
                    PatientBilldata.Total = targetReading;
                    PatientBilldata.IsTargetMet = true;
                    PatientBilldata.ReadyTobill = 1;
                    return PatientBilldata;
                }

                List<PatientDailyBillingData> lstDailyData = GetPatientBillingCounts(patientId, patientProgramId, connectionString);
                if (lstDailyData == null || lstDailyData.Count == 0)
                {

                    PatientBilldata = null;
                }

                foreach (PatientDailyBillingData pdb in lstDailyData)
                {
                    if (pdb.BillingCodeId == code.BillingCodeID)
                    {
                        PatientBilldata = new PatientBilldata();
                        PatientBilldata.BillingStartDate = (DateTime)pdb.StartDate;
                        PatientBilldata.CPTCode = code.BillingCode;
                        PatientBilldata.Completed = pdb.TotalDuration;
                        PatientBilldata.Total = code.TargetReadings*60;
                        PatientBilldata.IsTargetMet = pdb.TotalVitalCount >= code.TargetReadings ? true : false; ;
                        PatientBilldata.ReadyTobill = (pdb.TotalVitalCount >= code.TargetReadings && pdb.DaysCompleted >= code.BillingThreshold) ? 1 : 0;
                        return PatientBilldata;

                    }
                    else
                    {
                        PatientBilldata = null;
                    }

                }

                if (PatientBilldata == null)
                {

                    PatientBilldata = new PatientBilldata();
                    PatientBilldata.BillingStartDate = Convert.ToDateTime(patientStartdate.StartDate);
                    PatientBilldata.CPTCode = code.BillingCode;
                    int VitalCount = 0;
                    int DaysCompleted = 0;
                    PatientBilldata.Completed = VitalCount;
                    PatientBilldata.Total = code.TargetReadings*60;
                    PatientBilldata.IsTargetMet = (VitalCount >= code.TargetReadings) ? true : false;
                    PatientBilldata.ReadyTobill = (VitalCount >= code.TargetReadings && DaysCompleted >= code.BillingThreshold) ? 1 : 0;
                    return PatientBilldata;

                }

                return PatientBilldata;


            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception occured - class Name :" + this.GetType().Name + ",  Method Name : " + MethodBase.GetCurrentMethod().Name + ", Error Message :" + ex.Message + "");
                return null;
            }
        }

        public PatientBilldata ProcessCPT99490_CycleBasedBilling(int patientId, int patientProgramId, BillingCodesDetails code, string connectionString)
        {
            PatientBilldata pbd = new PatientBilldata();
            try
            {
                int DaysCompleted = 0; int TotalReading = 0;
                PatientStartDate PatientStartDate = new PatientStartDate();
                PatientStartDate = GetPatientBillingStartDateEx(patientId, patientProgramId, code.BillingCodeID, connectionString);
                if (PatientStartDate == null ||
                    PatientStartDate.Status.ToLower() == "invalid")
                {
                    pbd.CPTCode = code.BillingCode;
                    pbd.Completed = 0;
                    pbd.Total = code.TargetReadings*60;
                    pbd.IsTargetMet = false;
                    pbd.ReadyTobill = 0;
                    return pbd;
                }
                else if (PatientStartDate.StartDate > DateTime.UtcNow && PatientStartDate.Status.ToLower() == "active")
                {
                    PatientBilldata PatientBilldata = new PatientBilldata();
                    PatientBilldata.CPTCode = code.BillingCode;
                    PatientBilldata.Completed = 0;
                    PatientBilldata.Total = code.TargetReadings*60;
                    PatientBilldata.IsTargetMet = false;
                    PatientBilldata.ReadyTobill = 0;
                    return PatientBilldata;
                }
                else
                {

                    DateTime stDate = Convert.ToDateTime(PatientStartDate.StartDate);
                    DateTime endDate = DateTime.UtcNow;

                    //Note: This will happen in the last day of billing.. In the last day, billed date will set in 
                    //the patientbilling table and +1 day will come as next start day.. So we should   
                    // calcualte the start date with respect to the billing threshold
                    DateTime today = DateTime.UtcNow;
                    DateTime endDateTempay = GetLocalTimeFromUTC((DateTime)today, connectionString);
                    if (stDate.Date > endDateTempay)
                    {
                        //stDate = stDate.AddDays(-1 * code.BillingThreshold);
                        BilledDates bd = GetPatientLastBilledPeriods(patientId, patientProgramId, code.BillingCodeID, connectionString);
                        if (bd== null)
                        {
                            PatientBilldata PatientBilldata = new PatientBilldata();
                            PatientBilldata.CPTCode = code.BillingCode;
                            PatientBilldata.Completed = 0;
                            PatientBilldata.Total = code.TargetReadings*60;
                            PatientBilldata.IsTargetMet = false;
                            PatientBilldata.ReadyTobill = 0;
                            return PatientBilldata;
                        }
                        stDate = bd.StartDate;

                    }



                    if (PatientStartDate.Status.ToLower() == "billeddate")
                    {
                        stDate = GetUTCFromLocalTime((DateTime)stDate, connectionString);
                        endDate =DateTime.UtcNow;

                    }


                    List<PatientInteraction> PatientInteractiontim = GetPatientInteractiontime(patientId, patientProgramId, connectionString, stDate, endDate).ToList();
                    if (PatientInteractiontim == null)
                    {
                        pbd.CPTCode = code.BillingCode;
                        pbd.Completed = 0;
                        pbd.Total = code.TargetReadings*60;
                        pbd.IsTargetMet = false;
                        pbd.ReadyTobill = 0;
                        string datss = stDate.ToString();
                        datss = datss.Replace("Z", "");
                        pbd.BillingStartDate = DateTime.Parse(datss);
                        return pbd;
                    }
                    //var isEstablishedExist = PatientInteractiontim.Where(s => s.IsCallNote == 1).ToList();
                    //if (isEstablishedExist.Count() > 0)
                    //{
                    //    isEstablishedExist = isEstablishedExist.Where(s => s.IsEstablishedCall == 1).ToList();
                    //    if (isEstablishedExist != null && isEstablishedExist.Count() == 0)
                    //    {
                    //        PatientInteractiontim.Clear();
                    //    }
                    //}
                    if (PatientInteractiontim.Count == 0 || PatientInteractiontim == null /*|| isEstablishedExist.Count == 0*/)
                    {
                        pbd.CPTCode = code.BillingCode;
                        pbd.Completed = 0;
                        pbd.Total = code.TargetReadings*60;
                        pbd.IsTargetMet = false;
                        pbd.ReadyTobill = 0;
                        string datess = stDate.ToString();
                        datess = datess.Replace("Z", "");
                        pbd.BillingStartDate = DateTime.Parse(datess);
                        return pbd;
                    }
                    TotalReading = PatientInteractiontim.Sum(s => s.Duration);

                    if (TotalReading >= code.TargetReadings * 60)
                    {
                        TotalReading = code.TargetReadings * 60;
                    }
                    string dates = stDate.ToString();
                    dates = dates.Replace("Z", "");
                    pbd.BillingStartDate = DateTime.Parse(dates);
                    pbd.CPTCode = code.BillingCode;
                    pbd.Completed = TotalReading;
                    pbd.Total = code.TargetReadings * 60;
                    pbd.IsTargetMet = (TotalReading >= code.TargetReadings * 60) ? true : false;
                    pbd.ReadyTobill = (TotalReading >= (code.TargetReadings * 60) && DaysCompleted >= code.BillingThreshold) ? 1 : 0;
                    return pbd;
                }
            }
            catch (Exception ex)
            {
                throw;
            }

        }
        public PatientBilldata ProcessCPT99439_CycleBasedBilling(int patientId, int patientProgramId, BillingCodesDetails code,
                                                     BillingCodesDetails code490, string connectionString)
        {
            PatientBilldata pbd = new PatientBilldata();
            try
            {
                int DaysCompleted = 0; int TotalReading = 0;
                PatientStartDate PatientStartDate = new PatientStartDate();
                PatientStartDate = GetPatientBillingStartDateEx(patientId, patientProgramId, code.BillingCodeID, connectionString);
                if (PatientStartDate == null ||
                    PatientStartDate.Status.ToLower() == "invalid")
                {
                    pbd.CPTCode = code.BillingCode;
                    pbd.Completed = 0;
                    pbd.Total = code.TargetReadings*60;
                    pbd.IsTargetMet = false;
                    pbd.ReadyTobill = 0;
                    return pbd;
                }
                else if (PatientStartDate.StartDate > DateTime.UtcNow && PatientStartDate.Status.ToLower() == "active")
                {
                    PatientBilldata PatientBilldata = new PatientBilldata();
                    PatientBilldata.CPTCode = code.BillingCode;
                    PatientBilldata.Completed = 0;
                    PatientBilldata.Total = code.TargetReadings;
                    PatientBilldata.IsTargetMet = false;
                    PatientBilldata.ReadyTobill = 0;
                    return PatientBilldata;
                }
                else
                {
                    DateTime stDate = Convert.ToDateTime(PatientStartDate.StartDate);
                    DateTime endDate = DateTime.UtcNow;
                    //Note: This will happen in the last day of billing.. In the last day, billed date will set in 
                    //the patientbilling table and +1 day will come as next start day.. So we should   
                    // calcualte the start date with respect to the billing threshold
                    DateTime today = DateTime.UtcNow;
                    DateTime endDateTempay = GetLocalTimeFromUTC((DateTime)today, connectionString);
                    if (stDate.Date > endDateTempay)
                    {
                        //stDate = stDate.AddDays(-1 * code.BillingThreshold);
                        BilledDates bd = GetPatientLastBilledPeriods(patientId, patientProgramId, code.BillingCodeID, connectionString);
                        if (bd == null)
                        {
                            PatientBilldata PatientBilldata = new PatientBilldata();
                            PatientBilldata.CPTCode = code.BillingCode;
                            PatientBilldata.Completed = 0;
                            PatientBilldata.Total = code.TargetReadings;
                            PatientBilldata.IsTargetMet = false;
                            PatientBilldata.ReadyTobill = 0;
                            return PatientBilldata;
                        }
                        stDate = bd.StartDate;
                        endDate = bd.EndDate;
                    }

                    if (PatientStartDate.Status.ToLower() == "billeddate")
                    {
                        stDate = GetUTCFromLocalTime((DateTime)stDate, connectionString);
                        endDate =DateTime.UtcNow;
                    }
                    List<PatientInteraction> PatientInteractiontim = GetPatientInteractiontime(patientId, patientProgramId, connectionString, stDate, endDate).ToList();
                    if (PatientInteractiontim == null)
                    {
                        pbd.CPTCode = code.BillingCode;
                        pbd.Completed = 0;
                        pbd.Total = code.TargetReadings*60;
                        pbd.IsTargetMet = false;
                        pbd.ReadyTobill = 0;
                        string datss = stDate.ToString();
                        datss = datss.Replace("Z", "");
                        pbd.BillingStartDate = DateTime.Parse(datss);
                        return pbd;
                    }
                    //var isEstablishedExist = PatientInteractiontim.Where(s => s.IsCallNote == 1).ToList();
                    //if (isEstablishedExist.Count() > 0)
                    //{
                    //    isEstablishedExist = isEstablishedExist.Where(s => s.IsEstablishedCall == 1).ToList();
                    //    if (isEstablishedExist != null && isEstablishedExist.Count() == 0)
                    //    {
                    //        PatientInteractiontim.Clear();
                    //    }
                    //}
                    if (PatientInteractiontim.Count == 0 || PatientInteractiontim == null /*|| isEstablishedExist.Count == 0*/)
                    {
                        pbd.CPTCode = code.BillingCode;
                        pbd.Completed = 0;
                        pbd.Total = code.TargetReadings*60;
                        pbd.IsTargetMet = false;
                        pbd.ReadyTobill = 0;
                        string datess = stDate.ToString();
                        datess = datess.Replace("Z", "");
                        pbd.BillingStartDate = DateTime.Parse(datess);
                        return pbd;
                    }
                    TotalReading = PatientInteractiontim.Sum(s => s.Duration);

                    if (TotalReading > code490.TargetReadings * 60)
                    {
                        TimeSpan t1 = TimeSpan.FromSeconds(TotalReading);
                        TotalReading = (int)(t1.TotalSeconds - (code490.TargetReadings * 60));

                        string dates = stDate.ToString();
                        dates = dates.Replace("Z", "");
                        pbd.BillingStartDate = DateTime.Parse(dates);
                        pbd.CPTCode = code.BillingCode;
                        pbd.Completed = TotalReading;
                        if (TotalReading < code490.TargetReadings * 60)
                        {
                            // base 20
                            pbd.Total = code490.TargetReadings * 60;
                            pbd.ReadyTobill = (TotalReading >= (code490.TargetReadings * 60) && DaysCompleted >= code.BillingThreshold) ? 1 : 0;
                        }
                        else
                        {
                            //base 40
                            pbd.Total = code.TargetReadings * 60;
                            pbd.ReadyTobill = (TotalReading >= (code.TargetReadings * 60) && DaysCompleted >= code.BillingThreshold) ? 1 : 0;
                        }
                        pbd.IsTargetMet = (TotalReading >= code490.TargetReadings * 60) ? true : false;
                    }
                    else
                    {
                        //Target reading 20
                        string dats = stDate.ToString();
                        dats = dats.Replace("Z", "");
                        pbd.BillingStartDate = DateTime.Parse(dats);
                        pbd.CPTCode = code.BillingCode;
                        pbd.Completed = 0;
                        pbd.Total = code490.TargetReadings * 60;
                        pbd.IsTargetMet = (TotalReading >= code490.TargetReadings * 60) ? true : false;
                        pbd.ReadyTobill = (TotalReading >= (code490.TargetReadings * 60) && DaysCompleted >= code.BillingThreshold) ? 1 : 0;

                    }
                    return pbd;
                }
            }
            catch (Exception ex)
            {
                throw;
            }

        }


        public PatientBilldata ProcessCPT99491_CycleBasedBilling(int patientId, int patientProgramId, BillingCodesDetails code, string connectionString)
        {
            PatientBilldata pbd = new PatientBilldata();
            try
            {
                int DaysCompleted = 0; int TotalReading = 0;
                PatientStartDate PatientStartDate = new PatientStartDate();
                PatientStartDate = GetPatientBillingStartDateEx(patientId, patientProgramId, code.BillingCodeID, connectionString);
                if (PatientStartDate == null ||
                    PatientStartDate.Status.ToLower() == "invalid")
                {
                    pbd.CPTCode = code.BillingCode;
                    pbd.Completed = 0;
                    pbd.Total = code.TargetReadings*60;
                    pbd.IsTargetMet = false;
                    pbd.ReadyTobill = 0;
                    return pbd;
                }
                else if (PatientStartDate.StartDate > DateTime.UtcNow && PatientStartDate.Status.ToLower() == "active")
                {
                    PatientBilldata PatientBilldata = new PatientBilldata();
                    PatientBilldata.CPTCode = code.BillingCode;
                    PatientBilldata.Completed = 0;
                    PatientBilldata.Total = code.TargetReadings*60;
                    PatientBilldata.IsTargetMet = false;
                    PatientBilldata.ReadyTobill = 0;
                    return PatientBilldata;
                }
                else
                {

                    DateTime stDate = Convert.ToDateTime(PatientStartDate.StartDate);
                    DateTime endDate = DateTime.UtcNow;

                    //Note: This will happen in the last day of billing.. In the last day, billed date will set in 
                    //the patientbilling table and +1 day will come as next start day.. So we should   
                    // calcualte the start date with respect to the billing threshold
                    DateTime today = DateTime.UtcNow;
                    DateTime endDateTempay = GetLocalTimeFromUTC((DateTime)today, connectionString);
                    if (stDate.Date > endDateTempay)
                    {
                        //stDate = stDate.AddDays(-1 * code.BillingThreshold);
                        BilledDates bd = GetPatientLastBilledPeriods(patientId, patientProgramId, code.BillingCodeID, connectionString);
                        if (bd == null)
                        {
                            PatientBilldata PatientBilldata = new PatientBilldata();
                            PatientBilldata.CPTCode = code.BillingCode;
                            PatientBilldata.Completed = 0;
                            PatientBilldata.Total = code.TargetReadings*60;
                            PatientBilldata.IsTargetMet = false;
                            PatientBilldata.ReadyTobill = 0;
                            return PatientBilldata;
                        }
                        stDate = bd.StartDate;

                    }



                    if (PatientStartDate.Status.ToLower() == "billeddate")
                    {
                        stDate = GetUTCFromLocalTime((DateTime)stDate, connectionString);
                        endDate =DateTime.UtcNow;

                    }


                    List<PatientInteraction> PatientInteractiontim = GetPatientInteractiontime(patientId, patientProgramId, connectionString, stDate, endDate).ToList();
                    if (PatientInteractiontim == null)
                    {
                        pbd.CPTCode = code.BillingCode;
                        pbd.Completed = 0;
                        pbd.Total = code.TargetReadings*60;
                        pbd.IsTargetMet = false;
                        pbd.ReadyTobill = 0;
                        string dats = stDate.ToString();
                        dats = dats.Replace("Z", "");
                        pbd.BillingStartDate = DateTime.Parse(dats);
                        return pbd;
                    }
                    //var isEstablishedExist = PatientInteractiontim.Where(s => s.IsCallNote == 1).ToList();
                    //if (isEstablishedExist.Count() > 0)
                    //{
                    //    isEstablishedExist = isEstablishedExist.Where(s => s.IsEstablishedCall == 1).ToList();
                    //    if (isEstablishedExist != null && isEstablishedExist.Count() == 0)
                    //    {
                    //        PatientInteractiontim.Clear();
                    //    }
                    //}
                    if (PatientInteractiontim.Count == 0 || PatientInteractiontim == null /*|| isEstablishedExist.Count == 0*/)
                    {
                        pbd.CPTCode = code.BillingCode;
                        pbd.Completed = 0;
                        pbd.Total = code.TargetReadings*60;
                        pbd.IsTargetMet = false;
                        pbd.ReadyTobill = 0;
                        string dates = stDate.ToString();
                        dates = dates.Replace("Z", "");
                        pbd.BillingStartDate = DateTime.Parse(dates);
                        return pbd;
                    }
                    TotalReading = PatientInteractiontim.Sum(s => s.Duration);

                    if (TotalReading >= code.TargetReadings * 60)
                    {
                        TotalReading = code.TargetReadings * 60;
                    }
                    string datess = stDate.ToString();
                    datess = datess.Replace("Z", "");
                    pbd.BillingStartDate = DateTime.Parse(datess);
                    pbd.CPTCode = code.BillingCode;
                    pbd.Completed = TotalReading;
                    pbd.Total = code.TargetReadings * 60;
                    pbd.IsTargetMet = (TotalReading >= code.TargetReadings * 60) ? true : false;
                    pbd.ReadyTobill = (TotalReading >= (code.TargetReadings * 60) && DaysCompleted >= code.BillingThreshold) ? 1 : 0;
                    return pbd;
                }
            }
            catch (Exception ex)
            {
                throw;
            }

        }

        public PatientBilldata ProcessCPT99437_CycleBasedBilling(int patientId, int patientProgramId, BillingCodesDetails code,
                                                    BillingCodesDetails code491, string connectionString)
        {
            PatientBilldata pbd = new PatientBilldata();
            try
            {
                int DaysCompleted = 0; int TotalReading = 0;
                PatientStartDate PatientStartDate = new PatientStartDate();
                PatientStartDate = GetPatientBillingStartDateEx(patientId, patientProgramId, code.BillingCodeID, connectionString);
                if (PatientStartDate == null ||
                    PatientStartDate.Status.ToLower() == "invalid")
                {
                    pbd.CPTCode = code.BillingCode;
                    pbd.Completed = 0;
                    pbd.Total = code.TargetReadings*60;
                    pbd.IsTargetMet = false;
                    pbd.ReadyTobill = 0;
                    return pbd;
                }
                else if (PatientStartDate.StartDate > DateTime.UtcNow && PatientStartDate.Status.ToLower() == "active")
                {
                    PatientBilldata PatientBilldata = new PatientBilldata();
                    PatientBilldata.CPTCode = code.BillingCode;
                    PatientBilldata.Completed = 0;
                    PatientBilldata.Total = code.TargetReadings*60;
                    PatientBilldata.IsTargetMet = false;
                    PatientBilldata.ReadyTobill = 0;
                    return PatientBilldata;
                }
                else
                {
                    DateTime stDate = Convert.ToDateTime(PatientStartDate.StartDate);
                    DateTime endDate = DateTime.UtcNow;
                    //Note: This will happen in the last day of billing.. In the last day, billed date will set in 
                    //the patientbilling table and +1 day will come as next start day.. So we should   
                    // calcualte the start date with respect to the billing threshold
                    DateTime today = DateTime.UtcNow;
                    DateTime endDateTempay = GetLocalTimeFromUTC((DateTime)today, connectionString);
                    if (stDate.Date > endDateTempay)
                    {
                        //stDate = stDate.AddDays(-1 * code.BillingThreshold);
                        BilledDates bd = GetPatientLastBilledPeriods(patientId, patientProgramId, code.BillingCodeID, connectionString);
                        if (bd == null)
                        {
                            PatientBilldata PatientBilldata = new PatientBilldata();
                            PatientBilldata.CPTCode = code.BillingCode;
                            PatientBilldata.Completed = 0;
                            PatientBilldata.Total = code.TargetReadings*60;
                            PatientBilldata.IsTargetMet = false;
                            PatientBilldata.ReadyTobill = 0;
                            return PatientBilldata;
                        }
                        stDate = bd.StartDate;
                        endDate = bd.EndDate;
                    }

                    if (PatientStartDate.Status.ToLower() == "billeddate")
                    {
                        stDate = GetUTCFromLocalTime((DateTime)stDate, connectionString);
                        endDate =DateTime.UtcNow;
                    }
                    List<PatientInteraction> PatientInteractiontim = GetPatientInteractiontime(patientId, patientProgramId, connectionString, stDate, endDate).ToList();
                    if (PatientInteractiontim == null)
                    {
                        pbd.CPTCode = code.BillingCode;
                        pbd.Completed = 0;
                        pbd.Total = code.TargetReadings*60;
                        pbd.IsTargetMet = false;
                        pbd.ReadyTobill = 0;
                        string datess = stDate.ToString();
                        datess = datess.Replace("Z", "");
                        pbd.BillingStartDate = DateTime.Parse(datess);
                        return pbd;
                    }
                    //var isEstablishedExist = PatientInteractiontim.Where(s => s.IsCallNote == 1).ToList();
                    //if (isEstablishedExist.Count() > 0)
                    //{
                    //    isEstablishedExist = isEstablishedExist.Where(s => s.IsEstablishedCall == 1).ToList();
                    //    if (isEstablishedExist != null && isEstablishedExist.Count() == 0)
                    //    {
                    //        PatientInteractiontim.Clear();
                    //    }
                    //}
                    if (PatientInteractiontim.Count == 0 || PatientInteractiontim == null /*|| isEstablishedExist.Count == 0*/)
                    {
                        pbd.CPTCode = code.BillingCode;
                        pbd.Completed = 0;
                        pbd.Total = code.TargetReadings*60;
                        pbd.IsTargetMet = false;
                        pbd.ReadyTobill = 0;
                        string dates = stDate.ToString();
                        dates = dates.Replace("Z", "");
                        pbd.BillingStartDate = DateTime.Parse(dates);
                        return pbd;
                    }
                    TotalReading = PatientInteractiontim.Sum(s => s.Duration);

                    if (TotalReading > code491.TargetReadings * 60)
                    {
                        TimeSpan t1 = TimeSpan.FromSeconds(TotalReading);
                        TotalReading = (int)(t1.TotalSeconds - (code491.TargetReadings * 60));

                        string date = stDate.ToString();
                        date = date.Replace("Z", "");
                        pbd.BillingStartDate = DateTime.Parse(date);
                        pbd.CPTCode = code.BillingCode;
                        pbd.Completed = TotalReading;
                        if (TotalReading < code491.TargetReadings * 60)
                        {
                            // base 20
                            pbd.Total = code491.TargetReadings * 60;
                            pbd.ReadyTobill = (TotalReading >= (code491.TargetReadings * 60) && DaysCompleted >= code.BillingThreshold) ? 1 : 0;
                        }
                        else
                        {
                            //base 40
                            pbd.Total = code.TargetReadings * 60;
                            pbd.ReadyTobill = (TotalReading >= (code.TargetReadings * 60) && DaysCompleted >= code.BillingThreshold) ? 1 : 0;
                        }
                        pbd.IsTargetMet = (TotalReading >= code491.TargetReadings * 60) ? true : false;
                    }
                    else
                    {
                        //Target reading 20
                        string dateN = stDate.ToString();
                        dateN = dateN.Replace("Z", "");
                        pbd.BillingStartDate = DateTime.Parse(dateN);
                        pbd.CPTCode = code.BillingCode;
                        pbd.Completed = 0;
                        pbd.Total = code491.TargetReadings * 60;
                        pbd.IsTargetMet = (TotalReading >= code491.TargetReadings * 60) ? true : false;
                        pbd.ReadyTobill = (TotalReading >= (code491.TargetReadings * 60) && DaysCompleted >= code.BillingThreshold) ? 1 : 0;

                    }
                    return pbd;
                }
            }
            catch (Exception ex)
            {
                throw;
            }

        }
        public PatientBilldata ProcessCPT99487_CycleBasedBilling(int patientId, int patientProgramId, BillingCodesDetails code, string connectionString)
        {
            PatientBilldata pbd = new PatientBilldata();
            try
            {
                int DaysCompleted = 0; int TotalReading = 0;
                PatientStartDate PatientStartDate = new PatientStartDate();
                PatientStartDate = GetPatientBillingStartDateEx(patientId, patientProgramId, code.BillingCodeID, connectionString);
                if (PatientStartDate == null ||
                    PatientStartDate.Status.ToLower() == "invalid")
                {
                    pbd.CPTCode = code.BillingCode;
                    pbd.Completed = 0;
                    pbd.Total = code.TargetReadings*60;
                    pbd.IsTargetMet = false;
                    pbd.ReadyTobill = 0;
                    return pbd;
                }
                else if (PatientStartDate.StartDate > DateTime.UtcNow && PatientStartDate.Status.ToLower() == "active")
                {
                    PatientBilldata PatientBilldata = new PatientBilldata();
                    PatientBilldata.CPTCode = code.BillingCode;
                    PatientBilldata.Completed = 0;
                    PatientBilldata.Total = code.TargetReadings*60;
                    PatientBilldata.IsTargetMet = false;
                    PatientBilldata.ReadyTobill = 0;
                    return PatientBilldata;
                }
                else
                {

                    DateTime stDate = Convert.ToDateTime(PatientStartDate.StartDate);
                    DateTime endDate = DateTime.UtcNow;

                    //Note: This will happen in the last day of billing.. In the last day, billed date will set in 
                    //the patientbilling table and +1 day will come as next start day.. So we should   
                    // calcualte the start date with respect to the billing threshold
                    DateTime today = DateTime.UtcNow;
                    DateTime endDateTempay = GetLocalTimeFromUTC((DateTime)today, connectionString);
                    if (stDate.Date > endDateTempay)
                    {
                        //stDate = stDate.AddDays(-1 * code.BillingThreshold);
                        BilledDates bd = GetPatientLastBilledPeriods(patientId, patientProgramId, code.BillingCodeID, connectionString);
                        if (bd == null)
                        {
                            PatientBilldata PatientBilldata = new PatientBilldata();
                            PatientBilldata.CPTCode = code.BillingCode;
                            PatientBilldata.Completed = 0;
                            PatientBilldata.Total = code.TargetReadings*60;
                            PatientBilldata.IsTargetMet = false;
                            PatientBilldata.ReadyTobill = 0;
                            return PatientBilldata;
                        }
                        stDate = bd.StartDate;

                    }



                    if (PatientStartDate.Status.ToLower() == "billeddate")
                    {
                        stDate = GetUTCFromLocalTime((DateTime)stDate, connectionString);
                        endDate =DateTime.UtcNow;

                    }


                    List<PatientInteraction> PatientInteractiontim = GetPatientInteractiontime(patientId, patientProgramId, connectionString, stDate, endDate).ToList();
                    if (PatientInteractiontim == null)
                    {
                        pbd.CPTCode = code.BillingCode;
                        pbd.Completed = 0;
                        pbd.Total = code.TargetReadings*60;
                        pbd.IsTargetMet = false;
                        pbd.ReadyTobill = 0;
                        string dateNew = stDate.ToString();
                        dateNew = dateNew.Replace("Z", "");
                        pbd.BillingStartDate = DateTime.Parse(dateNew);


                        return pbd;
                    }
                    //var isEstablishedExist = PatientInteractiontim.Where(s => s.IsCallNote == 1).ToList();
                    //if (isEstablishedExist.Count() > 0)
                    //{
                    //    isEstablishedExist = isEstablishedExist.Where(s => s.IsEstablishedCall == 1).ToList();
                    //    if (isEstablishedExist != null && isEstablishedExist.Count() == 0)
                    //    {
                    //        PatientInteractiontim.Clear();
                    //    }
                    //}
                    if (PatientInteractiontim.Count == 0 || PatientInteractiontim == null /*|| isEstablishedExist.Count == 0*/)
                    {
                        pbd.CPTCode = code.BillingCode;
                        pbd.Completed = 0;
                        pbd.Total = code.TargetReadings*60;
                        pbd.IsTargetMet = false;
                        pbd.ReadyTobill = 0;
                        string date = stDate.ToString();
                        date = date.Replace("Z", "");
                        pbd.BillingStartDate = DateTime.Parse(date);
                        return pbd;
                    }
                    TotalReading = PatientInteractiontim.Sum(s => s.Duration);

                    if (TotalReading >= code.TargetReadings * 60)
                    {
                        TotalReading = code.TargetReadings * 60;
                    }
                    string Newdate = stDate.ToString();
                    Newdate = Newdate.Replace("Z", "");
                    pbd.BillingStartDate = DateTime.Parse(Newdate);
                    pbd.CPTCode = code.BillingCode;
                    pbd.Completed = TotalReading;
                    pbd.Total = code.TargetReadings * 60;
                    pbd.IsTargetMet = (TotalReading >= code.TargetReadings * 60) ? true : false;
                    pbd.ReadyTobill = (TotalReading >= (code.TargetReadings * 60) && DaysCompleted >= code.BillingThreshold) ? 1 : 0;
                    return pbd;
                }
            }
            catch (Exception ex)
            {
                throw;
            }

        }
        public PatientBilldata ProcessCPT99489_CycleBasedBilling(int patientId, int patientProgramId, BillingCodesDetails code,
                                                    BillingCodesDetails code487, string connectionString)
        {
            PatientBilldata pbd = new PatientBilldata();
            try
            {
                int DaysCompleted = 0; int TotalReading = 0;
                PatientStartDate PatientStartDate = new PatientStartDate();
                PatientStartDate = GetPatientBillingStartDateEx(patientId, patientProgramId, code.BillingCodeID, connectionString);
                if (PatientStartDate == null ||
                    PatientStartDate.Status.ToLower() == "invalid")
                {
                    pbd.CPTCode = code.BillingCode;
                    pbd.Completed = 0;
                    pbd.Total = code.TargetReadings*60;
                    pbd.IsTargetMet = false;
                    pbd.ReadyTobill = 0;
                    return pbd;
                }
                else if (PatientStartDate.StartDate > DateTime.UtcNow && PatientStartDate.Status.ToLower() == "active")
                {
                    PatientBilldata PatientBilldata = new PatientBilldata();
                    PatientBilldata.CPTCode = code.BillingCode;
                    PatientBilldata.Completed = 0;
                    PatientBilldata.Total = code.TargetReadings*60;
                    PatientBilldata.IsTargetMet = false;
                    PatientBilldata.ReadyTobill = 0;
                    return PatientBilldata;
                }
                else
                {
                    DateTime stDate = Convert.ToDateTime(PatientStartDate.StartDate);
                    DateTime endDate = DateTime.UtcNow;
                    //Note: This will happen in the last day of billing.. In the last day, billed date will set in 
                    //the patientbilling table and +1 day will come as next start day.. So we should   
                    // calcualte the start date with respect to the billing threshold
                    DateTime today = DateTime.UtcNow;
                    DateTime endDateTempay = GetLocalTimeFromUTC((DateTime)today, connectionString);
                    if (stDate.Date > endDateTempay)
                    {
                        //stDate = stDate.AddDays(-1 * code.BillingThreshold);
                        BilledDates bd = GetPatientLastBilledPeriods(patientId, patientProgramId, code.BillingCodeID, connectionString);
                        if (bd == null)
                        {
                            PatientBilldata PatientBilldata = new PatientBilldata();
                            PatientBilldata.CPTCode = code.BillingCode;
                            PatientBilldata.Completed = 0;
                            PatientBilldata.Total = code.TargetReadings*60;
                            PatientBilldata.IsTargetMet = false;
                            PatientBilldata.ReadyTobill = 0;
                            return PatientBilldata;
                        }
                        stDate = bd.StartDate;
                        endDate = bd.EndDate;
                    }

                    if (PatientStartDate.Status.ToLower() == "billeddate")
                    {
                        stDate = GetUTCFromLocalTime((DateTime)stDate, connectionString);
                        endDate =DateTime.UtcNow;
                    }
                    List<PatientInteraction> PatientInteractiontim = GetPatientInteractiontime(patientId, patientProgramId, connectionString, stDate, endDate).ToList();
                    if (PatientInteractiontim == null)
                    {
                        pbd.CPTCode = code.BillingCode;
                        pbd.Completed = 0;
                        pbd.Total = code.TargetReadings*60;
                        pbd.IsTargetMet = false;
                        pbd.ReadyTobill = 0;
                        string dateNew = stDate.ToString();
                        dateNew = dateNew.Replace("Z", "");
                        pbd.BillingStartDate = DateTime.Parse(dateNew);
                        return pbd;
                    }
                    //var isEstablishedExist = PatientInteractiontim.Where(s => s.IsCallNote == 1).ToList();
                    //if (isEstablishedExist.Count() > 0)
                    //{
                    //    isEstablishedExist = isEstablishedExist.Where(s => s.IsEstablishedCall == 1).ToList();
                    //    if (isEstablishedExist != null && isEstablishedExist.Count() == 0)
                    //    {
                    //        PatientInteractiontim.Clear();
                    //    }
                    //}
                    if (PatientInteractiontim.Count == 0 || PatientInteractiontim == null /*|| isEstablishedExist.Count == 0*/)
                    {
                        pbd.CPTCode = code.BillingCode;
                        pbd.Completed = 0;
                        pbd.Total = code.TargetReadings*60;
                        pbd.IsTargetMet = false;
                        pbd.ReadyTobill = 0;
                        string date = stDate.ToString();
                        date = date.Replace("Z", "");
                        pbd.BillingStartDate = DateTime.Parse(date);
                        return pbd;
                    }
                    TotalReading = PatientInteractiontim.Sum(s => s.Duration);

                    if (TotalReading > code487.TargetReadings * 60)
                    {
                        TimeSpan t1 = TimeSpan.FromSeconds(TotalReading);
                        TotalReading = (int)(t1.TotalSeconds - (code487.TargetReadings * 60));

                        string dateNew = stDate.ToString();
                        dateNew = dateNew.Replace("Z", "");
                        pbd.BillingStartDate = DateTime.Parse(dateNew);
                        pbd.CPTCode = code.BillingCode;
                        pbd.Completed = TotalReading;

                        //base 40
                        pbd.Total = code.TargetReadings * 60;
                        pbd.ReadyTobill = (TotalReading >= (code.TargetReadings * 60) && DaysCompleted >= code.BillingThreshold) ? 1 : 0;

                        pbd.IsTargetMet = (TotalReading >= code.TargetReadings * 60) ? true : false;
                    }
                    else
                    {
                        //Target reading 20
                        pbd.BillingStartDate = (DateTime)stDate;
                        pbd.CPTCode = code.BillingCode;
                        pbd.Completed = 0;
                        pbd.Total = code.TargetReadings * 60;
                        pbd.IsTargetMet = (TotalReading >= code.TargetReadings * 60) ? true : false;
                        pbd.ReadyTobill = (TotalReading >= (code.TargetReadings * 60) && DaysCompleted >= code.BillingThreshold) ? 1 : 0;

                    }
                    return pbd;
                }
            }
            catch (Exception ex)
            {
                throw;
            }

        }
        public PatientBilldata ProcessCPT99424_CycleBasedBilling(int patientId, int patientProgramId, BillingCodesDetails code, string connectionString)
        {
            PatientBilldata pbd = new PatientBilldata();
            try
            {
                int DaysCompleted = 0; int TotalReading = 0;
                PatientStartDate PatientStartDate = new PatientStartDate();
                PatientStartDate = GetPatientBillingStartDateEx(patientId, patientProgramId, code.BillingCodeID, connectionString);
                if (PatientStartDate == null ||
                    PatientStartDate.Status.ToLower() == "invalid")
                {
                    pbd.CPTCode = code.BillingCode;
                    pbd.Completed = 0;
                    pbd.Total = code.TargetReadings*60;
                    pbd.IsTargetMet = false;
                    pbd.ReadyTobill = 0;
                    return pbd;
                }
                else if (PatientStartDate.StartDate > DateTime.UtcNow && PatientStartDate.Status.ToLower() == "active")
                {
                    PatientBilldata PatientBilldata = new PatientBilldata();
                    PatientBilldata.CPTCode = code.BillingCode;
                    PatientBilldata.Completed = 0;
                    PatientBilldata.Total = code.TargetReadings*60;
                    PatientBilldata.IsTargetMet = false;
                    PatientBilldata.ReadyTobill = 0;
                    return PatientBilldata;
                }
                else
                {

                    DateTime stDate = Convert.ToDateTime(PatientStartDate.StartDate);
                    DateTime endDate = DateTime.UtcNow;

                    //Note: This will happen in the last day of billing.. In the last day, billed date will set in 
                    //the patientbilling table and +1 day will come as next start day.. So we should   
                    // calcualte the start date with respect to the billing threshold
                    DateTime today = DateTime.UtcNow;
                    DateTime endDateTempay = GetLocalTimeFromUTC((DateTime)today, connectionString);
                    if (stDate.Date > endDateTempay)
                    {
                        //stDate = stDate.AddDays(-1 * code.BillingThreshold);
                        BilledDates bd = GetPatientLastBilledPeriods(patientId, patientProgramId, code.BillingCodeID, connectionString);
                        if (bd == null)
                        {
                            PatientBilldata PatientBilldata = new PatientBilldata();
                            PatientBilldata.CPTCode = code.BillingCode;
                            PatientBilldata.Completed = 0;
                            PatientBilldata.Total = code.TargetReadings*60;
                            PatientBilldata.IsTargetMet = false;
                            PatientBilldata.ReadyTobill = 0;
                            return PatientBilldata;
                        }
                        stDate = bd.StartDate;

                    }



                    if (PatientStartDate.Status.ToLower() == "billeddate")
                    {
                        stDate = GetUTCFromLocalTime((DateTime)stDate, connectionString);
                        endDate =DateTime.UtcNow;

                    }


                    List<PatientInteraction> PatientInteractiontim = GetPatientInteractiontime(patientId, patientProgramId, connectionString, stDate, endDate).ToList();
                    if (PatientInteractiontim == null)
                    {
                        pbd.CPTCode = code.BillingCode;
                        pbd.Completed = 0;
                        pbd.Total = code.TargetReadings*60;
                        pbd.IsTargetMet = false;
                        pbd.ReadyTobill = 0;
                        string dateNew = stDate.ToString();
                        dateNew = dateNew.Replace("Z", "");
                        pbd.BillingStartDate = DateTime.Parse(dateNew);
                        return pbd;
                    }
                    //var isEstablishedExist = PatientInteractiontim.Where(s => s.IsCallNote == 1).ToList();
                    //if (isEstablishedExist.Count() > 0)
                    //{
                    //    isEstablishedExist = isEstablishedExist.Where(s => s.IsEstablishedCall == 1).ToList();
                    //    if (isEstablishedExist != null && isEstablishedExist.Count() == 0)
                    //    {
                    //        PatientInteractiontim.Clear();
                    //    }
                    //}
                    if (PatientInteractiontim.Count == 0 || PatientInteractiontim == null /*|| isEstablishedExist.Count == 0*/)
                    {
                        pbd.CPTCode = code.BillingCode;
                        pbd.Completed = 0;
                        pbd.Total = code.TargetReadings*60;
                        pbd.IsTargetMet = false;
                        pbd.ReadyTobill = 0;
                        string date = stDate.ToString();
                        date = date.Replace("Z", "");
                        pbd.BillingStartDate = DateTime.Parse(date);
                        return pbd;
                    }
                    TotalReading = PatientInteractiontim.Sum(s => s.Duration);

                    if (TotalReading >= code.TargetReadings * 60)
                    {
                        TotalReading = code.TargetReadings * 60;
                    }
                    string Newdate = stDate.ToString();
                    Newdate = Newdate.Replace("Z", "");
                    pbd.BillingStartDate = DateTime.Parse(Newdate);
                    pbd.CPTCode = code.BillingCode;
                    pbd.Completed = TotalReading;
                    pbd.Total = code.TargetReadings * 60;
                    pbd.IsTargetMet = (TotalReading >= code.TargetReadings * 60) ? true : false;
                    pbd.ReadyTobill = (TotalReading >= (code.TargetReadings * 60) && DaysCompleted >= code.BillingThreshold) ? 1 : 0;
                    return pbd;
                }
            }
            catch (Exception ex)
            {
                throw;
            }

        }
        public PatientBilldata ProcessCPT99425_CycleBasedBilling(int patientId, int patientProgramId, BillingCodesDetails code,
                                                    BillingCodesDetails code424, string connectionString)
        {
            PatientBilldata pbd = new PatientBilldata();
            try
            {
                int DaysCompleted = 0; int TotalReading = 0;
                PatientStartDate PatientStartDate = new PatientStartDate();
                PatientStartDate = GetPatientBillingStartDateEx(patientId, patientProgramId, code.BillingCodeID, connectionString);
                if (PatientStartDate == null ||
                    PatientStartDate.Status.ToLower() == "invalid")
                {
                    pbd.CPTCode = code.BillingCode;
                    pbd.Completed = 0;
                    pbd.Total = code.TargetReadings*60;
                    pbd.IsTargetMet = false;
                    pbd.ReadyTobill = 0;
                    return pbd;
                }
                else if (PatientStartDate.StartDate > DateTime.UtcNow && PatientStartDate.Status.ToLower() == "active")
                {
                    PatientBilldata PatientBilldata = new PatientBilldata();
                    PatientBilldata.CPTCode = code.BillingCode;
                    PatientBilldata.Completed = 0;
                    PatientBilldata.Total = code.TargetReadings*60;
                    PatientBilldata.IsTargetMet = false;
                    PatientBilldata.ReadyTobill = 0;
                    return PatientBilldata;
                }
                else
                {
                    DateTime stDate = Convert.ToDateTime(PatientStartDate.StartDate);
                    DateTime endDate = DateTime.UtcNow;
                    //Note: This will happen in the last day of billing.. In the last day, billed date will set in 
                    //the patientbilling table and +1 day will come as next start day.. So we should   
                    // calcualte the start date with respect to the billing threshold
                    DateTime today = DateTime.UtcNow;
                    DateTime endDateTempay = GetLocalTimeFromUTC((DateTime)today, connectionString);
                    if (stDate.Date > endDateTempay)
                    {
                        //stDate = stDate.AddDays(-1 * code.BillingThreshold);
                        BilledDates bd = GetPatientLastBilledPeriods(patientId, patientProgramId, code.BillingCodeID, connectionString);
                        if (bd == null)
                        {
                            PatientBilldata PatientBilldata = new PatientBilldata();
                            PatientBilldata.CPTCode = code.BillingCode;
                            PatientBilldata.Completed = 0;
                            PatientBilldata.Total = code.TargetReadings*60;
                            PatientBilldata.IsTargetMet = false;
                            PatientBilldata.ReadyTobill = 0;
                            return PatientBilldata;
                        }
                        stDate = bd.StartDate;
                        endDate = bd.EndDate;
                    }

                    if (PatientStartDate.Status.ToLower() == "billeddate")
                    {
                        stDate = GetUTCFromLocalTime((DateTime)stDate, connectionString);
                        endDate =DateTime.UtcNow;
                    }
                    List<PatientInteraction> PatientInteractiontim = GetPatientInteractiontime(patientId, patientProgramId, connectionString, stDate, endDate).ToList();
                    if (PatientInteractiontim == null)
                    {
                        pbd.CPTCode = code.BillingCode;
                        pbd.Completed = 0;
                        pbd.Total = code.TargetReadings*60;
                        pbd.IsTargetMet = false;
                        pbd.ReadyTobill = 0;
                        string Newdate = stDate.ToString();
                        Newdate = Newdate.Replace("Z", "");
                        pbd.BillingStartDate = DateTime.Parse(Newdate);
                        return pbd;
                    }
                    //var isEstablishedExist = PatientInteractiontim.Where(s => s.IsCallNote == 1).ToList();
                    //if (isEstablishedExist.Count() > 0)
                    //{
                    //    isEstablishedExist = isEstablishedExist.Where(s => s.IsEstablishedCall == 1).ToList();
                    //    if (isEstablishedExist != null && isEstablishedExist.Count() == 0)
                    //    {
                    //        PatientInteractiontim.Clear();
                    //    }
                    //}
                    if (PatientInteractiontim.Count == 0 || PatientInteractiontim == null /*|| isEstablishedExist.Count == 0*/)
                    {
                        pbd.CPTCode = code.BillingCode;
                        pbd.Completed = 0;
                        pbd.Total = code.TargetReadings*60;
                        pbd.IsTargetMet = false;
                        pbd.ReadyTobill = 0;
                        string date = stDate.ToString();
                        date = date.Replace("Z", "");
                        pbd.BillingStartDate = DateTime.Parse(date);
                        return pbd;
                    }
                    TotalReading = PatientInteractiontim.Sum(s => s.Duration);

                    if (TotalReading > code424.TargetReadings * 60)
                    {
                        TimeSpan t1 = TimeSpan.FromSeconds(TotalReading);
                        TotalReading = (int)(t1.TotalSeconds - (code424.TargetReadings * 60));

                        string dateNew = stDate.ToString();
                        dateNew = dateNew.Replace("Z", "");
                        pbd.BillingStartDate = DateTime.Parse(dateNew);
                        pbd.CPTCode = code.BillingCode;
                        pbd.Completed = TotalReading;
                        if (TotalReading < code424.TargetReadings * 60)
                        {
                            // base 20
                            pbd.Total = code424.TargetReadings * 60;
                            pbd.ReadyTobill = (TotalReading >= (code424.TargetReadings * 60) && DaysCompleted >= code.BillingThreshold) ? 1 : 0;
                        }
                        else
                        {
                            //base 40
                            pbd.Total = code.TargetReadings * 60;
                            pbd.ReadyTobill = (TotalReading >= (code.TargetReadings * 60) && DaysCompleted >= code.BillingThreshold) ? 1 : 0;
                        }
                        pbd.IsTargetMet = (TotalReading >= code424.TargetReadings * 60) ? true : false;
                    }
                    else
                    {
                        //Target reading 20
                        string dateNews = stDate.ToString();
                        dateNews = dateNews.Replace("Z", "");
                        pbd.BillingStartDate = DateTime.Parse(dateNews);
                        pbd.CPTCode = code.BillingCode;
                        pbd.Completed = 0;
                        pbd.Total = code424.TargetReadings * 60;
                        pbd.IsTargetMet = (TotalReading >= code424.TargetReadings * 60) ? true : false;
                        pbd.ReadyTobill = (TotalReading >= (code424.TargetReadings * 60) && DaysCompleted >= code.BillingThreshold) ? 1 : 0;

                    }
                    return pbd;
                }
            }
            catch (Exception ex)
            {
                throw;
            }

        }
        public PatientBilldata ProcessCPT99426_CycleBasedBilling(int patientId, int patientProgramId, BillingCodesDetails code, string connectionString)
        {
            PatientBilldata pbd = new PatientBilldata();
            try
            {
                int DaysCompleted = 0; int TotalReading = 0;
                PatientStartDate PatientStartDate = new PatientStartDate();
                PatientStartDate = GetPatientBillingStartDateEx(patientId, patientProgramId, code.BillingCodeID, connectionString);
                if (PatientStartDate == null ||
                    PatientStartDate.Status.ToLower() == "invalid")
                {
                    pbd.CPTCode = code.BillingCode;
                    pbd.Completed = 0;
                    pbd.Total = code.TargetReadings*60;
                    pbd.IsTargetMet = false;
                    pbd.ReadyTobill = 0;
                    return pbd;
                }
                else if (PatientStartDate.StartDate > DateTime.UtcNow && PatientStartDate.Status.ToLower() == "active")
                {
                    PatientBilldata PatientBilldata = new PatientBilldata();
                    PatientBilldata.CPTCode = code.BillingCode;
                    PatientBilldata.Completed = 0;
                    PatientBilldata.Total = code.TargetReadings*60;
                    PatientBilldata.IsTargetMet = false;
                    PatientBilldata.ReadyTobill = 0;
                    return PatientBilldata;
                }
                else
                {

                    DateTime stDate = Convert.ToDateTime(PatientStartDate.StartDate);
                    DateTime endDate = DateTime.UtcNow;

                    //Note: This will happen in the last day of billing.. In the last day, billed date will set in 
                    //the patientbilling table and +1 day will come as next start day.. So we should   
                    // calcualte the start date with respect to the billing threshold
                    DateTime today = DateTime.UtcNow;
                    DateTime endDateTempay = GetLocalTimeFromUTC((DateTime)today, connectionString);
                    if (stDate.Date > endDateTempay)
                    {
                        //stDate = stDate.AddDays(-1 * code.BillingThreshold);
                        BilledDates bd = GetPatientLastBilledPeriods(patientId, patientProgramId, code.BillingCodeID, connectionString);
                        if (bd == null)
                        {
                            PatientBilldata PatientBilldata = new PatientBilldata();
                            PatientBilldata.CPTCode = code.BillingCode;
                            PatientBilldata.Completed = 0;
                            PatientBilldata.Total = code.TargetReadings*60;
                            PatientBilldata.IsTargetMet = false;
                            PatientBilldata.ReadyTobill = 0;
                            return PatientBilldata;
                        }
                        stDate = bd.StartDate;

                    }



                    if (PatientStartDate.Status.ToLower() == "billeddate")
                    {
                        stDate = GetUTCFromLocalTime((DateTime)stDate, connectionString);
                        endDate =DateTime.UtcNow;

                    }


                    List<PatientInteraction> PatientInteractiontim = GetPatientInteractiontime(patientId, patientProgramId, connectionString, stDate, endDate).ToList();
                    if (PatientInteractiontim == null)
                    {
                        pbd.CPTCode = code.BillingCode;
                        pbd.Completed = 0;
                        pbd.Total = code.TargetReadings*60;
                        pbd.IsTargetMet = false;
                        pbd.ReadyTobill = 0;
                        string dateNews = stDate.ToString();
                        dateNews = dateNews.Replace("Z", "");
                        pbd.BillingStartDate = DateTime.Parse(dateNews);
                        return pbd;
                    }
                    //var isEstablishedExist = PatientInteractiontim.Where(s => s.IsCallNote == 1).ToList();
                    //if (isEstablishedExist.Count() > 0)
                    //{
                    //    isEstablishedExist = isEstablishedExist.Where(s => s.IsEstablishedCall == 1).ToList();
                    //    if (isEstablishedExist != null && isEstablishedExist.Count() == 0)
                    //    {
                    //        PatientInteractiontim.Clear();
                    //    }
                    //}
                    if (PatientInteractiontim.Count == 0 || PatientInteractiontim == null /*|| isEstablishedExist.Count == 0*/)
                    {
                        pbd.CPTCode = code.BillingCode;
                        pbd.Completed = 0;
                        pbd.Total = code.TargetReadings*60;
                        pbd.IsTargetMet = false;
                        pbd.ReadyTobill = 0;
                        string dateNew = stDate.ToString();
                        dateNew = dateNew.Replace("Z", "");
                        pbd.BillingStartDate = DateTime.Parse(dateNew);
                        return pbd;
                    }
                    TotalReading = PatientInteractiontim.Sum(s => s.Duration);

                    if (TotalReading >= code.TargetReadings * 60)
                    {
                        TotalReading = code.TargetReadings * 60;
                    }
                    string dates = stDate.ToString();
                    dates = dates.Replace("Z", "");
                    pbd.BillingStartDate = DateTime.Parse(dates);
                    pbd.CPTCode = code.BillingCode;
                    pbd.Completed = TotalReading;
                    pbd.Total = code.TargetReadings * 60;
                    pbd.IsTargetMet = (TotalReading >= code.TargetReadings * 60) ? true : false;
                    pbd.ReadyTobill = (TotalReading >= (code.TargetReadings * 60) && DaysCompleted >= code.BillingThreshold) ? 1 : 0;
                    return pbd;
                }
            }
            catch (Exception ex)
            {
                throw;
            }

        }
        public PatientBilldata ProcessCPT99427_CycleBasedBilling(int patientId, int patientProgramId, BillingCodesDetails code,
                                                    BillingCodesDetails code426, string connectionString)
        {
            PatientBilldata pbd = new PatientBilldata();
            try
            {
                int DaysCompleted = 0; int TotalReading = 0;
                PatientStartDate PatientStartDate = new PatientStartDate();
                PatientStartDate = GetPatientBillingStartDateEx(patientId, patientProgramId, code.BillingCodeID, connectionString);
                if (PatientStartDate == null ||
                    PatientStartDate.Status.ToLower() == "invalid")
                {
                    pbd.CPTCode = code.BillingCode;
                    pbd.Completed = 0;
                    pbd.Total = code.TargetReadings*60;
                    pbd.IsTargetMet = false;
                    pbd.ReadyTobill = 0;
                    return pbd;
                }
                else if (PatientStartDate.StartDate > DateTime.UtcNow && PatientStartDate.Status.ToLower() == "active")
                {
                    PatientBilldata PatientBilldata = new PatientBilldata();
                    PatientBilldata.CPTCode = code.BillingCode;
                    PatientBilldata.Completed = 0;
                    PatientBilldata.Total = code.TargetReadings*60;
                    PatientBilldata.IsTargetMet = false;
                    PatientBilldata.ReadyTobill = 0;
                    return PatientBilldata;
                }
                else
                {
                    DateTime stDate = Convert.ToDateTime(PatientStartDate.StartDate);
                    DateTime endDate = DateTime.UtcNow;
                    //Note: This will happen in the last day of billing.. In the last day, billed date will set in 
                    //the patientbilling table and +1 day will come as next start day.. So we should   
                    // calcualte the start date with respect to the billing threshold
                    DateTime today = DateTime.UtcNow;
                    DateTime endDateTempay = GetLocalTimeFromUTC((DateTime)today, connectionString);
                    if (stDate.Date > endDateTempay)
                    {
                        //stDate = stDate.AddDays(-1 * code.BillingThreshold);
                        BilledDates bd = GetPatientLastBilledPeriods(patientId, patientProgramId, code.BillingCodeID, connectionString);
                        if (bd == null)
                        {
                            PatientBilldata PatientBilldata = new PatientBilldata();
                            PatientBilldata.CPTCode = code.BillingCode;
                            PatientBilldata.Completed = 0;
                            PatientBilldata.Total = code.TargetReadings*60;
                            PatientBilldata.IsTargetMet = false;
                            PatientBilldata.ReadyTobill = 0;
                            return PatientBilldata;
                        }
                        stDate = bd.StartDate;
                        endDate = bd.EndDate;
                    }

                    if (PatientStartDate.Status.ToLower() == "billeddate")
                    {
                        stDate = GetUTCFromLocalTime((DateTime)stDate, connectionString);
                        endDate =DateTime.UtcNow;
                    }
                    List<PatientInteraction> PatientInteractiontim = GetPatientInteractiontime(patientId, patientProgramId, connectionString, stDate, endDate).ToList();
                    if (PatientInteractiontim == null)
                    {
                        pbd.CPTCode = code.BillingCode;
                        pbd.Completed = 0;
                        pbd.Total = code.TargetReadings*60;
                        pbd.IsTargetMet = false;
                        pbd.ReadyTobill = 0;
                        string dates = stDate.ToString();
                        dates = dates.Replace("Z", "");
                        pbd.BillingStartDate = DateTime.Parse(dates);
                        return pbd;
                    }
                    //var isEstablishedExist = PatientInteractiontim.Where(s => s.IsCallNote == 1).ToList();
                    //if (isEstablishedExist.Count() > 0)
                    //{
                    //    isEstablishedExist = isEstablishedExist.Where(s => s.IsEstablishedCall == 1).ToList();
                    //    if (isEstablishedExist != null && isEstablishedExist.Count() == 0)
                    //    {
                    //        PatientInteractiontim.Clear();
                    //    }
                    //}
                    if (PatientInteractiontim.Count == 0 || PatientInteractiontim == null /*|| isEstablishedExist.Count == 0*/)
                    {
                        pbd.CPTCode = code.BillingCode;
                        pbd.Completed = 0;
                        pbd.Total = code.TargetReadings*60;
                        pbd.IsTargetMet = false;
                        pbd.ReadyTobill = 0;
                        string dateNew = stDate.ToString();
                        dateNew = dateNew.Replace("Z", "");
                        pbd.BillingStartDate = DateTime.Parse(dateNew);
                        return pbd;
                    }
                    TotalReading = PatientInteractiontim.Sum(s => s.Duration);

                    if (TotalReading > code426.TargetReadings * 60)
                    {
                        TimeSpan t1 = TimeSpan.FromSeconds(TotalReading);
                        TotalReading = (int)(t1.TotalSeconds - (code426.TargetReadings * 60));

                        string dateNw = stDate.ToString();
                        dateNw = dateNw.Replace("Z", "");
                        pbd.BillingStartDate = DateTime.Parse(dateNw);
                        pbd.CPTCode = code.BillingCode;
                        pbd.Completed = TotalReading;
                        if (TotalReading < code426.TargetReadings * 60)
                        {
                            // base 20
                            pbd.Total = code426.TargetReadings * 60;
                            pbd.ReadyTobill = (TotalReading >= (code426.TargetReadings * 60) && DaysCompleted >= code.BillingThreshold) ? 1 : 0;
                        }
                        else
                        {
                            //base 40
                            pbd.Total = code.TargetReadings * 60;
                            pbd.ReadyTobill = (TotalReading >= (code.TargetReadings * 60) && DaysCompleted >= code.BillingThreshold) ? 1 : 0;
                        }
                        pbd.IsTargetMet = (TotalReading >= code426.TargetReadings * 60) ? true : false;
                    }
                    else
                    {
                        //Target reading 20
                        string dateN = stDate.ToString();
                        dateN = dateN.Replace("Z", "");
                        pbd.BillingStartDate = DateTime.Parse(dateN);
                        pbd.CPTCode = code.BillingCode;
                        pbd.Completed = 0;
                        pbd.Total = code426.TargetReadings * 60;
                        pbd.IsTargetMet = (TotalReading >= code426.TargetReadings * 60) ? true : false;
                        pbd.ReadyTobill = (TotalReading >= (code426.TargetReadings * 60) && DaysCompleted >= code.BillingThreshold) ? 1 : 0;

                    }
                    return pbd;
                }
            }
            catch (Exception ex)
            {
                throw;
            }

        }

        public string DownloadInvoice(string Blob_Conn_String, string ContainerName, string ConnectionString)
        {
            string Uri = "";
            try
            {
                BlobContainerClient containerClient = new BlobContainerClient(Blob_Conn_String, ContainerName);
                //string Filename = "Invoice" + DateTime.Now + "." + "pdf";
                string Filename = "Invoice_" + DateTime.Now + "." + "pdf";
                var filePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\" + "rpmfolder" + Path.GetExtension(Filename);
                if (!Directory.Exists(filePath))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                }
                GenerateInvoice(filePath);

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
        public class BilledDates
        {
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
        }
        public class VitalMesureData
        {
            public DateTime ValueDate { get; set; }
            public string Value { get; set; }
        }
        public class PatientStartDate
        {
            public DateTime? StartDate { get; set; }

            public string Status { get; set; }
        }

        public bool DeleteDraft(int PatientId, string ConnectionString)
        {
            bool response = false;
            using (SqlConnection con = new SqlConnection(ConnectionString))
            {
                SqlCommand command = new SqlCommand("usp_DelDraftPatients", con);

                command.CommandType = CommandType.StoredProcedure;


                command.Parameters.AddWithValue("@patientid", PatientId);
                con.Open();
                SqlParameter returnParameter = command.Parameters.Add("RetVal", SqlDbType.Int);
                returnParameter.Direction = ParameterDirection.ReturnValue;

                command.ExecuteNonQuery();
                int id = (int)returnParameter.Value;
                if (!id.Equals(0))
                {
                    response = true;
                }
                con.Close();
                return response;
            }

        }
        public bool DeletePatientDocuments(int documentid, string ConnectionString)
        {
            bool response = false;
            using (SqlConnection con = new SqlConnection(ConnectionString))
            {
                SqlCommand command = new SqlCommand("usp_DelPatientDocument", con);

                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.AddWithValue("@documentid", documentid);
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
        public string GetPatientBillingReportDetails(DateTime startDate, DateTime endDate, int? patientId, string clinic, int isMonth, string CreatedBy, string Format, string ConnectionString, string Blob_Conn_String, string ContainerName)
        {
            bool ispatient = false;
            bool isClinic = false;
            bool isCptCode = false;
            string Uri = "";
            //List<PatientBillReport> list = new List<PatientBillReport>();
            List<PatientBillReportDetails> Reportlist = new List<PatientBillReportDetails>();
            try
            {
                using (SqlConnection con = new SqlConnection(ConnectionString))
                {
                    con.Open();

                    using (SqlCommand command = new SqlCommand("usp_GetPatientBillingReportDetails", con))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.Add("@startDate", SqlDbType.DateTime).Value = startDate;
                        command.Parameters.Add("@endDate", SqlDbType.DateTime).Value = endDate;
                        if (!String.IsNullOrEmpty(patientId?.ToString()))
                        {
                            command.Parameters.Add("@patientId", SqlDbType.Int).Value = patientId;
                            ispatient = true;
                        }
                        else
                        {
                            command.Parameters.Add("@patientId", SqlDbType.Int).Value = DBNull.Value;
                        }

                        // command.Parameters.Add("@CreatedBy", SqlDbType.NVarChar).Value = CreatedBy;
                        SqlDataReader reader = command.ExecuteReader();
                        while (reader.Read())
                        {
                            PatientBillReportDetails info = new PatientBillReportDetails();
                            info.PatientName = reader["PATIENT"].ToString();
                            info.PatientNumber = reader["PatientNumber"].ToString();
                            info.DOS = Convert.ToDateTime(reader["DOS"]).ToShortDateString();
                            info.DOB = Convert.ToDateTime(reader["DOB"]).ToShortDateString(); ;
                            info.CPT99454 = reader["99454"].ToString();
                            info.CPT99457 = reader["99457"].ToString();
                            info.CPT99457 = Convert.ToString(ConvertSecondsToMinutes(Convert.ToInt32(info.CPT99457)));
                            info.CPT99458_First = reader["99458"].ToString();
                            info.CPT99458_First = Convert.ToString(ConvertSecondsToMinutes(Convert.ToInt32(info.CPT99458_First)));
                            info.CPT99458_Second = reader["99458Sec"].ToString();
                            info.CPT99458_Second = Convert.ToString(ConvertSecondsToMinutes(Convert.ToInt32(info.CPT99458_Second)));
                            info.EnrolledDate = Convert.ToDateTime(reader["ENROLL DATE"]);
                            info.clinic = reader["CLINIC"].ToString();
                            info.clinicname = reader["clinicname"].ToString();
                            Reportlist.Add(info);

                        }
                        reader.Close();
                        /*if (list.Count > 0)
                        {

                            foreach (var item in list)
                            {
                                using (SqlCommand commands = new SqlCommand("usp_GetReportData", con))
                                {

                                    commands.CommandType = CommandType.StoredProcedure;
                                    commands.Parameters.Add("@patientId", SqlDbType.Int).Value = item.PatientId;
                                    commands.Parameters.Add("@billingCode", SqlDbType.NVarChar).Value = item.BillingCode.ToString();

                                    // command.Parameters.Add("@CreatedBy", SqlDbType.NVarChar).Value = CreatedBy;
                                    SqlDataReader readers = commands.ExecuteReader();

                                    while (readers.Read())
                                    {

                                        item.DiagnosisCode = readers["DiagnosisCode"].ToString();
                                        item.DeviceName = readers["DeviceNames"].ToString();
                                        DateTime? lastDate = (readers["LastBilledDate"] as DateTime?) ?? null;
                                        if (lastDate == null)
                                        {
                                            item.LastReadyToBillDate = null;
                                        }
                                        else
                                        {
                                            DateTime dt = Convert.ToDateTime(readers["LastBilledDate"]);
                                            item.LastReadyToBillDate = dt.ToString("yyyy-MM-dd");

                                        }
                                        item.DeviceActivateDate = readers["DeviceActivationdate"].ToString();

                                        Reportlist.Add(item);

                                    }
                                    readers.Close();

                                }

                            }
                            list.Clear();
                        }*/
                    }
                }
                if (!string.IsNullOrEmpty(clinic))
                {
                    Reportlist = (List<PatientBillReportDetails>)Reportlist.Where(s => s.clinic == clinic).ToList();
                    isClinic = true;
                    isCptCode = true;
                    if (Reportlist.Count>0)
                        clinic = Reportlist[0].clinicname.ToString();
                }

                if (Reportlist.Count > 0)
                {
                    Uri = genaratePdfForDetails(Reportlist, patientId, clinic, isMonth, startDate, endDate, Blob_Conn_String, ContainerName, Format);
                }

                return Uri;
            }
            catch (Exception)
            {

                throw;
            }

        }
        public string GetPatientMissingBillingReportDetails(DateTime startDate, DateTime endDate, int? patientId, string clinic, int isMonth, string CreatedBy, string Format, string ConnectionString, string Blob_Conn_String, string ContainerName)
        {
            bool ispatient = false;
            bool isClinic = false;
            bool isCptCode = false;
            string Uri = "";
            //List<PatientBillReport> list = new List<PatientBillReport>();
            List<PatientBillReportDetails> Reportlist = new List<PatientBillReportDetails>();
            try
            {
                using (SqlConnection con = new SqlConnection(ConnectionString))
                {
                    con.Open();

                    using (SqlCommand command = new SqlCommand("usp_GetPatientMissingBillingReportDetails", con))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.Add("@startDate", SqlDbType.DateTime).Value = startDate;
                        command.Parameters.Add("@endDate", SqlDbType.DateTime).Value = endDate;
                        if (!String.IsNullOrEmpty(patientId?.ToString()))
                        {
                            command.Parameters.Add("@patientId", SqlDbType.Int).Value = patientId;
                            ispatient = true;
                        }
                        else
                        {
                            command.Parameters.Add("@patientId", SqlDbType.Int).Value = DBNull.Value;
                        }

                        // command.Parameters.Add("@CreatedBy", SqlDbType.NVarChar).Value = CreatedBy;
                        SqlDataReader reader = command.ExecuteReader();
                        while (reader.Read())
                        {
                            PatientBillReportDetails info = new PatientBillReportDetails();
                            info.PatientName = reader["PATIENT"].ToString();
                            info.PatientNumber = reader["PatientNumber"].ToString();
                            info.DOS = Convert.ToDateTime(reader["DOS"]).ToShortDateString();
                            info.DOB = Convert.ToDateTime(reader["DOB"]).ToShortDateString(); ;
                            info.CPT99454 = reader["99454"].ToString();
                            info.CPT99457 = reader["99457"].ToString();
                            info.CPT99457 = Convert.ToString(ConvertSecondsToMinutes(Convert.ToInt32(info.CPT99457)));
                            info.CPT99458_First = reader["99458"].ToString();
                            info.CPT99458_First = Convert.ToString(ConvertSecondsToMinutes(Convert.ToInt32(info.CPT99458_First)));
                            info.CPT99458_Second = reader["99458Sec"].ToString();
                            info.CPT99458_Second = Convert.ToString(ConvertSecondsToMinutes(Convert.ToInt32(info.CPT99458_Second)));
                            info.EnrolledDate = Convert.ToDateTime(reader["ENROLL DATE"]);
                            info.clinic = reader["CLINIC"].ToString();
                            info.clinicname = reader["clinicname"].ToString();
                            Reportlist.Add(info);

                        }
                        reader.Close();
                        /*if (list.Count > 0)
                        {

                            foreach (var item in list)
                            {
                                using (SqlCommand commands = new SqlCommand("usp_GetReportData", con))
                                {

                                    commands.CommandType = CommandType.StoredProcedure;
                                    commands.Parameters.Add("@patientId", SqlDbType.Int).Value = item.PatientId;
                                    commands.Parameters.Add("@billingCode", SqlDbType.NVarChar).Value = item.BillingCode.ToString();

                                    // command.Parameters.Add("@CreatedBy", SqlDbType.NVarChar).Value = CreatedBy;
                                    SqlDataReader readers = commands.ExecuteReader();

                                    while (readers.Read())
                                    {

                                        item.DiagnosisCode = readers["DiagnosisCode"].ToString();
                                        item.DeviceName = readers["DeviceNames"].ToString();
                                        DateTime? lastDate = (readers["LastBilledDate"] as DateTime?) ?? null;
                                        if (lastDate == null)
                                        {
                                            item.LastReadyToBillDate = null;
                                        }
                                        else
                                        {
                                            DateTime dt = Convert.ToDateTime(readers["LastBilledDate"]);
                                            item.LastReadyToBillDate = dt.ToString("yyyy-MM-dd");

                                        }
                                        item.DeviceActivateDate = readers["DeviceActivationdate"].ToString();

                                        Reportlist.Add(item);

                                    }
                                    readers.Close();

                                }

                            }
                            list.Clear();
                        }*/
                    }
                }
                if (!string.IsNullOrEmpty(clinic))
                {
                    Reportlist = (List<PatientBillReportDetails>)Reportlist.Where(s => s.clinic == clinic).ToList();
                    isClinic = true;
                    isCptCode = true;
                    if (Reportlist.Count>0)
                        clinic = Reportlist[0].clinicname.ToString();
                }

                if (Reportlist.Count > 0)
                {
                    Uri = genaratePdfForDetails(Reportlist, patientId, clinic, isMonth, startDate, endDate, Blob_Conn_String, ContainerName, Format);
                }

                return Uri;
            }
            catch (Exception)
            {

                throw;
            }

        }

        public static int ConvertSecondsToMinutes(int seconds)
        {
            return seconds / 60;
        }
        public string genaratePdfForDetails(List<PatientBillReportDetails> data, int? Patientid, string clinic, int isMonth, DateTime startDate, DateTime endDate, string Blob_Conn_String, string ContainerName, string format)
        {
            string Uri = "";
            DataTable dataTable = new DataTable(typeof(PatientBillReportDetails).Name);

            //Get all the properties
            PropertyInfo[] Props = typeof(PatientBillReportDetails).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo prop in Props)
            {
                //Defining type of data column gives proper data table 
                var type = (prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>) ? Nullable.GetUnderlyingType(prop.PropertyType) : prop.PropertyType);
                //Setting column names as Property names
                dataTable.Columns.Add(prop.Name, type);
            }
            foreach (PatientBillReportDetails item in data.OrderBy(x => Convert.ToDateTime(x.DOS)).ToList())
            {
                var values = new object[Props.Length];
                for (int i = 0; i < Props.Length; i++)
                {
                    //inserting property values to datatable rows
                    values[i] = Props[i].GetValue(item, null);
                }
                dataTable.Rows.Add(values);
            }

            // dataTable.Columns.Remove("patientId");
            dataTable.Columns.Remove("clinic");
            dataTable.Columns.Remove("clinicname");
            try
            {
                BlobContainerClient containerClient = new BlobContainerClient(Blob_Conn_String, ContainerName);
                string Filename = "PatientBillingReportDetails_" + DateTime.Now.Day+DateTime.Now.Month+DateTime.Now.Year+DateTime.Now.Hour+DateTime.Now.Minute+DateTime.Now.Second + "." + format;
                var filePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\" + "rpmfolder" + Path.GetExtension(Filename);
                if (!Directory.Exists(filePath))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                }

                if (format == "xlsx")
                {
                    WriteExcelFile(filePath, dataTable);
                }
                else
                {
                    ExportDataTableToPdfDetails(dataTable, Patientid, clinic, isMonth, startDate, endDate, filePath);
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
        void ExportDataTableToPdfDetails(DataTable dtblTable, int? Patientid, string clinic, int isMonth, DateTime startDate, DateTime endDate, String strPdfPath)
        {
            try
            {
                string PatientNumber = dtblTable.Rows[0][2].ToString();

                PdfPTable table = new PdfPTable(dtblTable.Columns.Count);
                System.IO.FileStream fs = new FileStream(strPdfPath, FileMode.Create, FileAccess.Write, FileShare.None);
                Document document = new Document();
                var pgSize = new iTextSharp.text.Rectangle(1400, 1000);
                document.SetPageSize(pgSize);
                PdfWriter writer = PdfWriter.GetInstance(document, fs);
                document.Open();

                //Report Header
                string strHeader = "PATIENTS BILL REPORT";
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
                prgAuthor.Add(new Chunk("\nReport Date      : " + DateTime.Now.ToShortDateString(), fntAuthor));
                if (!string.IsNullOrEmpty(Patientid.ToString()))
                {
                    string patientNumber = PatientNumber;
                    prgAuthor.Add(new Chunk("\nPatient Id    : " + patientNumber, fntAuthor));
                }
                if (!string.IsNullOrEmpty(clinic))
                {

                    prgAuthor.Add(new Chunk("\nClinic        : " + clinic, fntAuthor));
                }

                if (isMonth != 1)
                {
                    prgAuthor.Add(new Chunk("\nStart Date    : " + startDate.ToShortDateString(), fntAuthor));
                    prgAuthor.Add(new Chunk("\nEnd Date      : " + endDate.ToShortDateString(), fntAuthor));
                }
                else
                {
                    prgAuthor.Add(new Chunk("\nMonth       : " + startDate.ToString("MMMM") + " " + startDate.Year, fntAuthor));
                }

                document.Add(prgAuthor);

                //Add a line seperation
                Paragraph p = new Paragraph(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(0.0F, 100.0F, BaseColor.BLACK, Element.ALIGN_LEFT, 1)));
                document.Add(p);

                //Add line break
                document.Add(new Chunk("\n", fntHead));

                //Write the table

                //Table header
                BaseFont btnColumnHeader = BaseFont.CreateFont(BaseFont.TIMES_ROMAN, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
                Font fntColumnHeader = new Font(btnColumnHeader, 10, 1, BaseColor.WHITE);


                for (int i = 0; i < dtblTable.Columns.Count; i++)
                {
                    var cells = new PdfPCell() { HorizontalAlignment = Element.ALIGN_CENTER };
                    cells.BackgroundColor = BaseColor.GRAY;
                    string coulmn = dtblTable.Columns[i].ToString();
                    cells.MinimumHeight=50;
                    cells.Phrase = new Phrase(coulmn);
                    //  cells.AddElement(new Chunk(dtblTable.Columns[i].ColumnName.ToUpper(), fntColumnHeader));
                    table.AddCell(cells);
                }
                //table Data
                var cell = new PdfPCell() { HorizontalAlignment = Element.ALIGN_CENTER };
                for (int i = 0; i < dtblTable.Rows.Count; i++)
                {
                    for (int j = 0; j < dtblTable.Columns.Count; j++)
                    {
                        cell.BackgroundColor = i % 2 == 0
                    ? BaseColor.LIGHT_GRAY : BaseColor.WHITE;
                        string row = dtblTable.Rows[i][j].ToString();
                        cell.MinimumHeight=50;
                        cell.Phrase = new Phrase(row);
                        table.AddCell(cell);
                    }
                }
                //for (int i = 0; i < 200; i++)
                //{
                //    for (int j = 0; j < dtblTable.Columns.Count; j++)
                //    {
                //        cell.BackgroundColor = i % 2 == 0
                //    ? BaseColor.LIGHT_GRAY : BaseColor.WHITE;
                //        string row = "sample text".ToString();

                //        cell.Phrase = new Phrase(row);
                //        table.AddCell(cell);
                //    }
                //}

                document.Add(table);
                document.Close();
                writer.Close();
                fs.Close();
            }
            catch
            {
                throw;
            }
        }
        public PatientDocuments GetMyPatientDocuments(string username, int patientid, int patientProgramid, int DocId, string ConnectionString)
        {

            try
            {
                // string ConnectionString = ConfigurationManager.AppSettings["RPM"].ToString();
                using (SqlConnection con = new SqlConnection(ConnectionString))
                {
                    con.Open();

                    using (SqlCommand command = new SqlCommand("usp_GetPatientDocumentById", con))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@PatientProgramId", patientProgramid);
                        command.Parameters.AddWithValue("@patientid", patientid);
                        command.Parameters.AddWithValue("@DocId", DocId);
                        command.Parameters.AddWithValue("@CreatedBy", username);
                        SqlParameter returnParameter = command.Parameters.Add("RetVal", SqlDbType.Int);
                        returnParameter.Direction = ParameterDirection.ReturnValue;
                        SqlDataReader reader = command.ExecuteReader();

                        PatientDocuments patientData = new PatientDocuments();

                        while (reader.Read())
                        {

                            patientData.DocumentType = Convert.ToString(reader["DocumentType"]);
                            patientData.DocumentName = Convert.ToString(reader["DocumentName"]);
                            patientData.DocumentUNC = Convert.ToString(reader["DocumentUNC"]);
                            patientData.CreatedOn = Convert.ToDateTime(reader["CreatedOn"]);
                            patientData.Id = Convert.ToInt32(reader["Id"]);

                        }

                        if (reader.FieldCount == 0)
                        {
                            con.Close();
                            return null;
                        }
                        con.Close();
                        return patientData;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception occured - class Name :" + this.GetType().Name + ",  Method Name : " + MethodBase.GetCurrentMethod().Name + ", Error Message :" + ex.Message + "");
                return null;
            }
        }

        public void UpdatePatientSmsDetails(string UserName, SaveSmsInfo Info, string ConnectionString)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(ConnectionString))
                {
                    SqlCommand command = new SqlCommand("usp_InsPatientSMSDetails", con);

                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@CreatedBy", UserName);
                    command.Parameters.AddWithValue("@PatientUserName", Info.PatientUserName);
                    command.Parameters.AddWithValue("@fromNo", Info.fromNo);
                    command.Parameters.AddWithValue("@toNo", Info.toNo);
                    command.Parameters.AddWithValue("@Body", Info.Body);
                    command.Parameters.AddWithValue("@SentDate", Info.SentDate);
                    command.Parameters.AddWithValue("@Direction", Info.Direction);
                    command.Parameters.AddWithValue("@Status", Info.Status);
                    SqlParameter returnParameter = command.Parameters.Add("RetVal", SqlDbType.Int);
                    returnParameter.Direction = ParameterDirection.ReturnValue;
                    con.Open();
                    command.ExecuteReader();
                    con.Close();
                }

            }
            catch (Exception ex) { throw ex; }

        }
        public void UpdateIncomingSmsDetails(smshook Info, string ConnectionString)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(ConnectionString))
                {
                    SqlCommand command = new SqlCommand("usp_InsIncomingSMSDetails", con);

                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@fromNo", Info.From);
                    command.Parameters.AddWithValue("@toNo", Info.To);
                    command.Parameters.AddWithValue("@Body", Info.Body);
                    command.Parameters.AddWithValue("@Status", Info.SmsStatus);
                    SqlParameter returnParameter = command.Parameters.Add("RetVal", SqlDbType.Int);
                    returnParameter.Direction = ParameterDirection.ReturnValue;
                    con.Open();
                    command.ExecuteReader();
                    con.Close();
                }

            }
            catch (Exception ex) { throw ex; }
        }

        // for getting patient sms history (incoming and outgoing)
        public List<GetSmsInfo> GetPatientSmsDetails(int PatientId, int PatientProgramId, DateTime StartDate, DateTime EndDate, string CreatedBy, string ConnectionString)
        {
            List<GetSmsInfo> fullsmsdetails = new List<GetSmsInfo>();
            try
            {
                using (SqlConnection con = new SqlConnection(ConnectionString))
                {
                    SqlCommand command = new SqlCommand("usp_GetPatientSMSDetails", con);

                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@CreatedBy", CreatedBy);
                    command.Parameters.AddWithValue("@PatientId", PatientId);
                    command.Parameters.AddWithValue("@PatientProgramId", PatientProgramId);
                    command.Parameters.AddWithValue("@StartDate", StartDate);
                    command.Parameters.AddWithValue("@EndDate", EndDate);

                    SqlParameter returnParameter = command.Parameters.Add("RetVal", SqlDbType.Int);
                    returnParameter.Direction = ParameterDirection.ReturnValue;
                    con.Open();
                    SqlDataReader reader = command.ExecuteReader();


                    while (reader.Read())
                    {
                        GetSmsInfo info = new GetSmsInfo();
                        info.SentDate = Convert.ToDateTime(reader["SentDate"]);
                        info.Body = reader["SmsBody"].ToString();
                        info.Sender = reader["CreatedBy"].ToString();
                        info.Status = reader["Status"].ToString();
                        info.Direction = reader["Direction"].ToString();
                        if (info.Direction== "inbound-api")
                        {
                            info.Sender = "Patient";
                        }
                        else if (info.Direction== "outbound-api")
                        {
                            info.Sender = "Care Team";
                        }

                        fullsmsdetails.Add(info);
                    }
                    if (reader.FieldCount == 0)
                    {
                        return null;
                    }
                    con.Close();
                }

            }
            catch (Exception ex) { throw ex; }
            return fullsmsdetails;
        }

        public bool UpdateChatWebhook(chathook hook, string ConnectionString)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(ConnectionString))
                {
                    con.Open();
                    string insertQuery = "INSERT INTO twiliochatwebhook (author,Body,Attributes,AccountSid,ClientIdentity,EventType,Source,ConversationSid,ParticipantSid) VALUES (@author,@Body,@Attributes,@AccountSid,@ClientIdentity,@EventType,@Source,@ConversationSid,@ParticipantSid)";
                    using (SqlCommand command = new SqlCommand(insertQuery, con))
                    {
                        command.Parameters.AddWithValue("@author", hook.author);
                        command.Parameters.AddWithValue("@Body", hook.Body);
                        command.Parameters.AddWithValue("@Attributes", hook.Attributes);

                        command.Parameters.AddWithValue("@AccountSid", hook.AccountSid);
                        command.Parameters.AddWithValue("@ClientIdentity", hook.ClientIdentity);

                        command.Parameters.AddWithValue("@EventType", hook.EventType);

                        command.Parameters.AddWithValue("@Source", hook.Source);

                        command.Parameters.AddWithValue("@ConversationSid", hook.ConversationSid);

                        command.Parameters.AddWithValue("@ParticipantSid", hook.ParticipantSid);
                        // command.Parameters.AddWithValue("@DateCreated", hook.DateCreated);

                        command.ExecuteNonQuery();
                        con.Close();
                    }

                }

            }
            catch (Exception ex) { throw ex; }
            return true;
        }


    }



}
