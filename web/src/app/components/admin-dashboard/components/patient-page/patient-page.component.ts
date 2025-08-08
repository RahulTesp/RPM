import {
  Component,
  OnInit,
  ViewChild,
  ChangeDetectorRef,
  AfterViewInit,
} from '@angular/core';
import { HttpService } from '../../sevices/http.service';
import { ChartOptions, ChartType, ChartDataset } from 'chart.js';
// import { Label } from 'ng2-charts';
import { MatTableDataSource } from '@angular/material/table';
import { MatPaginator, PageEvent } from '@angular/material/paginator';
import { Router, ActivatedRoute } from '@angular/router';
import { RPMService } from '../../sevices/rpm.service';
import { SelectionModel } from '@angular/cdk/collections';
import { MatSort, Sort } from '@angular/material/sort';
import { DatePipe } from '@angular/common';
import { AuthService } from 'src/app/services/auth.service';
import dayjs from 'dayjs';
import utc from 'dayjs/plugin/utc';
import timezone from 'dayjs/plugin/timezone';

dayjs.extend(utc);
dayjs.extend(timezone);
@Component({
  selector: 'app-patient-page',
  templateUrl: './patient-page.component.html',
  styleUrls: ['./patient-page.component.scss'],
})
export class PatientPageComponent implements OnInit {
  patientpageflag: any;
  variable: any;
  http_patient_data: any;
  dataSourceBilling: any;
  billing_missing_info: any;
  billing_onhold_info: any;
  billing_ready_to_bill: any;
  billing_total: any;

  trends_frequency: number;
  year_selected: any;
  graph_dataSource_2: any;
  year_selected_2: any;
  trends_frequency_2: number;
  cpt_code: string;
  graph_model_2: any;
  duration_val: any;
  billing_duration_val = 0;

  accessRights: any;
  // Table DataSource
  dataSourceTableList: any;
  @ViewChild(MatPaginator) paginator: MatPaginator;
  @ViewChild(MatSort) sort = new MatSort();

  loading: boolean;
  http_patients_with_device: any;
  loading1: boolean;
  // Total Patient List temporary Array
  total_patient_list_tmp: [];
  cautious_patient_data_array: any;
  normal_patient_data_array: any;

  // DropDown List
  patientListArray: any;
  patientVitalsArray: any;
  patientactiveArray: any;
  // Select DropDown
  patientTypeSelect: any;
  patientVitalSelect: any;
  patienttypeDataArray: any;
  patientVitalDataArray: any;

  totaltableList: any;
  http_billing_data: any;
  http_billing_data_info: any;

  total_patient_billing: any;
  billing_cpt_code: any;
  billing_type: any;
  rolelist: any;
  roles: any;
  durationUrldata: any;
  billingTypevariable: any;

  billing_last_click() {
    this.billing_duration_val = 1;
  }
  billing_next_click() {
    this.billing_duration_val = 2;
  }
  billing_today() {
    this.billing_duration_val = 0;
  }

  menu = [
    {
      menu_id: 1,
      menu_title: 'Patient List',
    },
    {
      menu_id: 2,
      menu_title: 'Billing Data',
    },
    // {
    //   menu_id: 3,
    //   menu_title: 'Patient Trends',
    // },
  ];

  CurrentMenuItem: any = 1;
  // Change Main Screen

  ChangeScreen(button: any) {
    this.patientNavigationStatus = sessionStorage.getItem(
      'patientNavigationStatus'
    );
    this.CurrentMenuItem = button;

    switch (button) {
      case 1:
        this.variable = 1;
        //Navigation change
        if (this.durationSelectionValue == undefined) {
          this.durationSelectionValue = 7;
        }
        this.patientTypeSelect = undefined;
        this.patientVitalSelect = undefined;
        this.maskingDataSrc();
        this.total_patient_list_tmp = this.masterData;

        // this.searchPatient_assignMember = false;
        // this.searchPatientList_physician = false;
        // this.searchPatientList_Name = false;
        // this.searchPatientList_Id = false;
        // this.searchPatientList_assign = false;

        this.dataSourceTableList = new MatTableDataSource(
          this.total_patient_list_tmp
        );
        this.changeDetectorRef.detectChanges();
        this.dataSourceTableList.paginator = this.paginator;
        this.dataSourceTableList.sort = this.sort;
        break;

      case 2:
        this.variable = 2;
        this.billingcontainer = 1;
        this.billingpgm = 'undefined';
        this.patientTypebilling = 'undefined';
        this.totalvitalfiltervalue = 'undefined';
        this.interactionFilterValue = 'undefined';

        this.SearchpatientBilling = '';
        this.billingAssigneeName = '';
        this.searchValueName = false;
        this.searchPatientList_assign = false;
        this.c1 = false;
        this.c2 = false;

        this.getBillingDataSource();
        if (this.dataSourceTableList) {
          this.dataSourceTableList.paginator = this.paginator;
          this.dataSourceTableList.sort = this.sort;
        }

        break;

      case 3:
        this.variable = 3;
        break;

      default:
        this.variable = 1;
        break;
    }
  }

  constructor(
    private rpm: RPMService,
    private http: HttpService,
    private _router: Router,
    public datepipe: DatePipe,
    private changeDetectorRef: ChangeDetectorRef,
    private _route: ActivatedRoute,
    private Auth: AuthService
  ) {
    var that = this;
    this.patientpageflag = false;
    this.loading1 = true;
    this.rolelist = sessionStorage.getItem('Roles');
    this.roles = JSON.parse(this.rolelist);
    this.getBillingTypeProgram();
  }

  mainDataSource: any;
  rowCick(elemnet: any) {}
  PatientList_columnHeader = [
    'selection',
    'PatientId',
    'PatientNumber',
    'PatientName',
    'ProgramName',
    'ProgramDuration',
    'PhysicianName',
    'CinicNameOrAssigned',
    'Assigned',
    'Priority',
    'action',
    'PatientProgramId',
  ];

  radioOptions = false;
  rowselect = false;
  currindex: any;

  selection: SelectionModel<Element> = new SelectionModel<Element>(false, []);
  someVar: any = false;
  selectRow($event: any, row: any) {
    // $event.preventDefault();
    // if (!row.selected) {
    //   this.dataSourceTableList.filteredData.forEach(
    //     (row: { selected: boolean }) => (row.selected = false)
    //   );
    //   row.selected = true;
    //   if (row.selected) {
    //     this.someVar = true;
    //   }
    // }
    if (!row.selected) {
      this.dataSourceTableList.filteredData.forEach(
        (row: any) => (row.selected = false)
      );
      row.selected = true;
    }
    this.someVar = true;
  }
  containerSelection: any;
  billingcontainer = 1;
  critical_patient_data_array: any;
  onClickPatientCritical() {
    this.containerSelection = 2;
    this.patientProgramStaus = 'Critical';

    this.maskingDataSrc();
    this.searchPatientList_Name = false;
    this.searchPatientList_assign = false;
    this.searchPatientList_physician = false;
    this.searchPatient_assignMember = false;
    this.searchProgram_Name = false;
  }

  onClickPatientCautious() {
    this.containerSelection = 3;
    this.patientProgramStaus = 'Cautious';
    this.maskingDataSrc();
    this.searchPatientList_Name = false;
    this.searchPatientList_assign = false;
    this.searchPatientList_physician = false;
    this.searchPatient_assignMember = false;
    this.searchProgram_Name = false;
  }
  onClickPatientNormal() {
    this.containerSelection = 4;
    this.patientProgramStaus = 'Normal';
    this.maskingDataSrc();
    this.searchPatientList_Name = false;
    this.searchPatientList_assign = false;
    this.searchPatientList_physician = false;
    this.searchPatient_assignMember = false;
    this.searchProgram_Name = false;
  }

  onClickPatientTotal() {
    this.containerSelection = 1;
    this.patientProgramStaus = undefined;

    this.maskingDataSrc();
    this.searchPatientList_Name = false;
    this.searchPatientList_assign = false;
    this.searchPatientList_physician = false;
    this.searchPatient_assignMember = false;
    this.searchProgram_Name = false;
  }

  refreshTable() {
    this.containerSelection = 1;
    this.patientProgramStaus = undefined;
    this.patientTypeSelect = undefined;
    this.patientVitalSelect = undefined;
    this.maskingDataSrc();
  }
  patientCategory: any;
  patientProgramStaus: any;
  clinicorTeam: any;
  homeVitalSelection: any;
  homedurationvalue: any;
  durationActivePatients: any;
  homePageoption: any;
  SelectedMenu: any;
  pagestatus: any;

