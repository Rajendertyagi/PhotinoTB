// State
let tabs = [];
let activeTabId = null;

// API (Exposed to global scope)
window.Tabs = {
  create: createTab,
  close: closeTab,
  switch: switchTab,
  updateUrl: updateUrl,
  getActive: () => tabs.find(t => t.id === activeTabId)
};

// Initialize first tab
document.addEventListener('DOMContentLoaded', () => {
  createTab('https://www.bing.com');
});

function createTab(url) {
  const id = Date.now();
  tabs.push({ id, title: 'New Tab', url });
  activeTabId = id;
  renderTabs();
  return activeTabId;
}

function closeTab(id) {
  const index = tabs.findIndex(t => t.id === id);
  if (index === -1) return;

  tabs.splice(index, 1);

  if (tabs.length === 0) {
    window.__TAURI__.window.getCurrent().close();
    return;
  }

  if (activeTabId === id) {
    const newIdx = Math.min(index, tabs.length - 1);
    activeTabId = tabs[newIdx].id;
    switchTab(activeTabId);
  } else {
    renderTabs();
  }
}

function switchTab(id) {
  activeTabId = id;
  const tab = tabs.find(t => t.id === id);
  if (tab) {
    document.getElementById('url').value = tab.url;
    // Send to backend to update browser window
    window.__TAURI__.core.invoke('navigate', { url: tab.url });
    renderTabs();
  }
}

function updateUrl(url) {
  const tab = tabs.find(t => t.id === activeTabId);
  if (tab) {
    tab.url = url;
    tab.title = url.replace(/^https?:\/\//, '').split('/')[0] || 'Loading...';
    renderTabs();
  }
}

function renderTabs() {
  const container = document.getElementById('tabs');
  container.innerHTML = '';

  tabs.forEach(tab => {
    const el = document.createElement('div');
    el.className = `tab ${tab.id === activeTabId ? 'active' : ''}`;
    el.onclick = () => switchTab(tab.id);
    el.innerHTML = `
      <span>${tab.title}</span>
      <span class="tab-close" onclick="event.stopPropagation(); Tabs.close(${tab.id})">✕</span>
    `;
    container.appendChild(el);
  });

  // Add "+" button
  const addBtn = document.createElement('div');
  addBtn.className = 'tab';
  addBtn.style.background = 'transparent';
  addBtn.style.cursor = 'pointer';
  addBtn.innerHTML = '+';
  addBtn.onclick = () => {
    createTab('https://www.bing.com');
  };
  container.appendChild(addBtn);
}
