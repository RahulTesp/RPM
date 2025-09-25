
import {
  Component,
  OnInit,
  TemplateRef,
  ViewChild,
  Output,
  EventEmitter,
  ElementRef,
} from '@angular/core';
import { FormControl, FormGroup } from '@angular/forms';
import { MatDialog } from '@angular/material/dialog';
import { RPMService } from '../../sevices/rpm.service';
import { Router } from '@angular/router';
import { jsPDF } from 'jspdf';
import { AuthService } from 'src/app/services/auth.service';
import { DatePipe } from '@angular/common';
import { PatientDataDetailsService } from '../patient-detail-page/Models/service/patient-data-details.service';
import { Subscription } from 'rxjs';
import { ReportDataService } from './services/report-data.service';
import { DownloadPatientReportService } from './services/download-patient-report.service';
import {
  CHART_OPTIONS,
  CHART_COLORS,
  CHART_LEGEND,
  CHART_TYPE,
} from './interfaces/chart-config-interface';
import { PatientReportApiService } from './services/patient-report-api.service';
import { PatientUtilService } from '../patient-detail-page/Models/service/patient-util.service';
import * as FileSaver from 'file-saver';
import { HttpClient } from '@angular/common/http';
import dayjs from 'dayjs';
import utc from 'dayjs/plugin/utc';
import timezone from 'dayjs/plugin/timezone';

@Component({
  selector: 'app-reports',
  templateUrl: './reports.component.html',
  styleUrls: ['./reports.component.scss'],
})
export class ReportsComponent implements OnInit {
  @Output() emitter: EventEmitter<string> = new EventEmitter<string>();

  @ViewChild('autocompletebilling') autocompletebilling: any;
  @ViewChild('pdfData') chartElementRef: ElementRef;
  campaignOne: FormGroup;
  accessRights: any;
  master_data: any;
  clinics: any;
  clinic: any;
  cptcode: any;
  isMonth = false;
  selectedPatient: any;
  selectedProgram: any;
  selectedProgramName: any;
  startDate: string;
  endDate: string;
  rolelist: any;
  month: any;
  month1: any;
  http_getActivePatientList: any;
  keyword = 'searchField';

  //PatientReport variables
  fromDate: any;
  enddate: any;
  roles: any;
  http_all_patientList: any;
  program_name: any;
  patientVital: any;
  programDetails: any;
  patientProgramname: any;
  idProgram: any
  patientListArray: any;
  today = new Date();
  http_healthtrends: any;
  sevenDaysAgo = new Date(this.today).setDate(this.today.getDate() - 30);
  ThirtyDaysAgo = new Date(this.today).setDate(this.today.getDate() - 30);
  SevensDaysAgo = new Date(this.today).setDate(this.today.getDate() - 7);
  Tomorrow = new Date(this.today).setDate(this.today.getDate() + 1);
  Today = new Date();
  keyword1 = 'patientsearchField';
  http_rpm_patient: any;
  http_rpmpdetail: any;
  http_medication_data: any;
  http_get_symptoms: any;
  temp_symptoms: any;
  PatientCriticalAlerts: any;
  BillingInfo: any;
  patientStatusData: any;
  patiantVital: any;
  VitalsConsolidated: any;
  http_vitalBloodPressure: any;
  http_vitalGlucoseData: any;
  http_VitalOxygen: any;
  http_VitalWight: any;
  http_vitalData: any;
  http_NewNoteList: any;
  doc: any;
  add_patient_masterdata: any;
  RptStartDate: any;
  RptEndDate: any;
  http_NewNote: any;
  patientProgramName: any;
  http_NewNoteList_C: any;
  currentPatient: any;
  currentProgram: any;
  listB: any;
  listA: any;
  listC: any;
  symptomh: any;
  HtmlGraph: any;
  Notesh: any;
  retArr: Array<string>;
  http_SmsData: any;

  private subscriptions: Subscription[] = [];

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

  public lineChartOptions: any = CHART_OPTIONS;
  public lineChartColors: Array<any> = CHART_COLORS;
  public lineChartLegend: boolean = CHART_LEGEND;
  public lineChartType: any = CHART_TYPE;