  ngOnInit(): void {
    this.patientProgramStaus = undefined;
    this.patientTypeSelect = undefined;
    this.patientVitalSelect = undefined;
    this.durationSelectionValue = undefined;
    this.clinicorTeam = undefined;
    this.firstload = true;
    this._route.queryParams.subscribe((params) => {
      this.patientNavigationStatus = sessionStorage.getItem(
        'patientNavigationStatus'
      );
      this.pagestatus = sessionStorage.getItem('patient-page-status');
      if (
        this.patientNavigationStatus == 'true' &&
        this.pagestatus == 'patient-page'
      ) {
        this.CurrentMenuItem = sessionStorage.getItem('SelectedMenu');
        this.variable = this.SelectedMenu;
        if (this.CurrentMenuItem == 1) {
          this.patientTypeSelect = sessionStorage.getItem('patientTypeStatus');
          this.patientVitalSelect =
            sessionStorage.getItem('patientVitalStatus');
          this.patientProgramStaus = sessionStorage.getItem(
            'patientProgramStatus'
          );
          this.durationSelectionValue = sessionStorage.getItem(
            'programDurationSelect'
          );
          this.durationSelectionValue = parseInt(this.durationSelectionValue);

          if (this.patientTypeSelect == 'undefined') {
            this.patientTypeSelect = undefined;
          } else if (this.patientVitalSelect == 'undefined') {
            this.patientVitalSelect = undefined;
          } else if (this.patientProgramStaus == 'undefined') {
            this.patientProgramStaus = undefined;
          }
          this.tableFilterValue = sessionStorage.getItem('SelectedFilter');
          this.pageIndxValue = sessionStorage.getItem('patientPageIndex')!;
          this.pageIndxValue = parseInt(this.pageIndxValue);
          this.searchPatientList_physician = sessionStorage.getItem(
            'searchPatientList_physician'
          );
          if (this.searchPatientList_physician == 'true') {
            this.searchPatientList_physician = true;
          } else {
            this.searchPatientList_physician = false;
          }
          this.searchPatientList_Name = sessionStorage.getItem(
            'searchPatientList_Name'
          );
          if (this.searchPatientList_Name == 'true') {
            this.searchPatientList_Name = true;
          } else {
            this.searchPatientList_Name = false;
          }
          this.searchPatientList_assign = sessionStorage.getItem(
            'searchPatientList_assign'
          );
          if (this.searchPatientList_assign == 'true') {
            this.searchPatientList_assign = true;
          } else {
            this.searchPatientList_assign = false;
          }
          this.searchPatient_assignMember = sessionStorage.getItem(
            'searchPatient_assignMember'
          );
          if (this.searchPatient_assignMember == 'true') {
            this.searchPatient_assignMember = true;
          } else {
            this.searchPatient_assignMember = false;
          }
          this.searchProgram_Name = sessionStorage.getItem(
            'searchProgram_Name'
          );
          if (this.searchProgram_Name == 'true') {
            this.searchProgram_Name = true;
          } else {
            this.searchProgram_Name = false;
          }
          this.maskingDataSrc();
        } else if (this.CurrentMenuItem == 2) {
          this.variable = 2;
          this.patientTypebilling =
            sessionStorage.getItem('patientTypeBilling')!;
          this.billingTypevariable = sessionStorage.getItem(
            'billingTypeVariable'
          );
          this.programType = sessionStorage.getItem('patientProgram')!;

          this.c1 = JSON.parse(sessionStorage.getItem('CycleOne')!);
          this.c2 = JSON.parse(sessionStorage.getItem('CycleTwo')!);
          this.Today = JSON.parse(sessionStorage.getItem('Today')!);
          this.next7days = JSON.parse(sessionStorage.getItem('next7Day')!);
          this.billingcontainer = JSON.parse(
            sessionStorage.getItem('billingContainer')!
          );
          this.billingpgm = sessionStorage.getItem('billingProgram')!;
          this.totalvitalfiltervalue =
            sessionStorage.getItem('totalVitalValue')!;
          this.interactionFilterValue =
            sessionStorage.getItem('interactionValue')!;
          this.pageIndxValue = sessionStorage.getItem('pageIndexbilling')!;
          this.pageIndxValue = parseInt(this.pageIndxValue);
          this.SearchpatientBilling = sessionStorage.getItem(
            'searchBillingPatients'
          )!;
          if (this.SearchpatientBilling == null) {
            this.SearchpatientBilling = '';
          }
          this.billingAssigneeName = sessionStorage.removeItem(
            'BillingAssigneeName'
          )!;
          if (this.billingAssigneeName == null) {
            this.billingAssigneeName = '';
          }
          this.billlingContainerClick(this.billingcontainer);

          sessionStorage.removeItem('patientNavigationStatus');
          sessionStorage.removeItem('SelectedMenu');
        }
      } else {
        sessionStorage.removeItem('patient-page-status');
        this.homePageoption = params.PageOption;
        this.patientCategory = params.patientCategory;

        this.patientProgramStaus = params.programStatus;
        this.clinicorTeam = params.clinicorTeam;
        this.homeVitalSelection = params.vitalSelection;
        this.homedurationvalue = params.durationSelection;
        this.searchValueId = false;
        this.searchValueName = false;
        this.searchPatientList_Id = false;
        this.searchPatientList_Name = false;
        this.searchPatientList_assign = false;
        this.searchPatientList_physician = false;
        this.searchPatient_assignMember = false;
        this.searchProgram_Name=false;
        if (this.patientCategory) {
          this.homeVitalSelection = undefined;
          this.homedurationvalue = undefined;
        }
        if (this.homeVitalSelection) {
          this.patientCategory = undefined;
        }

        this.patientTypeSelect = this.patientCategory;
        this.patientVitalSelect = this.homeVitalSelection;

        if (this.homedurationvalue) {
          if (this.homedurationvalue == '1') {
            this.durationSelectionValue = 0;
          } else if (this.homedurationvalue == '2') {
            this.durationSelectionValue = 7;
          } else if (this.homedurationvalue == '3') {
            this.durationSelectionValue = 30;
          }
        } else if (this.durationSelectionValue == undefined) {
          this.durationSelectionValue = 7;
        }

        this.maskingDataSrc();
        this.variable = 1;
      }
    });
    this.accessRights = sessionStorage.getItem('useraccessrights');
    this.accessRights = JSON.parse(this.accessRights);
    this.critical_patient_data_array = [];
    this.cautious_patient_data_array = [];
    this.normal_patient_data_array = [];

    // this.variable = 1;
  }

  clickHandler(row: any) {
    let route = '/admin/patients_detail';
    this._router.navigate([route], {
      queryParams: {
        id: row.PatientId,
        programId: row.PatientProgramId,
        patientUserName: row.PatientNumber,
      },
      skipLocationChange: true,
    });
    sessionStorage.setItem('patientTypeStatus', this.patientTypeSelect);
    sessionStorage.setItem('patientVitalStatus', this.patientVitalSelect);
    sessionStorage.setItem('patientProgramStatus', this.patientProgramStaus);
    sessionStorage.setItem(
      'programDurationSelect',
      this.durationSelectionValue
    );
    sessionStorage.setItem('SelectedMenu', this.CurrentMenuItem);
    sessionStorage.setItem('SelectedFilter', this.tableFilterValue);
    sessionStorage.setItem('patientPageIndex', this.pageIndxValue);
    sessionStorage.setItem(
      'searchPatientList_physician',
      this.searchPatientList_physician
    );
    sessionStorage.setItem(
      'searchPatientList_Name',
      this.searchPatientList_Name
    );
    sessionStorage.setItem(
      'searchPatientList_assign',
      this.searchPatientList_assign
    );
    sessionStorage.setItem(
      'searchPatient_assignMember',
      this.searchPatient_assignMember
    );
 sessionStorage.setItem(
      'searchProgram_Name',
      this.searchProgram_Name
    );

    sessionStorage.setItem('patient-page-status', 'patient-page');
  }
  addpatient() {
    this._router.navigate(['/admin/addpatient']);
  }
  backtohome() {
    let route = '/admin/home';
    this._router.navigate([route]);
  }

  entroll_date: any;
  getEntrollDate(data: any) {
    data = data.trim();

    if (data != '') {
      return this.datepipe.transform(this.convertToLocalTime(data), 'MMM d, y');
    } else {
      return 'Not Enrolled';
    }
  }

  getEntrolldateCopy(data: any) {
    data = data.trim();

    if (data != '') {
      return data;
    } else {
      return '12/12/1900';
    }
  }
  reset: boolean;

