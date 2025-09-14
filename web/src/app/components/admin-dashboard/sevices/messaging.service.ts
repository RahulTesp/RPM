import { Injectable } from '@angular/core';
import { AngularFireMessaging } from '@angular/fire/compat/messaging';
import { BehaviorSubject } from 'rxjs';
import { MatSnackBar } from '@angular/material/snack-bar';
import { RPMService } from './rpm.service';

@Injectable()
export class MessagingService {
  public notificationData = new BehaviorSubject<any>('notificationcount');
  notificationData$ = this.notificationData.asObservable();
  private unreadCountSubject = new BehaviorSubject<number>(0);
  public unreadCount$ = this.unreadCountSubject.asObservable();
  firebase_token: any;
  notifications: any;
  unread: number = 0;
  list: any[] = [];
  unreadlist: any[] = [];

  constructor(
    private afm: AngularFireMessaging,
    private snackBar: MatSnackBar,
    private rpmService: RPMService
  ) {
    this.receiveMessage();
    this.registerMessageEventListener();
  }

  requestPermission() {
    return this.afm.requestPermission.subscribe(
      () => {
        console.log('Permission granted!');
        this.requestToken();
      },
      (error) => {
        console.error('Permission denied:', error);
      }
    );
  }

  requestToken() {
    // Return a Promise for better handling
    return new Promise((resolve, reject) => {
      // Wait for service worker to be ready
      if ('serviceWorker' in navigator) {
        navigator.serviceWorker.ready.then(() => {
          console.log('Service worker is ready');

          // Subscribe to get token
          this.afm.requestToken.subscribe({
            next: (token) => {
              this.firebase_token = token;
              this.saveFirebaseToken();
              resolve(token);
            },
            error: (err) => {
              console.error('Error getting token:', err);
              reject(err);
            }
          });
        });
      } else {
        reject('Service workers not supported');
      }
    });
  }

  receiveMessage() {
    this.afm.messages.subscribe((message) => {
      console.log('Foreground message received:', message.data);
     this.notificationData.next(message);
      // this.snackBar.open(
      //   message?.data?.Title || 'New message',
      //   message?.data?.Body || 'Check your notifications',
      //   { duration: 5000 }
      // );
          this.snackBar.open(
            message?.data?.Title|| 'New message',
          '',
         // payload.data.Body,
          { duration: 5000 }
        );

      this.GetNotifications()
      .then(() => console.log('Notifications refreshed after background message'))
      .catch(err => console.error('Failed to refresh notifications:', err));
    });
  }

  // registerMessageEventListener() {
  //   const channel = new BroadcastChannel('notification-channel');
  //   channel.addEventListener('message', (event) => {
  //     const payload = event.data;
  //     console.log('Background notification received:', payload.data.body);
  //     this.notificationData.next(payload.data.body);
  //     if (payload.data && payload.data.Body) {
  //       this.snackBar.open(
  //         payload.notification?.title || 'New message',
  //         '',
  //         { duration: 5000 }
  //       );

  //     }

  //   });
  //   return channel;
  // }
  registerMessageEventListener() {
    const channel = new BroadcastChannel('notification-channel');
    channel.addEventListener('message', (event) => {
      const payload = event.data;
      console.log('Background notification received:', payload.data.body);
      this.notificationData.next(payload.data.body);

       if (payload.data && payload.data.Body) {
      //   this.snackBar.open(
      //    payload.notification?.title || 'New message',
      //     '',
      //    // payload.data.Body,
      //     { duration: 5000 }
      //   );

        this.GetNotifications()
          .then(() => console.log('Notifications refreshed after background message'))
          .catch(err => console.error('Failed to refresh notifications:', err));
      }
    });
    return channel;
  }

  saveFirebaseToken() {
    if (!this.firebase_token) return;

    this.rpmService
      .rpm_post(
        `/api/notification/insertfirebasetoken?Token=${this.firebase_token}`,
        {}
      )
      .then(
        (data) => {
          console.log('Token saved successfully');
        },
        (err) => {
          console.error('Error saving token:', err);
        }
      );
  }
  GetNotifications() {

    return this.rpmService.rpm_get('/api/notification/user')
      .then((data) => {
        this.notifications = data;
        this.unread = this.notifications.TotalUnRead;
        this.list = this.notifications.Data;
        this.unreadlist = this.list.filter((item: { IsRead: boolean }) => {
          return item.IsRead === false;
        });
        this.notificationData.next({
          type: 'update',
          count: this.unread,
          data: this.list
        });
       // this.updateUnreadCountFromList(this.list);
        // You can also update your BehaviorSubject with the notification count if needed
        // this.notificationData.next({ type: 'count', count: this.unread });

        return data;
      })
      .catch((error) => {
        console.error('Error fetching notifications:', error);
        throw error;
      });
  }

  getCurrentUnreadCount(): number {
     const data = this.notificationData.getValue();
     return data?.count ?? 0;
  }

  markNotificationAsRead(notificationId: number, notificationAuditId: number): Promise<any> {
    const reqBody = {
      NotificationId: notificationId,
      NotificationAuditId: notificationAuditId
    };

    return this.rpmService.rpm_put('/api/notification/readstatus', reqBody)
      .then((response) => {
        // Refresh notification data
        return this.GetNotifications();
      });
  }
}
