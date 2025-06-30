import {
  Component,
  OnInit,
  ChangeDetectorRef,
} from '@angular/core';

import { MatTableDataSource } from '@angular/material/table';
import { SelectionModel } from '@angular/cdk/collections';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { DatePipe } from '@angular/common';
export interface PeriodicElement {
  time: string;
  activityname: string;
  selection: '';
  description: string;
}
import { AuthService } from 'src/app/services/auth.service';
import { RPMService } from '../../../sevices/rpm.service';
import dayjs from 'dayjs';
import utc from 'dayjs/plugin/utc';
import timezone from 'dayjs/plugin/timezone';

dayjs.extend(utc);
dayjs.extend(timezone);
@Component({
  selector: 'app-patient-todo',
  templateUrl: './patient-todo.component.html',
  styleUrls: ['./patient-todo.component.scss'],
})
export class PatientTodoComponent implements OnInit {
  checkedSeries: any;
  dataSourceTable: any;
  dataSource: any;

  getTodoList: any;

  loading: any;
  today = new Date();
  ThirtyDaysAgo = new Date(this.today).setDate(this.today.getDate() - 30);
  SevensDaysAgo = new Date(this.today).setDate(this.today.getDate() - 7);

  constructor(
    private router: Router,
    private rpm: RPMService,
    public datepipe: DatePipe,
    private auth: AuthService,
    private changeDetectorRef: ChangeDetectorRef
  ) {
    var that = this;
    this.loading = true;

    var startdate = this.convertDate(this.today);
    var enddate = this.convertDate(this.today);
    startdate = startdate + 'T00:00:00';
    enddate = enddate + 'T23:59:59';

    // startdate = this.auth.ConvertToUTCRangeInput(new Date(startdate));
    // enddate = this.auth.ConvertToUTCRangeInput(new Date(enddate));
    // startdate = new Date(startdate).toISOString().replace('Z', '');
    // enddate = new Date(enddate).toISOString().replace('Z', '');


    that.rpm
      .rpm_get(
        `/api/patients/gettodolist?StartDate=${startdate}&EndDate=${enddate}`
      )
      .then((data) => {
        this.getTodoList = data;
        this.loading = false;
        this.dataSource = this.getTodoList;

        this.dataSourceTable = new MatTableDataSource(this.dataSource);
      });
  }

  frmatodorange = new FormGroup({
    startdate: new FormControl(),
    enddate: new FormControl(),
  });
  rangetodoCalc() {
    var that = this;
    var range_start_date = new Date(
      this.frmatodorange.controls.startdate.value
    );
    range_start_date = this.convertDate(range_start_date);

    var range_end_date = new Date(this.frmatodorange.controls.enddate.value);

    range_end_date = this.convertDate(range_end_date);

    var startDate;
    var endDate;
    startDate = range_start_date + 'T00:00:00';
    // startDate = this.auth.ConvertToUTCRangeInput(new Date(startDate));
    endDate = range_end_date + 'T23:59:59';
    // endDate = this.auth.ConvertToUTCRangeInput(new Date(endDate));
    var that = this;
    if (
      range_start_date != null &&
      range_end_date != null &&
      range_start_date <= range_end_date
    ) {
      this.rpm
        .rpm_get(
          `/api/patients/gettodolist?StartDate=${range_start_date}&EndDate=${range_end_date}`
        )
        .then((data) => {
          this.getTodoList = data;
          this.loading = false;
          this.dataSource = this.getTodoList;

          this.dataSourceTable = new MatTableDataSource(this.dataSource);
          this.changeDetectorRef.detectChanges();
          // this.dataSourceTable.paginator = this.paginator;
          // this.dataSourceTable.sort = this.sort;
        });
    }
  }
  ngOnInit(): void {}
  retArr: Array<string>;
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
  convertToLocalTime(stillUtc: any) {
    stillUtc = stillUtc + 'Z';
    const local = dayjs.utc(stillUtc).local().format('YYYY-MM-DD HH:mm:ss');
    return local;
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
    return dt[0];
  }
  displayedColumns: string[] = ['Date', 'Time', 'ScheduleType', 'Decription'];

  someVar: any = false;
  selection: SelectionModel<Element> = new SelectionModel<Element>(false, []);

  selectRow($event: any, row: any) {
    console.info('clicked', $event);
    // this.switchvariable = 2;
    $event.preventDefault();

    if (!row.selected) {
      /*  this.isShown = !this.isShown; */
      this.dataSourceTable.filteredData.forEach(
        (row: { selected: boolean }) => (row.selected = false)
      );

      row.selected = true;
      if (row.selected) {
        this.someVar = true;
      }
    }
  }
  switchvariable: any;
  navigatetodolist() {
    this.switchvariable = 1;
  }

  cancel() {
    this.switchvariable = false;
  }
  searchPatientList_Name = false;

  searchPatientListName() {
    this.searchPatientList_Name = !this.searchPatientList_Name;
  }

  highlight(row: any) {
    row.selected == true;
  }
  onClickMinimize() {
    this.switchvariable = false;
  }
  backhome() {
    let route = '/admin/patient-home';
    this.router.navigate([route]);
  }
}
