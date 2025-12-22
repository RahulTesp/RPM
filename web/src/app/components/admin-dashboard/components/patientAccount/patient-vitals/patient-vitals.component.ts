import {
  Component,
  OnInit,
  TemplateRef,
  ViewChild,
  ChangeDetectorRef,
  Output,
} from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router, ActivatedRoute } from '@angular/router';
import { AuthService } from 'src/app/services/auth.service';
import { MatDialog, MatDialogConfig } from '@angular/material/dialog';
import { UntypedFormGroup, UntypedFormControl, Validators } from '@angular/forms';
import { MatTableDataSource } from '@angular/material/table';
import { MatPaginator } from '@angular/material/paginator';
import { MatSort } from '@angular/material/sort';
import { DatePipe } from '@angular/common';
import { RPMService } from '../../../sevices/rpm.service';
import dayjs from 'dayjs';
import utc from 'dayjs/plugin/utc';
import timezone from 'dayjs/plugin/timezone';
import { PatientDataDetailsService } from '../../patient-detail-page/Models/service/patient-data-details.service';
dayjs.extend(utc);
dayjs.extend(timezone);
export interface SMS {
  Date: string;
  Time: string;
  message: string;
  Contactname: string;
}

const SMSData: SMS[] = [
  {
    Date: 'Dec 26, 2021',
    Time: '07:28 PM, EST',
    message: 'Lorem ipsum dolor sit amet, consetetur',
    Contactname: 'John Abraham',
  },
  {
    Date: 'Dec 24, 2021',
    Time: '01:32 PM, EST',
    message: 'Lorem ipsum dolor sit amet, consetetur',
    Contactname: 'John Abraham',
  },
  {
    Date: 'Nov 3, 2021',
    Time: '08:52 AM, EST',
    message: 'Lorem ipsum dolor sit amet, consetetur',
    Contactname: 'John Abraham',
  },
  {
    Date: 'Sep 29, 2021',
    Time: '05:32 AM, EST',
    message: 'Lorem ipsum dolor sit amet, consetetur',
    Contactname: 'John Abraham',
  },
];

export interface Chat {
  Id: number;
  Date: string;
  Time: string;
  Contactname: string;
  selected: string;
  action: string;
}

const ChatData: Chat[] = [
  {
    selected: '',
    Id: 1,
    Date: 'Dec 29, 2021',
    Time: '03:16 PM, EST',
    Contactname: 'John Abraham',
    action: '',
  },
  {
    selected: '',
    Id: 2,
    Date: 'Dec 25, 2021',
    Time: '03:46 PM, EST',
    Contactname: 'John Abraham',
    action: '',
  },
];

export interface Goal {
  Id: number;
  Date: string;
  Time: string;
  Contactname: string;
}

const GoalData: Goal[] = [
  {
    Id: 1,
    Date: 'Dec 29, 2021',
    Time: '03:16 PM, EST',
    Contactname: 'John Abraham',
  },
  {
    Id: 2,
    Date: 'Dec 25, 2021',
    Time: '03:46 PM, EST',
    Contactname: 'John Abraham',
  },
];

@Component({
  selector: 'app-patient-vitals',
  templateUrl: './patient-vitals.component.html',
  styleUrls: ['./patient-vitals.component.scss'],
})
export class PatientVitalsComponent implements OnInit {
  @ViewChild('myPreviewPdf', { static: true }) myPreviewPdf: TemplateRef<any>;
  @ViewChild('myPreviewUpdateTemp', { static: true })
  myPreviewUpdateTemp: TemplateRef<any>;

  @ViewChild(MatPaginator)
  paginator: MatPaginator;
  @ViewChild(MatSort) sort = new MatSort();
  healthtrendgrahselect: any;
  documentData: any;
  total_number: any;
  MasterDataQuestionTemp: any;
  QuestionArrayBase: any;
  heath_trends_frequency: number;
  http_rpm_patientList: any;
  additionaNotes: any;
  loading = false;
  loading2 = false;
  myprofile: any;
  variable: any = 1;
  device: any;
  callVariable = false;
  activitycallVariable = false;
  interval: any;
  public Duration: any;
  callTime = 0;
  chatVariable = false;
  chat_panel_visible = false;
  call_panel_visible = false;
  call_connected: any = false;
  phoneNumber: any;
  callHideShow: any = true;
  stopHideShow: any = false;
  phoneno: any;
  phoneNumberPrimary: any;
  phoneNumberSecondary: any;
  progressValue: number = 23;
  totalValue: number = 28;
  percentagevalue: number;

  activitySchedule: any;
  scheduleVariable = false;
  dataSourceTable: any;
  dataSourceTableSymptom: any;
  dataSourceTableUpload: any;
  // dataSourceTableCall: any;
  dataSourceTableSMS: any;
  dataSourceTableChat: any;
  dataSourceTableGoal: any;
  // dataSourceTableReview: any;
  currentMonth: any;

  //health trends
  lineChartLabelMonth1 = [];
  lineChartLabelMonth2 = [];
  patient_bp_value: string;
  patient_bp_duration: string;
  vital_readings: any;
  plan_duration_balance: string;
  patient_pulse_value: string;
  patient_pulse_duration: string;
  patient_bg_value: string;
  patient_bg_duration: string;
  bg_nonfasting_vital_readings: any;
  bpplan: boolean = false;
  bgplan: boolean = false;
  pulseplan: boolean = false;
  program_id: any;

  //document template
  documentVariable = false;
  docAttachVariable = false;
  updateVariable = false;
  Doc: any;
  file: any;
  image: any;
  docType: any;
  docDesc: any;
  //program info
  patient_id: any;

  patientName: string;
  patientID: string;
  patientdob: string;
  patient_address: string;

  currentDate: any;
  currentDateNo: any;
  currentDay: any;
  currentTime: any;
  patientFirstName: any;
  patientLastName: any;
  PatientprogramId: any;
  patientdataUserId: any;
  ProgramHistory: any;
  vitals: any;
  activeProgram = false;
  programStartTime: any;
  programEndTime: any;
  CurrentProgramSelected: any;
  currentprogramId: any;
  ActiveProgram: any;
  programStart: any;
  program_name: any;
  patiantVital: any;
  program_endTime: any;
  consulationDate: any;
  patientconsultationDate: any;
  assignedDate: any;
  assignedMember: any;
  devicedetails: any;
  vitaldetails: any;
  PatientIdOld: any;
  PatientProgramduration: any;
  careplan: any;
  careplanStatus: any;
  patient_dob: any;

  // Medication  Template variable Declaration Start
  medicationVariable = false;
  medication_durationValue = 0;
  http_medication_data: any;
  meditation_table_id: any;
  tempdatasource: any;
  medication_update_variable = false;
  uploadVariable = false;
  SelectedMedicalScheduleInterval: any;
  SelectedMedicalScheduleTime: any;
  endDate: any;
  loading_note: any;
  heath_trends_frequencies: number[] = []; // one per chart
  healthtrendVitalNameArray:any;

  //symptoms
  http_get_symptoms: any;
  // Vital Reading Template
  http_vitalData: any;
  vital_temporaryData: any;
  vitalDataSource: any;
  http_vitalGlucoseData: any;
  vitalDataGlucoseSource: any;
  http_VitalOxygen: any;
  http_VitalWight: any;
  Health_date_from: any;
  Health_date_to: any;
  http_7day_vitalData: any;
  today = new Date();
  ThirtyDaysAgo = new Date(this.today).setDate(this.today.getDate() - 30);
  SevensDaysAgo = new Date(this.today).setDate(this.today.getDate() - 7);
  Tomorrow = new Date(this.today).setDate(this.today.getDate() + 1);
  Today = new Date();

  //  Schedule Template Start
  http_getSchedulemasterdata: any;
  http_scheduleAssigneeList: any;
  scheduleUserList: any;
  frequencyValue: any;
  ScheduleTypeIdArray: any;
  filterOptionSchedule: any;
  SelectedUserData: any;
  SelectUserId: any;
  ScheduleMon = 0;
  ScheduleTue = 0;
  ScheduleWed = 0;
  ScheduleThur = 0;
  ScheduleFri = 0;
  ScheduleSat = 0;
  ScheduleSun = 0;
  ScheduleSubMon = 0;
  ScheduleSubTue = 0;
  ScheduleSubWed = 0;
  ScheduleSubThur = 0;
  ScheduleSubFri = 0;
  ScheduleSubSat = 0;
  ScheduleSubSun = 0;
  morningSchedule = 0;
  eveningSchedule = 0;
  aftNoonSchedule = 0;
  nightSchedule = 0;
  lineChartColor = 'yellow';
  daycount:any;
  public pdfs: any;
  public page = 2;
  public pageLabel!: string;
  
  public chartData: any = [];
  public chartData2: any = [];
  public lineChartData2: Array<any> = [];
 public lineChartData: Array<any> = [
    {
      data: [],
      label: 'Series A',
      lineTension: 0,
    },
  ];
  public lineChartLabels: Array<any> = [
    'January',
    'February',
    'March',
    'April',
    'May',
    'June',
    'July',
  ];
  color = 'primary';
  // Manjusha code change
  public lineChartOptions: any = {
    responsive: true,
    maintainAspectRatio: false,
    layout: {
      padding: {
        top: 10,
        bottom: 30,
        left: 10,
        right: 20,
      }
    },
    pan: {
      enabled: true,
      mode: 'xy',
      rangeMin: {
        x: null,
        y: null,
      },
      rangeMax: {
        x: null,
        y: null,
      },

      onPan: function (e: any) {},
    },
    zoom: {
      enabled: true,
      drag: false,

      mode: 'xy',

      rangeMin: {
        x: null,
        y: null,
      },
      rangeMax: {
        x: null,
        y: null,
      },

      speed: 0.1,
    },
    line: {
      tension: 0.5,
    },
    scales: {
      x: {
        ticks: {
          autoSkip: false,
          maxRotation: 75,
          minRotation: 0
        }
      }
    },
    legend: {
      display: true,
      position: 'bottom',
      labels: {
        boxWidth: 10,
      },
    },
  };
  public lineChartColors: Array<any> = [
    {
      // backgroundColor: 'red',
      fill: false,
      borderColor: 'lightgrey',
      pointBackgroundColor: 'green',
      // pointBorderColor: '#fff',
      pointBorderColor: 'green',
      pointHoverBackgroundColor: '#fff',
      pointHoverBorderColor: 'rgba(148,159,177,0.8)',
    },
    {
      // backgroundColor: 'red',
      fill: false,
      borderColor: 'orange',
      pointBackgroundColor: 'red',
      // pointBorderColor: '#fff',
      pointBorderColor: 'red',
      pointHoverBackgroundColor: '#fff',
      pointHoverBorderColor: 'rgba(148,159,177,0.8)',
    },
    {
      // backgroundColor: 'green',
      fill: false,
      borderColor: 'lightblue',
      pointBackgroundColor: 'blue',
      // pointBorderColor: '#fff',
      pointBorderColor: 'blue',
      pointHoverBackgroundColor: '#fff',
      pointHoverBorderColor: 'rgba(148,159,177,0.8)',
    },
  ];
  public lineChartLegend: boolean = true;
  public lineChartType: any = 'line';


