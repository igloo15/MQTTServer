import { Component, Inject, AfterViewChecked, ElementRef, ViewChild, OnInit, OnDestroy  } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from "rxjs";
import { TimerObservable } from "rxjs/observable/TimerObservable";
import 'rxjs/add/operator/takeWhile';
import 'rxjs/add/operator/skipWhile';

@Component({
  selector: 'app-message-data',
  templateUrl: './messages.component.html',
  styleUrls: ['./messages.component.css']
})
export class MessagesComponent implements OnDestroy {
  @ViewChild('scrollMe') private myScrollContainer: ElementRef;
  public messages: Message[];
  private internalBaseUrl: string;
  private internalHttp: HttpClient;
  public interval: number;
  public alive: boolean;
  public activeScroll: boolean;
  private stop: boolean;


  constructor(http: HttpClient, @Inject('BASE_URL') baseUrl: string) {
    this.internalBaseUrl = baseUrl;
    this.internalHttp = http;
    this.alive = true;
    this.activeScroll = true;
    this.stop = false;
    this.interval = 5000;
    this.GetData(this);
  }

  private GetData(self: MessagesComponent) {
    if (self.stop)
      return;

    if (!this.alive)
      setTimeout(() => self.GetData(self), self.interval);
    else {
      self.internalHttp.get<Message[]>(self.internalBaseUrl + 'api/Message').subscribe(result => {
        self.messages = result;

        if (self.activeScroll)
          self.scrollToBottom();

        setTimeout(() => self.GetData(self), self.interval);
      });
    }
  }
   
  private scrollToBottom(): void {
    try {
      this.myScrollContainer.nativeElement.scrollTop = this.myScrollContainer.nativeElement.scrollHeight+100;
    } catch (err) { }
  }

  ngOnDestroy() {

  }
}

interface Message {
  serviceName: string;
  time: string;
  topic: string;
}
