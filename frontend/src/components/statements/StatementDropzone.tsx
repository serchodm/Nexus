import React, { useState, useRef } from 'react';
import { UploadCloud, FileSpreadsheet, AlertCircle } from 'lucide-react';

interface StatementDropzoneProps {
  onFileSelect: (file: File) => void;
  isLoading?: boolean;
  selectedAccountName?: string;
}

export const StatementDropzone: React.FC<StatementDropzoneProps> = ({
  onFileSelect,
  isLoading = false,
  selectedAccountName,
}) => {
  const [isDragOver, setIsDragOver] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const fileInputRef = useRef<HTMLInputElement>(null);

  const handleDragOver = (e: React.DragEvent) => {
    e.preventDefault();
    setIsDragOver(true);
  };

  const handleDragLeave = (e: React.DragEvent) => {
    e.preventDefault();
    setIsDragOver(false);
  };

  const validateAndProcessFile = (file: File) => {
    setError(null);
    const validExtensions = ['.xlsx', '.xls', '.csv', '.tsv'];
    const fileName = file.name.toLowerCase();
    const hasValidExt = validExtensions.some((ext) => fileName.endsWith(ext));

    if (!hasValidExt) {
      setError('Formato de archivo no soportado. Por favor, sube un fichero Excel (.xlsx/.xls) o delimitado (.csv/.tsv).');
      return;
    }

    onFileSelect(file);
  };

  const handleDrop = (e: React.DragEvent) => {
    e.preventDefault();
    setIsDragOver(false);

    if (e.dataTransfer.files && e.dataTransfer.files.length > 0) {
      validateAndProcessFile(e.dataTransfer.files[0]);
    }
  };

  const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    if (e.target.files && e.target.files.length > 0) {
      validateAndProcessFile(e.target.files[0]);
    }
  };

  return (
    <div style={{ marginBottom: '2rem' }}>
      <div
        onDragOver={handleDragOver}
        onDragLeave={handleDragLeave}
        onDrop={handleDrop}
        onClick={() => !isLoading && fileInputRef.current?.click()}
        style={{
          border: isDragOver
            ? '2px dashed var(--accent-cyan)'
            : '2px dashed var(--border-subtle)',
          borderRadius: 'var(--radius-xl)',
          padding: '2.5rem 1.5rem',
          textAlign: 'center',
          background: isDragOver
            ? 'hsla(188, 94%, 53%, 0.05)'
            : 'var(--bg-surface)',
          cursor: isLoading ? 'wait' : 'pointer',
          transition: 'all var(--transition-normal)',
          boxShadow: isDragOver ? 'var(--shadow-glow)' : 'var(--shadow-sm)',
        }}
      >
        <input
          type="file"
          ref={fileInputRef}
          onChange={handleFileChange}
          accept=".xlsx,.xls,.csv,.tsv"
          style={{ display: 'none' }}
        />

        <div
          style={{
            display: 'inline-flex',
            alignItems: 'center',
            justifyContent: 'center',
            width: '56px',
            height: '56px',
            borderRadius: '50%',
            background: isDragOver ? 'hsla(188, 94%, 53%, 0.15)' : 'hsla(217, 91%, 60%, 0.1)',
            color: isDragOver ? 'var(--accent-cyan)' : 'var(--primary-500)',
            marginBottom: '1rem',
          }}
        >
          {isLoading ? (
            <FileSpreadsheet size={28} className="animate-pulse" />
          ) : (
            <UploadCloud size={28} />
          )}
        </div>

        <h3 style={{ fontSize: '1.1rem', fontWeight: 600, color: 'var(--text-main)', margin: '0 0 0.5rem' }}>
          {isLoading
            ? 'Analizando extracto bancario en memoria...'
            : selectedAccountName
            ? `Arrastra o selecciona el extracto para "${selectedAccountName}"`
            : 'Arrastra y suelta tu extracto bancario aquí'}
        </h3>

        <p style={{ fontSize: '0.85rem', color: 'var(--text-muted)', margin: '0 auto 1rem', maxWidth: '460px' }}>
          Compatible con exportaciones oficiales de <strong>BBVA</strong> e <strong>ING Direct</strong> (.xlsx, .csv, .tsv). 
          Los números de tarjeta se enmascaran automáticamente y no se inserta nada sin tu confirmación.
        </p>

        <span
          style={{
            display: 'inline-block',
            fontSize: '0.75rem',
            fontWeight: 600,
            padding: '0.35rem 0.85rem',
            borderRadius: 'var(--radius-full)',
            background: 'var(--bg-surface-elevated)',
            color: 'var(--text-dim)',
            border: '1px solid var(--border-glass)',
          }}
        >
          {isLoading ? 'Calculando saldos y comprobando salvaguardas...' : 'Explorar Archivo en tu Equipo'}
        </span>
      </div>

      {error && (
        <div
          style={{
            marginTop: '0.75rem',
            padding: '0.75rem 1rem',
            borderRadius: 'var(--radius-md)',
            background: 'var(--danger-bg)',
            border: '1px solid var(--danger)',
            color: 'var(--danger)',
            fontSize: '0.85rem',
            display: 'flex',
            alignItems: 'center',
            gap: '0.5rem',
          }}
        >
          <AlertCircle size={16} />
          <span>{error}</span>
        </div>
      )}
    </div>
  );
};
