// Case 01 — review the design of this change (new files added in one PR).

// notifications/sender.ts
export interface NotificationSender {
  sendEmail(to: string, subject: string, body: string): Promise<void>;
  sendSms(to: string, body: string): Promise<void>;
  sendPush(deviceToken: string, title: string, body: string): Promise<void>;
}

// notifications/sendgrid-sender.ts
import sendgrid from "./sendgrid-client";
import twilio from "./twilio-client";
import firebase from "./firebase-client";

export class DefaultNotificationSender implements NotificationSender {
  sendEmail(to: string, subject: string, body: string): Promise<void> {
    return sendgrid.send({ to, subject, body });
  }
  sendSms(to: string, body: string): Promise<void> {
    return twilio.messages.create({ to, body });
  }
  sendPush(deviceToken: string, title: string, body: string): Promise<void> {
    return firebase.messaging().send({ token: deviceToken, notification: { title, body } });
  }
}

// notifications/factory.ts
import { NotificationSender } from "./sender";
import { DefaultNotificationSender } from "./sendgrid-sender";

let instance: NotificationSender | undefined;

export function getNotificationSender(): NotificationSender {
  if (!instance) {
    instance = new DefaultNotificationSender();
  }
  return instance;
}

// signup/welcome.ts — the only consumer
import { getNotificationSender } from "../notifications/factory";

export async function sendWelcomeEmail(email: string, name: string): Promise<void> {
  await getNotificationSender().sendEmail(
    email,
    "Welcome!",
    `Hi ${name}, thanks for signing up.`,
  );
}
