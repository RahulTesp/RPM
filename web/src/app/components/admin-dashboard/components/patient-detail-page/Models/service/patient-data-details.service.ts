import { Injectable } from '@angular/core';
import { RPMService } from '../../../../sevices/rpm.service';
import { BehaviorSubject } from 'rxjs';
import { PatientUtilService } from './patient-util.service';
import { AuthService } from 'src/app/services/auth.service';
import { DatePipe } from '@angular/common';

@Injectable({
  providedIn: 'root',
})
export class PatientDataDetailsService {
  public patientDataSubject = new BehaviorSubject<any>(null);
  patientData$ = this.patientDataSubject.asObservable();
  public patientData: any;
  private patientCallDataSubject = new BehaviorSubject<any>(null);
  patientCallData$ = this.patientCallDataSubject.asObservable();

  private patientHealthtrendSubject = new BehaviorSubject<any>(null);
  patientHealthtrend$ = this.patientHealthtrendSubject.asObservable();

  public today = new Date();
  public sevenDaysAgo = new Date(this.today).setDate(this.today.getDate() - 30);

  public ThirtyDaysAgo = new Date(this.today).setDate(
    this.today.getDate() - 30
  );

  constructor(
    private rpmService: RPMService,
    private patientutil: PatientUtilService,
    private auth: AuthService,
    public datepipe: DatePipe
  ) {}

  /**
   * Fetch Patient Data using rpm_get() and store in cache + sessionStorage
   */
  async fetchPatientInfo(patientId: string, programId: string) {
    const url = `/api/patient/getpatient?PatientId=${patientId}&PatientprogramId=${programId}`;

    try {
      this.patientDataSubject.next(null);
      const data = await this.rpmService.rpm_get(url);
      this.patientDataSubject.next(data);
      // Store in sessionStorage (Persistent Cache)
      this.storeSessionData(data);

      return data;
    } catch (error) {
      console.error('Error fetching patient data:', error);
      throw error;
    }
  }

  /**
   * Get Cached Patient Data from Memory
   */
  getCachedPatientData(): any {
    return this.patientDataSubject.value;
  }

  /**
   * Store API Response in sessionStorage
   */
  private storeSessionData(data: any) {
    if (data) {
      sessionStorage.setItem('patientdataUserId', data.PatientDetails.UserId);
      // sessionStorage.setItem(
      //   'pname',
      //   `${data.PatientDetails.FirstName} ${data.PatientDetails.LastName}`
      // );
      sessionStorage.setItem(
        'patientPgmName',
        data.PatientProgramdetails.ProgramName
      );
      // sessionStorage.setItem(
      //   'patientStatusData',
      //   data.PatientProgramdetails.Status
      // );
      // sessionStorage.setItem(
      //   'careteamuser',
      //   data.PatientProgramdetails.CareTeamUserId
      // );
      sessionStorage.setItem(
        'viatls',
        JSON.stringify(data.PatientProgramdetails.PatientVitalInfos)
      );
    }
  }
  clearCachedPatientData() {
    console.log('Resetting patient cache...');
    this.patientDataSubject.next(null);
    sessionStorage.removeItem('patientdataUserId');
    sessionStorage.removeItem('pname');
    sessionStorage.removeItem('patientPgmName');
    sessionStorage.removeItem('patientStatusData');
    sessionStorage.removeItem('careteamuser');
    sessionStorage.removeItem('viatls');
  }

  private convertToISODate(date: Date | number): string {
    if (typeof date === 'number') date = new Date(date);
    return date.toISOString().split('T')[0];
  }
  private formatDateForQuery(date: any, isEndDate = false): string {
    const formattedDate =
      this.convertToISODate(date) + (isEndDate ? 'T23:59:59' : 'T00:00:00');
    return this.auth.ConvertToUTCRangeInput(new Date(formattedDate));
  }