  minValue: number = 30;
  maxValue: number = 75;
  middle: number = 20;
  minDate: any;
  maxDate: any;
  maxCallLogDate: any;
  constructor(
    public dialog: MatDialog,
    public rpmservice: RPMService,
    private _router: Router,
    public datepipe: DatePipe,
    private httpobj: HttpClient,
    private auth: AuthService,
    private patientService: PatientDataDetailsService,
    private patientreportService: ReportDataService,
    private patientdownloadService: DownloadPatientReportService,
    private patientUtilService: PatientUtilService,
    private PatientReportapi: PatientReportApiService
  ) {}
  range = new FormGroup({
    start: new FormControl(),
    end: new FormControl(),
  });

  loadPatients() {
    this.patientreportService
      .getPatientList()
      .then((data) => {
        // Call billing detail method in component
        this.programChangeBillingDetail(this.programType);
      })
      .catch((error) => {
        console.error('Error loading patients:', error);
      });
  }

  openDatepicker() {
    this.range.reset();
  }

  resetMaxDate() {
    this.maxDate = new Date();
  }

  setMaxDate(date: HTMLInputElement) {
    this.minDate = new Date(date.value);
    var maxdate = new Date(
      new Date(this.minDate).setDate(this.minDate.getDate() + 30)
    );
    this.maxDate = new Date(maxdate);
  }

  setMaxDateCallLog(date: HTMLInputElement) {
    this.minDate = new Date(date.value);
    var maxdate = new Date(
      new Date(this.minDate).setDate(this.minDate.getDate() - 90)
    );
    this.maxDate = new Date(maxdate);

    this.callnonEstrange.controls['start'].setValue(this.maxDate);
    this.callnonEstrange.controls['end'].setValue(this.minDate);
  }
  setMaxDateCallInfo(date: HTMLInputElement) {
    this.minDate = new Date(date.value);
    var maxdate = new Date(
      new Date(this.minDate).setDate(this.minDate.getDate() - 90)
    );
    this.maxDate = new Date(maxdate);

    this.calllogrange.controls['start'].setValue(this.maxDate);
    this.calllogrange.controls['end'].setValue(this.minDate);
  }
  getTimeZoneOffsetDate() {
    let offsetValue = new Date().getTimezoneOffset();
    var offset;
    offset = offsetValue <= 0 ? '+' : '-';
    var dateoffsetToSent = offset + Math.abs(offsetValue);
    return dateoffsetToSent;
  }

  isOpen = false;
  ngOnInit(): void {
    const patientId = sessionStorage.getItem('PatientId') || '';
    const programId = sessionStorage.getItem('ProgramId') || '';
    this.buttonclick = true;
    this.buttonclick1 = true;
    this.call_variable = 1;
    this.callSelected = 1;
    this.billing_variable = 1;
    this.getBillingCode();
    this.maxDate = new Date();
    this.maxCallLogDate = new Date();
    this.programType = 'RPM';

    this.accessRights = sessionStorage.getItem('useraccessrights');
    this.accessRights = JSON.parse(this.accessRights);
    this.master_data = sessionStorage.getItem('add_patient_masterdata');
    if (this.master_data) {
      this.master_data = JSON.parse(this.master_data);
      this.clinics = this.master_data.ClinicDetails;
      this.programDetails = this.master_data.ProgramDetailsMasterData;
    }
    this.clinic = '';
    this.cptcode = '';
    const today = new Date();
    const month = today.getMonth();
    const year = today.getFullYear();
    this.campaignOne = new FormGroup({
      start: new FormControl(new Date(year, month, 1)),
      end: new FormControl(new Date(year, month, 20)),
    });
    this.rolelist = sessionStorage.getItem('Roles');
    this.rolelist = JSON.parse(this.rolelist);

    this.roles = sessionStorage.getItem('Roles');
    this.roles = JSON.parse(this.roles);
    this.subscriptions.push(
      this.patientreportService.users$.subscribe((users) => {
        this.http_userlist = users;
      })
    );

    // Load user data
    this.loadUsers();
    this.getBillingTypeProgram();
    this.variable = 1;

    this.subscriptions.push(
      this.patientreportService.allPatients$.subscribe((patients) => {
        this.http_all_patientList = patients;
        this.patientListArray = Array.isArray(patients)
          ? [...patients]
          : Object.values(patients);
      })
    );

    this.subscriptions.push(
      this.patientreportService.activePatients$.subscribe((patients) => {
        this.http_getActivePatientList = patients;
      })
    );
    this.loadPatients();
  }
  variable: any;
  toggle = true;
  toggle1 = true;
  toggle2 = true;
  toggle3 = true;
  status = 'Enable';

