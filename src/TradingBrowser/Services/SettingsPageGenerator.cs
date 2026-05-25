using System.Net;

namespace TradingBrowser.Services;

/// <summary>
/// Generates the HTML content for the in-browser Settings page.
/// Matches the requested design: Sidebar navigation + Main content area.
/// </summary>
public static class SettingsPageGenerator
{
    /// <summary>
    /// Generates the complete HTML for the Settings page.
    /// </summary>
    public static string GenerateHtml()
    {
        return @"
        <!DOCTYPE html>
        <html>
        <head>
            <meta charset='utf-8'>
            <title>Settings</title>
            <style>
                :root {
                    --bg-color: #202124;
                    --sidebar-bg: #202124;
                    --content-bg: #202124;
                    --text-color: #e8eaed;
                    --text-secondary: #9aa0a6;
                    --hover-bg: #303134;
                    --active-bg: #303134;
                    --active-text: #8ab4f8;
                    --border-color: #3c4043;
                    --card-bg: #292a2d;
                }
                body {
                    background-color: var(--bg-color);
                    color: var(--text-color);
                    font-family: 'Segoe UI', sans-serif;
                    margin: 0;
                    display: flex;
                    height: 100vh;
                    overflow: hidden;
                }
                /* Sidebar Styles */
                .sidebar {
                    width: 240px;
                    background-color: var(--sidebar-bg);
                    padding: 20px 10px;
                    display: flex;
                    flex-direction: column;
                    gap: 5px;
                    border-right: 1px solid var(--border-color);
                }
                .sidebar-title {
                    font-size: 24px;
                    font-weight: 500;
                    padding: 0 15px 20px 15px;
                    color: var(--text-color);
                }
                .nav-item {
                    padding: 10px 15px;
                    cursor: pointer;
                    border-radius: 4px;
                    color: var(--text-secondary);
                    font-size: 14px;
                    display: flex;
                    align-items: center;
                    gap: 12px;
                    transition: background 0.2s;
                }
                .nav-item:hover {
                    background-color: var(--hover-bg);
                    color: var(--text-color);
                }
                .nav-item.active {
                    background-color: var(--active-bg);
                    color: var(--active-text);
                    font-weight: 500;
                }
                /* Main Content Styles */
                .main-content {
                    flex-grow: 1;
                    padding: 40px;
                    overflow-y: auto;
                    background-color: var(--content-bg);
                }
                .section-header {
                    font-size: 22px;
                    font-weight: 400;
                    margin-bottom: 20px;
                }
                .setting-card {
                    background-color: var(--card-bg);
                    border-radius: 8px;
                    padding: 20px;
                    max-width: 800px;
                    margin-bottom: 20px;
                }
                .setting-row {
                    display: flex;
                    justify-content: space-between;
                    align-items: center;
                    padding: 15px 0;
                    border-bottom: 1px solid var(--border-color);
                }
                .setting-row:last-child {
                    border-bottom: none;
                }
                .setting-info h3 {
                    margin: 0 0 5px 0;
                    font-size: 14px;
                    font-weight: 500;
                }
                .setting-info p {
                    margin: 0;
                    font-size: 12px;
                    color: var(--text-secondary);
                }
                /* Toggle Switch */
                .switch {
                    position: relative;
                    display: inline-block;
                    width: 36px;
                    height: 20px;
                }
                .switch input { opacity: 0; width: 0; height: 0; }
                .slider {
                    position: absolute;
                    cursor: pointer;
                    top: 0; left: 0; right: 0; bottom: 0;
                    background-color: #5f6368;
                    transition: .4s;
                    border-radius: 20px;
                }
                .slider:before {
                    position: absolute;
                    content: '';
                    height: 14px;
                    width: 14px;
                    left: 3px;
                    bottom: 3px;
                    background-color: white;
                    transition: .4s;
                    border-radius: 50%;
                }
                input:checked + .slider { background-color: #8ab4f8; }
                input:checked + .slider:before { transform: translateX(16px); }
                
                /* Dropdown */
                select {
                    background: #303134;
                    color: #e8eaed;
                    border: 1px solid #5f6368;
                    padding: 5px 10px;
                    border-radius: 4px;
                }
            </style>
        </head>
        <body>
            <div class='sidebar'>
                <div class='sidebar-title'>Settings</div>
                <div class='nav-item active' onclick='showSection(""search"")'>🔍 Search engine</div>
                <div class='nav-item' onclick='showSection(""appearance"")'> Appearance</div>
                <div class='nav-item' onclick='showSection(""privacy"")'>🔒 Privacy</div>
                <div class='nav-item' onclick='showSection(""performance"")'>⚡ Performance</div>
            </div>
            
            <div class='main-content'>
                <!-- Search Engine Section -->
                <div id='search' class='settings-section'>
                    <div class='section-header'>Search engine</div>
                    <div class='setting-card'>
                        <div class='setting-row'>
                            <div class='setting-info'>
                                <h3>Search engine</h3>
                                <p>The search engine used for address bar searches.</p>
                            </div>
                            <select id='engineSelect' onchange='saveSetting(""SearchEngine"", this.value)'>
                                <option value='Google'>Google</option>
                                <option value='Bing'>Bing</option>
                                <option value='DuckDuckGo'>DuckDuckGo</option>
                            </select>
                        </div>
                        <div class='setting-row'>
                            <div class='setting-info'>
                                <h3>Suggestions from the search engine</h3>
                                <p>Get suggestions as you type.</p>
                            </div>
                            <label class='switch'>
                                <input type='checkbox' checked id='suggestToggle' onchange='saveSetting(""Suggestions"", this.checked)'>
                                <span class='slider'></span>
                            </label>
                        </div>
                    </div>
                </div>

                <!-- Appearance Section (Placeholder) -->
                <div id='appearance' class='settings-section' style='display:none;'>
                    <div class='section-header'>Appearance</div>
                    <div class='setting-card'>
                        <div class='setting-row'>
                            <div class='setting-info'>
                                <h3>Theme</h3>
                                <p>Currently set to Dark mode.</p>
                            </div>
                        </div>
                    </div>
                </div>

                <!-- Privacy Section (Placeholder) -->
                <div id='privacy' class='settings-section' style='display:none;'>
                    <div class='section-header'>Privacy and security</div>
                    <div class='setting-card'>
                        <p>Cookies and site data management will go here.</p>
                    </div>
                </div>

                <!-- Performance Section (Placeholder) -->
                <div id='performance' class='settings-section' style='display:none;'>
                    <div class='section-header'>Performance</div>
                    <div class='setting-card'>
                        <p>Memory saver and hardware acceleration toggles.</p>
                    </div>
                </div>
            </div>

            <script>
                // Navigation Logic
                function showSection(id) {
                    // Hide all sections
                    document.querySelectorAll('.settings-section').forEach(el => el.style.display = 'none');
                    // Show selected
                    document.getElementById(id).style.display = 'block';
                    
                    // Update sidebar active state
                    document.querySelectorAll('.nav-item').forEach(el => el.classList.remove('active'));
                    event.target.classList.add('active');
                }

                // Save Logic
                function saveSetting(key, value) {
                    window.chrome.webview.postMessage('SETTING_UPDATE:' + key + ':' + value);
                }
            </script>
        </body>
        </html>";
    }
}
