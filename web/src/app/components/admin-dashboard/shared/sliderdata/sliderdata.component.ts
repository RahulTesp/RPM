import { Component, OnInit ,Output,Input} from '@angular/core';

@Component({
  selector: 'app-sliderdata',
  templateUrl: './sliderdata.component.html',
  styleUrls: ['./sliderdata.component.scss']
})
export class SliderdataComponent implements OnInit {

   @Input() low_critical:any;
   @Input()  low_cautious:any;
   @Input()  low_normal:any;
   @Input()  high_normal:any;
   @Input() high_cautious:any;
   @Input()  high_critical:any
  constructor() { }

  ngOnInit(): void {
  }

}
