import { Component, OnInit, TemplateRef } from '@angular/core';
import { SelectionModel } from '@angular/cdk/collections';
import { RPMService } from '../../sevices/rpm.service';
import {
  FormControl,
  FormBuilder,
  FormGroup,
  Validators,
  FormArray,
} from '@angular/forms';
import { MatDialog } from '@angular/material/dialog';
import { HttpService } from '../../sevices/http.service';
import { DatePipe } from '@angular/common';
import * as uuid from 'uuid';
import moment from 'moment';
import { ConfirmDialogComponent } from '../../shared/confirm-dialog/confirm-dialog.component';
import { MatTableDataSource } from '@angular/material/table';


import {
  MomentDateAdapter,
  MAT_MOMENT_DATE_ADAPTER_OPTIONS,
} from '@angular/material-moment-adapter';
import {
  DateAdapter,
  MAT_DATE_FORMATS,
  MAT_DATE_LOCALE,
} from '@angular/material/core';
import * as _moment from 'moment';
import { default as _rollupMoment } from 'moment';
import { ActivatedRoute, Router } from '@angular/router';

import { AuthService } from 'src/app/services/auth.service';
import { ConfirmDialogServiceService } from '../../shared/confirm-dialog-panel/service/confirm-dialog-service.service';

// import { prepareSyntheticListenerFunctionName } from '@angular/compiler/src/render3/util';

export const MY_FORMATS = {
  parse: {
    dateInput: 'MMM, DD YYYY',
  },
  display: {
    dateInput: 'MMM, DD YYYY',
    monthYearLabel: 'MMM YYYY',
    dateA11yLabel: 'LL',
    monthYearA11yLabel: 'MMMM YYYY',
  },
};