  //**************************************************************************************************** */
  //Patient Data List Source:

  activePatientDataSrc: any;
  nonActivePatientDataSrc: any;
  masterData: any;

  maskingDataSrc() {
    this.ProcessPatientList();
  }
  totalPatientCount: any;
  setDataTable(data: any) {
    this.masterData = data;
    this.dataSourceTableList = new MatTableDataSource(this.masterData);
    this.patientNavigationStatus = sessionStorage.getItem(
      'patientNavigationStatus'
    );
    if (this.patientNavigationStatus == 'true') {
      this.redirectionFilter(this.tableFilterValue);
    }
    this.totalPatientCount = this.dataSourceTableList.filteredData.length;

    this.changeDetectorRef.detectChanges();
    this.dataSourceTableList.paginator = this.paginator;

    // this.paginator.firstPage();

    this.dataSourceTableList.sort = this.sort;
    this.dataSourceTableList.sortingDataAccessor = (
      item: { [x: string]: any; ProgramDuration: string | number | Date },
      property: string | number
    ) => {
      switch (property) {
        case 'ProgramDuration':
          return new Date(this.getEntrolldateCopy(item.EnrolledDate));
        default:
          return item[property];
      }
    };
    // this.paginator.firstPage();
    this.containerPanelSelection();
    sessionStorage.removeItem('patientNavigationStatus');
    sessionStorage.removeItem('SelectedMenu');
    sessionStorage.removeItem('SelectedFilter');
    sessionStorage.removeItem('patientPageIndex');
  }

  patiletListLoading: any;
  activeProgramStatusHomeCount: any;
  firstload = true;
  ProcessPatientList() {
    var that = this;
    // this.searchPatient_assignMember = false;
    // this.searchPatientList_physician = false;
    // this.searchPatientList_Name = false;
    // this.searchPatientList_Id = false;
    // this.searchPatientList_assign = false;
    this.patiletListLoading = true;
    this.HighlightButton(this.durationSelectionValue);
    var todayDate = new Date().toISOString();
    var offsetValue = this.getTimeZoneOffsetData();
    that.rpm
      .rpm_get(
        `/api/patient/getallpatientslist?ToDate=${todayDate}&UtcOffset=${offsetValue}&Days=${this.durationSelectionValue}&RoleId=${this.roles[0].Id}`
      )
      .then((data2) => {
        that.activePatientDataSrc = data2;
        var tempactivePatientDataSrc = that.activePatientDataSrc;

        var activeFilterVarible;

        // VitalArray:
        if (this.firstload) {
          this.patientVitalsArray = Array.from(
            new Set(
              tempactivePatientDataSrc.map((a: { Vital: any }) => a.Vital)
            )
          ).map((Vital) => {
            return tempactivePatientDataSrc.find(
              (a: { Vital: any }) => a.Vital === Vital
            );
          });
          console.log('Patient Vital array'+this.patientVitalsArray)
        }
        this.firstload = false;
        var tempCountdatasrc;

        //Filter Based On Clinic Or Team

        if (this.clinicorTeam) {
          activeFilterVarible = this.filterBasedOnClinicOrTeam(
            tempactivePatientDataSrc
          );
          tempCountdatasrc = this.filterBasedOnClinicOrTeam(
            tempactivePatientDataSrc
          );
        } else {
          activeFilterVarible = tempactivePatientDataSrc;
          tempCountdatasrc = tempactivePatientDataSrc;
        }

        this.CalCulateCriticalCautiousCount(tempCountdatasrc);

        if (
          this.patientVitalSelect == 'All Vitals' ||
          this.patientVitalSelect == 'undefined'
        ) {
          this.patientVitalSelect = undefined;
        }

        if (this.patientProgramStaus == 'undefined') {
          this.patientProgramStaus = undefined;
        }
        // Filter Based PatientType PatientVital and programStatus
        if (
          this.patientVitalSelect != undefined &&
          this.patientTypeSelect != undefined &&
          this.patientProgramStaus != undefined
        ) {
          activeFilterVarible = activeFilterVarible.filter((data: any) => {
            return (
              data.PatientType == this.patientTypeSelect &&
              data.Priority == this.patientProgramStaus &&
              data.Vital == this.patientVitalSelect
            );
          });
        } else if (
          this.patientVitalSelect == undefined &&
          this.patientTypeSelect != undefined &&
          this.patientProgramStaus != undefined
        ) {
          activeFilterVarible = activeFilterVarible.filter((data: any) => {
            return (
              data.PatientType == this.patientTypeSelect &&
              data.Priority == this.patientProgramStaus
            );
          });
        } else if (
          this.patientVitalSelect != undefined &&
          this.patientTypeSelect == undefined &&
          this.patientProgramStaus != undefined
        ) {
          activeFilterVarible = activeFilterVarible.filter((data: any) => {
            return (
              data.Vital == this.patientVitalSelect &&
              data.Priority == this.patientProgramStaus
            );
          });
        } else if (
          this.patientVitalSelect != undefined &&
          this.patientTypeSelect != undefined &&
          this.patientProgramStaus == undefined
        ) {
          activeFilterVarible = activeFilterVarible.filter((data: any) => {
            return (
              data.Vital == this.patientVitalSelect &&
              data.PatientType == this.patientTypeSelect
            );
          });
        } else if (
          this.patientVitalSelect != undefined &&
          this.patientTypeSelect == undefined &&
          this.patientProgramStaus == undefined
        ) {
          activeFilterVarible = activeFilterVarible.filter((data: any) => {
            return data.Vital == this.patientVitalSelect;
          });
        } else if (
          this.patientVitalSelect == undefined &&
          this.patientTypeSelect != undefined &&
          this.patientProgramStaus == undefined
        ) {
          activeFilterVarible = activeFilterVarible.filter((data: any) => {
            return data.PatientType == this.patientTypeSelect;
          });
        } else if (
          this.patientVitalSelect == undefined &&
          this.patientTypeSelect == undefined &&
          this.patientProgramStaus != undefined
        ) {
          activeFilterVarible = activeFilterVarible.filter((data: any) => {
            return data.Priority == this.patientProgramStaus;
          });
        } else if (
          this.patientVitalSelect == undefined &&
          this.patientTypeSelect == undefined &&
          this.patientProgramStaus != undefined
        ) {
          activeFilterVarible = tempactivePatientDataSrc;
        }

        this.setDataTable(activeFilterVarible);

        this.patiletListLoading = false;
      });
  }

  // Clinic Based Filter Function
  filterBasedOnClinicOrTeam(dataSrc: any) {
    var myid = sessionStorage.getItem('userid');
    var user_name = sessionStorage.getItem('user_name');
    if (this.clinicorTeam) {
      if (
        this.clinicorTeam == 'All Teams' ||
        this.clinicorTeam == 'All Clinics'
      ) {
        dataSrc = dataSrc;
      } else if (this.clinicorTeam == 'MyPatients') {
        if (this.roles[0].Id == 6) {
          dataSrc = dataSrc.filter((data: any) => {
            if (data.PhysicianName != undefined) {
              return data.PhysicianName == user_name;
            }

            return;
          });
        } else if (this.roles[0].Id == 8) {
          // New User Clinic Manager
          return dataSrc;
        } else {
          dataSrc = dataSrc.filter(function (data: any) {
            if (data.AssignedMemberId != undefined) {
              return data.AssignedMemberId == myid;
            }
            return;
          });
        }
      } else {
        if (this.roles[0].Id == 1) {
          dataSrc = dataSrc.filter((data: any) => {
            if (data.ClinicName != undefined) {
              return data.ClinicName == this.clinicorTeam;
            }
            return;
          });
        } else {
          dataSrc = dataSrc;
        }
      }
    }
    return dataSrc;
  }

  HighlightButton(durationSelect: any) {
    if (durationSelect == 0) {
      this.duration_val = 0;
    } else if (durationSelect == 7) {
      this.duration_val = 1;
    } else if (durationSelect == 30) {
      this.duration_val = 2;
    }
  }

  containerPanelSelection() {
    if (this.patientProgramStaus == 'Critical') {
      this.containerSelection = 2;
    } else if (this.patientProgramStaus == 'Cautious') {
      this.containerSelection = 3;
    } else if (this.patientProgramStaus == 'Normal') {
      this.containerSelection = 4;
    } else {
      this.containerSelection = 1;
    }
  }
  dropDownSelect() {
    this.homePageoption == 'patientPage';
    this.patientProgramStaus = undefined;
    this.containerSelection = 1;
    this.clinicorTeam = undefined;
    this.maskingDataSrc();
  }

