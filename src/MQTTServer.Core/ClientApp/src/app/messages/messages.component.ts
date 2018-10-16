import { Component, Inject, AfterViewChecked, ElementRef, ViewChild, OnInit  } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from "rxjs";
import { TimerObservable } from "rxjs/observable/TimerObservable";
import 'rxjs/add/operator/takeWhile';

@Component({
  selector: 'app-message-data',
  templateUrl: './messages.component.html',
  styleUrls: ['./messages.component.css']
})
export class MessagesComponent {
  @ViewChild('scrollMe') private myScrollContainer: ElementRef;
  public messages: Message[];
  private internalBaseUrl: string;
  private internalHttp: HttpClient;
  private interval: number;
  public alive: boolean;
  public activeScroll: boolean;


  constructor(http: HttpClient, @Inject('BASE_URL') baseUrl: string) {
    this.internalBaseUrl = baseUrl;
    this.internalHttp = http;
    this.alive = true;
    this.activeScroll = true;
    this.interval = 5000;
    this.RefreshData();
  }

  public RefreshData() {
    TimerObservable.create(0, this.interval)
      .takeWhile(() => this.alive)
      .subscribe(() => {
        this.internalHttp.get<Message[]>(this.internalBaseUrl + 'api/Message').subscribe(result => {
          this.messages = result;
          if (this.activeScroll)
            this.scrollToBottom();
        }, error => console.error(error));
      });
    
  }


  private scrollToBottom(): void {
    try {
      this.myScrollContainer.nativeElement.scrollTop = this.myScrollContainer.nativeElement.scrollHeight+100;
    } catch (err) { }
  }
}

interface Message {
  serviceName: string;
  time: string;
  topic: string;
}
