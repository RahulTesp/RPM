import { Component, Output, EventEmitter } from '@angular/core';

import { DatePipe } from '@angular/common';

@Component({
  selector: 'app-date-range-control',
  templateUrl: './date-range-control.component.html',
  styleUrls: ['./date-range-control.component.scss'],
  providers: [DatePipe],
})
export class DateRangeControlComponent {
  isCalendarOpen: boolean = false;
  // The string to display the selected date range.
  selectedRange: string = '';

  // For calendar view.
  currentMonth: number = new Date().getMonth(); // 0-based (0 = January)
  currentYear: number = new Date().getFullYear();
  // Array of month names to display.
  monthNames: string[] = [
    'January',
    'February',
    'March',
    'April',
    'May',
    'June',
    'July',
    'August',
    'September',
    'October',
    'November',
    'December',
  ];
  // Array of years for the dropdown.
  years: number[] = [];

  calendarDays: { date: Date; isCurrentMonth: boolean }[] = [];

  // Selected start and end dates.
  startDate: Date | null = null;
  endDate: Date | null = null;
  @Output() DateRangeSelected: EventEmitter<{
    startDate: Date;
    endDate: Date;
  }> = new EventEmitter();

  constructor(private datePipe: DatePipe) {}

  ngOnInit() {
    // Generate years for dropdown (e.g., from currentYear - 10 to currentYear + 10)
    const startYear = this.currentYear - 10;
    const endYear = this.currentYear + 10;
    for (let y = startYear; y <= endYear; y++) {
      this.years.push(y);
    }

    // Set default selection: first 7 days starting from today.
    const today = new Date();
    this.endDate = today; // End date is today

    const start = new Date();
    start.setDate(today.getDate() - 29); // Go back 29 days to make it 30 total
    this.startDate = start;
    this.selectedRange = `${this.formatDate(
      this.startDate
    )} - ${this.formatDate(this.endDate)}`;
    this.DateRangeSelected.emit({
      startDate: this.startDate,
      endDate: this.endDate,
    });

    this.generateCalendar();
  }

  openCalendar(): void {
    // When opening, if dates are not set, ensure default selection.
    if (!this.startDate || !this.endDate) {
      const today = new Date();
      this.endDate = today; // End date is today

      const start = new Date();
      start.setDate(today.getDate() - 29); // Go back 29 days to make it 30 total
      this.startDate = start;
      this.selectedRange = `${this.formatDate(
        this.startDate
      )} - ${this.formatDate(this.endDate)}`;
    }
    this.isCalendarOpen = true;
  }

  closeCalendar(): void {
    this.isCalendarOpen = false;
  }

  generateCalendar(): void {
    this.calendarDays = [];
    // Get the first and last day of the current month.
    const firstDayOfMonth = new Date(this.currentYear, this.currentMonth, 1);
    const lastDayOfMonth = new Date(this.currentYear, this.currentMonth + 1, 0);
    const startDay = firstDayOfMonth.getDay(); // 0 = Sunday
    const numDays = lastDayOfMonth.getDate();

    // Generate days from previous month to fill the first week.
    let prevMonthDays: { date: Date; isCurrentMonth: boolean }[] = [];
    if (startDay > 0) {
      const prevMonth = this.currentMonth === 0 ? 11 : this.currentMonth - 1;
      const prevYear =
        this.currentMonth === 0 ? this.currentYear - 1 : this.currentYear;
      const lastDayOfPrevMonth = new Date(prevYear, prevMonth + 1, 0).getDate();
      for (let i = startDay - 1; i >= 0; i--) {
        const day = lastDayOfPrevMonth - i;
        prevMonthDays.push({
          date: new Date(prevYear, prevMonth, day),
          isCurrentMonth: false,
        });
      }
    }

    // Generate days for current month.
    let currentMonthDays: { date: Date; isCurrentMonth: boolean }[] = [];
    for (let day = 1; day <= numDays; day++) {
      currentMonthDays.push({
        date: new Date(this.currentYear, this.currentMonth, day),
        isCurrentMonth: true,
      });
    }

    // Generate days for next month to fill grid .
    const totalCells = prevMonthDays.length + currentMonthDays.length;
    const nextDaysCount =
      totalCells % 7 === 0 ? 42 - totalCells : 7 - (totalCells % 7);
    let nextMonthDays: { date: Date; isCurrentMonth: boolean }[] = [];
    const nextMonth = this.currentMonth === 11 ? 0 : this.currentMonth + 1;
    const nextYear =
      this.currentMonth === 11 ? this.currentYear + 1 : this.currentYear;
    for (let day = 1; day <= nextDaysCount; day++) {
      nextMonthDays.push({
        date: new Date(nextYear, nextMonth, day),
        isCurrentMonth: false,
      });
    }

    this.calendarDays = [
      ...prevMonthDays,
      ...currentMonthDays,
      ...nextMonthDays,
    ];
  }

