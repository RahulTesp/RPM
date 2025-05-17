import { Component, OnInit, TemplateRef, ViewChild } from '@angular/core';
import { MatDialog, MatDialogConfig } from '@angular/material/dialog';
import {
  UntypedFormControl,
  UntypedFormBuilder,
  UntypedFormGroup,
  Validators,
  UntypedFormArray,
  FormControl,
} from '@angular/forms';
import { RPMService } from '../../sevices/rpm.service';
import { ActivatedRoute, Router } from '@angular/router';
import _ from 'lodash';
import { StatusMessageComponent } from '../../shared/status-message/status-message.component';
import { DatePipe } from '@angular/common';
import * as uuid from 'uuid';
import moment from 'moment';
import { AuthService } from 'src/app/services/auth.service';
import { ConfirmDialogServiceService } from '../../shared/confirm-dialog-panel/service/confirm-dialog-service.service';


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

  vitalList = [];
  editvitalList = [];
  documentType: any;
  public showDialog = false;
  public confirmDialogTitle: string;
  public confirmDialogMessage: string;
  confirmAction: (() => void) | null = null;

  constructor(
    private _route: ActivatedRoute,
    public dialog: MatDialog,
    private PatientFormBuilderObj: UntypedFormBuilder,
    private rpm: RPMService,
    private router: Router,
    public datepipe: DatePipe,
    private Auth: AuthService,
     private showconfirmDialog: ConfirmDialogServiceService
  ) {
    var that = this;
    this.initForms();
    this.intializeImages();
    this.ImageStatus = false;
    this.discharged_img = this.unbold_icon;
    // this.ImageStatus = true;
    this.diaganosisMainList = [{ DiagnosisName: '', DiagnosisCode: '' }];

    // this.editProgramdata.get('programname')?.valueChanges.subscribe((val) => {
    //   var ProgramSelected = that.master_data.ProgramDetailsMasterData.filter(
    //     (Pgm: { ProgramId: number }) => Pgm.ProgramId === parseInt(val)
    //   );
    //   this.ediProgramSelectedId = ProgramSelected;
    //   that.editProgramGoal = ProgramSelected[0].goalDetails;
    //   that.editProgramDiagoList = ProgramSelected[0].Vitals[0].Dignostics;

    // });
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
  ngOnInit(): void {
    this.variable = 1;
    this.documentAddFlag = false;
    this.programnamechangefirst = true;
    this.cityFlag = false;
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
          that.vitalList = [];
        }

        that.editProgramGoal = ProgramSelected[0].goalDetails;
        that.editProgramDiagoList = ProgramSelected[0].Vitals[0].Dignostics;
        for (let x of this.editProgramDiagoList) {
          x.DiagnosisFieldEdit = x.DiagnosisName;
        }
      }
      this.diaganosisMainList = [{ DiagnosisName: '', DiagnosisCode: '' }];
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
      // if(that.durationValue > 1 && that.durationValue <=12)
      // {
      if (this.programForm.controls.startdate.value) {
        this.calculateEndDate();
      }

      // }else{
      //   alert('Duration Period is Maximum 12 months')
      // }
    });

    // this.programForm.get('enddate')?.valueChanges.subscribe((val) => {
    //     if (
    //       this.programForm.controls.enddate.value &&
    //       this.programForm.controls.startdate.value
    //     ) {
    //       that.durationValue = this.monthDiff(
    //         this.programForm.controls.startdate.value,
    //         this.programForm.controls.enddate.value
    //       );
    //     }
    // });
    this.programForm.get('programname')?.valueChanges.subscribe((value) => {
      that.Program_Selected = that.master_data.ProgramDetailsMasterData.filter(
        (Pgm: { ProgramId: number }) => Pgm.ProgramId === parseInt(value)
      );
      if (that.Program_Selected.length > 0) {
        that.vital_selected = that.Program_Selected[0].Vitals;
      }
      if (!this.programnamechangefirst_1) {
        that.vitalList = [];
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
    that.CommunicationType = that.master_data.DeviceCommunicationTypes;
    that.deviceTypeDataId = 1;

    this.devArr = [];
    this.careTeamMembers = this.master_data.CareTeamMembers;
    var myuser = sessionStorage.getItem('user_name');
    var myid = sessionStorage.getItem('userid');

    // this.careTeamMembers.push(myobj)
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
      consultdate: new UntypedFormControl('', [Validators.required]),
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
        that.PatientVitalInfos_copy =
          that.Patientdata.PatientVitalDetails.PatientVitalInfos;
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
        // that.programForm.controls['target'].setValue(that.Patientdata.PatientProgramdetails.TargetReadings);
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

        that.programForm.controls['consultdate'].setValue(
          this.convertDate(consultDateData)
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
              // this.billingSrcArray = this.billingCCM_C;
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
              // this.billingSrcArray = this.billingCCM_C;
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
              // this.billingSrcArray = this.billingCCM_C;
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
              // this.billingSrcArray = this.billingCCM_C;
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

        that.patientdevicedetails =
          that.Patientdata.PatientDevicesDetails.PatientDeviceInfos;
        if (that.patientdevicedetails.length == 0) {
          var vitalArr = [];
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
        that.DeviceCheck();
        that.DiagnosisInfos =
          that.Patientdata.PatientPrescribtionDetails.PatientDiagnosisInfos;
        that.InsuranceInfo =
          that.Patientdata.PatientInsurenceDetails.PatientInsurenceInfos;
        that.EnrolmentDetails = that.Patientdata.PatientEnrolledDetails;
        console.log("EnrolmentDetails");
        console.log(that.EnrolmentDetails);
        that.ActiveDetails = that.Patientdata.ActivePatientDetails;
        that.ReadyToDischarge =
          that.Patientdata.ReadyForDischargePatientDetails;
        that.OnHold = that.Patientdata.OnHoldPatientDetais;
        that.InActive = that.Patientdata.InActivePatientDetais;
        that.Discharged = that.Patientdata.DischargedPatientDetails;
        that.vitalListChange();
      });
  }
  codeSelected: any;
  DeviceCheck() {
    for (let x of this.patientdevicedetails) {
      if (x.DeviceStatus == 'Active') {
        this.DeviceNotAvaliable = false;
        break;
      } else {
        this.DeviceNotAvaliable = true;
      }
    }
  }
  a: any;
  vitalschedule: any;
  InsuranceInfo: any;
  addNewDevice() {
    var obj = {
      VitalName: '',
      DeviceStatus: 'InActive',
      VitalId: '',
      DeviceNumber: '',
    };
    if (this.patientdevicedetails.length < 5) {
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
      for (let x of this.Diagnosis_Name) {
        x.DiagnosisField = x.DiagnosisName;
        //alert(x.DiagnosisField)
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
    // if(this.DurationEdit)
    // {
    if (this.durationValue < 12) {
      this.durationValue++;
    } else {
      this.durationValue = 12;
    }

    this.calculateEndDate();
    // }
  }
  decrement_duration() {
    // this.durationStatusEnable();
    // if(this.DurationEdit)
    // {

    if (this.durationValue > 1) {
      this.durationValue--;
    } else {
      this.durationValue = 1;
    }
    this.calculateEndDate();
    // }
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
    var someDateValue = moment(someDate).add(this.durationValue, 'M');
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
  // options: Options = {
  //   floor: 0,
  //   step: 1,
  //   ceil: 200,
  //   showTicks: false,
  // };
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
      if (this.Diagnosis_List.length < 5) {
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
  programaddNewDiaganostic() {
    // if(this.diaganosisMainList.length<2){
    //   this.diaganosisMainList.push(this.editObjDiagnosis);
    // }

    this.editObjDiagnosis = { Id: 0, DiagnosisCode: null, DiagnosisName: null };

    if (
      this.diaganosisMainList[this.diaganosisMainList.length - 1]
        .DiagnosisCode != null &&
      this.diaganosisMainList[this.diaganosisMainList.length - 1]
        .DiagnosisName != null
    ) {
      if (this.diaganosisMainList.length < 5) {
        for (let i = 0; i <= this.diaganosisMainList.length; i++) {
          this.diaganosisMainList.push(this.editObjDiagnosis);
          this.display = !this.display;
          return;
        }
      }
    }
  }
  programaremoveNewDiaganostic() {
    if (this.diaganosisMainList.length > 1) {
      this.diaganosisMainList.pop();
      this.display = !this.display;
    }
  }
  confirm_action() {
    if (this.variable == 1) {
      this.UpdatePatientInfo();
      this.editPatientProgram();
    } else if (this.variable == 2) {
      this.UpdatePatientInfo();
      this.editPatientProgram();
    }
  }
  UpdatePatientInfo() {
    this.markFormGroupTouched(this.PatientInfoForm);

    //call on clicking savedraft after opening the drafted patient
    //if (this.PatientInfoForm.valid){

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
      console.log(req_body);
      var that = this;
      this.rpm.rpm_post('/api/patient/updatepatient', req_body).then(
        (data) => {
          this.loading = false;
          if (this.variable != 2) {
            this.dialog.closeAll();
            this.redirect_patient();
          }

          // alert("Success, Patient Updated Successfully..!")
          //show success popup patient is updated
        },
        (err) => {
          this.loading = false;
          if (this.variable != 2) {
            this.dialog.closeAll();
            this.redirect_patient();
          }
          // alert("Error, Patient details update failed..!")
          //show error pop up, could not update patient
        }
      );
    } else {
      alert('Please complete all mandatory fields..!');
      this.loading = false;
    }
    // }
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
    // var status = "Prescribed";

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
          // this.unsetDischarged();
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
          // this.unsetDischarged();
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
          // this.unsetDischarged();
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
          // this.unsetDischarged();
          this.isPrescribed = true;
          this.isEnrolled = true;
          this.isActive = true;
          this.isReadyToDischarge = true;
          this.isDischarged = false;
        }
        // if (
        //   (status == 'Discharged' || status == 'OnHold') &&
        //   !this.isDischarged
        // ) {
        //   this.unsetPrescribed();
        //   this.unsetEnrolled();
        //   this.unsetActive();
        //   this.unsetReadyToDischarge();
        //   this.setDischarged();
        //   this.isPrescribed = true;
        //   this.isEnrolled = true;
        //   this.isActive = true;
        //   this.isReadyToDischarge = true;
        //   this.isDischarged = true;
        // }
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
          // this.unsetDischarged();
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
          // this.unsetDischarged();
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
          // this.unsetDischarged();
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
          // this.unsetDischarged();
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
          // this.unsetDischarged();
          this.isPrescribed = true;
          this.isEnrolled = true;
          this.isActive = true;
          this.isReadyToDischarge = true;
          this.isDischarged = false;
          this.ImageStatus = true;
        }
        // if (
        //   (status == 'Discharged' || status == 'OnHold') &&
        //   !this.isDischarged &&
        //   this.resp.Status == 'ReadyToDischarge'
        // ) {
        //   this.unsetPrescribed();
        //   this.unsetEnrolled();
        //   this.unsetActive();
        //   this.unsetReadyToDischarge();
        //   this.setDischarged();
        //   this.isPrescribed = true;
        //   this.isEnrolled = true;
        //   this.isActive = true;
        //   this.isReadyToDischarge = true;
        //   this.isDischarged = true;
        // }
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

    // this.onHoldPanelImgChange();

    // this.prescribed_img = this.inactive_icon;
    // this.enrolled_img = this.inactive_icon;
    // this.active_img = this.inactive_icon;
    // this.readytodischarge_img = this.inactive_icon;
    // this.discharged_img = this.inactive_icon;
    // this.isPrescribed = false;
    // this.isEnrolled = false;
    // this.isActive = false;
    // this.isReadyToDischarge = false;
    // this.isDischarged = false;
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
    // dateval = mm2 + '/' + dd2 + '/' + yyyy
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
    // dateval = yyyy + '-' + mm2 + '-' + dd2;
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
      // if(this.insurance_pname!="" &&this.isDeviceActive){//&&this.Patientdata_Document.PatientDocumentDetails.PatientDocumentinfos.length>0){
      //   console.log("All Patient Details are Verified")
      // }
      // else{
      //   alert("Please complete Device Registration/Primary Insurance to Make a Patient Active...!")
      //   return;
      // }
      if (
        this.Patientdata &&
        this.Patientdata.PatientProgramdetails.ProgramName != 'CCM' &&
        this.Patientdata.PatientProgramdetails.ProgramName != 'PCM'
      ) {
        if (this.isDeviceActive) {
          //&&this.Patientdata_Document.PatientDocumentDetails.PatientDocumentinfos.length>0){
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
    // var diagnostics = this.processDiagnostics(this.programForm.controls.diagnosisDataList.value);
    var diagnosis = this.Diagnosis_List;
    // var insuranceDeails = this.processInsurance(this.programForm_3.controls.insurance1.value,this.programForm_3.controls.insurance2.value,this.programForm_3.controls.insurance3.value)

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
      // ConsultationDate: this.Auth.ConvertToUTCRangeInput(
      //   new Date(this.programForm.controls.consultdate.value + 'T00:00:00')
      // ),
      //26/10/2023
      ConsultationDate: this.programForm.controls.consultdate.value,
      CareTeamUserId: this.programForm.controls.assignedMember.value,
      PatientStatus: this.status_to_send,
      // TargetReadings: this.programForm.controls.target.value,
      // StartDate: this.programForm.controls.startdate.value,
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
    if ((this.current_status_value = 'Prescribed')) {
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
            // that.dialog.open(that.successTemplate);
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
    // this.ReloadDeviceList(1);
    // this.ReloadDeviceList(2);
    // this.ReloadDeviceList(3);
    // this.ReloadDeviceList(4);

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
        (data) => {
          // this.openDialogWindow('Success', `Device Added Successfully!!!`);
          this.showconfirmDialog.showConfirmDialog(
            'Device Added Successfully!!!',
            'Success',
            () => {
              this.ReloadDeviceList(1);
              this.ReloadDeviceList(2);
              this.ReloadDeviceList(3);
              this.ReloadDeviceList(4);
            },
            false
          );
        },
        (err) => {
          // this.openDialogWindow('Error', `Device not added to user assets!!!`);
          this.showconfirmDialog.showConfirmDialog(
            'Device not added to user assets!!!',
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
        (data) => {
          this.showconfirmDialog.showConfirmDialog(
            'Device Error Updated Successfully.',
            'Success',
            () => {
              this.ReloadDeviceList(1);
              this.ReloadDeviceList(2);
              this.ReloadDeviceList(3);
              this.ReloadDeviceList(4);
            },
            false
          );
        },
        (err) => {
          this.showconfirmDialog.showConfirmDialog(
            'Failed to Update Device Error',
            'Error',
            () => {
              this.ReloadDeviceList(1);
              this.ReloadDeviceList(2);
              this.ReloadDeviceList(3);
              this.ReloadDeviceList(4);
            },
            false
          );
          // this.openDialogWindow('Error', `Failed to Update Device Error.`);
          //show error pop up, could not update patient
        }
      );
    }
  }
  RemoveDevice(index: any) {
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

      this.rpm.rpm_post('/api/device/removedevice/iglucose', req_body).then(
        (data) => {
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
          //show success popup patient is updated
        },
        (err) => {
          if (!this.ErrorFlag) {
            this.showconfirmDialog.showConfirmDialog(
              'Device not removed from user assets.',
              'Error',
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
          // this.reloadMasterData();
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
            },
            false
          );
        },
        (err) => {
          // this.openDialogWindow('Error',`Device not removed from user assets.`);
          this.showconfirmDialog.showConfirmDialog(
            'Device not removed from user assets.',
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
        (err) => {
          // this.openDialogWindow('Error', `Device Test Failed.`);
          this.showconfirmDialog.showConfirmDialog(
            'Device Test Failed.',
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

    // dialogConfig.disableClose = true;
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

    // a[0].setAttribute("style", "background-image:"+this.image.name);
    // a[0].setAttribute("style", "background: url(\"https://rpmstorage123.blob.core.windows.net/rpmprofilepictures/CL500626\"); background-repeat: no-repeat;  background-size: 100% 100%;");
  }
  openFileData(e: any) {
    this.Doc = e.target.files[0];
    this.documentType = this.Doc.type;
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
      this.Doc != undefined
    ) {
      if (this.documentType != 'application/pdf') {
        alert('Please Select Pdf File Type');
        this.DownloadStatus = false;
        return;
      } else {
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
      }
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
    this.submitImage(this.pid);
  }
  submitImage(pid: any) {
    const myPhoto = uuid.v4();
    var formData: any = new FormData();
    formData.append(myPhoto, this.image);
    var that = this;
    this.rpm.rpm_post(`/api/patient/addimage?PatientId=${pid}`, formData).then(
      (data) => {
        that.UpdatePatient_Image(this.pid, this.patientprogramid);
        // that.PatientInfoForm.controls['clinicname'].setValue(
        //   this.cname.toString(),
        //   { onlySelf: true }
        // );
      },
      (err) => {
        alert('Error While Adding Image....!');
      }
    );
  }

  downloadFile(FileName: any) {
    // FileSaver.saveAs("ConsentForm", FileName);
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
    // let's call our modal window
    // if (status == 'OnHold' && timeofcall == 2) {
    //   this.ImageStatus = false;
    // }
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
        // this.dialogRef = this.dialog.open(ConfirmDialogComponent, {
        //   maxWidth: '400px',
        //   data: {
        //     title: 'Are you sure?',
        //     message: ' Do you want to disCharge the patient ',
        //   },
        // });
        this.showDialog = true;
        this.showconfirmDialog.showConfirmDialog(
        'Do you want to disCharge the patient ?',
          'Are you sure?',
          () => {
            this.ChangePatientStatus(status, timeofcall);
          }
        );
      } else {
        // this.dialogRef = this.dialog.open(ConfirmDialogComponent, {
        //   maxWidth: '400px',
        //   data: {
        //     title: 'Are you sure?',
        //     message: 'Do you want to release from Onhold ? ',
        //   },
        // });

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
          // this.dialogRef = this.dialog.open(ConfirmDialogComponent, {
          //   maxWidth: '400px',
          //   data: {
          //     title:
          //       'Active Patient should be Moved to Ready To discharge Before Discharge',
          //     message:
          //       'Are you Sure, You want to Move the patient to Ready To Discharge',
          //   },
          // });
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
            // this.dialogRef = this.dialog.open(ConfirmDialogComponent, {
            //   maxWidth: '400px',
            //   data: {
            //     title: 'Are you sure?',
            //     message: 'You want to change patient status to Active ? ',
            //   },
            // });
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
            // this.dialogRef = this.dialog.open(ConfirmDialogComponent, {
            //   maxWidth: '400px',
            //   data: {
            //     title: 'Are you sure?',
            //     message:
            //       'You want to change patient Status to ' + status + '? ',
            //   },
            // });
          }
        }
      }
    }

    // listen to response
    this.dialogRef.afterClosed().subscribe((dialogResult: any) => {
      // if user pressed yes dialogResult will be true,
      // if he pressed no - it will be false

      if (dialogResult) {
        this.ChangePatientStatus(status, timeofcall);
        // this.paneldialogVariable = true;
      } else {
        return;
      }
    });
  }

  MarkDeviceErrorDialog(index: any) {
    // const dialogRef = this.dialog.open(ConfirmDialogComponent, {
    //   maxWidth: '500px',
    //   data: {
    //     title: 'Are you sure?',
    //     message: 'You Want to Mark this Device as Error? ',
    //   },
    // });
    // dialogRef.afterClosed().subscribe((dialogResult) => {

    //   if (dialogResult) {
    //     this.MarkError(index);
    //   } else {
    //   }
    // });

    this.showconfirmDialog.showConfirmDialog(
    'You Want to Mark this Device as Error? ',
      'Are you sure?',
      () => {
        this.MarkError(index);
      }
    );
  }

  RemoveDeviceDialog(index: any) {
    // const dialogRef = this.dialog.open(ConfirmDialogComponent, {
    //   maxWidth: '500px',
    //   data: {
    //     title: 'Are you sure?',
    //     message: 'You Want to Remove Device from Patient? ',
    //   },
    // });

    // dialogRef.afterClosed().subscribe((dialogResult) => {

    //   if (dialogResult) {
    //     this.ResetDevice(index);
    //   } else {
    //   }
    // });

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
        that.patientdevicedetails =
          that.Patientdata_Device.PatientDevicesDetails.PatientDeviceInfos;
        if (that.patientdevicedetails.length == 0) {
          var vitalArr = [];
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
  // diagnosisChanged(event: any, index: any) {
  //   var DiagnosisSelected = this.Diagnosis_Name.filter(
  //     (data: { DiagnosisName: any }) => {
  //       return data.DiagnosisName == event;
  //     }
  //   );
  //   if (DiagnosisSelected.length > 0) {
  //     this.Diagnosis_List[index].DiagnosisCode =
  //       DiagnosisSelected[0].DiagnosisCode;
  //   }
  // }

  diagnosisChanged(event: any, index: any) {
    var DiagnosisSelected = this.Diagnosis_Name.filter(
      (data: { DiagnosisName: any }) => {
        // return data.DiagnosisName==event
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

  // EditProgramdiagnosisChanged(event: any, index: any) {
  //   var DiagnosisSelected = this.editProgramDiagoList.filter(
  //     (data: { DiagnosisName: any }) => {
  //       return data.DiagnosisName == event;
  //     }
  //   );
  //   if (DiagnosisSelected.length > 0) {
  //     this.diaganosisMainList[index].DiagnosisCode =
  //       DiagnosisSelected[0].DiagnosisCode;
  //   }
  // }

  EditProgramdiagnosisChanged(event: any, index: any) {
    var DiagnosisSelected = this.editProgramDiagoList.filter(
      (data: { DiagnosisName: any }) => {
        return data == event;
      }
    );

    if (DiagnosisSelected.length > 0) {
      const found = this.diaganosisMainList.some(
        (el: { DiagnosisCode: void }) =>
          el.DiagnosisCode == DiagnosisSelected[0].DiagnosisCode
      );

      if (!found) {
        this.diaganosisMainList[index].DiagnosisCode =
          DiagnosisSelected[0].DiagnosisCode;
        this.diaganosisMainList[index].DiagnosisName =
          DiagnosisSelected[0].DiagnosisName;
      } else {
        alert('Diagnosis Already Selected');
        this.programaremoveNewDiaganostic();
      }
    }
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
      // alert("Please Enter a Valid Date")

      return (this.futureDateError = true);
    }
    return (this.futureDateError = false);
  }

  programNameEditPanel() {
    // Edit Program
    var that = this;
    that.editProgramVital = [];
    this.editProgramdata.get('programname')?.valueChanges.subscribe((val) => {
      var ProgramSelected = that.master_data.ProgramDetailsMasterData.filter(
        (Pgm: { ProgramId: number }) => Pgm.ProgramId === parseInt(val)
      );

      if (ProgramSelected.length > 0) {
        this.ediProgramSelectedId = ProgramSelected;
        that.editProgramVital = this.ediProgramSelectedId[0].Vitals;
        if (!this.programnamechangefirst_2) {
          that.editvitalList = [];
        }

        that.editProgramGoal = ProgramSelected[0].goalDetails;
        that.editProgramDiagoList = ProgramSelected[0].Vitals[0].Dignostics;
      }
      this.programnamechangefirst_2 = false;
    });

    this.editvitalList = [];

    this.dialog.open(this.programedit);
  }
  editProgramdata = new UntypedFormGroup({
    programname: new UntypedFormControl(null, [Validators.required]),
  });

  newProgramid: any;
  EditProgramData() {
    var req_body: any = {};

    if (
      this.diaganosisMainList[0].DiagnosisName != '' &&
      this.diaganosisMainList[0].DiagnosisName != null &&
      this.diaganosisMainList[0].DiagnosisCode != '' &&
      this.diaganosisMainList[0].DiagnosisCode != null &&
      this.editvitalList.length > 0
    ) {
      req_body['PatientId'] = parseInt(this.pid);

      req_body['PatientProgramId'] = parseInt(this.patientprogramid);
      req_body['ProgramId'] = parseInt(
        this.editProgramdata.controls.programname.value
      );
      req_body['GoalDetails'] = this.editProgramGoal;
      req_body['ProgramDiagnosis'] = this.diaganosisMainList;
      req_body['VitalIds'] = this.editvitalList;
      this.rpm
        .rpm_post('/api/patient/updatepatientprogramdetails', req_body)
        .then(
          (data) => {
            this.newProgramid = data;

            alert('Program Changed Successfully !!');

            let route = '/admin/patients_detail';
            this.router.navigate([route], {
              queryParams: { id: this.pid, programId: this.patientprogramid },
              skipLocationChange: true,
            });

            this.dialog.closeAll();

            this.editProgramdata.controls['programname'].setValue(0);

            this.diaganosisMainList = [
              { DiagnosisName: '', DiagnosisCode: '' },
            ];
            this.editProgramdata.controls['programname'].setValue(0);
            this.editProgramDiagoList = [];
          },
          (err) => {
            alert('Something Went Wrong!!');
            this.diaganosisMainList = [
              { DiagnosisName: '', DiagnosisCode: '' },
            ];
            this.editProgramdata.controls['programname'].setValue(0);
          }
        );
    } else {
      alert('Please Complete the Form!!');
      this.diaganosisMainList = [{ DiagnosisName: '', DiagnosisCode: '' }];
      this.editProgramdata.controls['programname'].setValue(0);

      // this.editProgramDiagoList = []
      this.editvitalList = [];
      this.editProgramVital = [];
      this.editProgramDiagoList = [];
    }
  }
  EditProgramCancel() {
    this.dialog.closeAll();

    this.diaganosisMainList = [{ DiagnosisName: '', DiagnosisCode: '' }];

    this.editProgramdata.controls['programname'].setValue(0);

    this.editProgramDiagoList = [];
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
          //alert(x.DiagnosisField)
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
      .then((data) => {
        this.last_status = data;
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
    // if (
    //   this.current_status_value == 'OnHold' &&
    //   this.last_status == 'InActive'
    // ) {
    this.status_to_send = 'InActive';
    this.active_img = this.bold_icon;
    //   this.onhold_img = this.unbold_icon;
    //   this.OnHold = false;
    // }
  }
  convertToLocalTime(stillUtc: any) {
    if (stillUtc.includes('+')) {
      var temp = stillUtc.split('+');

      stillUtc = temp[0];
    }
    stillUtc = stillUtc + 'Z';
    var local = moment(stillUtc).local().format('YYYY-MM-DD HH:mm:ss');
    return local;
  }

  vitalEditPgmListChange() {
    this.loading = true;
    var req_body = { VitalIds: this.editvitalList };
    this.rpm.rpm_post(`/api/patient/getdiagnosiscodebyvitalid`, req_body).then(
      (data) => {
        this.editProgramDiagoList = data;

        this.loading = false;
        for (let x of this.editProgramDiagoList) {
          // New Code
          x.DiagnosisFieldEdit = x.DiagnosisName + '-' + x.DiagnosisCode;
          //alert(x.DiagnosisField)
        }
        var j = 0;
        var found = false;
        var indexes = [];
        for (let y of this.diaganosisMainList) {
          found = false;
          for (let x of this.editProgramDiagoList) {
            console.log(
              'x.DiagnosisCode: ' +
                x.DiagnosisCode +
                'y.DiagnosisCode: ' +
                y.DiagnosisCode
            );
            if (
              x.DiagnosisCode == y.DiagnosisCode &&
              y.DiagnosisCode.trim() != ''
            ) {
              found = true;
              break;
            }
          }

          if (!found) {
            indexes.push(j);
          }
          j = j + 1;
        }
        indexes.reverse();
        for (let obj of indexes) {
          this.diaganosisMainList.splice(obj, 1);
          if (this.diaganosisMainList.length == 0) {
            this.diaganosisMainList = [];
            this.objDiagnosis = {
              Id: 0,
              DiagnosisCode: null,
              DiagnosisName: null,
            };
            this.diaganosisMainList.push(this.objDiagnosis);
          }
        }
      },
      (err) => {
        alert('could not find Diagnosis for the selected Vitals/Conditions');
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
    // const dialogRef = this.dialog.open(ConfirmDialogComponent, {
    //   maxWidth: '400px',
    //   data: {
    //     title: 'Are you sure?',
    //     message: 'Do You Want to Delete the Document ? ',
    //   },
    // });
    // dialogRef.afterClosed().subscribe((dialogResult: any) => {
    //   // if user pressed yes dialogResult will be true,
    //   // if he pressed no - it will be false

    //   if (dialogResult) {
    //     this.deleteDocument(documnetId);
    //   } else {
    //     return;
    //   }
    // });

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
        console.log(that.languageList);
      },
      (err) => {
        console.log(err.error);
      }
    );
  }
  rangeControl = new FormControl<[number, number]>([this.minValue, this.maxValue]);
}
