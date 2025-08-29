import { Component, OnInit, TemplateRef, ViewChild } from '@angular/core';
import { HttpService } from '../../sevices/http.service';
import { RPMService } from '../../sevices/rpm.service';
import {
  FormControl,
  FormGroup,
  FormControlName,
  Validators,
  FormBuilder,
} from '@angular/forms';
import { MatDialog } from '@angular/material/dialog';
import { Router } from '@angular/router';
import * as uuid from 'uuid';
import { StatusDialogBoxComponent } from '../../shared/side-bar/models/status-dialog-box/status-dialog-box.component';
import { AuthService } from 'src/app/services/auth.service';
import { passwordNotContainUsernameValidator } from '../../shared/side-bar/models/profile-menu-button/password-username-validator';

@Component({
  selector: 'app-myprofile',
  templateUrl: './myprofile.component.html',
  styleUrls: ['./myprofile.component.scss'],
})
export class MyprofileComponent implements OnInit {
  http_user_data: any;
  loading: boolean;
  display_edit = true;
  isedit = true;
  UserList: any;
  http_state: any;
  stateVariable: any;
  cityVariable: any;
  cityArray: any;
  StatesAndCities: any;
  cities: any;
  selectedUserRole: any;
  TimeZoneList: any;
  user_role_status: any;



  // Password:
  profileName: string;
  userName: any;
  isOpen: boolean = false;
  userData: any;
  isChangePasswordVisible: boolean;
  passwordForm: FormGroup;
  password: any;
  result: any;
  submitted: boolean = false;
  showPassword: boolean = false;
  showConfirmPassword: boolean = false;
  passwordErrors: any;
  logoutAfterDialog = false;
  @ViewChild('statusDialog') statusDialog!: StatusDialogBoxComponent;

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

  accessList = [
    { accessright: 'Default User Access Right 1' },
    { accessright: 'Default User Access Right 2' },
    { accessright: 'Default User Access Right 3' },
    { accessright: 'Default User Access Right 4' },
    { accessright: 'Default User Access Right 5' },
    { accessright: 'Default User Access Right 6' },
    { accessright: 'Default User Access Right 7' },
    { accessright: 'Default User Access Right 8' },
    { accessright: 'Default User Access Right 9' },
    { accessright: 'Default User Access Right 10' },
    { accessright: 'Default User Access Right 11' },
    { accessright: 'Default User Access Right 12' },
  ];




  registerprofile = new FormGroup({
    email: new FormControl(null, [Validators.required, Validators.email]),
    mobile: new FormControl(null, [
      Validators.required,
      Validators.pattern('^[0-9]*$'),
      Validators.minLength(10),
      Validators.maxLength(10),
    ]),
    dob: new FormControl(null),
    gender: new FormControl(null),
    firstname: new FormControl(null, [Validators.required]),
    middlename: new FormControl(),
    lastname: new FormControl(null, [Validators.required]),
    username: new FormControl(null, [Validators.required]),
    state: new FormControl(null, [Validators.required]),
    city: new FormControl(null, [Validators.required]),
    timezone: new FormControl(null, [Validators.required]),
    zipcode: new FormControl(null),
    userrole: new FormControl(null, [Validators.required]),
  });

  loading1: boolean;

  constructor(
    private rpm: RPMService,
    private http: HttpService,
    public dialog: MatDialog,
    private router: Router,
    private auth: AuthService,
    private formBuilder: FormBuilder


  ) {
    this.loading = true;
    this.loading1 = true;
    this.showPassword = false;
    this.initPasswordForm();

    this.rpm
      .rpm_get('/api/authorization/masterdatastatesandcities')
      .then((data) => {
        this.StatesAndCities = data;
        this.TimeZoneList = this.StatesAndCities.TimeZones;
        this.loading1 = false;
      });

    this.registerprofile.get('state')?.valueChanges.subscribe((data) => {
      this.cityArray = this.http_state.Cities.filter(
        (c: { StateId: number }) => c.StateId === this.stateVariable
      );
    });
    this.http_state = sessionStorage.getItem('states_cities');
    this.http_state = JSON.parse(this.http_state);

    this.UserList = sessionStorage.getItem('roles_masterdata');
    this.UserList = JSON.parse(this.UserList);

    var that = this;
    that.rpm.rpm_get('/api/users/getmyprofiles').then((data) => {
      this.loading = false;
      that.http_user_data = data;

      that.registerprofile.controls.email.setValue(that.http_user_data.Email);
      that.registerprofile.controls.mobile.setValue(
        that.http_user_data.MobileNo
      );
      that.registerprofile.controls.firstname.setValue(
        that.http_user_data.FirstName
      );
      that.registerprofile.controls.middlename.setValue(
        that.http_user_data.MiddleName
      );
      that.registerprofile.controls.lastname.setValue(
        that.http_user_data.LastName
      );
      that.registerprofile.controls.username.setValue(
        that.http_user_data.UserName
      );
      //  that.registerprofile.controls.gender.setValue(that.http_user_data.Gender);
      that.registerprofile.controls.state.setValue(that.http_user_data.StateId);
      that.registerprofile.controls.city.setValue(that.http_user_data.CityId);
      //  var dob2 = that.http_user_data.DOB
      //  dob2 = dob2.split('T')[0]
      //  dob2.split("-").reverse().join("-")
      //  that.registerprofile.controls.dob.setValue(dob2);
      that.registerprofile.controls.zipcode.setValue(
        that.http_user_data.ZipCode
      );
      that.registerprofile.controls.userrole.setValue(
        that.http_user_data.RoleIds[0]
      );

      that.registerprofile.controls.timezone.setValue(
        that.http_user_data.TimeZoneID
      );
      this.cityVariable = this.http_user_data.CityId;
      this.stateVariable = this.http_user_data.StateId;
    });
  }
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
  selectedCity() {
  }

