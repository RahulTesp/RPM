import { DatePipe } from '@angular/common';
import { Injectable } from '@angular/core';
import jsPDF from 'jspdf';
import { ReportDataService } from './report-data.service';
import moment from 'moment';
import { RPMService } from '../../../sevices/rpm.service';
import autoTable from 'jspdf-autotable';
import { PatientUtilService } from '../../patient-detail-page/Models/service/patient-util.service';
import html2canvas from 'html2canvas';
// import Chart from 'chart.js';
import { PatientReportApiService } from './patient-report-api.service';

@Injectable({
  providedIn: 'root',
})
export class DownloadPatientReportService {
  constructor(
    private datepipe: DatePipe,
    private patientReportService: ReportDataService,
    private rpmService: RPMService,
    private patientUtilService: PatientUtilService,
    private patientReportApiService: PatientReportApiService
  ) {}
  private Notesh: number = 30;
  private vitalsConsolidated: any[] = []; // 🟢 Store consolidated vitals
  private VitalsSevenDaysConsolidated: any[] = []; // 🟢 Store last 7 days vitals
  private reviewNotes: any;
  private callNotes: any;

  /**
   * Set main heading style for PDF
   */
  setMainHeadingStyle(doc: jsPDF): void {
    doc.setFont('helvetica', 'bold');
    doc.setFontSize(12);
    doc.setTextColor('#590CA7');
  }

  generatePatientAndProgramInfo(
    doc: jsPDF,
    patientDetails: any,
    programDetails: any,
    patientProgramname: string
  ): number {
    let textHeight = 70;

    // Patient Information Section
    this.setMainHeadingStyle(doc);
    doc.text('Patient Information', 10, textHeight);
    this.setContentStyle(doc);

    // Patient name
    doc.setDrawColor('#818495');
    textHeight = textHeight + 5;
    doc.line(10, textHeight, 200, textHeight);
    doc.setTextColor('#818495');
    textHeight = textHeight + 5;
    doc.text('Patient Name : ', 30, textHeight);
    doc.setTextColor('black');
    doc.text(
      patientDetails['PatientDetails'].FirstName +
        ' ' +
        patientDetails['PatientDetails'].LastName,
      60,
      textHeight
    );

    // Patient ID
    textHeight = textHeight + 5;
    doc.line(10, textHeight, 200, textHeight);
    doc.setTextColor('#818495');
    textHeight = textHeight + 5;
    doc.text('Patient Id : ', 30, textHeight);
    doc.setTextColor('black');
    doc.text(patientDetails['PatientDetails'].UserName, 60, textHeight);
    textHeight = textHeight + 5;

    // Date of Birth and Height
    doc.line(10, textHeight, 200, textHeight);
    doc.setTextColor('#818495');
    textHeight = textHeight + 5;
    doc.text('Date of Birth : ', 30, textHeight);
    doc.setTextColor('black');

    doc.text(
      this.formatDate(patientDetails['PatientDetails'].DOB),
      60,
      textHeight
    );
    doc.setTextColor('#818495');
    doc.text('Height : ', 130, textHeight);
    doc.setTextColor('black');
    doc.text(
      patientDetails['PatientDetails'].Height.toString(),
      150,
      textHeight
    );

    // Gender and Weight
    textHeight = textHeight + 5;
    doc.line(10, textHeight, 200, textHeight);
    doc.setTextColor('#818495');
    textHeight = textHeight + 5;
    doc.text('Gender : ', 30, textHeight);
    doc.setTextColor('black');
    doc.text(patientDetails['PatientDetails'].Gender, 60, textHeight);
    doc.setTextColor('#818495');
    doc.text('Weight : ', 130, textHeight);
    doc.setTextColor('black');
    doc.text(
      patientDetails['PatientDetails'].Weight.toString(),
      150,
      textHeight
    );
    textHeight = textHeight + 5;
    doc.line(10, textHeight, 200, textHeight);
    textHeight = textHeight + 20;

    // Program Information Section
    this.setMainHeadingStyle(doc);
    doc.text('Program Information', 10, textHeight);
    this.setContentStyle(doc);

    // Program Name
    textHeight = textHeight + 5;
    doc.line(10, textHeight, 200, textHeight);
    doc.setTextColor('#818495');
    textHeight = textHeight + 5;
    doc.text('Program Name : ', 26, textHeight);
    doc.setTextColor('black');

    const program_name = patientDetails.PatientProgramdetails.ProgramId;
    const pgm = programDetails.filter(
      (c: { ProgramId: any }) => c.ProgramId === program_name
    );
    if (pgm.length > 0) {
      doc.text(pgm[0].ProgramName, 60, textHeight);
    }

    // Program Start Date and Duration
    textHeight = textHeight + 5;
    doc.line(10, textHeight, 200, textHeight);
    doc.setTextColor('#818495');
    textHeight = textHeight + 5;
    doc.text('Program Start Date : ', 20, textHeight);
    doc.setTextColor('black');

    doc.text(
      this.formatDate(patientDetails['PatientProgramdetails'].StartDate),
      60,
      textHeight
    );

    doc.setTextColor('#818495');
    doc.text('Duration : ', 130, textHeight);
    doc.setTextColor('black');
    doc.text(
      patientDetails['PatientProgramdetails'].Duration.toString(),
      150,
      textHeight
    );
    textHeight = textHeight + 5;

    // Vitals or Condition Monitored
    doc.line(10, textHeight, 200, textHeight);
    doc.setTextColor('#818495');
    textHeight = textHeight + 5;
    if (patientProgramname == 'CCM' || patientProgramname == 'PCM') {
      doc.text('Condition Monitored : ', 18, textHeight);
    } else {
      doc.text('Vitals Monitored : ', 18, textHeight);
    }
    doc.setTextColor('black');

    const patiantVital = patientDetails[
      'PatientProgramdetails'
    ].PatientVitalInfos.filter(
      (ds: { Selected: boolean }) => ds.Selected == true
    );

    let vitals = '';
    for (const vital of patiantVital) {
      vitals += vital['Vital'].toString();
      if (patiantVital.length > 1) {
        vitals += '\n';
      }
    }

    const splitVitals = doc.splitTextToSize(vitals, 150);
    doc.text(splitVitals, 60, textHeight);

    const vitalsDim = doc.getTextDimensions(splitVitals);
    textHeight = textHeight + vitalsDim.h;
    doc.line(10, textHeight, 200, textHeight);

    // Program Enrollment
    textHeight = textHeight + 5;
    doc.setTextColor('#818495');
    doc.text('Program Enrollment : ', 18, textHeight);
    doc.setTextColor('black');

    doc.text(
      this.formatDate(patientDetails['PatientProgramdetails'].StartDate),
      60,
      textHeight
    );

    // Diagnosis
    textHeight = textHeight + 5;
    doc.line(10, textHeight, 200, textHeight);
    doc.setTextColor('#818495');
    textHeight = textHeight + 5;
    doc.text('Diagnosis : ', 35, textHeight);
    doc.setTextColor('black');

    let diagnosis = '';
    for (const diag of patientDetails['PatientPrescribtionDetails']
      .PatientDiagnosisInfos) {
      diagnosis += (
        diag['DiagnosisName'] +
        ' - ' +
        diag['DiagnosisCode']
      ).toString();
      if (
        patientDetails['PatientPrescribtionDetails'].PatientDiagnosisInfos
          .length > 1
      ) {
        diagnosis += ',\n';
      }
    }

    const splitDiagnosis = doc.splitTextToSize(diagnosis, 150);
    doc.text(splitDiagnosis, 60, textHeight);

    const diagnosisDim = doc.getTextDimensions(splitDiagnosis);
    textHeight = textHeight + diagnosisDim.h;
    doc.line(10, textHeight, 200, textHeight);
    doc.setTextColor('#818495');

    // Management Period
    textHeight = textHeight + 5;
    doc.text('Management Period : ', 20, textHeight);
    doc.setTextColor('black');
    const pStart = this.datepipe.transform(
      patientDetails['PatientProgramdetails'].StartDate
    );
    const currentDay = this.datepipe.transform(new Date());
    doc.text(pStart + ' - ' + currentDay, 60, textHeight);
    textHeight = textHeight + 10;
    doc.line(10, textHeight, 200, textHeight);

    // Signature and Date
    textHeight = textHeight + 50;
    doc.line(28, textHeight, 95, textHeight);
    doc.line(120, textHeight, 195, textHeight);
    doc.setTextColor('#818495');
    doc.text('Signature ', 10, textHeight);
    doc.text('Date ', 110, textHeight);

    return textHeight; // Return the current height for further additions to the PDF
  }

