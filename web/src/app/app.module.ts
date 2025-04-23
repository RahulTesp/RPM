import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { LoginComponent } from './components/login/login.component';
import { ForgotPasswordComponent } from './components/forgot-password/forgot-password.component';
import { PageNotFoundComponent } from './components/page-not-found/page-not-found.component';
import { MaterialModule } from './material/angular-material.module';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatGridListModule } from '@angular/material/grid-list';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';
import { NgChartsModule } from 'ng2-charts';
import { CommonModule } from '@angular/common';
import { environment } from '../environments/environment';
import { AngularFireModule } from '@angular/fire/compat';
import { ServiceWorkerModule } from '@angular/service-worker';
import { AngularFireMessagingModule } from '@angular/fire/compat/messaging';
import { MessagingService } from './components/admin-dashboard/sevices/messaging.service';



@NgModule({ declarations: [
        AppComponent,
        LoginComponent,
        ForgotPasswordComponent,
        PageNotFoundComponent,
    ],
    bootstrap: [AppComponent],
    imports: [
        CommonModule,
        BrowserModule,
        AppRoutingModule,
        FormsModule,
        MaterialModule,
        MatSidenavModule,
        MatGridListModule,
        BrowserAnimationsModule,
        ReactiveFormsModule,
        NgChartsModule,
        AngularFireModule.initializeApp(environment.firebase),
        AngularFireMessagingModule,
           ServiceWorkerModule.register('firebase-messaging-sw.js', {
                  enabled: true,
                  registrationStrategy: 'registerImmediately'
                })


      ], providers: [provideHttpClient(withInterceptorsFromDi()),   MessagingService,] })
export class AppModule {}