  ngOnInit(): void {
    this.myProfileOnload();
     this.isChangePasswordVisible = false;
     this.getProfileName();

    if (this.passwordForm) {
      this.passwordForm.get('newpw')?.valueChanges.subscribe(() => {
        this.updatePasswordErrors();
      });
    }
  }
  myProfileOnload() {
    this.registerprofile.get('email')?.disable();
    this.registerprofile.get('mobile')?.disable();
    this.registerprofile.get('firstname')?.disable();
    this.registerprofile.get('middlename')?.disable();
    this.registerprofile.get('username')?.disable();
    this.registerprofile.get('lastname')?.disable();
    this.registerprofile.get('zipcode')?.disable();

    this.registerprofile.get('timezone')?.disable();
    this.registerprofile.get('state')?.disable();
    this.registerprofile.get('city')?.disable();
    this.registerprofile?.get('userrole')?.disable();

    this.display_edit = true;
  }

  editprofile() {
    this.registerprofile.get('email')?.enable();
    this.registerprofile.get('mobile')?.enable();
    this.registerprofile.get('firstname')?.enable();
    this.registerprofile.get('middlename')?.enable();
    this.registerprofile.get('lastname')?.enable();
    this.registerprofile.get('zipcode')?.enable();

    this.registerprofile.get('timezone')?.enable();
    this.registerprofile.get('state')?.enable();
    this.registerprofile.get('city')?.enable();
    this.registerprofile?.get('userrole')?.enable();

    this.display_edit = false;
    this.isedit = false;
  }
  updateUser() {
    var req_body: any = {};
    if (this.registerprofile.valid && this.cityVariable != undefined) {
      req_body['Id'] = sessionStorage.getItem('userid');
      req_body['UserName'] = this.http_user_data.UserName;
      req_body['RoleId'] = this.http_user_data.RoleIds;
      req_body['MobileNo'] = this.registerprofile.controls.mobile.value;
      req_body['Email'] = this.registerprofile.controls.email.value;
      req_body['Status'] = 'Active';
      req_body['FirstName'] = this.registerprofile.controls.firstname.value;
      req_body['LastName'] = this.registerprofile.controls.lastname.value;
      req_body['MiddleName'] = this.registerprofile.controls.middlename.value;
      req_body['CityId'] = this.registerprofile.controls.city.value;
      req_body['StateId'] = this.registerprofile.controls.state.value;
      req_body['ZipCode'] = this.registerprofile.controls.zipcode.value;
      req_body['CountryId'] = 233;
      req_body['TimeZoneID'] = this.registerprofile.controls.timezone.value;
      req_body['OrganizationID'] = this.http_user_data.OrganizationID;

      this.rpm.rpm_post('/api/users/updateuser', req_body).then(
        (data) => {
          alert('User details updated successfully...!');
          this.myProfileOnload();
        },
        (err) => {
          alert('Could not update user details...!');
        }
      );
    } else {
      alert('Please complete the form..!');
    }
  }

  discardPage() {
    this.myProfileOnload();
  }

