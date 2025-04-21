export class AddPatientMasterData{
    ProgramDetailsMasterData:object[]
    ClinicDetails:object[]
    PhysicianDetails:object[]
    Cities:object[]
    CareTeamMembers:object[]
    PatientStatuses:object[]
}

export class CityAndStatesMasterData{
    States:object[]
    Cities:object[]
}












export class home{
    patient_overview:{
        is_clinic:boolean
        list:[
                {
                    name:string
                    active:number
                    onhold:number
                    prescribed:number
                    enrolled:number
                    readytodischarge:number
                    discharged:number
                    total:number
                }
            ]
    }
    patient_health_overview:{
       all:{
            day:{
                crtical:number
                cautious:number
                normal:number
            }
            week:{
                crtical:number
                cautious:number
                normal:number
            }
            month:{
                crtical:number
                cautious:number
                normal:number
            }
       }
       bg:{
            day:{
                crtical:number
                cautious:number
                normal:number
            }
            week:{
                crtical:number
                cautious:number
                normal:number
            }
            month:{
                crtical:number
                cautious:number
                normal:number
            }
        }
        bp:{
            day:{
                crtical:number
                cautious:number
                normal:number
            }
            week:{
                crtical:number
                cautious:number
                normal:number
            }
            month:{
                crtical:number
                cautious:number
                normal:number
            }
       }
       weight:{
            day:{
                crtical:number
                cautious:number
                normal:number
            }
            week:{
                crtical:number
                cautious:number
                normal:number
            }
            month:{
                crtical:number
                cautious:number
                normal:number
            }
        }
        pox:{
                day:{
                    crtical:number
                    cautious:number
                    normal:number
                }
                week:{
                    crtical:number
                    cautious:number
                    normal:number
                }
                month:{
                    crtical:number
                    cautious:number
                    normal:number
                }
        }
   }
    billing:{
        today:[
                    {
                        cptcode:number
                        data:
                        {
                        
                            ready:number
                            missing:number
                            onhold:number
                            total:number                        
                        }
                    }
                ]
        last5:[
                    {
                        cptcode:number
                        data:
                        {
                         
                            ready:number
                            missing:number
                            onhold:number
                            total:number                        
                        }
                    }
                ]
        next5:[
            {
                cptcode:number
                data:
                {
                  
                    ready:number
                    missing:number
                    onhold:number
                    total:number                        
                }
            }
        ]
    }
        
    todays_tasks:[
            {
                description:string
                priority:string
                alert:boolean
                patient:string
            }
        ]
    
    team_overview:{
        overview:[
            {
                teamname:string
                alerts:number
                duetoday:number
                slabreached:number
            }
        ]
    }

}

export class userprofile{
    firstname:string
    lastname:string
    middlename:string
    email:string
    phone:string
    username:string
    location:string
    timezone:string
    role:string
    team:string
    accessrights:[]
}

export class worklist{
    alerts:[
        {
            alert_name:string
            alert_description:string
            priority:string
            iscritical:boolean
            alert_type:string
            patient_name:string
            assignedto:string
            duedate:string
            status:string
            duration_min:number
            frequency:string
            comments:string
            watcher:[]
        }
    ]
    tasks:[
        {
            tasktype:string
            taskname:string
            description:string
            contact_name:string
            created_by:string
            due_date:string
            status:string
            assignee_name:string
            priority:string
            comments:string
            watcher:[]
        }
    ]
}

export class listener{
    notification:[
        {
            id:string
            description:string
            time:string
            read:boolean
        }
    ]
    schedule:[
        {
            id:string
            type:string
            description:string
            patientname:string
            patient_id:string
            assignee:string
            assignee_id:string
            start_date:string
            end_date:string
            frequency:{
                time:string
                weeks:[]
                days:[]
            }
            start_time:string
            duration_min:number
        }
    ]
    alerts:[
        {
            alert_id:string
            alert_type:string
            alert_description:string
            priority:string
            iscritical:boolean
            patient_name:string
            patient_id:string
            assignee_name:string
            assignee_id:string
            duedate:string
            status:string
            watcher:string
        }
    ]
}