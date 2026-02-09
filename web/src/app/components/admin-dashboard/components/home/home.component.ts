import { MasterDataService } from './../../sevices/master-data.service';
import { home } from '../../models/data-models';
import { Component, Input, OnInit, ViewChild } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { RPMService } from '../../sevices/rpm.service';
import { RightSidebarComponent } from '../../shared/right-sidebar/right-sidebar.component';
import { AuthService } from 'src/app/services/auth.service';
import { MatTableDataSource } from '@angular/material/table';
import { from } from 'rxjs';
import { filter } from 'rxjs/operators';
import { MessagingService } from '../../sevices/messaging.service';

export interface TeamData {
  teamname: string;
  alerts: number;
  duetoday: number;
  slabreached: number;
}

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.scss'],
})
export class HomeComponent implements OnInit {
  @ViewChild(RightSidebarComponent) private rightsidebar: RightSidebarComponent;
  pubsuburl: any;
  loading1 = false;
  loading2 = false;
  loading3 = false;
  loading4 = false;
  loading5 = false;
  loading6 = false;
  loading7 = false;
  loading8 = false;
  roles: any;
  rolelist: any;
  PatientOverview: any;
  health_overview_vital: any;
  myprofile: any;
  clinicName: any;
  userAccessRight: any;
  http_getPatientList: any;
  PatientOrContact: any;
  roles_masterdata: any;
  states_cities: any;
  add_patient_masterdata: any;
  http_getActivePatientList: any;
  PatientHealthOverview: any;
  overview: any;
  health_overview: any;
  isHealthoverviewVisible: any;
  isPatientOverviewVisible: any;
  @Input() Critical_Radius: number;
  @Input() Cautious_Radius: number;
  @Input() Normal_Radius: number;
  unassigned_members: any;
  clinicorpatientInfo: any;
  firebase_token: any;

  today: Date;
  LatestSchedule: any;
  UpcomingSchedule: any;
  scheduleData: { description: string; patientname: string; time: string }[];
  alertsArray: any;
  subject: any;
  http_getNonActivePatientList: any;
  message1: any;
  constructor(
    private rpmservice: RPMService,
    private http: HttpClient,
    private route: Router,
    private auth: AuthService,
    private ms: MessagingService,
    private masterdata :MasterDataService
  ) {}

