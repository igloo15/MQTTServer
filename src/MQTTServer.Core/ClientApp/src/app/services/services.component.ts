import { Component, Inject, OnDestroy } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { TimerObservable } from "rxjs/observable/TimerObservable";
import 'rxjs/add/operator/takeWhile';
import 'rxjs/add/operator/skipWhile';

@Component({
  selector: 'app-service-data',
  templateUrl: './services.component.html'
})
export class ServicesComponent implements OnDestroy {
  public services: ServiceStatus[];
  public alive: boolean;
  public interval: number;
  private stop: boolean;
  private internalBaseUrl: string;
  private internalHttp: HttpClient;


  constructor(http: HttpClient, @Inject('BASE_URL') baseUrl: string) {
    this.internalBaseUrl = baseUrl;
    this.internalHttp = http;
    this.interval = 5000;
    this.alive = true;
    this.stop = false;
    this.GetData(this);
  }

  private GetData(self: ServicesComponent) {
    if (self.stop)
      return;

    if (!self.alive)
      setTimeout(() => self.GetData(self), self.interval);
    else {
      self.internalHttp.get<ServiceStatus[]>(self.internalBaseUrl + 'api/Service').subscribe(result => {
        self.services = result;
        setTimeout(() => self.GetData(self), self.interval);
      });
    }
  }

  ngOnDestroy() {
    this.stop = true;
  }
}

interface ServiceStatus {
  clientId: string;
  endpoint: string;
  timeConnected: string;
  isConnected: boolean;
  protocolVersion: string;
  timeSinceLastMessage: number;
  timeSinceLastNonKeepAlive: number;
  pendingMessages: number;
}

