import { Injectable } from '@angular/core';
import { RPMService } from '../../../sevices/rpm.service';
import { AuthService } from 'src/app/services/auth.service';
import jsPDF from 'jspdf';

@Injectable({
  providedIn: 'root',
})
export class PatientReportApiService {
  constructor(private rpmService: RPMService, private auth: AuthService) {}

  async getPatientInfo(patientId: string, programId: string): Promise<any> {
    return this.rpmService.rpm_get(
      `/api/patient/getpatient?PatientId=${patientId}&PatientProgramId=${programId}`
    );
  }

  async getTrends(
    patientId: string,
    programId: string,
    startDate: string,
    endDate: string
  ): Promise<any> {
    return this.rpmService.rpm_get(
      `/api/patient/getpatienthealthtrends?PatientId=${patientId}&PatientProgramId=${programId}&StartDate=${startDate}&EndDate=${endDate}`
    );
  }

  async getVitalReading(
    patientId: string,
    programId: string,
    startDate: string,
    endDate: string
  ): Promise<any> {
    return this.rpmService.rpm_get(
      `/api/patient/getpatientvitalreadingsv1?PatientId=${patientId}&PatientProgramId=${programId}&StartDate=${startDate}&EndDate=${endDate}`
    );
  }

  async getMedication(patientId: string, programId: string): Promise<any> {
    return this.rpmService.rpm_get(
      `/api/patient/getpatientmedication?PatientId=${patientId}&PatientProgramId=${programId}`
    );
  }

  async getBillingData(
    patientId: string,
    programId: string,
    billedDate: string
  ): Promise<any> {
    return this.rpmService.rpm_get(
      `/api/patient/getpatientlastbilledcyclebydate?patientId=${patientId}&patientProgramId=${programId}&billeddate=${billedDate}`
    );
  }

  async getPatientSymptoms(patientId: string, programId: string): Promise<any> {
    return this.rpmService.rpm_get(
      `/api/patient/getpatientsymptoms?PatientId=${patientId}&PatientProgramId=${programId}`
    );
  }

  async getSMSData(
    patientId: string,
    programId: string,
    startDate: string,
    endDate: string
  ): Promise<any> {
    return this.rpmService.rpm_get(
      `/api/patient/getpatientsmshistory?PatientId=${patientId}&PatientProgramId=${programId}&StartDate=${startDate}&EndDate=${endDate}`
    );
  }

  async getVitalReading7Days(
    patientId: string,
    programId: string,
    startDate: string,
    endDate: string
  ): Promise<any> {
    return this.rpmService.rpm_get(
      `/api/patient/getpatientvitalreadingsv1?PatientId=${patientId}&PatientProgramId=${programId}&StartDate=${startDate}&EndDate=${endDate}`
    );
  }

  async getPatientNotes(
    patientId: string,
    patientProgramId: string,
    noteType: string,
    startDate: string,
    endDate: string
  ): Promise<any> {
    const startReportDate = startDate.replace(/\.\d{3}Z$/, '');
    const endReportDate = endDate.replace(/\.\d{3}Z$/, '');
    const apiUrl = `/api/patient/getpatientnotes?PatientId=${patientId}&PatientProgramId=${patientProgramId}&NoteType=${noteType}&StartDate=${startReportDate}&EndDate=${endReportDate}`;

    try {
      return await this.rpmService.rpm_get(apiUrl);
    } catch (error) {
      console.error(`Error fetching ${noteType} patient notes:`, error);
      return [];
    }
  }

  // ✅ Fetch Note Details by ID (for REVIEW or CALL)
  async getNoteDetails(
    programName: string,
    noteType: string,
    noteId: string
  ): Promise<any> {
    const apiUrl = `/api/patient/getpatientnotesbyprogram?ProgramName=${programName}&Type=${noteType}&PatientNoteId=${noteId}`;

    try {
      return await this.rpmService.rpm_get(apiUrl);
    } catch (error) {
      console.error(
        `Error fetching ${noteType} note details for ID ${noteId}:`,
        error
      );
      return null;
    }
  }
}
