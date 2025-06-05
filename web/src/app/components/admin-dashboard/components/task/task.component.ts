import { RightSidebarComponent } from './../../shared/right-sidebar/right-sidebar.component';
import { RPMService } from '../../sevices/rpm.service';
import { MatSort, Sort } from '@angular/material/sort';
import { ChartType } from 'chart.js';
import { AuthService } from 'src/app/services/auth.service';
import {
  Component,
  OnInit,
  ViewChild,
  ChangeDetectorRef,
  TemplateRef,
} from '@angular/core';
import { FormGroup, FormControl, Validators } from '@angular/forms';
import { MatTableDataSource } from '@angular/material/table';
import { MatPaginator, PageEvent } from '@angular/material/paginator';
import { DatePipe } from '@angular/common';
import * as _moment from 'moment';
import { default as _rollupMoment } from 'moment';
import { MatDialog, MatDialogConfig } from '@angular/material/dialog';
import { StatusMessageComponent } from '../../shared/status-message/status-message.component';
import { Router, ActivatedRoute } from '@angular/router';
import { DateRangeControlComponent } from '../../shared/date-range-control/date-range-control.component';
import { MasterDataService } from '../../sevices/master-data.service';

const moment = _rollupMoment || _moment;

export const MY_FORMATS = {
  parse: {
    dateInput: 'LL',
  },
  display: {
    dateInput: 'MMM DD, YYYY',
    monthYearLabel: 'MMM YYYY',
    dateA11yLabel: 'LL',
    monthYearA11yLabel: 'MMMM YYYY',
  },
};

@Component({
  selector: 'app-task',
  templateUrl: './task.component.html',
  styleUrls: ['./task.component.scss'],
})
export class TaskComponent implements OnInit {
  @ViewChild('dateRange') dateRangeComponent!: DateRangeControlComponent;
  range = new FormGroup({
    start: new FormControl(),
    end: new FormControl(),
  });
  rangeTask = new FormGroup({
    start: new FormControl(),
    end: new FormControl(),
  });
  @ViewChild(RightSidebarComponent) private rightsidebar: RightSidebarComponent;
  curdate: any;

  item_test = 'urgent';

  date_data = 'July 2022';
  http_worklist: any;
  kpi_alerts: any;
  kpi_tasks: any;
  collection: any;
  taskdataSource: any = new MatTableDataSource();
  @ViewChild(MatSort) sort = new MatSort();

  accessRights: any;

  // Alert Data Page
  tableDataSource: any;
  httpGetAlertData: any;
  loading = true;
  total_number: any;
  http_alertDatabyId: any;
  critical_alertData: any;
  temp_dataSource: any;
  slaBreachData: any;
  openAlertData: any;
  completeStatusData: any;
  http_unassigned: any;
  alertId: any;
  roleData: any;

  RightSideBarObj: RightSidebarComponent;
  @ViewChild(MatPaginator) paginator: MatPaginator;

  // Task Data Page
  containerSelectionpanel: any;
  task_variable_update: any;
  task_table_id: any;
  taskUserList: any;
  httpGetAllTask: any;
  taskDueToday: any;
  taskSlaBreached: any;
  taskPendingLater: any;
  taskwatcherRole: any;
  taskComplete: any;
  today = new Date();
  CurrentMenu: any;
  updatetask_data = false;
  http_unassigned_members: any;
  worklistgettaskbyid: any;
  taskId: any;
  http_TaskMasterData: any;
  http_taskPatientListArray: any;
  http_taskAssigneeList: any;
  filterTaskUserList: any;
  // Schedule Template Start
  ScheduleDataSource: any;
  http_getMasterData: any;
  ScheduleTypeIdArray: any;
  frequencyValue: any;
  filterOptionSchedule: any;
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
  filterOptionUser: any;
  http_getSchedulemasterdata: any;
  scheduleUserList: any;
  roles: any;
  roleId: any;
  editScheduleMon = false;
  editScheduleTue = false;
  editScheduleWed = false;
  editScheduleThur = false;
  editScheduleFri = false;
  editScheduleSat = false;
  editScheduleSun = false;
  editScheduleSubMon = false;
  editScheduleSubTue = false;
  editScheduleSubWed = false;
  editScheduleSubThur = false;
  editScheduleSubFri = false;
  editScheduleSubSat = false;
  editScheduleSubSun = false;
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

  today_date = new Date();
  sevenDaysAgo = new Date(this.today).setDate(this.today.getDate() - 7);
  taskTypeIdArray: any;
  taskStatusIdArray: any;

  // Change Main Screen Based On Menu Click

  loading2: boolean;
  loading3: boolean;
  rangeSelectionEnable: any;

  ChangeScreen(button: any) {
    this.tableDataSource = [];

    this.CurrentMenu = button;
    switch (button) {
      case 1:
        this.variable = 1;
        this.containerSelectionpanel = 1;
        this.searchCreated = false;
        this.searchTaskValueName = false;
        this.alertSearchValeName = false;
        this.alertAssignedName = false;
        this.getAllAlert();
        break;

      case 2:
        this.variable = 2;
        this.rangeSelectionEnable = true;
        this.rangeTask.reset();
        this.searchCreated = false;
        this.searchTaskValueName = false;
        this.alertSearchValeName = false;
        this.alertAssignedName = false;

        this.getAllTask();
        this.taskcontainerSelection = 0;

        // this.loading = true;

        // setTimeout(() =>{
        //   this.loading = false;
        // },500);
        break;

      case 3:
        this.variable = 3;
        this.getTaskScheduleList();
        break;

      case 4:
        this.variable = 4;
        this.getPatientSMS();
        break;

      default:
        this.variable = 1;
        break;
    }
  }

  menu = [
    {
      menu_id: 1,
      menu_title: 'Alert',
    },
    {
      menu_id: 2,
      menu_title: 'Tasks',
    },
    {
      menu_id: 3,
      menu_title: 'Schedule',
    },

    // {
    //   menu_id: 4,
    //   menu_title: 'KPI Performance',
    // },
  ];

  //**********************************************************************/
  // Alert Template Start
  acknowledgeAlertForm = new FormGroup({
    Name: new FormControl(null),
    alertType: new FormControl(null),
    alertdescription: new FormControl(null),
    dueDate: new FormControl(null),
    alertPriority: new FormControl(null),
    alertstatus: new FormControl(null),
    assignee_name: new FormControl(null),
    comments: new FormControl(null),
  });
  http_useraccessrights: any;
  alertAccess: any;

  constructor(
    private rpm: RPMService,
    public datepipe: DatePipe,
    private changeDetectorRef: ChangeDetectorRef,
    public dialog: MatDialog,
    private rpmservice: RPMService,
    private auth: AuthService,
    private _router: Router,
    private _route: ActivatedRoute,
    private masterDataService:MasterDataService
  ) {
    this.initializeSessionData();
    this.getMasterdata();
    this.loadScheduleMasterData();
    this.loadTaskMasterData();
    this.initializeDateRangeForm();
  }

  private initializeSessionData(): void {
    this.userid = sessionStorage.getItem('userid') || '';
    this.user_name = sessionStorage.getItem('user_name') || '';

    const unassigned = sessionStorage.getItem('unassigned_members');
    this.http_unassigned = unassigned ? JSON.parse(unassigned) : [];

    const access = sessionStorage.getItem('useraccessrights');
    this.http_useraccessrights = access ? JSON.parse(access) : { UserAccessRights: [] };

    this.alertAccess = this.http_useraccessrights.UserAccessRights.filter(
      (data: { AccessName: string }) => data.AccessName === 'AlertAccess'
    );

    const roles = sessionStorage.getItem('Roles');
    this.roleId = roles ? JSON.parse(roles) : [];

    this.rangeSelectionEnable = true;

    const unassignedMembers = sessionStorage.getItem('unassigned_members');
    this.http_unassigned_members = unassignedMembers ? JSON.parse(unassignedMembers) : [];
  }
  private loadScheduleMasterData(): void {
    const roleId = this.roleId[0]?.Id;
    if (!roleId) return;

    this.masterDataService.getScheduleMasterData(roleId)
      .then(res => {
        this.http_getSchedulemasterdata = res.rawData;
        this.scheduleUserList = res.patientOrContacts;
      })
      .catch(err => {
        console.error('Failed to load schedule master data', err);
      });
  }
  private loadTaskMasterData(): void {
    const roleId = this.roleId[0]?.Id;
    const patientId = null;

    if (!roleId || !patientId) return;

    this.masterDataService.getFilteredTaskAssignees(roleId, patientId)
      .then((result) => {
        this.http_TaskMasterData = result.taskMasterData;
        this.http_taskPatientListArray = result.taskMasterData.PatientList;
        this.http_taskAssigneeList = result.filteredAssignees;
      })
      .catch((err) => {
        console.error('Error loading task master data:', err);
      });
  }

