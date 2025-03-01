import { ComponentProps, ReactNode } from "react";

interface IconButtonWithTextProps extends ComponentProps<"button"> {
  label?: string;
  icon?: ReactNode;
  className?: string;
  children?: ReactNode;
}

const IconButtonWithText = ({
  icon,
  label = "",
  className = "",
  children,
  ...props
}: IconButtonWithTextProps) => {
  return (
    <button
      {...props}
      className={`flex w-full items-center justify-between gap-2 px-3 py-2 rounded-md transition select-none cursor-pointer ${className}`}
    >
      <div className="flex items-center gap-2 flex-1 min-w-0">
        {icon && <span className="flex-shrink-0">{icon}</span>}
        <span className="truncate">{label}</span>
      </div>

      {children && <div className="flex flex-shrink-0">{children}</div>}
    </button>
  );
};

export default IconButtonWithText;