  durationSelectionValue: any;
  getDurationPatientList(duration: any) {
    this.reset = true;
    if (duration == 'today') {
      this.duration_val = 0;
      this.durationSelectionValue = 0;
      this.maskingDataSrc();
    } else if (duration == '7day') {
      this.duration_val = 1;
      this.durationSelectionValue = 7;
      this.maskingDataSrc();
    } else if (duration == '30day') {
      this.duration_val = 2;
      this.durationSelectionValue = 30;
      this.maskingDataSrc();
    }
  }

  CalCulateCriticalCautiousCount(dataSrc: any) {
    if (
      this.patientVitalSelect == 'All Vitals' ||
      this.patientVitalSelect == 'undefined'
    ) {
      this.patientVitalSelect = undefined;
    }

    if (
      this.patientVitalSelect != undefined &&
      this.patientTypeSelect != undefined
    ) {
      dataSrc = dataSrc.filter((data: any) => {
        return (
          data.PatientType == this.patientTypeSelect &&
          data.Vital == this.patientVitalSelect
        );
      });
    } else if (
      this.patientVitalSelect != undefined &&
      this.patientTypeSelect == undefined
    ) {
      dataSrc = dataSrc.filter((data: any) => {
        return data.Vital == this.patientVitalSelect;
      });
    } else if (
      this.patientVitalSelect == undefined &&
      this.patientTypeSelect != undefined
    ) {
      dataSrc = dataSrc.filter((data: any) => {
        return data.PatientType == this.patientTypeSelect;
      });
    } else if (
      this.patientVitalSelect == undefined &&
      this.patientTypeSelect == undefined
    ) {
      dataSrc = dataSrc;
    }

    this.critical_patient_data_array = dataSrc.filter((data: any) => {
      return data.Priority == 'Critical';
    });
    this.cautious_patient_data_array = dataSrc.filter((data: any) => {
      return data.Priority == 'Cautious';
    });
    this.normal_patient_data_array = dataSrc.filter((data: any) => {
      return data.Priority == 'Normal';
    });

    this.totaltableList = dataSrc.length;
  }

  searchValueId: any;
  searchValueName: any;
  searchPatientList_Id: any;
  searchPatientList_Name: any;
  searchPatientList_assign: any;
  searchPatientList_physician: any;
  searchPatient_assignMember: any;
  searchProgram_Name: any;
  searchPatientListAssign() {
    this.searchPatientList_assign = !this.searchPatientList_assign;
    this.searchPatientList_physician = false;
    this.searchPatientList_Name = false;
    this.searchPatientList_Id = false;
    this.searchValueName = false;
    this.searchPatient_assignMember = false;
    this.searchProgram_Name = false;
    this.clinicNameSerach = '';
  }
  searchPatientListAssignClose() {
    this.searchPatientList_assign = false;
    this.searchPatientList_physician = false;
    this.searchPatientList_Name = false;
    this.searchPatientList_Id = false;
    this.searchValueName = false;
    this.searchPatient_assignMember = false;
    this.searchProgram_Name = false;
    this.clinicNameSerach = '';
  }
  searchPatientAssignedMember() {
    this.searchPatient_assignMember = !this.searchPatient_assignMember;
    this.searchPatientList_physician = false;
    this.searchPatientList_Name = false;
    this.searchPatientList_Id = false;
    this.searchPatientList_assign = false;
    this.searchProgram_Name = false;
    this.assigneeSerach = '';
  }
  searchPatientAssignedMemberClose() {
    this.searchPatient_assignMember = false;
    this.searchPatientList_physician = false;
    this.searchPatientList_Name = false;
    this.searchPatientList_Id = false;
    this.searchPatientList_assign = false;
    this.searchProgram_Name = false;
    this.assigneeSerach = '';
    this.pageIndxValue = 0;
  }
  searchPatientListPhysician() {
    this.searchPatientList_physician = !this.searchPatientList_physician;
    this.searchPatientList_assign = false;
    this.searchPatientList_Name = false;
    this.searchPatientList_Id = false;
    this.searchPatient_assignMember = false;
    this.searchProgram_Name = false;
    this.phySearchValue = '';
  }
  searchPatientListPhysicianClose() {
    this.searchPatientList_physician = false;
    this.searchPatientList_assign = false;
    this.searchPatientList_Name = false;
    this.searchPatientList_Id = false;
    this.searchPatient_assignMember = false;
    this.searchProgram_Name = false;
    this.phySearchValue = '';
    this.pageIndxValue = 0;
  }
  searchPatientListId() {
    this.searchPatientList_Id = !this.searchPatientList_Id;
    this.searchPatientList_physician = false;
    this.searchPatientList_assign = false;
    this.searchPatientList_Name = false;
  }
  searchPatientListName() {
    this.searchPatientList_Name = !this.searchPatientList_Name;
    this.searchPatientList_assign = false;
    this.searchPatientList_physician = false;
    this.searchPatient_assignMember = false;
    this.searchProgram_Name = false;
    this.patientSearch = '';
  }
  searchPatientListNameClose() {
    this.searchPatientList_Name = false;
    this.searchPatientList_assign = false;
    this.searchPatientList_physician = false;
    this.searchPatient_assignMember = false;
    this.searchProgram_Name = false;
    this.patientSearch = '';
    this.pageIndxValue = 0;
  }
  searchProgramListName() {
    this.searchProgram_Name = ! this.searchProgram_Name;
    this.searchPatientList_assign = false;
    this.searchPatientList_physician = false;
    this.searchPatient_assignMember = false;
    this.searchPatientList_Name = false;
    this.programSearch = '';
  }
  searchProgramListNameClose() {
    this.searchPatientList_Name = false;
    this.searchPatientList_assign = false;
    this.searchPatientList_physician = false;
    this.searchPatient_assignMember = false;
    this.searchProgram_Name = false;
    this.programSearch = '';
    this.pageIndxValue = 0;
  }
  serachIdClick() {
    this.searchValueId = !this.searchValueId;
  }
  searchNameClick() {
    this.searchValueName = !this.searchValueName;
    this.searchPatientList_assign = false;
    this.billingAssigneeName = '';
  }
  tableFilterValue: any;

  applyFilter(event: Event) {
    const filterValue = (event.target as HTMLInputElement).value;
    this.dataSourceTableList.filter = filterValue.trim().toLowerCase();
  }

  setupFilter(column: string) {
    this.dataSourceTableList.filterPredicate = (d: any, filter: string) => {
      const textToSearch = (d[column] && d[column].toLowerCase()) || '';
      return textToSearch.indexOf(filter) !== -1;
    };
  }

  applyDataFilter(filterValue: any) {
    filterValue = (<HTMLInputElement>filterValue.target).value.trim();
    filterValue = filterValue.toLowerCase();
    this.tableFilterValue = filterValue;
    this.dataSourceTableList.filter = filterValue;
  }

  VitalSelectData: any;
  loading3: any;

  patientNavigationStatus: any;
  patient_page_nav: any;
  renewStatus: any;

  ngOnDestroy(): void {
    sessionStorage.removeItem('patientNavigationStatus');
  }

  // Billing Data Code Start
  // ************************************************************************************************************************************************8
  patientTypebilling = 'undefined';

  SelectedPanel: any;

  // Billing Data:
  clickBillingHandler(row: any) {
    let route = '/admin/patients_detail';
    this._router.navigate([route], {
      queryParams: { id: row.PatientId, programId: row.PatientProgramID },
      skipLocationChange: true,
    });
    // this._router.navigate([route], {state: {data: {id: row.PatientId, programId: row.PatientProgramId}}});
    sessionStorage.setItem('SelectedMenu', this.CurrentMenuItem);
    sessionStorage.setItem('patientTypeStatus', this.patientTypeSelect);
    sessionStorage.setItem('vitalkTypeStatus', this.patientVitalSelect);
    sessionStorage.setItem('totalCount', this.totaltableList);
    sessionStorage.setItem(
      'tabledataSource',
      JSON.stringify(this.dataSourceTableList.filteredData)
    );
    sessionStorage.setItem(
      'tableCritical',
      JSON.stringify(this.critical_patient_data_array)
    );
    sessionStorage.setItem(
      'tableCautious',
      JSON.stringify(this.cautious_patient_data_array)
    );
    sessionStorage.setItem(
      'tableNormal',
      JSON.stringify(this.normal_patient_data_array)
    );
  }
  BillingList_columHeader = [
    'PatientNumberid',
    'PatientName',
    'Program',
    'AssignedMemeber',
    'DaysCompleted',
    'TortalVitalCount',
    'interaction',
    'CodeorNextBilling',
    'patientId',
    'PatientProgramId',
  ];

