import { Component, ElementRef, OnInit, ViewChild } from '@angular/core';
import {
  AbstractControl,
  UntypedFormBuilder,
  UntypedFormControl,
  UntypedFormGroup,
  ValidationErrors,
  Validators,
} from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from 'src/app/services/auth.service';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { environment } from 'src/environments/environment';
import { RPMService } from '../admin-dashboard/sevices/rpm.service';
import { HttpService } from '../admin-dashboard/sevices/http.service';
import { Event } from 'reconnecting-websocket/dist/events';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css'],
})
export class LoginComponent implements OnInit {
  @ViewChild('focusFirstbox') focusFirstbox: ElementRef;
  @ViewChild('focusSecondbox') focusSecondbox: ElementRef;
  @ViewChild('focusThirdbox') focusThirdbox: ElementRef;
  @ViewChild('focusFourthbox') focusFourthbox: ElementRef;
  @ViewChild('focusFifthbox') focusFifthbox: ElementRef;
  @ViewChild('focusSixthbox') focusSixthbox: ElementRef;
  @ViewChild('focusSeventhbox') focusSeventhbox: ElementRef;
  @ViewChild('focusEightbox') focusEightbox: ElementRef;

  show: boolean = false;

  // click event function toggle
  showpassword() {
    this.show = !this.show;
  }

  verificationForm = new UntypedFormGroup({});

