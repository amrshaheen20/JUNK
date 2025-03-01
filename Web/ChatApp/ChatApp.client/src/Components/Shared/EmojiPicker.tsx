import data from "@emoji-mart/data";
import Picker from "@emoji-mart/react";
import { useTheme } from "../../Hooks/use-theme";

interface EmojiPickerProps {
  onSelect: (emoji: string) => void;
}

const EmojiPicker = ({ onSelect }: EmojiPickerProps) => {
  const theme = useTheme(); 

  return (
    <Picker
      autoFocus
      data={data}
      theme={theme}
      showPreview={false}
      showSkinTones={false}
      onEmojiSelect={(emoji: { native: string }) => {
        if (emoji?.native) {
          onSelect(emoji.native);
        }
      }}
    />
  );
};

export default EmojiPicker;
