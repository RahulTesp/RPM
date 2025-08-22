import { AdminComponent } from './../admin/admin.component';
import { Component, OnInit, ViewChild } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { RPMService } from '../../sevices/rpm.service';
import { MatDialog, MatDialogConfig } from '@angular/material/dialog';
import { StatusMessageComponent } from '../../shared/status-message/status-message.component';
import { ActivatedRoute, Router } from '@angular/router';
import { AuthService } from 'src/app/services/auth.service';
import { ConfirmDialogServiceService } from '../../shared/confirm-dialog-panel/service/confirm-dialog-service.service';
import * as uuid from 'uuid';

@Component({
  selector: 'app-add-user',
  templateUrl: './add-user.component.html',
  styleUrls: ['./add-user.component.scss'],
})
export class AddUserComponent implements OnInit {
  @ViewChild(AdminComponent) private admincomponent: AdminComponent;
  public showCancelButton=false;
  public file: any;

  checked = true;
  varData = true;
  accessList = [
    { accessright: 'Default User Access Right 1' },
    { accessright: 'Default User Access Right 2' },
    { accessright: 'Default User Access Right 3' },
    { accessright: 'Default User Access Right 4' },
    { accessright: 'Default User Access Right 5' },
  ];
  registerForm = new FormGroup({
    email: new FormControl(null, [
      Validators.required,
      Validators.email,
      Validators.pattern('^[a-z0-9._%+-]+@[a-z0-9.-]+\\.[a-z]{2,4}$'),
    ]),
    phone: new FormControl(null, [
      Validators.required,
      Validators.pattern('^[0-9]*$'),
      Validators.minLength(10),
      Validators.maxLength(10),
    ]),
    fname: new FormControl(null, [Validators.required]),
    lname: new FormControl(null, [Validators.required]),
    middlename: new FormControl(null),
    uname: new FormControl(null, [Validators.required]),
    state: new FormControl(null, [Validators.required]),
    city: new FormControl(null, [Validators.required]),
    timezone: new FormControl(null, [Validators.required]),
    userrole: new FormControl(null, [Validators.required]),
    zipcode: new FormControl(null, [Validators.required]),
    // dob: new FormControl(null, [Validators.required]),
    // gender: new FormControl(null, [Validators.required]),
    clinicname: new FormControl(null),
  });
  // userRole Variable
  selectedUserRole: any;
  UserList: any;
  http_state: any;
  stateVariable: any;
  cityVariable: any;

  userId: any;
  put_user_id: any;

  http_user_data: any;

  editVariable = false;
//  imagePath :any;
  public imagePath: string;
  GenderArray = [
    {
      value: 'M',
      name: 'Male',
    },
    {
      value: 'F',
      name: 'Female',
    },
  ];
  // UserVerify
  http_verify: any;
  verifyUser: any;
  successUser: any;
  uservalue: any;
  clinicdisplay: any;

  http_clinicData: any;
  masterdata2: unknown;
  timezones: any;

  constructor(
    private _route: ActivatedRoute,
    private rpm: RPMService,
    public dialog: MatDialog,
    private auth: AuthService,
    private router: Router,
    private confirmDialog: ConfirmDialogServiceService
  ) {
    //  UserVerify
    this.registerForm.get('uname')?.valueChanges.subscribe((data) => {
      if (data == null || this.registerForm.get('uname')?.invalid) {
        this.verifyUser = false;
        this.successUser = false;
      }
    });
    // State Dropdown For City Selection

    this.registerForm.get('state')?.valueChanges.subscribe((data) => {
      this.cityArray = this.http_state.Cities.filter(
        (c: { StateId: number }) => c.StateId === this.stateVariable
      );
    });
    this.http_state = sessionStorage.getItem('states_cities');
    this.http_state = JSON.parse(this.http_state);
    this.timezones = this.http_state.TimeZones;
    this.http_clinicData = sessionStorage.getItem('clinic_masterdata');
    this.http_clinicData = JSON.parse(this.http_clinicData);

    // Only display clinicName When selection is Physician
    this.registerForm.get('userrole')?.valueChanges.subscribe((data) => {

        if (data == 6 || data == 8) {
        this.clinicdisplay = true;
      } else {
        this.clinicdisplay = false;
        this.registerForm.controls['clinicname'].setValidators([]);
        this.registerForm.controls['clinicname'].updateValueAndValidity();
      }
    });
  }

