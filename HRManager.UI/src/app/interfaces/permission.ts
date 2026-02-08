// src/app/interfaces/permission.ts
export interface Permission {
  id: string;
  code: string;
  name: string;
  module: string;
  category: string;
  description?: string;
  isActive: boolean;
}