  registerSchedule = new UntypedFormGroup({
    scheduleType: new UntypedFormControl(null, [Validators.required]),
    scheduleDescription: new UntypedFormControl(null, [Validators.required]),
    startDate: new UntypedFormControl(null, [Validators.required]),
    endDate: new UntypedFormControl(null, [Validators.required]),
    startTime: new UntypedFormControl(null, [Validators.required]),
    frequency: new UntypedFormControl(null),
    assigneeName: new UntypedFormControl(null, [Validators.required]),
    scheduleMonday: new UntypedFormControl(null),
    scheduleTuesday: new UntypedFormControl(null),
    scheduleWednesday: new UntypedFormControl(null),
    scheduleThursday: new UntypedFormControl(null),
    scheduleFriday: new UntypedFormControl(null),
    scheduleSaturday: new UntypedFormControl(null),
    scheduleSunday: new UntypedFormControl(null),
    scheduleSubMonday: new UntypedFormControl(null),
    scheduleSubTuesday: new UntypedFormControl(null),
    scheduleSubWednesday: new UntypedFormControl(null),
    scheduleSubThursday: new UntypedFormControl(null),
    scheduleSubFriday: new UntypedFormControl(null),
    scheduleSubSaturday: new UntypedFormControl(null),
    scheduleSubSunday: new UntypedFormControl(null),
    weekSelectionFrequency: new UntypedFormControl(null),
  });
  constructor(
    private rpmservice: RPMService,
    private rpm: RPMService,
    private route: Router,
    private auth: AuthService,
    private dialog: MatDialog,
    private _route: ActivatedRoute,
    public datepipe: DatePipe,
    private changeDetectorRef: ChangeDetectorRef,
    private patientService: PatientDataDetailsService,
    private router: Router,

    
  ) {
    this.getVitalReading();
    this.getBillingInfo();
  }
  billingInfo: any;
  billingratio: any;
  getBillingInfo() {
    var that = this;
    that.rpmservice
      .rpm_get(`/api/patients/getcurrentcyclereading`)
      .then((data) => {
        this.billingInfo = data;
        this.billingratio =
          (this.billingInfo.TotalReadings / this.billingInfo.DaysCompleted) *
          100;
      });
  }

  getVitalReading() {
    var that = this;

    var that = this;

    var startdate = this.convertDate(this.ThirtyDaysAgo);
    var enddate = this.convertDate(this.Today);
    startdate = startdate + 'T00:00:00';
    enddate = enddate + 'T23:59:59';
    this.frmvitalrange.controls['start'].setValue(startdate);
    this.frmvitalrange.controls['end'].setValue(enddate);

    startdate = this.auth.ConvertToUTCRangeInput(new Date(startdate));
    enddate = this.auth.ConvertToUTCRangeInput(new Date(enddate));

    that.rpmservice
      .rpm_get(
        `/api/patients/getpatientvitalreadings?StartDate=${startdate}&EndDate=${enddate}`
      )
      .then((data) => {
        that.http_vitalData = data;
       const vitalEntry = Object.entries(this.http_vitalData).find(([_, value]) => Array.isArray(value) && value.length > 0);
       if (vitalEntry) {
          const vitalName = vitalEntry[0]; // key name
          this.defaultScreen(vitalName);
        }
        that.vital_temporaryData = this.newVitalreadingData(
          this.http_vitalData.BloodPressure,
          'BloodPressureReadings'
        );
        that.http_vitalGlucoseData = this.newVitalreadingData(
          this.http_vitalData.BloodGlucose,
          'BloodGlucoseReadings'
        );
        that.http_VitalOxygen =
          that.http_vitalData &&
          this.newVitalreadingData(
            this.http_vitalData.BloodOxygen,
            'BloodOxygenReadings'
          );
        that.http_VitalWight =
          that.http_vitalData &&
          this.newVitalreadingData(
            this.http_vitalData.Weight,
            'WeightReadings'
          );
        if (
          that.http_vitalGlucoseData &&
          that.http_vitalGlucoseData.length > 0
        ) {
          this.ChangeVitalScreen('BloodGlucose');
        } else if (
          that.vital_temporaryData &&
          that.vital_temporaryData.length > 0
        ) {
          this.ChangeVitalScreen('BloodPressure');
        } else if (that.http_VitalOxygen && that.http_VitalOxygen.length > 0) {
          this.ChangeVitalScreen('Oxygen');
        } else if (that.http_VitalWight && that.http_VitalWight.length > 0) {
          this.ChangeVitalScreen('Weight');
        }
      });
  }
  newVitalreadingData(data: any, vitalInfo: any) {
    var that = this;
    var vitalDataArray: any[] = [];
    var dataSample = data.map(function (item: any) {
      var readingDate = that.convertToLocalTime(item.ReadingTime);

      if (vitalInfo == 'BloodPressureReadings') {
        vitalDataArray.push({
          Diastolic: item.Diastolic,
          DiastolicStatus: item.DiastolicStatus,
          ReadingTime: readingDate,
          Remarks: item.Remarks,
          Status: item.Status,
          Systolic: item.Systolic,
          SystolicStatus: item.SystolicStatus,
          pulse: item.pulse,
          pulseStatus: item.pulseStatus,
          DateData: readingDate,
        });
      } else if (vitalInfo == 'BloodGlucoseReadings') {
        vitalDataArray.push({
          BGmgdl: item.BGmgdl,
          ReadingTime: readingDate,
          Status: item.Status,
          Schedule: item.Schedule,
          Remarks: item.Remarks,
          DateData: readingDate,
        });
      } else if (vitalInfo == 'BloodOxygenReadings') {
        vitalDataArray.push({
          Oxygen: item.Oxygen,
          ReadingTime: readingDate,
          Pulse: item.Pulse,
          Remarks: item.Remarks,
          DateData: readingDate,
          PulseStatus: item.PulseStatus,
          OxygenStatus: item.OxygenStatus,
          Status: item.Status,
        });
      } else if (vitalInfo == 'WeightReadings') {
        vitalDataArray.push({
          BWlbs: item.BWlbs,
          ReadingTime: readingDate,
          Remarks: item.Remarks,
          DateData: readingDate,
          Status: item.Status,
        });
      }
    });

    const groups = vitalDataArray.reduce((groups: any, vitals: any) => {
      const date = vitals.DateData.split(' ')[0];
      if (!groups[date]) {
        groups[date] = [];
      }
      groups[date].push(vitals);
      return groups;
    }, {});

    var groupArrays;
    if (vitalInfo == 'BloodPressureReadings') {
      groupArrays = Object.keys(groups).map((ReadingDate: any) => {
        return {
          ReadingDate,
          BloodPressureReadings: groups[ReadingDate],
        };
      });
    } else if (vitalInfo == 'BloodGlucoseReadings') {
      groupArrays = Object.keys(groups).map((ReadingDate: any) => {
        return {
          ReadingDate,
          BloodGlucoseReadings: groups[ReadingDate],
        };
      });
    } else if (vitalInfo == 'BloodOxygenReadings') {
      groupArrays = Object.keys(groups).map((ReadingDate: any) => {
        return {
          ReadingDate,
          BloodOxygenReadings: groups[ReadingDate],
        };
      });
    } else if (vitalInfo == 'WeightReadings') {
      groupArrays = Object.keys(groups).map((ReadingDate: any) => {
        return {
          ReadingDate,
          WeightReadings: groups[ReadingDate],
        };
      });
    }

    return groupArrays;
  }

  frmvitalrange = new UntypedFormGroup({
    start: new UntypedFormControl(),
    end: new UntypedFormControl(),
  });

  menu = [
    {
      menu_id: 1,
      menu_title: 'Program Info',
    },
    {
      menu_id: 2,
      menu_title: 'Clinical Info',
    },
    {
      menu_id: 3,
      menu_title: 'Activity',
    },
  ];

  weekFrequency = [
    {
      id: 1,
      weekPeriod: 'First Week',
    },
    {
      id: 2,
      weekPeriod: 'Second Week',
    },
    {
      id: 3,
      weekPeriod: 'Third Week',
    },
    {
      id: 4,
      weekPeriod: 'Fourth Week',
    },
    {
      id: 5,
      weekPeriod: 'Last Week',
    },
  ];
  CurrentMenu: any = 1;

  ChangeScreen(button: any) {
    this.CurrentMenu = button;

    switch (button) {
      case 1:
        this.variable = 1;
        this.getPatientInfo();
        // sessionStorage.setItem('ActiveMainMenu', 'ProgramInfo');
        break;

      case 2:
        this.variable = 2;
        // sessionStorage.setItem('ActiveMainMenu', 'ClinicalInfo');
        this.clinicInfoMenu = this.getDisplyClinicalInfoMenu();

        break;

      case 3:
        this.variable = 3;
        this.getPatientInfo();
        this.getCallInfoData();
        this.activityMenuVariable = 2;
        this.activityInfoMenuSelect(this.activityMenuVariable);
        // sessionStorage.setItem('ActiveMainMenu', 'ProgramInfo');

        break;
    }
  }

  // Data Table Source Change Function
  dataSourceChange(mainmenu: any, submenu: any) {
    if (mainmenu == 3) {
      if (submenu == 5) {
      } else if (submenu == 4) {
        return;
      } else if (submenu == 3) {
        return;
      } else if (submenu == 2) {
        //this.getCallInfoData();
        this.getTableDataSource(this.http_CallNotesData);
      } else {
      }
    } else if (mainmenu == 2) {
      if (submenu == 5) {
        this.getTableDataSource(this.dataSourceTableUpload);
      } else if (submenu == 4) {
        this.getTableDataSource(this.http_get_symptoms);
      } else if (submenu == 3) {
        this.getTableDataSource(this.http_medication_data);
      } else if (submenu == 1) {
        this.getTableDataSource(this.http_vitalData);
      }
    } else {
    }
  }

  getTableDataSource(data: any) {
    this.dataSourceTable = new MatTableDataSource(data);
    this.changeDetectorRef.detectChanges();
    this.dataSourceTable.paginator = this.paginator;
    this.dataSourceTable.sort = this.sort;
    this.total_number = this.dataSourceTable.filteredData.length;
  }
  timeConvert(n: any) {
    var num = n;

    var hours = Math.floor(num / 3600);
    var remain_secs = num % 3600;
    var minutes = Math.floor(remain_secs / 60);
    var secs = remain_secs % 60;
    var min;
    var sec;
    var hr;
    if (hours < 10) {
      hr = '0' + hours;
    } else {
      hr = hours;
    }
    if (minutes < 10) {
      min = '0' + minutes;
    } else {
      min = minutes;
    }
    if (secs < 10) {
      sec = '0' + secs;
    } else {
      sec = secs;
    }
    return hr + ':' + min + ':' + sec;
  }
  programInfoMenu = [
    {
      menu_id: 1,
      menu_title: 'Program Details',
      target: 'programInfodata.html',
    },
    {
      menu_id: 2,
      menu_title: 'Device Details',
      target: 'deviceInfodata.html',
    },
    {
      menu_id: 3,
      menu_title: 'Vital Schedules',
    },
    {
      menu_id: 4,
      menu_title: 'Insurance Details',
    },
    {
      menu_id: 5,
      menu_title: 'Documents',
    },
    {
      menu_id: 6,
      menu_title: 'Program Timeline',
    },
  ];
  //chatcontainer

  chatMessage = [
    {
      text: 'Hi',
      id: '1',
      chat: 'receiver',
    },
    { text: 'Hello', id: '2', chat: 'sender' },
    {
      text: 'What Knid of problem facing',
      id: '2',
      chat: 'sender',
    },
    {
      text: 'Nothing',
      id: '1',
      chat: 'receiver',
    },
  ];

  close_chat_window() {
    this.chatVariable = false;
  }

