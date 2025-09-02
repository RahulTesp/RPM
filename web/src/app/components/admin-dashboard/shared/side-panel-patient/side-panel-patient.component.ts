
import {
  Component,
  OnInit,
  Input,
  EventEmitter,
  Output,
  ViewChild,
  TemplateRef,
} from '@angular/core';
import { filter } from 'rxjs/operators';
import {
  UntypedFormControl,
  UntypedFormGroup,
  Validators,
} from '@angular/forms';
import { RPMService } from '../../sevices/rpm.service';
import { map, startWith } from 'rxjs/operators';
import { StatusMessageComponent } from '../status-message/status-message.component';
import { MatDialog, MatDialogConfig } from '@angular/material/dialog';
import { DatePipe } from '@angular/common';
import { ConfirmDialogServiceService } from '../confirm-dialog-panel/service/confirm-dialog-service.service';
import { PatientDataDetailsService } from '../../components/patient-detail-page/Models/service/patient-data-details.service';

import { TimeOption, generateTimeOptions } from './interface'
import { MasterDataService } from '../../sevices/master-data.service';
import dayjs from 'dayjs';
import utc from 'dayjs/plugin/utc';
import timezone from 'dayjs/plugin/timezone';

dayjs.extend(utc);
dayjs.extend(timezone);

@Component({
  selector: 'app-side-panel-patient',
  templateUrl: './side-panel-patient.component.html',
  styleUrls: ['./side-panel-patient.component.scss'],
})
export class SidePanelPatientComponent implements OnInit {
  hour: TimeOption[] = generateTimeOptions(24);    // 00 to 23
  minutes: TimeOption[] = generateTimeOptions(60); // 00 to 59
  sec: TimeOption[] = generateTimeOptions(60);
  minimizevalue = false;
  @ViewChild('myPreviewTemp', { static: true }) myPreviewTemp: TemplateRef<any>;
  http_AssigneeListSchedule: any;
  http_getPatientList: any;
  http_listener: any;
  http_getId: any;
  dataSource: any = [];
  taskTypeIdArray: any;
  taskStatusIdArray: any;
  ScheduleTypeIdArray: any;
  noteTypeIdArray: any;
  @Input() patientidHome: any;
  @Input() CallTypeVariable: any;
  http_TaskMasterData: any;
  http_note_data: any;
  filteredOptions: any;
  userName: any;
  http_unassigned_member: any;
  http_taskAssigneeList: any;
  // ///// Medication ///////////////////////

  endDate: any;
  morningSchedule = false;
  eveningSchedule = false;
  aftNoonSchedule = false;
  nightSchedule = false;
  loading: any;
  loading_note: any;


  @Input() activityMenuVariable: any;

  public NoteHours = '00';
  public NoteMinutes = '00';
  public NoteSec = '00';
  public isManagerProvider = false;
  public noteId: any;
  public noteData: any;

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

  @Input() symptom_update_variable: any;

  @Input() loadingSidePanel: any;

  medication_update_variable: any;
  notesObj: { VitalId: number; Notes: string };
  roles: string | null;
  rolelist: string | null;

  @Output() ResetReviewTimer = new EventEmitter();
  resetReviewTimer() {
    this.ResetReviewTimer.emit();
  }
  onAddData() {
    if (this.dataSource[0].VitalId == 0 && this.dataSource[0].Notes == '') {
      return;
    }
    this.dataSource.push(this.notesObj);
  }
  vital_table = [
    {
      vital_id: 1,
      vital_name: 'Blood Pressure',
    },
    {
      vital_id: 2,
      vital_name: 'Blood Glucose',
    },
  ];
  isOpen = false;
  menu_choice = 1;
  display_tool_bar = true;
  vitals: any;
  additionaNotes: any;
  showNoteModal = false;
  patientData$ = this.patientService.patientDataSubject.asObservable();
  selection_flag = false;
  getMenuChoice_main(choice: any) {
    this.selection_flag = true;
    this.getMenuChioce(choice);
  }
  getMenuOpen() {

    if (this.patientSatausData && this.patientSatausData != 'Discharged') {
      this.isOpen = !this.isOpen;
    } else {
      alert(
        'Sorry !! This Functionality is not Enabled for Discharge Patients'
      );
      return;
    }
  }


  getMenuChioce(choice: any): void {
    this.patientStatus = this.patientSatausData;
    this.refreshTaskAssign();
    this.refreshScheduleAssign();
    this.handleMenuChoiceSpecificLogic(choice);
    this.setPatientForms();
    this.menu_choice = choice;
    this.noteUpdationVariable = false;
    this.durationValue = 0;
    this.toggleToolbar(choice);
    this.loadVitals();
    this.updateTaskAssignees();
  }

  private handleMenuChoiceSpecificLogic(choice: number): void {
    if (choice === 2) {
      this.NoteTime = sessionStorage.getItem('PageTimer');
      this.startNoteTimeInterval();

      this.patientProgramName = sessionStorage.getItem('patientPgmName');

      if (this.selection_flag) {
        this.getMasterDataQuestions(this.patientProgramName);
        this.selection_flag = false;
      }
    }
  }
  private startNoteTimeInterval(): void {
    setInterval(() => {
      this.NoteTime = sessionStorage.getItem('PageTimer');
      if (this.NoteTime) {
        this.NoteTime = this.NoteTime.replace("'", '"');
      }
    }, 1000);
  }

  private setPatientForms(): void {
    const name = this.patientName;
    this.registerTask.controls.frmpatientName.setValue(name);
    this.registerSchedule.controls.frmpatientName.setValue(name);
    this.registerSymptoms.controls.frmpatientName.setValue(name);
    this.addMedicationForm.controls.frmpatientName.setValue(name);
    this.viewAlertform.controls.frmpatientName.setValue(name);
  }

  private toggleToolbar(choice: number): void {
    this.display_tool_bar = choice === 1;
  }
  private updateTaskAssignees(): void {

    this.masterdataService
    .getFilteredTaskAssignees(this.roleId[0]?.Id, this.patientId)
    .then((res) => {
      this.http_TaskMasterData = res.taskMasterData;
      this.http_taskAssigneeList = res.filteredAssignees;
    });
  }

  private loadVitals(): void {
    const vitalsData = sessionStorage.getItem('viatls');
    this.vitals = vitalsData ? JSON.parse(vitalsData) : null;
  }

  onClickCancel() {
    this.menuChoice = 1;
    this.getMenuChioce(1);
    this.refreshTaskAssign();
    this.refreshScheduleAssign();
    this.scheduleTableReload();
    this.resetCallPanel();
    this.update_schedule_variable = false;
    this.isOpen = false;
    this.symptom_update_variable = false;
    this.task_updation_variable = false;
    this.userName = sessionStorage.getItem('user_name');
    this.registerSchedule.reset();
    this.registerTask.reset();
    this.registerSymptoms.reset();
    this.resetMedication();
    this.setPatientForms();
  }

  OnClickTaskCancel() {
    this.getMenuChioce(1);
    this.registerTask.reset();
    this.task_updation_variable = false;

    this.registerTask.controls.frmpatientName.setValue(this.patientName);
    this.isOpen = false;
  }

  OnClickAlertCancel() {
    this.getMenuChioce(1);
    this.registerTask.reset();

    this.registerTask.controls.frmpatientName.setValue(this.patientName);
    this.isOpen = false;
  }

  OnClickScheduleCancel() {
    this.getMenuChioce(1);
    this.registerSchedule.reset();
    this.update_schedule_variable = false;

    this.registerSchedule.controls.frmpatientName.setValue(this.patientName);
    this.isOpen = false;
    this.checkedSingle = true;
    this.checkedSeries = false;
  }

  OnClickSymptomCancel() {
    this.getMenuChioce(1);
    this.registerSymptoms.reset();
    this.symptom_update_variable = false;
    this.registerSymptoms.controls.frmpatientName.setValue(this.patientName);
    this.isOpen = false;
  }

