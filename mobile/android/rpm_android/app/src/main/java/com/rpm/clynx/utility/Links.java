package com.rpm.clynx.utility;

public class Links {

    //public static  final String BASE_URL= "https://rpmwebapp.azurewebsites.net/api";
    // public static  final String BASE_URL= "https://rpmappservicepreprod.azurewebsites.net/api";
    // public static  final String BASE_URL= "https://c-lynxapi.azurewebsites.net/api";
    //public static  final String BASE_URL= "https://rpmdevnew.azurewebsites.net/api";
    public static  final String BASE_URL= "https://cx-dev-server.azurewebsites.net/api";
   // public static  final String BASE_URL= "https://rpm-multivital-api-v2.azurewebsites.net";
    //public static  final String BASE_URL= "https://cx-preprod-server.azurewebsites.net/api";
    //public static  final String BASE_URL= "https://rpm-dev-tespcare.azurewebsites.net/api";
    //public static  final String BASE_URL= "https://cx-dev-client.azurewebsites.net/api";
    //public static  final String BASE_URL= "https://md-preprod-server.azurewebsites.net/api";
    //public static  final String BASE_URL= "https://meditprodapi.azurewebsites.net/api";


    public static  final String LOGIN= "/authorization/Patientlogin";
    public static  final String GENERATE_OTP = "/authorization/forgetpassword?";
    public static  final String RESET_PASSWORD = "/authorization/userresetpasswordverifiy";
    public static  final String OTP_VERIFY = "/authorization/UserloginVerifiy";
    public static  final String LOGOUT= "/authorization/logout";
    public static  final String PATIENT_PROFILE= "/patient/getpatientprofile";
    public static  final String GET_MY_PROFILE= "/users/getmyprofileandprogram";
    public static  final String CHANGE_PASSWORD= "/authorization/updatepassword";
    public static  final String FB_TOK_SAVE= "/notification/insertfirebasetoken?Token=";
    public static  final String VIDEOCALL= "/comm/joinroom?room=";
    public static  final String TODOLIST= "/patients/gettodolist?";

    //public static  final String VITAL_SUMMARY= "/patient/getRecentPatientVitalSummary";
    public static  final String VITAL_SUMMARY= "/patients/getpatienthealthtrends?";

    //public static  final String VITALS= "/patient/getVitalSummaryDetails?";
    //public static  final String VITALHEALTHTRENDS30 = "/patient/getPatientVitalSummary?dayCount=30";
    //public static  final String VITALHEALTHTRENDS7 = "/patient/getPatientVitalSummary?dayCount=7";
    // public static  final String VITALHEALTHTRENDS30 = "/patient/getPatientVitalSummary?";
    public static  final String VITALHEALTHTRENDS30 = "/patients/getpatienthealthtrends?";
    //public static  final String VITALHEALTHTRENDS7 = "/patient/getPatientVitalSummary?";
    public static  final String VITALHEALTHTRENDS7 = "/patients/getpatienthealthtrends?";
    public static  final String NOTIFICATION= "/notification/user";
    public static final String NOTIFICATION_COUNT = "/notification/count/";
    public static final String NOTIFICATION_CONNECTHUB = "/authorization/connecthub";
    public static final String GET_PATIENT = "/patients/getpatient";
    public static final String GET_ACTIVITYSCHEDULES = "/patients/getpatientschedules?";
    public static final String GET_MEDICATIONS = "/patients/getpatientmedication";
    public static final String GET_SYMPTOMS = "/patients/getpatientsymptoms";
    public static final String NOTI_DELETE = "/patients/notification/deletenotifications?";
    public static final String NOTI_DELETE_ALL = "/notification/delete/unread?notificationId=0";
    public static final String GET_VITALREADINGS = "/patients/getpatientvitalreadings?";
    public static  final String ADD_MEDICATION = "/patients/addpatientmedication";
    public static  final String CALL_REJECT = "/comm/notifibyfirebase?toUser=";
    public static  final String MEMBERS_LIST = "/careteam/getpatientcareteammembers";
    public static  final String CHAT_SID = "/comm/getchatsid?ToUser=";
    //public static  final String CHAT_TOK_REG = "/comm/regeneratechattoken";
    public static  final String CHAT_UPDATE = "/comm/updatechatresource";
    public static  final String chat_token_url = "/comm/getchattoken?app=android";
    public static  final String chatreg_token_url = "/comm/regeneratechattoken?app=android";
    public static  final String chatheartbeat_URL = "/comm/chatheartbeat";
    public static  final String NotifyConversation_URL = "/comm/NotifyConversation";
}
