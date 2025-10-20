import { Component, OnInit, ViewChild, ChangeDetectorRef } from '@angular/core';
import { SelectionModel } from '@angular/cdk/collections';
import { ActivatedRoute, Router } from '@angular/router';
// import { Options } from '@angular-slider/ngx-slider';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { MatPaginator } from '@angular/material/paginator';
import { RPMService } from '../../sevices/rpm.service';
import { MatTableDataSource } from '@angular/material/table';
import { MatDialog, MatDialogConfig } from '@angular/material/dialog';
import { StatusMessageComponent } from '../../shared/status-message/status-message.component';
import { DatePipe } from '@angular/common';
import { RightSidebarComponent } from '../../shared/right-sidebar/right-sidebar.component';
import { ConfirmDialogComponent } from '../../shared/confirm-dialog/confirm-dialog.component';
import { ConfirmDialogServiceService } from '../../shared/confirm-dialog-panel/service/confirm-dialog-service.service';

@Component({
  selector: 'app-admin',
  templateUrl: './admin.component.html',
  styleUrls: ['./admin.component.scss'],
})
export class AdminComponent implements OnInit {
  http_systemRules: any;
  http_listener: any;
  variable = 1;
  @ViewChild(RightSidebarComponent) private rightSidebar: RightSidebarComponent;

  // Main Table DataSource
  tableDataSource: any;
  // Clinic Tab
  CurrentMenuItem = 1;
  http_clinic_data: any;
  clinic_id: any;
  http_clinicDatabyId: any;
  // Switiching Varibale For Change Text  Update And  Add Text
  switchingVariable = false;
  // active and inactive Clinic Filter
  active_clinic: any;
  inactive_clinic: any;
  // variable used to display table data for active and inactive Clinic Filter
  clinic_data: any;
  //  Temp Table to hold actual table data
  table_temporary: any;
  // display current total for mat table paginator
  total_number: any;
  // Hold Clinic Names onload dropdown
  clinic_name_array: any;
  // Display Total Clinic Number on panel
  totalClinicNumber: any;
  //  http call for state dropdown master data
  http_state: any;
  //  to set ngmodel  for selected state for filter city
  stateVariable: any;

  // UserManagement

  http_userlist: any;
  globelUserData: any;

  // System Rules
  system_rules_variable: any;

  // Clinic Page Registration Start

  // Validation Input

  loading: boolean;
  registerClinic = new FormGroup({
    clinicname: new FormControl(null, [Validators.required]),
    branchname: new FormControl(null),
    cliniccode: new FormControl(null, [Validators.required]),
    mobile: new FormControl(null, [
      Validators.required,
      Validators.pattern('^[0-9]*$'),
      Validators.minLength(10),
      Validators.maxLength(10),
    ]),
    altMobile: new FormControl(null),
    addressline1: new FormControl(null, [Validators.required]),
    addressline2: new FormControl(null),
    clinic_city: new FormControl(null, [Validators.required]),
    clinic_state: new FormControl(null, [Validators.required]),
    time_zone: new FormControl(null, [Validators.required]),
    zipcode: new FormControl(null, [Validators.required]),
  });

  @ViewChild(MatPaginator) paginator: MatPaginator;

  // Main Menu
  adminMenu = [
    {
      menu_id: 1,
      menu_title: 'Clinics',
    },
    {
      menu_id: 2,
      menu_title: 'User Management',
    },
    // {
    //   menu_id: 3,
    //   menu_title: 'System Rules',
    // },
  ];

  CurrentMenu = 1;
  masterdata2: unknown;
  page: any;
  timezones: any;
  clinicName: unknown;
  rolelist: any;
  roles: any;

  // Change menu Tab
  ChangeScreen(button: any) {
    if (button != 3) {
      this.CurrentMenu = button;
      switch (button) {
        case 1:
          this.variable = 1;
          this.getAllClinic();
          this.clearUserSelection();

          break;

        case 2:
          this.variable = 2;
          this.addVar = 1;
          this.user_containerSelection = undefined;
          this.loading = true;
          this.getAllUser();
          this.searchUserList_Name = false;
          this.searchALLValueCode = false;
          this.clearSelection();

          break;
        case 3:
          this.variable = 3;
          break;
      }
    }
  }

