// BUG-22: service worker riêng cho Firebase Cloud Messaging (background push).
// Đăng ký ở scope riêng (/firebase-cloud-messaging-push-scope) trong
// push-subscription.service.ts để không xung đột với ngsw-worker.js (Angular PWA).
//
// LƯU Ý: điền đúng config Firebase Console giống hệt src/environments/environment*.ts
// — service worker là file tĩnh, không đọc được biến môi trường Angular lúc build.
importScripts('https://www.gstatic.com/firebasejs/10.14.1/firebase-app-compat.js');
importScripts('https://www.gstatic.com/firebasejs/10.14.1/firebase-messaging-compat.js');

firebase.initializeApp({
  apiKey: '',
  authDomain: '',
  projectId: '',
  storageBucket: '',
  messagingSenderId: '',
  appId: '',
});

const messaging = firebase.messaging();

messaging.onBackgroundMessage((payload) => {
  const title = payload.notification?.title ?? 'Health+';
  self.registration.showNotification(title, {
    body: payload.notification?.body ?? '',
    icon: '/icons/icon-192x192.png',
    badge: '/icons/icon-72x72.png',
    data: payload.data,
  });
});