  // Clinical Activity Table Vital Monitoring

  // Clinical Menu Item
  clinicInfoMenu: any;
 
  http_ActivityScheduleData: any;

  documentTableColumns = [
    'DocumentType',
    'Description',
    'CreatedOn',
    'DocumentUNC',
  ];

  CancelDocForm() {
    this.docType = null;
    this.docDesc = null;
    this.fileName = null;
    this.documentVariable = false;
  }

  openPdf(pdfSrc: any) {
    this.pdfs = pdfSrc;
    this.dialog.open(this.myPreviewPdf);
    // this.updateVariable = !this.updateVariable;
  }
  http_getDocument: any;
  documentviewSrc: any;
  openDocUpdatePanel(data: any) {
    this.updateVariable = true;
    var that = this;
    that.rpmservice
      .rpm_get(`/api/patients/getmydocumentbyid?DocumentId=${data.Id}`)
      .then(
        (data) => {
          that.http_getDocument = data;
          this.docType = that.http_getDocument.DocumentType;
          this.fileName = that.http_getDocument.DocumentName;
          this.documentviewSrc = that.http_getDocument.DocumentUNC;
        },
        (err) => {
          console.log(err);
        }
      );
  }

  // Activity-Review Template Start
  fileName: any;
  documentType: any;
  openFileData(e: any) {
    this.Doc = e.target.files[0];
    this.documentType = this.Doc.type;
    this.fileName = this.Doc.name;
  }
  CallNotesColumnHeader = [
    'Id',
    'AllCalls',
    'Date',
    'Time',
    'CompletedBy',
    'Duration',
    'CallType',
  ];
  ChatColumnHeader = [
    'Id',
    'selection',
    'Date',
    'Time',
    'Contactname',
    'action',
  ];
  SmsColumnHeader = ['Date', 'Time', 'Message', 'Contactname'];
  displayedGoalColumns = ['Id', 'Date', 'Time', 'Contactname'];
  NoteData: any;
  NoteTypeId: any;
  noteTypeIdArray: any;
  NoteTime: any;
  NoteSource: any;
  callNoteData: any;
  establishedValue = undefined;
  caregiver = false;
  noteId: any;


  getCallDataupdatePreview() {
    this.dialog.open(this.myPreviewUpdateTemp);
  }
  OnpreviewUpdateCancel() {
    this.activitycallVariable = false;
  }
  convertSecToTime(seconds: any) {
    var hs = Math.trunc(seconds / 3600);
    var remh = Math.trunc(seconds % 3600);
    var min = Math.trunc(remh / 60);
    var sec = Math.trunc(remh % 60);
    var rsec = '';
    var rmin = '';
    var rhs = '';
    if (hs < 10) {
      rhs = '0' + hs.toString();
    } else {
      rhs = hs.toString();
    }
    if (sec < 10) {
      rsec = '0' + sec.toString();
    } else {
      rsec = sec.toString();
    }
    if (min < 10) {
      rmin = '0' + min.toString();
    } else {
      rmin = min.toString();
    }
    var result = rhs + ':' + rmin + ':' + rsec;
    return result;
  }
  // Activity-Review Template Ends
  addDocumentPage() {
    this.documentVariable = !this.documentVariable;
  }
  onClickDocumentAdd() {
    this.docAttachVariable = !this.docAttachVariable;
    this.pdfs = 'assets/sample.pdf';
    this.dialog.open(this.myPreviewPdf);
  }

  scroll(el: HTMLElement) {
    el.scrollIntoView();
    el.scrollIntoView({ behavior: 'smooth' });
  }

  currentPhysician: any;
  calltimer: any;
  callTimerEnabled: boolean;

  makeCallReady() {
    this.callVariable = !this.callVariable;

    this.startTimer();
    //  this.callCustomer();
  }

  makeCallConnected() {
    this.call_connected = !this.call_connected;
    if (this.call_connected) {
      this.callCustomer();
    } else {
      // this.callTimerEnabled=false;
      this.Stop();

    }
  }

  callDisConnected() {
    this.call_connected = false;
    this.Stop();
  }
  callCustomer() {}
  Stop() {
    this.device.disconnectAll();
    this.stopHideShow = false;
    this.callHideShow = true;
  }
  incr: any = 1;
  progress: any = 0;
  onclickcancel() {
    this.callVariable = false;
  }

  onClicknoteTemplateCancel() {
    this.activitycallVariable = false;
  }

  onClickChat() {
    // this.chatVariable = !this.chatVariable;
    // this.chat_panel_visible = true;
  }
  onclickchatarrow(row: any) {
    this.chat_panel_visible = true;
  }
  updateValue() {
    this.percentagevalue = (this.progressValue * 100) / this.totalValue;
  }
  onClickMinimize() {
    this.documentVariable = false;
  }
  onClickAdd() {
    this.docAttachVariable = !this.docAttachVariable;
  }
  toogle: boolean = false;
  minimize = {
    height: '3rem',
    position: 'fixed',
    bottom: '0px',
    overflow: 'hidden',
  };
  maximize = { height: '90vh' };
  minimumstyle = {};
  maximimumstyle = {};
  minimizeValue = false;
  minimizeScreen() {
    this.toogle = !this.toogle;
    this.minimizeValue = true;
  }
  maximizeScreen() {
    this.toogle = !this.toogle;
    this.minimizeValue = false;
  }

  onClickdocCancel() {
    this.documentVariable = false;
    this.docType = null;
    this.docDesc = null;
  }

  onClickupdatedocCancel() {
    this.updateVariable = false;
  }

  onClickmedicationCancel() {
    this.medicationVariable = false;
    this.medication_update_variable = false;
  }
  onClickuploadCancel() {
    this.uploadVariable = false;
  }
  onclickactivityschcancel() {
    this.scheduleVariable = false;
  }
  activePage: any;
  ngOnInit(): void {
    this.heath_trends_frequency = 7;
    this.healthtrendgrahselect = 'current_week';
    this.activePage = sessionStorage.getItem('ActiveMainMenu');
    if (this.activePage == 'ProgramInfo') {
      this.ChangeScreen(1);
    } else if (this.activePage == 'ClinicalInfo') {
      this.ChangeScreen(2);
    }
    this.getScheduleData();
    var d = new Date();
    d.setDate(d.getDate() - 7);

    setInterval(() => this.updateValue(), 150);
    this.currentDate = new Date();
    this.currentDateNo = this.currentDate.getDate() - 6;
    this.currentMonth = this.currentDate.getMonth();
    this.currentDay = this.currentDate.getDay();
    this.currentTime =
      this.currentDate.getHours() + ':' + this.currentDate.getMinutes();

    this.ProgramHistory = [];
    this.CurrentProgramSelected = undefined;
    this._route.queryParams.subscribe((params) => {
      this.getPatientInfo();
      this.getMedication();
      this.getSymptom();
      this.getUploads();
      this.getCallInfoData();
      this.getHealthTrends(this.heath_trends_frequency);
    });
  }
  onProgramHstoryChange(programId: any) {
    this.CurrentProgramSelected = programId;
  }

  minValue: number = 30;
  maxValue: number = 75;
  middle: number = 20;
  http_healthtrends: any;

  bg_vital_readings: any;

  WeekStartDate: any;
  WeekEndDate: any;
  allLineChartData: any=[];

  getHealthTrends(daycount:number) {
  try {
    var date1;
    var date2;
    var that = this;
    date1 = new Date();
    var utcdate1 = this.convertDate(date1) + 'T23:59:59';
    date1 = this.convertDate(date1.setDate(date1.getDate()));
    date2 = new Date();
    date2 = this.convertDate(
      date2.setDate(date2.getDate() - (daycount - 1))
    );
    var utcdate2 = this.convertDate(date2) + 'T00:00:00';

    utcdate1 = this.auth.ConvertToUTCRangeInput(new Date(utcdate1));
    utcdate2 = this.auth.ConvertToUTCRangeInput(new Date(utcdate2));

    this.loading = true;
    this.rpm
      .rpm_get(
        `/api/patients/getpatienthealthtrends?StartDate=${utcdate2}&EndDate=${utcdate1}`
      )
      .then(
        (data) => {
          try {
            this.http_healthtrends = data;
            console.log('Health Trend');
            console.log(this.http_healthtrends);
            this.healthtrendVitalNameArray = this.extractVitalNames(this.http_healthtrends);
            // this.heath_trends_frequencies = new Array(data.length).fill(30);
           //  Clear previous data before pushing new ones
           this.allLineChartData = []; // clear previous data

          // Only process trends with actual data
        this.http_healthtrends.forEach((trendData: any) => {
        if (!trendData.Values || trendData.Values.length === 0) {
          if (this.daycount == 7) {
            this.setEmptyGraphHealthInfo();
          } else {
            this.setEmpty30DaysGraphHealthInfo();
          }
          return;
        }

        const isVital = trendData.Values?.[0]?.label === 'Vital';
        const originalLabels = this.patientService.convertDateforHealthTrends(
          trendData.Time,
          isVital
        );

        const originalDataSets = trendData.Values.map((item: any) => ({
          data: [...item.data],
          label: item.label,
          fill: false,
          lineTension: 0.5
        }));

        const {
          filteredData,
          filteredLabels
        } = this.filterChartDataAndLabelsTogether(
          originalDataSets,
          originalLabels,
          trendData.VitalName
        );

        const lineChartData = originalDataSets.map((ds: any, idx: any) => ({
          ...ds,
          data: filteredData[idx]
        }));

        this.allLineChartData.push({
          lineChartLabels: filteredLabels,
          lineChartData
        });
      });

          } catch (innerErr) {
            console.error('Processing error:', innerErr);
            this.setEmptyGraphHealthInfo();
            this.loading = false;
          }
        },
        (err) => {
          console.error('API error:', err);
          this.setEmptyGraphHealthInfo();
          this.loading = false;
        }
      );
  } catch (err) {
    console.error('getHealthTrends error:', err);
    this.setEmptyGraphHealthInfo();
    this.loading = false;
  }
}
extractVitalNames(vitalData: any[]): string[] {
    if (!Array.isArray(vitalData)) return [];
    return vitalData.map((item) => item.VitalName).filter((name) => name);
  }
filterChartDataAndLabelsTogether(
    datasets: { data: any[]; label: string }[],
    labels: string[],
    vitalName: string
  ): { filteredData: any[][]; filteredLabels: string[] } {
    const filteredData = datasets.map(ds => [...ds.data]);
    const filteredLabels = [...labels];

    for (let i = filteredLabels.length - 1; i >= 0; i--) {
      const isNullAcrossAll = filteredData.every(ds => ds[i] === null);

      if (
        isNullAcrossAll &&
        i > 0 &&
        i < filteredLabels.length - 1 &&
        vitalName !== 'Blood Glucose'
      ) {
        const [prevDate] = filteredLabels[i - 1]?.split(' - ') || [];
        const [currDate] = filteredLabels[i]?.split(' - ') || [];
        const [nextDate] = filteredLabels[i + 1]?.split(' - ') || [];

        if (currDate === prevDate || currDate === nextDate) {
          filteredLabels.splice(i, 1);
          filteredData.forEach(ds => ds.splice(i, 1));
        }
      }
    }

    return {
      filteredData,
      filteredLabels
    };
  }
   setEmptyGraphHealthInfo() {
    var date_val = new Date();
    var x = [0, 1, 2, 3, 4, 5, 6];
    var DefaultDates = [];
    var date_val_set = '';
    for (var item1 of x) {
      date_val_set = this.convertDate(date_val.setDate(date_val.getDate()));
      DefaultDates.push(date_val_set);
      date_val_set = this.convertDate(date_val.setDate(date_val.getDate() - 1));
    }

    const fallbackData = {
      VitalName: 'No Data',
      VitalId: 1,
      Time: DefaultDates.reverse(),
      Values: [
        { data: [null, null, null, null, null, null, null], label: 'No data available' },
      ],
    };
    const lineChartLabels = this.lineChartLabels;
    var temp = [];
    for (var item of fallbackData.Values) {
      var obj = {
        data: item.data,
        label: item.label,
        fill: false,
        lineTension: 0.5,
      };
      temp.push(obj);
    }
    this.lineChartData = temp;
    const lineChartData = this.lineChartData;
    // Push processed data into array
    this.allLineChartData.push({
      lineChartLabels,
      lineChartData,
    });
  }

