import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';
import { RPMService } from '../../../sevices/rpm.service';
import { map } from 'rxjs/operators';
interface Patient {
  PatientNumber: string;
  PatientName: string;
  Program: string;
  PatientType: string;
  searchField?: string;
  patientsearchField?: string;
  // Add other properties as needed
}

interface Role {
  Id: string;
  // Add other role properties as needed
}
@Injectable({
  providedIn: 'root',
})
export class ReportDataService {
  private allPatientsSubject = new BehaviorSubject<Patient[]>([]);
  private activePatientsSubject = new BehaviorSubject<Patient[]>([]);

  public allPatients$ = this.allPatientsSubject.asObservable();
  public activePatients$ = this.activePatientsSubject.asObservable();
  private usersSubject = new BehaviorSubject<any>([]);
  public users$ = this.usersSubject.asObservable();

  private billingProgramsSubject = new BehaviorSubject<any>([]);
  public billingPrograms$ = this.billingProgramsSubject.asObservable();

  private roleList: Role[] = [];

  constructor(private rpmservice: RPMService) {
    // Replace 'any' with your actual service type
    this.loadRoles();
  }

  loadRoles(): Role[] {
    try {
      const rolesStr = sessionStorage.getItem('Roles');
      this.roleList = rolesStr ? JSON.parse(rolesStr) : [];
      return this.roleList;
    } catch (error) {
      console.error('Error loading roles from session storage:', error);
      return [];
    }
  }

  getRoles(): Role[] {
    return this.roleList;
  }

  getTimeZoneOffsetDate(): number {
    return new Date().getTimezoneOffset();
  }

  getPatientList(): Promise<void> {
    // Ensure roles are loaded
    const roles = this.getRoles();
    if (!roles || roles.length === 0) {
      return Promise.reject('No roles available');
    }

    var todayDate = new Date().toISOString();
    var offsetValue = this.getTimeZoneOffsetDate();

    return this.rpmservice
      .rpm_get(
        `/api/patient/getallpatientslist?ToDate=${todayDate}&UtcOffset=${offsetValue}&Days=7&RoleId=${roles[0].Id}`
      )
      .then((data: any) => {
        // Store all patients
        const allPatients = data;

        // Process search fields for all patients
        for (let patient of allPatients) {
          patient.patientsearchField =
            patient.PatientNumber +
            ' - ' +
            patient.PatientName +
            '[' +
            patient.Program +
            ']';
        }

        this.allPatientsSubject.next(allPatients);

        // Filter active patients
        const activePatients = data.filter((patient: Patient) => {
          return (
            patient.PatientType === 'Active' ||
            patient.PatientType === 'OnHold' ||
            patient.PatientType === 'ReadyToDischarge'
          );
        });

        // Process search fields for active patients
        for (let patient of activePatients) {
          patient.searchField =
            patient.PatientNumber +
            ' - ' +
            patient.PatientName +
            '[' +
            patient.Program +
            ']';
        }

        this.activePatientsSubject.next(activePatients);

        return;
      });
  }
  getAllUsers(): Promise<any[]> {
    // Make sure roles are loaded
    const roles = this.getRoles();
    if (!roles || roles.length === 0) {
      return Promise.reject('No roles available');
    }

    return this.rpmservice
      .rpm_get('/api/users/getallusers?RoleId=' + roles[0].Id)
      .then((data: any) => {
        // Extract user list from the response
        let userList = data.Details || [];

        // Filter active users who are not physicians
        userList = userList.filter(
          (user: any) => user.IsActive !== false && user.Role !== 'Physician'
        );

        // Update the BehaviorSubject with the new data
        this.usersSubject.next(userList);

        // Return the data for direct access if needed
        return userList;
      });
  }

  async getBillingCode(): Promise<any> {
    try {
      const billingCodes = await this.rpmservice.rpm_get(
        '/api/patient/getbillingCodes'
      );
      return billingCodes;
    } catch (error) {
      console.error('Error fetching billing codes:', error);
      throw error;
    }
  }

  getBillingReport(
    startDate: string,
    endDate: string,
    month: string,
    clinic: string,
    cptcode: string,
    isMonth: boolean,
    selectedBillingPatient: string,
    fileformat: string
  ): Promise<any> {
    // Validate inputs
    if (!fileformat) {
      return Promise.reject(new Error('Please Select File format'));
    }

    // If using month selection, calculate start/end dates
    if (isMonth) {
      if (!month) {
        return Promise.reject(new Error('Please select a Month.'));
      }

      const dateofmonth = month.toString() + '-01T00:00:00Z';
      const date = new Date(dateofmonth);
      const firstDay = new Date(date.getUTCFullYear(), date.getUTCMonth(), 1);
      const lastDay = new Date(
        date.getUTCFullYear(),
        date.getUTCMonth() + 1,
        0
      );

      startDate = this.convertDate(firstDay);
      endDate = this.convertDate(lastDay);
    }

    // Validate dates
    if (!startDate || !endDate) {
      return Promise.reject(new Error('Please select a valid Date Range.'));
    }

    // Set default values for optional parameters
    clinic = clinic || '';
    cptcode = cptcode || '';
    selectedBillingPatient = selectedBillingPatient || '';
    const isMonthParam = isMonth ? 1 : 0;

    // Make API call
    return this.rpmservice
      .rpm_get(
        `/api/patient/getpatientbillingreport?StartDate=${startDate}&EndDate=${endDate}&patientId=${selectedBillingPatient}&Clinic=${clinic}&CPTCode=${cptcode}&isMonth=${isMonthParam}&Format=${fileformat}`
      )
      .then(
        (data: any) => {
          return data; // Return URL for download
        },
        (err: any) => {
          throw new Error('No Billing Report available for this period.');
        }
      );
  }

