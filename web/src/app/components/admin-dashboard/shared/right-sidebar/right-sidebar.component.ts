import { MasterDataService } from './../../sevices/master-data.service';
import { TaskComponent } from './../../components/task/task.component';
import {
  Component,
  OnInit,
  Input,
  Output,
  EventEmitter,
  ViewChild,
} from '@angular/core';
import { UntypedFormGroup, UntypedFormControl, Validators } from '@angular/forms';
import {
  filter,
  map,
  startWith,
} from 'rxjs/operators';
import { RPMService } from '../../sevices/rpm.service';
import { Router } from '@angular/router';
import { MatDialog, MatDialogConfig } from '@angular/material/dialog';
import { StatusMessageComponent } from '../../shared/status-message/status-message.component';
import moment from 'moment';
import { DatePipe } from '@angular/common';
import { from } from 'rxjs';
import { ConfirmDialogServiceService } from '../confirm-dialog-panel/service/confirm-dialog-service.service';

@Component({
  selector: 'app-right-sidebar',
  templateUrl: './right-sidebar.component.html',
  styleUrls: ['./right-sidebar.component.scss'],

})
export class RightSidebarComponent implements OnInit {
  @Input() sidepaneldisplay: any;
  @ViewChild(TaskComponent) private taskcomponent: TaskComponent;
  @ViewChild('auto') auto: any;
  loading: any;
  mouseOvered = false;
  http_listener: any;
  dataSource: any = [];
  myForm: any;
  rolelist: any;
  roles: any;
  pubsuburl: any;
  alertId: any;
  http_unassigned: any;
  http_alertDatabyId: any;
  roleData: any;

  critical_alert_var = false;
  isOpen = false;
  menu_choice = 1;
  display_tool_bar = true;
  menuTitle: any;
  myControl = new UntypedFormControl();
  @Input() variable_close: any;
  http_getPatientList: any;
  options: any;
  filteredOptions: any;
  userarr: string[];
  accessRights: any;
  taskTypeIdArray: any;
  http_getMasterData: any;
  taskStatusIdArray: any;
  filterOptionUser: any;
  http_TaskMasterData: any;
  http_taskPatientListArray: any;
  http_taskAssigneeList: any;
  http_AssigneeListSchedule: any;

  @Input() task_update_variable: any;

  http_getSchedulemasterdata: any;
  http_scheduleAssigneeList: any;
  scheduleUserList: any;
  frequencyValue: any;
  ScheduleTypeIdArray: any;
  filterOptionSchedule: any;
  SelectedUserData: any;
  SelectUserId: any;


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
  StatesAndCities: any;
  cities: any;
  statesArray: any;

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
  LatestSchedule: any;
  UpcomingSchedule: any;
  taskUserList: any;
  filterOptionAssigneeTask: any;
  managerList: any;
  http_manager: any;

  todaty_date: any;
  unassigned_members: any;
  http_CareTeamList: any;

  // Schedule Template Ends
  onAddData() {
    this.dataSource.push(this.dataSource.length);
  }

  getMenuChioce(choice: any) {
    switch (choice) {
      case 2:
        this.menuTitle = 'Add Notes';
        break;
      case 3:
        this.durationValue = 0;
        this.menuTitle = 'Add Schedule';
        this.refreshScheduleAssign();
        break;
      case 4:
        this.registerTask.reset();
        this.refreshTaskAssign();
        // this.menuTitle ='Add Task';

        break;
      case 5:
        this.menuTitle = 'Add Symptoms';
        break;
      case 6:
        this.menuTitle = 'Add Medication';
        break;
      case 7:
        this.menuTitle = 'Add Reports';
        break;
    }
    this.verificationStatus = false;

    this.menu_choice = choice;
    if (choice == 1) {
      this.display_tool_bar = true;
    } else {
      this.display_tool_bar = false;
    }
    this.loadTaskMasterData();
  }
  onClickCancel() {
    this.registerTask.reset();
    this.SelectId = undefined;
    this.menuChoice = 1;
    this.getMenuChioce(1);
    this.isOpen = false;
    this.refreshScheduleAssign();
    this.refreshTaskAssign();

    this.registerSchedule.reset();
    this.registerSchedule.controls.assigneeName.setValue(this.userName);
    this.update_schedule_variable = false;
    this.worklist_edit = false;
    this.registerTeamForm.reset();
    this.teamMemberId = [];
    this.new_member = null;
    this.tmp_member = [];

    this.http_unassigned = sessionStorage.getItem('unassigned_members');
    this.http_unassigned = JSON.parse(this.http_unassigned);
    this.checkedSingle = true;
    this.checkedSeries = false;
  }

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

  SelectedData: any;
  SelectId: any;
  ProgramId: any;
  selectedPatient(event: { option: { value: any } }) {
    this.SelectedData = event.option.value;
    this.SelectId = this.SelectedData.PatientId;
    this.ProgramId = this.SelectedData.PatientProgramId;
  }

  alertPriorityData: any;
  alertsArray: any;
  scheduleData: any;
  filteredTeamOptions: any;
  selectedValue: any;
  http_unassignedMember: any;
  rightSidebarloader: any;
  ngOnInit(): void {
    var that = this;
    that.rolelist = sessionStorage.getItem('Roles');
    that.roles = JSON.parse(this.rolelist);
    that.rpmservice
      .rpm_get('/api/home/getdashboardalerts?RoleId=' + this.roles[0].Id)
      .then((data) => {
        that.alertPriorityData = data;

        if (that.alertPriorityData.length == 0) {
          that.alertPriorityData = [];
        }
        window.scroll(0, 0);
        this.getAlertData(true);
      });

    this.todaty_date = this.datepipe.transform(new Date(), 'yyyy-MM-dd');
    this.filteredTeamOptions = this.myControl.valueChanges.pipe(
      startWith(''),
      map((val) => this.filterTeam(val))
    );
    this.GetPriorityAlerts();

    this.scheduleData = [];
    this.calculateUpcomingSchedule();
    var that = this;
    that.rpmservice
      .rpm_get('/api/authorization/operationalmasterdata')
      .then((data) => {
        that.http_getMasterData = data;
        this.masterReload.emit();
        sessionStorage.setItem(
          'operational_masterdata',
          JSON.stringify(that.http_getMasterData)
        );
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

        this.registerTask
          .get('task_status')
          ?.setValue(that.taskStatusIdArray[0].Name);
        this.selectedValue = that.taskStatusIdArray[0].Name;
      });
  }
  filterTeam(val: any) {
    return this.http_unassigned.filter((option: { Name: any }) =>
      option.Name.toString()
        .toLowerCase()
        .includes(val.toString().toLowerCase())
    );
  }

  filterUser(val: any) {
    return this.scheduleUserList.filter((option: { Name: any }) =>
      option.Name.toLowerCase().includes(val.toString().toLowerCase())
    );
  }

  getOptionUserText(option: { Name: any }) {
    return option.Name;
  }
  getOptionPatientText(option: { PatientName: any }) {
    return option.PatientName;
  }

  filterTasUser(val: any) {
    return this.scheduleUserList.filter((option: { Name: any }) =>
      option.Name.toLowerCase().includes(val.toString().toLowerCase())
    );
  }
  getOptionTaskUserText(option: { Name: any }) {
    return option.Name;
  }

  roleId: any;
  viewAlertform = new UntypedFormGroup({
    Name: new UntypedFormControl(null, [Validators.required]),
    alertType: new UntypedFormControl(null, [Validators.required]),
    alertdescription: new UntypedFormControl(null, [Validators.required]),
    dueDate: new UntypedFormControl(null, [Validators.required]),
    alertPriority: new UntypedFormControl(null, [Validators.required]),
    alertstatus: new UntypedFormControl(null, [Validators.required]),
    assignee_name: new UntypedFormControl(null, [Validators.required]),
    comments: new UntypedFormControl(null),
  });
  PatientOrContact: any;
  userName: any;
  userId: any;
  patient_list: any;
  filterPatient: any;
  filterOptionTask: any;
  filterTaskUserList: any;
  taskPatientListTemp: any;
  constructor(
    private rpmservice: RPMService,
    private _router: Router,
    public dialog: MatDialog,
    public datepipe: DatePipe,
    private confirmDialog: ConfirmDialogServiceService,
    private masterdataService: MasterDataService
  ) {
    this.initializeDefaults();
    this.initializeSessionStorage();
    this.initializeFormValueChanges();
    this.initializeScheduleSubscriptions();
    this.loadMasterData();
    this.loadScheduleData();
    this.loadTaskMasterData();
    this.loadCareTeamList();
    this.loadVendors();
  }
  private initializeFormValueChanges(): void {
    this.filterOptionSchedule = this.registerSchedule.get('userName')?.valueChanges.pipe(
      startWith(''),
      map(val => this.filterScheduleUser(val))
    );

    this.filterTaskUserList = this.registerTask.get('userName')?.valueChanges.pipe(
      startWith(''),
      map(val => this.filterTaskUser(val))
    );

    this.registerTeamForm.get('mangerName')?.valueChanges.subscribe(() => {
      // Future logic for team manager change
    });
  }
  private initializeScheduleSubscriptions(): void {
    const days = ['Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday', 'Sunday'];
    const scheduleFlags = ['Mon', 'Tue', 'Wed', 'Thur', 'Fri', 'Sat', 'Sun'];
    const subScheduleFlags = ['SubMon', 'SubTue', 'SubWed', 'SubThur', 'SubFri', 'SubSat', 'SubSun'];

    days.forEach((day, idx) => {
      this.registerSchedule.get(`schedule${day}`)?.valueChanges.subscribe(val => {
        (this as any)[`Schedule${scheduleFlags[idx]}`] = val === true;
      });
      this.registerSchedule.get(`scheduleSub${day}`)?.valueChanges.subscribe(val => {
        (this as any)[`Schedule${subScheduleFlags[idx]}`] = val === true;
      });
    });

    this.registerSchedule.get('frequency')?.valueChanges.subscribe(() => {
      scheduleFlags.concat(subScheduleFlags).forEach(flag => (this as any)[`Schedule${flag}`] = false);
    });
  }
  private loadMasterData(): void {
    this.getTeamData();

    this.rpmservice.rpm_get('/api/careteam/getunassignedmembers').then(data => {
      this.unassigned_members = data;
      sessionStorage.setItem('unassigned_members', JSON.stringify(this.unassigned_members));
      this.http_unassigned = this.unassigned_members;
    });

    this.rpmservice.rpm_get(`/api/authorization/useraccessrights?RoleId=${this.roleId[0].Id}`)
      .then(data => this.accessRights = data);

      this.rpmservice
      .rpm_get(`/api/users/getallusers?RoleId=${this.roleId[0].Id}`)
      .then((data) => {
        const response = data as {
          Details: { Role: string; IsActive: boolean }[];
        };

        this.http_manager = response;
        this.managerList = response.Details.filter(
          (x) => x.Role === 'CareTeamManager' && x.IsActive
        );
      })
      .catch((err) => {
        console.error('Failed to fetch users:', err);
      });

  }
  private loadScheduleData(): void {
    const roleId = this.roleId[0]?.Id;
    if (!roleId) return;

    this.masterdataService.getScheduleMasterData(roleId)
      .then(res => {
        this.http_getSchedulemasterdata = res.rawData;
        this.scheduleUserList = res.patientOrContacts;
        this.ScheduleTypeIdArray = res.scheduleTypes;
        this.http_AssigneeListSchedule = [];
      })
      .catch(err => {
        console.error('Error loading schedule data:', err);
      });
    }
  private loadTaskMasterData(): void {
    const roleId = this.roleId[0]?.Id;
    const userId = +this.userId;

    if (!roleId || !userId) return;

    this.masterdataService.getFilteredTaskAssignees(roleId, undefined, userId)
      .then((res) => {
        this.http_TaskMasterData = res.taskMasterData;
        this.http_taskPatientListArray = res.taskMasterData.PatientList;
        this.taskPatientListTemp = [...this.http_taskPatientListArray];

        this.taskPatientListTemp.forEach((x: any) => {
          x.searchFieldTask = `${x.PatientName}[${x.ProgramName}]`;
        });

        this.http_taskAssigneeList = res.filteredAssignees;
      })
      .catch((err) => {
        console.error('Error fetching task master data:', err);
      });
  }

  private loadCareTeamList(): void {
    this.rpmservice.rpm_get(`/api/careteam/team`).then(data => {
      this.http_CareTeamList = data;
    });
  }
  private loadVendors(): void {
    const statesData = JSON.parse(sessionStorage.getItem('states_cities') || '{}');
    if (statesData?.States) {
      this.statesArray = statesData.States;
      this.registerVendor.get('state')?.valueChanges.subscribe(stateId => {
        this.cities = statesData.Cities.filter(
          (c: { StateId: number }) => c.StateId === parseInt(stateId)
        );
      });
    }

    this.registerDevice.get('vendorNameDevice')?.valueChanges.subscribe(val => {
      this.vendorSelected = this.VendorList.filter(
        (v: { DeviceVendorId: any }) => v.DeviceVendorId === parseInt(val)
      );
      if (this.vendorSelected.length > 0) {
        this.registerDevice.controls['addressline1'].setValue(this.vendorSelected[0].Address);
      }
    });
  }

