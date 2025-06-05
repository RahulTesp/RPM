import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'app-team-detail',
  templateUrl: './team-detail.component.html',
  styleUrls: ['./team-detail.component.scss']
})
export class TeamDetailComponent implements OnInit {

  constructor() { }

  ngOnInit(): void {
  }

  menu = [
    {
      menu_id: 1,
      menu_title: 'Patients',
    },
    {
      menu_id: 2,
      menu_title: 'Alerts',
    },
    {
      menu_id: 3,
      menu_title: 'Tasks',
    },
    {
      menu_id: 4,
      menu_title: 'Schedules',
    },


  ];
  CurrentMenu: any = 1;
  // Change Main Screen
  variable:any =1;
  ChangeScreen(button: any) {
    this.CurrentMenu = button;
    switch (button) {
      case 1:
        this.variable = 1;
        break;

      case 2:
        this.variable = 2;
        break;
        case 3:
          this.variable = 3;
          break;

        case 4:
          this.variable = 4;
          break;
      default:
        this.variable = 1;
        break;
    }
  }
  rowCick(elemnet: any) {}
  PatientList_columnHeader = [
    'selection',
    'patientid',
    'patient',
    'program',
    'weeks_remaining',
    "physician_name",
    "assigned_member",
    "patient_vital_status",

  ];
  radioOptions=false;
  selectRow(element:any)
  {

   var ele = document.getElementsByClassName("patient_rows")

   for(let i=0;i < ele.length;i++){
    ele[i].setAttribute("style","visibility:hidden;")

    // e.setAttribute("style","visibility:visible;")
   }
   var x = ele[element.currentTarget.id]
   x.setAttribute("style","visibility:visible;")

  }
  dataSourcePatientDataList = [			{
    "patientid":"CMD1214568",
     "patient":"Sam Alexander",
     "program":"Care Plan-BG",
     "weeks_remaining":2,
     "physician_name":"Dr.James McArthur",
     "assigned_member":"John Abraham",
     "vitals_perday":20,
     "interactions":"42:08",
     "type":"active",
     "vital":"blood pressure",
     "patient_vital_status":"critical"
    },
    {"patientid":"CMD1214646",
     "patient":"Leann Harvey",
     "program":"Care Plan-BG",
     "weeks_remaining":3,
     "physician_name":"Dr.James McArthur",
     "assigned_member":"John Abraham",
     "vitalsperday":20,
     "interactions":"22:16",
     "type":"onhold",
     "vital":"blood glucose",
     "patient_vital_status":"cautious"
    },
    {"patientid":"CMD1214474",
     "patient":"Jane Davis",
     "program":"Care Plan-BG",
     "weeks_remaining":3,
     "physician_name":"Dr.James McArthur",
     "assigned_member":"John Abraham",
     "vitalsperday":20,
     "interactions":"21:16",
     "type":"enrolled",
     "vital":"weight",
     "patient_vital_status":"normal"
     }]


     Alert_Header = [
      'alert_name',
      'priority',
      'patient_name',
      'assignee_name',
      'due_date',
      'status',
    ];

    Tasks_columnHeader = [
      'tasktype',
      'priority',
      'patient_name',
      'created_by',
      'due_date',
      'status',
      'action',
    ];


searchAssigned = false;
searchValueName = false;
alertSearchValeName = false;
alertAssignedName = false;

serachAssigned()
{
   this.searchAssigned = !this.searchAssigned;
}
searchNameClick()
{
  this.searchValueName = !this.searchValueName;
}
applyFilter(event: Event) {

  const filterValue = (event.target as HTMLInputElement).value;
  // this.Tasks_dataSource.filter = filterValue.trim().toLowerCase();

}

// Alert
serachAlertAssigned()
{
   this.alertAssignedName = !this.alertAssignedName;
}
searchAlertNameClick()
{
  this.alertSearchValeName = !this.alertSearchValeName;
}
applyAlertFilter(event: Event)
{
  const filterValue = (event.target as HTMLInputElement).value;
  // this.alertdatasource.filter = filterValue.trim().toLowerCase();
}


alertdatasource = [ {
  "id":"ID1",
  "alert_name": "Test Alert",
  "alert_description": "blood pressure above critical range",
  "priority": "urgent",
  "iscritical": true,
  "alert_type": "Critical Alerts",
  "patient_name": "Patient X",
  "assignedto": "Member X",
  "duedate": "10-11-2022 10:45:12 AM",
  "status": "Todo",
  "duration_min" : 10,
  "frequency":"monthly",
  "comments": "Test comments",
  "watcher": ["watcher A", "watcher B"]
  },
 {
  "id":"ID2",
  "alert_name": "Test Alert 2",
  "alert_description": "blood pressure above critical range",
  "priority": "urgent",
  "iscritical": true,
  "alert_type": "SLA Breached",
  "patient_name": "Patient X",
  "assignedto": "Member X",
  "duedate": "10-11-2022 10:45:12 AM",
  "status": "Todo",
  "duration_min" : 10,
  "frequency":"monthly",
  "comments": "Test comments",
  "watcher": ["watcher A", "watcher B"]
  }]


