import axiosInstance from "./axiosInstance";

export interface LabTestDto {
  id: string;
  name: string;
  description: string;
}
export interface PatientLabResultDto {
  id: string;
  testName: string;
  date: string;
  officeName: string;
  fileUrl: string;
}

export const labApi = {
  getTests: async () => {
    const response = await axiosInstance.get<LabTestDto[]>("/labs/tests");
    return response.data;
  },
  getMyLabs: async () => {
    const response = await axiosInstance.get<PatientLabResultDto[]>("/labs/my");
    return response.data;
  },
  addLabResult: async (
    patientAccountId: string,
    testId: string,
    fileId: string,
  ) => {
    const response = await axiosInstance.post(
      `/labs/patient/${patientAccountId}`,
      { testId, fileId },
    );
    return response.data;
  },
};
