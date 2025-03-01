import React, { ReactNode, useEffect, useRef, useState } from "react";

interface Dropdown_Menu_Props {
  dropdown_Button: ReactNode | ((props: { open: boolean }) => ReactNode);
  children: ReactNode;
  className?: string;
  OntoggleChanged?: (value: boolean) => void;
}

function Dropdown_Menu({
  dropdown_Button,
  children,
  className,
  OntoggleChanged,
}: Dropdown_Menu_Props) {
  const dropdownRef = useRef<HTMLDivElement | null>(null);
  const [open, setOpen] = useState(false);

  useEffect(() => {
    const handler = (event: MouseEvent) => {
      if (
        dropdownRef.current &&
        !dropdownRef.current.contains(event.target as Node)
      ) {
        setOpen(false);
      }
    };

    document.addEventListener("click", handler);
    return () => {
      document.removeEventListener("click", handler);
    };
  }, []);

  const toggleDropdown = () => {
    setOpen((prevOpen) => !prevOpen);
    if (OntoggleChanged) OntoggleChanged(open);
  };

  const closeDropdown = () => {
    setOpen(false);

    if (OntoggleChanged) OntoggleChanged(false);
  };

  const renderChildren = () => {
    const applyOnClick = (child: React.ReactNode): React.ReactNode => {
      if (!React.isValidElement(child)) return child;
  
      const modifiedChild = React.cloneElement(child as React.ReactElement, {
        onClick: (e: React.MouseEvent<HTMLDivElement>) => {
          if (child.props.onClick) {
            closeDropdown();
            (child.props.onClick as React.MouseEventHandler)(e);
          }
        },
      });
  
      if (child.props.children) {
        return React.cloneElement(modifiedChild, {
          children: React.Children.map(child.props.children, applyOnClick),
        });
      }
  
      return modifiedChild;
    };
  
    return React.Children.map(children, applyOnClick);
  };
  
  return (
    <div ref={dropdownRef}>
      <span
        onClick={(e) => {
          e.preventDefault();
          toggleDropdown();
        }}
      >
       {typeof dropdown_Button === "function"
          ? dropdown_Button({ open })
          : dropdown_Button}
      </span>
      <div
        className={`absolute mt-2 p-2 bg-gray-600 text-white rounded-lg shadow-lg z-50 ${className} 
        [&>button]:w-full [&>button]:text-left [&>button]:py-2 [&>button]:px-4 
        [&>button]:hover:bg-gray-500 [&>button]:rounded-md [&>button]:cursor-pointer
        [&>button]:font-bold [&>button]:disabled:bg-gray-600 [&>button]:disabled:cursor-default`}
        hidden={!open}
      >
        {renderChildren()}
      </div>
    </div>
  );
}

export default Dropdown_Menu;