  async processHealthTrends(
    doc: jsPDF,
    healthTrends: any
  ): Promise<{ chartData: any; chartLabels: string[] }> {
    if (
      !healthTrends ||
      !healthTrends.Values ||
      healthTrends.Values.length === 0
    ) {
      return this.generateEmptyChartData();
    }

    // Convert dates to readable format
    const chartLabels = this.convertDateforHealthTrends(healthTrends.Time);

    // Process data points and clean up null values
    let valuesArray = JSON.parse(JSON.stringify(healthTrends.Values)); // Deep copy

    // Filter out null values
    let j = 0;
    for (const item of valuesArray) {
      let i = 0;
      for (const value of item.data) {
        if (j === 0) {
          try {
            if (value === null && i > 0 && i < item.data.length) {
              const linedt1 = chartLabels[i].split(' - ');
              const linedt0 = chartLabels[i - 1].split(' - ');
              const linedt2 = chartLabels[i + 1].split(' - ');

              if (linedt1[0] === linedt0[0] || linedt1[0] === linedt2[0]) {
                chartLabels.splice(i, 1);

                // Remove the corresponding data point from all series
                let k = 0;
                for (const tmpItem of valuesArray) {
                  valuesArray[k].data.splice(i, 1);
                  k++;
                }
              }
            }
            i++;
          } catch (ex) {
            console.error('Error processing health trend data:', ex);
          }
        }
      }
      j++;
    }

    // Format data for chart
    const chartData: any = valuesArray.map(
      (item: { data: any; label: any }) => ({
        data: item.data,
        label: item.label,
        fill: false,
        lineTension: 0.5,
      })
    );

    //// Start
    // const canvas = document.createElement('canvas');
    // canvas.width = 400;
    // canvas.height = 400;
    // const ctx = canvas.getContext('2d');

    // if (ctx) {
    //   // Create chart in memory
    //   const chart = new Chart(ctx, {
    //     type: 'line',
    //     data: {
    //       labels: chartLabels,
    //       datasets: chartData,
    //     },
    //     options: {
    //       responsive: false,
    //       plugins: {
    //         title: {
    //           display: true,
    //           text: 'Health Trends (No Data)',
    //         },
    //         legend: {
    //           display: true,
    //           labels: {
    //             color: '#4CAF50', // ✅ Legend label color (green in this case)
    //             font: {
    //               size: 12,
    //               weight: 'bold',
    //             },
    //           },
    //         },
    //       },
    //       scales: {
    //         // Declare the axis ID explicitly
    //         y: {
    //           type: 'linear',
    //           beginAtZero: true,
    //         },
    //         x: {
    //           type: 'category',
    //         },
    //       },
    //     } as any, // 👈 Easiest workaround to avoid TS errors
    //   });

    //   // Give chart some time to render (important for async chart draw)
    //   setTimeout(() => {
    //     const imgData = canvas.toDataURL('image/png');
    //     // const pdf = new jsPDF('landscape', 'mm', 'a4');
    //     const pdfWidth = doc.internal.pageSize.getWidth();
    //     const pdfHeight = doc.internal.pageSize.getHeight();
    //     const aspectRatio = canvas.width / canvas.height;
    //     const imgHeight = pdfWidth / aspectRatio;

    //     doc.addImage(imgData, 'PNG', 0, 30, pdfWidth, imgHeight);
    //     // doc.save('health-trend-empty.pdf');

    //     chart.destroy(); // Cleanup
    //   }, 500);
    // } else {
    //   console.error('Error processing Generating the Graph');
    // }

    return { chartData, chartLabels };
  }

