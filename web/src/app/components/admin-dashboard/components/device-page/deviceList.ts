export interface devicelist {
    device_id: string;
    device: string;
    device_type: string;
    patient_name: string;
    active_date: string;
    current_status:string;
}
const deviceList: devicelist[] = [
    {
        device_id: 'BP-056497', 
        device: 'Blood Pressure Monitor', 
        device_type: 'LTE', 
        patient_name: 'Thejasree.M', 
        active_date: '21/10/2022', 
        current_status: 'Active' 
    },
    {
        device_id: 'BP-056498', 
        device: 'Blood Pressure Monitor', 
        device_type: 'LTE', 
        patient_name: 'Thejasree.M', 
        active_date: '21/10/2022', 
        current_status: 'Error' 
    }
]
  