import { AfterViewInit, Component, ElementRef, Input, ViewChild, forwardRef } from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';

@Component({
  selector: 'app-dual-range-slider',
  templateUrl: './dual-range-slider.component.html',
  styleUrls: ['./dual-range-slider.component.scss'],
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => DualRangeSliderComponent),
      multi: true,
    },
  ],
})
export class DualRangeSliderComponent implements ControlValueAccessor,AfterViewInit  {
  @Input() min = 0;
  @Input() max = 100;

  @ViewChild('rangeTrack', { static: true }) rangeTrack!: ElementRef;

  lowerValue = 25;
  upperValue = 75;

  private onChange = (_: any) => {};
  private onTouched = () => {};

  ngAfterViewInit() {
    this.updateSliderBackground();
  }

  writeValue(value: [number, number]): void {
    if (value) {
      this.lowerValue = value[0];
      this.upperValue = value[1];
      this.updateSliderBackground();
    }
  }

  registerOnChange(fn: any): void {
    this.onChange = fn;
  }

  registerOnTouched(fn: any): void {
    this.onTouched = fn;
  }

  onValueChange(): void {
    if (this.lowerValue > this.upperValue) {
      [this.lowerValue, this.upperValue] = [this.upperValue, this.lowerValue];
    }
    this.updateSliderBackground();
    this.onChange([this.lowerValue, this.upperValue]);
    this.onTouched();
  }

  updateSliderBackground() {
    const percent1 = ((this.lowerValue - this.min) / (this.max - this.min)) * 100;
    const percent2 = ((this.upperValue - this.min) / (this.max - this.min)) * 100;

    const bg = `linear-gradient(to right,
      #ddd ${percent1}%,
      #8be3d9 ${percent1}%,
      #8be3d9 ${percent2}%,
      #ddd ${percent2}%)`;

    this.rangeTrack.nativeElement.style.background = bg;
  }
}
