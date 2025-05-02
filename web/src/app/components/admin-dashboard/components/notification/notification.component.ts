
import { Component, OnInit } from '@angular/core';
import { RPMService } from '../../sevices/rpm.service';
import { Router } from '@angular/router';
import moment from 'moment';
import { ConfirmDialogServiceService } from '../../shared/confirm-dialog-panel/service/confirm-dialog-service.service';
import { MessagingService } from '../../sevices/messaging.service';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-notification',
  templateUrl: './notification.component.html',
  styleUrls: ['./notification.component.scss'],
})
export class NotificationComponent implements OnInit {
  listener_dataSource: any;
  read: boolean;
  notification_value: any;
  private notificationSubscription: Subscription | null = null;

  // Pagination variables
  unreadPageSize: number = 8;
  unreadCurrentPage: number = 1;
  historyPageSize: number = 8;
  historyCurrentPage: number = 1;

  // Display variables
  displayedUnread: any[] = [];
  displayedHistory: any[] = [];

  // Total counts
  totalUnread: number = 0;
  totalHistory: number = 0;

  constructor(
    private rpm: RPMService,
    private router: Router,
    private confirmDialog: ConfirmDialogServiceService,
    private messageservice: MessagingService
  ) {}

  ngOnInit(): void {
    this.refreshNotifications();

    this.notificationSubscription = this.messageservice.notificationData$.subscribe(data => {
      if (data && data !== 'notificationcount' && data.type === 'update') {
        this.unread = this.messageservice.unread;
        this.list = this.messageservice.list || [];
        this.processNotifications();
      }
    });

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

  notificationlist: any[] = [];
  historylist: any[] = [];
  rolelist: any;
  notifications: any;
  unread: any;
  list: any;
  unreadlist: any;
  readlist: any;

  // Process notifications and set up pagination
  processNotifications() {
    if (!this.list || this.list.length === 0) return;

    // Extract all notification items from the list
    let allUnreadItems: any[] = [];
    let allHistoryItems: any[] = [];

    this.list.forEach((notification: any) => {
      if (notification.NotificationList && notification.NotificationList.length > 0) {
        notification.NotificationList.forEach((item: any) => {
          if (item.IsRead === false) {
            allUnreadItems.push(item);
          } else {
            allHistoryItems.push(item);
          }
        });
      }
    });

    this.totalUnread = allUnreadItems.length;
    this.totalHistory = allHistoryItems.length;

    // Set initial display
    this.updateDisplayedUnread();
    this.updateDisplayedHistory();
  }

  // Load more unread notifications
  loadMoreUnread() {
    this.unreadCurrentPage++;
    this.updateDisplayedUnread();
  }

  // Load more history notifications
  loadMoreHistory() {
    this.historyCurrentPage++;
    this.updateDisplayedHistory();
  }

  // Update displayed unread items based on current page
  updateDisplayedUnread() {
    if (!this.list) return;

    let allUnreadItems: any[] = [];

    this.list.forEach((notification: any) => {
      if (notification.NotificationList && notification.NotificationList.length > 0) {
        notification.NotificationList.forEach((item: any) => {
          if (item.IsRead === false) {
            allUnreadItems.push(item);
          }
        });
      }
    });

    // Get items for current page
    const startIndex = 0;
    const endIndex = this.unreadCurrentPage * this.unreadPageSize;
    this.displayedUnread = allUnreadItems.slice(startIndex, endIndex);
  }

  // Update displayed history items based on current page
  updateDisplayedHistory() {
    if (!this.list) return;

    let allHistoryItems: any[] = [];

    this.list.forEach((notification: any) => {
      if (notification.NotificationList && notification.NotificationList.length > 0) {
        notification.NotificationList.forEach((item: any) => {
          if (item.IsRead === true) {
            allHistoryItems.push(item);
          }
        });
      }
    });

    // Get items for current page
    const startIndex = 0;
    const endIndex = this.historyCurrentPage * this.historyPageSize;
    this.displayedHistory = allHistoryItems.slice(startIndex, endIndex);
  }

  // Check if there are more unread items to load
  hasMoreUnread(): boolean {
    return this.displayedUnread.length < this.totalUnread;
  }

  // Check if there are more history items to load
  hasMoreHistory(): boolean {
    return this.displayedHistory.length < this.totalHistory;
  }



  readNotification(NotificationId: number, NotificationAuditId: number) {
    this.messageservice.markNotificationAsRead(NotificationId, NotificationAuditId)
    .then(() => {
      console.log('Notification marked as read and data refreshed');
    })
    .catch(err => {
      console.error('Failed to update notification:', err);
      alert('Failed to update Notification.');
    });
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
    const today = new Date();
    const localTimeString = this.convertToLocalTime(date);
    const notification_date = new Date(localTimeString);
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
          this.confirmDialog.showConfirmDialog(
            'Notification Deleted Successfully',
            'Message',
            () => {
              this.refreshNotifications();
            },
            false
          );
        },
        (err) => {
          this.confirmDialog.showConfirmDialog(
            err.error || 'Could not delete notification.',
            'Error',
            null,
            false
          );
        }
      );
  }

  openDeleteNotificationDialog(documnetId: any) {
    this.confirmDialog.showConfirmDialog(
      'Do You Want to Delete the Notification?',
      'Are you sure?',
      () => {
        this.deleteNotification(documnetId);
      },
      true
    );
  }

  openClearAllDialog() {
    this.confirmDialog.showConfirmDialog(
      'Do You Want to Delete all Notifications?',
      'Are you sure?',
      () => {
        this.clearAll();
      },
      true
    );
  }

  clearAll() {
    this.rpm
      .rpm_post(`/api/notification/deletenotifications?notificationId=0`, {})
      .then(
        (data) => {
          this.confirmDialog.showConfirmDialog(
            'All Notifications Deleted Successfully',
            'Message',
            () => {
              this.refreshNotifications();
            },
            false
          );
        },
        (err) => {
          this.confirmDialog.showConfirmDialog(
            err.error || 'Could not delete notifications.',
            'Error',
            null,
            false
          );
        }
      );
  }

  refreshNotifications() {
    this.messageservice.GetNotifications()
      .then(() => {
        this.unread = this.messageservice.unread;
        this.list = this.messageservice.list || [];
        this.processNotifications();
      })
      .catch(error => {
        console.error('Error refreshing notifications:', error);
      });
  }

  ngOnDestroy(): void {
    if (this.notificationSubscription) {
      this.notificationSubscription.unsubscribe();
    }
  }
}
