import { SidePanelPatientComponent } from '../../shared/side-panel-patient/side-panel-patient.component';
import { TwilioVideoServiceService } from '../../../../services/twilio-video-service.service';
import { Device } from '@twilio/voice-sdk';
import {
  Component,
  OnInit,
  TemplateRef,
  ViewChild,
  ChangeDetectorRef,
  ElementRef,
  OnDestroy,
} from '@angular/core';
import { SelectionModel } from '@angular/cdk/collections';
import { MatDialog, MatDialogConfig } from '@angular/material/dialog';
import { ActivatedRoute, Router } from '@angular/router';
import { BreakpointObserver, BreakpointState } from '@angular/cdk/layout';
import { RPMService } from '../../sevices/rpm.service';
import { AuthService } from 'src/app/services/auth.service';
import { DatePipe } from '@angular/common';
import { webSocket } from 'rxjs/webSocket';
import { MatTableDataSource } from '@angular/material/table';
import { MatPaginator } from '@angular/material/paginator';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { StatusMessageComponent } from '../../shared/status-message/status-message.component';
import { MatSort } from '@angular/material/sort';
import { jsPDF } from 'jspdf';
import { ConfirmDialogComponent } from '../../shared/confirm-dialog/confirm-dialog.component';
import { ChatbuttonComponent } from '../../shared/chatbutton/chatbutton.component';
import { PatientDataDetailsService } from './Models/service/patient-data-details.service';
import { skip, take, takeUntil } from 'rxjs/operators';
import { PatientUtilService } from './Models/service/patient-util.service';
import { Subject, Subscription } from 'rxjs';
import { PatientChatService } from '../../shared/chatbutton/service/patient-chat.service';
import {
  billingC_CCM,
  billingCCM_C,
  billingCCM_P,
  billingPCM_C,
  billingPCM_P,
} from './Models/interface';
import { DownloadPatientReportService } from '../reports/services/download-patient-report.service';
import { PatientReportApiService } from '../reports/services/patient-report-api.service';
import { ConfirmDialogServiceService } from '../../shared/confirm-dialog-panel/service/confirm-dialog-service.service';
import dayjs from 'dayjs';
import utc from 'dayjs/plugin/utc';
import timezone from 'dayjs/plugin/timezone';

dayjs.extend(utc);
dayjs.extend(timezone);

export interface Symptoms {
  Symptom: string;
  Description: string;
  Date: string;
  Time: string;
  selected: boolean;
  selectable?: boolean;
}

const SymtomsData: Symptoms[] = [
  {
    Symptom: 'Skin Rashes',
    Description: 'Lorem ipsum dolor sit amet, consetetur ',
    Date: 'Dec 29, 2021',
    Time: '09:12 PM, EST',
    selected: false,
  },
];

export interface BillingData {
  CPTCode: string;
  Last_Billing_Cycle: string;
  report: File;
}

export interface DocumentData {
  Id: number;
  DocumentType: string;
  DocumentName: string;
  CreatedOn: string;
  DocumentUNC: string;
}

// declare const Twilio: any;
// const Video = Twilio.Video;
@Component({
  selector: 'app-patient-detail-page',
  templateUrl: './patient-detail-page.component.html',
  styleUrls: ['./patient-detail-page.component.scss'],
})
export class PatientDetailPageComponent implements OnInit, OnDestroy {
  @ViewChild(SidePanelPatientComponent)
  private patientrightsidebar: SidePanelPatientComponent;
  private chatButon: ChatbuttonComponent;
  @ViewChild('programRenew', { static: true }) programRenew: TemplateRef<any>;
  @ViewChild('localVideo', { static: true })
  localVideo?: ElementRef<HTMLElement>;
  @ViewChild('remoteVideo', { static: true })
  remoteVideo?: ElementRef<HTMLElement>;
  room_name: any;
  access_tokan: any;
  app_id: any;
  additionaNotes: any;
  totalInteractionTime: any;
  pecentageValue: any;
  displayText: any;
  interactionpercentValue: any;
  isEstablished: boolean;
  patient_id: any;
  selected: any;
  variable: any = 1;
  myValue = false;
  checked = true;
  vitalVariable: any;
  clinicalVariable: any;
  ScreenSizevalue = true;
  http_patientInfo: any;
  connection: any = null;
  device: any;
  data_json: any;
  phoneNumber: any;
  callHideShow: any = true;
  stopHideShow: any = false;
  call_connected: any = false;
  smallScreenVariable: any = true;
  progress_value: any;
  http_rpm_patientList: any;
  patientAddressFirstLine: any;
  patientAddressLastLine: any;
  programStart: any;
  program_id: any;
  CurrentTime: any;
  program_name: any;
  timerValue = 0;
  calltimerValue = 0;
  programDuration: any;
  patiantVital: any;
  program_endTime: any;
  callVariable = false;
  dataSourcedata: any = [];
  PageTimer: any;
  loading: boolean;
  master_data: any;
  StatesAndCities: any;
  clinic: any;
  clinicName: any;
  clinicCode: any;
  state: any;
  city: any;

  socketConnection: unknown;
  goaldataSource: any;
  loading_note: any;
  videoOnVariable: any;

  patient_city: any;
  phoneExtension: any;
  vitals: any;
  patientHeight: any;
  timezone: any;
  patientdataUserId: any;
  CurrentProgramSelected: any;
  PageTimerBeforeRenew: any;
  patientStatusData: any;
  loading3: any;
  loading4: boolean;
  onHoldDate: any;
  dateCompare = 'undefined';
  public unreadCount: number = 0;
  @ViewChild(MatSort) sort = new MatSort();

  dataSourceTable: any;
  @ViewChild(MatPaginator) paginator: MatPaginator;
  @ViewChild('myPreviewTemp', { static: true }) myPreviewTemp: TemplateRef<any>;
  @ViewChild('myPreviewUpdateTemp', { static: true })
  myPreviewUpdateTemp: TemplateRef<any>;
  @ViewChild('myPreviewPdf', { static: true }) myPreviewPdf: TemplateRef<any>;
  @ViewChild('myVideotemp', { static: true }) myVideotemp: TemplateRef<any>;

  total_number: any;
  http_get_symptoms: any;
  temp_symptoms: any;
  filteredOptions: any;
  symptom_update_variable: any;
  http_medication_data: any;
  temp_medication: any;
  medication_durationValue = 0;


  // Vital Reading Template
  http_vitalData: any;
  vital_temporaryData: any;
  vitalDataSource: any;
  http_vitalGlucoseData: any;
  vitalDataGlucoseSource: any;
  today = new Date();
  sevenDaysAgo = new Date(this.today).setDate(this.today.getDate() - 30);

  ThirtyDaysAgo = new Date(this.today).setDate(this.today.getDate() - 30);

  SevensDaysAgo = new Date(this.today).setDate(this.today.getDate() - 7);
  Tomorrow = new Date(this.today).setDate(this.today.getDate() + 1);
  Today = new Date();

  // Activity Menu
  // Review Notes Template Declaration Start
  http_reviewNotesData: any;
  review_temporaryData: any;
  http_chatData:any;
  reviewDataSource: any;

  // Review Notes Template Declartion Ends

  // Call Notes Template Start

  http_CallNotesData: any;
  callNotes_temporaryData: any;
  CallNotesDataSource: any;

  // Call Notes Template Ends

  // Get Schedule Template Start

  http_ActivityScheduleData: any;
  // Get Schedule Template Ends
  // Get Activity Alert & Task Start
  http_ActivityAlertTask: any;
  http_ActivityAlertTaskTemp: any;
  http_getId: any;
  // Get Activity Alert & Task Ends
  selectedSymptoms: any;

  http_unassigned_members: any;
  pid: any;
  cid: any;
  ptime: any;
  heath_trends_frequency: number;
  interval1: NodeJS.Timeout;
  interval: NodeJS.Timeout;
  interval2: NodeJS.Timeout;
  interval3: NodeJS.Timeout;
  calledittimerValue: any;
  public pdfs: any;
  public page = 2;
  public pageLabel!: string;
  private unreadSubscription: Subscription | null = null;
  private destroy$ = new Subject<void>();
  public showDialog = false;
  public showProgramRenewModal = false;
  public smsdialogpanel=false;

  confirmAction: (() => void) | null = null;
  public showNoteupdateModal = false;

  openPdf(pdfSrc: any) {
    this.pdfs = pdfSrc;
    this.dialog.open(this.myPreviewPdf);
  }
  resetReviewTimer() {
    this.timerValue = 0;
  }
  resetCallTimer() {
    this.calltimerValue = 0;
    this.calltimer = this.timeConvert(this.calltimerValue);
  }
  medicationDurtaionincrement() {
    this.medication_durationValue++;
  }
  reformat(event: any) {
    if (event.data) {
      const two_chars_no_colons_regex = /([^:]{2}(?!:))/g;
      event.target.value = event.target.value.replace(
        two_chars_no_colons_regex,
        '$1:'
      );
    }
  }
  medicationDurtaiondecrement() {
    if (this.medication_durationValue > 0) {
      this.medication_durationValue--;
    } else {
      this.medication_durationValue = 0;
    }
  }

  // Medication  Template variable Declaration End

  programDetails: any;
  QuestionArrayBase: any;
  patientProgramName: any;
  ProgramHistory: any;
  private chatHistoryDataSubscription: Subscription;

  constructor(
    private router: Router,
    private auth: AuthService,
    public datepipe: DatePipe,
    private rpm: RPMService,
    private dialog: MatDialog,
    private _route: ActivatedRoute,
    public breakpoint: BreakpointObserver,
    private changeDetectorRef: ChangeDetectorRef,
    public twilioService: TwilioVideoServiceService,
    public patientutilService: PatientUtilService,
    private patientService: PatientDataDetailsService,
    private patientchatservice: PatientChatService,
    private PatientReportapi: PatientReportApiService,
    private patientdownloadService: DownloadPatientReportService,
    private confirmDialog:ConfirmDialogServiceService
  ) {
    var that = this;

    this.convertToLocalTime('');
    this.currentpPatientId = sessionStorage.getItem('PatientId');
    that.master_data = sessionStorage.getItem('add_patient_masterdata');
    that.master_data = JSON.parse(that.master_data);
    that.StatesAndCities = sessionStorage.getItem('states_cities');
    that.StatesAndCities = JSON.parse(that.StatesAndCities);
    that.programDetails = that.master_data.ProgramDetailsMasterData;
    this.rolelist = sessionStorage.getItem('Roles');
    this.billingtypeVariable = sessionStorage.getItem('billingType');
    this.rolelist = JSON.parse(this.rolelist);
    this.videoOnVariable = false;

    if (this.rolelist[0].Id == 1 || this.rolelist[0].Id == 3) {
      this.isManagerProvider = true;
    } else {
      this.isManagerProvider = false;
    }
    this.rpm.rpm_get(`/api/config/client`).then((data) => {
      console.log('Phone number extension');
      console.log(data);
      this.phoneExtension = data;
    });

    this.renewProgramForm.get('startdate')?.valueChanges.subscribe((val) => {
      if (this.renewProgramForm.controls.startdate.value) {
        this.calculateEndDate();
      }
    });
  }

  billingTableColumns: any;
  documentTableColumns: any;
  loading_two = false;
  today_date: any;
  PatientIdOld: any;
  message1: any;
  // New Change 13/07/2021
  callType: any;

  ngOnInit(): void {
    this.callType = null;

    this.twilioService.localVideo = this.localVideo;
    this.twilioService.remoteVideo = this.remoteVideo;
    this.twilioService.disconnectCallOnParticipantDisconnect(
      this.disconnect.bind(this)
    );
    this.ProgramHistory = [];
    this.CurrentProgramSelected = undefined;
    this.unlockAccountStatus = false;
    this.establishedValue = undefined;

    this.today_date = this.datepipe.transform(new Date(), 'yyyy-MM-dd');

    //this.loading_two = true;
    this.heath_trends_frequency = 30;
    this.billingTableColumns = [
      'CPTCode',
      'Last_Billing_Cycle',
      'reading',
      'status',
    ];
    this.documentTableColumns = [
      'DocumentType',
      'DocumentName',
      'CreatedOn',
      'DocumentUNC',
    ];
    this.screenHeight = screen.height;

    this.loading = true;
    //  check Ashwin

    // When initial Load Get Data from Query Parameter

    this._route.queryParams
      .pipe(takeUntil(this.destroy$))
      .subscribe((params) => {
        this.handleQueryParams(params);
        this.patientchatservice.setChatPanelOpen(this.chatVariable);
        this.patientchatservice.ensureInitialized(params.patientUserName);
      });

    this.initializedocumentColumns();

    this.defaultScreen(this.vital_menu_item[0].vital_name);
    this.getCallTokenMethod();
    // Only subscribe to the unread count observable Twilio Chat
    this.unreadSubscription = this.patientchatservice.unreadCount$.subscribe(
      (count) => {
        this.unreadCount = count;
      }
    );

    this.chatHistoryDataSubscription = this.patientchatservice.chatHistoryData$.subscribe(data => {
      if (data) {
        this.http_chatData = data;
        this.dataSourceChange(5, 5);
      }
    });
  }

  getCallTokenMethod() {
    this.rpm.rpm_get('/api/comm/CallToken').then((data: any) => {
      this.data_json = data;
      // this.device = Twilio.Device.setup(this.data_json.token, {
      //   codecPreferences: ['opus', 'pcmu'],
      //   fakeLocalDTMF: true,
      //   enableRingingState: true,
      //   debug: true,
      // });

      const deviceOptions = {
       edge: 'umatilla',
      //codecPreferences: ['opus', 'pcmu'],
       fakeLocalDTMF: true,
      enableRingingState: true,
       debug: true,
      };

    this.device = new Device( this.data_json.token, deviceOptions);


// Don't forget to register in v2.x
      this.device.register();
      this.setupHandlers(this.device);
    });
  }
  /**
    Extracted Function to Handle Query Params
   */
  private handleQueryParams(params: any): void {
    const patientId = params.id;
    const programId = params.programId;

    // Store previous Patient ID
    this.PatientIdOld = sessionStorage.getItem('PatientId');

    // Clear any existing timers/intervals
    this.clearIntervals();

    // Reset patient-related variables
    this.progrss_billing_array = [];
    this.patientInteractionMin = '00';
    this.patientInteractionSec = '00';

    // Assign patient & program IDs
    this.patient_id = patientId;
    this.program_id = programId;
    this.currentpPatientId = patientId;

    // Store values in sessionStorage (only if changed)
    sessionStorage.setItem('PatientId', patientId);
    sessionStorage.setItem('ProgramId', programId);

    // Fetch Data
    this.loadPatientInfo();
    this.getProgramHistory();
    this.getPatientCriticalAlerts(patientId);
    this.calculateUpcomingSchedule(patientId);
    this.getLatestAlertsAndTasks();
    this.getBillingOverview(patientId, programId);
    this.getPatientDetails();
    this.getActivityMenuLoadData();
    this.getClinicMenuLoadData();
    this.getUserAccountLockData();

    // UI Update
    this.ChangeScreen(1);

    // Ensure download status is reset
    this.DownloadStatus = false;

    // Fetch Health Trends
    this.getHealthTrends(this.heath_trends_frequency);
  }

  async loadPatientInfo() {
    // const cachedData = this.patientService.getCachedPatientData();

    // if (cachedData) {
    //   this.http_rpm_patientList = cachedData;
    //   this.loading = false;
    // } else {
    try {
      this.http_rpm_patientList = await this.patientService.fetchPatientInfo(
        this.patient_id,
        this.program_id
      );
      this.getchatData(this.http_rpm_patientList.PatientDetails.UserName)

      this.setPatientData();
      this.loading = false;
    } catch (error) {
      console.error('Error loading patient info:', error);
    }
    //}
  }
  private setPatientData() {
    this.processPatientHeight();
    this.processPatientTimezone();
    this.storePatientStatus();
    this.fetchStatesAndCities();
    this.initializeTimers();
    this.fetchProgramDetails();
    this.initializeSidePanel();

  }

  /**
    Extracted Function: Process Patient Height
   */
  private processPatientHeight() {
    this.http_rpm_patientList.PatientDetails.Height =
      this.patientutilService.getPatientHeight(
        this.http_rpm_patientList.PatientDetails.Height
      );
  }

  /**
    Extracted Function: Process Patient Timezone
   */
  private processPatientTimezone() {
    this.ptime = this.patientTimeZone();
  }

  /**
    Extracted Function: Store Patient Status in Session
   */
  private storePatientStatus() {
    const patientStatus =
      this.http_rpm_patientList['PatientProgramdetails'].Status;
    this.patientStatusData = patientStatus;
    sessionStorage.setItem('patientStatusData', patientStatus);
  }

