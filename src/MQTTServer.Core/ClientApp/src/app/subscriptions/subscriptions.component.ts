import { Component, Inject, OnDestroy } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { TimerObservable } from "rxjs/observable/TimerObservable";
import 'rxjs/add/operator/takeWhile';
import 'rxjs/add/operator/skipWhile';

@Component({
  selector: 'app-service-data',
  templateUrl: './subscriptions.component.html'
})
export class SubscriptionsComponent implements OnDestroy {
  public services: ServiceSubscription[];
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

  private GetData(self: SubscriptionsComponent) {
    if (self.stop)
      return;

    if (!self.alive)
      setTimeout(() => self.GetData(self), self.interval);
    else {
      self.internalHttp.get<ServiceSubscription[]>(self.internalBaseUrl + 'api/Diag/Subscriptions').subscribe(result => {
        self.services = result;
        setTimeout(() => self.GetData(self), self.interval);
      });
    }
  }

  ngOnDestroy() {
    this.stop = true;
  }
}

interface ServiceSubscription {
  name: string;
  subscriptions: string[];
}

