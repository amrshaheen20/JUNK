import { AttachmentResponseDto } from "./Dtos";

export function GetFileUrl(Id?: string): string | undefined {
  if (!Id) return undefined;

  return `${import.meta.env.VITE_API_URL}/Attachments/${Id}`;
}

export function GetFileFromObjectUrl(
  obj: AttachmentResponseDto
): string | undefined {
  return `${import.meta.env.VITE_API_URL}/Attachments/${obj.id}/${obj.name}`;
}
