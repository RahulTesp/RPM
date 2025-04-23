import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';

@Component({
  selector: 'app-report-common-layout',
  templateUrl: './report-common-layout.component.html',
  styleUrls: ['./report-common-layout.component.scss'],
})
export class ReportCommonLayoutComponent implements OnInit {
  @Output() downloadClicked = new EventEmitter<void>();
  @Input() loadingstatus: boolean;

  constructor() {}

  ngOnInit(): void {}
}
