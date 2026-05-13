import axiosInstance from "./axiosInstance";

export interface DoctorAppointmentDto {
  id: string;
  patientName: string;
  patientBirthDate: string;
  patientGender: string;
  scheduledStart: string;
  scheduledEnd: string;
  status: string;
  typeName: string;
}

export interface CompleteAppointmentDto {
  complaints: string;
  objectiveData: string;
  assessment: string;
  plan: string;
}

export const doctorApi = {
  getMyAppointments: async (date: string) => {
    const response = await axiosInstance.get<DoctorAppointmentDto[]>(
      `/appointments/doctor/my?date=${date}`,
    );
    return response.data;
  },
  completeAppointment: async (
    appointmentId: string,
    data: CompleteAppointmentDto,
  ) => {
    const response = await axiosInstance.post(
      `/appointments/${appointmentId}/complete`,
      data,
    );
    return response.data;
  },
};
