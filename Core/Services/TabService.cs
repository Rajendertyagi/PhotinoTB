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
            Logger.Info("Tabs", $"Creating new tab (current count: {_tabs.Count})");
            var t = new TabModel { Id = _nextId++ };
            _tabs.Add(t);
            Logger.Debug("Tabs", $"Tab #{t.Id} added to list");
            ActivateTab(t.Id);
            TabAdded?.Invoke(this, t);
            Logger.Info("Tabs", $"Tab #{t.Id} created successfully");
            return t;
        }

        public void CloseTab(int id)
        {
            Logger.Info("Tabs", $"Closing tab #{id}");
            var t = _tabs.FirstOrDefault(x => x.Id == id);
            if (t == null)
            {
                Logger.Warning("Tabs", $"Tab #{id} not found");
                return;
            }
            
            _tabs.Remove(t);
            TabRemoved?.Invoke(this, t);
            Logger.Debug("Tabs", $"Tab #{id} removed from list (remaining: {_tabs.Count})");
            
            if (_tabs.Count == 0)
            {
                Logger.Info("Tabs", "No tabs remaining, creating new tab");
                CreateTab();
            }
            else if (_active?.Id == id)
            {
                Logger.Debug("Tabs", "Closed active tab, activating last tab");
                ActivateTab(_tabs.Last().Id);
            }
            
            Logger.Info("Tabs", $"Tab #{id} closed successfully");
        }

        public void ActivateTab(int id)
        {
            Logger.Debug("Tabs", $"Activating tab #{id}");
            _active = _tabs.FirstOrDefault(x => x.Id == id);
            if (_active == null)
            {
                Logger.Warning("Tabs", $"Tab #{id} not found for activation");
                return;
            }
            ActiveTabChanged?.Invoke(this, _active);
            Logger.Info("Tabs", $"Tab #{id} activated (Title: {_active.Title})");
        }

        public void UpdateTab(int id, string url, string title)
        {
            var t = _tabs.FirstOrDefault(x => x.Id == id);
            if (t != null)
            {
                t.Url = url;
                t.Title = title;
                Logger.Debug("Tabs", $"Tab #{id} updated: {url}");
            }
        }
    }
}