  async getVitalReading(
    patientId: string,
    programId: string,
    startDate?: string,
    endDate?: string
  ) {
    const start = this.formatDateForQuery(startDate || this.ThirtyDaysAgo);
    const end = this.formatDateForQuery(endDate || this.today, true);
    const url = `/api/patient/getpatientvitalreadingsv1?PatientId=${patientId}&PatientProgramId=${programId}&StartDate=${start}&EndDate=${end}`;
    try {
      const data = (await this.rpmService.rpm_get(url)) as any;

      const processedVitals: Record<string, any> = {};

      if (data.BloodPressure != null) {
        processedVitals.BloodPressure = this.patientutil.newVitalreadingData(
          data.BloodPressure,
          'BloodPressureReadings'
        );
      }
      if (data.BloodGlucose != null) {
        processedVitals.BloodGlucose = this.patientutil.newVitalreadingData(
          data.BloodGlucose,
          'BloodGlucoseReadings'
        );
      }
      if (data.BloodOxygen != null) {
        processedVitals.BloodOxygen = this.patientutil.newVitalreadingData(
          data.BloodOxygen,
          'BloodOxygenReadings'
        );
      }
      if (data.Weight != null) {
        processedVitals.Weight = this.patientutil.newVitalreadingData(
          data.Weight,
          'WeightReadings'
        );
      }

      return {
        vitalData: data,
        processedVitals,
      };
    } catch (error) {
      console.error('Error fetching vital reading data:', error);
      throw error;
    }
  }

  async getPatientMedication(
    patientId: string,
    programId: string
  ): Promise<any> {
    const url = `/api/patient/getpatientmedication?PatientId=${patientId}&PatientProgramId=${programId}`;

    try {
      const data = await this.rpmService.rpm_get(url); // Ensure this returns a Promise
      return data;
    } catch (error) {
      console.error(
        `Error fetching medication for PatientId=${patientId}, ProgramId=${programId}:`,
        error
      );
      throw new Error('Failed to fetch medication data. Please try again.');
    }
  }

  async getSymptomsData(patientId: string, programId: string): Promise<any> {
    const url = `/api/patient/getpatientsymptoms?PatientId=${patientId}&PatientProgramId=${programId}`;

    try {
      const data = await this.rpmService.rpm_get(url); // Call API service
      return data;
    } catch (error) {
      console.error(
        `❌ Error fetching symptoms for PatientId=${patientId}, ProgramId=${programId}:`,
        error
      );
      throw new Error('Failed to fetch symptoms data. Please try again.');
    }
  }

  async getPatientSchedules(
    patientId: string,
    startDate: string,
    endDate: string
  ): Promise<any> {
    try {
      return await this.rpmService.rpm_get(
        `/api/patient/getpatientschedules?PatientId=${patientId}&StartDate=${startDate}&EndDate=${endDate}`
      );
    } catch (error) {
      console.error('Error fetching patient schedules:', error);
      throw error;
    }
  }

  async getPatientReviewNotes(
    patientId: string,
    programId: string,
    startDate: string,
    endDate: string
  ): Promise<any> {
    const apiUrl = '/api/patient/getpatientnotesbyprogram';
    try {
      const url = `${apiUrl}?PatientId=${patientId}&PatientProgramId=${programId}&&NoteType=REVIEW&StartDate=${startDate}&EndDate=${endDate}`;
      return await this.rpmService.rpm_get(url);
    } catch (error) {
      console.error('❌ Error fetching alerts and tasks:', error);
      return [];
    }
  }
  async getPatientChat(
    patientId: string
  ): Promise<any> {
    const apiUrl = '/api/comm/getallchats';
    try {
      const url = `${apiUrl}?ToUser=${patientId}`;
      return await this.rpmService.rpm_get(url);
    } catch (error) {
      console.error('❌ Error fetching alerts and tasks:', error);
      return [];
    }
  }


