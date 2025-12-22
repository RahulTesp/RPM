import { Component, Input, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute, Resolve, Router } from '@angular/router';
import { Observable } from 'rxjs';
// import { Options } from '@angular-slider/ngx-slider';
import { AuthService } from 'src/app/services/auth.service';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { DatePipe } from '@angular/common';
import { RightSidebarComponent } from '../../../shared/right-sidebar/right-sidebar.component';
import { RPMService } from '../../../sevices/rpm.service';
import dayjs from 'dayjs';
import utc from 'dayjs/plugin/utc';
import timezone from 'dayjs/plugin/timezone';
import { PatientDataDetailsService } from '../../patient-detail-page/Models/service/patient-data-details.service';

dayjs.extend(utc);
dayjs.extend(timezone);
@Component({
  selector: 'app-patient-home',
  templateUrl: './patient-home.component.html',
  styleUrls: ['./patient-home.component.scss'],
})
export class PatientHomeComponent implements OnInit {
  @ViewChild(RightSidebarComponent) private rightsidebar: RightSidebarComponent;
  loading = false;
  tab_addtodo = false;
  myprofile: any;
  scheduleData: any;
  latestAlertsAndTasks: any;
  chatMessage: any;
  loading2 = false;
  patientName: string;
  patientID: string;
  patientdob: string;
  patient_address: string;
  plan_duration_balance: string;
  patient_bp_value: string;
  patient_bp_duration: string;
  patient_pulse_value: string;
  patient_pulse_duration: string;
  patient_bg_value: string;
  patient_bg_duration: string;
  PatientInfo: any;
  patient_id: any;
  currentDate: any;
  currentDateNo: any;
  currentDay: any;
  currentTime: any;
  currentMonth: any;
  public Duration: any;
  frequencyValue: any;
  bpplan: boolean = false;
  bgplan: boolean = false;
  pulseplan: boolean = false;
  CurrentProgramSelected: any;
  ProgramHistory: any;
  currentpPatientId: any;
  currentprogramId: any;
  ActiveProgram: any;
  call_panel_visible = false;
  callTime = 0;
  getTodoList: any;
  bg_nonfasting_vital_readings: any;
  careplan: any;
  careplanStatus: any;
  interval: any;
  patient_dob: any;
  vital_readings: any;
  tab_todo_list_view = false;
  ProfileName: any;
  observable$: Observable<Object>;
  rolelist: any;
  heath_trends_frequencies: number[] = []; // one per chart
  heath_trends_frequency:any;
  healthtrendVitalNameArray:any;
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
  makeCallReady() {
    this.call_panel_visible = true;
    this.startTimer();
  }
  chat_panel_visible = false;

  makeChat() {
    // this.switchvariable=4;
    // this.chat_panel_visible = true;
  }

  close_chat_window() {
    this.chat_panel_visible = false;
  }
  close_modal() {
    this.call_panel_visible = false;
    this.stopTimer();
  }
  constructor(
    private rpmservice: RPMService,
    private rpm: RPMService,
    private auth: AuthService,
    private _route: ActivatedRoute,
    public datepipe: DatePipe,
    private patientService: PatientDataDetailsService,
    
  ) {


    this.getPatientData();
    this.getBillingInfo();
    this.getTodoListData();
  }

