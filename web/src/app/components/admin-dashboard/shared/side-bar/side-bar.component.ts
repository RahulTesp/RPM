import { Component, EventEmitter, OnInit, Output, TemplateRef } from '@angular/core';
import { AuthService } from 'src/app/services/auth.service';
import { MatDialog, MatDialogConfig } from '@angular/material/dialog';
import { Router } from '@angular/router';
import { RPMService } from '../../sevices/rpm.service';
import {
  FormControl,
  FormGroup,
  Validators,
  FormBuilder,
} from '@angular/forms';
import { StatusMessageComponent } from '../../shared/status-message/status-message.component';

import { BreakpointObserver } from '@angular/cdk/layout';

@Component({
  selector: 'app-side-bar',
  templateUrl: './side-bar.component.html',
  styleUrls: ['./side-bar.component.scss'],
})
export class SideBarComponent implements OnInit {
  http_user_data: any;
  username: any;
  newpw: any;
  passwordForm: FormGroup;
  submitted: boolean = false;
  result: any;
  password: any;

  user_name: any;
  roles: any;
  access: any;
  isAdmin: any;
  FirstLetter: any;
  Lastletter: any;
  profileName = '';

  AdminVar: any;
  // activePageMainMenu: any;
  activePage: any;
  @Output() emitter: EventEmitter<string> = new EventEmitter<string>();

  menuItems = [
    {
      id: 'menuhome',
      title: 'Home',
      icon: '/assets/04-Icons/Icons_Home.svg',
      activeIcon: '/assets/04-Icons/Icons_Home A.svg',
      route: '/admin/home',
    },
    {
      id: 'menuPatients',
      title: 'Patients',
      icon: 'assets/04-Icons/Icons_Patients.svg',
      activeIcon: '/assets/04-Icons/Icons_Patients A.svg',
      route: '/admin/patients',
    },
    {
      id: 'menuworklist',
      title: 'Work List',
      icon: 'assets/04-Icons/Icons_Work List.svg',
      activeIcon: '/assets/04-Icons/Icons_Work List A.svg',
      route: '/admin/task',
    },
    {
      id: 'menuteam',
      title: 'Teams',
      icon: 'assets/04-Icons/Icons_Teams.svg',
      activeIcon: '/assets/04-Icons/Icons_Teams A.svg',
      route: '/admin/teams',
      hideForRoles: [6, 8], // Hide for Physician role (Id = 3)
    },
    {
      id: 'menuDevice',
      title: 'Device',
      icon: 'assets/04-Icons/Icons_Devices.svg',
      activeIcon: '/assets/04-Icons/Icons_Devices A.svg',
      route: '/admin/device',
      hideForRoles: [6, 8],
    },
    {
      id: 'menureport',
      title: 'Reports',
      icon: 'assets/04-Icons/Icons_Reports.svg',
      activeIcon: '/assets/04-Icons/Icons_Reports A.svg',
      route: '/admin/report',
    },
    {
      id: 'menuadmin',
      title: 'Admin',
      icon: 'assets/04-Icons/Icons_Admin.svg',
      activeIcon: '/assets/04-Icons/Icons_Admin A.svg',
      route: '/admin/admin',
      hideForRoles: [6, 8,4],
    },
    // Patient Login Menu
    {
      id: 'menupatienthome',
      title: 'Home',
      icon: 'assets/04-Icons/Icons_Home.svg',
      activeIcon: '/assets/04-Icons/Icons_Home A.svg',
      route: '/admin/patient-home',
      patientOnly: true,
    },
    {
      id: 'menuprogram',
      title: 'Program',
      icon: 'assets/04-Icons/Icons_Patients.svg',
      route: '/admin/patient-vitals/programInfo',
      activeIcon: '/assets/04-Icons/Icons_Patients A.svg',
      patientOnly: true,
    },
    {
      id: 'menutodo',
      title: 'Todo List',
      icon: 'assets/04-Icons/Icons_Notes.svg',
      activeIcon: '/assets/04-Icons/Icons_Notes A.svg',
      route: '/admin/patient-todo',
      patientOnly: true,
    },
    {
      id: 'menuvitals',
      title: 'Vitals',
      icon: 'assets/04-Icons/Icons_Vitals.svg',
      route: '/admin/patient-vitals/clinicInfo',
      activeIcon: '/assets/04-Icons/Icons_Vitals.svg',
      patientOnly: true,
    },
  ];

