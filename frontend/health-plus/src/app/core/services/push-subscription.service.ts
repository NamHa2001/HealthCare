import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { ApiService } from './api.service';

export interface PushSubscribeRequest {
  fcmToken: string;
  deviceType: string;
}

@Injectable({ providedIn: 'root' })
export class PushSubscriptionService {
  private readonly api = inject(ApiService);

  async register(): Promise<void> {
    if (!('serviceWorker' in navigator) || !('PushManager' in window)) return;

    try {
      const registration = await navigator.serviceWorker.ready;
      const permission = await Notification.requestPermission();
      if (permission !== 'granted') return;

      // Generate a stable device token based on browser fingerprint
      const token = await this.getDeviceToken(registration);
      if (!token) return;

      await firstValueFrom(
        this.api.post<void>('notifications/push-subscribe', {
          fcmToken: token,
          deviceType: this.getDeviceType(),
        })
      );
    } catch {
      // Push subscription is optional — silently fail
    }
  }

  async unregister(subscriptionId: string): Promise<void> {
    try {
      await firstValueFrom(
        this.api.delete(`notifications/push-subscribe/${subscriptionId}`)
      );
    } catch { /* silent */ }
  }

  private async getDeviceToken(registration: ServiceWorkerRegistration): Promise<string | null> {
    try {
      // Use existing push subscription endpoint or derive from subscription
      const sub = await registration.pushManager.getSubscription();
      if (sub) return btoa(sub.endpoint).slice(0, 255);

      // No existing subscription — generate a stable browser ID as fallback
      const stored = localStorage.getItem('hp_device_token');
      if (stored) return stored;

      const id = crypto.randomUUID();
      localStorage.setItem('hp_device_token', id);
      return id;
    } catch {
      return null;
    }
  }

  private getDeviceType(): string {
    const ua = navigator.userAgent;
    if (/android/i.test(ua)) return 'android';
    if (/iphone|ipad|ipod/i.test(ua)) return 'ios';
    return 'web';
  }
}