  patientcptSelect: any;
  dropDownBillingSelect(patientPage: any) {}
  billingInfodata: any;
  BillingPgArray: any;

  getBillingDataSource() {
    this.dataSourceTableList = undefined;
    this.patiletListLoading = true;
    var that = this;
    this.loading1 = true;
    that.billingTypevariable = sessionStorage.getItem('billingType');
    this.billingpgm = 'undefined';
    this.patientTypebilling = 'undefined';
    this.totalvitalfiltervalue = 'undefined';
    this.interactionFilterValue = 'undefined';
    this.billingcontainer = 1;
    this.SearchpatientBilling = '';
    this.billingAssigneeName = '';
    this.searchValueName = false;
    // for clinic
    this.searchPatientList_assign = false;
    this.pageIndxValue = 0;

    that.rpm
      .rpm_get(
        '/api/patient/getpatientbillingDataList?patientType=&patientFilter=&patientId&patientName=&program&assignedmember&Index=1&readingFilter&interactionFilter&RoleId=' +
          this.roles[0].Id +
          '&ProgramType=' +
          this.programType
      )
      .then((data2) => {
        this.billingInfodata = data2;

        this.http_billing_data_info =
          this.billingInfodata.PatientBilldataRecords;

        this.dataSourceTableList = new MatTableDataSource(
          this.http_billing_data_info
        );
        this.changeDetectorRef.detectChanges();
        this.loading1 = false;
        this.paginator.firstPage();

        this.patiletListLoading = false;
      });
  }
  pageIndxValue: any;
  getNextBillingDetails(event: PageEvent) {
    if (this.variable == 2) {
      this.pageIndxValue = event.pageIndex;

      var pageIndex = event.pageIndex * 10 + 1;

      var that = this;
      var searchbillingpgm;
      var searchtotalvitalfiltervalue;
      var searchinteractionfiltervalue;
      this.loading1 = true;
      if (this.billingpgm == 'undefined') {
        searchbillingpgm = '';
      } else {
        searchbillingpgm = this.billingpgm;
      }

      if (this.totalvitalfiltervalue == 'undefined') {
        searchtotalvitalfiltervalue = '';
      } else {
        searchtotalvitalfiltervalue = this.totalvitalfiltervalue;
      }

      if (this.interactionFilterValue == 'undefined') {
        searchinteractionfiltervalue = '';
      } else {
        searchinteractionfiltervalue = this.interactionFilterValue;
      }

      var searchPatient;
      if (
        this.SearchpatientBilling == undefined ||
        this.SearchpatientBilling == ''
      ) {
        searchPatient = '';
      } else {
        searchPatient = this.SearchpatientBilling;
      }

      var assigneeNameSearch;
      if (
        this.billingAssigneeName == undefined ||
        this.billingAssigneeName == ''
      ) {
        assigneeNameSearch = '';
      } else {
        assigneeNameSearch = this.billingAssigneeName;
      }

      var patienttype;
      if (this.patientTypebilling == 'undefined') {
        patienttype = '';
      } else {
        patienttype = this.patientTypebilling;
      }

      var CylcleValue;
      if (this.billingTypevariable == 'cycle') {
        if (this.c1 == false && this.c2 == false) {
          CylcleValue = '';
        } else if (this.c1 == true && this.c2 == false) {
          CylcleValue = 'c1';
        } else if (this.c1 == false && this.c2 == true) {
          CylcleValue = 'c2';
        }
      } else {
        if (this.Today == false && this.next7days == false) {
          CylcleValue = '';
        } else if (this.Today == true && this.next7days == false) {
          CylcleValue = 'Today';
        } else if (this.Today == false && this.next7days == true) {
          CylcleValue = 'Next7Days';
        }
      }
      this.loading1 = true;
      this.patiletListLoading = true;
      that.rpm
        .rpm_get(
          '/api/patient/getpatientbillingDataList?patientType=' +
            patienttype +
            '&patientFilter=' +
            CylcleValue +
            '&patientId&patientName=' +
            searchPatient +
            '&program=' +
            searchbillingpgm +
            '&assignedmember=' +
            assigneeNameSearch +
            '&Index=' +
            pageIndex +
            '&readingFilter=' +
            searchtotalvitalfiltervalue +
            '&interactionFilter=' +
            searchinteractionfiltervalue +
            '&RoleId=' +
            this.roles[0].Id +
            '&ProgramType=' +
            this.programType
        )
        .then((data2) => {
          this.loading1 = false;
          this.patiletListLoading = false;
          this.billingInfodata = data2;

          this.http_billing_data_info =
            this.billingInfodata.PatientBilldataRecords;
          this.BillingPgArray = Array.from(
            new Set(
              this.http_billing_data_info.map(
                (a: { Program: any }) => a.Program
              )
            )
          ).map((Program) => {
            return this.http_billing_data_info.find(
              (a: { Program: any }) => a.Program == Program
            );
          });
          this.dataSourceTableList = new MatTableDataSource(
            this.http_billing_data_info
          );
          this.changeDetectorRef.detectChanges();
          // this.dataSourceTableList.paginator = this.paginator;
          // this.dataSourceTableList.sort = this.sort;
        });
    } else {
      // if (event.pageIndex > event.previousPageIndex!) {
      this.pageIndxValue = event.pageIndex;
      this.pageSize = event.pageSize;
      // }
    }
  }

  c1 = false;
  c2 = false;

  Today = false;
  next7days = false;
  billlingContainerClick(containerSelect: any) {
    // if (this.billingTypevariable == 'cycle') {
    //   this.c1 = true;
    //   this.c2 = false;
    // } else if (this.billingTypevariable == '30days') {
    //   this.Today = true;
    //   this.next7days = false;
    // }

    this.billingDataFilter();
    this.billingcontainer = containerSelect;
  }
  cycleOneClick() {
    if (this.billingTypevariable == 'cycle') {
      this.c1 = true;
      this.c2 = false;
    } else if (this.billingTypevariable == '30days') {
      this.Today = true;
      this.next7days = false;
    }

    this.billingDataFilter();
    this.billingcontainer = 2;
  }
  cycleTwoClick() {
    if (this.billingTypevariable == 'cycle') {
      this.c2 = true;
      this.c1 = false;
    } else if (this.billingTypevariable == '30days') {
      this.Today = false;
      this.next7days = true;
    }

    this.billingDataFilter();
    this.billingcontainer = 3;
  }

  onClickTotal() {
    this.billingcontainer = 1;
    this.c2 = false;
    this.c1 = false;
    this.Today = false;
    this.next7days = false;
    this.billingDataFilter();
  }
  billingpgm = 'undefined';
  totalvitalfiltervalue = 'undefined';
  interactionFilterValue = 'undefined';

  SearchpatientBilling = '';
  billingAssigneeName = '';

