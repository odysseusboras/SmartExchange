import { Injectable } from '@angular/core';
import {
  HubConnection,
  HubConnectionBuilder,
  LogLevel,
} from '@microsoft/signalr';
import { Subject } from 'rxjs';
import { environment } from '../environments/environment';

export enum ConnectionStatus {
  Connecting = 'Connecting',
  Connected = 'Connected',
  Disconnected = 'Disconnected',
}

@Injectable({
  providedIn: 'root',
})
export class SignalRService {
  private hubConnection!: HubConnection;
  public messages$ = new Subject<any>();
  public connectionStatus$ = new Subject<ConnectionStatus>();

  constructor() {
    this.startConnection();
  }

  private startConnection() {
    const hubUrl = environment.hubUrl;

    this.hubConnection = new HubConnectionBuilder()
      .withUrl(hubUrl, { withCredentials: true })
      .withAutomaticReconnect([0, 2000, 10000, 30000])
      .configureLogging(LogLevel.Information)
      .build();

    this.Connect();

    this.hubConnection.onreconnecting((err) => {
      console.log('Reconnecting...', err);
      this.connectionStatus$.next(ConnectionStatus.Connecting);
    });

    this.hubConnection.onreconnected((connectionId) => {
      console.log('Reconnected. Connection ID:', connectionId);
      this.connectionStatus$.next(ConnectionStatus.Connected);
    });

    this.hubConnection.onclose((error) => {
      console.error('Connection closed. Attempting to reconnect...', error);
      this.connectionStatus$.next(ConnectionStatus.Disconnected);
      this.Connect();
    });

    this.hubConnection.on('ReceiveMessage', (message: any) => {
      this.messages$.next(message);
    });
  }

  private Connect() {
    this.connectionStatus$.next(ConnectionStatus.Connecting);
    this.hubConnection
      .start()
      .then(() => {
        console.log('Connected successfully');
        this.connectionStatus$.next(ConnectionStatus.Connected);
      })
      .catch((err) => {
        console.error('Error while trying to reconnect: ', err);
        this.connectionStatus$.next(ConnectionStatus.Disconnected);
        setTimeout(() => {
          this.Connect();
        }, 1000);
      });
  }
}
