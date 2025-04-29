import { Component, OnInit, TemplateRef } from '@angular/core';
import { HttpService } from '../../sevices/http.service';
import { RPMService } from '../../sevices/rpm.service';
import {
  FormControl,
  FormGroup,
  FormControlName,
  Validators,
} from '@angular/forms';
import { MatDialog } from '@angular/material/dialog';
import { Router } from '@angular/router';

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
    private router: Router
  ) {
    this.loading = true;
    this.loading1 = true;
    this.rpm
      .rpm_get('/api/authorization/masterdatastatesandcities')
      .then((data) => {
        this.StatesAndCities = data;
        console.log(this.StatesAndCities);
        this.TimeZoneList = this.StatesAndCities.TimeZones;
        console.log(this.TimeZoneList);
        this.loading1 = false;
      });

    this.registerprofile.get('state')?.valueChanges.subscribe((data) => {
      this.cityArray = this.http_state.Cities.filter(
        (c: { StateId: number }) => c.StateId === this.stateVariable
      );
      console.log(this.stateVariable);
    });
    this.http_state = sessionStorage.getItem('states_cities');
    this.http_state = JSON.parse(this.http_state);

    this.UserList = sessionStorage.getItem('roles_masterdata');
    this.UserList = JSON.parse(this.UserList);

    var that = this;
    that.rpm.rpm_get('/api/users/getmyprofiles').then((data) => {
      this.loading = false;
      that.http_user_data = data;
      console.log('User Role is:');
      console.log(that.http_user_data);
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
    console.log(this.cityVariable);
  }

  ngOnInit(): void {
    this.myProfileOnload();
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
}