  taskdataSource = [	  {
    "id":"ID1",
    "tasktype":"Vital Reading",
    "taskname": "Follow up with team ABCD",
    "description":"Test description",
    "patient_name":"Maria",
    "patient_id":"PT003",
    "created_by":"Priya",
    "created_id":"MN001",
    "due_date": "26-11-2021",
    "status":"To do",
    "assignee_name":"Alex",
    "priority":"high",
    "comments":"test comments",
    "watcher": ["abc","xyz"]
   },
    {
    "id":"ID2",
    "tasktype":"Vital Reading",
    "taskname": "Follow up with team ABCD",
    "description":"Test description",
    "patient_name":"Maria",
    "patient_id":"PT004",
    "created_by":"Priya",
    "created_id":"MN001",
    "due_date": "26-11-2021",
    "status":"To do",
    "assignee_name":"Alex",
    "priority":"high",
    "comments":"test comments",
    "watcher": ["abc","xyz"]
   }]





   keys: Array<string>;
   searchValue:any;
//    schedule = [



//       {
//         "date": 'Nov 26 2021',
//         "id":'SCH001',
//         "time": '11:30AM - 11:45AM',
//         "schedule": 'Call with Physician',
//         "description": 'Check Patient Vital Check Patient VitalCheck Patient VitalCheck Patient Patient Vital Check Patient VitalCheck Patient VitalCheck Patient  Patient Vital Check Patient VitalCheck Patient VitalCheck Patient  Patient Vital Check Patient VitalCheck Patient VitalCheck Patient  Patient Vital Check Patient VitalCheck Patient VitalCheck Patient  Patient Vital Check Patient VitalCheck Patient VitalCheck Patient  Patient Vital Check Patient VitalCheck Patient VitalCheck Patient  Patient Vital Check Patient VitalCheck Patient VitalCheck Patient  Patient Vital Check Patient VitalCheck Patient VitalCheck Patient  Patient Vital Check Patient VitalCheck Patient VitalCheck Patient  Patient Vital Check Patient VitalCheck Patient VitalCheck Patient  Patient Vital Check Patient VitalCheck Patient VitalCheck Patient  Patient Vital Check Patient VitalCheck Patient VitalCheck Patient  Patient Vital Check Patient VitalCheck Patient VitalCheck Patient  Patient Vital Check Patient VitalCheck Patient VitalCheck Patient  Patient Vital Check Patient VitalCheck Patient VitalCheck Patient  Patient Vital Check Patient VitalCheck Patient VitalCheck Patient  Patient Vital Check Patient VitalCheck Patient VitalCheck Patient  VitalCheck Patient Vital Check Patient Vital Check Patient Vital Check Patient Vital Check Patient Vital ',
//         "allcontacts": 'Francis Underwood',
//       },
//       {
//         "date": 'Nov 26 2021',
//        "id":'SCH002',
//         "time": '03:00PM - 03:15PM',
//         "schedule": 'Wellness one Call ',
//         "description": 'urgent',
//         "allcontacts": 'Sam Alexander',
//       },
//       {
//         "date": 'Nov 26 2021',
//         "id":'SCH001',
//         "time": '11:30AM - 11:45AM',
//         "schedule": 'Call with Physician',
//         "description": 'Check Patient Vital Check Patient VitalCheck Patient VitalCheck Patient Patient Vital Check Patient VitalCheck Patient VitalCheck Patient  Patient Vital Check Patient VitalCheck Patient VitalCheck Patient  Patient Vital Check Patient VitalCheck Patient VitalCheck Patient  Patient Vital Check Patient VitalCheck Patient VitalCheck Patient  Patient Vital Check Patient VitalCheck Patient VitalCheck Patient  Patient Vital Check Patient VitalCheck Patient VitalCheck Patient  Patient Vital Check Patient VitalCheck Patient VitalCheck Patient  Patient Vital Check Patient VitalCheck Patient VitalCheck Patient  Patient Vital Check Patient VitalCheck Patient VitalCheck Patient  Patient Vital Check Patient VitalCheck Patient VitalCheck Patient  Patient Vital Check Patient VitalCheck Patient VitalCheck Patient  Patient Vital Check Patient VitalCheck Patient VitalCheck Patient  Patient Vital Check Patient VitalCheck Patient VitalCheck Patient  Patient Vital Check Patient VitalCheck Patient VitalCheck Patient  Patient Vital Check Patient VitalCheck Patient VitalCheck Patient  Patient Vital Check Patient VitalCheck Patient VitalCheck Patient  Patient Vital Check Patient VitalCheck Patient VitalCheck Patient  VitalCheck Patient Vital Check Patient Vital Check Patient Vital Check Patient Vital Check Patient Vital ',
//         "allcontacts": 'Francis Underwood',
//       },
//       {
//         "date": 'Nov 26 2021',
//        "id":'SCH002',
//         "time": '03:00PM - 03:15PM',
//         "schedule": 'Wellness one Call',
//         "description": 'urgent',
//         "allcontacts": 'Sam Alexander',
//       },


// ]


