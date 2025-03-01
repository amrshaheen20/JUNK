// ======================
// Core Response Structure
// ======================
export interface DataWithId {
  id: string;
}

// ==============
// Shared Enums/Types
// ==============
export enum ChannelType {
  TEXT = "TEXT",
  VIDEO = "VIDEO",
  AUDIO = "AUDIO",
}

export enum MemberRole {
  ADMIN = "ADMIN",
  MODERATOR = "MODERATOR",
  GUEST = "GUEST",
}

// ==============
// Attachment Types
// ==============
export interface AttachmentResponseDto {
  id: string;
  name: string;
  contentType: string;
}

// ==============
// User/Profile Types
// ==============
export interface AuthorResponseDto {
  id?: string;
  imageId?: string;
  userName?: string;
  isMember: boolean;
}

export interface MutualServerDto {
  id: string;
  name: string;
  imageId?: string;
}

export interface ProfileResponseDto {
  id: string;
  displayName: string;
  userName: string;
  imageId?: string;
  email: string;
  bio?: string;
  joinTime: Date;
}

export interface ProfileWithMoreDataResponseDto extends ProfileResponseDto {
  mutualServers: MutualServerDto[];
}

// ==============
// Member Types
// ==============
export interface MemberResponseDto {
  id: string;
  userName: string;
  imageId?: string;
  role: MemberRole;
}

// ==============
// Channel Types
// ==============
export interface ChannelResponseDto {
  id: string;
  name: string;
  type: ChannelType;
  CreatedBy: string;
  serverId: string;
  createdAt: Date;
}

export interface CreateChannelRequestDto {
  name: string;
  channelType?: ChannelType;
}

// ==============
// Message Types
// ==============
export interface MessageResponseDto {
  id: string;
  channelId: string;
  content?: string;
  attachments?: AttachmentResponseDto[];
  author: AuthorResponseDto;
  isEdited: boolean;
  createdAt: Date;
}

// ==============
// Conversation Types
// ==============
export interface ConversationResponseDto {
  id: string;
  channelId: string;
  targetProfile: ProfileResponseDto;
  createdAt: Date;
}

// ==============
// Server Types
// ==============
export interface ServerResponseDto {
  id: string;
  name: string;
  imageId?: string;
  inviteCode: string;
  membersCount: number;
  channelsCount: number;
  memberSince?: Date;
}