  setEmpty30DaysGraphHealthInfo() {
    var date_val = new Date();
    var x = [
      0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20,
      21, 22, 23, 24, 25, 26, 27, 28, 29, 30,
    ];
    var DefaultDates = [];
    var date_val_set = '';
    for (var item1 of x) {
      date_val_set = this.convertDate(date_val.setDate(date_val.getDate()));
      DefaultDates.push(date_val_set);
      date_val_set = this.convertDate(date_val.setDate(date_val.getDate() - 1));
    }

    const fallbackData = {
      VitalName: 'No Data',
      VitalId: 1,
      Time: DefaultDates.reverse(),
      Values: [
        { data: [null, null, null, null, null, null, null], label: 'No data available' },
      ],
    };
    this.lineChartLabels = this.patientService.convertDateforHealthTrends(
      fallbackData.Time,
      fallbackData
    );
    const lineChartLabels = this.lineChartLabels;
    var temp = [];
    for (var item of fallbackData.Values) {
      var obj = {
        data: item.data,
        label: item.label,
        fill: false,
        lineTension: 0.5,
      };
      temp.push(obj);
    }
    this.lineChartData = temp;
    const lineChartData = this.lineChartData;
    this.allLineChartData.push({
      lineChartLabels,
      lineChartData,
    });
  }
   healthtrendsHealthInfo(selected_val: any,index:any) {
    this.heath_trends_frequencies[index] = selected_val;
    this.getSingleHealthTrendData(index, selected_val);
  }
 async getSingleHealthTrendData(index: number, daycount: number) {

 try {
    var date1;
    var date2;
    var that = this;
    date1 = new Date();
    var utcdate1 = this.convertDate(date1) + 'T23:59:59';
    date1 = this.convertDate(date1.setDate(date1.getDate()));
    date2 = new Date();
    date2 = this.convertDate(
      date2.setDate(date2.getDate() - (daycount - 1))
    );
    var utcdate2 = this.convertDate(date2) + 'T00:00:00';

    utcdate1 = this.auth.ConvertToUTCRangeInput(new Date(utcdate1));
    utcdate2 = this.auth.ConvertToUTCRangeInput(new Date(utcdate2));

    this.loading = true;
    this.rpm
      .rpm_get(
        `/api/patients/getpatienthealthtrends?StartDate=${utcdate2}&EndDate=${utcdate1}`
      )
      .then(
        (data) => {
          try {
            this.http_healthtrends = data;
             const trendData = this.http_healthtrends[index]; // get the vital at that index

      if (!trendData || !trendData.Values || trendData.Values.length === 0) {
        const emptyGraph = (daycount === 7)
          ? this.createEmptyGraph(7)
          : this.createEmptyGraph(30);

        this.allLineChartData[index] = emptyGraph;
        return;
      }

      const isVital = trendData.Values?.[0]?.label === 'Vital';
      const lineChartLabels = this.patientService.convertDateforHealthTrends(
        trendData.Time,
        isVital
      );

      const lineChartData = trendData.Values.map(
        (item: { data: any[]; label: any }) => ({
          data: this.cleanData(item.data, trendData.VitalName),
          label: item.label,
          fill: false,
          lineTension: 0.5,
        })
      );

      this.allLineChartData[index] = {
        lineChartLabels,
        lineChartData,
      };
          } catch (innerErr) {
            console.error('Processing error:', innerErr);
            this.setEmptyGraphHealthInfo();
            this.loading = false;
          }
        },
        (err) => {
          console.error('API error:', err);
          this.setEmptyGraphHealthInfo();
          this.loading = false;
        }
      );
     } catch (err) {
    console.error('getHealthTrends error:', err);
    this.setEmptyGraphHealthInfo();
    this.loading = false;
  }
  }
createEmptyGraph(daycount: number) {
    const days = Array.from({ length: daycount }, (_, i) => {
      const d = new Date();
      d.setDate(d.getDate() - (daycount - 1 - i));
      return this.convertDate(d);
    });
    const labels = this.patientService.convertDateforHealthTrends(days, false);
    return {
      lineChartLabels: labels,
      lineChartData: [
        {
          data: Array(daycount).fill(null),
          label: 'No data available',
          fill: false,
          lineTension: 0.5,
        },
      ],
    };
  }
  private cleanData(dataArray: any[], vitalName: string): any[] {
    if (!Array.isArray(dataArray)) return [];

    return dataArray.filter((value, i) => {
      if (value !== null) return true;

      if (i > 0 && i < dataArray.length - 1 && vitalName !== 'Blood Glucose') {
        const [prevDate] = this.lineChartLabels[i - 1]?.split(' - ') || [];
        const [currDate] = this.lineChartLabels[i]?.split(' - ') || [];
        const [nextDate] = this.lineChartLabels[i + 1]?.split(' - ') || [];

        if (currDate === prevDate || currDate === nextDate) {
          this.lineChartLabels.splice(i, 1);
          return false;
        }
      }
      return true;
    });
  }
  getHealthTrendWeek(daycount: number, value: any) {
    var start_day;

    var end_day;

    var utc_startday;

    var utc_endday;

    var that = this;

    if (value === 'previous') {
      start_day = new Date(this.WeekStartDate);

      this.WeekStartDate = start_day;

      start_day = start_day.setDate(start_day.getDate() - 7);

      start_day = new Date(start_day);

      start_day = start_day.toISOString();

      end_day = new Date(this.WeekEndDate);

      this.WeekEndDate = end_day;

      end_day = end_day.setDate(end_day.getDate() - 7);

      end_day = new Date(end_day);

      end_day = end_day.toISOString();

      // end_day = new Date(this.WeekEndDate);

      // this.WeekEndDate = end_day.toISOString();

      // end_day = this.convertDate(this.WeekEndDate.setDate(this.WeekEndDate.getDate() - 7));

      utc_startday = start_day; // + 'T00:00:00';

      utc_endday = end_day; // + 'T23:59:59';

      //utc_startday = this.auth.ConvertToUTCRangeInput(new Date(utc_startday));

      //utc_endday = this.auth.ConvertToUTCRangeInput(new Date(utc_endday));
    } else if (value === 'next') {
      // start_day = new Date(this.WeekStartDate);

      // this.WeekStartDate = start_day;

      // start_day = this.convertDate(start_day.setDate(start_day.getDate() + 7));

      // end_day = new Date(this.WeekEndDate);

      // this.WeekEndDate = end_day;

      // end_day = this.convertDate(end_day.setDate(end_day.getDate() + 7));

      start_day = new Date(this.WeekStartDate);

      this.WeekStartDate = start_day;

      start_day = start_day.setDate(start_day.getDate() + 7);

      start_day = new Date(start_day);

      start_day = start_day.toISOString();

      end_day = new Date(this.WeekEndDate);

      this.WeekEndDate = end_day;

      end_day = end_day.setDate(end_day.getDate() + 7);

      end_day = new Date(end_day);

      end_day = end_day.toISOString();

      utc_startday = start_day; //+ 'T00:00:00';

      utc_endday = end_day; //+ 'T23:59:59';

      //utc_startday = this.auth.ConvertToUTCRangeInput(new Date(utc_startday));

      //utc_endday = this.auth.ConvertToUTCRangeInput(new Date(utc_endday));
    }

    this.loading = true;

    this.rpm

      .rpm_get(
        `/api/patients/getpatienthealthtrends?StartDate=${utc_startday}&EndDate=${utc_endday}`
      )

      .then(
        (data) => {
          this.http_healthtrends = data;

          if (that.http_healthtrends.Values.length > 0) {
            that.lineChartLabels = that.convertDateforHealthTrends(
              that.http_healthtrends.Time
            );
          }

          if (
            this.http_healthtrends &&
            that.http_healthtrends.Values.length > 0
          ) {
            var temp = [];

            var j = 0;

            for (var item of that.http_healthtrends.Values) {
              var i = 0;

              for (var x of item.data) {
                if (j == 0) {
                  try {
                    if (
                      x == null &&
                      i > 0 &&
                      i < item.data.length &&
                      that.http_healthtrends.VitalName != 'Blood Glucose'
                    ) {
                      var linedt1 = this.lineChartLabels[i].split(' - ');

                      var linedt0 = this.lineChartLabels[i - 1].split(' - ');

                      var linedt2 = this.lineChartLabels[i + 1].split(' - ');

                      if (
                        linedt1[0] == linedt0[0] ||
                        linedt1[0] == linedt2[0]
                      ) {
                        this.lineChartLabels.splice(i, 1);

                        var k = 0;

                        for (var tmpitem of that.http_healthtrends.Values) {
                          that.http_healthtrends.Values[k].data.splice(i, 1);

                          k = k + 1;
                        }
                      }
                    }

                    i = i + 1;
                  } catch (ex) {
                    console.log('exception' + ex);
                  }
                }
              }

              j = j + 1;

              var obj = {
                data: item.data,

                label: item.label,

                fill: false,

                lineTension: 0.5,
              };

              temp.push(obj);
            }

            that.lineChartData = temp;

            this.loading = false;
          } else {
            this.setEmptyGraph();

            this.loading = false;
          }
        },

        (err) => {
          this.setEmptyGraph();
        }
      );

    //this.getlinechartLabel(this.lineChartData);
    // this.OrderHealthTrendValue(this.lineChartData);
  }
  Current_month: any;
  Current_month_temp: any;
  currentYear: any;
  firstDayOfMonth: any;
  lastDayOfMonth: any;

  getHealthMonth(value: any) {
    this.healthtrendgrahselect = 'current_month';

    var current_date;

    if (value == 'current_month') {
      current_date = new Date();
      this.currentYear = current_date.getFullYear();
      this.Current_month = current_date.getMonth();
      this.Current_month_temp = current_date.getMonth();
    }
    if (value == 'previous') {
      this.Current_month_temp = this.Current_month_temp - 1;
      if (this.Current_month_temp < 0) {
        this.Current_month_temp = 11;
        this.currentYear = this.currentYear - 1;
      }
      this.Current_month = this.Current_month_temp;
    }
    if (value == 'next') {
      this.Current_month_temp = this.Current_month_temp + 1;
      if (this.Current_month_temp > 11) {
        this.Current_month_temp = 0;
        this.currentYear = this.currentYear + 1;
      }
      this.Current_month = this.Current_month_temp;
    }
    this.firstDayOfMonth = new Date(this.currentYear, this.Current_month, 1);
    this.lastDayOfMonth = new Date(this.currentYear, this.Current_month + 1, 0);

    this.getHealthTrendsrange(this.firstDayOfMonth, this.lastDayOfMonth);
  }