  addTeam() {
    this.rightSidebar.add_team();
    // this.teamPage.addteambutton()
    // this.addVar = 9;
  }
  editTeam() {
    this.rightSidebar.edit_team();
  }

  navigateUserPage(user: any) {
    this.user_containerSelection = undefined;
    let route = '/admin/user';
    this.router.navigate([route], {
      queryParams: { id: user.UserId, edit_variable: true },
      skipLocationChange: true,
    });
  }

  constructor(
    private rpm: RPMService,
    private router: Router,
    public dialog: MatDialog,
    private changeDetectorRef: ChangeDetectorRef,
    private _route: ActivatedRoute,
    public datePipe: DatePipe,
    private confirmDialog:ConfirmDialogServiceService,
  ) {
    this._route.queryParams.subscribe((params) => {
      this.page = parseInt(params.page);
      this.http_state = sessionStorage.getItem('states_cities');
      this.http_state = JSON.parse(this.http_state);
      this.timezones = this.http_state.TimeZones;
      var that = this;
      that.rolelist = sessionStorage.getItem('Roles');
      that.roles = JSON.parse(this.rolelist);

      if (this.page) {
        this.ChangeScreen(2);

        this.getAllUser();
      } else {
        this.ChangeScreen(1);
        this.getAllClinic();
      }

      // Get State
      that.http_state = sessionStorage.getItem('states_cities');
      that.http_state = JSON.parse(that.http_state);
      //  Get UserData
    });
  }

  userDataArray: any;
  loading1: any;
  getAllUser() {
    var that = this;
    that.loading = true;

    that.rpm
      .rpm_get('/api/users/getallusers?RoleId=' + that.roles[0].Id)
      .then((data) => {
        that.http_userlist = data;

        that.globelUserData = this.http_userlist.Details;
        this.userDataArray = this.http_userlist.Summary.filter(function (
          data: any
        ) {
          return (
            data.Name != 'EnrollmentMember' && data.Name != 'ProviderWatcher'
          );
        });

        this.tableDataSource = new MatTableDataSource(that.globelUserData);
        this.changeDetectorRef.detectChanges();
        this.tableDataSource.paginator = this.paginator;
        that.loading = false;
      });
  }
  getLastActivity(data: any) {
    if (data == null) {
      return 'Not Applicable';
    } else {
      return this.datePipe.transform(data, 'MMM d, y');
    }
  }
  http_clinic_data_copy: any;
  getAllClinic() {
    this.loading = true;
    this.rpm.rpm_get('/api/clinic/getallclinics').then((data) => {
      this.loading = false;
      this.SelectClinic = 'all';
      this.selectLocation = 'all';
      this.http_clinic_data = data;
      this.http_clinic_data_copy = this.http_clinic_data;
      sessionStorage.setItem(
        'clinic_masterdata',
        JSON.stringify(this.http_clinic_data)
      );
      this.tableDataSource = this.http_clinic_data;
      this.table_temporary = this.tableDataSource;
      // display total number on table
      this.total_number = this.tableDataSource.length;
      // display total box
      this.totalClinicNumber = this.tableDataSource.length;
      // Temporary Array for table display
      this.globalClinics = this.table_temporary;
      // on Load active Clinic
      this.active_clinic = this.http_clinic_data.filter(function (data: any) {
        return data.Active === true;
      });
      // on Load inactive Clinic
      this.inactive_clinic = this.http_clinic_data.filter(function (data: any) {
        return data.Active === false;
      });
      // onload Loaction array for dropdown
      this.selectLocationArray = this.http_clinic_data;

      // this.selectLocationArray = this.removeDuplicates(
      //   this.selectLocationArray
      // );

      this.selectLocationArray = Array.from(
        new Map(
          this.selectLocationArray.map((e: { CityId: any }) => [e.CityId, e])
        ).values()
      );

      // Paginator For Table
      this.tableDataSource = new MatTableDataSource(this.http_clinic_data);
      this.tableDataSource.paginator = this.paginator;

      // this.SetDropdownFilter();
    });
  }

  // Temp Array for holding data For  total number container display
  totalDisplayTable: any;
  clinic_containerSelection = 0;

