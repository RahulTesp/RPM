import { Component, OnInit } from '@angular/core';
import { Subscription } from 'rxjs';
import { ReportDataService } from '../../services/report-data.service';
import { FormControl, FormGroup } from '@angular/forms';
import { RPMService } from 'src/app/components/admin-dashboard/sevices/rpm.service';

@Component({
  selector: 'app-callinfo-data-report',
  templateUrl: './callinfo-data-report.component.html',
  styleUrls: ['./callinfo-data-report.component.scss'],
})
export class CallinfoDataReportComponent implements OnInit {
  public keyword2 = 'UserName';
  public selectedcareTeam: any;
  public selectedcareTeamId: any;
  public http_userlist: any;
  public careteam: any;
  minDate: any;
  maxDate: any;
  maxCallLogDate: any;
  public RptCallLogStartDate: string;
  public RptCallLogEndDate: string;
  public buttonclick = true;
  private url: any;
  // calllogrange = new FormGroup({
  //   start: new FormControl(),
  //   end: new FormControl(),
  // });
  // range = new FormGroup({
  //   start: new FormControl(),
  //   end: new FormControl(),
  // });
  private subscriptions: Subscription[] = [];

  constructor(
    private patientreportService: ReportDataService,
    private rpmservice: RPMService
  ) {}

  ngOnInit(): void {
    this.subscriptions.push(
      this.patientreportService.users$.subscribe((users) => {
        this.http_userlist = users;
      })
    );
  }
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
  onFocused(e: any) {
    // do something when input is focused
  }
  GetDateRange(e: any) {
    this.RptCallLogStartDate = e.startDate;
    this.RptCallLogEndDate = e.endDate;
  }
  downloadCallInfo() {
    var that = this;

    if (this.selectedcareTeamId == undefined) {
      this.careteam = null;
    } else {
      this.careteam = this.selectedcareTeamId;
    }

    var req_body = {
      StartDate: this.patientreportService.convertDate(
        this.RptCallLogStartDate
      ),
      EndDate: this.patientreportService.convertDate(this.RptCallLogEndDate),
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
      }
    );
  }
  downloadFile(FileName: any) {
    const a = document.createElement('a');
    a.href = FileName;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
  }

  // setMaxDateCallInfo(date: HTMLInputElement) {
  //   this.minDate = new Date(date.value);
  //   var maxdate = new Date(
  //     new Date(this.minDate).setDate(this.minDate.getDate() - 90)
  //   );
  //   this.maxDate = new Date(maxdate);

  //   this.calllogrange.controls['start'].setValue(this.maxDate);
  //   this.calllogrange.controls['end'].setValue(this.minDate);
  // }
  // openDatepicker() {
  //   this.range.reset();
  // }
}
