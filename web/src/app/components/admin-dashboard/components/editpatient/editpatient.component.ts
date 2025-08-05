import { Component, OnInit, TemplateRef, ViewChild, ChangeDetectorRef } from '@angular/core';
import { MatDialog, MatDialogConfig } from '@angular/material/dialog';
import {
  UntypedFormControl,
  UntypedFormBuilder,
  UntypedFormGroup,
  Validators,
  UntypedFormArray,
  FormControl,
  FormArray,
  FormGroup,
  AbstractControl,
} from '@angular/forms';
import { RPMService } from '../../sevices/rpm.service';
import { ActivatedRoute, Router } from '@angular/router';
import _ from 'lodash';
import { StatusMessageComponent } from '../../shared/status-message/status-message.component';
import { DatePipe } from '@angular/common';
import * as uuid from 'uuid';
import { AuthService } from 'src/app/services/auth.service';
import { ConfirmDialogServiceService } from '../../shared/confirm-dialog-panel/service/confirm-dialog-service.service';
import { Subscription } from 'rxjs';
import dayjs from 'dayjs';
import utc from 'dayjs/plugin/utc';
import timezone from 'dayjs/plugin/timezone';

dayjs.extend(utc);
dayjs.extend(timezone);
export interface document {
  type: string;
  name: string;
  date: string;
  document: string;
}

const Element_Data: document[] = [
  {
    type: 'PatientConsentForm',
    name: 'Signed document from patient',
    date: 'Aug24,2021',
    document: '',
  },
];
@Component({
  selector: 'app-editpatient',
  templateUrl: './editpatient.component.html',
  styleUrls: ['./editpatient.component.scss'],
})
export class EditpatientComponent implements OnInit {
  @ViewChild('programedit', { static: true }) programedit: TemplateRef<any>;

  display = false;
  dataSource: any = [];
  diagonisticsData: any = [];
  patient_variable: any;
  program_variable: any;
  loginForm: UntypedFormGroup;
  programForm: UntypedFormGroup;
  isOpen = false;
  editvariable = false;
  patient_id: any;
  program_id: any;
  master_data: any;
  loading: boolean;
  PatientInfoForm: UntypedFormGroup;
  resp: any;
  pid: any;
  states: any;
  Clinic_Selected: any;
  Program_Selected: any;
  vital_selected: any;
  Goals_Selected: any;
  Diagnosis_Name: any;
  Diagnosis_code_selected: any;
  state_selected: any;
  CityAndStates: any;
  patientprogramid: any;
  StatesAndCities: any;
  cities: any;
  programForm_2: UntypedFormGroup;
  PatientVitalInfos: any;
  groupedVitalScheduleLists: {
    [x: string]: Pick<any, string | number | symbol>[];
  };
  programForm_3: UntypedFormGroup;
  primaryinsurnace: any;
  secondaryinsurnace: any;
  patientdevicedetails: any;
  DiagnosisInfos: any;
  failureTemplate: TemplateRef<any>;
  successTemplate: TemplateRef<any>;
  EnrolmentDetails: any;
  ActiveDetails: any;
  devArr: any;
  Diagnosis_List: any = [];
  careTeamMembers: any;
  programDetails: any;
  timezones: any;
  add_patient_masterdata: unknown;

  deviceName: any;
  measurementName: any;
  unitName: any;
  vitalName: any;
  vitalMeasureName: any;
  Doc: any;
  file: any;
  image: any;
  ReadyToDischarge: any;
  OnHold: any;
  Discharged: any;
  InActive: any;
  cname: any;
  ccode: any;
  cid: any;
  DownloadStatus = false;
  programedit_masterdata: any;
  editProgramGoal: any;
  editProgramDiagoList: any;
  ediProgramSelectedId: any;
  diaganosisMainList: any;
  editProgramVital: any;
  last_status: any;

  vitalList: number[] = [];
  selectedvitalList: number[] = [];
  vitalListDialog: number[] = [];
  oldVitalListDialog: number[] = [];
  editvitalList = [];
  documentType: any;
  public showDialog = false;
  public confirmDialogTitle: string;
  public confirmDialogMessage: string;
  confirmAction: (() => void) | null = null;
  vitallist = [];
  matchingVitals: number[] = [];
  selectedProgramId: number;
  selectedProgram: false;
  initiallySelectedVitals: number[] = [];
  programVitals: any;
  initialFormValues: any;
  allVitalsSelected = false;
  private programNameValueChangesSubscription: Subscription | undefined;
  constructor(
    private _route: ActivatedRoute,
    public dialog: MatDialog,
    private PatientFormBuilderObj: UntypedFormBuilder,
    private rpm: RPMService,
    private router: Router,
    public datepipe: DatePipe,
    private Auth: AuthService,
    private showconfirmDialog: ConfirmDialogServiceService,
    private fb: UntypedFormBuilder,
    private cdRef: ChangeDetectorRef
  ) {
    var that = this;
    this.initForms();
    this.intializeImages();
    this.ImageStatus = false;
    this.discharged_img = this.unbold_icon;
    this.diaganosisMainList = [{ DiagnosisName: '', DiagnosisCode: '' }];
    this.selectedDiagnoses = [{ DiagnosisName: '', DiagnosisCode: '' }];
    this.editProgramdata = this.fb.group({
      programname: ['', Validators.required],
      vitalListDialog: [[], Validators.required],
      selectedDiagnoses: this.fb.array([]),
    });
    this.diagnosisFormGroups = this.selectedDiagnoses.map(() => this.fb.group({
      diagnosis: [''],
      DiagnosisCode: ['']
    }));

  }

  getInitialValue(diagnosis: any): string {
    return diagnosis?.DiagnosisName || '';
  }

  get diagnosisList() {
    return this.editProgramdata.get('selectedDiagnoses') as FormArray;
  }

  addDiagnosis(diagnosis = { diagnosisName: '', diagnosisCode: '' }) {
    const group = this.fb.group({
      diagnosisName: [diagnosis.diagnosisName],
      diagnosisCode: [{ value: diagnosis.diagnosisCode, disabled: true }],
    });
    this.diagnosisList.push(group);
  }

  getPhysicianList() {
    this.physicianList = this.master_data.PhysicianDetails.filter(
      (phy: { ClinicID: any }) =>
        phy.ClinicID === parseInt(this.resp.OrganizationID)
    );
  }
  private markFormGroupTouched(formGroup: UntypedFormGroup) {
    (<any>Object).values(formGroup.controls).forEach((control: UntypedFormGroup) => {
      control.markAsTouched();

      if (control.controls) {
        this.markFormGroupTouched(control);
      }
    });
  }

  scheduleChanged(schedule: any) {
    // var res;
    // for(let vital of  this.PatientVitalInfos){
    //   if(vital.VitalName=='Blood Glucose'){
    //     if(schedule=='3'){//'Before Meal'){
    //       res = vital.VitalMeasureInfos.filter((data: { MeasureName: string; })=>{
    //         return data.MeasureName=='Glucose before meal'
    //       })
    //     }
    //     else if(schedule=='4'){//'After Meal'){
    //       res = vital.VitalMeasureInfos.filter((data: { MeasureName: string; })=>{
    //         return data.MeasureName=='Glucose before meal'
    //       })
    //     }
    //     else{//} if(schedule=='Before & After Meal'){
    //       var tmp=this.PatientVitalInfos_copy.filter((data: { VitalName: string; })=>{
    //         return data.VitalName=='Blood Glucose';
    //       })
    //       res = tmp[0].VitalMeasureInfos
    //     }
    //     vital.VitalMeasureInfos = res
    //     console.log(res);
    //     // this.PatientVitalInfos.VitalMeasureInfos= res;
    //     // console.log( this.PatientVitalInfos.VitalMeasureInfos)
    //   }
    // }
  }
  rolelist: any;
  deviceTypeDataId: any;
  programnamechangefirst = true;
  programnamechangefirst_1 = true;
  programnamechangefirst_2 = true;
  cityFlag: any;
  diagnosisFormGroups: FormGroup[] = [];
  ngOnInit(): void {
    this.variable = 1;
    this.documentAddFlag = false;
    this.programnamechangefirst = true;
    this.cityFlag = false;
    this.diagnosisFormGroups = this.selectedDiagnoses.map((d: { DiagnosisName: any; DiagnosisCode:any;}) =>
      new FormGroup({
        diagnosis: new FormControl(d.DiagnosisName),
        DiagnosisCode: new FormControl(d.DiagnosisCode)
      })
    );
    // Edit Program
    var that = this;
    this.getLanguage();

    this.editProgramdata.get('programname')?.valueChanges.subscribe((val) => {
      var ProgramSelected = that.master_data.ProgramDetailsMasterData.filter(
        (Pgm: { ProgramId: number }) => Pgm.ProgramId === parseInt(val)
      );
      if (ProgramSelected.length > 0) {
        this.ediProgramSelectedId = ProgramSelected;
        that.editProgramVital = this.ediProgramSelectedId[0].Vitals;
        if (!this.programnamechangefirst) {
          //that.vitalList = [];
        }

        that.editProgramGoal = ProgramSelected[0].goalDetails;
        that.editProgramDiagoList = ProgramSelected[0].Vitals[0].Dignostics;
        for (let x of this.editProgramDiagoList) {
          x.DiagnosisFieldEdit = x.DiagnosisName;
        }
      }
      this.diaganosisMainList = [{ DiagnosisName: '', DiagnosisCode: '' }];
      this.selectedDiagnoses = [{ DiagnosisName: '', DiagnosisCode: '' }];
      this.programnamechangefirst = false;
    });

    this.PatientInfoForm.get('clinicname')?.valueChanges.subscribe((val) => {
      that.Clinic_Selected = that.master_data.ClinicDetails.filter(
        (clinic: { Id: any }) => clinic.Id === parseInt(val)
      );
      if (that.Clinic_Selected.length > 0) {
        that.PatientInfoForm.controls['cliniccode'].setValue(
          that.Clinic_Selected[0].ClinicCode
        );
        that.programForm.controls['cliniccode'].setValue(
          that.Clinic_Selected[0].ClinicCode
        );
      }

      this.getPhysicianList();
      this.getLastPatientStatus();
    });

    this.programForm.get('startdate')?.valueChanges.subscribe((val) => {
      if (this.programForm.controls.startdate.value) {
        this.calculateEndDate();
      }
    });

    this.programForm.get('programname')?.valueChanges.subscribe((value) => {
      that.Program_Selected = that.master_data.ProgramDetailsMasterData.filter(
        (Pgm: { ProgramId: number }) => Pgm.ProgramId === parseInt(value)
      );
      console.log('Program Selected:');
      console.log(that.Program_Selected);
      if (that.Program_Selected.length > 0) {
        that.vital_selected = that.Program_Selected[0].Vitals;
      }
      if (!this.programnamechangefirst_1) {
        //that.vitalList = [];
      }
      this.programnamechangefirst_1 = false;
      // this.vitalList=[];
    });
    this.programForm.get('diagnosisData')?.valueChanges.subscribe((val) => {
      that.Diagnosis_code_selected =
        that.Program_Selected[0].Vitals[0].Dignostics.filter(
          (diagnosis: { DiagnosisName: any }) =>
            diagnosis.DiagnosisName == val[0].DiagnosisName
        );
      that.diagonisticsData = that.Diagnosis_code_selected[0].DiagnosisCode;
    });

    this.PatientInfoForm.get('state')?.valueChanges.subscribe((data) => {
      if (this.cityFlag == true) {
        this.PatientInfoForm.get('city')?.reset();
      }
      that.cities = that.StatesAndCities.Cities.filter(
        (c: { StateId: number }) => c.StateId === parseInt(data)
      );
      this.cityFlag = true;
    });
    this.loading = true;
    var that = this;
    this.rolelist = sessionStorage.getItem('Roles');
    this.rolelist = JSON.parse(this.rolelist);
    this.rpm
      .rpm_get(
        '/api/patient/getprogramdetailsmasterdataaddpatient?RoleId=' +
          this.rolelist[0].Id
      )
      .then((data) => {
        that.add_patient_masterdata = data;

        sessionStorage.setItem(
          'add_patient_masterdata',
          JSON.stringify(that.add_patient_masterdata)
        );
      });
    that.master_data = sessionStorage.getItem('add_patient_masterdata');
    that.master_data = JSON.parse(that.master_data);
    that.CommunicationType = that.master_data?.DeviceCommunicationTypes;
    that.deviceTypeDataId = 1;

    this.devArr = [];
    this.careTeamMembers = this.master_data.CareTeamMembers;
    this.programDetails = this.master_data.ProgramDetailsMasterData;
    this.programDetails = this.master_data.ProgramDetailsMasterData.filter((program: { Name: string; Vitals: string | any[]; }) =>
      program.Name === "RPM" && program.Vitals.length > 1
    );
    var myuser = sessionStorage.getItem('user_name');
    var myid = sessionStorage.getItem('userid');

    const uniqueValuesSet = new Set();
    this.devArr = this.master_data.DeviceDetails.filter((obj: any) => {
      // check if name property value is already in the set
      const isPresentInSet = uniqueValuesSet.has(obj.DeviceName);
      uniqueValuesSet.add(obj.DeviceName);

      // return the negated value of
      // isPresentInSet variable
      return !isPresentInSet;
    });
    this.deviceCommunicationTypeChange();
    that.insuracevendors = that.master_data.InsurenceDetails;

    this._route.queryParams.subscribe((params) => {
      that.GetPatientInfo(params.id, params.programId);

      that.pid = params.id;
      that.patientprogramid = params.programId;
    });
  }