  /**
   * Generate empty chart data when no data is available
   */
  generateEmptyChartData(): { chartData: any; chartLabels: string[] } {
    const date = new Date();
    const defaultDates: string[] = [];

    // Generate 7 days of dates
    for (let i = 0; i < 7; i++) {
      const formattedDate = this.patientReportService.convertDate(
        new Date(date)
      );
      defaultDates.push(formattedDate);
      date.setDate(date.getDate() - 1);
    }

    // Reverse to get chronological order
    const timeArray = defaultDates.reverse();

    // Create empty health trend data
    const emptyData: any = {
      VitalName: 'No Data',
      VitalId: 1,
      Time: timeArray,
      Values: [
        { data: [null, null, null, null, null, null, null], label: 'Vital' },
      ],
    };

    // Process empty data
    const chartLabels = this.convertDateforHealthTrends(emptyData.Time);
    const chartData = emptyData.Values.map(
      (item: { data: any; label: any }) => ({
        data: item.data,
        label: item.label,
        fill: false,
        lineTension: 0.5,
      })
    );

    return { chartData, chartLabels };
  }

  /**
   * Convert date array to formatted labels for chart
   */
  convertDateforHealthTrends(dateArr: string[]): string[] {
    const formattedDates: string[] = [];

    for (const dateVal of dateArr) {
      const dateParts = dateVal.split('+');
      const localDateTime = this.convertToLocalTime(dateParts[0]);
      const dateSplit = localDateTime.split(' ')[0].split('-');

      // Convert month number to abbreviation
      const monthMap: { [key: string]: string } = {
        '01': 'Jan',
        '02': 'Feb',
        '03': 'Mar',
        '04': 'Apr',
        '05': 'May',
        '06': 'Jun',
        '07': 'Jul',
        '08': 'Aug',
        '09': 'Sep',
        '10': 'Oct',
        '11': 'Nov',
        '12': 'Dec',
      };

      const month = monthMap[dateSplit[1]] || '';
      const day = dateSplit[2];
      const date = month + '-' + day;

      // Format time
      const time =
        this.datepipe.transform(
          this.convertToLocalTime(dateParts[0]),
          'h:mm a'
        ) || '';

      // Combine date and time
      formattedDates.push(date + ' - ' + time);
    }

    return formattedDates;
  }

  /**
   * Convert UTC time to local time
   */
  convertToLocalTime(stillUtc: any) {
    stillUtc = stillUtc + 'Z';
    var local = moment(stillUtc).local().format('YYYY-MM-DD HH:mm:ss');
    return local;
  }

  // ✅ Extract Critical Alerts from Symptoms
  extractCriticalAlerts(symptoms: any[]): any[] {
    if (!symptoms || symptoms.length === 0) return [];

    return symptoms
      .filter((data) => data.Symptom === 'Critical Event')
      .map((data) => ({
        Priority: 'Critical',
        VitalAlert: data.Description,
        Time: data.SymptomStartDateTime,
      }));
  }

  // ✅ Generate Program Goals Report
  generateProgramGoalsReport(
    doc: jsPDF,
    patientGoals: any,
    criticalAlerts: any[]
  ): void {
    let goalh = 30;
    doc.setTextColor('black');

    // ✅ Render Patient Goals
    if (patientGoals?.goalDetails?.length) {
      goalh = this.renderGoals(doc, patientGoals.goalDetails, goalh);
    }

    // ✅ Render Critical Events
    this.renderCriticalEvents(doc, criticalAlerts, goalh);
  }

  // ✅ Render Goals
  private renderGoals(doc: jsPDF, goals: any[], startHeight: number): number {
    let goalh = startHeight;
    goals.forEach((goal) => {
      this.setSubHeadingStyle(doc);
      doc.text(goal.Goal, 15, goalh);

      const textWidth = doc.getTextWidth(goal.Goal);
      doc.setDrawColor('black');
      doc.line(15, goalh + 1, 15 + textWidth, goalh + 1);

      this.setContentStyle(doc);
      goalh += 10;
      doc.text(goal.Description, 20, goalh);
      goalh += 25;
    });
    return goalh;
  }

  // ✅ Render Critical Events
  private renderCriticalEvents(
    doc: jsPDF,
    alerts: any[],
    startHeight: number
  ): void {
    let goalh = startHeight;

    this.setSubHeadingStyle(doc);
    const eventsTitle = 'Critical Events';
    doc.text(eventsTitle, 15, goalh);

    const textWidth = doc.getTextWidth(eventsTitle);
    doc.setDrawColor('black');
    doc.line(15, goalh + 1, 15 + textWidth, goalh + 1);
    goalh += 10;

    this.setContentStyle(doc);

    if (alerts.length > 0) {
      alerts.forEach((alert) => {
        const formattedDate = this.formatDate(alert.Time);
        doc.text(formattedDate, 20, goalh);

        const dateWidth = doc.getTextWidth(formattedDate);
        doc.setDrawColor('black');
        doc.line(20, goalh + 1, 20 + dateWidth, goalh + 1);
        goalh += 10;

        doc.text(alert.VitalAlert, 20, goalh);
        goalh += 10;
      });
    } else {
      doc.text('No Data', 20, goalh);
    }
  }
  //Notes

