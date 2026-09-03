interface Props {
  type: 'info' | 'warning' | 'error';
  message: string;
  onDismiss?: () => void;
}

const styles = {
  info: 'bg-blue-50 border-blue-200 text-blue-800',
  warning: 'bg-yellow-50 border-yellow-200 text-yellow-800',
  error: 'bg-red-50 border-red-200 text-red-800',
};

export function AlertBanner({ type, message, onDismiss }: Props) {
  return (
    <div className={`rounded-lg border p-3 text-sm ${styles[type]}`}>
      <div className="flex items-center justify-between">
        <span>{message}</span>
        {onDismiss && (
          <button onClick={onDismiss} className="ml-4 font-bold opacity-50 hover:opacity-100">
            &times;
          </button>
        )}
      </div>
    </div>
  );
}