  deviceCommunicationTypeChange() {
    var that = this;
    that.master_data = sessionStorage.getItem('add_patient_masterdata');
    that.master_data = JSON.parse(that.master_data);
    that.deviceList = that.master_data.DeviceDetails;
    this.deviceVitalLists();
  }
  insuracevendors: any;
  private initForms() {
    this.PatientInfoForm = this.PatientFormBuilderObj.group({
      firstname: new UntypedFormControl('', [Validators.required]),
      middlename: new UntypedFormControl(null),
      dateofbirth: new UntypedFormControl('', [Validators.required]),
      gender: new UntypedFormControl('', [Validators.required]),
      lastname: new UntypedFormControl('', [Validators.required]),
      email: new UntypedFormControl('', [Validators.email]),
      mobile: new UntypedFormControl('', [
        Validators.required,
        Validators.pattern('[- +()0-9]+'),

        Validators.minLength(10),
        Validators.maxLength(10),
      ]),
      alternatenumber: new UntypedFormControl(null),
      addressline1: new UntypedFormControl('', [Validators.required]),
      addressline2: new UntypedFormControl(null),
      zipcode: new UntypedFormControl('', [Validators.required]),
      city: new UntypedFormControl('', [Validators.required]),
      state: new UntypedFormControl('', [Validators.required]),
      calltime: new UntypedFormControl(null),
      timezone: new UntypedFormControl(null),
      emergencycontact1: new UntypedFormControl(null),
      emergencycontact2: new UntypedFormControl(null),
      relation1: new UntypedFormControl(null),
      relation1_mobile: new UntypedFormControl(null),
      relation2: new UntypedFormControl(null),
      relation2_mobile: new UntypedFormControl(null),
      language: new UntypedFormControl('', [Validators.required]),
      preference2: new UntypedFormControl(null),
      preference3: new UntypedFormControl(null),
      preference4: new UntypedFormControl(null),
      clinicname: new UntypedFormControl('', [Validators.required]),
      cliniccode: new UntypedFormControl({ value: '' }),
      additionalnotes: new UntypedFormControl(''),
    });

    this.programForm = this.PatientFormBuilderObj.group({
      programname: new UntypedFormControl('', [Validators.required]),
      programduration: new UntypedFormControl('', [Validators.required]),
      vitals: new UntypedFormControl({ value: '' }),
      startdate: new UntypedFormControl('', [Validators.required]),
      enddate: new UntypedFormControl('', [Validators.required]),
      patientstatus: new UntypedFormControl('', [Validators.required]),
      physician: new UntypedFormControl('', [Validators.required]),
      diagnosisDataList: this.PatientFormBuilderObj.array([
        this.createDiagnosisGroup(),
      ]),
      deviceDataList: this.PatientFormBuilderObj.array([
        this.createDeviceGroup(),
      ]),
      vitalScheduleList: this.PatientFormBuilderObj.array([
        this.createVitalScheduleGroup(),
      ]),
      clinic: new UntypedFormControl({ value: '' }, [Validators.required]),
      cliniccode: new UntypedFormControl({ value: '' }),
      consultdate: new UntypedFormControl('',null),
      assignedMember: new UntypedFormControl('', [Validators.required]),
      PrescribedDate: new UntypedFormControl({ value: '' }, [Validators.required]),
      EntrolledDate: new UntypedFormControl({ value: '' }, [Validators.required]),
    });
    this.programForm_2 = this.PatientFormBuilderObj.group({
      deviceDataList: this.PatientFormBuilderObj.array([
        this.createDeviceGroup(),
      ]),
      vitalScheduleList: this.PatientFormBuilderObj.array([
        this.createVitalScheduleGroup(),
      ]),
      insurance1: new UntypedFormControl('', [Validators.required]),
      insurance2: new UntypedFormControl(''),
      insurance3: new UntypedFormControl(''),
      billingcode1: new UntypedFormControl(''),
      billingcode2: new UntypedFormControl(''),
      billingcode3: new UntypedFormControl(''),
      billingcode4: new UntypedFormControl(''),
    });

    this.programForm_3 = this.PatientFormBuilderObj.group({
      deviceDataList: this.PatientFormBuilderObj.array([
        this.createDeviceGroup(),
      ]),
      vitalScheduleList: this.PatientFormBuilderObj.array([
        this.createVitalScheduleGroup(),
      ]),
      insurance1: new UntypedFormControl('', [Validators.required]),
      insurance2: new UntypedFormControl(''),
      insurance3: new UntypedFormControl(''),
      billingcode1: new UntypedFormControl(''),
      billingcode2: new UntypedFormControl(''),
      billingcode3: new UntypedFormControl(''),
      billingcode4: new UntypedFormControl(''),
    });
    this.programForm.controls['enddate'].disable();
  }
  createGoalsGroup(): UntypedFormGroup {
    return this.PatientFormBuilderObj.group({
      Goal: ['', [Validators.required]],
      Description: ['', [Validators.required]],
    });
  }
  createDiagnosisGroup(): UntypedFormGroup {
    return this.PatientFormBuilderObj.group({
      DiagnosisName: ['', [Validators.required]],
      DiagnosisCode: ['', [Validators.required]],
    });
  }
  createDeviceGroup(): UntypedFormGroup {
    return this.PatientFormBuilderObj.group({
      vitalmonitoring: ['', [Validators.required]],
      deviceid: ['', [Validators.required]],
      devicevitaltype: ['', [Validators.required]],
      devicetype: ['', [Validators.required]],
      devicestatus: ['', [Validators.required]],
    });
  }
  createVitalScheduleGroup() {
    return this.PatientFormBuilderObj.group({
      vital: ['', [Validators.required]],
    });
  }
  get goalList(): UntypedFormArray {
    return <UntypedFormArray>this.programForm.get('goalList');
  }
  get diagnosisDataList(): UntypedFormArray {
    return <UntypedFormArray>this.programForm.get('diagnosisDataList');
  }
  get deviceDataList(): UntypedFormArray {
    return <UntypedFormArray>this.programForm.get('deviceDataList');
  }
  get vitalScheduleList(): UntypedFormArray {
    return <UntypedFormArray>this.programForm.get('vitalScheduleList');
  }
  addNewGoal() {
    this.goalList.push(this.createGoalsGroup());
  }

  setGoalsSelected() {
    var program = this.master_data.ProgramDetailsMasterData.filter(
      (Program: { ProgramId: any }) =>
        Program.ProgramId ===
        parseInt(this.Patientdata.PatientProgramdetails.ProgramId)
    );
    this.Goals_Selected = this.Patientdata.PatientProgramGoals.goalDetails;
    this.Diagnosis_code_selected =
      this.Patientdata.PatientPrescribtionDetails.PatientDiagnosisInfos;
    this.vital_selected =
      this.Patientdata.PatientProgramdetails.PatientVitalInfos;
    this.master_data.VitalScheduleLists.filter(
      (Program: { ProgramId: any }) =>
        Program.ProgramId ===
        parseInt(this.Patientdata.PatientProgramdetails.VitalId)
    );
    this.preSetGoals();
    this.preSetDiagnosis();
  }
  preSetGoals() {
    this.goalList.clear();
    for (let g of this.Patientdata.PatientProgramGoals.goalDetails) {
      var goal = this.PatientFormBuilderObj.group({
        Goal: [g.Goal, [Validators.required]],
        Description: [g.Description, [Validators.required]],
      });
      this.goalList.push(goal);
    }
  }
  preSetDiagnosis() {
    this.diagnosisDataList.clear();
    for (let g of this.Patientdata.PatientPrescribtionDetails
      .PatientDiagnosisInfos) {
      var diagnosis = this.PatientFormBuilderObj.group({
        DiagnosisName: [g.DiagnosisName, [Validators.required]],
        DiagnosisCode: [g.DiagnosisCode, [Validators.required]],
      });
      this.diagnosisDataList.push(diagnosis);
    }
  }
  preSetVitalSchedules() {
    this.vitalScheduleList.clear();
    for (let g of this.Patientdata.PatientProgramdetails.PatientVitalInfos) {
      var vitalSchedule = this.PatientFormBuilderObj.group({
        vital: [g.VitalId, [Validators.required]],
      });
      this.vitalScheduleList.push(vitalSchedule);
    }
  }
  onControlChange(event: any, itemindex: number) {
    if (event.srcElement) {
      const controlName = event.srcElement.name;
      if (controlName == 'vital') {
        const particularVitals = this.vitalScheduleList;
        const particularVitalFormGroup = particularVitals.controls[
          itemindex
        ] as UntypedFormGroup;
      }
    }
  }
  makeParagraphsofDescription(desc: any) {
    var arr = desc.split(',');
    var r = '';
    for (let ele of arr) {
      r = r + ' -' + ele + '\n';
    }
    return r;
  }
  monthDiff(d1: any, d2: any) {
    var months;
    var c1 = new Date(d1);
    var c2 = new Date(d2);
    months = (c2.getFullYear() - c1.getFullYear()) * 12;
    months -= c1.getMonth();
    months += c2.getMonth();
    return months <= 0 ? 0 : months;
  }
  Patientdata: any;
  physicianList: any;
  PatientVitalInfos_copy: any;
  DeviceNotAvaliable = true;
  languageSelected: any;
  GetPatientInfo(PatientId: any, ProgramId: any) {
    //call on clicking on one patient row in the draft list, pass patient id from table to get the info
    var that = this;
    that.rpm
      .rpm_get(
        '/api/patient/getpatient?PatientId=' +
          PatientId +
          '&PatientprogramId=' +
          ProgramId
      )
      .then((data) => {
        that.Patientdata = data;
        this.processMasterData();
        that.PatientVitalInfos =
          that.Patientdata.PatientVitalDetails.PatientVitalInfos;
        this.vital_selected =
          this.Patientdata.PatientProgramdetails.PatientVitalInfos;

        let vitalArray = that.vital_selected.filter(
          (ds: { Selected: boolean }) => ds.Selected == true
        );

        that.vitalList = vitalArray.map((item: any, index: any, arr: any) => {
          return item.VitalId;
        });
        console.log('Vital List Selected Vital Array',that.vitalList);
        that.vitalListDialog = vitalArray.map((item: any, index: any, arr: any) => {
          return item.VitalId;
        });
        this.selectedProgram = this.selectedProgram;
        this.initiallySelectedVitals = [...this.vitalListDialog];
        this.allVitalsSelected = this.programVitals.every((vital: { VitalId: number; }) =>
          this.vitalListDialog.includes(vital.VitalId)
        );

        that.datasourceDocument =
          that.Patientdata.PatientDocumentDetails.PatientDocumentinfos;
        that.groupedVitalScheduleLists = _.mapValues(
          _.groupBy(this.master_data.VitalScheduleLists, 'VitalId'),
          (vlist) => vlist.map((vital) => _.omit(vital, 'VitalId'))
        );
        that.resp = that.Patientdata.PatientDetails;
        that.master_data = sessionStorage.getItem('add_patient_masterdata');
        that.master_data = JSON.parse(that.master_data);
        that.StatesAndCities = sessionStorage.getItem('states_cities');
        that.StatesAndCities = JSON.parse(that.StatesAndCities);
        that.timezones = this.StatesAndCities.TimeZones;
        that.status_to_send = that.Patientdata.PatientProgramdetails.Status;
        that.current_status_value =
          that.Patientdata.PatientProgramdetails.Status;

        that.ChangePatientStatus(
          that.Patientdata.PatientProgramdetails.Status,
          1
        );

        that.loading = false;
        that.PatientInfoForm.controls['firstname'].setValue(
          that.resp.FirstName
        );
        that.PatientInfoForm.controls['middlename'].setValue(
          that.resp.MiddleName
        );
        that.PatientInfoForm.controls['lastname'].setValue(that.resp.LastName);
        that.PatientInfoForm.controls['dateofbirth'].setValue(
          that.convertDate(that.resp.DOB)
        );
        that.PatientInfoForm.controls['gender'].setValue(that.resp.Gender);
        that.PatientInfoForm.controls['email'].setValue(that.resp.Email);
        that.PatientInfoForm.controls['mobile'].setValue(that.resp.MobileNo);
        that.PatientInfoForm.controls['alternatenumber'].setValue(
          that.resp.AlternateMobNo
        );
        that.PatientInfoForm.controls['addressline1'].setValue(
          that.resp.Address1
        );
        that.PatientInfoForm.controls['addressline2'].setValue(
          that.resp.Address2
        );
        that.PatientInfoForm.controls['zipcode'].setValue(that.resp.ZipCode);
        that.PatientInfoForm.controls['city'].setValue(that.resp.CityId, {
          onlySelf: true,
        });
        that.PatientInfoForm.controls['state'].setValue(that.resp.StateId, {
          onlySelf: true,
        });
        that.PatientInfoForm.controls['timezone'].setValue(
          that.resp.TimeZoneID,
          { onlySelf: true }
        );
        that.PatientInfoForm.controls['emergencycontact1'].setValue(
          that.resp.Contact1Name
        );
        that.PatientInfoForm.controls['relation1'].setValue(
          that.resp.Contact1RelationName
        );
        that.PatientInfoForm.controls['relation1_mobile'].setValue(
          that.resp.Contact1Phone
        );
        that.PatientInfoForm.controls['emergencycontact2'].setValue(
          that.resp.Contact2Name
        );
        that.PatientInfoForm.controls['relation2'].setValue(
          that.resp.Contact2RelationName
        );
        that.PatientInfoForm.controls['relation2_mobile'].setValue(
          that.resp.Contact2Phone
        );
        that.PatientInfoForm.controls['calltime'].setValue(that.resp.CallTime);
        that.PatientInfoForm.controls['language'].setValue(that.resp.Language);
        that.PatientInfoForm.controls['preference2'].setValue(
          that.resp.Preference1
        );
        that.PatientInfoForm.controls['preference3'].setValue(
          that.resp.Preference2
        );
        that.PatientInfoForm.controls['preference4'].setValue(
          that.resp.Preference3
        );
        that.PatientInfoForm.controls['additionalnotes'].setValue(
          that.resp.Notes
        );

        that.getPhysicianList();
        that.Clinic_Selected = that.master_data.ClinicDetails.filter(
          (clinic: { Id: any }) =>
            clinic.Id === parseInt(that.resp.OrganizationID)
        );
        if (that.Clinic_Selected.length > 0) {
          this.cname = that.Clinic_Selected[0].ClinicName;
          this.ccode = that.Clinic_Selected[0].ClinicCode;
          this.cid = that.Clinic_Selected[0].Id;
          that.programForm.controls['cliniccode'].setValue(this.ccode);
        }
        that.PatientInfoForm.controls['clinicname'].setValue(this.cname);
        that.PatientInfoForm.controls['cliniccode'].setValue(this.ccode);

        var patientHeighArray = that.resp.Height.toString().split('.');

        if (patientHeighArray.length > 1) {
          that.HeightInchValue = patientHeighArray[1];
        } else {
          that.HeightInchValue = '00';
        }

        if (patientHeighArray.length > 0) {
          that.HeightFeetValue = patientHeighArray[0];
        } else {
          that.HeightFeetValue = '0';
        }

        that.weightValue = that.resp.Weight;
        that.Goals_Selected = that.Patientdata.PatientProgramGoals.goalDetails;
        that.Diagnosis_List =
          that.Patientdata.PatientPrescribtionDetails.PatientDiagnosisInfos;
        that.pid = PatientId;
        that.programForm.controls['programname'].setValue(
          that.Patientdata.PatientProgramdetails.ProgramId,
          { onlySelf: true }
        );
        that.durationValue = that.Patientdata.PatientProgramdetails.Duration;
        that.programForm.controls['startdate'].setValue(
          that.convertDate(
            this.convertToLocalTime(
              that.Patientdata.PatientProgramdetails.StartDate
            )
          )
        );
        that.programForm.controls['enddate'].setValue(
          that.convertDate(that.Patientdata.PatientProgramdetails.EndDate)
        );

        that.programForm.controls['assignedMember'].setValue(
          that.Patientdata.PatientProgramdetails.CareTeamUserId,
          { onlySelf: true }
        );
        that.programForm.controls['patientstatus'].setValue(
          that.Patientdata.PatientPrescribtionDetails.Status,
          { onlySelf: true }
        );
        that.programForm.controls['physician'].setValue(
          that.Patientdata.PatientPrescribtionDetails.PhysicianId,
          { onlySelf: true }
        );

        that.programForm.controls['clinic'].setValue(
          that.Patientdata.PatientPrescribtionDetails.Clinic
        );
        var consultDateData = this.datepipe.transform(
          that.Patientdata.PatientPrescribtionDetails.ConsultationDate,
          'MMM/dd/yyyy'
        );
          this.programForm.patchValue({
          consultdate: consultDateData &&
                      consultDateData !== '1/1/1970 12:00:00 AM'
            ? this.convertDate(consultDateData)
            : null
        });
        // Manjusha code change
        // Parse the custom date format "dd-MM-yyyy HH:mm:ss"
        const rawDate = that.Patientdata.PatientPrescribtionDetails.ConsultationDate;

         var  ConsultationDateIso  = new Date(rawDate);
       // Convert to ISO string
      const isoString = ConsultationDateIso.toISOString();
      that.programForm.controls['consultdate'].setValue(
          this.convertDate(isoString)
       ); 
        var prescribeDateData = this.datepipe.transform(
          this.convertToLocalTime(
            that.Patientdata.PatientPrescribtionDetails.PrescribedDate
          ),
          'MMM/dd/yyyy'
        );

        that.programForm.controls['PrescribedDate'].setValue(
          this.convertDate(prescribeDateData)
        );

        that.programForm.controls['EntrolledDate'].setValue(
          that.convertDate(
            this.convertToLocalTime(
              that.Patientdata.PatientEnrolledDetails.AssignedDate
            )
          )
        );
        if (this.Patientdata.PatientProgramdetails.ProgramName == 'RPM') {
          that.programForm_3.controls['billingcode1'].setValue(
            that.master_data.BillingCodes[0].BillingCode +
              ' - ' +
              that.master_data.BillingCodes[0].Description
          );
          that.programForm_3.controls['billingcode2'].setValue(
            that.master_data.BillingCodes[1].BillingCode +
              ' - ' +
              that.master_data.BillingCodes[1].Description
          );
          that.programForm_3.controls['billingcode3'].setValue(
            that.master_data.BillingCodes[2].BillingCode +
              ' - ' +
              that.master_data.BillingCodes[2].Description
          );
          that.programForm_3.controls['billingcode4'].setValue(
            that.master_data.BillingCodes[3].BillingCode +
              ' - ' +
              that.master_data.BillingCodes[3].Description
          );
        } else {
          switch (this.program_name) {
            case 'CCM-C':
              that.codeSelected = [];
              that.codeSelected = that.master_data.BillingCodes.filter(
                (code: { BillingCode: any }) => code.BillingCode === 'G0506'
              );
              if (that.codeSelected.length > 0) {
                that.programForm_3.controls['billingcode1'].setValue(
                  that.codeSelected[0].BillingCode +
                    ' - ' +
                    that.codeSelected[0].Description
                );
              }

              that.codeSelected = [];
              that.codeSelected = that.master_data.BillingCodes.filter(
                (code: { BillingCode: any }) => code.BillingCode === '99490'
              );
              if (that.codeSelected.length > 0) {
                that.programForm_3.controls['billingcode2'].setValue(
                  that.codeSelected[0].BillingCode +
                    ' - ' +
                    that.codeSelected[0].Description
                );
              }

              that.codeSelected = [];
              that.codeSelected = that.master_data.BillingCodes.filter(
                (code: { BillingCode: any }) => code.BillingCode === '99439'
              );
              if (that.codeSelected.length > 0) {
                that.programForm_3.controls['billingcode3'].setValue(
                  that.codeSelected[0].BillingCode +
                    ' - ' +
                    that.codeSelected[0].Description
                );
              }

              break;
            case 'CCM-P':
              that.codeSelected = [];
              that.codeSelected = that.master_data.BillingCodes.filter(
                (code: { BillingCode: any }) => code.BillingCode === 'G0506'
              );
              if (that.codeSelected.length > 0) {
                that.programForm_3.controls['billingcode1'].setValue(
                  that.codeSelected[0].BillingCode +
                    ' - ' +
                    that.codeSelected[0].Description
                );
              }

              that.codeSelected = [];
              that.codeSelected = that.master_data.BillingCodes.filter(
                (code: { BillingCode: any }) => code.BillingCode === '99491'
              );
              if (that.codeSelected.length > 0) {
                that.programForm_3.controls['billingcode2'].setValue(
                  that.codeSelected[0].BillingCode +
                    ' - ' +
                    that.codeSelected[0].Description
                );
              }

              that.codeSelected = [];
              that.codeSelected = that.master_data.BillingCodes.filter(
                (code: { BillingCode: any }) => code.BillingCode === '99437'
              );
              if (that.codeSelected.length > 0) {
                that.programForm_3.controls['billingcode3'].setValue(
                  that.codeSelected[0].BillingCode +
                    ' - ' +
                    that.codeSelected[0].Description
                );
              }

              break;
            case 'C-CCM':
              that.codeSelected = [];
              that.codeSelected = that.master_data.BillingCodes.filter(
                (code: { BillingCode: any }) => code.BillingCode === 'G0506'
              );
              if (that.codeSelected.length > 0) {
                that.programForm_3.controls['billingcode1'].setValue(
                  that.codeSelected[0].BillingCode +
                    ' - ' +
                    that.codeSelected[0].Description
                );
              }

              that.codeSelected = [];
              that.codeSelected = that.master_data.BillingCodes.filter(
                (code: { BillingCode: any }) => code.BillingCode === '99487'
              );
              if (that.codeSelected.length > 0) {
                that.programForm_3.controls['billingcode2'].setValue(
                  that.codeSelected[0].BillingCode +
                    ' - ' +
                    that.codeSelected[0].Description
                );
              }

              that.codeSelected = [];
              that.codeSelected = that.master_data.BillingCodes.filter(
                (code: { BillingCode: any }) => code.BillingCode === '99489'
              );
              if (that.codeSelected.length > 0) {
                that.programForm_3.controls['billingcode3'].setValue(
                  that.codeSelected[0].BillingCode +
                    ' - ' +
                    that.codeSelected[0].Description
                );
              }

              break;
            case 'PCM-P':
              that.codeSelected = [];
              that.codeSelected = that.master_data.BillingCodes.filter(
                (code: { BillingCode: any }) => code.BillingCode === '99424'
              );
              if (that.codeSelected.length > 0) {
                that.programForm_3.controls['billingcode1'].setValue(
                  that.codeSelected[0].BillingCode +
                    ' - ' +
                    that.codeSelected[0].Description
                );
              }

              that.codeSelected = [];
              that.codeSelected = that.master_data.BillingCodes.filter(
                (code: { BillingCode: any }) => code.BillingCode === '99425'
              );
              if (that.codeSelected.length > 0) {
                that.programForm_3.controls['billingcode2'].setValue(
                  that.codeSelected[0].BillingCode +
                    ' - ' +
                    that.codeSelected[0].Description
                );
              }

              break;
            case 'PCM-C':
              that.codeSelected = [];
              that.codeSelected = that.master_data.BillingCodes.filter(
                (code: { BillingCode: any }) => code.BillingCode === '99426'
              );
              if (that.codeSelected.length > 0) {
                that.programForm_3.controls['billingcode1'].setValue(
                  that.codeSelected[0].BillingCode +
                    ' - ' +
                    that.codeSelected[0].Description
                );
              }

              that.codeSelected = [];
              that.codeSelected = that.master_data.BillingCodes.filter(
                (code: { BillingCode: any }) => code.BillingCode === '99427'
              );
              if (that.codeSelected.length > 0) {
                that.programForm_3.controls['billingcode2'].setValue(
                  that.codeSelected[0].BillingCode +
                    ' - ' +
                    that.codeSelected[0].Description
                );
              }

              break;
          }
        }

        that.vitalschedule =
          that.groupedVitalScheduleLists[
            that.PatientVitalInfos[0] && that.PatientVitalInfos[0].VitalId
          ];
        that.primaryinsurnace =
          that.Patientdata.PatientInsurenceDetails.PatientInsurenceInfos.filter(
            (ins: { IsPrimary: boolean }) => ins.IsPrimary == true
          );
        that.secondaryinsurnace =
          that.Patientdata.PatientInsurenceDetails.PatientInsurenceInfos.filter(
            (ins: { IsPrimary: boolean }) => ins.IsPrimary == false
          );

        if (that.primaryinsurnace.length > 0) {
          this.insurance_pname = that.primaryinsurnace[0].InsuranceVendorName;
        }
        if (that.secondaryinsurnace.length > 0) {
          this.insurance_sname = that.secondaryinsurnace[0].InsuranceVendorName;
        }
        if (that.secondaryinsurnace.length > 1) {
          this.insurance_tname = that.secondaryinsurnace[1].InsuranceVendorName;
        }

        var patientdevicedetailsData =
          that.Patientdata.PatientDevicesDetails.PatientDeviceInfos;
        var vitalArr = [];
        if (patientdevicedetailsData.length == 0) {
          for (let v of that.PatientVitalInfos) {
            var obj = {
              VitalName: v.VitalName,
              DeviceStatus: 'InActive',
              VitalId: v.VitalId,
              DeviceNumber: '',
            };
            vitalArr.push(obj);
          }
          that.patientdevicedetails = vitalArr;

        }
        if (patientdevicedetailsData.length != 0) {
          for (let v of that.PatientVitalInfos) {
            const isPresent = this.checkVitalName(
              patientdevicedetailsData,
              v.VitalName
            );
            // Manjusha code change
            if (isPresent) {
              var device = this.getVitalObject(
                patientdevicedetailsData,
                v.VitalName
              );
              vitalArr.push(...device);
            } else {
              var obj = {
                VitalName: v.VitalName,
                DeviceStatus: 'InActive',
                VitalId: v.VitalId,
                DeviceNumber: '',
              };
              if (!vitalArr.some((item) => item.VitalName == obj.VitalName)) {
                vitalArr.push(obj);
              }
            }
          }
          that.patientdevicedetails = vitalArr;
          console.log('Patient Device Details');
          console.log(that.patientdevicedetails)
        }
        that.DeviceCheck();
        that.DiagnosisInfos =
          that.Patientdata.PatientPrescribtionDetails.PatientDiagnosisInfos;
        that.InsuranceInfo =
          that.Patientdata.PatientInsurenceDetails.PatientInsurenceInfos;
        that.EnrolmentDetails = that.Patientdata.PatientEnrolledDetails;
        that.ActiveDetails = that.Patientdata.ActivePatientDetails;
        that.ReadyToDischarge =
          that.Patientdata.ReadyForDischargePatientDetails;
        that.OnHold = that.Patientdata.OnHoldPatientDetais;
        that.InActive = that.Patientdata.InActivePatientDetais;
        that.Discharged = that.Patientdata.DischargedPatientDetails;
        that.vitalListChange();
      });
  }

