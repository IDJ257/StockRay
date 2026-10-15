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
    <div className="modal-card auth-card" role="dialog" aria-modal="true" aria-labelledby={`${kind}Title`} aria-describedby={`${kind}Subtitle`}>
      <button id={`${kind}Close`} className="modal-close" type="button" aria-label={register ? 'Close registration' : 'Close login'} onClick={close}>&times;</button>
      <div className="auth-brand">StockRay<span>{register ? 'Create an account' : 'Account access'}</span></div>
      <header className="auth-heading">
        <p className="auth-eyebrow">Your private dashboard</p>
        <h2 id={`${kind}Title`} className="auth-title">{register ? 'Make room for your watchlist.' : 'Welcome back.'}</h2>
        <p id={`${kind}Subtitle`} className="auth-subtitle">{register ? 'Create your account to keep the symbols you follow together in one place.' : 'Log in to pick up where you left off and follow your saved symbols.'}</p>
      </header>
      <form id={`${kind}Form`} className="auth-form" ref={form} onSubmit={submit} aria-busy={busy}>
        {register && <label className="auth-field">Username<input name="username" type="text" autoComplete="username" placeholder="Choose a username" required /></label>}
        <label className="auth-field">Email<input name="email" type={register ? 'email' : 'text'} autoComplete="email" placeholder="Your email address" required /></label>
        <label className="auth-field">Password<input name="password" type="password" minLength={register ? 8 : undefined} autoComplete={register ? 'new-password' : 'current-password'} placeholder={register ? 'Create a password' : 'Your password'} aria-describedby={register ? 'registerPasswordHint' : undefined} required /></label>
        {register && <p id="registerPasswordHint" className="auth-field-hint">Use at least 8 characters.</p>}
        <button className="auth-button" type="submit" disabled={busy}>{busy ? (register ? 'Creating account…' : 'Logging in…') : (register ? 'Create account' : 'Log in')}<span aria-hidden="true">&rarr;</span></button>
        <div id={`${kind}Message`} className={`auth-message${isError ? ' error' : message ? ' success' : ''}`} role="status" aria-live="polite">{message}</div>
      </form>
      <footer className="auth-footer">{register ? 'Your watchlist, alongside the public overview.' : 'Your saved symbols. Your own perspective.'}</footer>
    </div>
  </div>;
}