  ngOnInit(): void {
    this.rolelist = sessionStorage.getItem('Roles');
    this.rolelist = JSON.parse(this.rolelist);
    if (this.rolelist[0].Id == 7) {
      this.route.navigate(['/admin/patient-home']);
    }

    this.patients = {
      name: '',
      active: 0,
      onhold: 0,
      prescribed: 0,
      enrolled: 0,
      readytodischarge: 0,
      discharged: 0,
      total: 0,
    };
    this.default_select_health_overview = 'All Vitals';
    this.billing_cpt_code = '99453';

    this.http_home = {
      patient_overview: {
        is_clinic: false,
        list: [
          {
            name: '',
            active: 0,
            onhold: 0,
            prescribed: 0,
            enrolled: 0,
            readytodischarge: 0,
            discharged: 0,
            total: 0,
          },
        ],
      },
      patient_health_overview: {
        all: {
          day: { crtical: 0, cautious: 0, normal: 0 },
          week: { crtical: 0, cautious: 0, normal: 0 },
          month: { crtical: 0, cautious: 0, normal: 0 },
        },
        bp: {
          day: { crtical: 0, cautious: 0, normal: 0 },
          week: { crtical: 0, cautious: 0, normal: 0 },
          month: { crtical: 0, cautious: 0, normal: 0 },
        },
        bg: {
          day: { crtical: 0, cautious: 0, normal: 0 },
          week: { crtical: 0, cautious: 0, normal: 0 },
          month: { crtical: 0, cautious: 0, normal: 0 },
        },
        weight: {
          day: { crtical: 0, cautious: 0, normal: 0 },
          week: { crtical: 0, cautious: 0, normal: 0 },
          month: { crtical: 0, cautious: 0, normal: 0 },
        },
        pox: {
          day: { crtical: 0, cautious: 0, normal: 0 },
          week: { crtical: 0, cautious: 0, normal: 0 },
          month: { crtical: 0, cautious: 0, normal: 0 },
        },
      },
      billing: {
        today: [
          { cptcode: 0, data: { ready: 0, missing: 0, onhold: 0, total: 0 } },
        ],
        last5: [
          { cptcode: 0, data: { ready: 0, missing: 0, onhold: 0, total: 0 } },
        ],
        next5: [
          { cptcode: 0, data: { ready: 0, missing: 0, onhold: 0, total: 0 } },
        ],
      },
      todays_tasks: [
        {
          description: '',
          priority: '',
          alert: false,
          patient: '',
        },
      ],
      team_overview: {
        overview: [{ teamname: '', alerts: 0, duetoday: 0, slabreached: 0 }],
      },
    };

    var that = this;
    this.masterloader = false;
    that.rolelist = sessionStorage.getItem('Roles');
    that.roles = JSON.parse(this.rolelist);

    if (!sessionStorage.getItem('roles_masterdata')) {
      this.getRoleMasterData();
    } else {
      that.loading1 = true;
      that.roles_masterdata = sessionStorage.getItem('roles_masterdata');
      that.roles_masterdata = JSON.parse(that.roles_masterdata);
    }
    if (!sessionStorage.getItem('states_cities')) {
      this.getMasterDataStateAndCities();
    } else {
      that.loading2 = true;
      that.states_cities = sessionStorage.getItem('states_cities');
      that.states_cities = JSON.parse(that.states_cities);
    }
    if (!sessionStorage.getItem('useraccessrights')) {
      this.getUserAccessRights();
    } else {
      that.loading3 = true;
      that.userAccessRight = sessionStorage.getItem('useraccessrights');
      that.userAccessRight = JSON.parse(that.userAccessRight);
      that.isHealthoverviewVisible =
        that.userAccessRight.UserAccessRights.filter(
          (data: { AccessName: string }) =>
            data.AccessName == 'PatientHealthOverviewAccess'
        );
      that.isPatientOverviewVisible =
        that.userAccessRight.UserAccessRights.filter(
          (data: { AccessName: string }) =>
            data.AccessName == 'PatientOverviewAccess'
        );
    }

    this.rolelist = sessionStorage.getItem('Roles');
    this.rolelist = JSON.parse(this.rolelist);
    this.loadAllData();

    // Future 23/06/2023
    this.ms.requestPermission();
    this.ms.requestToken();
    this.message1 = this.ms.receiveMessage();
  }

  /// API CALLS
  isLoading: any;
  async loadAllData() {
    try {
      this.isLoading = true; // Start loader before making API calls

      await Promise.all([
        this.getProgramDetailsMasterData(),
        this.getUnAssignedMembers(),
        // this.getScheduleMasterData(),
        this.getMyProfile(),
        this.getAllClinic(),
        this.getAllPatientList(),
        this.getDashboardPatientStatus(),
        this.fetchdata(7),
        this.GetPriorityAlerts(),
        this.calculateUpcomingSchedule(),
        this.getTodayTask(),
        this.getTeamData(),
        this.getBillingCode(),
      ]);

      this.isLoading = false; // Stop loader after all API calls complete
    } catch (error) {
      console.error('Error loading data:', error);
      this.isLoading = false; // Ensure the loader stops even if an API fails
    }
  }

  //Get Role MasterData
  async getRoleMasterData() {
    try {
      const data = await this.rpmservice.rpm_get(
        '/api/authorization/rolesmasterdata'
      );
      this.roles_masterdata = data;
      sessionStorage.setItem(
        'roles_masterdata',
        JSON.stringify(this.roles_masterdata)
      );
      this.loading1 = true;
    } catch (error) {
      console.error('Error fetching role master data:', error);
    }
  }
  //Get Role MasterData City And State
  getMasterDataStateAndCities() {
    var that = this;
    that.rpmservice
      .rpm_get('/api/authorization/masterdatastatesandcities')
      .then((data) => {
        that.states_cities = data;
        sessionStorage.setItem(
          'states_cities',
          JSON.stringify(that.states_cities)
        );
        that.loading2 = true;
      });
  }
  //Get MasterData UserAccessRights

  getUserAccessRights() {
    var that = this;
    that.rpmservice
      .rpm_get('/api/authorization/useraccessrights?RoleId=' + this.roles[0].Id)
      .then((data) => {
        that.userAccessRight = data;
        sessionStorage.setItem(
          'useraccessrights',
          JSON.stringify(that.userAccessRight)
        );
        that.loading3 = true;
        that.isHealthoverviewVisible =
          that.userAccessRight.UserAccessRights.filter(
            (data: { AccessName: string }) =>
              data.AccessName == 'PatientHealthOverviewAccess'
          );
        that.isPatientOverviewVisible =
          that.userAccessRight.UserAccessRights.filter(
            (data: { AccessName: string }) =>
              data.AccessName == 'PatientOverviewAccess'
          );
      });
  }

