export const environment = {
  production: false,
  apiUrl: 'http://localhost:5199/api',
  // apiUrl: 'https://hddtlp6f-5199.asse.devtunnels.ms/api',
  // BUG-22: điền giá trị thật từ Firebase Console > Project Settings > Cloud Messaging
  // (Web Push certificates → VAPID key) trước khi push notification hoạt động được.
  firebase: {
    apiKey: '',
    authDomain: '',
    projectId: '',
    storageBucket: '',
    messagingSenderId: '',
    appId: '',
    vapidKey: '',
  },
};
