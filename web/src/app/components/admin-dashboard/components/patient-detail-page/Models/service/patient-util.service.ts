import { Injectable } from '@angular/core';
import { AuthService } from 'src/app/services/auth.service';
import dayjs from 'dayjs';
import utc from 'dayjs/plugin/utc';
import timezone from 'dayjs/plugin/timezone';

dayjs.extend(utc);
dayjs.extend(timezone);
@Injectable({
  providedIn: 'root',
})
export class PatientUtilService {
  constructor(private auth: AuthService) {}

  newVitalreadingData(data: any, vitalInfo: string) {
    if (!data || !Array.isArray(data)) return [];

    let vitalDataArray: any[] = [];

    // Loop through data and process each vital type correctly
    data.forEach((item: any) => {
      const readingDate = this.convertToLocalTime(item.ReadingTime);

      const commonFields = {
        ReadingTime: readingDate,
        Remarks: item.Remarks,
        DateData: readingDate,
        Status: item.Status,
      };

      switch (vitalInfo) {
        case 'BloodPressureReadings':
          vitalDataArray.push({
            ...commonFields,
            Diastolic: item.Diastolic,
            DiastolicStatus: item.DiastolicStatus,
            Systolic: item.Systolic,
            SystolicStatus: item.SystolicStatus,
            Pulse: item.pulse,
            PulseStatus: item.pulseStatus,
          });
          break;

        case 'BloodGlucoseReadings':
          vitalDataArray.push({
            ...commonFields,
            BGmgdl: item.BGmgdl,
            Schedule: item.Schedule,
          });
          break;

        case 'BloodOxygenReadings':
          vitalDataArray.push({
            ...commonFields,
            Oxygen: item.Oxygen,
            Pulse: item.Pulse,
            PulseStatus: item.PulseStatus,
            OxygenStatus: item.OxygenStatus,
          });
          break;

        case 'WeightReadings':
          vitalDataArray.push({
            ...commonFields,
            BWlbs: item.BWlbs,
          });
          break;

        default:
          console.warn(`Unknown vital type: ${vitalInfo}`);
      }
    });

    //  Grouping by Date
    const groups = vitalDataArray.reduce((groups: any, vitals: any) => {
      const date = vitals.DateData.split(' ')[0]; // Extract date without time
      if (!groups[date]) {
        groups[date] = [];
      }
      groups[date].push(vitals);
      return groups;
    }, {});

    //  Return grouped data in correct structure
    return Object.keys(groups).map((ReadingDate: string) => ({
      ReadingDate,
      [`${vitalInfo}`]: groups[ReadingDate], // Dynamically assigning vital type
    }));
  }
  // Manjusha code change
  convertToLocalTime(stillUtc: any) {
    if (stillUtc) {
      const isDateOnly = /^\d{4}-\d{2}-\d{2}$/.test(stillUtc);
      if (isDateOnly) {
        const local = dayjs.utc(stillUtc).local().format('YYYY-MM-DD HH:mm:ss');
        return local;
      }
      if (stillUtc.includes('+')) {
        stillUtc = stillUtc.split('+')[0];
      }

      // Append Z if missing
      if (!stillUtc.endsWith('Z')) {
        stillUtc = stillUtc + 'Z';
      }

      const local = dayjs.utc(stillUtc).local().format('YYYY-MM-DD HH:mm:ss');
      return local;
    }

    return null;
  }

  getCurrentMonthDates(): { start: Date; end: Date } {
    try {
      const today = new Date();
      const monthStart = new Date(today.getFullYear(), today.getMonth(), 1);
      const monthEnd = new Date(today.getFullYear(), today.getMonth() + 1, 0);

      return {
        start: monthStart,
        end: monthEnd,
      };
    } catch (error) {
      console.error('Error calculating current month dates:', error);
      // Fallback to today if there's an error
      const today = new Date();
      return { start: today, end: today };
    }
  }
  formatDateForApi(date: Date, isStartDate: boolean): string {
    try {
      const dateStr = this.convertDate(date);
      const timeStr = isStartDate ? 'T00:00:00' : 'T23:59:59';
      const fullDateStr = dateStr + timeStr;

      return this.auth.ConvertToUTCRangeInput(new Date(fullDateStr));
    } catch (error) {
      console.error('Error formatting date for API:', error);
      // Return current date as fallback
      return new Date().toISOString();
    }
  }

  convertDate(date: Date): string {
    try {
      if (!date || !(date instanceof Date) || isNaN(date.getTime())) {
        throw new Error('Invalid date provided');
      }

      const year = date.getFullYear();
      const month = (date.getMonth() + 1).toString().padStart(2, '0');
      const day = date.getDate().toString().padStart(2, '0');
      return `${year}-${month}-${day}`;
    } catch (error) {
      console.error('Error converting date:', error);
      // Return today's date as fallback
      const today = new Date();
      const year = today.getFullYear();
      const month = (today.getMonth() + 1).toString().padStart(2, '0');
      const day = today.getDate().toString().padStart(2, '0');
      return `${year}-${month}-${day}`;
    }
  }

  public getDateRange(
    startDate?: Date,
    endDate?: Date
  ): { startDate: Date; endDate: Date } {
    try {
      if (startDate && endDate) {
        return { startDate, endDate };
      }

      // Default to current month
      const currentDate = new Date();
      const firstDay = new Date(
        currentDate.getFullYear(),
        currentDate.getMonth(),
        1
      );
      const lastDay = new Date(
        currentDate.getFullYear(),
        currentDate.getMonth() + 1,
        0
      );

      return { startDate: firstDay, endDate: lastDay };
    } catch (error) {
      console.error('Error calculating date range:', error);
      // Fallback to current day if there's an issue
      const today = new Date();
      return { startDate: today, endDate: today };
    }
  }

  convertSecToTime(seconds: number): string {
    try {
      if (isNaN(seconds) || seconds < 0) {
        return '00:00:00';
      }

      const hours = Math.floor(seconds / 3600);
      const minutes = Math.floor((seconds % 3600) / 60);
      const remainingSeconds = seconds % 60;

      const hoursStr = hours.toString().padStart(2, '0');
      const minutesStr = minutes.toString().padStart(2, '0');
      const secondsStr = remainingSeconds.toString().padStart(2, '0');

      return `${hoursStr}:${minutesStr}:${secondsStr}`;
    } catch (error) {
      console.error('Error converting seconds to time:', error);
      return '00:00:00';
    }
  }
  getPatientHeight(height: any) {
    var heightdata = height.toString().split('.');
    if (heightdata[0] == undefined) {
      heightdata[0] = 0;
    } else {
      heightdata[0] = heightdata[0];
    }
    if (heightdata[1] == undefined) {
      heightdata[1] = 0;
    } else {
      heightdata[1] = heightdata[1];
    }
    var patientHeight =
      heightdata[0] + ' ' + " ' " + ' ' + heightdata[1] + ' ' + ' " ';
    return patientHeight;
  }
}
