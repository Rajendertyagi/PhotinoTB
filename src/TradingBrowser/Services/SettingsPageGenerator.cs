using System.Net;

namespace TradingBrowser.Services;

/// <summary>
/// Generates the complete Settings page HTML/CSS/JS.
/// Matches your confirmed preferences: Dark-only, HTTPS warning, FolderPicker bridge, Restart banner.
/// </summary>
public static class SettingsPageGenerator
{
    /// <summary>
    /// Generates the full Settings page with all categories and controls.
    /// </summary>
    public static string GenerateHtml(
        string searchEngine, bool showSuggestions, bool showFullUrls,
        bool httpsWarning, bool clearDataPending,
        string downloadPath, bool askBeforeSave,
        string startupMode, string startupPages)
    {
        // Pre-calculate HTML attributes for interpolation
        string googleSelected = searchEngine == "Google" ? "selected" : "";
        string bingSelected = searchEngine == "Bing" ? "selected" : "";
        string ddgSelected = searchEngine == "DuckDuckGo" ? "selected" : "";
        
        string suggestionsChecked = showSuggestions ? "checked" : "";
        string fullUrlsChecked = showFullUrls ? "checked" : "";
        string httpsChecked = httpsWarning ? "checked" : "";
        string askSaveChecked = askBeforeSave ? "checked" : "";
        string restoreChecked = startupMode == "Restore" ? "checked" : "";

        // Using C# 11 Raw String Literals ($$""") 
        // Single { } are treated as raw text (perfect for CSS/JS).
        // Double {{ }} are used for C# variable interpolation.
        return $$"""
        <!DOCTYPE html>
        <html>
        <head>
            <meta charset='utf-8'>
            <title>Settings</title>
            <style>
                :root {
                    --bg: #202124; --card: #292a2d; --text: #e8eaed; --sub: #9aa0a6;
                    --border: #3c4043; --accent: #8ab4f8; --hover: #303134; --danger: #f28b82;
                }
                body { background: var(--bg); color: var(--text); font-family: 'Segoe UI', sans-serif; margin: 0; display: flex; height: 100vh; overflow: hidden; }
                .sidebar { width: 240px; padding: 20px 10px; display: flex; flex-direction: column; gap: 4px; border-right: 1px solid var(--border); }
                .nav-item { padding: 10px 15px; cursor: pointer; border-radius: 4px; color: var(--sub); font-size: 14px; display: flex; align-items: center; gap: 12px; transition: background 0.15s; }
                .nav-item:hover { background: var(--hover); color: var(--text); }
                .nav-item.active { background: var(--hover); color: var(--accent); font-weight: 500; }
                .main { flex: 1; padding: 40px; overflow-y: auto; }
                .section { display: none; max-width: 800px; }
                .section.active { display: block; }
                .card { background: var(--card); border-radius: 8px; padding: 24px; margin-bottom: 16px; }
                .row { display: flex; justify-content: space-between; align-items: center; padding: 16px 0; border-bottom: 1px solid var(--border); }
                .row:last-child { border-bottom: none; }
                .label h3 { margin: 0 0 6px 0; font-size: 14px; font-weight: 500; }
                .label p { margin: 0; font-size: 12px; color: var(--sub); }
                .toggle { position: relative; width: 40px; height: 20px; }
                .toggle input { opacity: 0; width: 0; height: 0; }
                .slider { position: absolute; inset: 0; background: #5f6368; border-radius: 20px; cursor: pointer; transition: 0.2s; }
                .slider:before { content: ''; position: absolute; width: 14px; height: 14px; left: 3px; bottom: 3px; background: white; border-radius: 50%; transition: 0.2s; }
                input:checked + .slider { background: var(--accent); }
                input:checked + .slider:before { transform: translateX(20px); }
                select, input { background: var(--hover); border: 1px solid var(--border); color: var(--text); padding: 8px 12px; border-radius: 6px; font-size: 13px; }
                select:focus, input:focus { outline: none; border-color: var(--accent); }
                .btn { background: var(--hover); border: 1px solid var(--border); color: var(--text); padding: 8px 16px; border-radius: 6px; cursor: pointer; font-size: 13px; }
                .btn:hover { background: var(--border); }
                .btn-primary { background: var(--accent); color: #000; border: none; }
                .btn-danger { background: var(--danger); color: #000; border: none; }
                .path-display { font-family: monospace; font-size: 12px; color: var(--sub); margin-top: 6px; word-break: break-all; }
                /* Restart Banner */
                .banner { position: fixed; bottom: 20px; left: 50%; transform: translateX(-50%) translateY(100px); background: var(--card); border: 1px solid var(--accent); border-radius: 8px; padding: 12px 20px; display: flex; align-items: center; gap: 16px; box-shadow: 0 4px 12px rgba(0,0,0,0.5); transition: transform 0.3s ease; z-index: 100; }
                .banner.show { transform: translateX(-50%) translateY(0); }
                .banner p { margin: 0; font-size: 14px; }
                .banner .actions { display: flex; gap: 8px; }
            </style>
        </head>
        <body>
            <div class='sidebar'>
                <div class='nav-item active' onclick='showSection("search")'>🔍 Search</div>
                <div class='nav-item' onclick='showSection("privacy")'>🔒 Privacy</div>
                <div class='nav-item' onclick='showSection("downloads")'>⬇️ Downloads</div>
                <div class='nav-item' onclick='showSection("startup")'>🚀 Startup</div>
                <div class='nav-item' onclick='showSection("appearance")'>🎨 Appearance</div>
            </div>
            
            <div class='main'>
                <!-- Search Section -->
                <div id='search' class='section active'>
                    <h2>Search engine</h2>
                    <div class='card'>
                        <div class='row'>
                            <div class='label'>
                                <h3>Search engine</h3>
                                <p>Used in the address bar and new tabs.</p>
                            </div>
                            <select id='engine' onchange='save("SearchEngine", this.value)'>
                                <option value='Google' {{googleSelected}}>Google</option>
                                <option value='Bing' {{bingSelected}}>Bing</option>
                                <option value='DuckDuckGo' {{ddgSelected}}>DuckDuckGo</option>
                            </select>
                        </div>
                        <div class='row'>
                            <div class='label'>
                                <h3>Show search suggestions</h3>
                                <p>Get predictions as you type.</p>
                            </div>
                            <label class='toggle'><input type='checkbox' id='suggestions' {{suggestionsChecked}} onchange='save("ShowSuggestions", this.checked)'><span class='slider'></span></label>
                        </div>
                        <div class='row'>
                            <div class='label'>
                                <h3>Show full URLs</h3>
                                <p>Display complete address bar text.</p>
                            </div>
                            <label class='toggle'><input type='checkbox' id='fullUrls' {{fullUrlsChecked}} onchange='save("ShowFullUrls", this.checked)'><span class='slider'></span></label>
                        </div>
                    </div>
                </div>

                <!-- Privacy Section -->
                <div id='privacy' class='section'>
                    <h2>Privacy and security</h2>
                    <div class='card'>
                        <div class='row'>
                            <div class='label'>
                                <h3>HTTPS-Only Mode</h3>
                                <p>Warn before loading insecure HTTP pages.</p>
                            </div>
                            <label class='toggle'><input type='checkbox' id='httpsWarn' {{httpsChecked}} onchange='save("HttpsWarning", this.checked)'><span class='slider'></span></label>
                        </div>
                        <div class='row'>
                            <div class='label'>
                                <h3>Clear browsing data</h3>
                                <p>Delete cache, cookies, and history.</p>
                            </div>
                            <button class='btn btn-danger' onclick='clearData()'>Clear data</button>
                        </div>
                    </div>
                </div>

                <!-- Downloads Section -->
                <div id='downloads' class='section'>
                    <h2>Downloads</h2>
                    <div class='card'>
                        <div class='row'>
                            <div class='label'>
                                <h3>Download location</h3>
                                <p>Files will be saved here.</p>
                                <div class='path-display' id='dlPathDisplay'>{{downloadPath}}</div>
                            </div>
                            <button class='btn' onclick='changeDownloadPath()'>Change</button>
                        </div>
                        <div class='row'>
                            <div class='label'>
                                <h3>Ask where to save each file</h3>
                                <p>Always prompt for save location.</p>
                            </div>
                            <label class='toggle'><input type='checkbox' id='askSave' {{askSaveChecked}} onchange='save("AskBeforeSave", this.checked)'><span class='slider'></span></label>
                        </div>
                    </div>
                </div>

                <!-- Startup Section -->
                <div id='startup' class='section'>
                    <h2>On startup</h2>
                    <div class='card'>
                        <div class='row'>
                            <div class='label'>
                                <h3>Continue where you left off</h3>
                                <p>Restore previous session automatically.</p>
                            </div>
                            <label class='toggle'><input type='checkbox' id='restoreSession' {{restoreChecked}} onchange='save("RestoreSession", this.checked)'><span class='slider'></span></label>
                        </div>
                    </div>
                </div>

                <!-- Appearance Section -->
                <div id='appearance' class='section'>
                    <h2>Appearance</h2>
                    <div class='card'>
                        <div class='row'>
                            <div class='label'>
                                <h3>Theme</h3>
                                <p>Currently locked to Dark mode for performance consistency.</p>
                            </div>
                            <span style='color: var(--sub); font-size: 13px;'>🌙 Dark (Fixed)</span>
                        </div>
                    </div>
                </div>
            </div>

            <!-- Restart Banner -->
            <div class='banner' id='restartBanner'>
                <p>Some settings require a restart.</p>
                <div class='actions'>
                    <button class='btn' onclick='dismissBanner()'>Dismiss</button>
                    <button class='btn btn-primary' onclick='restartApp()'>Restart now</button>
                </div>
            </div>

            <script>
                function showSection(id) {
                    document.querySelectorAll('.section').forEach(el => el.style.display = 'none');
                    document.getElementById(id).style.display = 'block';
                    document.querySelectorAll('.nav-item').forEach(el => el.classList.remove('active'));
                    if (event && event.target) event.target.classList.add('active');
                }

                function save(key, value) {
                    window.chrome.webview.postMessage('SETTING_UPDATE:' + key + ':' + value);
                    // Show restart banner for specific settings
                    if (['HttpsWarning', 'AskBeforeSave', 'RestoreSession'].includes(key)) {
                        document.getElementById('restartBanner').classList.add('show');
                    }
                }

                function clearData() {
                    if (confirm('Clear all browsing data? This cannot be undone.')) {
                        window.chrome.webview.postMessage('CLEAR_ALL_DATA');
                    }
                }

                function changeDownloadPath() {
                    window.chrome.webview.postMessage('CHANGE_DOWNLOAD_PATH');
                }

                function restartApp() {
                    window.chrome.webview.postMessage('RESTART_APP');
                }

                function dismissBanner() {
                    document.getElementById('restartBanner').classList.remove('show');
                }
            </script>
        </body>
        </html>
        """;
    }
}
