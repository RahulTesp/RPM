import { any } from 'lodash/fp';
import { NotificationPanelService } from './../../shared/notification-panel/notification-panel.service';
import {
  ChangeDetectorRef,
  Component,
  OnInit,
  ViewChild,
  ViewEncapsulation,
} from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { elementAt, map, startWith } from 'rxjs/operators';
import { RPMService } from '../../sevices/rpm.service';
import { parseInt } from 'lodash';
import { MatDialog, MatDialogConfig } from '@angular/material/dialog';
import { StatusMessageComponent } from '../../shared/status-message/status-message.component';
import { MatPaginator, PageEvent } from '@angular/material/paginator';
import { MatTableDataSource } from '@angular/material/table';
import { MatSort, Sort } from '@angular/material/sort';
import { LiveAnnouncer } from '@angular/cdk/a11y';
import { DatePipe } from '@angular/common';
import { AuthService } from 'src/app/services/auth.service';
import { RightSidebarComponent } from './../../shared/right-sidebar/right-sidebar.component';
import moment from 'moment';

export interface TeamData {
  teamname: string;
  alerts: number;
  duetoday: number;
  slabreached: number;
}
@Component({
  selector: 'app-team-page',
  templateUrl: './team-page.component.html',
  styleUrls: ['./team-page.component.scss'],
})
export class TeamPageComponent implements OnInit {
  @ViewChild(MatSort) sort: MatSort;

  teamtask_table_render = 1;
  myControl = new FormControl();
  http_unassignedMember: any;
  http_manager: any;
  managerList: any;
  accessRights: any;
  options: any;
  filteredOptions: any;
  // userarr:string[];

  tableDataSource: any;
  @ViewChild(MatPaginator) paginator: MatPaginator;
  @ViewChild(MatPaginator) paginatorData: MatPaginator;
  tableDataLength: any;
  http_task_list: any;
  temporaryTeam_taskList: any;
  summary_taskList: any;
  taskunAssignedMember: any;
  taskDueDate: any;
  taskSlaBreached: any;
  taskPendingLater: any;
  careTeamSelect: any;
  loader1: any;
  loader2: any;

  // Team Alert Variable Declaration:
  http_alert_list: any;
  temporaryTeam_alertList: any;
  alertUnAssignedMember: any;
  alertSlaBreached: any;
  alertCritical: any;
  alertOpen: any;
  alertCompleted: any;
  alertSummary: any;
  alertMember: any;
  careTeamSelectAlert: any;
  teamNamefilterAlert: any;
  alertTableonLoading: any;
  registerTeamForm = new FormGroup({
    teamname: new FormControl(null, [Validators.required]),
    mangerName: new FormControl(null, [Validators.required]),
  });

  roleId: any;
  roles: any;
  access: any;
  AdminVar: any;
  isAdmin: any;
  masterdata2: unknown;
  pageIndex: any;
  ngAfterViewInit() {}

  announceSortChange(sortState: Sort) {
    if (sortState.direction) {
      this._liveAnnouncer.announce(`Sorted ${sortState.direction}ending`);
    } else {
      this._liveAnnouncer.announce('Sorting cleared');
    }
  }
  AlertTypeArray: any;
  constructor(
    private _liveAnnouncer: LiveAnnouncer,
    public datepipe: DatePipe,
    private rpm: RPMService,
    private router: Router,
    public _notificationSvc: NotificationPanelService,
    public dialog: MatDialog,
    private changeDetectorRef: ChangeDetectorRef,
    private auth: AuthService,
    private _route: ActivatedRoute
  ) {
    var that = this;

    that.roles = sessionStorage.getItem('Roles');
    that.roleId = JSON.parse(this.roles);
    that.access = sessionStorage.getItem('useraccessrights');
    that.access = JSON.parse(this.access);
    if (!that.access.UserAccessRights) {
      this.AdminVar == 'N';
    } else if (that.access.UserAccessRights < 1) {
      this.AdminVar == 'N';
    } else {
      that.isAdmin = that.access.UserAccessRights.filter(function (data: any) {
        return data.AccessName == 'AdminAccess';
      });
      if (!that.isAdmin) {
        this.AdminVar == 'N';
      } else if (this.isAdmin.length == 1) {
        this.AdminVar = this.isAdmin[0].AccessRight;
      } else {
        this.AdminVar == 'N   ';
      }
    }

    this.AlertTypeArray = [
      { Type: 'Missing' },
      { Type: 'Cautious' },
      { Type: 'Critical' },
    ];
  }

  SelectedData: any;
  SelectId: any;
  SelectedName: any;
  selectedPatient(event: { option: { value: any } }) {
    console.log(event.option.value);
    this.SelectedData = event.option.value;
    this.SelectId = this.SelectedData.CareTeamMemberUserId;
    this.SelectedName = this.SelectedData.Name;
  }