   schedule = [
     {
    date: 'Nov 22 2021',
    data: [
      {
        "id":'SCH001',
        "time": '11:30AM - 11:45AM',
        "schedule": 'Call with Physician',
        "description": 'Check Patient Vital Check Patient VitalCheck Patient VitalCheck Patient Patient Vital Check Patient VitalCheck Patient VitalCheck Patient  Patient Vital Check Patient VitalCheck Patient VitalCheck Patient  Patient Vital Check Patient VitalCheck Patient VitalCheck Patient  Patient Vital Check Patient VitalCheck Patient VitalCheck Patient  Patient Vital Check Patient VitalCheck Patient VitalCheck Patient  Patient Vital Check Patient VitalCheck Patient VitalCheck Patient  Patient Vital Check Patient VitalCheck Patient VitalCheck Patient  Patient Vital Check Patient VitalCheck Patient VitalCheck Patient  Patient Vital Check Patient VitalCheck Patient VitalCheck Patient  Patient Vital Check Patient VitalCheck Patient VitalCheck Patient  Patient Vital Check Patient VitalCheck Patient VitalCheck Patient  Patient Vital Check Patient VitalCheck Patient VitalCheck Patient  Patient Vital Check Patient VitalCheck Patient VitalCheck Patient  Patient Vital Check Patient VitalCheck Patient VitalCheck Patient  Patient Vital Check Patient VitalCheck Patient VitalCheck Patient  Patient Vital Check Patient VitalCheck Patient VitalCheck Patient  Patient Vital Check Patient VitalCheck Patient VitalCheck Patient  VitalCheck Patient Vital Check Patient Vital Check Patient Vital Check Patient Vital Check Patient Vital ',
        "allcontacts": 'Francis Underwood',
      },
      {
       "id":'SCH002',
        "time": '03:00PM - 03:15PM',
        "schedule": 'Wellness one  Call',
        "description": 'urgent',
        "allcontacts": 'Sam Alexander',
      },
    ],
  },
  {
    date: 'Nov 23 2021',
    data: [
      {
        "id":'SCH001',
        "time": '11:30AM - 11:45AM',
        "schedule": 'Call with Physician',
        "description": 'Check Patient Vital   Patient Vital Check Patient Vital ',
        "allcontacts": 'Francis Underwood',
      },
      {
       "id":'SCH002',
        "time": '03:00PM - 03:15PM',
        "schedule": 'Wellness one  Call',
        "description": 'urgent',
        "allcontacts": 'Sam Alexander',
      },
    ],
  },
  {
    date: 'Nov 24 2021',
    data: [
      {
        "id":'SCH001',
        "time": '11:30AM - 11:45AM',
        "schedule": 'Call with Physician',
        "description": 'Check Patient Vital   Patient Vital Check Patient Vital ',
        "allcontacts": 'Francis Underwood',
      },
      {
       "id":'SCH002',
        "time": '03:00PM - 03:15PM',
        "schedule": 'Wellness one  Call',
        "description": 'urgent',
        "allcontacts": 'Sam Alexander',
      },
    ],
  },
  {
    date: 'Nov 25 2021',
    data: [
      {
        "id":'SCH001',
        "time": '11:30AM - 11:45AM',
        "schedule": 'Call with Physician',
        "description": 'Check Patient Vital   Patient Vital Check Patient Vital ',
        "allcontacts": 'Francis Underwood',
      },
      {
       "id":'SCH002',
        "time": '03:00PM - 03:15PM',
        "schedule": 'Wellness one  Call',
        "description": 'urgent',
        "allcontacts": 'Sam Alexander',
      },
    ],
  }
]


applyFilterdata()
{
  // let c = this.schedule.filter(d => d.data.every(c => this.searchValue.includes(c.id)));
  // console.log(c);
 let result = this.schedule.filter(a => a.data.some(c => this.searchValue.includes(c.id)));
}

 data =[{ menuName: "Hot dogs", menu: [ { dishId: '1', dish_has_categories: [{ CategoryId: '8' }] }, { dishId: '2', dish_has_categories: [{ CategoryId: '9' }] }] }, { menuName: "Burgers", menu: [{ dishId: '3', dish_has_categories: [{ CategoryId: '6' }] }, { dishId: '4', dish_has_categories: [{ CategoryId: '4' }] }] }, { name: "Drinks", menu: [] } ]

applyFiltert()
{

//   let op = this.data.filter(val => {
//     let menu = val.menu.some(({dish_has_categories}) => dish_has_categories.some(({CategoryId}) => CategoryId === '8'))
//     return menu
//   })

//   console.log('filtered values -->\n',op)
//   let names = op.map(({menuName})=> menuName)

// console.log('Names --> \n', names)
}





}
