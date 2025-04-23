import { Component, Inject, OnInit } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';

@Component({
  selector: 'app-status-message',
  templateUrl: './status-message.component.html',
  styleUrls: ['./status-message.component.scss']
})
export class StatusMessageComponent implements OnInit {

  constructor(
    public dialogRef: MatDialogRef<StatusMessageComponent>,
    @Inject(MAT_DIALOG_DATA) public data: any) { }

    onCancel(): void {
      this.dialogRef.close();

    }

  ngOnInit(): void {
  }

}