  searchBillingAssignMember() {
    this.SearchpatientBilling = '';
    this.searchPatientList_assign = true;
    this.searchValueName = false;
    this.assigneeSerach = '';

  }
  refreshBillingData() {
    this.c1 = false;
    this.c2 = false;
    this.getBillingDataSource();
  }
  billing_tableCount: any;
  billingDataFilter() {
    var that = this;
    var searchbillingpgm;
    var searchtotalvitalfiltervalue;
    var searchinteractionfiltervalue;
    this.loading1 = true;
    if (this.billingpgm == 'undefined') {
      searchbillingpgm = '';
    } else {
      searchbillingpgm = this.billingpgm;
    }

    if (this.totalvitalfiltervalue == 'undefined') {
      searchtotalvitalfiltervalue = '';
    } else {
      searchtotalvitalfiltervalue = this.totalvitalfiltervalue;
    }

    if (this.interactionFilterValue == 'undefined') {
      searchinteractionfiltervalue = '';
    } else {
      searchinteractionfiltervalue = this.interactionFilterValue;
    }

    var searchPatient;
    if (
      this.SearchpatientBilling == undefined ||
      this.SearchpatientBilling == ''
    ) {
      searchPatient = '';
    } else {
      searchPatient = this.SearchpatientBilling;
    }

    var assigneeNameSearch;
    if (
      this.billingAssigneeName == undefined ||
      this.billingAssigneeName == ''
    ) {
      assigneeNameSearch = '';
    } else {
      assigneeNameSearch = this.billingAssigneeName;
    }

    var patienttype;
    if (this.patientTypebilling == 'undefined') {
      patienttype = '';
    } else {
      patienttype = this.patientTypebilling;
    }

    var CylcleValue;
    if (this.billingTypevariable == 'cycle') {
      if (this.c1 == false && this.c2 == false) {
        CylcleValue = '';
      } else if (this.c1 == true && this.c2 == false) {
        CylcleValue = 'c1';
      } else if (this.c1 == false && this.c2 == true) {
        CylcleValue = 'c2';
      }
    } else {
      if (this.Today == false && this.next7days == false) {
        CylcleValue = '';
      } else if (this.Today == true && this.next7days == false) {
        CylcleValue = 'Today';
      } else if (this.Today == false && this.next7days == true) {
        CylcleValue = 'Next7Days';
      }
    }
    this.loading1 = true;
    this.patiletListLoading = true;
    var billingpageIndxValue;
    if (this.patientNavigationStatus == 'true') {
      billingpageIndxValue = this.pageIndxValue * 10 + 1;
    } else {
      billingpageIndxValue = 1;
      this.pageIndxValue = 0;
    }

    that.rpm
      .rpm_get(
        '/api/patient/getpatientbillingDataList?patientType=' +
          patienttype +
          '&patientFilter=' +
          CylcleValue +
          '&patientId&patientName=' +
          searchPatient +
          '&program=' +
          searchbillingpgm +
          '&assignedmember=' +
          assigneeNameSearch +
          '&Index=' +
          billingpageIndxValue +
          '&readingFilter=' +
          searchtotalvitalfiltervalue +
          '&interactionFilter=' +
          searchinteractionfiltervalue +
          '&RoleId=' +
          this.roles[0].Id +
          '&ProgramType=' +
          this.programType
      )
      .then((data2) => {
        this.loading1 = false;
        this.patiletListLoading = false;
        this.billingInfodata = data2;
        this.billing_tableCount = this.billingInfodata.TotalCounts.Total;
        this.http_billing_data_info =
          this.billingInfodata.PatientBilldataRecords;
        that.dataSourceTableList = new MatTableDataSource(
          this.http_billing_data_info
        );

        // Navigation Change
        this.changeDetectorRef.detectChanges();

        // this.dataSourceTableList.paginator = this.paginator;
        // this.paginator.firstPage();
        sessionStorage.removeItem('patientProgram')!;
        sessionStorage.removeItem('CycleOne')!;
        sessionStorage.removeItem('CycleTwo')!;
        sessionStorage.removeItem('Today')!;
        sessionStorage.removeItem('next7Day')!;
        sessionStorage.removeItem('billingContainer')!;
        sessionStorage.removeItem('billingProgram')!;
        sessionStorage.removeItem('totalVitalValue')!;
        sessionStorage.removeItem('interactionValue')!;
        sessionStorage.removeItem('pageIndexbilling')!;
        sessionStorage.removeItem('patientNavigationStatus');
        sessionStorage.removeItem('searchBillingPatients');
        sessionStorage.removeItem('BillingAssigneeName');
      });
  }

  searchNameClose() {
    this.searchValueName = !this.searchValueName;
    this.SearchpatientBilling = '';
    this.billingDataFilter();
    this.pageIndxValue = 0;
  }

  searchAssigneeClose() {
    this.searchPatientList_assign = !this.searchPatientList_assign;
    this.billingAssigneeName = '';
    this.billingDataFilter();
    this.pageIndxValue = 0;
  }
  ConvertMinute(timeValue: any) {
    var ConvertData = Math.floor(timeValue / 60);
    var convertSec = timeValue % 60;

    var timeData = ConvertData + ':' + convertSec;
    return timeData;
  }

  navigatePatientDetail(row: any) {
    let route = '/admin/patients_detail';
    this._router.navigate([route], {
      queryParams: { id: row.patientId, programId: row.PatientProgramId },
      skipLocationChange: true,
    });
    // Navigation  change
    sessionStorage.setItem('patientTypeBilling', this.patientTypebilling);
    sessionStorage.setItem('billingTypeVariable', this.billingTypevariable);
    sessionStorage.setItem('patientProgram', this.programType);
    sessionStorage.setItem('SelectedMenu', this.CurrentMenuItem);
    sessionStorage.setItem('CycleOne', JSON.stringify(this.c1));
    sessionStorage.setItem('CycleTwo', JSON.stringify(this.c2));
    sessionStorage.setItem('Today', JSON.stringify(this.Today));
    sessionStorage.setItem('next7Day', JSON.stringify(this.next7days));
    sessionStorage.setItem(
      'billingContainer',
      JSON.stringify(this.billingcontainer)
    );

    sessionStorage.setItem('billingProgram', this.billingpgm);
    sessionStorage.setItem('totalVitalValue', this.totalvitalfiltervalue);
    sessionStorage.setItem('interactionValue', this.interactionFilterValue);
    sessionStorage.setItem('pageIndexbilling', this.pageIndxValue);
    sessionStorage.setItem('searchBillingPatients', this.SearchpatientBilling);
    sessionStorage.setItem('BillingAssigneeName', this.billingAssigneeName);

    sessionStorage.setItem('patient-page-status', 'patient-page');
  }

  // DownLoad Csv
  downloadCsv() {
    if (this.dataSourceTableList.filteredData.length > 0) {
      let csvData = this.ConvertToCSV(this.dataSourceTableList.filteredData, [
        'PatientNumber',
        'PatientName',
        'PatientType',
        'AssignedMember',
        'ClinicName',
        'ProgramName',
        'EnrolledDate',
        'PhysicianName',
      ]);
      const a = document.createElement('a');
      let filename = 'patientList';
      a.href = 'data:text/csv,' + csvData;
      a.setAttribute('download', filename + '.csv');
      document.body.appendChild(a);
      a.click();
      document.body.removeChild(a);
    } else {
      alert('No Data Found');
    }
  }
  ConvertToCSV(objArray: any, headerList: any) {
    let str = '';
    let row = 'No,';
    for (let index in headerList) {
      row += headerList[index] + ',';
    }
    row = row.slice(0, -1);
    str += row + '\r\n';
    for (let i = 0; i < objArray.length; i++) {
      let line = i + 1 + '';
      for (let index in headerList) {
        let head = headerList[index];
        line += ',"' + objArray[i][head] + '"';
      }
      str += line + '\r\n';
    }
    return str;
  }

  //Chart Data Start
  // ****************************************************************************************************//

  public barChartOptions: ChartOptions = {
    responsive: true,
    plugins: {
      legend: {
        display: true,                  // Show the legend
        position: 'bottom',             // Position of the legend (top, bottom, left, right)
        labels: {
          boxWidth: 10,                 // Width of the color box in the legend
          padding: 15,                  // Padding between the legend items
        },
      },
    },

    scales: {
      x: {
          grid: {
            color: 'rgba(0, 0, 0, 0)',
          },
        },
      },
  };
  public barChartLabels: any;
  public barChartType: ChartType = 'bar';
  public barChartLegend = true;
  public barChartPlugins = [];
  public barChartData: ChartDataset[];
  graph_dataSource: any;
  graph_model: any;
  vital: any;
  patient_type: any;
  year_data: any = [];
  month_data = [
    'Jan',
    'Feb',
    'Mar',
    'Apr',
    'May',
    'Jun',
    'Jul',
    'Aug',
    'Sep',
    'Oct',
    'Nov',
    'Dec',
  ];
  year_data_2: any = [];
  month_data_2 = [
    'Jan',
    'Feb',
    'Mar',
    'Apr',
    'May',
    'Jun',
    'Jul',
    'Aug',
    'Sep',
    'Oct',
    'Nov',
    'Dec',
  ];

  filter_objects_vital(obj: any, filter_data: any) {
    var marvelHeroes = obj.filter(function (hero: { vital: any }) {
      return hero.vital == filter_data;
    });

    return marvelHeroes;
  }
  filter_objects_patient(obj: any, filter_data: any) {
    var marvelHeroes = obj.filter(function (hero: { patients_type: any }) {
      return hero.patients_type == filter_data;
    });

    return marvelHeroes;
  }