  Patient_Health() {
    this.variable = 1;

    this.toggle = !this.toggle;
    this.toggle1 = true;
    this.toggle2 = true;
  }
  Billing() {
    this.variable = 2;

    this.toggle1 = !this.toggle1;
    this.toggle2 = true;
    this.toggle = true;
  }
  Team_Performance() {
    this.variable = 3;

    this.toggle2 = !this.toggle2;
    this.toggle1 = true;
    this.toggle = true;
  }
  Device() {
    this.variable = 4;

    this.toggle3 = !this.toggle3;
    this.toggle1 = true;
    this.toggle = true;
  }

  menu = [
    {
      menu_id: 1,
      menu_title: 'Patient Health',
    },
    {
      menu_id: 2,
      menu_title: 'Billing',
    },
    // {
    //   menu_id : 3,
    //   menu_title:'Team Performance'
    // },
    // {
    //   menu_id:4,
    //   menu_title:'Device'
    // }
  ];
  CurrentMenu: any = 1;
  // Change Main Screen

  ChangeScreen(button: any) {
    this.CurrentMenu = button;
    this.selectedPatient = null;
    this.selectedProgram = null;
    this.patientIsMonth = false;
    this.isMonth = false;
    this.month = null;
    this.month1 = null;
    this.clinic = '';
    this.cptcode = '';
    switch (button) {
      case 1:
        this.variable = 1;
        break;

      case 2:
        this.variable = 2;
        this.fileformatvariable = undefined;
        break;

      case 3:
        this.variable = 3;
        break;

      case 4:
        this.variable = 4;
        break;
      case 5:
        this.variable = 5;
        break;
      default:
        this.variable = 1;
        break;
    }
  }

  patientpage: any;

  openDialog(templateRef: TemplateRef<any>) {
    this.dialog.open(templateRef);
  }

  dateRangeScheduleChange(
    dateRangeStart: HTMLInputElement,
    dateRangeEnd: HTMLInputElement
  ) {
    this.startDate = this.patientreportService.convertDate(
      dateRangeStart.value
    );
    this.endDate = this.patientreportService.convertDate(dateRangeEnd.value);
  }
  url: any;
  buttonclick: any;
  async getBillingReport(startDate: any, endDate: any): Promise<void> {
    this.buttonclick = false;

    try {
      if (!this.isMonth) {
        this.startDate = this.patientreportService.convertDate(startDate.value);
        this.endDate = this.patientreportService.convertDate(endDate.value);
      }

      this.url = await this.patientreportService.getBillingReport(
        this.startDate,
        this.endDate,
        this.month,
        this.clinic,
        this.cptcode,
        this.isMonth,
        this.selectedBillingPatient,
        this.fileformatvariable
      );

      this.downloadFile(this.url);
    } catch (error: any) {
      alert(error.message);
    } finally {
      this.buttonclick = true;
    }
  }

  async getBillingReportDetail(startDate: any, endDate: any): Promise<void> {
    this.buttonclick = false;

    try {
      this.url = await this.patientreportService.getBillingReportDetail(
        this.startDate,
        this.endDate,
        this.month,
        this.clinic,
        this.cptcode,
        this.selectedBillingDetailPatient,
        this.fileformatvariable,
        this.programType
      );

      this.downloadFile(this.url);
    } catch (error: any) {
      alert(error.message);
    } finally {
      this.buttonclick = true;
    }
  }

