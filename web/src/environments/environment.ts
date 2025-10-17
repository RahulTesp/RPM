// This file can be replaced during build by using the `fileReplacements` array.
// `ng build` replaces `environment.ts` with `environment.prod.ts`.

// import { VAPID_KEY } from '@angular/fire/compat/messaging';

// The list of file replacements can be found in `angular.json`.
export const environment = {
  production: false,
  //  host: 'rpm-demo-api.azurewebsites.net',
  host: 'cx-preprod-server.azurewebsites.net',
  //host: 'cx-dev-server.azurewebsites.net',
  //host: 'rpm-demo-api.azurewebsites.net',
  // host: 'cx-dev-server.azurewebsites.net',
  //host: 'md-preprod-server.azurewebsites.net',
  // host: 'md-dev-server.azurewebsites.net',
//  host: 'c-lynxapi.azurewebsites.net',
  //host: 'rpm-demo-api.azurewebsites.net',
  // host: 'meditprodapi.azurewebsites.net',
  // host:'rpm-dev-tespcare.azurewebsites.net',
   //host : 'rpm-multivital-api-v2.azurewebsites.net',

  protocol: 'https',
  firebase: {
    apiKey: 'AIzaSyACz7ajD-LO6x6FlcSjTcevAV0dVDypans',
    authDomain: 'rpmadmin-a5143.firebaseapp.com',
    projectId: 'rpmadmin-a5143',
    storageBucket: 'rpmadmin-a5143.appspot.com',
    messagingSenderId: '1089577441128',
    //appId: '1:1089577441128:web:d656ba5644330dfc3168dd',
    //measurementId: 'G-R0H1QC84SS',
    appId: '1:1089577441128:web:675fbd66311496b33168dd',
    measurementId: 'G-723GDEFNHZ',
    vapidKey:
      'BPoa8OvqqjOqFSNALjNhDLD0jtYGyVTQgtq-7EiYot99AgQvbAzhoOEQQWhr2xjGSIqX6dvMqWGxl_6lKF-QqGI',
  },
};
/*
 * For easier debugging in development mode, you can import the following file
 * to ignore zone related error stack frames such as `zone.run`, `zoneDelegate.invokeTask`.
 *
 * This import should be commented out in production mode because it will have a negative impact
 * on performance if an error is thrown.
 */
// import 'zone.js/plugins/zone-error';  // Included with Angular CLI.