  getHealthTrendsrange(startDate: any, endDate: any) {
    var start_day;
    var end_day;

    var that = this;
    end_day = new Date(endDate);

    start_day = new Date(startDate);
    //end_day = endDate;

    // start_day = startDate;
    start_day = this.convertDate(start_day.setDate(start_day.getDate()));

    var utc_startday = start_day + 'T00:00:00';
    end_day = this.convertDate(end_day.setDate(end_day.getDate()));
    var utc_endday = end_day + 'T23:59:59';

    utc_startday = this.auth.ConvertToUTCRangeInput(new Date(utc_startday));
    utc_endday = this.auth.ConvertToUTCRangeInput(new Date(utc_endday));

    this.loading = true;
    this.rpm
      .rpm_get(
        `/api/patients/getpatienthealthtrends?StartDate=${utc_startday}&EndDate=${utc_endday}`
      )
      .then(
        (data) => {
          this.http_healthtrends = data;
          if (that.http_healthtrends.Values.length > 0) {
            that.lineChartLabels = that.convertDateforHealthTrends(
              that.http_healthtrends.Time
            );
          }
          if (
            this.http_healthtrends &&
            that.http_healthtrends.Values.length > 0
          ) {
            var temp = [];
            var j = 0;
            for (var item of that.http_healthtrends.Values) {
              var i = 0;
              for (var x of item.data) {
                if (j == 0) {
                  try {
                    if (
                      x == null &&
                      i > 0 &&
                      i < item.data.length &&
                      that.http_healthtrends.VitalName != 'Blood Glucose'
                    ) {
                      var linedt1 = this.lineChartLabels[i].split(' - ');
                      var linedt0 = this.lineChartLabels[i - 1].split(' - ');
                      var linedt2 = this.lineChartLabels[i + 1].split(' - ');
                      if (
                        linedt1[0] == linedt0[0] ||
                        linedt1[0] == linedt2[0]
                      ) {
                        this.lineChartLabels.splice(i, 1);
                        var k = 0;
                        for (var tmpitem of that.http_healthtrends.Values) {
                          that.http_healthtrends.Values[k].data.splice(i, 1);

                          k = k + 1;
                        }
                      }
                    }
                    i = i + 1;
                  } catch (ex) {
                    console.log('exception' + ex);
                  }
                }
              }
              j = j + 1;
              var obj = {
                data: item.data,
                label: item.label,
                fill: false,
                lineTension: 0.5,
              };
              temp.push(obj);
            }
            that.lineChartData = temp;
            this.loading = false;
          } else {
            this.setEmptyGraphMonth();
            this.loading = false;
          }
        },
        (err) => {
          this.setEmptyGraphMonth();
        }
      );
  }

  setEmptyGraph() {
    var date_val = new Date(this.WeekEndDate);
    var x = [0, 1, 2, 3, 4, 5, 6];
    var DefaultDates = [];
    var date_val_set = '';
    for (var item1 of x) {
      date_val_set = this.convertDate(date_val.setDate(date_val.getDate()));
      DefaultDates.push(date_val_set);
      date_val_set = this.convertDate(date_val.setDate(date_val.getDate() - 1));
    }

    this.http_healthtrends = {
      VitalName: this.billingInfo.PatientVital,
      VitalId: 1,
      Time: DefaultDates.reverse(), //["2022-07-28T00:00:00","2022-07-29T01:00:00","2022-08-01T10:00:00", "2022-08-01T00:00:00", "2022-08-02T02:00:00","2022-08-03T12:00:00","2022-08-04T00:00:00"],
      Values: [
        { data: [null, null, null, null, null, null, null], label: 'Vital' },
      ],
    };
    this.lineChartLabels = this.convertDateforHealthTrends(
      this.http_healthtrends.Time
    );

    var temp = [];
    for (var item of this.http_healthtrends.Values) {
      var obj = {
        data: item.data,
        label: item.label,
        fill: false,
        lineTension: 0.5,
      };
      temp.push(obj);
    }
    this.lineChartData = temp;
  }
  daysInMonth(month: any, year: any) {
    return new Date(parseInt(year), parseInt(month) + 1, 0).getDate();
  }
  getDatesInRange(startDate: any, endDate: any) {
    // const start = new Date(new Date(startDate).setUTCHours(0, 0, 0, 0));
    // const end = new Date(new Date(endDate).setUTCHours(0, 0, 0, 0));
    const start = new Date(startDate);
    const end = new Date(endDate);
    const date = new Date(start.getTime());

    const dates = [];

    while (date <= end) {
      var data = date.toISOString();
      dates.push(data);
      date.setDate(date.getDate() + 1);
    }

    return dates;
  }
  setEmptyGraphMonth() {
    var numberOfDays = this.daysInMonth(
      this.Current_month_temp,
      this.currentYear
    );
    var datavalue = [];
    var x = [];
    for (let i = 1; i < numberOfDays; i++) {
      x.push(i);
      datavalue.push(null);
    }

    var DefaultDatesdata = this.getDatesInRange(
      this.firstDayOfMonth,
      this.lastDayOfMonth
    );

    this.http_healthtrends = {
      VitalName: this.billingInfo.PatientVital,
      VitalId: 1,
      Time: DefaultDatesdata, //["2022-07-28T00:00:00","2022-07-29T01:00:00","2022-08-01T10:00:00", "2022-08-01T00:00:00", "2022-08-02T02:00:00","2022-08-03T12:00:00","2022-08-04T00:00:00"],
      Values: [{ data: datavalue, label: 'Vital' }],
    };
    this.lineChartLabels = this.convertDateforHealthTrends(
      this.http_healthtrends.Time
    );

    var temp = [];
    for (var item of this.http_healthtrends.Values) {
      var obj = {
        data: item.data,
        label: item.label,
        fill: false,
        lineTension: 0.5,
      };
      temp.push(obj);
    }
    this.lineChartData = temp;
  }
  clinicalMenuVariable: any;
  ClinicalInfoMenuSelect(menu: any) {
    switch (menu) {
      case 1:
        this.clinicalMenuVariable = 1;
        break;
      case 2:
        this.clinicalMenuVariable = 2;

        break;
      case 3:
        this.clinicalMenuVariable = 3;
        this.getMedication();

        this.dataSourceChange(2, 3);
        break;
      case 4:
        this.clinicalMenuVariable = 4;
        this.getSymptom();
        this.dataSourceChange(2, this.clinicalMenuVariable);
        break;
      case 5:
        this.clinicalMenuVariable = 5;
        this.getUploads();
        this.dataSourceChange(2, this.clinicalMenuVariable);
        break;
      default:
        this.clinicalMenuVariable = 1;
        break;
    }
    this.clinicalMenuVariable = menu;
  }
  currentVital: any;
  ChangeVitalScreen(button: any) {
    this.currentVital = button;
  }
  vital_menu_item = [
    { vital_name: 'BloodGlucose' },
    { vital_name: 'BloodPressure' },
    { vital_name: 'Oxygen' },
    { vital_name: 'Weight' },
  ];

  defaultScreen(id: any) {
    if (id !== 'None') {
      this.currentVital = id;
    } else {
      this.currentVital = 'None';
    }

    return this.currentVital;
  }
  public ascNumberSort = true;
  public sortVitalGlucoseColumn() {
    this.http_vitalGlucoseData.reverse();
  }

  public sortVitalBPColumn() {
    this.vital_temporaryData.reverse();
  }
  public sortOxygenColumn() {
    this.http_VitalOxygen.reverse();
  }
  public sortWeightColumn() {
    this.http_VitalWight.reverse();
  }

  // Activity Template:
  activityInfoMenu = [
    // {
    //   menu_id: 1,
    //   menu_title: 'Schedules',
    // },

    {
      menu_id: 2,
      menu_title: 'Calls',
    },

    // {
    //   menu_id: 3,
    //   menu_title: 'Chats',
    // },

    // {
    //   menu_id: 4,
    //   menu_title: 'SMS',
    // },
    // {
    //   menu_id: 5,
    //   menu_title: 'Goal Status',
    // },
  ];

  activityMenuVariable = 1;

  activityInfoMenuSelect(menu: any) {
    this.activityMenuVariable = menu;
    switch (menu) {
      // case 1:
      //   this.activityMenuVariable = 1;
      //   this.getScheduleData();
      //   this.dataSourceChange(3, 2);

      //   break;
      case 2:
        this.activityMenuVariable = 2;
        //this.getCallInfoData();
        this.dataSourceChange(3, 2);

        break;
      case 3:
        this.activityMenuVariable = 3;

        break;
      case 4:
        this.activityMenuVariable = 4;
        this.dataSourceChange(3, 4);
        break;
        // case 5:
        //   this.activityMenuVariable = 5;
        //   break;
        //   break;
        // case 6:
        //   this.activityMenuVariable = 6;
        //   break;
        // default:
        this.activityMenuVariable = 1;
        break;
    }
  }
  // navigateactivitySchedule() {
  //   this.scheduleVariable = !this.scheduleVariable;
  // }

  durationValue = 10;
  increment_duration() {
    if (this.durationValue < 10) {
      this.durationValue++;
    } else {
      this.durationValue = 10;
    }
  }

  decrement_duration() {
    if (this.durationValue > 0) {
      this.durationValue--;
    } else {
      this.durationValue = 0;
    }
  }
  medicationDurtaionincrement() {
    this.medication_durationValue++;
  }
  medicationDurtaiondecrement() {
    if (this.medication_durationValue > 0) {
      this.medication_durationValue--;
    } else {
      this.medication_durationValue = 0;
    }
  }
  frmactivityCallrange = new UntypedFormGroup({
    start: new UntypedFormControl(),
    end: new UntypedFormControl(),
  });
  rangeActivityCallCalc() {
    var that = this;
    var range_start_date = new Date(
      this.frmactivityCallrange.controls.start.value
    );
    range_start_date = this.convertDate(range_start_date);

    var range_end_date = new Date(this.frmactivityCallrange.controls.end.value);
    range_end_date.setDate(range_end_date.getDate());

    range_end_date = this.convertDate(range_end_date);
    var startDate;
    var endDate;
    // this.frmactivityCallrange.controls['start'].setValue(range_start_date);
    // this.frmactivityCallrange.controls['end'].setValue(range_end_date);
    startDate = range_start_date + 'T00:00:00';
    startDate = this.auth.ConvertToUTCRangeInput(new Date(startDate));
    endDate = range_end_date + 'T23:59:59';
    endDate = this.auth.ConvertToUTCRangeInput(new Date(endDate));

    var that = this;
    if (
      range_start_date != null &&
      range_end_date != null &&
      range_start_date <= range_end_date
    ) {
      this.rpm
        .rpm_get(
          `/api/patients/getmynotes?NoteType=CALL&StartDate=${range_start_date}&EndDate=${range_end_date}`
        )
        .then((data) => {
          this.http_CallNotesData = data;
          this.loading = false;
          this.dataSourceChange(3, 2);
        });
    }
  }

