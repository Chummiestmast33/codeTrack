import { useEffect, useRef, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { submissionFile } from '../../api/deliveries.js'

const imageTypes = { png: 'image/png', jpg: 'image/jpeg', jpeg: 'image/jpeg', webp: 'image/webp' }
const textExtensions = new Set(['cpp', 'c', 'h', 'py', 'java', 'txt', 'md'])

export default function SubmissionFileActions({ submission }) {
  const { t } = useTranslation()
  const [busy, setBusy] = useState(false)
  const [error, setError] = useState(false)
  const [preview, setPreview] = useState(null)
  const dialog = useRef(null)
  const extension = submission.fileName?.split('.').pop().toLowerCase()
  const canPreview = textExtensions.has(extension) || Boolean(imageTypes[extension]) || extension === 'pdf'

  useEffect(() => {
    if (preview) dialog.current?.showModal()
    return () => {
      if (preview?.url) URL.revokeObjectURL(preview.url)
    }
  }, [preview])

  async function openFile(showPreview) {
    setBusy(true)
    setError(false)
    try {
      const ticket = await submissionFile(submission.id)
      // No API bearer token is sent to storage; the signed URL grants temporary access.
      const response = await fetch(ticket.downloadUrl, { credentials: 'omit', cache: 'no-store' })
      if (!response.ok) throw new Error('File request failed')
      const blob = await response.blob()
      if (showPreview) {
        if (textExtensions.has(extension)) {
          setPreview({ text: await blob.text() })
        } else {
          // Only feed PDF bytes to the browser's native PDF viewer, never HTML
          // mislabeled by a submission's content type or filename.
          if (extension === 'pdf' && await blob.slice(0, 5).text() !== '%PDF-') {
            throw new Error('Invalid PDF file')
          }
          const type = imageTypes[extension] || 'application/pdf'
          setPreview({ url: URL.createObjectURL(new Blob([blob], { type })), image: Boolean(imageTypes[extension]) })
        }
      } else {
        const url = URL.createObjectURL(new Blob([blob], { type: 'application/octet-stream' }))
        const link = document.createElement('a')
        link.href = url
        link.download = ticket.fileName
        document.body.appendChild(link)
        link.click()
        link.remove()
        // Allow the browser to start consuming the blob before releasing it.
        setTimeout(() => URL.revokeObjectURL(url), 1000)
      }
    } catch {
      setError(true)
    } finally {
      setBusy(false)
    }
  }

  if (!submission.storagePath) return null

  return (
    <div className="mt-2 space-y-2">
      <div className="flex flex-wrap gap-2" aria-busy={busy}>
        <button type="button" className="btn" disabled={busy} onClick={() => openFile(false)}>
          {t('submissions.downloadFile')}
        </button>
        {canPreview && <button type="button" className="btn" disabled={busy} onClick={() => openFile(true)}>
          {t('submissions.previewFile')}
        </button>}
        {busy && <span role="status">{t('common.loading')}</span>}
      </div>
      {error && <p role="alert" className="alert alert-danger">{t('submissions.fileAccessError')}</p>}
      {preview && (
        <dialog ref={dialog} onClose={() => setPreview(null)} className="m-auto w-[min(90vw,64rem)] max-h-[90vh] rounded-xl border border-current bg-surface p-6 text-ink backdrop:bg-black/60" aria-label={t('submissions.previewFile')}>
          <div className="mb-4 flex items-center justify-between gap-4">
            <h3 className="min-w-0 break-all font-bold">{submission.fileName}</h3>
            <button type="button" className="btn" onClick={() => dialog.current.close()}>{t('submissions.closePreview')}</button>
          </div>
          {preview.text !== undefined ? (
            <pre className="max-h-[65vh] overflow-auto whitespace-pre-wrap break-words font-mono text-sm">{preview.text}</pre>
          ) : preview.image ? (
            <img src={preview.url} alt={submission.fileName} className="mx-auto max-h-[65vh] max-w-full object-contain" />
          ) : (
            <iframe src={preview.url} title={submission.fileName} referrerPolicy="no-referrer" className="h-[65vh] w-full border-0" />
          )}
          <p className="mt-3 text-sm text-muted">{t('submissions.previewHint')}</p>
        </dialog>
      )}
    </div>
  )
}
