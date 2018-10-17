import { Component, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-nav-menu',
  templateUrl: './nav-menu.component.html',
  styleUrls: ['./nav-menu.component.css']
})
export class NavMenuComponent {
  isExpanded = false;
  private internalHttp: HttpClient;
  private internalUrl: string;
  public version: string;

  constructor(http: HttpClient, @Inject('BASE_URL') baseUrl: string) {
    this.internalHttp = http;
    this.internalUrl = baseUrl;
    this.getVersion();
  }

  getVersion() {
    this.internalHttp.get<string>(this.internalUrl + 'api/Diag/Version').subscribe(result => {

      this.version = result;

    }, error => console.error(error));
  }

  collapse() {
    this.isExpanded = false;
  }

  toggle() {
    this.isExpanded = !this.isExpanded;
  }
}
