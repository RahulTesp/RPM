import { DatePipe } from '@angular/common';
import { Injectable } from '@angular/core';
import jsPDF from 'jspdf';
import { ReportDataService } from './report-data.service';
import { RPMService } from '../../../sevices/rpm.service';
import autoTable from 'jspdf-autotable';
import { PatientUtilService } from '../../patient-detail-page/Models/service/patient-util.service';
import html2canvas from 'html2canvas';
import { PatientReportApiService } from './patient-report-api.service';
import { PatientDataDetailsService } from '../../patient-detail-page/Models/service/patient-data-details.service';
import dayjs from 'dayjs';
import utc from 'dayjs/plugin/utc';
import timezone from 'dayjs/plugin/timezone';
@Injectable({
  providedIn: 'root',
})
export class DownloadPatientReportService {
  vitals: any;
  patientInfo: any;
  constructor(
    private datepipe: DatePipe,
    private patientReportService: ReportDataService,
    private rpmService: RPMService,
    private patientService: PatientDataDetailsService,
    private patientUtilService: PatientUtilService,
    private patientReportApiService: PatientReportApiService
  ) {}
  private Notesh: number = 30;
  private vitalsConsolidated: any[] = []; // 🟢 Store consolidated vitals
  private VitalsSevenDaysConsolidated: any[] = []; // 🟢 Store last 7 days vitals
  private VitalLast30DaysConsolidated: any[] = []; // 🟢 Store last 30 days vitals
  private reviewNotes: any;
  private callNotes: any;
  private selectedVitals: any;
  currentpPatientId: any;
  currentProgramId: any;

  generatePatientAndProgramInfo(
    doc: jsPDF,
    patientDetails: any,
    programDetails: any,
    patientProgramname: string
  ): number {
    let textHeight = 70;

    // Patient Information Section
    this. Style_SetMainHeading(doc);
    doc.text('Patient Information', 10, textHeight);
    this.Style_SetContent(doc);

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
      //this.formatDate(patientDetails['PatientDetails'].DOB)
      this.convertToLocalFormat(patientDetails['PatientDetails'].DOB),
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
    this. Style_SetMainHeading(doc);
    doc.text('Program Information', 10, textHeight);
    this.Style_SetContent(doc);

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
      this.convertToLocalFormat(patientDetails['PatientProgramdetails'].StartDate),
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
  this.convertToLocalFormat(patientDetails['PatientProgramdetails'].StartDate),
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
    // const pStart = this.datepipe.transform(
    //   patientDetails['PatientProgramdetails'].StartDate
    // );
    const pStart = this.datepipe.transform( this.convertToLocalFormat(patientDetails['PatientProgramdetails'].StartDate))
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
      !healthTrends
    ) {
      return this.generateEmptyChartData();
    }
    
    // Convert dates to readable format
    const chartLabels = this.convertDateforHealthTrends(healthTrends[0].Time);

    // Process data points and clean up null values
    
    let valuesArray = JSON.parse(JSON.stringify(healthTrends[0].Values)); // Deep copy

