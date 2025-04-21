import { DatePipe } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import moment from 'moment';
import { RPMService } from 'src/app/components/admin-dashboard/sevices/rpm.service';
import { AuthService } from 'src/app/services/auth.service';

@Component({
  selector: 'app-program-renew',
  templateUrl: './program-renew.component.html',
  styleUrl: './program-renew.component.scss'
})
export class ProgramRenewComponent {
  newProgramid: any;
  @Output() cancelRenew = new EventEmitter<void>();
  @Input() currentpPatientId:any;
  @Input() currentProgramId:any;
  today_date: any;

 startDateValue:any
    renewProgramForm = new FormGroup({
      startdate: new FormControl(null, [Validators.required]),
      pgmendDate: new FormControl(null, [Validators.required]),
    });


constructor(  private auth: AuthService,private rpm: RPMService,private router: Router, public datepipe: DatePipe,){
  this.today_date = this.datepipe.transform(new Date(), 'yyyy-MM-dd');
}

  renewProgram() {
    var req_body: any = {};
    req_body['PatientId'] = parseInt(this.currentpPatientId);
    req_body['PatientProgramId'] = parseInt(this.currentProgramId);
    req_body['StartDate'] = this.startDateValue;
    req_body['EndDate'] =
      this.renewProgramForm.controls.pgmendDate.value + 'T00:00:00';
    this.rpm.rpm_post('/api/patient/renewpatientprogram', req_body).then(
      (data) => {
        this.auth.reloadPatientList('PatientList Updated');
        alert('Program Renewed Successfully !!');
        this.newProgramid = data;

        let route = '/admin/patients_detail';

        this.router.navigate([route], {
          queryParams: { id: this.currentpPatientId, programId: this.newProgramid },
          skipLocationChange: true,
        });

        this.cancelRenew.emit();
       // this.showProgramRenewModal=false;
        this.renewProgramForm.controls['startdate'].setValue(null);
        this.renewProgramForm.controls['pgmendDate'].setValue(null);
       // this.notes_update_panel = false;
      },
      (err) => {
        alert('Could not Renew Program!!');
      }
    );
  }

  Cancelrenew() {

    this.renewProgramForm.controls['startdate'].setValue(null);
    this.renewProgramForm.controls['pgmendDate'].setValue(null);
    this.cancelRenew.emit();
  //  this.showProgramRenewModal = false;
  }
    durationValue = 12;
    increment_duration() {
      if (this.durationValue < 12) {
        this.durationValue++;
      } else {
        this.durationValue = 12;
      }
      this.calculateEndDate();
    }
    decrement_duration() {
      if (this.durationValue > 1) {
        this.durationValue--;
      } else {
        this.durationValue = 1;
      }
      this.calculateEndDate();
    }

    calculateEndDate() {


      let someDate: any = this.renewProgramForm.controls.startdate.value;

      if (someDate !== null && someDate !== undefined) {
        if (someDate.includes('T')) {
          // Do nothing or leave it as is
        } else {
          someDate = someDate + 'T00:00:00';
        }
        someDate = this.auth.ConvertToUTCRangeInput(new Date(someDate));
      } else {

        someDate = '';
      }

      this.startDateValue = someDate;
      var someDateValue = moment(someDate).add(this.durationValue, 'M');

      this.renewProgramForm.controls['pgmendDate'].setValue(
        this.convertDate(someDateValue)
      );
    }
    convertDate(dateval: any) {
      let today = new Date(dateval);
      let dd = today.getDate();
      let dd2;
      if (dd < 10) {
        dd2 = '0' + dd;
      } else {
        dd2 = dd;
      }
      let mm = today.getMonth() + 1;
      let mm2;
      if (mm < 10) {
        mm2 = '0' + mm;
      } else {
        mm2 = mm;
      }
      const yyyy = today.getFullYear();
      dateval = yyyy + '-' + mm2 + '-' + dd2;
      return dateval;
    }

}