  downloadPdf(pdfUrl: any, pdfName: any) {
    FileSaver.saveAs(pdfUrl, pdfName);
  }
  backtohome() {
    let route = '/admin/home';
    this._router.navigate([route]);
  }
  radioChange(event: any) {
    if (event.value == 1) {
      this.campaignOne.reset();
      this.isMonth = true;
    } else if (event.value == 0) {
      const today = new Date();
      const month = today.getMonth();
      const year = today.getFullYear();
      this.campaignOne = new FormGroup({
        start: new FormControl(new Date(year, month, 1)),
        end: new FormControl(new Date(year, month, 20)),
      });
      this.isMonth = false;
      this.month = null;
    }
  }
  patientIsMonth = false;
  radioChangePatient(event: any) {
    if (event.value == 1) {
      this.range.reset();
      this.patientIsMonth = true;
      this.fromDate = null;
      this.enddate = null;
    } else if (event.value == 0) {
      this.patientIsMonth = false;
      this.month1 = null;
    }
  }
  selectedBillingPatient: any;
  selectedBillingPatientProgram: any;
  selectedBillingDetailPatient: any;
  selectEvent(item: any) {
    this.selectedBillingPatient = item.PatientId;
    this.selectedBillingPatientProgram = item.PatientProgramId;
  }

  selectEvent1(item: any) {
    this.selectedPatient = item.PatientId;
    this.selectedProgram = item.PatientProgramId;
  }

  onChangeSearch(val: string) {
    this.selectedPatient = val;
    this.selectedBillingPatient = val;
  }
  // New Code 24/04/2023
  selectBillingEvent(item: any) {
    this.selectedBillingDetailPatient = item.PatientId;
    this.selectedBillingPatientProgram = item.PatientProgramId;
  }
  OnChangeSeachDetail(val: string) {
    this.selectedBillingDetailPatient = val;
  }
  onClearSearchbilldetail(event: any) {
    this.selectedBillingDetailPatient = null;
  }
  onChangeSearch1(val: string) {
    this.selectedPatient = val;
  }

  onFocused(e: any) {
    // do something when input is focused
  }
  onClearSearch(event: any) {
    this.selectedBillingPatient = null;
  }

  onClearSearch1(event: any) {
    this.selectedPatient = null;
    this.selectedProgram = null;
  }
  onCancel() {
    this.buttonclick1 = true;
  }

  downloadFile(FileName: any) {
    const a = document.createElement('a');
    a.href = FileName;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
  }

  fileformatvariable: any;
  fileformatvariableExcel: any;
  fileformatvariablepdf: any;
  fileformatChange(event: any) {
    if (event.value == 'excel') {
      this.fileformatvariable = 'xlsx';
    } else if (event.value == 'pdf') {
      this.fileformatvariable = 'pdf';
    }
  }

  buttonclick1 = true;

  dateRangeSet(StartDate: any, EndDate: any) {
    if (new Date(EndDate.value) > new Date()) {
      alert('Cannot Download report for future date.');
      return;
    }
    this.buttonclick1 = false;
    if (this.selectedPatient == null || this.selectedPatient == undefined) {
      alert('Please select a Patient.');
      this.buttonclick1 = true;
      return;
    }
    if (this.patientIsMonth) {
      if (!this.month1) {
        alert('Please select a Month.');
        this.buttonclick1 = true;
        return;
      }
      var dateofmonth = this.month1.toString();
      var dt = dateofmonth.split('-');
      var year = parseInt(dt[0]);
      var month = parseInt(dt[1]);
      var firstDay = new Date(year, month - 1, 1);
      var lastDay = new Date(year, month, 0);
      this.RptStartDate = this.patientreportService.convertDate(firstDay);
      this.RptEndDate = this.patientreportService.convertDate(lastDay);
      if (new Date(this.RptEndDate) > new Date()) {
        alert('Report is not available for the month.');
        this.buttonclick1 = true;
        return;
      }
    } else {
      if (StartDate.value == '' || EndDate.value == '') {
        alert('Please select a valid Date Range.');
        this.buttonclick1 = true;
        return;
      }
      this.RptStartDate = this.patientreportService.convertDate(
        StartDate.value
      );
      this.RptEndDate = this.patientreportService.convertDate(EndDate.value);
    }
    if (
      this.RptStartDate == null ||
      this.RptStartDate == undefined ||
      this.RptEndDate == null ||
      this.RptEndDate == undefined
    ) {
      alert('Please select a valid Date Range.');
      this.buttonclick1 = true;
      return;
    }
    this.loadPatientData();
  }

