import { ChangeEvent, ComponentProps, useRef } from "react";
import { BiPlus } from "react-icons/bi";
import { GetFileUrl } from "../../Common/GetMediaUrl";

export interface UploadedFileProps {
  File: File | null;
  FileId?: string | null;
}

interface Image_Upload_Props extends ComponentProps<"div"> {
  File: UploadedFileProps;
  onFileChange: (file: UploadedFileProps) => void;
}

function Image_Upload({ File, onFileChange, ...props }: Image_Upload_Props) {
  const fileInputRef = useRef<HTMLInputElement | null>(null);

  const handlePhotoChange = (e: ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (file) {
      onFileChange({ File: file, FileId: null });
    }
  };

  const imageSrc = File?.File
    ? URL.createObjectURL(File.File)
    : File?.FileId
    ? GetFileUrl(File.FileId)
    : null;

  return (
    <div {...props} className="flex justify-center mb-6 " >
      <div 
      className="w-32 h-32 cursor-pointer "
      onClick={() => fileInputRef.current?.click()}>
        {imageSrc ? (
          <img
            src={imageSrc}
            className="rounded-full border-4 border-blue-500 object-cover w-full h-full"
          />
        ) : (
          <div className="rounded-full border-4 border-gray-500 flex justify-center items-center bg-gray-200 w-full h-full">
            <BiPlus className="text-5xl text-gray-500" />
          </div>
        )}
      </div>

      <input
        ref={fileInputRef}
        type="file"
        onChange={handlePhotoChange}
        className="hidden"
        accept="image/*"
      />
    </div>
  );
}

export default Image_Upload;