  private initializeDateRangeForm(): void {
    const today = new Date();
    const month = today.getMonth();
    const year = today.getFullYear();

    this.campaignOne = new FormGroup({
      start: new FormControl(new Date(year, month, 13)),
      end: new FormControl(new Date(year, month, 16)),
      date: new FormControl(moment()),
    });
  }


  getMasterdata() {
    var that = this;
    this.rpm
      .rpm_get('/api/authorization/operationalmasterdata')
      .then((data) => {
        that.http_getMasterData = data;
        sessionStorage.setItem(
          'operational_masterdata',
          JSON.stringify(that.http_getMasterData)
        );
        that.http_getMasterData = sessionStorage.getItem(
          'operational_masterdata'
        );
        that.http_getMasterData = JSON.parse(that.http_getMasterData);

        that.ScheduleTypeIdArray = that.http_getMasterData.filter(function (
          data: any
        ) {
          return data.Type == 'PatientShType';
        });
        that.taskTypeIdArray = that.http_getMasterData.filter(function (
          data: any
        ) {
          return data.Type == 'PatientTaskType';
        });

        that.taskStatusIdArray = that.http_getMasterData.filter(function (
          data: any
        ) {
          return data.Type == 'Task';
        });
      });
  }
  getInitialTasks() {
    this.rpm.rpm_get('/api/tasks/worklistgettasks').then((data) => {
      this.loading = false;
      this.httpGetAllTask = data;
      this.temp_dataSource = this.httpGetAllTask.Details;
      this.tableDataSource = new MatTableDataSource(
        this.httpGetAllTask.Details
      );
      this.tableDataSource.paginator = this.paginator;
      this.tableDataSource.sort = this.sort;
      this.total_number = this.tableDataSource.filteredData.length;

      this.taskSlaBreached = this.temp_dataSource.filter(function (data: any) {
        return data.SLABreached == 1;
      });

      this.taskComplete = this.temp_dataSource.filter(function (data: any) {
        return data.Status == 'Complete';
      });
      this.taskDueToday = this.temp_dataSource.filter((data: any) => {
        return (
          new Date(data.taskDueToday).setHours(0, 0, 0, 0) <=
            this.today.setHours(0, 0, 0, 0) && data.Status != 'Complete'
        );
      });

      var that = this;
      this.taskPendingLater = this.temp_dataSource.filter(function (data: any) {
        // return data.Status != 'ToDo';
        return (
          new Date(data.taskDueToday).setHours(0, 0, 0, 0) >
            that.today.setHours(0, 0, 0, 0) && data.Status != 'Complete'
        );
      });
      this.taskwatcherRole = this.temp_dataSource.filter(function (data: any) {
        return data.SLABreached == 1;
      });

      this.tableDataSource = new MatTableDataSource(
        this.httpGetAllTask.Details
      );
      this.changeDetectorRef.detectChanges();
      this.tableDataSource.paginator = this.paginator;
      this.tableDataSource.sort = this.sort;
    });
  }
  // Api Call Get All Alert for WorkList Start
  AlertTypeArray: any;

  getAllAlert() {
    this.loading = true;
    this.statusFilterValue = 'status';
    this.priorityFilterValue = undefined;
    this.pageIndex = 0;
    this.rpm
      .rpm_get('/api/alerts/worklistgetalerts?RoleId=' + this.roleId[0].Id)
      .then((data) => {
        this.httpGetAlertData = data;

        this.temp_dataSource = this.httpGetAlertData.Details;
        this.temp_dataSource = this.temp_dataSource.filter(function (
          data: any
        ) {
          return data.Status != 'Complete';
        });
        this.tableDataSource = new MatTableDataSource(this.temp_dataSource);
        this.tableDataSource.paginator = this.paginator;
        this.tableDataSource.sort = this.sort;

        // this.total_number = this.httpGetAlertData.Summary[0].TotalPatients;
        var d = new Date();

        this.total_number = this.tableDataSource.filteredData.length;
        this.critical_alertData = this.temp_dataSource.filter(function (
          data: any
        ) {
          return data.Prioriy == 'Critical';
        });

        this.slaBreachData = this.httpGetAlertData.Details.filter(
          (data: any) => {
            return data.SLABreached == 1;
          }
        );
        this.openAlertData = this.temp_dataSource.filter(function (data: any) {
          return data.Status != 'Complete';
        });
        this.completeStatusData = this.httpGetAlertData.Details.filter(
          (data: any) => {
            return data.Status == 'Complete';
          }
        );
        // this.onClickOpenalert();

        this.AlertTypeArray = this.httpGetAlertData.AlertTypes;
        this.loading = false;
      });
  }

  // Api Call Get All Alert for WorkList End

  // Alert Table Column Header

  Alert_Header = [
    'VitalAlert',
    'PatientId',
    'PatientProgramID',
    'Prioriy',
    'PatientName',
    'AssignedMember',
    'ProgramName',
    'DueDate',
    'Status',
    'action',
  ];
  alertStatusArray = [
    {
      value: 1,
      name: 'ToDo',
    },
    {
      value: 2,
      name: 'InProgress',
    },
    {
      value: 3,
      name: 'Complete',
    },
  ];

  //  Filter Based On Button Click Alert Page Start
  onClickCriticalAlert() {
    this.containerSelectionpanel = 2;
    this.pageIndex = 0;
    this.getReloadDataAlert();
    this.statusFilterValue = 'status';
    this.priorityFilterValue = undefined;
    this.alertAssignedName = false;
    this.alertSearchValeName = false;
  }

  onClickSlaBreach() {
    this.containerSelectionpanel = 3;
    this.pageIndex = 0;
    this.getReloadDataAlert();
    this.statusFilterValue = 'status';
    this.priorityFilterValue = undefined;
    this.alertAssignedName = false;
    this.alertSearchValeName = false;
  }
  onClickOpenalert() {
    this.containerSelectionpanel = 4;
    this.pageIndex = 0;
    this.getReloadDataAlert();
    this.statusFilterValue = 'status';
    this.priorityFilterValue = undefined;
    this.alertAssignedName = false;
    this.alertSearchValeName = false;
  }
  teamtask_table_render = 1;
  onClickCompletedalert() {
    this.containerSelectionpanel = 5;
    this.pageIndex = 0;
    this.getReloadDataAlert();
    this.statusFilterValue = 'status';
    this.priorityFilterValue = undefined;
    this.alertAssignedName = false;
    this.alertSearchValeName = false;
  }
  onClickpatientAssigned() {
    this.containerSelectionpanel = 1;
    this.statusFilterValue = 'status';
    this.priorityFilterValue = undefined;
  }

  //  Filter Based On Button Click Alert Page End
  // Get Api Call For Update Alert Get alert by Id Start
  navigateAcknowldge(dataInput: any) {
    this.rightsidebar.acknowledgealert(dataInput);
  }

  statusFilterValue: any;