  //Get MasterData ProgramDetails
  getProgramDetailsMasterData() {
    var that = this;
    this.rpmservice
      .rpm_get(
        '/api/patient/getprogramdetailsmasterdataaddpatient?RoleId=' +
          this.rolelist[0].Id
      )
      .then((data) => {
        that.add_patient_masterdata = data;
        sessionStorage.setItem(
          'add_patient_masterdata',
          JSON.stringify(that.add_patient_masterdata)
        );
        that.loading2 = true;
      });
  }
  //Get MasterData UnAssignedMembers

  getUnAssignedMembers() {
    var that = this;
    that.rpmservice
      .rpm_get('/api/careteam/getunassignedmembers')
      .then((data) => {
        that.unassigned_members = data;
        sessionStorage.setItem(
          'unassigned_members',
          JSON.stringify(that.unassigned_members)
        );
        that.loading3 = true;
      });
  }
  // Get MasterData Schedule
  // getScheduleMasterData(): void {
  //   const roleId = this.roles[0]?.Id;
  //   if (!roleId) return;

  //   this.masterdata.getScheduleMasterData(roleId)
  //     .then((res) => {
  //       this.PatientOrContact = res.rawData;
  //       this.PatientOrContact.PatientOrContactName = res.patientOrContacts;

  //       sessionStorage.setItem(
  //         'PatientOrContact',
  //         JSON.stringify(res.patientOrContacts)
  //       );

  //       this.loading4 = true;
  //     })
  //     .catch((err) => {
  //       console.error('Failed to fetch schedule master data', err);
  //     });
  // }



  //Get Masterdata MyProfile

  getMyProfile() {
    var that = this;
    that.rpmservice.rpm_get('/api/users/getmyprofiles').then((data) => {
      that.myprofile = data;
      var name = that.myprofile.FirstName + ' ' + that.myprofile.LastName;
      sessionStorage.setItem('user_name', name);
      sessionStorage.setItem('userid', that.myprofile.UserId);
      that.loading6 = true;
    });
  }
  //Get masterData All Clinic
  getAllClinic() {
    var that = this;
    this.rpmservice.rpm_get('/api/clinic/getallclinics').then((data) => {
      this.clinicName = data;
      sessionStorage.setItem(
        'clinic_masterdata',
        JSON.stringify(that.clinicName)
      );
      that.loading7 = true;
    });
  }

  getAllPatientList() {
    var todayDate = new Date().toISOString();
    var offsetValue = this.getTimeZoneOffsetData();
    var that = this;
    this.rpmservice
      .rpm_get(
        `/api/patient/getallpatientslist?ToDate=${todayDate}&UtcOffset=${offsetValue}&Days=7&RoleId=${this.roles[0].Id}`
      )
      .then((data) => {
        that.http_getNonActivePatientList = data;
        sessionStorage.setItem(
          'Patient_List',
          JSON.stringify(that.http_getNonActivePatientList)
        );
        that.loading8 = true;
      });
  }

  getDashboardPatientStatus() {
    var that = this;
    that.rpmservice
      .rpm_get('/api/home/getdashboardpatientstatus?RoleId=' + this.roles[0].Id)
      .then(
        (data) => {
          that.PatientOverview = data;
          // if (this.roles[0].Id != 1) {
          //   if (that.PatientOverview.length > 0) {
          //     this.filter_patient_overview(that.PatientOverview[0].Name);
          //     that.overview = that.PatientOverview[0].Name;
          //     that.clinicorpatientInfo = that.PatientOverview[0].Name;
          //   }
          // } else {
          //   if (that.PatientOverview.length > 0) {
          //     this.filter_patient_overview_provider(that.PatientOverview[0].Id);
          //     that.overview = that.PatientOverview[0].Id;
          //     that.clinicorpatientInfo = that.PatientOverview[0].Name;
          //   }
          // }
          if (this.PatientOverview.length > 0) {
            const { Name, Id } = this.PatientOverview[0]; // Destructure for readability
            this.clinicorpatientInfo = Name;

            if (this.roles[0]?.Id !== 1) {
              this.filter_patient_overview(Name);
              this.overview = Name;
            } else {
              this.filter_patient_overview_provider(Id);
              this.overview = Id;
            }
          }

          that.loading5 = true;
          this.getbillingCycleType();
        },
        (err) => {
          console.log('PatientStatus Data Loading Failed' + err);
        }
      );
  }

