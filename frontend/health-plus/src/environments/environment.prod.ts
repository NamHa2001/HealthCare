export const environment = {
  production: true,
  // Site API trên MonsterASP — đổi nếu tên site thực tế khác.
  apiUrl: 'https://healthplus-api.runasp.net/api',
  // BUG-22: điền giá trị thật từ Firebase Console > Project Settings > Cloud Messaging.
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
