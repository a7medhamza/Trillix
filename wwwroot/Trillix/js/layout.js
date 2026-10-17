const ICONS = {
  products:
    '<svg aria-hidden="true" class="size-5 shrink-0" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8"><path stroke-linecap="round" stroke-linejoin="round" d="M20 7l-8-4-8 4m16 0l-8 4m8-4v10l-8 4m0-10L4 7m8 4v10M4 7v10l8 4"/></svg>',
  add:
    '<svg aria-hidden="true" class="size-5 shrink-0" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8"><path stroke-linecap="round" stroke-linejoin="round" d="M12 5v14m7-7H5"/></svg>',
  orders:
    '<svg aria-hidden="true" class="size-5 shrink-0" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8"><path stroke-linecap="round" stroke-linejoin="round" d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2"/></svg>',
  customers:
    '<svg aria-hidden="true" class="size-5 shrink-0" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8"><path stroke-linecap="round" stroke-linejoin="round" d="M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0z"/></svg>',
};

function renderSidebar() {
  const aside = document.getElementById('sidebar');
  if (!aside) return;

  const path = location.pathname;
  const items = [
    { href: 'products.html', label: 'Products', icon: ICONS.products, match: 'products.html' },
    { href: 'add-product.html', label: 'Add product', icon: ICONS.add, match: 'add-product.html' },
    { href: '#', label: 'Orders', icon: ICONS.orders, disabled: true },
    { href: '#', label: 'Customers', icon: ICONS.customers, disabled: true },
  ];

  aside.className =
    'bg-neutral text-neutral-content flex flex-col min-h-screen py-6 px-3';

  const links = items
    .map((item) => {
      const active =
        !item.disabled && path.includes(item.match || item.href);
      const cls = active
        ? 'menu-active bg-neutral-content/10'
        : item.disabled
          ? 'opacity-50 pointer-events-none'
          : '';
      return (
        '<li><a href="' +
        item.href +
        '" class="flex items-center gap-3 rounded-field px-3 py-2 ' +
        cls +
        '">' +
        item.icon +
        '<span>' +
        item.label +
        '</span></a></li>'
      );
    })
    .join('');

  aside.innerHTML =
    '<div class="px-3 mb-8 flex items-center gap-1.5">' +
    '<span class="text-xl font-extrabold tracking-tight">Trillix</span>' +
    '<span class="size-2 rounded-full bg-accent" aria-hidden="true"></span>' +
    '</div>' +
    '<ul class="menu menu-vertical flex-1 gap-1 p-0">' +
    links +
    '</ul>' +
    '<button type="button" id="logout-btn" class="btn btn-outline border-neutral-content/30 text-neutral-content mt-4 w-full">Log out</button>';

  document.getElementById('logout-btn').addEventListener('click', async () => {
    const refresh = sessionStorage.getItem(REFRESH_KEY);
    clearAuth();
    if (refresh) {
      try {
        await fetch(ENDPOINTS.logout, {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify({ refreshToken: refresh }),
        });
      } catch {
        /* ignore */
      }
    }
    redirectToLogin();
  });
}

document.addEventListener('DOMContentLoaded', renderSidebar);
