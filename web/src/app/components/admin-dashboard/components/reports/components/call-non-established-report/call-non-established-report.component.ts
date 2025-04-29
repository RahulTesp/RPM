import { Component, OnInit } from '@angular/core';
import { ReportDataService } from '../../services/report-data.service';
import { FormControl, FormGroup } from '@angular/forms';

@Component({
  selector: 'app-call-non-established-report',
  templateUrl: './call-non-established-report.component.html',
  styleUrls: ['./call-non-established-report.component.scss'],
})
export class CallNonEstablishedReportComponent implements OnInit {
  private url: any;
  private roles: any;
  public buttonclick = true;
  nonEstCallStart: any;
  nonEstCallEnd: any;
  constructor(private patientreportService: ReportDataService) {}

  ngOnInit(): void {
    this.roles = sessionStorage.getItem('Roles');
    this.roles = JSON.parse(this.roles);
  }

  callnonEstrange = new FormGroup({
    start: new FormControl(),
    end: new FormControl(),
  });
  downloadNonEstablishedCallInfo(): void {
    // const startValue = this.callnonEstrange.controls.start.value;
    // const endValue = this.callnonEstrange.controls.end.value;
    const startValue = this.nonEstCallStart;
    const endValue = this.nonEstCallEnd;
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
        this.downloadFile(this.url.message);
        this.buttonclick = true;
      })
      .catch((err) => {
        alert('No Report available for the period.');
        this.buttonclick = true;
        this.callnonEstrange.reset();
      });
  }
  GetDateRange(e: any) {
    this.nonEstCallStart = e.startDate;
    this.nonEstCallEnd = e.endDate;
  }
  downloadFile(FileName: any) {
    const a = document.createElement('a');
    a.href = FileName;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
  }
}