export interface userdata {
  name: string;
  Patient_id: number;
  date: string;
  selected: boolean;
}
@Component({
  selector: 'app-addpatient',
  templateUrl: './addpatient.component.html',
  styleUrls: ['./addpatient.component.scss'],
  providers: [
    // `MomentDateAdapter` can be automatically provided by importing `MomentDateModule` in your
    // application's root module. We provide it at the component level here, due to limitations of
    // our example generation script.
    {
      provide: DateAdapter,
      useClass: MomentDateAdapter,
      deps: [MAT_DATE_LOCALE, MAT_MOMENT_DATE_ADAPTER_OPTIONS],
    },

    { provide: MAT_DATE_FORMATS, useValue: MY_FORMATS },
  ],
})
export class AddpatientComponent implements OnInit {
  PatientId: any;
  PatientPgmId: any;
  HeightInchValue = '00';
  HeightFeetValue = '0';
  buttonname = 'DRAFTS';
  pid: any;
  PatientInfoForm: FormGroup;
  programForm: FormGroup;
  pagesubmit: boolean = false;
  submitted = false;
  display = false;
  display_edit = true;
  display_pagestatus = true;
  master_data: any;
  Program_Selected: any;
  vital_selected: any;
  Goals_Selected: any;
  state_selected: any;
  hideConfirmButton: boolean;
  Diagnosis_Name: any;
  Diagnosis_code_selected: any;
  patient_variable: any;
  program_variable: any;
  dataSource: any = [];
  diagonisticsData: any = [];
  userdata: userdata[];
  patient_id: any;
  row: any;
  Clinic_Selected: any;
  states: any;
  variable: any;
  patientPassword: any;
  phoneNumber = '^(+d{1,3}[- ]?)?d{10}$';
  loading = true;
  program = [
    { menu_id: 1, menu_title: 'Program Details' },
    { menu_id: 1, menu_title: 'Device Details' },
    { menu_id: 1, menu_title: 'Vital Schedules' },
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
  hideSaveDraftButton: boolean;
  diffInMonths: number;
  StatesAndCities: any;
  programid: unknown;
  patientprogramid: unknown;
  careTeamMembers: any;
  timezones: any;
  add_patient_masterdata: unknown;
  physicianList: any;
  file: any;
  newProgramVariable: any;
  PatientProgramStatus: any;
  vitalList = [];
  scroll(el: HTMLElement) {
    el.scrollIntoView();
    el.scrollIntoView({ behavior: 'smooth' });
  }
  searchPatientName: any;
  constructor(
    private model: HttpService,
    private frmobj: FormBuilder,
    public dialog: MatDialog,
    private rpm: RPMService,
    private router: Router,
    public datepipe: DatePipe,
    private Auth: AuthService,
    private _route: ActivatedRoute,
    private showconfirmDialog: ConfirmDialogServiceService
  ) {
    this.PatientProgramStatus = false;
    this.newProgramVariable = false;

    this.pid = null;
    this.Diagnosis_List = [{ DiagnosisName: '', DiagnosisCode: '' }];
    this.searchPatientName = false;
  }
  cities: any;
  image: any;
  openFile() {
    this.image = null;
    var a = document.getElementById('image');
    a?.click();
  }
  handle(e: any) {
    this.image = e.target.files[0];
    var a = document.getElementsByClassName('uploadPhoto');
    this.file = this.image.name;

    // a[0].setAttribute("style", "background-image:"+this.image.name);
    // a[0].setAttribute("style", "background: url(\"https://rpmstorage123.blob.core.windows.net/rpmprofilepictures/CL500626\"); background-repeat: no-repeat;  background-size: 100% 100%;");
  }
  submitImage(pid: any) {
    if (this.image) {
      const myPhoto = uuid.v4();
      var formData: any = new FormData();
      formData.append(myPhoto, this.image);
      this.rpm
        .rpm_post(`/api/patient/addimage?PatientId=${pid}`, formData)
        .then(
          (data) => {},
          (err) => {
            console.log('Img error');
          }
        );
    }
  }

  todaty_date: any;
  rolelist: any;
  cityFlag: any;
  ngOnInit(): void {
    // this.todaty_date = new Date();
    this.todaty_date = this.datepipe.transform(new Date(), 'yyyy-MM-dd');
    this.cityFlag = false;
    this.timezonecalculation();
    this.durationValue = 12;
    this.cities = [];
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
    this.careTeamMembers = this.master_data.CareTeamMembers;
    var myuser = sessionStorage.getItem('user_name');
    var myid = sessionStorage.getItem('userid');

    that.StatesAndCities = sessionStorage.getItem('states_cities');
    that.StatesAndCities = JSON.parse(that.StatesAndCities);
    this.timezones = this.StatesAndCities.TimeZones;
    that.loading = false;
    this.initForms();
    this.programForm.controls.assignedMember.value;
    that.programForm.controls['assignedMember'].setValue(myid, {
      onlyself: true,
    });
    this.hideConfirmButton = true;
    this.hideSaveDraftButton = false;
    this.setPrescribedDataToCurrent();
    this.pid = null;
    this.dataSource.length = 1;
    this.diagonisticsData.length = 1;
    // if (this.display_pagestatus == true) {
    //   this.display_edit = true;
    // } else {
    //   this.display_edit = false;
    // }
    var that = this;
    this.PatientInfoForm.get('clinicname')?.valueChanges.subscribe((val) => {
      that.Clinic_Selected = that.master_data.ClinicDetails.filter(
        (clinic: { Id: any }) => clinic.Id === parseInt(val)
      );
      that.PatientInfoForm.controls['cliniccode'].setValue(
        that.Clinic_Selected[0].ClinicCode
      );
      this.getPhysicianList();
    });
    this.programForm.get('startdate')?.valueChanges.subscribe((val) => {
      if (this.programForm.controls.startdate.value) {
        this.calculateEndDate();
      }
    });
    // this.programForm.get('programname')?.valueChanges.subscribe((value) => {
    this.programForm.get('programname')?.valueChanges.subscribe((val) => {
      that.Program_Selected = that.master_data.ProgramDetailsMasterData.filter(
        (Pgm: { ProgramId: number }) => Pgm.ProgramId == parseInt(val)
      );

      this.Diagnosis_List = [{ DiagnosisName: '', DiagnosisCode: '' }];
      that.durationValue = that.Program_Selected[0].Duration;
      that.vital_selected = that.Program_Selected[0].Vitals;
      that.vitalList = [];
      that.Goals_Selected = that.Program_Selected[0].goalDetails;
      that.Diagnosis_Name = that.Program_Selected[0].Vitals[0].Dignostics;

      for (let x of this.Diagnosis_Name) {
        x.DiagnosisField = x.DiagnosisName;
        //alert(x.DiagnosisField)
      }

      if (this.goals) {
        this.goals.clear();
        for (let g of that.Program_Selected[0].goalDetails) {
          var Res = this.makeParagraphsofDescription(g.Description);
          var a = this.frmobj.group({
            Goal: [g.Goal, [Validators.required]],
            Description: [Res, [Validators.required]],
          });
          this.goals.push(a);
        }
      }
    });

    this.programForm.get('diagnosisData')?.valueChanges.subscribe((val) => {
      that.Diagnosis_code_selected =
        that.Program_Selected[0].Vitals[0].Dignostics.filter(
          (diagnosis: { DiagnosisName: any }) =>
            diagnosis.DiagnosisName == val[0].DiagnosisName
        );

      that.diagonisticsData = that.Diagnosis_code_selected[0].DiagnosisCode;
    });
    this.programForm.get('goals')?.valueChanges.subscribe((data) => {
      alert(data[0].Goal);
    });
    this.PatientInfoForm.get('state')?.valueChanges.subscribe((data) => {
      if (this.cityFlag == true) {
        this.PatientInfoForm.get('city')?.reset();
      }

      that.cities = [];
      that.cities = that.StatesAndCities.Cities.filter(
        (c: { StateId: number }) => c.StateId === parseInt(data)
      );
      this.cityFlag = true;
    });
    this.programForm.controls['patientstatus'].setValue('Prescribed', {
      onlySelf: true,
    });
    this.PatientInfoForm.controls['language'].setValue('English');

    this._route.queryParams.subscribe((params) => {
      this.PatientId = params.id;
      this.PatientPgmId = params.programId;
      this.PatientInfoForm.get('state')?.valueChanges.subscribe((data) => {
        that.cities = that.StatesAndCities.Cities.filter(
          (c: { StateId: number }) => c.StateId === parseInt(data)
        );
      });
      if (this.PatientId && this.PatientPgmId) {
        this.getPatientDetails(this.PatientId, this.PatientPgmId);
      }
    });
    this.getLanguage();
  }

  private initForms() {
    this.PatientInfoForm = this.frmobj.group({
      firstname: new FormControl('', [Validators.required]),
      middlename: new FormControl(null),
      dateofbirth: new FormControl('', [Validators.required]),
      gender: new FormControl('', [Validators.required]),
      lastname: new FormControl('', [Validators.required]),

      // email: new FormControl(null, [Validators.pattern('^[a-z0-9._%+-]+@[a-z0-9.-]+.[a-z]{2,4}$'),]),

      email: new FormControl('', [Validators.email]),

      mobile: new FormControl('', [
        Validators.required,
        Validators.pattern('^[0-9]*$'),
        //Validators.pattern('[- +()0-9]+'),
        Validators.minLength(10),
        Validators.maxLength(10),
      ]),
      alternatenumber: new FormControl('', [
        Validators.pattern('^[0-9]*$'),
        Validators.minLength(10),
        Validators.maxLength(10),
      ]),
      addressline1: new FormControl('', [Validators.required]),
      addressline2: new FormControl(null),
      zipcode: new FormControl('', [Validators.required]),
      city: new FormControl('', [Validators.required]),
      state: new FormControl('', [Validators.required]),
      calltime: new FormControl(null),
      timezone: new FormControl('', [Validators.required]),
      emergencycontact1: new FormControl(''),
      emergencycontact2: new FormControl(null),
      relation1: new FormControl(null),
      relation1_mobile: new FormControl('', [
        Validators.pattern('^[0-9]*$'),
        Validators.minLength(10),
        Validators.maxLength(10),
      ]),
      relation2: new FormControl(null),
      relation2_mobile: new FormControl('', [
        Validators.pattern('^[0-9]*$'),
        Validators.minLength(10),
        Validators.maxLength(10),
      ]),
      language: new FormControl('', [Validators.required]),
      preference2: new FormControl(null),
      preference3: new FormControl(null),
      preference4: new FormControl(null),
      additionalnotes: new FormControl(null),
      clinicname: new FormControl('', [Validators.required]),
      cliniccode: new FormControl(null),
    });

    this.programForm = this.frmobj.group({
      programname: new FormControl('', [Validators.required]),
      // programduration: new FormControl('', [Validators.required]),
      // vitals: new FormControl({ value: '' }),
      vitals: new FormControl(''),
      startdate: new FormControl('', [Validators.required]),
      enddate: new FormControl('', [Validators.required]),
      patientstatus: new FormControl('', [Validators.required]),
      // target: new FormControl(null),
      // physician: new FormControl('', [Validators.required]),
      physician: new FormControl(null),
      /*diagnosisData: this.frmobj.array([
        this.frmobj.group({
          DiagnosisName: ['', [Validators.required]],
          DiagnosisCode: '',
        }),
      ]),*/
      // clinic: new FormControl({ value: '' }, [Validators.required]),
      clinic: new FormControl('', [Validators.required]),
      cliniccode: new FormControl(null),
      // consultdate: new FormControl('', [Validators.required]),
      consultdate: new FormControl(null),

      assignedMember: new FormControl('', [Validators.required]),
      PrescribedDate: new FormControl('', [Validators.required]),
    });

    this.PatientInfoForm.valueChanges.subscribe((val) => {
      if (this.PatientInfoForm.dirty) {
        this.buttonname = 'SAVEDRAFT';
      }
    });
  }

  getPhysicianList() {
    if (this.resp) {
      this.physicianList = this.master_data.PhysicianDetails.filter(
        (phy: { ClinicID: any }) =>
          phy.ClinicID === parseInt(this.resp.OrganizationID)
      );
    }
  }

  public get goals() {
    return this.programForm.get('goals') as FormArray;
  }

  goalForm = this.frmobj.group({
    Goal: new FormControl('', [Validators.required]),
    Description: new FormControl('', [Validators.required]),
  });

  addNewGoal() {
    this.goals.push(this.goalForm);
  }
  PreDefinedGoals(goals: any) {
    for (let g in goals) {
      var goalForm = this.frmobj.group({
        Goal: new FormControl(''),
        Description: new FormControl(''),
      });
      this.goals.push(this.goalForm);
    }
  }

  // Add Diaganostics Code
  public get diagnosisData() {
    return this.programForm.get('diagnosisData') as FormArray;
  }

  DiaganosticsForm = this.frmobj.group({
    DiagnosisName: new FormControl('', [Validators.required]),
    DiagnosisCode: new FormControl('', [Validators.required]),
  });

  // addNewDiaganostics() {
  //   if(this.diagnosisData.length<2){
  //         this.DiaganosticsForm.reset();
  //         this.diagnosisData.push(this.DiaganosticsForm);
  //     }
  //   }

  Patientinfo() {
    this.variable = 1;
    this.draftvariable = 0;
  }
  Programinfo() {
    this.variable = 2;
  }

  draftvariable = 0;

  draft() {
    this.draftvariable = 1;
    this.GetPatientDraftList();
  }

  onClickNavigation(id: any) {
    switch (id) {
      case 1:
        this.patient_variable = 1;
        break;
    }
  }

  onClickNavigation1(id: any) {
    switch (id) {
      case 1:
        this.program_variable = 1;
        break;
    }
  }

  columnHeader = [
    'PatientName',
    'PatientId',
    'CreatedOn',
    'Time',
    'action',
  ];

  datasourceDocument: any;

  selection: SelectionModel<Element> = new SelectionModel<Element>(false, []);
  someVar: any = false;
  selectRow($event: any, row: userdata) {
   // $event.preventDefault();
    if (!row.selected) {
      this.datasourceDocument.filteredData.forEach(
        (row: any) => (row.selected = false)
      );
      row.selected = true;
    }
    this.someVar = true;
  }

  CurrentMenu: any = 1;
  // Change Main Screen

  ChangeScreen(button: any) {
    switch (button) {
      case 1:
        // if (!this.pid) {
        this.variable = 1;
        this.CurrentMenu = button;
        this.hideConfirmButton = true;

        this.hideSaveDraftButton = false;
        // }
        break;

      case 2:
        if (this.pid) {
          this.variable = 2;
          this.CurrentMenu = button;
          this.hideSaveDraftButton = true;

          this.hideConfirmButton = false;
        }
        break;

      default:
        if (this.pid) {
          this.variable = 2;
          this.CurrentMenu = button;
          this.hideSaveDraftButton = true;

          this.hideConfirmButton = false;
        } else {
          this.variable = 1;
          this.CurrentMenu = button;
          this.hideConfirmButton = true;

          this.hideSaveDraftButton = false;
        }
        break;
    }
  }

  defaultHeight: any;
  durationValue = 12;
  increment_duration() {
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
  // increment() {
  //   if(this.defaultHeight < 30)
  //   {
  //     this.defaultHeight++;
  //   }
  // }
  // decrement() {
  //   if (this.defaultHeight > 0) {
  //     this.defaultHeight--;
  //   } else {
  //     this.defaultHeight = 0;
  //   }
  //   this.calculateEndDate();
  // }
  weightValue = 0;
  increment_weight() {
    if (this.weightValue < 1000) {
      this.weightValue++;
    }
  }
  decrement_weight() {
    if (this.weightValue > 0) {
      this.weightValue--;
    } else {
      this.weightValue = 0;
    }
  }

  onAddDiaganostic() {
    this.diagonisticsData.push(this.diagonisticsData.length);
  }

  openDialog(templateRef: TemplateRef<any>) {
    this.markFormGroupTouched(this.programForm);
    // New Code Change 03/05/2023
    if (this.vitalList.length <= 0) {
      alert('Please Add Vital/Condition Monitored.');
      return;
    }

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

    if (this.programForm.valid) {
      if (this.vitalList.length == 0) {
        alert('Conditions/Vital Monitored Required');
      } else {
        this.dialog.open(templateRef);
      }
    } else {
      alert('Please complete the form');
    }
  }
  cancel_dialog() {
    this.dialog.closeAll();
  }
  confirm_dialog(templateRef: TemplateRef<any>) {
    this.dialog.closeAll();
    this.UpdatePatient_Program();
    this.AddPatientProgram(templateRef);
    this.Auth.reloadPatientList('PatientList Updated');
  }

  confirm_AddPatient_dialog(templateRef: TemplateRef<any>) {
    this.dialog.closeAll();
    this.AddPatient(templateRef);
  }
  validateForm = false;
  SubmitPatient(templateRef: any) {
    // this.validateForm=true;
    this.markFormGroupTouched(this.PatientInfoForm);

    if (this.pid) {
      this.UpdatePatientInfo();
    } else {
      this.AddPatient(templateRef);
    }
  }
  private markFormGroupTouched(formGroup: FormGroup) {
    (<any>Object).values(formGroup.controls).forEach((control: FormGroup) => {
      control.markAsTouched();

      if (control.controls) {
        this.markFormGroupTouched(control);
      }
    });
  }
  result: any;
  AddPatient(templateRef: any) {
    //call on save draft click
    var req_body: any = {};

    var patientHeight = this.HeightFeetValue + '.' + this.HeightInchValue;
    this.defaultHeight = parseFloat(patientHeight).toFixed(2);

    if (this.PatientInfoForm.valid) {
      this.loading = true;
      req_body['Status'] = 'Draft';
      req_body['FirstName'] = this.PatientInfoForm.controls.firstname.value;
      req_body['MiddleName'] = this.PatientInfoForm.controls.middlename.value;
      req_body['LastName'] = this.PatientInfoForm.controls.lastname.value;
      req_body['OrganizationID'] =
        this.PatientInfoForm.controls.clinicname.value;
      req_body['DOB'] = this.PatientInfoForm.controls.dateofbirth.value;
      req_body['Gender'] = this.PatientInfoForm.controls.gender.value;
      req_body['Email'] = this.PatientInfoForm.controls.email.value;
      req_body['MobileNo'] = this.PatientInfoForm.controls.mobile.value;
      req_body['AlternateMobNo'] =
        this.PatientInfoForm.controls.alternatenumber.value;
      req_body['Height'] = this.defaultHeight;
      req_body['Weight'] = this.weightValue;
      req_body['Address1'] = this.PatientInfoForm.controls.addressline1.value;
      req_body['Address2'] = this.PatientInfoForm.controls.addressline2.value;
      req_body['ZipCode'] = this.PatientInfoForm.controls.zipcode.value;
      req_body['CityID'] = this.PatientInfoForm.controls.city.value;
      req_body['StateId'] = this.PatientInfoForm.controls.state.value;
      req_body['TimeZoneID'] = this.PatientInfoForm.controls.timezone.value;
      req_body['Contact1Name'] =
        this.PatientInfoForm.controls.emergencycontact1.value;
      req_body['Contact1RelationName'] =
        this.PatientInfoForm.controls.relation1.value;
      req_body['Contact1Phone'] =
        this.PatientInfoForm.controls.relation1_mobile.value;
      req_body['Contact2Name'] =
        this.PatientInfoForm.controls.emergencycontact2.value;
      req_body['Contact2RelationName'] =
        this.PatientInfoForm.controls.relation2.value;
      req_body['Contact2Phone'] =
        this.PatientInfoForm.controls.relation2_mobile.value;
      req_body['CallTime'] = this.PatientInfoForm.controls.calltime.value;
      req_body['Language'] = this.PatientInfoForm.controls.language.value;
      req_body['Preference1'] = this.PatientInfoForm.controls.preference2.value;
      req_body['Preference2'] = this.PatientInfoForm.controls.preference3.value;
      req_body['Preference3'] = this.PatientInfoForm.controls.preference4.value;
      req_body['Notes'] = this.PatientInfoForm.controls.additionalnotes.value;
      req_body['CountryId'] = 233;
      req_body['Picture'] = null;

      var that = this;
      this.rpm.rpm_post('/api/patient/addpatient', req_body).then(
        (data) => {
          //show patient id created
          this.loading = false;
          this.result = data;
          that.pid = this.result.PatientId;
          this.patientPassword = this.result.password;
          this.dialog.open(templateRef);

          that.ChangeScreen(2);
          that.GetDraftedPatientInfo(that.pid, 2);
          this.submitImage(that.pid);
          this.cityFlag = false;
        },
        (err) => {
          //show error patient id creation failed
          alert('Please complete all mandatory fields..!');
          this.loading = false;
        }
      );
    } else {
      alert('Please complete all mandatory fields..!');
    }
  }
  GetPatientDraftList() {
    //call on drafts button click

    var that = this;
    this.rpm.rpm_get('/api/patient/getdraftpatients').then(
      (data) => {
        this.datasourceDocument = data;
        this.datasourceDocument = new MatTableDataSource(
          this.datasourceDocument
        );

        //data will have the datasource for patient list table
      },
      (err) => {
        //show error could not fetch draft list show empty table
        alert(err);
      }
    );
  }
  resp: any;
  GetDraftedPatientInfo(PatientId: any, page: number) {
    //call on clicking on one patient row in the draft list, pass patient id from table to get the info
    var that = this;
    this.pid = PatientId;
    this.variable = page;
    this.rpm
      .rpm_get('/api/patient/getdraftpatientDetails?PatientId=' + PatientId)
      .then(
        (data) => {
          that.draftvariable = 0;
          that.resp = data;
          this.PatientInfoForm.controls['firstname'].setValue(
            that.resp.FirstName
          );
          this.PatientInfoForm.controls['middlename'].setValue(
            that.resp.MiddleName
          );
          this.PatientInfoForm.controls['lastname'].setValue(
            that.resp.LastName
          );
          this.PatientInfoForm.controls['dateofbirth'].setValue(
            that.convertDate(that.resp.DOB)
          );
          this.PatientInfoForm.controls['gender'].setValue(that.resp.Gender, {
            onlySelf: true,
          });
          this.PatientInfoForm.controls['email'].setValue(that.resp.Email);
          this.PatientInfoForm.controls['mobile'].setValue(that.resp.MobileNo);
          this.PatientInfoForm.controls['alternatenumber'].setValue(
            that.resp.AlternateMobNo
          );
          this.PatientInfoForm.controls['addressline1'].setValue(
            that.resp.Address1
          );
          this.PatientInfoForm.controls['addressline2'].setValue(
            that.resp.Address2
          );
          this.PatientInfoForm.controls['zipcode'].setValue(that.resp.ZipCode);
          this.PatientInfoForm.controls['city'].setValue(that.resp.CityID);
          this.PatientInfoForm.controls['state'].setValue(that.resp.StateId);
          this.PatientInfoForm.controls['timezone'].setValue(
            that.resp.TimeZoneID
          );
          this.PatientInfoForm.controls['emergencycontact1'].setValue(
            that.resp.Contact1Name
          );
          this.PatientInfoForm.controls['relation1'].setValue(
            that.resp.Contact1RelationName
          );
          this.PatientInfoForm.controls['relation1_mobile'].setValue(
            that.resp.Contact1Phone
          );
          this.PatientInfoForm.controls['emergencycontact2'].setValue(
            that.resp.Contact2Name
          );
          this.PatientInfoForm.controls['relation2'].setValue(
            that.resp.Contact2RelationName
          );
          this.PatientInfoForm.controls['relation2_mobile'].setValue(
            that.resp.Contact2Phone
          );
          this.PatientInfoForm.controls['calltime'].setValue(
            that.resp.CallTime
          );
          this.PatientInfoForm.controls['language'].setValue(
            that.resp.Language
          );
          this.PatientInfoForm.controls['preference2'].setValue(
            that.resp.Preference1
          );
          this.PatientInfoForm.controls['preference3'].setValue(
            that.resp.Preference2
          );
          this.PatientInfoForm.controls['preference4'].setValue(
            that.resp.Preference3
          );

          this.PatientInfoForm.controls['additionalnotes'].setValue(
            that.resp.Notes
          );
          that.PatientInfoForm.controls['clinicname'].setValue(
            that.resp.OrganizationID,
            { onlySelf: true }
          );
          this.getPhysicianList();
          that.programForm.controls['clinic'].setValue(that.resp.ClinicName);
          that.programForm.controls['cliniccode'].setValue(
            that.resp.ClinicCode
          );
          this.defaultHeight = that.resp.Height;
          this.weightValue = that.resp.Weight;
          that.pid = PatientId;

          //data will have the patient data use this to show the in the add patient and basic info on program page
        },
        (err) => {
          //show error could not fetch draft list show empty table

          alert('could not fethch patient info');
        }
      );
  }
  UpdatePatientInfo() {
    //call on clicking savedraft after opening the drafted patient
    if (this.PatientInfoForm.valid) {
      this.loading = true;
      var req_body = {
        PatientId: this.pid,
        Picture: null,
        FirstName: this.PatientInfoForm.controls.firstname.value,
        MiddleName: this.PatientInfoForm.controls.middlename.value,
        LastName: this.PatientInfoForm.controls.lastname.value,
        DOB: this.PatientInfoForm.controls.dateofbirth.value,
        Gender: this.PatientInfoForm.controls.gender.value,
        Height: this.defaultHeight,
        Weight: this.weightValue,
        Email: this.PatientInfoForm.controls.email.value,
        MobileNo: this.PatientInfoForm.controls.mobile.value,
        AlternateMobNo: this.PatientInfoForm.controls.alternatenumber.value,
        OrganizationID: this.PatientInfoForm.controls.clinicname.value,
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
        Status: 'Draft',
        Preference1: this.PatientInfoForm.controls.preference2.value,
        Preference2: this.PatientInfoForm.controls.preference3.value,
        Preference3: this.PatientInfoForm.controls.preference4.value,
        Notes: this.PatientInfoForm.controls.additionalnotes.value,
      };
      var that = this;
      this.rpm.rpm_post('/api/patient/updatepatient', req_body).then(
        (data) => {
          this.loading = false;
          alert('Patient Details Updated Successfully...!');
          that.ChangeScreen(2);
          that.GetDraftedPatientInfo(that.pid, 2);
          this.submitImage(that.pid);
          this.cityFlag = false;
          //show success popup patient is updated
        },
        (err) => {
          this.loading = false;
          alert('Could not Update Patient  - ' + err);
          //show error pop up, could not update patient
        }
      );
    }
  }
  responseData: any;
  UpdatePatient_Program() {
    //call on clicking savedraft after opening the drafted patient
    this.markFormGroupTouched(this.programForm);
    if (this.PatientInfoForm.valid) {
      var req_body = {
        PatientId: this.pid,
        Picture: null,
        FirstName: this.PatientInfoForm.controls.firstname.value,
        MiddleName: this.PatientInfoForm.controls.middlename.value,
        LastName: this.PatientInfoForm.controls.lastname.value,
        DOB: this.PatientInfoForm.controls.dateofbirth.value,
        Gender: this.PatientInfoForm.controls.gender.value,
        Height: this.defaultHeight,
        Weight: this.weightValue,
        Email: this.PatientInfoForm.controls.email.value,
        MobileNo: this.PatientInfoForm.controls.mobile.value,
        AlternateMobNo: this.PatientInfoForm.controls.alternatenumber.value,
        OrganizationID: this.PatientInfoForm.controls.clinicname.value,
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
        Status: 'Draft',
        Preference1: this.PatientInfoForm.controls.preference2.value,
        Preference2: this.PatientInfoForm.controls.preference3.value,
        Preference3: this.PatientInfoForm.controls.preference4.value,
        Notes: this.PatientInfoForm.controls.additionalnotes.value,
      };
      var that = this;
      this.rpm.rpm_post('/api/patient/updatepatient', req_body).then(
        (data) => {
          // alert("Patient Details Updated Successfully...!")
          // that.ChangeScreen(2);
          // that.GetDraftedPatientInfo(that.pid, 2);
          // this.submitImage(that.pid)
          //show success popup patient is updated
        },
        (err) => {
          console.log('Could not Update Patient  - ' + err);
          //show error pop up, could not update patient
        }
      );
    }
  }
  AddPatientProgram(templateRef: any) {
    //call on confirm button click

    var range_start_date = this.startDateValue;
    range_start_date = this.convertDate(range_start_date);
    var range_end_date = this.programForm.controls.enddate.value;
    var startDate = this.startDateValue;
    var endDate = range_end_date + 'T00:00:00';

    var req_body: any = {};
    // New Code 03/05/2023
    if (this.vitalList.length <= 0) {
      alert('Please Select Vital/Condtion Monitored');
      return;
    }

    if (this.newProgramVariable) {
      if (this.programForm.valid && this.PatientInfoForm.valid) {
        this.loading = true;
        req_body['PatientId'] = this.pid;
        req_body['PatientProgramId'] = this.PatientPgmId;
        req_body['ProgramId'] = this.programForm.controls.programname.value;
        req_body['PhysicianId'] = null;
        req_body['ConsultationDate'] = null;
        req_body['CareTeamUserId'] =
          this.programForm.controls.assignedMember.value;
        req_body['PatientStatus'] =
          this.programForm.controls.patientstatus.value;
        req_body['PrescribedDate'] = this.Auth.ConvertToUTCRangeInput(
          new Date(this.programForm.controls.PrescribedDate.value + 'T00:00:00')
        );
        // req_body['PrescribedDate'] =  this.convertDate(this.programForm.controls.PrescribedDate.value);
        req_body['StartDate'] = startDate;
        req_body['EndDate'] = endDate;
        req_body['GoalDetails'] = this.Goals_Selected;
        req_body['ProgramDiagnosis'] = this.Diagnosis_List;
        req_body['VitalIds'] = this.vitalList;

        var that = this;

        this.rpm.rpm_post('/api/patient/addnewpatientprogram', req_body).then(
          (data) => {
            this.loading = false;
            alert('New Program Added Successfully');
            this.Auth.reloadPatientList('PatientList Updated');
            this.dialog.closeAll();
            this.newProgramVariable = false;
            let route = '/admin/patients';
            this.router.navigate([route]);

            // show success patient id created and show pop-up with 3 options
          },
          (err) => {
            this.loading = false;
            this.Auth.reloadPatientList('PatientList Updated');

            alert('Could not Add Patient Program');
            this.newProgramVariable = false;
            //  show error could not create patient program
          }
        );
      } else {
        alert('Please fill all the information requested...!');
      }
    } else {
      if (this.programForm.valid && this.PatientInfoForm.valid) {
        this.loading = true;
        req_body['PatientId'] = this.pid;
        req_body['ProgramId'] = parseInt(this.programForm.controls.programname.value);
        if(this.programForm.controls.physician.value == null){
          req_body['PhysicianId'] = 0;
        }
        else{
          req_body['PhysicianId'] = parseInt(this.programForm.controls.physician.value);
        }
        req_body['ConsultationDate'] =
          this.programForm.controls.consultdate.value;
        req_body['CareTeamUserId'] =
          parseInt(this.programForm.controls.assignedMember.value);
        req_body['PatientStatus'] =
          this.programForm.controls.patientstatus.value;
        // req_body['PrescribedDate'] =  this.convertDate(this.programForm.controls.PrescribedDate.value);
        req_body['PrescribedDate'] = this.Auth.ConvertToUTCRangeInput(
          new Date(this.programForm.controls.PrescribedDate.value + 'T00:00:00')
        );
        // req_body['TargetReadings'] = this.programForm.controls.target.value;
        req_body['StartDate'] = startDate;
        req_body['EndDate'] = endDate;
        req_body['GoalDetails'] = this.Goals_Selected;
        req_body['ProgramDiagnosis'] = this.Diagnosis_List;
        req_body['VitalIds'] = this.vitalList;

        var that = this;

        this.rpm.rpm_post('/api/patient/addpatientprogram', req_body).then(
          (data) => {
            this.loading = false;
            this.dialog.open(templateRef);
            this.responseData = data;
            that.patientprogramid = this.responseData.message;

            this.Auth.reloadPatientList('PatientList Updated');

            // show success patient id created and show pop-up with 3 options
          },
          (err) => {
            this.loading = false;
            alert('Could not Add Patient Program');
            this.Auth.reloadPatientList('PatientList Updated');
            //  show error could not create patient program
          }
        );
      } else {
        alert('Please fill all the information requested...!');
      }
    }

    // if (this.programForm.valid)
    // {
    //   this.loading=true;
    //   req_body['PatientId'] = this.pid;
    //   req_body['ProgramId'] = this.programForm.controls.programname.value;
    //   req_body['PhysicianId'] = this.programForm.controls.physician.value;
    //   req_body['ConsultationDate'] =  this.programForm.controls.consultdate.value;
    //   req_body['CareTeamUserId'] =  this.programForm.controls.assignedMember.value;
    //   req_body['PatientStatus'] = this.programForm.controls.patientstatus.value;
    //   req_body['PrescribedDate'] =  this.convertDate(this.programForm.controls.PrescribedDate.value);
    //   // req_body['TargetReadings'] = this.programForm.controls.target.value;
    //   req_body['StartDate'] = this.programForm.controls.startdate.value;
    //   req_body['EndDate'] = this.programForm.controls.enddate.value;
    //   req_body['GoalDetails'] = this.Goals_Selected;
    //   req_body['ProgramDiagnosis'] =  this.Diagnosis_List;
    //   var that = this;
    //   console.log('Program Form Values' + JSON.stringify(this.programForm.value));
    //   console.log(req_body);
    //   this.rpm.rpm_post('/api/patient/addpatientprogram', req_body).then(
    //     (data) => {
    //       this.loading=false;
    //       console.log(data);
    //       this.dialog.open(templateRef);
    //       that.patientprogramid = data;
    //       this.Auth.reloadPatientList('PatientList Updated');

    //       // show success patient id created and show pop-up with 3 options
    //     },
    //     (err) => {
    //       this.loading=false;
    //       this.Auth.reloadPatientList('PatientList Updated');

    //       alert('Could not Add Patient Program');
    //       this.Auth.reloadPatientList('PatientList Updated');
    //       //  show error could not create patient program
    //     }
    //   );
    // }
    // else{
    //   alert("Please fill all the information requested...!")
    // }
  }
  GetPatientInfo(PatientId: any, ProgramId: any) {
    //call on clicking edit button from patient details page, this is for edit patient to fill the current saved info
    var that = this;
    this.rpm
      .rpm_get(
        '/api/patient/getpatient?PatientId=' +
          PatientId +
          '&PatientprogramId=' +
          ProgramId
      )
      .then(
        (data) => {
          //data will have the patient data use this to show the in the add patient and basic info on program page
        },
        (err) => {
          //show error could not fetch draft list show empty table
        }
      );
  }
  setPrescribedDataToCurrent() {
    let today = new Date();
    // let dd = today.getDate();
    // let mm = today.getMonth() + 1;
    // const yyyy = today.getFullYear();
    // var dateval = yyyy + '-' + mm + '-' + dd;
    // this.programForm.controls['PrescribedDate'].setValue(this.convertDateMMDD(today));
    this.programForm.controls['PrescribedDate'].setValue(
      this.convertDate(today)
    );
  }
  makeParagraphsofDescription(desc: any) {
    var arr = desc.split(',');
    // var res=[];
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

  findEndDateUsingDuration() {}
  ClearPage() {
    this.pid = null;
    this.PatientInfoForm.reset();
    this.programForm.reset();
    this.ChangeScreen(1);
  }
  redirectToPateintDetails() {
    let route = '/admin/patients_detail';
    this.dialog.closeAll();
    this.router.navigate([route], {
      queryParams: { id: this.pid, programId: this.patientprogramid },
      skipLocationChange: true,
    });
  }
  redirectToEditPateint() {
    let route = '/admin/editpatient';
    this.dialog.closeAll();
    this.router.navigate([route], {
      queryParams: { id: this.pid, programId: this.patientprogramid },
      skipLocationChange: true,
    });
  }
  startDateValue: any;
  calculateEndDate() {
    var someDate = this.programForm.controls.startdate.value;
    someDate = someDate + 'T00:00:00';
    someDate = this.Auth.ConvertToUTCRangeInput(new Date(someDate));
    this.startDateValue = someDate;
    var someDateValue = moment(someDate).add(this.durationValue, 'M');
    this.programForm.controls['enddate'].setValue(
      this.convertDate(someDateValue)
    );
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
  utcminutes: any;
  utc: any;
  timezonecalculation() {
    var today = new Date();
    var UTCDifference = +5.3;
    // var current_year = today.getUTCFullYear() + UTCDifference;

    var ampm = this.utc >= 12 ? 'PM' : 'AM';

    this.utc = today.getUTCHours() + UTCDifference;

    var utcminutes = new Date().getUTCMinutes();

    this.utc = this.utc < 10 ? '0' + this.utc : this.utc;

    this.utcminutes = utcminutes < 10 ? '0' + utcminutes : utcminutes;

    var CurrentTime = this.utc + ':' + utcminutes + ' ' + ampm;

    return CurrentTime;
  }
  Diagnosis_List: any;
  // diagnosisChanged(event:any,index:any){

  //   var DiagnosisSelected = this.Diagnosis_Name.filter((data: { DiagnosisName: any; })=>{
  //     return data.DiagnosisName==event
  //   })
  //   if(DiagnosisSelected.length>0){
  //     this.Diagnosis_List[index].DiagnosisCode = DiagnosisSelected[0].DiagnosisCode
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

      // this.Diagnosis_List[index].dcode = DiagnosisSelected[0].DiagnosisCode
      // this.dcode= this.Diagnosis_List[index].DiagnosisCode;
    }
  }
  cleared(event: any, index: any) {
    this.Diagnosis_List[index].DiagnosisCode = null;
    this.Diagnosis_List[index].DiagnosisName = null;
  }
  keyword = 'DiagnosisField';

  objDiagnosis = { Id: 0, DiagnosisCode: null, DiagnosisName: null };

  addNewDiaganostics_Patient() {
    // this.Diagnosis_Name=this.Diagnosis_Name.filter(
    //   (diagnosis: { Selected: any }) =>
    //     diagnosis.Selected == false
    // );

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
  futureDateError: boolean;
  checkDateValidity(date: any): boolean {
    const mxDate = new Date(this.todaty_date);
    const inputDate = new Date(date);

    if (inputDate > mxDate) {
      // alert("Please Enter a Valid Date")

      return (this.futureDateError = true);
    }
    return (this.futureDateError = false);
  }
  http_rpm_patientList: any;
  http_ProgramData: any;
  getPatientDetails(patientId: any, pgmId: any) {
    var that = this;

    this.rpm
      .rpm_get(
        `/api/patient/getpatient?PatientId=${patientId}&PatientprogramId=${pgmId}`
      )
      .then((data) => {
        this.pid = patientId;
        this.variable = 2;
        this.CurrentMenu = 2;
        this.hideConfirmButton = false;
        this.hideSaveDraftButton = true;
        this.PatientProgramStatus = true;

        this.newProgramVariable = true;
        this.http_rpm_patientList = data;
        this.http_ProgramData = data;
        var PatientProgramData =
          this.http_ProgramData.PatientPrescribtionDetails;
        this.http_rpm_patientList = this.http_rpm_patientList.PatientDetails;
        this.PatientInfoForm.controls['firstname'].setValue(
          this.http_rpm_patientList.FirstName
        );
        this.PatientInfoForm.controls['middlename'].setValue(
          this.http_rpm_patientList.MiddleName
        );
        this.PatientInfoForm.controls['lastname'].setValue(
          this.http_rpm_patientList.LastName
        );
        this.PatientInfoForm.controls['dateofbirth'].setValue(
          that.convertDate(this.http_rpm_patientList.DOB)
        );
        var genderValue = this.http_rpm_patientList.Gender;
        var genderOption;
        if (genderValue == 'Male') {
          genderOption = 'M';
        } else if (genderValue == 'Female') {
          genderOption = 'F';
        }
        this.PatientInfoForm.controls['gender'].setValue(genderOption, {
          onlySelf: true,
        });
        // this.PatientInfoForm.controls['gender'].setValue(that.resp.Gender, { onlySelf: true });

        this.PatientInfoForm.controls['email'].setValue(
          this.http_rpm_patientList.Email
        );
        this.PatientInfoForm.controls['mobile'].setValue(
          this.http_rpm_patientList.MobileNo
        );
        this.PatientInfoForm.controls['alternatenumber'].setValue(
          this.http_rpm_patientList.AlternateMobNo
        );
        this.PatientInfoForm.controls['addressline1'].setValue(
          this.http_rpm_patientList.Address1
        );
        this.PatientInfoForm.controls['addressline2'].setValue(
          this.http_rpm_patientList.Address2
        );
        this.PatientInfoForm.controls['zipcode'].setValue(
          this.http_rpm_patientList.ZipCode
        );
        this.PatientInfoForm.controls['city'].setValue(
          this.http_rpm_patientList.CityId,
          { onlySelf: true }
        );
        this.PatientInfoForm.controls['state'].setValue(
          this.http_rpm_patientList.StateId
        );
        this.PatientInfoForm.controls['timezone'].setValue(
          this.http_rpm_patientList.TimeZoneID
        );
        this.PatientInfoForm.controls['emergencycontact1'].setValue(
          this.http_rpm_patientList.Contact1Name
        );
        this.PatientInfoForm.controls['relation1'].setValue(
          this.http_rpm_patientList.Contact1RelationName
        );
        this.PatientInfoForm.controls['relation1_mobile'].setValue(
          this.http_rpm_patientList.Contact1Phone
        );
        this.PatientInfoForm.controls['emergencycontact2'].setValue(
          this.http_rpm_patientList.Contact2Name
        );
        this.PatientInfoForm.controls['relation2'].setValue(
          this.http_rpm_patientList.Contact2RelationName
        );
        this.PatientInfoForm.controls['relation2_mobile'].setValue(
          this.http_rpm_patientList.Contact2Phone
        );
        this.PatientInfoForm.controls['calltime'].setValue(
          this.http_rpm_patientList.CallTime
        );
        this.PatientInfoForm.controls['language'].setValue(
          this.http_rpm_patientList.Language
        );
        this.PatientInfoForm.controls['preference2'].setValue(
          this.http_rpm_patientList.Preference1
        );
        this.PatientInfoForm.controls['preference3'].setValue(
          this.http_rpm_patientList.Preference2
        );
        this.PatientInfoForm.controls['preference4'].setValue(
          this.http_rpm_patientList.Preference3
        );

        this.PatientInfoForm.controls['additionalnotes'].setValue(
          this.http_rpm_patientList.Notes
        );
        this.defaultHeight = this.http_rpm_patientList.Height;
        this.weightValue = this.http_rpm_patientList.Weight;

        var patientHeighArray =
          this.http_rpm_patientList.Height.toString().split('.');

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

        that.PatientInfoForm.controls['clinicname'].setValue(
          this.http_rpm_patientList.OrganizationID,
          { onlySelf: true }
        );
        this.getPhysicianList();
        that.programForm.controls['clinic'].setValue(PatientProgramData.Clinic);

        // that.Clinic_Selected = that.master_data.ClinicDetails.filter(
        //   (clinic: { ClinicName: any }) => clinic.ClinicName === PatientProgramData.Clinic
        // );

        var ClinicCode = that.Clinic_Selected[0].ClinicCode;
        that.programForm.controls['cliniccode'].setValue(ClinicCode);

        // that.programForm.controls['cliniccode'].setValue(
        //  this.http_rpm_patientList.ClinicCode
        // );

        that.programForm.controls['physician'].setValue(
          PatientProgramData.Physician
        );
      });
  }

  heightFeetArray = [
    {
      id: 0,
      data: 0,
    },
    {
      id: 1,
      data: 1,
    },
    {
      id: 2,
      data: 2,
    },
    {
      id: 3,
      data: 3,
    },
    {
      id: 4,
      data: 4,
    },
    {
      id: 5,
      data: 5,
    },
    {
      id: 6,
      data: 6,
    },
    {
      id: 7,
      data: 7,
    },
    {
      id: 8,
      data: 8,
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

  Diagnosis_List_tmp: any;
  vitalListChange() {
    this.loading = true;
    var req_body = { VitalIds: this.vitalList };
    this.rpm.rpm_post(`/api/patient/getdiagnosiscodebyvitalid`, req_body).then(
      (data) => {
        this.Diagnosis_Name = data;

        console.log('Diagnosis Ds');
        console.log(this.Diagnosis_Name);
        this.loading = false;
        // New Code Change 29/03/2023
        for (let x of this.Diagnosis_Name) {
          x.DiagnosisField = x.DiagnosisName + '-' + x.DiagnosisCode;
        }
        var j = 0;
        var found = false;
        var indexes = [];
        for (let y of this.Diagnosis_List) {
          found = false;
          for (let x of this.Diagnosis_Name) {
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
          this.Diagnosis_List.splice(obj, 1);
          if (this.Diagnosis_List.length == 0) {
            this.Diagnosis_List = [];
            this.objDiagnosis = {
              Id: 0,
              DiagnosisCode: null,
              DiagnosisName: null,
            };
            this.Diagnosis_List.push(this.objDiagnosis);
          }
        }
      },
      (err) => {
        alert('could not find Diagnosis for the selected Vitals/Conditions');
        this.loading = false;
      }
    );
  }
  deleteDocument(patientId: any) {
    this.rpm
      .rpm_post(`/api/patient/deletedraft?patientid=${patientId}`, {})
      .then(
        (data) => {
          alert('Patient Removed  Successfully');
          this.GetPatientDraftList();
        },
        (err) => {
          console.log(err);
        }
      );
  }
  openDeleteDocumentDialog(patientId: any) {

    this.showconfirmDialog.showConfirmDialog(
      'Do You Want to Delete the Patient ? ',
      'Are you sure?',
      () => this.deleteDocument(patientId),
      true
    );

  }
  tableFilterValue: any;
  searchDraftClose() {
    this.searchPatientName = true;

  }
  applyDataFilter(event: Event) {
    const filterValue = (event.target as HTMLInputElement).value;
    this.datasourceDocument.filter = filterValue.trim().toLowerCase();
    console.log(this.datasourceDocument.filter);
  }

  setupFilter(column: string) {
    this.datasourceDocument.filterPredicate = (d: any, filter: string) => {
      const textToSearch = (d[column] && d[column].toLowerCase()) || '';
      return textToSearch.indexOf(filter) !== -1;
    };
  }
  searchDraft() {
    this.searchPatientName = false;
    this.GetPatientDraftList();
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
    convertToLocalTime(stillUtc: any) {
    if (stillUtc) {
      if (stillUtc.includes('+')) {
        var temp = stillUtc.split('+');
        stillUtc = temp[0];
      }
    }
    stillUtc = stillUtc + 'Z';
    var local = moment(stillUtc).local().format('YYYY-MM-DD HH:mm:ss');
    return local;
  }
}