  loginForm = new UntypedFormGroup({
    username: new UntypedFormControl(null, [Validators.required]),
    password: new UntypedFormControl(null, [Validators.required]),
  });
  restPasswordloginForm = this.formBuilder.group(
    {
      newpassword: new UntypedFormControl(
        null,
        Validators.compose([
          Validators.required,
          Validators.minLength(8),
          Validators.pattern(
            '^(?=.*?[A-Z]{2})(?=.*?[0-9]{2})(?=.*?[#?!@$%^&*-]).{8,}$'
          ),
        ])
      ),
      confirmpassword: new UntypedFormControl('', [Validators.required]),
    },
    {
      validators: this.MustMatch('newpassword', 'confirmpassword'),
    }
  );
  resetInitloginForm = new UntypedFormGroup({
    username: new UntypedFormControl('', [Validators.required]),
  });
  MustMatch(newpw: any, confirmpw: any) {
    return (formGroup: UntypedFormGroup) => {
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

  host: any;
  username = '';
  loginVariable = 1;
  IsUsername: boolean;
  UserButtonDisabled: boolean;
  IsPassword: boolean;
  loading: boolean;
  LoginButtonDisabled: boolean;
  SERVER_URL: any;
  variable: any;
  mobileNum: any;
  constructor(
    private auth: AuthService,
    private router: Router,
    private http: HttpClient,

    private formBuilder: UntypedFormBuilder
  ) {
    this.SERVER_URL = environment.protocol + '://' + environment.host;

    this.IsUsername = true;
    this.UserButtonDisabled = false;
    this.IsPassword = true;
    this.LoginButtonDisabled = true;
    this.loading = false;
    this.host = this.auth.get_environment();
    this.variable = 1;
    this.confirmStatuscheck = true;

    this.loginForm.get('password')?.valueChanges.subscribe((x) => {
      if (
        this.loginForm.value.username != null &&
        (this.loginForm.value.password != null || this.onPasteValue == true)
      ) {
        this.LoginButtonDisabled = false;
      } else {
        this.LoginButtonDisabled = true;
      }
    });
    // this.restPasswordloginForm
    //   .get('confirmpassword')
    //   ?.valueChanges.subscribe((x) => {
    //     if (
    //       this.restPasswordloginForm.controls['newpassword']?.getError(
    //         'minlength'
    //       ) != null &&
    //       this.restPasswordloginForm.controls['newpassword']?.getError(
    //         'pattern'
    //       ) != null
    //     ) {
    //       this.confirmPasswordError = false;
    //       this.confirmStatuscheck = false;
    //     } else {
    //       this.confirmStatuscheck = true;

    //     }
    //   });
  }
  resetError() {
    this.confirmPasswordError = false;
    if (this.restPasswordloginForm.valid) {
      this.confirmStatuscheck = false;
    }
  }
  confirmStatus = false;

  resetPasswordNewPassword() {
    this.resetError();
    if (
      this.restPasswordloginForm.controls.newpassword.value ===
      this.restPasswordloginForm.controls.confirmpassword.value
    ) {
      console.log(this.restPasswordloginForm.controls.newpassword.value);
      console.log(this.restPasswordloginForm.controls.confirmpassword.value);

      this.confirmStatus = true;
    } else {
      this.confirmStatus = false;
    }
  }
  onPasteValue = false;
  onPastePassword(e: any) {
    let clipboardData = e.clipboardData || window.Clipboard;
    let pastedText = clipboardData.getData('text');
    this.onPasteValue = true;
    if (this.loginForm.value.username != null && pastedText != null) {
      this.LoginButtonDisabled = false;
    } else {
      this.LoginButtonDisabled = true;
    }
  }
  txtValue: any;
  message: string;
  response: any;

  isUserValid() {
    var that = this;
    if (that.loginForm.value.username) {
      that.UserButtonDisabled = true;
      var user = { UserName: that.loginForm.value.username };
      that.auth.login('/api/authorization/verifyusername', user).then(
        (data) => {
          that.UserButtonDisabled = false;
          that.loginVariable = 1;
        },
        (err) => {
          let timeLeft = 10;
          that.UserButtonDisabled = false;
          var interval = setInterval(() => {
            if (timeLeft > 0) {
              timeLeft--;
              that.IsUsername = false;
            } else {
              clearInterval(interval);
              that.IsUsername = true;
            }
          }, 1000);
        }
      );
    }
  }

  verificationTkn: any;
  mfaEnabled: any;
  countlimit: any;
  validMobileNum: any;
  validEmail: any;
  maildrecived: any;
  login() {
    if (this.loginForm.valid) {
      var that = this;
      that.LoginButtonDisabled = true;
      that.loading = true;
      var login = {
        UserName: that.loginForm.value.username,
        Password: that.loginForm.value.password,
      };
      that.auth.login('/api/authorization/Userlogin', login).then(
        (data) => {
          that.response = data;
          this.verificationTkn = that.response.tkn;
          this.mfaEnabled = that.response.MFA;
          this.validMobileNum = that.response.ValidMobile;
          this.validEmail = that.response.ValidMailId;
          this.maildrecived = that.response.MailId;

          if (this.mfaEnabled) {
            if (this.validMobileNum == false && this.validEmail == false) {
              alert('OTP failed to sent,Please contact Admin');
              window.location.reload();
              return;
            }
            this.variable = 2;
            that.loading = false;
            this.mobileNum = that.response.Mobilenumber;
            this.countlimit = that.response.TimeLimit * 60;
            this.countDownVal = that.response.TimeLimit * 60;
            this.countDown();
          } else {
            if (that.response.Roles.length == 0) {
              alert('No Role Information found for the user...!');
              that.router.navigate(['/login']);
              sessionStorage.clear();
              sessionStorage.clear();
            } else if (that.response.Roles[0].Id == 7) {
              // alert('Patient Login Not Allowed.');
              // that.router.navigate(['/login']);
              // sessionStorage.clear();
              // sessionStorage.clear();
              that.auth.setToken(that.response.tkn, that.response.tkt);
              sessionStorage.setItem(
                'Roles',
                JSON.stringify(that.response.Roles)
              );
              that.router.navigate(['/admin/patient-home']);
            } else {
              that.auth.setToken(that.response.tkn, that.response.tkt);
              sessionStorage.setItem(
                'Roles',
                JSON.stringify(that.response.Roles)
              );
              that.router.navigate(['/admin/home']);
              this.auth.firstLogin = true;
            }
            that.loading = false;
          }
        },
        (err) => {
          let timeLeft = 10;
          that.LoginButtonDisabled = false;
          that.loading = false;

          if (err.status === 12030) {
            window.location.reload();
            this.auth.unauthorized();
            alert('NetWork Error occurred.');
          } else {
            if (err.status == 401) {
              var interval = setInterval(() => {
                if (timeLeft > 0) {
                  timeLeft--;
                  that.IsPassword = false;
                } else {
                  clearInterval(interval);
                  that.IsPassword = true;
                }
              }, 1000);
              this.auth.unauthorizedOTP();
            } else if (err.status == 503) {
              alert(err.error);
              window.location.reload();
              this.auth.unauthorized();
            } else if (err.status == 403) {
              alert(err.error);
              window.location.reload();
              this.auth.unauthorized();
            } else if (err.status == 404) {
              alert(err.error);
              window.location.reload();
              this.auth.unauthorized();
            }
          }
        }
      );
    }
  }

  ResentOTP() {
    if (this.loginForm.valid) {
      var that = this;
      that.loading = true;
      var login = {
        UserName: that.loginForm.value.username,
        Password: that.loginForm.value.password,
      };
      this.txtFirstDigit = '';
      this.txtSecondDigit = '';
      this.txtThirdDigit = '';
      this.txtFourthDigit = '';
      this.txtFifthDigit = '';
      this.txtSixthDigit = '';
      this.txtSevenDigit = '';
      this.txtEightDigit = '';
      that.auth.login('/api/authorization/Userlogin', login).then(
        (data) => {
          that.response = data;
          this.verificationTkn = that.response.tkn;
          this.countDownStart();

          this.mfaEnabled = that.response.MFA;
          if (this.mfaEnabled) {
            that.loading = false;
          }
        },
        (err) => {
          that.loading = false;
          alert(err);
        }
      );
    }
  }

  ngOnInit(): void {
    if (this.auth.isLoggedIn()) {
      this.router.navigate(['/admin']);
    }
  }
  txtFirstDigit = '';
  txtSecondDigit = '';
  txtThirdDigit = '';
  txtFourthDigit = '';
  txtFifthDigit = '';
  txtSixthDigit = '';
  txtSevenDigit = '';
  txtEightDigit = '';
  digiLength: any;
  otpData: any;
  invalidOtp = false;

  onTextChange() {
    if (
      this.txtFirstDigit != '' &&
      this.txtFirstDigit != ' ' &&
      this.txtSecondDigit != '' &&
      this.txtSecondDigit != ' ' &&
      this.txtThirdDigit != '' &&
      this.txtThirdDigit != ' ' &&
      this.txtFourthDigit != '' &&
      this.txtFourthDigit != ' ' &&
      this.txtFifthDigit != '' &&
      this.txtFifthDigit != ' ' &&
      this.txtSixthDigit != '' &&
      this.txtSixthDigit != ' ' &&
      this.txtSevenDigit != '' &&
      this.txtSevenDigit != ' ' &&
      this.txtEightDigit != '' &&
      this.txtEightDigit != ' '
    ) {
      this.otpData =
        this.txtFirstDigit +
        this.txtSecondDigit +
        this.txtThirdDigit +
        this.txtFourthDigit +
        this.txtFifthDigit +
        this.txtSixthDigit +
        this.txtSevenDigit +
        this.txtEightDigit;

      var req_body: any = {};
      var that = this;
      req_body['UserName'] = this.loginForm.value.username;
      req_body['OTP'] = this.otpData;
      this.validateOTP(
        '/api/authorization/UserloginVerifiy',
        req_body,
        '' + this.verificationTkn
      ).then(
        (data) => {
          that.response = data;

          if (that.response.Roles.length == 0) {
            alert('No Role Information found for the user...!');
            that.router.navigate(['/login']);
            this.variable = 1;
            sessionStorage.clear();
            sessionStorage.clear();
          } else if (that.response.Roles[0].Id == 7) {
            // alert('Patient Login Not Allowed.');
            // that.router.navigate(['/login']);
            // this.variable = 1;
            // sessionStorage.clear();
            // sessionStorage.clear();
            that.auth.setToken(that.response.tkn, that.response.tkt);
            sessionStorage.setItem(
              'Roles',
              JSON.stringify(that.response.Roles)
            );
            that.router.navigate(['/admin/patient-home']);
          } else {
            that.auth.setToken(that.response.tkn, that.response.tkt);
            sessionStorage.setItem(
              'Roles',
              JSON.stringify(that.response.Roles)
            );

            that.router.navigate(['/admin/home']);
          }
        },

        (err) => {
          // alert(err.error);
          that.variable = 2;
          var timeLeft = 5;
          if (err.status === 12030) {
            window.location.reload();
            that.auth.unauthorizedOTP();
            alert('NetWork Error occurred.');
          } else {
            if (err.status == 401) {
              that.auth.unauthorizedOTP();
              var interval = setInterval(() => {
                if (timeLeft > 0) {
                  timeLeft--;
                  this.invalidOtp = true;
                } else {
                  clearInterval(interval);
                  this.invalidOtp = false;
                }
              }, 800);
              // alert('Please Enter Valid OTP');
            } else if (err.status == 403) {
              alert('Account Temporarily Blocked');
              this.invalidOtp = false;
              that.auth.unauthorizedOTP();
              window.location.reload();
            }
          }
          that.setEmtyOTPFields();
        }
      );
    }
  }
  setEmtyOTPFields() {
    this.txtFirstDigit = '';
    this.txtSecondDigit = '';
    this.txtThirdDigit = '';
    this.txtFourthDigit = '';
    this.txtFifthDigit = '';
    this.txtSixthDigit = '';
    this.txtSevenDigit = '';
    this.txtEightDigit = '';
    this.focusFirstbox.nativeElement.focus();
  }
  validateOTP(url: any, params: any, tkn: any) {
    let headers = new HttpHeaders();
    headers = headers.append('Bearer', tkn);
    console.log(url);
    console.log(params);
    console.log(tkn);
    let promise = new Promise((resolve, reject) => {
      this.http
        .post<any>(this.SERVER_URL + url, params, { headers: headers })
        .toPromise()
        .then(
          (data) => {
            resolve(data);
            window.location.reload();
          },
          (err) => {
            if (err.status == 401) {
              this.auth.unauthorizedOTP();
            }
            reject(err);
          }
        );
    });
    return promise;
  }
  countDownVal: any;
  interval: any;
  countDown() {
    this.interval = setInterval(() => {
      this.countDownVal--;
      if (this.countDownVal == 0) {
        clearInterval(this.interval);
      }
    }, 1000);
  }

  countDownStart() {
    this.countDownVal = this.countlimit;
    this.countDown();
  }
  countDownresetStart() {
    this.countDownVal = this.resetTimer;
    this.countDown();
  }
  OnNextFunction(position: any) {
    switch (position) {
      case 1:
        if (this.txtFirstDigit != '') {
          this.focusSecondbox.nativeElement.focus();
        }
        break;
      case 2:
        if (this.txtSecondDigit != '') {
          this.focusThirdbox.nativeElement.focus();
        }
        break;
      case 3:
        if (this.txtThirdDigit != '') {
          this.focusFourthbox.nativeElement.focus();
        }
        break;
      case 4:
        if (this.txtFourthDigit != '') {
          this.focusFifthbox.nativeElement.focus();
        }
        break;
      case 5:
        if (this.txtFifthDigit != '') {
          this.focusSixthbox.nativeElement.focus();
        }
        break;
      case 6:
        if (this.txtSixthDigit != '') {
          this.focusSeventhbox.nativeElement.focus();
        }
        break;
      case 7:
        if (this.txtSevenDigit != '') {
          this.focusEightbox.nativeElement.focus();
        }
        break;
    }
  }
  goResetPassWordInit() {
    this.variable = 3;
  }
  resttkn: any;
  resetusername: any;
  resetMailId: any;
  restMobileNum: any;
  resetTimer: any;
  resetdata: any;
  loading_otp: any;
  validMobile: any;
  validMail: any;
  getResetPasswordOtp() {
    this.loading_otp = true;
    this.resetusername = this.resetInitloginForm.value.username;
    if (this.resetInitloginForm.valid) {
      this.auth
        .resetOtp(
          `/api/authorization/forgetpassword?username=${this.resetusername}`
        )
        .then(
          (data) => {
            this.resetdata = data;
            this.resttkn = this.resetdata.tkn;
            this.resetMailId = this.resetdata.MailId;
            this.restMobileNum = this.resetdata.Mobilenumber;
            this.resetTimer = this.resetdata.TimeLimit * 60;
            this.validMobile = this.resetdata.ValidMobile;
            this.validMail = this.resetdata.ValidMailId;
            this.variable = 4;
            this.countDownresetStart();
            this.loading_otp = false;
            this.confirmStatuscheck = true;

            this.confirmBtnStatus();
            this.setResetEmtyOTPFields();
          },
          (err) => {
            //show error patient id creation failed
            alert(err.error);
            this.loading_otp = false;
          }
        );
    } else {
      alert('Please Complete the Form');
      this.loading_otp = false;
    }
  }
  txtResetFirstDigit = '';
  txtResetSecondDigit = '';
  txtResetThirdDigit = '';
  txtResetFourthDigit = '';
  txtResetFifthDigit = '';
  txtResetSixthDigit = '';
  txtResetSevenDigit = '';
  txtResetEightDigit = '';
  otpResetData: any;
  digiresetLength: any;
  passwordNotMatch = false;
  errMessage: any;
  confirmPasswordError = false;
  confirmPassword() {
    this.loading_otp = true;
    this.otpResetData =
      this.txtResetFirstDigit +
      this.txtResetSecondDigit +
      this.txtResetThirdDigit +
      this.txtResetFourthDigit +
      this.txtResetFifthDigit +
      this.txtResetSixthDigit +
      this.txtResetSevenDigit +
      this.txtResetEightDigit;

    var req_body: any = {};
    var that = this;
    let tkn;

    req_body['UserName'] = this.resetusername;
    req_body['OTP'] = this.otpResetData;
    req_body['Password'] = this.restPasswordloginForm.value.newpassword;
    if (this.restPasswordloginForm.valid) {
      {
        this.passwordNotMatch = false;
        this.validatePassword(
          `/api/authorization/userresetpasswordverifiy`,
          req_body,
          this.resttkn
        ).then(
          (data) => {
            this.loading_otp = false;
            alert('Password changed successfully, Please re-login to continue');
          },
          (err) => {
            //show error patient id creation failed
            var timeLeft = 5;
            if (err.status == 401) {
              this.errMessage = err.error;
              var interval = setInterval(() => {
                if (timeLeft > 0) {
                  timeLeft--;
                  this.invalidOtp = true;
                } else {
                  clearInterval(interval);
                  this.invalidOtp = false;
                }
              }, 800);
              this.setResetEmtyOTPFields();
            } else if (err.status == 403) {
              alert(err.error);
              window.location.reload();
              this.auth.unauthorized();
            }
            this.loading_otp = false;
          }
        );
      }
    } else {
      if (
        this.restPasswordloginForm.value.confirmpassword == null ||
        this.restPasswordloginForm.value.confirmpassword == ''
      ) {
        this.confirmPasswordError = true;
      }
      this.loading_otp = false;
    }
  }
  confirmStatuscheck: any;
  confirmBtnStatus() {
    if (
      this.restPasswordloginForm.controls['newpassword']?.getError(
        'minlength'
      ) != null &&
      this.restPasswordloginForm.controls['newpassword']?.getError('pattern') !=
        null &&
      this.restPasswordloginForm.controls['newpassword'].value != null
    ) {
      return true;
    } else {
      return false;
    }
  }
  OnResetNextFunction(position: any) {
    switch (position) {
      case 1:
        if (this.txtResetFirstDigit != '') {
          this.focusSecondbox.nativeElement.focus();
        }
        break;
      case 2:
        if (this.txtResetSecondDigit != '') {
          this.focusThirdbox.nativeElement.focus();
        }
        break;
      case 3:
        if (this.txtResetThirdDigit != '') {
          this.focusFourthbox.nativeElement.focus();
        }
        break;
      case 4:
        if (this.txtResetFourthDigit != '') {
          this.focusFifthbox.nativeElement.focus();
        }
        break;
      case 5:
        if (this.txtResetFifthDigit != '') {
          this.focusSixthbox.nativeElement.focus();
        }
        break;
      case 6:
        if (this.txtResetSixthDigit != '') {
          this.focusSeventhbox.nativeElement.focus();
        }
        break;
      case 7:
        if (this.txtResetSevenDigit != '') {
          this.focusEightbox.nativeElement.focus();
        }
        break;
    }
  }
  validatePassword(url: any, params: any, tkn: any) {
    let headers = new HttpHeaders();
    headers = headers.append('Bearer', tkn);
    let promise = new Promise((resolve, reject) => {
      this.http
        .post<any>(this.SERVER_URL + url, params, { headers: headers })
        .toPromise()
        .then(
          (data) => {
            resolve(data);
            window.location.reload();
          },
          (err) => {
            if (err.status == 401) {
              this.auth.unauthorizedOTP();
            }
            reject(err);
          }
        );
    });
    return promise;
  }
  setResetEmtyOTPFields() {
    this.txtResetFirstDigit = '';
    this.txtResetSecondDigit = '';
    this.txtResetThirdDigit = '';
    this.txtResetFourthDigit = '';
    this.txtResetFifthDigit = '';
    this.txtResetSixthDigit = '';
    this.txtResetSevenDigit = '';
    this.txtResetEightDigit = '';
    this.focusFirstbox.nativeElement.focus();
  }
}