    // Filter out null values
    let j = 0;
    for (const item of valuesArray) {
      let i = 0;
      for (const value of item.data) {
        if (j === 0) {
          try {
            //if (value === null && i > 0 && i < item.data.length) {
            if (
              value === null &&
              i > 0 &&
              i < item.data.length - 1 &&
              chartLabels[i] &&
              chartLabels[i - 1] &&
              chartLabels[i + 1]
            ) {

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
            //console.error('Error processing health trend data:', ex);
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
  allLineChartData: any=[];
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

    if (!Array.isArray(dateArr)) {
      //console.error('Invalid input to convertDateforHealthTrends:', dateArr);
      return formattedDates; // return empty array instead of crashing
    }

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

      
    const localDateTime1 = this.convertToLocalTime(dateParts[0]);
    const parsedDate = new Date(localDateTime1);

      if (isNaN(parsedDate.getTime())) {
        console.warn('Invalid date encountered:', localDateTime1);
        continue; // Skip this iteration
      }       

        const time = this.datepipe.transform(parsedDate, 'h:mm a') || '';


      // Format time
      /*const time =
        this.datepipe.transform(
          this.convertToLocalTime(dateParts[0]),
          'h:mm a'
        ) || '';*/

      // Combine date and time
      formattedDates.push(date + ' - ' + time);
    }
    return formattedDates;
  }

  /**
   * Convert UTC time to local time
   */
  convertToLocalTime(stillUtc: any) {
    //stillUtc = stillUtc + 'Z';
    
    if (typeof stillUtc === 'string' && !stillUtc.endsWith('Z')) {
    stillUtc += 'Z';
  }

    const local = dayjs.utc(stillUtc).local().format('YYYY-MM-DD HH:mm:ss');
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

private renderGoals(doc: jsPDF, goals: any[], startHeight: number): number {
  let goalh = startHeight;
  const pageHeight = doc.internal.pageSize.height;

  goals.forEach(goal => {
    // Page break check
    if (goalh + 20 > pageHeight - 20) {
      doc.addPage();
      goalh = 20;
    }

    this.Style_SetSubHeading(doc);
    doc.text(goal.Goal, 15, goalh);
    const textWidth = doc.getTextWidth(goal.Goal);
    doc.line(15, goalh + 1, 15 + textWidth, goalh + 1);

    goalh += 6;
    this.Style_SetContent(doc);

    const wrappedDescription = doc.splitTextToSize(goal.Description, 180);
    doc.text(wrappedDescription, 20, goalh);
    goalh += wrappedDescription.length * 5 + 4; // compact spacing
  });

  return goalh;
}

private renderCriticalEvents(doc: jsPDF, alerts: any[], startHeight: number): void {
  // Always start on a new page
  doc.addPage();
  let goalh = 20; // Reset Y for new page
  const pageHeight = doc.internal.pageSize.height;

  this.Style_SetSubHeading(doc);
  const eventsTitle = 'Critical Events';
  doc.text(eventsTitle, 15, goalh);
  const textWidth = doc.getTextWidth(eventsTitle);
  doc.line(15, goalh + 1, 15 + textWidth, goalh + 1);
  goalh += 6;

  this.Style_SetContent(doc);

  if (alerts.length > 0) {
    alerts.forEach(alert => {
      // Page break check for long content
      if (goalh + 20 > pageHeight - 20) {
        doc.addPage();
        goalh = 20;
      }

      const formattedDate = this.formatDate(alert.Time);
      doc.text(formattedDate, 20, goalh);
      goalh += 5;

      const wrappedAlert = doc.splitTextToSize(alert.VitalAlert, 180);
      doc.text(wrappedAlert, 20, goalh);
      goalh += wrappedAlert.length * 5 + 4;
    });
  } else {
    doc.text('No Data', 20, goalh);
  }
}

  //Notes

  // ✅ Fetch and Populate Notes Details Recursively
  async populateNotesDetails(
    notes: any[],
    idProgram: string,
    noteType: string
  ): Promise<any[]> {
    for (let i = 0; i < notes.length; i++) {
      const details = await this.patientReportApiService.getNoteDetails(
        idProgram,
        noteType,
        notes[i].Id
      );
      notes[i]['NoteDetails'] = details;
      notes[i]['Type'] = noteType;
    }

    return notes;
  }

 
generateCallNotesReport(doc: jsPDF, data: any[]): void {

  const marginLeft = 20;
  const maxWidth = 170;
  const lineHeight = 6;
  const pageHeight = doc.internal.pageSize.height;
  this.Notesh = 30;
  this.Style_SetSubHeading(doc);
  this.setPages(doc, 'Notes', 15);
  this.drawLine(doc, 'Notes', 15);
  this.Notesh += 10;
  doc.setFontSize(14);
  
  for (const notes of data) {
    this.Notesh += 10;
      this.Style_SetSubHeading(doc);
      const formattedDate = this.datepipe.transform(
        this.convertToLocalTime(notes.CreatedOn)
      ) as string;

    this.setPages(doc, formattedDate, 20);
    this.drawLine(doc, formattedDate, 20);
   this.Style_SetContent(doc);
    this.Notesh += 5;
    
    this.addTextWithBreak(
      doc,
      `Duration : ${this.patientReportService.timeConvert(notes.Duration)}`,
      marginLeft,
      maxWidth,
      lineHeight
    );

    this.addTextWithBreak(doc, `Completed By : ${notes.CompletedBy}`, marginLeft, maxWidth, lineHeight);
    this.addTextWithBreak(doc, `Note Type : ${notes.NoteType}`, marginLeft, maxWidth, lineHeight);
    this.addTextWithBreak(doc, `Type : ${notes.Type}`, marginLeft, maxWidth, lineHeight);

    if (notes.Type !== 'REVIEW') {
      this.addTextWithBreak(
        doc,
        `Call Established : ${notes.IsEstablished ? 'Yes' : 'No'}`,
        marginLeft,
        maxWidth,
        lineHeight
      );
      this.addTextWithBreak(
        doc,
        `Care Giver : ${notes.IsCareGiver ? 'Yes' : 'No'}`,
        marginLeft,
        maxWidth,
        lineHeight
      );
    }

    this.Notesh += 5;
    this.processNoteDetails(doc, notes); // Make sure this method also uses splitTextToSize
    this.checkPageBreak(doc);
    this.Notesh += 3;
  }

  
  this.Notesh += 20; // Add bottom spacing
  doc.addPage();
}

private addTextWithBreak(
  doc: jsPDF,
  text: string,
  x: number,
  maxWidth: number,
  lineHeight: number
): void {
  const lines = doc.splitTextToSize(text, maxWidth);
  const pageHeight = doc.internal.pageSize.height;

  // Reset font and color before adding text
  this.Style_SetContent(doc);

  if (this.Notesh + lines.length * lineHeight > pageHeight) {
    doc.addPage();
    this.Notesh = 30;
  }

  doc.text(lines, x, this.Notesh);
  this.Notesh += lines.length * lineHeight;
}
// ✅ Utility: Check and add page break if needed
private checkPageBreak(doc: jsPDF, estimatedHeight: number = 0): void {
  const pageHeight = doc.internal.pageSize.height;
  const bottomMargin = 20;

  if (this.Notesh + estimatedHeight > pageHeight - bottomMargin) {
    doc.addPage();
    this.Notesh = 30; // Reset top margin
  }
}

// ✅ Utility: Force wrap long words (fix for overflow issue)
private forceWrapLongWords(text: string, maxWordLength: number = 50): string {
  return text.replace(new RegExp(`(\\S{${maxWordLength}})`, 'g'), '$1 ');
}

// ✅ Render a block of text with wrapping and page-break handling
private renderTextBlock(doc: jsPDF, text: string, x: number): void {
  
  this.Style_SetContent(doc);
  const maxWidth = 170;
  const lineHeight = 6;

  // Apply long-word wrapping fix
  const wrappedText = this.forceWrapLongWords(text);
  const lines = doc.splitTextToSize(wrappedText, maxWidth);
  const estimatedHeight = lines.length * lineHeight;

  this.checkPageBreak(doc, estimatedHeight);
  doc.text(lines, x, this.Notesh);

  this.Notesh += estimatedHeight;
}

// ✅ Render section header with underline
private renderSectionHeader(doc: jsPDF, title: string, x: number): void {
  this.checkPageBreak(doc, 15);
  this.setPages(doc, title, x);
  this.drawLine(doc, title, x);
  this.Notesh += 10;
}

// ✅ Render answers list
/*private renderAnswers(doc: jsPDF, answers: any[], x: number): void {
  if (answers.length > 0) {
    for (const answer of answers) {
      this.checkPageBreak(doc, 6);
      this.setPages(doc, `- ${answer.Answer}`, x);
      this.Notesh += 6;
    }
  } else {
    this.checkPageBreak(doc, 6);
    this.setPages(doc, '- None', x);
    this.Notesh += 6;
  }
 
}*/

private renderAnswers(doc: jsPDF, answers: any[], x: number): void {
  const pageWidth = doc.internal.pageSize.getWidth();
  const margin = 15;
  const maxWidth = pageWidth - margin * 2;
  const lineHeight = 6;
  this.Style_SetContent(doc);

  if (answers.length > 0) {
    for (const answer of answers) {
      const wrappedText = doc.splitTextToSize(`- ${answer.Answer}`, maxWidth);

      this.checkPageBreak(doc, wrappedText.length * lineHeight);
      doc.text(wrappedText, x, this.Notesh);

      this.Notesh += wrappedText.length * lineHeight;
    }
  } else {
    const wrappedText = doc.splitTextToSize('- None', maxWidth);
    this.checkPageBreak(doc, wrappedText.length * lineHeight);
    doc.text(wrappedText, x, this.Notesh);
    this.Notesh += wrappedText.length * lineHeight;
  }
}

// ✅ Main: Process Note Details
private processNoteDetails(doc: jsPDF, notes: any): void {
  if (!notes.NoteDetails) return;

  for (const mainQuestion of notes.NoteDetails.MainQuestions) {
    this.renderSectionHeader(doc, mainQuestion.Question, 20);

    const checkedAnswers = mainQuestion.AnswerTypes.filter((a: any) => a.Checked);
    this.renderAnswers(doc, checkedAnswers, 25);

    this.processSubQuestions(doc, mainQuestion.SubQuestions);

    if (mainQuestion.Notes) {
      this.renderTextBlock(doc, `Extra Notes : ${mainQuestion.Notes}`, 25);
    }
  }

  if (notes.NoteDetails.Notes) {
    this.renderSectionHeader(doc, 'Additional Notes', 20);
    this.renderTextBlock(doc, notes.NoteDetails.Notes, 25);
  }
}


  // ✅ Process Sub Questions
  private processSubQuestions(doc: jsPDF, subQuestions: any[]): void {
    this.Style_SetContent(doc);
    subQuestions.forEach((subQuestion) => {
       
      this.Notesh += 5;
      this.checkPageSpace(doc);
      this.setPages(doc, subQuestion.Question, 25);
      this.drawLine(doc, subQuestion.Question, 25);
      this.Notesh += 5;
      this.checkPageSpace(doc);

      let hasCheckedAnswer = subQuestion.AnswerTypes.some(
        (answer: { Checked: any }) => answer.Checked
      );
      if (hasCheckedAnswer) {
        subQuestion.AnswerTypes.filter(
          (answer: { Checked: any }) => answer.Checked
        ).forEach((answer: { Answer: any }) => {
          this.checkPageSpace(doc);
          this.setPages(doc, `- ${answer.Answer}`, 25);
          this.Notesh += 5;
        });
      } else {
        this.checkPageSpace(doc);
        this.setPages(doc, '- None', 25);
        this.Notesh += 5;
      }
    });
  }

  private checkPageSpace(doc: jsPDF, buffer: number = 20): void {
    const pageHeight = doc.internal.pageSize.height;
    if (this.Notesh + buffer > pageHeight) {
      doc.addPage();
      this.Notesh = 30; // Reset to top margin
    }
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
    idProgram: any
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
          idProgram,
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
        idProgram
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
    idProgram: any
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
          idProgram,
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

    this. Style_SetMainHeading(doc);
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
   this.Style_SetSubHeading(doc);

    doc.text(title, 15, this.symptomh);

    this.drawsymptomsLine(doc, title, 15);
    this.symptomh += 10;
    if(data && data.length > 0){
    //if (data.length > 0) {
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
        //`Date: ${this.convertToLocalTime(symptom.SymptomStartDateTime)}`,
        `Date: ${this.datepipe.transform(
          this.convertToLocalTime(symptom.SymptomStartDateTime),'MMM d - h:mm a' 
        )}`,
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
        med.StartDate!,
        'MMM d, y'
      ),
      this.datepipe.transform(
        //this.convertToLocalTime(med.EndDate)!,
        med.EndDate!,
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
  // Manjusha code change
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
        this.BloodGlucoseReportDisplayTest.bind(this)
      );
    }
    if (vitals.BloodPressure?.length > 0) {
      this.generateVitalTable(
        doc,
        'Blood Pressure Readings',
        ['Date', 'Time', 'SBP', 'DBP', 'Pulse', 'Remarks'],
        vitals.BloodPressure,
        'BloodPressureReadings',
        this.formatBloodPressureRow
      );
    }
    if (vitals.BloodOxygen?.length > 0) {
      this.generateVitalTable(
        doc,
        'Blood Oxygen Readings',
        ['Date', 'Time', 'Oxygen', 'Pulse', 'Remarks'],
        vitals.BloodOxygen,
        'BloodOxygenReadings',
        this.formatBloodOxygenRow
      );
    }
    if (vitals.Weight?.length > 0) {
      this.generateVitalTable(
        doc,
        'Weight Readings',
        ['Date', 'Time', 'Weight', 'Remarks'],
        vitals.Weight,
        'WeightReadings',
        this.formatWeightRow
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
    const vitalTypeMap: { [key: string]: string } = {
      BloodPressureReadings: 'Blood Pressure',
      BloodGlucoseReadings: 'Blood Glucose',
      BloodOxygenReadings: 'Oxygen',
      WeightReadings: 'Weight',
    };
    const vitalType = vitalTypeMap[readingType];

    // 🏷️ Add Title Above Table
    const startY = this.currentY || 30;
    doc.setFontSize(12);
    doc.text(title, 15, startY);
    const tableStartY = startY + 7;

    for (let vital of vitalData) {
      for (let reading of vital[readingType]) {
        reading.VitalType = vitalType;
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
      startY: tableStartY,
      didParseCell: this.formatTableCells,
    });
    const docAny = doc as any;
    this.currentY = docAny.lastAutoTable?.finalY + 10 || tableStartY + rows.length * 10;
  }
  private formatBloodPressureRow = (reading: any): string[] => {
    return [
      this.datepipe.transform(reading.ReadingTime) ?? 'N/A',
      this.datepipe.transform(reading.ReadingTime, 'h:mm a') ?? 'N/A',
      reading.Systolic ?? '-',
      reading.Diastolic ?? '-',
      reading.Pulse ?? '-',
      reading.Remarks ?? '-',
    ];
  };

  private formatBloodOxygenRow = (reading: any): string[] => {
    return [
      this.datepipe.transform(reading.ReadingTime) ?? 'N/A',
      this.datepipe.transform(reading.ReadingTime, 'h:mm a') ?? 'N/A',
      reading.Oxygen ?? '-',
      reading.Pulse ?? '-',
      reading.Remarks ?? '-',
    ];
  };

  private formatWeightRow = (reading: any): string[] => {
    return [
      this.datepipe.transform(reading.ReadingTime) ?? 'N/A',
      this.datepipe.transform(reading.ReadingTime, 'h:mm a') ?? 'N/A',
      reading.BWlbs ?? '-',
      reading.Remarks ?? '-',
    ];
  };

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

  // Manjusha code change
  async generateVitalReadingSummary(doc: jsPDF,currentpPatientId:any, currentProgramId:any): Promise<void> {
    doc.addPage();
    this.Style_SetSubHeading(doc);
    const statSumm = 'Vital Readings - Status Summary';
    doc.text(statSumm, 15, 20);
    const textWidth = doc.getTextWidth(statSumm);
    doc.line(15, 21, 15 + textWidth, 21);
    this.patientInfo = await this.patientService.fetchPatientInfo(
      currentpPatientId,
      currentProgramId
    );
    this.vitals = this.patientInfo.PatientProgramdetails.PatientVitalInfos;
    if (this.vitals) {
      const selectedVitals = this.vitals
        .filter((v: any) => v.Selected)
        .map((v: any) => v.Vital);
      this.selectedVitals = selectedVitals;
    }
    let currentY = 30;
    this.selectedVitals.forEach((vital: string) => {
      doc.setFontSize(12);
      doc.text(`${vital} - Summary`, 15, currentY);

      currentY += 5;
      const columns = [
        'Days of Reading',
        'Selected Last 7 days',
        'Selected Last 30 Days',
      ];
      const rows: string[][] = [];

      // 🔍 Filter 7-day and 30-day data for current vital
      const vitalKeyMap: { [key: string]: string } = {
        'Blood Pressure': 'BloodPressure',
        'Blood Glucose': 'BloodGlucose',
        'Oxygen': 'BloodOxygen',
        'Weight': 'Weight',
      };
      const vitalKey = vitalKeyMap[vital];
      const vital7 = this.VitalsSevenDaysConsolidated.filter(
        (v) => v.VitalType === vital
      );
      const vital30 = this.VitalLast30DaysConsolidated.filter(
        (v) => v.VitalType === vital
      );
      // Total Days
      rows.push([
        'Total Days of Reading',
        Object.keys(this.groupByDate(vital7)).length.toString(),
        Object.keys(this.groupByDate(vital30)).length.toString(),
      ]);

      // Critical
      const critical7 = vital7.filter((v) => v.Status === 'Critical');
      const critical30 = vital30.filter((v) => v.Status === 'Critical');
      rows.push([
        'Critical Readings',
        Object.keys(this.groupByDate(critical7)).length.toString(),
        Object.keys(this.groupByDate(critical30)).length.toString(),
      ]);

      // Cautious
      const cautious7 = vital7.filter((v) => v.Status === 'Cautious');
      const cautious30 = vital30.filter((v) => v.Status === 'Cautious');
      rows.push([
        'Cautious Readings',
        Object.keys(this.groupByDate(cautious7)).length.toString(),
        Object.keys(this.groupByDate(cautious30)).length.toString(),
      ]);

      autoTable(doc, {
        head: [columns],
        body: rows,
        startY: currentY + 5,
        styles: { fontSize: 10 },
        didDrawPage: (data) => {
          if (data.cursor) {
            currentY = data.cursor.y + 10;
          }
        }
      });
    });
  }

  groupByDate(vitalDataArray: any[]) {
    const groups = vitalDataArray.reduce((groups: any, vitals: any) => {
        const date = vitals.DateData.split(' ')[0]; // Extract date without time
        if (!groups[date]) {
            groups[date] = [];
        }
        groups[date].push(vitals);
        return groups;
    }, {});

    return groups;
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

  http_7day_vitalDataReport: any;
  generateDaysVitals(vitalData: any): void {
    this.VitalsSevenDaysConsolidated = [];
    this.http_7day_vitalDataReport = vitalData;
    var vitalBloodPressure =
      this.http_7day_vitalDataReport &&
      this.patientUtilService.newVitalreadingData(
        this.http_7day_vitalDataReport.BloodPressure,
        'BloodPressureReadings'
      );

    var vitalGlucoseData =
      this.http_7day_vitalDataReport &&
      this.patientUtilService.newVitalreadingData(
        this.http_7day_vitalDataReport.BloodGlucose,
        'BloodGlucoseReadings'
      );

    var VitalOxygen =
      this.http_7day_vitalDataReport &&
      this.patientUtilService.newVitalreadingData(
        this.http_7day_vitalDataReport.BloodOxygen,
        'BloodOxygenReadings'
      );

    var VitalWight =
      this.http_7day_vitalDataReport &&
      this.patientUtilService.newVitalreadingData(
        this.http_7day_vitalDataReport.Weight,
        'WeightReadings'
      );

    if (vitalGlucoseData && vitalGlucoseData.length > 0) {
      for (let vl of vitalGlucoseData) {
        for (let vv of vl.BloodGlucoseReadings) {
          vv.VitalType = 'Blood Glucose';
          this.VitalsSevenDaysConsolidated.push(vv);
        }
      }
    } if (vitalBloodPressure && vitalBloodPressure.length > 0) {
      for (let vl of vitalBloodPressure) {
        for (let vv of vl.BloodPressureReadings) {
          vv.VitalType = 'Blood Pressure';
          this.VitalsSevenDaysConsolidated.push(vv);
        }
      }
    } if (VitalOxygen && VitalOxygen.length > 0) {
      for (let vl of VitalOxygen) {
        for (let vv of vl.BloodOxygenReadings) {
          vv.VitalType = 'Oxygen';
          this.VitalsSevenDaysConsolidated.push(vv);
        }
      }
    } if (VitalWight && VitalWight.length > 0) {
      for (let vl of VitalWight) {
        for (let vv of vl.WeightReadings) {
          vv.VitalType = 'Weight';
          this.VitalsSevenDaysConsolidated.push(vv);
        }
      }
    }
  }

  http_30day_vitalDataReport: any;
  generate30DaysVitals(vitalData: any): void {
    this.VitalLast30DaysConsolidated = [];
    this.http_30day_vitalDataReport = vitalData;
    var vitalBloodPressure =
      this.http_30day_vitalDataReport &&
      this.patientUtilService.newVitalreadingData(
        this.http_30day_vitalDataReport.BloodPressure,
        'BloodPressureReadings'
      );

    var vitalGlucoseData =
      this.http_30day_vitalDataReport &&
      this.patientUtilService.newVitalreadingData(
        this.http_30day_vitalDataReport.BloodGlucose,
        'BloodGlucoseReadings'
      );

    var VitalOxygen =
      this.http_30day_vitalDataReport &&
      this.patientUtilService.newVitalreadingData(
        this.http_30day_vitalDataReport.BloodOxygen,
        'BloodOxygenReadings'
      );

    var VitalWight =
      this.http_30day_vitalDataReport &&
      this.patientUtilService.newVitalreadingData(
        this.http_30day_vitalDataReport.Weight,
        'WeightReadings'
      );

    if (vitalGlucoseData && vitalGlucoseData.length > 0) {
      for (let vl of vitalGlucoseData) {
        for (let vv of vl.BloodGlucoseReadings) {
          vv.VitalType = 'Blood Glucose';
          this.VitalLast30DaysConsolidated.push(vv);
        }
      }
    } if (vitalBloodPressure && vitalBloodPressure.length > 0) {
      for (let vl of vitalBloodPressure) {
        for (let vv of vl.BloodPressureReadings) {
          vv.VitalType = 'Blood Pressure';
          this.VitalLast30DaysConsolidated.push(vv);
        }
      }
    } if (VitalOxygen && VitalOxygen.length > 0) {
      for (let vl of VitalOxygen) {
        for (let vv of vl.BloodOxygenReadings) {
          vv.VitalType = 'Oxygen';
          this.VitalLast30DaysConsolidated.push(vv);
        }
      }
    } if (VitalWight && VitalWight.length > 0) {
      for (let vl of VitalWight) {
        for (let vv of vl.WeightReadings) {
          vv.VitalType = 'Weight';
          this.VitalLast30DaysConsolidated.push(vv);
        }
      }
    }
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

  currentY: number = 10;
  

  
async captureHealthTrendsChart(
  doc: jsPDF,
  chartElement: HTMLElement,
  title: string
): Promise<jsPDF> {
  // === Layout config (readable, no shrinking) ===
  const leftMargin = 10;
  const rightMargin = 10;
  const topMargin = 15;         // top margin for new pages
  const bottomMargin = 12;

  const titleFontSize = 12;
  const titleGapBelow = 6;      // gap between title and image
  const spacingAfterImage = 10; // gap after image before next block

  // Get page dimensions (robust across jsPDF versions)
  const pageWidth =
    (doc as any).internal?.pageSize?.getWidth?.() ??
    (doc as any).internal?.pageSize?.width ??
    210; // A4 width (mm)
  const pageHeight =
    (doc as any).internal?.pageSize?.getHeight?.() ??
    (doc as any).internal?.pageSize?.height ??
    297; // A4 height (mm)

  // Fixed chart size (NO shrinking)
  const imageWidthMm = pageWidth - leftMargin - rightMargin;
  const imageHeightMm = 65;     // keep this fixed for readability

  // Ensure the chart element is capturable
  const prevVisibility = chartElement.style.visibility;
  const prevDisplay = chartElement.style.display;
  chartElement.style.visibility = 'visible';
  chartElement.style.display = ''; // avoid display:none while capturing

  // Normalize currentY if it somehow slipped above the top margin
  if (!Number.isFinite(this.currentY) || this.currentY < topMargin) {
    this.currentY = topMargin;
  }

  return new Promise((resolve, reject) => {
    try {
      html2canvas(chartElement, {
        scale: 2,
        useCORS: true,
        logging: false,
        backgroundColor: null,
      })
        .then((canvas) => {
          try {
            const hasTitle = !!title && title.trim().length > 0;

            // Compute the total block height needed (title + gap + fixed image + spacing after)
            const titleBlockMm = hasTitle ? (titleFontSize * 0.4) + titleGapBelow : 0;
            const totalNeededMm = titleBlockMm + imageHeightMm + spacingAfterImage;

            // If not enough room on the current page, add a page BEFORE drawing
            if (this.currentY + totalNeededMm > pageHeight - bottomMargin) {
              doc.addPage();
              this.currentY = topMargin;
            }

            // --- (1) Draw the vital title (UNDERLINED) ---
            if (hasTitle) {
              doc.setFontSize(titleFontSize);
              doc.text(title, leftMargin + 5, this.currentY);

              doc.setDrawColor('black');
              const textWidth = doc.getTextWidth(title);
              doc.line(
                leftMargin + 5,
                this.currentY + 1,
                leftMargin + 5 + textWidth,
                this.currentY + 1
              );

              // gap under the title
              this.currentY += titleGapBelow;
            }

            // --- (2) Add the chart image at fixed size (NO shrinking) ---
            const imgData = canvas.toDataURL('image/png');
            doc.addImage(
              imgData,
              'PNG',
              leftMargin,
              this.currentY,
              imageWidthMm,
              imageHeightMm
            );

            // --- (3) Advance cursor after image ---
            this.currentY += imageHeightMm + spacingAfterImage;

            // If we land at the bottom, prep next content at top of next page
            if (this.currentY > pageHeight - bottomMargin) {
              doc.addPage();
              this.currentY = topMargin;
            }

            resolve(doc);
          } catch (error) {
            console.error('Error processing chart canvas:', error);
            reject(error);
          } finally {
            // Restore element styles
            chartElement.style.visibility = prevVisibility;
            chartElement.style.display = prevDisplay;
          }
        })
        .catch((error) => {
          console.error('Error capturing chart with html2canvas:', error);
          chartElement.style.visibility = prevVisibility;
          chartElement.style.display = prevDisplay;
          reject(error);
        });
    } catch (error) {
      console.error('Error in captureHealthTrendsChart:', error);
      chartElement.style.visibility = prevVisibility;
      chartElement.style.display = prevDisplay;
      reject(error);
    }
  });
}

  getChartCurrentY(): number {
    return this.currentY;
  }

  resetPosition(): void {
    this.currentY = 30;
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
   this.Style_SetContent(doc);
   const pageWidth = doc.internal.pageSize.getWidth();
   const margin = 15; // left/right margin
   const maxWidth = pageWidth - margin * 2; // available width for text
  // Split text into multiple lines to fit within maxWidth
   const wrappedText = doc.splitTextToSize(text, maxWidth);
   doc.text(wrappedText, x, this.Notesh);
  }
  
private setSymptomPages(doc: jsPDF, text: string, x: number): void {
  this.Style_SetContent(doc);
  const pageWidth = doc.internal.pageSize.getWidth();
  const pageHeight = doc.internal.pageSize.getHeight();
  const margin = 15;
  const maxWidth = pageWidth - margin * 2;
  const wrappedText = doc.splitTextToSize(text, maxWidth);

  // Check if content fits on current page
  if (this.symptomh + wrappedText.length * 6 > pageHeight - 20) {
    doc.addPage();
    this.symptomh = 20; // reset Y for new page
  }

  doc.text(wrappedText, x, this.symptomh);
  // Compact spacing
  this.symptomh += wrappedText.length * 5 + 4;
}

  


  // ✅ Format Date (Assuming you have a function for this)
  private formatDate(dateStr: string): string {
    return new Date(dateStr).toLocaleString(); // Modify as per requirements
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
  
  private convertToLocalFormat(dateStr: string): string {
  if (!dateStr) return '';

  // Normalize input
  let normalized = dateStr;
  if (dateStr.includes('+')) {
    normalized = dateStr.split('+')[0];
  }
  if (!normalized.endsWith('Z')) {
    normalized += 'Z'; // interpret as UTC
  }

  // Convert UTC → Local and format
  return dayjs.utc(normalized).local().format('MM/DD/YYYY');
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