  getOptionText(option: { Name: any }) {
    if (option != null) {
      return option.Name;
    } else {
      return 'Select';
    }
  }
  patientNavigationStatus: any;
  ngOnInit(): void {
    this.accessRights = sessionStorage.getItem('useraccessrights');
    this.accessRights = JSON.parse(this.accessRights);
    sessionStorage.removeItem('patient-page-status');

    this._route.queryParams.subscribe((params) => {
      sessionStorage.removeItem('patient-page-status');
      this.patientNavigationStatus = sessionStorage.getItem(
        'patientNavigationStatus'
      );

      if (this.patientNavigationStatus == 'true') {
        this.CurrentMenu = sessionStorage.getItem('SelectedMenu');
        this.CurrentMenu = parseInt(this.CurrentMenu);
        this.variable = this.CurrentMenu;
        this.pageIndex = sessionStorage.getItem('teampageIndex');
        this.pageIndex = parseInt(this.pageIndex);

        if (this.CurrentMenu == 1) {
          this.AlercontainerSelection = sessionStorage.getItem(
            'AlercontainerSelection'
          );
          this.AlercontainerSelection = parseInt(this.AlercontainerSelection);
          this.careTeamSelectAlert = sessionStorage.getItem(
            'careTeamSelectAlert'
          );
          if (this.careTeamSelectAlert == 'undefined') {
            this.careTeamSelectAlert = undefined;
          } else {
            this.careTeamSelectAlert = parseInt(this.careTeamSelectAlert);
          }
          this.priorityFilterValue = sessionStorage.getItem(
            'priorityAlertFilterValue'
          );
          this.priorityTaskFilterValue = parseInt(this.priorityFilterValue);
          this.statusFilterValue = sessionStorage.getItem(
            'statusFilterAlertValue'
          );
          this.tableFilterValuealert = sessionStorage.getItem(
            'SelectedFilterAlert'
          );
          if (
            this.priorityFilterValue == null ||
            this.priorityFilterValue == 'undefined'
          ) {
            this.priorityFilterValue = undefined;
          }
          if (
            this.statusFilterValue == null ||
            this.statusFilterValue == 'undefined'
          ) {
            this.statusFilterValue = 'status';
          }
          this.alertSearchValeName = sessionStorage.getItem(
            'searchPatientListalert'
          );
          this.searchAssigned = sessionStorage.getItem('searchAssigneealert');
          if (this.alertSearchValeName == 'true') {
            this.alertSearchValeName = true;
          } else {
            this.alertSearchValeName = false;
          }
          if (this.searchAssigned == 'true') {
            this.searchAssigned = true;
          } else {
            this.searchAssigned = false;
          }
          if (this.alertPatientSearch == undefined) {
            this.alertPatientSearch = '';
          }
          if (this.alertAssigneeSearch == undefined) {
            this.alertAssigneeSearch = '';
          }

          this.AlertFromPatientDetail();
        } else if (this.CurrentMenu == 2) {
          this.taskcontainerSelection = sessionStorage.getItem(
            'TaskcontainerSelection'
          );
          this.startDate = sessionStorage.getItem('startdate');
          this.endDate = sessionStorage.getItem('enddate');
          this.range.controls['start'].setValue(this.startDate);
          this.range.controls['end'].setValue(this.endDate);
          this.taskcontainerSelection = parseInt(this.taskcontainerSelection);
          this.priorityTaskFilterValue = sessionStorage.getItem(
            'priorityTaskFilterValue'
          );
          this.priorityTaskFilterValue = parseInt(this.priorityTaskFilterValue);
          this.statusFilterTaskValue = sessionStorage.getItem(
            'statusFilterTaskValue'
          );
          if (this.priorityTaskFilterValue == null) {
            this.priorityTaskFilterValue = 1;
          }
          if (this.statusFilterTaskValue == null) {
            this.statusFilterTaskValue = 'status';
          }
          this.careTeamSelectTask =
            sessionStorage.getItem('careTeamSelectTask');
          if (this.careTeamSelectTask == 'undefined') {
            this.careTeamSelectTask = undefined;
          } else {
            this.careTeamSelectTask = parseInt(this.careTeamSelectTask);
          }
          this.pageIndex = sessionStorage.getItem('teampageIndex');
          this.pageIndex = parseInt(this.pageIndex);
          this.tableFilterValuetask = sessionStorage.getItem('SelectedFilter');
          this.searchValueName = sessionStorage.getItem(
            'searchPatientListTask'
          );
          this.taskAssigned = sessionStorage.getItem('searchAssigneeTask');
          if (this.searchValueName == 'true') {
            this.searchValueName = true;
          } else {
            this.searchValueName = false;
          }
          if (this.taskAssigned == 'true') {
            this.taskAssigned = true;
          } else {
            this.taskAssigned = false;
          }
          if (this.taskPatientSearch == undefined) {
            this.taskPatientSearch = '';
          }
          if (this.taskAssigneeSearch == undefined) {
            this.taskAssigneeSearch = '';
          }
          this.TaskNavigationData();
        }
      } else {
        this.loader1 = true;
        this.getinitAlert();
        this.alertTableonLoading = true;
        this.AlercontainerSelection = 0;
        this.taskcontainerSelection = 0;
        this.priorityTaskFilterValue = 1;
        this.statusFilterTaskValue = 'status';
        this.priorityFilterValue = undefined;
        this.statusFilterValue = 'status';
        this.CurrentMenu = 1;
        this.alertSearchValeName = false;
        this.taskAssigned = false;
        this.searchValueName = false;
        this.searchAssigned = false;
        this.variable = this.CurrentMenu;
        this.alertPatientSearch = '';
        this.alertAssigneeSearch = '';
        this.taskPatientSearch = '';
        this.taskAssigneeSearch = '';
      }
    });
  }
  tempDataSource: any;
  teamNamefilter: any;

  menu = [
    {
      menu_id: 1,
      menu_title: 'Alert',
    },
    {
      menu_id: 2,
      menu_title: 'Tasks',
    },
  ];

  addteamVariable = false;
  rolelist: any;
  rangeSelectionEnable: any;
  CurrentMenu: any = 1;
  // Change Main Screen
  variable: any = 1;
  ChangeScreen(button: any) {
    this.CurrentMenu = button;
    switch (button) {
      case 1:
        this.variable = 1;
        this.AlercontainerSelection = 0;
        this.alertTableonLoading = true;

        this.alertSearchValeName = false;
        this.taskAssigned = false;
        this.searchValueName = false;
        this.searchAssigned = false;
        this.getinitAlert();

        break;

      case 2:
        this.variable = 2;
        this.range.reset();

        this.alertSearchValeName = false;
        this.taskAssigned = false;
        this.searchAssigned = false;

        this.searchValueName = false;
        this.taskcontainerSelection = 0;
        this.rangeSelectionEnable = true;
        this.getinitTask();

        break;

      default:
        this.variable = 1;
        break;
    }
  }

  clickHandler(data: any) {
    console.log(data.teamname);
    this.table_detailed_view = true;
  }
  // team Alert template Start

  displayedAlertColumns = [
    'TeamName',
    'CareTeamId',
    'Members',
    'TotalPatients',
    'CriticalAlertS',
    'SLABreached',
    'OpenAlerts',
    'CompletedAlerts',
  ];

  CriticalAlertSrc: any;
  totalCriticalAlert: any;

  alert_table_render = 1;

  Alert_columnHeader = [
    'AlertID',
    'AlertType',
    'Priority',
    'PatientName',
    'AssignedMember',
    'DueDate',
    'Status',
    'GotoAlert',
  ];
  range = new FormGroup({
    start: new FormControl(),
    end: new FormControl(),
  });

  rangeTaskCalc() {}
  setupFilter(column: string) {
    this.tableDataSource.filterPredicate = (d: any, filter: string) => {
      const textToSearch = (d[column] && d[column].toLowerCase()) || '';
      return textToSearch.indexOf(filter) !== -1;
    };
  }

  applyDataFilter(filterValue: any) {
    filterValue = (<HTMLInputElement>filterValue.target).value.trim(); // Remove whitespace
    filterValue = filterValue.toLowerCase();
    this.tableFilterValuealert = filterValue;
    // MatTableDataSource defaults to lowercase matches
    this.tableDataSource.filter = filterValue;
  }
  searchAlertNameClick() {
    this.alertSearchValeName = !this.alertSearchValeName;
    this.searchAssigned = false;
  }
  searchAlertNameclose() {
    this.alertSearchValeName = false;
    // New Code 25/04/2023
    this.priorityFilterValue = undefined;
    this.statusFilterValue = 'status';
    this.AlertReload();
  }