  getTodoListData() {
    var that = this;
    var startDate;
    var today = new Date();
    var startdate = this.convertDate(today);
    var enddate = this.convertDate(today);

    startDate = startdate + 'T00:00:00';
    enddate = enddate + 'T23:59:59';

    // startDate = this.auth.ConvertToUTCRangeInput(new Date(startDate));
    // enddate = this.auth.ConvertToUTCRangeInput(new Date(enddate));

    that.rpm
      .rpm_get(
        `/api/patients/gettodolist?StartDate=${startDate}&EndDate=${enddate}`
      )
      .then((data: any) => {
        this.getTodoList = data;
      });
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
  onclickcancel() {
    this.call_panel_visible = false;
  }
  program_name: any;
  CompleteionPercentage = 0;
  patiantVital: any;
  programStartTime: any;
  programEndTime: any;
  program_endTime: any;
  programStart: any;
  PatientCriticalAlerts: any;
  http_get_symptoms: any;
  currentPhysician: any;
  assignedMember: any;
  getSymptom() {
    this.rpm.rpm_get(`/api/patients/getpatientsymptoms`).then((data: any) => {
      this.http_get_symptoms = data;
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
    });
  }
  prescribedassignedDate: any;
  getPatientData() {
    var that = this;
    that.loading2 = true;
    // that.rpmservice.rpm_get('/api/patients/getpatient').then((data) => {
    that.rpmservice.rpm_get('/api/patients/getpatient').then((data: any) => {
      that.myprofile = data;

      this.program_name = this.myprofile['PatientProgramdetails'].ProgramName;
      this.patiantVital =
        this.myprofile['PatientProgramdetails'].PatientVitalInfos;
      this.patiantVital = this.patiantVital.filter(
        (ds: { Selected: boolean }) => ds.Selected == true
      );

      this.programStartTime = this.myprofile['PatientProgramdetails'].StartDate;
      this.programEndTime = this.myprofile['PatientProgramdetails'].EndDate;
      this.program_endTime = this.datepipe.transform(
        this.convertToLocalTime(this.programEndTime),
        'MMM/dd/yyyy'
      );
      this.programStart = this.datepipe.transform(
        this.convertToLocalTime(this.programStartTime),
        'MMM/dd/yyyy'
      );
      this.currentPhysician =
        this.myprofile.PatientPrescribtionDetails.Physician;
      this.assignedMember = this.myprofile.PatientProgramdetails.AssignedMember;

      this.CompleteionPercentage =
        (that.myprofile.ProfileSummary.CurrentDuration /
          that.myprofile.ProfileSummary.TotalDuration) *
        100;

      this.ProfileName =
        that.myprofile.FirstName + ' ' + that.myprofile.LastName;
      sessionStorage.setItem('user_name', this.ProfileName);
      sessionStorage.setItem('userid', that.myprofile.UserId);
      that.loading2 = false;
      that.patientName =
        that.myprofile.PatientDetails.FirstName +
        that.myprofile.PatientDetails.MiddleName +
        ' ' +
        that.myprofile.PatientDetails.LastName;
      that.patientID = that.myprofile.PatientDetails.UserId;
      this.patient_dob = this.convertDateforPatientDOB(
        that.myprofile.PatientDetails.DOB
      );
      that.patientdob = this.patient_dob;
      that.patient_address =
        that.myprofile.PatientDetails.CityName +
        ',' +
        that.myprofile.PatientDetails.State;
      that.plan_duration_balance =
        that.myprofile.PatientProgramdetails.ProgramStatus;

      that.careplan = that.myprofile.PatientProgramdetails.PatientVitalInfos;
      that.careplanStatus = that.myprofile.PatientProgramdetails.Status;
      const patientcareplan: string[] = [];
      for (
        let i = 0;
        i < that.myprofile.PatientProgramdetails.PatientVitalInfos.length;
        i++
      ) {
        patientcareplan.push(
          that.myprofile.PatientProgramdetails.PatientVitalInfos[i].Vital
        );
      }

      if (patientcareplan.includes('Blood Glucose')) {
        this.bgplan = true;
      } else if (patientcareplan.includes('Blood Pressure')) {
        this.bpplan = true;
      } else if (patientcareplan.includes('Pulse')) {
        this.pulseplan = true;
      }
    });
  }
  ngOnInit(): void {
    this.switchvariable = 1;
    this.heath_trends_frequency = 30;

    this.rolelist = sessionStorage.getItem('Roles');
    this.rolelist = JSON.parse(this.rolelist);
    var programrole = this.rolelist[0].ProgramName;

    this.CurrentProgramSelected = undefined;
    this._route.queryParams.subscribe((params) => {
      this.getHealthTrends(this.heath_trends_frequency);
    });
    this.getSymptom();
  }
  // onProgramHstoryChange(programId: any) {
  //   this.CurrentProgramSelected = programId;
  // }

 
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

  minValue: number = 30;
  maxValue: number = 75;
  middle: number = 20;


  http_healthtrends: any;
  daycount: number;
  bg_vital_readings: any;
  patient_vital_name: any;
  vital_unit: any;
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
  convertToLocalTime(stillUtc: any): string | any {
    if (!stillUtc) {
      return null;
    }

    if (stillUtc.includes('+')) {
      stillUtc = stillUtc.split('+')[0];
    }

    stillUtc = stillUtc + 'Z'; // ensure UTC format
    const local = dayjs.utc(stillUtc).local().format('YYYY-MM-DD HH:mm:ss');
    return local;
  }

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
    // console.log(dt[0])
    const bdate = new Date(dt[0]);
    const timeDiff = Math.abs(Date.now() - bdate.getTime());
    var age = Math.floor(timeDiff / (1000 * 3600 * 24) / 365);
    var dob =
      month + ' ' + dtSplit[2] + ',' + dtSplit[0] + '(' + age + 'years)';
    return dob;
  }