  convertToLocalTime(stillUtc: any) {
    if (stillUtc) {
      if (stillUtc.includes('+')) {
        var temp = stillUtc.split('+');
        stillUtc = temp[0];
      }
    }
    stillUtc = stillUtc + 'Z';
    const local = dayjs.utc(stillUtc).local().format('YYYY-MM-DD HH:mm:ss');

    return local;
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

  retArr: Array<string>;
  convertDateforPatientDOB(dateArr: any) {
    var dt = dateArr.split('T');
    var dtSplit = dt[0].split('-');
    var month = '';
    switch (dtSplit[1]) {
      case '01':
        month = 'Jan';
        break;
      case '02':
        month = 'Feb';
        break;
      case '03':
        month = 'Mar';
        break;
      case '04':
        month = 'Apr';
        break;
      case '05':
        month = 'May';
        break;
      case '06':
        month = 'Jun';
        break;
      case '07':
        month = 'Jul';
        break;
      case '08':
        month = 'Aug';
        break;
      case '09':
        month = 'Sep';
        break;
      case '10':
        month = 'Oct';
        break;
      case '11':
        month = 'Nov';
        break;
      case '12':
        month = 'Dec';
        break;
    }

    const bdate = new Date(dt[0]);
    const timeDiff = Math.abs(Date.now() - bdate.getTime());
    var age = Math.floor(timeDiff / (1000 * 3600 * 24) / 365);
    var dob =
      month + ' ' + dtSplit[2] + ',' + dtSplit[0] + ' (' + age + 'years)';
    return dob;
  }

  convertDateforHealthTrends(dateArr: any) {
    this.retArr = [];
    if (dateArr.length > 0) {
      for (let dateval of dateArr) {
        var newdate;
        if (this.http_healthtrends.Values[0].label != 'Vital') {
          newdate = this.convertToLocalTime(dateval);
        } else {
          newdate = dateval;
        }

        if (newdate.includes('T')) {
          var dt = newdate.split('T');
        } else {
          var dt = newdate.split(' ');
        }

        var dtSplit = dt[0].split('-');

        var month = '';
        switch (dtSplit[1]) {
          case '01':
            month = 'Jan';
            break;
          case '02':
            month = 'Feb';
            break;
          case '03':
            month = 'Mar';
            break;
          case '04':
            month = 'Apr';
            break;
          case '05':
            month = 'May';
            break;
          case '06':
            month = 'Jun';
            break;
          case '07':
            month = 'Jul';
            break;
          case '08':
            month = 'Aug';
            break;
          case '09':
            month = 'Sep';
            break;
          case '10':
            month = 'Oct';
            break;
          case '11':
            month = 'Nov';
            break;
          case '12':
            month = 'Dec';
            break;
        }
        var dat = month + '-' + dtSplit[2];

        if (this.http_healthtrends.Values[0].label != 'Vital') {
          var time = this.datepipe.transform(
            this.convertToLocalTime(dateval),
            'h:mm a'
          );
        } else {
          var time = this.datepipe.transform(dateval, 'h:mm a');
        }
        dat = dat + ' - ' + time;
        this.retArr.push(dat);
      }
    }

    return this.retArr;
  }

  ConvertToUTC(data: any) {
    var timenow = new Date().toLocaleTimeString();
    var combinedDate = new Date(data + ' ' + timenow);
    var combineDateIsoFrm = new Date(combinedDate.toUTCString()).toISOString();

    return combineDateIsoFrm;
  }
  ConvertTimeVital(dateArr: any) {
    this.retArr = [];
    var dt = dateArr.split('T');
    const [hourString, minute] = dt[1].split(':');
    const hour = +hourString % 24;
    return (hour % 12 || 12) + ':' + minute + (hour < 12 ? ' AM' : ' PM');
  }
  ConvertDateVital(dateArr: any) {
    this.retArr = [];
    var dt = dateArr.split('T');
    var dtSplit = dt[0].split('-');
    var month = '';
    switch (dtSplit[1]) {
      case '01':
        month = 'Jan';
        break;
      case '02':
        month = 'Feb';
        break;
      case '03':
        month = 'Mar';
        break;
      case '04':
        month = 'Apr';
        break;
      case '05':
        month = 'May';
        break;
      case '06':
        month = 'Jun';
        break;
      case '07':
        month = 'Jul';
        break;
      case '08':
        month = 'Aug';
        break;
      case '09':
        month = 'Sep';
        break;
      case '10':
        month = 'Oct';
        break;
      case '11':
        month = 'Nov';
        break;
      case '12':
        month = 'Dec';
        break;
    }
    var dat = month + ' ' + dtSplit[2] + ',' + dtSplit[0];
    return dat;
  }

  //  Medication Template Start
  //****************** Medication Data Start ***************************/

  // datasouceMedication =  Medication_DATA;

  selectmedicationRow($event: any, row: any) {
    $event.preventDefault();

    if (!row.selected) {
      this.dataSourceTable.filteredData.forEach(
        (row: { selected: boolean }) => (row.selected = false)
      );
      row.selected = true;
    }
  }
  someVar: any = false;
  selectRow($event: any, row: any) {
    $event.preventDefault();

    if (!row.selected) {
      this.dataSourceTableChat.filteredData.forEach(
        (row: { selected: boolean }) => (row.selected = false)
      );

      row.selected = true;
      if (row.selected) {
        this.someVar = true;
      }
    }
  }

  displayedMedicationColumns = [
    'Medicinename',
    'MedicineSchedule',
    'interval',
    'Morning',
    'AfterNoon',
    'Evening',
    'Night',
    'StartDate',
    'EndDate',
    'Description',
  ];
  /* dataSource = this.Medication_DATA; */

  addMedicationForm = new UntypedFormGroup({
    medicineName: new UntypedFormControl(null),
    scheduleInterval: new UntypedFormControl(null, [Validators.required]),
    scheduleTime: new UntypedFormControl(null, [Validators.required]),
    scheduleMorning: new UntypedFormControl(null),
    scheduleAfterNoon: new UntypedFormControl(null),
    scheduleEvening: new UntypedFormControl(null),
    scheduleNight: new UntypedFormControl(null),
    medicationstartDate: new UntypedFormControl(null),
    medicationendDate: new UntypedFormControl(null),
    medicationcomment: new UntypedFormControl(null),
  });

  updateMedicationForm = new UntypedFormGroup({
    updatemedicineName: new UntypedFormControl(null),
    scheduleInterval: new UntypedFormControl(null, [Validators.required]),
    scheduleTime: new UntypedFormControl(null, [Validators.required]),
    scheduleMorning: new UntypedFormControl(null),
    scheduleAfterNoon: new UntypedFormControl(null),
    scheduleEvening: new UntypedFormControl(null),
    scheduleNight: new UntypedFormControl(null),
    medicationstartDate: new UntypedFormControl(null),
    medicationendDate: new UntypedFormControl(null),
    medicationcomment: new UntypedFormControl(null),
  });

  addDocumentForm = new UntypedFormGroup({
    docType: new UntypedFormControl(null),
    docDesc: new UntypedFormControl(null),
  });
  openFile(e: any) {
    this.Doc = e.target.files[0];

    var a = document.getElementsByClassName('uploadPhoto');
    this.file = this.Doc.name;
  }

  mydateFormat(event: any) {
    console.log('Event is ');
    console.log(event);
  }

  IntervalArray = ['Weekly', 'Daily', 'Alernative'];
  IntervalTime = ['After Meal', 'Before Meal'];

  navigateclinicmedication() {
    this.medicationVariable = !this.medicationVariable;
  }

  medicationReload() {
    this.getMedication();
  }
  getMedicationData(data: any) {
    this.meditation_table_id = data.Id;

    this.medication_update_variable = true;
    var scheduleArray = data.MedicineSchedule.split(',');
    this.SelectedMedicalScheduleInterval = scheduleArray[0];
    this.SelectedMedicalScheduleTime = scheduleArray[1].trim();

    this.updateMedicationForm.controls['updatemedicineName'].setValue(
      data.Medicinename
    );
    this.updateMedicationForm.controls['scheduleInterval'].setValue(
      data.SelectedMedicalScheduleInterval
    );
    this.updateMedicationForm.controls['scheduleTime'].setValue(
      data.SelectedMedicalScheduleTime
    );
    this.updateMedicationForm.controls['scheduleMorning'].setValue(
      data.Morning
    );
    this.updateMedicationForm.controls['scheduleAfterNoon'].setValue(
      data.AfterNoon
    );
    this.updateMedicationForm.controls['scheduleEvening'].setValue(
      data.Evening
    );
    this.updateMedicationForm.controls['scheduleNight'].setValue(data.Night);
    this.updateMedicationForm.controls['medicationstartDate'].setValue(
      this.convertToLocalTime(data.StartDate)
    );
    this.updateMedicationForm.controls['medicationendDate'].setValue(
      this.convertToLocalTime(data.EndDate)
    );

    this.updateMedicationForm.controls['medicationcomment'].setValue(
      data.Description
    );

    if (data.Morning == true) {
      this.morningSchedule = 1;
    } else {
      this.morningSchedule = 0;
    }
    if (data.AfterNoon == true) {
      this.aftNoonSchedule = 1;
    } else {
      this.aftNoonSchedule = 0;
    }
    if (data.Evening == true) {
      this.eveningSchedule = 1;
    } else {
      this.eveningSchedule = 0;
    }
    if (data.Night == true) {
      this.nightSchedule = 1;
    } else {
      this.nightSchedule = 0;
    }
  }
  addMedication() {
    var req_body: any = {};
    req_body['Medicinename'] =
      this.addMedicationForm.controls.medicineName.value;
    req_body['MedicineSchedule'] =
      this.addMedicationForm.controls.scheduleInterval.value;
    req_body['MedicineSchedule'] =
      this.addMedicationForm.controls.scheduleTime.value;
    req_body['Morning'] = this.addMedicationForm.controls.scheduleMorning.value;
    req_body['AfterNoon'] =
      this.addMedicationForm.controls.scheduleAfterNoon.value;
    req_body['Evening'] = this.addMedicationForm.controls.scheduleEvening.value;
    req_body['Night'] = this.addMedicationForm.controls.scheduleNight.value;
    req_body['StartDate'] = this.convertToLocalTime(
      this.addMedicationForm.controls.medicationstartDate.value
    );
    req_body['EndDate'] = this.convertToLocalTime(
      this.addMedicationForm.controls.medicationendDate.value
    );
    req_body['Description'] =
      this.addMedicationForm.controls.medicationcomment.value;

    this.rpm.rpm_post('/api/patients/addpatientmedication', req_body).then(
      (data) => {
        this.dialog.closeAll();
        alert('Medication added Successfully..!');
        this.addMedicationForm.reset();
        this.medicationVariable = false;
      },
      (err) => {
        this.dialog.closeAll();
        alert('Failed to add Medication..!');
      }
    );
  }

  updateMedication() {
    var updatemedicineName = `${this.updateMedicationForm.controls.updatemedicineName.value}`;
    var medicalSchedule = `${this.updateMedicationForm.controls.scheduleInterval.value},${this.updateMedicationForm.controls.scheduleTime.value}`;
    var updatestartDate = `${this.convertToLocalTime(
      this.updateMedicationForm.controls.medicationstartDate.value
    )}`;
    var updateendDate = `${this.convertToLocalTime(
      this.updateMedicationForm.controls.medicationendDate.value
    )}`;
    var updatemedicineDes = `${this.updateMedicationForm.controls.medicationcomment.value}`;

    var req_body: any = {};
    req_body['Id'] = parseInt(this.meditation_table_id);

    req_body['Medicinename'] = updatemedicineName;
    req_body['MedicineSchedule'] = medicalSchedule;
    req_body['Morning'] =
      this.updateMedicationForm.controls.scheduleMorning.value;
    req_body['AfterNoon'] =
      this.updateMedicationForm.controls.scheduleAfterNoon.value;
    req_body['Evening'] =
      this.updateMedicationForm.controls.scheduleEvening.value;
    req_body['Night'] = this.updateMedicationForm.controls.scheduleNight.value;
    req_body['StartDate'] = updatestartDate;
    req_body['EndDate'] = updateendDate;
    /*  req_body['EndDate'] = this.convertToLocalTime(this.updateMedicationForm.controls.medicationendDate); */
    req_body['Description'] = updatemedicineDes;
    var that = this;

    this.rpm.rpm_post('/api/patient/updatepatientmedication', req_body).then(
      (data) => {
        //  alert(data+"Medication Form Updated Successfully!!")
        alert('Medication Form Updated Successfully!!');
        this.updateMedicationForm.reset();
        this.medication_update_variable = false;
        this.medicationReload();
        /*  this.getMenuChioce(1);  */
      },
      (err) => {
        console.log(err);
        alert('Failed to update Medication..!');
      }
    );
  }

  //Medication Template End
  // Symtoms Template Data Start
  symtomsTableColumn = [
    'Id',
    'Symptom',
    'Description',
    'SymptomStartDateTime',
    'time',
  ];
  displayedSymtomColumns = [
    'selection',
    'documenttype',
    'Description',
    'Date',
    'actions',
  ];
  displayeduploadColumns = [
    'DocumentType',
    'DocumentName',
    'CreatedOn',
    'actions',
  ];

  /*  dataSource1 = SymtomsData; */
  /* dataSource2=Upload; */
  dataSource3 = SMSData;
  /*  dataSource4=CallData; */
  dataSource5 = ChatData;
  dataSource6 = GoalData;

  // Document Template

  uploaddoc() {
    var formData: any = new FormData();

    formData.append('DocumentType', this.docType);
    formData.append('DocumentName', this.docDesc);
    formData.append('Document', this.Doc);

    if (this.Doc != null && this.docType != null && this.docDesc != null) {
      this.rpm.rpm_post('/api/patients/adddocument', formData).then(
        (data) => {
          alert('Document Added Successfully');
          this.getUploads();
          this.CancelDocForm();
          this.documentVariable = false;
          this.uploadVariable = false;
        },
        (err) => {
          console.log('Error While Adding Document..!');
        }
      );
    } else {
      alert('Please Complete the Form');
    }
  }

  navigateclinicupload() {
    this.uploadVariable = !this.uploadVariable;
  }

  healthtrends(selected_val: any) {
    this.heath_trends_frequency = selected_val;

    this.getHealthTrends(selected_val);
  }

  frmactivitySchedulerange = new UntypedFormGroup({
    start: new UntypedFormControl(),
    end: new UntypedFormControl(),
  });
  rangeActivityScheduleCalc() {
    var that = this;
    var range_start_date = new Date(
      this.frmactivitySchedulerange.controls.start.value
    );
    range_start_date = this.convertDate(range_start_date);

    var range_end_date = new Date(
      this.frmactivitySchedulerange.controls.end.value
    );
    range_end_date.setDate(range_end_date.getDate() + 1);

    range_end_date = this.convertDate(range_end_date);
    var that = this;
    if (
      range_start_date != null &&
      range_end_date != null &&
      range_start_date < range_end_date
    ) {
      that.rpm
        .rpm_get(
          `/api/patients/getpatientschedules?StartDate=${range_start_date}&EndDate=${
            range_end_date + 'T23:59:59'
          }`
        )
        .then((data) => {
          that.http_ActivityScheduleData = data;
        });
    }
  }
  currentpPatientId: any;
  /* today = new Date(); */
  getScheduleData() {
    var startdate;
    var enddate;
    var scheduleDataCurrentDate = new Date();
    startdate = new Date(
      this.today.getFullYear(),
      scheduleDataCurrentDate.getMonth(),
      1
    );
    enddate = new Date(
      this.today.getFullYear(),
      scheduleDataCurrentDate.getMonth() + 1,
      0
    );
    startdate = this.convertDate(startdate);
    startdate = startdate + 'T00:00:00';
    startdate = this.auth.ConvertToUTCRangeInput(new Date(startdate));
    enddate = this.convertDate(enddate);
    enddate = enddate + 'T23:59:59';
    enddate = this.auth.ConvertToUTCRangeInput(new Date(enddate));
    this.frmactivitySchedulerange.controls['start'].setValue(startdate);
    this.frmactivitySchedulerange.controls['end'].setValue(enddate);
    this.rpm
      .rpm_get(
        `/api/patients/getpatientschedules?StartDate=${startdate}&EndDate=${enddate}`
      )
      .then((data) => {
        this.http_ActivityScheduleData = data;
      });
  }
  http_CallNotesData: any;
  getCallInfoData() {
    var callCurrentDate = new Date();
    var startdate = new Date(
      this.today.getFullYear(),
      callCurrentDate.getMonth(),
      1
    );
    var enddate = new Date(
      this.today.getFullYear(),
      callCurrentDate.getMonth() + 1,
      0
    );
    startdate = this.convertDate(startdate);
    enddate = this.convertDate(enddate);
    this.frmactivityCallrange.controls['start'].setValue(startdate);
    this.frmactivityCallrange.controls['end'].setValue(enddate);

    this.loading = true;
    this.rpm
      .rpm_get(
        `/api/patients/getmynotes?NoteType=CALL&StartDate=${startdate}&EndDate=${enddate}`
      )
      .then((data) => {
        this.http_CallNotesData = data;
        this.loading = false;
        // this.tempdatasource = this.http_CallNotesData;
        this.dataSourceChange(5, 2);
        // this.dataSourceTable = new MatTableDataSource(this.tempdatasource);
        // this.dataSourceTable.paginator = this.paginator;
        // this.total_number = this.dataSourceTable.filteredData.length;
      });
  }
  startTimer() {
    if (this.interval) {
      clearInterval(this.interval);
    }
    this.interval = setInterval(() => {
      this.updTimer();
    }, 1000);
  }

  updTimer() {
    let day = '00';
    this.callTime += 1000;
    if (this.callTime >= 86400000) {
      let d = ~~(this.callTime / 86400000); //bitwise operator to remove decimals
      day = d.toString();
    }

    if (day != '00') {
      this.Duration =
        day + ':' + new Date(this.callTime).toUTCString().split(/ /)[4];
    } else {
      this.Duration = new Date(this.callTime).toUTCString().split(/ /)[4];
      this.Duration =
        this.Duration[3] +
        this.Duration[4] +
        this.Duration[5] +
        this.Duration[6] +
        this.Duration[7];
    }
  }
  stopTimer() {
    this.callTime = 0;
    this.Duration = '00:00';
    if (this.interval) {
      clearInterval(this.interval);
    }
  }
  close_modal() {
    this.call_panel_visible = false;
    this.stopTimer();
  }
  CompleteionPercentage = 0;
  programgoals: any;
  program_vital_name: any;
  clinic: any;
  clinicName: any;
  clinicCode: any;
  prescribedassignedDate: any;
  getPatientInfo() {
    this.patientdataUserId = null;
    this.loading2 = true;

    this.rpm.rpm_get(`/api/patients/getpatient`).then((data) => {
      this.http_rpm_patientList = data;

      //this.assignedDate = this.datepipe.transform(prescribedassignedDate, 'MMM/dd/yyyy');
      var assignedDate =
        this.http_rpm_patientList['PatientPrescribtionDetails'].PrescribedDate;
      this.prescribedassignedDate = this.datepipe.transform(
        this.convertToLocalTime(assignedDate),
        'MMM/dd/yyyy'
      );
      this.CompleteionPercentage =
        (this.http_rpm_patientList.ProfileSummary.CurrentDuration /
          this.http_rpm_patientList.ProfileSummary.TotalDuration) *
        100;
      var name =
        this.http_rpm_patientList.FirstName +
        ' ' +
        this.http_rpm_patientList.LastName;
      sessionStorage.setItem('user_name', name);
      sessionStorage.setItem('userid', this.http_rpm_patientList.UserId);
      this.patientName =
        this.http_rpm_patientList.PatientDetails.FirstName +
        this.http_rpm_patientList.PatientDetails.MiddleName +
        ' ' +
        this.http_rpm_patientList.PatientDetails.LastName;
      this.patientID = this.http_rpm_patientList.PatientDetails.UserId;
      this.patient_dob = this.convertDateforPatientDOB(
        this.http_rpm_patientList.PatientDetails.DOB
      );

      this.clinicName =
        this.http_rpm_patientList['PatientPrescribtionDetails'].Clinic;

      this.clinicCode =
        this.http_rpm_patientList['PatientPrescribtionDetails'].ClinicCode;

      this.patientdob = this.patient_dob;
      this.patient_address =
        this.http_rpm_patientList.PatientDetails.CityName +
        ' ' +
        ',' +
        this.http_rpm_patientList.PatientDetails.State;
      this.plan_duration_balance =
        this.http_rpm_patientList.PatientProgramdetails.ProgramStatus;
      this.careplan =
        this.http_rpm_patientList.PatientProgramdetails.PatientVitalInfos;
      this.careplanStatus =
        this.http_rpm_patientList.PatientProgramdetails.Status;
      const patientcareplan: string[] = [];
      for (
        let i = 0;
        i <
        this.http_rpm_patientList.PatientProgramdetails.PatientVitalInfos
          .length;
        i++
      ) {
        patientcareplan.push(
          this.http_rpm_patientList.PatientProgramdetails.PatientVitalInfos[i]
            .Vital
        );
      }
      if (patientcareplan.includes('Blood Glucose')) {
        this.bgplan = true;
      } else if (patientcareplan.includes('Blood Pressure')) {
        this.bpplan = true;
      } else if (patientcareplan.includes('Pulse')) {
        this.pulseplan = true;
      }

      this.program_name =
        this.http_rpm_patientList['PatientProgramdetails'].ProgramName;
      // this.program_vital_name =
      //   this.http_rpm_patientList.PatientProgramdetails.ProgramId;
      // var pgm = this.http_rpm_patientList['PatientProgramdetails'].filter(
      //   (c: { ProgramId: any }) => c.ProgramId == this.program_vital_name
      // );
      // if (pgm.length > 0) {
      //   this.program_vital_name = pgm[0].ProgramName;
      // }

      this.programStartTime =
        this.http_rpm_patientList['PatientProgramdetails'].StartDate;
      this.programStart = this.datepipe.transform(
        this.convertToLocalTime(this.programStartTime),
        'MMM/dd/yyyy'
      );

      this.programEndTime =
        this.http_rpm_patientList['PatientProgramdetails'].EndDate;
      this.program_endTime = this.datepipe.transform(
        this.convertToLocalTime(this.programEndTime),
        'MMM/dd/yyyy'
      );

      this.consulationDate =
        this.http_rpm_patientList[
          'PatientPrescribtionDetails'
        ].ConsultationDate;
      this.patientconsultationDate = this.datepipe.transform(
        this.consulationDate,
        'MMM/dd/yyyy'
      );
      var assignedDate =
        this.http_rpm_patientList['PatientPrescribtionDetails'].PrescribedDate;
      this.assignedDate = this.datepipe.transform(assignedDate, 'MMM/dd/yyyy');
      this.currentPhysician =
        this.http_rpm_patientList.PatientPrescribtionDetails.Physician;
      this.assignedMember =
        this.http_rpm_patientList.PatientProgramdetails.AssignedMember;
      this.patiantVital =
        this.http_rpm_patientList['PatientProgramdetails'].PatientVitalInfos;
      this.patiantVital = this.patiantVital.filter(
        (ds: { Selected: boolean }) => ds.Selected == true
      );

      this.patientdataUserId = this.http_rpm_patientList.PatientDetails.UserId;
      this.PatientProgramduration =
        this.http_rpm_patientList['PatientProgramdetails'].Duration;
      sessionStorage.setItem('patientdataUserId', this.patientdataUserId);
      this.vitals =
        this.http_rpm_patientList.PatientProgramdetails.PatientVitalInfos;
      sessionStorage.setItem('viatls', JSON.stringify(this.vitals));
      sessionStorage.setItem(
        'pname',
        this.http_rpm_patientList['PatientDetails'].FirstName +
          ' ' +
          this.http_rpm_patientList['PatientDetails'].LastName
      );
      sessionStorage.setItem(
        'patientPgmName',
        this.http_rpm_patientList['PatientProgramdetails'].ProgramName
      );
      sessionStorage.setItem(
        'program_id',
        this.http_rpm_patientList['PatientProgramdetails'].PatientProgramId
      );
      if (
        this.http_rpm_patientList['PatientProgramdetails'].Status ==
        'Discharged'
      ) {
        this.activeProgram = false;
      } else {
        this.activeProgram = true;
      }

      var d = new Date();
      var n = d.getUTCDate();
      this.callTimerEnabled = false;
      this.programgoals =
        this.http_rpm_patientList.PatientProgramGoals.goalDetails;
      // sessionStorage.setItem(
      //   'pname',
      //   this.http_rpm_patientList['PatientDetails'].FirstName +
      //     ' ' +
      //     this.http_rpm_patientList['PatientDetails'].LastName
      // );
      // sessionStorage.setItem(
      //   'patientPgmName',
      //   this.http_rpm_patientList['PatientProgramdetails'].ProgramName
      // );

      // sessionStorage.setItem(
      //   'careteamuser',
      //   this.http_rpm_patientList.PatientProgramdetails.CareTeamUserId
      // );
      this.patientFirstName =
        this.http_rpm_patientList['PatientDetails'].FirstName;
      this.programStartTime =
        this.http_rpm_patientList['PatientProgramdetails'].StartDate;
      this.programEndTime =
        this.http_rpm_patientList['PatientProgramdetails'].EndDate;

      this.programStart = this.datepipe.transform(
        this.convertToLocalTime(this.programStartTime),
        'MMM/dd/yyyy'
      );

      this.program_endTime = this.datepipe.transform(
        this.programEndTime,
        'MMM/dd/yyyy'
      );
      this.consulationDate =
        this.http_rpm_patientList[
          'PatientPrescribtionDetails'
        ].ConsultationDate;
      this.patientconsultationDate = this.datepipe.transform(
        this.consulationDate,
        'MMM/dd/yyyy'
      );
      var assignedDate =
        this.http_rpm_patientList['PatientPrescribtionDetails'].PrescribedDate;
      this.assignedDate = this.datepipe.transform(
        this.convertToLocalTime(assignedDate),
        'MMM/dd/yyyy'
      );
      this.currentPhysician =
        this.http_rpm_patientList.PatientPrescribtionDetails.Physician;
      this.assignedMember =
        this.http_rpm_patientList.PatientProgramdetails.AssignedMember;
      /*  this.setCommDetails(); */
      this.devicedetails =
        this.http_rpm_patientList.PatientDevicesDetails.PatientDeviceInfos;
      this.vitaldetails =
        this.http_rpm_patientList.PatientVitalDetails.PatientVitalInfos;
      this.program_name =
        this.http_rpm_patientList['PatientProgramdetails'].ProgramName;

      this.documentData =
        this.http_rpm_patientList.PatientDocumentDetails.PatientDocumentinfos;
      this.loading2 = false;
    });
  }

  getMedication() {
    this.loading = true;
    this.rpm.rpm_get(`/api/patients/getpatientmedication`).then((data) => {
      this.http_medication_data = data;
      this.loading = false;
      //this.tempdatasource = this.http_medication_data;
      this.dataSourceChange(2, 3);
    });
  }

  getSymptom() {
    this.rpm.rpm_get(`/api/patients/getpatientsymptoms`).then((data) => {
      this.dataSourceTableSymptom = data;
      /*  this.http_get_symptoms = data; */
    });
  }
  getUploads() {
    this.rpm.rpm_get(`/api/patients/getpatientuploads`).then((data) => {
      this.dataSourceTableUpload = data;
      this.dataSourceChange(2, 5);
    });
  }
  rangeVitalScheduleCalc() {
    var that = this;
    var range_start_date = this.frmvitalrange.controls.start.value;
    range_start_date = this.convertDate(range_start_date);
    range_start_date = range_start_date + 'T00:00:00';
    range_start_date = this.auth.ConvertToUTCRangeInput(
      new Date(range_start_date)
    );

    // var range_end_date =  new Date(this.frmvitalrange.controls.end.value);
    // range_end_date = this.convertDate(range_end_date);
    var range_end_date = this.frmvitalrange.controls.end.value;
    range_end_date = this.convertDate(range_end_date);
    range_end_date = range_end_date + 'T23:59:59';
    range_end_date = this.auth.ConvertToUTCRangeInput(new Date(range_end_date));

    // this.frmvitalrange.controls['start'].setValue(range_start_date);
    // this.frmvitalrange.controls['end'].setValue(range_end_date);

    if (
      range_start_date != null &&
      range_end_date != null &&
      range_start_date <= range_end_date
    ) {
      that.rpm
        .rpm_get(
          `/api/patients/getpatientvitalreadings?StartDate=${range_start_date}&EndDate=${range_end_date}`
        )
        .then((data) => {
          that.http_vitalData = data;
          console.log('Vital Reading data');
          console.log(that.http_vitalData);
          this.defaultScreen(this.http_vitalData[0].VitalName);


          that.vital_temporaryData = this.newVitalreadingData(
            this.http_vitalData.BloodPressure,
            'BloodPressureReadings'
          );
          that.http_vitalGlucoseData = this.newVitalreadingData(
            this.http_vitalData.BloodGlucose,
            'BloodGlucoseReadings'
          );
          that.http_VitalOxygen =
            that.http_vitalData &&
            this.newVitalreadingData(
              this.http_vitalData.BloodOxygen,
              'BloodOxygenReadings'
            );
          that.http_VitalWight =
            that.http_vitalData &&
            this.newVitalreadingData(
              this.http_vitalData.Weight,
              'WeightReadings'
            );
          if (
            that.http_vitalGlucoseData &&
            that.http_vitalGlucoseData.length > 0
          ) {
            this.ChangeVitalScreen('BloodGlucose');
          } else if (
            that.vital_temporaryData &&
            that.vital_temporaryData.length > 0
          ) {
            this.ChangeVitalScreen('BloodPressure');
          } else if (
            that.http_VitalOxygen &&
            that.http_VitalOxygen.length > 0
          ) {
            this.ChangeVitalScreen('Oxygen');
          } else if (that.http_VitalWight && that.http_VitalWight.length > 0) {
            this.ChangeVitalScreen('Weight');
          }
        });
    } else {
      return;
    }
  }

  getDaysCalc(startdate: any, enddate: any) {
    let date_1 = new Date(startdate);
    let date_2 = new Date(enddate);
    let difference = date_2.getTime() - date_1.getTime();

    let TotalDays = Math.ceil(difference / (1000 * 3600 * 24));

    return Math.abs(TotalDays);
  }
  caluculateEndDate(startDate: any) {
    var someDate = new Date(startDate);
    var numberOfDaysToAdd = this.medication_durationValue;
    var result = someDate.setDate(someDate.getDate() + numberOfDaysToAdd);
    return new Date(result);
  }

  CCMClinicalMenu = [
    {
      menu_id: 3,
      menu_title: 'Medication',
    },
    {
      menu_id: 4,
      menu_title: 'Symptoms',
    },
    {
      menu_id: 5,
      menu_title: 'Uploads',
    },
  ];

  rpmClinicalMenu = [
    {
      menu_id: 1,
      menu_title: 'Vital Readings',
    },
    {
      menu_id: 2,
      menu_title: 'Health Trends',
    },
    {
      menu_id: 3,
      menu_title: 'Medication',
    },
    {
      menu_id: 4,
      menu_title: 'Symptoms',
    },
    {
      menu_id: 5,
      menu_title: 'Uploads',
    },
  ];
  rolelist: any;
  getDisplyClinicalInfoMenu() {
    var ClinicalMenuResult;
    this.rolelist = sessionStorage.getItem('Roles');
    this.rolelist = JSON.parse(this.rolelist);
    var programrole = this.rolelist[0].ProgramName;
    if (programrole != 'RPM') {
      ClinicalMenuResult = this.CCMClinicalMenu;
      this.clinicalMenuVariable = 3;
      this.getMedication();
      // this.dataSourceChange(2, 3);
    } else {
      ClinicalMenuResult = this.rpmClinicalMenu;
      this.clinicalMenuVariable = 1;
    }

    return ClinicalMenuResult;
  }
  backhome() {
    let route = '/admin/patient-home';
    this.route.navigate([route]);
  }
  monthIndex: any;
  selectedMonth: any;
  selectedMonthName(monthNumber: any, year: any) {
    return dayjs().month(monthNumber).format('MMMM') + ',' + year;
  }

  getlinechartLabel(lineData: any) {
    lineData.map((v: any) => {
      console.log(v.label);
      if (v.label == 'Non-Fasting') {
        this.lineChartColor = 'red';
        var temp = {
          // backgroundColor: 'red',
          fill: false,
          borderColor: this.lineChartColor,
          pointBackgroundColor: this.lineChartColor,
          // pointBorderColor: '#fff',
          pointBorderColor: 'orange',
          pointHoverBackgroundColor: '#fff',
          pointHoverBorderColor: 'rgba(148,159,177,0.8)',
        };
        this.lineChartColors.push(temp);
      } else if (v.label == 'Fasting') {
        this.lineChartColor = 'green';
        var temp = {
          // backgroundColor: 'red',
          fill: false,
          borderColor: this.lineChartColor,
          pointBackgroundColor: this.lineChartColor,
          // pointBorderColor: '#fff',
          pointBorderColor: 'orange',
          pointHoverBackgroundColor: '#fff',
          pointHoverBorderColor: 'rgba(148,159,177,0.8)',
        };
        this.lineChartColors.push(temp);
      } else if (v.label == 'Vital') {
        this.lineChartColor = 'blue';
        var temp = {
          // backgroundColor: 'red',
          fill: false,
          borderColor: this.lineChartColor,
          pointBackgroundColor: this.lineChartColor,
          // pointBorderColor: '#fff',
          pointBorderColor: 'orange',
          pointHoverBackgroundColor: '#fff',
          pointHoverBorderColor: 'rgba(148,159,177,0.8)',
        };
        this.lineChartColors.push(temp);
      }
    });
    console.log(this.lineChartColor);

    return this.lineChartColors;
  }
  HealthData: Array<any> = [];
  OrderHealthTrendValue(HealthtrendData: any) {
    if (HealthtrendData.length > 0) {
      if (HealthtrendData[0].label && HealthtrendData[0].label != 'Vital') {
        if (HealthtrendData[0].label == 'Fasting') {
          var tempData = HealthtrendData[0];
          tempData.borderColor = 'pink';

          this.HealthData.push(tempData);
        } else if (HealthtrendData[1].label == 'Non-Fasting') {
          var tempData = HealthtrendData[1];
          tempData.borderColor = 'yellow';

          this.HealthData.push(tempData);
        }
      }

      console.log('HealthData');
      console.log(this.HealthData);
    }
  }
}