  keyword2 = 'UserName';
  selectedcareTeam: any;
  selectedcareTeamId: any;
  selectCallInfoEvent(item: any) {
    this.selectedcareTeam = item.UserName;
    this.selectedcareTeamId = item.UserId;
  }

  onChangeCallinfoSearch(val: string) {
    this.selectedcareTeam = val;
    this.selectedcareTeamId = val;
  }

  onClearCallinfoSearch(event: any) {
    this.selectedcareTeam = null;
    this.selectedcareTeamId = null;
  }

  http_userlist: any;

  loadUsers() {
    this.patientreportService.getAllUsers().catch((error) => {
      console.error('Error loading users:', error);
    });
  }
  calllogrange = new FormGroup({
    start: new FormControl(),
    end: new FormControl(),
  });
  careteam: any;
  calllogStart: any;
  calllogEnd: any;

  downloadCallInfo(startDate: any, EndDate: any) {
    {
      var that = this;
      this.calllogStart = new Date(this.calllogrange.controls.start.value);
      this.calllogEnd = new Date(this.calllogrange.controls.end.value);

      if (this.selectedcareTeamId == undefined) {
        this.careteam = null;
      } else {
        this.careteam = this.selectedcareTeamId;
      }
      if (
        startDate.value == '' ||
        startDate.value == undefined ||
        EndDate.value == '' ||
        EndDate.value == undefined
      ) {
        alert('Please select a valid Date Range.');
        this.buttonclick = true;
        this.calllogrange.reset();
        return;
      }
      var req_body = {
        StartDate: this.patientreportService.convertDate(startDate.value),
        EndDate: this.patientreportService.convertDate(EndDate.value),
        Userid: this.careteam,
      };

      that.rpmservice.rpm_post(`/api/notes/CallLogByCareTeam`, req_body).then(
        (data) => {
          this.url = data;
          this.downloadFile(this.url);
          this.buttonclick = true;
        },
        (err) => {
          alert('No Report avaliable for the period.');
          this.buttonclick = true;
          this.calllogrange.reset();
        }
      );
    }
  }
  call_variable: any;
  callSelected: any;
  getMenucallInfo(menuVariable: any) {
    this.callSelected = menuVariable;
    this.call_variable = menuVariable;
  }
  nonEstCallStart: any;
  nonEstCallEnd: any;
  callnonEstrange = new FormGroup({
    start: new FormControl(),
    end: new FormControl(),
  });

  downloadNonEstablishedCallInfo(): void {
    const startValue = this.callnonEstrange.controls.start.value;
    const endValue = this.callnonEstrange.controls.end.value;
    this.buttonclick = false;
    if (!startValue || !endValue) {
      alert('Please select a valid Date Range.');
      this.buttonclick = true;
      this.callnonEstrange.reset();
      return;
    }

    const roleId = this.roles[0].Id;

    this.patientreportService
      .downloadNonEstablishedCallInfo(startValue, endValue, roleId)
      .then((url) => {
        this.url = url;
        this.downloadFile(this.url);
        this.buttonclick = true;
      })
      .catch((err) => {
        alert('No Report available for the period.');
        this.buttonclick = true;
        this.callnonEstrange.reset();
      });
  }
  billing_variable: any;
  getMenuBillingInfo(data: any) {
    this.billing_variable = data;
    this.call_variable = data;
    this.clinic = '';
    this.cptcode = '';
    this.selectedBillingPatient = null;
    this.fileformatvariable = undefined;
    this.onClearSearch(null);
    this.programType = 'RPM';
    this.campaignOne.reset();
    this.programChangeBillingDetail(this.programType);
    this.month = null;
    this.isMonth = false;
  }

  billingCode: any;

  async getBillingCode(): Promise<void> {
    try {
      this.billingCode = await this.patientreportService.getBillingCode();
    } catch (error) {
      console.error('Error loading billing codes:', error);
    }
  }

  programType: any;
  programChange(event: any) {
    this.filterBillingData();
  }
  billingTypeProgramArray: any;
  billingProgramNameTempArray: any;
  billingProgramNameArray: any;

