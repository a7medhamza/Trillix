const TOKEN_KEY = 'trillix_admin_token';
const REFRESH_KEY = 'trillix_admin_refresh';

/** App routes resolve relative to <base href="/Trillix/administration/"> on admin pages. */
const LOGIN_PAGE = 'login.html';

const ENDPOINTS = {
  login: '/api/admin/auth/login',
  logout: '/api/admin/auth/logout',
  products: '/api/admin/products',
  product: (id) => `/api/admin/products/${id}`,
  productInventory: (id) => `/api/admin/products/${id}/inventory`,
  productImages: (id) => `/api/admin/products/${id}/images`,
};

function getToken() {
  return sessionStorage.getItem(TOKEN_KEY);
}

function setSessionTokens(accessToken, refreshToken) {
  sessionStorage.setItem(TOKEN_KEY, accessToken);
  if (refreshToken) sessionStorage.setItem(REFRESH_KEY, refreshToken);
}

function clearAuth() {
  sessionStorage.removeItem(TOKEN_KEY);
  sessionStorage.removeItem(REFRESH_KEY);
}

function redirectToLogin() {
  location.href = LOGIN_PAGE;
}

function requireAuth() {
  if (!getToken()) redirectToLogin();
}

async function api(path, options = {}) {
  const headers = new Headers(options.headers || {});
  if (
    options.body &&
    typeof options.body === 'string' &&
    !headers.has('Content-Type')
  ) {
    headers.set('Content-Type', 'application/json');
  }
  const token = getToken();
  if (token) headers.set('Authorization', 'Bearer ' + token);

  const res = await fetch(path, { ...options, headers });

  if (res.status === 401) {
    clearAuth();
    redirectToLogin();
    throw { status: 401, body: 'Unauthorized' };
  }

  const text = await res.text();
  let body = null;
  if (text) {
    try {
      body = JSON.parse(text);
    } catch {
      body = text;
    }
  }

  if (!res.ok) throw { status: res.status, body };
  return body;
}

async function apiForm(path, formData) {
  const headers = new Headers();
  const token = getToken();
  if (token) headers.set('Authorization', 'Bearer ' + token);

  const res = await fetch(path, { method: 'POST', headers, body: formData });

  if (res.status === 401) {
    clearAuth();
    redirectToLogin();
    throw { status: 401, body: 'Unauthorized' };
  }

  const text = await res.text();
  let body = null;
  if (text) {
    try {
      body = JSON.parse(text);
    } catch {
      body = text;
    }
  }

  if (!res.ok) throw { status: res.status, body };
  return body;
}
