// src/components/profile/AccountHeader.tsx
interface Props {
  email: string;
  role: string;
  onLogout: () => void;
}

export default function AccountHeader({ email, role, onLogout }: Props) {
  return (
    <div className="flex justify-between items-end border-b border-slate-100 pb-6">
      <div>
        <h1 className="text-4xl font-black text-slate-900 tracking-tight">
          Личный кабинет
        </h1>
        <p className="text-slate-500 mt-2 text-lg">
          Добро пожаловать,{" "}
          <span className="text-slate-900 font-semibold">{email}</span>
        </p>
      </div>
      <div className="flex flex-col items-end gap-3">
        <div className="px-4 py-1.5 bg-blue-100 text-blue-700 rounded-full text-sm font-bold uppercase tracking-wider">
          Роль: {role}
        </div>
        <button
          onClick={onLogout}
          className="text-red-500 hover:text-red-700 font-semibold text-sm transition-colors flex items-center gap-1"
        >
          <svg
            className="w-4 h-4"
            fill="none"
            stroke="currentColor"
            viewBox="0 0 24 24"
          >
            <path
              strokeLinecap="round"
              strokeLinejoin="round"
              strokeWidth="2"
              d="M17 16l4-4m0 0l-4-4m4 4H7m6 4v1a3 3 0 01-3 3H6a3 3 0 01-3-3V7a3 3 0 013-3h4a3 3 0 013 3v1"
            ></path>
          </svg>
          Выйти из системы
        </button>
      </div>
    </div>
  );
}