  /**
    Extracted Function: Fetch and Process States & Cities
   */
  private fetchStatesAndCities() {
    const statesCitiesData = sessionStorage.getItem('states_cities');
    this.StatesAndCities = statesCitiesData
      ? JSON.parse(statesCitiesData)
      : null;

    if (this.StatesAndCities) {
      this.patient_city = this.StatesAndCities.Cities.find(
        (city: { CityId: any }) =>
          city.CityId === this.http_rpm_patientList.PatientDetails.CityId
      );

      this.timezone = this.StatesAndCities.TimeZones.find(
        (tz: { Id: any }) =>
          tz.Id === this.http_rpm_patientList.PatientDetails.TimeZoneID
      );
    }
  }

  /**
    Extracted Function: Initialize Timers & Page Refresh Logic
   */
  private initializeTimers() {
    this.loading = false;
    this.callTimerEnabled = false;
    this.clearExistingIntervals();

    this.startRealTimeClock();
    this.startPageTimers();
    this.startPatientTimeZoneUpdater();
  }

  /**
    Extracted Function: Clear Existing Intervals (Prevent Memory Leaks)
   */
  private clearExistingIntervals() {
    clearInterval(this.interval);
    clearInterval(this.interval1);
    clearInterval(this.interval2);
  }

  /**
   Extracted Function: Start Real-Time Clock
   */
  private startRealTimeClock() {
    this.interval1 = setInterval(() => {
      const now = new Date();
      this.CurrentTime = `${now.getHours()}:${now.getMinutes()}:${now.getSeconds()}`;
    }, 1000);
  }

  /**
    Extracted Function: Start Page Timer Logic
   */
  private startPageTimers() {
    this.PageTimerBeforeRenew = sessionStorage.getItem('PageTimerBeforeRenew');

    if (this.PageTimerBeforeRenew) {
      this.timerValue = this.convertTimeToSec(this.PageTimerBeforeRenew);
      this.startIntervalTimer();
      sessionStorage.removeItem('PageTimerBeforeRenew');
    } else {
      this.startIntervalTimer();
    }
  }

  /**
   Extracted Function: Start Interval Timer (Handles Page Timer & Call Timer)
   */
  private startIntervalTimer() {
    this.interval = setInterval(() => {
      if (!this.callTimerEnabled) {
        this.timerValue++;
        this.PageTimer = this.timeConvert(this.timerValue);
        sessionStorage.setItem('PageTimer', this.PageTimer);

        if (this.timerValue === 86400) {
          clearInterval(this.interval);
        }
      }

      if (this.callTimerEnabled && !this.editTimerenabled) {
        this.calltimerValue++;
        this.calltimer = this.timeConvert(this.calltimerValue);
        sessionStorage.setItem('calltimer', this.calltimer);

        if (this.calltimerValue === 86400) {
          clearInterval(this.interval);
        }
      }
    }, 1000);
  }

  /**
    Extracted Function: Start Timezone Updater (Refreshes Patient Timezone)
   */
  private startPatientTimeZoneUpdater() {
    this.interval2 = setInterval(() => {
      this.ptime = this.patientTimeZone();
    }, 60000);
  }

  /**
   Extracted Function: Fetch & Process Program Details
   */
  private fetchProgramDetails() {
    this.loading3 = false;
    const programDetails = this.http_rpm_patientList['PatientProgramdetails'];

    this.programStart = this.datepipe.transform(
      this.convertToLocalTime(programDetails.StartDate),
      'MMM/dd/yyyy'
    );

    this.program_endTime = this.datepipe.transform(
      programDetails.EndDate,
      'MMM/dd/yyyy'
    );

    this.patiantVital = (programDetails.PatientVitalInfos || []).filter(
      (ds: { Selected: boolean }) => ds.Selected === true
    );
    console.log('ProgramDetails vital info');
    console.log(programDetails.PatientVitalInfos);
    console.log('Patient Vitals:');
    console.log(this.patiantVital);
    this.vitals =
      this.http_rpm_patientList.PatientProgramdetails.PatientVitalInfos;
    this.processAdditionalPatientData(programDetails);
  }

  /**
    Extracted Function: Process Additional Patient Data
   */
  private processAdditionalPatientData(programDetails: any) {
    this.setPatientClinicAndProgramDetails();
    this.getNoteIdArray(programDetails.ProgramName);
    this.getMasterDataQuestions(programDetails.ProgramName);
    this.patientProgramname = programDetails.ProgramName;
    this.getBillingInfoSrc();
    this.getBillingData();

    this.onHoldDate = this.convertToLocalTime(
      this.http_rpm_patientList.OnHoldPatientDetais.AssignedDate
    );
  }

  /**
    Extracted Function: Update Sidebar (If Available)
   */
  private initializeSidePanel() {
    if (this.patientrightsidebar) {
      this.patientrightsidebar.updatePatientName();
      this.patientrightsidebar.refresh();
    }
  }

  private setPatientClinicAndProgramDetails() {
    const patientDetails = this.http_rpm_patientList.PatientDetails;
    const programDetails = this.http_rpm_patientList.PatientProgramdetails;

    // Get Clinic Details Using Service
    const clinic = this.patientService.getClinicDetails(
      this.master_data,
      patientDetails.OrganizationID
    );
    if (clinic) {
      this.clinicName = clinic.ClinicName;
      this.clinicCode = clinic.ClinicCode;
    }

    // Assign State
    this.state = patientDetails.State;

    // Get City Name Using Service
    this.city = this.patientService.getCityName(
      this.StatesAndCities,
      patientDetails.CityId
    );

    // Get Program Name Using Service
    this.program_name = this.patientService.getProgramName(
      this.programDetails,
      programDetails.ProgramId
    );
  }
  tz: any;
  patientTimeZone() {
    var UTCDifference = this.http_rpm_patientList.PatientDetails.UTCDifference;
    var utlcval = '00:00';
    if (UTCDifference == -4.0) {
      utlcval = '-04:00';
      this.tz = 'EDT';
    } else if (UTCDifference == -5.0) {
      utlcval = '-05:00';
      this.tz = 'CDT';
    } else if (UTCDifference == -6.0) {
      utlcval = '-06:00';
      this.tz = 'MDT';
    } else if (UTCDifference == -7.0) {
      utlcval = '-07:00';
      this.tz = 'PDT';
    } else if (UTCDifference == -8.0) {
      utlcval = '-08:00';
      this.tz = 'AKDT';
    } else if (UTCDifference == -9.0) {
      utlcval = '-09:00';
      this.tz = 'HDT';
    } else if (UTCDifference == -10.0) {
      utlcval = '-10:00';
      this.tz = 'HST';
    }

   const res4 = dayjs().utcOffset(utlcval).format('hh:mm A');
    return res4;
  }
  // Vital Reading Template Start

  displayedBPColumn: string[] = [
    'ReadingDate',
    'ReadingTime',
    'Systolic',
    'Diastolic',
    'pulse',
    'Remarks',
    'Status',
  ];

  displayedGlucoseColumn: string[] = [
    'ReadingDate',
    'ReadingTime',
    'Schedule',
    'BGmgdl',
    'Remarks',
    'Status',
  ];

  bgColorGlucose: any;

  // Vital Reading  Template Ends

  //  Medication Template Start

  displayedMedicationColumns = [
    'selection',
    'Id',
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
    'actions',
  ];

  IntervalArray = ['Weekly', 'Daily', 'Alernative'];
  IntervalTime = ['After Meal', 'Before Meal'];

  medicationId: any;
  getMedicationData(data: any) {
    var patientStatus = this.http_rpm_patientList['PatientDetails'].Status;

    this.patientrightsidebar.getMedicationUpdateValue(data, patientStatus);
  }
  endDate: any;
  currentpPatientId: any;
  currentProgramId: any;

  // Medication Template End

  // Symtoms Template Data Start
  symtomsTableColumn = [
    'selection',
    'Id',
    'Symptom',
    'Description',
    'SymptomStartDateTime',
    'time',
    'action',
  ];

  //  AutoFill Symptom End

  //  Get Symptom Data  For Edit Start
  symptomId: any;
  editSymptoms(data: any) {
    var patientStatus = this.http_rpm_patientList['PatientDetails'].Status;

    this.symptom_update_variable = true;
    this.patientrightsidebar.getSymptomUpdateValue(data, patientStatus);
  }
  //  Get Symptom Data  For Edit End

  // Update Symptom Data Start

  // Update Symptom Data End
  noteid: any;
  rolelist: any;
  isManagerProvider = false;

  OpenUpdateNotes(element: any) {
    var patientStatus = this.http_rpm_patientList['PatientDetails'].Status;

    this.patientrightsidebar.getReviewNoteUpdation(element, patientStatus);
  }

  OnCancelNotes() {
    if (this.EditcallNotes) {
      this.callTimerEnabled = false;
      this.EditcallNotes = false;
      this.OnCancelNotesTimer();
      this.resetCallPanel();
      this.resetCallTimer();
    } else {
      this.OnCancelNotesTimer();
    }

    this.EditcallNotes = false;

    if (this.callVariable) {
      this.callVariable = false;
      this.callTimerEnabled = false;
    } else {
      // this.notes_update_panel = false;
      this.callTimerEnabled = false;
    }
    this.incomingVariableDisable = false;
    this.incomingCallVal = false;
    this.resetCallNotes();
    this.resetCallTimer();
    this.stopCallEditTimer();
    this.callDisConnected();
    if(this.videoOnVariable == true)
    {
      this.disconnectVideo();
    }
    this.videoOnVariable = false;
    //this.disconnectVideo();
  }
  NoteData: any;
  NoteTypeId: any;
  noteTypeIdArray: any;
  NoteTime: any;
  NoteSource: any;
  vital_table = [
    {
      vital_id: 1,
      vital_name: 'Blood Pressure',
    },
    {
      vital_id: 2,
      vital_name: 'Blood Glucose',
    },
  ];
  notesObj = {
    VitalId: 0,
    Notes: '',
  };
  notes_update_panel = false;
  iscallestablished = false;
  Ispatient = false;
  callestablishvarible: any;
  isValidTime(str: any) {
    let exactMatch = new RegExp('([01]?[0-9]|2[0-3]):[0-5][0-9]');

    if (str.empty()) {
      return false;
    }

    if (str.match(exactMatch)) {
      return true;
    } else {
      return false;
    }
  }

  billingReload() {
    this.getBillingOverview(this.pid, this.currentProgramId);
    if (
      this.http_rpm_patientList['PatientProgramdetails'].Status == 'InActive'
    ) {
      this.loadPatientInfo();
    }
  }
  resetCallNotes() {
    this.NoteTypeId = 0;
  }

  calledittimer: any;
  editTimerenabled = false;
  startCallEditTimer() {
    this.callTimerEnabled = true;
    this.callNoteUpdateTimer = 0;
    this.editTimerenabled = true;
    this.interval3 = setInterval(() => {
      this.callNoteUpdateTimer = this.callNoteUpdateTimer + 1;

      this.calledittimerValue = this.calledittimerValue + 1;
      this.calledittimer = this.timeConvert(this.calledittimerValue);
      this.NoteTime = this.calledittimer;

      if (this.calltimerValue === 86400) {
        clearInterval(this.interval3);
      }
    }, 1000);
  }
  stopCallEditTimer() {
    this.callTimerEnabled = false;
    this.editTimerenabled = false;
    clearInterval(this.interval3);
  }

  convertTimeToSec(time: any) {
    var split = time.split(':');
    var result =
      parseInt(split[0]) * 3600 + parseInt(split[1]) * 60 + parseInt(split[2]);
    return result;
  }

  convertDropdownTimeToSec() {
    var result =
      parseInt(this.NoteHours) * 3600 +
      parseInt(this.NoteMinutes) * 60 +
      parseInt(this.NoteSec);
    return result;
  }
  // convertSecToTime(seconds: any) {
  //   var hs = Math.trunc(seconds / 3600);
  //   var remh = Math.trunc(seconds % 3600);
  //   var min = Math.trunc(remh / 60);
  //   var sec = Math.trunc(remh % 60);
  //   var rsec = '';
  //   var rmin = '';
  //   var rhs = '';
  //   if (hs < 10) {
  //     rhs = '0' + hs.toString();
  //   } else {
  //     rhs = hs.toString();
  //   }
  //   if (sec < 10) {
  //     rsec = '0' + sec.toString();
  //   } else {
  //     rsec = sec.toString();
  //   }
  //   if (min < 10) {
  //     rmin = '0' + min.toString();
  //   } else {
  //     rmin = min.toString();
  //   }
  //   var result = rhs + ':' + rmin + ':' + rsec;
  //   return result;
  // }
  onChange: (value: string) => void;
  filter(val: any) {
    return this.http_get_symptoms.filter((option: { Symptom: any }) =>
      option.Symptom.toLowerCase().includes(val.toString().toLowerCase())
    );
  }
  getOptionUserText(option: { Symptom: any }) {
    return option.Symptom;
  }
  selectSymtomRow($event: any, row: any) {
    $event.preventDefault();
    if (!row.selected) {
      this.dataSourceTable.filteredData.forEach(
        (row: { selected: boolean }) => (row.selected = false)
      );
      row.selected = true;
      if (row.selected) {
        this.someVar = true;
      }
    }
  }

  // Symtom Template Data Ends

  // Activity-Review Template Start

  ActivityTaskColumnHeader = [
    'Id',
    'Type',
    'TaskOrAlert',
    'Priority',
    'CreatedBy',
    'AssignedMember',
    'DueDate',
    'Status',
    'action',
  ];

  // Activity-Review Template Ends

  // Activity-Review Template Start

  CallNotesColumnHeader = [
    'Id',
    'NoteType',
    'Date',
    'Time',
    'CompletedBy',
    'Duration',
    'CallType',
    'Notes',
  ];

  // Activity-Review Template Ends

  // Activity-Review Template Start

  ReviewColumnHeader = [
    'Id',
    'NoteType',
    'Date',
    'Time',
    'CompletedBy',
    'Duration',
    'Notes',
  ];

  // Activity-Review Template Ends

  // Activity schedule Start

  // dateRangeScheduleChange(
  //   dateRangeStart: HTMLInputElement,
  //   dateRangeEnd: HTMLInputElement
  // ) {
  //   var startDate = this.convertDate(dateRangeStart.value);
  //   var endDate = this.convertDate(dateRangeEnd.value);

  //   var that = this;
  //   if (dateRangeEnd.value != null && dateRangeStart.value != null) {
  //     that.rpm
  //       .rpm_get(
  //         `/api/patient/getpatientschedules?PatientId=${this.patient_id}&StartDate=${startDate}&EndDate=${endDate}`
  //       )
  //       .then((data) => {
  //         that.http_ActivityScheduleData = data;
  //       });
  //   }
  // }

  // Activity Schedule Ends

  progrss_billing_array: any;

  setupHandlers(device: any) {
    device.on('ready', (_device: any) => {
      this.updateCallStatus('Ready to call');
    });
    device.on('error', (error: any) => {
      this.updateCallStatus('ERROR: ' + error.message);
    });
    device.on('connect', (connection: any) => {
      if ('phoneNumber' in connection.message) {
        this.updateCallStatus('In call with ' + connection.message.phoneNumber);
        this.callHideShow = false;
        this.stopHideShow = true;
      } else {
        this.updateCallStatus('In call with support');
      }
    });
    device.on('disconnect', (connection: any) => {
      this.updateCallStatus('Call End');
      this.stopHideShow = false;
      this.callHideShow = true;
      this.callDisConnected();
    });
    this.device = device;
  }
  updateCallStatus(status: string): void {
    console.log("Status -> "+status);
  }
  callCustomer() {
    if (!this.phoneExtension) {
      this.phoneExtension.CountryCode = '+1';
    }
    this.phoneNumber = this.phoneExtension.CountryCode + this.phoneno;
    this.callTimerEnabled = true;
    this.updateCallStatus('Calling ' + this.phoneNumber + '...');

    var params = { To: this.phoneNumber };
    this.device.connect({ params });
  }
  Stop() {
    this.device.disconnectAll();
    this.stopHideShow = false;
    this.callHideShow = true;
  }

  // Messaging MatDialog

  openDialogWithTemplateRef() {
    if (
      this.http_rpm_patientList &&
      this.http_rpm_patientList['PatientProgramdetails'].Status != 'Discharged'
    ) {
      //this.dialog.open(templateRef);
      this.smsdialogpanel = true;
    }
  }
  smsCancel()
  {
    this.smsdialogpanel = false;
  }

