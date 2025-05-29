import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { environment } from 'src/environments/environment';
import { BehaviorSubject } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  token: any;
  server: any;
  host: any;
  firstLogin:any;
  private data = new BehaviorSubject('default data');
  data$ = this.data.asObservable();

  private notificationdata = new BehaviorSubject('notificationcount');
  notificationdata$ = this.notificationdata.asObservable();

  constructor(private router: Router, private http: HttpClient) {
    this.server = environment.protocol + '://' + environment.host;
    this.host = environment.host;
  }

  setToken(token: string, type: string): void {
    sessionStorage.setItem('tkn', token);
    sessionStorage.setItem('tkt', type);
  }

  getToken(): string | null {
    return sessionStorage.getItem('tkn');
  }

  getTokenType(): string | null {
    return sessionStorage.getItem('tkt');
  }

  isLoggedIn() {
    return this.getToken() !== null;
  }

  removeToken() {
    sessionStorage.removeItem('tkn');
    sessionStorage.removeItem('tkt');
  }

  login(url: any, requestBody: any) {
    let promise = new Promise((resolve, reject) => {
      this.http
        .post<any>(this.server + url, requestBody)
        .toPromise()
        .then(
          (data) => {
            resolve(data);
            console.log(data);
          },
          (err) => {
            reject(err);
          }
        );
    });
    return promise;
  }

  logout(url: any) {
    let headers = new HttpHeaders();
    this.token = this.getToken();
    headers = headers.append('Bearer', this.token);
    let promise = new Promise((resolve, reject) => {
      this.http
        .post<any>(this.server + url, null, { headers: headers })
        .toPromise()
        .then(
          (data) => {
            this.removeToken();
            sessionStorage.clear();
            sessionStorage.clear();
            this.router.navigate(['/login']);
            resolve(data);
          },
          (err) => {
            console.log(err);
            if (err.status == 401) {
              this.unauthorized();
            }
            this.removeToken();
            sessionStorage.clear();
            sessionStorage.clear();
            this.router.navigate(['/login']);
            reject();
          }
        );
    });
    return promise;
  }
  resetOtp(url: any) {
    let promise = new Promise((resolve, reject) => {
      this.http
        .get<any>(this.server + url)
        .toPromise()
        .then(
          (data) => {
            resolve(data);
            console.log(data);
          },
          (err) => {
            reject(err);
          }
        );
    });
    return promise;
  }

  getuserVrify(url: string, requestBody: any) {
    let headers = new HttpHeaders();
    this.token = this.getToken();
    headers = headers.append('Bearer', this.token);
    let promise = new Promise((resolve, reject) => {
      this.http
        .post<any>(this.server + url, requestBody, { headers: headers })
        .toPromise()
        .then(
          (res) => {
            // Success
            resolve(res);
          },
          (err) => {
            reject(err);
          }
        );
    });
    return promise;
  }
  unauthorizedOTP() {
    this.removeToken();
    sessionStorage.clear();
  }
  unauthorized() {
    this.removeToken();
    sessionStorage.clear();
    this.router.navigate(['/login']);
    window.location.reload();
    var url = '/api/authorization/logout';
    let headers = new HttpHeaders();
    this.token = this.getToken();
    headers = headers.append('Bearer', this.token);
    let promise = new Promise((resolve, reject) => {
      this.http
        .post<any>(this.server + url, null, { headers: headers })
        .toPromise()
        .then(
          (data) => {
            this.removeToken();
            sessionStorage.clear();
            sessionStorage.clear();
            this.router.navigate(['/login']);
            resolve(data);
          },
          (err) => {
            // console.log(err)

            this.removeToken();
            sessionStorage.clear();
            sessionStorage.clear();
            this.router.navigate(['/login']);
            reject();
          }
        );
    });
    return promise;
  }

  reloadPatientList(data: string) {
    this.data.next(data);
  }

  reloadnotification(data: any) {
    this.notificationdata.next(data);
  }
  get_environment() {
    return this.host;
  }

  ConvertToUTCRangeInput(data: any) {
    var combineDateIsoFrm = new Date(data.toUTCString()).toISOString();

    return combineDateIsoFrm;
  }

  ConvertToUTCRangeInputs(data: any) {
    const dateObj = new Date(data); // Ensures it's a Date instance
    const combineDateIsoFrm = dateObj.toISOString(); // Already in UTC
    return combineDateIsoFrm;
  }
  
}