  OnClickMedicationCancel() {
    this.getMenuChioce(1);
    // this.addMedicationForm.reset();
    this.resetMedication();
    this.medication_update_variable = false;
    this.addMedicationForm.controls.frmpatientName.setValue(this.patientName);
    this.isOpen = false;
    this.medication_durationValue = 1;
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
      Name: 'Low',
    },
    // {
    //   Id:4,
    //   Name:'Urgent'
    // }
  ];

  registernotes = new UntypedFormGroup({
    notetype: new UntypedFormControl(null, [Validators.required]),
    patientname: new UntypedFormControl(null, [Validators.required]),
    vital: new UntypedFormControl(null, [Validators.required]),
    description: new UntypedFormControl(null, [Validators.required]),
  });
  userid: any;
  selectedValue: any;
  http_getSchedulemasterdata: any;
  QuestionArrayBase: any;
  previewArray: any;

  patientProgramName: any;
  noteMasterData: any;
  registerSchedule = new UntypedFormGroup({
    frmpatientName: new UntypedFormControl(null, [Validators.required]),
    scheduleType: new UntypedFormControl(null, [Validators.required]),
    scheduleDescription: new UntypedFormControl(null, [Validators.required]),
    startDate: new UntypedFormControl(null, [Validators.required]),
    endDate: new UntypedFormControl(null, [Validators.required]),
    startTime: new UntypedFormControl(null, [Validators.required]),
    frequency: new UntypedFormControl(null, [Validators.required]),
    scheduleMonday: new UntypedFormControl(false),
    scheduleTuesday: new UntypedFormControl(false),
    scheduleWednesday: new UntypedFormControl(false),
    scheduleThursday: new UntypedFormControl(false),
    scheduleFriday: new UntypedFormControl(false),
    scheduleSaturday: new UntypedFormControl(false),
    scheduleSunday: new UntypedFormControl(false),
    scheduleSubMonday: new UntypedFormControl(false),
    scheduleSubTuesday: new UntypedFormControl(false),
    scheduleSubWednesday: new UntypedFormControl(false),
    scheduleSubThursday: new UntypedFormControl(false),
    scheduleSubFriday: new UntypedFormControl(false),
    scheduleSubSaturday: new UntypedFormControl(false),
    scheduleSubSunday: new UntypedFormControl(false),
    weekSelectionFrequency: new UntypedFormControl(false),
  });

  ngOnInit(): void {
    this.initializeSessionData();
    this.setupFormValueChanges();
    this.fetchScheduleMasterData();
    this.fetchTaskMasterData();
    this.update_symptom_list();
    this.updatePatientName();

    this.patientService.patientDataSubject
    .pipe(filter(data => data !== null))
    .subscribe(data => {
      this.patientName = data.PatientDetails.FirstName + ' ' + data.PatientDetails.LastName;
      this.patientSatausData = data.PatientProgramdetails.Status
    });
  this.todaty_date = this.datepipe.transform(new Date(), 'yyyy-MM-dd');
  this.patientId = sessionStorage.getItem('PatientId');


  //  Symptom Type Filter
  this.filteredOptions = this.registerSymptoms
    .get('symptomsType')
    ?.valueChanges.pipe(
      startWith(''),
      map((val) => this.filter(val))
    );
  this.notesObj = {
    VitalId: 0,
    Notes: '',
  };
  this.dataSource = [
    {
      VitalId: 0,
      Notes: '',
    },
  ];
}


  constructor(
    private rpm: RPMService,
    public dialog: MatDialog,
    public datepipe: DatePipe,
    public confirmDialog: ConfirmDialogServiceService,
    private patientService: PatientDataDetailsService,
    private masterdataService:MasterDataService
  ) {}
  initializeSessionData(): void {
    this.patientProgramName = sessionStorage.getItem('patientPgmName');
    this.http_getId = JSON.parse(sessionStorage.getItem('operational_masterdata') || '[]');
    this.symptom_update_variable = false;
    this.medication_update_variable = false;
    this.task_updation_variable = false;
    this.userName = sessionStorage.getItem('user_name');
    this.userid = sessionStorage.getItem('userid');
    this.roles = sessionStorage.getItem('Roles');
    this.pid = sessionStorage.getItem('PatientId');
    this.roleId = this.roles ? JSON.parse(this.roles) : [];
    this.patientdataUserId = sessionStorage.getItem('patientdataUserId');

    this.taskTypeIdArray = this.http_getId.filter((data: any) => data.Type === 'PatientTaskType');
    this.taskStatusIdArray = this.http_getId.filter((data: any) => data.Type === 'Task');
    this.selectedValue = this.taskStatusIdArray[0]?.Name || '';
  }

  setupFormValueChanges(): void {
    const days = ['Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday', 'Sunday'];

    days.forEach(day => {
      this.registerSchedule.get(`schedule${day}`)?.valueChanges.subscribe(val => {
        (this as any)[`Schedule${day.slice(0, 3)}`] = val === true;
      });

      this.registerSchedule.get(`scheduleSub${day}`)?.valueChanges.subscribe(val => {
        (this as any)[`ScheduleSub${day.slice(0, 3)}`] = val === true;
      });
    });

    this.registerSchedule.get('frequency')?.valueChanges.subscribe(() => {
      days.forEach(day => {
        (this as any)[`Schedule${day.slice(0, 3)}`] = false;
        (this as any)[`ScheduleSub${day.slice(0, 3)}`] = false;
      });
    });


  }
  fetchScheduleMasterData(): void {
    const roleId = this.roleId[0]?.Id;
    const patientUserId = this.patientdataUserId;

    if (!roleId) return;

    this.masterdataService.getScheduleMasterData(roleId, patientUserId)
      .then(res => {
        this.http_getSchedulemasterdata = res.rawData;
        this.ScheduleTypeIdArray = res.scheduleTypes;
        this.scheduleUserList = res.patientOrContacts;
        this.http_AssigneeListSchedule = res.filteredAssignees;

        sessionStorage.setItem('PatientOrContact', JSON.stringify(res.patientOrContacts));
      })
      .catch(err => {
        console.error('Error fetching schedule master data:', err);
      });
  }

  fetchTaskMasterData(): void {
    this.rpm.rpm_get(`/api/tasks/gettaskmasterdata?RoleId=${this.roleId[0]?.Id}`).then((data) => {
      const response = data as any;

      this.http_TaskMasterData = response;
      const CurrentPatientCareTeam = response.PatientList.find((d: { PatientId: any; }) => d.PatientId === this.patientId);
      this.http_taskAssigneeList = response.CareTeamMembersList;

      if (CurrentPatientCareTeam) {
        this.http_taskAssigneeList = this.http_taskAssigneeList.filter(
          (          d: { CareTeamId: any; }) => d.CareTeamId === CurrentPatientCareTeam.CareTeamId
        );
      }
    });
  }


  convertToSeconds(hours: any, minutes: any, seconds: any) {
    return Number(hours) * 60 * 60 + Number(minutes) * 60 + Number(seconds);
  }

  menuChoice: any;
  patientId: any;

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
      msgInfo = ' YesterDay';
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
    } else if (diff_min > 43200 && diff_min < 86400) {
      msgInfo = '1 month Ago';
    } else if (diff_min > 86400 && diff_min < 172800) {
      msgInfo = '2 month Ago';
    }

    return msgInfo;
  }

  receiveItem($event: any) {
    console.log('Event ' + $event);
  }
  updatePatientName() {
    this.patientdataUserId = sessionStorage.getItem('patientdataUserId');
    // this.getPatientname();
    this.registerTask.controls.frmpatientName.setValue(this.patientName);
    this.registerSchedule.controls.frmpatientName.setValue(this.patientName);
    this.registerSymptoms.controls.frmpatientName.setValue(this.patientName);
    this.addMedicationForm.controls.frmpatientName.setValue(this.patientName);
    this.viewAlertform.controls.frmpatientName.setValue(this.patientName);
  }
  // Schedule Template

  durationValue = 0;
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

  //  Frequency
  frequencyValue: any;
  medication_durationValue = 1;
  medicationDurtaionincrement() {
    this.medication_durationValue++;
  }
  medicationDurtaiondecrement() {
    if (this.medication_durationValue > 1) {
      this.medication_durationValue--;
    } else {
      this.medication_durationValue = 1;
    }
  }
  //  Add Task Start

  registerTask = new UntypedFormGroup({
    frmpatientName: new UntypedFormControl(null, [Validators.required]),
    tasktype: new UntypedFormControl(null, [Validators.required]),
    description: new UntypedFormControl(null, [Validators.required]),
    duedate: new UntypedFormControl(null, [Validators.required]),
    priority: new UntypedFormControl(null, [Validators.required]),
    task_status: new UntypedFormControl(null, [Validators.required]),
    task_comment: new UntypedFormControl(null),
  });

  AddTask() {
    this.patientId = sessionStorage.getItem('PatientId');
    if (this.task_updation_variable) {
      this.updateTask();
    } else {
      var req_body: any = {};
      if (this.task_careteam_id == undefined || this.task_careteam_id == null) {
        alert('Please select a CareTeam Person.');
        return;
      }
      if (this.registerTask.valid) {
        req_body['PatientId'] = this.patientId;
        req_body['TaskTypeId'] = this.registerTask.controls.tasktype.value;
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

        this.rpm.rpm_post('/api/tasks/addtask', req_body).then(
          (data) => {
            this.confirmDialog.showConfirmDialog(
              'New Task Added Successfully!!',
              'Message',
              () => {
                this.getMenuChioce(1);
                this.taskTableReload();
                this.openTaskTableReload()
                this.registerTask.reset();
                this.loading = false;
              },
              false
            );

          },
          (err) => {
            this.loading = false;
          }
        );
      } else {
        this.loading = false;

        this.confirmDialog.showConfirmDialog(
          'Please Complete the form',
          'Warning',
          () => {

          },
          false
        );
      }
    }
  }
  @Output() TaskTablereload = new EventEmitter();
  @Output() openTaskTablereload = new EventEmitter();
  taskTableReload() {
    this.TaskTablereload.emit();
  }
  openTaskTableReload(){
    this.openTaskTablereload.emit()

  }
  updateTask() {
    this.patientId = sessionStorage.getItem('PatientId');
    let latest_date = this.datepipe.transform(
      this.registerTask.controls.duedate.value,
      'yyyy-MM-dd'
    );
    if (this.registerTask.valid) {
      var req_body: any = {
        Id: this.taskId,
        CareteamMemberUserId: this.task_careteam_id,
        Name: this.registerTask.controls.description.value,
        Comments: this.registerTask.controls.task_comment.value,
        PatientId: this.patientId,
        TaskTypeId: this.registerTask.controls.tasktype.value,
        DueDate: this.ConvertToUTCInput(new Date(latest_date + 'T00:00:00')),
        PriorityId: this.registerTask.controls.priority.value,
        Status: this.registerTask.controls.task_status.value,
        WatcherUserId: null,
      };
      this.rpm.rpm_post('/api/tasks/updatetask', req_body).then(
        (data) => {

          this.confirmDialog.showConfirmDialog(
            'Task Updated Successfully!!',
            'Message',
            () => {
              this.task_updation_variable = false;
              this.registerTask.reset();
              this.getMenuChioce(1);

              this.taskTableReload();
              this.openTaskTableReload();
            },
            false
          );

        },
        (err) => {
          this.confirmDialog.showConfirmDialog(
            'Something Went Wrong',
            'Error',
            () => {
              this.task_updation_variable = true;
              this.getMenuChioce(1);
            },
            false
          );


        }
      );
    } else {
      this.confirmDialog.showConfirmDialog(
        'Please Complete the Form',
        'Warning',
        () => {
         null
        },
        false
      );
    }
  }

  // Add Schedule

  patientdataUserId: any;
  weekSelectionValue: any;
  dayMonSelectionValue: any;
  dayTueSelectionValue: any;
  dayWedSelectionValue: any;
  dayThurSelectionValue: any;
  dayFriSelectionValue: any;
  daySatSelectionValue: any;
  daySunSelectionValue: any;

  AddSchedule(): void {
    this.patientdataUserId = sessionStorage.getItem('patientdataUserId');

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
        AssignedTo: this.patientdataUserId,
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

      this.rpm.rpm_post('/api/schedules/addschedule', req_body).then(
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
    return this.registerSchedule.get(`scheduleSub${day}`)?.value === true;
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

  private resetScheduleForm(): void {
    this.registerSchedule.reset();
    this.userName = sessionStorage.getItem('user_name');
    this.menu_choice = 1;
    this.scheduleTableReload();
    this.getMenuChioce(1);
    this.isOpen = false;
    this.onClickCancel();
    this.loading = false;
    this.durationValue = 0;
  }

  @Output() ScheduleTablereload = new EventEmitter();
  scheduleTableReload() {
    this.ScheduleTablereload.emit();
  }

  // Schedule Template Ends
  NoteTypeId: any;
  NoteTime: any;
  pid: any;
  cid: any;
  @Output() reviewTableReload = new EventEmitter();
  notesReload() {
    this.reviewTableReload.emit();
  }
  @Output() billingInfoReload = new EventEmitter();
  BillingInfoReload() {
    this.billingInfoReload.emit();
  }

  isValidTime(str: any) {
    let exactMatch = new RegExp('([01]?[0-9]|2[0-3]):[0-5][0-9]');
    if (str.empty()) {
      return false;
    }
    if (str.match(exactMatch)) {
      return true;
    } else {
      return false;
    }
  }

  convertTimeToSec(time: any) {
    var split = time.split(':');
    var result =
      parseInt(split[0]) * 3600 + parseInt(split[1]) * 60 + parseInt(split[2]);
    return result;
  }

  // Add Symptoms:
  roleId: any;
  patientName: any;
  todaty_date: any;
  patientSatausData: any;
  callType: any;
  optionData: any;
  filter(val: any) {
    return this.optionData.filter((option: { Symptoms: any }) =>
      option.Symptoms.toLowerCase().includes(val.toString().toLowerCase())
    );
  }

  registerSymptoms = new UntypedFormGroup({
    frmpatientName: new UntypedFormControl(null, [Validators.required]),
    symptomsDescription: new UntypedFormControl(null, [Validators.required]),
  });

  currentpPatientId: any;
  currentProgramId: any;
  SaveSymptoms() {
    if (this.symptom_update_variable) {
      this.updateSymptom();
    } else {
      var req_body: any = {};
      this.registerTask.controls.frmpatientName.setValue(this.patientName);
      this.currentpPatientId = sessionStorage.getItem('PatientId');
      this.currentpPatientId = JSON.parse(this.currentpPatientId);
      this.currentProgramId = sessionStorage.getItem('ProgramId');
      this.currentProgramId = JSON.parse(this.currentProgramId);
      if (!this.Symptoms_List) {
        this.confirmDialog.showConfirmDialog(
          'Please enter Symptom Type.',
          'Warning',
          () => {
           null
          },
          false
        );
        return;
      }
      if (this.registerSymptoms.valid) {
        req_body['PatientId'] = this.currentpPatientId;
        req_body['PatientProgramId'] = this.currentProgramId;
        req_body['Symptom'] = this.Symptoms_List;
        req_body['Description'] =
          this.registerSymptoms.controls.symptomsDescription.value;
        req_body['SymptomStartDateTime'] = new Date();
        this.loading = true;
        var that = this;
        this.rpm.rpm_post('/api/patient/addpatientsymptoms', req_body).then(
          (data) => {
            this.confirmDialog.showConfirmDialog(
              'New Symptom Added Successfully!!',
              'Message',
              () => {
                this.registerSymptoms.reset();
                this.registerSymptoms.controls.frmpatientName.setValue(
                  this.patientName
                );
                this.menu_choice = 1;
                this.getMenuChioce(1);
                this.isOpen = false;
                this.symptomReload();
                this.update_symptom_list();
                this.loading = false;
                this.Symptoms_List = undefined;
              },
              false
            );

          },
          (err) => {
            this.confirmDialog.showConfirmDialog(
              'Could not add new symptom...!!',
              'Error',
              () => {
               null
              },
              false
            );
            this.loading = false;
          }
        );
      } else {
        this.confirmDialog.showConfirmDialog(
          'Please Complete the Form',
          'Warning',
          () => {
           null
          },
          false
        );
        this.loading = false;
      }
    }
  }

  //  Update Symptoms:
  getSymptomkbyid: any;
  symptomId: any;

  getSymptomUpdateValue(symptom_data: any, patientstatus: any) {
    this.patientStatus = patientstatus;
    this.updateSymptomMenuChoice(5);
    if (symptom_data) {
      this.symptomId = symptom_data.Id;
      this.Symptoms_List = symptom_data.Symptom;
      this.registerSymptoms.controls['symptomsDescription'].setValue(
        symptom_data.Description
      );
    }
  }
  symptomTitle: any;
  updateSymptomMenuChoice(menu: any) {
    this.getMenuChioce(menu);
    if (menu == 5) {
      this.symptom_update_variable = true;
      this.symptomTitle = 'Add Symptoms';
    }
  }
  addSymptomMenuChoice(menu: any) {
    this.getMenuChioce(menu);
    if (menu == 5) {
      this.symptom_update_variable = false;
      this.symptomTitle = 'Update Symptoms';
    }
  }
  @Output() SymptomTablereload = new EventEmitter();
  symptomReload() {
    this.SymptomTablereload.emit();
  }

  @Output() patientDetailSubMenuChange = new EventEmitter();
  ActivityMenuChange(data: any) {
    this.patientDetailSubMenuChange.emit();
  }

  updateSymptom() {
    var req_body: any = {};
    this.currentpPatientId = sessionStorage.getItem('PatientId');

    this.currentProgramId = sessionStorage.getItem('ProgramId');

    this.registerTask.controls.frmpatientName.setValue(this.patientName);

    if (this.registerSymptoms.valid && this.Symptoms_List != null) {
      req_body['Id'] = parseInt(this.symptomId);
      req_body['PatientId'] = parseInt(this.currentpPatientId);
      req_body['PatientProgramId'] = parseInt(this.currentProgramId);
      req_body['Symptom'] = this.Symptoms_List;
      req_body['Description'] =
        this.registerSymptoms.controls.symptomsDescription.value;
      req_body['SymptomStartDateTime'] = new Date();

      this.rpm.rpm_post('/api/patient/updatepatientsymptoms', req_body).then(
        (data) => {
          this.confirmDialog.showConfirmDialog(
            'Symptom Updated Successfully!!',
            'Message',
            () => {
              this.symptom_update_variable = false;
              this.getMenuChioce(1);
              this.registerSymptoms.reset();
              this.registerSymptoms.controls.frmpatientName.setValue(
                this.patientName
              );
              this.symptomReload();
              this.update_symptom_list();
            },
            false
          );


        },
        (err) => {
          this.confirmDialog.showConfirmDialog(
            'Could not update Symptom data...!!',
            'Error',
            () => {
             null
            },
            false
          );
        }
      );
    } else {
      this.confirmDialog.showConfirmDialog(
        'Please Complete the Form',
        'Warning',
        () => {
         null
        },
        false
      );
    }
  }

  //  add Medication:

  IntervalArray = ['Monthly', 'Weekly', 'Daily', 'Alternative'];
  IntervalTime = ['After Meal', 'Before Meal'];
  SelectedMedicalScheduleInterval: any;
  SelectedMedicalScheduleTime: any;
  @Output() MedicationTablereload = new EventEmitter();
  medicationReload() {
    this.MedicationTablereload.emit();
  }



  medicationStartDate: any;
  caluculateEndDate(startDate: any) {
    var numberOfDaysToAdd = this.medication_durationValue - 1;
    this.medicationStartDate = startDate;
    var result;
    if (this.SelectedMedicalScheduleInterval == 'Weekly') {
      result = dayjs(startDate).add(numberOfDaysToAdd * 7, 'day');

    } else if (this.SelectedMedicalScheduleInterval == 'Monthly') {
      result = dayjs(startDate).add(numberOfDaysToAdd, 'month');

    } else if (this.SelectedMedicalScheduleInterval == 'Daily') {
      result = dayjs(startDate).add(numberOfDaysToAdd, 'day');

    } else if (this.SelectedMedicalScheduleInterval == 'Alternative') {
      numberOfDaysToAdd = numberOfDaysToAdd * 2;


      result = dayjs(startDate).add(numberOfDaysToAdd, 'day');

    }
    return this.convertDate(result) + 'T00:00:00';
  }
  getDaysCalc(startdate: any, enddate: any) {
    let date_1 = new Date(startdate);
    let date_2 = new Date(enddate);
    let difference = date_2.getTime() - date_1.getTime();
    let TotalDays = Math.ceil(difference / (1000 * 3600 * 24));
    return Math.abs(TotalDays);
  }

  getWeekCalc(startdate: any, enddate: any) {
    let date_1 = new Date(startdate);
    let date_2 = new Date(enddate);
    let difference = date_2.getTime() - date_1.getTime();
    let TotalDays = Math.ceil(difference / (1000 * 3600 * 24)) / 7;
    return Math.abs(TotalDays);
  }
  getAlternativeCalc(startdate: any, enddate: any) {
    let date_1 = new Date(startdate);
    let date_2 = new Date(enddate);
    let difference = date_2.getTime() - date_1.getTime();
    let TotalDays = Math.ceil(difference / (1000 * 3600 * 24)) / 2;
    return Math.abs(TotalDays);
  }

  diff_months_duration(startDate: any, endDate: any) {
    return (
      new Date(endDate).getMonth() -
      new Date(startDate).getMonth() +
      12 * (new Date(endDate).getFullYear() - new Date(startDate).getFullYear())
    );
  }
  diff_Months_set_duration(startDate: any, endDate: any) {
    var duration;
    if (this.SelectedMedicalScheduleInterval == 'Weekly') {
      duration = this.getWeekCalc(startDate, endDate);
    } else if (this.SelectedMedicalScheduleInterval == 'Monthly') {
      duration = this.diff_months_duration(startDate, endDate);
      duration = duration;
    } else if (this.SelectedMedicalScheduleInterval == 'Daily') {
      duration = this.getDaysCalc(startDate, endDate);
    } else if (this.SelectedMedicalScheduleInterval == 'Alternative') {
      duration = this.getAlternativeCalc(startDate, endDate);
    }
    return duration;
  }

  durationText = '';
  calculatedurationtext(e: any) {
    if (this.SelectedMedicalScheduleInterval == 'Weekly') {
      this.durationText = 'Week';
    } else if (this.SelectedMedicalScheduleInterval == 'Monthly') {
      this.durationText = 'Month';
    } else if (this.SelectedMedicalScheduleInterval == 'Daily') {
      this.durationText = 'Day';
    } else if (this.SelectedMedicalScheduleInterval == 'Alternative') {
      this.durationText = 'Day';
    }
    return this.durationText;
  }

  ConvertToUTCInput(data: any) {
    var combineDateIsoFrm = new Date(data.toUTCString()).toISOString();

    return combineDateIsoFrm;
  }
  addMedicationForm = new UntypedFormGroup({
    frmpatientName: new UntypedFormControl(null, [Validators.required]),
    medicineName: new UntypedFormControl(null, [Validators.required]),
    scheduleInterval: new UntypedFormControl(null, [Validators.required]),
    scheduleTime: new UntypedFormControl(null, [Validators.required]),
    scheduleMorning: new UntypedFormControl(false),
    scheduleAfterNoon: new UntypedFormControl(false),
    scheduleEvening: new UntypedFormControl(false),
    scheduleNight: new UntypedFormControl(false),
    medicationstartDate: new UntypedFormControl(null, [Validators.required]),
    medicationcomment: new UntypedFormControl(null, [Validators.required]),
  });

 resetMedication()
 {
  this.addMedicationForm.reset({
    frmpatientName: null,
    scheduleMorning: false,
    scheduleAfterNoon: false,
    scheduleEvening: false,
    scheduleNight: false,
    medicineName:null,
    scheduleInterval:null,
    medicationcomment:null,
    medicationstartDate:null
  });
 }
  postMedication() {
    if (this.medication_update_variable) {
      this.updateMedication();
    } else {
      var req_body: any = {};
      const {
        scheduleMorning,
        scheduleAfterNoon,
        scheduleEvening,
        scheduleNight
      } = this.addMedicationForm.value;

      if (
        this.addMedicationForm.valid &&
          (scheduleMorning || scheduleAfterNoon || scheduleEvening || scheduleNight)
      ) {
        var startDate =
          this.addMedicationForm.controls.medicationstartDate.value +
          'T00:00:00';
        this.endDate = this.caluculateEndDate(startDate);

        var medicalSchedule = `${this.addMedicationForm.controls.scheduleInterval.value}`;
        this.currentpPatientId = sessionStorage.getItem('PatientId');
        this.currentpPatientId = JSON.parse(this.currentpPatientId);
        this.currentProgramId = sessionStorage.getItem('ProgramId');
        this.currentProgramId = JSON.parse(this.currentProgramId);

        req_body['PatientId'] = this.currentpPatientId;
        req_body['PatientProgramId'] = this.currentProgramId;
        req_body['Medicinename'] =
          this.addMedicationForm.controls.medicineName.value;
        req_body['MedicineSchedule'] = medicalSchedule;
        req_body['Morning'] = scheduleMorning ;
        req_body['AfterNoon'] = scheduleAfterNoon;
        req_body['Evening'] = scheduleEvening;
        req_body['Night'] = scheduleNight;
        req_body['StartDate'] = this.medicationStartDate;
        req_body['EndDate'] = this.endDate;
        req_body['Description'] =
          this.addMedicationForm.controls.medicationcomment.value;
        req_body['BeforeOrAfterMeal'] = this.SelectedMedicalScheduleTime;

        var that = this;
        this.loading = true;

        this.rpm.rpm_post('/api/patient/addpatientmedication', req_body).then(
          (data) => {
            this.confirmDialog.showConfirmDialog(
              'New Medication Added Successfully!!',
              'Message',
              () => {
            this.resetMedication();
            this.addMedicationForm.controls.frmpatientName.setValue(
              this.patientName
            );
            this.menu_choice = 1;
            this.medicationReload();
            this.getMenuChioce(1);
            this.loading = false;
            this.medication_durationValue = 1;
              },
              false
            );

          },
          (err) => {
            this.confirmDialog.showConfirmDialog(
              'Could not Add Medication...!!',
              'Error',
              () => {
               null
              },
              false
            );

            this.loading = false;
          }
        );
      } else {
        this.confirmDialog.showConfirmDialog(
          'Please Complete the form',
          'Warning',
          () => {
           null
          },
          false
        );
        this.loading = false;
      }
    }
  }

  medicationId: any;
  getMedicationUpdateValue(data: any, patientstatus: any) {
    this.patientStatus = patientstatus;
    this.getMenuChioce(6);
    this.medication_update_variable = true;
    if (data) {
      // var scheduleArray = data.MedicineSchedule.split(',');
      this.SelectedMedicalScheduleInterval = data.MedicineSchedule;
      this.SelectedMedicalScheduleTime = data.BeforeOrAfterMeal;
      this.medicationId = data.Id;
      this.addMedicationForm.controls['medicineName'].setValue(
        data.Medicinename
      );
      this.addMedicationForm.controls['scheduleInterval'].setValue(
        this.SelectedMedicalScheduleInterval
      );
      this.addMedicationForm.controls['scheduleTime'].setValue(
        this.SelectedMedicalScheduleTime
      );
      this.addMedicationForm.controls['scheduleMorning'].setValue(data.Morning);
      this.addMedicationForm.controls['scheduleAfterNoon'].setValue(
        data.AfterNoon
      );
      this.addMedicationForm.controls['scheduleEvening'].setValue(data.Evening);
      this.addMedicationForm.controls['scheduleNight'].setValue(data.Night);
      this.addMedicationForm.controls['medicationstartDate'].setValue(
        this.convertDate(data.StartDate)
      );
      this.addMedicationForm.controls['medicationcomment'].setValue(
        data.Description
      );
      //  this.medication_durationValue =  this.getDuration(data.StartDate ,data.EndDate);
      this.medication_durationValue = Number(
        this.diff_Months_set_duration(data.StartDate, data.EndDate)
      );
      this.medication_durationValue = this.medication_durationValue + 1;
    }
  }

  updateMedication() {

    var startDate =
    this.addMedicationForm.controls.medicationstartDate.value +
    'T00:00:00';
    this.endDate = this.caluculateEndDate(startDate);
    var medicalSchedule = `${this.addMedicationForm.controls.scheduleInterval.value}`;

    var req_body: any = {};

    if (
      this.addMedicationForm.valid &&
      (this.addMedicationForm.get('scheduleMorning')?.value ||
        this.addMedicationForm.get('scheduleAfterNoon')?.value ||
        this.addMedicationForm.get('scheduleEvening')?.value ||
        this.addMedicationForm.get('scheduleNight')?.value ||
        medicalSchedule != undefined)
    ) {
      req_body['PatientId'] = this.currentpPatientId;
      req_body['PatientProgramId'] = this.currentProgramId;
      req_body['Id'] = parseInt(this.medicationId);
      req_body['Medicinename'] =
        this.addMedicationForm.controls.medicineName.value;
      req_body['MedicineSchedule'] = medicalSchedule;
      req_body['Morning'] = this.addMedicationForm.get('scheduleMorning')?.value;
      req_body['AfterNoon'] = this.addMedicationForm.get('scheduleAfterNoon')?.value;
      req_body['Evening'] = this.addMedicationForm.get('scheduleEvening')?.value;
      req_body['Night'] = this.addMedicationForm.get('scheduleNight')?.value;
      req_body['StartDate'] = this.medicationStartDate;
      req_body['EndDate'] = this.endDate;
      req_body['Description'] =
        this.addMedicationForm.controls.medicationcomment.value;
      req_body['BeforeOrAfterMeal'] = this.SelectedMedicalScheduleTime;

      var that = this;

      this.rpm.rpm_post('/api/patient/updatepatientmedication', req_body).then(
        (data) => {
          this.confirmDialog.showConfirmDialog(
            'Medication Form Updated Successfully!!',
            'Message',
            () => {
              this.resetMedication();
              this.addMedicationForm.controls.frmpatientName.setValue(
                this.patientName
              );
              this.medication_update_variable = false;
              this.medicationReload();
              this.getMenuChioce(1);
              this.medication_durationValue = 1;
            },
            false
          );

        },
        (err) => {
          this.confirmDialog.showConfirmDialog(
            'Something Went Wrong',
            'Error',
            () => {
             null
            },
            false
          );
        }
      );
    } else {
      this.confirmDialog.showConfirmDialog(
        'Please Complete the form',
        'Warning',
        () => {
         null
        },
        false
      )
    }
  }
  task_updation_variable = false;
  taskbyIdValue: any;
  alertbyIdValue: any;
  taskId: any;
  alertId: any;
  patientStatus: any;
  TaskAssigneeMemberList: any;
  http_AlertAssigneeList: any;
  getTaskAlertUpdation(data: any, patientstatus: any) {
    this.patientStatus = patientstatus;
    if (data) {
      if (data.TaskOrAlert == 'Task') {
        this.task_updation_variable = true;
        this.getMenuChioce(4);
        this.taskId = data.Id;
        this.rpm.rpm_get(`/api/tasks/worklistgettaskbyid?Id=${data.Id}`).then(
          (data) => {
            this.taskbyIdValue = data;
            this.alertAssigneeName = this.taskbyIdValue.CareTeamId;

            this.http_taskAssigneeList =
              this.http_TaskMasterData.CareTeamMembersList.filter(
                (c: { CareTeamId: number }) =>
                  c.CareTeamId === this.alertAssigneeName
              );
            this.TaskAssigneeMemberList = this.taskbyIdValue.Members;
            this.registerTask.controls.frmpatientName.setValue(
              this.taskbyIdValue.PatientName
            );
            this.registerTask.controls.tasktype.setValue(
              parseInt(this.taskbyIdValue.TaskTypeId)
            );
            this.registerTask.controls.description.setValue(
              this.taskbyIdValue.Description
            );
            var duedate = this.convertToLocalTime(this.taskbyIdValue.DueDate);
            this.registerTask.controls.duedate.setValue(duedate);
            this.registerTask.controls.priority.setValue(
              parseInt(this.taskbyIdValue.PriorityId)
            );
            this.registerTask.controls.task_status.setValue(
              this.taskbyIdValue.Status,
              { onlySelf: true }
            );
            this.task_careteam_name = this.taskbyIdValue.AssignedMember;
            this.task_careteam_id = this.taskbyIdValue.AssignedMemberId;
            this.registerTask.controls.task_comment.setValue(
              this.taskbyIdValue.Comments
            );
          },
          (err) => {
            this.confirmDialog.showConfirmDialog(
              'Failed to Load Task data..!!',
              'Error',
              () => {
               null
              },
              false
            );
          }
        );
      } else {
        this.alertId = data.Id;
        this.getMenuChioce(8);
        this.rpm.rpm_get(`/api/alerts/worklistgetalertbyid?Id=${data.Id}`).then(
          (data) => {
            this.alertbyIdValue = data;

            this.alertAssigneeName = this.alertbyIdValue.CareTeamId;
            this.http_AlertAssigneeList = this.alertbyIdValue.Members;
            this.viewAlertform.controls.alertType.setValue(
              this.alertbyIdValue.AlertType
            );
            if (this.alertbyIdValue.CreatedOn) {
              this.viewAlertform.controls.alertdescription.setValue(
                this.alertbyIdValue.Description +
                  '.' +
                  this.convertToLocalTimedisplay(this.alertbyIdValue.CreatedOn)
              );
            } else {
              this.viewAlertform.controls.alertdescription.setValue(
                this.alertbyIdValue.Description
              );
            }
            var dueDate = this.alertbyIdValue.DueDate;

            this.viewAlertform.controls.dueDate.setValue(
              this.convertToLocalTimedisplay(dueDate)
            );
            this.viewAlertform.controls.alertPriority.setValue(
              this.alertbyIdValue.Priority
            );
            this.viewAlertform.controls.alertstatus.setValue(
              this.alertbyIdValue.Status,
              { onlySelf: true }
            );
            this.alert_careteam_name = this.alertbyIdValue.AssignedMember;
            this.alert_careteam_id = this.alertbyIdValue.AssignedMemberId;
            this.viewAlertform.controls.comments.setValue(
              this.alertbyIdValue.Comments
            );
          },
          (err) => {
            this.confirmDialog.showConfirmDialog(
              'Failed to Load Alert data..!!',
              'Error',
              () => {
               null
              },
              false
            );
          }
        );
      }
    }
  }



  alertAssigneeName: any;
  self_alert_assignee_variable = false;
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

  viewAlertform = new UntypedFormGroup({
    frmpatientName: new UntypedFormControl(null, [Validators.required]),
    alertType: new UntypedFormControl(null, [Validators.required]),
    alertdescription: new UntypedFormControl(null, [Validators.required]),
    dueDate: new UntypedFormControl(null, [Validators.required]),
    alertPriority: new UntypedFormControl(null, [Validators.required]),
    alertstatus: new UntypedFormControl(null, [Validators.required]),
    assignee_name: new UntypedFormControl(null, [Validators.required]),
    comments: new UntypedFormControl(null),
  });
  self_assignee_variable = false;

  selfAssign() {
    var myid = sessionStorage.getItem('userid');
    var myname = sessionStorage.getItem('user_name');
    this.task_careteam_name = myname;
    this.task_careteam_id = myid;
    this.self_assignee_variable = true;
  }
  savealert() {
    var req_body: any = {};
    this.loading = true;
    if (
      this.alert_careteam_id == null ||
      this.alert_careteam_id == undefined ||
      this.alert_careteam_id == 'undefined'
    ) {
      this.confirmDialog.showConfirmDialog(
        'Please Select Assigned Member',
        'Warning',
        () => {
         null
        },
        false
      );
      this.loading = false;
      return;
    }
    req_body = {
      AlertId: this.alertId,
      RoleId: this.roleId[0].Id,
      AlertStatus: this.viewAlertform.controls.alertstatus.value,
      UserId: parseInt(this.alert_careteam_id),
      Comments: this.viewAlertform.controls.comments.value,
    };
    this.rpm.rpm_post(`/api/alerts/savealertresponse`, req_body).then(
      (data) => {
        this.confirmDialog.showConfirmDialog(
          'Alert Update Successfully!!',
          'Message',
          () => {
            this.getMenuChioce(1);
            this.taskTableReload();
            this.openTaskTableReload();
            this.loading = false;
          },
          false
        );

      },
      (err) => {
        this.confirmDialog.showConfirmDialog(
          'Something Went Wrong',
          'Error',
          () => {
            this.menu_choice = 1;
            this.loading = false;
          },
          false
        );

      }
    );
  }

  duration: any;
  getDuration(firstDate: any, secondDate: any) {
    const oneDay = 24 * 60 * 60 * 1000;
    firstDate = new Date(firstDate);
    secondDate = new Date(secondDate);
    this.duration = Math.round(Math.abs((firstDate - secondDate) / oneDay));
    return this.duration;
  }

  convertDate(dateval: any) {
    console.log('convert Date');
    console.log(dateval)
    let today = new Date(dateval);
       console.log('convert Date After');
    console.log(today)
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

  keyword_task_careteam = 'UserName';
  keyword_task_careteamupdate = 'Member';

  task_careteam_id: any;
  task_careteam_name: any;

  selectEvent_task_careteam(item: any) {
    if (typeof item != 'string') {
      this.task_careteam_id = item.UserId;
      this.task_careteam_name = item.UserName;
    }

    if (this.task_careteam_name == this.userName) {
      this.self_assignee_variable = true;
    } else {
      this.self_assignee_variable = false;
    }
  }
  selectEvent_task_update_careteam(item: any) {
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
  clearTaskEvent(item: any) {
    this.task_careteam_id = null;
    this.task_careteam_name = null;
    this.self_assignee_variable = false;
  }
  onChangeSearch_task_careteam(val: string) {
    this.task_careteam_id = val;
  }

  onFocused_task_careteam(e: any) {
  }

  //  Schedule assignee Change

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
  clearscheduleEvent(item: any) {
    this.schedule_careteam_id = null;
    this.schedule_careteam_name = null;
  }
  onChangeSearch_schedule_careteam(val: string) {
    this.schedule_careteam_id = val;
  }

  onFocused_schedule_careteam(e: any) {
  }
  schedule_edit_id: any;
  update_schedule_variable = false;
  ScheduleDatabyId: any;
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
  weekSelectionFrequency: any;
  Scheduled_userName: any;
  scheduleUserList: any;
  userId: any;
  AssigneeMemberStatus: any;
  navigateEditSchedule(id: any) {
    this.schedule_edit_id = id;
    this.update_schedule_variable = true;
    this.getMenuChioce(3);
    this.rpm
      .rpm_get(
        `/api/schedules/getworklistschedulesbyid?CurrentScheduleId=${this.schedule_edit_id}`
      )
      .then((data) => {
        this.ScheduleDatabyId = data;
        this.AssigneeMemberStatus = this.ScheduleDatabyId.IsCompleted;

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
          } //if(this.ScheduleDatabyId.WeekSelection == 1)
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

          var objSelected = this.scheduleUserList.filter((obj: { Id: any }) => {
            return obj.Id == this.ScheduleDatabyId.AssignedTo;
          });


          var careTeamList = this.http_getSchedulemasterdata.AssigneeList;

          this.schedule_careteam_id = this.ScheduleDatabyId.AssignedBy;
          var objAssignee = careTeamList.filter((obj: { Id: any }) => {
            return obj.Id == this.schedule_careteam_id;
          });

          this.schedule_careteam_name = objAssignee[0];

          that.durationValue = that.ScheduleDatabyId.StartDate.Duration;

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
              this.ScheduleDatabyId.EndDate
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
          this.Scheduled_userName = objSelected[0];
          that.registerSchedule.controls['scheduleType'].setValue(
            parseInt(this.ScheduleDatabyId.ScheduleTypeId),
            { onlySelf: true }
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
          that.registerSchedule.controls['weekSelectionFrequency'].setValue(
            this.weekSelectionFrequency
          );
          that.registerSchedule.controls['frequency'].setValue(
            this.frequencyValue
          );
        }
      });
  }


  editSchedule() {
    if (this.checkedSingle) {
      this.updateSingleSchedule();
    } else {
      this.updateRecurringSchedule();
    }
  }


  private updateSingleSchedule() {
    if (!this.registerSchedule.valid) return this.showWarning('Please complete the form');

    const [scheduleyear, schedulemonth, scheduleday] = this.registerSchedule.controls.startDate.value.split('-').map(Number);
    var scheduleDate = new Date(scheduleyear, schedulemonth - 1, scheduleday, 0, 0, 0);
    const req_body = {
      CurrentScheduleId: this.schedule_edit_id,
      ScheduleDate: this.convertDate(scheduleDate),
      StartTime: this.registerSchedule.controls.startTime.value,
      Duration: this.durationValue,
      Comments: this.registerSchedule.controls.scheduleDescription.value,
      IsCompleted: false,
    };

    this.rpm.rpm_post('/api/schedules/updatecurrentschedule', req_body).then(
      () => {
        this.confirmDialog.showConfirmDialog(
          'Worklist Schedule Updated Successfully!!',
          'Message',
          () => this.onScheduleUpdateSuccess(),
          false
        );
      },
      () => this.showWarning('Something Went Wrong', 'Error')
    );
  }


  private updateRecurringSchedule() {
    this.setFrequencyDaySelections();

    if (!this.registerSchedule.valid) return this.showWarning('Please complete the form');
    if (!this.validateFrequencyAndDays()) return;

    if (!this.schedule_careteam_id)
      return this.showWarning('Please select a Assignee Name.');

    const startDate = this.convertDate(this.registerSchedule.controls.startDate.value);
    const endDate = this.convertDate(this.registerSchedule.controls.endDate.value);

    if (endDate <= startDate)
      return this.showWarning('Please select a valid Start Date and End Date.');

    this.userId = sessionStorage.getItem('userid');

    const req_body = {
      Id: this.ScheduleDatabyId.Id,
      AssignedTo: parseInt(this.ScheduleDatabyId.AssignedTo),
      ScheduleTypeId: parseInt(this.registerSchedule.controls.scheduleType.value),
      Schedule: this.registerSchedule.controls.frequency.value,
      Comments: this.registerSchedule.controls.scheduleDescription.value,
      StartDate: startDate,
      EndDate: endDate,
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

    this.rpm.rpm_post('/api/schedules/updateschedule', req_body).then(
      () => {
        this.confirmDialog.showConfirmDialog(
          'Worklist Schedule Updated Successfully!!',
          'Message',
          () => this.onScheduleUpdateSuccess(),
          false
        );
      },
      () => this.showWarning('Something Went Wrong', 'Error')
    );
  }


  private showWarning(message: string, title: string = 'Warning') {
    this.confirmDialog.showConfirmDialog(message, title, () => null, false);
  }

  private onScheduleUpdateSuccess() {
    this.update_schedule_variable = false;
    this.registerSchedule.reset();
    this.scheduleTableReload();
    this.getMenuChioce(1);
    this.checkedSingle = true;
    this.checkedSeries = false;
    this.onClickCancel();
    this.durationValue = 0;
  }

  private setFrequencyDaySelections() {
    const controls = this.registerSchedule.controls;

    if (this.frequencyValue === 'Daily') {
      this.setAllDays(true);
    } else if (this.frequencyValue === 'Weekly') {
      this.setWeekDays([
        controls.scheduleMonday.value,
        controls.scheduleTuesday.value,
        controls.scheduleWednesday.value,
        controls.scheduleThursday.value,
        controls.scheduleFriday.value,
        controls.scheduleSaturday.value,
        controls.scheduleSunday.value,
      ]);
    } else if (this.frequencyValue === 'Monthly') {
      this.weekSelectionValue = controls.weekSelectionFrequency.value;
      this.setWeekDays([
        controls.scheduleSubMonday.value,
        controls.scheduleSubTuesday.value,
        controls.scheduleSubWednesday.value,
        controls.scheduleSubThursday.value,
        controls.scheduleSubFriday.value,
        controls.scheduleSubSaturday.value,
        controls.scheduleSubSunday.value,
      ]);
    }
  }

  private setAllDays(value: boolean) {
    this.dayMonSelectionValue =
      this.dayTueSelectionValue =
      this.dayWedSelectionValue =
      this.dayThurSelectionValue =
      this.dayFriSelectionValue =
      this.daySatSelectionValue =
      this.daySunSelectionValue =
        value;
  }

  private setWeekDays(values: (boolean | undefined)[]) {
    [
      this.dayMonSelectionValue,
      this.dayTueSelectionValue,
      this.dayWedSelectionValue,
      this.dayThurSelectionValue,
      this.dayFriSelectionValue,
      this.daySatSelectionValue,
      this.daySunSelectionValue,
    ] = values.map((val) => !!val);
  }

  private validateFrequencyAndDays(): boolean {
    const freq = this.registerSchedule.controls.frequency.value;
    const daysSelected =
      this.dayMonSelectionValue ||
      this.dayTueSelectionValue ||
      this.dayWedSelectionValue ||
      this.dayThurSelectionValue ||
      this.dayFriSelectionValue ||
      this.daySatSelectionValue ||
      this.daySunSelectionValue;

    if (!freq) {
      this.showWarning('Please select Frequency...!');
      return false;
    }

    if (freq === 'Monthly' && (this.weekSelectionValue === undefined || this.weekSelectionValue === null)) {
      this.showWarning('Please select Week of the Month...!');
      return false;
    }

    if (!daysSelected) {
      this.showWarning('Please select a day of Week...!');
      return false;
    }

    return true;
  }

  keyword_symptoms = 'Symptoms';
  Symptoms_Id: any;
  Symptoms_List: any;
  Symptoms_UserType: any;

  selectEvent_symptoms(item: any) {
    if (typeof item != 'string') {
      this.Symptoms_List = item.Symptoms;
    }
  }

  onChangeSearch_symptoms(val: string) {
    this.Symptoms_List = val;
  }

  onFocused_symptoms(e: any) {}
  clearEventschedule(e: any) {
    this.Symptoms_List = null;
  }
  onFocused_schedule(e: any) {}

  update_symptom_list() {
    var that = this;
    that.rpm.rpm_get(`/api/patient/getsymptomsmasterdata`).then((data) => {
      that.optionData = data;
    });
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
    } else if (
      event.target.name == 'singleOccurance' &&
      event.target.checked == false
    ) {
      this.checkedSingle = false;
      this.checkedSeries = true;
    }

    if (
      event.target.name == 'seriesOccurance' &&
      event.target.checked == true
    ) {
      this.checkedSeries = true;
      this.checkedSingle = false;
    } else if (
      event.target.name == 'seriesOccurance' &&
      event.target.checked == false
    ) {
      this.checkedSeries = false;
      this.checkedSingle = true;
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
        this.convertToLocalTime(this.ScheduleDatabyId.CurrentScheduleDate)
      );
      this.registerSchedule.controls['endDate'].setValue(
        this.convertToLocalTime(this.ScheduleDatabyId.EndDate)
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
        this.convertToLocalTime(this.ScheduleDatabyId.StartDate)
      );
      this.registerSchedule.controls['endDate'].setValue(
        this.convertToLocalTime(this.ScheduleDatabyId.EndDate)
      );
    }
  }

  selfAssignSchedule() {
    var myid = sessionStorage.getItem('userid');
    var myname = sessionStorage.getItem('user_name');

    this.schedule_careteam_name = myname;
    this.schedule_careteam_id = myid;
    this.self_assignee_variable = true;
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

  convertToLocalTime(stillUtc: any) {
    stillUtc = stillUtc + 'Z';
    const local = dayjs.utc(stillUtc).local().format('MM-DD-YYYY HH:mm');

    return local;
  }

  convertToLocalTimedisplay(stillUtc: any) {
    stillUtc = stillUtc + 'Z';
    const local = dayjs.utc(stillUtc).local().format('MM-DD-YYYY HH:mm');

    return local;
  }

  checked() {
    return this.QuestionArrayBase.ans.filter((item: { checked: any }) => {
      return item.checked;
    });
  }

  getDataforquestions() {
    this.showNoteModal = true;
  }
  checkedAns: any;
  checkAnsId: any[] = [];
  AnswerIdArray = [];
  MainQuestionReturnArray = Array();

  previewConfirm() {
    this.MainQuestionReturnArray = [];
    if (this.QuestionArrayBase) {
      for (let i = 0; i < this.QuestionArrayBase.length; i++) {
        if (this.QuestionArrayBase[i].AnswerTypes.length > 0) {
          this.checkedAns = this.QuestionArrayBase[i].AnswerTypes.filter(
            (item: { Checked: any }) => {
              return item.Checked;
            }
          );
          var AnswerArray = this.ProcessArray(this.checkedAns);
          this.checkedAns = [];

          if (AnswerArray.length > 0) {
            this.MainQuestionReturnArray.push({
              QuestionId: this.QuestionArrayBase[i].QuestionId,
              Notes: this.QuestionArrayBase[i].Notes,
              AnswersIds: AnswerArray,
            });
          } else {
            this.MainQuestionReturnArray.push({
              QuestionId: this.QuestionArrayBase[i].QuestionId,
              Notes: this.QuestionArrayBase[i].Notes,
              AnswersIds: [],
            });
          }
        }

        if (this.QuestionArrayBase[i].SubQuestions.length > 0) {
          for (
            let j = 0;
            j < this.QuestionArrayBase[i].SubQuestions.length;
            j++
          ) {
            this.checkedAns = this.QuestionArrayBase[i].SubQuestions[
              j
            ].AnswerTypes.filter((item: { Checked: any }) => {
              return item.Checked;
            });
            var AnswerArray = this.ProcessArray(this.checkedAns);
            this.checkedAns = [];

            if (AnswerArray.length > 0) {
              this.MainQuestionReturnArray.push({
                QuestionId:
                  this.QuestionArrayBase[i].SubQuestions[j].QuestionId,
                Notes: this.QuestionArrayBase[i].SubQuestions[j].Notes,
                AnswersIds: AnswerArray,
              });
            } else {
              this.MainQuestionReturnArray.push({
                QuestionId:
                  this.QuestionArrayBase[i].SubQuestions[j].QuestionId,
                Notes: this.QuestionArrayBase[i].SubQuestions[j].Notes,
                AnswersIds: [],
              });
            }
          }
        }
      }
    }
    this.pid = sessionStorage.getItem('PatientId');
    this.cid = sessionStorage.getItem('userid');

    this.currentProgramId = sessionStorage.getItem('ProgramId');
    if (
      this.NoteTypeId != undefined &&
      this.NoteTypeId != 'undefined' &&
      this.NoteTime != undefined
    ) {
      if (this.additionaNotes == undefined) {
        this.additionaNotes = '';
      }

      if (this.noteUpdationVariable == true) {
        var timeDuration =
          this.NoteHours + ':' + this.NoteMinutes + ':' + this.NoteSec;

        var req_body: any = {};
        req_body['Id'] = parseInt(this.noteId);
        req_body['NoteTypeId'] = parseInt(this.NoteTypeId);
        req_body['NoteType'] = 'REVIEW';
        req_body['IsEstablishedCall'] = false;
        req_body['IsCareGiver'] = true;
        req_body['IsCallNote'] = false;
        req_body['Duration'] = this.convertTimeToSec(timeDuration);
        req_body['Notes'] = this.additionaNotes;
        req_body['CompletedByUserId'] = parseInt(this.cid);
        req_body['MainQuestions'] = this.MainQuestionReturnArray;
        req_body['CreatedBy'] = 'sa';
        this.loading_note = true;

        this.rpm.rpm_post('/api/notes/updatenotev1', req_body).then(
          (data) => {
            this.menu_choice = 1;
            this.getMenuChioce(1);
            this.isOpen = false;
            this.loading_note = false;
           this.showNoteModal=false;
            this.confirmDialog.showConfirmDialog(
              'Note Updated Successfully!!',
              'Message',
              () => {
                this.notesReload();

                this.QuestionArrayBase = this.noteMasterData.MainQuestions;
                this.BillingInfoReload();
                this.noteUpdationVariable = false;
                this.dialog.closeAll();
                this.resetCallPanel();

              },
              false
            );

          },
          (err) => {

            this.confirmDialog.showConfirmDialog(
              'Could not Update Note...!!',
              'Error',
              () => {
                this.loading_note = false;

                this.QuestionArrayBase = this.noteMasterData.MainQuestions;
              },
              false
            );
            this.dialog.closeAll();
          }
        );
      } else if (this.noteUpdationVariable == false) {
        var req_body: any = {};
        req_body['PatientId'] = parseInt(this.pid);
        req_body['PatientProgramId'] = parseInt(this.currentProgramId);
        req_body['NoteTypeId'] = parseInt(this.NoteTypeId);
        req_body['NoteType'] = 'REVIEW';
        req_body['IsEstablishedCall'] = false;
        req_body['IsCareGiver'] = true;
        req_body['IsCallNote'] = false;

        req_body['Duration'] = this.convertTimeToSec(this.NoteTime);

        req_body['Notes'] = this.additionaNotes;
        req_body['CompletedByUserId'] = parseInt(this.cid);
        req_body['MainQuestions'] = this.MainQuestionReturnArray;
        req_body['CreatedBy'] = 'sa';
        //New Change 13/07/2023
        req_body['calltype'] = null;

        this.loading_note = true;

        this.rpm.rpm_post('/api/notes/addnotev1', req_body).then(
          (data) => {
            this.showNoteModal=false;
            this.isOpen = false;
            this.loading_note = false;
            this.menu_choice = 1;
            this.getMenuChioce(1);
            this.confirmDialog.showConfirmDialog(
              'New Note Added Successfully!!',
              'Message',
              () => {
                this.QuestionArrayBase = this.noteMasterData.MainQuestions;

                this.notesReload();
                this.resetReviewTimer();
                this.BillingInfoReload();
                this.dialog.closeAll();
                this.resetCallPanel();

              },
              false
            );

          },
          (err) => {
            // this.confirmDialog.showConfirmDialog(
            //   'Could not add Note...!!',
            //   'Error',
            //   () => {
            //     this.QuestionArrayBase = this.noteMasterData.MainQuestions;
            //     this.loading_note = false;
            //   },
            //   false
            // );
            alert('Could not add Note...!!')
            this.dialog.closeAll();
          }
        );
      }
    } else {
      this.loading_note = false;
      alert('Please Complete The Form ');
      this.dialog.closeAll();
    }
  }

  ProcessArray(answerItemArray: any) {
    var AnswerIdArray = Array();
    for (let i = 0; i < answerItemArray.length; i++) {
      AnswerIdArray.push(answerItemArray[i].AnswerId);
    }
    return AnswerIdArray;
  }
  noteUpdationVariable = false;
  getUpdtionReview(data: any) {
    this.noteUpdationVariable = true;
    this.getMenuChioce(2);
  }
  Htmlele: any;

  getReviewNoteUpdation(data: any, patientstatus: any) {
    this.patientStatus = patientstatus;
    if (this.roleId[0].Id == 1 || this.roleId[0].Id == 3) {
      this.isManagerProvider = true;
    } else {
      this.isManagerProvider = false;
    }
    this.getMenuChioce(2);
    this.noteUpdationVariable = true;
    this.noteId = data.Id;

    this.QuestionArrayBase = null;

    this.rpm
      .rpm_get(
        `/api/patient/getpatientnotesbyprogram?ProgramName=${this.patientProgramName}&Type=REVIEW&PatientNoteId=${this.noteId}`
      )
      .then((data) => {
        if (data) {
          this.patientNoteArray(this.patientProgramName);
          this.noteData = data;
          this.NoteTypeId = this.noteData.NoteTypeId;
          var noteDurationString = this.convertSecToTime(
            this.noteData.Duration
          ).split(':');
          this.NoteHours = noteDurationString[0];
          this.NoteMinutes = noteDurationString[1];
          this.NoteSec = noteDurationString[2];
          this.additionaNotes = this.noteData.Notes;
          this.QuestionArrayBase = this.noteData.MainQuestions;
        }
      });
  }
  convertSecToTime(seconds: any) {
    var hs = Math.trunc(seconds / 3600);
    var remh = Math.trunc(seconds % 3600);
    var min = Math.trunc(remh / 60);
    var sec = Math.trunc(remh % 60);
    var rsec = '';
    var rmin = '';
    var rhs = '';
    if (hs < 10) {
      rhs = '0' + hs.toString();
    } else {
      rhs = hs.toString();
    }
    if (sec < 10) {
      rsec = '0' + sec.toString();
    } else {
      rsec = sec.toString();
    }
    if (min < 10) {
      rmin = '0' + min.toString();
    } else {
      rmin = min.toString();
    }
    var result = rhs + ':' + rmin + ':' + rsec;
    return result;
  }
  previewCancel() {
    this.noteUpdationVariable = false;
    this.showNoteModal = false;

    this.resetCallPanel();
    this.getMenuChioce(1);
    this.addMedicationForm.controls.frmpatientName.setValue(this.patientName);
    this.isOpen = false;
    this.callType = null;
  }
  MasterDataQuestionTemp: any;
  getMasterDataQuestions(patientProgramName: any) {
    this.rpm
      .rpm_get(
        `/api/patient/getmasterdatanotes?ProgramName=${patientProgramName}&Type=REVIEW`
      )
      .then(
        (data) => {
          if (data) {
            this.noteMasterData = data;
            this.MasterDataQuestionTemp = this.noteMasterData.MainQuestions;
            sessionStorage.setItem(
              'MasterDataQuestionTemp',
              JSON.stringify(this.MasterDataQuestionTemp)
            );
            this.QuestionArrayBase = this.noteMasterData.MainQuestions;
          }
        },
        (err) => {
          this.noteMasterData = [];
        }
      );

    var that = this;

    patientProgramName = sessionStorage.getItem('patientPgmName');

    if (patientProgramName == 'RPM') {
      that.noteTypeIdArray = that.http_getId.filter(function (data: any) {
        return data.Type == 'PatientNoteType' && data.Name != 'Incoming Call';
      });
    } else if (patientProgramName == 'CCM') {
      that.noteTypeIdArray = that.http_getId.filter(function (data: any) {
        return data.Type == 'CCMNotes' && data.Name != 'Incoming Call';
      });
    } else if (patientProgramName == 'PCM') {
      that.noteTypeIdArray = that.http_getId.filter(function (data: any) {
        return data.Type == 'PCMNotes' && data.Name != 'Incoming Call';
      });
    }
  }

  patientNoteArray(patientProgramName: any) {
    var that = this;

    if (patientProgramName == 'RPM') {
      that.noteTypeIdArray = that.http_getId.filter(function (data: any) {
        return data.Type == 'PatientNoteType' && data.Name != 'Incoming Call';
      });
    } else if (patientProgramName == 'CCM') {
      that.noteTypeIdArray = that.http_getId.filter(function (data: any) {
        return data.Type == 'CCMNotes' && data.Name != 'Incoming Call';
      });
    } else if (patientProgramName == 'PCM') {
      that.noteTypeIdArray = that.http_getId.filter(function (data: any) {
        return data.Type == 'PCMNotes' && data.Name != 'Incoming Call';
      });
    }
  }

  resetCallPanel() {
    this.MasterDataQuestionTemp = sessionStorage.getItem(
      'MasterDataQuestionTemp'
    );
    this.QuestionArrayBase = JSON.parse(this.MasterDataQuestionTemp);

    this.additionaNotes = '';
    this.NoteTypeId = undefined;
  }
  ngOnDestroy() {

    this.patientStatus = null;
  }

  isAllSelected(item: any) {
    for (let i = 0; i < this.QuestionArrayBase.length; i++) {
      if (this.QuestionArrayBase[i].Question == 'Readings Summary Report') {
        var SummaryAnswer = this.QuestionArrayBase[i].AnswerTypes;
        if (this.QuestionArrayBase[i].AnswerTypes.length > 0) {
          SummaryAnswer.forEach((val: { id: any; Checked: boolean }) => {
            if (val.id == item.target.id) val.Checked = !val.Checked;
            else {
              val.Checked = false;
            }
          });
        }
      }
    }
  }

  toogle: boolean = false;
  minimize = {
    height: '0.1rem',
    position: 'absolute',
    top: 0,
    overflow: 'hidden',
    backgroundColor: 'powderblue !important',
  };
  maximize = { height: '90vh' };
  minimizeValue = false;
  minimizeScreen() {
    this.toogle = !this.toogle;
    this.minimizeValue = true;
  }
  maximizeScreen() {
    this.toogle = !this.toogle;
    this.minimizeValue = false;
  }

  ngOnChanges() {
  }
  ConvertToUTC(data: any) {
    var timenow = new Date().toLocaleTimeString();
    var combinedDate = new Date(data + ' ' + timenow);
    var combineDateIsoFrm = new Date(combinedDate.toUTCString()).toISOString();

    return combineDateIsoFrm;
  }

  http_rpm_patientList: any;


  refresh() {
    this.registerSchedule.reset();
    this.registerTask.reset();
    this.registerSymptoms.reset();
  //  this.addMedicationForm.reset();
    this.resetMedication();
    this.getMenuChioce(1);
  }

  //New Change Alert

  keyword_alert_careteam = 'Member';
  alert_careteam_id: any;
  alert_careteam_name: any;

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

    this.alert_careteam_id = val;
  }

  onFocused_alert_careteam(e: any) {

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

  closenoterDialogModal()
  {
    this.showNoteModal=false;
  }

}