  loading_sms = false;
  sentSMS(messsage: any) {
    this.loading_sms = true;
    if (!this.phoneExtension) {
      this.phoneExtension.CountryCode = '+1';
    }
    const phoneNumber =
      this.phoneExtension.CountryCode +
      this.http_rpm_patientList.PatientDetails.MobileNo;
    var patientusername = this.http_rpm_patientList.PatientDetails.UserName;

    const req_body = {
      PatientUserName: patientusername,
      toPhoneNo: phoneNumber,
      Message: messsage,
    };
    if (!messsage.trim().length) {
      alert('Message Content Should Not be Empty');
      this.loading_sms = false;
      return;
    }

    // var that = this
    this.rpm.rpm_post('/api/comm/SMSService', req_body).then(
      (data) => {
        //show patient id created
        // this.dialog.closeAll();
        this.smsCancel();  // close the panel
        alert('Message Sent Successfully..!');
        this.loading_sms = false;
        this.getSMSData();
      },
      (err) => {
        // this.dialog.closeAll();
        this.smsCancel(); // close the panel
        if(err.status == 400)
        {
          alert(err.error.message);
        }else{
          alert('SMS Sent Failed')
        }
        console.log(err.error.message);
        this.loading_sms = false;
      }
    );
  }

  display() {
    if (this.variable == 1 && this.isSmall) {
      this.smallScreenVariable = true;
    } else if (this.variable != 1 && this.isSmall) {
      this.smallScreenVariable = false;
    } else if (!this.isSmall) {
      this.smallScreenVariable = true;
    }
  }
  isSmall: boolean;

  ngAfterViewInit() {
    this.breakpoint
      .observe(['(max-width: 1280px)'])
      .subscribe((bs: BreakpointState) => {
        if (bs.matches) {
          this.isSmall = true;
          this.display();
        }
      });

    this.breakpoint
      .observe(['(min-width: 1280px)'])
      .subscribe((bs: BreakpointState) => {
        if (bs.matches) {
          // this.smallScreenVariable = true;
          this.isSmall = false;
          this.display();
        }
      });
  }

  screenHeight: any;
  PubSubConnect(socket: any) {
    var subject = webSocket(socket);
    subject.subscribe(
      (msg) => {
        console.log(msg);
      },
      (err) => {
        console.log(err);
      },
      () => console.log('complete')
    );
  }

  // activeProgram = false;
  MasterDataQuestionTemp: any;
  getMasterDataQuestions(patientProgramName: any) {
    this.patientProgramName =
      this.http_rpm_patientList['PatientProgramdetails'].ProgramName;

    this.rpm
      .rpm_get(
        `/api/patient/getmasterdatanotes?ProgramName=${patientProgramName}&Type=CALL`
      )
      .then(
        (data) => {
          if (data) {
            this.noteMasterData = data;
            this.MasterDataQuestionTemp = this.noteMasterData.MainQuestions;
            sessionStorage.setItem(
              'MasterDataQuestionTemp',
              JSON.stringify(this.MasterDataQuestionTemp)
            );

            this.QuestionArrayBase = this.noteMasterData.MainQuestions;

            this.loading_two = false;
          }
        },
        (err) => {
          this.noteMasterData = [];
          this.loading_two = false;
        }
      );
  }
  getNoteIdArray(patientProgramName: any) {
    this.http_getId = sessionStorage.getItem('operational_masterdata');
    this.http_getId = JSON.parse(this.http_getId);
    var that = this;
    if (patientProgramName == 'RPM') {
      that.noteTypeIdArray = that.http_getId.filter(function (data: any) {
        return data.Type == 'PatientNoteType';
      });
    } else if (patientProgramName == 'CCM') {
      that.noteTypeIdArray = that.http_getId.filter(function (data: any) {
        return data.Type == 'CCMNotes';
      });
    } else if (patientProgramName == 'PCM') {
      that.noteTypeIdArray = that.http_getId.filter(function (data: any) {
        return data.Type == 'PCMNotes';
      });
    }
  }

  getMedicationReload() {
    this.ChangeScreen(4);
    this.getMedicationreloadData();
    this.ClinicalInfoMenuSelect(3);
  }

  getSymptomsReloadData() {
    this.ChangeScreen(4);
    this.getSymptomsData();
    this.ClinicalInfoMenuSelect(4);
  }

  async getSymptomsData() {
    this.loading = true;

    // Get session data
    this.currentpPatientId = sessionStorage.getItem('PatientId');
    this.currentProgramId = sessionStorage.getItem('ProgramId');

    if (!this.currentpPatientId || !this.currentProgramId) {
      this.loading = false;
      return;
    }

    try {
      const data = await this.patientService.getSymptomsData(
        this.currentpPatientId,
        this.currentProgramId
      );
      this.http_get_symptoms = data;
      this.temp_symptoms = data;

      // Extract critical alerts from symptoms
      this.PatientCriticalAlerts = data
        .filter((entry: any) => entry.Symptom === 'Critical Event')
        .map((entry: any) => ({
          Priority: 'Critical',
          VitalAlert: entry.Description,
          Time: entry.SymptomStartDateTime,
        }));

      this.dataSourceChange(4, 4);
    } catch (error) {
      console.error(' Error:', error);
    } finally {
      this.loading = false;
    }
  }
  getMedicationreloadData() {
    this.loading = true;
    this.currentpPatientId = sessionStorage.getItem('PatientId');
    this.currentProgramId = sessionStorage.getItem('ProgramId');
    this.patientService
      .getPatientMedication(this.currentpPatientId, this.currentProgramId)
      .then((data) => {
        this.http_medication_data = data;
        this.temp_medication = data;
        this.loading = false;
        this.dataSourceChange(4, 3);
      })
      .catch((error) => {
        this.loading = false;

        console.error('Error fetching medication:', error);
      });
  }

  getReviewDataReload() {
    this.ChangeScreen(5);
    this.getreviewData();
  }

  getTaskAlertDataReload() {
    this.ChangeScreen(5);
    this.getalertTask();
  }
 getOpenTaskDataReload() {

   this.getLatestAlertsAndTasks();
  }

  async getreviewData() {
    try {
      this.activityInfoMenuSelect(4);

      this.currentpPatientId = sessionStorage.getItem('PatientId') || '';
      this.currentProgramId = sessionStorage.getItem('ProgramId') || '';

      const { start, end } = this.patientService.getDateRange(
        this.frmactivityReviewrange.controls.start.value,
        this.frmactivityReviewrange.controls.end.value,
        this.convertDate.bind(this)
      );
      this.frmactivityReviewrange.controls['start'].setValue(start);
      this.frmactivityReviewrange.controls['end'].setValue(end);

      this.http_reviewNotesData =
        await this.patientService.getPatientReviewNotes(
          this.currentpPatientId,
          this.currentProgramId,
          start,
          end
        );

      this.dataSourceChange(5, 4);
    } catch (error) {
      console.error('❌ Error fetching alerts and tasks:', error);
    }
  }

  async getchatData(patientusername: any) {
    try {
      // Use patient username from the parameter or from patient details
      var username = patientusername || this.http_rpm_patientList.PatientDetails.UserName;

      // Call the activity info method (kept from your original code)
      this.activityInfoMenuSelect(5);

      // Get chat data through the service
      this.http_chatData = await this.patientchatservice.getPatientChat(username);

      // Call data source change method (kept from your original code)
      this.dataSourceChange(5, 5);
    } catch (error:any) {
      if(error.status == 404)
      {
        return;
      }else{
      console.error('❌ Error fetching chat data:', error);

      }
    }
  }
  async getCallNotes() {
    try {
      this.activityInfoMenuSelect(3);

      this.currentpPatientId = sessionStorage.getItem('PatientId') || '';
      this.currentProgramId = sessionStorage.getItem('ProgramId') || '';

      const { start, end } = this.patientService.getDateRange(
        this.frmactivityCallrange.controls.start.value,
        this.frmactivityCallrange.controls.end.value,
        this.convertDate.bind(this)
      );
      this.frmactivityCallrange.controls['start'].setValue(start);
      this.frmactivityCallrange.controls['end'].setValue(end);

      this.http_CallNotesData = await this.patientService.getPatientCallNotes(
        this.currentpPatientId,
        this.currentProgramId,
        start,
        end
      );

      this.dataSourceChange(5, 3);
    } catch (error) {
      console.error('❌ Error fetching alerts and tasks:', error);
    }
  }

  getReviewReload() {
    this.ChangeScreen(5);
    this.getreviewData();
  }

  getScheduleDataReload() {
    this.ChangeScreen(5);
    this.getScheduleData();
  }

  getPatientDetails() {
    this.getScheduleData();

    this.getalertTask();

    this.getreviewData();
    this.getCallNotes();
    this.getSMSData();
    // Get Activity Schedule Ends
    // Get Activity Schedule Start

    // Get Activity Schedule Ends
  }

  getActivityMenuLoadData() {
    this.getalertTask();
    this.getreviewData();
    this.getCallNotes();
    this.getScheduleData();
    this.getSMSData();

  }

  getClinicMenuLoadData() {
    this.getMedicationreloadData();
    this.getVitalReading();
    this.getSymptomsData();
  }

  http_VitalOxygen: any;
  http_VitalWight: any;
  http_7day_vitalData: any;
  SampleVitalData: any;

  async getVitalReading(startDate?: string, endDate?: string) {
    try {
      this.loading = true; // Show loading indicator

      if (startDate == undefined && endDate == undefined) {
        var start = this.convertDate(this.ThirtyDaysAgo);
        var end = this.convertDate(this.Today);
        start = start + 'T00:00:00';
        end = end + 'T23:59:59';
        var startRange = this.auth.ConvertToUTCRangeInput(new Date(start));
        var endRange = this.auth.ConvertToUTCRangeInput(new Date(end));
        this.frmvitalrange.controls['start'].setValue(startRange);
        this.frmvitalrange.controls['end'].setValue(endRange);
      }
      const patientId = sessionStorage.getItem('PatientId');
      const programId = sessionStorage.getItem('ProgramId');

      if (patientId != null && programId != null) {
        const result = await this.patientService.getVitalReading(
          patientId,
          programId,
          startDate,
          endDate
        );
        if (result) {
          this.http_vitalData = result.vitalData;
          this.vital_temporaryData = result.processedVitals.BloodPressure;
          this.http_vitalGlucoseData = result.processedVitals.BloodGlucose;
          this.http_VitalOxygen = result.processedVitals.BloodOxygen;
          this.http_VitalWight = result.processedVitals.Weight;

          this.selectFirstAvailableVital();
        }
      }
    } catch (error) {
      console.error('Error processing vital data:', error);
    } finally {
      this.loading = false;
    }
  }
  frmvitalrange = new FormGroup({
    start: new FormControl(),
    end: new FormControl(),
  });
  async rangeVitalScheduleCalc() {
    const startDate = this.frmvitalrange.controls['start'].value;
    const endDate = this.frmvitalrange.controls['end'].value;

    await this.getVitalReading(startDate, endDate);
  }

  selectFirstAvailableVital() {
    const vitalScreens = {
      BloodGlucose: this.http_vitalGlucoseData,
      BloodPressure: this.vital_temporaryData,
      Oxygen: this.http_VitalOxygen,
      Weight: this.http_VitalWight,
    };

    for (const [vital, data] of Object.entries(vitalScreens)) {
      if (data && data.length > 0) {
        this.ChangeVitalScreen(vital);
        break;
      }
    }
  }
  currentVital: any;
  ChangeVitalScreen(button: any) {
    this.currentVital = button;
  }

  // Get SMS Data for 30 Days For Report Generation

  http_ActivitySMSData: any;
  http_ActivitySMSMonthlyData: any;
  async getSMSData(useCustomRange = false, startDate?: Date, endDate?: Date) {
    try {
      let formattedStartDate: string;
      let formattedEndDate: string;

      if (!useCustomRange) {
        try {
          // Get current month dates
          const { start: monthStart, end: monthEnd } =
            this.patientutilService.getCurrentMonthDates();

          formattedStartDate = this.patientutilService.formatDateForApi(
            monthStart,
            true
          );
          formattedEndDate = this.patientutilService.formatDateForApi(
            monthEnd,
            false
          );

          // Update form controls
          this.frmactivitysmsrange.controls['start'].setValue(
            formattedStartDate
          );
          this.frmactivitysmsrange.controls['end'].setValue(formattedEndDate);
        } catch (error) {
          console.error('Error setting up default date range:', error);
          throw new Error('Failed to set up default date range');
        }
      } else {
        try {

          if (!startDate || !endDate) {

            startDate = new Date(this.frmactivitysmsrange.controls.start.value);
            endDate = new Date(this.frmactivitysmsrange.controls.end.value);
          }


          formattedStartDate = this.patientutilService.formatDateForApi(
            startDate,
            true
          );
          formattedEndDate = this.patientutilService.formatDateForApi(
            endDate,
            false
          );
        } catch (error) {
          console.error('Error with custom date range:', error);
          throw new Error('Failed to process custom date range');
        }
      }
      const patientId = sessionStorage.getItem('PatientId');
      const programId = sessionStorage.getItem('ProgramId');
      if (!patientId) {
        throw new Error('No patient ID available');
      }
      if (!programId) {
        throw new Error('No patient ID available');
      }
      // Fetch data using the service
      try {
        const data = await this.patientService.getPatientSMSData(
          patientId,
          programId,
          formattedStartDate,
          formattedEndDate
        );
        this.http_ActivitySMSData = data;
        this.http_ActivitySMSMonthlyData = data;
        this.dataSourceChange(5, 6);
      } catch (error) {
        console.error('Error fetching schedule data:', error);

        this.http_ActivitySMSData = [];
        throw new Error('Failed to fetch schedule data');
      }
    } catch (error) {
      console.error('Error in getScheduleData:', error);
    }
  }


  displayedSMSColumns = ['Date', 'Time', 'Body', 'Sender'];

  async getalertTask() {
    try {
      this.activityInfoMenuSelect(2);
      this.selectedStatusValue = undefined;

      this.currentpPatientId = sessionStorage.getItem('PatientId') || '';
      this.currentProgramId = sessionStorage.getItem('ProgramId') || '';

      const { start, end } = this.patientService.getDateRange(
        this.frmactivityAlertrange.controls.start.value,
        this.frmactivityAlertrange.controls.end.value,
        this.convertDate.bind(this)
      );
      this.frmactivityAlertrange.controls['start'].setValue(start);
      this.frmactivityAlertrange.controls['end'].setValue(end);

      this.http_ActivityAlertTask = await this.patientService.getAlertTask(
        this.currentpPatientId,
        this.currentProgramId,
        start,
        end
      );

      this.http_ActivityAlertTaskTemp = [...this.http_ActivityAlertTask];
      this.dataSourceChange(5, 2);

      this.priorityTaskFilterValue = undefined;
    } catch (error) {
      console.error('❌ Error fetching alerts and tasks:', error);
    }
  }



  PatientCriticalAlerts: any;
  getPatientCriticalAlerts(patient_id: any) {
    // this.rpm.rpm_get(`/api/patient/getpatientcriticalalerts?PatientId=${patient_id}`).then((data) => {
    //     this.PatientCriticalAlerts = data;
    // });
    // this.PatientCriticalAlerts=[]
    // this.currentpPatientId  = sessionStorage.getItem('PatientId');
    // this.currentProgramId =  sessionStorage.getItem('ProgramId');
    // var that=this;
    // that.rpm.rpm_get(`/api/patient/getpatientsymptoms?PatientId=${this.currentpPatientId}&PatientProgramId=${this.currentProgramId}`).then((data) => {
    //   that.http_get_symptoms = data;
    //   //new change for critical events
    //   var tempEvents =this.http_get_symptoms.filter((data: { Symptom: string; })=>{
    //     return data.Symptom=='Critical Event'
    //   })
    //   for(let dataEvents of tempEvents){
    //     var objEvents = {
    //       "Priority":"Critical",
    //       "VitalAlert":dataEvents.Description,
    //       "Time":dataEvents.SymptomStartDateTime
    //     }
    //     this.PatientCriticalAlerts.push(objEvents)
    //   }
    // });
  }
  phoneno: any;

  onProgramHstoryChange(programId: any) {
    sessionStorage.setItem('PageTimerBeforeRenew', this.PageTimer);
    let route = '/admin/patients_detail';
    this.router.navigate([route], {
      queryParams: { id: this.patient_id, programId: programId },
      skipLocationChange: true,
    });
    this.CurrentProgramSelected = programId;
  }
  patientProgramname: any;
  // currentPhysician: any;
  calltimer: any;
  callTimerEnabled: boolean;

  makeCallReady() {
    if (
      this.http_rpm_patientList &&
      this.http_rpm_patientList['PatientProgramdetails'].Status != 'Discharged'
    ) {
      this.OnpreviewUpdateCancel();
      this.resetCallPanel();
      this.resetCallTimer();
      // this.patientProgramName = sessionStorage.getItem('patientPgmName');
      this.patientProgramName =
        this.http_rpm_patientList['PatientProgramdetails'].ProgramName;
      this.getMasterDataQuestions(this.patientProgramName);
      this.callVariable = true;

      this.callType = 'AUDIO';
    }
  }

