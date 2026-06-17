export interface FamilyMember {
  id: string;
  familyGroupId: string;
  userId: string | null;
  fullName: string;
  dateOfBirth: string; // ISO date "YYYY-MM-DD"
  gender: 'male' | 'female' | 'other';
  relationship: 'self' | 'spouse' | 'child' | 'parent' | 'sibling' | null;
  managedBy: string | null;
  createdAt: string;
}

export interface FamilyGroup {
  id: string;
  name: string;
  adminId: string;
  createdAt: string;
  members: FamilyMember[];
}

export interface AddMemberRequest {
  fullName: string;
  dateOfBirth: string;
  gender: string;
  relationship: string | null;
}