  // ✅ Fetch and Populate Notes Details Recursively
  async populateNotesDetails(
    notes: any[],
    programName: string,
    noteType: string
  ): Promise<any[]> {
    for (let i = 0; i < notes.length; i++) {
      const details = await this.patientReportApiService.getNoteDetails(
        programName,
        noteType,
        notes[i].Id
      );
      notes[i]['NoteDetails'] = details;
      notes[i]['Type'] = noteType;
    }

    return notes;
  }

  generateCallNotesReport(doc: jsPDF, data: any[]): void {
    this.Notesh = 30;
    this.setSubHeadingStyle(doc);

    this.setPages(doc, 'Notes', 15);
    this.drawLine(doc, 'Notes', 15);

    this.Notesh += 10;
    doc.setFontSize(14);

    for (let notes of data) {
      this.Notesh += 5;
      this.setSubHeadingStyle(doc);
      const formattedDate = this.datepipe.transform(
        this.convertToLocalTime(notes.CreatedOn)
      ) as string;

      this.setPages(doc, formattedDate, 20);
      this.drawLine(doc, formattedDate, 20);

      this.setContentStyle(doc);
      this.Notesh += 5;
      this.setPages(
        doc,
        `Duration : ${this.patientReportService.timeConvert(notes.Duration)}`,
        20
      );
      this.Notesh += 5;
      this.setPages(doc, `Completed By : ${notes.CompletedBy}`, 20);
      this.Notesh += 5;
      this.setPages(doc, `Note Type : ${notes.NoteType}`, 20);
      this.Notesh += 5;
      this.setPages(doc, `Type : ${notes.Type}`, 20);
      this.Notesh += 5;

      if (notes.Type !== 'REVIEW') {
        this.setPages(
          doc,
          `Call Established : ${notes.IsEstablished ? 'Yes' : 'No'}`,
          20
        );
        this.Notesh += 5;
        this.setPages(
          doc,
          `Care Giver : ${notes.IsCareGiver ? 'Yes' : 'No'}`,
          20
        );
        this.Notesh += 5;
      }

      this.Notesh += 5;
      this.processNoteDetails(doc, notes);
      this.Notesh += 3;
    }

    doc.addPage();
    this.setMainHeadingStyle(doc);
    // this.Report_HealthTrends();
  }

  // ✅ Process Note Details
  private processNoteDetails(doc: jsPDF, notes: any): void {
    if (!notes.NoteDetails) return;

    for (let mainQuestion of notes.NoteDetails.MainQuestions) {
      this.Notesh += 5;
      this.setPages(doc, mainQuestion.Question, 20);
      this.drawLine(doc, mainQuestion.Question, 20);
      this.Notesh += 5;

      let hasCheckedAnswer = mainQuestion.AnswerTypes.some(
        (answer: { Checked: any }) => answer.Checked
      );
      if (hasCheckedAnswer) {
        mainQuestion.AnswerTypes.filter(
          (answer: { Checked: any }) => answer.Checked
        ).forEach((answer: { Answer: any }) => {
          this.setPages(doc, `- ${answer.Answer}`, 25);
          this.Notesh += 5;
        });
      } else {
        this.setPages(doc, '- None', 25);
        this.Notesh += 5;
      }

      this.processSubQuestions(doc, mainQuestion.SubQuestions);

      if (mainQuestion.Notes) {
        this.setPages(doc, `Extra Notes : ${mainQuestion.Notes}`, 25);
        this.Notesh += 10;
      }
    }

    if (notes.NoteDetails.Notes) {
      this.Notesh += 5;
      this.setPages(doc, 'Additional Notes', 20);
      this.drawLine(doc, 'Additional Notes', 20);
      this.Notesh += 5;
      this.setPages(doc, notes.NoteDetails.Notes, 25);
      this.Notesh += 15;
    }
  }

  // ✅ Process Sub Questions
  private processSubQuestions(doc: jsPDF, subQuestions: any[]): void {
    subQuestions.forEach((subQuestion) => {
      this.Notesh += 5;
      this.setPages(doc, subQuestion.Question, 25);
      this.drawLine(doc, subQuestion.Question, 25);
      this.Notesh += 5;

      let hasCheckedAnswer = subQuestion.AnswerTypes.some(
        (answer: { Checked: any }) => answer.Checked
      );
      if (hasCheckedAnswer) {
        subQuestion.AnswerTypes.filter(
          (answer: { Checked: any }) => answer.Checked
        ).forEach((answer: { Answer: any }) => {
          this.setPages(doc, `- ${answer.Answer}`, 25);
          this.Notesh += 5;
        });
      } else {
        this.setPages(doc, '- None', 25);
        this.Notesh += 5;
      }
    });
  }
  /**
   * Gets patient review notes, then call notes, and generates report sections
   */
  async getReportNotes(
    doc: any,
    rpmStartDate: any,
    rpmEndDate: any,
    currentPatient: any,
    currentProgram: any,
    patientProgramName: any
  ): Promise<any[]> {
    try {
      // ✅ Fetch REVIEW Notes
      this.reviewNotes = await this.patientReportApiService.getPatientNotes(
        currentPatient,
        currentProgram,
        'REVIEW',
        rpmStartDate,
        rpmEndDate
      );

      if (this.reviewNotes.length > 0) {
        this.reviewNotes = await this.populateNotesDetails(
          this.reviewNotes,
          patientProgramName,
          'REVIEW'
        );
      }

      // Await the call notes function to ensure it completes
      const combinedNotes = await this.getReportCallNotes(
        doc,
        rpmStartDate,
        rpmEndDate,
        currentPatient,
        currentProgram,
        patientProgramName
      );

      return combinedNotes;
    } catch (error) {
      console.error('Error fetching review notes:', error);
      return [];
    }
  }

