export type Role = 'Participant' | 'Organizer';
export type EventCategory =
  | 'Culture'
  | 'Sport'
  | 'Education'
  | 'Entertainment'
  | 'Professional'
  | 'Charity';
export type ReservationStatus = 'Confirmed' | 'Cancelled';

export const CATEGORY_LABELS: Record<EventCategory, string> = {
  Culture: 'Culture',
  Sport: 'Sport',
  Education: 'Éducation',
  Entertainment: 'Divertissement',
  Professional: 'Professionnel',
  Charity: 'Caritatif',
};

export const CATEGORIES = Object.keys(CATEGORY_LABELS) as EventCategory[];

export interface Session {
  token: string;
  userId: string;
  displayName: string;
  role: Role;
}

export interface EventDto {
  id: string;
  title: string;
  description: string;
  category: EventCategory;
  city: string;
  venue: string;
  startsAtUtc: string;
  capacity: number;
  priceMga: number;
  seatsLeft: number;
  organizerName: string;
}

export type EventInput = Omit<EventDto, 'id' | 'seatsLeft' | 'organizerName'>;

export interface ReservationDto {
  id: string;
  eventId: string;
  eventTitle: string;
  eventStartsAtUtc: string;
  quantity: number;
  totalMga: number;
  status: ReservationStatus;
  ticketCode: string;
  createdAtUtc: string;
}

export interface ParticipantDto {
  displayName: string;
  quantity: number;
  ticketCode: string;
  status: ReservationStatus;
  createdAtUtc: string;
}

export interface EventStatsDto {
  eventId: string;
  title: string;
  capacity: number;
  seatsSold: number;
  seatsLeft: number;
  fillRate: number;
  revenueMga: number;
  commissionMga: number;
  netMga: number;
}

export interface EventFilters {
  q?: string;
  category?: EventCategory | '';
  city?: string;
}