  fetchdata(days: any) {
    var that = this;
    var todayDate = new Date().toISOString();
    var offsetValue = this.getTimeZoneOffsetData();

    that.rpmservice
      .rpm_get(
        `/api/home/getdashboardvitalcount?ToDate=${todayDate}&UtcOffset=${offsetValue}&Days=${days}&RoleId=${this.roles[0].Id}`
      )
      .then(
        (data) => {
          that.PatientHealthOverview = data;

          that.health_overview_vital = that.PatientHealthOverview[0].VitalName;
          that.filter_patient_health_overview(that.health_overview_vital);
          that.health_overview_vital = that.PatientHealthOverview[0].VitalName;
        },
        (err) => {
          that.Normal_Radius = 40;
          that.Cautious_Radius = 40;
          that.Critical_Radius = 40;
        }
      );
  }

  filter_patient_overview(event: any) {
    var filtered = this.PatientOverview.filter(
      (overview: { Name: any }) => overview.Name === event
    );
    this.patients = filtered[0].status;
    this.clinicorpatientInfo = filtered[0].Name;
  }

  filter_patient_overview_provider(event: any) {
    var filtered = this.PatientOverview.filter(
      (overview: { Id: any }) => overview.Id === event
    );
    this.clinicorpatientInfo = filtered[0].Name;
    this.patients = filtered[0].status;
  }
  filter_patient_health_overview(event: any) {
    var filtered = this.PatientHealthOverview.filter(
      (overview: { VitalName: any }) => overview.VitalName === event
    );
    this.health_overview = filtered[0].Priorities;
    console.log('Health OverView');
    console.log(this.health_overview)
    if (
      this.health_overview.Normal == this.health_overview.Cautious &&
      this.health_overview.Normal == this.health_overview.Critical
    ) {
      this.Normal_Radius = 40;
      this.Cautious_Radius = 40;
      this.Critical_Radius = 40;
    }
    if (
      this.health_overview.Normal > this.health_overview.Cautious &&
      this.health_overview.Normal > this.health_overview.Critical
    ) {
      this.Normal_Radius = 50;
      if (this.health_overview.Cautious > this.health_overview.Critical) {
        this.Cautious_Radius = 45;
        this.Critical_Radius = 40;
      } else if (
        this.health_overview.Cautious == this.health_overview.Critical
      ) {
        this.Cautious_Radius = 40;
        this.Critical_Radius = 40;
      } else {
        this.Cautious_Radius = 40;
        this.Critical_Radius = 45;
      }
    } else if (
      this.health_overview.Cautious > this.health_overview.Normal &&
      this.health_overview.Cautious > this.health_overview.Critical
    ) {
      this.Cautious_Radius = 50;
      if (this.health_overview.Normal > this.health_overview.Critical) {
        this.Normal_Radius = 45;
        this.Critical_Radius = 40;
      } else if (this.health_overview.Normal == this.health_overview.Critical) {
        this.Normal_Radius = 40;
        this.Critical_Radius = 40;
      } else {
        this.Normal_Radius = 40;
        this.Critical_Radius = 45;
      }
    } else if (
      this.health_overview.Critical > this.health_overview.Normal &&
      this.health_overview.Critical > this.health_overview.Cautious
    ) {
      this.Critical_Radius = 50;
      if (this.health_overview.Normal > this.health_overview.Cautious) {
        this.Normal_Radius = 45;
        this.Cautious_Radius = 40;
      } else if (this.health_overview.Normal == this.health_overview.Cautious) {
        this.Normal_Radius = 40;
        this.Cautious_Radius = 40;
      } else {
        this.Normal_Radius = 40;
        this.Cautious_Radius = 45;
      }
    } else {
      if (
        this.health_overview.Normal > this.health_overview.Cautious &&
        this.health_overview.Normal == this.health_overview.Critical
      ) {
        this.Normal_Radius = 45;
        this.Critical_Radius = 45;
        this.Cautious_Radius = 40;
      } else if (
        this.health_overview.Normal > this.health_overview.Critical &&
        this.health_overview.Normal == this.health_overview.Cautious
      ) {
        this.Normal_Radius = 45;
        this.Critical_Radius = 40;
        this.Cautious_Radius = 45;
      } else {
        this.Normal_Radius = 40;
        this.Critical_Radius = 45;
        this.Cautious_Radius = 45;
      }
    }
  }