  /**
   * Gets patient call notes, combines with review notes, and generates report
   */
  async getReportCallNotes(
    doc: any,
    rpmStartDate: any,
    rpmEndDate: any,
    currentPatient: any,
    currentProgram: any,
    patientProgramName: any
  ): Promise<any[]> {
    try {
      // Fetch CALL Notes
      this.callNotes = await this.patientReportApiService.getPatientNotes(
        currentPatient,
        currentProgram,
        'CALL',
        rpmStartDate,
        rpmEndDate
      );

      // Process call notes if any exist
      if (this.callNotes.length > 0) {
        this.callNotes = await this.populateNotesDetails(
          this.callNotes,
          patientProgramName,
          'CALL'
        );
      }

      // Combine review and call notes
      const combinedNotes = this.combineNotes(this.reviewNotes, this.callNotes);

      // Generate report section with combined notes
      await this.printCallNotes(doc, combinedNotes);

      return combinedNotes;
    } catch (error) {
      console.error('Error fetching call notes:', error);
      return [];
    }
  }

  /**
   * Combine review and call notes
   */
  combineNotes(reviewNotes: any[] = [], callNotes: any[] = []): any[] {
    const combinedList = reviewNotes.concat(callNotes);

    combinedList.sort((a: any, b: any) => {
      return (a.CreatedOn || a.CreatedOn).localeCompare(
        b.CreatedOn || b.CreatedOn
      );
    });

    return combinedList;
  }

  /**
   * Generate call notes report in PDF
   */
  async printCallNotes(doc: jsPDF, data: any): Promise<void> {
    const notesData = data;
    await this.generateCallNotesReport(doc, notesData);
    return Promise.resolve();
  }

  /**
   * Generate call notes report content
   */

  symptomh: any;
  generatePatientSummaryReport(
    doc: jsPDF,
    patientProgramname: string,
    symptoms: any[],
    medications: any[],
    billingInfo: any[],
    smsData: any[]
  ): void {
    if (patientProgramname !== 'CCM' && patientProgramname !== 'PCM') {
      doc.addPage();
    }

    this.setMainHeadingStyle(doc);
    doc.text('Patient Program Summary Report', 10, 20);
    this.symptomh = 30;

    // ✅ Render Symptoms Section
    this.renderSection(doc, 'Symptoms', symptoms, (doc, data) => {
      this.processSymptoms(doc, data);
    });
    doc.addPage();
    // ✅ Render Medications Section
    this.renderSection(doc, 'Medications', medications, (doc, data) => {
      this.processMedications(doc, data);
    });
    doc.addPage();
    // ✅ Render Billing Information
    this.renderSection(doc, 'Billing Information', billingInfo, (doc, data) => {
      this.processBillingInfo(doc, data);
    });
    doc.addPage();
    // ✅ Render SMS Data
    this.renderSection(doc, 'SMS Data', smsData, (doc, data) => {
      this.processSmsData(doc, data);
    });
  }

  // ✅ Render Generic Section with AutoTable
  private renderSection(
    doc: jsPDF,
    title: string,
    data: any[],
    renderFunction: (doc: jsPDF, data: any[]) => void
  ): void {
    this.symptomh = 30;
    this.setSubHeadingStyle(doc);

    doc.text(title, 15, this.symptomh);

    this.drawsymptomsLine(doc, title, 15);
    this.symptomh += 10;

    if (data.length > 0) {
      renderFunction(doc, data);
    } else {
      doc.text('', 15, this.symptomh + 10);
    }
  }

  // ✅ Process Symptoms
  private processSymptoms(doc: jsPDF, symptoms: any[]): void {
    if (!symptoms || symptoms.length === 0) {
      console.error('Symptoms data is empty or undefined:', symptoms);
      return;
    }

    if (!doc) {
      console.error('PDF Document (doc) is undefined!');
      return;
    }

    symptoms.forEach((symptom) => {
      this.setSymptomPages(
        doc,
        `Date: ${this.Report_ConvertDate(symptom.SymptomStartDateTime)}`,
        20
      );
      this.symptomh += 3;
      doc.line(20, this.symptomh, 100, this.symptomh);
      this.symptomh += 5;
      this.setSymptomPages(
        doc,
        `${symptom.Symptom}: ${symptom.Description}`,
        20
      );
      this.symptomh += 25;
    });
  }

  // ✅ Process Medications
  private processMedications(doc: jsPDF, medications: any[]): void {
    const columns = [
      'Medicine Name',
      'Schedule',
      'Intervals',
      'Start Date',
      'End Date',
    ];
    const rows = medications.map((med) => [
      med.Medicinename,
      med.MedicineSchedule,
      this.ProcessSchedules(med),
      this.datepipe.transform(
        this.convertToLocalTime(med.StartDate)!,
        'MMM d, y'
      ),
      this.datepipe.transform(
        this.convertToLocalTime(med.EndDate)!,
        'MMM d, y'
      ),
    ]);

    autoTable(doc, {
      head: [columns],
      body: rows,
      startY: 35,
    });
  }

  // ✅ Process Billing Information
  private processBillingInfo(doc: jsPDF, billingInfo: any[]): void {
    const columns = ['CPTCode', 'Last Billing Cycle', 'Reading', 'Status'];
    const rows = billingInfo.map((bill) => [
      bill.CPTCode,
      this.billCycledisplay(bill.Last_Billing_Cycle),
      this.convertBillingSec(bill.CPTCode, bill.reading),
      bill.status,
    ]);

    autoTable(doc, {
      head: [columns],
      body: rows,
      startY: 35,
    });
  }
  ConvertMinute(timeValue: number): string {
    const minutes = Math.floor(timeValue / 60);
    const seconds = timeValue % 60;
    return `${minutes}:${seconds.toString().padStart(2, '0')}`;
  }

