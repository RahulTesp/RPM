import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { ConfirmDialogServiceService } from './service/confirm-dialog-service.service';

@Component({
  selector: 'app-confirm-dialog-panel',
  templateUrl: './confirm-dialog-panel.component.html',
  styleUrls: ['./confirm-dialog-panel.component.scss'],
})
export class ConfirmDialogPanelComponent implements OnInit {

  ngOnInit(): void {}
  onBackdropClick() {
    this.onCancel(); // treat outside click as cancel
  }


  title = '';
  message = '';
  showCancel = true;

  constructor(public confirmDialog: ConfirmDialogServiceService) {
    confirmDialog.title$.subscribe(t => this.title = t);
    confirmDialog.message$.subscribe(m => this.message = m);
    confirmDialog.showCancel$.subscribe(sc => this.showCancel = sc);
  }

  onsubmit(status:boolean) {
    this.confirmDialog.handleConfirm(true);
  }

  onCancel() {
    this.confirmDialog.handleConfirm(false);
  }
}
