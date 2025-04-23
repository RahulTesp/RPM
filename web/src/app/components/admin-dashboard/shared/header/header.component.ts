import { MasterDataService } from './../../sevices/master-data.service';
import { BreakpointObserver } from '@angular/cdk/layout';
import {
  Component,
  EventEmitter,
  OnInit,
  Output,
  ViewChild,
} from '@angular/core';
import { MatDrawer } from '@angular/material/sidenav';
import { HttpService } from '../../sevices/http.service';
import {
  Event,
  Router,
  NavigationStart,
  NavigationEnd,
  RouterEvent,
} from '@angular/router';
import { RPMService } from '../../sevices/rpm.service';
import { MatDialog, MatDialogConfig } from '@angular/material/dialog';
import { StatusMessageComponent } from '../status-message/status-message.component';
import { webSocket } from 'rxjs/webSocket';
import { AuthService } from 'src/app/services/auth.service';
import { MessagingService } from '../../sevices/messaging.service';
import { Subject } from 'rxjs';
import { getMessaging, getToken, onMessage } from 'firebase/messaging';
import moment from 'moment';
import { DatePipe } from '@angular/common';
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
  private unsubscribe$ = new Subject<void>();
  message: Subject<any> = new Subject<any>();
  constructor(
    private ms: MessagingService,
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

    this.Auth.notificationdata$.subscribe((res) => {
      this.GetNotifications();
    });
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
    sessionStorage.clear();
  }
  time24To12(a: any) {
    //below date doesn't matter.
    a = '18:04';
    return new Date('1955-11-05T' + a + 'Z').toLocaleTimeString('bestfit', {
      timeZone: 'UTC',
      hour12: !0,
      hour: 'numeric',
      minute: 'numeric',
    });
  }
  convertToLocalTime(stillUtc: any) {
    stillUtc = stillUtc + 'Z';
    var local = moment(stillUtc).local().format('YYYY-MM-DD HH:mm:ss');
    return local;
  }
  data_patient: any;
  message1: any;
  ngOnInit(): void {
    this.ms.notificationData$.subscribe((notification) => {
      if (notification) {
        this.notificationBody = notification;

        setTimeout(() => (this.notificationBody = null), 5000);
      }

    });
    if (this.Auth.firstLogin == true) {
      this.ms.requestPermission();
      this.ms.requestToken();
      this.message1 = this.ms.receiveMessage();

      this.Auth.firstLogin = false;
    }
    this.message1 = this.ms.registerMessageEventListener();

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

    var hr = this.today.getHours();
    var mn = this.today.getMinutes();

    this.current_time = this.masterdataservice.formatAMPM(this.today);
    var interval2 = setInterval(() => {
      this.today = new Date();
      hr = this.today.getHours();
      mn = this.today.getMinutes();

      this.current_time = this.masterdataservice.formatAMPM(this.today);
    }, 60000);
    var strDate = this.today.toString();
    var zn = strDate.split('(');
    var zn1 = zn[1].replace(')', '');
    if (zn1 == 'India Standard Time') {
      this.tz = 'IST';
    } else if (zn1 == 'Hawaii Standard Time') {
      this.tz = 'HST';
    } else if (zn1 == 'Hawaii-Aleutian Daylight Time') {
      this.tz = 'HDT';
    } else if (zn1 == 'Alaska Daylight Time') {
      this.tz = 'AKDT';
    } else if (zn1 == 'Pacific Daylight Time') {
      this.tz = 'PDT';
    } else if (zn1 == 'Mountain Standard Time') {
      this.tz = 'MST';
    } else if (zn1 == 'Mountain Daylight Time') {
      this.tz = 'MDT';
    } else if (zn1 == 'Eastern Daylight Time') {
      this.tz = 'EDT';
    } else if (zn1 == 'Central Standard Time') {
      this.tz = 'CDT';
    }
    this.GetNotifications();


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

  notificationList = [
    {
      notification_title: 'John Abraham added as Watcher',
      notification_time: '8 minutes',
    },
  ];
  roles: any;
  http_getPatientList: any;
  notification_click() {
    // New Change 11/05/2023
    if (this.unread > 0) {
      this.notification_active = !this.notification_active;
    } else {
      this.router.navigate(['/admin/notification']);
    }
  }
  open_notifications() {
    this.notification_active = false;
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

    // dialogConfig.disableClose = true;
    dialogConfig.autoFocus = true;

    dialogConfig.data = {
      title: title,
      item: item,
    };

    this.dialog.open(StatusMessageComponent, dialogConfig);
  }

  subject: any;
  pubsuburl: any;
  msg: any;
  subscribePubsub() {
    this.subject = webSocket(this.pubsuburl);
    this.subject.subscribe(
      (msg: any) => {
        this.msg = msg;
        if (this.msg.EventType == 'NotificationRead') {
          this.GetNotifications();
        }
      },
      (err: any) => {
        // alert(err)
        // this.rpmservice.rpm_get("/api/home/getdashboardalerts?RoleId="+this.roles[0].Id).then((data)=>{
        //   this.alertsArray=data;
        // });
        // this.subscribePubsub();
        console.log(err);
      },
      () => {
        console.log('completed');
      }
    );
  }
  rolelist: any;
  notifications: any;
  unread: any;
  list: any;
  unreadlist: any;
  GetNotifications() {
    var that = this;
    that.rolelist = sessionStorage.getItem('Roles');
    that.rolelist = JSON.parse(this.rolelist);
    that.rpm.rpm_get('/api/notification/user').then((data) => {
      that.notifications = data;
      that.unread = that.notifications.TotalUnRead;
      that.list = that.notifications.Data;
      that.unreadlist = that.list.filter((data: { IsRead: boolean }) => {
        return data.IsRead == false;
      });
    });
  }
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
    } else if (diff_min > 20161 && diff_min < 30240) {
      msgInfo = '2 week Ago';
    } else if (diff_min > 30241 && diff_min < 40320) {
      msgInfo = '3 week Ago';
    } else if (diff_min > 40321 && diff_min < 86400) {
      msgInfo = '1 month Ago';
    } else if (diff_min > 86401 && diff_min < 172800) {
      msgInfo = '2 month Ago';
    } else if (diff_min > 172801 && diff_min < 216000) {
      msgInfo = '3 month Ago';
    } else if (diff_min > 216001 && diff_min < 259200) {
      msgInfo = '4 month Ago';
    } else if (diff_min > 259201 && diff_min < 302400) {
      msgInfo = '5 month Ago';
    } else if (diff_min > 302401 && diff_min < 345600) {
      msgInfo = '6 month Ago';
    } else {
      msgInfo = 'more Than 6 months';
    }

    return msgInfo;
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
    // fetch remote data from here
    // And reassign the 'data' which is binded to 'data' property.
  }

  onFocused(e: any) {
    // do something when input is focused
  }
}
