export interface BillingData {
  CPTCode: string;
  Completed: number;
  Total: number | null;
  IsTargetMet: boolean;
  BillingStartDate: string;
  ProgramName: string;
  Priority?: string;
  AssignedMember?: string;
  TaskOrAlert?: string;
}
export const billingPCM_P = [
  {
    cptcode: 8,
    description: 'PCM Services - 1st 30 Mins - Clinical Staff ',
  },
  {
    cptcode: 99,
    description: "PCM Services - Add'l 30 Mins - Clinical Staff ",
  },
];

export const billingPCM_C = [
  {
    cptcode: 99424,
    description: 'PCM Services - 1st 30 Mins - Physician or QHCP ',
  },
  {
    cptcode: 99425,
    description: "PCM Services - Add'l 30 Mins - Physician or QHCP  ",
  },
];

export const billingC_CCM = [
  {
    cptcode: 'G0506',
    description: 'Initial visit Care Planning ',
  },
  {
    cptcode: 99487,
    description: 'C-CCM Services - 1st 60 Mins - Clinical Staff ',
  },
  {
    cptcode: 99489,
    description: "C-CCM Services - Add'l 30 Mins - Clinical Staff ",
  },
];
export const billingCCM_P = [
  {
    cptcode: 'G0506',
    description: 'Initial visit Care Planning ',
  },

  {
    cptcode: 99491,
    description: 'CCM Services - 1st 30 Mins - Physician or QHCP ',
  },
  {
    cptcode: 99437,
    description: "CCM Services - Add'l 30 Mins - Physician or QHCP ",
  },
];
export const billingCCM_C = [
  { cptcode: 'G0506', description: 'Initial visit Care Planning ' },
  {
    cptcode: 99490,
    description: 'CCM Services - 1st 20 Mins - Clinical Staff ',
  },
  {
    cptcode: 99439,
    description: "CCM Services - Add'l 20 Mins - Clinical Staff ",
  },
];