  incomingVariableDisable = false;
  makeCallConnected() {
    this.incomingVariableDisable = true;

    if (this.incomingCallVal == false) {
      if (!this.phoneno) {
        alert('Please Select a Phone Number.');
        return;
      }

      this.call_connected = !this.call_connected;
      if (this.call_connected) {
        this.callCustomer();
      } else {
        this.Stop();
      }
    }
  }

  callDisConnected() {
    this.call_connected = false;
    this.Stop();
  }

  onAddData() {
    if (this.NoteSource[0].VitalId == 0 && this.NoteSource[0].Notes == '') {
      return;
    }
    var notesObj = {
      VitalId: 0,
      Notes: '',
    };
    this.NoteSource.push(notesObj);
  }
  onClickCancel() {
    this.callVariable = false;

    this.callType = null;
  }
  onClickSave() {
    this.callVariable = false;

    this.callType = null;
  }
  chatVariable = false;

  chatClick() {
    this.chatVariable = !this.chatVariable;
  }
  menu = [
    {
      menu_id: 1,
      menu_title: 'Summary',
    },
    {
      menu_id: 2,
      menu_title: 'Patient Info',
    },
    {
      menu_id: 3,
      menu_title: 'Program Info',
    },
    {
      menu_id: 4,
      menu_title: 'Clinical Info',
    },
    {
      menu_id: 5,
      menu_title: 'Activity',
    },
  ];
  CurrentMenu: any = 1;
  // Change Main Screen
  testscreen(value: any) {
    alert(value);
  }
  ChangeScreen(button: any) {
    this.CurrentMenu = button;

    switch (button) {
      case 1:
        this.variable = 1;
        this.clinicalMenuVariable = 1;
        this.activityMenuVariable = 1;
        this.calculateUpcomingSchedule(this.patient_id)
        this.dataSourceChange(this.variable, 1);

        break;

      case 2:
        this.variable = 2;
        this.clinicalMenuVariable = 1;
        this.activityMenuVariable = 1;
        this.dataSourceChange(this.variable, 1);

        break;

      case 3:
        this.variable = 3;
        this.clinicalMenuVariable = 1;
        this.activityMenuVariable = 1;

        this.dataSourceChange(this.variable, 1);

        break;

      case 4:
        this.variable = 4;

        this.activityMenuVariable = 1;
        if (
          this.patientProgramname == 'CCM' ||
          this.patientProgramname == 'PCM'
        ) {
          this.clinicalMenuVariable = 3;
          this.dataSourceChange(this.variable, 3);
        } else {
          this.clinicalMenuVariable = 1;
          this.dataSourceChange(this.variable, 1);
        }

        this.clinicInfoMenu = this.getDisplyClinicalInfoMenu();

        break;

      case 5:
        this.variable = 5;
        this.clinicalMenuVariable = 1;
        this.activityMenuVariable = 1;
        this.activityInfoMenu = this.getDisplyActivityInfoMenu();

        this.dataSourceChange(this.variable, 1);
        break;

      case 6:
        this.variable = 6;
        this.clinicalMenuVariable = 1;
        this.activityMenuVariable = 1;
        this.activityInfoMenu = this.getDisplyActivityInfoMenu();

        this.dataSourceChange(this.variable, 1);
        break;
      default:
        this.variable = 1;

        break;
    }
    this.display();
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
      menu_title: 'Billing Info',
    },
    {
      menu_id: 5,
      menu_title: 'Documents',
    },
  ];

  // Summary
  critical_data = [
    {
      alert: 'No data',
      time: 'No data',
      priority: 'No data',
    },
  ];

  task = [
    {
      alert: 'No data',
      assigned: 'No data',
      priority: 'No data',
    },
  ];

  // Program Info
  billinginfo = [
    {
      cptcode: 99453,
      description: 'Patient Activation',
    },
    {
      cptcode: 99454,
      description: 'Remote Vitals Monitoring',
    },
    {
      cptcode: 99457,
      description: 'Patient Interaction',
    },
    {
      cptcode: 99458,
      description: 'Additional Patient Interaction',
    },
  ];

  billingCCM = [];

  billingSrcArray: any;

  getBillingInfoSrc() {
    if (this.patientProgramname != 'RPM') {
      switch (this.program_name) {
        case 'CCM-C':
          this.billingSrcArray = billingCCM_C;
          break;
        case 'CCM-P':
          this.billingSrcArray = billingCCM_P;
          break;
        case 'C-CCM':
          this.billingSrcArray = billingC_CCM;
          break;
        case 'PCM-P':
          this.billingSrcArray = billingPCM_C;
          break;
        case 'PCM-C':
          this.billingSrcArray = billingPCM_P;
          break;
      }
    } else {
      this.billingSrcArray = this.billinginfo;
    }
  }

  program_details = {};

  // Chart Summary Panel
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
  public lineChartOptions: any = {
    responsive: true,
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
    maintainAspectRatio: false,
    line: {
      tension: 0.5,
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

  minValue: number = 30;
  maxValue: number = 75;
  middle: number = 20;

  // options: Options = {
  //   floor: 0,
  //   step: 10,
  //   ceil: 100,
  //   showTicks: false,
  // };
  displayValue() {}

  //  Billing Table

  BillingInfo: any;

  async getBillingData() {
    try {
      // Validate required parameters
      if (!this.patient_id || !this.program_id) {
        throw new Error('Missing patient ID or program ID');
      }

      // Get billing data using service
      this.BillingInfo = await this.patientService.getPatientLastBilledCycle(
        this.patient_id,
        this.program_id,
        this.patientStatusData || ''
      );
    } catch (error) {
      console.error('Error in getBillingData:', error);
      this.BillingInfo = [];
    }
  }
  // Document Table
  //  Billing Table
  DocumentTableColumns: any;
  documentData: DocumentData[];
  initializedocumentColumns(): void {
    this.DocumentTableColumns = [
      {
        name: 'Document Type',
        logo: 'home',
        dataKey: 'DocumentType',
        position: 'left',
        isSortable: false,
      },
      {
        name: 'Document Name',
        logo: 'search',
        dataKey: 'DocumentName',
        position: 'right',
        isSortable: false,
      },
      {
        name: 'Update Date',
        logo: 'search',
        dataKey: 'CreatedOn',
        position: 'left',
        isSortable: true,
      },

      {
        name: 'Document',
        logo: 'search',
        dataKey: 'DocumentUNC',
        position: 'left',
        isSortable: true,
      },
    ];
  }
  getDocumentData(): any[] {
    return [
      {
        Document_type: 'ABCD',
        description: 'PatientName',
        update_date: 'Care Plan-OG',
        document: 'description',
      },
      {
        Document_type: 'ABCD',
        description: 'PatientName',
        update_date: 'Care Plan-OG',
        document: 'description',
      },
      {
        Document_type: 'ABCD',
        description: 'PatientName',
        update_date: 'Care Plan-OG',
        document: 'description',
      },
      {
        Document_type: 'ABCD',
        description: 'PatientName',
        update_date: 'Care Plan-OG',
        document: 'description',
      },
    ];
  }

  scroll(el: HTMLElement) {
    el.scrollIntoView();
    el.scrollIntoView({ behavior: 'smooth' });
  }

  stringToHTML(str: any) {
    var parser = new DOMParser();
    var doc = parser.parseFromString(str, 'text/html');
    return doc.body;
  }

  // Clinical Activity Table Vital Monitoring

  // Clinical Menu Item
  clinicInfoMenu = [
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
    // {
    //   menu_id:5,
    //   menu_title:'Uploads'
    // },
  ];
  clinicalMenuVariable = 1;

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
        this.dataSourceChange(this.variable, this.clinicalMenuVariable);
        break;
      case 4:
        this.clinicalMenuVariable = 4;
        this.dataSourceChange(this.variable, this.clinicalMenuVariable);
        break;
      case 5:
        this.clinicalMenuVariable = 5;
        break;
      default:
        this.clinicalMenuVariable = 1;
        break;
    }
    this.clinicalMenuVariable = menu;
  }

  test = true;

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

  // *************** Vital Reading End **********************************
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
    this.someVar = true;
  }

  clearSelection() {
    if (this.dataSourceTable) {
      this.dataSourceTable &&
        this.dataSourceTable.filteredData.forEach(
          (row: { selected: boolean }) => (row.selected = false)
        );
    } else {
      return;
    }
  }
  //  ***********************************************************//
  //  Symptoms Table

  displayedColumns = [
    'selection',
    'position',
    'name',
    'weight',
    'symbol',
    'index',
    'actions',
  ];
  displayedSymtomColumns = [
    'selection',
    'Symptom',
    'Description',
    'Date',
    'Time',
    'actions',
  ];
  displaychatColumns = [
    'ContactName',
    'LastMessage',
    'Date',
    'Time',
    'ConversationSid',
    'actions',
  ];
  dataSource = SymtomsData;
  selection: SelectionModel<Element> = new SelectionModel<Element>(false, []);
  someVar: any = false;

  selectRow($event: any, row: Symptoms) {
    $event.preventDefault();
    if (!row.selected) {
      this.dataSource.forEach((row) => (row.selected = false));
      row.selected = true;
    }
    this.someVar = true;
  }

  delete(row_id: any) {}
  edit(element: any) {}

  // Document Template
  clickDocument(id: any) {}

  // Activity Template:
  activityInfoMenu = [
    {
      menu_id: 1,
      menu_title: 'Schedules',
    },
    {
      menu_id: 2,
      menu_title: 'Alerts & Tasks',
    },
    {
      menu_id: 3,
      menu_title: 'Calls',
    },
    {
      menu_id: 4,
      menu_title: 'Reviews',
    },
    {
      menu_id:5,
      menu_title:'Chats'
    },
    {
      menu_id: 6,
      menu_title: 'SMS',
    },
    // {
    //   menu_id:7,
    //   menu_title:'Goal Status'
    // },ng
  ];

  CCMactivityInfoMenu = [
    {
      menu_id: 1,
      menu_title: 'Schedules',
    },
    {
      menu_id: 2,
      menu_title: 'Tasks',
    },
    {
      menu_id: 3,
      menu_title: 'Calls',
    },
    {
      menu_id: 4,
      menu_title: 'Reviews',
    },
    {
      menu_id:5,
      menu_title:'Chats'
    },
    {
      menu_id: 6,
      menu_title: 'SMS',
    },
    // {
    //   menu_id:7,
    //   menu_title:'Goal Status'
    // },
  ];

  activityMenuVariable = 1;

  activityInfoMenuSelect(menu: any) {
    this.activityMenuVariable = menu;
    switch (menu) {
      case 1:
        this.activityMenuVariable = 1;
        break;
      case 2:
        this.activityMenuVariable = 2;
        this.dataSourceChange(this.variable, this.activityMenuVariable);
        break;
      case 3:
        this.activityMenuVariable = 3;
        this.dataSourceChange(this.variable, this.activityMenuVariable);
        break;
      case 4:
        this.activityMenuVariable = 4;
        this.dataSourceChange(this.variable, this.activityMenuVariable);
        break;
      case 5:
        this.activityMenuVariable = 5;
        this.dataSourceChange(this.variable, this.activityMenuVariable);
        break;
      case 6:
        this.activityMenuVariable = 6;
        this.dataSourceChange(this.variable, this.activityMenuVariable);
        break;
      case 7:
        this.activityMenuVariable = 7;
        break;
      default:
        this.activityMenuVariable = 1;
        break;
    }
  }

  // Update Task And Alert
  getUpdateTaskAndAlertValue(data: any) {
    var patientStatus = this.http_rpm_patientList['PatientDetails'].Status;
    if (this.patientrightsidebar) {
      this.patientrightsidebar.getTaskAlertUpdation(data, patientStatus);
    } else {
      return;
    }
  }

  //  Time convert

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
  editpatient() {
    if (this.http_rpm_patientList['PatientDetails'].Status != 'Discharged') {
      let route = '/admin/editpatient';
      this.router.navigate([route], {
        queryParams: { id: this.patient_id, programId: this.program_id },
        skipLocationChange: true,
      });
    } else {
      alert('You Cannot Edit a Discharged Patient');
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

  // Data Table Source Change Function
  dataSourceChange(mainmenu: any, submenu: any) {
    if (mainmenu == 5) {
      if (submenu == 7) {
      } else if (submenu == 6) {
        this.getTableDataSource(this.http_ActivitySMSData);
      } else if (submenu == 5) {
        this.getTableDataSource(this.http_chatData);
      } else if (submenu == 4) {
        this.getTableDataSource(this.http_reviewNotesData);
      } else if (submenu == 3) {
        this.getTableDataSource(this.http_CallNotesData);
      } else if (submenu == 2) {
        this.getTableDataSource(this.http_ActivityAlertTaskTemp);
      } else {
      }
    } else if (mainmenu == 4) {
      this.clearSelection();

      if (submenu == 5) {

      } else if (submenu == 4) {
        this.getTableDataSource(this.http_get_symptoms);
      } else if (submenu == 3) {
        this.getTableDataSource(this.http_medication_data);
      } else if (submenu == 1) {
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

  openDialog(title: any, item: any) {
    const dialogConfig = new MatDialogConfig();
    dialogConfig.width = '400px';

    // dialogConfig.disableClose = true;
    dialogConfig.autoFocus = true;

    dialogConfig.data = {
      title: title,
      item: item,
    };

    this.dialog.open(StatusMessageComponent, dialogConfig);
  }

  http_healthtrends: any;
  Health_date_from: any;
  Health_date_to: any;
  healthtrenddisplay = true;

  async getHealthTrends(daycount: number) {
    try {
      this.http_healthtrends = await this.patientService.fetchHealthTrendInfo(
        this.patient_id,
        this.program_id,
        daycount,
        this.convertDate.bind(this),
        this.auth.ConvertToUTCRangeInput.bind(this.auth)
      );

      this.healthtrenddisplay = this.http_healthtrends?.VitalName != null;

      if (this.http_healthtrends?.Values?.length > 0) {
        this.lineChartLabels = this.patientService.convertDateforHealthTrends(
          this.http_healthtrends.Time,
          this.http_healthtrends
        );

        this.processChartData(daycount);
      } else {
        this.setGraphFallback(daycount);
      }
    } catch (error) {
      this.setGraphFallback(daycount);
      console.error('❌ Error:', error);
    } finally {
      this.loading = false;
    }
  }

  private processChartData(daycount: number) {
    const temp = [];
    let j = 0;

    for (const item of this.http_healthtrends.Values) {
      let i = 0;

      for (const x of item.data) {
        if (j === 0) {
          try {
            if (
              x == null &&
              i > 0 &&
              i < item.data.length &&
              this.http_healthtrends.VitalName !== 'Blood Glucose'
            ) {
              const linedt1 = this.lineChartLabels[i].split(' - ');
              const linedt0 = this.lineChartLabels[i - 1].split(' - ');
              const linedt2 = this.lineChartLabels[i + 1]?.split(' - ') || [];

              if (linedt1[0] === linedt0[0] || linedt1[0] === linedt2[0]) {
                this.lineChartLabels.splice(i, 1);
                this.http_healthtrends.Values.forEach(
                  (tmpitem: { data: any[] }) => tmpitem.data.splice(i, 1)
                );
              }
            }
            i++;
          } catch (ex) {
            console.error('Exception:', ex);
          }
        }
      }

      j++;
      temp.push({
        data: item.data,
        label: item.label,
        fill: false,
        lineTension: 0.5,
      });
    }

    this.lineChartData = temp;
  }

  private setGraphFallback(daycount: number) {
    if (daycount === 7) {
      this.setEmptyGraph();
    } else if (daycount === 30) {
      this.setEmpty30DaysGraph();
    }
  }
  setEmptyGraph() {
    var date_val = new Date();
    var x = [0, 1, 2, 3, 4, 5, 6];
    var DefaultDates = [];
    var date_val_set = '';
    for (var item1 of x) {
      date_val_set = this.convertDate(date_val.setDate(date_val.getDate()));
      DefaultDates.push(date_val_set);
      date_val_set = this.convertDate(date_val.setDate(date_val.getDate() - 1));
    }

    this.http_healthtrends = {
      VitalName: 'No Data',
      VitalId: 1,
      Time: DefaultDates.reverse(),
      Values: [
        { data: [null, null, null, null, null, null, null], label: 'Vital' },
      ],
    };
    this.lineChartLabels = this.patientService.convertDateforHealthTrends(
      this.http_healthtrends.Time,
      this.http_healthtrends
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

  setEmpty30DaysGraph() {
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

    this.http_healthtrends = {
      VitalName: 'No Data',
      VitalId: 1,
      Time: DefaultDates.reverse(),
      Values: [
        { data: [null, null, null, null, null, null, null], label: 'Vital' },
      ],
    };
    this.lineChartLabels = this.patientService.convertDateforHealthTrends(
      this.http_healthtrends.Time,
      this.http_healthtrends
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
  UpcomingSchedule: any;
  LatestSchedule: any;
  calculateUpcomingSchedule(patient_id: any) {
    var that = this;
    this.today = new Date();
    var startdate = this.convertDate(this.today);
    this.today.setDate(this.today.getDate() + 7);
    var enddate = this.convertDate(this.today);
    that.rpm
      .rpm_get(
        `/api/patient/getpatientschedules?PatientId=${patient_id}&StartDate=${
          startdate + 'T00:00:00'
        }&EndDate=${enddate + 'T23:59:59'}`
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
          that.scheduleData = [
            {
              description: 'No Upcoming Schedules',
              patientname: '',
              time: '',
            },
          ];
        } else {
          that.scheduleData = that.LatestSchedule;
        }
      });
  }

  scheduleData: any;
  alertsAndTasks: any;
  latestAlertsAndTasks: any;
  OpenAlertsAndTasks: any;

  async getLatestAlertsAndTasks() {
    this.loading = true;

    try {
      const { latestTasks, allTasks } =
        await this.patientService.getLatestAlertsAndTasks(
          this.patient_id,
          this.program_id,
          this.convertDate.bind(this)
        );

      this.latestAlertsAndTasks = latestTasks;
      this.alertsAndTasks = allTasks;
    } catch (error) {
      console.error('❌ Error:', error);
    } finally {
      this.loading = false;
    }
  }

  patientInteractionMin: any;
  patientInteractionSec: any;
  time: any;

  resetval: any;
  resetPassword() {
    var that = this;
    var req_body = {
      PatientId: this.patient_id,
    };
    that.rpm.rpm_post(`/api/patient/updatepatientpassword`, req_body).then(
      (data) => {
        this.resetval = data;
        this.confirmDialog.showConfirmDialog(
          `Password reset successfully !!!,\nNew password :` + this.resetval.password,
          'Success',
          () => {
            null
          },
          false
        );
      },
      (err) => {
        // this.showDialog = true;
        this.confirmDialog.showConfirmDialog(
          `Password reset Failed!!!`,
          'Error',
          () => {
            null
          },
          false
        );
      }
    );
  }


  retArr: Array<string>;

  BillingOverview: any;
  getBillingOverview(pid: any, programid: any) {
    pid = sessionStorage.getItem('PatientId');
    programid = sessionStorage.getItem('ProgramId');
    var that = this;
    that.progrss_billing_array = null;
    this.loading4 = true;
    that.rpm
      .rpm_get(
        `/api/patient/getbillinginfobyPatientId?patientId=${pid}&patientProgramId=${programid}`
      )
      .then(
        (data) => {
          that.BillingOverview = data;
          console.log('Billing Overview');
          console.log(that.BillingOverview);
          that.progrss_billing_array = [];
          if (!that.BillingOverview) {
            that.BillingOverview = [];
          }
          var interactionTime = 0;
          for (let x of that.BillingOverview) {
            // New Code 12/05/2023
            if (x.Total == null) {
              this.pecentageValue = 0;
            } else if (x.Completed > x.Total) {
              this.pecentageValue = 100;
            } else if (x.Total == 0 && x.IsTargetMet) {
              this.pecentageValue = 100;
            } else {
              this.pecentageValue = (x.Completed / x.Total) * 100;
            }
            if (this.pecentageValue >= 100) {
              if (x.CPTCode == '99454') {
                this.displayText = x.Completed + '/' + x.Total;
                this.BillingPeriodStart = this.convertDate(
                  new Date(x.BillingStartDate)
                );
                this.BillingPeriodEnd = new Date(x.BillingStartDate);
                if (this.billingtypeVariable == '30days') {
                  this.BillingPeriodEnd = this.convertDate(
                    new Date(
                      this.BillingPeriodEnd.setDate(
                        this.BillingPeriodEnd.getDate() + 29
                      )
                    )
                  );
                  if (this.BillingPeriodStart == '1-01-01') {
                    this.BillingPeriodStart = undefined;
                    this.BillingPeriodEnd = undefined;
                  } else {
                    this.BillingPeriodStart = this.Billing_ConvertDate(
                      this.BillingPeriodStart
                    );
                    this.BillingPeriodEnd = this.Billing_ConvertDate(
                      this.BillingPeriodEnd
                    );
                  }
                } else {
                  this.BillingPeriodEnd = new Date(
                    this.BillingPeriodEnd.setMonth(
                      this.BillingPeriodEnd.getMonth() + 1
                    )
                  );
                  var day = this.BillingPeriodEnd.getDate();
                  if (day <= 15) {
                    this.BillingPeriodEnd.setDate(1);
                  } else {
                    this.BillingPeriodEnd.setDate(16);
                  }
                  this.BillingPeriodEnd = this.convertDate(
                    this.BillingPeriodEnd
                  );
                  // this.BillingPeriodEnd = this.convertDate(new Date(this.BillingPeriodEnd.setDate(this.BillingPeriodEnd.getDate()-1)))
                  if (this.BillingPeriodStart == '1-01-01') {
                    this.BillingPeriodStart = undefined;
                    this.BillingPeriodEnd = undefined;
                  } else {
                    this.BillingPeriodStart = this.Billing_ConvertDate(
                      this.BillingPeriodStart
                    );
                    this.BillingPeriodEnd = this.Billing_ConvertDate(
                      this.BillingPeriodEnd
                    );
                  }
                }
              } else if (x.CPTCode == '99453') {
                this.displayText = 'Completed';
              } else if (x.CPTCode == '99457') {
                var min = Math.trunc(x.Completed / 60);
                var seconds = Math.trunc(x.Completed % 60);
                if (x.Total < 60) {
                  this.displayText = min + ':' + seconds + '/' + x.Total;
                } else {
                  this.displayText = min + ':' + seconds + '/' + x.Total / 60;
                }
              } else if (x.CPTCode == '99458') {
                var min = Math.trunc(x.Completed / 60);
                var seconds = Math.trunc(x.Completed % 60);
                if (x.Total < 60) {
                  this.displayText = min + ':' + seconds + '/' + x.Total;
                } else {
                  this.displayText = min + ':' + seconds + '/' + x.Total / 60;
                }
              } else {
                // this.displayText = x.Total +'/'+ x.Total;
                if (x.CPTCode == '99453' || x.CPTCode == '99454') {
                  this.displayText = x.Completed + '/' + x.Total;
                } else {
                  var min = Math.trunc(x.Completed / 60);
                  var seconds = Math.trunc(x.Completed % 60);
                  this.displayText = min + ':' + seconds + '/' + x.Total / 60;
                }
              }
            } else {
              if (x.CPTCode == '99457') {
                var min = Math.trunc(x.Completed / 60);
                var seconds = Math.trunc(x.Completed % 60);
                if (x.Total < 60) {
                  this.displayText = min + ':' + seconds + '/' + x.Total;
                } else {
                  this.displayText = min + ':' + seconds + '/' + x.Total / 60;
                }
              } else if (x.CPTCode == '99458') {
                var min = Math.trunc(x.Completed / 60);
                var seconds = Math.trunc(x.Completed % 60);
                if (x.Total < 60) {
                  this.displayText = min + ':' + seconds + '/' + x.Total;
                } else {
                  this.displayText = min + ':' + seconds + '/' + x.Total / 60;
                }
              } else {
                if (x.CPTCode == '99453' || x.CPTCode == '99454') {
                  this.displayText = x.Completed + '/' + x.Total;
                } else if (x.Total == null) {
                  // New Code 12/05/2023
                  var min = Math.trunc(x.Completed / 60);
                  var seconds = Math.trunc(x.Completed % 60);
                  this.displayText = min + ':' + seconds;
                } else {
                  var min = Math.trunc(x.Completed / 60);
                  var seconds = Math.trunc(x.Completed % 60);
                  this.displayText = min + ':' + seconds + '/' + x.Total / 60;
                }

                this.BillingPeriodStart = this.convertDate(
                  new Date(x.BillingStartDate)
                );
                this.BillingPeriodEnd = new Date(x.BillingStartDate);
                if (this.billingtypeVariable == '30days') {
                  this.BillingPeriodEnd = this.convertDate(
                    new Date(
                      this.BillingPeriodEnd.setDate(
                        this.BillingPeriodEnd.getDate() + 29
                      )
                    )
                  );
                  if (this.BillingPeriodStart == '1-01-01') {
                    this.BillingPeriodStart = undefined;
                    this.BillingPeriodEnd = undefined;
                  } else {
                    this.BillingPeriodStart = this.Billing_ConvertDate(
                      this.BillingPeriodStart
                    );
                    this.BillingPeriodEnd = this.Billing_ConvertDate(
                      this.BillingPeriodEnd
                    );
                  }
                } else {
                  this.BillingPeriodEnd = new Date(
                    this.BillingPeriodEnd.setMonth(
                      this.BillingPeriodEnd.getMonth() + 1
                    )
                  );
                  var day = this.BillingPeriodEnd.getDate();
                  if (day <= 15) {
                    this.BillingPeriodEnd.setDate(1);
                  } else {
                    this.BillingPeriodEnd.setDate(16);
                  }
                  this.BillingPeriodEnd = this.convertDate(
                    this.BillingPeriodEnd
                  );
                  // this.BillingPeriodEnd = this.convertDate(new Date(this.BillingPeriodEnd.setDate(this.BillingPeriodEnd.getDate()-1)))
                  if (this.BillingPeriodStart == '1-01-01') {
                    this.BillingPeriodStart = undefined;
                    this.BillingPeriodEnd = undefined;
                  } else {
                    this.BillingPeriodStart = this.Billing_ConvertDate(
                      this.BillingPeriodStart
                    );
                    this.BillingPeriodEnd = this.Billing_ConvertDate(
                      this.BillingPeriodEnd
                    );
                  }
                }
              }
            }
            //New Code 12/05/2023
            var progress;
            if (x.Total != null) {
              progress = x.Completed.toString() + '/' + x.Total.toString();
            } else {
              progress = 0;
            }

            if (x.CPTCode == '99457') {
              interactionTime = interactionTime + x.Completed;
            } else if (x.CPTCode == '99458') {
              interactionTime = interactionTime + x.Completed;
            }

            var obj = {
              billing_code: x.CPTCode,
              billing_value: this.pecentageValue,
              progress_value: this.displayText,
              targetmet: x.IsTargetMet,
            };
            this.progrss_billing_array.push(obj);
          }
          if (that.BillingOverview.length > 0) {
            if (that.BillingOverview[0].ProgramName == 'RPM') {
              this.CalculateInteractionTime(interactionTime);
              this.colorcodeval = 19;
            } else {
              var Totalval = 0;
              var CompletedVal = 0;
              for (let x of that.BillingOverview) {
                Totalval = Totalval + x.Total;
                CompletedVal = CompletedVal + x.Completed;
              }
              let patientInteractionMin2 = CompletedVal / 60;
              this.patientInteractionMin = Math.trunc(patientInteractionMin2);
              let patientInteractionSec2 = CompletedVal % 60;
              this.patientInteractionSec = Math.trunc(patientInteractionSec2);
              this.interactionpercentValue =
                ((this.patientInteractionMin * 60 +
                  this.patientInteractionSec) /
                  Totalval) * 100;
              switch (that.BillingOverview[0].ProgramName) {
                case 'CCM-C':
                  this.colorcodeval = 19;
                  if (this.patientInteractionMin < 20) {
                    this.totalInteractionTime = 20;
                  } else {
                    this.totalInteractionTime = Totalval / 60;
                    this.totalInteractionTime = Math.trunc(
                      this.totalInteractionTime
                    );
                  }
                  break;
                case 'CCM-P':
                  this.colorcodeval = 29;
                  if (this.patientInteractionMin < 30) {
                    this.totalInteractionTime = 30;
                  } else {
                    this.totalInteractionTime = Totalval / 60;
                    this.totalInteractionTime = Math.trunc(
                      this.totalInteractionTime
                    );
                  }
                  break;
                case 'C-CCM':
                  this.colorcodeval = 59;
                  if (this.patientInteractionMin < 60) {
                    this.totalInteractionTime = 60;
                  } else {
                    this.totalInteractionTime = Totalval / 60;
                    this.totalInteractionTime = Math.trunc(
                      this.totalInteractionTime
                    );
                  }
                  break;
                case 'PCM-C':
                  this.colorcodeval = 29;
                  if (this.patientInteractionMin < 30) {
                    this.totalInteractionTime = 30;
                  } else {
                    this.totalInteractionTime = Totalval / 60;
                    this.totalInteractionTime = Math.trunc(
                      this.totalInteractionTime
                    );
                  }
                  break;
                case 'PCM-P':
                  this.colorcodeval = 29;
                  if (this.patientInteractionMin < 30) {
                    this.totalInteractionTime = 30;
                  } else {
                    this.totalInteractionTime = Totalval / 60;
                    this.totalInteractionTime = Math.trunc(
                      this.totalInteractionTime
                    );
                  }
                  break;
              }
            }
          }
          this.loading4 = false;
        },
        (err) => {
          console.log(err);
          that.progrss_billing_array = [];
          that.BillingOverview = [];
          this.loading4 = false;
        }
      );
  }

  colorcodeval = 0;
  send_date = new Date();
  formattedDate: any;
  BillingPeriodEnd: any;
  BillingPeriodStart: any;
  add_months(dt: any, n: any) {
    return new Date(dt.setMonth(dt.getMonth() + n));
  }
  CalculateInteractionTime(time: any) {
    if (time > 3600) {
      this.patientInteractionMin = 60;
      this.patientInteractionSec = '00';
      this.getInteractionTime(this.patientInteractionMin);
      this.interactionpercentValue =
        ((this.patientInteractionMin * 60 + this.patientInteractionSec) /
          3600) * 100;
      let patientInteractionMin = time / 60;
      this.patientInteractionMin = Math.trunc(patientInteractionMin);
      let patientInteractionSec = time % 60;
      this.patientInteractionSec = Math.trunc(patientInteractionSec);
    } else {
      let patientInteractionMin = time / 60;
      this.patientInteractionMin = Math.trunc(patientInteractionMin);
      let patientInteractionSec = time % 60;
      this.patientInteractionSec = Math.trunc(patientInteractionSec);
      this.getInteractionTime(patientInteractionMin);
      this.interactionpercentValue =
        ((this.patientInteractionMin * 60 + this.patientInteractionSec) /
          3600) * 100;
       console.log('Interaction Progress - 1');
       console.log(this.interactionpercentValue);
      // that.interactionpercentValue = parseInt(that.interactionpercentValue)
      if (
        this.interactionpercentValue < 1 &&
        this.interactionpercentValue > 0
      ) {
        this.interactionpercentValue = 1;
      }else{
        this.interactionpercentValue = this.interactionpercentValue;
      }
      if (this.patientInteractionSec < 10) {
        this.patientInteractionSec =
          '0' + this.patientInteractionSec.toString();
      }
    }
  }
  utc: any;
  timezonecalculation() {
    var UTCDifference = this.http_rpm_patientList.PatientDetails.UTCDifference;
    var int = Math.floor(UTCDifference);
    var dec = (UTCDifference - int) * 100;
    this.utc = new Date(
      new Date().getTime() - (5 * 60 * 60 * 1000 + 30 * 60 * 1000)
    ).toLocaleTimeString();
    this.CurrentTime = new Date(
      new Date().getTime() +
        (int * (60 * 60 * 1000) + dec * (60 * 1000)) -
        (5 * 60 * 60 * 1000 + 30 * 60 * 1000)
    ).toLocaleTimeString();
    return this.CurrentTime.slice(0, -3);
  }

  healthtrends(selected_val: any) {
    this.heath_trends_frequency = selected_val;
    this.getHealthTrends(selected_val);
  }
  patient_page_nav: any;
  backtopatientlist() {
    this.patient_page_nav = sessionStorage.getItem('patient-page-status');
    if (this.patient_page_nav == 'patient-page') {
      let route = '/admin/patients';
      this.router.navigate([route]);
      sessionStorage.setItem('patientNavigationStatus', 'true');
    } else if (this.patient_page_nav == 'worklist') {
      let route = '/admin/task';
      this.router.navigate([route]);
      sessionStorage.setItem('patientNavigationStatus', 'true');
    } else if (this.patient_page_nav == 'team') {
      let route = '/admin/teams';
      this.router.navigate([route]);
      sessionStorage.setItem('patientNavigationStatus', 'true');
    } else {
      let route = '/admin/patients';
      this.router.navigate([route]);
    }
  }

  getInteractionTime(interaction_time: any) {
    if (interaction_time < 20) {
      this.totalInteractionTime = 20;
    } else if (interaction_time >= 20 && interaction_time < 40) {
      this.totalInteractionTime = 40;
    } else if (interaction_time > 40 && interaction_time < 60) {
      this.totalInteractionTime = 60;
    } else {
      this.totalInteractionTime = 60;
    }
  }

  clearIntervals() {
    clearInterval(this.interval);
    this.timerValue = 0;
    this.calltimerValue = 0;
    clearInterval(this.interval1);
    clearInterval(this.interval2);
    //clearInterval(this.intervalnote);
    clearInterval(this.interval3);
  }
  openSchedulebyId(id: any) {
    this.patientrightsidebar.navigateEditSchedule(id);
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

  public ScheduleSort() {
    this.http_ActivityScheduleData.reverse();
  }

  getAge(dateString: any) {
    var today = new Date();
    var birthDate = new Date(dateString);
    var age = today.getFullYear() - birthDate.getFullYear();
    var m = today.getMonth() - birthDate.getMonth();
    if (m < 0 || (m === 0 && today.getDate() < birthDate.getDate())) {
      age--;
    }
    return age;
  }
  selectedStatus: any;
  onSatatusChange(status: any) {
    this.http_ActivityAlertTaskTemp = this.http_ActivityAlertTask;

    if (status == 'ToDo') {
      this.http_ActivityAlertTaskTemp = this.http_ActivityAlertTaskTemp.filter(
        function (data: any) {
          return data.Status == 'ToDo';
        }
      );
    } else if (status == 'Completed') {
      this.http_ActivityAlertTaskTemp = this.http_ActivityAlertTaskTemp.filter(
        function (data: any) {
          return data.Status == 'Complete';
        }
      );
    } else if (status == 'InProgress') {
      this.http_ActivityAlertTaskTemp = this.http_ActivityAlertTaskTemp.filter(
        function (data: any) {
          return data.Status == 'InProgress';
        }
      );
    } else if (status == 'undefined') {
      this.http_ActivityAlertTaskTemp = this.http_ActivityAlertTask;
    }
    this.dataSourceChange(5, 2);
  }
  priorityTaskFilterValue = undefined;
  selectedStatusValue = undefined;
  PrirotyTaskFilterResult(data: any) {
    this.http_ActivityAlertTaskTemp = this.http_ActivityAlertTask;
    let dataSourceAlertFilter;
    let PriorityFiterValue: any;
    let statusValue: any;

    if (this.priorityTaskFilterValue == 'undefined') {
      this.priorityTaskFilterValue = undefined;
    }
    if (this.selectedStatusValue == 'undefined') {
      this.selectedStatusValue = undefined;
    }

    if (this.priorityTaskFilterValue == 2) {
      PriorityFiterValue = 'Urgent';
    } else if (this.priorityTaskFilterValue == 3) {
      PriorityFiterValue = 'High';
    } else if (this.priorityTaskFilterValue == 4) {
      PriorityFiterValue = 'Medium';
    } else if (this.priorityTaskFilterValue == 5) {
      PriorityFiterValue = 'Low';
    } else if (this.priorityTaskFilterValue == 6) {
      PriorityFiterValue = 'Critical';
    } else if (this.priorityTaskFilterValue == 7) {
      PriorityFiterValue = 'Severe';
    } else if (this.priorityTaskFilterValue == 8) {
      PriorityFiterValue = 'Moderate';
    } else if (
      this.priorityTaskFilterValue == 1 ||
      this.priorityTaskFilterValue == undefined
    ) {
      PriorityFiterValue = 'Priority';
      dataSourceAlertFilter = this.http_ActivityAlertTaskTemp;
    }

    if (this.selectedStatusValue == 'ToDo') {
      statusValue = 'ToDo';
    } else if (this.selectedStatusValue == 'Completed') {
      statusValue = 'Complete';
    } else if (this.selectedStatusValue == 'InProgress') {
      statusValue = 'InProgress';
    } else if (this.selectedStatusValue == 'undefined') {
      statusValue = 'undefined';
      // this.http_ActivityAlertTaskTemp = this.http_ActivityAlertTask;
    }

    if (
      this.selectedStatusValue != undefined &&
      this.priorityTaskFilterValue != undefined
    ) {
      dataSourceAlertFilter = this.http_ActivityAlertTaskTemp.filter(
        (data: any) => {
          return (
            data.Priority == PriorityFiterValue && data.Status == statusValue
          );
        }
      );
    } else if (
      this.selectedStatusValue == undefined &&
      this.priorityTaskFilterValue != undefined
    ) {
      dataSourceAlertFilter = this.http_ActivityAlertTaskTemp.filter(
        (data: any) => {
          return data.Priority == PriorityFiterValue;
        }
      );
    } else if (
      this.selectedStatusValue != undefined &&
      this.priorityTaskFilterValue == undefined
    ) {
      dataSourceAlertFilter = this.http_ActivityAlertTaskTemp.filter(
        (data: any) => {
          return data.Status == statusValue;
        }
      );
    } else if (
      this.selectedStatusValue == undefined &&
      this.priorityTaskFilterValue == undefined
    ) {
      this.http_ActivityAlertTaskTemp = dataSourceAlertFilter;
    }

    this.http_ActivityAlertTaskTemp = dataSourceAlertFilter;
    this.dataSourceChange(5, 2);
  }

  frmactivitySchedulerange = new FormGroup({
    start: new FormControl(),
    end: new FormControl(),
  });
  frmactivitysmsrange = new FormGroup({
    start: new FormControl(),
    end: new FormControl(),
  });

  async getScheduleData(
    useCustomRange = false,
    startDate?: Date,
    endDate?: Date
  ) {
    try {
      // Select the activity info menu item
      this.activityInfoMenuSelect(1);

      let formattedStartDate: string;
      let formattedEndDate: string;

      // Determine date range based on parameters
      if (!useCustomRange) {
        try {
          // Get current month dates


          const { start: monthStart, end: monthEnd } =
            this.patientutilService.getCurrentMonthDates();

          formattedStartDate = this.patientutilService.formatDateForApi(
            monthStart,
            false
          );

          formattedEndDate = this.patientutilService.formatDateForApi(
            monthEnd,
            false
          );

          // Update form controls
          this.frmactivitySchedulerange.controls['start'].setValue(
            formattedStartDate
          );
          this.frmactivitySchedulerange.controls['end'].setValue(
            formattedEndDate
          );
        } catch (error) {
          console.error('Error setting up default date range:', error);
          throw new Error('Failed to set up default date range');
        }
      } else {
        try {
          // Use provided custom range
          if (!startDate || !endDate) {
            // Use form values if direct dates aren't provided
            startDate = new Date(
              this.frmactivitySchedulerange.controls.start.value
            );
            endDate = new Date(
              this.frmactivitySchedulerange.controls.end.value
            );
          }
          const dateStr = this.convertDate(startDate);
          const dateEnd = this.convertDate(endDate);

          // Validate date range
          // if (!startDate || !endDate || startDate > endDate) {
          //   throw new Error('Invalid date range');
          // }

          // formattedStartDate = this.patientutilService.formatDateForApi(
          //   startDate,
          //   false
          // );

          formattedStartDate = dateStr + 'T00:00:00'
         formattedEndDate = dateEnd + 'T23:59:59'
          // formattedEndDate = this.patientutilService.formatDateForApi(
          //   endDate,
          //   false
          // );

        } catch (error) {
          console.error('Error with custom date range:', error);
          throw new Error('Failed to process custom date range');
        }
      }

      // Use the patient ID
      const patientId = this.currentpPatientId || this.patient_id;
      if (!patientId) {
        throw new Error('No patient ID available');
      }

      // Fetch data using the service
      try {
        const data = await this.patientService.getPatientSchedules(
          patientId,
          formattedStartDate,
          formattedEndDate
        );
        this.http_ActivityScheduleData = data;
      } catch (error) {
        console.error('Error fetching schedule data:', error);
        // Handle the error appropriately for your UI
        // For example, show an error message to the user
        this.http_ActivityScheduleData = []; // Reset or set to empty as appropriate
        throw new Error('Failed to fetch schedule data');
      }
    } catch (error) {
      console.error('Error in getScheduleData:', error);
      // Handle any uncaught errors
      // You might want to display a user-friendly error message
    }
  }

  frmactivityAlertrange = new FormGroup({
    start: new FormControl(),
    end: new FormControl(),
  });

  frmactivityCallrange = new FormGroup({
    start: new FormControl(),
    end: new FormControl(),
  });

  frmactivityReviewrange = new FormGroup({
    start: new FormControl(),
    end: new FormControl(),
  });

  scheduleSelected = Array();
  filteredItem: any;
  scheduleComplete(event: any, id: any) {
    if (event.target.checked == true) {
      this.scheduleSelected.push(id);
    } else {
      var a = this.scheduleSelected.indexOf(id);

      this.scheduleSelected.splice(a, 1);
    }
  }

  saveCompleteTask() {
    var req_body: any = {};
    req_body['IDs'] = this.scheduleSelected;
    this.rpm.rpm_post('/api/schedules/updatecompletedschedule', req_body).then(
      (data) => {
        this.getScheduleData();

        alert('Schedule Status Changed Successfully!!');
        this.scheduleSelected = []
      },
      (err:any) => {
        //show error patient id creation failed
        alert(err.error.message);
      }
    );
  }

  Billing_ConvertDate(dateval: string): string {
    if (!dateval) return '';
     console.log(dateval)
    const months = [
      'Jan',
      'Feb',
      'Mar',
      'Apr',
      'May',
      'Jun',
      'Jul',
      'Aug',
      'Sep',
      'Oct',
      'Nov',
      'Dec',
    ];
    const [year, month, day] = dateval.split('-');

    return `${day}-${months[parseInt(month, 10) - 1]}-${year}`;
  }

  Htmlele: any;

  NoteHours = '00';
  NoteMinutes = '00';
  NoteSec = '00';
  intervalnote: any;
  showNoteModal = false;
  getDataforquestions() {
    this.showNoteModal = true;
    //this.dialog.open(this.myPreviewTemp);
  }

  getCallDataupdatePreview() {
    this.showNoteupdateModal = true;
    // this.dialog.open(this.myPreviewUpdateTemp);
  }
  noteMasterData: any;
  checkedAns: any;
  checkAnsId: any[] = [];
  AnswerIdArray = [];
  MainQuestionReturnArray = Array();

  previewConfirm() {
    this.MainQuestionReturnArray = [];

    if (this.QuestionArrayBase) {
      for (let i = 0; i < this.QuestionArrayBase.length; i++) {
        if (this.QuestionArrayBase[i].AnswerTypes.length > 0) {
          this.checkedAns = this.QuestionArrayBase[i].AnswerTypes.filter(
            (item: { Checked: any }) => {
              return item.Checked;
            }
          );
          var AnswerArray = this.ProcessArray(this.checkedAns);
          this.checkedAns = [];

          if (AnswerArray.length > 0) {
            this.MainQuestionReturnArray.push({
              QuestionId: this.QuestionArrayBase[i].QuestionId,
              Notes: this.QuestionArrayBase[i].Notes,
              AnswersIds: AnswerArray,
            });
          } else {
            this.MainQuestionReturnArray.push({
              QuestionId: this.QuestionArrayBase[i].QuestionId,
              Notes: this.QuestionArrayBase[i].Notes,
              AnswersIds: [],
            });
          }
        }

        if (this.QuestionArrayBase[i].SubQuestions.length > 0) {
          for (
            let j = 0;
            j < this.QuestionArrayBase[i].SubQuestions.length;
            j++
          ) {
            this.checkedAns = this.QuestionArrayBase[i].SubQuestions[
              j
            ].AnswerTypes.filter((item: { Checked: any }) => {
              return item.Checked;
            });
            var AnswerArray = this.ProcessArray(this.checkedAns);
            this.checkedAns = [];

            if (AnswerArray.length > 0) {
              this.MainQuestionReturnArray.push({
                QuestionId:
                  this.QuestionArrayBase[i].SubQuestions[j].QuestionId,
                Notes: this.QuestionArrayBase[i].SubQuestions[j].Notes,
                AnswersIds: AnswerArray,
              });
            } else {
              this.MainQuestionReturnArray.push({
                QuestionId:
                  this.QuestionArrayBase[i].SubQuestions[j].QuestionId,
                Notes: this.QuestionArrayBase[i].SubQuestions[j].Notes,
                AnswersIds: [],
              });
            }
          }
        }
      }
    }

    this.pid = sessionStorage.getItem('PatientId');
    this.cid = sessionStorage.getItem('userid');
    this.currentProgramId = sessionStorage.getItem('ProgramId');

    if (
      this.NoteTypeId != undefined &&
      this.NoteTypeId != 'undefined' &&
      this.establishedValue != undefined &&
      this.establishedValue != 'undefined '
    ) {
      var req_body: any = {};
      req_body['PatientId'] = parseInt(this.pid);
      req_body['PatientProgramId'] = parseInt(this.currentProgramId);
      req_body['NoteTypeId'] = parseInt(this.NoteTypeId);
      req_body['NoteType'] = 'CALL';
      //Update Note CallType Added
      req_body['calltype'] = this.callType;
      req_body['IsEstablishedCall'] = this.establishedValue;
      req_body['IsCareGiver'] = this.caregiver;
      req_body['IsCallNote'] = true;
      if (this.callType == 'AUDIO') {
        req_body['Duration'] = this.convertTimeToSec(this.calltimer);
      } else {
        req_body['Duration'] = this.videoCallTimervalue;
      }

      req_body['Notes'] = this.additionaNotes;
      req_body['CompletedByUserId'] = parseInt(this.cid);
      req_body['MainQuestions'] = this.MainQuestionReturnArray;
      req_body['CreatedBy'] = 'sa';
      this.loading = true;
      this.loading_note = true;

      this.rpm.rpm_post('/api/notes/addnotev1', req_body).then(
        (data) => {
          if (this.EditcallNotes) {
            this.callTimerEnabled = false;
            this.EditcallNotes = false;
            this.OnpreviewUpdateCancel();
            this.resetCallPanel();
            this.resetCallTimer();
          }
          this.EditcallNotes = false;

          alert('New Note Added Successfully!!');
          this.QuestionArrayBase = this.noteMasterData.MainQuestions;

          this.callVariable = false;
          this.notes_update_panel = false;

          this.ChangeScreen(5);
          this.getCallNotes();

          this.resetCallTimer();
          this.callTimerEnabled = false;
          this.loading = false;
          this.dialog.closeAll();

          this.incomingCallVal = false;
          this.incomingVariableDisable = false;
          this.callDisConnected();
          this.showNoteModal=false;
          if(this.videoOnVariable == true)
            {
              this.disconnectVideo();
            }
          //this.disconnectVideo();
          this.billingReload();
          this.loading_note = false;
          this.resetCallPanel();
        },
        (err) => {
          this.loading = false;

          this.incomingCallVal = false;
          this.incomingVariableDisable = false;
          this.QuestionArrayBase = this.noteMasterData.MainQuestions;
          alert('Could not add Note...!!');
          this.dialog.closeAll();
          this.billingReload();
          this.loading_note = false;
        }
      );

      // End Of add Call
    } else {
      this.loading = false;
      this.QuestionArrayBase = this.noteMasterData.MainQuestions;
      alert('Please Complete The Form ');
      this.dialog.closeAll();
      this.loading_note = false;
    }
  }

  previewUpdateConfirm() {
    this.MainQuestionReturnArray = [];
    if (this.QuestionArrayBase) {
      for (let i = 0; i < this.QuestionArrayBase.length; i++) {
        if (this.QuestionArrayBase[i].AnswerTypes.length > 0) {
          this.checkedAns = this.QuestionArrayBase[i].AnswerTypes.filter(
            (item: { Checked: any }) => {
              return item.Checked;
            }
          );
          var AnswerArray = this.ProcessArray(this.checkedAns);
          this.checkedAns = [];

          if (AnswerArray.length > 0) {
            this.MainQuestionReturnArray.push({
              QuestionId: this.QuestionArrayBase[i].QuestionId,
              Notes: this.QuestionArrayBase[i].Notes,
              AnswersIds: AnswerArray,
            });
          } else {
            this.MainQuestionReturnArray.push({
              QuestionId: this.QuestionArrayBase[i].QuestionId,
              Notes: this.QuestionArrayBase[i].Notes,
              AnswersIds: [],
            });
          }
        }

        if (this.QuestionArrayBase[i].SubQuestions.length > 0) {
          for (
            let j = 0;
            j < this.QuestionArrayBase[i].SubQuestions.length;
            j++
          ) {
            this.checkedAns = this.QuestionArrayBase[i].SubQuestions[
              j
            ].AnswerTypes.filter((item: { Checked: any }) => {
              return item.Checked;
            });
            var AnswerArray = this.ProcessArray(this.checkedAns);
            this.checkedAns = [];

            if (AnswerArray.length > 0) {
              this.MainQuestionReturnArray.push({
                QuestionId:
                  this.QuestionArrayBase[i].SubQuestions[j].QuestionId,
                Notes: this.QuestionArrayBase[i].SubQuestions[j].Notes,
                AnswersIds: AnswerArray,
              });
            } else {
              this.MainQuestionReturnArray.push({
                QuestionId:
                  this.QuestionArrayBase[i].SubQuestions[j].QuestionId,
                Notes: this.QuestionArrayBase[i].SubQuestions[j].Notes,
                AnswersIds: [],
              });
            }
          }
        }
      }
    }

    this.pid = sessionStorage.getItem('PatientId');
    this.cid = sessionStorage.getItem('userid');
    this.currentProgramId = sessionStorage.getItem('ProgramId');

    if (
      this.NoteTypeId != undefined &&
      this.NoteTypeId != 'undefined' &&
      this.establishedValue != undefined &&
      this.establishedValue != 'undefined'
    ) {
      var req_body: any = {};
      req_body['Id'] = parseInt(this.noteId);
      req_body['NoteTypeId'] = parseInt(this.NoteTypeId);
      req_body['NoteType'] = 'CALL';
      req_body['IsEstablishedCall'] = this.establishedValue;
      req_body['IsCareGiver'] = this.caregiver;
      req_body['IsCallNote'] = true;
      req_body['Duration'] = this.convertTimeToSec(this.NoteTime);
      req_body['Notes'] = this.additionaNotes;
      req_body['CompletedByUserId'] = parseInt(this.cid);
      req_body['MainQuestions'] = this.MainQuestionReturnArray;
      req_body['CreatedBy'] = 'sa';
      //Update Note CallType Added
      req_body['calltype'] = this.callType;
      this.loading_note = true;

      this.rpm.rpm_post('/api/notes/updatenotev1', req_body).then(
        (data) => {
          alert('Call Note Updated Successfully!!');
          this.showNoteupdateModal = false;
          this.QuestionArrayBase = this.noteMasterData.MainQuestions;
          this.getCallNotes();

          this.notes_update_panel = false;
          if (!this.EditcallNotes) {
            this.stopCallEditTimer();
          }

          this.dialog.closeAll();

          this.billingReload();
          this.loading_note = false;

          this.resetCallPanel();
        },
        (err) => {
          this.QuestionArrayBase = this.noteMasterData.MainQuestions;
          alert('Could not Updated Note...!!');
          this.dialog.closeAll();

          this.billingReload();
          this.loading_note = false;
        }
      );
    } else {
      this.loading = false;

      alert('Please Complete The Form ');
      this.dialog.closeAll();
      this.loading_note = false;
    }
  }

  ProcessArray(answerItemArray: any) {
    var AnswerIdArray = Array();
    for (let i = 0; i < answerItemArray.length; i++) {
      AnswerIdArray.push(answerItemArray[i].AnswerId);
    }
    return AnswerIdArray;
  }
  establishedValue: any;

  onestablishedChange(e: any) {

  }
  caregiver = false;
  onCareGiverChange(e: any) {

  }

  callNoteData: any;
  noteId: any;
  callNoteUpdateTimer = 0;
  interval5: any;
  EditcallNotes = false;
  incomingStatus = false;

  async getCallnotedata(element: any) {
    try {
      // Update UI state
      this.notes_update_panel = true;
      this.incomingStatus = false;

      // Check if this is an incoming call
      if (element.NoteType === 'Incoming Call') {
        this.incomingStatus = true;
      }

      // Validate input
      if (!element.Id || !this.patientProgramName) {
        throw new Error('Missing note ID or program name');
      }

      // Fetch note data using service
      const data = await this.patientService.getPatientCallNoteById(
        this.patientProgramName,
        element.Id
      );

      // Process the response
      if (data) {
        // Set edit mode if timer is enabled
        if (this.callTimerEnabled) {
          this.EditcallNotes = true;
        }

        // Update component state with note data
        this.callNoteData = data;
        this.noteId = element.Id;
        this.NoteTypeId = this.callNoteData.NoteTypeId;
        this.establishedValue = this.callNoteData.IsEstablishedCall;
        this.caregiver = this.callNoteData.IsCareGiver;
        this.NoteTime = this.patientutilService.convertSecToTime(
          this.callNoteData.Duration
        );
        this.additionaNotes = this.callNoteData.Notes;
        this.QuestionArrayBase = this.callNoteData.MainQuestions;
        this.calledittimerValue = parseInt(this.callNoteData.Duration);

        // Start the edit timer if not in edit mode
        if (!this.EditcallNotes) {
          this.startCallEditTimer();
        }
      } else {
        throw new Error('No data returned for the note');
      }
    } catch (error) {
      console.error('Error getting call note data:', error);
      // Handle the error appropriately in the UI
      // For example, show a message to the user
      this.notes_update_panel = false;
    }
  }
  isAllSelected(item: any) {
    for (let i = 0; i < this.QuestionArrayBase.length; i++) {
      if (this.QuestionArrayBase[i].Question == 'Readings Summary Report') {
        var SummaryAnswer = this.QuestionArrayBase[i].AnswerTypes;
        if (this.QuestionArrayBase[i].AnswerTypes.length > 0) {
          SummaryAnswer.forEach((val: { id: any; Checked: boolean }) => {
            if (val.id == item.target.id) val.Checked = !val.Checked;
            else {
              val.Checked = false;
            }
          });
        }
      }
    }
  }

  cancelClickEnable = false;

  OnCancelupdateNotes() {
    this.resetCallNotes();
    // this.resetCallTimer();
    this.stopCallEditTimer();
    // this.callTimerEnabled=false;
    this.notes_update_panel = false;
  }
  onPreviewSummaryCancel()
  {
    this.showNoteupdateModal = false;
  }

  OnpreviewUpdateCancel() {
    this.notes_update_panel = false;


    if (this.EditcallNotes) {
      clearInterval(this.interval);
      this.timerValue = this.timerValue + this.callNoteUpdateTimer;
      this.interval = setInterval(() => {
        this.calltimerValue = this.calltimerValue + 1;
        this.calltimer = this.timeConvert(this.calltimerValue);
        sessionStorage.setItem('calltimer', this.calltimer);
        if (this.calltimerValue === 86400) {
          clearInterval(this.interval);
        }
      }, 1000);
    } else {
      clearInterval(this.interval);
      this.timerValue = this.timerValue + this.callNoteUpdateTimer;
      this.interval = setInterval(() => {
        if (!this.callTimerEnabled) {
          this.timerValue = this.timerValue + 1;
          this.PageTimer = this.timeConvert(this.timerValue);
          sessionStorage.setItem('PageTimer', this.PageTimer);
          if (this.timerValue === 86400) {
            clearInterval(this.interval);
          }
        }
        if (this.callTimerEnabled && !this.editTimerenabled) {
          this.calltimerValue = this.calltimerValue + 1;
          this.calltimer = this.timeConvert(this.calltimerValue);
          sessionStorage.setItem('calltimer', this.calltimer);
          if (this.calltimerValue === 86400) {
            clearInterval(this.interval);
          }
        }
      }, 1000);
    }

    //this.callTimerEnabled=false;
    this.stopCallEditTimer();
    this.resetCallPanel();
  }
  OnCancelNotesTimer() {
    clearInterval(this.interval);
    this.timerValue = this.timerValue + this.calltimerValue;

    this.interval = setInterval(() => {
      if (!this.callTimerEnabled) {
        this.timerValue = this.timerValue + 1;
        this.PageTimer = this.timeConvert(this.timerValue);
        sessionStorage.setItem('PageTimer', this.PageTimer);
        if (this.timerValue === 86400) {
          clearInterval(this.interval);
        }
      }
      if (this.callTimerEnabled && !this.editTimerenabled) {
        this.calltimerValue = this.calltimerValue + 1;
        this.calltimer = this.timeConvert(this.calltimerValue);
        sessionStorage.setItem('calltimer', this.calltimer);
        if (this.calltimerValue === 86400) {
          clearInterval(this.interval);
        }
      }
    }, 1000);
  }

  OnpreviewCancel() {
    this.stopCallEditTimer();
    this.showNoteModal=false;
    //this.callVariable = false;
    this.callDisConnected();
    this.incomingCallVal = false;
    this.incomingVariableDisable = false;
    //25/07/2023
    if(this.videoOnVariable == true)
    {
      this.disconnectVideo();
    }
    this.videoOnVariable = false;
    //this.disconnectVideo();

  }
  closenoterDialogModal()
  {
    this.showNoteModal=false;
  }
  resetCallPanel() {
    this.MasterDataQuestionTemp = sessionStorage.getItem(
      'MasterDataQuestionTemp'
    );
    this.QuestionArrayBase = JSON.parse(this.MasterDataQuestionTemp);

    this.additionaNotes = '';
    this.NoteTypeId = undefined;
    this.establishedValue = undefined;
    this.caregiver = false;
  }
  addnewProgam() {
    let route = '/admin/addpatient';
    this.router.navigate([route], {
      queryParams: {
        id: this.currentpPatientId,
        programId: this.currentProgramId,
      },
      skipLocationChange: true,
    });
  }
  Cancelrenew() {
    this.showProgramRenewModal = false;
  }

  getrenewProgram() {
    this.showProgramRenewModal = true;
    this.showNoteModal = false;
  }

  renewProgramForm = new FormGroup({
    startdate: new FormControl(null, [Validators.required]),
    pgmendDate: new FormControl(null, [Validators.required]),
  });

  durationValue = 12;
  increment_duration() {
    if (this.durationValue < 12) {
      this.durationValue++;
    } else {
      this.durationValue = 12;
    }
    this.calculateEndDate();
  }
  decrement_duration() {
    if (this.durationValue > 1) {
      this.durationValue--;
    } else {
      this.durationValue = 1;
    }
    this.calculateEndDate();
  }
  startDateValue: any;
  calculateEndDate() {
    // var someDate = this.renewProgramForm.controls.startdate.value;
    // if (someDate.includes('T')) {
    //   someDate = someDate;
    // } else {
    //   someDate = someDate + 'T00:00:00';
    // }
    // someDate = this.auth.ConvertToUTCRangeInput(new Date(someDate));

    let someDate: any = this.renewProgramForm.controls.startdate.value;

    if (someDate !== null && someDate !== undefined) {
      if (someDate.includes('T')) {
        // Do nothing or leave it as is
      } else {
        someDate = someDate + 'T00:00:00';
      }
      someDate = this.auth.ConvertToUTCRangeInput(new Date(someDate));
    } else {
      // Handle the case where someDate is null, maybe set it to a default value
      someDate = ''; // Or whatever you need
    }

    this.startDateValue = someDate;
    const someDateValue = dayjs(someDate).add(this.durationValue, 'month');
    this.renewProgramForm.controls['pgmendDate'].setValue(
      this.convertDate(someDateValue)
    );
  }

  http_dataTabledischargeSouce: any;
  tableDataSrc: any;

  timerUpdate(timerValue: any) {
    if (!this.callTimerEnabled) {
      timerValue = timerValue + 1;
      this.PageTimer = this.timeConvert(timerValue);
      sessionStorage.setItem('PageTimer', this.PageTimer);
      if (timerValue === 86400) {
        clearInterval(this.interval);
      }
    }
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
  incomingCallVal = false;
  incomingCall() {
    this.callTimerEnabled = true;
    this.incomingCallVal = true;
    this.establishedValue = true;
    var noteValue = this.noteTypeIdArray.filter(function (data: any) {
      return data.Name == 'Incoming Call';
    });
    if (noteValue[0].Id) {
      this.NoteTypeId = noteValue[0].Id;
    }
  }
  currentprogramId: any;
  ActiveProgram: any;
  getProgramHistory() {
    this.currentpPatientId = sessionStorage.getItem('PatientId');
    this.currentprogramId = sessionStorage.getItem('ProgramId');

    this.rpm
      .rpm_get(
        `/api/patient/getallpatientprograms?PatientId=${this.currentpPatientId}`
      )
      .then((data) => {
        this.ProgramHistory = data;

        var CurrentSelection = this.ProgramHistory.filter(
          (data: { PatientProgramId: any }) =>
            data.PatientProgramId == this.currentprogramId
        );

        this.ActiveProgram = CurrentSelection[0];
        this.CurrentProgramSelected = CurrentSelection[0].PatientProgramId;
      });
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
    // {
    //   menu_id:5,
    //   menu_title:'Uploads'
    // },
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
    // {
    //   menu_id:5,
    //   menu_title:'Uploads'
    // },
  ];
  getDisplyClinicalInfoMenu() {
    var ClinicalMenuResult;
    if (this.patientProgramname == 'CCM' || this.patientProgramname == 'PCM') {
      ClinicalMenuResult = this.CCMClinicalMenu;
    } else {
      ClinicalMenuResult = this.rpmClinicalMenu;
    }

    return ClinicalMenuResult;
  }
  getDisplyActivityInfoMenu() {
    var ActivityMenuResult;
    if (this.patientProgramname == 'CCM' || this.patientProgramname == 'PCM') {
      ActivityMenuResult = this.CCMactivityInfoMenu;
    } else {
      ActivityMenuResult = this.activityInfoMenu;
    }

    return ActivityMenuResult;
  }
  ConvertMinute(timeValue: any) {
    var ConvertData = Math.floor(timeValue / 60);
    var convertSec = timeValue % 60;

    var timeData = ConvertData + ':' + convertSec;
    return timeData;
  }
  timevalue: any;
  convertBillingSec(data: any, timeValedata: any) {
    if (data == '99453' || data == '99454') {
      this.timevalue = timeValedata;
    } else {
      this.timevalue = this.ConvertMinute(timeValedata);
    }
    return this.timevalue;
  }

  billdisplay: any;
  billCycledisplay(data: any) {
    if (data == null) {
      this.billdisplay = 'Not Available';
    } else {
      this.billdisplay = data;
    }
    return this.billdisplay;
  }

  getPatientStatusClass() {
    var patientStatus;
    if (this.http_rpm_patientList['PatientProgramdetails'].Status) {
      if (
        this.http_rpm_patientList['PatientProgramdetails'].Status == 'Active'
      ) {
        patientStatus = 'patient_active';
      } else if (
        this.http_rpm_patientList['PatientProgramdetails'].Status ==
          'Prescribed' ||
        this.http_rpm_patientList['PatientProgramdetails'].Status == 'Enrolled'
      ) {
        patientStatus = 'patient_prescribed';
      } else if (
        this.http_rpm_patientList['PatientProgramdetails'].Status ==
          'Discharged' ||
        this.http_rpm_patientList['PatientProgramdetails'].Status ==
          'ReadyToDischarge'
      ) {
        patientStatus = 'patient_discharge';
      } else if (
        this.http_rpm_patientList['PatientProgramdetails'].Status == 'OnHold'
      ) {
        patientStatus = 'patient_onhold';
      } else if (
        this.http_rpm_patientList['PatientProgramdetails'].Status == 'InActive'
      ) {
        patientStatus = 'patient_inActive';
      }
    }

    return patientStatus;
  }

  openDischrageDialog() {
    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      maxWidth: '400px',
      data: {
        title: 'Are you sure?',
        message: ' Do You Want to  Discharge the Patient ? ',
      },
    });
    dialogRef.afterClosed().subscribe((dialogResult: any) => {
      // if user pressed yes dialogResult will be true,
      // if he pressed no - it will be false

      if (dialogResult) {
        this.dischargePatient();
        this.loadPatientInfo();
      } else {
        return;
      }
    });
  }

  dischargePatient() {
    var insuranceDeails =
      this.http_rpm_patientList.PatientInsurenceDetails.PatientInsurenceInfos;
    this.pid = sessionStorage.getItem('PatientId');
    var vitalsData =
      this.http_rpm_patientList['PatientProgramdetails'].PatientVitalInfos;
    var result = vitalsData.filter((data: any) => {
      return data.Selected == true;
    });
    var VitalIdAyrray = result.map(function (item: any) {
      return item.VitalId;
    });
    var data =
      this.http_rpm_patientList.PatientDevicesDetails.PatientDeviceInfos;

    var req_body = {
      PatientId: parseInt(this.pid),
      PatientProgramId:
        this.http_rpm_patientList['PatientProgramdetails'].PatientProgramId, //
      PhysicianId:
        this.http_rpm_patientList['PatientPrescribtionDetails'].PhysicianId, //
      ConsultationDate: this.convertDate(
        this.http_rpm_patientList['PatientPrescribtionDetails'].ConsultationDate
      ), //
      CareTeamUserId:
        this.http_rpm_patientList['PatientProgramdetails'].CareTeamUserId, //
      PatientStatus: 'Discharged',
      StartDate: this.http_rpm_patientList['PatientProgramdetails'].StartDate, //
      EndDate: this.http_rpm_patientList['PatientProgramdetails'].EndDate, //
      patientProgramGoals:
        this.http_rpm_patientList.PatientProgramGoals.goalDetails, //
      PatientProgramDiagnosis:
        this.http_rpm_patientList['PatientPrescribtionDetails']
          .PatientDiagnosisInfos, //
      PatientVitalDetails:
        this.http_rpm_patientList.PatientVitalDetails.PatientVitalInfos,
      PatientInsurenceDetails: insuranceDeails, //
      PrescribedDate: this.convertDate(
        this.http_rpm_patientList['PatientPrescribtionDetails'].PrescribedDate
      ), //
      EnrolledDate: this.convertDate(
        this.http_rpm_patientList['PatientEnrolledDetails'].AssignedDate
      ), //
      VitalIds: VitalIdAyrray, //
    };
    if (req_body['ConsultationDate'] == 'NaN-NaN-NaN') {
      req_body['ConsultationDate'] = null;
    }
    var that = this;
    this.rpm.rpm_post('/api/patient/updatepatientprogram', req_body).then(
      (data) => {
        this.loading = false;
        this.auth.reloadPatientList('PatientList Updated');
        var i = 0;
        for (let x of that.http_rpm_patientList.PatientDevicesDetails
          .PatientDeviceInfos) {
          that.RemoveDevice(i);
          i++;
        }
      },
      (err) => {
        this.loading = false;
        alert('Something Went Wrong!!');
      }
    );
  }

  RemoveDevice(index: any) {
    this.pid = sessionStorage.getItem('PatientId');
    if (
      this.http_rpm_patientList['PatientDevicesDetails'].PatientDeviceInfos[
        index
      ].DeviceNumber
    ) {
      var req_body = {
        PatientId: parseInt(this.pid),
        VendorId: 1,
        DeviceId:
          this.http_rpm_patientList['PatientDevicesDetails'].PatientDeviceInfos[
            index
          ].DeviceNumber,
        DeviceModel:
          this.http_rpm_patientList['PatientDevicesDetails'].PatientDeviceInfos[
            index
          ].DeviceModel,
        PatientNumber:
          this.http_rpm_patientList['PatientDevicesDetails'].PatientDeviceInfos[
            index
          ].UserName,
        FirstName: this.http_rpm_patientList['PatientDetails'].FirstName,
        LastName: this.http_rpm_patientList['PatientDetails'].LastName,
        TimeZoneOffset: this.http_rpm_patientList['PatientDetails'].TimeZoneID,
      };

      this.rpm.rpm_post('/api/device/removedevice/iglucose', req_body).then(
        (data) => {

          this.showDialog = true;
          this.confirmDialog.showConfirmDialog(
            `Device Removed Successfully`,
            'Success',
            () => {
              null
            },
            false
          );

        },
        (err:any) => {
          this.showDialog = true;
          this.confirmDialog.showConfirmDialog(
            err.error.message,
            'Error',
            () => {
              null
            },
            false
          );

        }
      );
    }
  }
  billingType: any;
  billingtypeVariable: any;
  getbillingCycleType() {
    var that = this;
    that.rpm.rpm_get('/api/patient/getbillingtype').then((data) => {
      this.billingType = data;
      this.billingtypeVariable = this.billingType.Provider;
      sessionStorage.setItem('billingType', this.billingType.Provider);
    });
  }
  unlockAccountStatus = false;
  AccountLockData: any;
  unlockResponse: any;
  UnlockAccount() {
    this.currentpPatientId = sessionStorage.getItem('PatientId');
    var patientdata = { Patientid: this.currentpPatientId };

    this.rpm.rpm_post(`/api/authorization/UnlockUser`, patientdata).then(
      (data) => {
        this.unlockResponse = data;
        this.unlockAccountStatus = false;
        alert('Successfully Unlock the Account');
      },
      (err) => {
        console.log(err);
      }
    );
  }
  getUserAccountLockData() {
    this.currentpPatientId = sessionStorage.getItem('PatientId');
    var patientdata = { Patientid: this.currentpPatientId };

    this.rpm.rpm_post(`/api/users/UserStatusCheck`, patientdata).then(
      (data) => {
        this.AccountLockData = data;

        this.unlockAccountStatus = this.AccountLockData.isLocked;
      },
      (err) => {
        console.log(err);
      }
    );
  }

  deleteDocument(documentId: any) {
    this.rpm
      .rpm_post(
        `/api/patient/deletepatientdocuments?documentid=${documentId}`,
        {}
      )
      .then(
        (data) => {
          alert('Document Delete Successfully');
          this.getReload();
        },
        (err) => {
          console.log(err);
        }
      );
  }

  getReload() {
    this.rpm
      .rpm_get(
        `/api/patient/getpatient?PatientId=${this.patient_id}&PatientprogramId=${this.program_id}`
      )
      .then((data) => {
        this.http_rpm_patientList = data;
        this.documentData =
          this.http_rpm_patientList.PatientDocumentDetails.PatientDocumentinfos;
      });
  }
  // Video Template:
  disconnect() {
    this.videoCallConnected = false;
    var patientusername = this.http_rpm_patientList.PatientDetails.UserName;
    this.rpm
      .rpm_get(
        `/api/comm/VideoRoom?PatientId=${patientusername}&IsNotify=true&isCallActive=false`
      )
      .then(
        async (body) => {},
        (error: any) => {
          console.log('error')
          alert(error.error);
        }
      );

    try {
      if (
        this.twilioService &&
        this.twilioService.roomObj != null &&
        this.twilioService.roomObj != undefined
      ) {
        this.twilioService.roomObj.disconnect();
        this.twilioService.roomObj = null;
        this.twilioService.previewing = false;
        this.videovar = false;
      } else {
        console.log(this.twilioService);
      }
      this.twilioService.videoConnected = false;
    } catch (err) {
      console.log(err);
      this.videovar = false;
    }
  }
  disconnectVideo() {
    this.videoOnVariable = false;
    this.stopVideoTimer();
    this.callType = null;
    this.disconnect();
    this.showNoteModal=false;
    //this.OnCancelNotesTimer();
  }
  videoCallConnected = false;
  videovar: any;

  connect() {
    let accessToken = this.access_tokan;
    this.videoCallConnected = true;
    this.videovar = true;

    this.twilioService.connectToRoom(accessToken, {
      name: this.room_name,
      audio: true,
      video: {
        width: { ideal: 1300, max: 1920 },
        height: { ideal: 700, max: 1080 },
        frameRate: { ideal: 24, max: 30 },
      },

      bandwidthProfile: {
        video: {
          mode: 'collaboration',
          renderDimensions: {
            high: { height: 1080, width: 1980 },
            standard: { height: 720, width: 1280 },
            low: { height: 176, width: 144 },
          },

        },
      },
    });
    this.twilioService.isVideoOn = false;
    this.twilioService.microphone = true;

  }

  mute() {
    this.twilioService.mute();
  }

  unmute() {
    this.twilioService.unmute();
  }

  videoToken: any;
  VideoRedial(redial: boolean) {
    var patientusername = this.http_rpm_patientList.PatientDetails.UserName;
    this.twilioService.dialing = true;
    this.videoOnVariable = true;

    this.rpm
      .rpm_get(
        `/api/comm/VideoRoom?PatientId=${patientusername}&IsNotify=true&isCallActive=true`
      )
      .then(
        async (body) => {
          if (body) {
            this.videoToken = body;

            this.access_tokan = this.videoToken.token;
            this.room_name = this.videoToken.room;
            if (this.twilioService) {
              this.twilioService.microphone = false;
              this.twilioService.cameraOnSwitch = false;
            }

            this.connect();

            if (!redial) {
              this.videoOnVariable = true;
              this.callType = 'VIDEO';
              this.startVideoTimer();
            }
          }
        },
        (error: any) => {
          alert(error.error);
        }
      );
  }

  getVideoconnection(redial: boolean) {
    if (this.videoOnVariable == false) {
      var patientusername = this.http_rpm_patientList.PatientDetails.UserName;
      this.twilioService.dialing = true;
      this.twilioService.isVideoOn = true;


      this.rpm
        .rpm_get(
          `/api/comm/VideoRoom?PatientId=${patientusername}&IsNotify=true&isCallActive=true`
        )
        .then(
          async (body) => {
            if (body) {
              this.videoToken = body;

              this.access_tokan = this.videoToken.token;
              this.room_name = this.videoToken.room;
              if (this.twilioService) {
                this.twilioService.microphone = false;
              }

              this.connect();

              if (!redial) {
                this.videoOnVariable = true;
                this.callType = 'VIDEO';
                this.startVideoTimer();
              }
            }
          },
          (error: any) => {
            alert(error.error);
          }
        );
    } else {
    }
  }
  cameraVarOn = false;

  disableVideo() {
    this.twilioService.cameraOff();
  }
  enableVideo() {
    this.twilioService.cameraOn();
  }
  // screenShare()
  // {
  //   this.twilioService.screenShare()
  // }

  videoCallTimervalue = 0;
  videoTimer: any;

  interval4: any;

  startVideoTimer() {
    this.callTimerEnabled = true;
    if (this.interval4) {
      clearInterval(this.interval4);
    }
    this.interval4 = setInterval(() => {
      this.videoCallTimervalue = this.videoCallTimervalue + 1;
      this.time_convert(this.videoCallTimervalue);

      if (this.calltimerValue === 86400) {
        clearInterval(this.interval4);
      }
    }, 1000);
  }
  stopVideoTimer() {
    //New Change 25/07/2023
    this.videoCallTimervalue = 0;
    clearInterval(this.interval4);
    this.callTimerEnabled = false;
  }

  time_convert(num: any) {
    var hours = Math.floor(num / 60);
    var minutes = num % 60;
    this.videoTimer = hours + ':' + minutes;
  }
  // Chat Code Start

  openChatPanel() {
    this.chatVariable = true;
    this.patientchatservice.setChatPanelOpen(true);
  }

  CloseChatPanlBlock() {
    this.chatVariable = false;
    this.patientchatservice.setChatPanelOpen(false);
  }
  updateUnreadCount(count: number) {
    this.unreadCount = count;
  }
  convertToLocalTimeOnly(stillUtc: any) {
    if (stillUtc) {
      if (stillUtc.includes('+')) {
        var temp = stillUtc.split('+');
        stillUtc = temp[0];
      }
    }
    stillUtc = stillUtc + 'Z';
    const local = dayjs.utc(stillUtc).local().format('HH:mm:ss');
    return local;
  }

  getTimefromDate(text: any) {
    const myArray = text.split(' ');
    return myArray[1];
  }

  // Report code

  doc: any;
  DownloadStatus = false;
  private httpPatient: any;
  private httpHealthTrend: any;
  private httpVitalData: any;
  private httpMedicationData: any;
  private httpSymptoms: any;
  private httpSMSData: any;
  private httpbillingInfo: any;
  private http7VitalData: any;
  private selectedPatient: any;
  private selectedProgram: any;
  private startDateReport: any;
  private endDateReport: any;
  private HtmlGraph: any;

  downloadReport() {
    this.DownloadStatus = true;
    this.loadPatientReportData();
  }
  async loadPatientReportData() {
    this.selectedPatient = sessionStorage.getItem('PatientId');
    this.selectedProgram = sessionStorage.getItem('ProgramId');
    var startDate = this.convertDate(this.ThirtyDaysAgo);
    var endDate = this.convertDate(this.Today);

    startDate = startDate + 'T00:00:00';
    endDate = endDate + 'T23:59:59';
    try {
      this.startDateReport = this.auth.ConvertToUTCRangeInput(
        new Date(startDate)
      );
      this.endDateReport = this.auth.ConvertToUTCRangeInput(new Date(endDate));

      this.httpPatient = await this.PatientReportapi.getPatientInfo(
        this.selectedPatient,
        this.selectedProgram
      );
      this.patientStatusData =
        this.httpPatient?.PatientProgramdetails?.Status || '';
      this.httpHealthTrend = await this.PatientReportapi.getTrends(
        this.selectedPatient,
        this.selectedProgram,
        this.startDateReport,
        this.endDateReport
      );

      this.httpVitalData = await this.PatientReportapi.getVitalReading(
        this.selectedPatient,
        this.selectedProgram,
        this.startDateReport,
        this.endDateReport
      );

      this.httpMedicationData = await this.PatientReportapi.getMedication(
        this.selectedPatient,
        this.selectedProgram
      );

      this.httpbillingInfo = await this.PatientReportapi.getBillingData(
        this.selectedPatient,
        this.selectedProgram,
        this.endDateReport
      );

      this.httpSymptoms = await this.PatientReportapi.getPatientSymptoms(
        this.selectedPatient,
        this.selectedProgram
      );

      this.httpSMSData = await this.PatientReportapi.getSMSData(
        this.selectedPatient,
        this.selectedProgram,
        this.startDateReport,
        this.endDateReport
      );

      const startDate7Days = this.auth.ConvertToUTCRangeInput(
        new Date(
          new Date(this.endDateReport).setDate(
            new Date(this.endDateReport).getDate() - 7
          )
        )
      );

      this.http7VitalData = await this.PatientReportapi.getVitalReading7Days(
        this.selectedPatient,
        this.selectedProgram,
        startDate7Days,
        endDate
      );

      console.log('✅ Patient data loaded successfully');
      this.downloadPatientReport();
    } catch (error) {
      console.error('🚨 Error loading patient data:', error);
    }
  }
  downloadPatientReport() {
    this.patientStatusData = this.httpPatient.PatientProgramdetails.Status;
    this.patientProgramname =
      this.httpPatient['PatientProgramdetails'].ProgramName;
    this.reportStart();
    this.getPatientAndProgramInfo();
  }

  reportStart() {
    this.doc = new jsPDF();
    this.patientdownloadService.generateReportHeading(
      this.doc,
      this.startDateReport,
      this.endDateReport
    );
    this.patientdownloadService.generatePatientAndProgramInfo(
      this.doc,
      this.httpPatient,
      this.programDetails,
      this.patientProgramname
    );
    this.doc.addPage();
    this.patientdownloadService.generateReportProgramDetails(
      this.doc,
      this.httpPatient,
      this.programDetails
    );
    this.getHealthTrendsReport();
  }

  async getHealthTrendsReport() {
    if (!this.http_healthtrends || !this.http_healthtrends.Values) {
      this.setEmptyGraphReport();
      return;
    }

    const { chartData, chartLabels } =
      await this.patientdownloadService.processHealthTrends(
        this.doc,
        this.http_healthtrends
      );
    this.lineChartData = chartData;
    this.lineChartLabels = chartLabels;
  }

  setEmptyGraphReport() {
    const { chartData, chartLabels } =
      this.patientdownloadService.generateEmptyChartData();
    this.lineChartData = chartData;
    this.lineChartLabels = chartLabels;
  }

  async getPatientAndProgramInfo() {
    this.patientStatusData = this.httpPatient.PatientProgramdetails.Status;
    this.PatientCriticalAlerts =
      this.patientdownloadService.extractCriticalAlerts(this.httpSymptoms);
    this.patientdownloadService.generateProgramGoalsReport(
      this.doc,
      this.httpPatient.PatientProgramGoals,
      this.PatientCriticalAlerts
    );
    this.doc.addPage();
    await this.patientdownloadService.getReportNotes(
      this.doc,
      this.startDateReport,
      this.endDateReport,
      this.selectedPatient,
      this.selectedProgram,
      this.patientProgramname
    );
    if (this.patientProgramname == 'CCM' || this.patientProgramname == 'PCM') {
      this.patientdownloadService.generatePatientSummaryReport(
        this.doc,
        this.patientProgramname,
        this.httpSymptoms,
        this.httpMedicationData,
        this.httpbillingInfo,
        this.httpSMSData
      );
      this.doc.save('PatientReport.pdf');
      this.DownloadStatus = false;
    } else {
      this.HtmlGraph = document.querySelector('#pdfGraph');
      await this.patientdownloadService.captureHealthTrendsChart(
        this.doc,
        this.HtmlGraph
      );
      this.patientdownloadService.generateHealthTrendsTable(
        this.doc,
        this.httpVitalData
      );
      this.patientdownloadService.generateDaysVitals(this.httpVitalData);
      this.patientdownloadService.generatePatientSummaryReport(
        this.doc,
        this.patientProgramname,
        this.httpSymptoms,
        this.httpMedicationData,
        this.BillingInfo,
        this.httpSMSData
      );
      this.patientdownloadService.generateVitalReadingSummary(this.doc);
      this.DownloadStatus = false;
      this.doc.save('PatientReport.pdf');
    }
  }
  closenoterDialogUpdateModal(){
    this.showNoteupdateModal = false;
  }
  ngOnDestroy() {
    this.clearIntervals();
    this.destroy$.next();
    this.destroy$.complete();
    this.patientService.clearCachedPatientData();
    this.patientchatservice.initialized = false;
    if (this.unreadSubscription) {
      this.unreadSubscription.unsubscribe();
    }
    if (this.chatHistoryDataSubscription) {
      this.chatHistoryDataSubscription.unsubscribe();
    }
  }

  closesmsDialogModal(){
    this.smsdialogpanel=false;
  }


}