  checkVitalName(vitals: any[], nameToCheck: string): boolean {
    return vitals.some(
      (vital) => vital.VitalName.toLowerCase() === nameToCheck.toLowerCase()
    );
  }
  // Manjusha code change
  getVitalObject(vitals: any[], nameToFind: string): any {
    return vitals.filter(
      (vital) => vital.VitalName.toLowerCase() === nameToFind.toLowerCase()
    );
  }
   // Manjusha code change
  codeSelected: any;
  DeviceCheck() {
    const allVitalsActive = this.PatientVitalInfos.every((vital: { VitalName: string }) => {
      const vitalName = vital?.VitalName?.toLowerCase();
      if (!vitalName) return false; // if vital name is missing, consider it failed

      return this.patientdevicedetails.some(
        (device: { VitalName?: string; DeviceStatus: string }) =>
          device?.VitalName?.toLowerCase() === vitalName &&
          device.DeviceStatus === 'Active'
      );
    });
    this.DeviceNotAvaliable = !allVitalsActive;
  }

  DeviceCheckOnSubmit() {
    if (!Array.isArray(this.patientdevicedetails)) {
      console.error('patientdevicedetails is not an array:', this.patientdevicedetails);
      return false;
    }
    let devicesToRemove: string[] = [];
    for (let x of this.patientdevicedetails) {
      if (!this.editProgramdata.controls.vitalListDialog.value.includes(x.VitalId) && x.DeviceStatus === 'Active') {
        devicesToRemove.push(x.DeviceName);
      }
    }
    if (devicesToRemove.length > 0) {
      let devicesList = devicesToRemove.join(', ');
      let message = devicesToRemove.length === 1
      ? `The selected plan does not support the ${devicesList}. Please remove the device to change the plan.`
      : `The selected plan does not support the following devices: ${devicesList}. Please remove them to change the plan.`;

    alert(message);
      this.showProgramEditModal = false;
      return false;
    }
    return true;
  }
  //
  a: any;
  vitalschedule: any;
  InsuranceInfo: any;
  // Manjusha code change
  addNewDevice() {
    var obj = {
      VitalName: '',
      DeviceStatus: 'InActive',
      VitalId: '',
      DeviceNumber: '',
    };
    if (this.patientdevicedetails?.length < 10) {
      this.patientdevicedetails.push(obj);
    }
  }
  vitalchanged(event: any, i: number) {
    this.vitalschedule = this.groupedVitalScheduleLists[event];
  }

  CurrentMenu: any = 1;
  deviceList: any;
  // Change Main Screen
  processMasterData() {
    var that = this;
    if (!sessionStorage.getItem('add_patient_masterdata')) {
      this.rolelist = sessionStorage.getItem('Roles');
      this.rolelist = JSON.parse(this.rolelist);
      this.rpm
        .rpm_get(
          '/api/patient/getprogramdetailsmasterdataaddpatient?RoleId=' +
            this.rolelist[0].Id
        )
        .then((data) => {
          that.add_patient_masterdata = data;
          sessionStorage.setItem(
            'add_patient_masterdata',
            JSON.stringify(that.add_patient_masterdata)
          );
        });
    }
    that.master_data = sessionStorage.getItem('add_patient_masterdata');
    that.master_data = JSON.parse(that.master_data);
    that.deviceList = that.master_data.DeviceDetails;
    this.deviceVitalLists();

    that.Program_Selected = that.master_data.ProgramDetailsMasterData.filter(
      (Pgm: { ProgramId: number }) =>
        Pgm.ProgramId ===
        parseInt(that.Patientdata.PatientProgramdetails.ProgramId)
    );
    if (that.Program_Selected.length > 0) {
      that.Diagnosis_Name = that.Program_Selected[0].Vitals[0].Dignostics;
      this.program_name = that.Program_Selected[0].ProgramName;
      this.program_id = that.Program_Selected[0].ProgramId;
      this.programVitals = that.Program_Selected[0].Vitals;
      this.selectedProgram = this.programDetails.some((program: { ProgramId: number; }) => program.ProgramId === that.Program_Selected[0].ProgramId);
      //
      for (let x of this.Diagnosis_Name) {
        x.DiagnosisField = x.DiagnosisName;
      }
    }
  }
  program_name: any;
  ChangeScreen(button: any) {
    this.CurrentMenu = button;
    switch (button) {
      case 1:
        this.variable = 1;
        break;

      case 2:
        this.variable = 2;
        break;

      default:
        this.variable = 1;
        break;
    }
  }

  editprogram() {
    this.editvariable = !this.editvariable;
  }
  openDialog(templateRef: TemplateRef<any>) {
    for (let i = 0; i < this.Diagnosis_List.length; i++) {
      if (
        this.Diagnosis_List[i].DiagnosisName == '' ||
        this.Diagnosis_List[i].DiagnosisName == null ||
        this.Diagnosis_List[i].DiagnosisName == undefined
      ) {
        alert('Please Add Patient Diagnosis.');
        return;
      }
    }
    if (this.vitalList.length == 0) {
      alert('Conditions/Vital Monitored Required');
      return;
    }

    this.dialog.closeAll();
    this.dialog.open(templateRef);
  }
  confirmDialog(
    templateSuccess: TemplateRef<any>,
    templateFailure: TemplateRef<any>
  ) {
    this.failureTemplate = templateFailure;
    this.successTemplate = templateSuccess;
    this.confirm_action();
  }