  host: any;
  constructor(
    private auth: AuthService,
    private observer: BreakpointObserver,
    public dialog: MatDialog,
    private router: Router,
    private rpm: RPMService,
    private formBuilder: FormBuilder
  ) {
    var that = this;
    this.host = this.auth.get_environment();
    // this.activePageMainMenu = sessionStorage.getItem('ActiveMainMenu');
    this.roles = sessionStorage.getItem('Roles');
    this.roles = JSON.parse(this.roles);
    that.rpm
      .rpm_get('/api/authorization/useraccessrights?RoleId=' + that.roles[0].Id)
      .then((data) => {
        that.access = data;
        if (!that.access.UserAccessRights) {
          this.AdminVar == 'N';
        } else if (that.access.UserAccessRights < 1) {
          this.AdminVar == 'N';
        } else {
          that.isAdmin = that.access.UserAccessRights.filter(function (
            data: any
          ) {
            return data.AccessName == 'AdminAccess';
          });
          if (!that.isAdmin) {
            this.AdminVar == 'N';
          } else if (this.isAdmin.length == 1) {
            this.AdminVar = this.isAdmin[0].AccessRight;
          } else {
            this.AdminVar == 'N   ';
          }
        }
      });

    this.user_name = sessionStorage.getItem('user_name');
  }

  get filteredMenuItems() {
    return this.menuItems.filter((item) => {
      const isPatientMenu = item.patientOnly && this.roles[0].Id === 7;
      const isAdminOrStaffMenu = !item.patientOnly && this.roles[0].Id !== 7;
      const isRestricted = item.hideForRoles?.includes(this.roles[0].Id);

      return (isPatientMenu || isAdminOrStaffMenu) && !isRestricted;
    });
  }

  myprofile: any;
  get f() {
    return this.passwordForm.controls;
  }

  small_screen = true;

  ngAfterViewInit() {
    this.observer.observe(['(max-width: 1280px)']).subscribe((res) => {
      this.small_screen = false;
    });
  }

  isOpen = false;

  value: any;
  ngOnInit(): void {
    // this.activePageMainMenu = sessionStorage.getItem('ActiveMainMenu');
  }
  onDestroy() {
    sessionStorage.clear();
  }

  displayvar = false;
  display() {
    this.displayvar = !this.displayvar;
  }

  profileImg = '../../../../../assets/04-Icons/profilepic.jpg';
  profile() {
    this.router.navigate(['/admin/myprofile']);
  }
  feedback() {
    this.router.navigate(['/admin/feedback']);
  }

  openDialog(templateRef: TemplateRef<any>) {
    this.dialog.open(templateRef);
  }

  openDialog1(title: any, item: any) {
    const dialogConfig = new MatDialogConfig();
    dialogConfig.width = '400px';

    // dialogConfig.disableClose = true;
    dialogConfig.autoFocus = true;

    dialogConfig.data = {
      title: title,
      item: item,
    };

    this.dialog.open(StatusMessageComponent, dialogConfig);
  }
  // activeMainMenu(value: any) {
  //   if (value == 1) {
  //     sessionStorage.setItem('ActiveMainMenu', 'ProgramInfo');
  //     this.activePageMainMenu = 'currentActive1';
  //   } else if (value == 2) {
  //     sessionStorage.setItem('ActiveMainMenu', 'ClinicalInfo');
  //     this.activePageMainMenu = 'currentActive2';
  //   }
  // }

  smallScreenOpenProfile() {
    this.profile();
  }

   toggleMenu() {
    this.emitter.emit('out');
  }

}
