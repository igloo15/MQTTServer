import { Component, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-service-data',
  templateUrl: './services.component.html'
})
export class ServicesComponent {
  public services: Service[];
  private internalBaseUrl: string;
  private internalHttp: HttpClient;

  constructor(http: HttpClient, @Inject('BASE_URL') baseUrl: string) {
    this.internalBaseUrl = baseUrl;
    this.internalHttp = http;
    this.RefreshData();
  }

  public RefreshData() {
    this.internalHttp.get<Service[]>(this.internalBaseUrl + 'api/Service').subscribe(result => {
      this.services = result;
    }, error => console.error(error));
  }
}

interface Service {
  name: string;
  subscriptions: string[];
  connected: string;
  endPoint: string;
}

