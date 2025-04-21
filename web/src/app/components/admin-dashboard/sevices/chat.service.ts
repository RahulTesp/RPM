import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { BehaviorSubject } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class ChatService {
  constructor(private http: HttpClient) {}

  private url = 'https://cx-dev-server.azurewebsites.net/api/comm/getchattoken';
  private url2 =
    'https://cx-dev-server.azurewebsites.net/api/comm/regeneratechattoken';

  userName: BehaviorSubject<string> = new BehaviorSubject('');

  setUsername(text: string) {
    this.userName.next(text);
  }

  getToken(name: string) {

    return this.http.get<string>(`${this.url}`);
  }
  getTToken(name: string) {

    return this.http.get<string>(`${this.url2}`);
  }
}