  selection_team_hospital = 'all';
  http_home: home;
  isOpen = false;
  home_patientoverview: any = true;
  is_admin: true;
  patients: any;

  // Patient Health OverView

  high_size = 60;
  medium_size = 45;
  low_size = 30;
  patient_data = {
    critical: 0,
    catious: 0,
    normal: 0,
  };
  high: any;

  node_patient_health_overview_dropdown = [
    'All Vitals',
    'Blood Presure',
    'Blood Glucose',
    'Weight',
    'Pulse Oximeter',
  ];
  default_select_health_overview = 'All Vitals';
  heath_overview_frequency = 2;

  node_patient_health_overview_tmp = {
    day: { crtical: 0, cautious: 0, normal: 0 },
    week: { crtical: 0, cautious: 0, normal: 0 },
    month: { crtical: 0, cautious: 0, normal: 0 },
  };

  healthoverview(value: any) {
    this.heath_overview_frequency = value;
    if (value == 3) {
      this.patient_data['critical'] =
        this.node_patient_health_overview_tmp.month.crtical;
      this.patient_data['catious'] =
        this.node_patient_health_overview_tmp.month.cautious;
      this.patient_data['normal'] =
        this.node_patient_health_overview_tmp.month.normal;
      this.fetchdata(30);
    } else if (value == 2) {
      this.patient_data['critical'] =
        this.node_patient_health_overview_tmp.week.crtical;
      this.patient_data['catious'] =
        this.node_patient_health_overview_tmp.week.cautious;
      this.patient_data['normal'] =
        this.node_patient_health_overview_tmp.week.normal;

      this.fetchdata(7);
    } else {
      this.patient_data['critical'] =
        this.node_patient_health_overview_tmp.day.crtical;
      this.patient_data['catious'] =
        this.node_patient_health_overview_tmp.day.cautious;
      this.patient_data['normal'] =
        this.node_patient_health_overview_tmp.day.normal;

      this.fetchdata(0);
    }
  }

  // Billing Information
  billing_cycle_val = 1;
  billing_cpt_code: any;
  billing_array: any;
  patient_bill = {
    total: 0,
    ready: 0,
    missing: 0,
    onhold: 0,
  };

  billing_cycle(cycle: any) {
    if ((cycle = -5)) {
    } else if ((cycle = 5)) {
    } else {
    }
  }
  // Task Information Panel data
  calculateUpcomingSchedule() {
    var that = this;
    this.today = new Date();
    var startdate = this.convertDate(this.today);
    this.today.setDate(this.today.getDate() + 7);
    var enddate = this.convertDate(this.today);
    that.rpmservice
      .rpm_get(
        `/api/schedules/getworklistschedules?StartDate=${startdate}&EndDate=${enddate}`
      )
      .then((data) => {
        that.LatestSchedule = [];
        that.UpcomingSchedule = data;

        var count = 0;
        for (let x of that.UpcomingSchedule) {
          if (count == 1) {
            break;
          }
          for (let y of x.SchedueInfos) {
            if (y.IsCompleted == false) {
              var obj = {
                id: y.Id,
                description: y.Description,
                patientname: y.ContactName,
                time: y.ScheduleTime,
              };

              that.LatestSchedule.push(obj);
            }
          }
          count = count + 1;
        }
        if (that.LatestSchedule.length == 0) {
          that.LatestSchedule = [
            {
              description: 'No Upcoming Schedules',
              patientname: '',
              time: '',
            },
          ];
        }
      });
  }

  teamlist: TeamData[];

  addpatient() {
    this.route.navigate(['/admin/addpatient']);
  }
  billingInfo: any;
  dataSrcBillingInfo: any;
  target_to_bill = false;
  ready_to_bill = true;
  // getPatientBillingInfo(days:any){
  //   var that =this;
  //   var StartDate = null
  //   var EndDate = null
  //   if(days=='last5'){
  //     EndDate= this.convertDate(new Date())
  //     StartDate= new Date()
  //     StartDate = this.convertDate(StartDate.setDate( StartDate.getDate() - 5 ));
  //     this.billing_cycle_val=0;
  //     this.target_to_bill = false;
  //     this.ready_to_bill = true;
  //   }
  //   else if (days=='next5'){
  //     StartDate= this.convertDate(new Date())
  //     EndDate= new Date()
  //     EndDate = this.convertDate(EndDate.setDate( EndDate.getDate() + 5 ));
  //     this.billing_cycle_val=2
  //     this.target_to_bill = true;
  //     this.ready_to_bill = false;
  //   } else if (days=='prvious_month'){