  prepare_data(graph_dataSource: any) {
    this.vital = 'All Vitals';
    this.patient_type = 'all';
    this.barChartLabels = graph_dataSource.barChartLabels;
    this.graph_model = this.filter_objects_vital(
      graph_dataSource.data,
      'All Vitals'
    )[0];
    this.barChartData = [
      {
        data: this.filter_objects_patient(
          this.graph_model.data,
          'Prescribed Patients'
        )[0].data,
        label: 'Prescribed Patients',
        barPercentage: 0.5,
        hoverBackgroundColor: '#FFBF77',
        backgroundColor: '#FFBF77',
      },
      {
        data: this.filter_objects_patient(
          this.graph_model.data,
          'Enrolled Patients'
        )[0].data,
        label: 'Enrolled Patients',
        barPercentage: 0.5,
        hoverBackgroundColor: '#8BE3D9',
        backgroundColor: '#8BE3D9',
      },
      {
        data: this.filter_objects_patient(
          this.graph_model.data,
          'Active Patients'
        )[0].data,
        label: 'Active Patients',
        barPercentage: 1,
        hoverBackgroundColor: '#6DA86F',
        backgroundColor: '#6DA86F',
      },
      {
        data: this.filter_objects_patient(
          this.graph_model.data,
          'Ready To Discharge'
        )[0].data,
        label: 'Ready To Discharge',
        barPercentage: 0.5,
        hoverBackgroundColor: '#164D61',
        backgroundColor: '#164D61',
      },
      {
        data: this.filter_objects_patient(
          this.graph_model.data,
          'Discharged'
        )[0].data,
        label: 'Discharged',
        barPercentage: 0.5,
        hoverBackgroundColor: '#8226DF',
        backgroundColor: '#8226DF',
      },
      {
        data: this.filter_objects_patient(
          this.graph_model.data,
          'Onhold Patients'
        )[0].data,
        label: 'Onhold Patients',
        barPercentage: 0.5,
        hoverBackgroundColor: '#C1C7D5',
        backgroundColor: '#C1C7D5',
      },
    ];
  }

  get_data(choice: any) {
    switch (choice) {
      case 1:
        this.trends_frequency = 1;
        var that = this;
        that.http
          .HTTP_GET('/api/patient_trends_status?time=month')
          .then((data) => {
            that.graph_dataSource = data;
            that.prepare_data(that.graph_dataSource);
          });
        break;
      case 3:
        this.trends_frequency = 3;
        var that = this;
        that.http
          .HTTP_GET('/api/patient_trends_status?time=year')
          .then((data) => {
            that.graph_dataSource = data;
            that.prepare_data(that.graph_dataSource);
          });
        break;
      default:
        this.trends_frequency = 2;
        var that = this;
        that.http
          .HTTP_GET('/api/patient_trends_status?time=all')
          .then((data) => {
            that.graph_dataSource = data;
            that.prepare_data(that.graph_dataSource);
          });
        break;
    }
  }
  decrement() {
    if ((this.trends_frequency = 2)) {
      let index = this.year_data.indexOf(this.year_selected);
      if (index > 0) {
        this.year_selected = this.year_selected - 1;
        this.get_data_increment_decrement(this.year_selected);
      }
    }
  }
  increment() {
    if ((this.trends_frequency = 2)) {
      let index = this.year_data.indexOf(this.year_selected);
      if (index < this.year_data.length - 1) {
        this.year_selected = this.year_selected + 1;
        this.get_data_increment_decrement(this.year_selected);
      }
    }
  }
  get_data_increment_decrement(freq: any) {
    switch (this.trends_frequency) {
      case 1:
        var that = this;
        that.http
          .HTTP_GET('/api/patient_trends_status?time=month:' + freq)
          .then((data) => {
            that.graph_dataSource = data;
            that.prepare_data(that.graph_dataSource);
          });
        break;
      case 3:
        var that = this;
        that.http
          .HTTP_GET('/api/patient_trends_status?time=all')
          .then((data) => {
            that.graph_dataSource = data;
            that.prepare_data(that.graph_dataSource);
          });
        break;
      default:
        var that = this;
        that.http
          .HTTP_GET('/api/patient_trends_status?time=year:' + freq)
          .then((data) => {
            that.graph_dataSource = data;
            that.prepare_data(that.graph_dataSource);
          });
        break;
    }
  }
  filter_data() {
    switch (this.vital) {
      case 'Blood Sugar':
        this.graph_model = this.filter_objects_vital(
          this.graph_dataSource.data,
          'Blood Sugar'
        )[0];
        break;
      case 'Blood Pressure':
        this.graph_model = this.filter_objects_vital(
          this.graph_dataSource.data,
          'Blood Pressure'
        )[0];
        break;
      case 'Weight':
        this.graph_model = this.filter_objects_vital(
          this.graph_dataSource.data,
          'Weight'
        )[0];
        break;
      case 'Pulse Oximeter':
        this.graph_model = this.filter_objects_vital(
          this.graph_dataSource.data,
          'Pulse Oximeter'
        )[0];
        break;
      default:
        this.graph_model = this.filter_objects_vital(
          this.graph_dataSource.data,
          'All Vitals'
        )[0];

        break;
    }

    switch (this.patient_type) {
      case 'Prescribed Patients':
        this.barChartData = [
          {
            data: this.filter_objects_patient(
              this.graph_model.data,
              'Prescribed Patients'
            )[0].data,
            label: 'Prescribed Patients',
            barPercentage: 0.5,
            hoverBackgroundColor: '#FFBF77',
            backgroundColor: '#FFBF77',
          },
        ];

        break;
      case 'Enrolled Patients':
        this.barChartData = [
          {
            data: this.filter_objects_patient(
              this.graph_model.data,
              'Enrolled Patients'
            )[0].data,
            label: 'Enrolled Patients',
            barPercentage: 0.5,
            hoverBackgroundColor: '#8BE3D9',
            backgroundColor: '#8BE3D9',
          },
        ];
        break;
      case 'Active Patients':
        this.barChartData = [
          {
            data: this.filter_objects_patient(
              this.graph_model.data,
              'Active Patients'
            )[0].data,
            label: 'Active Patients',
            barPercentage: 1,
            hoverBackgroundColor: '#6DA86F',
            backgroundColor: '#6DA86F',
          },
        ];
        break;
      case 'Ready To Discharge':
        this.barChartData = [
          {
            data: this.filter_objects_patient(
              this.graph_model.data,
              'Ready To Discharge'
            )[0].data,
            label: 'Ready To Discharge',
            barPercentage: 1,
            hoverBackgroundColor: '#6DA86F',
            backgroundColor: '#6DA86F',
          },
        ];
        break;
      case 'Discharged':
        this.barChartData = [
          {
            data: this.filter_objects_patient(
              this.graph_model.data,
              'Discharged'
            )[0].data,
            label: 'Discharged',
            barPercentage: 1,
            hoverBackgroundColor: '#6DA86F',
            backgroundColor: '#6DA86F',
          },
        ];
        break;
      case 'Onhold Patients':
        this.barChartData = [
          {
            data: this.filter_objects_patient(
              this.graph_model.data,
              'Onhold Patients'
            )[0].data,
            label: 'Onhold Patients',
            barPercentage: 1,
            hoverBackgroundColor: '#6DA86F',
            backgroundColor: '#6DA86F',
          },
        ];
        break;
      default:
        this.barChartData = [
          {
            data: this.filter_objects_patient(
              this.graph_model.data,
              'Prescribed Patients'
            )[0].data,
            label: 'Prescribed Patients',
            barPercentage: 0.5,
            hoverBackgroundColor: '#FFBF77',
            backgroundColor: '#FFBF77',
          },
          {
            data: this.filter_objects_patient(
              this.graph_model.data,
              'Enrolled Patients'
            )[0].data,
            label: 'Enrolled Patients',
            barPercentage: 0.5,
            hoverBackgroundColor: '#8BE3D9',
            backgroundColor: '#8BE3D9',
          },
          {
            data: this.filter_objects_patient(
              this.graph_model.data,
              'Active Patients'
            )[0].data,
            label: 'Active Patients',
            barPercentage: 1,
            hoverBackgroundColor: '#6DA86F',
            backgroundColor: '#6DA86F',
          },
          {
            data: this.filter_objects_patient(
              this.graph_model.data,
              'Ready To Discharge'
            )[0].data,
            label: 'Ready To Discharge',
            barPercentage: 0.5,
            hoverBackgroundColor: '#164D61',
            backgroundColor: '#164D61',
          },
          {
            data: this.filter_objects_patient(
              this.graph_model.data,
              'Discharged'
            )[0].data,
            label: 'Discharged',
            barPercentage: 0.5,
            hoverBackgroundColor: '#8226DF',
            backgroundColor: '#8226DF',
          },
          {
            data: this.filter_objects_patient(
              this.graph_model.data,
              'Onhold Patients'
            )[0].data,
            label: 'Onhold Patients',
            barPercentage: 0.5,
            hoverBackgroundColor: '#C1C7D5',
            backgroundColor: '#C1C7D5',
          },
        ];

        break;
    }
  }

