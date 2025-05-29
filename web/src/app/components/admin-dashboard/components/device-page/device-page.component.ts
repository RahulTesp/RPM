import {
  Component,
  OnInit,
  ViewChild,
  ChangeDetectorRef,
  AfterViewInit,
  TemplateRef,
} from '@angular/core';
import { HttpService } from '../../sevices/http.service';
// import { ChartOptions, ChartType, ChartDataset } from 'chart.js';
// import { Label } from 'ng2-charts';
import { MatTableDataSource } from '@angular/material/table';
import { MatPaginator } from '@angular/material/paginator';
import { Router, ActivatedRoute } from '@angular/router';
import { RPMService } from '../../sevices/rpm.service';
import { SelectionModel } from '@angular/cdk/collections';
import { MatSort, Sort } from '@angular/material/sort';
import { DatePipe } from '@angular/common';
import { RightSidebarComponent } from './../../shared/right-sidebar/right-sidebar.component';
import { MatDialog } from '@angular/material/dialog';

export interface devicelist {
  device_id: string;
  device: string;
  device_type: string;
  patient_name: string;
  active_date: string;
  current_status: string;
}
const deviceList: devicelist[] = [
  {
    device_id: 'BP-056497',
    device: 'Blood Pressure Monitor',
    device_type: 'LTE',
    patient_name: 'Thejasree.M',
    active_date: '21/10/2022',
    current_status: 'Active',
  },
  {
    device_id: 'BP-056498',
    device: 'Blood Pressure Monitor',
    device_type: 'LTE',
    patient_name: 'Thejasree.M',
    active_date: '21/10/2022',
    current_status: 'Error',
  },
  {
    device_id: 'BG-056498',
    device: 'Blood Glucose Monitor',
    device_type: 'LTE',
    patient_name: 'Dayana',
    active_date: '21/10/2022',
    current_status: 'Error',
  },
];

@Component({
  selector: 'app-device-page',
  templateUrl: './device-page.component.html',
  styleUrls: ['./device-page.component.scss'],
})
export class DevicePageComponent implements OnInit {
  // variable: any;
  variable = 1;
  // Table DataSource
  //deviceListSource: any;

  deviceListSource: any;

  @ViewChild(MatPaginator) paginator: MatPaginator;
  @ViewChild(MatSort) sort = new MatSort();
  @ViewChild(RightSidebarComponent) private rightsidebar: RightSidebarComponent;

  loading: boolean;
  loading1: boolean;

  // DropDown List
  patientListArray: any;
  patientVitalsArray: any;
  patientactiveArray: any;
  // Select DropDown
  deviceSelect: any;
  patientVitalSelect: any;
  patienttypeDataArray: any;
  patientVitalDataArray: any;
  containerSelection = 0;

  device_variable_update: any;
  vendor_variable_update: any;
  task_variable_update: any;
  rolelist: any;
  roles: any;
  deviceNumber: any;
  public deviceavailabledialog = false;

  constructor(
    private rpm: RPMService,
    private http: HttpService,
    private _router: Router,
    public datepipe: DatePipe,
    private changeDetectorRef: ChangeDetectorRef,
    private _route: ActivatedRoute,
    public dialog: MatDialog
  ) {
    var that = this;
    this.rolelist = sessionStorage.getItem('Roles');
    this.roles = JSON.parse(this.rolelist);

    this.getdeviceList();
  }
  DeviceList_columnHeader = [
    'DeviceID',
    'DeviceNumber',
    'Device',
    'DeviceType',
    'PatientName',
    'ActiveDate',
    'DeviceStatus',
  ];
  ngOnInit(): void {}
  backtohome() {
    let route = '/admin/home';
    this._router.navigate([route]);
  }

  add_vendor() {
    //this.vendor_variable_update = false;

    this.rightsidebar.addMenuChoice(11);

    this.task_variable_update = true;
  }
  add_device() {
    //this.device_variable_update = false;

    this.rightsidebar.addMenuChoice(12);
    this.task_variable_update = true;
  }