  getBillingTypeProgram() {
    this.patientreportService
      .getBillingTypePrograms()
      .then((data) => {
        this.billingProgramNameTempArray = data;
        this.filterBillingData();
        this.billingTypeProgramArray =
          this.patientreportService.removeDuplicateObjects(
            this.billingProgramNameTempArray,
            'ProgramType'
          );
      })
      .catch((error) => {
        console.error('Error in getBillingTypeProgram:', error);
      });
  }

  filterBillingData() {
    this.billingProgramNameArray = this.billingProgramNameTempArray.filter(
      (data: any) => {
        return data.ProgramType == this.programType;
      }
    );
  }
  removeDuplicateObjects(array: any, property: any) {
    const uniqueIds: any[] = [];

    const unique = array.filter((element: { [x: string]: any }) => {
      const isDuplicate = uniqueIds.includes(element[property]);

      if (!isDuplicate) {
        uniqueIds.push(element[property]);

        return true;
      }

      return false;
    });
    return unique;
  }
  billingDetailPatientList: any;
  clinicBillingDetails: any;
  programChangeBillingDetail(e: any) {
    this.billingDetailPatientList = [];
    this.selectedBillingPatient = null;
    this.selectedBillingDetailPatient = null;

    if (this.autocompletebilling) {
      this.autocompletebilling.clear();
    }
    this.onClearSearchbilldetail(null);

    this.billingDetailPatientList = this.http_getActivePatientList.filter(
      (data: any) => {
        return data.Program == this.programType;
      }
    );
    this.clinicBillingDetails = this.removeDuplicateObjects(
      this.billingDetailPatientList,
      'ClinicId'
    );
  }

