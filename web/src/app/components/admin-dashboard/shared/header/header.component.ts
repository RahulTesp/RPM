import { Notification } from './../notification-panel/notification';
import { MasterDataService } from './../../sevices/master-data.service';
import { BreakpointObserver } from '@angular/cdk/layout';
import {
  Component,
  EventEmitter,
  OnInit,
  Output,
  ViewChild,
} from '@angular/core';
import { HttpService } from '../../sevices/http.service';
import {
  Router
} from '@angular/router';
import { RPMService } from '../../sevices/rpm.service';
import { MatDialog, MatDialogConfig } from '@angular/material/dialog';
import { StatusMessageComponent } from '../status-message/status-message.component';
import { webSocket } from 'rxjs/webSocket';
import { AuthService } from 'src/app/services/auth.service';
import { MessagingService } from '../../sevices/messaging.service';
import { Subject, Subscription } from 'rxjs';
import { DatePipe } from '@angular/common';
import dayjs from 'dayjs';
import utc from 'dayjs/plugin/utc';
import timezone from 'dayjs/plugin/timezone';

dayjs.extend(utc);
dayjs.extend(timezone);
@Component({
  selector: 'app-header',
  templateUrl: './header.component.html',
  styleUrls: ['./header.component.scss'],
})
export class HeaderComponent implements OnInit {
  screensmall = false;
  username: any;
  notification_count = 3;
  notification_active = false;
  menuState = 'in';
  user_name: any;
  today = new Date();
  currentDate = new Date();
  current_day: any;
  current_month: any;
  current_year = this.today.getUTCFullYear();
  notificationBody: string | null = null;
  public notificationLists: { message: string; time: string }[] = [];
  // current_time = this.formatAMPM(this.today)
  @Output() emitter: EventEmitter<string> = new EventEmitter<string>();
  @ViewChild('auto') auto: any;
  tz: string;
  myprofile: any;
  current_time: string;
  host: any;
  private subscription: Subscription | null = null;
  private unsubscribe$ = new Subject<void>();
  message: Subject<any> = new Subject<any>();
  constructor(
    public ms: MessagingService,
    private observer: BreakpointObserver,
    public model: HttpService,
    private router: Router,
    private rpm: RPMService,
    private dialog: MatDialog,
    private _router: Router,
    private Auth: AuthService,
    public datepipe: DatePipe,
    private masterdataservice:MasterDataService
  ) {
    this.host = this.Auth.get_environment();

    this.Auth.data$.subscribe((res) => {
      this.getPatientList();
    });

    // this.Auth.notificationdata$.subscribe((res) => {
    //   this.GetNotifications();
    // });

    this.user_name = sessionStorage.getItem('user_name');
    if (!this.user_name || this.user_name == 'undefined undefined') {
      var that = this;
      that.rpm.rpm_get('/api/users/getmyprofiles').then((data) => {
        that.myprofile = data;
        that.user_name =
          that.myprofile.FirstName + ' ' + that.myprofile.LastName;
      });
    }
    this.userInputfeild = false;
  }
  dateval: any;

  timeZoneOffset: any;
  onDestroy() {
    sessionStorage.clear();
  }

  convertToLocalTime(stillUtc: any) {
    stillUtc = stillUtc + 'Z';
   const local = dayjs.utc(stillUtc).local().format('YYYY-MM-DD HH:mm:ss');
    return local;
  }
  data_patient: any;
  message1: any;
  ngOnInit(): void {

  //  this.subscription = this.ms.notificationData$.subscribe(data => {
  //   if (data && data.type === 'update') {
  //     this.unread = data.count;
  //   }
  // });
    this.subscription = this.ms.notificationData$.subscribe(data => {
      if (data && data.notification) {
        // Refresh notifications when we receive a new one
        this.refreshNotifications();
      }
    });

    this.refreshNotifications();

    if (this.Auth.firstLogin == true) {
      this.ms.requestPermission();
      this.ms.requestToken();
      this.message1 = this.ms.receiveMessage();

      this.Auth.firstLogin = false;
    }
   // this.message1 = this.ms.registerMessageEventListener();

    var days = [
      'Sunday',
      'Monday',
      'Tuesday',
      'Wednesday',
      'Thursday',
      'Friday',
      'Saturday',
    ];
    var months = [
      'January',
      'February',
      'March',
      'April',
      'May',
      'June',
      'July',
      'August',
      'September',
      'October',
      'November',
      'December',
    ];

    this.current_day = days[this.today.getDay()];
    this.current_month = months[this.today.getMonth()];
    this.dateval = this.today.getDate();
    this.current_time = this.masterdataservice.formatAMPM(this.today);
    this.getPatientList();
  }