  serachAlertAssigned() {
    this.taskAssigned = !this.taskAssigned;
    this.alertSearchValeName = false;
  }
  AlercontainerSelection: any;

  onClickAlertFilter(alertType: any, selectedContainer: any) {
    this.loader1 = true;
    this.pageIndex = 0;
    if (this.patientNavigationStatus != 'true') {
      this.alertSearchValeName = false;
      this.searchAssigned = false;
    }
    this.alertPatientSearch = '';
    this.alertAssigneeSearch = '';
    this.statusFilterValue = 'status';
    this.priorityFilterValue = undefined;
    var teamid;

    if (
      this.careTeamSelectAlert != undefined &&
      this.careTeamSelectAlert != 'undefined'
    ) {
      teamid = this.careTeamSelectAlert;
    } else {
      teamid = -1;
    }

    this.AlercontainerSelection = selectedContainer;
    this.rpm
      .rpm_get(
        `/api/alerts/getteamalertsbyidandalerttype?AlertType=${alertType}&RoleId=${this.roleId[0].Id}&CareTeamId=${teamid}`
      )
      .then((data) => {
        this.alert_table_render = 2;
        this.alertCritical = data;
        console.log(this.alertCritical);

        this.loader1 = false;
        this.tableDataSource = new MatTableDataSource(
          this.alertCritical.Details
        );
        this.totalCriticalAlert = this.tableDataSource.filteredData.length;

        this.changeDetectorRef.detectChanges();

        this.tableDataSource.paginator = this.paginator;
        this.paginator.firstPage();
      });
  }

  getinitAlert() {
    var that = this;
    this.loader1 = true;
    this.alert_table_render = 1;
    that.rpm
      .rpm_get(`/api/careteam/getteamalerts?RoleId=${this.roleId[0].Id}`)
      .then((data) => {
        that.http_alert_list = data;
        this.temporaryTeam_alertList = that.http_alert_list.Details;
        this.alertSummary = that.http_alert_list.Summary;
        this.getSummaryAlertUpdate(
          this.careTeamSelectAlert,
          this.temporaryTeam_alertList
        );
        if (this.alertTableonLoading) {
          this.tableDataSource = new MatTableDataSource(
            that.http_alert_list.Details
          );
          this.changeDetectorRef.detectChanges();
          this.tableDataSource.paginator = this.paginator;
          this.paginator.firstPage();
        }
        this.loader1 = false;
        console.log('Loader 1 Completed');
      });
  }
  getAlertfromPatient() {
    var that = this;

    that.rpm
      .rpm_get(`/api/careteam/getteamalerts?RoleId=${this.roleId[0].Id}`)
      .then((data) => {
        that.http_alert_list = data;
        this.temporaryTeam_alertList = that.http_alert_list.Details;
        this.alertSummary = that.http_alert_list.Summary;
        this.getSummaryAlertUpdate(
          this.careTeamSelectAlert,
          this.temporaryTeam_alertList
        );
      });
  }

  alertMemberCount: any;
  alertcriticalCount: any;
  alertopenCount: any;
  alertcompletedCount: any;
  alertslaCount: any;
  careTeamSelectedItem: any;

  getSummaryAlertUpdate(team: any, datasrc: any) {
    if (team != undefined && team != 'undefined') {
      var Datasource = datasrc.filter((data: any) => {
        return data.CareTeamId == team;
      });
      this.alertMemberCount = Datasource[0].Members;
      this.alertcriticalCount = Datasource[0].CriticalAlerts;
      this.alertopenCount = Datasource[0].OpenAlerts;
      this.alertslaCount = Datasource[0].SLABreached;
      this.alertcompletedCount = Datasource[0].CompletedAlerts;
    } else {
      this.alertMemberCount = this.alertSummary[0].Members;
      this.alertcriticalCount = this.alertSummary[0].CriticalAlertS;
      this.alertopenCount = this.alertSummary[0].OpenAlerts;
      this.alertslaCount = this.alertSummary[0].SLABreached;
      this.alertcompletedCount = this.alertSummary[0].CompletedAlerts;
    }
  }

  dropDownAlertSelect() {
    this.alertTableonLoading = false;
    this.alert_table_render = 1;

    //  this.getinitAlert()
    this.AlercontainerSelection = 0;
    this.getSummaryAlertUpdate(
      this.careTeamSelectAlert,
      this.temporaryTeam_alertList
    );
    if (
      this.careTeamSelectAlert != undefined &&
      this.careTeamSelectAlert != 'undefined'
    ) {
      this.teamNamefilterAlert = this.temporaryTeam_alertList.filter(
        (data: any) => {
          return data.CareTeamId == this.careTeamSelectAlert;
        }
      );
    } else {
      this.getinitAlert();
      this.teamNamefilterAlert = this.http_alert_list.Details;
    }
    this.tableDataSource = new MatTableDataSource(this.teamNamefilterAlert);
    this.tableDataSource.paginator = this.paginator;
    this.paginator.firstPage();
  }

  renderAlerttableData(alertType: any) {
    var teamid;
    this.loader1 = true;
    if (
      this.careTeamSelectAlert != undefined &&
      this.careTeamSelectAlert != 'undefined'
    ) {
      teamid = this.careTeamSelectAlert;
    } else {
      teamid = -1;
    }
    this.rpm
      .rpm_get(
        `/api/alerts/getteamalertsbyidandalerttype?AlertType=${alertType}&RoleId=${this.roleId[0].Id}&CareTeamId=${teamid}`
      )
      .then((data) => {
        this.alert_table_render = 2;
        this.alertCritical = data;
        this.loader1 = false;
        this.dataSrcPiority = this.alertCritical.Details;
        this.getPriorityFilterData();
        sessionStorage.removeItem('patientNavigationStatus');
        sessionStorage.removeItem('SelectedMenu');
        sessionStorage.removeItem('teampageIndex');
        sessionStorage.removeItem('AlercontainerSelection');
        sessionStorage.removeItem('TaskcontainerSelection');
        sessionStorage.removeItem('priorityTaskFilterValue');
        sessionStorage.removeItem('statusFilterTaskValue');
        sessionStorage.removeItem('careTeamSelectAlert');
        sessionStorage.removeItem('SelectedFilterAlert');
        sessionStorage.removeItem('searchPatientListalert');
        sessionStorage.removeItem('searchAssigneealert');
        sessionStorage.removeItem('statusFilterAlertValue');
        sessionStorage.removeItem('priorityAlertFilterValue');
      });
  }
  priorityFilterValue: any;
  statusFilterValue: any;
  AlertFilter() {
    // this.alertSearchValeName = false;
    // this.searchAssigned = false;
    switch (this.AlercontainerSelection) {
      case 1:
        this.renderAlerttableData('Critical');
        break;
      case 2:
        this.renderAlerttableData('SLABreached');
        break;

      case 3:
        this.renderAlerttableData('Open');
        break;

      case 4:
        this.renderAlerttableData('Completed');
        break;
    }
  }