  async getPatientCallNotes(
    patientId: string,
    programId: string,
    startDate: string,
    endDate: string
  ): Promise<any> {
    const apiUrl = '/api/patient/getpatientnotes';
    try {
      const url = `${apiUrl}?PatientId=${patientId}&PatientProgramId=${programId}&&NoteType=CALL&StartDate=${startDate}&EndDate=${endDate}`;
      const data = await this.rpmService.rpm_get(url);
      this.patientCallDataSubject.next(data);
      return data;
    } catch (error) {
      console.error('❌ Error fetching alerts and tasks:', error);
      return [];
    }
  }

  async getPatientSMSData(
    patientId: string,
    programId: string,
    startDate: string,
    endDate: string
  ): Promise<any> {
    try {
      return await this.rpmService.rpm_get(
        `/api/patient/getpatientsmshistory?PatientId=${patientId}&PatientProgramId=${programId}&StartDate=${startDate}&EndDate=${endDate}`
      );
    } catch (error) {
      console.error('Error fetching patient schedules:', error);
      throw error; // Re-throw to allow component-level handling
    }
  }

  async getAlertTask(
    patientId: string,
    programId: string,
    startDate: string,
    endDate: string
  ): Promise<any> {
    const apiUrl = '/api/patient/getpatientAlertandtask';
    try {
      const url = `${apiUrl}?PatientId=${patientId}&PatientProgramId=${programId}&StartDate=${startDate}&EndDate=${endDate}`;
      return await this.rpmService.rpm_get(url);
    } catch (error) {
      console.error('❌ Error fetching alerts and tasks:', error);
      return [];
    }
  }

  getDateRange(
    startValue: string | null,
    endValue: string | null,
    convertDate: (date: Date) => string
  ): { start: string; end: string } {
    let startDate: Date;
    let endDate: Date;

    if (!startValue && !endValue) {
      const currentDate = new Date();
      startDate = new Date(
        currentDate.getFullYear(),
        currentDate.getMonth(),
        1
      );
      endDate = new Date(
        currentDate.getFullYear(),
        currentDate.getMonth() + 1,
        0
      );
    } else if (endValue) {
      startDate = new Date(startValue!);
      endDate = new Date(endValue!);
    } else {
      throw new Error('Invalid date range provided.');
    }

    const formattedStart = this.auth.ConvertToUTCRangeInput(
      new Date(`${convertDate(startDate)}T00:00:00`)
    );
    const formattedEnd = this.auth.ConvertToUTCRangeInput(
      new Date(`${convertDate(endDate)}T23:59:59`)
    );

    return { start: formattedStart, end: formattedEnd };
  }

  async getPatientCallNoteById(
    programName: string,
    noteId: string
  ): Promise<any> {
    try {
      return await this.rpmService.rpm_get(
        `/api/patient/getpatientnotes?ProgramName=${programName}&Type=CALL&PatientNoteId=${noteId}`
      );
    } catch (error) {
      console.error('Error fetching patient call note by ID:', error);
      throw error;
    }
  }

  async getPatientLastBilledCycle(
    patientId: string,
    programId: string,
    status: string
  ): Promise<any> {
    try {
      return await this.rpmService.rpm_get(
        `/api/patient/getpatientlastbilledcycle?patientId=${patientId}&patientProgramId=${programId}&status=${status}`
      );
    } catch (error) {
      console.error('Error fetching patient billing data:', error);
      // Return empty array on error for consistent handling
      return [];
    }
  }

  async fetchHealthTrendInfo(
    patientId: string,
    programId: string,
    daycount: number,
    convertDate: (date: Date) => string,
    convertToUTCRangeInput: (date: Date) => string
  ): Promise<any> {
    try {
      const utcdate1 = convertToUTCRangeInput(
        new Date(convertDate(new Date()) + 'T23:59:59')
      );

      const date2 = new Date();
      date2.setDate(date2.getDate() - (daycount - 1));
      const utcdate2 = convertToUTCRangeInput(
        new Date(convertDate(date2) + 'T00:00:00')
      );
      const apiUrl = '/api/patient/getpatienthealthtrends';
      const url = `${apiUrl}?PatientId=${patientId}&PatientProgramId=${programId}&StartDate=${utcdate2}&EndDate=${utcdate1}`;

      return await this.rpmService.rpm_get(url);
    } catch (error) {
      console.error(
        `❌ Error fetching health trends for PatientId=${patientId}:`,
        error
      );
      throw new Error('Failed to fetch health trends data. Please try again.');
    }
  }

