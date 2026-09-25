export interface HealthResponse {
  status: string;
  timestamp: string;
  version: string;
}

export interface ApiError {
  statusCode: number;
  message: string;
  errors?: Record<string, string[]>;
}
