import {
  Component,
  OnInit,
  ViewChild,
  ChangeDetectorRef,
  HostListener,
} from '@angular/core';
import { MatDrawer } from '@angular/material/sidenav';
import { BreakpointObserver } from '@angular/cdk/layout';
import {
  Event,
  Router,
  NavigationStart,
  NavigationEnd,
  RouterEvent,
} from '@angular/router';
import { BnNgIdleService } from 'bn-ng-idle';
import { AuthService } from 'src/app/services/auth.service';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss'],
})
export class DashboardComponent implements OnInit {
  @ViewChild(MatDrawer)
  sidenav!: MatDrawer;
  loading = true;
  sessionSubscription: Subscription;

  isInEmployee: boolean;

  constructor(
    private observer: BreakpointObserver,
    private cdRef: ChangeDetectorRef,
    private _router: Router,
    private bnIdle: BnNgIdleService,
    private auth: AuthService
  ) {
    this._router.events.subscribe((RouterEvent: Event) => {
      if (RouterEvent instanceof NavigationStart) {
        this.loading = true;
      }
      if (RouterEvent instanceof NavigationEnd) {
        this.loading = false;
      }
    });
    // this.sessionSubscription = this.bnIdle.startWatching(20).subscribe((res) => {
    //   if(res) {
    //       // this.auth.logout('/api/authorization/logout');
    //       // this.auth.removeToken();
    //       this.auth.unauthorized();
    //       alert("session expired");
    //       console.log(res);
    //   }
    // })

    //  this.bnIdle.startWatching(20).subscribe((res) => {
    //     if(res) {
    //         // this.auth.logout('/api/authorization/logout');
    //         // this.auth.removeToken();
    //         this.auth.unauthorized();
    //         alert("session expired");
    //         console.log(res);
    //     }
    //   })
  }
  // @HostListener('window:unload', [ '$event' ])
  // unloadHandler(event:any) {
  //         this.auth.unauthorized();
  // }

  // @HostListener('window:beforeunload', [ '$event' ])
  // beforeUnloadHandler(event:any) {
  //         this.auth.unauthorized();
  // }

  ngAfterViewInit() {
    setTimeout(
      () =>
        this.observer.observe(['(max-width: 1500px)']).subscribe((res) => {
          if (res.matches) {
            this.sidenav.mode = 'over';
            this.sidenav.close();
          } else {
            this.sidenav.mode = 'side';
            this.sidenav.open();
          }
        }),
      0
    );
  }

  menuState: string = 'in';

  toggleMenu(value: any) {
    this.menuState = value;

    this.menuState = this.menuState === 'out' ? 'in' : 'out';
    if (this.menuState === 'out') {
      this.sidenav.open();
      this.sidenav.mode = 'over';
      this.menuState = 'in';
    } else {
      this.sidenav.close();
      this.sidenav.mode = 'over';
      this.menuState = 'out';
    }
  }

  ngAfterContentChecked() {
    this.cdRef.detectChanges();
  }

  ngOnInit(): void {}
  ngOnDestroy(): void {}
}
