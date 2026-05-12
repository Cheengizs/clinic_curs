import { BrowserRouter, Routes, Route, Link } from "react-router-dom";
import { useEffect, useState } from "react";
import DoctorsCatalog from "./pages/DoctorsCatalog";
import Login from "./pages/Login";
import Home from "./pages/Home";
import Offices from "./pages/Offices";
import Profile from "./pages/Profile";

function App() {
  // Состояние для отслеживания авторизации
  const [isAuth, setIsAuth] = useState(!!localStorage.getItem("accessToken"));

  // Слушаем изменения в localStorage (чтобы шапка обновлялась после логина)
  useEffect(() => {
    const handleStorageChange = () => {
      setIsAuth(!!localStorage.getItem("accessToken"));
    };
    window.addEventListener("storage", handleStorageChange);
    return () => window.removeEventListener("storage", handleStorageChange);
  }, []);

  return (
    <BrowserRouter>
      <div className="min-h-screen bg-slate-50 flex flex-col font-sans text-slate-900">
        <header className="bg-white/80 backdrop-blur-md sticky top-0 z-50 border-b border-slate-200 shadow-sm">
          <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 h-16 flex justify-between items-center">
            <Link to="/" className="flex items-center gap-2 group">
              <div className="w-8 h-8 bg-blue-600 rounded-lg flex items-center justify-center text-white font-bold text-xl shadow-md">
                C
              </div>
              <span className="text-2xl font-black text-slate-800 tracking-tight">
                Clinic<span className="text-blue-600">.</span>
              </span>
            </Link>

            <nav className="flex items-center gap-8">
              <Link
                to="/doctors"
                className="text-sm font-semibold text-slate-600 hover:text-blue-600 transition-colors"
              >
                Специалисты
              </Link>
              <Link
                to="/offices"
                className="text-sm font-semibold text-slate-600 hover:text-blue-600 transition-colors"
              >
                Офисы
              </Link>

              {/* ДИНАМИЧЕСКАЯ ССЫЛКА */}
              <Link
                to={isAuth ? "/profile" : "/login"}
                className="bg-slate-900 text-white text-sm font-semibold px-5 py-2.5 rounded-full hover:bg-slate-800 transition-all shadow-md"
              >
                {isAuth ? "Мой профиль" : "Личный кабинет"}
              </Link>
            </nav>
          </div>
        </header>

        <main className="flex-grow flex flex-col">
          <Routes>
            <Route path="/" element={<Home />} />
            <Route path="/doctors" element={<DoctorsCatalog />} />
            <Route path="/offices" element={<Offices />} />
            <Route
              path="/login"
              element={<Login onLogin={() => setIsAuth(true)} />}
            />
            <Route
              path="/profile"
              element={<Profile onLogout={() => setIsAuth(false)} />}
            />
          </Routes>
        </main>
      </div>
    </BrowserRouter>
  );
}

export default App;