  // active ,Inactive container Click:
  FilterByStatus(status: any) {
    this.totalDisplayTable = this.globalClinics;
    this.searchValueName = false;
    var filterData;
    if (this.SelectClinic != 'all' && this.selectLocation != 'all') {
      filterData = this.totalDisplayTable.filter((data: any) => {
        return (
          data.Name == this.SelectClinic && data.CityId == this.selectLocation
        );
      });
    } else if (this.SelectClinic != 'all' && this.selectLocation == 'all') {
      filterData = this.totalDisplayTable.filter((data: any) => {
        return data.Name == this.SelectClinic;
      });
    } else if (this.SelectClinic == 'all' && this.selectLocation != 'all') {
      filterData = this.totalDisplayTable.filter((data: any) => {
        return data.CityId == this.selectLocation;
      });
    } else if (this.SelectClinic == 'all' && this.selectLocation == 'all') {
      filterData = this.totalDisplayTable;
    }

    // if (this.selectLocation != 'all') {
    //   filterData = this.totalDisplayTable.filter((data: any) => {
    //     return data.CityId == this.selectLocation;
    //   });
    // }

    //status is either Active or Inactive or None
    if (status == 'Active') {
      this.clinic_containerSelection = 2;
      this.clinic_data = filterData.filter(function (data: any) {
        return data.Active == true;
      });
    } else if (status == 'Inactive') {
      this.clinic_containerSelection = 3;

      this.clinic_data = filterData.filter(function (data: any) {
        return data.Active == false;
      });
    } else if (status == 'none') {
      this.clinic_containerSelection = 1;
      this.clinic_data = filterData;
      this.totalClinicNumber = this.clinic_data.length;
    }

    this.tableDataSource = new MatTableDataSource(this.clinic_data);
    this.changeDetectorRef.detectChanges();
    this.tableDataSource.paginator = this.paginator;
    this.total_number = this.clinic_data.length;

    // this.totalClinicNumber = this.globalClinics.length;
  }

  // Selected Clinic Name
  SelectClinic: any;
  globalClinics: any;
  // Clinic Location array
  selectLocationArray: any;
  //  selected Clinic Location
  selectLocation: any;

  // Dropdown Filter based on Loaction and Clinic name

  SetDropdownFilter() {
    this.clinic_containerSelection = 1;
    this.clinic_data = this.http_clinic_data;

    if (this.SelectClinic != 'all') {
      this.clinic_data = this.clinic_data.filter((data: any) => {
        return data.Name == this.SelectClinic;
      });
    }

    if (this.selectLocation != 'all') {
      this.clinic_data = this.clinic_data.filter((data: any) => {
        return data.CityId == this.selectLocation;
      });
    }

    this.tableDataSource = new MatTableDataSource(this.clinic_data);
    this.changeDetectorRef.detectChanges();
    this.tableDataSource.paginator = this.paginator;
    this.total_number = this.clinic_data.length;
    this.totalClinicNumber = this.tableDataSource.filteredData.length;

    // active_clinic updation based on clinic name and Location
    this.active_clinic = this.clinic_data.filter(function (data: any) {
      return data.Active === true;
    });
    // inactive_clinic updation based on clinic name and Location
    this.inactive_clinic = this.clinic_data.filter(function (data: any) {
      return data.Active === false;
    });
  }

  // Filter City By State
  cityArray: any;
  filterCitybyState() {
    if (this.stateVariable) {
      this.cityArray = this.http_state.Cities.filter((data: any) => {
        {
          return data.StateId == this.stateVariable;
        }
      });
    }
  }

  // Clinic page Functionalites End
  //UserManagement Start