  redirect_patient() {
    this.dialog.closeAll();
    this.backtodetailpage();
  }

  datasourceDocument: any;

  onAddData1() {
    this.dataSource.push(this.dataSource.length);
    this.display = !this.display;
  }
  variable: any;

  Patientinfo() {
    this.variable = 1;
  }
  Programinfo() {
    this.variable = 2;
  }
  scroll(el: HTMLElement) {
    el.scrollIntoView();
    el.scrollIntoView({ behavior: 'smooth' });
  }
  onAddData() {
    this.dataSource.push(this.dataSource.length);
  }

  onAddDiaganostic() {
    this.diagonisticsData.push(this.diagonisticsData.length);
  }
  durationValue = 12;
  patientHeighValue: any;

  DurationEdit = true;

  durationStatusEnable() {
    if (
      this.Patientdata.PatientProgramdetails.Status == 'Active' ||
      this.Patientdata.PatientProgramdetails.Status == 'OnHold' ||
      this.Patientdata.PatientProgramdetails.Status == 'ReadyToDischarge' ||
      this.Patientdata.PatientProgramdetails.Status == 'Discharged'
    ) {
      this.DurationEdit = false;
    }
  }
  increment_duration() {
    this.durationStatusEnable();
    if (this.durationValue < 12) {
      this.durationValue++;
    } else {
      this.durationValue = 12;
    }

    this.calculateEndDate();
  }
  decrement_duration() {

    if (this.durationValue > 1) {
      this.durationValue--;
    } else {
      this.durationValue = 1;
    }
    this.calculateEndDate();
  }

  startDateValue: any;
  endDateValue: any;
  calculateEndDate() {
    var someDate = this.programForm.controls.startdate.value;
    if (someDate.includes('T')) {
      someDate = someDate;
    } else {
      someDate = someDate + 'T00:00:00';
    }
    someDate = this.Auth.ConvertToUTCRangeInput(new Date(someDate));
    this.startDateValue = someDate;
    const someDateValue = dayjs(someDate).add(this.durationValue, 'month');
    this.programForm.controls['enddate'].setValue(
      this.convertDate(someDateValue)
    );
  }
  weightValue = 176;
  increment_weight() {
    this.weightValue++;
  }
  decrement_weight() {
    if (this.weightValue > 0) {
      this.weightValue--;
    } else {
      this.weightValue = 0;
    }
  }
  heightValue = 8;
  increment_height() {
    this.heightValue++;
  }
  decrement_height() {
    if (this.heightValue > 0) {
      this.heightValue--;
    } else {
      this.heightValue = 0;
    }
  }
  lowerthresholdvalue = 62;
  lowerthresholdvalue_increment() {
    this.lowerthresholdvalue++;
  }
  lowerthresholdvalue_decrement() {
    if (this.lowerthresholdvalue > 0) {
      this.lowerthresholdvalue--;
    } else {
      this.lowerthresholdvalue = 0;
    }
  }
  higherthresholdvalue = 128;
  higherthresholdvalue_increment() {
    this.higherthresholdvalue++;
  }
  higherthresholdvalue_decrement() {
    if (this.higherthresholdvalue > 0) {
      this.higherthresholdvalue--;
    } else {
      this.higherthresholdvalue = 0;
    }
  }
  lowercriticalthreshold = 52;
  lowercriticalthreshold_increment() {
    this.lowercriticalthreshold++;
  }
  lowercriticalthreshold_decrement() {
    if (this.lowercriticalthreshold > 0) {
      this.lowercriticalthreshold--;
    } else {
      this.lowercriticalthreshold = 0;
    }
  }
  highercriticalthreshold = 148;
  highercriticalthreshold_increment() {
    this.highercriticalthreshold++;
  }
  highercriticalthreshold_decrement() {
    if (this.highercriticalthreshold > 0) {
      this.highercriticalthreshold--;
    } else {
      this.highercriticalthreshold = 0;
    }
  }

  minValue: number = 72;
  maxValue: number = 200;
  middle: number = 90;
  displayValue() {}

  patient = [
    { menu_id: 1, menu_title: 'Personal Information' },
    { menu_id: 1, menu_title: 'Communication Datails' },
    { menu_id: 1, menu_title: 'Patient Preferences' },
  ];

  onClickNavigation(id: any) {
    switch (id) {
      case 1:
        this.patient_variable = 1;
        break;
    }
  }

  program = [
    { menu_id: 1, menu_title: 'Program Details' },
    { menu_id: 1, menu_title: 'Device Details' },
    { menu_id: 1, menu_title: 'Vital Schdules' },
    { menu_id: 1, menu_title: 'Billing & Insurance' },
    { menu_id: 1, menu_title: 'Documents' },
  ];
  menu = [
    {
      menu_id: 1,
      menu_title: 'Patient Info',
    },
    {
      menu_id: 2,
      menu_title: 'Program Info',
    },
  ];
  onClickNavigation1(id: any) {
    switch (id) {
      case 1:
        this.program_variable = 1;
        break;
    }
  }
  columnHeader = ['selection', 'PatientName', 'PatientId', 'CreatedOn'];
  displayedColumns: string[] = [
    'Document_Type',
    'Document_Name',
    'Uploaded_Date',
    'Document',
  ];
  onSubmit() {
    // if (this.loginForm.invalid) {
    //   alert('Please Fill all Feilds');
    // } else {
    //   alert('Register Seuccessfully!! ');
    // }
  }
  addeditpage_cancel() {
    this.editvariable = !this.editvariable;
  }

  public get goals() {
    return this.programForm.get('goals') as UntypedFormArray;
  }

  goalForm = this.PatientFormBuilderObj.group({
    Goal: new UntypedFormControl('', [Validators.required]),
    Description: new UntypedFormControl('', [Validators.required]),
  });
  // Add Diaganostics Code
  public get diagnosisData() {
    return this.programForm.get('diagnosisData') as UntypedFormArray;
  }

  DiaganosticsForm = this.PatientFormBuilderObj.group({
    DiagnosissName: new UntypedFormControl('', [Validators.required]),
    DiagnosisCode: new UntypedFormControl('', [Validators.required]),
  });

  objDiagnosis = { Id: 0, DiagnosisCode: null, DiagnosisName: null };
  addNewDiaganostics() {
    this.objDiagnosis = { Id: 0, DiagnosisCode: null, DiagnosisName: null };

    if (
      this.Diagnosis_List[this.Diagnosis_List.length - 1].DiagnosisCode !=
        null &&
      this.Diagnosis_List[this.Diagnosis_List.length - 1].DiagnosisName != null
    ) {
      if (this.Diagnosis_List.length < 10) {
        for (let i = 0; i <= this.Diagnosis_List.length; i++) {
          this.Diagnosis_List.push(this.objDiagnosis);
          this.display = !this.display;
          return;
        }
      }
    }
  }

  removeDiaganostics_Patient() {
    if (this.Diagnosis_List.length > 1) {
      this.Diagnosis_List.pop();
      this.display = !this.display;
    }
  }

  editObjDiagnosis = { Id: 0, DiagnosisCode: null, DiagnosisName: null };

  // Manjusha code change
  programaddNewDiaganostic() {
    const selectedDiagnosesArray = this.editProgramdata.get('selectedDiagnoses') as FormArray;

    // Get the last diagnosis form group (if any)
    const lastItem = selectedDiagnosesArray.at(selectedDiagnosesArray.length - 1);

    // Only add a new one if last item is filled
    if (
      !lastItem ||
      (lastItem.get('DiagnosisCode')?.value != null &&
       lastItem.get('DiagnosisName')?.value != null)
    ) {
      if (selectedDiagnosesArray.length < 10) {
        selectedDiagnosesArray.push(this.fb.group({
          Id: [0],
          DiagnosisCode: [null],
          DiagnosisName: [null],
        }));

        // Optional: toggle UI or emit event
        this.display = !this.display;
      } else {
        alert("Maximum 5 diagnoses allowed.");
      }
    } else {
      alert("Please fill the current diagnosis before adding a new one.");
    }
  }

  programaremoveNewDiaganostic() {
    const selectedDiagnosesArray = this.editProgramdata.get('selectedDiagnoses') as FormArray;

    if (selectedDiagnosesArray.length > 1) {
      selectedDiagnosesArray.removeAt(selectedDiagnosesArray.length - 1); //Proper reactive removal
      this.display = !this.display;
    } else {
      alert('At least one diagnosis must remain.');
    }
  }

  confirm_action() {
      this.submitImage(this.pid).then((res) => {
        if (this.variable === 1 || this.variable === 2) {
          this.UpdatePatientInfo();
          this.editPatientProgram();
        }
      }).catch((err) => {
        alert(err.error.message);
      });
  }
  UpdatePatientInfo() {
    this.markFormGroupTouched(this.PatientInfoForm);

    //call on clicking savedraft after opening the drafted patient

    var patientHeight = this.HeightFeetValue + '.' + this.HeightInchValue;
    this.patientHeighValue = parseFloat(patientHeight).toFixed(2);
    var req_body: any = {};
    if (this.PatientInfoForm.valid) {
      this.loading = true;
      req_body = {
        PatientId: this.pid,
        Picture: 'null',
        FirstName: this.PatientInfoForm.controls.firstname.value,
        MiddleName: this.PatientInfoForm.controls.middlename.value,
        LastName: this.PatientInfoForm.controls.lastname.value,
        DOB: this.PatientInfoForm.controls.dateofbirth.value,
        Gender: this.PatientInfoForm.controls.gender.value,
        Height: this.patientHeighValue,
        Weight: this.weightValue,
        Email: this.PatientInfoForm.controls.email.value,
        MobileNo: this.PatientInfoForm.controls.mobile.value,
        AlternateMobNo: this.PatientInfoForm.controls.alternatenumber.value,
        OrganizationID: this.resp.OrganizationID,
        Address1: this.PatientInfoForm.controls.addressline1.value,
        Address2: this.PatientInfoForm.controls.addressline2.value,
        ZipCode: this.PatientInfoForm.controls.zipcode.value,
        CityID: this.PatientInfoForm.controls.city.value,
        StateId: this.PatientInfoForm.controls.state.value,
        TimeZoneID: this.PatientInfoForm.controls.timezone.value,
        Contact1Name: this.PatientInfoForm.controls.emergencycontact1.value,
        Contact1RelationName: this.PatientInfoForm.controls.relation1.value,
        Contact1Phone: this.PatientInfoForm.controls.relation1_mobile.value,
        Contact2Name: this.PatientInfoForm.controls.emergencycontact2.value,
        Contact2RelationName: this.PatientInfoForm.controls.relation2.value,
        Contact2Phone: this.PatientInfoForm.controls.relation2_mobile.value,
        CallTime: this.PatientInfoForm.controls.calltime.value,
        Language: this.PatientInfoForm.controls.language.value,
        Status: 'Prescribed',
        Preference1: this.PatientInfoForm.controls.preference2.value,
        Preference2: this.PatientInfoForm.controls.preference3.value,
        Preference3: this.PatientInfoForm.controls.preference4.value,
        Notes: this.PatientInfoForm.controls.additionalnotes.value,
      };
      var that = this;
      this.rpm.rpm_post('/api/patient/updatepatient', req_body).then(
        (data) => {
          this.loading = false;
          if (this.variable != 2) {
            this.dialog.closeAll();
            this.redirect_patient();

          }
        },
        (err) => {
          this.loading = false;
          if (this.variable != 2) {
            this.dialog.closeAll();
            this.redirect_patient();
          }
        }
      );
    } else {
      alert('Please complete all mandatory fields..!');
      this.loading = false;
    }
  }
  backtodetailpage() {
    let route = '/admin/patients_detail';
    this.router.navigate([route], {
      queryParams: { id: this.pid, programId: this.patientprogramid },
      skipLocationChange: true,
    });
  }

  isPrescribed: boolean;
  isEnrolled: boolean;
  isActive: boolean;
  isReadyToDischarge: boolean;
  isDischarged: boolean;
  isReActive: boolean;
  bold_icon = '../../../../../assets/04-Icons/Icons_Check B.svg';
  unbold_icon = '../../../../../assets/04-Icons/Icons_Check.svg';
  inactive_icon = '../../../../../assets/04-Icons/Icons_Check_password.svg';
  prescribed_img: any;
  enrolled_img: any;
  active_img: any;
  readytodischarge_img: any;
  discharged_img: any;
  onhold_img: any;
  isReActive_img: any;
  intializeImages() {
    this.prescribed_img = this.unbold_icon;
    this.enrolled_img = this.unbold_icon;
    this.active_img = this.unbold_icon;
    this.readytodischarge_img = this.unbold_icon;
    this.isPrescribed = false;
    this.isEnrolled = false;
    this.isActive = false;
    this.isReadyToDischarge = false;
    this.isDischarged = false;
    this.isReActive = false;
    this.isReActive_img = this.unbold_icon;
  }

