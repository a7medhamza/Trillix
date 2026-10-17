function slugify(name) {
  return String(name)
    .toLowerCase()
    .replace(/[^a-z0-9]+/g, '-')
    .replace(/^-+|-+$/g, '');
}

function showToast(message, type) {
  const host = document.getElementById('toast-host');
  if (!host) return;
  const alertClass = type === 'error' ? 'alert-error' : 'alert-success';
  const el = document.createElement('div');
  el.className = 'toast toast-end toast-top z-50';
  el.innerHTML =
    '<div role="alert" class="alert ' + alertClass + ' shadow-lg">' +
    '<span>' + escapeHtml(message) + '</span></div>';
  host.appendChild(el);
  setTimeout(() => el.remove(), 4000);
}

function escapeHtml(s) {
  return String(s)
    .replace(/&/g, '&amp;')
    .replace(/</g, '&lt;')
    .replace(/>/g, '&gt;')
    .replace(/"/g, '&quot;');
}

function showConfirm({ title, message, confirmLabel, danger }) {
  return new Promise((resolve) => {
    const dialog = document.getElementById('confirm-modal');
    if (!dialog) {
      resolve(window.confirm(message || title));
      return;
    }
    document.getElementById('confirm-title').textContent = title || 'Confirm';
    document.getElementById('confirm-message').textContent = message || '';
    const ok = document.getElementById('confirm-ok');
    ok.textContent = confirmLabel || 'Confirm';
    ok.className = 'btn ' + (danger ? 'btn-error' : 'btn-primary');

    const onOk = () => {
      cleanup();
      resolve(true);
    };
    const onCancel = () => {
      cleanup();
      resolve(false);
    };
    const cleanup = () => {
      ok.removeEventListener('click', onOk);
      document.getElementById('confirm-cancel').removeEventListener('click', onCancel);
      dialog.close();
    };

    ok.addEventListener('click', onOk);
    document.getElementById('confirm-cancel').addEventListener('click', onCancel);
    dialog.showModal();
  });
}

function formatCurrency(amount) {
  return new Intl.NumberFormat(undefined, {
    style: 'currency',
    currency: 'USD',
  }).format(amount);
}

function fieldError(inputId, message) {
  const el = document.getElementById(inputId);
  if (!el) return;
  const hint =
    document.getElementById('err-' + inputId) ||
    el.closest('fieldset')?.querySelector('.validator-hint');
  if (hint) {
    hint.textContent = message || '';
    hint.hidden = !message;
  }
  if (message) el.classList.add('input-error');
  else el.classList.remove('input-error');
}