  //     this.target_to_bill = false;
  //     this.ready_to_bill = true;
  //     this.billing_cycle_val=1
  //   } else if (days=='current_month'){

  //     this.billing_cycle_val=2
  //     this.target_to_bill = true;
  //     this.ready_to_bill = false;
  //   }
  //   else{
  //     StartDate= this.convertDate(new Date())
  //     EndDate= new Date()
  //     EndDate = this.convertDate(EndDate.setDate( EndDate.getDate() +1 ));
  //     this.billing_cycle_val=1
  //     this.target_to_bill = false;
  //     this.ready_to_bill = true;
  //   }

  //   that.rpmservice.rpm_get(`/api/patient/getpatientbillinginfo?StartDate=${StartDate}&EndDate=${EndDate}`).then((data)=>{
  //     that.billingInfo = data
  //     that.dataSrcBillingInfo =that.billingInfo;
  //     // that.filterByCPTCode('all');
  //   },
  //   (err)=>{
  //   console.log(err.status)
  //     if(err.status==401){
  //       this.auth.unauthorized();
  //     }
  //   });

  // }
  billingCycle: any;
  getBillingInfoData(data: any) {
    var that = this;
    if (data == 'C1') {
      this.billingCycle = 'C1';
    } else if (data == 'C2') {
      this.billingCycle = 'C2';
    } else if (data == 'Today') {
      this.billingCycle = 'Today';
    } else if (data == 'Next7Days') {
      this.billingCycle = 'Next7Days';
    } else if (data == 'all') {
      this.billingCycle = 'all';
    }

    if (
      this.billingtypeVariable == '30days' &&
      this.billing_cpt_code == '99453'
    ) {
      this.billingCycle = 'all';
    }
    that.rpmservice
      .rpm_get(
        `/api/patient/getpatientbillinginfoCounts?BillingCode=${this.billing_cpt_code}&Cycle=${this.billingCycle}&RoleId=${this.roles[0].Id}`
      )
      .then(
        (data) => {
          that.billingInfo = data;
          that.dataSrcBillingInfo = that.billingInfo;
        },
        (err) => {
          console.log(err.status);

          if (err.status == 401) {
            this.auth.unauthorized();
          }
        }
      );
  }

  loading9: any;
  getBillingInfoInitLoad() {
    var that = this;
    this.loading9 = false;

    that.rpmservice
      .rpm_get(
        `/api/patient/getpatientbillinginfoCounts?BillingCode=99453&Cycle=C1&RoleId=${this.roles[0].Id}`
      )
      .then(
        (data) => {
          that.billingInfo = data;
          that.dataSrcBillingInfo = that.billingInfo;
          this.loading9 = true;
        },
        (err) => {
          this.loading9 = true;

          if (err.status == 401) {
            this.auth.unauthorized();
          }
        }
      );
  }
  monthdisplay: any;
  filterByCPTCode(filterCode: any) {
    if (filterCode == '99457' || filterCode == '99458') {
      this.monthdisplay = true;
    } else {
      this.monthdisplay = false;
    }
    if (filterCode == 'all') {
      this.dataSrcBillingInfo = [];
      var BillingCode = 'all';
      var Total = 0;
      var TargetMet = 0;
      var ReadyToBill = 0;
      var MissingInfo = 0;
      var OnHold = 0;
      for (let x of this.billingInfo) {
        Total = Total + x.Total;
        TargetMet = TargetMet + x.TargetMet;
        ReadyToBill = ReadyToBill + x.ReadyToBill;
        MissingInfo = MissingInfo = x.MissingInfo;
        OnHold = OnHold + x.OnHold;
      }
      var obj = {
        BillingCode: 'all',
        Total: Total,
        TargetMet: TargetMet,
        ReadyToBill: ReadyToBill,
        MissingInfo: MissingInfo,
        OnHold: OnHold,
      };
      this.dataSrcBillingInfo.push(obj);
    } else {
      this.dataSrcBillingInfo = this.billingInfo.filter(
        (data: { BillingCode: any }) => {
          return data.BillingCode == filterCode;
        }
      );
    }
  }
  convertDate(dateval: any) {
    let today = new Date(dateval);
    let dd = today.getDate();
    let dd2;
    if (dd < 10) {
      dd2 = '0' + dd;
    } else {
      dd2 = dd;
    }
    let mm = today.getMonth() + 1;
    let mm2;
    if (mm < 10) {
      mm2 = '0' + mm;
    } else {
      mm2 = mm;
    }
    const yyyy = today.getFullYear();
    dateval = yyyy + '-' + mm2 + '-' + dd2;
    return dateval;
  }
  convertToLocalTime(utcDate: any) {
    utcDate = utcDate + 'Z';

    var localDate = new Date(utcDate);
    var localDate = new Date(utcDate);
    return this.notificationtime(localDate);
  }

