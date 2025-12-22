import { Component, ElementRef, Input, OnInit, ViewChild } from '@angular/core';
import { NavigationEnd, Router } from '@angular/router';

@Component({
  selector: 'app-menu-item',
  templateUrl: './menu-item.component.html',
  styleUrls: ['./menu-item.component.scss'],
})
export class MenuItemComponent implements OnInit {
  isActive = false;
  currentPath: string = '';
  @Input() backgroundImage: string = '';
  @Input() menuItem: any; // Receives menu item data

  @Input() customClass: string = '';
  constructor(private router: Router) {}
  ngOnInit(): void {
    this.router.events.subscribe((event) => {
      if (event instanceof NavigationEnd) {
        this.currentPath = event.url;
      }
    });
    this.currentPath = this.router.url;
  }

  checkActiveRoute(menuItemPath: string) {
    this.currentPath = menuItemPath;
    this.activeMainMenu( this.currentPath);
  
  }

  isActiveMenu(menuItemPath: string): boolean {
    return this.currentPath === menuItemPath;
  }
  activeMainMenu(value: any) {
    if (value == '/admin/patient-vitals/programInfo') {
      sessionStorage.setItem('ActiveMainMenu', 'ProgramInfo');
    } else if (value == '/admin/patient-vitals/clinicInfo') {
      sessionStorage.setItem('ActiveMainMenu', 'ClinicalInfo');
    }
  }
}