  AlertReload() {
    // New Code Change 25/04/2023
    // if (this.patientNavigationStatus != 'true') {
    //   this.priorityFilterValue = undefined;
    //   this.statusFilterValue = 'status';
    // }

    this.AlertFilter();
  }
  AlertFromPatientDetail() {
    this.getAlertfromPatient();
    this.AlertReload();
  }

  dataSrcPiority: any;
  getPriorityFilterData() {
    let dataPriorityfilter: string;
    let dataStatusFilter: string;
    let dataSourceAlertFilter;
    if (this.priorityFilterValue == 'Critical') {
      dataPriorityfilter = 'Critical';
    } else if (this.priorityFilterValue == 'Cautious') {
      dataPriorityfilter = 'Cautious';
    } else if (this.priorityFilterValue == 'Missing') {
      dataPriorityfilter = 'Missing';
    } else if (this.priorityFilterValue == undefined) {
      dataSourceAlertFilter = this.dataSrcPiority;
    }

    if (this.statusFilterValue == 'todo') {
      dataStatusFilter = 'ToDo';
    } else if (this.statusFilterValue == 'inprogress') {
      dataStatusFilter = 'InProgress';
    } else if (this.statusFilterValue == 'complete') {
      dataStatusFilter = 'Complete';
    } else if (this.statusFilterValue == 'status') {
      dataSourceAlertFilter = this.dataSrcPiority;
    }

    if (this.statusFilterValue != 'status') {
      dataSourceAlertFilter = this.dataSrcPiority.filter(function (data: any) {
        return data.Status == dataStatusFilter;
      });
    }

    if (this.priorityFilterValue != undefined) {
      dataSourceAlertFilter = dataSourceAlertFilter.filter(function (
        data: any
      ) {
        return data.Prioriy == dataPriorityfilter;
      });
    }

    this.tableDataSource = new MatTableDataSource(dataSourceAlertFilter);
    if (this.patientNavigationStatus == 'true') {
      console.log(this.tableFilterValuealert);
      console.log('data');
      this.redirectionAlertFilter(this.tableFilterValuealert);
    }
    this.changeDetectorRef.detectChanges();
    this.tableDataSource.paginator = this.paginator;
    this.totalCriticalAlert = this.tableDataSource.filteredData.length;

    // this.paginator.firstPage();

    this.tableDataSource.sort = this.sort;
  }

  // team Alert template End

  searchAssigned: any;
  searchValueName: any;
  alertSearchValeName: any;
  taskAssigned: any;
  teamNameVar = false;
  teamNameAlertVar = false;
  Tasks_dataSource: any;
  serachAssigned() {
    this.taskAssigned = !this.taskAssigned;
    this.searchValueName = false;
    this.alertSearchValeName = false;
  }
  serachAssignedTaskClose() {
    this.taskAssigned = false;
    this.DateRangeTask();
  }
  serachAssignedAlertClose() {
    this.searchAssigned = false;
    this.statusFilterValue = 'status';
    this.priorityFilterValue = undefined;
    this.AlertReload();
  }
  serachAssignedAlertOpen() {
    this.searchAssigned = true;
    this.searchValueName = false;
    this.alertSearchValeName = false;
  }
  searchNameClick() {
    this.searchValueName = !this.searchValueName;
    this.searchAssigned = false;
    this.taskAssigned = false;
  }
  searchNameClickClose() {
    this.searchValueName = false;
    this.DateRangeTask();
  }
  searchTeamNameClick() {
    this.teamNameVar = !this.teamNameVar;
  }
  searchTeamNameClickClose() {
    this.teamNameVar = !this.teamNameVar;
    this.getinitTask();
  }
  searchTeamAlertVar() {
    this.teamNameAlertVar = !this.teamNameAlertVar;
  }
  searchTeamAlertVarClose() {
    this.teamNameAlertVar = !this.teamNameAlertVar;
    this.getinitAlert();
  }
  applyFilter(event: Event) {
    const filterValue = (event.target as HTMLInputElement).value;
    this.Tasks_dataSource.filter = filterValue.trim().toLowerCase();
  }

  taskSummary: any;
  rangeContainer: any;
  httpCompletedTask: any;
  loading: any;
  total_number: any;
  taskComplete: any;
  tasktableDataSource: any;
  careTeamSelectTask: any;
  teamNamefilterTask: any;
  startDate: any;
  endDate: any;
  loadingvar: any;
  firstload = true;

  setupTaskFilter(column: string) {
    this.tasktableDataSource.filterPredicate = (d: any, filter: string) => {
      const textToSearch = (d[column] && d[column].toLowerCase()) || '';
      return textToSearch.indexOf(filter) !== -1;
    };
  }

  applyDataTaskFilter(filterValue: any) {
    filterValue = (<HTMLInputElement>filterValue.target).value.trim(); // Remove whitespace
    filterValue = filterValue.toLowerCase(); // MatTableDataSource defaults to lowercase matches
    this.tableFilterValuetask = filterValue;

    this.tasktableDataSource.filter = filterValue;
  }

  setupTaskTeamFilter(column: string) {
    this.tableDataSource.filterPredicate = (d: any, filter: string) => {
      const textToSearch = (d[column] && d[column].toLowerCase()) || '';
      return textToSearch.indexOf(filter) !== -1;
    };
  }

  applyDataTeamTaskFilter(filterValue: any) {
    filterValue = (<HTMLInputElement>filterValue.target).value.trim(); // Remove whitespace
    filterValue = filterValue.toLowerCase();
    // MatTableDataSource defaults to lowercase matches
    this.tableDataSource.filter = filterValue;
  }
  setupAlertTeamFilter(column: string) {
    this.tableDataSource.filterPredicate = (d: any, filter: string) => {
      const textToSearch = (d[column] && d[column].toLowerCase()) || '';
      return textToSearch.indexOf(filter) !== -1;
    };
  }

  applyDataAlertTeamFilter(filterValue: any) {
    filterValue = (<HTMLInputElement>filterValue.target).value.trim(); // Remove whitespace
    filterValue = filterValue.toLowerCase(); // MatTableDataSource defaults to lowercase matches
    this.tableDataSource.filter = filterValue;
  }

