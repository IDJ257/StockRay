import { useCallback, useEffect, useLayoutEffect, useState, useSyncExternalStore } from 'react';
import { Link, useLocation, useNavigate } from 'react-router-dom';
import { AuthModal, type AuthKind } from './AuthModal';
import { PrivateDashboard, PublicDashboard } from './Dashboards';
import { store } from './store';

export function App() {
  const location = useLocation();
  const navigate = useNavigate();
  const privatePage = /^\/PrivateDash(?:\/|$)/i.test(location.pathname);
  const { authenticated } = useSyncExternalStore(store.subscribe, store.getSnapshot);
  const [modal, setModal] = useState<AuthKind | null>(null);
  const close = useCallback(() => setModal(null), []);
  const registered = useCallback(() => setModal('login'), []);

  useLayoutEffect(() => {
    document.body.classList.toggle('public-page', !privatePage);
    document.body.classList.toggle('private-page', privatePage);
    document.title = privatePage ? 'Private' : 'Public';
    document.querySelectorAll<HTMLLinkElement>('link[data-style]').forEach(link => {
      link.media = (link.dataset.style === 'private') === privatePage ? 'all' : 'not all';
    });
    setModal(null);
  }, [privatePage]);

  useLayoutEffect(() => {
    document.body.classList.toggle('logged-in', authenticated === true);
    document.body.classList.toggle('modal-open', modal !== null);
  }, [authenticated, modal]);

  useEffect(() => {
    const legacy = /^\/(Login|Register)(?:\/|$)/i.exec(location.pathname);
    if (legacy) {
      setModal(legacy[1].toLowerCase() as AuthKind);
      navigate('/PublicDash/', { replace: true });
    }
  }, [location.pathname, navigate]);

  async function logout() {
    try { await store.logout(); }
    catch (error) { console.error(error); }
    finally { navigate('/PublicDash/'); }
  }

  return <>
    <div className="menu-shell">
      <div className="menu-edge" aria-hidden="true" />
      <nav className="side-menu" aria-label="Main menu">
        <Link className="menu-link" to={privatePage ? '/PublicDash/' : '/PrivateDash/'}>{privatePage ? 'Public  Dashboard' : 'Private Dashboard'}</Link>
        {!privatePage && <>
          <a className="menu-link" data-register-open onClick={() => setModal('register')}>Register</a>
          <a className="menu-link" data-login-open onClick={() => setModal('login')}>Login</a>
        </>}
        <a className="menu-link" data-logout onClick={() => void logout()}>Log out</a>
      </nav>
    </div>
    {privatePage ? <PrivateDashboard /> : <PublicDashboard />}
    {!privatePage && <>
      <AuthModal kind="register" open={modal === 'register'} close={close} registered={registered} />
      <AuthModal kind="login" open={modal === 'login'} close={close} registered={registered} />
    </>}
  </>;
}
