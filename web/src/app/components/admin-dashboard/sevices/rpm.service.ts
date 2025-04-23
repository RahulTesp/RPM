import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { environment } from 'src/environments/environment';
import { AuthService } from 'src/app/services/auth.service';

@Injectable({
  providedIn: 'root',
})
export class RPMService {
  SERVER_URL: any;
  token: any;

  constructor(private http: HttpClient, private auth: AuthService) {
    this.SERVER_URL = environment.protocol + '://' + environment.host;
  }
  rpm_get(url: string) {
    let headers = new HttpHeaders();
    this.token = this.auth.getToken();
    headers = headers.append('Bearer', this.token);
    let promise = new Promise((resolve, reject) => {
      this.http
        .get<any>(this.SERVER_URL + url, { headers: headers })
        .toPromise()
        .then(
          (data) => {
            resolve(data);
          },
          (err) => {
            if (err.status == 401) {
              this.auth.unauthorized();
              //window.location.reload();
            }
            reject(err);
          }
        );
    });
    return promise;
  }

  rpm_put(url: string, params: any) {
    let headers = new HttpHeaders();
    this.token = this.auth.getToken();
    headers = headers.append('Bearer', this.token);
    let promise = new Promise((resolve, reject) => {
      this.http
        .put<any>(this.SERVER_URL + url, params, { headers: headers })
        .toPromise()
        .then(
          (data) => {
            resolve(data);
          },
          (err) => {
            console.log(err);
            if (err.status == 401) {
              this.auth.unauthorized();
              //window.location.reload();
            }
            reject();
          }
        );
    });
    return promise;
  }
  rpm_post(url: any, params: any) {
    let headers = new HttpHeaders();
    this.token = this.auth.getToken();
    headers = headers.append('Bearer', this.token);
    let promise = new Promise((resolve, reject) => {
      this.http
        .post<any>(this.SERVER_URL + url, params, { headers: headers })
        .toPromise()
        .then(
          (data) => {
            resolve(data);
          },
          (err) => {
            console.log(err);
            if (err.status == 401) {
              this.auth.unauthorized();
              //window.location.reload();
            }
            reject(err);
          }
        );
    });
    return promise;
  }
}