  convertBillingSec(cptCode: string, timeValue: number): string {
    return cptCode === '99453' || cptCode === '99454'
      ? timeValue.toString()
      : this.ConvertMinute(timeValue);
  }

  billCycledisplay(data: string | null): string {
    return data ?? 'Not Available';
  }
  // ✅ Process SMS Data
  private processSmsData(doc: jsPDF, smsData: any[]): void {
    const columns = ['Date', 'Time', 'Sender', 'Message'];
    const rows = smsData.map((data) => [
      this.formatBasedonDate(
        this.convertToLocalTime(data.SentDate),
        'MMM d, y'
      ),
      this.formatBasedonDate(this.convertToLocalTime(data.SentDate), 'h:mm a'),
      data.Sender,
      data.Body.replace(/\s+/g, ' ').trim(),
    ]);

    autoTable(doc, {
      head: [columns],
      body: rows,
      startY: 35,
      columnStyles: {
        0: { cellWidth: 25 },
        1: { cellWidth: 20 },
        2: { cellWidth: 30 },
      },
    });
  }

  // ✅ Process Medication Schedules
  ProcessSchedules(med: any): string {
    const scheduleMap: { [key: string]: string } = {
      Morning: 'M',
      AfterNoon: 'AN',
      Evening: 'E',
      Night: 'N',
    };

    const intervals: string[] = [];

    for (const key in scheduleMap) {
      if (med[key]) {
        intervals.push(this.Report_IntervalFlag(med[key], scheduleMap[key]));
      }
    }

    return intervals.length > 0 ? intervals.join(' - ') : 'None';
  }

  generateHealthTrendsTable(doc: jsPDF, vitalData: any): void {
    this.vitalsConsolidated = [];
    const vitals = {
      BloodGlucose: this.patientUtilService.newVitalreadingData(
        vitalData.BloodGlucose,
        'BloodGlucoseReadings'
      ),
      BloodPressure: this.patientUtilService.newVitalreadingData(
        vitalData.BloodPressure,
        'BloodPressureReadings'
      ),
      BloodOxygen: this.patientUtilService.newVitalreadingData(
        vitalData.BloodOxygen,
        'BloodOxygenReadings'
      ),
      Weight: this.patientUtilService.newVitalreadingData(
        vitalData.Weight,
        'WeightReadings'
      ),
    };

    if (vitals.BloodGlucose?.length > 0) {
      this.generateVitalTable(
        doc,
        'Blood Glucose Readings',
        [
          'Date',
          'Schedule',
          'Morning',
          'AfterNoon',
          'Evening',
          'Night',
          'Status',
        ],
        vitals.BloodGlucose,
        'BloodGlucoseReadings',
        this.BloodGlucoseReportDisplayTest
      );
    } else if (vitals.BloodPressure?.length > 0) {
      this.generateVitalTable(
        doc,
        'Blood Pressure Readings',
        ['Date', 'Time', 'SBP', 'DBP', 'Pulse', 'Remarks'],
        vitals.BloodPressure,
        'BloodPressureReadings'
      );
    } else if (vitals.BloodOxygen?.length > 0) {
      this.generateVitalTable(
        doc,
        'Blood Oxygen Readings',
        ['Date', 'Time', 'Oxygen', 'Pulse', 'Remarks'],
        vitals.BloodOxygen,
        'BloodOxygenReadings'
      );
    } else if (vitals.Weight?.length > 0) {
      this.generateVitalTable(
        doc,
        'Weight Readings',
        ['Date', 'Time', 'Schedule', 'Weight', 'Remarks'],
        vitals.Weight,
        'WeightReadings'
      );
    }
  }

  // ✅ Generate Vital Readings Table
  private generateVitalTable(
    doc: jsPDF,
    title: string,
    columns: string[],
    vitalData: any[],
    readingType: string,
    formatter?: (data: any) => string[]
  ): void {
    let rows: string[][] = [];

    for (let vital of vitalData) {
      for (let reading of vital[readingType]) {
        this.vitalsConsolidated.push(reading); // 🟢 Store consolidated readings

        let temp = formatter
          ? formatter(reading)
          : this.formatVitalRow(reading);
        rows.push(temp);
      }
    }

    autoTable(doc, {
      head: [columns],
      body: rows,
      startY: 105,
      didParseCell: this.formatTableCells,
    });
  }

  // ✅ Format Vital Row Data
  private formatVitalRow(reading: any): string[] {
    return [
      this.datepipe.transform(reading.ReadingTime) ?? 'N/A',
      this.datepipe.transform(reading.ReadingTime, 'h:mm a') ?? 'N/A',
      reading.Systolic ?? '-',
      reading.Diastolic ?? '-',
      reading.Pulse ?? '-',
      reading.Remarks ?? '-',
    ];
  }

  // ✅ Table Cell Styling (Colors for Critical & Cautious)
  private formatTableCells(data: any): void {
    const colorMap: Record<string, string> = {
      Critical: 'red',
      Cautious: 'orange',
    };

    const cellText = String(data.cell.text[0]); // Ensure it is a string

    if (colorMap[cellText]) {
      data.cell.styles.textColor = colorMap[cellText];
      Object.values(data.row.cells).forEach((cell: any) => {
        cell.styles.textColor = colorMap[cellText];
      });
    }
  }

  getTimefromDate(text: any) {
    const myArray = text.split(' ');
    return myArray[1];
  }