  user_data: any;
  user_containerSelection = undefined;
  userPanelCountClick(content: any) {
    this.user_containerSelection = content;
    this.searchALLValueCode = false;
    this.searchUserList_Name = false;

    if (content == 'Provider') {
      this.user_data = this.globelUserData.filter(function (data: any) {
        return data.Role == 'Provider';
      });
    } else if (content == 'CareTeamManager') {
      this.user_data = this.globelUserData.filter(function (data: any) {
        return data.Role == 'CareTeamManager';
      });
    } else if (content == 'CareTeamMember') {
      this.user_data = this.globelUserData.filter(function (data: any) {
        return data.Role == 'CareTeamMember';
      });
    } else if (content == 'Physician') {
      this.user_data = this.globelUserData.filter(function (data: any) {
        return data.Role == 'Physician';
      });
    } else if (content == 'ClinicManager') {
      this.user_data = this.globelUserData.filter(function (data: any) {
        return data.Role == 'ClinicManager';
      });
    } else {
      this.user_data = this.globelUserData;
    }

    this.tableDataSource = new MatTableDataSource(this.user_data);
    this.changeDetectorRef.detectChanges();
    this.tableDataSource.paginator = this.paginator;
    this.total_number = this.user_data.length;
    this.paginator.firstPage();
    this.clearUserSelection();
  }

  ngOnInit(): void {
    this.initializemedicineColumns();
    this.registerClinic.get('clinic_state')?.valueChanges.subscribe((val) => {
      if (val !== null && val !== undefined) {
        // Only proceed if val is not null or undefined
        this.cityArray = this.http_state.Cities.filter(
          (data: { StateId: any }) => data.StateId === parseInt(val)
        );
      } else {
        // Handle the case when val is null (optional)
        this.cityArray = []; // Or perform some default behavior
      }
    });
  }
  // Usermanagement Data Table
  columnHeader = [
    'selection',
    'Id',
    'Name',
    'Code',
    'PatientCount',
    'PhysicianCount',
    'Active',
    'action',
  ];
  dataSource: any;

  userColumnHeader = [
    'selection',
    'UserName',
    'PatientCount',
    'ActiveSince',
    'Role',
    'Group',
    'LastActivity',
    'Status',
    'action',
  ];

  delete(element: any) {}
  selection: SelectionModel<Element> = new SelectionModel<Element>(false, []);
  someVar: any = false;
  selectRow($event: any, row: any) {
    // $event.preventDefault();
    if (!row.selected) {
      this.tableDataSource.filteredData.forEach(
        (row: { selected: boolean }) => (row.selected = false)
      );
      row.selected = true;
      if (row.selected) {
        this.someVar = true;
      }
    }
  }
  clinicSelected: any;
  navigateAddClinic(dataInput: any) {
    this.rpm.rpm_get(`/api/clinic/getclinic?clinicid=${dataInput.Id}`).then(
      (data) => {
        this.clinicSelected = dataInput.Id;
        this.switchingVariable = true;
        this.http_clinicDatabyId = data;
        this.addVar = 2;
        this.registerClinic.controls.clinicname.setValue(
          this.http_clinicDatabyId.Name
        );
        this.registerClinic.controls.cliniccode.setValue(
          this.http_clinicDatabyId.Code
        );
        this.registerClinic.controls.mobile.setValue(
          this.http_clinicDatabyId.PhoneNumber
        );
        this.registerClinic.controls.altMobile.setValue(
          this.http_clinicDatabyId.AlternateNumber
        );
        this.registerClinic.controls.addressline1.setValue(
          this.http_clinicDatabyId.AddrLine1
        );
        this.registerClinic.controls.addressline2.setValue(
          this.http_clinicDatabyId.AddrLine2
        );
        this.registerClinic.controls.clinic_state.setValue(
          this.http_clinicDatabyId.StateId.toString(),
          { onlySelf: true }
        );
        this.registerClinic.controls.clinic_city.setValue(
          this.http_clinicDatabyId.CityId.toString(),
          { onlySelf: true }
        );
        this.registerClinic.controls.time_zone.setValue(
          this.http_clinicDatabyId.TimeZoneId,
          { onlySelf: true }
        );
        this.registerClinic.controls.zipcode.setValue(
          this.http_clinicDatabyId.ZipCode
        );
      },
      (err) => {
        this.clinicSelected = null;
        this.openDialog('Error', `Something Went Wrong!!! ${err.error}`);
      }
    );
  }

