import { useState } from "react";
import { FaCheck } from "react-icons/fa6";
import { LuClipboard } from "react-icons/lu";
import { IconButton } from "./IconButton";
import { logError } from "../../Common/Logger";

interface CopyButtonProps {
  text: string;
  className?: string;
}

export default function CopyButton({ text, className }: CopyButtonProps) {
  const [isCopied, setIsCopied] = useState(false);

  const handleCopy = async () => {
    try {
      await navigator.clipboard.writeText(text);
      setIsCopied(true);
      setTimeout(() => setIsCopied(false), 2000);
    } catch (err) {
      logError("Failed to copy: ", err);
    }
  };

  return (
    <IconButton onClick={handleCopy} className={className}>
      {isCopied ? (
        <FaCheck className="text-green-400" />
      ) : (
        <LuClipboard className="text-gray-400" />
      )}
    </IconButton>
  );
}