  activeDeviceCountArray: any;
  availableDeviceCountArray: any;
  errorDeviceCountArray: any;
  usedDeviceCountArray: any;
  tempDeviceList: any;
  deviceTypeSelect: any;
  @ViewChild('DeviceAvailable', { static: true })
  dialogDeviceAvailable: TemplateRef<any>;
  patiantName: any;
  getdeviceList() {
    var that = this;
    this.loading1 = true;
    this.containerSelection = 0;
    this.deviceSelect = undefined;
    this.deviceTypeSelect = undefined;
    this.filterDeviceStatus = 'undefined';
    this.searchDeviceNumber = false;
    this.PatientNameSearch = false;

    that.rpm.rpm_get('/api/devices/getalldevices').then((data2) => {
      this.loading1 = false;
      const transformedData = (data2 as any[]).map((device: any) => {
        const rawDate = device.DeviceActivatedDateTime;
        if (rawDate) {
          const [datePart, timePart] = rawDate.split(' ');
          const [day, month, year] = datePart.split('-');
          const isoDateStr = `${year}-${month}-${day}T${timePart || '00:00:00'}`;
          device.DeviceActivatedDateTime = new Date(isoDateStr);
        }
        return device;
      });      
    
      this.tempDeviceList = transformedData;
      this.deviceListSource = new MatTableDataSource(transformedData);

      this.activeDeviceCountArray = this.tempDeviceList.filter(
        (c: { DeviceStatus: any }) => c.DeviceStatus == 'Active'
      );

      this.availableDeviceCountArray = this.tempDeviceList.filter(
        (c: { DeviceStatus: any }) => c.DeviceStatus == 'Available'
      );

      this.errorDeviceCountArray = this.tempDeviceList.filter(
        (c: { DeviceStatus: any }) => c.DeviceStatus == 'Error'
      );
      this.usedDeviceCountArray = this.tempDeviceList.filter(
        (c: { DeviceStatus: any }) => c.DeviceStatus == 'Used'
      );

      this.deviceListSource = new MatTableDataSource(this.deviceListSource);
      this.changeDetectorRef.detectChanges();
      this.deviceListSource.paginator = this.paginator;
      this.paginator.firstPage();
    });
    this.getDeviceArrayList();
  }
  DeviceListArray: any;
  DeviceMasterdataArray: any;
  DeviceListarray: any;
  DeviceTypeList: any;
  VendorList: any;
  DeviceMakeArray: any;
  getDeviceArrayList() {
    var that = this;
    this.loading1 = true;
    that.rpm.rpm_get('/api/devices/devicemasterdata').then((data2) => {
      this.loading1 = false;
      this.DeviceMasterdataArray = data2;
      this.DeviceListarray = this.DeviceMasterdataArray.Device;
      this.DeviceTypeList = this.DeviceMasterdataArray.DeviceType;
      this.VendorList = this.DeviceMasterdataArray.DeviceVendor;
      this.DeviceMakeArray = this.DeviceMasterdataArray.DeviceManufacturer;

      sessionStorage.setItem('DeviceListArray', this.DeviceListarray);
      sessionStorage.setItem('DeviceTypeList', this.DeviceTypeList);
      sessionStorage.setItem('DeviceMakeArray', this.DeviceMakeArray);
      sessionStorage.setItem('VendorList', this.VendorList);
    });
  }