  getinitTask() {
    var that = this;
    this.loader2 = true;
    var today = new Date();
    this.rangeContainer = false;
    this.teamtask_table_render = 1;
    this.taskcontainerSelection = 0;
    this.careTeamSelectTask = undefined;

    var ThirtyDaysAgo = new Date(today).setDate(today.getDate() + 30);

    var dueToday = this.convertDate(today) + 'T23:59:59';
    dueToday = this.auth.ConvertToUTCRangeInput(new Date(dueToday));
    this.loadingvar = false;

    if (
      this.range.controls.start.value != null &&
      this.range.controls.end.value != null
    ) {
      this.startDate = new Date(this.range.controls.start.value);
      this.endDate = new Date(this.range.controls.end.value);
    } else {
      // this.startDate = ThirtyDaysAgo;
      // this.endDate = today;
      this.startDate = today;
      this.endDate = ThirtyDaysAgo;
    }

    this.startDate = this.convertDate(this.startDate);
    this.endDate = this.convertDate(this.endDate);
    this.startDate = this.startDate + 'T00:00:00';
    this.endDate = this.endDate + 'T23:59:59';
    this.startDate = this.auth.ConvertToUTCRangeInput(new Date(this.startDate));
    this.endDate = this.auth.ConvertToUTCRangeInput(new Date(this.endDate));
    if (this.rangeSelectionEnable) {
      this.range.controls['start'].setValue(this.startDate);
      this.range.controls['end'].setValue(this.endDate);
    }

    that.rpm
      .rpm_get(
        `/api/careteam/getteamtasks?TodayDate=${dueToday}&StartDate=${this.startDate}&EndDate=${this.endDate}&RoleId=${this.roleId[0].Id}`
      )
      .then((data) => {
        that.http_task_list = data;
        this.temporaryTeam_taskList = that.http_task_list.Details;
        this.taskSummary = that.http_task_list.Summary;
        this.getSummaryTaskUpdate(
          this.careTeamSelectTask,
          this.temporaryTeam_taskList
        );
        this.rangeSelectionEnable = false;

        this.tableDataSource = new MatTableDataSource(
          this.http_task_list.Details
        );
        this.changeDetectorRef.detectChanges();
        this.tableDataSource.paginator = this.paginator;
        this.loader2 = false;
      });
  }
  // New Function Related to Navigation from Patient-detail
  getTaskfromPatientdetail() {
    var that = this;
    this.loader2 = true;
    var today = new Date();
    // this.rangeContainer = false;

    // this.careTeamSelectTask = undefined;

    var ThirtyDaysAgo = new Date(today).setDate(today.getDate() + 30);

    var dueToday = this.convertDate(today) + 'T23:59:59';
    dueToday = this.auth.ConvertToUTCRangeInput(new Date(dueToday));
    this.loadingvar = false;

    if (
      this.range.controls.start.value != null &&
      this.range.controls.end.value != null
    ) {
      this.startDate = new Date(this.range.controls.start.value);
      this.endDate = new Date(this.range.controls.end.value);
    } else {
      // this.startDate = ThirtyDaysAgo;
      // this.endDate = today;
      this.startDate = today;
      this.endDate = ThirtyDaysAgo;
    }

    this.startDate = this.convertDate(this.startDate);
    this.endDate = this.convertDate(this.endDate);
    this.startDate = this.startDate + 'T00:00:00';
    this.endDate = this.endDate + 'T23:59:59';
    this.startDate = this.auth.ConvertToUTCRangeInput(new Date(this.startDate));
    this.endDate = this.auth.ConvertToUTCRangeInput(new Date(this.endDate));
    // if (this.rangeSelectionEnable) {
    //   this.range.controls['start'].setValue(this.startDate);
    //   this.range.controls['end'].setValue(this.endDate);
    // }

    that.rpm
      .rpm_get(
        `/api/careteam/getteamtasks?TodayDate=${dueToday}&StartDate=${this.startDate}&EndDate=${this.endDate}&RoleId=${this.roleId[0].Id}`
      )
      .then((data) => {
        that.http_task_list = data;
        this.temporaryTeam_taskList = that.http_task_list.Details;
        this.taskSummary = that.http_task_list.Summary;
        this.getSummaryTaskUpdate(
          this.careTeamSelectTask,
          this.temporaryTeam_taskList
        );
        // this.rangeSelectionEnable = false;

        this.loader2 = false;
      });
  }
  // New Function Related to Navigation from Patient-detail

  TaskNavigationData() {
    this.getTaskfromPatientdetail();
    this.taskFilter();
  }

  taskMemberCount: any;
  taskduetodayCount: any;
  taskSlaBrachedCount: any;
  taskPendingLaterCount: any;
  taskcompletedTaskCount: any;

  getSummaryTaskUpdate(team: any, datasrc: any) {
    if (team != undefined && team != 'undefined') {
      var Datasource = datasrc.filter((data: any) => {
        return data.CareTeamId == team;
      });
      this.taskMemberCount = Datasource[0].Members;
      this.taskduetodayCount = Datasource[0].TasksDueToday;
      this.taskSlaBrachedCount = Datasource[0].SLABreached;
      this.taskPendingLater = Datasource[0].PendingLater;
      this.taskcompletedTaskCount = Datasource[0].CompletedTasks;
    } else {
      this.taskMemberCount = this.taskSummary[0].Members;
      this.taskduetodayCount = this.taskSummary[0].TasksDueToday;
      this.taskSlaBrachedCount = this.taskSummary[0].SLABreached;
      this.taskPendingLater = this.taskSummary[0].PendingLater;
      this.taskcompletedTaskCount = this.taskSummary[0].CompletedTasks;
    }
  }

  Tasks_columnHeader = [
    'TaskID',
    'TaskType',
    'Priority',
    'Patient',
    'AssignedMember',
    'DueDate',
    'Status',
    'GotoTask',
  ];

  dropDownTaskSelect() {
    this.teamtask_table_render = 1;
    this.taskcontainerSelection = 0;

    // this.range.reset();

    this.rangeContainer = false;
    this.getSummaryTaskUpdate(
      this.careTeamSelectTask,
      this.temporaryTeam_taskList
    );

    if (
      this.careTeamSelectTask != undefined &&
      this.careTeamSelectTask != 'undefined'
    ) {
      this.teamNamefilterTask = this.temporaryTeam_taskList.filter(
        (data: any) => {
          return data.CareTeamId == this.careTeamSelectTask;
        }
      );
    } else {
      this.getinitTask();
      this.teamNamefilterTask = this.http_task_list.Details;
    }
    this.tableDataSource = new MatTableDataSource(this.teamNamefilterTask);
    this.tableDataSource.paginator = this.paginator;
    this.paginator.firstPage();
  }

