import { Injectable } from '@angular/core';
import { RPMService } from './rpm.service';
export interface TaskMasterData {
  CareTeamMembersList: any[]; // Replace `any` with your actual model
  PatientList: { PatientId: number; CareTeamId: number }[];
}
@Injectable({
  providedIn: 'root'
})
export class MasterDataService {

  constructor(private rpm: RPMService) {}

  getFilteredTaskAssignees(
  roleId: number,
  patientId?: number,
  userId?: number
): Promise<{
  taskMasterData: TaskMasterData;
  filteredAssignees: any[];
}> {
  return this.rpm
    .rpm_get(`/api/tasks/gettaskmasterdata?RoleId=${roleId}`)
    .then((data) => {
      const response = data as TaskMasterData;
      let filteredAssignees = response.CareTeamMembersList;

      // 🧠 Filter by PatientId if provided
      if (patientId) {
        const currentCareTeam = response.PatientList.find(
          (p) => p.PatientId === patientId
        );
        if (currentCareTeam) {
          filteredAssignees = filteredAssignees.filter(
            (member) => member.CareTeamId === currentCareTeam.CareTeamId
          );
        }
      }

      // 🧠 Filter by UserId if provided
      // Note: This block currently runs ONLY if patientId is NOT present.
      // If that's the desired logic (PatientId filter takes precedence), it's fine.
      else if (userId) {
        const userTeam = filteredAssignees.find((m) => m.UserId === +userId);
        if (userTeam) {
          filteredAssignees = filteredAssignees.filter(
            (m) => m.CareTeamId === userTeam.CareTeamId
          );
        }
      }

      // -----------------------------------------------------------
      // ✅ NEW: DE-DUPLICATION STEP (The Core Fix)
      // Remove duplicate team members based on their unique UserId.
      // -----------------------------------------------------------
      const uniqueMembersMap = new Map();
      
      filteredAssignees.forEach((item: any) => {
        // Use 'UserId' as the unique key for the map
        if (!uniqueMembersMap.has(item.UserId)) {
          uniqueMembersMap.set(item.UserId, item);
        }
      });
      
      // Convert the map values (the unique objects) back to an array
      const finalUniqueAssignees = Array.from(uniqueMembersMap.values());
      // -----------------------------------------------------------

      return {
        taskMasterData: response,
        filteredAssignees: finalUniqueAssignees, // Return the guaranteed unique list
      };
    });
}

// getFilteredTaskAssignees(
//   roleId: number,
//   patientId?: number,
//   userId?: number
// ): Promise<{
//   taskMasterData: TaskMasterData;
//   filteredAssignees: any[];
// }> {
//   return this.rpm
//     .rpm_get(`/api/tasks/gettaskmasterdata?RoleId=${roleId}`)
//     .then((data) => {
//       const response = data as TaskMasterData;
//       let filteredAssignees = response.CareTeamMembersList;

//       // 🧠 Filter by PatientId if provided
//       if (patientId) {
//         const currentCareTeam = response.PatientList.find(
//           (p) => p.PatientId === patientId
//         );
//         if (currentCareTeam) {
//           filteredAssignees = filteredAssignees.filter(
//             (member) => member.CareTeamId === currentCareTeam.CareTeamId
//           );
//         }
//       }

//       // 🧠 Filter by UserId if provided
//       else if (userId) {
//         const userTeam = filteredAssignees.find((m) => m.UserId === +userId);
//         if (userTeam) {
//           filteredAssignees = filteredAssignees.filter(
//             (m) => m.CareTeamId === userTeam.CareTeamId
//           );
//         }
//       }

//       return {
//         taskMasterData: response,
//         filteredAssignees,
//       };
//     });
// }
getScheduleMasterData(roleId: number, patientUserId?: number): Promise<{
  rawData: any;
  scheduleTypes: any[];
  patientOrContacts: any[];
  filteredAssignees: any[];
}> {
  return this.rpm
    .rpm_get(`/api/schedules/getschedulemasterdata?RoleId=${roleId}`)
    .then((data: any) => {
      const scheduleTypes = data.ScheduleTypes?.filter((x: any) => x.IsPatient === 1) || [];
      const patientOrContacts = data.PatientOrContactName?.filter((x: any) => x.IsPatient === 1) || [];
      const assignees = data.AssigneeList || [];

      // Optional: Filter Assignees by patient's care team
      let filteredAssignees = assignees;
      if (patientUserId) {
        const userTeam = patientOrContacts.find((c: any) => c.Id === patientUserId);
        if (userTeam) {
          filteredAssignees = assignees.filter((c: any) => c.CareTeamId === userTeam.CareTeamId);
        }
      }

      // Optional: Add searchField to patientOrContacts
      patientOrContacts.forEach((x: any) => {
        x.searchField = `${x.Name}[${x.ProgramName}]`;
      });

      return {
        rawData: data,
        scheduleTypes,
        patientOrContacts,
        filteredAssignees,
      };
    });
}
  formatAMPM(duedate: any) {
    var hours = duedate.getHours();
    var minutes = duedate.getMinutes();
    var ampm = hours >= 12 ? 'PM' : 'AM';
    hours = hours % 12;
    hours = hours ? hours : 12; // the hour '0' should be '12'
    var Xminutes = minutes < 10 ? '0' + minutes : minutes;
    var strTime = hours + ':' + Xminutes + ' ' + ampm;
    return strTime;
  }
}
