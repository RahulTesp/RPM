import { Injectable } from '@angular/core';
import { AngularFireMessaging } from '@angular/fire/compat/messaging';
import { BehaviorSubject, Subject } from 'rxjs';
import { MatSnackBar } from '@angular/material/snack-bar';
import { takeUntil } from 'rxjs/operators';
import { initializeApp } from 'firebase/app';
import { getMessaging, getToken, onMessage } from 'firebase/messaging';
import { RPMService } from './rpm.service';
import {environment} from '../../../../environments/environment'
@Injectable()
export class MessagingService {
  public notificationData = new BehaviorSubject<any>('notificationcount');
  notificationData$ = this.notificationData.asObservable();
  firebase_token: any;

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
              console.log('Token received:', token);
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
      this.snackBar.open(
        message?.data?.Title || 'New message',
        message?.data?.Body || 'Check your notifications',
        { duration: 5000 }
      );
    });
  }

  registerMessageEventListener() {
    const channel = new BroadcastChannel('notification-channel');
    channel.addEventListener('message', (event) => {
      const payload = event.data;
      console.log('Background notification received:', payload.data.body);
      this.notificationData.next(payload.data.body);
      if (payload.data && payload.data.Body) {
        this.snackBar.open(
          payload.notification?.title || 'New message',
          payload.data.Body,
          { duration: 5000 }
        );
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
}