  notificationtime(date: any) {
    let today = new Date();
    let notification_date = new Date(date);
    let msgInfo: string = '';

    let diff_min = Math.round(
      (today.getTime() - notification_date.getTime()) / 60000
    );
    if (diff_min == 0) {
      msgInfo = 'Few Seconds Ago';
    } else if (diff_min < 60) {
      msgInfo = diff_min + ' Min';
    } else if (diff_min < 1440) {
      var hr = Math.round(diff_min / 60);
      msgInfo = hr + ' Hrs';
    } else if (diff_min > 1440 && diff_min < 2880) {
      var day = Math.round(diff_min / 1400);
      msgInfo = ' Yesterday';
    } else if (diff_min > 2880 && diff_min < 10080) {
      let dayofweek = notification_date.getDay();
      let dayval = '';
      switch (dayofweek) {
        case 0:
          dayval = 'Sunday';
          break;
        case 1:
          dayval = 'Monday';
          break;
        case 2:
          dayval = 'Tuesday';
          break;
        case 3:
          dayval = 'Wednesday';
          break;
        case 4:
          dayval = 'Thursday';
          break;
        case 5:
          dayval = 'Friday';
          break;
        case 6:
          dayval = 'Saturday';
          break;
      }

      msgInfo = dayval;
    } else if (diff_min > 10080 && diff_min < 20160) {
      msgInfo = '1 Week Ago';
    } else if (diff_min > 43200 && diff_min < 86400) {
      msgInfo = '1 month Ago';
    } else if (diff_min > 86400 && diff_min < 172800) {
      msgInfo = '2 month Ago';
    } else {
      msgInfo = 'Few months Ago';
    }

    return msgInfo;
  }

  acknowledgealert(alert: any) {
    this.rightsidebar.acknowledgealert(alert);
  }
  priorityAlert: any;
  GetPriorityAlerts() {
    var that = this;
    that.rolelist = sessionStorage.getItem('Roles');
    that.roles = JSON.parse(this.rolelist);
    that.rpmservice
      .rpm_get('/api/home/getdashboardalerts?RoleId=' + this.roles[0].Id)
      .then((data) => {
        that.alertsArray = data;
        that.priorityAlert = data;

        if (that.priorityAlert.length == 0) {
          that.priorityAlert = [];
        }
        window.scroll(0, 0);
        this.getAlertData(true);
        if (that.alertsArray.length == 0) {
          that.alertsArray = [
            {
              Id: 0,
              PatientId: 0,
              PatientName: '',
              PatientProgramId: 0,
              VitalAlert: 'No Alerts',
              AssignedToCareTeamUserId: 0,
              Time: '1100-00-0T00:00:00',
            },
          ];
        }
      });
  }

  navigateEditSchedule(id: any) {
    this.rightsidebar.navigateEditSchedule_from_Worklist(id);
  }


  clinicorteam: any;
  vitalSelection: any;
  durationSelection: any;
  navigatePatientPage(patientCat: any, programStatus: any, pageOption: any) {
    // this.clinicorteam = this.overview;

    const roleId = this.roles[0].Id;

    let route = '/admin/patients';
    this.route.navigate([route], {
      queryParams: {
        PageOption: pageOption,
        patientCategory: patientCat,
        programStatus: programStatus,
        clinicorTeam: roleId === 1 || roleId === 6 ? this.clinicorpatientInfo : undefined,
        vitalSelection: this.health_overview_vital,
        durationSelection: this.heath_overview_frequency,
      },
    });
  }
  masterloader: any;
  masterReload() {
    this.masterloader = true;
  }