  filterDeviceStatus = 'undefined';
  DeviceSelectDatasource: any;
  DeviceStatusFilter(deviceStatus: any, selectedControl: any) {
    var dataSource;
    this.PatientNameSearch = false;
    this.searchDeviceNumber = false;

    if (deviceStatus == 'Active') {
      this.containerSelection = 1;
      this.filterDeviceStatus = 'Active';
    } else if (deviceStatus == 'Available') {
      this.containerSelection = 2;
      this.filterDeviceStatus = 'Available';
    } else if (deviceStatus == 'Error') {
      this.containerSelection = 3;
      this.filterDeviceStatus = 'Error';
    } else if (deviceStatus == 'Used') {
      this.containerSelection = 4;
      this.filterDeviceStatus = 'Used';
    }

    if (
      this.deviceSelect != undefined &&
      deviceStatus != 'undefined' &&
      this.deviceTypeSelect != undefined
    ) {
      dataSource = this.tempDeviceList.filter(
        (c: { DeviceStatus: any; Device: any; DeviceType: any }) =>
          c.DeviceStatus == deviceStatus &&
          c.Device == this.deviceSelect &&
          c.DeviceType == this.deviceTypeSelect
      );
    } else if (
      deviceStatus == 'undefined' &&
      this.deviceSelect != undefined &&
      this.deviceTypeSelect != undefined
    ) {
      dataSource = this.tempDeviceList.filter(
        (c: { Device: any; DeviceType: any }) =>
          c.Device == this.deviceSelect && c.DeviceType == this.deviceTypeSelect
      );
    } else if (
      deviceStatus != 'undefined' &&
      this.deviceSelect == undefined &&
      this.deviceTypeSelect != undefined
    ) {
      dataSource = this.tempDeviceList.filter(
        (c: { DeviceStatus: any; DeviceType: any }) =>
          c.DeviceStatus == deviceStatus &&
          c.DeviceType == this.deviceTypeSelect
      );
    } else if (
      deviceStatus != 'undefined' &&
      this.deviceSelect != undefined &&
      this.deviceTypeSelect == undefined
    ) {
      dataSource = this.tempDeviceList.filter(
        (c: { DeviceStatus: any; Device: any }) =>
          c.DeviceStatus == deviceStatus && c.Device == this.deviceSelect
      );
    } else if (
      deviceStatus != 'undefined' &&
      this.deviceSelect == undefined &&
      this.deviceTypeSelect == undefined
    ) {
      dataSource = this.tempDeviceList.filter(
        (c: { DeviceStatus: any }) => c.DeviceStatus == deviceStatus
      );
    } else if (
      deviceStatus == 'undefined' &&
      this.deviceSelect != undefined &&
      this.deviceTypeSelect == undefined
    ) {
      dataSource = this.tempDeviceList.filter(
        (c: { Device: any }) => c.Device == this.deviceSelect
      );
    } else if (
      deviceStatus == 'undefined' &&
      this.deviceSelect == undefined &&
      this.deviceTypeSelect != undefined
    ) {
      dataSource = this.tempDeviceList.filter(
        (c: { DeviceType: any }) => c.DeviceType == this.deviceTypeSelect
      );
    } else {
      dataSource = this.tempDeviceList;
    }

    this.deviceListSource = new MatTableDataSource(dataSource);
    this.changeDetectorRef.detectChanges();
    this.deviceListSource.paginator = this.paginator;

    if (selectedControl == 'dropdown' && this.deviceSelect != undefined) {
      var dataupperPanelsource: any;
      dataupperPanelsource = this.tempDeviceList.filter(
        (c: { Device: any }) => c.Device == this.deviceSelect
      );

      this.activeDeviceCountArray = dataupperPanelsource.filter(
        (c: { DeviceStatus: any }) => c.DeviceStatus == 'Active'
      );
      this.availableDeviceCountArray = dataupperPanelsource.filter(
        (c: { DeviceStatus: any }) => c.DeviceStatus == 'Available'
      );
      this.errorDeviceCountArray = dataupperPanelsource.filter(
        (c: { DeviceStatus: any }) => c.DeviceStatus == 'Error'
      );
      this.usedDeviceCountArray = dataupperPanelsource.filter(
        (c: { DeviceStatus: any }) => c.DeviceStatus == 'Used'
      );
    } else if (
      selectedControl == 'dropdown' &&
      this.deviceSelect == undefined
    ) {
      this.activeDeviceCountArray = this.tempDeviceList.filter(
        (c: { DeviceStatus: any }) => c.DeviceStatus == 'Active'
      );
      this.availableDeviceCountArray = this.tempDeviceList.filter(
        (c: { DeviceStatus: any }) => c.DeviceStatus == 'Available'
      );
      this.errorDeviceCountArray = this.tempDeviceList.filter(
        (c: { DeviceStatus: any }) => c.DeviceStatus == 'Error'
      );
      this.usedDeviceCountArray = this.tempDeviceList.filter(
        (c: { DeviceStatus: any }) => c.DeviceStatus == 'Used'
      );
    }

    this.paginator.firstPage();
  }

