import axiosInstance from "./axiosInstance";

export interface MedicalRecordDto {
  id: string;
  date: string;
  doctorName: string;
  specialization: string;
  complaints: string;
  objectiveData: string;
  assessment: string;
  plan: string;
}

export const patientApi = {
  getMedicalHistory: async () => {
    const response =
      await axiosInstance.get<MedicalRecordDto[]>("/clinic/my-history");
    return response.data;
  },
};