  // Filter By State
  cityArray: any;
  filterCitybyState() {
    if (this.stateVariable) {
      this.cityVariable = undefined;

      this.cityArray = this.http_state.Cities.filter((data: any) => {
        {
          return data.StateId == this.stateVariable;
        }
      });
    }
  }
  private markFormGroupTouched(formGroup: FormGroup) {
    (<any>Object).values(formGroup.controls).forEach((control: FormGroup) => {
      control.markAsTouched();

      if (control.controls) {
        this.markFormGroupTouched(control);
      }
    });
  }
  unlockAccountStatus: any;
  ngOnInit(): void {
    this.UserList = sessionStorage.getItem('roles_masterdata');
    this.UserList = JSON.parse(this.UserList);

    this._route.queryParams.subscribe((params) => {
      this.put_user_id = params.id;
      this.editVariable = params.edit_variable;
      this.userVerifyCompleted = this.editVariable;
      this.userbuttondisplay = this.editVariable;
      this.unlockAccountStatus = true;
      this.getUserAccountLockData();

    });

  this.getUserInfo();
  }
  myDate = new Date();
  // Register User
  password: any;
  result: any;
  registerUser() {
    var req_body: any = {};
    this.markFormGroupTouched(this.registerForm);
    if (
      this.registerForm.valid &&
      (this.successUser || this.editVariable) &&
      this.registerForm.controls.city.value != undefined
    ) {
      req_body['UserName'] = this.registerForm.controls.uname.value;
      req_body['RoleId'] = [this.registerForm.controls.userrole.value];
      req_body['MobileNo'] = this.registerForm.controls.phone.value;
      req_body['Email'] = this.registerForm.controls.email.value;
      req_body['Status'] = 'Active';
      req_body['FirstName'] = this.registerForm.controls.fname.value;
      req_body['LastName'] = this.registerForm.controls.lname.value;
      req_body['MiddleName'] = this.registerForm.controls.middlename.value;
      req_body['CityId'] = this.registerForm.controls.city.value;
      req_body['StateId'] = this.registerForm.controls.state.value;
      req_body['ZipCode'] = this.registerForm.controls.zipcode.value;
      // req_body['DOB'] = this.registerForm.controls.dob.value;
      // req_body['Gender'] = this.registerForm.controls.gender.value;
      req_body['CountryId'] = 233;
      req_body['TimeZoneID'] = this.registerForm.controls.timezone.value;
      if (this.clinicdisplay) {
        req_body['OrganizationID'] =
          this.registerForm.controls.clinicname.value;
      } else {
        req_body['OrganizationID'] = 0;
      }

      var that = this;
      if (this.editVariable) {
        req_body['Id'] = this.put_user_id;
        this.rpm.rpm_post('/api/users/updateuser', req_body).then(
          (data) => {
          this.confirmDialog.showConfirmDialog(
            'User Details Updated Successfully!!',
            'Success',
            () => {
              this.router.navigate(['/admin/admin'], { queryParams: { page: 2 } });
              this.resetAddPateintMasterData();
              this.userVerifyCompleted = false;
              this.submitImage(that.userId);
            },
            false
          );
          },
          (err) => {

            this.confirmDialog.showConfirmDialog(
              'Something went wrong while updating.',
              'Error',
              null ,
              false// No action on confirm
            );
          }
        );
      } else {
        this.rpm.rpm_post('/api/users/adduser', req_body).then(
          (data) => {
            that.result = data;
            that.userId = that.result.UserId;
            that.password = that.result.password;
            this.submitImage(that.userId);
            // this.openDialog(
            //   'Message',
            //   `New User Added Successfully!! \n
            //    Password:${that.password}
            //   `
            // );
            // this.showDialog = true;
              this.confirmDialog.showConfirmDialog(
                `New User Added Successfully!! \n
               Password:${that.password}
              `,
                'Success',
                () => {
                  this.router.navigate(['/admin/admin'], { queryParams: { page: 2 } });
                  this.resetAddPateintMasterData();
                  this.userVerifyCompleted = false;
                  this.admincomponent.getAllUser();
                },
                false
              );
          },
          (err) => {
            this.confirmDialog.showConfirmDialog(
              'Something Went Wrong',
              'Error',
              null,
              false // No action on confirm
            );
          }
        );
      }
    } else if (!this.successUser && !this.editVariable) {
      this.confirmDialog.showConfirmDialog(
        'Please verify user',
        'Error',
        null,
        false // No action on confirm
      );
    } else {
      alert('Please complete the form...!');
    }
  }