  taskTypeVariable: any;
  getTaskdetailData(TaskType: any, selectedContainer: any) {
    this.taskTypeVariable = TaskType;
    this.searchValueName = false;
    this.searchAssigned = false;
    this.taskAssigned = false;
    this.taskcontainerSelection = selectedContainer;
    this.statusFilterTaskValue = 'status';
    this.priorityTaskFilterValue = 1;
    this.pageIndex = 0;
    this.taskPatientSearch = '';
    this.taskAssigneeSearch = '';
    this.getTaskDetail();
  }
  taskcontainerSelection: any;
  getTaskDetail() {
    this.teamtask_table_render = 2;

    this.rangeContainer = true;
    var that = this;
    this.loader2 = true;
    var today = new Date();
    this.rangeContainer = false;

    var ThirtyDaysAgo = new Date(today).setDate(today.getDate() + 30);

    var dueToday = this.convertDate(today) + 'T23:59:59';
    dueToday = this.auth.ConvertToUTCRangeInput(new Date(dueToday));

    if (
      this.range.controls.start.value != null &&
      this.range.controls.end.value != null
    ) {
      this.startDate = new Date(this.range.controls.start.value);
      this.endDate = new Date(this.range.controls.end.value);
    } else {
      // this.startDate = ThirtyDaysAgo;
      // this.endDate = today;
      this.startDate = today;
      this.endDate = ThirtyDaysAgo;
    }

    this.startDate = this.convertDate(this.startDate);
    this.endDate = this.convertDate(this.endDate);
    this.startDate = this.startDate + 'T00:00:00';
    this.endDate = this.endDate + 'T23:59:59';

    this.startDate = this.auth.ConvertToUTCRangeInput(new Date(this.startDate));
    this.endDate = this.auth.ConvertToUTCRangeInput(new Date(this.endDate));

    var careTeamId;
    if (
      this.careTeamSelectTask == undefined ||
      this.careTeamSelectTask == 'undefined'
    ) {
      careTeamId = -1;
    } else {
      careTeamId = this.careTeamSelectTask;
    }

    that.rpm
      .rpm_get(
        `/api/tasks/getteamtaskbyidandtasktype?TaskType=${this.taskTypeVariable}&TodayDate=${dueToday}&RoleId=${this.roleId[0].Id}&StartDate=${this.startDate}&EndDate=${this.endDate}&CareTeamId=${careTeamId}`
      )
      .then((data) => {
        this.loader2 = false;
        this.httpCompletedTask = data;

        this.tasktableDataSource = new MatTableDataSource(
          this.httpCompletedTask.Details
        );
        this.tasktableDataSource.paginator = this.paginator;
        this.paginator.firstPage();

        this.total_number = this.tasktableDataSource.filteredData.length;
      });
  }

  renderTasktableData(tasktype: any) {
    this.teamtask_table_render = 2;
    this.rangeContainer = true;
    var that = this;
    this.loader2 = true;
    var today = new Date();
    this.rangeContainer = false;

    var ThirtyDaysAgo = new Date(today).setDate(today.getDate() + 30);

    var dueToday = this.convertDate(today) + 'T23:59:59';
    dueToday = this.auth.ConvertToUTCRangeInput(new Date(dueToday));

    if (
      this.range.controls.start.value != null &&
      this.range.controls.end.value != null
    ) {
      this.startDate = new Date(this.range.controls.start.value);
      this.endDate = new Date(this.range.controls.end.value);
    } else {
      // this.startDate = ThirtyDaysAgo;
      // this.endDate = today;
      this.startDate = today;
      this.endDate = ThirtyDaysAgo;
    }

    this.startDate = this.convertDate(this.startDate);
    this.endDate = this.convertDate(this.endDate);
    this.startDate = this.startDate + 'T00:00:00';
    this.endDate = this.endDate + 'T23:59:59';

    this.startDate = this.auth.ConvertToUTCRangeInput(new Date(this.startDate));
    this.endDate = this.auth.ConvertToUTCRangeInput(new Date(this.endDate));
    this.range.controls['start'].setValue(this.startDate);
    this.range.controls['end'].setValue(this.endDate);

    var careTeamId;
    if (
      this.careTeamSelectTask == undefined ||
      this.careTeamSelectTask == 'undefined'
    ) {
      careTeamId = -1;
    } else {
      careTeamId = this.careTeamSelectTask;
    }

    that.rpm
      .rpm_get(
        `/api/tasks/getteamtaskbyidandtasktype?TaskType=${tasktype}&TodayDate=${dueToday}&RoleId=${this.roleId[0].Id}&StartDate=${this.startDate}&EndDate=${this.endDate}&CareTeamId=${careTeamId}`
      )
      .then((data) => {
        this.loader2 = false;
        this.httpCompletedTask = data;
        this.dataTaskFilterSrc = this.httpCompletedTask.Details;
        this.dropdownTaskFilter();
        sessionStorage.removeItem('TaskcontainerSelection');
        sessionStorage.removeItem('startdate');
        sessionStorage.removeItem('enddate');
        sessionStorage.removeItem('statusFilterTaskValue');
        sessionStorage.removeItem('priorityTaskFilterValue');
        sessionStorage.removeItem('SelectedMenu');
        sessionStorage.removeItem('careTeamSelectTask');
        sessionStorage.removeItem('patientNavigationStatus');
        sessionStorage.removeItem('SelectedFilter');
        sessionStorage.removeItem('searchPatientListTask');
        sessionStorage.removeItem('searchAssigneeTask');
      });
  }
  priorityTaskFilterValue: any;
  statusFilterTaskValue: any;

  taskFilter() {
    // this.searchValueName = false;
    // this.searchAssigned = false;
    switch (this.taskcontainerSelection) {
      case 1:
        this.renderTasktableData('DueToday');
        break;
      case 2:
        this.renderTasktableData('SLABreached');
        break;

      case 3:
        this.renderTasktableData('PendingLater');
        break;

      case 4:
        this.renderTasktableData('Completed');
        break;
    }
  }