  public barChartLabels_2: any;
  public barChartData_2: ChartDataset[];

  prepare_data_2(graph_dataSource: any) {
    this.cpt_code = 'all';
    this.barChartLabels_2 = graph_dataSource.barChartLabels;
    this.graph_model_2 = this.filter_objects_cpt(
      graph_dataSource.data,
      'all'
    )[0];
    this.barChartData_2 = [
      {
        data: this.graph_model_2.billing_compliance,
        label: 'Billing Compliance',
        barPercentage: 0.5,
        hoverBackgroundColor: '#6DA86F',
        backgroundColor: '#6DA86F',
      },
      {
        data: this.graph_model_2.missing_information,
        label: 'Missing Information',
        barPercentage: 0.5,
        hoverBackgroundColor: '#FD5276',
        backgroundColor: '#FD5276',
      },
      {
        data: this.graph_model_2.onhold_patients,
        label: 'Onhold Patients',
        barPercentage: 0.5,
        hoverBackgroundColor: '#C1C7D5',
        backgroundColor: '#C1C7D5',
      },
    ];
  }
  filter_objects_cpt(obj: any, filter_data: any) {
    var marvelHeroes = obj.filter(function (hero: { cpt_code: any }) {
      return hero.cpt_code == filter_data;
    });

    return marvelHeroes;
  }

  get_data_2(choice: any) {
    switch (choice) {
      case 1:
        this.trends_frequency_2 = 1;
        var that = this;
        that.http
          .HTTP_GET('/api/patient_trends_billing_adherance?time=month')
          .then((data) => {
            that.graph_dataSource_2 = data;
            that.prepare_data_2(that.graph_dataSource_2);
          });
        break;
      case 3:
        this.trends_frequency_2 = 3;
        var that = this;
        that.http
          .HTTP_GET('/api/patient_trends_billing_adherance?time=year')
          .then((data) => {
            that.graph_dataSource_2 = data;
            that.prepare_data_2(that.graph_dataSource_2);
          });
        break;
      default:
        this.trends_frequency_2 = 2;
        var that = this;
        that.http
          .HTTP_GET('/api/patient_trends_billing_adherance?time=all')
          .then((data) => {
            that.graph_dataSource_2 = data;
            that.prepare_data_2(that.graph_dataSource_2);
          });
        break;
    }
  }
  decrement_2() {
    if ((this.trends_frequency_2 = 2)) {
      let index = this.year_data_2.indexOf(this.year_selected_2);
      if (index > 0) {
        this.year_selected_2 = this.year_selected_2 - 1;
        this.get_data_increment_decrement_2(this.year_selected_2);
      }
    }
  }
  increment_2() {
    if ((this.trends_frequency_2 = 2)) {
      let index = this.year_data_2.indexOf(this.year_selected_2);
      if (index < this.year_data_2.length - 1) {
        this.year_selected_2 = this.year_selected_2 + 1;
        this.get_data_increment_decrement_2(this.year_selected_2);
      }
    }
  }
  get_data_increment_decrement_2(freq: any) {
    switch (this.trends_frequency_2) {
      case 1:
        var that = this;
        that.http
          .HTTP_GET('/api/patient_trends_billing_adherance?time=month:' + freq)
          .then((data) => {
            that.graph_dataSource_2 = data;
            that.prepare_data_2(that.graph_dataSource_2);
          });
        break;
      case 3:
        var that = this;
        that.http
          .HTTP_GET('/api/patient_trends_billing_adherance?time=all')
          .then((data) => {
            that.graph_dataSource_2 = data;
            that.prepare_data_2(that.graph_dataSource_2);
          });
        break;
      default:
        var that = this;
        that.http
          .HTTP_GET('/api/patient_trends_billing_adherance?time=year:' + freq)
          .then((data) => {
            that.graph_dataSource_2 = data;
            that.prepare_data_2(that.graph_dataSource_2);
          });
        break;
    }
  }

  filter_data_2() {
    switch (this.cpt_code) {
      case '99458':
        this.graph_model_2 = this.filter_objects_cpt(
          this.graph_dataSource_2.data,
          '99458'
        )[0];
        break;
      case '99453':
        this.graph_model_2 = this.filter_objects_cpt(
          this.graph_dataSource_2.data,
          '99453'
        )[0];
        break;
      case '99454':
        this.graph_model_2 = this.filter_objects_cpt(
          this.graph_dataSource_2.data,
          '99454'
        )[0];
        break;
      case '99457':
        this.graph_model_2 = this.filter_objects_cpt(
          this.graph_dataSource_2.data,
          '99457'
        )[0];
        break;
      default:
        this.graph_model_2 = this.filter_objects_cpt(
          this.graph_dataSource_2.data,
          'all'
        )[0];
        break;
    }

    this.barChartData_2 = [
      {
        data: this.graph_model_2.billing_compliance,
        label: 'Billing Compliance',
        barPercentage: 0.5,
        hoverBackgroundColor: '#6DA86F',
        backgroundColor: '#6DA86F',
      },
      {
        data: this.graph_model_2.missing_information,
        label: 'Missing Information',
        barPercentage: 0.5,
        hoverBackgroundColor: '#FD5276',
        backgroundColor: '#FD5276',
      },
      {
        data: this.graph_model_2.onhold_patients,
        label: 'Onhold Patients',
        barPercentage: 0.5,
        hoverBackgroundColor: '#C1C7D5',
        backgroundColor: '#C1C7D5',
      },
    ];
  }

  // Timezone Offset Function Start
  getTimeZoneOffsetData() {
    console.log(Intl.DateTimeFormat().resolvedOptions().timeZone);
    let offsetValue = new Date().getTimezoneOffset();
    var offset;
    offset = offsetValue <= 0 ? '+' : '-';
    var dateOffsetToSent = offset + Math.abs(offsetValue);

    return dateOffsetToSent;
  }

  //billing change

  programType = 'RPM';
  programChange(event: any) {
    // this.programType=event;
    this.getBillingDataSource();
    this.filterBillingData();
  }
  billingTypeProgramArray: any;
  billingProgramNameTempArray: any;
  billingProgramNameArray: any;
  getBillingTypeProgram() {
    var that = this;
    that.rpm.rpm_get('/api/patient/getAllprograms').then((data2) => {
      this.billingTypeProgramArray = data2;
      this.billingProgramNameTempArray = data2;
      this.filterBillingData();

      this.billingTypeProgramArray = this.removeDuplicateObjects(
        this.billingTypeProgramArray,
        'ProgramType'
      );
    });
  }

  filterBillingData() {
    this.billingProgramNameArray = this.billingProgramNameTempArray.filter(
      (data: any) => {
        return data.ProgramType == this.programType;
      }
    );
  }
  removeDuplicateObjects(array: any, property: any) {
    const uniqueIds: any[] = [];

    const unique = array.filter((element: { [x: string]: any }) => {
      const isDuplicate = uniqueIds.includes(element[property]);

      if (!isDuplicate) {
        uniqueIds.push(element[property]);

        return true;
      }

      return false;
    });

    return unique;
  }
  phySearchValue: any;
  patientSearch: any;
  clinicNameSerach: any;
  assigneeSerach: any;
  programSearch:any;
  redirectionFilter(value: any) {
    if (value == 'undefined') {
      value = '';
    }
    if (this.tableFilterValue != 'undefined') {
      this.dataSourceTableList.filter = value.trim().toLowerCase();
      if (this.searchPatientList_physician == true) {
        this.setupFilter('PhysicianName');
        this.phySearchValue = value;
      } else if (this.searchPatientList_Name == true) {
        this.setupFilter('PatientName');
        this.patientSearch = value;
      } else if (this.searchPatientList_assign == true) {
        this.setupFilter('ClinicName');
        this.clinicNameSerach = value;
      } else if (this.searchPatient_assignMember == true) {
        this.setupFilter('AssignedMember');
        this.assigneeSerach = value;
      }else if(this.searchProgram_Name = false)
      {
         this.setupFilter('ProgramName');
         this.programSearch = value;
      }
    } else {
      this.phySearchValue = value;
      this.patientSearch = value;
      this.clinicNameSerach = value;
      this.assigneeSerach = value;
      this.programSearch = value;
      return;
    }
  }
  pageSize: any;
  pageChange(pageEvent: PageEvent) {
    // if (pageEvent.pageIndex > pageEvent.previousPageIndex!)
    this.pageSize = pageEvent.pageSize;
    this.pageIndxValue = pageEvent.pageIndex;
    console.log(this.pageIndxValue);
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
}