  getBillingReportDetail(
    startDate: string,
    endDate: string,
    month: string,
    clinic: string,
    cptcode: string,
    selectedBillingDetailPatient: string,
    fileformat: string,
    programType: string
  ): Promise<any> {
    // Set default values
    const isMonth = false; // Note: This is hardcoded to false in the original code

    // Validate file format
    if (!fileformat) {
      return Promise.reject(new Error('Please Select File format'));
    }

    // If using month selection (this block seems unused based on isMonth=false above)
    if (isMonth) {
      const dateofmonth = month.toString() + '-01T00:00:00Z';
      const date = new Date(dateofmonth);
      const firstDay = new Date(date.getUTCFullYear(), date.getUTCMonth(), 1);
      const lastDay = new Date(
        date.getUTCFullYear(),
        date.getUTCMonth() + 1,
        0
      );

      startDate = this.convertDate(firstDay);
      endDate = this.convertDate(lastDay);
    }

    // Validate dates
    if (!startDate || !endDate) {
      return Promise.reject(new Error('Please select a valid Date Range.'));
    }

    // Set default values for optional parameters
    clinic = clinic || '';
    cptcode = cptcode || '';
    selectedBillingDetailPatient = selectedBillingDetailPatient || '';

    // Make API call
    return this.rpmservice
      .rpm_get(
        `/api/patient/getbillingreportdetails?StartDate=${startDate}&EndDate=${endDate}&Clinic=${clinic}&patientId=${selectedBillingDetailPatient}&isMonth=0&Format=${fileformat}&ProgramType=${programType}`
      )
      .then(
        (data: any) => {
          return data; // Return URL for download
        },
        (err: any) => {
          throw new Error('No Billing Report available for the period.');
        }
      );
  }
  getMissingBillingReportDetail(
    startDate: string,
    endDate: string,
    month: string,
    clinic: string,
    cptcode: string,
    selectedBillingDetailPatient: string,
    fileformat: string,
    programType: string
  ): Promise<any> {
    // Set default values
    const isMonth = false; // This is hardcoded to false in the original code

    // Validate file format
    if (!fileformat) {
      return Promise.reject(new Error('Please Select File format'));
    }

    // If using month selection (this block seems unused based on isMonth=false)
    if (isMonth) {
      const dateofmonth = month.toString() + '-01T00:00:00Z';
      const date = new Date(dateofmonth);
      const firstDay = new Date(date.getUTCFullYear(), date.getUTCMonth(), 1);
      const lastDay = new Date(
        date.getUTCFullYear(),
        date.getUTCMonth() + 1,
        0
      );

      startDate = this.convertDate(firstDay);
      endDate = this.convertDate(lastDay);
    }

    // Validate dates
    if (!startDate || !endDate) {
      return Promise.reject(new Error('Please select a valid Date Range.'));
    }

    // Set default values for optional parameters
    clinic = clinic || '';
    cptcode = cptcode || '';
    selectedBillingDetailPatient = selectedBillingDetailPatient || '';

    // Make API call
    return this.rpmservice
      .rpm_get(
        `/api/patient/getmissingbillingreportdetails?StartDate=${startDate}&EndDate=${endDate}&Clinic=${clinic}&patientId=${selectedBillingDetailPatient}&isMonth=0&Format=${fileformat}&ProgramType=${programType}`
      )
      .then(
        (data: any) => {
          return data; // Return URL for download
        },
        (err: any) => {
          throw new Error('No Billing Report available for the period');
        }
      );
  }

  getBillingTypePrograms(): Promise<any> {
    return this.rpmservice.rpm_get('/api/patient/getAllprograms').then(
      (data: any) => {
        // Update the BehaviorSubject with the fetched data
        this.billingProgramsSubject.next(data);
        return data;
      },
      (err: any) => {
        console.error('Error fetching billing type programs:', err);
        throw err;
      }
    );
  }

  /**
   * Remove duplicate objects from array based on a key
   */
  removeDuplicateObjects(array: any[], key: string): any[] {
    return array.filter(
      (obj, index, self) => index === self.findIndex((t) => t[key] === obj[key])
    );
  }
  // ✅ Convert Date to API Format

  downloadNonEstablishedCallInfo(
    startDate: string,
    endDate: string,
    roleId: number
  ): Promise<any> {
    if (!startDate || !endDate) {
      return Promise.reject('Invalid Date Range');
    }

    const req_body = {
      StartDate: this.convertDate(startDate),
      EndDate: this.convertDate(endDate),
      RoleId: roleId,
    };

    return this.rpmservice.rpm_post(
      `/api/notes/NonEstablishedCallReport`,
      req_body
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
  timeConvert(n: number): string {
    const hours = Math.floor(n / 3600)
      .toString()
      .padStart(2, '0');
    const minutes = Math.floor((n % 3600) / 60)
      .toString()
      .padStart(2, '0');
    const seconds = (n % 60).toString().padStart(2, '0');

    return `${hours}:${minutes}:${seconds}`;
  }
}
