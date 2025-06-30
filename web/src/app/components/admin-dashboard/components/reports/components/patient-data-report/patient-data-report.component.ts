import { Component, Input, OnInit } from '@angular/core';
import {
  CHART_COLORS,
  CHART_LEGEND,
  CHART_OPTIONS,
  CHART_TYPE,
} from '../../interfaces/chart-config-interface';
import { AuthService } from 'src/app/services/auth.service';
import { ReportDataService } from '../../services/report-data.service';
import { DownloadPatientReportService } from '../../services/download-patient-report.service';
// import { PatientUtilService } from '../../../patient-detail-page/Models/service/patient-util.service';
import { PatientReportApiService } from '../../services/patient-report-api.service';
import jsPDF from 'jspdf';
import { FormControl, FormGroup } from '@angular/forms';
import { ConfirmDialogServiceService } from 'src/app/components/admin-dashboard/shared/confirm-dialog-panel/service/confirm-dialog-service.service';

@Component({
  selector: 'app-patient-data-report',
  templateUrl: './patient-data-report.component.html',
  styleUrls: ['./patient-data-report.component.scss'],
})
export class PatientDataReportComponent implements OnInit {
  @Input() httpPatientList: any;
  private master_data: any;
  private RptStartDate: any;
  private RptEndDate: any;
  private http_rpm_patient: any;
  private patientStatusData: any;
  public http_healthtrends: any;
  public selectedPatient: any;
  private selectedProgram: any;
  private http_SmsData: any;
  private http_vitalData: any;
  private BillingInfo: any;
  private http_get_symptoms: any;
  private http_7day_vitalData: any;
  private doc: any;
  private http_medication_data: any;
  private selectedProgramName: any;
  private patientProgramname: any;
  private programDetails: any;
  public downloadstatus = true;
  private HtmlGraph: any;
  private PatientCriticalAlerts: any;
  public keyword1 = 'patientsearchField';
  public maxDate: any;
  public minDate: any;
  public confirmDialogTitle: string;
  public confirmDialogMessage: string;
  fromDate: any;
  enddate: any;
  month1: any;
  public showDialog = false;
  range = new FormGroup({
    start: new FormControl(null),
    end: new FormControl(null),
  });

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
  constructor(
    private auth: AuthService,
    private patientreportService: ReportDataService,
    private patientdownloadService: DownloadPatientReportService,
    // private patientUtilService: PatientUtilService,
    private PatientReportapi: PatientReportApiService,
    private confirmDialog:ConfirmDialogServiceService
  ) {}

  ngOnInit(): void {
    this.master_data = sessionStorage.getItem('add_patient_masterdata');
    if (this.master_data) {
      this.master_data = JSON.parse(this.master_data);
      this.programDetails = this.master_data.ProgramDetailsMasterData;
    }
  }

  selectEvent1(item: any) {
    this.selectedPatient = item.PatientId;
    this.selectedProgram = item.PatientProgramId;
  }
  onChangeSearch1(val: string) {
    this.selectedPatient = val;
  }
  onClearSearch1(event: any) {
    this.selectedPatient = null;
    this.selectedProgram = null;
  }

  onFocused(e: any) {
    // do something when input is focused
  }

  patientIsMonth = false;
  radioChangePatient(event: any) {
    const selectedRadioPanel = event.target.value;

    if (selectedRadioPanel == 1) {
      this.range.reset();
      this.patientIsMonth = true;
      this.fromDate = null;
      this.enddate = null;
    } else if (selectedRadioPanel == 0) {
      this.patientIsMonth = false;
      this.month1 = null;
    }
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
  downloadPatientReport() {
    this.patientStatusData = this.http_rpm_patient.PatientProgramdetails.Status;
    this.patientProgramname =
      this.http_rpm_patient['PatientProgramdetails'].ProgramName;
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
      this.patientProgramname
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
      this.downloadstatus = true;
    } else {
      this.HtmlGraph = document.querySelector('#pdfGraph');
      await this.patientdownloadService.captureHealthTrendsChart(
        this.doc,
        this.HtmlGraph
      );
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
      this.patientdownloadService.generateVitalReadingSummary(this.doc);
      this.doc.save('PatientReport.pdf');
      this.downloadstatus = true;
    }
  }

  dateRangeSet() {

    this.downloadstatus = false;
    if (this.selectedPatient == null || this.selectedPatient == undefined) {
      this.confirmDialog.showConfirmDialog(
        'Please select a Patient.',
        'Message',
        () => {
      null
        },
        false
      );
      this.downloadstatus = true;
      return;
    }
    if (this.patientIsMonth) {
      if (!this.month1) {
        alert('Please select a Month.');
        this.downloadstatus = true;
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
        this.downloadstatus = true;
        return;
      }
    } else {

      this.RptStartDate = this.patientreportService.convertDate(
        this.RptStartDate
      );
      this.RptEndDate = this.patientreportService.convertDate(this.RptEndDate);
    }
    if (
      this.RptStartDate == null ||
      this.RptStartDate == undefined ||
      this.RptEndDate == null ||
      this.RptEndDate == undefined
    ) {
      alert('Please select a valid Date Range.');
      this.downloadstatus = true;
      return;
    }
    this.loadPatientData();
  }

  GetDateRange(e: any) {
    this.RptStartDate = e.startDate;
    this.RptEndDate = e.endDate;
  }

  VitalsSevenDaysConsolidated: any;

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

      this.downloadPatientReport();
    } catch (error) {
      console.error('🚨 Error loading patient data:', error);
    }
  }

  openConfirm() {
    this.showDialog = true;
  }

  handleConfirm(result: Event) {
    this.showDialog = false;
    if (result) {
      console.log('Confirmed!');
      // Perform your delete logic here
    } else {
      console.log('Cancelled.');
    }
  }



}
