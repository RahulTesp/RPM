import { DevicePageComponent } from './components/device-page/device-page.component';
import { MyprofileComponent } from './components/myprofile/myprofile.component';
import { TeamDetailComponent } from './components/team-detail/team-detail.component';
import { TeamPageComponent } from './components/team-page/team-page.component';
import { ReportsComponent } from './components/reports/reports.component';
import { AddUserComponent } from './components/add-user/add-user.component';
import { PatientPageComponent } from './components/patient-page/patient-page.component';
import { AdminComponent } from './components/admin/admin.component';
import { TaskComponent } from './components/task/task.component';
import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { DashboardComponent } from './components/dashboard_module/dashboard.component';
import { HomeComponent } from './components/home/home.component';
import { AddpatientComponent } from './components/addpatient/addpatient.component';
import { NotificationComponent } from './components/notification/notification.component';
import { PatientDetailPageComponent } from './components/patient-detail-page/patient-detail-page.component';
import { EditpatientComponent } from './components/editpatient/editpatient.component';

import { AuthService } from 'src/app/services/auth.service';
import { PatientTodoComponent } from './components/patientAccount/patient-todo/patient-todo.component';
import { PatientReportComponent } from './components/patientAccount/patient-report/patient-report.component';
import { PatientHomeComponent } from './components/patientAccount/patient-home/patient-home.component';
import { PatientVitalsComponent } from './components/patientAccount/patient-vitals/patient-vitals.component';

const routes: Routes = [
  {
    path: '',
    component: DashboardComponent,
    children: [
      { path: 'home', component: HomeComponent },
      { path: 'addpatient', component: AddpatientComponent },
      { path: 'task', component: TaskComponent },
      { path: 'teams', component: TeamPageComponent },
      { path: 'admin', component: AdminComponent },
      { path: 'patients', component: PatientPageComponent },
      { path: 'patients_detail', component: PatientDetailPageComponent },
      { path: 'user', component: AddUserComponent },
      { path: 'report', component: ReportsComponent },
      { path: 'notification', component: NotificationComponent },
      { path: 'team-detail', component: TeamDetailComponent },
      { path: 'myprofile', component: MyprofileComponent },
      { path: 'editpatient', component: EditpatientComponent },
      { path: 'device', component: DevicePageComponent },
      { path: 'patient-report', component: PatientReportComponent },
      { path: 'patient-todo', component: PatientTodoComponent },
      { path: 'patient-home', component: PatientHomeComponent },
      { path: 'patient-vitals/clinicInfo', component: PatientVitalsComponent },
      { path: 'patient-vitals/programInfo', component: PatientVitalsComponent },
      {
        path: '',
        redirectTo: '/admin/home',
        pathMatch: 'full',
        // canActivate: [AuthService],
      },
      { path: '', redirectTo: '/admin/patient-home', pathMatch: 'full' },
    ],
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class AdminDashboardRoutingModule {}
