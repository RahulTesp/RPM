import { Component, EventEmitter, Input, Output } from '@angular/core';

@Component({
  selector: 'app-status-dialog-box',
  templateUrl: './status-dialog-box.component.html',
  styleUrls: ['./status-dialog-box.component.scss'],
})
export class StatusDialogBoxComponent {
  isStatusDialogVisible = false;
  dialogTitle = '';
  dialogMessage = '';

  // Optionally emit an event when the dialog closes
  @Output() dialogClosed = new EventEmitter<void>();

  showSuccessDialog(): void {
    this.dialogTitle = 'Success';
    this.dialogMessage = 'Password Change succefully!! ';
    this.isStatusDialogVisible = true;
  }

  showFailDialog(errmessage:any) {
    this.dialogTitle = 'Failure';
    this.dialogMessage = errmessage;
    this.isStatusDialogVisible = true;
  }

  closeStatusDialog(): void {
    this.isStatusDialogVisible = false;
    this.dialogClosed.emit();
  }
}