  openDialog(templateRef: TemplateRef<any>) {
    if (this.registerprofile.valid) {
      this.dialog.open(templateRef);
    } else {
      alert('Please complete the form..!');
    }
  }
  rolelist: any;
  backtohome() {
    this.rolelist = sessionStorage.getItem('Roles');
    this.rolelist = JSON.parse(this.rolelist);
    if (this.rolelist[0].Id == 7) {
      this.router.navigate(['/admin/patient-home']);
    } else {
      this.router.navigate(['/admin/home']);
    }
    // let route = '/admin/home';
    // this.router.navigate([route]);
  }
    image: any;
    file:any;
    openFile() {
      this.image = null;
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
    submitImage(pid: any) {
      if (this.image) {
        const myPhoto = uuid.v4();
        var formData: any = new FormData();
        formData.append(myPhoto, this.image);
        this.rpm
          .rpm_post(`/api/patient/addimage?PatientId=${pid}`, formData)
          .then(
            (data) => {},
            (err) => {
              console.log('Img error');
            }
          );
      }
    }


// Password Changes
initPasswordForm(){
  this.passwordForm = this.formBuilder.group(
        {
          username: new FormControl(null, [Validators.required]),
          oldpw: new FormControl(null, [Validators.required]),
          newpw: new FormControl(
            null,
            Validators.compose([
              Validators.required,
              Validators.minLength(8),
              Validators.pattern(
                '^(?=.*?[A-Z])(?=.*?[0-9])(?=.*?[#?!@$%^&*-]).{8,}$'
              ),
              passwordNotContainUsernameValidator('username'),
            ])
          ),

          confirmpw: new FormControl('', [Validators.required]),
        },

        {
          validators: this.MustMatch('newpw', 'confirmpw'),
        }
      );
}
  ChangePassword() {
    this.isChangePasswordVisible = true;
    this.showPassword = false;
    console.log('Show Password'+this.showPassword)
    this.closeDropdown();
  }

closeDropdown() {
    this.isOpen = false;
  }


  MustMatch(newpw: any, confirmpw: any) {
    return (formGroup: FormGroup) => {
      const passwordcontrol = formGroup.controls[newpw];
      const confirmpasswordcontrol = formGroup.controls[confirmpw];

      if (
        confirmpasswordcontrol.errors &&
        !confirmpasswordcontrol.errors['MisMatch']
      ) {
        return;
      }

      if (passwordcontrol.value !== confirmpasswordcontrol.value) {
        confirmpasswordcontrol.setErrors({ MustMatch: true });
      } else {
        confirmpasswordcontrol.setErrors(null);
      }
    };
  }
  CloseChangePasswordDialog() {
    this.isChangePasswordVisible = false;
    this.passwordForm.reset();
  }
  get NewTextEntered() {
    return this.passwordForm.controls;
  }
  getProfileName() {
    this.rpm.rpm_get('/api/users/getmyprofiles').then((data) => {
      this.userData = data;
      if (this.userData) {
        var firstName = this.userData.FirstName.charAt(0);
        var lastName = this.userData.LastName.charAt(0);
        this.profileName = firstName + lastName;
      }

      this.userName = this.userData.UserName;
      this.passwordForm.controls.username?.setValue(this.userData.UserName);

    });
  }

  confirm() {
    var req_body: any = {};

    this.passwordForm.controls.username?.setValue(this.userName);
    if (this.passwordForm.valid) {
      req_body['UserName'] = this.passwordForm.controls.username.value;
      req_body['OldPassword'] = this.passwordForm.controls.oldpw.value;
      req_body['NewPassword'] = this.passwordForm.controls.newpw.value;

      this.rpm.rpm_post('/api/authorization/updatepassword', req_body).then(
        (data) => {
          this.statusDialog.showSuccessDialog();
          this.logoutAfterDialog = true;
          this.passwordForm.reset();

        },
        (err) => {

          this.statusDialog.showFailDialog(err.error.Status);
          this.passwordForm.reset();
        }
      );
    }else{
      alert('All password fields are required. Kindly provide the Old, New, and Confirm Password')

    }
  }
  updatePasswordErrors(): void {
    const control = this.passwordForm.get('newpw');
    if (!control) {
      return;
    }
    this.passwordErrors = [];

    if (control.errors) {
      if (control.errors.minlength) {
        this.passwordErrors.push('Minimum 8 characters required');
      }
      if (control.errors.pattern) {
        this.passwordErrors.push(
          'Min 2 uppercase, 2 numbers and 1 special character required'
        );
      }
      if (control.errors.containsUsername) {
        this.passwordErrors.push('New password should not contain username');
      }
    }
  }
  showPasswordVisible() {
    this.showPassword = !this.showPassword;
  }
  showConfirmPasswordVisible() {
    this.showConfirmPassword = !this.showConfirmPassword;
  }
  isPasswordMatching(): boolean {
    const newPassword = this.passwordForm.get('newpw')?.value;
    const confirmPassword = this.passwordForm.get('confirmpw')?.value;
    return newPassword && confirmPassword && newPassword === confirmPassword;
  }
onDialogClosed() {
  if (this.logoutAfterDialog) {
    this.logout();
  }
}
 async logout(): Promise<void> {
    try {
      await this.auth.logout('/api/authorization/logout');
    } catch (error) {
      console.error('Logout error:', error);
    } finally {
      this.auth.removeToken();
      await this.router.navigate(['/login']);
      window.location.reload();
    }
  }
}
