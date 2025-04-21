
importScripts('https://www.gstatic.com/firebasejs/10.6.0/firebase-app-compat.js');
importScripts('https://www.gstatic.com/firebasejs/10.6.0/firebase-messaging-compat.js');


firebase.initializeApp({
  apiKey: "AIzaSyACz7ajD-LO6x6FlcSjTcevAV0dVDypans",
  authDomain: "rpmadmin-a5143.firebaseapp.com",
  projectId: "rpmadmin-a5143",
  storageBucket: "rpmadmin-a5143.appspot.com",
  messagingSenderId: "1089577441128",
  appId: "1:1089577441128:web:d656ba5644330dfc3168dd",
  measurementId: "G-R0H1QC84SS",
});

const messaging = firebase.messaging();

// Handle background messages
messaging.onBackgroundMessage(function(payload) {
  console.log('Received background message:', payload);

  // Forward to the Angular app using BroadcastChannel
  const channel = new BroadcastChannel('notification-channel');
  channel.postMessage(payload);

  // Show notification if needed
  const notificationTitle = payload.notification?.title || 'New Notification';
  const notificationOptions = {
    body: payload.notification?.body || payload.data?.Body || '',
    icon: '/assets/icons/icon-72x72.png',
    data: payload.data
  };

  return self.registration.showNotification(notificationTitle, notificationOptions);
});

// Also listen for push events
self.addEventListener("push", (event) => {
  if (!event.data) return;

  const payload = event.data.json();
  const channel = new BroadcastChannel("notification-channel");
  channel.postMessage(payload);

  // Show notification if not already handled by onBackgroundMessage
  if (!payload.notification) {
    const notificationTitle = payload.data?.title || 'New Notification';
    const notificationOptions = {
      body: payload.data?.Body || '',
      icon: '/assets/icons/icon-72x72.png',
      data: payload.data
    };

    event.waitUntil(
      self.registration.showNotification(notificationTitle, notificationOptions)
    );
  }
});

// Handle notification click
self.addEventListener('notificationclick', (event) => {
  event.notification.close();

  // This looks to see if the current is already open and focuses if it is
  event.waitUntil(
    clients.matchAll({
      type: "window"
    }).then((clientList) => {
      for (const client of clientList) {
        if (client.url === '/' && 'focus' in client)
          return client.focus();
      }
      if (clients.openWindow)
        return clients.openWindow('/');
    })
  );
});
