import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { initializeApp, FirebaseApp } from 'firebase/app';
import { getMessaging, getToken, onMessage, Messaging } from 'firebase/messaging';
import { environment } from '../../../environments/environment';
import { ApiService } from './api.service';

export interface PushSubscribeRequest {
  fcmToken: string;
  deviceType: string;
}

/**
 * BUG-22: trước đây không có tích hợp Firebase SDK thật — gửi lên backend một
 * crypto.randomUUID() giả danh "fcmToken", Firebase Admin SDK phía backend chắc chắn
 * từ chối token này nên push không bao giờ hoạt động cho bất kỳ ai. Giờ dùng
 * firebase/messaging thật: getToken() với VAPID key + service worker riêng cho FCM.
 */
@Injectable({ providedIn: 'root' })
export class PushSubscriptionService {
  private readonly api = inject(ApiService);
  private app: FirebaseApp | null = null;
  private messaging: Messaging | null = null;

  async register(): Promise<void> {
    if (!('serviceWorker' in navigator) || !('PushManager' in window)) return;
    if (!environment.firebase?.apiKey || !environment.firebase?.vapidKey) {
      // Chưa cấu hình Firebase project — bỏ qua thay vì gửi token giả lên backend.
      return;
    }

    try {
      const permission = await Notification.requestPermission();
      if (permission !== 'granted') return;

      const swRegistration = await navigator.serviceWorker.register('/firebase-messaging-sw.js', {
        scope: '/firebase-cloud-messaging-push-scope',
      });

      const messaging = this.getMessagingInstance();
      const token = await getToken(messaging, {
        vapidKey: environment.firebase.vapidKey,
        serviceWorkerRegistration: swRegistration,
      });

      if (!token) return;

      onMessage(messaging, () => {
        // Foreground push — UI hiện tại đã có polling/refresh riêng cho dữ liệu,
        // ở đây chỉ cần không throw để không phá luồng.
      });

      await firstValueFrom(
        this.api.post<void>('notifications/push-subscribe', {
          fcmToken: token,
          deviceType: this.getDeviceType(),
        })
      );
    } catch {
      // Push subscription là optional — lỗi (bị chặn permission, browser không hỗ trợ, ...) bỏ qua âm thầm.
    }
  }

  async unregister(subscriptionId: string): Promise<void> {
    try {
      await firstValueFrom(
        this.api.delete(`notifications/push-subscribe/${subscriptionId}`)
      );
    } catch { /* silent */ }
  }

  private getMessagingInstance(): Messaging {
    if (!this.messaging) {
      this.app = initializeApp(environment.firebase);
      this.messaging = getMessaging(this.app);
    }
    return this.messaging;
  }

  private getDeviceType(): string {
    const ua = navigator.userAgent;
    if (/android/i.test(ua)) return 'android';
    if (/iphone|ipad|ipod/i.test(ua)) return 'ios';
    return 'web';
  }
}
