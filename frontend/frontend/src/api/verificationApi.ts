import axiosInstance from "./axiosInstance";

export interface SubmitVerificationDto {
  firstName: string;
  lastName: string;
  middleName: string;
  birthDate: string;
  gender: "male" | "female"; // <--
  passportSeriesNumber: string;
  personalNumber: string;
  residentialAddress: string; // <--
  officeId: string;
  scheduledAt: string;
}

export interface VerificationRequestDto extends SubmitVerificationDto {
  id: string;
  status: "wait" | "verified" | "declined";
  createdAt: string;
}

export const verificationApi = {
  submitRequest: async (dto: SubmitVerificationDto) => {
    const response = await axiosInstance.post("/verification/submit", dto);
    return response.data;
  },
  getMyRequest: async () => {
    const response =
      await axiosInstance.get<VerificationRequestDto>("/verification/my");
    return response.data;
  },
  updateRequest: async (dto: SubmitVerificationDto) => {
    const response = await axiosInstance.put("/verification/my", dto);
    return response.data;
  },
  getPendingRequests: async () => {
    const response = await axiosInstance.get<VerificationRequestDto[]>(
      "/verification/pending",
    );
    return response.data;
  },
  processRequest: async (requestId: string, isApproved: boolean) => {
    const response = await axiosInstance.post(
      `/verification/${requestId}/process`,
      { isApproved },
    );
    return response.data;
  },
  editRequestByStaff: async (requestId: string, dto: SubmitVerificationDto) => {
    const response = await axiosInstance.put(`/verification/${requestId}`, dto);
    return response.data;
  },
};
