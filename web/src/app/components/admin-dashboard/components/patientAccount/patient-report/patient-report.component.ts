import {
  Component,
  OnInit,
  TemplateRef,
  ViewChild,
  Output,
  EventEmitter,
} from '@angular/core';
import { UntypedFormControl, UntypedFormGroup } from '@angular/forms';

import { MatDialog } from '@angular/material/dialog';



import { Router } from '@angular/router';

import { jsPDF } from 'jspdf';
import autoTable from 'jspdf-autotable';
import { HttpClient } from '@angular/common/http';

import { AuthService } from 'src/app/services/auth.service';
import { DatePipe } from '@angular/common';
import moment from 'moment';

import html2canvas from 'html2canvas';
import { RPMService } from '../../../sevices/rpm.service';
import * as FileSaver from 'file-saver';






export interface PeriodicElement {
  name: string;
  action: string;
  date: string;
  /*  selection:string; */
  description: string;
  id: number;
}

@Component({
  selector: 'app-patient-report',
  templateUrl: './patient-report.component.html',
  styleUrls: ['./patient-report.component.scss'],
})
export class PatientReportComponent implements OnInit {
  @Output() emitter: EventEmitter<string> = new EventEmitter<string>();

  @ViewChild('autocompletebilling') autocompletebilling: any;