  // Navigation: previous month.
  previousMonth(): void {
    if (this.currentMonth === 0) {
      this.currentMonth = 11;
      this.currentYear--;
    } else {
      this.currentMonth--;
    }
    this.generateCalendar();
  }

  // Navigation: next month.
  nextMonth(): void {
    if (this.currentMonth === 11) {
      this.currentMonth = 0;
      this.currentYear++;
    } else {
      this.currentMonth++;
    }
    this.generateCalendar();
  }

  selectDate(date: Date): void {
    // If no range is selected or both dates are set, start new range.
    if (!this.startDate || (this.startDate && this.endDate)) {
      this.startDate = date;
      this.endDate = null;
    } else if (this.startDate && !this.endDate) {
      if (date >= this.startDate) {
        this.endDate = date;
      } else {
        // If selected date is before start, reset startDate.
        this.startDate = date;
      }
    }
  }

  // isSelected(date: Date): boolean {
  //   if (!this.startDate) return false;
  //   if (this.startDate && !this.endDate) {
  //     return this.areDatesEqual(date, this.startDate);
  //   }
  //   if (this.startDate && this.endDate) {
  //     return date >= this.startDate && date <= this.endDate;
  //   }
  //   return false;
  // }
  isSelected(date: Date): boolean {
    const normalizedDate = this.normalizeDate(date);

    if (!this.startDate) return false;

    if (this.startDate && !this.endDate) {
      return this.areDatesEqual(
        normalizedDate,
        this.normalizeDate(this.startDate)
      );
    }

    if (this.startDate && this.endDate) {
      return (
        normalizedDate >= this.normalizeDate(this.startDate) &&
        normalizedDate <= this.normalizeDate(this.endDate)
      );
    }

    return false;
  }
  areDatesEqual(date1: Date, date2: Date): boolean {
    return (
      date1.getFullYear() === date2.getFullYear() &&
      date1.getMonth() === date2.getMonth() &&
      date1.getDate() === date2.getDate()
    );
  }

  applyDateRange(): void {
    if (this.startDate && this.endDate) {
      this.selectedRange = `${this.formatDate(
        this.startDate
      )} - ${this.formatDate(this.endDate)}`;
      this.closeCalendar();
      this.DateRangeSelected.emit({
        startDate: this.startDate,
        endDate: this.endDate,
      });
    }
  }
  formatDate(date: Date): string {
    return this.datePipe.transform(date, 'MMM d, y') ?? '';
  }
  // formatDate(date: Date): string {
  //   const year = date.getFullYear();
  //   const month = (date.getMonth() + 1).toString().padStart(2, '0');
  //   const day = date.getDate().toString().padStart(2, '0');
  //   return `${year}-${month}-${day}`;
  // }
  normalizeDate(date: Date): Date {
    return new Date(date.getFullYear(), date.getMonth(), date.getDate());
  }
  onOverlayClick(event: MouseEvent): void {
    console.log('close');
    this.closeCalendar(); // Close on outside click
  }
}