  UpdateClinic() {
    var req_body: any = {};
    if (this.registerClinic.valid) {
      req_body['Id'] = this.clinicSelected;
      req_body['Name'] = this.registerClinic.controls.clinicname.value;
      req_body['Code'] = this.registerClinic.controls.cliniccode.value;
      req_body['ParentOrganizationID'] = 0;
      req_body['PhoneNumber'] = this.registerClinic.controls.mobile.value;
      req_body['AlternateNumber'] =
        this.registerClinic.controls.altMobile.value;
      req_body['AddrLine1'] = this.registerClinic.controls.addressline1.value;
      req_body['AddrLine2'] = this.registerClinic.controls.addressline2.value;
      req_body['ZipCode'] = this.registerClinic.controls.zipcode.value;
      req_body['CityId'] = this.registerClinic.controls.clinic_city.value;
      req_body['StateId'] = this.registerClinic.controls.clinic_state.value;
      req_body['CountryId'] = 233;
      req_body['TimeZoneId'] = this.registerClinic.controls.time_zone.value;

      var that = this;

      this.rpm.rpm_post('/api/clinic/updateclinic', req_body).then(
        (data) => {
          // this.openDialog('Message', `Clinic Data Updated Successfully!!`);
          // alert( data+"New Clinic Updated Successfully!!");
          this.confirmDialog.showConfirmDialog(
            'Clinic Data Updated Successfully!!',
            'Message',
            () => {
              this.updatePatientMasterData();
              this.updateClinicMasterData();
              this.clinicSelected = null;
              this.addVar = 1;
              this.getAllClinic();
              this.registerClinic.reset();
              this.switchingVariable = false;
            },
            false
          );
          // this.updatePatientMasterData();
          // this.updateClinicMasterData();
          // this.clinicSelected = null;
          // this.addVar = 1;
          // this.getAllClinic();
          // this.registerClinic.reset();
          // this.switchingVariable = false;
        },
        (err) => {
          //show error patient id creation failed
          // this.openDialog('Error', `Could not Update Clinic.`);
          this.confirmDialog.showConfirmDialog(
            `Could not Update Clinic.`,
            'Error',
            () => {
              this.updatePatientMasterData();
              this.updateClinicMasterData();
              this.addVar = 1;
            },
            false
          );
          // this.updatePatientMasterData();
          // this.updateClinicMasterData();
          // this.addVar = 1;
        }
      );
    } else {
      alert('Please complete Clinic Details.');
    }
  }


