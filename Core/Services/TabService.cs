using System;
using System.Collections.Generic;
using System.Linq;
using TB_Browser.Core.Logging;
using TB_Browser.Core.Models;

namespace TB_Browser.Core.Services
{
    public class TabService : ITabService
    {
        private readonly List<TabModel> _tabs = new();
        private int _nextId = 1;
        private TabModel? _active;

        public IReadOnlyList<TabModel> Tabs => _tabs;
        public TabModel? ActiveTab => _active;
        public event EventHandler<TabModel>? ActiveTabChanged;
        public event EventHandler<TabModel>? TabAdded;
        public event EventHandler<TabModel>? TabRemoved;

        public TabModel CreateTab()
        {
            var t = new TabModel { Id = _nextId++ };
            _tabs.Add(t);
            ActivateTab(t.Id);
            TabAdded?.Invoke(this, t);
            Logger.Info("Tabs", $"Created #{t.Id}");
            return t;
        }

        public void CloseTab(int id)
        {
            var t = _tabs.FirstOrDefault(x => x.Id == id);
            if (t == null) return;
            _tabs.Remove(t);
            TabRemoved?.Invoke(this, t);
            Logger.Info("Tabs", $"Closed #{id}");
            if (_tabs.Count == 0) CreateTab();
            else if (_active?.Id == id) ActivateTab(_tabs.Last().Id);
        }

        public void ActivateTab(int id)
        {
            _active = _tabs.FirstOrDefault(x => x.Id == id);
            ActiveTabChanged?.Invoke(this, _active!);
        }

        public void UpdateTab(int id, string url, string title)
        {
            var t = _tabs.FirstOrDefault(x => x.Id == id);
            if (t != null) { t.Url = url; t.Title = title; }
        }
    }
}