  dataTaskFilterSrc: any;
  dropdownTaskFilter() {
    let dataSourceTaskFilter;
    let dataPriorityfilter: string;
    let dataStatusFilter: string;
    if (this.priorityTaskFilterValue == 3) {
      dataPriorityfilter = 'High';
    } else if (this.priorityTaskFilterValue == 4) {
      dataPriorityfilter = 'Medium';
    } else if (this.priorityTaskFilterValue == 5) {
      dataPriorityfilter = 'Low';
    } else if (this.priorityTaskFilterValue == 1) {
      dataSourceTaskFilter = this.dataTaskFilterSrc;
    }

    if (this.statusFilterTaskValue == 'todo') {
      dataStatusFilter = 'ToDo';
    } else if (this.statusFilterTaskValue == 'inprogress') {
      dataStatusFilter = 'InProgress';
    } else if (this.statusFilterTaskValue == 'complete') {
      dataStatusFilter = 'Complete';
    } else if (this.statusFilterTaskValue == 'status') {
      dataSourceTaskFilter = this.dataTaskFilterSrc;
    }

    if (this.statusFilterTaskValue != 'status') {
      dataSourceTaskFilter = this.dataTaskFilterSrc.filter(function (
        data: any
      ) {
        return data.Status == dataStatusFilter;
      });
    }

    if (this.priorityTaskFilterValue != 1) {
      dataSourceTaskFilter = dataSourceTaskFilter.filter(function (data: any) {
        return data.Priority == dataPriorityfilter;
      });
    }

    this.tasktableDataSource = new MatTableDataSource(dataSourceTaskFilter);

    if (this.patientNavigationStatus == 'true') {
      this.redirectionFilter(this.tableFilterValuetask);
    }
    this.changeDetectorRef.detectChanges();
    this.tasktableDataSource.paginator = this.paginator;
    // this.paginator.firstPage();

    this.tasktableDataSource.sort = this.sort;
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

  // Update

  @ViewChild(RightSidebarComponent) private rightsidebar: RightSidebarComponent;

  task_variable_update: any;
  task_table_id: any;
  team_alert_id: any;
  today: any;
  taskDueToday: any;
  taskwatcherRole: any;

  navigateteamtask(dataInput: any) {
    this.task_variable_update = true;
    this.task_table_id = dataInput.TaskID;
    this.rightsidebar.getTeamTaskUpdateValue(this.task_table_id);
  }

  navigateteamviewalert(dataInput: any) {
    this.team_alert_id = dataInput.TaskID;
    //this.rightsidebar.getTaskAlertUpdation(dataInput);
    this.rightsidebar.getTaskAlertUpdation(dataInput);
  }
  httpGetAllTask: any;
  temp_dataSource: any;

  getreloadTask() {
    var that = this;
    this.loader2 = true;
    var today = new Date();
    this.range.reset();
    this.rangeContainer = false;
    this.rangeSelectionEnable = true;
    var ThirtyDaysAgo = new Date(today).setDate(today.getDate() + 30);
    var dueToday = this.convertDate(today) + 'T23:59:59';
    dueToday = this.auth.ConvertToUTCRangeInput(new Date(dueToday));
    this.loadingvar = false;

    if (
      this.range.controls.start.value != null &&
      this.range.controls.end.value != null
    ) {
      this.startDate = new Date(this.range.controls.start.value);
      this.endDate = new Date(this.range.controls.end.value);
    } else {
      // this.startDate = ThirtyDaysAgo;
      // this.endDate = today;
      this.startDate = today;
      this.endDate = ThirtyDaysAgo;
    }

    this.startDate = this.convertDate(this.startDate);
    this.endDate = this.convertDate(this.endDate);
    this.startDate = this.startDate + 'T00:00:00';
    this.endDate = this.endDate + 'T23:59:59';
    this.startDate = this.auth.ConvertToUTCRangeInput(new Date(this.startDate));
    this.endDate = this.auth.ConvertToUTCRangeInput(new Date(this.endDate));
    if (this.rangeSelectionEnable) {
      this.range.controls['start'].setValue(this.startDate);
      this.range.controls['end'].setValue(this.endDate);
    }

    that.rpm
      .rpm_get(
        `/api/careteam/getteamtasks?TodayDate=${dueToday}&StartDate=${this.startDate}&EndDate=${this.endDate}&RoleId=${this.roleId[0].Id}`
      )
      .then((data) => {
        that.http_task_list = data;
        this.temporaryTeam_taskList = that.http_task_list.Details;
        this.taskSummary = that.http_task_list.Summary;
        this.getSummaryTaskUpdate(
          this.careTeamSelectTask,
          this.temporaryTeam_taskList
        );
        this.rangeSelectionEnable = false;
        this.loader2 = false;
      });
  }

  getAllTask() {
    this.variable = 2;
    this.CurrentMenu = 2;
    this.getreloadTask();
    this.teamtask_table_render = 2;
    this.taskFilter();
    this.tasktableDataSource = new MatTableDataSource(this.dataTaskFilterSrc);
    this.changeDetectorRef.detectChanges();
    this.tasktableDataSource.paginator = this.paginator;
    this.paginator.firstPage();
    this.tasktableDataSource.sort = this.sort;
  }

  getAllAlert() {
    this.variable = 1;
    this.CurrentMenu = 1;
    this.alertTableonLoading = false;
    this.getinitAlert();
    this.AlertFilter();
    this.alert_table_render = 2;
    this.tableDataSource = new MatTableDataSource(this.dataSrcPiority);
    this.changeDetectorRef.detectChanges();
    this.tableDataSource.paginator = this.paginator;
    this.paginator.firstPage();
    this.tableDataSource.sort = this.sort;
  }

  convertToLocalTime(stillUtc: any) {
    stillUtc = stillUtc + 'Z';
    var local = moment(stillUtc).local().format('YYYY-MM-DD HH:mm:ss');
    return local;
  }

  DateRangeTask() {
    this.searchValueName = false;
    this.searchAssigned = false;
    this.statusFilterTaskValue = 'status';
    this.priorityTaskFilterValue = 1;
    if (this.taskcontainerSelection == 0) {
      this.getinitTask();
    } else {
      var that = this;
      var today = new Date();
      var dueToday = this.convertDate(today) + 'T23:59:59';
      dueToday = this.auth.ConvertToUTCRangeInput(new Date(dueToday));
      this.loadingvar = false;

      if (
        this.range.controls.start.value != null &&
        this.range.controls.end.value != null
      ) {
        this.startDate = new Date(this.range.controls.start.value);
        this.endDate = new Date(this.range.controls.end.value);
      }

      this.startDate = this.convertDate(this.startDate);
      this.endDate = this.convertDate(this.endDate);
      this.startDate = this.startDate + 'T00:00:00';
      this.endDate = this.endDate + 'T23:59:59';
      this.startDate = this.auth.ConvertToUTCRangeInput(
        new Date(this.startDate)
      );
      this.endDate = this.auth.ConvertToUTCRangeInput(new Date(this.endDate));
      if (this.rangeSelectionEnable) {
        this.range.controls['start'].setValue(this.startDate);
        this.range.controls['end'].setValue(this.endDate);
      }

      that.rpm
        .rpm_get(
          `/api/careteam/getteamtasks?TodayDate=${dueToday}&StartDate=${this.startDate}&EndDate=${this.endDate}&RoleId=${this.roleId[0].Id}`
        )
        .then((data) => {
          that.http_task_list = data;
          this.temporaryTeam_taskList = that.http_task_list.Details;
          this.taskSummary = that.http_task_list.Summary;
          this.getSummaryTaskUpdate(
            this.careTeamSelectTask,
            this.temporaryTeam_taskList
          );
          this.loader2 = false;
          this.getTaskDetail();
        });
    }
  }
  //  Table Detailed View
  table_detailed_view = false;
  displayedteamColumns = [
    'Id',
    'TeamName',
    'Members',
    'TotalPatients',
    'TasksDueToday',
    'SLABreached',
    'PendingLater',
    'CompletedTasks',
  ];
  dataSourceteam: any;

  navigateTeamdetail(team: any) {
    this.router.navigate(['/admin/team-detail']);
  }
  navigatePatientdetail(row: any) {
    let route = '/admin/patients_detail';
    this.router.navigate([route], {
      queryParams: { id: row.PatientId, programId: row.PatientProgramID },
      skipLocationChange: true,
    });
    sessionStorage.setItem('patient-page-status', 'team');
    sessionStorage.setItem(
      'AlercontainerSelection',
      this.AlercontainerSelection
    );
    sessionStorage.setItem('teampageIndex', this.pageIndex);
    sessionStorage.setItem('SelectedMenu', this.CurrentMenu);
    sessionStorage.setItem(
      'priorityAlertFilterValue',
      this.priorityFilterValue
    );
    sessionStorage.setItem('statusFilterAlertValue', this.statusFilterValue);
    sessionStorage.setItem('careTeamSelectAlert', this.careTeamSelectAlert);
    sessionStorage.setItem('SelectedFilterAlert', this.tableFilterValuealert);
    sessionStorage.setItem('searchPatientListalert', this.alertSearchValeName);
    sessionStorage.setItem('searchAssigneealert', this.searchAssigned);
  }
  tableFilterValuetask: any;
  tableFilterValuealert: any;
  navigatePatientdetailfromTask(row: any) {
    let route = '/admin/patients_detail';
    this.router.navigate([route], {
      queryParams: { id: row.PatientId, programId: row.PatientProgramId },
      skipLocationChange: true,
    });
    sessionStorage.setItem('patient-page-status', 'team');
    sessionStorage.setItem('SelectedMenu', this.CurrentMenu);
    sessionStorage.setItem(
      'TaskcontainerSelection',
      this.taskcontainerSelection
    );
    sessionStorage.setItem('SelectedMenu', this.CurrentMenu);
    sessionStorage.setItem('startdate', this.range.controls.start.value);
    sessionStorage.setItem('enddate', this.range.controls.end.value);
    sessionStorage.setItem(
      'priorityTaskFilterValue',
      this.priorityTaskFilterValue
    );
    sessionStorage.setItem('statusFilterTaskValue', this.statusFilterTaskValue);
    sessionStorage.setItem('careTeamSelectTask', this.careTeamSelectTask);
    sessionStorage.setItem('teampageIndex', this.pageIndex);
    sessionStorage.setItem('SelectedFilter', this.tableFilterValuetask);
    sessionStorage.setItem('searchPatientListTask', this.searchValueName);
    sessionStorage.setItem('searchAssigneeTask', this.taskAssigned);
  }

  openDialog(title: any, item: any) {
    const dialogConfig = new MatDialogConfig();
    dialogConfig.width = '400px';
    dialogConfig.autoFocus = true;

    dialogConfig.data = {
      title: title,
      item: item,
    };

    this.dialog.open(StatusMessageComponent, dialogConfig);
  }

  sendInfo() {
    this._notificationSvc.info('Hello World', 'This is an information', 5000);
  }
  sendSuccess() {
    this._notificationSvc.success('Hello World', 'This is a success !');
  }

  sendWarning() {
    this._notificationSvc.warning('Hello World', 'This is a warning !');
  }

  sendError() {
    this._notificationSvc.error('Hello World', 'This is an error :(');
  }

  loremIpsum() {
    this._notificationSvc.error(
      'Lorem ipsum',
      'Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed non risus. Suspendisse lectus tortor, dignissim sit amet, adipiscing nec, ultricies sed, dolor. Cras elementum ultrices diam. Maecenas ligula massa, varius a, semper congue, euismod non, mi. Proin porttitor, orci nec nonummy molestie, enim est eleifend mi, non fermentum diam nisl sit amet erat. Duis semper. Duis arcu massa, scelerisque vitae, consequat in, pretium a, enim. Pellentesque congue. Ut in risus volutpat libero pharetra tempor. Cras vestibulum bibendum augue. Praesent egestas leo in pede. Praesent blandit odio eu enim. Pellentesque sed dui ut augue blandit sodales. Vestibulum ante ipsum primis in faucibus orci luctus et ultrices posuere cubilia Curae; Aliquam nibh. Mauris ac mauris sed pede pellentesque fermentum. Maecenas adipiscing ante non diam sodales hendrerit.',
      10000
    );
  }
  ngOnDestroy(): void {
    sessionStorage.removeItem('patientNavigationStatus');
  }
  pageSize: any;
  pageChange(pageEvent: PageEvent) {
    if (pageEvent.pageIndex > pageEvent.previousPageIndex!)
      this.pageSize = pageEvent.pageSize;
    this.pageIndex = pageEvent.pageIndex;
  }
  taskAssigneeSearch: any;
  taskPatientSearch: any;
  alertAssigneeSearch: any;
  alertPatientSearch: any;
  redirectionFilter(value: any) {
    if (value == 'undefined') {
      value = '';
    }
    if (this.tableFilterValuetask != 'undefined') {
      this.tasktableDataSource.filter = value.trim().toLowerCase();
      if (this.searchValueName == true) {
        this.setupTaskFilter('Patient');
        this.taskPatientSearch = value;
      } else if (this.taskAssigned == true) {
        this.setupTaskFilter('AssignedMember');
        this.taskAssigneeSearch = value;
      }
    } else {
      this.taskPatientSearch = value;
      this.taskAssigneeSearch = value;

      return;
    }
  }

  redirectionAlertFilter(value: any) {
    if (value == 'undefined') {
      value = '';
    }
    if (this.tableFilterValuealert != 'undefined') {
      this.tableDataSource.filter = value.trim().toLowerCase();
      if (this.alertSearchValeName == true) {
        this.setupFilter('PatientName');
        this.alertPatientSearch = value;
      } else if (this.searchAssigned == true) {
        this.setupFilter('AssignedMember');
        this.alertAssigneeSearch = value;
      }
    } else {
      this.alertPatientSearch = value;
      this.alertAssigneeSearch = value;

      return;
    }
  }
}
