import {
  Directive,
  ElementRef,
  EventEmitter,
  Input,
  Output,
} from '@angular/core';

@Directive({
  selector: '[appAmIVisible]',
})
export class AmIVisibleDirective {
  constructor(private element: ElementRef) {}

  @Output('elementVisible') elementVisible = new EventEmitter<boolean>();
  @Input('isTargetElement') isTargetElement: boolean;

  public intersectionOptions = {
    root: null, //implies the root is the document viewport
    rootMargin: '0px',
    threshold: [0, 0.5, 1],
  };

  ngAfterViewInit() {
    let observer = new IntersectionObserver(
      this.intersectionCallback.bind(this),
      this.intersectionOptions
    );

    if (this.isTargetElement) {
      observer.observe(this.element.nativeElement);
    }
  }

  intersectionCallback(entries: any[], observer: any) {
    entries.forEach((entry) => {

      if (entry.intersectionRatio === 1) {
        this.elementVisible.emit(true); //element is completely visible in the viewport
      } else {
        this.elementVisible.emit(false);
      }
    });
  }
}
