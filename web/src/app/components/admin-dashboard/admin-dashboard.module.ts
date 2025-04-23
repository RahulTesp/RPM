import { Component, NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AdminDashboardRoutingModule } from './admin-dashboard-routing.module';
import { HeaderComponent } from './shared/header/header.component';
import { SideBarComponent } from './shared/side-bar/side-bar.component';
import { DashboardComponent } from './components/dashboard_module/dashboard.component';
import { provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';
import { OverlayModule } from '@angular/cdk/overlay';
import { MaterialModule } from '../../../app/material/angular-material.module';
import { HomeComponent } from './components/home/home.component';
import { RightSidebarComponent } from './shared/right-sidebar/right-sidebar.component';
import { RightCommonButtonComponent } from './shared/right-common-button/right-common-button.component';
import { TaskComponent } from './components/task/task.component';
import { AdminComponent } from './components/admin/admin.component';
import { PatientPageComponent } from './components/patient-page/patient-page.component';
import { PatientDetailPageComponent } from './components/patient-detail-page/patient-detail-page.component';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { AddUserComponent } from './components/add-user/add-user.component';
import { AddpatientComponent } from './components/addpatient/addpatient.component';
import { HttpService } from './sevices/http.service';
import { NgChartsModule } from 'ng2-charts';
import { ReportsComponent } from './components/reports/reports.component';
import { NotificationComponent } from './components/notification/notification.component';
// import { NgxSliderModule } from '@angular-slider/ngx-slider';

import { SidePanelPatientComponent } from './shared/side-panel-patient/side-panel-patient.component';
import { BnNgIdleService } from 'bn-ng-idle';
import { SliderdataComponent } from '../../components/admin-dashboard/shared/sliderdata/sliderdata.component';
import { TeamPageComponent } from './components/team-page/team-page.component';
import { TeamDetailComponent } from './components/team-detail/team-detail.component';
import { NotificationPanelComponent } from './shared/notification-panel/notification-panel.component';
import { SearchFilterPipe } from './components/team-detail/search-filter.pipe';
import { MyprofileComponent } from './components/myprofile/myprofile.component';
import { DatePipe } from '@angular/common';
import { RPMService } from './sevices/rpm.service';
import { EditpatientComponent } from './components/editpatient/editpatient.component';
import { ScrollingModule } from '@angular/cdk/scrolling';
import { StatusMessageComponent } from './shared/status-message/status-message.component';
import { MatSortModule } from '@angular/material/sort';
import { AutocompleteLibModule } from 'angular-ng-autocomplete';
import { ConfirmDialogComponent } from './shared/confirm-dialog/confirm-dialog.component';
import 'hammerjs';
import 'chartjs-plugin-zoom';
import { MenuItemComponent } from './shared/side-bar/models/menu-item/menu-item.component';
import { NgxExtendedPdfViewerModule } from 'ngx-extended-pdf-viewer';
import { DevicePageComponent } from './components/device-page/device-page.component';
import { AmIVisibleDirective } from './directives/am-ivisible.directive';
import { MessagingService } from './sevices/messaging.service';
import { environment } from 'src/environments/environment';
import { AsyncPipe } from '@angular/common';
import { DragDropModule } from '@angular/cdk/drag-drop';
import { StatusDialogBoxComponent } from './shared/side-bar/models/status-dialog-box/status-dialog-box.component';
import { ProfileMenuButtonComponent } from './shared/side-bar/models/profile-menu-button/profile-menu-button.component';
import { PatientDataReportComponent } from './components/reports/components/patient-data-report/patient-data-report.component';
import { BillingDataReportComponent } from './components/reports/components/billing-data-report/billing-data-report.component';
import { CallinfoDataReportComponent } from './components/reports/components/callinfo-data-report/callinfo-data-report.component';
import { ChatbuttonComponent } from './shared/chatbutton/chatbutton.component';
import { BillingSummaryReportComponent } from './components/reports/components/billing-summary-report/billing-summary-report.component';
import { CallNonEstablishedReportComponent } from './components/reports/components/call-non-established-report/call-non-established-report.component';
import { ReportCommonLayoutComponent } from './components/reports/components/report-common-layout/report-common-layout.component';
import { DateRangeControlComponent } from './shared/date-range-control/date-range-control.component';
import { ConfirmDialogPanelComponent } from './shared/confirm-dialog-panel/confirm-dialog-panel.component';
import { PatientDataDetailsService } from './components/patient-detail-page/Models/service/patient-data-details.service';
import { DownloadPatientReportService } from './components/reports/services/download-patient-report.service';
import { PatientHomeComponent } from './components/patientAccount/patient-home/patient-home.component';
import { PatientTodoComponent } from './components/patientAccount/patient-todo/patient-todo.component';
import { PatientVitalsComponent } from './components/patientAccount/patient-vitals/patient-vitals.component';
import { PatientReportComponent } from './components/patientAccount/patient-report/patient-report.component';
import { ProgramRenewComponent } from './components/patient-detail-page/Models/components/program-renew/program-renew.component';
import { DualRangeSliderComponent } from './shared/dual-range-slider/dual-range-slider.component';
import { ActivityinfoChatComponent } from './components/patient-detail-page/Models/components/activityinfo-chat/activityinfo-chat.component';


@NgModule({ declarations: [
        DateRangeControlComponent,
        CallNonEstablishedReportComponent,
        BillingSummaryReportComponent,
        HeaderComponent,
        SideBarComponent,
        DashboardComponent,
        HomeComponent,
        AddpatientComponent,
        RightSidebarComponent,
        RightCommonButtonComponent,
        TaskComponent,
        AdminComponent,
        StatusDialogBoxComponent,
        MenuItemComponent,
        ProfileMenuButtonComponent,
        PatientPageComponent,
        PatientDetailPageComponent,
        AddUserComponent,
        ReportsComponent,
        ReportCommonLayoutComponent,
        NotificationComponent,
        SidePanelPatientComponent,
        SliderdataComponent,
        TeamPageComponent,
        TeamDetailComponent,
        NotificationPanelComponent,
        SearchFilterPipe,
        MyprofileComponent,
        EditpatientComponent,
        StatusMessageComponent,
        ConfirmDialogComponent,
        DevicePageComponent,
        AmIVisibleDirective,
        PatientHomeComponent,
        PatientTodoComponent,
        PatientVitalsComponent,
        ChatbuttonComponent,
        PatientDataReportComponent,
        BillingDataReportComponent,
        CallinfoDataReportComponent,
        ConfirmDialogPanelComponent,
        PatientReportComponent,
        ProgramRenewComponent,
        DualRangeSliderComponent,
        ActivityinfoChatComponent,

    ], imports:
    [CommonModule,
        AdminDashboardRoutingModule,
        MaterialModule,
        OverlayModule,
        FormsModule,
        AutocompleteLibModule,
        ReactiveFormsModule,
        NgChartsModule,
        ScrollingModule,
        MatSortModule,
        NgxExtendedPdfViewerModule,
        DragDropModule,


    ], providers: [
        HttpService,
        BnNgIdleService,
        DatePipe,
        PatientDataDetailsService,
        RPMService,
        DownloadPatientReportService,
        AsyncPipe,
        {
            provide: 'SW_PATH',
            useValue: '/firebase-messaging-sw.js', // Path to your service worker file
        },
        {
            provide: 'NOTIFICATIONS_ENABLED',
            useValue: true, // Enable notifications
        },
        provideHttpClient(withInterceptorsFromDi()),
    ] })
export class AdminDashboardModule {}