  /** Convert Date for Health Trends Display */
  convertDateforHealthTrends(dateArr: any[], http_healthtrends: any): string[] {
    const MONTHS = [
      'Jan',
      'Feb',
      'Mar',
      'Apr',
      'May',
      'Jun',
      'Jul',
      'Aug',
      'Sep',
      'Oct',
      'Nov',
      'Dec',
    ];

    return dateArr.map((dateval) => {
      let newdate =
        http_healthtrends.Values[0].label !== 'Vital'
          ? this.patientutil.convertToLocalTime(dateval)
          : dateval;

      const [datePart] = newdate.includes('T')
        ? newdate.split('T')
        : newdate.split(' ');
      const [year, month, day] = datePart.split('-');

      const formattedDate = `${MONTHS[parseInt(month) - 1]}-${day}`;
      const formattedTime = this.datepipe.transform(newdate, 'h:mm a');

      return `${formattedDate} - ${formattedTime}`;
    });
  }

  async getLatestAlertsAndTasks(
    patientId: string,
    programId: string,
    convertDate: (date: Date) => string
  ): Promise<{ latestTasks: any; allTasks: any }> {
    try {
      const currentDate = new Date();
      const startdate = this.auth.ConvertToUTCRangeInput(
        new Date(`${convertDate(currentDate)}T00:00:00`)
      );
      const enddate = this.auth.ConvertToUTCRangeInput(
        new Date(`${convertDate(currentDate)}T23:59:59`)
      );
      const apiUrl = '/api/patient/getpatientAlertandtask';
      const url = `${apiUrl}?PatientId=${patientId}&PatientProgramId=${programId}&StartDate=${startdate}&EndDate=${enddate}`;

      // Explicitly cast the response as AlertOrTask[]
      const allAlertsAndTasks: any = await this.rpmService.rpm_get(url);

      if (!Array.isArray(allAlertsAndTasks)) {
        console.warn('⚠ API response is not an array:', allAlertsAndTasks);
        return { latestTasks: [], allTasks: [] };
      }

      // Filter and transform data
      const openAlertsAndTasks = allAlertsAndTasks.filter(
        (item) => item.Status === 'ToDo'
      );

      // Get the latest 3 tasks
      const latestTasks = openAlertsAndTasks.slice(0, 3).map((task) => ({
        tasktype: task.Type,
        priority: task.Priority,
        assigned: task.AssignedMember,
        type: task.TaskOrAlert,
      }));

      return { latestTasks, allTasks: allAlertsAndTasks };
    } catch (error) {
      console.error('❌ Error fetching latest alerts and tasks:', error);
      return { latestTasks: [], allTasks: [] };
    }
  }

  /** Fetch review notes by patient note ID */

  /**
    Get Clinic Details
   */
  getClinicDetails(masterData: any, organizationId: any) {
    return (
      masterData?.ClinicDetails.find(
        (clinic: { Id: any }) => clinic.Id === parseInt(organizationId)
      ) || null
    );
  }

  /**
    Get City Name
   */
  getCityName(statesAndCities: any, cityId: any): string | null {
    return (
      statesAndCities?.Cities.find(
        (c: { CityId: any }) => c.CityId === parseInt(cityId)
      )?.CityName || null
    );
  }
  /**
    Get Program Name
   */
  getProgramName(programDetails: any, programId: any): string {
    return (
      programDetails.find((c: { ProgramId: any }) => c.ProgramId === programId)
        ?.ProgramName || programId
    );
  }
}
