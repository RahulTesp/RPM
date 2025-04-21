import { Component, OnInit,Input,Output,EventEmitter} from '@angular/core';


@Component({
  selector: 'app-right-common-button',
  templateUrl: './right-common-button.component.html',
  styleUrls: ['./right-common-button.component.scss']
})
export class RightCommonButtonComponent implements OnInit {

  menu_item:any;
  constructor() { }

  ngOnInit(): void {
  }

  // Side Bar button
isOpen = false;
// @Output() menuChoice  = new EventEmitter<string>();
// @Output() menuChoice: EventEmitter<any> = new EventEmitter();

//  getMenuItem(value:any)
//  {
    // this.menu_item = value;
    // this.menuChoice.emit(this.menu_item);
    // this.menuChoice.emit(value);


//  }
}