  getPatientList() {
    this.roles = sessionStorage.getItem('Roles');
    this.roles = JSON.parse(this.roles);
    if (this.roles[0].Id != 7) {
      this.rpm
        .rpm_get('/api/patient/getsearchpatientlist?RoleId=' + this.roles[0].Id)
        .then((data) => {
          this.http_getPatientList = data;
          for (let x of this.http_getPatientList) {
            x.searchField =
              x.PatientNumber +
              ' - ' +
              x.PatientName +
              '[' +
              x.ProgramName +
              ']';
          }
        });
    }
  }

  roles: any;
  http_getPatientList: any;
  notification_click() {
      this.router.navigate(['/admin/notification']);

  }
  ngAfterViewInit() {
    this.observer.observe(['(max-width: 1500px)']).subscribe((res) => {
      if (res.matches) {
        this.screensmall = true;
        this.toggleMenu();
      } else {
        this.screensmall = false;
        this.toggleMenu();
      }
    });
  }
  toggleMenu() {
    this.emitter.emit(this.menuState);
  }


  searchtopic: any;
  searchResult: any;
  userInputfeild = false;
  searchPatient() {
    this.userInputfeild = !this.userInputfeild;
    var that = this;
    that.rpm
      .rpm_get(`/api/patient/searchpatient?PatientNumber=${this.searchtopic}`)
      .then(
        (data) => {
          that.searchResult = data;
          if (
            that.searchResult.PatientId != null &&
            that.searchResult.PatientId != undefined &&
            that.searchResult.PatientId != 0
          ) {
            let route = '/admin/patients_detail';
            this.auto.clear();
            this._router.navigate([route], {
              queryParams: {
                id: that.searchResult.PatientId,
                programId: that.searchResult.PatientProgramId,
              },
              skipLocationChange: true,
            });
            sessionStorage.removeItem('patient-page-status');
          } else {
            that.openDialogWindow(
              'Not Found',
              `Patient Details not found...!!!`
            );
          }

          this.auto.clear();
        },
        (err) => {
          that.openDialogWindow('Not Found', `Patient Details not found...!!!`);
          this.auto.clear();
        }
      );
    this.searchtopic = '';
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
  }

  rolelist: any;
  notifications: any;
  unread: any;
  list: any;
  unreadlist: any;
  refreshNotifications() {
    this.ms.GetNotifications()
      .then(() => {
        // Update local properties with data from service
        this.unread = this.ms.unread;
        this.list = this.ms.list || [];
        this.unreadlist = this.list.filter((data: { IsRead: boolean }) => {
                return data.IsRead == false;
              });
      })
      .catch(error => {
        console.error('Error refreshing notifications:', error);
      });
  }


  keyword = 'searchField';
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

  SelectId: any;
  ProgramId: any;
  selectEvent(item: any) {
    // do something with selected item
    this.SelectId = item.PatientId;
    if (this.http_getPatientList && this.http_getPatientList.length > 0) {
      var selected = this.http_getPatientList.filter(
        (data: { PatientId: any }) => {
          return data.PatientId == this.SelectId;
        }
      );

      if (selected != null && selected != undefined && selected.length != 0) {
        this.ProgramId = selected[0].PatientProgramId;
        let route = '/admin/patients_detail';
        this.auto.clear();
        this._router.navigate([route], {
          queryParams: { id: this.SelectId, programId: this.ProgramId },
          skipLocationChange: true,
        });

        sessionStorage.removeItem('patient-page-status');
      } else {
        alert('Invalid Patient Id');
      }
    }
  }

  onChangeSearch(val: string) {
  }
  onFocused(e: any) {
  }
}