  private initializeDefaults(): void {
    this.update_schedule_variable = false;
    this.worklist_edit = false;
  }
  private initializeSessionStorage(): void {
    this.PatientOrContact = JSON.parse(sessionStorage.getItem('PatientOrContact') || '{}');
    this.userName = sessionStorage.getItem('user_name') || '';
    this.userId = sessionStorage.getItem('userid') || '';
    this.roles = sessionStorage.getItem('Roles') || '';
    this.roleId = JSON.parse(this.roles);
    this.registerSchedule.controls.assigneeName.setValue(this.userName);
    this.patient_list = JSON.parse(sessionStorage.getItem('Patient_List') || '[]');
    this.http_unassigned = JSON.parse(sessionStorage.getItem('unassigned_members') || '[]');

    this.patient_list.forEach((x: any) => {
      x.searchFieldPatient = `${x.PatientName}[${x.Program}]`;
    });
  }

  http_team_data: any;

  getTeamData() {
    this.rpmservice.rpm_get('/api/careteam/team').then((data) => {
      this.http_CareTeamList = data;
    });
  }

  redirectPatientPage() {
    if (this.patient_list && this.patient_list.length > 0) {
      var selected = this.patient_list.filter((data: { PatientId: any }) => {
        return data.PatientId == this.SelectId;
      });

      if (selected != null && selected != undefined && selected.length != 0) {
        this.ProgramId = selected[0].PatientProgramId;
        let route = '/admin/patients_detail';
        this._router.navigate([route], {
          queryParams: { id: this.SelectId, programId: this.ProgramId },
          skipLocationChange: true,
        });
      } else {
        alert('Please Select Patient');
      }
    } else {
      alert('No Active Patients Found');
    }
  }
  // Patient Detail Navigation ends
  @Output() AlertTablereload = new EventEmitter();
  alertAssigneeName: any;
  alertAckReload() {
    this.AlertTablereload.emit();
  }
  acknowledgealert(alert: any) {
    this.menu_choice = 8;
    this.alertId = alert.Id;
    this.display_tool_bar = false;
    this.rpmservice
      .rpm_get(`/api/alerts/worklistgetalertbyid?Id=${alert.Id}`)
      .then((data) => {
        this.http_alertDatabyId = data;

        this.alertAssigneeName = this.http_alertDatabyId.CareTeamId;

        this.http_taskAssigneeList = this.http_alertDatabyId.Members;

        this.viewAlertform.controls.Name.setValue(
          this.http_alertDatabyId.PatientName
        );
        this.viewAlertform.controls.alertType.setValue(
          this.http_alertDatabyId.AlertType
        );
        if (this.http_alertDatabyId.CreatedOn) {
          this.viewAlertform.controls.alertdescription.setValue(
            this.http_alertDatabyId.Description +
              this.convertToLocalTimedisplay(this.http_alertDatabyId.CreatedOn)
          );
        } else {
          this.viewAlertform.controls.alertdescription.setValue(
            this.http_alertDatabyId.Description
          );
        }

        var alertdate = this.http_alertDatabyId.DueDate;

        this.viewAlertform.controls.dueDate.setValue(
          this.convertToLocalTimedisplay(alertdate)
        );
        this.viewAlertform.controls.alertPriority.setValue(
          this.http_alertDatabyId.Priority
        );
        this.viewAlertform.controls.alertstatus.setValue(
          this.http_alertDatabyId.Status,
          { onlySelf: true }
        );
        this.viewAlertform.controls.comments.setValue(
          this.http_alertDatabyId.Comments
        );
        this.alert_careteam_name = this.http_alertDatabyId.AssignedMember;
        this.alert_careteam_id = this.http_alertDatabyId.AssignedMemberId;
      });
  }



  savealert() {
    this.rolelist = sessionStorage.getItem('Roles');
    this.roles = JSON.parse(this.rolelist);

    var req_body: any = {};
    var roleId = this.roles[0].Id;
    if (
      this.alert_careteam_id == null ||
      this.alert_careteam_id == undefined ||
      this.alert_careteam_id == 'undefined'
    ) {
      alert('Please Select Assigned Member');
      return;
    }
    req_body = {
      AlertId: this.alertId,
      RoleId: roleId,
      AlertStatus: this.viewAlertform.controls.alertstatus.value,
      UserId: parseInt(this.alert_careteam_id),
      Comments: this.viewAlertform.controls.comments.value,
    };

    this.rpmservice.rpm_post(`/api/alerts/savealertresponse`, req_body).then(
      (data) => {
       alert(`Alert Saved Successfully!! `);
        this.alertAckReload();
        this.menu_choice = 1;
        this.display_tool_bar = true;
        this.GetPriorityAlerts();
        if (this.taskcomponent) {
          this.taskcomponent.getAllAlert();
        }
      },
      (err) => {
        alert(`Something Went Wrong ${err}`);
        this.menu_choice = 1;
      }
    );
  }

  acknowledgeAlert = false;
  ontaskClickCancel() {}

  receiveItem($event: any) {
    console.log('Event ' + $event);
  }

  GetPriorityAlerts() {
    var that = this;
    that.rolelist = sessionStorage.getItem('Roles');
    that.roles = JSON.parse(this.rolelist);
    that.rpmservice
      .rpm_get('/api/home/getdashboardalerts?RoleId=' + this.roles[0].Id)
      .then((data) => {
        that.alertsArray = data;

        if (that.alertsArray.length == 0) {
          that.alertsArray = [

          ];
        }
      });
  }
  subject: any;
  msg: any;

  ngOnDestroy() {
    if (this.subject) {
      this.subject.unsubscribe();
      // this.subject.close();
    }
  }

  // Add Task Template Start

