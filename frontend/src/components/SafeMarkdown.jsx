import ReactMarkdown from 'react-markdown'

/**
 * Markdown renderer. Raw HTML is NOT enabled (no rehype-raw), so embedded
 * HTML is escaped by react-markdown itself — this is the XSS safeguard
 * (RNF-16). If raw HTML is ever needed, it must go through DOMPurify first.
 */
export default function SafeMarkdown({ text }) {
  return (
    <div className="markdown">
      <ReactMarkdown>{text ?? ''}</ReactMarkdown>
    </div>
  )
}
