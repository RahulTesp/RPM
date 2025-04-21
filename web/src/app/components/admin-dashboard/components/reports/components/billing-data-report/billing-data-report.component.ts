import { Component, Input, OnInit } from '@angular/core';
import { FormControl, FormGroup } from '@angular/forms';
import { ReportDataService } from '../../services/report-data.service';

@Component({
  selector: 'app-billing-data-report',
  templateUrl: './billing-data-report.component.html',
  styleUrls: ['./billing-data-report.component.scss'],
})
export class BillingDataReportComponent implements OnInit {
  public buttonclick = true;
  public billing_variable: number;
  public clinic: any;
  public cptcode: any;
  public selectedBillingPatient: any;
  private fileformatvariable: any;
  public isMonth = false;
  public month: any;
  private programType: string;
  billingreportFormGroup: FormGroup;
  private selectedBillingDetailPatient: any;
  private autocompletebilling: any;
  @Input() http_getActivePatientList: any;
  @Input() clinicBillingDetails: any;
  private selectedPatient: any;
  public keyword = 'searchField';
  public billingCode: any;
  startDate: string;
  endDate: string;
  url: any;

  constructor(private patientreportService: ReportDataService) {}

  ngOnInit(): void {
    this.billing_variable = 1;
    this.programType = 'RPM';
    this.getClincDetails();
    this.getBillingCode();
    this.clinic = '';
    this.cptcode = '';
  }

  onClearSearch(event: any) {
    this.selectedBillingPatient = null;
  }
  billingDetailPatientList: any;

  programChangeBillingDetail(e: any) {
    this.billingDetailPatientList = [];
    this.selectedBillingPatient = null;
    this.selectedBillingDetailPatient = null;

    if (this.autocompletebilling) {
      this.autocompletebilling.clear();
    }
    this.onClearSearchbilldetail(null);

    this.getClincDetails();
  }

  getClincDetails() {
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
  onClearSearchbilldetail(event: any) {
    this.selectedBillingDetailPatient = null;
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

  selectedBillingPatientProgram: any;

  selectEvent(item: any) {
    this.selectedBillingPatient = item.PatientId;
    this.selectedBillingPatientProgram = item.PatientProgramId;
  }
  onChangeSearch(val: string) {
    this.selectedPatient = val;
    this.selectedBillingPatient = val;
  }
  onFocused(e: any) {}
  async getBillingCode(): Promise<void> {
    try {
      this.billingCode = await this.patientreportService.getBillingCode();
    } catch (error) {
      console.error('Error loading billing codes:', error);
    }
  }
  radioChange(event: any) {
    if (event.target.value == 1) {
      this.isMonth = true;
    } else if (event.target.value == 0) {
      this.isMonth = false;
      this.month = null;
    }
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
  async getBillingReport(): Promise<void> {
    this.buttonclick = false;

    try {
      if (!this.isMonth) {
        this.startDate = this.patientreportService.convertDate(this.startDate);
        this.endDate = this.patientreportService.convertDate(this.endDate);
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
  downloadFile(FileName: any) {
    const a = document.createElement('a');
    a.href = FileName;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
  }
}