  priorityFilterValue: any;
  filter = false;
  filterds: any;
  dropdownFilter() {
    let dataSourceAlertFilter;
    this.alertSearchValeName = false;
    this.alertAssignedName = false;
    if (this.containerSelectionpanel == 1) {
      this.temp_dataSource = this.httpGetAlertData.Details;
    } else if (this.containerSelectionpanel == 2) {
      this.temp_dataSource = this.critical_alertData;
    } else if (this.containerSelectionpanel == 3) {
      this.temp_dataSource = this.slaBreachData;
    } else if (this.containerSelectionpanel == 4) {
      this.temp_dataSource = this.openAlertData;
    } else if (this.containerSelectionpanel == 5) {
      this.temp_dataSource = this.completeStatusData;
    }

    let dataPriorityfilter: string;
    let dataStatusFilter: string;
    if (this.priorityFilterValue == 'Critical') {
      dataPriorityfilter = 'Critical';
    } else if (this.priorityFilterValue == 'Cautious') {
      dataPriorityfilter = 'Cautious';
    } else if (this.priorityFilterValue == 'Missing') {
      dataPriorityfilter = 'Missing';
    } else if (this.priorityFilterValue == 'Normal') {
      dataPriorityfilter = 'Normal';
    } else if (this.priorityFilterValue == undefined) {
      dataSourceAlertFilter = this.temp_dataSource;
    }

    if (this.statusFilterValue == 'todo') {
      dataStatusFilter = 'ToDo';
    } else if (this.statusFilterValue == 'inprogress') {
      dataStatusFilter = 'InProgress';
    } else if (this.statusFilterValue == 'complete') {
      dataStatusFilter = 'Complete';
    } else if (this.statusFilterValue == 'status') {
      dataSourceAlertFilter = this.temp_dataSource;
    }

    if (this.statusFilterValue != 'status') {
      dataSourceAlertFilter = this.temp_dataSource.filter(function (data: any) {
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
    this.tableDataSource.paginator = this.paginator;
    this.tableDataSource.sort = this.sort;
  }

  dropdownTaskFilter() {
    this.searchTaskValueName = false;
    this.searchCreated = false;
    let dataSourceTaskFilter;
    if (this.taskcontainerSelection == 0) {
      //this.temp_dataSource = this.httpGetAllTask.Details;
      this.temp_dataSource = this.taskDueToday;
    } else if (this.taskcontainerSelection == 1) {
      this.temp_dataSource = this.taskDueToday;
    } else if (this.taskcontainerSelection == 2) {
      this.temp_dataSource = this.taskSlaBreached;
    } else if (this.taskcontainerSelection == 3) {
      this.temp_dataSource = this.taskPendingLater;
    } else if (this.taskcontainerSelection == 4) {
      this.temp_dataSource = this.taskComplete;
    }

    let dataPriorityfilter: string;
    let dataStatusFilter: string;
    if (this.priorityTaskFilterValue == 3) {
      dataPriorityfilter = 'High';
    } else if (this.priorityTaskFilterValue == 4) {
      dataPriorityfilter = 'Medium';
    } else if (this.priorityTaskFilterValue == 5) {
      dataPriorityfilter = 'Low';
    } else if (this.priorityTaskFilterValue == 1) {
      dataSourceTaskFilter = this.temp_dataSource;
    }

    if (this.statusFilterTaskValue == 'todo') {
      dataStatusFilter = 'ToDo';
    } else if (this.statusFilterTaskValue == 'inprogress') {
      dataStatusFilter = 'InProgress';
    } else if (this.statusFilterTaskValue == 'complete') {
      dataStatusFilter = 'Complete';
    } else if (this.statusFilterTaskValue == 'status') {
      dataSourceTaskFilter = this.temp_dataSource;
    }

    if (this.statusFilterTaskValue != 'status') {
      dataSourceTaskFilter = this.temp_dataSource.filter(function (data: any) {
        return data.Status == dataStatusFilter;
      });
    }

    if (this.priorityTaskFilterValue != 1) {
      dataSourceTaskFilter = dataSourceTaskFilter.filter(function (data: any) {
        return data.Priority == dataPriorityfilter;
      });
    }

    this.tableDataSource = new MatTableDataSource(dataSourceTaskFilter);
    this.tableDataSource.paginator = this.paginator;
    this.tableDataSource.sort = this.sort;
  }

  setupFilter(column: string) {
    this.tableDataSource.filterPredicate = (d: any, filter: string) => {
      const textToSearch = (d[column] && d[column].toLowerCase()) || '';
      return textToSearch.indexOf(filter) !== -1;
    };
  }

  applyDataFilter(filterValue: any) {
    filterValue = (<HTMLInputElement>filterValue.target).value.trim(); // Remove whitespace
    filterValue = filterValue.toLowerCase();
    if (this.CurrentMenu == 2) {
      this.tableFilterValuetask = filterValue;
    } else if (this.CurrentMenu == 1) {
      this.tableFilterValuealert = filterValue;
    }

    // MatTableDataSource defaults to lowercase matches
    this.tableDataSource.filter = filterValue;
  }

  // Get Api Call For Update Alert Get alert by Id End

  // Api For Update Alert End

  // Alert Template End
  // *****************************************************************************************************
  // Task Template Start

  updatetaskform = new FormGroup({
    patientname: new FormControl(null, [Validators.required]),
    tasktype: new FormControl(null),
    taskdes: new FormControl(null),
    taskpriority: new FormControl(null),
    updatedate: new FormControl(null),
    taskstatus: new FormControl(null),
    assigneename: new FormControl(null),
    task_comment: new FormControl(null),
  });
  startDate: any;
  endDate: any;
  getAllTask() {
    this.loading2 = true;
    this.pageIndex = 0;
    this.statusFilterTaskValue = 'status';
    this.priorityTaskFilterValue = 1;
    var that = this;
    var today = new Date();

    var ThirtyDaysAgo = new Date(today).setDate(today.getDate() + 30);
    if (
      this.rangeTask.controls.start.value != null &&
      this.rangeTask.controls.end.value != null
    ) {
      this.startDate = new Date(this.rangeTask.controls.start.value);
      this.endDate = new Date(this.rangeTask.controls.end.value);
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
      this.rangeTask.controls['start'].setValue(this.startDate);
      this.rangeTask.controls['end'].setValue(this.endDate);
    }

    this.rpm
      .rpm_get(
        `/api/tasks/worklistgettasks?StartDate=${this.startDate}&EndDate=${this.endDate}`
      )
      .then((data) => {
        this.httpGetAllTask = data;
        this.temp_dataSource = this.httpGetAllTask.Details;

        this.temp_dataSource = this.temp_dataSource.filter(function (
          data: any
        ) {
          return data.Status != 'Complete';
        });
        this.tableDataSource = new MatTableDataSource(this.temp_dataSource);

        this.tableDataSource.sort = this.sort;

        this.total_number = this.tableDataSource.filteredData.length;

        this.taskSlaBreached = this.httpGetAllTask.Details.filter(function (
          data: any
        ) {
          return data.SLABreached == 1;
        });

        this.taskComplete = this.httpGetAllTask.Details.filter(function (
          data: any
        ) {
          // this.taskComplete = this.temp_dataSource.filter(function(data:any) {

          return data.Status == 'Complete';
        });
        this.today = new Date();
        this.taskDueToday = this.temp_dataSource.filter((data: any) => {
          return (
            new Date(this.convertToLocalTime(data.DueDate)).setHours(
              0,
              0,
              0,
              0
            ) <= this.today.setHours(0, 0, 0, 0) && data.Status != 'Complete'
          );
        });

        var that = this;
        this.taskPendingLater = this.temp_dataSource.filter((data: any) => {
          return (
            new Date(this.convertToLocalTime(data.DueDate)).setHours(
              0,
              0,
              0,
              0
            ) > that.today.setHours(0, 0, 0, 0) && data.Status != 'Complete'
          );
        });
        this.taskwatcherRole = this.temp_dataSource.filter(function (
          data: any
        ) {
          return data.SLABreached == 1;
        });
        this.loading2 = false;
        this.rangeSelectionEnable = false;
        this.tableDataSource.paginator = this.paginator;
        this.paginator.firstPage();
      });
  }

  Tasks_columnHeader = [
    'TaskID',
    'TaskType',
    'Priority',
    'Patient',
    'PatientId',
    'PatientProgramId',
    'CreatedBy',
    'DueDate',
    'Status',
    'action',
  ];
  TaskpriorityArray = [
    {
      value: 1,
      name: 'High',
    },
    {
      value: 2,
      name: 'Medium',
    },

    {
      value: 3,
      name: 'Low',
    },
  ];
  taskcontainerSelection: any;
  onClickTaskTotal() {
    this.tableDataSource = new MatTableDataSource(this.httpGetAllTask.Details);
    this.tableDataSource.paginator = this.paginator;
    this.tableDataSource.sort = this.sort;

    this.total_number = this.tableDataSource.filteredData.length;
    this.taskcontainerSelection = 0;
  }
  onClickTaskPendingLater() {
    this.taskcontainerSelection = 3;
    this.pageIndex = 0;
    this.getReloadDataTask();
  }

  onClickTaskSlaBreach() {
    this.taskcontainerSelection = 2;
    this.pageIndex = 0;
    this.getReloadDataTask();
  }
  onClickTaskdue() {
    this.taskcontainerSelection = 1;
    this.pageIndex = 0;
    this.getReloadDataTask();
  }
  onClickCompletedTask() {
    this.taskcontainerSelection = 4;
    this.pageIndex = 0;
    this.getReloadDataTask();
  }

  navigateupdatetask(dataInput: any) {
    this.task_variable_update = true;
    this.task_table_id = dataInput.TaskID;
    this.rightsidebar.getTaskUpdateValue(this.task_table_id);
  }

  // Task Template End

  // Schedule Template Start

  // Api Call For task Schedule

  ScheduleData: any;
  getTaskScheduleList() {
    this.range.reset();
    this.today = new Date();

    var firstDay = new Date(this.today.getFullYear(), this.today.getMonth(), 1);
    var lastDay = new Date(
      this.today.getFullYear(),
      this.today.getMonth() + 1,
      0
    );

    var startdate = this.convertDate(firstDay);

    var enddate = this.convertDate(lastDay);
    this.loading3 = true;

    this.rpm
      .rpm_get(
        `/api/schedules/getworklistschedules?StartDate=${startdate}&EndDate=${enddate}`
      )
      .then((data) => {
        this.loading3 = false;
        this.ScheduleDataSource = data;
      });
  }

  ScheduleDatabyId: any;

  filterScheduleUser(val: any) {
    return this.scheduleUserList.filter((option: { Name: any }) =>
      option.Name.toLowerCase().includes(val.toString().toLowerCase())
    );
  }

  getOptionScheduleUserText(option: { Name: any }) {
    return option.Name;
  }
  filterTaskUser(val: any) {
    return this.http_taskPatientListArray.filter(
      (option: { PatientName: any }) =>
        option.PatientName.toLowerCase().includes(val.toString().toLowerCase())
    );
  }
  getOptionTaskUserNameText(option: { PatientName: any }) {
    return option.PatientName;
  }
  selectedUser(event: { option: { value: any } }) {
    this.SelectedUserData = event.option.value;
    this.SelectUserId = this.SelectedUserData.Id;
  }
  datestart: any;
  dateend: any;
  dateRangeChange(
    dateRangeStart: HTMLInputElement,
    dateRangeEnd: HTMLInputElement
  ) {
    this.datestart = dateRangeStart;
    this.dateend = dateRangeEnd;
    var that = this;
    if (dateRangeEnd.value != null && dateRangeStart.value != null) {
      //&& ((dateRangeStart.value) < (dateRangeEnd.value))
      that.rpm
        .rpm_get(
          `/api/schedules/getworklistschedules?StartDate=${dateRangeStart.value}&EndDate=${dateRangeEnd.value}`
        )
        .then((data) => {
          this.ScheduleDataSource = data;
        });
    }
  }
  dateRangeChangeSchedule() {
    var that = this;
    if (this.dateend.value != null && this.datestart.value != null) {
      //&& ((dateRangeStart.value) < (dateRangeEnd.value))
      that.rpm
        .rpm_get(
          `/api/schedules/getworklistschedules?StartDate=${this.datestart.value}&EndDate=${this.dateend.value}`
        )
        .then((data) => {
          this.ScheduleDataSource = data;
        });
    }
  }
  weekSelectionFrequency: any;
  schedule_edit_id: any;
  userid: any;
  user_name: any;
  navigateEditSchedule(id: any) {
    this.rightsidebar.navigateEditSchedule_from_Worklist(id);
  }

  // Schedule Template End

  test: any;
  campaignOne: FormGroup;
  chart: any;

  p: number[] = [];
  myCompOneObj: any;
  variable: any;
  patientNavigationStatus: any;
  SelectedMenu: any;
  taskpatientSearch: any;
  ngOnInit(): void {
    //console.log('Initial date range:', this.dateRangeComponent.rangeTask.value);
    // this.filterTable();

    sessionStorage.removeItem('patient-page-status');

    this._route.queryParams.subscribe((params) => {
      sessionStorage.removeItem('patient-page-status');
      this.patientNavigationStatus = sessionStorage.getItem(
        'patientNavigationStatus'
      );
      if (this.patientNavigationStatus == 'true') {
        this.CurrentMenu = sessionStorage.getItem('SelectedMenu');

        this.containerSelectionpanel = sessionStorage.getItem(
          'containerSelectionalert'
        );
        this.containerSelectionpanel = parseInt(this.containerSelectionpanel);
        this.taskcontainerSelection = sessionStorage.getItem(
          'containerSelectiontask'
        );
        this.taskcontainerSelection = parseInt(this.taskcontainerSelection);

        this.variable = parseInt(this.CurrentMenu);
        this.pageIndex = sessionStorage.getItem('PageIndex');
        this.pageIndex = parseInt(this.pageIndex);

        var alertData = sessionStorage.getItem('taskPriority');
        this.priorityTaskFilterValue = parseInt(alertData!);
        this.priorityFilterValue = sessionStorage.getItem('alertPriority');
        this.statusFilterTaskValue = sessionStorage.getItem('taskStatus');
        this.statusFilterValue = sessionStorage.getItem('alertStatus');
        if (
          this.statusFilterValue == null ||
          this.statusFilterValue == 'undefined'
        ) {
          this.statusFilterValue = 'status';
        }
        if (
          this.priorityFilterValue == null ||
          this.priorityFilterValue == 'undefined'
        ) {
          this.priorityFilterValue = undefined;
        }
        if (
          this.statusFilterTaskValue == null ||
          this.statusFilterTaskValue == 'undefined'
        ) {
          this.statusFilterTaskValue = 'status';
        }
        if (this.priorityTaskFilterValue == 0) {
          this.priorityTaskFilterValue = 1;
        }
        if (this.CurrentMenu == 1) {
          this.tableFilterValuealert = sessionStorage.getItem(
            'SelectedFilterAlert'
          );
          this.alertSearchValeName = sessionStorage.getItem(
            'alertSearchValeName'
          );
          this.alertAssignedName = sessionStorage.getItem('alertAssignedName');
          if (this.alertSearchValeName == 'true') {
            this.alertSearchValeName = true;
          } else {
            this.alertSearchValeName = false;
          }
          if (this.alertAssignedName == 'true') {
            this.alertAssignedName = true;
          } else {
            this.alertAssignedName = false;
          }
          this.navigationPageAlertReload();
        } else if (this.CurrentMenu == 2) {
          this.loading = false;
          this.tableFilterValuetask = sessionStorage.getItem('SelectedFilter');
          this.searchTaskValueName = sessionStorage.getItem(
            'searchPatientListTask'
          );
          if (this.searchTaskValueName == 'true') {
            this.searchTaskValueName = true;
          } else {
            this.searchTaskValueName = false;
          }
          this.searchCreated = sessionStorage.getItem('searchAssigneeTask');
          if (this.searchCreated == 'true') {
            this.searchCreated = true;
          } else {
            this.searchCreated = false;
          }
          this.startDate = sessionStorage.getItem('startdate');
          this.endDate = sessionStorage.getItem('enddate');
          this.rangeTask.controls['start'].setValue(this.startDate);
          this.rangeTask.controls['end'].setValue(this.endDate);
          this.navigationPageTaskReload();
        }
      } else {
        var that = this;
        this.loading = true;
        this.loading2 = true;
        this.loading3 = true;
        this.searchTaskValueName = false;
        this.searchCreated = false;
        this.alertAssignedName = false;

        this.containerSelectionpanel = 1;
        this.taskcontainerSelection = 0;
        this.priorityFilterValue = undefined;
        this.statusFilterTaskValue = 'status';
        this.statusFilterValue = 'status';
        this.priorityTaskFilterValue = 1;
        this.searchCreated = false;
        this.searchTaskValueName = false;
        this.alertSearchValeName = false;
        this.alertAssignedName = false;

        this.getTaskScheduleList();

        if (this.alertAccess && this.alertAccess[0].AccessRight == 'F') {
          this.variable = 1;
          this.CurrentMenu = 1;
          this.getAllAlert();
          this.loading2 = false;
        } else {
          this.variable = 2;
          this.CurrentMenu = 2;
          this.getAllTask();
          this.loading = false;
        }
      }
    });

    // Role Based Aceess End

    // graph:
    // this.myCompOneObj = new RightSidebarComponent(this.model,this.http,this.rpmservice,this._router,this.dialog);
    // this.RightSideBarObj = new RightSidebarComponent(this.model,this.http,this.rpmservice,this._router,this.dialog);
    // this.test = 'Ashwin';

    // this.doughnutChartOptions.centerText = 'Total Alerts:2';
    // this.doughnutChartOptionsOne.centerAlert = 'Total Tasks:2';
    // this.doughnutChartPlugins=[{
    //   beforeDraw(chart:any ) {
    //     const ctx = chart.ctx;
    //     const txt = 'Center Text';

    //     //Get options from the center object in options
    //     const sidePadding = 60;
    //     const sidePaddingCalculated = (sidePadding / 100) * (chart.innerRadius * 2)

    //     ctx.textAlign = 'center';
    //     ctx.textBaseline = 'middle';
    //     const centerX = ((chart.chartArea.left + chart.chartArea.right) / 2);
    //     const centerY = ((chart.chartArea.top + chart.chartArea.bottom) / 2);

    //     //Get the width of the string and also the width of the element minus 10 to give it 5px side padding
    //     const stringWidth = ctx.measureText(txt).width;
    //     const elementWidth = (chart.innerRadius * 2) - sidePaddingCalculated;

    //     // Find out how much the font can grow in width.
    //     const widthRatio = elementWidth / stringWidth;
    //     const newFontSize = Math.floor(30 * widthRatio);
    //     const elementHeight = (chart.innerRadius * 2);

    //     // Pick a new font size so it will not be larger than the height of label.
    //     const fontSizeToUse = Math.min(newFontSize, elementHeight);

    //     ctx.font = fontSizeToUse + 'px Arial';
    //     ctx.fillStyle = 'black';

    //     // Draw text in center
    //     ctx.fillText(chart.options.centerText, centerX, centerY);
    //   }
    // }];
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

  Tasks_dataSource: any;

  Schedule_dataSource: any;

  openDialog(title: any, item: any) {
    const dialogConfig = new MatDialogConfig();
    dialogConfig.width = '400px';

    // dialogConfig.disableClose = true;
    dialogConfig.autoFocus = true;

    dialogConfig.data = {
      title: title,
      item: item,
    };

    this.dialog.open(StatusMessageComponent, dialogConfig);
  }

  currentUser: any;
  // Schedule Template

  displayedColumn: string[] = [
    'date',
    'time',
    'Schedule',
    'description',
    'allcontacts',
  ];

  // searchCreated = false;
  // searchTaskValueName = false;
  // alertSearchValeName = false;
  // alertAssignedName = false;
  searchCreated: any;
  searchTaskValueName: any;
  alertSearchValeName: any;
  alertAssignedName: any;
  taskAssigneeSearch: any;

  serachTaskAssigned() {
    this.searchCreated = !this.searchCreated;
    this.searchTaskValueName = false;
    this.taskAssigneeSearch = '';
  }
  serachTaskAssignedClose() {
    this.searchCreated = false;
    this.getReloadDataTask();
  }
  searchTaskNameClick() {
    this.searchTaskValueName = !this.searchTaskValueName;
    this.searchCreated = false;
    this.taskpatientSearch = '';
  }
  searchTaskNameClose() {
    this.searchTaskValueName = false;
    this.getReloadDataTask();
  }
  applyFilter(event: Event) {
    const filterValue = (event.target as HTMLInputElement).value;
    this.tableDataSource.filteredData.filter = filterValue.trim().toLowerCase();
  }

  serachAlertAssigned() {
    this.alertAssignedName = !this.alertAssignedName;
    this.alertSearchValeName = false;
    this.alertAssigneeSearch = '';
  }
  seachAssignedclose() {
    this.alertAssignedName = false;
    this.getReloadDataAlert();
  }
  searchAlertNameClick() {
    this.alertSearchValeName = !this.alertSearchValeName;
    this.alertAssignedName = false;
    this.alertPatientSearch = '';
  }
  searchAlertNameclose() {
    this.alertSearchValeName = false;
    this.getReloadDataAlert();
  }
  applyAlertFilter(event: Event) {
    const filterValue = (event.target as HTMLInputElement).value;
    this.tableDataSource.filter = filterValue.trim().toLowerCase();
  }
  searchTaskValue: any;

  onFocusOutEvent(event: any) {
    // this.alertSearchValeName = false;
    // this.alertAssignedName =false;
    this.searchTaskValue = '';
    this.temp_dataSource = this.httpGetAllTask.Details;
    this.tableDataSource = new MatTableDataSource(this.temp_dataSource);
    this.tableDataSource.paginator = this.paginator;
    this.tableDataSource.sort = this.sort;
  }

  completed_time_task = 10;
  completed_sla_task = 12;
  open_task = 14;

  completed_alert_task = 12;
  completed_alert_sla = 15;
  open_alert = 23;

  schedule_data = false;
  task_data = false;
  alert_data = true;
  ontaskClickCancel() {
    this.alert_data = true;
    this.schedule_data = false;
  }
  onupdateTaskCancel() {
    this.alert_data = true;
    this.schedule_data = false;
  }
  onClickCancel() {
    this.schedule_data = false;
    this.alert_data = true;
  }
  add_task() {
    this.task_variable_update = false;
    this.rightsidebar.addMenuChoice(4);
  }

  durationValue = 10;
  increment() {
    this.durationValue++;
  }
  decrement() {
    if (this.durationValue > 0) {
      this.durationValue--;
    } else {
      this.durationValue = 0;
    }
  }
  rolelist: any;
  backtohome() {
    // let route = '/admin/home';
    // this._router.navigate([route]);
    this.rolelist = sessionStorage.getItem('Roles');
    this.rolelist = JSON.parse(this.rolelist);
    if (this.rolelist[0].Id == 7) {
      this._router.navigate(['/admin/patient-home']);
    } else {
      this._router.navigate(['/admin/home']);
    }
  }

  getOptionUserText(option: { username: any }) {
    return option.username;
  }
  addSchedule() {
    this.rightsidebar.addMenuChoice(3);
  }

  priorityArray = [
    {
      Id: 1,
      Name: 'High',
    },
    {
      Id: 2,
      Name: 'Medium',
    },
    {
      Id: 3,
      Name: 'Normal',
    },
    {
      Id: 4,
      Name: 'Urgent',
    },
  ];


  // Table Filter

  // Task Template
  statusFilterTaskValue: any;
  statusFilterTaskResult(data: any) {
    this.temp_dataSource = this.httpGetAllTask.Details;
    let dataSourceAlertFilter;

    if (data == 2) {
      dataSourceAlertFilter = this.temp_dataSource.filter(function (data: any) {
        return data.Status == 'ToDo';
      });
    } else if (data == 3) {
      dataSourceAlertFilter = this.temp_dataSource.filter(function (data: any) {
        return data.Status == 'InProgress';
      });
    } else if (data == 1) {
      dataSourceAlertFilter = this.temp_dataSource;
    }
    this.tableDataSource = new MatTableDataSource(dataSourceAlertFilter);
    this.tableDataSource.paginator = this.paginator;
    this.tableDataSource.sort = this.sort;
  }

  priorityTaskFilterValue: any;
  PrirotyTaskFilterResult(data: any) {
    this.temp_dataSource = this.httpGetAllTask.Details;
    let dataSourceAlertFilter;

    if (data == 2) {
      dataSourceAlertFilter = this.temp_dataSource.filter(function (data: any) {
        return data.Priority == 'Urgent';
      });
    } else if (data == 3) {
      dataSourceAlertFilter = this.temp_dataSource.filter(function (data: any) {
        return data.Priority == 'High';
      });
    } else if (data == 4) {
      dataSourceAlertFilter = this.temp_dataSource.filter(function (data: any) {
        return data.Priority == 'Medium';
      });
    } else if (data == 5) {
      dataSourceAlertFilter = this.temp_dataSource.filter(function (data: any) {
        return data.Priority == 'Low';
      });
    } else if (data == 1) {
      dataSourceAlertFilter = this.temp_dataSource;
    }
    this.tableDataSource = new MatTableDataSource(dataSourceAlertFilter);
    this.tableDataSource.paginator = this.paginator;
    this.tableDataSource.sort = this.sort;
  }
  public ScheduleSort() {
    this.ScheduleDataSource.reverse();
  }
  navigatePatientdetail(row: any) {
    let route = '/admin/patients_detail';
    this._router.navigate([route], {
      queryParams: { id: row.PatientId, programId: row.PatientProgramID },
      skipLocationChange: true,
    });
    sessionStorage.setItem('patient-page-status', 'worklist');
    sessionStorage.setItem(
      'containerSelectionalert',
      this.containerSelectionpanel
    );

    sessionStorage.setItem('SelectedMenu', this.CurrentMenu);
    sessionStorage.setItem('PageIndex', this.pageIndex);
    sessionStorage.setItem('alertPriority', this.priorityFilterValue!);
    sessionStorage.setItem('alertStatus', this.statusFilterValue!);
    sessionStorage.setItem('SelectedFilterAlert', this.tableFilterValuealert);
    sessionStorage.setItem('alertSearchValeName', this.alertSearchValeName);
    sessionStorage.setItem('alertAssignedName', this.alertAssignedName);
  }
  tableFilterValuetask: any;
  tableFilterValuealert: any;
  navigatePatientdetailfromTask(row: any) {
    let route = '/admin/patients_detail';
    this._router.navigate([route], {
      queryParams: { id: row.PatientId, programId: row.PatientProgramId },
      skipLocationChange: true,
    });
    sessionStorage.setItem('patient-page-status', 'worklist');
    sessionStorage.setItem(
      'containerSelectiontask',
      this.taskcontainerSelection
    );
    sessionStorage.setItem('startdate', this.rangeTask.controls.start.value);
    sessionStorage.setItem('enddate', this.rangeTask.controls.end.value);
    sessionStorage.setItem('SelectedMenu', this.CurrentMenu);
    sessionStorage.setItem('PageIndex', this.pageIndex);
    sessionStorage.setItem('taskPriority', this.priorityTaskFilterValue);
    sessionStorage.setItem('taskStatus', this.statusFilterTaskValue);
    sessionStorage.setItem('SelectedFilter', this.tableFilterValuetask);
    sessionStorage.setItem('searchPatientListTask', this.searchTaskValueName);
    sessionStorage.setItem('searchAssigneeTask', this.searchCreated);
  }

  rangeScheduleCalc() {
    var range_start_date = new Date(this.range.controls.start.value);
    range_start_date = this.convertDate(range_start_date);

    var range_end_date = new Date(this.range.controls.end.value);
    range_end_date.setDate(range_end_date.getDate());

    range_end_date = this.convertDate(range_end_date);
    var startDate;
    var endDate;
    startDate = range_start_date + 'T00:00:00';
    //startDate = this.auth.ConvertToUTCRangeInput(new Date(startDate));
    //endDate = range_end_date + 'T23:59:59';
    endDate = range_end_date + 'T00:00:00';
    //endDate = this.auth.ConvertToUTCRangeInput(new Date(endDate));

    var range_start_date = new Date(this.range.controls.start.value);
    range_start_date = this.convertDate(range_start_date);

    var range_end_date = new Date(this.range.controls.end.value);
    range_end_date.setDate(range_end_date.getDate() + 1);
    range_end_date = this.convertDate(range_end_date);

    if (
      range_start_date != null &&
      range_end_date != null &&
      range_start_date <= range_end_date
    ) {
      //&& ((dateRangeStart.value) < (dateRangeEnd.value))
      this.rpm
        .rpm_get(
          `/api/schedules/getworklistschedules?StartDate=${startDate}&EndDate=${endDate}`
        )
        .then((data) => {
          this.ScheduleDataSource = data;
        });
    }
  }

  scheduleSelected = Array();
  filteredItem: any;
  scheduleComplete(event: any, id: any) {
    if (event.target.checked == true) {
      this.scheduleSelected.push(id);
    } else {
      var a = this.scheduleSelected.indexOf(id);

      this.scheduleSelected.splice(a, 1);
    }
  }

  saveCompleteTask() {
    var req_body: any = {};
    req_body['IDs'] = this.scheduleSelected;
    this.rpmservice
      .rpm_post('/api/schedules/updatecompletedschedule', req_body)
      .then(
        (data) => {
          alert('Schedule Status Changed Successfully!!');
          this.getTaskScheduleList();
          this.rightsidebar.calculateUpcomingSchedule();
        },
        (err) => {
          //show error patient id creation failed
          alert('Something Went Wrong ');
        }
      );
  }

  // ////////////////////////////////// *Pie- Chart * /////////////////////////////////////////////////////////////

  public doughnutChartLabelsOne: any = [
    'Completed in Time -' + this.completed_time_task,
    'Completed SLA Breach - ' + this.completed_sla_task,
    'Open Tasks -' + this.open_task,
  ];

  public doughnutChartPluginsOne: any[] =
    [
      {
        beforeDraw(chart: any) {
          const ctx = chart.ctx;
          const txt = 'Center Text';

          //Get options from the center object in options
          const sidePadding = 60;
          const sidePaddingCalculated =
            (sidePadding / 100) * (chart.innerRadius * 2);

          ctx.textAlign = 'center';
          ctx.textBaseline = 'middle';
          const centerX = (chart.chartArea.left + chart.chartArea.right) / 2;
          const centerY = (chart.chartArea.top + chart.chartArea.bottom) / 2;

          //Get the width of the string and also the width of the element minus 10 to give it 5px side padding
          const stringWidth = ctx.measureText(txt).width;
          const elementWidth = chart.innerRadius * 2 - sidePaddingCalculated;

          // Find out how much the font can grow in width.
          const widthRatio = elementWidth / stringWidth;
          const newFontSize = Math.floor(10 * widthRatio);
          const elementHeight = chart.innerRadius * 2;

          // Pick a new font size so it will not be larger than the height of label.
          const fontSizeToUse = Math.min(newFontSize, elementHeight);

          ctx.font = fontSizeToUse + 'px Arial';
          ctx.fillStyle = 'black';

          // Draw text in center
          ctx.fillText(chart.options.centerAlert, centerX, centerY);
        },
      },
    ];

  public doughnutChartLabels: any = [
    'Completed in Time' + this.completed_alert_task,
    'Completed SLA Breach' + this.completed_alert_sla,
    'Open Tasks' + this.open_alert,
  ];
  public doughnutChartPlugins: any[] = [
    {
      beforeDraw(chart: any) {
        const ctx = chart.ctx;
        const txt = 'Center Text';

        //Get options from the center object in options
        const sidePadding = 60;
        const sidePaddingCalculated =
          (sidePadding / 100) * (chart.innerRadius * 2);

        ctx.textAlign = 'center';
        ctx.textBaseline = 'middle';
        const centerX = (chart.chartArea.left + chart.chartArea.right) / 2;
        const centerY = (chart.chartArea.top + chart.chartArea.bottom) / 2;

        //Get the width of the string and also the width of the element minus 10 to give it 5px side padding
        const stringWidth = ctx.measureText(txt).width;
        const elementWidth = chart.innerRadius * 2 - sidePaddingCalculated;

        // Find out how much the font can grow in width.
        const widthRatio = elementWidth / stringWidth;
        const newFontSize = Math.floor(10 * widthRatio);
        const elementHeight = chart.innerRadius * 2;

        // Pick a new font size so it will not be larger than the height of label.
        const fontSizeToUse = Math.min(newFontSize, elementHeight);

        ctx.font = fontSizeToUse + 'px Arial';
        ctx.fillStyle = 'black';

        // Draw text in center
        ctx.fillText(chart.options.centerText, centerX, centerY);
      },
    },
  ];

  value: any = 15;
  public Alert_KPI: any;
  public Tasks_KPI: any;
  public donutColors = [
    {
      backgroundColor: [
        'rgba(109,168,111, 1)',
        'rgba(253,82,118, 1)',
        'rgba(255,191,119, 1)',
      ],
    },
  ];

  public doughnutChartType: ChartType = 'doughnut';
  public doughnutChartOptions: any = {
    cutoutPercentage: 80,
    responsive: true,
    centerText: '',
    legend: {
      labels: {
        usePointStyle: true, //<-- set this
      },
      display: true,
      position: 'bottom',
    },
  };

  public doughnutChartOptionsOne: any = {
    cutoutPercentage: 80,
    responsive: true,
    centerAlert: '',
    legend: {
      labels: {
        usePointStyle: true, //<-- set this
      },
      display: true,
      position: 'bottom',
    },
  };

  convertToLocalTime(stillUtc: any) {
    stillUtc = stillUtc + 'Z';
    var local = moment(stillUtc).local().format('YYYY-MM-DD HH:mm:ss');
    return local;
  }
  dueDateStatus: any;
  dueDatePass(date1: any, status: any) {
    this.today = new Date();
    var dateOne = new Date(this.convertToLocalTime(date1)).setHours(0, 0, 0, 0);
    var dateTwo = this.today.setHours(0, 0, 0, 0);
    if (dateOne < dateTwo && status != 'Complete') {
      this.dueDateStatus = true;
    } else {
      this.dueDateStatus = false;
    }
    return this.dueDateStatus;
  }

  rangeTaskCalc() {}
  dataSrcTaskfrompatientDetail: any;
  getReloadDataTask() {
    if (this.patientNavigationStatus != 'true') {
      this.statusFilterTaskValue = 'status';
      this.priorityTaskFilterValue = 1;
    }

    this.searchTaskValueName = false;
    this.searchCreated = false;

    var that = this;
    var today = new Date();

    var ThirtyDaysAgo = new Date(today).setDate(today.getDate() + 30);

    if (
      this.rangeTask.controls.start.value != null &&
      this.rangeTask.controls.end.value != null
    ) {
      this.startDate = new Date(this.rangeTask.controls.start.value);
      this.endDate = new Date(this.rangeTask.controls.end.value);
    } else if (
      this.rangeTask.controls.start.value == null &&
      this.rangeTask.controls.end.value == null
    ) {
      // this.startDate = ThirtyDaysAgo;
      // this.endDate = today;
      this.startDate = today;
      this.endDate = ThirtyDaysAgo;
    } else {
      return;
    }
    this.loading2 = true;

    this.startDate = this.convertDate(this.startDate);
    this.endDate = this.convertDate(this.endDate);
    this.startDate = this.startDate + 'T00:00:00';
    this.endDate = this.endDate + 'T23:59:59';
    this.startDate = this.auth.ConvertToUTCRangeInput(new Date(this.startDate));
    this.endDate = this.auth.ConvertToUTCRangeInput(new Date(this.endDate));
    if (this.rangeSelectionEnable) {
      this.rangeTask.controls['start'].setValue(this.startDate);
      this.rangeTask.controls['end'].setValue(this.endDate);
    }

    this.rpm
      .rpm_get(
        `/api/tasks/worklistgettasks?StartDate=${this.startDate}&EndDate=${this.endDate}`
      )
      .then((data) => {
        this.httpGetAllTask = data;
        this.temp_dataSource = this.httpGetAllTask.Details;

        this.temp_dataSource = this.temp_dataSource.filter(function (
          data: any
        ) {
          return data.Status != 'Complete';
        });

        this.taskSlaBreached = this.httpGetAllTask.Details.filter(function (
          data: any
        ) {
          return data.SLABreached == 1;
        });

        this.taskComplete = this.httpGetAllTask.Details.filter(function (
          data: any
        ) {
          // this.taskComplete = this.temp_dataSource.filter(function(data:any) {

          return data.Status == 'Complete';
        });
        this.today = new Date();
        this.taskDueToday = this.temp_dataSource.filter((data: any) => {
          return (
            new Date(this.convertToLocalTime(data.DueDate)).setHours(
              0,
              0,
              0,
              0
            ) <= this.today.setHours(0, 0, 0, 0) && data.Status != 'Complete'
          );
        });

        var that = this;
        this.taskPendingLater = this.temp_dataSource.filter((data: any) => {
          return (
            new Date(this.convertToLocalTime(data.DueDate)).setHours(
              0,
              0,
              0,
              0
            ) > that.today.setHours(0, 0, 0, 0) && data.Status != 'Complete'
          );
        });
        this.taskwatcherRole = this.temp_dataSource.filter(function (
          data: any
        ) {
          return data.SLABreached == 1;
        });
        this.loading2 = false;
        this.rangeSelectionEnable = false;
        var dataSrc;

        switch (this.taskcontainerSelection) {
          case 0:
            dataSrc = this.taskDueToday;

            break;
          case 1:
            dataSrc = this.taskDueToday;

            break;
          case 2:
            dataSrc = this.taskSlaBreached;

            break;

          case 3:
            dataSrc = this.taskPendingLater;

            break;

          case 4:
            dataSrc = this.taskComplete;

            break;
        }
        this.dataSrcTaskfrompatientDetail = dataSrc;
        this.tableDataSource = new MatTableDataSource(dataSrc);
        this.changeDetectorRef.detectChanges();
        this.tableDataSource.paginator = this.paginator;
        this.tableDataSource.sort = this.sort;
        this.total_number = this.tableDataSource.filteredData.length;
      });
  }

  navigationPageTaskReload() {
    // this.searchTaskValueName = false;
    // this.searchCreated = false;

    var that = this;
    var today = new Date();

    var ThirtyDaysAgo = new Date(today).setDate(today.getDate() + 30);

    if (
      this.rangeTask.controls.start.value != null &&
      this.rangeTask.controls.end.value != null
    ) {
      this.startDate = new Date(this.rangeTask.controls.start.value);
      this.endDate = new Date(this.rangeTask.controls.end.value);
    } else if (
      this.rangeTask.controls.start.value == null &&
      this.rangeTask.controls.end.value == null
    ) {
      this.startDate = today;
      this.endDate = ThirtyDaysAgo;
    } else {
      return;
    }
    this.loading2 = true;

    this.startDate = this.convertDate(this.startDate);
    this.endDate = this.convertDate(this.endDate);
    this.startDate = this.startDate + 'T00:00:00';
    this.endDate = this.endDate + 'T23:59:59';
    this.startDate = this.auth.ConvertToUTCRangeInput(new Date(this.startDate));
    this.endDate = this.auth.ConvertToUTCRangeInput(new Date(this.endDate));
    if (this.rangeSelectionEnable) {
      this.rangeTask.controls['start'].setValue(this.startDate);
      this.rangeTask.controls['end'].setValue(this.endDate);
    }

    this.rpm
      .rpm_get(
        `/api/tasks/worklistgettasks?StartDate=${this.startDate}&EndDate=${this.endDate}`
      )
      .then((data) => {
        this.httpGetAllTask = data;
        this.temp_dataSource = this.httpGetAllTask.Details;

        this.temp_dataSource = this.temp_dataSource.filter(function (
          data: any
        ) {
          return data.Status != 'Complete';
        });

        this.taskSlaBreached = this.httpGetAllTask.Details.filter(function (
          data: any
        ) {
          return data.SLABreached == 1;
        });

        this.taskComplete = this.httpGetAllTask.Details.filter(function (
          data: any
        ) {
          return data.Status == 'Complete';
        });
        this.today = new Date();
        this.taskDueToday = this.temp_dataSource.filter((data: any) => {
          return (
            new Date(this.convertToLocalTime(data.DueDate)).setHours(
              0,
              0,
              0,
              0
            ) <= this.today.setHours(0, 0, 0, 0) && data.Status != 'Complete'
          );
        });

        var that = this;
        this.taskPendingLater = this.temp_dataSource.filter((data: any) => {
          return (
            new Date(this.convertToLocalTime(data.DueDate)).setHours(
              0,
              0,
              0,
              0
            ) > that.today.setHours(0, 0, 0, 0) && data.Status != 'Complete'
          );
        });
        this.taskwatcherRole = this.temp_dataSource.filter(function (
          data: any
        ) {
          return data.SLABreached == 1;
        });
        this.loading2 = false;
        this.rangeSelectionEnable = false;
        var dataSrc;

        switch (this.taskcontainerSelection) {
          case 0:
            dataSrc = this.taskDueToday;

            break;
          case 1:
            dataSrc = this.taskDueToday;

            break;
          case 2:
            dataSrc = this.taskSlaBreached;

            break;

          case 3:
            dataSrc = this.taskPendingLater;

            break;

          case 4:
            dataSrc = this.taskComplete;

            break;
        }

        this.dataSrcTaskfrompatientDetail = dataSrc;
        let dataPriorityfilter: string;
        let dataStatusFilter: string;
        var dataSourceTaskFilter;
        if (this.priorityTaskFilterValue == 3) {
          dataPriorityfilter = 'High';
        } else if (this.priorityTaskFilterValue == 4) {
          dataPriorityfilter = 'Medium';
        } else if (this.priorityTaskFilterValue == 5) {
          dataPriorityfilter = 'Low';
        } else if (
          this.priorityTaskFilterValue == 1 ||
          this.priorityTaskFilterValue == 0
        ) {
          dataSourceTaskFilter = this.dataSrcTaskfrompatientDetail;
        }

        if (this.statusFilterTaskValue == 'todo') {
          dataStatusFilter = 'ToDo';
        } else if (this.statusFilterTaskValue == 'inprogress') {
          dataStatusFilter = 'InProgress';
        } else if (this.statusFilterTaskValue == 'complete') {
          dataStatusFilter = 'Complete';
        } else if (
          this.statusFilterTaskValue == 'status' ||
          this.statusFilterTaskValue == null
        ) {
          dataSourceTaskFilter = this.dataSrcTaskfrompatientDetail;
        }

        if (
          this.statusFilterTaskValue != 'status' &&
          this.statusFilterTaskValue != null
        ) {
          dataSourceTaskFilter = this.dataSrcTaskfrompatientDetail.filter(
            function (data: any) {
              return data.Status == dataStatusFilter;
            }
          );
        }

        if (
          this.priorityTaskFilterValue != 1 &&
          this.priorityTaskFilterValue != 0
        ) {
          dataSourceTaskFilter = dataSourceTaskFilter.filter(function (
            data: any
          ) {
            return data.Priority == dataPriorityfilter;
          });
        }

        this.tableDataSource = new MatTableDataSource(dataSourceTaskFilter);
        if (this.patientNavigationStatus == 'true') {
          this.redirectionFilter(this.tableFilterValuetask);
        }
        this.changeDetectorRef.detectChanges();
        this.tableDataSource.paginator = this.paginator;
        this.tableDataSource.sort = this.sort;
        this.total_number = this.tableDataSource.filteredData.length;
        sessionStorage.removeItem('patientNavigationStatus');
        sessionStorage.removeItem('containerSelectionalert');
        sessionStorage.removeItem('containerSelectiontask');
        sessionStorage.removeItem('SelectedMenu');
        sessionStorage.removeItem('taskPriority');

        sessionStorage.removeItem('alertPriority');
        sessionStorage.removeItem('SelectedFilter');

        sessionStorage.removeItem('taskStatus');
        sessionStorage.removeItem('statusFilterValue');
      });
  }

  dataSrcAlertfrompatientDetail: any;
  getReloadDataAlert() {
    this.loading = true;
    this.rpm
      .rpm_get('/api/alerts/worklistgetalerts?RoleId=' + this.roleId[0].Id)
      .then((data) => {
        this.httpGetAlertData = data;

        this.temp_dataSource = this.httpGetAlertData.Details;
        this.temp_dataSource = this.temp_dataSource.filter(function (
          data: any
        ) {
          return data.Status != 'Complete';
        });

        // this.total_number = this.httpGetAlertData.Summary[0].TotalPatients;
        var d = new Date();

        this.critical_alertData = this.temp_dataSource.filter(function (
          data: any
        ) {
          return data.Prioriy == 'Critical';
        });

        this.slaBreachData = this.httpGetAlertData.Details.filter(
          (data: any) => {
            return data.SLABreached == 1;
          }
        );
        this.openAlertData = this.temp_dataSource.filter(function (data: any) {
          return data.Status != 'Complete';
        });
        this.completeStatusData = this.httpGetAlertData.Details.filter(
          (data: any) => {
            return data.Status == 'Complete';
          }
        );

        this.AlertTypeArray = this.httpGetAlertData.AlertTypes;
        this.loading = false;
        var tablesrc;
        switch (this.containerSelectionpanel) {
          case 0:
            tablesrc = this.openAlertData;
            break;
          case 1:
            tablesrc = this.openAlertData;
            break;
          case 2:
            tablesrc = this.critical_alertData;
            break;

          case 3:
            tablesrc = this.slaBreachData;
            break;

          case 4:
            tablesrc = this.openAlertData;
            break;

          case 5:
            tablesrc = this.completeStatusData;
            break;
        }
        this.dataSrcAlertfrompatientDetail = tablesrc;
        this.tableDataSource = new MatTableDataSource(tablesrc);
        this.changeDetectorRef.detectChanges();
        this.tableDataSource.paginator = this.paginator;
        this.tableDataSource.sort = this.sort;
        this.total_number = this.tableDataSource.filteredData.length;
      });
  }
  navigationPageAlertReload() {
    this.loading = true;
    this.rpm
      .rpm_get('/api/alerts/worklistgetalerts?RoleId=' + this.roleId[0].Id)
      .then((data) => {
        this.httpGetAlertData = data;

        this.temp_dataSource = this.httpGetAlertData.Details;
        this.temp_dataSource = this.temp_dataSource.filter(function (
          data: any
        ) {
          return data.Status != 'Complete';
        });

        // this.total_number = this.httpGetAlertData.Summary[0].TotalPatients;
        var d = new Date();

        this.critical_alertData = this.temp_dataSource.filter(function (
          data: any
        ) {
          return data.Prioriy == 'Critical';
        });

        this.slaBreachData = this.httpGetAlertData.Details.filter(
          (data: any) => {
            return data.SLABreached == 1;
          }
        );
        this.openAlertData = this.temp_dataSource.filter(function (data: any) {
          return data.Status != 'Complete';
        });
        this.completeStatusData = this.httpGetAlertData.Details.filter(
          (data: any) => {
            return data.Status == 'Complete';
          }
        );

        this.AlertTypeArray = this.httpGetAlertData.AlertTypes;
        this.loading = false;
        var tablesrc;
        switch (this.containerSelectionpanel) {
          case 0:
            tablesrc = this.openAlertData;
            break;
          case 1:
            tablesrc = this.openAlertData;
            break;
          case 2:
            tablesrc = this.critical_alertData;
            break;

          case 3:
            tablesrc = this.slaBreachData;
            break;

          case 4:
            tablesrc = this.openAlertData;
            break;

          case 5:
            tablesrc = this.completeStatusData;
            break;
        }
        this.dataSrcAlertfrompatientDetail = tablesrc;
        let dataSourceAlertFilter;
        // this.alertSearchValeName = false;
        // this.alertAssignedName = false;

        let dataPriorityfilter: string;
        let dataStatusFilter: string;
        if (this.priorityFilterValue == 'Critical') {
          dataPriorityfilter = 'Critical';
        } else if (this.priorityFilterValue == 'Cautious') {
          dataPriorityfilter = 'Cautious';
        } else if (this.priorityFilterValue == 'Missing') {
          dataPriorityfilter = 'Missing';
        } else if (this.priorityFilterValue == 'Normal') {
          dataPriorityfilter = 'Normal';
        } else if (
          this.priorityFilterValue == undefined ||
          this.priorityFilterValue == null
        ) {
          dataSourceAlertFilter = this.dataSrcAlertfrompatientDetail;
        }
        if (this.statusFilterValue == 'todo') {
          dataStatusFilter = 'ToDo';
        } else if (this.statusFilterValue == 'inprogress') {
          dataStatusFilter = 'InProgress';
        } else if (this.statusFilterValue == 'complete') {
          dataStatusFilter = 'Complete';
        } else if (
          this.statusFilterValue == 'status' ||
          this.statusFilterValue == null
        ) {
          dataSourceAlertFilter = this.dataSrcAlertfrompatientDetail;
        }

        if (
          this.statusFilterValue != 'status' &&
          this.statusFilterValue != null
        ) {
          dataSourceAlertFilter = this.dataSrcAlertfrompatientDetail.filter(
            function (data: any) {
              return data.Status == dataStatusFilter;
            }
          );
        }

        if (
          this.priorityFilterValue != undefined &&
          this.priorityFilterValue != null
        ) {
          dataSourceAlertFilter = dataSourceAlertFilter.filter(function (
            data: any
          ) {
            return data.Prioriy == dataPriorityfilter;
          });
        }

        this.dataSrcAlertfrompatientDetail = tablesrc;
        this.tableDataSource = new MatTableDataSource(dataSourceAlertFilter);
        if (this.patientNavigationStatus == 'true') {
          this.redirectionAlertFilter(this.tableFilterValuealert);
        }
        this.total_number = this.tableDataSource.filteredData.length;
        this.changeDetectorRef.detectChanges();
        this.tableDataSource.paginator = this.paginator;
        this.tableDataSource.sort = this.sort;
        sessionStorage.removeItem('patientNavigationStatus');
        sessionStorage.removeItem('containerSelectionalert');
        sessionStorage.removeItem('containerSelectiontask');
        sessionStorage.removeItem('SelectedMenu');
        sessionStorage.removeItem('taskPriority');
        sessionStorage.removeItem('alertPriority');
        sessionStorage.removeItem('taskStatus');
        sessionStorage.removeItem('statusFilterValue');
        sessionStorage.removeItem('SelectedFilterAlert');
        sessionStorage.removeItem('alertSearchValeName');
      });
  }
  ngOnDestroy(): void {
    sessionStorage.removeItem('patientNavigationStatus');
  }
  pageSize: any;
  pageIndex: any;
  pageChange(pageEvent: PageEvent) {
    if (pageEvent.pageIndex > pageEvent.previousPageIndex!)
      this.pageSize = pageEvent.pageSize;
    this.pageIndex = pageEvent.pageIndex;
  }
  redirectionFilter(value: any) {
    if (value == 'undefined') {
      value = '';
    }
    if (this.tableFilterValuetask != 'undefined') {
      this.tableDataSource.filter = value.trim().toLowerCase();
      if (this.searchTaskValueName == true) {
        this.setupFilter('Patient');
        this.taskpatientSearch = value;
      } else if (this.searchCreated == true) {
        this.setupFilter('CreatedBy');
        this.taskAssigneeSearch = value;
      }
    } else {
      this.taskpatientSearch = value;
      this.taskAssigneeSearch = value;

      return;
    }
  }
  alertAssigneeSearch: any;
  alertPatientSearch: any;
  redirectionAlertFilter(value: any) {
    if (value == 'undefined') {
      value = '';
    }
    if (this.tableFilterValuealert != 'undefined') {
      this.tableDataSource.filter = value.trim().toLowerCase();
      if (this.alertSearchValeName == true) {
        this.setupFilter('PatientName');
        this.alertPatientSearch = value;
      } else if (this.alertAssignedName == true) {
        this.setupFilter('AssignedMember');
        this.alertAssigneeSearch = value;
      }
    } else {
      this.alertPatientSearch = value;
      this.alertAssigneeSearch = value;

      return;
    }
  }

  // SMS Data
  message: any;
  SMS_Header = [
    'PatientId',
    'PatientProgramID',
    'PatientName',
    'ProgramName',
    'FromNumber',
    'Senddate',
    'SendTime',
    'Message',
    'action',
  ];
  getSMSData(element: any, templateRef: TemplateRef<any>) {
    this.message = element.Message;
    this.dialog.open(templateRef);
  }
  httpGetPatientSMSData: any;
  getPatientSMS() {
    this.loading = true;
    this.pageIndex = 0;
    this.rpm
      .rpm_get('/api/patient/getallpatientsSmslist?RoleId=' + this.roleId[0].Id)
      .then((data) => {
        this.httpGetPatientSMSData = data;
        this.temp_dataSource = this.httpGetPatientSMSData;
        this.tableDataSource = new MatTableDataSource(this.temp_dataSource);
        this.tableDataSource.paginator = this.paginator;
        this.tableDataSource.sort = this.sort;
        this.total_number = this.tableDataSource.filteredData.length;
        this.loading = false;
      });
  }
  convertISOFormat(date: any) {
    var isoDateString = new Date(date).toISOString().replace('Z', '');
    return isoDateString;
  }
  searchPatietNameSmsClose() {
    this.alertSearchValeName = false;
    this.getPatientSMS();
  }
  GetDateRange(event: any) {
    console.log('Current date range');
    console.log(event);
  }
}
