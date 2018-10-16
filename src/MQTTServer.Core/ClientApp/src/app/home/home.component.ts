import { Component, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from "rxjs";
import { TimerObservable } from "rxjs/observable/TimerObservable";
import 'rxjs/add/operator/takeWhile';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html'
})
export class HomeComponent {
  public diag: Diagnostic;
  private internalBaseUrl: string;
  private internalHttp: HttpClient;
  private interval: number;
  public alive: boolean;


  constructor(http: HttpClient, @Inject('BASE_URL') baseUrl: string) {
    this.internalBaseUrl = baseUrl;
    this.internalHttp = http;
    this.alive = true;
    this.interval = 5000;
    this.RefreshData();
  }

  public RefreshData() {
    TimerObservable.create(0, this.interval)
      .takeWhile(() => this.alive)
      .subscribe(() => {
        this.internalHttp.get<Diagnostic>(this.internalBaseUrl + 'api/Diag').subscribe(result => {
          this.diag = result;
        }, error => console.error(error));
      });

  }
}

interface Diagnostic {
  numClients: number;
  numSubscriptions: number;
  numMessages: number;
  messagesPerMinute: number;
}