  async getMissingBillingReportDetail(
    startDate: any,
    endDate: any
  ): Promise<void> {
    this.buttonclick = false;

    try {
      // Convert date inputs
      this.startDate = this.patientreportService.convertDate(startDate.value);
      this.endDate = this.patientreportService.convertDate(endDate.value);

      this.url = await this.patientreportService.getMissingBillingReportDetail(
        this.startDate,
        this.endDate,
        this.month,
        this.clinic,
        this.cptcode,
        this.selectedBillingDetailPatient,
        this.fileformatvariable,
        this.programType
      );

      this.downloadFile(this.url);
    } catch (error: any) {
      alert(error.message);
    } finally {
      this.buttonclick = true;
    }
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

  // Report Patient Data
  currentY: number=15;
  healthtrendVitalNameArray: any;

  downloadPatientReport() {
    this.patientStatusData = this.http_rpm_patient.PatientProgramdetails.Status;
    this.patientProgramname =
      this.http_rpm_patient['PatientProgramdetails'].ProgramName;
    this.idProgram = this.http_rpm_patient['PatientProgramdetails'].ProgramId;
    this.reportStart();
    this.getPatientAndProgramInfo();
  }

  reportStart() {
    this.doc = new jsPDF();
    this.patientdownloadService.generateReportHeading(
      this.doc,
      this.RptStartDate,
      this.RptEndDate
    );
    this.patientdownloadService.generatePatientAndProgramInfo(
      this.doc,
      this.http_rpm_patient,
      this.programDetails,
      this.patientProgramname
    );
    this.doc.addPage();
    this.patientdownloadService.generateReportProgramDetails(
      this.doc,
      this.http_rpm_patient,
      this.programDetails
    );
    this.getHealthTrends();
  }

  async getHealthTrends() {
    if (!this.http_healthtrends || !this.http_healthtrends.Values) {
      this.setEmptyGraph();
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

  setEmptyGraph() {
    const { chartData, chartLabels } =
      this.patientdownloadService.generateEmptyChartData();
    this.lineChartData = chartData;
    this.lineChartLabels = chartLabels;
  }
  // Manjusha code change
  async getPatientAndProgramInfo() {
    this.patientStatusData = this.http_rpm_patient.PatientProgramdetails.Status;
    this.PatientCriticalAlerts =
      this.patientdownloadService.extractCriticalAlerts(this.http_get_symptoms);
    this.patientdownloadService.generateProgramGoalsReport(
      this.doc,
      this.http_rpm_patient.PatientProgramGoals,
      this.PatientCriticalAlerts
    );
    this.doc.addPage();
    const startDate = this.auth.ConvertToUTCRangeInput(
      new Date(this.RptStartDate + 'T00:00:00')
    );
    const endDate = this.auth.ConvertToUTCRangeInput(
      new Date(this.RptEndDate + 'T23:59:59')
    );
    await this.patientdownloadService.getReportNotes(
      this.doc,
      startDate,
      endDate,
      this.selectedPatient,
      this.selectedProgram,
      this.idProgram
    );
    if (this.patientProgramname == 'CCM' || this.patientProgramname == 'PCM') {
      this.patientdownloadService.generatePatientSummaryReport(
        this.doc,
        this.patientProgramname,
        this.http_get_symptoms,
        this.http_medication_data,
        this.BillingInfo,
        this.http_SmsData
      );
      this.doc.save('PatientReport.pdf');
      this.buttonclick1 = true;
    } else {
      this.doc.setFontSize(14);
      this.doc.text('Patient Health Trends', 15, this.currentY);
      this.doc.setDrawColor('black');
      const headingWidth = this.doc.getTextWidth('Patient Health Trends');
      this.doc.line(15, this.currentY + 1, 15 + headingWidth, this.currentY + 1);
      this.currentY += 20;
      await this.patientdownloadService.resetPosition();
      const allGraphs = Array.from(document.querySelectorAll('.pdfData'));

      for (let i = 0; i < allGraphs.length; i++) {
        const graph = allGraphs[i] as HTMLElement;
        const vitalTitle = this.healthtrendVitalNameArray[i] || `Chart ${i + 1}`;
        await this.patientdownloadService.captureHealthTrendsChart(this.doc, graph, vitalTitle);
      }
      this.currentY = this.patientdownloadService.getChartCurrentY();
      this.patientdownloadService.generateHealthTrendsTable(
        this.doc,
        this.http_vitalData
      );
      this.patientdownloadService.generateDaysVitals(this.http_vitalData);
      this.patientdownloadService.generatePatientSummaryReport(
        this.doc,
        this.patientProgramname,
        this.http_get_symptoms,
        this.http_medication_data,
        this.BillingInfo,
        this.http_SmsData
      );
      await this.patientdownloadService.generateVitalReadingSummary(this.doc,this.selectedPatient, this.selectedProgram);
      this.doc.save('PatientReport.pdf');
      this.buttonclick1 = true;
    }
  }

  VitalsSevenDaysConsolidated: any;
  http_7day_vitalData: any;

  async loadPatientData() {
    try {
      const startDate = this.auth.ConvertToUTCRangeInput(
        new Date(this.RptStartDate + 'T00:00:00')
      );
      const endDate = this.auth.ConvertToUTCRangeInput(
        new Date(this.RptEndDate + 'T23:59:59')
      );

      this.http_rpm_patient = await this.PatientReportapi.getPatientInfo(
        this.selectedPatient,
        this.selectedProgram
      );
      this.patientStatusData =
        this.http_rpm_patient?.PatientProgramdetails?.Status || '';

      this.http_healthtrends = await this.PatientReportapi.getTrends(
        this.selectedPatient,
        this.selectedProgram,
        startDate,
        endDate
      );
      this.http_vitalData = await this.PatientReportapi.getVitalReading(
        this.selectedPatient,
        this.selectedProgram,
        startDate,
        endDate
      );

      this.http_medication_data = await this.PatientReportapi.getMedication(
        this.selectedPatient,
        this.selectedProgram
      );
      this.BillingInfo = await this.PatientReportapi.getBillingData(
        this.selectedPatient,
        this.selectedProgram,
        this.RptEndDate
      );

      this.http_get_symptoms = await this.PatientReportapi.getPatientSymptoms(
        this.selectedPatient,
        this.selectedProgram
      );
      this.http_SmsData = await this.PatientReportapi.getSMSData(
        this.selectedPatient,
        this.selectedProgram,
        startDate,
        endDate
      );

      const startDate7Days = this.auth.ConvertToUTCRangeInput(
        new Date(
          new Date(this.RptEndDate).setDate(
            new Date(this.RptEndDate).getDate() - 7
          )
        )
      );

      this.http_7day_vitalData =
        await this.PatientReportapi.getVitalReading7Days(
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

  ngOnDestroy() {
    this.subscriptions.forEach((sub) => sub.unsubscribe());
  }
}