  AddClinic() {
    var req_body: any = {};
    var duplicateClinic = this.http_clinic_data.filter(
      (data: { Code: any }) => {
        return this.registerClinic.controls.cliniccode.value == data.Code;
      }
    );
    if (duplicateClinic.length > 0) {
      alert('Clinic code Already Exists..!');
      return;
    }
    if (this.registerClinic.valid) {
      req_body['Name'] = this.registerClinic.controls.clinicname.value;
      req_body['Code'] = this.registerClinic.controls.cliniccode.value;
      req_body['ParentOrganizationID'] = 0;
      req_body['PhoneNumber'] = this.registerClinic.controls.mobile.value;
      req_body['AlternateNumber'] =
        this.registerClinic.controls.altMobile.value;
      req_body['AddrLine1'] = this.registerClinic.controls.addressline1.value;
      req_body['AddrLine2'] = this.registerClinic.controls.addressline2.value;
      req_body['ZipCode'] = this.registerClinic.controls.zipcode.value;
      req_body['CityId'] = this.registerClinic.controls.clinic_city.value;
      req_body['StateId'] = this.registerClinic.controls.clinic_state.value;
      req_body['CountryId'] = 233;
      req_body['TimeZoneId'] = this.registerClinic.controls.time_zone.value;
      var that = this;

      this.rpm.rpm_post('/api/clinic/addclinic', req_body).then(
        (data) => {
          that.clinic_id = data;
          // this.openDialog('Message', `New Clinic Added Successfully!!`);
          // alert("New Clinic Added Successfully!!")
          this.confirmDialog.showConfirmDialog(
            'New Clinic Added Successfully!!',
            'Message',
            () => {
              this.updatePatientMasterData();
              this.updateClinicMasterData();
              this.addVar = 1;
              this.getAllClinic();
              this.registerClinic.reset();
            },
            false
          );
          // this.updatePatientMasterData();
          // this.updateClinicMasterData();
          // this.addVar = 1;
          // this.getAllClinic();
          // this.registerClinic.reset();
        },
        (err) => {
          //show error patient id creation failed
          // this.openDialog('Error', `Could not Add Clinic.`);
          this.confirmDialog.showConfirmDialog(
            `Could not Add Clinic.`,
            'Error',
            () => {
              null
            },
            false
          );
          this.addVar = 1;
        }
      );
    } else {
      this.confirmDialog.showConfirmDialog(
        `Please complete Clinic Details.`,
        'Warning',
        () => {
          this.markFormGroupTouched(this.registerClinic);
        },
        false
      );
      // this.markFormGroupTouched(this.registerClinic);
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

  applyFilter(filterValue: any) {
    filterValue = (<HTMLInputElement>filterValue.target).value.trim(); // Remove whitespace
    filterValue = filterValue.toLowerCase(); // MatTableDataSource defaults to lowercase matches
    this.tableDataSource.filter = filterValue;
  }

  searchValueName = false;
  searchValueCode = false;
  searchNameClick() {
    this.searchValueName = !this.searchValueName;
  }

  searchCodeClick() {
    this.searchValueCode = !this.searchValueCode;
  }
  closeSerachClick() {
    this.searchValueName = false;
    this.getAllClinic();
    this.clinic_containerSelection = 0;
  }

  searchUserList_Name = false;
  searchALLValueCode = false;
  searchUserListName() {
    this.searchUserList_Name = !this.searchUserList_Name;
    this.searchALLValueCode = false;
  }
  searchUserListClose() {
    this.searchUserList_Name = false;
    this.searchALLValueCode = false;
    this.userPanelCountClick(this.user_containerSelection);
  }
  searchUserALLCODE() {
    this.searchALLValueCode = !this.searchALLValueCode;
    this.searchUserList_Name = false;
  }
  searchUserALLClose() {
    this.searchALLValueCode = false;
    this.searchUserList_Name = false;
    this.userPanelCountClick(this.user_containerSelection);
  }
  // System Rules:
  SystemRule = [
    {
      menu_id: 1,
      menu_title: 'Program & Goals',
    },
    {
      menu_id: 2,
      menu_title: 'Vital Rules',
    },

    {
      menu_id: 3,
      menu_title: 'Billing Codes',
    },
  ];

  onClickNavigation(id: any) {
    this.CurrentMenuItem = id;

    switch (id) {
      case 1:
        this.system_rules_variable = 1;
        break;
      case 2:
        this.system_rules_variable = 2;
        break;
      case 3:
        this.system_rules_variable = 3;
        break;
      default:
        this.system_rules_variable = 1;
        break;
    }
  }

  // Billing Details

  BillingDetails = [
    {
      cpt_code: '99453',
      requirement: [
        'One time per RPM program ',
        'Billed in First billing cycle of program activation',
        'One Vital read is a must',
      ],
      billing_period: 30,
      billing_period_unit: 'days',

      target_text: 'One time',
      target_min: '16',
      target_unit: 'Days',
      billing_frequency: 1,
      billing_frequency_unit: 'year',
    },
    {
      cpt_code: '99453',
      requirement: [
        'One time per RPM program ',
        'Billed in First billing cycle of program activation',
        'One Vital read is a must',
      ],
      billing_period: 30,
      billing_period_unit: 'days',

      target_text: 'One time',
      target_min: '16',
      target_unit: 'Days',
      billing_frequency: 1,
      billing_frequency_unit: 'year',
    },
  ];

  medicineTableColumns: any;

  initializemedicineColumns(): void {
    this.medicineTableColumns = [
      {
        name: 'Medicine Name',
        logo: 'home',
        dataKey: 'medicine_name',
        position: 'left',
        isSortable: false,
      },
      {
        name: 'Category',
        logo: 'home',
        dataKey: 'category',
        position: 'left',
        isSortable: false,
      },
      {
        name: 'Applicable for Symptoms',
        logo: 'home',
        dataKey: 'Applicable_for_Symptoms',
        position: 'left',
        isSortable: false,
      },
      {
        name: 'Remarks',
        logo: 'home',
        dataKey: 'remarks',
        position: 'left',
        isSortable: false,
      },
    ];
  }
  getMedicineList(): any[] {
    return [
      {
        medicine_name: 'Longer Medicine Name',
        category: 'Medicine Category 1',
        Applicable_for_Symptoms: 'Symptom 1, Symptom 2',
        remarks: 'Chance of Drowsiness',
      },
      {
        medicine_name: 'Longer Medicine Name',
        category: 'Medicine Category 1',
        Applicable_for_Symptoms: 'Symptom 1, Symptom 2',
        remarks: 'Chance of Drowsiness',
      },
      {
        medicine_name: 'Longer Medicine Name',
        category: 'Medicine Category 1',
        Applicable_for_Symptoms: 'Symptom 1, Symptom 2',
        remarks: 'Chance of Drowsiness',
      },
      {
        medicine_name: 'Longer Medicine Name',
        category: 'Medicine Category 1',
        Applicable_for_Symptoms: 'Symptom 1, Symptom 2',
        remarks: 'Chance of Drowsiness',
      },
      {
        medicine_name: 'Longer Medicine Name',
        category: 'Medicine Category 1',
        Applicable_for_Symptoms: 'Symptom 1, Symptom 2',
        remarks: 'Chance of Drowsiness',
      },
      {
        medicine_name: 'Longer Medicine Name',
        category: 'Medicine Category 1',
        Applicable_for_Symptoms: 'Symptom 1, Symptom 2',
        remarks: 'Chance of Drowsiness',
      },
      {
        medicine_name: 'Longer Medicine Name',
        category: 'Medicine Category 1',
        Applicable_for_Symptoms: 'Symptom 1, Symptom 2',
        remarks: 'Chance of Drowsiness',
      },
      {
        medicine_name: 'Longer Medicine Name',
        category: 'Medicine Category 1',
        Applicable_for_Symptoms: 'Symptom 1, Symptom 2',
        remarks: 'Chance of Drowsiness',
      },
    ];
  }
  addUser() {
    this.router.navigateByUrl('/admin/user');
  }

  addVar = 1;

  addClinicPage() {
    this.addVar = 2;
  }
  addProgramPage() {
    this.addVar = 3;
  }
  onClickCancel() {
    this.addVar = 1;
    this.switchingVariable = false;
    this.registerClinic.reset();
    this.getAllClinic();
  }
  addBillingCode() {
    this.addVar = 4;
  }
  editProgram() {
    this.addVar = 5;
  }
  editBillingCode() {
    this.addVar = 6;
  }
  addVital() {
    this.addVar = 7;
  }
  editVitalPage() {
    this.addVar = 8;
  }
  editdurationValue = 12;
  edit_increment() {
    this.editdurationValue++;
  }
  edit_decrement() {
    if (this.editdurationValue > 0) {
      this.editdurationValue--;
    } else {
      this.editdurationValue = 0;
    }
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

  frequencyValue = 0;
  frequencyincrement() {
    this.frequencyValue++;
  }
  frequencydecrement() {
    if (this.frequencyValue > 0) {
      this.frequencyValue--;
    } else {
      this.frequencyValue = 0;
    }
  }

  dataSource_element: any = [];
  onAddData() {
    this.dataSource_element.push(this.dataSource_element.length);
  }
  minValue: number = 30;
  maxValue: number = 75;
  middle: number = 20;

  // options: Options = {
  //   floor: 0,
  //   step: 10,
  //   ceil: 100,
  //   showTicks: false,
  // };

  displayValue() {}

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
  updateClinicMasterData() {
    this.rpm.rpm_get('/api/clinic/getallclinics').then((data) => {
      this.clinicName = data;
      sessionStorage.setItem(
        'clinic_masterdata',
        JSON.stringify(this.clinicName)
      );
    });
  }
  updatePatientMasterData() {
    this.rolelist = sessionStorage.getItem('Roles');
    this.rolelist = JSON.parse(this.rolelist);
    this.rpm
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
  }

  backtohome() {
    this.rolelist = sessionStorage.getItem('Roles');
    this.rolelist = JSON.parse(this.rolelist);
    if (this.rolelist[0].Id == 7) {
      this.router.navigate(['/admin/patient-home']);
    } else {
      this.router.navigate(['/admin/home']);
    }
    // let route = '/admin/home';
    // this.router.navigate([route]);
  }

  reloadData: any;
  applyFilterclinicNameby(event: Event) {
    const filterValue = (event.target as HTMLInputElement).value;

    var dataSourceTaskFilter;
    this.reloadData = this.tableDataSource.filteredData;
    dataSourceTaskFilter = this.tableDataSource.filteredData.filter(function (
      data: any
    ) {
      return data.Name.trim()
        .toLowerCase()
        .includes(filterValue.trim().toLowerCase());
    });

    this.tableDataSource = new MatTableDataSource(dataSourceTaskFilter);
    this.tableDataSource.paginator = this.paginator;
  }

  onreloadAfterFilter() {
    if (this.reloadData != undefined) {
      if (this.reloadData.length == 0) {
        this.tableDataSource = new MatTableDataSource(
          this.tableDataSource.filteredData
        );
        this.tableDataSource.paginator = this.paginator;
        this.reloadData = [];
      } else {
        this.tableDataSource = new MatTableDataSource(this.reloadData);
        this.tableDataSource.paginator = this.paginator;
        this.reloadData = [];
      }
    }
  }

  setupFilter(column: string) {
    this.tableDataSource.filterPredicate = (d: any, filter: string) => {
      const textToSearch = (d[column] && d[column].toLowerCase()) || '';
      return textToSearch.indexOf(filter) !== -1;
    };
  }

  applyDataFilter(filterValue: any) {
    filterValue = (<HTMLInputElement>filterValue.target).value.trim(); // Remove whitespace
    filterValue = filterValue.toLowerCase(); // MatTableDataSource defaults to lowercase matches
    this.tableDataSource.filter = filterValue;
  }
  removeDuplicates(arr: any) {
    arr.filter(
      (value: any, index: any, self: any) =>
        index === self.findIndex((t: any) => t.CityId === value.CityId)
    );
    // return arr.filter((item: any, index: any) => arr.indexOf(item) === index);
  }

  // openDeactivateDialog(documnetId: any) {
  //   const dialogRef = this.dialog.open(ConfirmDialogComponent, {
  //     maxWidth: '400px',
  //     data: {
  //       title: 'Are you sure?',
  //       message: 'Do You Want to Deactivate the User ? ',
  //     },
  //   });
  //   dialogRef.afterClosed().subscribe((dialogResult: any) => {


  //     if (dialogResult) {
  //       this.deactivateUser(documnetId);
  //     } else {
  //       return;
  //     }
  //   });
  // }
  openDeactivateDialog(documentId: any) {
    this.confirmDialog.showConfirmDialog(
      'Do You Want to Deactivate the User? ',
      'Are you sure?',
      () => {
      this.deactivateUser(documentId);
      },
      true  // showCancel = true enables Confirm/Cancel options
    );
    }
       deactivateUser(data: any) {
      this.rpm
        .rpm_post(`/api/users/deactivateuser?userid=${data.UserId}`, {})
        .then(
        (res) => {
          this.confirmDialog.showConfirmDialog(
          'User Deactivated Successfully',
          'Message',
          () => {
            this.getAllUser();
          },
          false
          );
        },
        (err) => {
          this.confirmDialog.showConfirmDialog(
          err.error.message || 'Could not deactivate user.',
          'Error',
          null,
          false
          );
        }
        );
      }
  inactivestatus: any;
  getUserstatus(data: any) {
    if (data == true) {
      this.inactivestatus = false;
      return 'Active';
    } else if (data == false) {
      this.inactivestatus = true;
      return 'Inactive';
    } else {
      return 'undefined';
    }
  }
  checkUser(data: any) {
    if (data.UserId != this.roles[0].Id) {
      return false;
    } else {
      return true;
    }
  }
  clearSelection() {
    if (this.http_clinic_data) {
      this.http_clinic_data &&
        this.http_clinic_data.forEach(
          (row: { selected: boolean }) => (row.selected = false)
        );
    } else {
      return;
    }
  }
  clearUserSelection() {
    if (this.tableDataSource) {
      this.tableDataSource &&
        this.tableDataSource.filteredData.forEach(
          (row: { selected: boolean }) => (row.selected = false)
        );
    } else {
      return;
    }
  }
}
