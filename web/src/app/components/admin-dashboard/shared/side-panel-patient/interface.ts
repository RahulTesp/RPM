export interface TimeOption {
  id: string;
  value: string;
}

export function generateTimeOptions(limit: number): TimeOption[] {
  return Array.from({ length: limit }, (_, i) => {
    const val = i.toString().padStart(2, '0');
    return { id: val, value: val };
  });
}