  campaignOne: UntypedFormGroup;
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
  //chart declarations
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
        // Format of min pan range depends on scale type
        x: null,
        y: null,
      },
      rangeMax: {
        // Format of max pan range depends on scale type
        x: null,
        y: null,
      },
      // Function called once panning is completed
      // Useful for dynamic data loading
      onPan: function (e: any) {
        console.log(`I was panned!!!`, e);
      },
    },
    zoom: {
      enabled: true,
      drag: false,
      mode: 'xy',

      rangeMin: {
        // Format of min zoom range depends on scale type
        x: null,
        y: null,
      },
      rangeMax: {
        // Format of max zoom range depends on scale type
        x: null,
        y: null,
      },

      // Speed of zoom via mouse wheel
      // (percentage of zoom on a wheel event)
      speed: 0.1,

      // Function called once zooming is completed
      // Useful for dynamic data loading
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
  minDate: any;
  maxDate: any;
  maxCallLogDate: any;
  constructor(
    public dialog: MatDialog,
    public rpmservice: RPMService,
    private _router: Router,
    public datepipe: DatePipe,
    private httpobj: HttpClient,
    private auth: AuthService
  ) {
    this.buttonclick = true;
    this.buttonclick1 = true;
    this.call_variable = 1;
    this.callSelected = 1;
    this.billing_variable = 1;

    this.maxDate = new Date();
    this.maxCallLogDate = new Date();
    this.programType = 'RPM';

    this.accessRights = sessionStorage.getItem('useraccessrights');
    this.accessRights = JSON.parse(this.accessRights);
    this.master_data = sessionStorage.getItem('add_patient_masterdata');

    this.clinic = '';
    this.cptcode = '';
    const today = new Date();
    const month = today.getMonth();
    const year = today.getFullYear();
    this.campaignOne = new UntypedFormGroup({
      start: new UntypedFormControl(new Date(year, month, 1)),
      end: new UntypedFormControl(new Date(year, month, 20)),
    });
    this.rolelist = sessionStorage.getItem('Roles');
    this.rolelist = JSON.parse(this.rolelist);

    this.roles = sessionStorage.getItem('Roles');
    this.roles = JSON.parse(this.roles);

    var todayDate = new Date().toISOString();
    // alert(todayDate);
    var offsetValue = this.getTimeZoneOffsetDate();

    this.getBillingTypeProgram();
  }
  range = new UntypedFormGroup({
    start: new UntypedFormControl(),
    end: new UntypedFormControl(),
  });

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

  convertToLocalTime(stillUtc: any) {
    stillUtc = stillUtc + 'Z';
    var local = moment(stillUtc).local().format('YYYY-MM-DD HH:mm:ss');
    return local;
  }

  isOpen = false;
  ngOnInit(): void {
    this.variable = 1;
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

  menu = [
    {
      menu_id: 1,
      menu_title: 'Patient Health',
    },
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

      default:
        this.variable = 1;
        break;
    }
  }

  patientpage: any;

  openDialog(templateRef: TemplateRef<any>) {
    this.dialog.open(templateRef);
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
  dateRangeScheduleChange(
    dateRangeStart: HTMLInputElement,
    dateRangeEnd: HTMLInputElement
  ) {
    this.startDate = this.convertDate(dateRangeStart.value);
    this.endDate = this.convertDate(dateRangeEnd.value);
  }
  url: any;
  buttonclick: any;

  downloadPdf(pdfUrl: any, pdfName: any) {
    FileSaver.saveAs(pdfUrl, pdfName);
  }
  backtohome() {
    let route = '/admin/patient-home';
    this._router.navigate([route]);
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
    this.selectedBillingPatient = val; // fetch remote data from here
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
    // FileSaver.saveAs("ConsentForm", FileName);
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
    console.log(event.value);
  }

  getPatientAndProgramInfo() {
    this.rpmservice.rpm_get(`/api/patients/getpatient`).then((data) => {
      this.http_rpm_patient = data;
      this.patientStatusData =
        this.http_rpm_patient.PatientProgramdetails.Status;
      this.getProgramGoals();
    });
  }
  getProgramGoals() {
    var that = this;

    that.temp_symptoms = that.http_get_symptoms;
    this.PatientCriticalAlerts = [];
    var tempEvents = this.http_get_symptoms.filter(
      (data: { Symptom: string }) => {
        return data.Symptom == 'Critical Event';
      }
    );
    for (let dataEvents of tempEvents) {
      var objEvents = {
        Priority: 'Critical',
        VitalAlert: dataEvents.Description,
        Time: dataEvents.SymptomStartDateTime,
      };
      this.PatientCriticalAlerts.push(objEvents);
    }
    this.Style_SetMainHeading();
    this.Report_ProgramGoals();
    this.doc.addPage();
    // });
    // getReport_Notes()
    this.getReport_Call_Notes();
  }

  getReport_Call_Notes() {
    var that = this;
    var startdate = this.RptStartDate;
    var enddate = this.RptEndDate;

    this.rpmservice
      .rpm_get(
        `/api/patients/getmynotes?NoteType=CALL&StartDate=${
          startdate + 'T00:00:00'
        }&EndDate=${enddate + 'T23:59:59'}`
      )
      .then(
        (data) => {
          that.http_NewNoteList_C = data;
          var i = 0;
          if (that.http_NewNoteList_C.length > 0) {
            this.getcallNotesbyIdAPI(i, that.http_NewNoteList_C);
          } else {
            var CombinedNotes = this.CombineNotes();
            that.printCallNotes(CombinedNotes);
          }
        },
        (err) => {}
      );
  }

  getcallNotesbyIdAPI(index: any, data: any) {
    var that = this;

    this.rpmservice
      .rpm_get(
        `/api/patients/getmynotesbyid?ProgramName=RPM&Type=CALL&PatientNoteId=${data[index].Id}`
      )
      .then(
        (data) => {
          that.http_NewNote = data;
          that.http_NewNoteList_C[index]['NoteDetails'] = that.http_NewNote;
          that.http_NewNoteList_C[index]['Type'] = 'CALL';
          if (index == that.http_NewNoteList_C.length - 1) {
            var CombinedNotes = this.CombineNotes();
            that.printCallNotes(CombinedNotes);
          } else {
            index = index + 1;
            this.getcallNotesbyIdAPI(index, that.http_NewNoteList_C);
          }
        },
        (err) => {}
      );
  }

  getHealthTrends() {
    // var startDate = this.auth.ConvertToUTCRangeInput(new Date(this.RptStartDate+'T00:00:00'))
    // var endDate=this.auth.ConvertToUTCRangeInput(new Date(this.RptEndDate+'T23:59:59'))

    var that = this;
    //that.rpmservice.rpm_get(`/api/patient/getpatienthealthtrends?PatientId=${this.selectedPatient}&PatientProgramId=${this.selectedProgram}&StartDate=${startDate}&EndDate=${endDate}`).then((data) => {
    //that.http_healthtrends = data;
    if (that.http_healthtrends.Values.length > 0) {
      that.lineChartLabels = that.convertDateforHealthTrends(
        that.http_healthtrends.Time
      );
      var temp = [];
      var j = 0;
      for (var item of that.http_healthtrends.Values) {
        var i = 0;
        for (var x of item.data) {
          if (j == 0) {
            try {
              if (x == null && i > 0 && i < item.data.length) {
                var linedt1 = this.lineChartLabels[i].split(' - ');
                var linedt0 = this.lineChartLabels[i - 1].split(' - ');
                var linedt2 = this.lineChartLabels[i + 1].split(' - ');
                if (linedt1[0] == linedt0[0] || linedt1[0] == linedt2[0]) {
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
    } else {
      this.setEmptyGraph();
    }
    /* },
      (err)=>{
        this.setEmptyGraph();
      });*/
  }

  Style_SetDocumentHeader() {
    this.doc.setFontSize(16);
    this.doc.setTextColor('black');
  }
  Style_SetMainHeading() {
    this.doc.setFontSize(14);
    this.doc.setTextColor('#590CA7');
  }
  Style_SetSubHeading() {
    this.doc.setFontSize(12);
    this.doc.setTextColor('black');
  }
  Style_SetContent() {
    this.doc.setFontSize(10);
    this.doc.setTextColor('black');
  }
  Report_ConvertDate(dateval: any) {
    var dt = dateval.split('T');
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
    var dat = month + ' ' + dtSplit[2];
    var time = this.datepipe.transform(
      this.convertToLocalTime(dateval),
      'h:mm a'
    );
    dat = dat + ' - ' + time;
    return dat;
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

  Report_PatientAndProgramInfo() {
    this.Style_SetMainHeading();
    var textHeight = 70;
    this.doc.text('Patient Information', 10, textHeight);
    this.Style_SetContent();

    this.doc.setDrawColor('#818495');
    textHeight = textHeight + 5;
    this.doc.line(10, textHeight, 200, textHeight);
    this.doc.setTextColor('#818495');
    textHeight = textHeight + 5;
    this.doc.text('Patient Name : ', 30, textHeight);
    this.doc.setTextColor('black');
    this.doc.text(
      this.http_rpm_patient['PatientDetails'].FirstName +
        ' ' +
        this.http_rpm_patient['PatientDetails'].LastName,
      60,
      textHeight
    );

    textHeight = textHeight + 5;
    this.doc.line(10, textHeight, 200, textHeight);
    this.doc.setTextColor('#818495');
    textHeight = textHeight + 5;

    this.doc.text('Patient Id : ', 30, textHeight);
    this.doc.setTextColor('black');
    this.doc.text(
      this.http_rpm_patient['PatientDetails'].UserName,
      60,
      textHeight
    );
    textHeight = textHeight + 5;

    this.doc.line(10, textHeight, 200, textHeight);
    this.doc.setTextColor('#818495');
    textHeight = textHeight + 5;

    this.doc.text('Date of Birth : ', 30, textHeight);
    this.doc.setTextColor('black');
    this.doc.text(
      this.datepipe.transform(this.http_rpm_patient['PatientDetails'].DOB),
      60,
      textHeight
    );
    this.doc.setTextColor('#818495');

    this.doc.text('Height : ', 130, textHeight);
    this.doc.setTextColor('black');
    this.doc.text(
      this.http_rpm_patient['PatientDetails'].Height.toString(),
      150,
      textHeight
    );

    textHeight = textHeight + 5;
    this.doc.line(10, textHeight, 200, textHeight);
    this.doc.setTextColor('#818495'); // Same Line

    textHeight = textHeight + 5;
    this.doc.text('Gender : ', 30, textHeight);
    this.doc.setTextColor('black');
    this.doc.text(
      this.http_rpm_patient['PatientDetails'].Gender,
      60,
      textHeight
    );
    this.doc.setTextColor('#818495');
    this.doc.text('Weight : ', 130, textHeight);
    this.doc.setTextColor('black');
    this.doc.text(
      this.http_rpm_patient['PatientDetails'].Weight.toString(),
      150,
      textHeight
    );
    textHeight = textHeight + 5;
    this.doc.line(10, textHeight, 200, textHeight);
    textHeight = textHeight + 20;

    this.Style_SetMainHeading();
    this.doc.text('Program Information', 10, textHeight);
    this.Style_SetContent();

    textHeight = textHeight + 5;
    this.doc.line(10, textHeight, 200, textHeight);
    this.doc.setTextColor('#818495');
    textHeight = textHeight + 5;
    this.doc.text('Program Name : ', 26, textHeight);
    this.doc.setTextColor('black');

    this.program_name = this.http_rpm_patient.PatientProgramdetails.ProgramName;
    // var pgm = this.programDetails.filter(
    //   (c: { ProgramId: any }) => c.ProgramId === this.program_name
    // );
    // if (pgm.length > 0) {
    //   this.program_name = pgm[0].ProgramName;
    this.doc.text(this.program_name, 60, textHeight);
    // }

    textHeight = textHeight + 5;
    this.doc.line(10, textHeight, 200, textHeight);
    this.doc.setTextColor('#818495');
    textHeight = textHeight + 5;
    this.doc.text('Program Start Date : ', 20, textHeight);
    this.doc.setTextColor('black');
    this.doc.text(
      this.datepipe.transform(
        this.http_rpm_patient['PatientProgramdetails'].StartDate
      ),
      60,
      textHeight
    );

    this.doc.setTextColor('#818495');
    this.doc.text('Duration : ', 130, textHeight);
    this.doc.setTextColor('black');
    this.doc.text(
      this.http_rpm_patient['PatientProgramdetails'].Duration.toString(),
      150,
      textHeight
    );
    textHeight = textHeight + 5;

    this.doc.line(10, textHeight, 200, textHeight);
    this.doc.setTextColor('#818495');
    textHeight = textHeight + 5;
    if (this.patientProgramname == 'CCM' || this.patientProgramname == 'PCM') {
      this.doc.text('Condition Monitored : ', 18, textHeight);
    } else {
      this.doc.text('Vitals Monitored : ', 18, textHeight);
    }
    this.doc.setTextColor('black');
    var vitals = '';
    this.patiantVital =
      this.http_rpm_patient['PatientProgramdetails'].PatientVitalInfos;
    this.patiantVital = this.patiantVital.filter(
      (ds: { Selected: boolean }) => ds.Selected == true
    );
    var counter = 0;
    for (var x of this.patiantVital) {
      counter++;
      vitals = vitals + x['Vital'].toString();
      if (this.patiantVital.length > 1) {
        // if(counter == 5){
        //   vitals = vitals+''.'+"\n "
        // }
        vitals = vitals + '\n';
      }
    }
    var splitTitle = this.doc.splitTextToSize(vitals, 150);
    this.doc.text(splitTitle, 60, textHeight);
    var ext = 0;
    if (splitTitle.length > 1) {
      ext = 5;
    }
    var dim = this.doc.getTextDimensions(splitTitle);
    // console.log(dim.h);
    var heightdim = dim.h;
    textHeight = textHeight + heightdim;
    this.doc.line(10, textHeight, 200, textHeight);

    textHeight = textHeight + 5;
    this.doc.setTextColor('#818495');
    this.doc.text('Program Enrollment : ', 18, textHeight);
    this.doc.setTextColor('black');
    this.doc.text(
      this.datepipe.transform(
        this.http_rpm_patient['PatientProgramdetails'].StartDate
      ),
      60,
      textHeight
    );

    textHeight = textHeight + 5;
    this.doc.line(10, textHeight, 200, textHeight);
    this.doc.setTextColor('#818495');
    textHeight = textHeight + 5;
    this.doc.text('Diagnosis : ', 35, textHeight);
    this.doc.setTextColor('black');
    var diagnosis = '';

    for (var x of this.http_rpm_patient['PatientPrescribtionDetails']
      .PatientDiagnosisInfos) {
      diagnosis =
        diagnosis +
        (x['DiagnosisName'] + ' - ' + x['DiagnosisCode']).toString();
      if (
        this.http_rpm_patient['PatientPrescribtionDetails']
          .PatientDiagnosisInfos.length > 1
      ) {
        diagnosis = diagnosis + ',\n';
      }
    }

    var splitTitle = this.doc.splitTextToSize(diagnosis, 150);
    this.doc.text(splitTitle, 60, textHeight);
    var ext = 0;
    if (splitTitle.length > 1) {
      ext = 5;
    }
    var diagonosisdim = this.doc.getTextDimensions(splitTitle);
    var heightdisgonsisdim = diagonosisdim.h;
    textHeight = textHeight + heightdisgonsisdim;
    this.doc.line(10, textHeight, 200, textHeight);
    this.doc.setTextColor('#818495');

    textHeight = textHeight + 5;
    this.doc.text('Management Period : ', 20, textHeight);
    this.doc.setTextColor('black');
    var PStart = this.datepipe.transform(
      this.http_rpm_patient['PatientProgramdetails'].StartDate
    );
    var CurrentDay = this.datepipe.transform(new Date());
    this.doc.text(PStart + ' - ' + CurrentDay, 60, textHeight);
    textHeight = textHeight + 10;
    this.doc.line(10, textHeight, 200, textHeight);

    textHeight = textHeight + 50;
    this.doc.line(28, textHeight, 95, textHeight);
    this.doc.line(120, textHeight, 195, textHeight);
    this.doc.setTextColor('#818495');
    this.doc.text('Signature ', 10, textHeight);
    this.doc.text('Date ', 110, textHeight);
  }

  Report_ProgramGoals() {
    var goalh = 30;
    this.doc.setTextColor('black');
    for (let goal of this.http_rpm_patient['PatientProgramGoals'].goalDetails) {
      this.Style_SetSubHeading();
      this.doc.text(goal.Goal, 15, goalh);
      const textWidth = this.doc.getTextWidth(goal.Goal);
      this.doc.setDrawColor('black');
      this.doc.line(15, goalh + 1, 15 + textWidth, goalh + 1);
      this.Style_SetContent();
      goalh = goalh + 10;
      this.doc.text(goal.Description, 20, goalh);
      goalh = goalh + 25;
    }
    this.Style_SetSubHeading();
    var events_var = 'Critical Events';
    this.doc.text(events_var, 15, goalh);
    const textWidth = this.doc.getTextWidth(events_var);
    this.doc.setDrawColor('black');
    this.doc.line(15, goalh + 1, 15 + textWidth, goalh + 1);
    goalh = goalh + 10;
    this.Style_SetContent();
    for (let alert of this.PatientCriticalAlerts) {
      var dt = this.Report_ConvertDate(alert.Time);
      this.doc.text(dt, 20, goalh);
      const textWidth = this.doc.getTextWidth(dt);
      this.doc.setDrawColor('black');
      this.doc.line(20, goalh + 1, 20 + textWidth, goalh + 1);
      // this.doc.line(20, goalh, 200, goalh);
      goalh = goalh + 10;
      this.doc.text(alert.VitalAlert, 20, goalh);
      goalh = goalh + 10;
    }
    if (this.PatientCriticalAlerts.length == 0) {
      this.doc.text('No Data', 20, goalh);
    }
  }

  CombineNotes() {
    //this.listA = this.http_NewNoteList;

    this.listB = this.http_NewNoteList_C;
    // this.listC = this.listA.concat(this.listB);
    this.listC = this.listB;
    this.listC.sort((a: any, b: any) => {
      return (a.CreatedOn || a.CreatedOn).localeCompare(
        b.CreatedOn || b.CreatedOn
      );
    });
    console.log(this.listC);
    return this.listC;
  }

  printCallNotes(data: any) {
    this.Notesh = 30;
    this.Style_SetSubHeading();
    var call_var = 'Notes';
    this.doc.setDrawColor('black');
    this.setPages(call_var, 15);
    const textWidth = this.doc.getTextWidth(call_var);
    this.doc.line(15, this.Notesh + 1, 15 + textWidth, this.Notesh + 1);
    this.Notesh = this.Notesh + 10;
    this.doc.setFontSize(14);
    for (let notes of data) {
      this.Style_SetSubHeading();
      this.Notesh = this.Notesh + 5;
      this.setPages(
        this.datepipe.transform(this.convertToLocalTime(notes.CreatedOn)),
        20
      );
      const textWidth = this.doc.getTextWidth(
        this.datepipe.transform(this.convertToLocalTime(notes.CreatedOn))
      );
      this.doc.line(20, this.Notesh + 1, 20 + textWidth, this.Notesh + 1);
      this.Style_SetContent();
      this.Notesh = this.Notesh + 5;
      this.setPages('Duration : ' + this.timeConvert(notes.Duration), 20);
      this.Notesh = this.Notesh + 5;
      this.setPages('Completed By : ' + notes.CompletedBy, 20);
      this.Notesh = this.Notesh + 5;
      this.setPages('Note Type : ' + notes.NoteType, 20);
      this.Notesh = this.Notesh + 5;

      this.setPages('Type : ' + notes.Type, 20);
      this.Notesh = this.Notesh + 5;
      if (notes.Type != 'REVIEW') {
        if (notes.IsEstablished) {
          this.setPages('Call Established : Yes', 20);
        } else {
          this.setPages('Call Established : No', 20);
        }
        this.Notesh = this.Notesh + 5;
        if (notes.IsCareGiver) {
          this.setPages('Care Giver : Yes', 20);
        } else {
          this.setPages('Care Giver : No', 20);
        }
      }

      this.Notesh = this.Notesh + 5;
      var checkedflag = false;
      var subquestioncheckedflag = false;
      if (notes.NoteDetails) {
        for (let x of notes.NoteDetails.MainQuestions) {
          checkedflag = false;

          this.Notesh = this.Notesh + 5;
          this.setPages(x.Question, 20);
          const textWidth = this.doc.getTextWidth(x.Question);
          this.doc.line(20, this.Notesh + 1, 20 + textWidth, this.Notesh + 1);
          this.Notesh = this.Notesh + 5;
          for (let y of x.AnswerTypes) {
            if (y.Checked) {
              // this.doc.text("- "+y.Answer, 25, this.Notesh);
              this.setPages('- ' + y.Answer, 25);
              this.Notesh = this.Notesh + 5;
              checkedflag = true;
            }
          }
          if (!checkedflag && x.AnswerTypes.length > 0) {
            this.setPages('- None', 25);
            this.Notesh = this.Notesh + 5;
          } else if (x.AnswerTypes.length == 0 && x.SubQuestions.length == 0) {
            this.setPages('- None', 25);
            this.Notesh = this.Notesh + 5;
          }

          this.Notesh = this.Notesh + 5;
          if (x.SubQuestions.length > 0) {
            for (let z of x.SubQuestions) {
              subquestioncheckedflag = false;

              this.Notesh = this.Notesh + 5;
              this.setPages(z.Question, 25);
              const textWidth = this.doc.getTextWidth(z.Question);
              this.doc.line(
                25,
                this.Notesh + 1,
                25 + textWidth,
                this.Notesh + 1
              );
              this.Notesh = this.Notesh + 5;
              for (let y of z.AnswerTypes) {
                if (y.Checked) {
                  // this.doc.text("- "+y.Answer, 25, this.Notesh);
                  this.setPages('- ' + y.Answer, 25);
                  this.Notesh = this.Notesh + 5;
                  subquestioncheckedflag = true;
                }
              }
              if (!subquestioncheckedflag) {
                this.setPages('- None', 25);
                this.Notesh = this.Notesh + 5;
              } else if (z.AnswerTypes.length == 0) {
                this.setPages('- None', 25);
                this.Notesh = this.Notesh + 5;
              }
            }
          }

          if (x.Notes) {
            // var enotes = "Extra Notes  "
            // this.setPages(enotes, 20);
            // const textWidth = this.doc.getTextWidth(enotes);
            // this.doc.line(20, this.Notesh+1, 20 + textWidth, this.Notesh+1)
            // this.Notesh=this.Notesh+5
            this.setPages('Extra Notes : ' + x.Notes, 25);
            this.Notesh = this.Notesh + 10;
          }
        }
        if (notes.NoteDetails.Notes) {
          this.Notesh = this.Notesh + 5;
          var ANotes = 'Additional Notes ';
          this.setPages(ANotes, 20);
          const textWidth = this.doc.getTextWidth(ANotes);
          this.doc.line(20, this.Notesh + 1, 20 + textWidth, this.Notesh + 1);
          this.Notesh = this.Notesh + 5;
          this.setPages(notes.NoteDetails.Notes, 25);
          this.Notesh = this.Notesh + 15;
        }
      }
      this.Notesh = this.Notesh + 3;
      // this.doc.line(10, this.Notesh, 100, this.Notesh);
    }
    this.doc.addPage();
    this.Style_SetMainHeading();

    this.Report_HealthTrends();
  }

  setPages(data: any, left: any) {
    if (this.Notesh > 280) {
      this.doc.addPage();
      this.Notesh = 30;
    }
    var splitTitle = this.doc.splitTextToSize(data, 180);
    var dim = this.doc.getTextDimensions(splitTitle);
    var heightdim = dim.h;
    if (splitTitle.length > 1) {
      this.Notesh = this.Notesh + 5;
      this.doc.text(splitTitle, left, this.Notesh);
      this.Notesh = this.Notesh + 5;
      this.Notesh = this.Notesh + heightdim;
      if (this.Notesh > 280) {
        this.doc.addPage();
        this.Notesh = 30;
      }
    } else {
      this.doc.text(splitTitle, left, this.Notesh);
    }
  }

  Report_HealthTrends() {
    if (this.patientProgramname == 'CCM' || this.patientProgramname == 'PCM') {
      this.Report_PatientSummaryReport();
      this.doc.save('PatientReport.pdf');
      this.buttonclick1 = true;
    } else {
      this.HtmlGraph = document.querySelector('#pdfGraph');
      this.HtmlGraph.style.visibility = 'visible';
      html2canvas(this.HtmlGraph).then((canvas) => {
        this.Style_SetSubHeading();
        var HealthTrends = 'Patient Health Trends';
        this.doc.text(HealthTrends, 15, 20);
        this.doc.setDrawColor('black');
        const textWidth = this.doc.getTextWidth(HealthTrends);
        this.doc.line(15, 21, 15 + textWidth, 21);
        var startdate = this.convertDate(this.ThirtyDaysAgo);
        var enddate = this.convertDate(this.Today);
        startdate = startdate + 'T00:00:00';
        enddate = enddate + 'T23:59:59';
        var imgData = canvas.toDataURL('image/jpeg', 1.0);
        this.doc.addImage(imgData, 10, 35, 180, 65);
        this.Report_HealthTrendsTable();
        this.Report_PatientSummaryReport();
        this.VitalReadingSummary();
        this.doc.save('PatientReport.pdf');
        //this.CombineNotes();
        this.buttonclick1 = true;
      });
      this.HtmlGraph.style.visibility = 'hidden';
    }
  }

  Report_PatientSummaryReport() {
    this.doc.addPage();

    this.Style_SetMainHeading();
    this.doc.text('Patient Program Summary Report', 10, 20);
    this.symptomh = 30;
    this.Style_SetSubHeading();
    var sy = 'Symptoms';
    this.doc.text(sy, 15, this.symptomh);
    this.doc.setDrawColor('black');
    const textWidth = this.doc.getTextWidth(sy);
    this.doc.line(15, this.symptomh + 1, 15 + textWidth, this.symptomh + 1);
    this.symptomh = this.symptomh + 20;
    for (let sy of this.http_get_symptoms) {
      this.Style_SetContent();
      this.setPagesSympomsMedications(
        'Date : ' + this.Report_ConvertDate(sy.SymptomStartDateTime),
        20
      );

      this.symptomh = this.symptomh + 3;
      this.doc.line(20, this.symptomh, 100, this.symptomh);

      this.symptomh = this.symptomh + 5;
      // this.doc.text(sy.Symptom+" : "+sy.Description, 20, this.symptomh);
      this.setPagesSympomsMedications(sy.Symptom + ' : ' + sy.Description, 20);
      this.symptomh = this.symptomh + 25;
    }

    this.doc.addPage();
    this.symptomh = 30;
    this.Style_SetSubHeading();
    var Med = 'Medications';
    this.doc.text(Med, 15, this.symptomh);
    this.doc.setDrawColor('black');
    const textWidth2 = this.doc.getTextWidth(Med);
    this.doc.line(15, this.symptomh + 1, 15 + textWidth2, this.symptomh + 1);
    this.http_medication_data;
    this.http_get_symptoms;
    var col = [
      'Medicine Name',
      'Schedule',
      'Intervals',
      'Start Date',
      'End Date',
    ];
    var rows: string[][] = [];
    for (let med of this.http_medication_data) {
      var temp = [
        med.Medicinename,
        med.MedicineSchedule,
        //this.Report_IntervalFlag(med.Morning,"M")+" - "+this.Report_IntervalFlag(med.AfterNoon,"AN")+" - "+this.Report_IntervalFlag(med.Evening,"E")+" - "+this.Report_IntervalFlag(med.Night,"N"),
        this.ProcessSchedules(med),
        this.datepipe.transform(this.convertToLocalTime(med.StartDate)),
        this.datepipe.transform(this.convertToLocalTime(med.EndDate)),
      ];
      rows.push(temp);
    }
    // this.doc.autoTable(col, rows, { startY: 35 });
    autoTable(this.doc, {
      head: [col],
      body: rows,
      startY: 35,
      // headStyles :{fillColor : "#CFF4F5"}
    });
    // this.doc.addPage();
    // this.Style_SetSubHeading();
    // var billing = 'Billing Information';
    // this.symptomh = 30;
    // this.doc.text(billing, 15, this.symptomh);
    // this.doc.setDrawColor('black');
    // const textWidth3 = this.doc.getTextWidth(billing);
    // this.doc.line(15, this.symptomh + 1, 15 + textWidth3, this.symptomh + 1);
    // console.log(this.BillingInfo);
    // var col = ['CPTCode', 'Last Billing Cycle', 'Reading', 'Status'];
    // var rows: string[][] = [];
    // if (this.BillingInfo != undefined) {
    //   for (let bill of this.BillingInfo) {
    //     var temp = [
    //       bill.CPTCode,
    //       this.billCycledisplay(bill.Last_Billing_Cycle),
    //       this.convertBillingSec(bill.CPTCode, bill.reading),
    //       bill.status,
    //     ];
    //     rows.push(temp);
    //   }
    // }
    // autoTable(this.doc, {
    //   head: [col],
    //   body: rows,
    //   startY: 35,
    // });
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

  convertDateforHealthTrends(dateArr: any) {
    this.retArr = [];
    for (let dateval of dateArr) {
      var newd = dateval.split('+');
      var newdate = this.convertToLocalTime(newd[0]);
      var dt = newdate.split(' ');
      // var dt1=dt[0].split(" ");
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
      var time = this.datepipe.transform(
        this.convertToLocalTime(newd[0]),
        'h:mm a'
      );
      dat = dat + ' - ' + time;
      this.retArr.push(dat);
    }
    return this.retArr;
  }

  Report_HealthTrendsTable() {
    this.VitalsConsolidated = [];
    this.Style_SetContent();
    this.http_vitalBloodPressure = this.newVitalreadingData(
      this.http_vitalData.BloodPressure,
      'BloodPressureReadings'
    );
    this.http_vitalGlucoseData = this.newVitalreadingData(
      this.http_vitalData.BloodGlucose,
      'BloodGlucoseReadings'
    );
    this.http_VitalOxygen =
      this.http_vitalData &&
      this.newVitalreadingData(
        this.http_vitalData.BloodOxygen,
        'BloodOxygenReadings'
      );
    this.http_VitalWight =
      this.http_vitalData &&
      this.newVitalreadingData(this.http_vitalData.Weight, 'WeightReadings');

    if (this.http_vitalGlucoseData && this.http_vitalGlucoseData.length > 0) {
      // var col = [
      //   'Date',
      //   'Time',
      //   'Schedule',
      //   'Blood Glucose (mg/dL)',
      //   'Remarks',
      // ];
      var col = [
        'Date',
        'M-BeforeMeal',
        'M-AfterMeal',
        'AN-BeforeMeal',
        'AN-AfterMeal',
        'Status',
      ];
      var rows: string[][] = [];
      for (let vl of this.http_vitalGlucoseData) {
        for (let vv of vl.BloodGlucoseReadings) {
          this.VitalsConsolidated.push(vv);
          var temp = this.BloodGlucoseReportDisplay(vv);
          //  [
          //   this.datepipe.transform(vv.ReadingTime),
          //   this.datepipe.transform(vv.ReadingTime, 'h:mm a'),
          //   vv.Schedule,
          //   vv.BGmgdl,
          //   vv.Remarks,
          // ];
          rows.push(temp);
        }
      }

      autoTable(this.doc, {
        head: [col],
        body: rows,
        startY: 105,

        didParseCell: (data) => {
          if (data.cell.text[0] == 'Critical') {
            data.cell.styles.textColor = 'red';
            data.row.cells[1].styles.textColor = 'red';
            data.row.cells[2].styles.textColor = 'red';
            data.row.cells[3].styles.textColor = 'red';
            data.row.cells[4].styles.textColor = 'red';
          }
          if (data.cell.text[0] == 'Cautious') {
            data.cell.styles.textColor = 'orange';
            data.row.cells[1].styles.textColor = 'orange';
            data.row.cells[2].styles.textColor = 'orange';
            data.row.cells[3].styles.textColor = 'orange';
            data.row.cells[4].styles.textColor = 'orange';
          }
        },
      });
    } else if (
      this.http_vitalBloodPressure &&
      this.http_vitalBloodPressure.length > 0
    ) {
      var col = ['Date', 'Time', 'SBP', 'DBP', 'Pulse', 'Remarks'];
      var rows: string[][] = [];
      for (let vl of this.http_vitalBloodPressure) {
        for (let vv of vl.BloodPressureReadings) {
          this.VitalsConsolidated.push(vv);
          var temp = [
            this.datepipe.transform(vv.ReadingTime),
            this.datepipe.transform(vv.ReadingTime, 'h:mm a'),
            vv.Systolic,
            vv.Diastolic,
            vv.pulse,
            vv.Remarks,
          ];
          rows.push(temp);
        }
      }

      autoTable(this.doc, {
        head: [col],
        body: rows,
        startY: 105,

        didParseCell: (data) => {
          if (data.cell.text[0] == 'Critical') {
            data.cell.styles.textColor = 'red';
          }
          if (data.cell.text[0] == 'Cautious') {
            data.cell.styles.textColor = 'orange';
          }
        },
      });
    } else if (this.http_VitalOxygen && this.http_VitalOxygen.length > 0) {
      var col = ['Date', 'Time', 'Oxygen', 'Pulse', 'Remarks'];
      var rows: string[][] = [];
      for (let vl of this.http_VitalOxygen) {
        for (let vv of vl.BloodOxygenReadings) {
          this.VitalsConsolidated.push(vv);
          var temp = [
            this.datepipe.transform(vv.ReadingTime),
            this.datepipe.transform(vv.ReadingTime, 'h:mm a'),
            vv.Oxygen,
            vv.Pulse,
            vv.Remarks,
          ];
          rows.push(temp);
        }
      }
      autoTable(this.doc, {
        head: [col],
        body: rows,
        startY: 105,

        didParseCell: (data) => {
          if (data.cell.text[0] == 'Critical') {
            data.cell.styles.textColor = 'red';
          }
          if (data.cell.text[0] == 'Cautious') {
            data.cell.styles.textColor = 'orange';
          }
        },
      });
    } else if (this.http_VitalWight && this.http_VitalWight.length > 0) {
      var col = ['Date', 'Time', 'Weight', 'Remarks'];
      var rows: string[][] = [];
      for (let vl of this.http_VitalWight) {
        for (let vv of vl.WeightReadings) {
          this.VitalsConsolidated.push(vv);
          var temp = [
            this.datepipe.transform(vv.ReadingTime),
            this.datepipe.transform(vv.ReadingTime, 'h:mm a'),
            vv.Schedule,
            vv.BWlbs,
            vv.Remarks,
          ];
          rows.push(temp);
        }
      }
      // this.doc.autoTable(col,rows, {

      //   startY: 100,

      //   didParseCell: function(cell:any,row:any) {
      //         if(cell.row.raw[4] =="Critical" ){
      //           cell.cell.styles.textColor = 'red';
      //         }
      //         if(cell.row.raw[4] =="Cautious" ){
      //           cell.cell.styles.textColor = 'orange';
      //         }
      //     }
      // });
      autoTable(this.doc, {
        head: [col],
        body: rows,
        startY: 105,

        didParseCell: (data) => {
          if (data.cell.text[0] == 'Critical') {
            data.cell.styles.textColor = 'red';
          }
          if (data.cell.text[0] == 'Cautious') {
            data.cell.styles.textColor = 'orange';
          }
        },
      });
    }
    this.DaysVitals();
  }

  newVitalreadingData(data: any, vitalInfo: any) {
    var that = this;
    var vitalDataArray: any[] = [];
    if (data != undefined) {
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
    }

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
  VitalsSevenDaysConsolidated: any;
  http_7day_vitalData: any;
  DaysVitals() {
    this.VitalsSevenDaysConsolidated = [];

    var vitalBloodPressure =
      this.http_7day_vitalData &&
      this.newVitalreadingData(
        this.http_7day_vitalData.BloodPressure,
        'BloodPressureReadings'
      );

    var vitalGlucoseData =
      this.http_7day_vitalData &&
      this.newVitalreadingData(
        this.http_7day_vitalData.BloodGlucose,
        'BloodGlucoseReadings'
      );

    var VitalOxygen =
      this.http_7day_vitalData &&
      this.newVitalreadingData(
        this.http_7day_vitalData.BloodOxygen,
        'BloodOxygenReadings'
      );

    var VitalWight =
      this.http_7day_vitalData &&
      this.newVitalreadingData(
        this.http_7day_vitalData.Weight,
        'WeightReadings'
      );

    if (vitalGlucoseData && vitalGlucoseData.length > 0) {
      for (let vl of vitalGlucoseData) {
        for (let vv of vl.BloodGlucoseReadings) {
          this.VitalsSevenDaysConsolidated.push(vv);
        }
      }
    } else if (vitalBloodPressure && vitalBloodPressure.length > 0) {
      for (let vl of vitalBloodPressure) {
        for (let vv of vl.BloodPressureReadings) {
          this.VitalsSevenDaysConsolidated.push(vv);
        }
      }
    } else if (VitalOxygen && VitalOxygen.length > 0) {
      for (let vl of VitalOxygen) {
        for (let vv of vl.BloodOxygenReadings) {
          this.VitalsSevenDaysConsolidated.push(vv);
        }
      }
    } else if (VitalWight && VitalWight.length > 0) {
      for (let vl of VitalWight) {
        for (let vv of vl.WeightReadings) {
          this.VitalsSevenDaysConsolidated.push(vv);
        }
      }
    }
  }

  setPagesSympomsMedications(data: any, left: any) {
    if (this.symptomh > 280) {
      this.doc.addPage();
      this.symptomh = 30;
    }
    var splitTitle = this.doc.splitTextToSize(data, 180);
    if (splitTitle.length > 1) {
      this.symptomh = this.symptomh + 5;
      this.doc.text(splitTitle, left, this.symptomh);
      this.symptomh = this.symptomh + 5;
    } else {
      this.doc.text(splitTitle, left, this.symptomh);
    }
  }
  Report_IntervalFlag(val: any, flg: any) {
    if (val) {
      return flg;
    } else {
      return '0';
    }
  }
  ProcessSchedules(med: any) {
    var res = '';
    if (med.Morning) {
      res = res + this.Report_IntervalFlag(med.Morning, 'M');
    }
    if (med.Morning && med.AfterNoon) {
      res = res + ' - ' + this.Report_IntervalFlag(med.AfterNoon, 'AN');
    } else if (!med.Morning && med.AfterNoon) {
      res = res + this.Report_IntervalFlag(med.AfterNoon, 'AN');
    }
    if (med.Morning && med.AfterNoon && med.Evening) {
      res = res + ' - ' + this.Report_IntervalFlag(med.Evening, 'E');
    } else if (!med.Morning && med.AfterNoon && med.Evening) {
      res = res + ' - ' + this.Report_IntervalFlag(med.Evening, 'E');
    } else if (med.Morning && !med.AfterNoon && med.Evening) {
      res = res + ' - ' + this.Report_IntervalFlag(med.Evening, 'E');
    } else if (!med.Morning && !med.AfterNoon && med.Evening) {
      // res = res + ' - ' + this.Report_IntervalFlag(med.Evening, 'E');
      res = res + this.Report_IntervalFlag(med.Evening, 'E');
    }
    if (med.Morning && med.AfterNoon && med.Evening && med.Night) {
      res = res + ' - ' + this.Report_IntervalFlag(med.Night, 'N');
    } else if (!med.Morning && !med.AfterNoon && med.Evening && med.Night) {
      res = res + ' - ' + this.Report_IntervalFlag(med.Night, 'N');
    } else if (med.Morning && !med.AfterNoon && !med.Evening && med.Night) {
      res = res + ' - ' + this.Report_IntervalFlag(med.Night, 'N');
    } else if (!med.Morning && med.AfterNoon && !med.Evening && med.Night) {
      res = res + ' - ' + this.Report_IntervalFlag(med.Night, 'N');
    } else if (!med.Morning && med.AfterNoon && med.Evening && med.Night) {
      res = res + ' - ' + this.Report_IntervalFlag(med.Night, 'N');
    } else if (med.Morning && !med.AfterNoon && med.Evening && med.Night) {
      res = res + ' - ' + this.Report_IntervalFlag(med.Night, 'N');
    } else if (!med.Morning && med.AfterNoon && !med.Evening && med.Night) {
      res = res + ' - ' + this.Report_IntervalFlag(med.Night, 'N');
    } else if (!med.Morning && !med.AfterNoon && !med.Evening && med.Night) {
      res = res + this.Report_IntervalFlag(med.Night, 'N');
    }

    if (res == '') {
      return 'None';
    }
    return res;
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

  reportStart() {
    this.doc = new jsPDF();
    this.Style_SetDocumentHeader();
    this.doc.text('REMOTE PATIENT MONITORING - REPORT', 50, 40);
    //date range
    this.doc.text(
      '(' + this.RptStartDate + ' To ' + this.RptEndDate + ')',
      67,
      50
    );
    this.Report_PatientAndProgramInfo();
    this.doc.addPage();
    this.Style_SetMainHeading();
    this.program_name = this.http_rpm_patient.PatientProgramdetails.ProgramName;
    // var pgm = this.programDetails.filter(
    //   (c: { ProgramId: any }) => c.ProgramId === this.program_name
    // );
    // if (pgm.length > 0) {
    //   this.program_name = pgm[0].ProgramName;
    //   this.doc.text(this.program_name, 10, 20);
    // }
    this.doc.text(this.program_name, 10, 20);
    this.getHealthTrends();
  }
  downloadPatientReport() {
    this.patientStatusData = this.http_rpm_patient.PatientProgramdetails.Status;
    this.patientProgramname =
      this.http_rpm_patient['PatientProgramdetails'].ProgramName;
    this.reportStart();
    this.getPatientAndProgramInfo();
    // });
  }
  buttonclick1 = true;

  dateRangeSet(StartDate: any, EndDate: any) {
    if (new Date(EndDate.value) > new Date()) {
      alert('Cannot Download report for future date.');
      return;
    }
    this.buttonclick1 = false;
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
      this.RptStartDate = this.convertDate(firstDay);
      this.RptEndDate = this.convertDate(lastDay);
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
      this.RptStartDate = this.convertDate(StartDate.value);
      this.RptEndDate = this.convertDate(EndDate.value);
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
    this.getPatientInfo();
  }

  getPatientInfo() {
    this.rpmservice.rpm_get(`/api/patients/getpatient`).then((data) => {
      this.http_rpm_patient = data;
      this.patientStatusData =
        this.http_rpm_patient.PatientProgramdetails.Status;
      this.getTrends();
    });
  }

  getTrends() {
    var startDate = this.auth.ConvertToUTCRangeInput(
      new Date(this.RptStartDate + 'T00:00:00')
    );
    var endDate = this.auth.ConvertToUTCRangeInput(
      new Date(this.RptEndDate + 'T23:59:59')
    );
    var that = this;
    that.rpmservice
      .rpm_get(
        `/api/patients/getpatienthealthtrends?StartDate=${startDate}&EndDate=${endDate}`
      )
      .then((data) => {
        that.http_healthtrends = data;
        this.getVitalReading();
      });
  }

  getVitalReading() {
    this.currentPatient = this.selectedPatient;
    this.currentProgram = this.selectedProgram;
    var that = this;
    var startDate = this.auth.ConvertToUTCRangeInput(
      new Date(this.RptStartDate + 'T00:00:00')
    );
    var endDate = this.auth.ConvertToUTCRangeInput(
      new Date(this.RptEndDate + 'T23:59:59')
    );
    that.rpmservice
      .rpm_get(
        `/api/patients/getpatientvitalreadings?StartDate=${startDate}&EndDate=${endDate}`
      )
      .then((data) => {
        that.http_vitalData = data;
        that.http_vitalBloodPressure = this.newVitalreadingData(
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
        this.getMedication();
      });
  }
  getMedication() {
    this.rpmservice
      .rpm_get(`/api/patients/getpatientmedication`)
      .then((data) => {
        this.http_medication_data = data;
        this.getPatientSymptoms();
      });
  }

  getPatientSymptoms() {
    var that = this;
    that.rpmservice.rpm_get(`/api/patients/getpatientsymptoms`).then((data) => {
      that.http_get_symptoms = data;
      that.getVitalReading7Days();
    });
  }
  getVitalReading7Days() {
    var endDate = this.auth.ConvertToUTCRangeInput(
      new Date(this.RptEndDate + 'T23:59:59')
    );
    var startDate = this.convertDate(
      new Date(this.RptEndDate).setDate(new Date(this.RptEndDate).getDate() - 7)
    );
    startDate = startDate + 'T00:00:00';
    var that = this;
    that.rpmservice
      .rpm_get(
        `/api/patients/getpatientvitalreadings?StartDate=${startDate}&EndDate=${endDate}`
      )
      .then((data) => {
        that.http_7day_vitalData = data;
        this.downloadPatientReport();
      });
  }

  VitalReadingSummary() {
    this.doc.addPage();

    this.Style_SetSubHeading();
    var StatSumm = 'Vital Readings - Status Summary';

    this.doc.text(StatSumm, 15, 20);
    this.doc.setDrawColor('black');
    const textWidth = this.doc.getTextWidth(StatSumm);
    this.doc.line(15, 21, 15 + textWidth, 21);
    var col = [
      'Days of Reading',
      'Selected Last 7 days',
      'Selected Last 30 Days',
    ];
    var rows: string[][] = [];

    var temp = [
      'Total Days of Reading',
      this.VitalsSevenDaysConsolidated.length,
      this.VitalsConsolidated.length,
    ];
    rows.push(temp);

    var Critcal30 = this.VitalsConsolidated.filter(
      (data: { Status: string }) => {
        return data.Status == 'Critical';
      }
    );
    var Critcal7 = this.VitalsSevenDaysConsolidated.filter(
      (data: { Status: string }) => {
        return data.Status == 'Critical';
      }
    );

    var temp = ['Critical Readings', Critcal7.length, Critcal30.length];
    rows.push(temp);

    var Cautious30 = this.VitalsConsolidated.filter(
      (data: { Status: string }) => {
        return data.Status == 'Cautious';
      }
    );
    var Cautious7 = this.VitalsSevenDaysConsolidated.filter(
      (data: { Status: string }) => {
        return data.Status == 'Cautious';
      }
    );

    var temp = ['Cautious Readings', Cautious7.length, Cautious30.length];
    rows.push(temp);

    // this.doc.autoTable(col,rows, { startY: 30});
    autoTable(this.doc, {
      head: [col],
      body: rows,
      startY: 30,
    });
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
  // getAllUser() {
  //   var that = this;
  //   that.rpmservice
  //     .rpm_get('/api/users/getallusers?RoleId=' + that.roles[0].Id)
  //     .then((data) => {
  //       that.http_userlist = data;
  //       that.http_userlist = that.http_userlist.Details;
  //       that.http_userlist = that.http_userlist.filter(function (data: any) {
  //         return data.IsActive != false && data.Role != 'Physician';
  //       });
  //       console.log(that.http_userlist);
  //     });
  // }
  calllogrange = new UntypedFormGroup({
    start: new UntypedFormControl(),
    end: new UntypedFormControl(),
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
        StartDate: this.convertDate(startDate.value),
        EndDate: this.convertDate(EndDate.value),
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
  callnonEstrange = new UntypedFormGroup({
    start: new UntypedFormControl(),
    end: new UntypedFormControl(),
  });

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
  getBillingCode() {
    var that = this;
    that.rpmservice.rpm_get('/api/patient/getbillingCodes').then((data2) => {
      this.billingCode = data2;
    });
  }

  programType: any;
  programChange(event: any) {
    // this.programType=event;
    this.filterBillingData();
  }
  billingTypeProgramArray: any;
  billingProgramNameTempArray: any;
  billingProgramNameArray: any;
  getBillingTypeProgram() {
    var that = this;
    that.rpmservice.rpm_get('/api/patient/getAllprograms').then(
      (data2) => {
        this.billingTypeProgramArray = data2;
        this.billingProgramNameTempArray = data2;
        this.filterBillingData();

        this.billingTypeProgramArray = this.removeDuplicateObjects(
          this.billingProgramNameTempArray,
          'ProgramType'
        );
      },
      (err) => {
        console.log(err);
      }
    );
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

  convertToLocalTimeOnly(stillUtc: any) {
    if (stillUtc) {
      if (stillUtc.includes('+')) {
        var temp = stillUtc.split('+');
        stillUtc = temp[0];
      }
    }
    stillUtc = stillUtc + 'Z';
    var local = moment(stillUtc).local().format('HH:mm:ss');
    return local;
  }

  getTimefromDate(text: any) {
    const myArray = text.split(' ');
    return myArray[1];
  }

  BloodGlucoseReportDisplay(data: any) {
    //var readingDate = this.convertToLocalTime(data.ReadingTime);
    var readingDate = data.ReadingTime;
    var vitalDataArray: any[] = [];

    var Morning = '00:00:00';
    var AfterNoon = '10:00:01';
    var Evening = '15:00:01';
    var Night = '21:00:01';

    var time = this.getTimefromDate(data.ReadingTime);

    if (Morning < time && time < AfterNoon) {
      if (data.Schedule == 'Fasting') {
        vitalDataArray.push(
          this.datepipe.transform(readingDate),
          'Fasting',
          data.BGmgdl + '\n' + this.datepipe.transform(readingDate, 'h:mm a'),
          null,
          null,
          null,
          data.Status
        );
      }

      if (data.Schedule == 'NonFasting') {
        vitalDataArray.push(
          this.datepipe.transform(readingDate),
          'NonFasting',
          data.BGmgdl + '\n' + this.datepipe.transform(readingDate, 'h:mm a'),
          null,
          null,
          null,
          data.Status
        );
      }
    } else if (AfterNoon < time && time < Evening) {
      if (data.Schedule == 'Fasting') {
        vitalDataArray.push(
          this.datepipe.transform(readingDate),
          'Fasting',
          null,
          data.BGmgdl + '\n' + this.datepipe.transform(readingDate, 'h:mm a'),
          null,
          null,
          data.Status
        );
      }

      if (data.Schedule == 'NonFasting') {
        vitalDataArray.push(
          this.datepipe.transform(readingDate),
          'NonFasting',
          null,
          data.BGmgdl + '\n' + this.datepipe.transform(readingDate, 'h:mm a'),
          null,
          null,
          data.Status
        );
      }
    } else if (Evening < time && time < Night) {
      if (data.Schedule == 'Fasting') {
        vitalDataArray.push(
          this.datepipe.transform(readingDate),
          'Fasting',
          null,
          null,
          data.BGmgdl + '\n' + this.datepipe.transform(readingDate, 'h:mm a'),
          null,
          data.Status
        );
      }
      if (data.Schedule == 'NonFasting') {
        vitalDataArray.push(
          this.datepipe.transform(readingDate),
          'NonFasting',
          null,
          null,
          data.BGmgdl + '\n' + this.datepipe.transform(readingDate, 'h:mm a'),
          null,
          data.Status
        );
      }
    } else if (Night < time) {
      if (data.Schedule == 'Fasting') {
        vitalDataArray.push(
          this.datepipe.transform(readingDate),
          'Fasting',
          null,
          null,
          null,
          data.BGmgdl + '\n' + this.datepipe.transform(readingDate, 'h:mm a'),

          data.Status
        );
      }
      if (data.Schedule == 'NonFasting') {
        vitalDataArray.push(
          this.datepipe.transform(readingDate),
          'Nonfasting',
          null,
          null,
          null,
          data.BGmgdl + '\n' + this.datepipe.transform(readingDate, 'h:mm a'),

          data.Status
        );
      }
    }
    return vitalDataArray;
  }
}
