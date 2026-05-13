export interface SpecializationDto {
  id: string;
  name: string;
  experienceYears: number;
}

export interface DoctorDto {
  id: string;
  firstName: string;
  lastName: string;
  middleName: string;
  bio: string;
  avatarUrl: string;
  ratingAvg: number;
  officeId: string;
  specializations: SpecializationDto[];
}

export interface PaginatedList<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
}

export interface OfficeDto {
  id: string;
  name: string;
  address: string;
  phone: string;
  photoUrl: string;
}

export interface AppointmentTypeDto {
  id: string;
  category: string | number;
  defaultDurationMinutes: number;
}
