import { Injectable } from '@angular/core';
import { home, userprofile, worklist, listener, AddPatientMasterData, CityAndStatesMasterData } from '../models/data-models'
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { AuthService } from 'src/app/services/auth.service';

@Injectable({
  providedIn: 'root'
})
export class HttpService {
  HTTP_HEADER:any;
  DOMAIN_URL:any;
//models to be added here
  home_data:home;
  userprofile_data:userprofile;
  worklist_data:worklist;
  listener_data:listener;
  AddPatientMasterData:AddPatientMasterData;
  CityAndStatesMasterData : CityAndStatesMasterData;
  token: any;
  constructor(private http: HttpClient, private auth:AuthService) { 
    this.HTTP_HEADER = sessionStorage.getItem("key");
    this.DOMAIN_URL = "https://rpmwebapp.azurewebsites.net";
  }
 
  HTTP_GET(URL:string) {
    let headers = new HttpHeaders();
    this.token = this.auth.getToken();
    headers = headers.append('Bearer', this.token);
    let promise = new Promise((resolve, reject)=>{
      this.http.get<any>(this.DOMAIN_URL+URL, { headers: headers })
      .toPromise()
      .then(
        data => {
            this.SAVE_DATA(data,URL);
            resolve(data);
        },
        err=>{
            reject();
        }
        
      )
    });
    return promise;
  }
  get_data(url: string): Observable<any> {
    
    this.http.get<any>(this.DOMAIN_URL+url).subscribe((data)=>{
      return data;
    },
    (err)=>{
      return throwError(new Error('No Response'));
    })
    return throwError(new Error('No Response'));
  
  }
  HTTP_PUT(URL_NAME:any, REQ_DATA:any){

  }
  HTTP_POST(URL_NAME:any, REQ_DATA:any){
    let promise = new Promise((resolve, reject)=>{
      this.http.get<any>(this.DOMAIN_URL+URL_NAME, REQ_DATA)
      .toPromise()
      .then(
        data => {
            this.SAVE_DATA(data,URL);
            resolve(data);
        },
        err=>{
            reject();
        }
        
      )
    });
    return promise;

  }
  SAVE_DATA(data:any, url:any){
    //urls to be added here and assign to model
    switch(url){
      case '/api/home':
        this.home_data=data;
        break;
      case '/api/profile':
        this.userprofile_data=data;
        break;
      case '/api/worklist':
        this.worklist_data=data;
        break;
      case '/api/listener':
        this.listener_data=data;
        break;
      case '/api/patient/getprogramdetailsmasterdataaddpatient':
        this.AddPatientMasterData=data;
        break;
      case '/api/authorization/masterdatastatesandcities':
        this.CityAndStatesMasterData=data;
        break;
    }
  }
}