  userArray = [
    {
      Id: 1,
      username: 'AbcTest',
    },
    {
      Id: 2,
      username: 'defTest',
    },
    {
      Id: 3,
      username: 'ghiTest',
    },
  ];
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
      Name: 'Low',
    },

  ];
  registerTask = new UntypedFormGroup({
    tasktype: new UntypedFormControl(null, [Validators.required]),
    description: new UntypedFormControl(null),
    duedate: new UntypedFormControl(null, [Validators.required]),
    priority: new UntypedFormControl(null, [Validators.required]),
    task_status: new UntypedFormControl(null, [Validators.required]),
    task_comment: new UntypedFormControl(null),
  });

  AddTask() {
    if (this.task_update_variable == true) {
      this.updateTask();
    } else if (this.task_update_variable == false) {
      var req_body: any = {};
      if (this.task_pid == undefined || this.task_pid == null) {
        alert('Please select a Patient.');
        return;
      }
      if (this.task_careteam_id == undefined || this.task_careteam_id == null) {
        alert('Please select a CareTeam Person.');
        return;
      }

      if (this.registerTask.valid) {

        req_body['PatientId'] = parseInt(this.task_pid);
        req_body['TaskTypeId'] = parseInt(
          this.registerTask.controls.tasktype.value
        );
        req_body['Name'] = this.registerTask.controls.description.value;
        req_body['DueDate'] = this.ConvertToUTCInput(
          new Date(this.registerTask.controls.duedate.value + 'T00:00:00')
        );
        req_body['PriorityId'] = this.registerTask.controls.priority.value;
        req_body['Status'] = this.registerTask.controls.task_status.value;

        req_body['CareteamMemberUserId'] = parseInt(this.task_careteam_id);

        req_body['Comments'] = this.registerTask.controls.task_comment.value;
        req_body['WatcherUserId'] = null;

        this.loading = true;

        this.rpmservice.rpm_post('/api/tasks/addtask', req_body).then(
          (data) => {
            alert('New Task Added Successfully!!');
            this.registerTask.reset();
            this.onClickCancel();
            this.taskReload();
            this.teamtaskReload();
            this.loading = false;
          },
          (err) => {
            //show error patient id creation failed
            alert('Error, could not add task..!, ' + err);
            this.loading = false;
          }
        );
      } else {
        alert('Please Complete the Form ');
        this.loading = false;
      }
    }
  }

  //  Add Task Template ends

  // Add Schedule Template Start
  // Add Schedule

  durationValue = 0;
  increment() {
    if (this.durationValue < 60) {
      this.durationValue++;
    } else {
      this.durationValue = 60;
    }
  }
  decrement() {
    if (this.durationValue > 0) {
      this.durationValue--;
    } else {
      this.durationValue = 0;
    }
  }
  userScheduleArray = [
    {
      Id: 1,
      username: 'AbcTest',
    },
    {
      Id: 2,
      username: 'defTest',
    },
    {
      Id: 3,
      username: 'ghiTest',
    },
  ];

  // Filter Schedule For Auto Fill Start
  filterScheduleUser(val: any) {
    return this.scheduleUserList.filter((option: { Name: any }) =>
      option.Name.toLowerCase().includes(val.toString().toLowerCase())
    );
  }
  filterTaskUser(val: any) {
    return this.http_taskPatientListArray.filter(
      (option: { PatientName: any }) =>
        option.PatientName.toLowerCase().includes(val.toString().toLowerCase())
    );
  }
  filterPatientUser(val: any) {
    return this.patient_list.filter((option: { PatientName: any }) =>
      option.PatientName.toLowerCase().includes(val.toString().toLowerCase())
    );
  }


  getOptionScheduleUserText(option: { Name: any }) {
    return option.Name;
  }
  getOptionTaskUserNameText(option: { PatientName: any }) {
    return option.PatientName;
  }
  getOptionTaskAssigneeText(option: { UserName: any }) {
    return option.UserName;
  }
  selectedUser(event: { option: { value: any } }) {
    this.SelectedUserData = event.option.value;
    this.SelectUserId = this.SelectedUserData.Id;
  }

  // Filter Schedule For Auto Fill End

  registerSchedule = new UntypedFormGroup({
    userName: new UntypedFormControl(null),
    scheduleType: new UntypedFormControl(null, [Validators.required]),
    scheduleDescription: new UntypedFormControl(null, [Validators.required]),
    startDate: new UntypedFormControl(null, [Validators.required]),
    endDate: new UntypedFormControl(null, [Validators.required]),
    startTime: new UntypedFormControl(null, [Validators.required]),
    frequency: new UntypedFormControl(null),
    assigneeName: new UntypedFormControl(null, [Validators.required]),
    scheduleMonday: new UntypedFormControl(null),
    scheduleTuesday: new UntypedFormControl(null),
    scheduleWednesday: new UntypedFormControl(null),
    scheduleThursday: new UntypedFormControl(null),
    scheduleFriday: new UntypedFormControl(null),
    scheduleSaturday: new UntypedFormControl(null),
    scheduleSunday: new UntypedFormControl(null),
    scheduleSubMonday: new UntypedFormControl(null),
    scheduleSubTuesday: new UntypedFormControl(null),
    scheduleSubWednesday: new UntypedFormControl(null),
    scheduleSubThursday: new UntypedFormControl(null),
    scheduleSubFriday: new UntypedFormControl(null),
    scheduleSubSaturday: new UntypedFormControl(null),
    scheduleSubSunday: new UntypedFormControl(null),
    weekSelectionFrequency: new UntypedFormControl(null),
  });

  weekSelectionValue: any;
  dayMonSelectionValue: any;
  dayTueSelectionValue: any;
  dayWedSelectionValue: any;
  dayThurSelectionValue: any;
  dayFriSelectionValue: any;
  daySatSelectionValue: any;
  daySunSelectionValue: any;

  AddSchedule(): void {


    if (this.update_schedule_variable) {
      this.editSchedule();
      return;
    }
    this.setDaySelections();
    if (!this.validateScheduleInputs()) {
      return;
    }

    const startDate = new Date(this.registerSchedule.controls.startDate.value);
    const endDate = new Date(this.registerSchedule.controls.endDate.value);

    if (endDate < startDate && endDate.getTime() !== startDate.getTime()) {
      return this.showWarning('Please select a Valid Start Date and End Date.');
    }

    if (
      this.registerSchedule.get('scheduleType')?.invalid &&
      this.registerSchedule.get('scheduleType')?.touched
    ) {
      return this.showWarning('Please select Schedule Type.');
    }

    // Step 3: Prepare request body
    if (this.registerSchedule.valid) {
      const req_body: any = {
        AssignedTo:  parseInt(this.schedule_careteam_id),
        ScheduleTypeId: parseInt(this.registerSchedule.controls.scheduleType.value),
        Schedule: this.registerSchedule.controls.frequency.value,
        Comments: this.registerSchedule.controls.scheduleDescription.value,
        StartDate: this.registerSchedule.controls.startDate.value,
        EndDate: this.registerSchedule.controls.endDate.value,
        StartTime: this.registerSchedule.controls.startTime.value,
        AssignedBy: parseInt(this.schedule_careteam_id),
        Mon: this.dayMonSelectionValue,
        Tue: this.dayTueSelectionValue,
        Wed: this.dayWedSelectionValue,
        Thu: this.dayThurSelectionValue,
        Fri: this.dayFriSelectionValue,
        Sat: this.daySatSelectionValue,
        Sun: this.daySunSelectionValue,
        WeekSelection: this.weekSelectionValue,
        Duration: this.durationValue,
      };

      this.loading = true;

      this.rpmservice.rpm_post('/api/schedules/addschedule', req_body).then(
        () => {
          this.confirmDialog.showConfirmDialog(
            'New Schedule Added Successfully!!!!',
            'Message',
            () => {
              this.resetScheduleForm();
            },
            false
          );
        },
        () => {
          this.showWarning('Sorry, Could Not Add Schedule..!');
          this.loading = false;
        }
      );
    } else {
      this.showWarning('Please Complete the form');
      this.loading = false;
    }
  }
  private resetScheduleForm()
  {
    this.registerSchedule.reset();
              this.onClickCancel();
              this.registerSchedule.controls.assigneeName.setValue(this.userName);
              this.scheduleReload();
              this.calculateUpcomingSchedule();
              this.loading = false;
              this.durationValue = 0;
  }
  private setDaySelections(): void {
    const freq = this.frequencyValue;

    if (freq === 'Daily') {
      this.weekSelectionValue = 0;
      this.dayMonSelectionValue = this.dayTueSelectionValue = this.dayWedSelectionValue =
      this.dayThurSelectionValue = this.dayFriSelectionValue = this.daySatSelectionValue =
      this.daySunSelectionValue = true;
    } else if (freq === 'Weekly') {
      this.weekSelectionValue = 0;
      this.dayMonSelectionValue = this.getDayValue('Monday');
      this.dayTueSelectionValue = this.getDayValue('Tuesday');
      this.dayWedSelectionValue = this.getDayValue('Wednesday');
      this.dayThurSelectionValue = this.getDayValue('Thursday');
      this.dayFriSelectionValue = this.getDayValue('Friday');
      this.daySatSelectionValue = this.getDayValue('Saturday');
      this.daySunSelectionValue = this.getDayValue('Sunday');
    } else if (freq === 'Monthly') {
      this.weekSelectionValue = this.registerSchedule.controls.weekSelectionFrequency.value;
      this.dayMonSelectionValue = this.getSubDayValue('Monday');
      this.dayTueSelectionValue = this.getSubDayValue('Tuesday');
      this.dayWedSelectionValue = this.getSubDayValue('Wednesday');
      this.dayThurSelectionValue = this.getSubDayValue('Thursday');
      this.dayFriSelectionValue = this.getSubDayValue('Friday');
      this.daySatSelectionValue = this.getSubDayValue('Saturday');
      this.daySunSelectionValue = this.getSubDayValue('Sunday');
    }
  }
  private getDayValue(day: string): boolean {
    return this.registerSchedule.get(`schedule${day}`)?.value === true;
  }
  private getSubDayValue(day: string): boolean {
    return this.registerSchedule.get(`schedule${day}`)?.value === true;
  }
  private validateScheduleInputs(): boolean {
    const freq = this.registerSchedule.controls.frequency.value;

    if (!freq) {
      this.showWarning('Please select Frequency...!');
      return false;
    }

    const noDaySelected = !(
      this.dayMonSelectionValue ||
      this.dayTueSelectionValue ||
      this.dayWedSelectionValue ||
      this.dayThurSelectionValue ||
      this.dayFriSelectionValue ||
      this.daySatSelectionValue ||
      this.daySunSelectionValue
    );

    if (freq === 'Monthly') {
      if (this.weekSelectionValue == null || this.weekSelectionValue == undefined) {
        this.showWarning('Please select Week of the Month...!');
        return false;
      }

      if (noDaySelected) {
        this.showWarning('Please select a day of Week...!');
        return false;
      }
    } else {
      if (noDaySelected) {
        this.showWarning('Please select a day of Week...!');
        return false;
      }
    }

    if (!this.schedule_careteam_id) {
      this.showWarning('Please select Assignee Name.');
      return false;
    }

    return true;
  }
  private showWarning(message: string, title: string = 'Warning') {
    this.confirmDialog.showConfirmDialog(message, title, () => null, false);
  }
  weekSelectionFrequency: any;
  schedule_edit_id: any;
  userid: any;
  user_name: any;
  ScheduleDatabyId: any;
  update_schedule_variable: any;
  worklist_edit = false;
  navigateEditSchedule_from_Worklist(data: any) {
    this.worklist_edit = true;
    this.navigateEditSchedule(data);
  }

  main_schedule_id: any;
  AssigneeMemberStatus: any;
  navigateEditSchedule(data: any) {
    this.schedule_edit_id = data;
    this.worklist_edit = true;
    this.update_schedule_variable = true;

    this.getMenuChioce(3);
    this.rpmservice
      .rpm_get(
        `/api/schedules/getworklistschedulesbyid?CurrentScheduleId=${this.schedule_edit_id}`
      )
      .then((data) => {
        this.ScheduleDatabyId = data;
        this.schedule_assignedId = this.ScheduleDatabyId.AssignedBy;
        this.main_schedule_id = this.ScheduleDatabyId.Id;
        this.AssigneeMemberStatus = this.ScheduleDatabyId.IsCompleted;

        var careTeamList = this.http_getSchedulemasterdata.AssigneeList;

        this.schedule_careteam_id = this.ScheduleDatabyId.AssignedBy;
        var objAssignee = careTeamList.filter((obj: { Id: any }) => {
          return obj.Id == this.schedule_careteam_id;
        });

        this.schedule_careteam_name = objAssignee[0];

        var that = this;
        if (data) {
          this.frequencyValue = this.ScheduleDatabyId.Schedule;
          if (this.ScheduleDatabyId.WeekSelection == 0) {
            this.editScheduleMon = this.ScheduleDatabyId.Mon;
            this.editScheduleTue = this.ScheduleDatabyId.Tue;
            this.editScheduleWed = this.ScheduleDatabyId.Wed;
            this.editScheduleThur = this.ScheduleDatabyId.Thu;
            this.editScheduleFri = this.ScheduleDatabyId.Fri;
            this.editScheduleSat = this.ScheduleDatabyId.Sat;
            this.editScheduleSun = this.ScheduleDatabyId.Sun;

            this.editScheduleSubMon = false;
            this.editScheduleSubTue = false;
            this.editScheduleSubWed = false;
            this.editScheduleSubThur = false;
            this.editScheduleSubFri = false;
            this.editScheduleSubSat = false;
            this.editScheduleSubSun = false;
          }
          else {
            this.weekSelectionFrequency = this.ScheduleDatabyId.WeekSelection;
            this.editScheduleSubMon = this.ScheduleDatabyId.Mon;
            this.editScheduleSubTue = this.ScheduleDatabyId.Tue;
            this.editScheduleSubWed = this.ScheduleDatabyId.Wed;
            this.editScheduleSubThur = this.ScheduleDatabyId.Thu;
            this.editScheduleSubFri = this.ScheduleDatabyId.Fri;
            this.editScheduleSubSat = this.ScheduleDatabyId.Sat;
            this.editScheduleSubSun = this.ScheduleDatabyId.Sun;

            this.editScheduleMon = false;
            this.editScheduleTue = false;
            this.editScheduleWed = false;
            this.editScheduleThur = false;
            this.editScheduleFri = false;
            this.editScheduleSat = false;
            this.editScheduleSun = false;
          }

          var objSelected = this.scheduleUserList.filter((obj: any) => {
            return (
              obj.Id == this.ScheduleDatabyId.AssignedTo &&
              true == obj.IsPatient
            );
          });

          //

          var ScheduleTypeView =
            this.http_getSchedulemasterdata.ScheduleTypes.filter(
              (data: any) => {
                return (
                  data.ScheduleTypeId == this.ScheduleDatabyId.ScheduleTypeId
                );
              }
            );

          if (this.update_schedule_variable == true) {
            this.Scheduled_userName = objSelected[0];
            setTimeout(() => {
              that.registerSchedule.controls['scheduleType'].setValue(
                parseInt(this.ScheduleDatabyId.ScheduleTypeId),
                { onlySelf: true }
              );
            }, 250);
          } else {
            this.Scheduled_userName = objSelected[0];
            setTimeout(() => {
              that.registerSchedule.controls['scheduleType'].setValue(
                parseInt(this.ScheduleDatabyId.ScheduleTypeId),
                { onlySelf: true }
              );
            }, 250);
          }

          if (this.checkedSingle) {
            that.registerSchedule.controls['startTime'].setValue(
              this.ScheduleDatabyId.CurrentScheduleStartTime
            );
            that.durationValue = parseInt(
              this.ScheduleDatabyId.CurrentScheduleDuration
            );
            that.registerSchedule.controls['scheduleDescription'].setValue(
              this.ScheduleDatabyId.CurrentScheduleComments
            );
            that.registerSchedule.controls['startDate'].setValue(
              this.convertDateData(this.ScheduleDatabyId.CurrentScheduleDate)
            );
            that.registerSchedule.controls['endDate'].setValue(
              this.convertDateData(this.ScheduleDatabyId.EndDate)
            );
          } else if (this.checkedSeries) {
            that.registerSchedule.controls['startTime'].setValue(
              this.ScheduleDatabyId.StartTime
            );
            that.durationValue = parseInt(this.ScheduleDatabyId.Duration);
            that.registerSchedule.controls['scheduleDescription'].setValue(
              this.ScheduleDatabyId.Comments
            );
            that.registerSchedule.controls['startDate'].setValue(
              this.convertDateData(this.ScheduleDatabyId.StartDate)
            );
            that.registerSchedule.controls['endDate'].setValue(
              this.convertDateData(this.ScheduleDatabyId.EndDate)
            );
          }
          this.registerSchedule.controls.assigneeName.setValue(
            this.ScheduleDatabyId.CreatedBy
          );
          that.registerSchedule.controls['weekSelectionFrequency'].setValue(
            this.weekSelectionFrequency
          );
          that.registerSchedule.controls['scheduleMonday'].setValue(
            this.editScheduleMon
          );
          that.registerSchedule.controls['scheduleTuesday'].setValue(
            this.editScheduleTue
          );
          that.registerSchedule.controls['scheduleWednesday'].setValue(
            this.editScheduleWed
          );
          that.registerSchedule.controls['scheduleThursday'].setValue(
            this.editScheduleThur
          );
          that.registerSchedule.controls['scheduleFriday'].setValue(
            this.editScheduleFri
          );
          that.registerSchedule.controls['scheduleSaturday'].setValue(
            this.editScheduleSat
          );
          that.registerSchedule.controls['scheduleSunday'].setValue(
            this.editScheduleSun
          );
          that.registerSchedule.controls['scheduleSubMonday'].setValue(
            this.editScheduleSubMon
          );
          that.registerSchedule.controls['scheduleSubTuesday'].setValue(
            this.editScheduleSubTue
          );
          that.registerSchedule.controls['scheduleSubWednesday'].setValue(
            this.editScheduleSubWed
          );
          that.registerSchedule.controls['scheduleSubThursday'].setValue(
            this.editScheduleSubThur
          );
          that.registerSchedule.controls['scheduleSubFriday'].setValue(
            this.editScheduleSubFri
          );
          that.registerSchedule.controls['scheduleSubSaturday'].setValue(
            this.editScheduleSubSat
          );
          that.registerSchedule.controls['scheduleSubSunday'].setValue(
            this.editScheduleSubSun
          );
        }
      });
  }

  updateSchedule_title: any;
  schedule_switching_variable: any;
  getUpdateScheduleVariable() {

    if (this.update_schedule_variable == true) {
      this.updateSchedule_title = 'Update Schedule';
      this.schedule_switching_variable = true;
    } else {
      this.updateSchedule_title = 'View Schedule';
      this.schedule_switching_variable = false;
    }
  }

  // editSchedule() {
  //   var userIspatient;
  //   if ((this.Scheduled_UserType = 0)) {
  //     userIspatient = false;
  //   } else {
  //     userIspatient = true;
  //   }

  //   if (this.checkedSingle) {
  //     if (
  //       this.registerSchedule.get('scheduleDescription')?.valid &&
  //       this.registerSchedule.get('startDate')?.valid &&
  //       this.registerSchedule.get('endDate')?.valid &&
  //       this.registerSchedule.get('startTime')?.valid
  //     ) {
  //       var req_body: any = {
  //         CurrentScheduleId: this.schedule_edit_id,
  //         ScheduleDate: this.convertDate(
  //           this.registerSchedule.controls.startDate.value
  //         ),
  //         StartTime: this.registerSchedule.controls.startTime.value,
  //         Duration: this.durationValue,
  //         Comments: this.registerSchedule.controls.scheduleDescription.value,
  //         IsCompleted: this.AssigneeMemberStatus,
  //       };

  //       var that = this;

  //       this.rpmservice
  //         .rpm_post('/api/schedules/updatecurrentschedule', req_body)
  //         .then(
  //           (data) => {

  //             alert("Worklist Schedule Updated Successfully!! ")
  //             this.update_schedule_variable = false;
  //             this.worklist_edit = false;
  //             this.registerSchedule.reset();
  //             this.onClickCancel();
  //             this.registerSchedule.controls.assigneeName.setValue(
  //               this.userName
  //             );
  //             this.scheduleReload();
  //             this.calculateUpcomingSchedule();
  //             this.checkedSingle = true;
  //             this.checkedSeries = false;
  //             this.durationValue = 0;
  //           },
  //           (err) => {
  //             alert('Something Went Wrong ')
  //           }
  //         );
  //     }
  //   } else if (this.checkedSeries) {
  //     if (this.frequencyValue == 'Daily') {
  //       this.weekSelectionValue = 0;
  //       this.dayMonSelectionValue = 1;
  //       this.dayTueSelectionValue = 1;
  //       this.dayWedSelectionValue = 1;
  //       this.dayThurSelectionValue = 1;
  //       this.dayFriSelectionValue = 1;
  //       this.daySatSelectionValue = 1;
  //       this.daySunSelectionValue = 1;
  //     } else if (this.frequencyValue == 'Weekly') {
  //       this.weekSelectionValue = 0;

  //       this.dayMonSelectionValue = this.registerSchedule.controls
  //         .scheduleMonday.value
  //         ? true
  //         : false; //this.ScheduleMon;
  //       this.dayTueSelectionValue = this.registerSchedule.controls
  //         .scheduleTuesday.value
  //         ? true
  //         : false; //this.ScheduleTue;
  //       this.dayWedSelectionValue = this.registerSchedule.controls
  //         .scheduleWednesday.value
  //         ? true
  //         : false; //this.ScheduleWed;
  //       this.dayThurSelectionValue = this.registerSchedule.controls
  //         .scheduleThursday.value
  //         ? true
  //         : false; //this.ScheduleThur;
  //       this.dayFriSelectionValue = this.registerSchedule.controls
  //         .scheduleFriday.value
  //         ? true
  //         : false; //this.ScheduleFri;
  //       this.daySatSelectionValue = this.registerSchedule.controls
  //         .scheduleSaturday.value
  //         ? true
  //         : false; //this.ScheduleSat;
  //       this.daySunSelectionValue = this.registerSchedule.controls
  //         .scheduleSunday.value
  //         ? true
  //         : false; //this.ScheduleSun;
  //     } else if (this.frequencyValue == 'Monthly') {
  //       this.weekSelectionValue =
  //         this.registerSchedule.controls.weekSelectionFrequency.value;

  //       this.dayMonSelectionValue = this.registerSchedule.controls
  //         .scheduleSubMonday.value
  //         ? true
  //         : false; //this.ScheduleMon;
  //       this.dayTueSelectionValue = this.registerSchedule.controls
  //         .scheduleSubTuesday.value
  //         ? true
  //         : false; //this.ScheduleTue;
  //       this.dayWedSelectionValue = this.registerSchedule.controls
  //         .scheduleSubWednesday.value
  //         ? true
  //         : false; //this.ScheduleWed;
  //       this.dayThurSelectionValue = this.registerSchedule.controls
  //         .scheduleSubThursday.value
  //         ? true
  //         : false; //this.ScheduleThur;
  //       this.dayFriSelectionValue = this.registerSchedule.controls
  //         .scheduleSubFriday.value
  //         ? true
  //         : false; //this.ScheduleFri;
  //       this.daySatSelectionValue = this.registerSchedule.controls
  //         .scheduleSubSaturday.value
  //         ? true
  //         : false; //this.ScheduleSat;
  //       this.daySunSelectionValue = this.registerSchedule.controls
  //         .scheduleSubSunday.value
  //         ? true
  //         : false; //this.ScheduleSun;
  //     }
  //     if (this.registerSchedule.valid) {
  //       if (this.registerSchedule.controls.frequency.value == null) {
  //         alert('Please select Frequency...!');
  //         return;
  //       } else if (
  //         this.registerSchedule.controls.frequency.value == 'Monthly'
  //       ) {
  //         if (
  //           this.weekSelectionValue == undefined ||
  //           this.weekSelectionValue == null
  //         ) {
  //           alert('Please select Week of the Month...!');
  //           return;
  //         } else {
  //         if (
  //             this.dayMonSelectionValue != undefined ||
  //             this.dayTueSelectionValue != undefined ||
  //             this.dayWedSelectionValue != undefined ||
  //             this.dayThurSelectionValue != undefined ||
  //             this.dayFriSelectionValue != undefined ||
  //             this.daySunSelectionValue != undefined
  //           ) {
  //           } else {
  //             alert('Please select a day of Week...!');
  //             return;
  //           }
  //         }
  //       } else {
  //        if (
  //           this.dayMonSelectionValue != undefined ||
  //           this.dayTueSelectionValue != undefined ||
  //           this.dayWedSelectionValue != undefined ||
  //           this.dayThurSelectionValue != undefined ||
  //           this.dayFriSelectionValue != undefined ||
  //           this.daySunSelectionValue != undefined
  //         ) {
  //         } else {
  //           alert('Please select a day of Week...!');
  //           return;
  //         }
  //       }
  //       if (this.Scheduled_user == undefined || this.Scheduled_user == null) {
  //         alert('Please select a Patient.');
  //         return;
  //       }

  //       if (
  //         this.schedule_careteam_id == undefined ||
  //         this.schedule_careteam_id == null
  //       ) {
  //         alert('Please select a Assignne Name.');
  //         return;
  //       }
  //       var strtDt = this.convertDate(
  //         this.registerSchedule.controls.startDate.value
  //       );
  //       var endDt = this.convertDate(
  //         this.registerSchedule.controls.endDate.value
  //       );

  //       if (endDt < strtDt) {
  //         if (endDt.getTime() === strtDt.getTime()) {
  //           return;
  //         } else {
  //           alert('Please select a Valid Start Date and End Date.');
  //           return;
  //         }
  //       }

  //       var req_body: any = {
  //         Id: this.ScheduleDatabyId.Id,
  //         AssignedTo: parseInt(this.Scheduled_user),

  //         ScheduleTypeId: parseInt(
  //           this.registerSchedule.controls.scheduleType.value
  //         ),
  //         Schedule: this.registerSchedule.controls.frequency.value,
  //         Comments: this.registerSchedule.controls.scheduleDescription.value,
  //         // StartDate : this.ConvertToUTC(this.registerSchedule.controls.startDate.value),
  //         // EndDate :this.ConvertToUTC(this.registerSchedule.controls.endDate.value),
  //         StartDate: this.convertDate(
  //           this.registerSchedule.controls.startDate.value
  //         ),
  //         EndDate: this.convertDate(
  //           this.registerSchedule.controls.endDate.value
  //         ),
  //         StartTime: this.registerSchedule.controls.startTime.value,
  //         AssignedBy: parseInt(this.schedule_careteam_id),
  //         Mon: this.dayMonSelectionValue,
  //         Tue: this.dayTueSelectionValue,
  //         Wed: this.dayWedSelectionValue,
  //         Thu: this.dayThurSelectionValue,
  //         Fri: this.dayFriSelectionValue,
  //         Sat: this.daySatSelectionValue,
  //         Sun: this.daySunSelectionValue,
  //         WeekSelection: parseInt(this.weekSelectionValue),
  //         Duration: this.durationValue,
  //         IsPatient: userIspatient,
  //       };

  //       var that = this;

  //       this.rpmservice
  //         .rpm_post('/api/schedules/updateschedule', req_body)
  //         .then(
  //           (data) => {

  //             this.confirmDialog.showConfirmDialog(
  //               'New Schedule Added Successfully!!!!',
  //               'Message',
  //               () => {
  //                 this.update_schedule_variable = false;
  //                 this.worklist_edit = false;
  //                 this.registerSchedule.reset();
  //                 this.onClickCancel();
  //                 this.registerSchedule.controls.assigneeName.setValue(
  //                   this.userName
  //                 );
  //                 this.scheduleReload();
  //                 this.calculateUpcomingSchedule();
  //                 this.checkedSingle = true;
  //                 this.checkedSeries = false;
  //                 this.durationValue = 0;
  //               },
  //               false
  //             );

  //           },
  //           (err) => {
  //             //show error patient id creation failed

  //             alert(`Something Went Wrong `);
  //             //  this.update_schedule_variable = false;
  //           }
  //         );
  //     } else {
  //       alert('Please Complete the form');
  //     }
  //   }
  // }

  editSchedule(): void {
    const isPatient = this.Scheduled_UserType !== 0;
    if (this.checkedSingle) {
      if (this.isSingleScheduleValid()) {
        const req_body = this.buildSingleScheduleUpdatePayload();
        this.rpmservice.rpm_post('/api/schedules/updatecurrentschedule', req_body)
          .then(() => {
            alert('Worklist Schedule Updated Successfully!!');
            this.resetScheduleForm();
          })
          .catch(() => alert('Something Went Wrong'));
      }
      return;
    }

    // For Series
    this.setDaySelectionsByFrequency();
    if (!this.isSeriesScheduleValid()) return;
    const req_body = this.buildSeriesScheduleUpdatePayload(isPatient);

    this.rpmservice.rpm_post('/api/schedules/updateschedule', req_body)
      .then(() => {
        this.confirmDialog.showConfirmDialog(
          'Schedule Updated Successfully!',
          'Message',
          () => this.resetScheduleForm(),
          false
        );
      })
      .catch(() => alert('Something Went Wrong'));
  }
  private isSingleScheduleValid():any {
    return this.registerSchedule.get('scheduleDescription')?.valid &&
           this.registerSchedule.get('startDate')?.valid &&
           this.registerSchedule.get('endDate')?.valid &&
           this.registerSchedule.get('startTime')?.valid;
  }
  private isSeriesScheduleValid(): boolean {
    const freq = this.registerSchedule.controls.frequency.value;

    if (!freq) {
      alert('Please select Frequency...!');
      return false;
    }

    if (freq === 'Monthly' && (this.weekSelectionValue == null || this.weekSelectionValue === undefined)) {
      alert('Please select Week of the Month...!');
      return false;
    }

    const daysSelected = [
      this.dayMonSelectionValue,
      this.dayTueSelectionValue,
      this.dayWedSelectionValue,
      this.dayThurSelectionValue,
      this.dayFriSelectionValue,
      this.daySatSelectionValue,
      this.daySunSelectionValue,
    ].some(Boolean);

    if (!daysSelected) {
      alert('Please select a day of Week...!');
      return false;
    }

    if (!this.Scheduled_user) {
      alert('Please select a Patient.');
      return false;
    }

    if (!this.schedule_careteam_id) {
      alert('Please select an Assignee Name.');
      return false;
    }

    const start = new Date(this.registerSchedule.controls.startDate.value);
    const end = new Date(this.registerSchedule.controls.endDate.value);

    if (end < start && end.getTime() !== start.getTime()) {
      alert('Please select a Valid Start Date and End Date.');
      return false;
    }

    return this.registerSchedule.valid;
  }
  private buildSingleScheduleUpdatePayload(): any {
    return {
      CurrentScheduleId: this.schedule_edit_id,
      ScheduleDate: this.convertDate(this.registerSchedule.controls.startDate.value),
      StartTime: this.registerSchedule.controls.startTime.value,
      Duration: this.durationValue,
      Comments: this.registerSchedule.controls.scheduleDescription.value,
      IsCompleted: this.AssigneeMemberStatus
    };
  }

  private buildSeriesScheduleUpdatePayload(isPatient: boolean): any {
    return {
      Id: this.ScheduleDatabyId.Id,
      AssignedTo: parseInt(this.Scheduled_user),
      ScheduleTypeId: parseInt(this.registerSchedule.controls.scheduleType.value),
      Schedule: this.registerSchedule.controls.frequency.value,
      Comments: this.registerSchedule.controls.scheduleDescription.value,
      StartDate: this.convertDate(this.registerSchedule.controls.startDate.value),
      EndDate: this.convertDate(this.registerSchedule.controls.endDate.value),
      StartTime: this.registerSchedule.controls.startTime.value,
      AssignedBy: parseInt(this.schedule_careteam_id),
      Mon: this.dayMonSelectionValue,
      Tue: this.dayTueSelectionValue,
      Wed: this.dayWedSelectionValue,
      Thu: this.dayThurSelectionValue,
      Fri: this.dayFriSelectionValue,
      Sat: this.daySatSelectionValue,
      Sun: this.daySunSelectionValue,
      WeekSelection: parseInt(this.weekSelectionValue),
      Duration: this.durationValue,
      IsPatient: isPatient
    };
  }
  private setDaySelectionsByFrequency(): void {
    const freq = this.frequencyValue;

    if (freq === 'Daily') {
      this.weekSelectionValue = 0;
      this.dayMonSelectionValue = this.dayTueSelectionValue = this.dayWedSelectionValue =
      this.dayThurSelectionValue = this.dayFriSelectionValue = this.daySatSelectionValue =
      this.daySunSelectionValue = true;
    } else if (freq === 'Weekly') {
      this.weekSelectionValue = 0;
      this.dayMonSelectionValue = !!this.registerSchedule.controls.scheduleMonday.value;
      this.dayTueSelectionValue = !!this.registerSchedule.controls.scheduleTuesday.value;
      this.dayWedSelectionValue = !!this.registerSchedule.controls.scheduleWednesday.value;
      this.dayThurSelectionValue = !!this.registerSchedule.controls.scheduleThursday.value;
      this.dayFriSelectionValue = !!this.registerSchedule.controls.scheduleFriday.value;
      this.daySatSelectionValue = !!this.registerSchedule.controls.scheduleSaturday.value;
      this.daySunSelectionValue = !!this.registerSchedule.controls.scheduleSunday.value;
    } else if (freq === 'Monthly') {
      this.weekSelectionValue = this.registerSchedule.controls.weekSelectionFrequency.value;
      this.dayMonSelectionValue = !!this.registerSchedule.controls.scheduleSubMonday.value;
      this.dayTueSelectionValue = !!this.registerSchedule.controls.scheduleSubTuesday.value;
      this.dayWedSelectionValue = !!this.registerSchedule.controls.scheduleSubWednesday.value;
      this.dayThurSelectionValue = !!this.registerSchedule.controls.scheduleSubThursday.value;
      this.dayFriSelectionValue = !!this.registerSchedule.controls.scheduleSubFriday.value;
      this.daySatSelectionValue = !!this.registerSchedule.controls.scheduleSubSaturday.value;
      this.daySunSelectionValue = !!this.registerSchedule.controls.scheduleSubSunday.value;
    }
  }


  // Add Schedule Template Ends
  // Common Function Start


  menuChoice: any;

  notificationtime(date: any) {
    let today = new Date();
    let notification_date = new Date(date);
    let msgInfo: string = '';

    let diff_min = Math.round(
      (today.getTime() - notification_date.getTime()) / 60000
    );
    //return(diff_min)
    if (diff_min == 0) {
      msgInfo = 'Few Seconds Ago';
    } else if (diff_min < 60) {
      msgInfo = diff_min + ' Min';
    } else if (diff_min < 1440) {
      var hr = Math.round(diff_min / 60);
      msgInfo = hr + ' Hrs';
    } else if (diff_min > 1440 && diff_min < 2880) {
      var day = Math.round(diff_min / 1400);
      msgInfo = 'Yesterday';
    } else if (diff_min > 2880 && diff_min < 10080) {
      // var day = Math.round(diff_min/(1400))
      let dayofweek = notification_date.getDay();
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
      }

      msgInfo = dayval;
    } else if (diff_min > 10080 && diff_min < 20160) {
      msgInfo = '1 Week Ago';
    } else if (diff_min > 10080 && diff_min < 20160) {
      msgInfo = '2 Weeks Ago';
    } else if (diff_min > 20160 && diff_min < 30240) {
      msgInfo = '3 Weeks Ago';
    } else if (diff_min > 30240 && diff_min < 40320) {
      msgInfo = '4 Weeks Ago';
    } else if (diff_min > 40320 && diff_min < 86400) {
      msgInfo = '1 month Ago';
    } else if (diff_min > 86400 && diff_min < 172800) {
      msgInfo = '2 month Ago';
    } else {
      msgInfo = '';
    }

    return msgInfo;
  }
  today: any;
  schedule_assignedId: any;

  calculateUpcomingSchedule() {
    var that = this;
    this.schedule_assignedId = null;
    this.today = new Date();
    var startdate = this.convertDate(this.today);
    this.today.setDate(this.today.getDate() + 7);
    var enddate = this.convertDate(this.today);
    that.rpmservice
      .rpm_get(
        `/api/schedules/getworklistschedules?StartDate=${startdate}&EndDate=${enddate}`
      )
      .then((data) => {
        that.LatestSchedule = [];
        that.UpcomingSchedule = data;

        var count = 0;
        for (let x of that.UpcomingSchedule) {
          if (count == 1) {
            break;
          }

          for (let y of x.SchedueInfos) {
            if (this.userId != y.AssignedBy) {
              y.ContactName = y.AssignedByName;
            } else {
              y.ContactName = y.ContactName;
            }

            if (y.IsCompleted == false) {
              var obj = {
                id: y.CurrentScheduleId,
                description: y.Description,
                patientname: y.ContactName,
                time: y.ScheduleTime,
              };
              that.LatestSchedule.push(obj);
            }
          }
          count = count + 1;
        }
        if (that.LatestSchedule.length == 0) {
          that.scheduleData = [
            {
              id: '',
              description: 'No Upcoming Schedules',
              patientname: '',
              time: '',
            },
          ];
        } else {
          that.scheduleData = that.LatestSchedule;
        }
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

  self_assignee_variable = false;
  selfAssign() {
    var myid = sessionStorage.getItem('userid');
    var myname = sessionStorage.getItem('user_name');
    this.task_careteam_name = myname;
    this.task_careteam_id = myid;
    this.self_assignee_variable = true;
  }
  selfAssignSchedule() {
    var myid = sessionStorage.getItem('userid');
    var myname = sessionStorage.getItem('user_name');

    this.schedule_careteam_name = myname;
    this.schedule_careteam_id = myid;
    this.self_assignee_variable = true;
  }

  self_alert_assignee_variable = false;
  //New Change Alert commented

  // selfAssignAlert() {
  //   this.self_alert_assignee_variable = true;
  //   var myid = sessionStorage.getItem('userid');
  //   this.viewAlertform.controls.assignee_name.setValue(myid, {
  //     onlySelf: true,
  //   });
  // }

  selfAssignTeamAlert() {
    this.self_alert_assignee_variable = true;
    var myid = sessionStorage.getItem('userid');
    this.viewTeamAlertform.controls.assignee_name.setValue(myid, {
      onlySelf: true,
    });
  }
  worklistgettaskbyid: any;
  taskId: any;
  WorkListTaskArrayList: any;

  getTaskUpdateValue(task_id: any) {
    this.taskId = task_id;

    if (task_id) {
      this.updateMenuChoice(4);
      this.rpmservice
        .rpm_get(`/api/tasks/worklistgettaskbyid?Id=${task_id}`)
        .then(
          (data) => {
            this.worklistgettaskbyid = data;
            this.alertAssigneeName = this.worklistgettaskbyid.CareTeamId;

            this.WorkListTaskArrayList = this.worklistgettaskbyid.Members;

            this.task_pname = this.worklistgettaskbyid.PatientName;
            this.task_pid = this.worklistgettaskbyid.PatientId;
            this.registerTask.controls.tasktype.setValue(
              parseInt(this.worklistgettaskbyid.TaskTypeId)
            );
            this.registerTask.controls.description.setValue(
              this.worklistgettaskbyid.Description
            );
            var duedate = this.convertDate(this.worklistgettaskbyid.DueDate);
            // dob2 = dob2.split('T')[0]
            this.registerTask.controls.duedate.setValue(
              this.convertToLocalTimeDate(this.worklistgettaskbyid.DueDate)
            );
            this.registerTask.controls.priority.setValue(
              parseInt(this.worklistgettaskbyid.PriorityId)
            );
            this.registerTask.controls.task_status.setValue(
              this.worklistgettaskbyid.Status,
              { onlySelf: true }
            );
            // this.registerTask.controls.assignee_name.setValue(this.worklistgettaskbyid.AssignedMemberId,{ onlySelf: true });
            this.task_careteam_name = this.worklistgettaskbyid.AssignedMember;
            this.task_careteam_id = this.worklistgettaskbyid.AssignedMemberId;
            this.registerTask.controls.task_comment.setValue(
              this.worklistgettaskbyid.Comments
            );
            this.taskId = task_id;
            // this.worklistgettaskbyid.AssignedMemberId = 1;
          },
          (err) => {
            //show error patient id creation failed

            alert('Error, Failed to Load Task data..!!');
          }
        );
    }
  }

  updateTask() {
    if (this.task_pid == undefined || this.task_pid == null) {
      alert('Please select a Patient.');
      return;
    }
    if (this.task_careteam_id == undefined || this.task_careteam_id == null) {
      alert('Please select a CareTeam Person.');
      return;
    }
    if (this.registerTask.valid) {
      let latest_date = this.datepipe.transform(
        this.registerTask.controls.duedate.value,
        'yyyy-MM-dd'
      );
      var req_body: any = {
        Id: this.taskId,
        // CareteamMemberUserId:this.registerTask.controls.assignee_name.value,
        CareteamMemberUserId: this.task_careteam_id,
        Name: this.registerTask.controls.description.value,
        Comments: this.registerTask.controls.task_comment.value,
        // PatientId: this.registerTask.controls.userName.value.PatientId,
        PatientId: parseInt(this.task_pid),
        TaskTypeId: this.registerTask.controls.tasktype.value,
        DueDate: this.ConvertToUTCInput(new Date(latest_date + 'T00:00:00')),
        PriorityId: this.registerTask.controls.priority.value,
        Status: this.registerTask.controls.task_status.value,
        WatcherUserId: null,
      };
      var that = this;
      this.rpmservice.rpm_post('/api/tasks/updatetask', req_body).then(
        (data) => {
          alert(`Task Updated Successfully!! `);
          alert('Update Successfully');
          this.getMenuChioce(1);
          this.taskReload();
        },
        (err) => {

          this.confirmDialog.showConfirmDialog(
            'Something Went Wrong',
            'Error',
            () => {
              this.task_update_variable = false;
              this.getMenuChioce(1);
            },
            false
          );

        }
      );
    } else {
      alert('Please Complete the Form ');
    }
  }
  @Output() TaskTablereload = new EventEmitter();
  taskReload() {
    this.TaskTablereload.emit();
  }
  @Output() ScheduleTablereload = new EventEmitter();
  scheduleReload() {
    this.ScheduleTablereload.emit();
  }
  //  @Output() UserReload = new EventEmitter();
  //  userReload(){
  //      this.UserReload.emit();

  //  }
  updateMenuChoice(menu: any) {
    var userTeam = this.http_TaskMasterData.CareTeamMembersList.filter(
      (c: { UserId: number }) => c.UserId == this.userId
    );
    this.http_taskAssigneeList =
      this.http_TaskMasterData.CareTeamMembersList.filter(
        (c: { CareTeamId: number }) => c.CareTeamId === userTeam[0].CareTeamId
      );
    this.task_update_variable = true;
    this.getMenuChioce(menu);
    if (menu == 4) {
      this.menuTitle = 'Update Task';
    }
  }
  addMenuChoice(menu: any) {
    this.task_update_variable = false;
    this.getMenuChioce(menu);
    if (menu == 4) {
      this.menuTitle = 'Add Task';
    }
  }
  myDate = new Date();

  //   add Team
  team_edit_variable: any;

  add_team() {
    this.onClickCancel();
    this.menu_choice = 9;
    this.display_tool_bar = false;
    this.team_edit_variable = false;

    this.teamMemberId = [];
    this.teamMemberArray = [];
    this.new_member = null;
    this.tmp_member = [];
  }

  edit_team() {
    this.onClickCancel();
    this.onClear_careteam('');
    this.menu_choice = 9;
    this.display_tool_bar = false;
    this.team_edit_variable = true;
    this.tmp_member = [];
  }

  SelectedTeamData: any;
  SelectTeamId: any;
  SelectedTeamName: any;
  masterdata2: unknown;

  selectedTeamPatient(event: { option: { value: any } }) {
    this.SelectedTeamData = event.option.value;
    this.SelectTeamId = this.SelectedTeamData.CareTeamMemberUserId;
    this.SelectedTeamName = this.SelectedTeamData.Name;
  }

  getOptionTeamText(option: { Name: any }) {
    if (option != null) {
      return option.Name;
    } else {
      return 'Select';
    }
  }

  registerTeamForm = new UntypedFormGroup({
    teamname: new UntypedFormControl(null, [Validators.required]),
    mangerName: new UntypedFormControl(null, [Validators.required]),
  });

  new_member: any;
  tmp_member: any;
  keyword_member = 'Name';
  selectEvent_member(item: any) {
    // do something with selected item

    this.SelectedTeamData = item;
    this.SelectTeamId = this.SelectedTeamData.CareTeamMemberUserId;
    this.SelectedTeamName = this.SelectedTeamData.Name;
  }

  onChangeSearch_member(val: string) {}

  onFocused_member(e: any) {}
  onClear_member(event: any) {
    this.SelectedTeamData = null;
    this.SelectTeamId = null;
    this.SelectedTeamName = null;
  }
  add_member() {
    var filtered = this.teamMemberArray.filter(
      (c) => c.teamId == this.SelectTeamId
    );
    if (filtered.length == 0) {
      if (this.SelectedTeamName) {
        var unaasignedValue = this.http_unassigned.filter(
          (c: { Name: string }) => c.Name == this.SelectedTeamName
        );

        if (unaasignedValue.length > 0) {
          this.teamMemberArray.push({
            teamName: this.SelectedTeamName,
            teamId: this.SelectTeamId,
          });
          var tmparr = this.http_unassigned.filter(
            (data: { CareTeamMemberUserId: any }) => {
              return data.CareTeamMemberUserId == this.SelectTeamId;
            }
          );
          if (tmparr.length > 0) {
            this.tmp_member.push(tmparr[0]);
          }

          this.http_unassigned.forEach(
            (value: { CareTeamMemberUserId: any }, index: any) => {
              if (value.CareTeamMemberUserId == this.SelectTeamId)
                this.http_unassigned.splice(index, 1);
            }
          );
          this.new_member = null;
        }
      }
    }
  }
  teamMemberArray = [
    {
      teamName: '',
      teamId: '',
    },
  ];
  teamMemberId: number[] = [];

  // removeItem(index:any){
  //   this.teamMemberArray.splice(index, 1);
  // }
  http_assigned: any;
  removeItem(index: any) {
    var teamremoveItem = this.teamMemberArray[index].teamId;
    var revert = [];
    if (this.http_CareTeamById) {
      var team_member = this.http_CareTeamById.TeamMembers.filter(
        (c: { UserId: any }) => c.UserId == teamremoveItem
      );
      if (team_member.length == 0) {
        revert = this.tmp_member.filter((data: { Name: string }) => {
          return data.Name == this.teamMemberArray[index].teamName;
        });
        if (revert.length > 0) {
          this.http_unassigned.push(revert[0]);
          this.teamMemberArray.splice(index, 1);
        }
      } else if (
        team_member[0].MemberPatientCount -
          team_member[0].MemberDischargePatientCount >
        0
      ) {
        alert(
          'Care Team Member has patients attached, cannot remove a member with Patients'
        );
        return;
      } else {
        revert = this.tmp_member.filter((data: { Name: string }) => {
          return data.Name == this.teamMemberArray[index].teamName;
        });
        if (revert.length > 0) {
          this.http_unassigned.push(revert[0]);
          this.teamMemberArray.splice(index, 1);
        } else {
          this.ProcessMainList(team_member);
          this.teamMemberArray.splice(index, 1);
        }
      }
    } else {
      revert = this.tmp_member.filter((data: { Name: string }) => {
        return data.Name == this.teamMemberArray[index].teamName;
      });
      if (revert.length > 0) {
        this.http_unassigned.push(revert[0]);
        this.teamMemberArray.splice(index, 1);
      } else {
        this.ProcessMainList(team_member);
        this.teamMemberArray.splice(index, 1);
      }
    }
  }
  addteamVariable = false;
  teamloading = false;

  ProcessMainList(data: any) {
    if (data.length > 0) {
      var obj = {
        CareTeamMemberUserId: data[0].UserId,
        Name: data[0].MemberFirstName + ' ' + data[0].MemberLastName,
        UserName: data[0].MemberFirstName,
      };
      this.http_unassigned.push(obj);
    }
  }

  registerTeam() {
    this.teamloading = true;
    var req_body: any = {};
    // For MemberUserId Normal array

    var duplicateTeamName = this.http_CareTeamList.filter(
      (data: { TeamName: any }) => {
        return this.registerTeamForm.controls.teamname.value == data.TeamName;
      }
    );
    if (duplicateTeamName.length > 0) {
      alert('Team Name  Already Exists..!');
      this.teamloading = false;
      // this.getMenuChioce(1);
      // this.registerTeamForm.reset()
      return;
    }

    if (this.registerTeamForm.valid && this.teamMemberArray.length > 0) {
      // this.teamMemberArray.slice(1).forEach((value) => {
      //   this.teamMemberId.push(parseInt(value.teamId));
      //  });
      for (let mem of this.teamMemberArray) {
        this.teamMemberId.push(parseInt(mem.teamId));
      }
      this.teamMemberArray = [];
      req_body['Name'] = this.registerTeamForm.controls.teamname.value;
      req_body['ManagerId'] = parseInt(
        this.registerTeamForm.controls.mangerName.value
      );
      if (this.teamMemberId.length > 0) {
        req_body['MemberUserId'] = this.teamMemberId;
      }

      this.rpmservice.rpm_post('/api/careteam/addcareteam', req_body).then(
        (data) => {
          this.teamloading = false;
          this.confirmDialog.showConfirmDialog(
            'Team Add Successfully!!',
            'Message',
            () => {
              this.addteamVariable = false;
              this.registerTeamForm.reset();
              this.teamMemberId = [];
              this.teamMemberArray = [];
              this.new_member = null;
              this.tmp_member = [];

              this.rolelist = sessionStorage.getItem('Roles');
              this.rolelist = JSON.parse(this.rolelist);
              this.getMenuChioce(1);
            },
            false
          )

          this.rpmservice
            .rpm_get(
              '/api/patient/getprogramdetailsmasterdataaddpatient?RoleId=' +
                this.rolelist[0].Id
            )
            .then((data) => {
              this.masterdata2 = data;
              sessionStorage.setItem(
                'add_patient_masterdata',
                JSON.stringify(this.masterdata2)
              );
            });
          this.rpmservice
            .rpm_get('/api/careteam/getunassignedmembers')
            .then((data) => {
              this.unassigned_members = data;
              sessionStorage.setItem(
                'unassigned_members',
                JSON.stringify(this.unassigned_members)
              );
              this.http_unassigned =
                sessionStorage.getItem('unassigned_members');
              this.http_unassigned = JSON.parse(this.http_unassigned);
            });
          this.getTeamData();
          this.userReload();
        },
        (err) => {
          this.teamloading = false;

          this.confirmDialog.showConfirmDialog(
            'Could not Add Team.',
            'Error',
            () => {
              this.getMenuChioce(1);
              this.teamMemberId = [];
              this.teamMemberArray = [];
              this.new_member = null;
              this.tmp_member = [];
            },
            false
          )

        }
      );
    } else {
      alert('Please Complete the Form.');
      this.teamloading = false;
    }
  }
  ClikedMe() {}
  convertToLocalTime(utcDate: any) {
    utcDate = utcDate + 'Z'; // ISO-8601 formatted date returned from server

    var localDate = new Date(utcDate);
    var localDate = new Date(utcDate);
    return this.notificationtime(localDate);
  }

  keyword = 'PatientName';
  data = [
    {
      id: 1,
      name: 'Georgia',
    },
    {
      id: 2,
      name: 'Usa',
    },
    {
      id: 3,
      name: 'England',
    },
  ];

  selectEvent(item: any) {
    // do something with selected item
    this.SelectId = item.PatientId;
  }

  onChangeSearch(val: string) {}

  onFocused(e: any) {}
  onClearEvent(e: any) {
    this.SelectId = undefined;
  }

  keyword_schedule = 'Name';

  Scheduled_user: any;
  Scheduled_userName: any;
  Scheduled_UserType: any;

  selectEvent_schedule(item: any) {
    // do something with selected item
    if (typeof item != 'string') {
      this.Scheduled_user = item.Id;
      this.Scheduled_userName = item.Name;
      this.Scheduled_UserType = item.IsPatient;
    }

    var userTeam = this.http_getSchedulemasterdata.PatientOrContactName.filter(
      (c: { Id: number }) => c.Id == this.Scheduled_user
    );
    this.http_AssigneeListSchedule =
      this.http_getSchedulemasterdata.AssigneeList;
    this.http_AssigneeListSchedule = this.http_AssigneeListSchedule.filter(
      (c: { CareTeamId: number }) => c.CareTeamId == userTeam[0].CareTeamId
    );
  }
  clearEvent_Sschedule(item: any) {
    this.Scheduled_user = null;
    this.Scheduled_userName = null;
    this.Scheduled_UserType = false;
    this.http_AssigneeListSchedule = [];
  }
  onChangeSearch_schedule(val: string) {
    // fetch remote data from here
    // And reassign the 'data' which is binded to 'data' property.
    this.Scheduled_user = val;
  }

  onFocused_schedule(e: any) {
    // do something when input is focused
    //this.Scheduled_user = e
  }

  keyword_task_patient = 'PatientName';
  task_pid: any;
  task_pname: any;

  selectEvent_task_patient(item: any) {
    // do something with selected item
    if (typeof item != 'string') {
      this.task_pid = item.PatientId;
      this.task_pname = item.PatientName;
    }

    var userTeam = this.http_TaskMasterData.PatientList.filter(
      (c: { PatientId: number }) => c.PatientId == this.task_pid
    );
    if (userTeam && userTeam.length > 0) {
      this.http_taskAssigneeList =
        this.http_TaskMasterData.CareTeamMembersList.filter(
          (c: { CareTeamId: number }) => c.CareTeamId == userTeam[0].CareTeamId
        );
    } else {
      return;
    }
    this.refreshTaskAssign();
    this.onClear_taskAssign(null);
    this.selectEvent_task_careteam(null);
    this.onFocused_task_careteam(null);

    this.task_careteam_name = null;
    this.task_careteam_id = null;
  }
  clearEvent_Task(item: any) {
    this.task_pid = null;
    this.task_pname = null;
  }
  onChangeSearch_task_patient(val: string) {
    // fetch remote data from here
    // And reassign the 'data' which is binded to 'data' property.
    this.task_pid = val;
  }

  onFocused_task_patient(e: any) {
    // do something when input is focused
    //this.Scheduled_user = e
  }
  selfTeamTaskAssign() {
    var myid = sessionStorage.getItem('userid');
    var myname = sessionStorage.getItem('user_name');
    this.task_team_careteam_name = myname;
    this.task_team_careteam_id = myid;
    this.self_assignee_variable = true;
  }

  keyword_task_careteam = 'UserName';
  keyword_task_careteamupdate = 'Member';

  task_careteam_id: any;
  task_careteam_name: any;
  keyword_teamtask_careteam = 'Member';
  task_team_careteam_id: any;
  task_team_careteam_name: any;

  //New Change Alert

  keyword_alert_careteam = 'Member';
  alert_careteam_id: any;
  alert_careteam_name: any;

  keyword_team_alert_careteam = 'Member';
  alert_team_careteam_id: any;
  alert_team_careteam_name: any;
  selectEvent_task_careteam(item: any) {
    // do something with selected item
    if (typeof item != 'string') {
      if (item != null) {
        this.task_careteam_id = item.UserId;
        this.task_careteam_name = item.Member;
      } else {
        this.task_careteam_id = null;
        this.task_careteam_name = null;
      }
    }

    if (this.task_careteam_name == this.userName) {
      this.self_assignee_variable = true;
    } else {
      this.self_assignee_variable = false;
    }
  }
  selectEvent_task_update_careteam(item: any) {
    // do something with selected item
    if (typeof item != 'string') {
      this.task_careteam_id = item.Userid;
      this.task_careteam_name = item.Member;
    }

    if (this.task_careteam_name == this.userName) {
      this.self_assignee_variable = true;
    } else {
      this.self_assignee_variable = false;
    }
  }
  onChangeSearch_task_careteam(val: string) {
    // fetch remote data from here
    // And reassign the 'data' which is binded to 'data' property.
    this.task_careteam_id = val;
  }

  onFocused_task_careteam(e: any) {
    // do something when input is focused
    //this.Scheduled_user = e
    this.task_careteam_id = null;
    this.task_careteam_name = null;
  }

  selectEvent_team_task_careteam(item: any) {
    // do something with selected item

    if (typeof item != 'string') {
      this.task_team_careteam_id = item.Userid;
      this.task_team_careteam_name = item.Member;
    }

    if (this.task_team_careteam_name == this.userName) {
      this.self_assignee_variable = true;
    } else {
      this.self_assignee_variable = false;
    }
  }

  onChangeSearch_team_task_careteam(val: string) {
    // fetch remote data from here
    // And reassign the 'data' which is binded to 'data' property.
    this.task_team_careteam_id = val;
  }
  // Schedule Assignee List

  keyword_schedule_careteam = 'UserName';
  schedule_careteam_id: any;
  schedule_careteam_name: any;

  selectEvent_schedule_careteam(item: any) {
    // do something with selected item
    if (typeof item != 'string') {
      this.schedule_careteam_id = item.Id;
      this.schedule_careteam_name = item.UserName;
    }

    if (this.schedule_careteam_name == this.userName) {
      this.self_assignee_variable = true;
    } else {
      this.self_assignee_variable = false;
    }
  }

  onChangeSearch_schedule_careteam(val: string) {
    // fetch remote data from here
    // And reassign the 'data' which is binded to 'data' property.
    this.schedule_careteam_id = val;
  }

  onFocused_schedule_careteam(e: any) {
    // do something when input is focused
    //this.Scheduled_user = e
  }

  keyword_careteam = 'TeamName';
  http_CareTeamById: any;
  careTeamId: any;
  teamName: any;
  selectEvent_careteam(item: any) {
    // do something with selected item

    this.rpmservice
      .rpm_get(`/api/careteam/teambyid?TeamId=${item.TeamId}`)
      .then((data) => {
        this.http_CareTeamById = data;
        this.careTeamId = item.TeamId;
        this.teamName = item.TeamName;
        this.teamMemberArray = this.processTeam(
          this.http_CareTeamById.TeamMembers
        );

        this.registerTeamForm.controls.mangerName.setValue(
          this.http_CareTeamById.ManagerUserId,
          { onlySelf: true }
        );
        this.registerTeamForm.controls.teamname.setValue(this.teamName);
      });
  }

  onChangeSearch_careteam(val: string) {
    // fetch remote data from here
    // And reassign the 'data' which is binded to 'data' property.
  }

  onFocused_careteam(e: any) {
    // do something when input is focused
    //this.Schedul
    this.registerTeamForm.controls.teamname.value;
  }

  onClear_careteam(e: any) {
    // do something when input is focused
    //this.Scheduled_user = e
    this.careTeamId = null;
    this.teamMemberArray = [];
    this.registerTeamForm.reset();
    this.new_member = null;
  }

  @Output() UserReload = new EventEmitter();
  userReload() {
    this.UserReload.emit();
  }

  @Output() masterReload: EventEmitter<any> = new EventEmitter();

  processTeam(list: any) {
    var tempArr = [];
    for (let obj of list) {
      var newobj = {
        teamName: obj.MemberFirstName + ' ' + obj.MemberLastName,
        teamId: obj.UserId,
      };
      tempArr.push(newobj);
    }
    return tempArr;
  }

  updateTeam() {
    this.teamloading = true;
    var req_body: any = {};
    // For MemberUserId Normal array
    for (let mem of this.teamMemberArray) {
      this.teamMemberId.push(parseInt(mem.teamId));
    }
    this.teamMemberArray = [];
    if (this.registerTeamForm.valid) {
      req_body['CareTeamId'] = this.careTeamId;
      req_body['Name'] = this.registerTeamForm.controls.teamname.value;
      req_body['ManagerId'] = parseInt(
        this.registerTeamForm.controls.mangerName.value
      );
      if (this.teamMemberId.length > 0) {
        req_body['MemberUserId'] = this.teamMemberId;
      }

      this.rpmservice.rpm_post('/api/careteam/updatecareteam', req_body).then(
        (data) => {
          this.teamloading = false;
          this.confirmDialog.showConfirmDialog(
            'Team Updated Successfully!!',
            'Message',
            () => {
              this.addteamVariable = false;
              this.registerTeamForm.reset();
              this.teamMemberId = [];
              this.new_member = null;

              this.tmp_member = [];

              this.rolelist = sessionStorage.getItem('Roles');
              this.rolelist = JSON.parse(this.rolelist);
              this.getMenuChioce(1);
            },
            false
          )

          this.rpmservice
            .rpm_get(
              '/api/patient/getprogramdetailsmasterdataaddpatient?RoleId=' +
                this.rolelist[0].Id
            )
            .then((data) => {
              this.masterdata2 = data;
              sessionStorage.setItem(
                'add_patient_masterdata',
                JSON.stringify(this.masterdata2)
              );
            });
          this.rpmservice
            .rpm_get('/api/careteam/getunassignedmembers')
            .then((data) => {
              this.unassigned_members = data;
              sessionStorage.setItem(
                'unassigned_members',
                JSON.stringify(this.unassigned_members)
              );
              this.http_unassigned =
                sessionStorage.getItem('unassigned_members');
              this.http_unassigned = JSON.parse(this.http_unassigned);
            });
          this.getTeamData();
          this.userReload();
        },
        (err) => {
          this.teamloading = false;
          this.confirmDialog.showConfirmDialog(
            `${err.error}`,
            'Error',
            () => {
              this.getMenuChioce(1);
              this.teamMemberId = [];
              this.new_member = null;
              this.tmp_member = [];
            },
            false
          )

        }
      );
    } else {
      alert('Please Complete the Form.');
      this.teamloading = false;
    }
  }

  checkedSeries = false;
  checkedSingle = true;

  CheckSingOrSeriesValue(event: any) {
    if (
      event.target.name == 'singleOccurance' &&
      event.target.checked == true
    ) {
      this.checkedSingle = true;
      this.checkedSeries = false;

      this.navigateEditSchedule(this.schedule_edit_id);
    } else {
      {
        this.checkedSingle = false;
        this.checkedSeries = true;

        // this.navigateEditSchedule(this.schedule_edit_id)
      }
    }

    if (
      event.target.name == 'seriesOccurance' &&
      event.target.checked == true
    ) {
      this.checkedSeries = true;
      this.checkedSingle = false;
      this.navigateEditSchedule(this.schedule_edit_id);
    } else {
      {
        this.checkedSeries = false;
        this.checkedSingle = true;
        // this.navigateEditSchedule(this.schedule_edit_id)
      }
    }

    this.settingSingleorSeriesValues();
    this.navigateEditSchedule(this.schedule_edit_id);
  }
  settingSingleorSeriesValues() {
    if (this.checkedSingle) {
      this.registerSchedule.controls['startTime'].setValue(
        this.ScheduleDatabyId.CurrentScheduleStartTime
      );
      // that.durationValue = that.ScheduleDatabyId.StartDate.Duration;
      this.durationValue = parseInt(this.ScheduleDatabyId.Duration);
      this.registerSchedule.controls['scheduleDescription'].setValue(
        this.ScheduleDatabyId.CurrentScheduleComments
      );
      this.registerSchedule.controls['startDate'].setValue(
        this.convertDate(this.ScheduleDatabyId.CurrentScheduleDate)
      );
      this.registerSchedule.controls['endDate'].setValue(
        this.convertDate(this.ScheduleDatabyId.EndDate)
      );
    } else if (this.checkedSeries) {
      this.registerSchedule.controls['startTime'].setValue(
        this.ScheduleDatabyId.StartTime
      );
      this.durationValue = parseInt(this.ScheduleDatabyId.Duration);
      this.registerSchedule.controls['scheduleDescription'].setValue(
        this.ScheduleDatabyId.Comments
      );
      this.registerSchedule.controls['startDate'].setValue(
        this.convertDate(this.ScheduleDatabyId.StartDate)
      );
      this.registerSchedule.controls['endDate'].setValue(
        this.convertDate(this.ScheduleDatabyId.EndDate)
      );
    }
  }
  refreshScheduleAssign() {
    this.schedule_careteam_name = null;
    this.schedule_careteam_id = null;
  }

  refreshTaskAssign() {
    this.task_careteam_id = null;
    this.task_careteam_name = null;
  }
  onClearSchedule(e: any) {
    this.schedule_careteam_id = null;
    this.schedule_careteam_name = null;
    this.self_assignee_variable = false;
  }
  onClear_taskAssign(e: any) {
    this.task_careteam_id = null;
    this.task_careteam_name = null;
    this.self_assignee_variable = false;
  }
  onClear_teamtaskAssign(e: any) {
    this.task_team_careteam_id = null;
    this.task_team_careteam_name = null;
    this.self_assignee_variable = false;
  }

  convertToLocalTimeDate(stillUtc: any) {
    stillUtc = stillUtc + 'Z';
    var local = moment(stillUtc).local().format('MM-DD-YYYY');
    return local;
  }

  convertToLocalTimedisplay(stillUtc: any) {
    stillUtc = stillUtc + 'Z';
    var local = moment(stillUtc).local().format('MM-DD-YYYY HH:mm');

    return local;
  }

  ConvertToUTC(data: any) {
    var timenow = new Date().toLocaleTimeString();
    var combinedDate = new Date(data + ' ' + timenow);
    var combineDateIsoFrm = new Date(combinedDate.toUTCString()).toISOString();

    return combineDateIsoFrm;
  }
  convertDateData(dateval: any) {
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
    dateval = mm2 + '-' + dd2 + '-' + yyyy;
    const myArray = dateval.split('T');
    return myArray;
  }
  OnPatientClick(PatientId: any, PatientProgramid: any) {
    let route = '/admin/patients_detail';
    this._router.navigate([route], {
      queryParams: { id: PatientId, programId: PatientProgramid },
      skipLocationChange: true,
    });
  }

  registerVendor = new UntypedFormGroup({
    vendorName: new UntypedFormControl(null, [Validators.required]),
    vendorCode: new UntypedFormControl(null, [Validators.required]),
    primaryMobVendor: new UntypedFormControl('', [
      Validators.required,
      Validators.pattern('[- +()0-9]+'),
      Validators.minLength(10),
      Validators.maxLength(10),
    ]),
    alternateMobVendor: new UntypedFormControl('', [
      Validators.pattern('[- +()0-9]+'),
      Validators.minLength(10),
      Validators.maxLength(10),
    ]),

    vendorAddress1: new UntypedFormControl(null, [Validators.required]),
    vendorAddress2: new UntypedFormControl(null, [Validators.required]),

    zipcode: new UntypedFormControl(null, [Validators.required]),
    city: new UntypedFormControl(null, [Validators.required]),
    state: new UntypedFormControl(null, [Validators.required]),
  });
  AddVendor() {
    var req_body: any = {};
    if (this.verificationStatus != true) {
      alert('Please Verify Vendor Code');
    }

    if (this.registerVendor.valid) {
      req_body['Name'] = this.registerVendor.controls.vendorName.value;
      req_body['Code'] = this.registerVendor.controls.vendorCode.value;
      req_body['Address1'] = this.registerVendor.controls.vendorAddress1.value;
      req_body['Address2'] = this.registerVendor.controls.vendorAddress2.value;
      req_body['CityId'] = parseInt(this.registerVendor.controls.city.value);
      req_body['StateId'] = parseInt(this.registerVendor.controls.state.value);
      req_body['CountryId'] = 233;
      req_body['ZipCode'] = this.registerVendor.controls.zipcode.value;
      req_body['PrimaryPhoneNumber'] =
        this.registerVendor.controls.primaryMobVendor.value;
      req_body['AlternatePhoneNumber'] =
        this.registerVendor.controls.alternateMobVendor.value;

      if (this.verificationStatus == true) {
        this.rpmservice.rpm_post('/api/devices/adddevicevendor', req_body).then(
          (data) => {
            alert('New Vendor Added Successfully!!');
            this.registerVendor.reset();

            this.menuChoice = 1;
            this.getMenuChioce(1);
          },

          (err) => {
            this.verificationStatus = false;
            alert(err.error);
          }
        );
      }
    } else {
      alert('Please Complete the Form ');
    }
  }

  registerDevice = new UntypedFormGroup({
    device: new UntypedFormControl(null, [Validators.required]),
    deviceType: new UntypedFormControl(null, [Validators.required]),
    deviceModel: new UntypedFormControl(null, [Validators.required]),
    deviceMake: new UntypedFormControl(null, [Validators.required]),
    IMEA: new UntypedFormControl(null, [Validators.required]),
    serialNumber: new UntypedFormControl(null, [Validators.required]),
    vendorNameDevice: new UntypedFormControl(null, [Validators.required]),
    purchaseDate: new UntypedFormControl(null, [Validators.required]),
    addressline1: new UntypedFormControl(null),
  });

  @Output() DeviceTablereload = new EventEmitter();
  deviceReload() {
    this.DeviceTablereload.emit();
  }
  AddDevice() {
    var req_body: any = {};
    var vendorSelected = this.VendorList.filter(
      (vendor: { DeviceVendorId: any }) =>
        vendor.DeviceVendorId ===
        parseInt(this.registerDevice.controls.vendorNameDevice.value)
    );
    var VendorName = vendorSelected[0].Name;

    if (this.registerDevice.valid) {
      req_body['Device'] = this.registerDevice.controls.device.value;
      req_body['DeviceType'] = this.registerDevice.controls.deviceType.value;
      req_body['DeviceModel'] = this.registerDevice.controls.deviceModel.value;
      req_body['DeviceVendor'] = VendorName;
      req_body['DeviceManufacturer'] =
        this.registerDevice.controls.deviceMake.value;
      req_body['DeviceSerialNo'] =
        this.registerDevice.controls.serialNumber.value;
      req_body['DeviceIMEINo'] = this.registerDevice.controls.IMEA.value;
      req_body['PurchaseDate'] =
        this.registerDevice.controls.purchaseDate.value;

      this.rpmservice.rpm_post('/api/devices/adddevice', req_body).then(
        (data) => {
          alert('New Device Added Successfully!!');
          this.registerVendor.reset();
          this.deviceReload();
          this.onClickVendorCancel();
        },
        (err) => {
          //show error patient id creation failed
          alert('Error, could not add Device..!, ' + err);
        }
      );
    } else {
      alert('Please Complete the Form ');
    }
  }
  onClickVendorCancel() {
    this.registerVendor.reset();

    this.menuChoice = 1;
    this.getMenuChioce(1);
    this.registerDevice.controls['state'].setValue(undefined);
    this.registerDevice.controls['city'].setValue(undefined);
  }
  onDeviceCancel() {
    this.registerDevice.reset();
    // this.registerVendor.controls['DeviceType'].setValue(undefined);
    // this.registerVendor.controls['Device'].setValue(undefined);

    this.menuChoice = 1;
    this.getMenuChioce(1);
  }

  DeviceMasterdataArray: any;
  DeviceArray: any;
  DeviceTypearray: any;
  DeviceMakeArray: any;
  VendorList: any;
  getDeviceArrayList() {
    var that = this;
    that.rpmservice.rpm_get('/api/devices/devicemasterdata').then((data2) => {
      this.DeviceMasterdataArray = data2;
      this.VendorList = this.DeviceMasterdataArray.DeviceVendor;
      this.DeviceTypearray = this.DeviceMasterdataArray.DeviceType;
      this.DeviceMakeArray = this.DeviceMasterdataArray.DeviceManufacturer;
      this.DeviceArray = this.DeviceMasterdataArray.Device;
      // this.DeviceArray = sessionStorage.getItem('DeviceListArray');
      // this.DeviceArray = JSON.parse(this.DeviceArray);
      // this.DeviceTypearray = sessionStorage.getItem('DeviceTypeList');
      // this.DeviceTypearray = JSON.parse(this.DeviceTypearray);

      // this.DeviceMakeArray = sessionStorage.getItem('DeviceMakeArray');
      // this.DeviceMakeArray = JSON.parse(this.DeviceMakeArray);

      // this.VendorList = sessionStorage.getItem('VendorList');
      // this.VendorList = JSON.parse(this.VendorList);
    });
  }
  vendorSelected: any;
  getVendorDetails() {
    var that = this;
  }
  verificationStatus: any;
  verifyVendorCode() {
    var req_body: any = {};
    var verificationCode = this.registerVendor.controls.vendorCode.value;
    if (this.registerVendor.controls.vendorCode.value != null) {
      this.rpmservice
        .rpm_post(
          `/api/devices/isvalidvendorcode?Code=${verificationCode}`,
          req_body
        )
        .then(
          (data) => {
            this.verificationStatus = data;
          },
          (err) => {
            //show error patient id creation failed
            alert('Vendor Code Verification failed.');
            this.verificationStatus = false;
          }
        );
    } else {
      alert('Please Enter Valid Vendor Code');
    }
  }
  ConvertToUTCInput(data: any) {
    var combineDateIsoFrm = new Date(data.toUTCString()).toISOString();

    return combineDateIsoFrm;
  }

  //  add Team

  @Output() TeamTaskTablereload = new EventEmitter();
  teamtaskReload() {
    this.TeamTaskTablereload.emit();
  }
  registerTeamTask = new UntypedFormGroup({
    // task_pname: new FormControl(null,[Validators.required]),
    tasktype: new UntypedFormControl(null, [Validators.required]),
    description: new UntypedFormControl(null),
    duedate: new UntypedFormControl(null, [Validators.required]),
    priority: new UntypedFormControl(null, [Validators.required]),
    task_status: new UntypedFormControl(null, [Validators.required]),
    // assignee_name:new FormControl(null,[Validators.required]),
    task_comment: new UntypedFormControl(null),
  });

  viewTeamAlertform = new UntypedFormGroup({
    Name: new UntypedFormControl(null, [Validators.required]),
    alertType: new UntypedFormControl(null, [Validators.required]),
    alertdescription: new UntypedFormControl(null, [Validators.required]),
    dueDate: new UntypedFormControl(null, [Validators.required]),
    alertPriority: new UntypedFormControl(null, [Validators.required]),
    alertstatus: new UntypedFormControl(null, [Validators.required]),
    assignee_name: new UntypedFormControl(null, [Validators.required]),
    comments: new UntypedFormControl(null),
  });

  http_taskData: any;
  teamTaskAlertArray: any;
  getTeamTaskUpdateValue(task_id: any) {
    this.taskId = task_id;
    this.taskArrayList = undefined;

    if (task_id) {
      this.updateMenuChoice(13);
      this.rpmservice
        .rpm_get(`/api/tasks/worklistgettaskbyid?Id=${task_id}`)
        .then(
          (data) => {
            this.http_taskData = data;
            this.taskArrayList = this.http_taskData.Members;
            this.alertAssigneeName = this.http_taskData.CareTeamId;

            this.http_taskAssigneeList =
              this.http_TaskMasterData.CareTeamMembersList.filter(
                (c: { CareTeamId: number }) =>
                  c.CareTeamId === this.alertAssigneeName
              );

            this.task_pname = this.http_taskData.PatientName;
            this.task_pid = this.http_taskData.PatientId;
            this.registerTeamTask.controls.tasktype.setValue(
              parseInt(this.http_taskData.TaskTypeId)
            );
            this.registerTeamTask.controls.description.setValue(
              this.http_taskData.Description
            );
            // var duedate = this.convertDate(this.http_taskData.DueDate)
            var date = this.convertToLocalTimeDate(this.http_taskData.DueDate);
            var dd = date.split('-');
            var duedate = dd[2] + '-' + dd[0] + '-' + dd[1];
            // dob2 = dob2.split('T')[0]
            this.registerTeamTask.controls.duedate.setValue(duedate);
            this.registerTeamTask.controls.priority.setValue(
              parseInt(this.http_taskData.PriorityId)
            );
            this.registerTeamTask.controls.task_status.setValue(
              this.http_taskData.Status,
              { onlySelf: true }
            );
            // this.registerTask.controls.assignee_name.setValue(this.worklistgettaskbyid.AssignedMemberId,{ onlySelf: true });
            this.task_team_careteam_name = this.http_taskData.AssignedMember;
            this.task_team_careteam_id = this.http_taskData.AssignedMemberId;

            this.registerTeamTask.controls.task_comment.setValue(
              this.http_taskData.Comments
            );
            this.taskId = task_id;
          },
          (err) => {
            //show error patient id creation failed

            alert('Error, Failed to Load Task data..!!');
          }
        );
    }
  }

  updateTaskTeam() {
    var duedatevalue: any;
    this.loading = true;

    if (this.registerTeamTask.controls.duedate.value) {
      if (this.registerTeamTask.controls.duedate.value.includes('T')) {
        var duedatevalue = this.registerTeamTask.controls.duedate.value;
      } else {
        duedatevalue =
          this.registerTeamTask.controls.duedate.value + 'T00:00:00';
      }
    }
    if (this.task_pid == undefined || this.task_pid == null) {
      alert('Please select a Patient.');
      this.loading = false;

      return;
    }
    if (
      this.task_team_careteam_id == undefined ||
      this.task_team_careteam_id == null
    ) {
      alert('Please select a CareTeam Person.');
      this.loading = false;

      return;
    }
    if (this.registerTeamTask.valid) {
      var req_body: any = {
        Id: this.taskId,
        CareteamMemberUserId: this.task_team_careteam_id,
        Name: this.registerTeamTask.controls.description.value,
        Comments: this.registerTeamTask.controls.task_comment.value,
        PatientId: parseInt(this.task_pid),
        TaskTypeId: this.registerTeamTask.controls.tasktype.value,
        DueDate: this.ConvertToUTCInput(new Date(duedatevalue)),
        PriorityId: this.registerTeamTask.controls.priority.value,
        Status: this.registerTeamTask.controls.task_status.value,
        WatcherUserId: null,
      };
      var that = this;
      this.rpmservice.rpm_post('/api/tasks/updatetask', req_body).then(
        (data) => {
          this.confirmDialog.showConfirmDialog(
            `Task Updated Successfully!`,
            'Update Task',
            () => {
              this.getMenuChioce(1);
              this.teamtaskReload();
            },
            false
          )
          this.loading = false;
        },
        (err) => {
          //show error patient id creation failed
          this.confirmDialog.showConfirmDialog(
            `Something Went Wrong`,
            'Message',
            () => {
              this.task_update_variable = false;
              this.getMenuChioce(1);
            },
            false
          )

          this.loading = false;
        }
      );
    } else {
      alert('Please Complete the Form ');
    }
  }

  teamalertId: any;
  http_alertdata: any;
  taskArrayList: any;

  getTaskAlertUpdation(data: any) {
    this.teamalertId = data.Id;
    this.getMenuChioce(14);
    this.rpmservice
      .rpm_get(`/api/alerts/worklistgetalertbyid?Id=${data.Id}`)
      .then(
        (data) => {
          this.http_alertdata = data;
          this.teamTaskAlertArray = this.http_alertdata.Members;

          // this.http_taskAssigneeList =
          //   this.http_TaskMasterData.CareTeamMembersList.filter(
          //     (c: { CareTeamId: number }) =>
          //       c.CareTeamId == this.alertAssigneeName
          //   );
          this.viewTeamAlertform.controls.alertType.setValue(
            this.http_alertdata.AlertType
          );
          if (this.http_alertdata.CreatedOn) {
            this.viewTeamAlertform.controls.alertdescription.setValue(
              this.http_alertdata.Description +
                '.' +
                this.convertToLocalTimedisplay(this.http_alertdata.CreatedOn)
            );
          } else {
            this.viewTeamAlertform.controls.alertdescription.setValue(
              this.http_alertdata.Description
            );
          }
          //  this.viewAlertform.controls.alertdescription.setValue(this.alertbyIdValue.Description);
          var dueDate = this.http_alertdata.DueDate;

          this.viewTeamAlertform.controls.dueDate.setValue(
            this.convertToLocalTimedisplay(dueDate)
          );
          this.viewTeamAlertform.controls.alertPriority.setValue(
            this.http_alertdata.Priority
          );
          this.viewTeamAlertform.controls.alertstatus.setValue(
            this.http_alertdata.Status,
            { onlySelf: true }
          );
          // setTimeout(() => {
          //   this.viewTeamAlertform.controls.assignee_name.setValue(
          //     parseInt(this.http_alertdata.AssignedMemberId),
          //     { onlySelf: true }
          //   );
          // }, 250);

          //New Change Alert
          this.alert_team_careteam_name = this.http_alertdata.AssignedMember;
          this.alert_team_careteam_id = this.http_alertdata.AssignedMemberId;

          this.viewTeamAlertform.controls.comments.setValue(
            this.http_alertdata.Comments
          );
          this.viewTeamAlertform.controls.Name.setValue(
            this.http_alertdata.PatientName
          );
        },
        (err) => {
          //show error patient id creation failed
          alert('Error, Failed to Load Alert data..!!');
        }
      );
  }

  @Output() TeamAlertTablereload = new EventEmitter();
  teamAlertReload() {
    this.TeamAlertTablereload.emit();
  }
  updateAlertTeam() {
    this.rolelist = sessionStorage.getItem('Roles');
    this.roles = JSON.parse(this.rolelist);
    var req_body: any = {};
    var roleId = this.roles[0].Id;
    this.loading = true;
    //New Change Alert
    if (
      this.alert_team_careteam_id == null ||
      this.alert_team_careteam_id == undefined ||
      this.alert_team_careteam_id == 'undefined'
    ) {
      alert('Please Select Assigned Member');
      this.loading = false;
      return;
    }

    req_body = {
      AlertId: this.teamalertId,
      RoleId: roleId,
      AlertStatus: this.viewTeamAlertform.controls.alertstatus.value,
      //New Change Alert
      // UserId: parseInt(this.viewTeamAlertform.controls.assignee_name.value),
      Userid: parseInt(this.alert_team_careteam_id),
      Comments: this.viewTeamAlertform.controls.comments.value,
    };

    this.rpmservice.rpm_post(`/api/alerts/savealertresponse`, req_body).then(
      (data) => {
        this.confirmDialog.showConfirmDialog(
          `Alert Saved Successfully!`,
          'Message',
          () => {
            this.alertAckReload();
            this.menu_choice = 1;
            this.display_tool_bar = true;
            this.GetPriorityAlerts();
            this.teamAlertReload();

          },
          false
        )
        this.loading = false;


      },
      (err) => {
        this.confirmDialog.showConfirmDialog(
          `Something Went Wrong ${err}`,
          'Error',
          () => {
            this.menu_choice = 1;

          },
          false
        )

      }
    );
  }

  public alertPriorityDataCount: number = 3;
  public alertData: any = [];

  fetchAlertData(currentLength: number) {
    let endLimit = currentLength + this.alertPriorityDataCount + 1;

    return from(this.alertPriorityData).pipe(
      filter((alertPriorityData: any) => {
        return (
          parseInt(alertPriorityData.Index) > currentLength &&
          parseInt(alertPriorityData.Index) < endLimit
        );
      })
    );
  }

  getAlertData(fetchData: boolean) {
    if (fetchData) {
      if (this.alertPriorityData.length > this.alertPriorityDataCount) {
        this.fetchAlertData(this.alertData.length - 1).subscribe(
          (response: any) => {
            this.alertData = this.alertData.concat(response);
          },
          (err) => {
            console.log(err);
          }
        );
      } else {
        this.alertData = this.alertPriorityData;
      }
    }
  }

  //New Change Alert
  selectEvent_alert_careteam(item: any) {
    // do something with selected item
    if (typeof item != 'string') {
      this.alert_careteam_id = item.UserId;
      this.alert_careteam_name = item.Member;
    }

    if (this.alert_careteam_name == this.userName) {
      this.self_alert_assignee_variable = true;
    } else {
      this.self_alert_assignee_variable = false;
    }
  }
  onChangeSearch_alert_careteam(val: string) {
    // fetch remote data from here
    // And reassign the 'data' which is binded to 'data' property.
    this.alert_careteam_id = val;
  }

  onFocused_alert_careteam(e: any) {
    // do something when input is focused
    //this.Scheduled_user = e
  }
  onClear_alertAssign(e: any) {
    this.alert_careteam_id = null;
    this.alert_careteam_name = null;
    this.self_alert_assignee_variable = false;
  }
  selfAssignAlert() {
    this.self_alert_assignee_variable = true;
    var myid = sessionStorage.getItem('userid');
    var myname = sessionStorage.getItem('user_name');
    this.alert_careteam_name = myname;
    this.alert_careteam_id = myid;
  }

  selectEvent_alert_team_careteam(item: any) {
    // do something with selected item
    if (typeof item != 'string') {
      this.alert_team_careteam_id = item.UserId;
      this.alert_team_careteam_name = item.Member;
    }

    if (this.alert_team_careteam_name == this.userName) {
      this.self_alert_assignee_variable = true;
    } else {
      this.self_alert_assignee_variable = false;
    }
  }
  onChangeSearch_alert_team_careteam(val: string) {
    // fetch remote data from here
    // And reassign the 'data' which is binded to 'data' property.
    this.task_careteam_id = val;
  }

  onFocused_alert_team_careteam(e: any) {
    // do something when input is focused
    //this.Scheduled_user = e
  }
  onClear_alertTeamAssign(e: any) {
    this.alert_team_careteam_id = null;
    this.alert_team_careteam_name = null;
    this.self_alert_assignee_variable = false;
  }
  selfTeamAssignAlert() {
    this.self_alert_assignee_variable = true;
    var myid = sessionStorage.getItem('userid');
    var myname = sessionStorage.getItem('user_name');
    this.alert_team_careteam_name = myname;
    this.alert_team_careteam_id = myid;
  }
}

//  add Teamchedule
