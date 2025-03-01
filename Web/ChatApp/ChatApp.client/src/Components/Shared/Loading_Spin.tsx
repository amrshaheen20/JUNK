import { AiOutlineLoading3Quarters } from "react-icons/ai";

interface Loading_Spinner_Props{
    className?:string;
}

function Loading_Spinner({className,...props}:Loading_Spinner_Props) {
    return ( <AiOutlineLoading3Quarters {...props} className={`text-white text-2xl animate-spin ${className}`} />);
}

export default Loading_Spinner;