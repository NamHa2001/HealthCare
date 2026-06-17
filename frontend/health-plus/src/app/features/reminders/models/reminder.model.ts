export type ReminderStatus = 'pending' | 'sent' | 'failed' | 'cancelled';
export type ReminderType = 'vaccine' | 'medication' | 'appointment' | 'custom';

export interface Reminder {
  id: string;
  type: ReminderType;
  title: string;
  body: string | null;
  remindAt: string;
  status: ReminderStatus;
  referenceId: string | null;
  sentAt: string | null;
  createdAt: string;
}

export interface CreateReminderRequest {
  type: ReminderType;
  title: string;
  body?: string | null;
  remindAt: string;
  referenceId?: string | null;
}
