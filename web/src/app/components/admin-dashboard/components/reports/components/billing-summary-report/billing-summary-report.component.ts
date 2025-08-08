import { Component, Input, OnInit } from '@angular/core';
import { DownloadPatientReportService } from '../../services/download-patient-report.service';
import { ReportDataService } from '../../services/report-data.service';

@Component({
  selector: 'app-billing-summary-report',
  templateUrl: './billing-summary-report.component.html',
  styleUrls: ['./billing-summary-report.component.scss'],
})
export class BillingSummaryReportComponent implements OnInit {
  public buttonclick = true;
  billingDetailPatientList: any;
  autocompletebilling: any;
  @Input() http_getActivePatientList: any;
  public clinicBillingDetails: any;
  private selectedBillingDetailPatient: any;
  public billingTypeProgramArray: any;
  private selectedBillingPatientProgram: any;
  public selectedPatient: any;
  private fileformatvariable: any;
  public selectedBillingPatient: any;
  programType: string;
  startDate: string;
  endDate: string;
  private month: any;
  private cptcode: any;
  url: any;
  public clinic: any;
  public keyword = 'searchField';
  public searchiconvisible = true;
  constructor(private patientreportService: ReportDataService) {
    this.programType = 'RPM';
  }

  ngOnInit(): void {
    this.getBillingTypeProgram();
    this.programChangeBillingDetail(this.programType);
    this.clinic = '';
    this.cptcode = '';
  }
  programChangeBillingDetail(e: any) {
    this.billingDetailPatientList = [];
    this.selectedBillingPatient = null;
    this.selectedBillingPatient = null;

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
  selectBillingEvent(item: any) {
    this.selectedBillingDetailPatient = item.PatientId;
    this.selectedBillingPatientProgram = item.PatientProgramId;
    console.log('selected program billing')
    console.log(item);
     this.searchiconvisible = false;
  }
  OnChangeSeachDetail(val: string) {
    this.selectedBillingDetailPatient = val;
  }

  onClearSearchbilldetail(event: any) {
    this.selectedBillingDetailPatient = null;
    this.searchiconvisible = true;
  }
  selectEvent(item: any) {
    this.selectedBillingPatient = item.PatientId;
    this.selectedBillingPatientProgram = item.PatientProgramId;
    this.searchiconvisible = false;
  }
  onChangeSearch(val: string) {
    this.selectedPatient = val;
    this.selectedBillingPatient = val;
    // fetch remote data from here
    // And reassign the 'data' which is binded to 'data' property.
  }
  onFocused(e: any) {
    this.searchiconvisible = false;
    // do something when input is focused
   this.searchiconvisible = false
  }
  dateRangeScheduleChange() {
    this.startDate = this.patientreportService.convertDate(this.startDate);
    this.endDate = this.patientreportService.convertDate(this.endDate);
  }

  GetDateRange(e: any) {
    this.startDate = e.startDate;
    this.endDate = e.endDate;
  }

  fileformatvariableExcel: any;
  fileformatvariablepdf: any;
  fileformatChange(event: any) {
    if (event.target.value == 'excel') {
      this.fileformatvariable = 'xlsx';
    } else if (event.target.value == 'pdf') {
      this.fileformatvariable = 'pdf';
    }
  }
  onClearSearch(event: any) {
    this.selectedBillingPatient = null;
  }

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
  async getBillingReportDetail(): Promise<void> {
    this.buttonclick = false;
    this.startDate = this.patientreportService.convertDate(this.startDate);
    this.endDate = this.patientreportService.convertDate(this.endDate);
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

      this.downloadFile(this.url.message);
    } catch (error: any) {
      alert(error.message);
    } finally {
      this.buttonclick = true;
    }
  }
  downloadFile(FileName: any) {
    const a = document.createElement('a');
    a.href = FileName;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
  }
}