  BloodGlucoseReportDisplayTest(data: any) {
    var readingDate = data.ReadingTime;
    var vitalDataArray: any[] = [];

    var Morning = '00:00:00';
    var AfterNoon = '10:00:01';
    var Evening = '15:00:01';
    var Night = '21:00:01';

    var time = this.getTimefromDate(data.ReadingTime);

    if (Morning < time && time < AfterNoon) {
      if (data.Schedule == 'Fasting') {
        vitalDataArray.push(
          this.datepipe.transform(readingDate),
          'Fasting',
          data.BGmgdl + '\n' + this.datepipe.transform(readingDate, 'h:mm a'),
          null,
          null,
          null,
          data.Status
        );
      }

      if (data.Schedule == 'Non-Fasting') {
        vitalDataArray.push(
          this.datepipe.transform(readingDate),
          'Non-Fasting',
          data.BGmgdl + '\n' + this.datepipe.transform(readingDate, 'h:mm a'),
          null,
          null,
          null,
          data.Status
        );
      }
    } else if (AfterNoon < time && time < Evening) {
      if (data.Schedule == 'Fasting') {
        vitalDataArray.push(
          this.datepipe.transform(readingDate),
          'Fasting',
          null,
          data.BGmgdl + '\n' + this.datepipe.transform(readingDate, 'h:mm a'),
          null,
          null,
          data.Status
        );
      }

      if (data.Schedule == 'Non-Fasting') {
        vitalDataArray.push(
          this.datepipe.transform(readingDate),
          'Non-Fasting',
          null,
          data.BGmgdl + '\n' + this.datepipe.transform(readingDate, 'h:mm a'),
          null,
          null,
          data.Status
        );
      }
    } else if (Evening < time && time < Night) {
      if (data.Schedule == 'Fasting') {
        vitalDataArray.push(
          this.datepipe.transform(readingDate),
          'Fasting',
          null,
          null,
          data.BGmgdl + '\n' + this.datepipe.transform(readingDate, 'h:mm a'),
          null,
          data.Status
        );
      }
      if (data.Schedule == 'Non-Fasting') {
        vitalDataArray.push(
          this.datepipe.transform(readingDate),
          'Non-Fasting',
          null,
          null,
          data.BGmgdl + '\n' + this.datepipe.transform(readingDate, 'h:mm a'),
          null,
          data.Status
        );
      }
    } else if (Night < time) {
      if (data.Schedule == 'Fasting') {
        vitalDataArray.push(
          this.datepipe.transform(readingDate),
          'Fasting',
          null,
          null,
          null,
          data.BGmgdl + '\n' + this.datepipe.transform(readingDate, 'h:mm a'),

          data.Status
        );
      }
      if (data.Schedule == 'Non-Fasting') {
        vitalDataArray.push(
          this.datepipe.transform(readingDate),
          'Non-fasting',
          null,
          null,
          null,
          data.BGmgdl + '\n' + this.datepipe.transform(readingDate, 'h:mm a'),

          data.Status
        );
      }
    }
    return vitalDataArray;
  }

  // setVitalsData(vitalsSevenDays: any[]): void {
  //   this.VitalsSevenDaysConsolidated = vitalsSevenDays;
  // }

  generateVitalReadingSummary(doc: jsPDF): void {
    doc.addPage();
    this.setSubHeadingStyle(doc);

    const statSumm = 'Vital Readings - Status Summary';
    doc.text(statSumm, 15, 20);
    const textWidth = doc.getTextWidth(statSumm);
    doc.line(15, 21, 15 + textWidth, 21);

    const columns = [
      'Days of Reading',
      'Selected Last 7 days',
      'Selected Last 30 Days',
    ];
    const rows: string[][] = [];

    rows.push([
      'Total Days of Reading',
      this.VitalsSevenDaysConsolidated.length.toString(),
      this.vitalsConsolidated.length.toString(),
    ]);

    const critical7 = this.VitalsSevenDaysConsolidated.filter(
      (data) => data.Status === 'Critical'
    );
    const critical30 = this.vitalsConsolidated.filter(
      (data) => data.Status === 'Critical'
    );
    rows.push([
      'Critical Readings',
      critical7.length.toString(),
      critical30.length.toString(),
    ]);

    const cautious7 = this.VitalsSevenDaysConsolidated.filter(
      (data) => data.Status === 'Cautious'
    );
    const cautious30 = this.vitalsConsolidated.filter(
      (data) => data.Status === 'Cautious'
    );
    rows.push([
      'Cautious Readings',
      cautious7.length.toString(),
      cautious30.length.toString(),
    ]);

    autoTable(doc, {
      head: [columns],
      body: rows,
      startY: 30,
    });
  }
  generateDaysVitals(vitalData: any): void {
    this.VitalsSevenDaysConsolidated = [];

    const vitals = {
      BloodGlucose: this.patientUtilService.newVitalreadingData(
        vitalData.BloodGlucose,
        'BloodGlucoseReadings'
      ),
      BloodPressure: this.patientUtilService.newVitalreadingData(
        vitalData.BloodPressure,
        'BloodPressureReadings'
      ),
      BloodOxygen: this.patientUtilService.newVitalreadingData(
        vitalData.BloodOxygen,
        'BloodOxygenReadings'
      ),
      Weight: this.patientUtilService.newVitalreadingData(
        vitalData.Weight,
        'WeightReadings'
      ),
    };

    Object.values(vitals).forEach((vitalArray: any) => {
      if (vitalArray?.length > 0) {
        for (let vital of vitalArray) {
          const readingsKey = Object.keys(vital)[1]; // Extract reading key dynamically
          for (let reading of vital[readingsKey]) {
            this.VitalsSevenDaysConsolidated.push(reading);
          }
        }
      }
    });
  }

