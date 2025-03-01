import { ComponentProps, ReactNode } from "react";

interface IconButtonProps extends ComponentProps<"button"> {
  className?: string;
  children: ReactNode;
}

export function IconButton({ className, children, ...props }: IconButtonProps) {
  return (
    <button
      {...props}
      className={` hover:cursor-pointer rounded-md p-2 flex items-center justify-center transition-all duration-200 text-xl ${className}`}
    >
      {children}
    </button>
  );
}
