import { Component, HostListener, OnInit, ViewChild } from '@angular/core';
import {
  FormBuilder,
  FormControl,
  FormGroup,
  Validators,
} from '@angular/forms';
import { Router } from '@angular/router';
import { RPMService } from 'src/app/components/admin-dashboard/sevices/rpm.service';
import { AuthService } from 'src/app/services/auth.service';
import { passwordNotContainUsernameValidator } from './password-username-validator';
import { StatusDialogBoxComponent } from '../status-dialog-box/status-dialog-box.component';

@Component({
  selector: 'app-profile-menu-button',
  templateUrl: './profile-menu-button.component.html',
  styleUrls: ['./profile-menu-button.component.scss'],
})
export class ProfileMenuButtonComponent implements OnInit {
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
  @ViewChild('statusDialog') statusDialog!: StatusDialogBoxComponent;
  constructor(
    private router: Router,
    private auth: AuthService,
    private rpm: RPMService,
    private formBuilder: FormBuilder
  ) {
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
  ngOnInit(): void {
    this.getProfileName();
    //this.formControlValueChanged();
    this.isChangePasswordVisible = false;
    if (this.passwordForm) {
      this.passwordForm.get('newpw')?.valueChanges.subscribe(() => {
        this.updatePasswordErrors();
      });
    }
  }

  toggleDropdown() {
    this.isOpen = !this.isOpen;
  }

  closeDropdown() {
    this.isOpen = false;
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

  profile() {
    this.router.navigate(['/admin/myprofile']);
    this.closeDropdown();
  }
  feedback() {
    this.router.navigate(['/admin/feedback']);
  }

  // Close the dropdown when clicking outside
  @HostListener('document:click', ['$event'])
  clickOutside(event: Event) {
    const clickedElement = event.target as HTMLElement;
    if (
      !clickedElement.closest('#menulogoutPanel') &&
      !clickedElement.closest('.dropdown-overlay')
    ) {
      this.closeDropdown();
    }
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
      console.log('User Name');
      console.log(this.userName);
    });
  }

  ChangePassword() {
    this.isChangePasswordVisible = true;
    this.closeDropdown();
  }

  // formControlValueChanged() {
  //   this.passwordForm.get('newpw')?.valueChanges.subscribe((data) => {
  //     this.userName = this.userData.UserName;
  //     this.password = data;

  //     if (this.password.includes(this.userName)) {
  //       this.result = true;
  //       console.log(this.result);
  //     } else {
  //       this.result = false;
  //     }
  //   });
  // }

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
    console.log(this.isChangePasswordVisible);
  }
  get NewTextEntered() {
    return this.passwordForm.controls;
  }

  confirm() {
    var req_body: any = {};
    if (this.passwordForm.valid) {
      req_body['UserName'] = this.passwordForm.controls.username.value;
      req_body['OldPassword'] = this.passwordForm.controls.oldpw.value;
      req_body['NewPassword'] = this.passwordForm.controls.newpw.value;

      this.rpm.rpm_post('/api/authorization/updatepassword', req_body).then(
        (data) => {
          this.statusDialog.showSuccessDialog();
          this.logout();
        },
        (err) => {
          this.statusDialog.showFailDialog();
        }
      );
    }
  }
  updatePasswordErrors(): void {
    const control = this.passwordForm.get('newpw');
    console.log(control);
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
}