  inactiveStatus = false;
  ConfirmChangePatientStatus(status: any, timeofcall: number) {
    this.changePatientStatusDialog(status, timeofcall);
  }
  inactiveVariable: any;
  ChangePatientStatus(status: any, timeofcall: number) {


    if (status == 'OnHold') {
      this.setOnHold();
      return;
    } else if (status == 'Discharged') {
      if (this.current_status_value == 'Active') {
        this.unsetPrescribed();
        this.unsetEnrolled();
        this.unsetActive();
        this.setReadyToDischarge();
        this.isPrescribed = true;
        this.isEnrolled = true;
        this.isActive = true;
        this.isReadyToDischarge = true;
        this.isDischarged = false;
      } else {
        this.setDischarged();
      }
    } else {
      this.unsetOnHold();
      if (this.Patientdata.PatientProgramdetails.Status == 'OnHold') {
        this.ImageStatus = false;
        if (this.last_status == 'InActive') {
          this.setInActive();
          this.isPrescribed = true;
          this.isEnrolled = true;
          this.isActive = false;
          this.isReadyToDischarge = false;
          this.isDischarged = false;
          this.inactiveVariable = true;
        } else if (this.last_status == 'ReadyToDischarge') {
          this.setReadyToDischarge();
          this.isPrescribed = true;
          this.isEnrolled = true;
          this.isActive = true;
          this.isReadyToDischarge = true;
          this.isDischarged = false;
          this.inactiveVariable = false;
        } else if (this.last_status == 'Active') {
          this.inactiveVariable = false;
          this.setActive();
          this.isPrescribed = true;
          this.isEnrolled = true;
          this.isActive = false;
          this.isReadyToDischarge = false;
          this.isDischarged = false;
        } else if (this.last_status == 'Enrolled') {
          this.setEnrolled();
          this.isPrescribed = true;
          this.isEnrolled = false;
          this.isActive = false;
          this.isReadyToDischarge = false;
          this.isDischarged = false;
          this.inactiveVariable = false;
        } else {
          this.setPrescribed();
        }

        if (
          this.status_to_send == 'Prescribed' ||
          this.status_to_send == 'Active' ||
          this.status_to_send == 'Enrolled' ||
          this.status_to_send == 'ReadyToDischarge' ||
          this.status_to_send == 'InActive'
        ) {
          this.ImageStatus = true;
        } else {
          this.ImageStatus = false;
        }
      } else {
        if (this.Patientdata.PatientProgramdetails.Status == 'Discharged') {
          this.setDischarged();
          this.isPrescribed = true;
          this.isEnrolled = true;
          this.isActive = true;
          this.isReadyToDischarge = true;
          this.isDischarged = true;
          this.inactiveVariable = false;
        } else if (
          this.Patientdata.PatientProgramdetails.Status == 'ReadyToDischarge'
        ) {
          if (this.isReActive) {
            this.setReActive();
            this.unsetReadyToDischarge();
            this.isPrescribed = true;
            this.isEnrolled = true;
            this.isActive = true;
            this.isReadyToDischarge = true;
            this.isDischarged = false;
            this.inactiveVariable = false;
          } else {
            this.setReadyToDischarge();
            this.isPrescribed = true;
            this.isEnrolled = true;
            this.isActive = true;
            this.isReadyToDischarge = true;
            this.isDischarged = false;
            this.inactiveVariable = false;
          }
        } else if (this.Patientdata.PatientProgramdetails.Status == 'Active') {
          this.inactiveVariable = false;
          this.setActive();
          this.isPrescribed = true;
          this.isEnrolled = true;
          this.isActive = false;
          this.isReadyToDischarge = false;
          this.isDischarged = false;
        } else if (
          this.Patientdata.PatientProgramdetails.Status == 'Enrolled'
        ) {
          this.setEnrolled();
          this.isPrescribed = true;
          this.isEnrolled = false;
          this.isActive = false;
          this.isReadyToDischarge = false;
          this.isDischarged = false;
          this.inactiveVariable = false;
        } else if (
          this.Patientdata.PatientProgramdetails.Status == 'InActive'
        ) {
          this.setInActive();
          this.isPrescribed = true;
          this.isEnrolled = true;
          this.isActive = false;
          this.isReadyToDischarge = false;
          this.isDischarged = false;
          this.inactiveVariable = true;
        } else {
          this.setPrescribed();
        }
      }

      if (timeofcall == 1) {
        if (
          (status == 'Prescribed' ||
            status == 'OnHold' ||
            status == 'Discharged') &&
          !this.isPrescribed
        ) {
          this.setPrescribed();
          this.unsetEnrolled();
          this.unsetActive();
          this.unsetReadyToDischarge();
          this.unSetReActive();
        } else if (
          (status == 'Enrolled' ||
            status == 'OnHold' ||
            status == 'Discharged') &&
          !this.isEnrolled
        ) {
          this.unsetPrescribed();
          this.setEnrolled();
          this.unsetActive();
          this.unsetReadyToDischarge();
          this.unSetReActive();
          this.isPrescribed = true;
          this.isEnrolled = false;
          this.isActive = false;
          this.isReadyToDischarge = false;
          this.isDischarged = false;
        }
        if (
          (status == 'Active' ||
            status == 'OnHold' ||
            status == 'Discharged') &&
          !this.isActive
        ) {
          this.unsetPrescribed();
          this.unsetEnrolled();
          this.setActive();
          this.unsetReadyToDischarge();
          this.unSetReActive();
          this.isPrescribed = true;
          this.isEnrolled = true;
          this.isActive = false;
          this.isReadyToDischarge = false;
          this.isDischarged = false;
        }
        if (
          (status == 'ReadyToDischarge' ||
            status == 'OnHold' ||
            status == 'Discharged') &&
          !this.isReadyToDischarge
        ) {
          this.unsetPrescribed();
          this.unsetEnrolled();
          this.unsetActive();
          this.setReadyToDischarge();
          this.unSetReActive();
          this.isPrescribed = true;
          this.isEnrolled = true;
          this.isActive = true;
          this.isReadyToDischarge = true;
          this.isDischarged = false;
        }
      } else {
        if (
          (status == 'Prescribed' ||
            status == 'OnHold' ||
            status == 'Discharged') &&
          !this.isPrescribed &&
          this.Patientdata.PatientProgramdetails.Status == 'Discharged'
        ) {
          this.setPrescribed();
          this.unsetEnrolled();
          this.unsetActive();
          this.unsetReadyToDischarge();
          this.unSetReActive();
        } else if (
          (status == 'Enrolled' ||
            status == 'OnHold' ||
            status == 'Discharged') &&
          !this.isEnrolled &&
          this.Patientdata.PatientProgramdetails.Status == 'Prescribed'
        ) {
          this.unsetPrescribed();
          this.setEnrolled();
          this.unsetActive();
          this.unsetReadyToDischarge();
          this.unSetReActive();
          this.isPrescribed = true;
          this.isEnrolled = false;
          this.isActive = false;
          this.isReadyToDischarge = false;
          this.isDischarged = false;
        }
        if (
          (status == 'Active' ||
            status == 'OnHold' ||
            status == 'Discharged') &&
          !this.isActive &&
          this.Patientdata.PatientProgramdetails.Status == 'Enrolled'
        ) {
          this.unsetPrescribed();
          this.unsetEnrolled();
          this.setActive();
          this.unsetReadyToDischarge();
          this.unSetReActive();
          this.isPrescribed = true;
          this.isEnrolled = true;
          this.isActive = false;
          this.isReadyToDischarge = false;
          this.isDischarged = false;
        }
        if (
          (status == 'InActive' ||
            status == 'OnHold' ||
            status == 'Discharged') &&
          !this.isActive &&
          this.Patientdata.PatientProgramdetails.Status == 'Enrolled'
        ) {
          this.unsetPrescribed();
          this.unsetEnrolled();
          this.setInActive();
          this.unsetReadyToDischarge();
          this.unSetReActive();
          this.isPrescribed = true;
          this.isEnrolled = true;
          this.isActive = false;
          this.isReadyToDischarge = false;
          this.isDischarged = false;
        }
        if (
          (status == 'ReadyToDischarge' ||
            status == 'OnHold' ||
            status == 'Discharged') &&
          !this.isReadyToDischarge &&
          this.Patientdata.PatientProgramdetails.Status == 'Active'
        ) {
          this.unsetPrescribed();
          this.unsetEnrolled();
          this.unsetActive();
          this.setReadyToDischarge();
          this.unSetReActive();
          this.isPrescribed = true;
          this.isEnrolled = true;
          this.isActive = true;
          this.isReadyToDischarge = true;
          this.isDischarged = false;
        }
        if (
          (status == 'ReadyToDischarge' ||
            status == 'OnHold' ||
            status == 'Discharged') &&
          !this.isReadyToDischarge &&
          this.Patientdata.PatientProgramdetails.Status == 'InActive'
        ) {
          this.unsetPrescribed();
          this.unsetEnrolled();
          this.unsetActive();
          this.setReadyToDischarge();
          this.unSetReActive();
          this.isPrescribed = true;
          this.isEnrolled = true;
          this.isActive = true;
          this.isReadyToDischarge = true;
          this.isDischarged = false;
          this.ImageStatus = true;
        }
      }
    }
  }
  status_to_send: any;
  setPrescribed() {
    this.prescribed_img = this.bold_icon;
    this.status_to_send = 'Prescribed';
  }
  setEnrolled() {
    this.enrolled_img = this.bold_icon;
    this.status_to_send = 'Enrolled';
  }
  setActive() {
    this.active_img = this.bold_icon;
    this.status_to_send = 'Active';
  }
  setReadyToDischarge() {
    this.readytodischarge_img = this.bold_icon;
    this.status_to_send = 'ReadyToDischarge';
  }
  setReActive() {
    this.isReActive_img = this.bold_icon;
    this.status_to_send = 'Active';
  }
  setDischarged() {
    this.discharged_img = this.bold_icon;
    this.status_to_send = 'Discharged';

    this.unsetOnHold();

    if (this.Patientdata.PatientProgramdetails.Status == 'OnHold') {
      if (this.status_to_send == 'Discharged') {
        this.ImageStatus = true;
      } else {
        this.ImageStatus = false;
      }
      if (
        this.status_to_send == 'Prescribed' ||
        this.status_to_send == 'Active' ||
        this.status_to_send == 'Enrolled' ||
        this.status_to_send == 'ReadyToDischarge' ||
        this.status_to_send == 'InActive'
      ) {
        if (this.status_to_send == 'Discharged') {
          this.ImageStatus = true;
        } else {
          this.ImageStatus = false;
        }
      }
    }
    if (
      this.current_status_value == 'Prescribed' ||
      this.current_status_value == 'Enrolled' ||
      this.current_status_value == 'ReadyToDischarge' ||
      this.current_status_value == 'InActive'
    ) {
      if (this.status_to_send == 'Discharged') {
        this.ImageStatus = true;
        this.intializeImages();
      } else {
        this.ImageStatus = false;
      }
    }
  }
  unsetPrescribed() {
    this.prescribed_img = this.unbold_icon;
  }
  unsetEnrolled() {
    this.enrolled_img = this.unbold_icon;
  }
  unsetActive() {
    this.active_img = this.unbold_icon;
  }
  unsetReadyToDischarge() {
    this.readytodischarge_img = this.unbold_icon;
  }
  unsetDischarged() {
    this.discharged_img = this.unbold_icon;
  }
  unSetReActive() {
    this.isReActive_img = this.unbold_icon;
  }
  onHold: any;
  ImageStatus: boolean;
  setOnHold() {
    this.onhold_img = this.bold_icon;
    this.status_to_send = 'OnHold';
    this.ActivePanelBar();
    if (this.Patientdata.PatientProgramdetails.Status != 'OnHold') {
      if (this.status_to_send == 'OnHold') {
        this.ImageStatus = true;
      } else {
        this.ImageStatus = false;
      }
    }
    this.onHold = true;
  }
  unsetOnHold() {
    this.onhold_img = this.unbold_icon;
    this.onHold = false;
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

  convertDateMMDD(dateval: any) {
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
    dateval = mm2 + '/' + dd2 + '/' + yyyy;
    return dateval;
  }
  isDeviceActive: any;
  editPatientProgram() {
    this.markFormGroupTouched(this.programForm);
    this.markFormGroupTouched(this.programForm_2);
    this.markFormGroupTouched(this.programForm_3);
    this.isDeviceActive = false;
    for (let obj of this.patientdevicedetails) {
      if (obj.DeviceStatus == 'Active' || obj.DeviceStatus == 'Error') {
        this.isDeviceActive = true;
      }
    }

    if (this.status_to_send == 'Active') {
      if (
        this.Patientdata &&
        this.Patientdata.PatientProgramdetails.ProgramName != 'CCM' &&
        this.Patientdata.PatientProgramdetails.ProgramName != 'PCM'
      ) {
        if (this.isDeviceActive) {
          console.log('All Patient Details are Verified');
        } else {
          alert(
            'Please complete Device Registration to Make a Patient Active...!'
          );
          this.dialog.closeAll();
          return;
        }
      }
    }

    console.log('Consultation Date confirm');
    console.log(this.programForm.controls.consultdate.value)
    // var diagnostics = this.processDiagnostics(this.programForm.controls.diagnosisDataList.value);
    var diagnosis = this.Diagnosis_List;

    var insuranceDeails = this.processInsurance(
      this.insurance_pname,
      this.insurance_sname,
      this.insurance_tname
    );
    this.loading = true;
    var endDate = this.programForm.controls.enddate.value + 'T00:00:00';
    var req_body = {
      PatientId: parseInt(this.pid),
      PatientProgramId: this.patientprogramid,
      PhysicianId: this.programForm.controls.physician.value,
      ConsultationDate: this.programForm.controls.consultdate.value,
      CareTeamUserId: this.programForm.controls.assignedMember.value,
      PatientStatus: this.status_to_send,
      StartDate: this.startDateValue,
      EndDate: endDate,
      patientProgramGoals: this.Goals_Selected,
      PatientProgramDiagnosis: diagnosis,
      PatientVitalDetails: this.PatientVitalInfos,
      PatientInsurenceDetails: insuranceDeails,

      PrescribedDate: this.Auth.ConvertToUTCRangeInput(
        new Date(this.programForm.controls.PrescribedDate.value + 'T00:00:00')
      ),

      EnrolledDate: this.Auth.ConvertToUTCRangeInput(
        new Date(this.programForm.controls.EntrolledDate.value + 'T00:00:00')
      ),
      VitalIds: this.vitalList,
    };
    if ((this.current_status_value == 'Prescribed')) {
      var datevalue = this.Auth.ConvertToUTCRangeInput(new Date());
      var dtArr = datevalue.split('T');
      req_body['EnrolledDate'] = this.Auth.ConvertToUTCRangeInput(
        new Date(dtArr[0] + 'T00:00:00')
      );
    }
    if (req_body['ConsultationDate'] == 'NaN-NaN-NaN') {
      req_body['ConsultationDate'] = null;
    }
    var that = this;
    if (this.PatientInfoForm.valid) {
      this.rpm.rpm_post('/api/patient/updatepatientprogram', req_body).then(
        (data) => {
          this.loading = false;
          if (
            this.Patientdata &&
            this.Patientdata.PatientProgramdetails.ProgramName != 'CCM'
          ) {
            if (that.status_to_send == 'Discharged') {
              var i = 0;
              for (let x of that.patientdevicedetails) {
                that.RemoveDevice(i);
                i++;
              }
            }
          }

          if (this.variable != 1) {
            this.redirect_patient();
            //show success popup patient is updated
          }
        },
        (err) => {
          this.loading = false;
          if (this.variable != 1) {
            that.dialog.open(that.failureTemplate);
            //show success popup patient is updated
          }

          //show error pop up, could not update patient
        }
      );
    } else {
      alert('Please complete all mandatory fields..!');
      this.loading = false;
      return;
    }
  }

  getVitalScheduleEditData(vital: any) {}
  deviceListBP: any;
  deviceListBG: any;
  deviceListWS: any;
  deviceListOX: any;
  deviceVitalLists() {
    var that = this;

    this.deviceListBP = this.deviceList.filter((data: { VitalId: any }) => {
      return data.VitalId == 1;
    });
    this.deviceListBG = this.deviceList.filter((data: { VitalId: any }) => {
      return data.VitalId == 2;
    });
    this.deviceListWS = this.deviceList.filter((data: { VitalId: any }) => {
      return data.VitalId == 3;
    });
    this.deviceListOX = this.deviceList.filter((data: { VitalId: any }) => {
      return data.VitalId == 4;
    });
    this.deviceListBP = this.deviceListBP.filter(
      (data: { DeviceCommunicationTypeId: any }) => {
        return data.DeviceCommunicationTypeId == this.deviceTypeDataId;
      }
    );
    this.deviceListBG = this.deviceListBG.filter(
      (data: { DeviceCommunicationTypeId: any }) => {
        return data.DeviceCommunicationTypeId == this.deviceTypeDataId;
      }
    );
    this.deviceListWS = this.deviceListWS.filter(
      (data: { DeviceCommunicationTypeId: any }) => {
        return data.DeviceCommunicationTypeId == this.deviceTypeDataId;
      }
    );
    this.deviceListOX = this.deviceListOX.filter(
      (data: { DeviceCommunicationTypeId: any }) => {
        return data.DeviceCommunicationTypeId == this.deviceTypeDataId;
      }
    );
  }

  processDiagnosis(diag: any) {
    if (this.DiagnosisInfos) {
      for (let i = 0; i < this.DiagnosisInfos.length; i++) {
        if (this.DiagnosisInfos[i].Id == diag.DiagnosisName) {
          this.DiagnosisInfos[i].DiagnosisCode = diag.DiagnosisCode;
        }
      }
    } else {
      this.DiagnosisInfos = [];
    }

    return this.DiagnosisInfos;
  }
  processInsurance(ins1: any, ins2: any, ins3: any) {
    var res = [];
    if (ins1) {
      var obj1 = {
        Id: 0,
        InsuranceVendorId: 0,
        InsuranceVendorName: ins1,
        IsPrimary: true,
      };
      res.push(obj1);
    }
    if (ins2) {
      var obj2 = {
        Id: 0,
        InsuranceVendorId: 0,
        InsuranceVendorName: ins2,
        IsPrimary: false,
      };
      res.push(obj2);
    }
    if (ins3) {
      var obj3 = {
        Id: 0,
        InsuranceVendorId: 0,
        InsuranceVendorName: ins3,
        IsPrimary: false,
      };
      res.push(obj3);
    }

    return res;
  }
  CommunicationType: any;
  reloadMasterData() {
    var that = this;
    this.rpm
      .rpm_get(
        '/api/patient/getprogramdetailsmasterdataaddpatient?RoleId=' +
          this.rolelist[0].Id
      )
      .then((data) => {
        that.add_patient_masterdata = data;
        sessionStorage.setItem(
          'add_patient_masterdata',
          JSON.stringify(that.add_patient_masterdata)
        );
        that.master_data = sessionStorage.getItem('add_patient_masterdata');
        that.master_data = JSON.parse(that.master_data);
        that.CommunicationType = that.master_data.DeviceCommunicationTypes;
      });
  }
  RegisterDevice(index: any) {
    if (this.current_status_value == 'ReadyToDischarge') {
      alert('Cannot Add Device at Ready To Discharge Status');
      return;
    }
    if (this.patientdevicedetails[index].DeviceNumber) {
      var deviceobj = this.master_data.DeviceDetails.filter(
        (device: { DeviceNumber: any }) => {
          return (
            device.DeviceNumber == this.patientdevicedetails[index].DeviceNumber
          );
        }
      );
      var req_body = {
        PatientId: this.pid,
        VendorId: 1,
        DeviceId: this.patientdevicedetails[index].DeviceNumber,
        DeviceModel: deviceobj[0].DeviceModel,
        PatientNumber: this.Patientdata.PatientDetails.UserName,
        FirstName: this.PatientInfoForm.controls.firstname.value,
        LastName: this.PatientInfoForm.controls.lastname.value,
        TimeZoneOffset: this.PatientInfoForm.controls.timezone.value,
      };

      this.rpm.rpm_post('/api/device/account/create/iglucose', req_body).then(
        (data:any) => {
          // this.openDialogWindow('Success', `Device Added Successfully!!!`);
          this.showconfirmDialog.showConfirmDialog(
            data.Message,
            'Success',
            () => {
              this.ReloadDeviceList(1);
              this.ReloadDeviceList(2);
              this.ReloadDeviceList(3);
              this.ReloadDeviceList(4);
              this.UpdatePatient_Device(this.pid, this.patientprogramid);
              this.ChangeScreen(2);

            },
            false
          );
        },
        (err:any) => {
          // this.openDialogWindow('Error', `Device not added to user assets!!!`);
          this.showconfirmDialog.showConfirmDialog(
            err.error.Message,
            'Error',
            () => {
              this.ReloadDeviceList(1);
              this.ReloadDeviceList(2);
              this.ReloadDeviceList(3);
              this.ReloadDeviceList(4);
            },
            false
          );
          //show error pop up, could not update patient
        }
      );
    } else {
      alert('Please select a Device.');
    }
  }
  ErrorFlag: boolean;
  MarkError(index: any) {
    this.ErrorFlag = true;
    if (this.patientdevicedetails[index].DeviceNumber) {
      var req_body = {
        DeviceSerialNo: this.patientdevicedetails[index].DeviceNumber,
        DeviceStatus: 'Error',
      };

      
      this.rpm.rpm_post('/api/device/updatedevicestatus', req_body).then(
        (data:any) => {
        this.showconfirmDialog.showConfirmDialog(
            data.message,
          'Success',
          () => {
             this.RemoveDevice(index);
              this.ReloadDeviceList(1);
              this.ReloadDeviceList(2);
              this.ReloadDeviceList(3);
              this.ReloadDeviceList(4);
              this.UpdatePatient_Device(this.pid, this.patientprogramid);
            },
            false
          );
        },
        (err:any) => {
          this.showconfirmDialog.showConfirmDialog(
            err.error.message,
            'Error',
            () => {
              this.ReloadDeviceList(1);
              this.ReloadDeviceList(2);
              this.ReloadDeviceList(3);
              this.ReloadDeviceList(4);
            },
            false
          );
        }
      );
    }
  }
  RemoveDevice(index: any) {
    if (this.patientdevicedetails[index].DeviceNumber) {
      var req_body = {
        PatientId: this.pid,
        VendorId: 1,
        DeviceId: this.patientdevicedetails[index].DeviceNumber,
        DeviceModel: this.patientdevicedetails[index].DeviceModel,
        PatientNumber: this.Patientdata.PatientDetails.UserName,
        FirstName: this.PatientInfoForm.controls.firstname.value,
        LastName: this.PatientInfoForm.controls.lastname.value,
        TimeZoneOffset: this.PatientInfoForm.controls.timezone.value,
      };

      this.rpm.rpm_post('/api/device/removedevice/iglucose', req_body).then(
        (data:any) => {

          if (!this.ErrorFlag) {
            // this.openDialogWindow('Success', `Device Removed Successfully.`);
            this.showconfirmDialog.showConfirmDialog(
              'Device Removed Successfully.',
              'Success',
              () => {
                this.ErrorFlag = false;
              },
              false
            );
          }
          // this.reloadMasterData();
          this.ReloadDeviceList(1);
          this.ReloadDeviceList(2);
          this.ReloadDeviceList(3);
          this.ReloadDeviceList(4);
          this.UpdatePatient_Device(this.pid, this.patientprogramid);

          //show success popup patient is updated
        },
        (err:any) => {

          if (!this.ErrorFlag) {
            this.showconfirmDialog.showConfirmDialog(
              err.error.Message,'Error' ,
              () => {
                this.ErrorFlag = false;
              },
              false
            );
          }
          this.ReloadDeviceList(1);
          this.ReloadDeviceList(2);
          this.ReloadDeviceList(3);
          this.ReloadDeviceList(4);
          //show error pop up, could not update patient
        }
      );
    }
  }
  ResetDevice(index: any) {
    if (this.patientdevicedetails[index].DeviceNumber) {
      // var deviceobj = this.master_data.DeviceDetails.filter((device: { DeviceNumber: any; })=>{
      //   return device.DeviceNumber==this.patientdevicedetails[index].DeviceNumber
      // })
      var req_body = {
        PatientId: this.pid,
        VendorId: 1,
        DeviceId: this.patientdevicedetails[index].DeviceNumber,
        DeviceModel: this.patientdevicedetails[index].DeviceModel,
        PatientNumber: this.Patientdata.PatientDetails.UserName,
        FirstName: this.PatientInfoForm.controls.firstname.value,
        LastName: this.PatientInfoForm.controls.lastname.value,
        TimeZoneOffset: this.PatientInfoForm.controls.timezone.value,
      };

      this.rpm.rpm_post('/api/device/resetdevice', req_body).then(
        (data) => {
          // this.openDialogWindow('Success', `Device Removed Successfully.`);
          this.showconfirmDialog.showConfirmDialog(
            'Device Removed Successfully.',
            'Success',
            () => {
              this.ReloadDeviceList(1);
              this.ReloadDeviceList(2);
              this.ReloadDeviceList(3);
              this.ReloadDeviceList(4);
              this.UpdatePatient_Device(this.pid, this.patientprogramid);

            },
            false
          );
        },
        (err:any) => {
          this.showconfirmDialog.showConfirmDialog(
            err.error.Message,
            'Error',
            () => {
              this.ReloadDeviceList(1);
              this.ReloadDeviceList(2);
              this.ReloadDeviceList(3);
              this.ReloadDeviceList(4);
            },
            false
          );
        }
      );
    }
  }
  TestDevice(index: any) {
    var req_body = undefined;
    this.rpm
      .rpm_post(
        '/api/device/validatedevice/iglucose?deviceid=' +
          this.patientdevicedetails[index].DeviceNumber,
        req_body
      )
      .then(
        (data) => {
          // this.openDialogWindow('Success', `Device Tested Successfully.`);
          this.showconfirmDialog.showConfirmDialog(
            'Device Tested Successfully.',
            'Success',
            () => {
            },
            false
          );
          //show success popup patient is updated
        },
        (err:any) => {
          // this.openDialogWindow('Error', `Device Test Failed.`);
          this.showconfirmDialog.showConfirmDialog(
            err.error.message,
            'Error',
            () => {
            },
            false
          );
          //show error pop up, could not update patient
        }
      );
  }
  ReloadDeviceList(vital_id: any) {
    var that = this;
    that.rpm
      .rpm_get('/api/patient/getavailabledevices?VitalId=' + vital_id)
      .then(
        (data) => {
          switch (vital_id) {
            case 1:
              that.deviceListBP = data;
              break;
            case 2:
              that.deviceListBG = data;
              break;
            case 3:
              that.deviceListWS = data;
              break;
            case 4:
              that.deviceListOX = data;
              break;
          }
        },
        (err) => {}
      );
  }
  openDialogWindow(title: any, item: any) {
    const dialogConfig = new MatDialogConfig();
    dialogConfig.width = '400px';

    dialogConfig.autoFocus = true;

    dialogConfig.data = {
      title: title,
      item: item,
    };

    this.dialog.open(StatusMessageComponent, dialogConfig);
    this.UpdatePatient_Device(this.pid, this.patientprogramid);
    this.ChangeScreen(2);
  }

  i: any;
  j: any;
  processVitalSchedule(vital: any, count: any) {
    this.vitalName = this.PatientVitalInfos[vital].VitalName;
    this.vitalMeasureName =
      this.PatientVitalInfos[vital].VitalMeasureInfos[count].MeasureName;
    this.lowerthresholdvalue =
      this.PatientVitalInfos[vital].VitalMeasureInfos[count].CautiousMinimum;
    this.higherthresholdvalue =
      this.PatientVitalInfos[vital].VitalMeasureInfos[count].CautiousMaximum;
    this.lowercriticalthreshold =
      this.PatientVitalInfos[vital].VitalMeasureInfos[count].CriticallMinimum;
    this.highercriticalthreshold =
      this.PatientVitalInfos[vital].VitalMeasureInfos[count].CriticalMaximum;
    this.minValue =
      this.PatientVitalInfos[vital].VitalMeasureInfos[count].NormalMinimum;
    this.maxValue =
      this.PatientVitalInfos[vital].VitalMeasureInfos[count].NormalMaximum;
      this.rangeControl.setValue([this.minValue, this.maxValue]);
    this.i = vital;
    this.j = count;
  }
  editVital(item: any, count: any) {
    this.processVitalSchedule;
    this.editvariable = true;
    this.processVitalSchedule(item, count);
  }

  updateVitalMesaures() {
    const [min, max] = this.rangeControl.value ?? [this.minValue, this.maxValue];
    this.minValue = min;
    this.maxValue = max;

    if (
      this.lowercriticalthreshold >= this.lowerthresholdvalue ||
      this.lowercriticalthreshold >= this.minValue ||
      this.lowercriticalthreshold >= this.maxValue ||
      this.lowercriticalthreshold >= this.higherthresholdvalue ||
      this.lowercriticalthreshold >= this.highercriticalthreshold
    ) {
      alert('Please Correct Vital Limits...!');
      return;
    }
    if (
      this.lowerthresholdvalue <= this.lowercriticalthreshold ||
      this.lowerthresholdvalue >= this.minValue ||
      this.lowerthresholdvalue >= this.maxValue ||
      this.lowerthresholdvalue >= this.higherthresholdvalue ||
      this.lowerthresholdvalue >= this.highercriticalthreshold
    ) {
      alert('Please Correct Vital Limits...!');
      return;
    }
    if (
      this.minValue <= this.lowercriticalthreshold ||
      this.minValue <= this.lowerthresholdvalue ||
      this.minValue >= this.maxValue ||
      this.minValue >= this.higherthresholdvalue ||
      this.minValue >= this.highercriticalthreshold
    ) {
      alert('Please Correct Vital Limits...!');
      return;
    }
    if (
      this.maxValue <= this.lowercriticalthreshold ||
      this.maxValue <= this.lowerthresholdvalue ||
      this.maxValue <= this.minValue ||
      this.maxValue >= this.higherthresholdvalue ||
      this.maxValue >= this.highercriticalthreshold
    ) {
      alert('Please Correct Vital Limits...!');
      return;
    }
    if (
      this.higherthresholdvalue <= this.lowercriticalthreshold ||
      this.higherthresholdvalue <= this.lowerthresholdvalue ||
      this.higherthresholdvalue <= this.minValue ||
      this.higherthresholdvalue <= this.maxValue ||
      this.maxValue >= this.highercriticalthreshold
    ) {
      alert('Please Correct Vital Limits...!');
      return;
    }
    if (
      this.highercriticalthreshold <= this.lowercriticalthreshold ||
      this.highercriticalthreshold <= this.lowerthresholdvalue ||
      this.highercriticalthreshold <= this.minValue ||
      this.highercriticalthreshold <= this.maxValue ||
      this.highercriticalthreshold <= this.higherthresholdvalue
    ) {
      alert('Please Correct Vital Limits...!');
      return;
    }
    this.PatientVitalInfos[this.i].VitalName = this.vitalName;
    this.PatientVitalInfos[this.i].VitalMeasureInfos[this.j].MeasureName =
      this.vitalMeasureName;
    this.PatientVitalInfos[this.i].VitalMeasureInfos[this.j].CautiousMinimum =
      this.lowerthresholdvalue;
    this.PatientVitalInfos[this.i].VitalMeasureInfos[this.j].CautiousMaximum =
      this.higherthresholdvalue;
    this.PatientVitalInfos[this.i].VitalMeasureInfos[this.j].CriticallMinimum =
      this.lowercriticalthreshold;
    this.PatientVitalInfos[this.i].VitalMeasureInfos[this.j].CriticalMaximum =
      this.highercriticalthreshold;
    this.PatientVitalInfos[this.i].VitalMeasureInfos[this.j].NormalMinimum =
    this.minValue;
    this.PatientVitalInfos[this.i].VitalMeasureInfos[this.j].NormalMaximum =
    this.maxValue;
    this.i = null;
    this.j = null;
    alert('Vital Measures Updated Successfully !!');
    this.editvariable = false;
  }
  profilePic: any;
  openFile(e: any) {
    this.Doc = e.target.files[0];
    var a = document.getElementsByClassName('uploadPhoto');
    this.profilePic = this.Doc.name;
  }
  handle(e: any) {
    this.Doc = e.target.files[0];
    var a = document.getElementsByClassName('uploadPhoto');
    this.file = this.Doc.name;
  }
  pdfTypeStatus = false;
  openFileData(e: any) {
    this.Doc = e.target.files[0];
    this.documentType = this.Doc.type;
    if(this.documentType != 'application/pdf')
    {
     this.Doc = null;
     this.pdfTypeStatus = true
    }else{
      this.pdfTypeStatus = false;
    }
     this.file = this.Doc.name;


  }
  submitDocument(pid: any) {
    var formData: any = new FormData();
    formData.append('PatientId', pid);
    formData.append('PatientProgramId', this.patientprogramid);
    formData.append('DocumentType', this.docType);
    formData.append('DocumentName', this.docDesc);
    formData.append('Document', this.Doc);
    this.DownloadStatus = true;
    if (
      this.Doc != null &&
      this.docDesc != null &&
      this.docDesc != undefined &&
      this.docType != null &&
      this.Doc != undefined &&  this.docDesc != ''
    ) {

        this.rpm.rpm_post(`/api/patient/adddocument`, formData).then(
          (data) => {
            alert('Document Added Successfully');
            this.CancelDocForm();
            this.UpdatePatient_Document(this.pid, this.patientprogramid);
            this.docType = null;
            this.docDesc = null;
            this.DownloadStatus = false;
          },
          (err) => {
            console.log('Error While Adding Document..!');
            this.DownloadStatus = false;
          }
        );

    } else {
      alert('Please Complete the Form');
      this.DownloadStatus = false;
    }
  }
  documentAddFlag: boolean;
  docType: any;
  docDesc: any;
  openAddDoc() {
    this.documentAddFlag = true;
  }
  AddDocument() {
    this.submitDocument(this.pid);
  }
  CancelDocForm() {
    this.docType = null;
    this.docDesc = null;
    this.Doc = null;
    this.documentAddFlag = false;
    this.file=null;
  }
  downloadDoc(doc: any) {
    // alert(doc);
  }

  openImage() {
    this.image = null;
    var a = document.getElementById('image');
    a?.click();
  }
  handleImage(e: any) {
    this.image = e.target.files[0];
    var a = document.getElementsByClassName('uploadPhoto');
    this.profilePic = this.image.name;
    //this.submitImage(this.pid);
  }

submitImage(pid: any) {
  const myPhoto = uuid.v4();
  const formData: any = new FormData();
  formData.append(myPhoto, this.image);
  if (this.image) {
    return this.rpm.rpm_post(`/api/patient/addimage?PatientId=${pid}`, formData)
      .then((data) => {
        this.UpdatePatient_Image(this.pid, this.patientprogramid);
        return data; // ? forward result
      })
      .catch((err: any) => {
         this.profilePic  = null;
         this.image = null;
        throw err; // ? forward error
      });
  } else {
    return Promise.resolve(null); // nothing to upload
  }
}
  downloadFile(FileName: any) {
    const a = document.createElement('a');
    a.href = FileName;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
  }

  deleteDevice(index: any) {
    this.patientdevicedetails.splice(index, 1);
  }

  keyword = 'DeviceNumber';

  selectEvent(item: any, i: any) {
    // do something with selected item
    this.patientdevicedetails[i].DeviceNumber = item.DeviceNumber;
  }

  onChangeSearch(val: string) {
    // fetch remote data from here
    // And reassign the 'data' which is binded to 'data' property.
  }

  onFocused(e: any) {
    // do something when input is focused
  }
  clearEvent(item: any, i: any) {
    this.patientdevicedetails[i].DeviceNumber = null;
  }
  current_status_value: any;
  current_status: any;
  dialogRef: any;
  changePatientStatusDialog(status: any, timeofcall: number) {
    if (
      this.current_status_value == 'Prescribed' &&
      (status == 'Active' || status == 'ReadyToDischarge')
    ) {
      return;
    }
    if (
      this.current_status_value == 'Enrolled' &&
      (status == 'Prescribed' || status == 'ReadyToDischarge')
    ) {
      return;
    }
    if (
      this.current_status_value == 'Active' &&
      (status == 'Prescribed' || status == 'Enrolled')
    ) {
      return;
    }
    if (
      this.current_status_value == 'ReadyToDischarge' &&
      (status == 'Prescribed' || status == 'Enrolled' || status == 'Active')
    ) {
      return;
    }
    if (
      this.current_status_value == 'ReadyToDischarge' &&
      status == 'ReActive'
    ) {
      this.isReActive = true;
    }

    if (this.current_status_value == 'OnHold' && status == 'OnHold') {
      return;
    }
    if (this.current_status_value == 'Discharged' && status == 'Discharged') {
      return;
    }
    if (this.current_status_value == 'OnHold') {

      if (status == 'Discharged') {
        this.showDialog = true;
        this.showconfirmDialog.showConfirmDialog(
        'Do you want to disCharge the patient ?',
          'Are you sure?',
          () => {
            this.ChangePatientStatus(status, timeofcall);
          }
        );
      } else {

        this.showconfirmDialog.showConfirmDialog(
        'Do you want to release from Onhold ?',
          'Are you sure?',
          () => {
            this.ChangePatientStatus(status, timeofcall);
          }
        );
      }
    } else {
      if (
        this.status_to_send != status &&
        this.current_status_value != status
      ) {
        if (this.current_status_value == 'Active' && status == 'Discharged') {
          this.showDialog = true;
          this.showconfirmDialog.showConfirmDialog(
          'Are you Sure, You want to Move the patient to Ready To Discharge',
            'Active Patient should be Moved to Ready To discharge Before Discharge',
            () => {
              this.ChangePatientStatus(status, timeofcall);
            }
          );
        } else {
          if (status == 'ReActive') {
            this.showDialog = true;
            this.showconfirmDialog.showConfirmDialog(
            'You want to change patient Status to Active? ',
              'Are you sure?',
              () => {
                this.ChangePatientStatus(status, timeofcall);
              }
            );
          } else {
            this.showDialog = true;
            this.showconfirmDialog.showConfirmDialog(
            'You want to change patient Status to ' + status + '? ',
              'Are you sure?',
              () => {
                this.ChangePatientStatus(status, timeofcall);
              }
            );
          }
        }
      }
    }

    // listen to response
    // this.dialogRef.afterClosed().subscribe((dialogResult: any) => {
    //   // if user pressed yes dialogResult will be true,
    //   // if he pressed no - it will be false

    //   if (dialogResult) {
    //     this.ChangePatientStatus(status, timeofcall);

    //   } else {
    //     return;
    //   }
    // });
  }

  MarkDeviceErrorDialog(index: any) {

    this.showconfirmDialog.showConfirmDialog(
    'You Want to Mark this Device as Error? ',
      'Are you sure?',
      () => {
        this.MarkError(index);
      }
    );
  }

  RemoveDeviceDialog(index: any) {
    this.showconfirmDialog.showConfirmDialog(
    'You Want to Remove Device from Patient? ',
      'Are you sure?',
      () => {
        this.ResetDevice(index);
      }
    );
  }

  Patientdata_Document: any;
  UpdatePatient_Document(PatientId: any, ProgramId: any) {
    //call on clicking on one patient row in the draft list, pass patient id from table to get the info
    var that = this;
    that.rpm
      .rpm_get(
        '/api/patient/getpatient?PatientId=' +
          PatientId +
          '&PatientprogramId=' +
          ProgramId
      )
      .then((data) => {
        that.Patientdata_Document = data;
        that.datasourceDocument =
          that.Patientdata_Document.PatientDocumentDetails.PatientDocumentinfos;
      });
  }
  Patientdata_Device: any;
  UpdatePatient_Device(PatientId: any, ProgramId: any) {
    //call on clicking on one patient row in the draft list, pass patient id from table to get the info
    var that = this;
    that.rpm
      .rpm_get(
        '/api/patient/getpatient?PatientId=' +
          PatientId +
          '&PatientprogramId=' +
          ProgramId
      )
      .then((data) => {
        that.Patientdata_Device = data;
        that.processMasterData();
        var patientdevicedetailsData =
          that.Patientdata_Device.PatientDevicesDetails.PatientDeviceInfos;
        var vitalArr = [];
        if (patientdevicedetailsData.length == 0) {
          for (let v of that.PatientVitalInfos) {
            var obj = {
              VitalName: v.VitalName,
              DeviceStatus: 'InActive',
              VitalId: v.VitalId,
              DeviceNumber: '',
            };
            vitalArr.push(obj);
          }
          that.patientdevicedetails = vitalArr;
        }
        if (patientdevicedetailsData.length != 0) {
          for (let v of that.PatientVitalInfos) {
            const isPresent = this.checkVitalName(
              patientdevicedetailsData,
              v.VitalName
            );

            if (isPresent) {
              var device = this.getVitalObject(
                patientdevicedetailsData,
                v.VitalName
              );
              vitalArr.push(...device);
            } else {
              var obj = {
                VitalName: v.VitalName,
                DeviceStatus: 'InActive',
                VitalId: v.VitalId,
                DeviceNumber: '',
              };
              if (!vitalArr.some((item) => item.VitalName == obj.VitalName)) {
                vitalArr.push(obj);
              }
            }
          }
          that.patientdevicedetails = vitalArr;
        }
        that.DeviceCheck();
        // this.DeviceNotAvaliable=true;
      });
  }

  Patientdata_Image: any;
  UpdatePatient_Image(PatientId: any, ProgramId: any) {
    //call on clicking on one patient row in the draft list, pass patient id from table to get the info
    var that = this;
    that.rpm
      .rpm_get(
        '/api/patient/getpatient?PatientId=' +
          PatientId +
          '&PatientprogramId=' +
          ProgramId
      )
      .then((data) => {
        that.Patientdata_Image = data;
        that.Patientdata.PatientDetails.Picture =
          that.Patientdata_Image.PatientDetails.Picture;
      });
  }
  diagnosisTitle: any;
  diagnosisChanged(event: any, index: any) {
    var DiagnosisSelected = this.Diagnosis_Name.filter(
      (data: { DiagnosisName: any }) => {
        return data == event;
      }
    );

    if (DiagnosisSelected.length > 0) {
      const found = this.Diagnosis_List.some(
        (el: { DiagnosisCode: void }) =>
          el.DiagnosisCode == DiagnosisSelected[0].DiagnosisCode
      );

      if (!found) {
        this.Diagnosis_List[index].DiagnosisCode =
          DiagnosisSelected[0].DiagnosisCode;
        this.Diagnosis_List[index].DiagnosisName =
          DiagnosisSelected[0].DiagnosisName;
      } else {
        alert('Diagnosis Already Selected');
        this.removeDiaganostics_Patient();
      }
    }
  }
  cleared(event: any, index: any) {
    this.Diagnosis_List[index].DiagnosisCode = null;
    this.Diagnosis_List[index].DiagnosisName = null;
  }
  clearedEdit(event: any, index: any) {
    this.diaganosisMainList[index].DiagnosisCode = null;
    this.diaganosisMainList[index].DiagnosisName = null;
  }
  keywordDignostics = 'DiagnosisField';
  keywordDignosticsEdit = 'DiagnosisFieldEdit';

  EditProgramdiagnosisChanged(event: any, index: number) {
    const DiagnosisSelected = this.editProgramDiagoList.filter(
      (data: any) => data === event
    );

    if (DiagnosisSelected.length === 0) return;

    const selected = DiagnosisSelected[0];

    const foundInMainList = this.diaganosisMainList.some(
      (el: { DiagnosisCode: string }) => el.DiagnosisCode === selected.DiagnosisCode
    );

    const selectedDiagnosesArray = this.editProgramdata.get('selectedDiagnoses') as FormArray;

    const foundInForm = selectedDiagnosesArray.controls.some(
      (ctrl: AbstractControl) => ctrl.value.DiagnosisCode === selected.DiagnosisCode
    );

    if (foundInForm || foundInMainList) {
      alert('Diagnosis Already Selected');
      this.programaremoveNewDiaganostic(); // Optional: only remove if you just added
      return;
    }

    // Update form group at index
    const formGroup = selectedDiagnosesArray.at(index) as FormGroup;
    formGroup.patchValue({
      DiagnosisCode: selected.DiagnosisCode,
      DiagnosisName: selected.DiagnosisName
    });
  }

  keyword_primary_insurance = 'VendorName';
  insurance_pid: any;
  insurance_pname: any;

  selectEvent__primary_Insurance(item: any) {
    // do something with selected item
    if (typeof item != 'string') {
      this.insurance_pid = item.VendorId;
      this.insurance_pname = item.VendorName;
    }
  }

  onChangeSearch_primary_Insurance(val: string) {
    // fetch remote data from here
    // And reassign the 'data' which is binded to 'data' property.
    this.insurance_pname = val;
  }

  onFocused_primary_Insurance(e: any) {
    // do something when input is focused
    //this.Scheduled_user = e
  }
  clearEvent_primary_Insurance(event: any) {
    this.insurance_pname = null;
  }
  keyword_secondary_insurance = 'VendorName';
  insurance_sid: any;
  insurance_sname: any;

  selectEvent__secondary_Insurance(item: any) {
    // do something with selected item
    if (typeof item != 'string') {
      this.insurance_sid = item.VendorId;
      this.insurance_sname = item.VendorName;
    }
  }

  onChangeSearch_secondary_Insurance(val: string) {
    // fetch remote data from here
    // And reassign the 'data' which is binded to 'data' property.
    this.insurance_sname = val;
  }

  onFocused_secondary_Insurance(e: any) {
    // do something when input is focused
    //this.Scheduled_user = e
  }
  clearEvent_secondary_Insurance(event: any) {
    this.insurance_sname = null;
  }
  keyword_ternary_insurance = 'VendorName';
  insurance_tid: any;
  insurance_tname: any;

  selectEvent__ternary_Insurance(item: any) {
    // do something with selected item
    if (typeof item != 'string') {
      this.insurance_tid = item.VendorId;
      this.insurance_tname = item.VendorName;
    }
  }

  onChangeSearch_ternary_Insurance(val: string) {
    // fetch remote data from here
    // And reassign the 'data' which is binded to 'data' property.
    this.insurance_tname = val;
  }

  onFocused_ternary_Insurance(e: any) {
    // do something when input is focused
    //this.Scheduled_user = e
  }
  clearEvent_ternary_Insurance(event: any) {
    this.insurance_tname = null;
  }

  HeightFeetValue = '0';
  HeightInchValue = '00';
  heightFeetArray = [
    {
      id: '0',
      data: '0',
    },
    {
      id: '1',
      data: '1',
    },
    {
      id: '2',
      data: '2',
    },
    {
      id: '3',
      data: '3',
    },
    {
      id: '4',
      data: '4',
    },
    {
      id: '5',
      data: '5',
    },
    {
      id: '6',
      data: '6',
    },
    {
      id: '7',
      data: '7',
    },
    {
      id: '8',
      data: '8',
    },
  ];
  heightInchArray = [
    {
      id: '00',
      data: '00',
    },
    {
      id: '01',
      data: '01',
    },
    {
      id: '02',
      data: '02',
    },
    {
      id: '03',
      data: '03',
    },
    {
      id: '04',
      data: '04',
    },
    {
      id: '05',
      data: '05',
    },
    {
      id: '06',
      data: '06',
    },
    {
      id: '07',
      data: '07',
    },
    {
      id: '08',
      data: '08',
    },

    {
      id: '09',
      data: '09',
    },
    {
      id: '10',
      data: '10',
    },

    {
      id: '11',
      data: '11',
    },
  ];
  futureDateError: boolean;
  today_date: any;
  checkDateValidity(date: any): boolean {
    this.today_date = this.datepipe.transform(new Date(), 'yyyy-MM-dd');

    const mxDate = new Date(this.today_date);
    const inputDate = new Date(date);

    if (inputDate > mxDate) {

      return (this.futureDateError = true);
    }
    return (this.futureDateError = false);
  }
  getDiagnosisControl(group: AbstractControl, key: string): FormControl {
    return group.get(key) as FormControl;
  }

  selectedDiagnoses: any;
  VitalId: number;

  get selectedDiagnosesArray(): FormArray {
    return this.editProgramdata.get('selectedDiagnoses') as FormArray;
  }

  getSelectedDiagnoses() {
    return this.editProgramdata.get('selectedDiagnoses') as FormArray;
  }
  showProgramEditModal = false;
  programNameEditPanel() {
    this.selectedProgramId = this.programForm.get('programname')?.value;
    const selectedDiagnosesArray = this.editProgramdata.get('selectedDiagnoses') as FormArray;
    selectedDiagnosesArray.clear(); // Clear old controls

    this.Diagnosis_List.forEach((diagnosis: { Id: any; DiagnosisName: any; DiagnosisCode: any; }) => {
      selectedDiagnosesArray.push(this.fb.group({
        Id: [diagnosis.Id],
        DiagnosisName: [diagnosis.DiagnosisName],
        DiagnosisCode: [diagnosis.DiagnosisCode]
      }));
    });
    this.selectedDiagnoses = selectedDiagnosesArray.value;
    // Store the current vital list before it gets changed
    this.vitalListDialog = this.vitalList;

    this.editProgramdata.patchValue({
      programname: this.selectedProgramId,
      vitalListDialog: this.vitalListDialog,
    });

    // Open the dialog after patching the values
    //this.dialog.open(this.programedit);
    if (this.programNameValueChangesSubscription) {
      this.programNameValueChangesSubscription.unsubscribe();
    }
    this.programNameValueChangesSubscription = this.editProgramdata.get('programname')?.valueChanges.subscribe((selectedProgramId) => {
      var ProgramSelected = this.master_data.ProgramDetailsMasterData.filter(
        (Pgm: { ProgramId: number }) => Pgm.ProgramId === parseInt(selectedProgramId)
      );

      if (ProgramSelected.length > 0) {
        this.ediProgramSelectedId = ProgramSelected;
        this.editProgramVital = this.ediProgramSelectedId[0].Vitals;
        this.oldVitalListDialog = this.vitalListDialog;
        this.matchingVitals = this.oldVitalListDialog.filter((vitalId: number) =>
          this.editProgramVital.some((vitallist: { VitalId: number }) => vitallist.VitalId === vitalId)
        );
        this.vitalListDialog = this.matchingVitals;
        // Call the vitalEditPgmListChange function when program name is changed
        this.vitalEditPgmListChange();
      }
    });
    this.showProgramEditModal = true;
    this.updateProgramDropdownState();
  }

  updateProgramDropdownState() {
    const shouldDisable =
      this.Patientdata?.PatientProgramdetails?.Status === 'Active' &&
      !!this.selectedProgram;

    if (shouldDisable) {
      this.editProgramdata.get('programname')?.disable({ emitEvent: false });
    } else {
      this.editProgramdata.get('programname')?.enable({ emitEvent: false });
    }
  }

  trackByVitalId(index: number, item: any): any {
    return item.VitalId;  // Return unique identifier for each option
  }
  editProgramdata = new UntypedFormGroup({
    programname: new UntypedFormControl(null, [Validators.required]),
  });

  isFormDirty(): boolean {
    const currentProgramName = this.editProgramdata.controls.programname.value;
    const currentVitals = this.editProgramdata.controls.vitalListDialog.value;
    const currentDiagnoses = (this.editProgramdata.get('selectedDiagnoses') as FormArray).value;

    const vitalsChanged = !this.arrayEqual(this.initiallySelectedVitals, currentVitals);
    const programChanged = Number(this.program_id) !== Number(currentProgramName);
    const diagnosisChanged = !this.arrayEqual(currentDiagnoses, this.Diagnosis_List);

    return vitalsChanged || programChanged || diagnosisChanged;
  }

  arrayEqual(a: any[], b: any[]): boolean {
    // If the arrays have different lengths, they're not equal
    if (a.length !== b.length) return false;

    for (let i = 0; i < a.length; i++) {
      // If the elements are objects, compare their properties
      if (typeof a[i] === 'object' && typeof b[i] === 'object') {
        // If the element is an object, compare the properties deeply (you can customize this based on the structure of the object)
        if (!this.deepEqual(a[i], b[i])) {
          return false;
        }
      } else {
        // If the elements are primitives, compare them directly
        if (a[i] !== b[i]) {
          return false;
        }
      }
    }
    return true;
  }
  // Helper function to deeply compare two objects (used for complex types)
  deepEqual(obj1: any, obj2: any): boolean {
    // If the objects are not the same reference, compare their properties
    if (obj1 === obj2) return true;

    // If either is not an object, they can't be equal
    if (typeof obj1 !== 'object' || typeof obj2 !== 'object' || obj1 === null || obj2 === null) {
      return false;
    }

    const keys1 = Object.keys(obj1);
    const keys2 = Object.keys(obj2);

    if (keys1.length !== keys2.length) return false;

    // Compare each key and value recursively
    for (let key of keys1) {
      if (!keys2.includes(key) || !this.deepEqual(obj1[key], obj2[key])) {
        return false;
      }
    }

    return true;
  }

  newProgramid: any;
  EditProgramData() {
    const selectedDiagnosesArray = this.editProgramdata.get('selectedDiagnoses') as FormArray;
    const selectedDiagnoses = selectedDiagnosesArray.value;
    const vitalList = this.editProgramdata.controls.vitalListDialog.value;

    if (
      selectedDiagnoses.length > 0 &&
      selectedDiagnoses[0].DiagnosisName &&
      selectedDiagnoses[0].DiagnosisCode &&
      vitalList.length > 0
    ) {
      const req_body: any = {
        PatientId: parseInt(this.pid),
        PatientProgramId: parseInt(this.patientprogramid),
        ProgramId: parseInt(this.editProgramdata.controls.programname.value),
        GoalDetails: this.editProgramGoal,
        ProgramDiagnosis: selectedDiagnoses,
        VitalIds: vitalList
      };

      if (!this.DeviceCheckOnSubmit()) return;

      this.rpm.rpm_post('/api/patient/updatepatientprogramdetails', req_body).then(
        (data) => {
          alert('Program Changed Successfully !!');
          this.router.navigate(['/admin/patients_detail'], {
            queryParams: { id: this.pid, programId: this.patientprogramid },
            skipLocationChange: true
          });
          this.dialog.closeAll();
          this.editProgramdata.controls['programname'].setValue(0);
          this.diaganosisMainList = [{ DiagnosisName: '', DiagnosisCode: '' }];
          this.editProgramDiagoList = [];
        },
        (err) => {
          alert('Something Went Wrong!!');
          this.diaganosisMainList = [{ DiagnosisName: '', DiagnosisCode: '' }];
        }
      );
    } else {
      alert('Please Complete the Form!!');
      this.vitalEditPgmListChange();
      this.editProgramDiagoList = [];
    }
  }

  EditProgramCancel() {
    this.showProgramEditModal = false;  // Hide the modal
    if (this.programNameValueChangesSubscription) {
      this.programNameValueChangesSubscription.unsubscribe();
    }
  }

  vitalListChange() {
    this.loading = true;
    var req_body = { VitalIds: this.vitalList };
    this.rpm.rpm_post(`/api/patient/getdiagnosiscodebyvitalid`, req_body).then(
      (data) => {
        this.Diagnosis_Name = data;
        this.loading = false;
        // New Code Change 21/04/2023
        for (let x of this.Diagnosis_Name) {
          x.DiagnosisField = x.DiagnosisName + '-' + x.DiagnosisCode;
        }
        for (let x of this.Diagnosis_Name) {
          x.DiagnosisField = x.DiagnosisName + '-' + x.DiagnosisCode;
        }
      },
      (err) => {
        alert('could not find Diagnosis for the selected Vitals/Conditions');
        this.loading = false;
      }
    );
  }
  getLastPatientStatus() {
    this.rpm
      .rpm_get(
        `/api/patient/getpatientlastpgmstatus?PatientId=${this.pid}&PatientProgramId=${this.patientprogramid}`
      )
      .then((data:any) => {
        this.last_status = data.message;
      });
  }

  inactive_img: any;
  disableValue: any;

  ActivePanelBar() {
    if (
      this.current_status_value == 'OnHold' ||
      this.status_to_send == 'OnHold'
    ) {
      this.disableValue = true;
      this.intializeImages();
    }
    if (
      this.current_status_value == 'Discharged' ||
      this.status_to_send == 'Discharged'
    ) {
      this.disableValue = true;
      this.intializeImages();
    }
    return this.disableValue;
  }

  getStatusValue() {
    var displayValue;
    switch (this.current_status_value) {
      case 'Active':
        displayValue = 'Active';
        break;
      case 'Enrolled':
        displayValue = 'Active';
        break;
      case 'Prescribed':
        displayValue = 'Active';
        break;
      case 'ReadyToDischarge':
        if (
          this.last_status == 'InActive' &&
          this.status_to_send == 'InActive'
        ) {
          displayValue = 'InActive';
        } else {
          displayValue = 'Active';
        }
        break;
      case 'InActive':
        displayValue = 'InActive';
        break;
      case 'OnHold':
        if (
          (this.last_status == 'InActive' &&
            this.status_to_send == 'InActive') ||
          (this.last_status == 'InActive' && this.status_to_send == 'OnHold')
        ) {
          displayValue = 'InActive';
        } else {
          displayValue = 'Active';
        }
        break;
    }

    return displayValue;
  }

  OnClickActive() {
    if (
      this.current_status_value == 'OnHold' &&
      this.last_status == 'InActive'
    ) {
      this.ConfirmChangePatientStatus('InActive', 2);
    } else {
      this.ConfirmChangePatientStatus('Active', 2);
    }
  }

  setInActive() {
    this.status_to_send = 'InActive';
    this.active_img = this.bold_icon;
  }
  convertToLocalTime(stillUtc: any) {
    if (stillUtc.includes('+')) {
      var temp = stillUtc.split('+');

      stillUtc = temp[0];
    }
    stillUtc = stillUtc + 'Z';
    const local = dayjs.utc(stillUtc).local().format('YYYY-MM-DD HH:mm:ss');
    return local;
  }

  vitalEditPgmListChange() {
    this.loading = true;
    const req_body = { VitalIds: this.vitalListDialog };

    this.rpm.rpm_post(`/api/patient/getdiagnosiscodebyvitalid`, req_body).then(
      (data) => {
        this.editProgramDiagoList = data;
        this.loading = false;

        for (let x of this.editProgramDiagoList) {
          x.DiagnosisFieldEdit = x.DiagnosisName + '-' + x.DiagnosisCode;
        }

        const selectedDiagnosesArray = this.editProgramdata.get('selectedDiagnoses') as FormArray;

        let j = 0;
        while (j < selectedDiagnosesArray.length) {
          const currentDiagnosis = selectedDiagnosesArray.at(j).value;
          const match = this.editProgramDiagoList.some(
            (            x: { DiagnosisCode: any; }) => x.DiagnosisCode === currentDiagnosis.DiagnosisCode && currentDiagnosis.DiagnosisCode?.trim() !== ''
          );

          if (!match) {
            selectedDiagnosesArray.removeAt(j); // Remove using FormArray API
          } else {
            j++;
          }
        }

        // Add default if no diagnoses left
        if (selectedDiagnosesArray.length === 0) {
          selectedDiagnosesArray.push(
            this.fb.group({
              Id: [0],
              DiagnosisName: [null],
              DiagnosisCode: [null],
            })
          );
        }

        // Optionally update your internal model
        this.selectedDiagnoses = selectedDiagnosesArray.value;
      },
      (err) => {
        alert('Could not find Diagnosis for the selected Vitals/Conditions');
        this.loading = false;
      }
    );
  }

  deleteDocument(documentId: any) {
    this.rpm
      .rpm_post(
        `/api/patient/deletepatientdocuments?documentid=${documentId}`,
        {}
      )
      .then(
        (data) => {
          alert('Document Delete Successfully');
          this.getReload();
        },
        (err) => {
          console.log(err);
        }
      );
  }

  openDeleteDocumentDialog(documnetId: any) {

    this.showDialog = true;
    this.showconfirmDialog.showConfirmDialog(
     'Do You Want to Delete the Document ?',
      'Are you sure?',
      () => {
        this.deleteDocument(documnetId);
      }
    );
  }

  http_data: any;
  getReload() {
    this.rpm
      .rpm_get(
        `/api/patient/getpatient?PatientId=${this.pid}&PatientprogramId=${this.patientprogramid}`
      )
      .then((data) => {
        var that = this;
        this.http_data = data;
        this.datasourceDocument =
          this.http_data.PatientDocumentDetails.PatientDocumentinfos;
      });
  }
  languageList: any;
  getLanguage() {
    var that = this;
    this.rpm.rpm_get('/api/users/getlanguages').then(
      (data) => {
        that.languageList = data;
      },
      (err) => {
        console.log(err.error);
      }
    );
  }
  rangeControl = new FormControl<[number, number]>([this.minValue, this.maxValue]);
  // Manjusha code change
  isDropdownDisabled(): boolean {
    return this.Patientdata?.PatientProgramdetails?.Status === 'Active' && !!this.selectedProgram;
  }

  isOptionDisabled(vitalId: number): boolean {
    // Disable the option only if it's selected and the status is 'Active'
    return (this.Patientdata.PatientProgramdetails.Status === 'Active' &&
            !!this.selectedProgram &&
            this.initiallySelectedVitals.includes(vitalId));  // Check if the vitalId is selected
  }

  getUniqueVitals() {
    const seen = new Set();
    return this.patientdevicedetails.filter((device: { VitalName: string; }) => {
      const key = device.VitalName?.toLowerCase();
      if (!key || seen.has(key)) return false;
      seen.add(key);
      return true;
    });
  }

  onVitalChange(device: any, event: Event) {
    const selectedValue = (event.target as HTMLSelectElement).value;
    const selectedId = Number(selectedValue);

    const selectedVital = this.getUniqueVitals().find((v: { VitalId: number }) => v.VitalId === selectedId);
    if (!selectedVital) return;

    const selectedVitalName = selectedVital.VitalName.toLowerCase();

    const alreadyActive = this.patientdevicedetails.some(
      (d: any) =>
        d.DeviceStatus === 'Active' &&
        d.VitalName.toLowerCase() === selectedVitalName
    );

    if (alreadyActive) {
      alert(
        'This vital already has an active device assigned. Please choose a different vital or remove the existing device before adding a new one.'
      );

      // Temporarily mark invalid and clear value
      device.invalidSelection = true;
      device.VitalId = null;

      // Force a refresh after short delay
      setTimeout(() => {
        device.invalidSelection = false;
      });

      return;
    }

    // If valid, update previousVitalId
    device.previousVitalId = selectedId;
  }
}