  setupFilter(column: string) {
    this.deviceListSource.filterPredicate = (d: any, filter: string) => {
      const textToSearch = (d[column] && d[column].toLowerCase()) || '';
      return textToSearch.indexOf(filter) !== -1;
    };
  }
  searchDeviceNumber = false;
  searchDeviceNumberData() {
    this.searchDeviceNumber = !this.searchDeviceNumber;
    this.PatientNameSearch = false;
  }
  searchDeviceNumberDataClose() {
    this.searchDeviceNumber = false;
    this.PatientNameSearch = false;
    this.getdeviceList();
  }
  applyDataFilter(filterValue: any) {
    filterValue = (<HTMLInputElement>filterValue.target).value.trim(); // Remove whitespace
    filterValue = filterValue.toLowerCase();
    filterValue = filterValue;
    this.deviceListSource.filter = filterValue;
  }
  PatientNameSearch = false;
  PatientNameSearchData() {
    this.PatientNameSearch = !this.PatientNameSearch;
    this.searchDeviceNumber = false;
  }
  DeviceAvailbleConfirm(data: any) {
   // this.dialog.open(this.dialogDeviceAvailable);
    this.deviceavailabledialog = true;
    this.deviceNumber = data.DeviceNumber;
    this.patiantName = data.PatientName;
  }
  makeDeviceAvailable() {
    var req_body;
    req_body = { DeviceNumber: this.deviceNumber };
    this.loading1 = true;
    this.rpm.rpm_post('/api/device/MakeDeviceAvailable', req_body).then(
      (data) => {
        this.DeviceAvailbleRefresh();
        alert('Removed Device Successfully');
        this.dialog.closeAll();
        this.loading1 = false;
      },
      (err) => {
        this.dialog.closeAll();
        this.loading1 = false;
        throw err;
      }
    );
  }

  DeviceAvailbleRefresh() {
    this.loading1 = true;

    this.rpm.rpm_get('/api/devices/getalldevices').then((data2) => {
      this.tempDeviceList = data2;
      this.activeDeviceCountArray = this.tempDeviceList.filter(
        (c: { DeviceStatus: any }) => c.DeviceStatus == 'Active'
      );
      this.availableDeviceCountArray = this.tempDeviceList.filter(
        (c: { DeviceStatus: any }) => c.DeviceStatus == 'Available'
      );
      this.errorDeviceCountArray = this.tempDeviceList.filter(
        (c: { DeviceStatus: any }) => c.DeviceStatus == 'Error'
      );
      this.usedDeviceCountArray = this.tempDeviceList.filter(
        (c: { DeviceStatus: any }) => c.DeviceStatus == 'Used'
      );
      this.deviceListSource = new MatTableDataSource(
        this.availableDeviceCountArray
      );
      this.changeDetectorRef.detectChanges();
      this.deviceListSource.paginator = this.paginator;
      this.paginator.firstPage();
      this.containerSelection = 2;
    });
    this.loading1 = false;
  }
  Cancel() {
    this.dialog.closeAll();
    this.deviceavailabledialog = false;
  }
}
