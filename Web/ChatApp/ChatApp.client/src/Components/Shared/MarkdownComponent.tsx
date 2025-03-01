import ReactMarkdown from "react-markdown";

interface MarkdownProps {
  children: string;
}

export default function MarkdownComponent({ children }: MarkdownProps) {
  return (
    <div className="prose prose-invert max-w-none whitespace-pre-wrap break-all select-text">
      <ReactMarkdown
        components={{
          h1: ({ children }) => (
            <h1 className="text-2xl font-bold text-blue-400">{children}</h1>
          ),
          h2: ({ children }) => (
            <h2 className="text-xl font-semibold text-green-400">{children}</h2>
          ),
          h3: ({ children }) => (
            <h3 className="text-lg font-medium text-red-400">{children}</h3>
          ),
          p: ({ children }) => (
            <p className="font-mono whitespace-pre-wrap">
              {children}
            </p>
          ),
          ul: ({ children }) => (
            <ul className="list-disc pl-5 text-gray-300">{children}</ul>
          ),
          a: ({ children, href }) => (
            <a
              href={href}
              className="text-blue-400 hover:underline"
              target="_blank"
              rel="noopener noreferrer"
            >
              {children}
            </a>
          ),
          pre: ({ children }) => (
            <pre className="bg-gray-700 text-gray-200 p-3 rounded w-full overflow-auto max-w-full break-all border-1 border-gray-600 shadow-lg">
              {children}
            </pre>
          ),
          code: ({ children }) => (
            <code className="bg-gray-700 text-gray-200 px-2 py-1 rounded break-words whitespace-pre-wrap w-full block">
              {children}
            </code>
          ),
        }}
      >
        {children.replace(/\r\n|\r|\n/gi, '&nbsp;\n')}
      </ReactMarkdown>
    </div>
  );
}
