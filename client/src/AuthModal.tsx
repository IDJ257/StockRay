import { useEffect, useRef, useState, type FormEvent } from 'react';
import { ApiError, mutate } from './api';
import { store } from './store';

export type AuthKind = 'login' | 'register';

export function AuthModal({ kind, open, close, registered }: {
  kind: AuthKind; open: boolean; close: () => void; registered: () => void;
}) {
  const register = kind === 'register';
  const [visible, setVisible] = useState(false);
  const [hidden, setHidden] = useState(true);
  const [message, setMessage] = useState('');
  const [isError, setError] = useState(false);
  const [busy, setBusy] = useState(false);
  const form = useRef<HTMLFormElement>(null);
  const successTimer = useRef<ReturnType<typeof setTimeout> | undefined>(undefined);
  const submission = useRef(0);

  useEffect(() => {
    ++submission.current;
    if (successTimer.current) clearTimeout(successTimer.current);
    if (open) {
      form.current?.reset();
      setMessage(''); setError(false); setBusy(false); setHidden(false);
      const frame = requestAnimationFrame(() => setVisible(true));
      return () => cancelAnimationFrame(frame);
    }
    setVisible(false);
    const timer = setTimeout(() => setHidden(true), 220);
    return () => clearTimeout(timer);
  }, [open]);

  useEffect(() => {
    const onKey = (event: KeyboardEvent) => { if (event.key === 'Escape' && open) close(); };
    document.addEventListener('keydown', onKey);
    return () => document.removeEventListener('keydown', onKey);
  }, [open, close]);

  useEffect(() => () => {
    ++submission.current;
    if (successTimer.current) clearTimeout(successTimer.current);
  }, []);

  async function submit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    if (busy) return;
    const values = new FormData(event.currentTarget);
    const email = String(values.get('email') || '');
    const password = String(values.get('password') || '');
    const username = String(values.get('username') || '').trim();
    if (!email || !password || (register && !username)) {
      setMessage(register ? 'Please complete all fields.' : 'Please enter your email and password.');
      setError(true); return;
    }
    setBusy(true);
    const current = ++submission.current;
    try {
      if (register) await mutate('/register', 'POST', { UserName: username, Email: email.trim(), Password: password });
      else await store.login(email, password);
      if (current !== submission.current) return;
      setError(false);
      setMessage(register ? 'Registration successful. You can log in now.' : 'Login successful.');
      successTimer.current = setTimeout(() => {
        close();
        if (register) registered();
      }, register ? 800 : 350);
    } catch (error) {
      if (current !== submission.current) return;
      const fallback = register ? 'Registration failed.' : 'Login failed. Check your email and password.';
      setMessage(error instanceof ApiError && error.message !== 'Endpoint issues' ? error.message : fallback);
      setError(true);
    } finally { if (current === submission.current) setBusy(false); }
  }

  return <div id={`${kind}Overlay`} className={`modal-overlay${visible ? ' is-visible' : ''}`} hidden={hidden}
    onClick={event => { if (event.target === event.currentTarget) close(); }}>
    <div className="modal-card auth-card" role="dialog" aria-modal="true" aria-labelledby={`${kind}Title`}>
      <button id={`${kind}Close`} className="modal-close" type="button" aria-label={register ? 'Close registration' : 'Close login'} onClick={close}>×</button>
      <h2 id={`${kind}Title`} className="auth-title">{register ? 'Create your account' : 'Welcome back'}</h2>
      <p className="auth-subtitle">{register ? 'Register to start building a private dashboard.' : 'Log in to view your private dashboard.'}</p>
      <form id={`${kind}Form`} className="auth-form" ref={form} onSubmit={submit}>
        {register && <label className="auth-field">Username<input name="username" type="text" autoComplete="username" placeholder="username" required /></label>}
        <label className="auth-field">Email<input name="email" type={register ? 'email' : 'text'} autoComplete="email" placeholder={register ? 'email' : 'Your email'} required /></label>
        <label className="auth-field">Password<input name="password" type="password" minLength={register ? 8 : undefined} autoComplete={register ? 'new-password' : 'current-password'} placeholder={register ? 'password' : 'Your password'} required /></label>
        <button className="auth-button" type="submit" disabled={busy}>{register ? 'Register' : 'Login'}</button>
        <div id={`${kind}Message`} className={`auth-message${isError ? ' error' : message ? ' success' : ''}`} role="status" aria-live="polite">{message}</div>
      </form>
    </div>
  </div>;
}
