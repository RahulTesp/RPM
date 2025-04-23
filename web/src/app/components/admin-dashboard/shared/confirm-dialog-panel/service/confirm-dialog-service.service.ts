import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class ConfirmDialogServiceService {
  private _showDialog = new BehaviorSubject<boolean>(false);
  private _message = new BehaviorSubject<string>('');
  private _title = new BehaviorSubject<string>('Message');
  private _showCancel = new BehaviorSubject<boolean>(true);
  private _confirmAction: (() => void) | null = null;

  showDialog$ = this._showDialog.asObservable();
  title$ = this._title.asObservable();
  message$ = this._message.asObservable();
  showCancel$ = this._showCancel.asObservable();

  showConfirmDialog(
    message: string,
    title: string = 'Message',
    action: (() => void) | null = null,
    showCancel: boolean = true
  ) {
    this._title.next(title);
    this._message.next(message);
    this._showDialog.next(true);
    this._showCancel.next(showCancel);
    this._confirmAction = action;
  }

  handleConfirm(confirmed: boolean) {
    this._showDialog.next(false);

    if (confirmed && this._confirmAction) {
      this._confirmAction();
    }

    this._confirmAction = null;
  }
}
