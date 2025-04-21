import { Component } from '@angular/core';
import { Event,Router,NavigationStart,NavigationEnd, RouterEvent } from '@angular/router';
@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent {
  title = 'Remote Patient Monitoring';
  loading = true;
  constructor(private _router :Router){
    
    this._router.events.subscribe((RouterEvent:Event)=>{
      if(RouterEvent instanceof NavigationStart){
        this.loading=true;
      }
      if(RouterEvent instanceof NavigationEnd){
        this.loading=false;
      }
    })
  }
  
 
}