  billingloader: any;
  billingType: any;
  billingtypeVariable: any;
  getbillingCycleType() {
    var that = this;
    this.billingloader = false;
    that.rpmservice.rpm_get('/api/patient/getbillingtype').then((data) => {
      this.billingType = data;
      this.billingtypeVariable = this.billingType.Provider;
      sessionStorage.setItem('billingType', this.billingType.Provider);
      this.billingloader = true;
      if (this.billingType && this.billingtypeVariable == 'cycle') {
        this.billingCycle = 'C1';
        this.getBillingInfoInitLoad();
      } else if (this.billingType && this.billingtypeVariable == '30days') {
        this.billingCycle = 'all';
        this.getBillingInfoInitTodayLoad();
      }
    });
  }

  getBillingInfoInitTodayLoad() {
    var that = this;
    this.loading9 = false;

    that.rpmservice
      .rpm_get(
        `/api/patient/getpatientbillinginfoCounts?BillingCode=99453&Cycle=all&RoleId=${this.roles[0].Id}`
      )
      .then(
        (data) => {
          that.billingInfo = data;
          that.dataSrcBillingInfo = that.billingInfo;

          this.loading9 = true;
        },
        (err) => {
          this.loading9 = true;

          if (err.status == 401) {
            this.auth.unauthorized();
          }
        }
      );
  }

  columnHeader = ['TeamId', 'TeamName', 'Alert', 'DueToday', 'SlaBreached'];
  teamtableDataSource: any;
  loading10: any;
  todayTask: any;
  getTodayTask() {
    var that = this;
    this.loading10 = false;
    var Today = new Date();
    var startdate = this.convertDate(Today);
    var enddate = this.convertDate(Today);
    startdate = startdate + 'T00:00:00';
    enddate = enddate + 'T23:59:59';
    startdate = this.auth.ConvertToUTCRangeInput(new Date(startdate));
    enddate = this.auth.ConvertToUTCRangeInput(new Date(enddate));
    that.rpmservice
      .rpm_get(
        `/api/home/Getdashboardtodaysalertsandtasks?RoleId=${this.roles[0].Id}&StartDate=${startdate}&EndDate=${enddate}`
      )
      .then((data) => {
        that.todayTask = data;
        that.loading10 = true;
      });
  }

  teamData: any;
  getTeamData() {
    var that = this;
    this.loading10 = false;
    var Today = new Date();

    var startdate = this.convertDate(Today);
    var enddate = this.convertDate(Today);
    startdate = startdate + 'T00:00:00';
    enddate = enddate + 'T23:59:59';
    startdate = this.auth.ConvertToUTCRangeInput(new Date(startdate));
    enddate = this.auth.ConvertToUTCRangeInput(new Date(enddate));
    that.rpmservice
      .rpm_get(
        `/api/home/Getdashboardteamoverview?RoleId=${this.roles[0].Id}&StartDate=${startdate}&EndDate=${enddate}`
      )
      .then((data) => {
        this.teamData = data;

        this.teamtableDataSource = new MatTableDataSource(this.teamData);
        that.loading10 = true;
      });
  }

  getTimeZoneOffsetData() {
    let offsetValue = new Date().getTimezoneOffset();
    var offset;
    offset = offsetValue <= 0 ? '+' : '-';
    var dateOffsetToSent = offset + Math.abs(offsetValue);
    return dateOffsetToSent;
  }
  billingCode: any;
  getBillingCode() {
    var that = this;
    that.rpmservice.rpm_get('/api/patient/getbillingCodes').then((data2) => {
      this.billingCode = data2;
    });
  }

  public alertPriorityCount: number = 3;
  public priorityAlertData: any = [];

  fetchLatestAlert(currentLength: number) {
    let endLimit = currentLength + this.alertPriorityCount + 1;
    return from(this.priorityAlert).pipe(
      filter((alert: any) => {
        return (
          parseInt(alert.Index) > currentLength &&
          parseInt(alert.Index) < endLimit
        );
      })
    );
  }

  getAlertData(fetchData: boolean) {
    if (fetchData) {
      this.fetchLatestAlert(this.priorityAlertData.length - 1).subscribe(
        (response) => {
          this.priorityAlertData = this.priorityAlertData.concat(response);
        },
        (err) => {
          console.log(err);
        }
      );
    }
  }
}