  notificationtime(date: any) {
    let today = new Date();
    let notification_date = this.convertToLocalTime(date);

    let notification_date_sample: Date | null = null;
    if (notification_date) {
      notification_date_sample = new Date(notification_date);
    }

    let msgInfo: string = '';
    if(notification_date_sample){
      let diff_min = Math.round(
        (today.getTime() - notification_date_sample.getTime()) / 60000
      );
    if (diff_min == 0) {
      msgInfo = 'Few Seconds Ago';
    } else if (diff_min < 60) {
      msgInfo = diff_min + ' Min' + ' ' + 'Ago';
    } else if (diff_min < 1440) {
      var hr = Math.round(diff_min / 60);
      msgInfo = hr + ' Hrs' + ' ' + 'Ago';
    } else if (diff_min > 1440 && diff_min <= 10080) {
      //2880

      var day = this.datediff(notification_date_sample, today);

      let dayofweek = notification_date_sample.getDay();
      // if (day < ) {
      //   msgInfo = day + ' ' + '-' + ' ' + 'Days' + ' ' + 'Ago';
      // } else {
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
        // }
      }
      msgInfo = dayval;

      // } else if (diff_min > 2880 && diff_min < 10080) {
    } else if (diff_min > 10080 && diff_min < 20160) {
      msgInfo = '1 Week Ago';
    } else if (diff_min > 20161 && diff_min < 30240) {
      msgInfo = '2 week Ago';
    } else if (diff_min > 30241 && diff_min < 40320) {
      msgInfo = '3 week Ago';
    } else if (diff_min > 40321 && diff_min < 302400) {
      var monthdiff = this.monthDiff(notification_date_sample, today);

      msgInfo = monthdiff + ' ' + 'Months Ago';
    }
    // else if (diff_min > 86401 && diff_min < 172800) {

    //   msgInfo = '2 month Ago';
    // } else if (diff_min > 172801 && diff_min < 216000) {

    //   msgInfo = '3 month Ago';
    // } else if (diff_min > 216001 && diff_min < 259200) {

    //   msgInfo = '4 month Ago';
    // } else if (diff_min > 259201 && diff_min < 302400) {
    //   msgInfo = '5 month Ago';
    // } else if (diff_min > 302401 && diff_min < 345600) {
    //   msgInfo = '6 month Ago';
    // }
    else {
      if ((this.patient_bp_value = 'No Reading')) {
        msgInfo = ' ';
      } else {
        msgInfo = 'more Than 5 months';
      }
    }
  }

    return msgInfo;
  }

  monthDiff(dateFrom: any, dateTo: any) {
    return (
      dateTo.getMonth() -
      dateFrom.getMonth() +
      12 * (dateTo.getFullYear() - dateFrom.getFullYear())
    );
  }

  datediff(first: any, second: any) {
    return Math.floor((second - first) / (1000 * 60 * 60 * 24));
  }

  task = [
    {
      alert: 'No data',
      assigned: 'No data',
      priority: 'No data',
    },
  ];
  switchvariable: any;
  tab_navigatetodolist() {
    this.tab_addtodo = true;
  }
  navigatetodolist() {
    this.switchvariable = 2;
  }
  meditation_table_id: any;
  SelectedMedicalScheduleInterval: any;
  SelectedMedicalScheduleTime: any;
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
  updateMedicationForm = new FormGroup({
    updatemedicineName: new FormControl(null),
    scheduleInterval: new FormControl(null, [Validators.required]),
    scheduleTime: new FormControl(null, [Validators.required]),
    scheduleMorning: new FormControl(null),
    scheduleAfterNoon: new FormControl(null),
    scheduleEvening: new FormControl(null),
    scheduleNight: new FormControl(null),
    medicationstartDate: new FormControl(null),
    medicationendDate: new FormControl(null),
    medicationcomment: new FormControl(null),
  });
  navigatetodo(data: any) {
    this.switchvariable = 3;
    this.meditation_table_id = data.Id;

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
  onClickMinimize() {
    this.switchvariable = 1;
  }
  cancel() {
    this.switchvariable = 1;
  }
  tab_minimizeScreen() {
    this.tab_addtodo = false;
  }
  tab_cancel() {
    this.tab_addtodo = false;
  }
  durationValue = 10;
  increment_duration() {
    if (this.durationValue < 10) {
      this.durationValue++;
    } else if (this.durationValue >= 10) {
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
    //  this.toogle=!this.toogle;
    //  this.minimizeValue = true;
    this.switchvariable = 1;
  }
  maximizeScreen() {
    this.toogle = !this.toogle;
    this.minimizeValue = false;
  }

  addSchedule() {}
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
}