  emailId = '';
  mobileNumber = '';
  userCountStatus: any;
  // GEt User Info For Edit
  getUserInfo() {
    if (this.editVariable) {
      this.rpm
        .rpm_get(`/api/users/getuserprofiles?UserId=${this.put_user_id}`)
        .then((data) => {
          this.http_user_data = data;
          console.log( this.http_user_data)

          if (this.http_user_data.HasPatients == 1) {
            this.userCountStatus = true;
          } else {
            this.userCountStatus = false;
          }

          this.registerForm.controls.phone.setValue(
            this.http_user_data.MobileNo
          );
          this.registerForm.controls.email.setValue(this.http_user_data.Email);
          this.registerForm.controls.fname.setValue(
            this.http_user_data.FirstName
          );
          this.registerForm.controls.middlename.setValue(
            this.http_user_data.MiddleName
          );
          this.registerForm.controls.lname.setValue(
            this.http_user_data.LastName
          );
          this.registerForm.controls.uname.setValue(
            this.http_user_data.UserName
          );
          // this.registerForm.controls.state.setValue(
          //   parseInt(this.http_user_data.StateId)
          // );
          // this.registerForm.controls.city.setValue(
          //   parseInt(this.http_user_data.CityId)
          // );

          this.registerForm.controls.state.setValue(this.http_user_data.StateId || 0);
          this.registerForm.controls.city.setValue(this.http_user_data.CityId || 0);

          this.registerForm.controls.timezone.setValue(
            this.http_user_data.TimeZoneID
          );
          this.registerForm.controls.zipcode.setValue(
            this.http_user_data.ZipCode
          );
          // this.registerForm.controls.dob.setValue(
          //   this.convertDate(this.http_user_data.DOB)
          // );

          // this.registerForm.controls.gender.setValue(
          //   this.http_user_data.Gender
          // );
          this.registerForm.controls.userrole.setValue(
            this.http_user_data.RoleIds[0]
          );

          this.selectedUserRole = this.http_user_data.RoleIds[0];

          if (this.selectedUserRole == 6 || this.selectedUserRole == 8) {
            this.clinicdisplay = true;

            this.registerForm.controls.clinicname.setValue(
              this.http_user_data.OrganizationID
            );
          } else {
            this.clinicdisplay = false;
          }
          this.stateVariable = this.http_user_data.StateId;
          this.cityVariable = this.http_user_data.CityId;
          this.imagePath = this.http_user_data.Picture;
          this.imagePath = encodeURI(this.imagePath)

        });
    }
  }

  // User Verify Function

  userVerifyCompleted: any;
  userbuttondisplay: any;
  userVerify() {
    var user = { UserName: this.registerForm.value.uname };

    this.auth.getuserVrify('/api/authorization/verifyusername', user).then(
      (data) => {
        this.http_verify = data;
        this.verifyUser = true;
        this.successUser = false;
        this.userVerifyCompleted = false;
      },
      (err) => {
        this.http_verify = err;

        if (this.http_verify.status == 404) {
          this.userVerifyCompleted = true;

          this.verifyUser = false;
          this.successUser = true;
        }
      }
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
  rolelist: any;
  resetAddPateintMasterData() {
    this.rolelist = sessionStorage.getItem('Roles');
    this.rolelist = JSON.parse(this.rolelist);
    this.rpm
      .rpm_get(
        '/api/patient/getprogramdetailsmasterdataaddpatient?RoleId=' +
          this.rolelist[0].Id
      )
      .then((data) => {
        this.masterdata2 = data;
        sessionStorage.setItem(
          'add_patient_masterdata',
          JSON.stringify(this.masterdata2)
        );
      });
  }
  discard() {
    let route = '/admin/admin';
    this.router.navigate([route], { queryParams: { page: 2 } });
  }
  AccountLockData: any;
  unlockResponse: any;
  UnlockAccount() {
    var userdata = { UserId: this.put_user_id };

    this.rpm.rpm_post(`/api/authorization/UnlockUser`, userdata).then(
      (data) => {
        this.unlockResponse = data;
        this.unlockAccountStatus = false;
        alert('Successfully Unlock the Account');
      },
      (err) => {
        console.log(err);
      }
    );
  }
  getUserAccountLockData() {
    var userdata = { UserId: this.put_user_id };

    this.rpm.rpm_post(`/api/users/UserStatusCheck`, userdata).then(
      (data) => {
        this.AccountLockData = data;
        this.unlockAccountStatus = this.AccountLockData.isLocked;
        // this.unlockAccountStatus = true;
      },
      (err) => {
        console.log(err);
      }
    );
  }
  image: any;
  openFile() {
    var a = document.getElementById('image');
    a?.click();
  }
  handle(e: any) {
    this.image = e.target.files[0];
    var a = document.getElementsByClassName('uploadPhoto');
    this.file = this.image.name;

    // a[0].setAttribute("style", "background-image:"+this.image.name);
    // a[0].setAttribute("style", "background: url(\"https://rpmstorage123.blob.core.windows.net/rpmprofilepictures/CL500626\"); background-repeat: no-repeat;  background-size: 100% 100%;");
  }

  async saveUserFlow() {
  try {
    await this.submitImage(this.put_user_id);  // wait until upload completes
    this.registerUser();               // run only if upload succeeded
  } catch (err:any) {
   alert(`${err.error.message} Registration aborted.`);

  }
  }
  submitImage(pid: any) {
  if (this.image) {
    const myPhoto = uuid.v4();
    const formData: any = new FormData();
    formData.append(myPhoto, this.image);

    // ? return the Promise
    return this.rpm
      .rpm_post(`/api/users/addimage?UserId=${pid}`, formData)
      .then((data) => {
        return data;
      })
      .catch((err: any) => {
       this.image = null;
       this.file = null;
        throw err;
      });
  } else {
    // ? always return a promise
    return Promise.resolve(null);
  }
}
  }