  getVitalsSevenDays(): any[] {
    return this.VitalsSevenDaysConsolidated;
  }

  private formatBasedonDate(
    date: string,
    format: string = 'yyyy-MM-dd'
  ): string {
    return this.datepipe.transform(date, format) ?? 'N/A';
  }
  // ✅ Report Interval Flag
  Report_IntervalFlag(val: any, flg: any): string {
    return val ? flg : '0';
  }

  async captureHealthTrendsChart(
    doc: jsPDF,
    chartElement: HTMLElement
  ): Promise<jsPDF> {
    return new Promise((resolve, reject) => {
      try {
        // Make chart visible for capturing
        chartElement.style.visibility = 'visible';

        html2canvas(chartElement)
          .then((canvas) => {
            try {
              // Set heading style and add title
              this.setSubHeadingStyle(doc);
              const healthTrends = 'Patient Health Trends';
              doc.text(healthTrends, 15, 20);

              // Add underline
              doc.setDrawColor('black');
              const textWidth = doc.getTextWidth(healthTrends);
              doc.line(15, 21, 15 + textWidth, 21);

              // Add chart image to PDF
              const imgData = canvas.toDataURL('image/jpeg', 1.0);
              doc.addImage(imgData, 10, 35, 180, 65);

              // Hide the chart element again
              // chartElement.style.visibility = 'hidden';

              resolve(doc);
            } catch (error) {
              console.error('Error processing chart canvas:', error);
              //chartElement.style.visibility = 'hidden';
              reject(error);
            }
          })
          .catch((error) => {
            console.error('Error capturing chart with html2canvas:', error);
            // chartElement.style.visibility = 'hidden';
            reject(error);
          });
      } catch (error) {
        console.error('Error in captureHealthTrendsChart:', error);
        //chartElement.style.visibility = 'hidden';
        reject(error);
      }
    });
  }

  generateReportHeading(doc: any, startDate: any, endDate: any) {
    this.Style_SetDocumentHeader(doc);
    doc.text('REMOTE PATIENT MONITORING - REPORT', 50, 40);
    doc.text(
      '(' +
        this.datepipe.transform(startDate, 'MMM d, y') +
        ' To ' +
        this.datepipe.transform(endDate, 'MMM d, y') +
        ')',
      67,
      50
    );
  }
  generateReportProgramDetails(
    doc: any,
    httpPatient: any,
    programDetails: any
  ) {
    this.Style_SetMainHeading(doc);
    var program_name = httpPatient.PatientProgramdetails.ProgramId;
    var pgm = programDetails.filter(
      (c: { ProgramId: any }) => c.ProgramId === program_name
    );
    if (pgm.length > 0) {
      program_name = pgm[0].ProgramName;
      doc.text(program_name, 10, 20);
    }
  }

  // ✅ Utility function: Draw a Line
  private drawLine(doc: jsPDF, text: string, x: number): void {
    const textWidth = doc.getTextWidth(text);
    doc.line(x, this.Notesh + 1, x + textWidth, this.Notesh + 1);
  }
  private drawsymptomsLine(doc: jsPDF, text: string, x: number): void {
    const textWidth = doc.getTextWidth(text);
    doc.line(x, this.symptomh + 1, x + textWidth, this.symptomh + 1);
  }
  // ✅ Utility function: Set Text on Page
  private setPages(doc: jsPDF, text: string, x: number): void {
    doc.text(text, x, this.Notesh);
  }
  private setSymptomPages(doc: jsPDF, text: string, x: number): void {
    doc.text(text, x, this.symptomh);
    doc.setTextColor('black');
  }

  // ✅ Format Date (Assuming you have a function for this)
  private formatDate(dateStr: string): string {
    return new Date(dateStr).toLocaleString(); // Modify as per requirements
  }

  // ✅ Set Subheading Style
  private setSubHeadingStyle(doc: jsPDF): void {
    doc.setFontSize(12);
    doc.setFont('helvetica', 'bold');
    doc.setTextColor('black');
  }

  // ✅ Set Content Style
  private setContentStyle(doc: jsPDF): void {
    doc.setFontSize(10);
    doc.setFont('helvetica', 'normal');
  }
  Report_ConvertDate(dateval: any) {
    var dt = dateval.split('T');
    var dtSplit = dt[0].split('-');

    var month = '';
    switch (dtSplit[1]) {
      case '01':
        month = 'Jan';
        break;
      case '02':
        month = 'Feb';
        break;
      case '03':
        month = 'Mar';
        break;
      case '04':
        month = 'Apr';
        break;
      case '05':
        month = 'May';
        break;
      case '06':
        month = 'Jun';
        break;
      case '07':
        month = 'Jul';
        break;
      case '08':
        month = 'Aug';
        break;
      case '09':
        month = 'Sep';
        break;
      case '10':
        month = 'Oct';
        break;
      case '11':
        month = 'Nov';
        break;
      case '12':
        month = 'Dec';
        break;
    }
    var dat = month + ' ' + dtSplit[2];
    var time = this.datepipe.transform(
      this.convertToLocalTime(dateval),
      'h:mm a'
    );
    dat = dat + ' - ' + time;
    return dat;
  }
  Style_SetDocumentHeader(doc: jsPDF) {
    doc.setFontSize(16);
    doc.setTextColor('black');
  }
  Style_SetMainHeading(doc: jsPDF) {
    doc.setFontSize(14);
    doc.setTextColor('#590CA7');
  }
  Style_SetSubHeading(doc: jsPDF) {
    doc.setFontSize(12);
    doc.setTextColor('black');
  }
  Style_SetContent(doc: jsPDF) {
    doc.setFontSize(10);
    doc.setTextColor('black');
  }
}
