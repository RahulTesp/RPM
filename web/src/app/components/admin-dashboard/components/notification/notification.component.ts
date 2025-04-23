import { Component, OnInit } from '@angular/core';

import { HttpService } from '../../sevices/http.service';
import { RPMService } from '../../sevices/rpm.service';
import { ActivatedRoute, Router } from '@angular/router';
import { AuthService } from 'src/app/services/auth.service';
import { ConfirmDialogComponent } from '../../shared/confirm-dialog/confirm-dialog.component';
import { MatDialog } from '@angular/material/dialog';
import moment from 'moment';

@Component({
  selector: 'app-notification',
  templateUrl: './notification.component.html',
  styleUrls: ['./notification.component.scss'],
})
export class NotificationComponent implements OnInit {
  listener_dataSource: any;
  read: boolean;
  notification_value: any;
  constructor(
    private http: HttpService,
    private rpm: RPMService,
    private router: Router,
    private Auth: AuthService,
    public dialog: MatDialog
  ) {
    var that = this;
  }

  ngOnInit(): void {
    this.GetNotifications();

    this.notificationlist = [
      {
        description: '',
        time: '',
      },
    ];

    this.historylist = [
      {
        description: '',
        time: '',
      },
    ];
  }

  //this.notificationlist=[];

  notificationlist: any[] = [];

  historylist: any[] = [];

  rolelist: any;
  notifications: any;
  unread: any;
  list: any;
  unreadlist: any;
  readlist: any;
  GetNotifications() {
    var that = this;
    that.rolelist = sessionStorage.getItem('Roles');
    that.rolelist = JSON.parse(this.rolelist);
    that.rpm.rpm_get('/api/notification/user').then((data) => {
      that.notifications = data;
      that.unread = that.notifications.TotalUnRead;
      // that.unreadlist = that.notifications.Data.map((element: { NotificationList: any[]; }) => {
      //   return {...element, NotificationList: element.NotificationList.filter((data) => data.IsRead == false)}
      // })

      // console.log(that.unreadlist);
      // that.unreadlist = that.notifications.Data.filter((data: { IsRead: boolean; })=>{
      //   return data.IsRead==false
      // })

      // that.readlist = that.notifications.Data.filter((data: { IsRead: boolean; })=>{
      //   return data.IsRead==true
      // })
      console.log(that.readlist);
    });
  }
  readNotification(NotificationId: any, NotificationAuditId: any) {
    var that = this;
    var reqBody = {
      NotificationId: NotificationId,
      NotificationAuditId: NotificationAuditId,
    };
    that.rpm.rpm_put('/api/notification/readstatus', reqBody).then(
      (data) => {
        alert('Notification marked as read.');
        this.GetNotifications();
        this.Auth.reloadnotification('Notification Upadted');
      },
      (err) => {
        alert('Failed to update Notification.');
      }
    );
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

  notificationTime(date: any) {
    const today = new Date(); // Current time in local timezone

    // Convert the UTC date to local time string
    const localTimeString = this.convertToLocalTime(date);

    // Parse the local time string into a Date object
    const notification_date = new Date(localTimeString);

    // Calculate time difference in minutes
    const diff_min = Math.round(
      (today.getTime() - notification_date.getTime()) / 60000
    );

    let msgInfo = '';

    if (diff_min == 0) {
      msgInfo = 'Few Seconds Ago';
    } else if (diff_min < 60) {
      msgInfo = diff_min + ' Min';
    } else if (diff_min < 1440) {
      var hr = Math.round(diff_min / 60);
      msgInfo = hr + ' Hrs';
    } else if (diff_min > 1440 && diff_min < 2880) {
      msgInfo = 'Yesterday';
    } else if (diff_min > 2880 && diff_min < 10080) {
      // Get day of week from the date object
      let dayofweek = notification_date.getDay();
      let dayval = '';
      switch (dayofweek) {
        case 0: dayval = 'Sunday'; break;
        case 1: dayval = 'Monday'; break;
        case 2: dayval = 'Tuesday'; break;
        case 3: dayval = 'Wednesday'; break;
        case 4: dayval = 'Thursday'; break;
        case 5: dayval = 'Friday'; break;
        case 6: dayval = 'Saturday'; break;
      }
      msgInfo = dayval;
    } else if (diff_min > 10080 && diff_min < 20160) {
      msgInfo = '1 Week Ago';
    } else if (diff_min > 20160 && diff_min < 30240) {
      msgInfo = '2 Weeks Ago';
    } else if (diff_min > 30240 && diff_min < 40320) {
      msgInfo = '3 Weeks Ago';
    } else if (diff_min > 40320 && diff_min < 86400) {
      msgInfo = '1 Month Ago';
    } else if (diff_min > 86400 && diff_min < 172800) {
      msgInfo = '2 Months Ago';
    } else if (diff_min > 172800 && diff_min < 216000) {
      msgInfo = '3 Months Ago';
    } else if (diff_min > 216000 && diff_min < 259200) {
      msgInfo = '4 Months Ago';
    } else if (diff_min > 259200 && diff_min < 302400) {
      msgInfo = '5 Months Ago';
    } else if (diff_min > 302400 && diff_min < 345600) {
      msgInfo = '6 Months Ago';
    } else {
      msgInfo = 'More Than 6 Months';
    }

    return msgInfo;
  }
  isShown: boolean = true;
  after() {
    this.isShown = !this.isShown;
  }
  backtohome() {
    // let route = '/admin/home';
    // this.router.navigate([route])
    this.rolelist = sessionStorage.getItem('Roles');
    this.rolelist = JSON.parse(this.rolelist);
    if (this.rolelist[0].Id == 7) {
      this.router.navigate(['/admin/patient-home']);
    } else {
      this.router.navigate(['/admin/home']);
    }
  }
  deleteNotification(notificationId: any) {
    this.rpm
      .rpm_post(
        `/api/notification/deletenotifications?notificationId=${notificationId}`,
        {}
      )
      .then(
        (data) => {
          alert('Notification Deleted Successfully');
          this.GetNotifications();
        },
        (err) => {
          alert(err.error);
        }
      );
  }

  openDeleteNotificationDialog(documnetId: any) {
    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      maxWidth: '400px',
      data: {
        title: 'Are you sure?',
        message: 'Do You Want to Delete the Notification ? ',
      },
    });
    dialogRef.afterClosed().subscribe((dialogResult: any) => {
      if (dialogResult) {
        this.deleteNotification(documnetId);
      } else {
        return;
      }
    });
  }

  openClearAllDialog() {
    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      maxWidth: '400px',
      data: {
        title: 'Are you sure?',
        message: 'Do You Want to Delete all Notifications ? ',
      },
    });
    dialogRef.afterClosed().subscribe((dialogResult: any) => {
      // if user pressed yes dialogResult will be true,
      // if he pressed no - it will be false

      if (dialogResult) {
        this.clearAll();
      } else {
        return;
      }
    });
  }
  clearAll() {
    this.rpm
      .rpm_post(`/api/notification/deletenotifications?notificationId=0`, {})
      .then(
        (data) => {
          this.GetNotifications();
          alert('Notification Deleted Successfully');
        },
        (err) => {
          alert(err.error);
        }
      );
  }
}
