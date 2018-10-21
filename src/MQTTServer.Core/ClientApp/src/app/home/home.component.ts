import { Component, Inject, OnDestroy } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from "rxjs";
import { TimerObservable } from "rxjs/observable/TimerObservable";
import 'rxjs/add/operator/takeWhile';
import 'rxjs/add/operator/skipWhile';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html'
})
export class HomeComponent implements OnDestroy {
  public diags: Diagnostic[];
  private internalBaseUrl: string;
  private internalHttp: HttpClient;
  private stop: boolean;
  public interval: number;
  public alive: boolean;
  public endpoint: string;


  constructor(http: HttpClient, @Inject('BASE_URL') baseUrl: string) {
    this.internalBaseUrl = baseUrl;
    this.internalHttp = http;
    this.alive = true;
    this.stop = false;
    this.interval = 5000;
    this.endpoint = null;
    this.GetEndpoint();
    this.GetData(this);
  }

  public GetEndpoint() {
    this.internalHttp.get(this.internalBaseUrl + 'api/Diag/Endpoint', { responseType: 'text' }).subscribe(result => {
      this.endpoint = result;
    });
  }

  public GetData(self: HomeComponent) {
    if (self.stop)
      return;

    if (!self.alive)
      setTimeout(() => self.GetData(self), self.interval);
    else {
      self.internalHttp.get<Diagnostic[]>(self.internalBaseUrl + 'api/Diag/Status').subscribe(result => {
        self.diags = result;
        setTimeout(() => self.GetData(self), self.interval);
      });
    }
  }

  ngOnDestroy() {
    this.stop = true;
  }
}

interface Diagnostic {
  name: string;
  value: string;
}
