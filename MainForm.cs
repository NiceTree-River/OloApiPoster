using System.IO.Compression;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Xml;

namespace OloApiDesktopTool;

public sealed class MainForm : Form
{
    // Keep the default OLO host here so the quick tools only need credentials and an ID.
    private const string DefaultDomain = "https://ordering.api.olo.com";

    private readonly TextBox domainTextBox = new();
    private readonly ComboBox methodComboBox = new();
    private readonly TextBox pathTextBox = new();
    private readonly TextBox bodyTextBox = new();
    private readonly TextBox keyTextBox = new();
    private readonly TextBox secretTextBox = new();
    private readonly TextBox requestResultTextBox = CreateOutputBox();
    private readonly TextBox restaurantKeyTextBox = new();
    private readonly TextBox restaurantSecretTextBox = new();
    private readonly TextBox restaurantIdTextBox = new();
    private readonly TextBox restaurantDomainTextBox = new();
    private readonly TextBox restaurantResultTextBox = CreateOutputBox();
    private readonly TextBox xmlKeyTextBox = new();
    private readonly TextBox xmlSecretTextBox = new();
    private readonly TextBox xmlRestaurantIdTextBox = new();
    private readonly TextBox xmlDomainTextBox = new();
    private readonly TextBox xmlResultTextBox = CreateOutputBox();
    private readonly Label requestStatusLabel = new();
    private readonly Label restaurantStatusLabel = new();
    private readonly Label xmlStatusLabel = new();
    private readonly Button sendButton = new();
    private readonly Button restaurantButton = new();
    private readonly Button xmlButton = new();
    private readonly Button saveXmlButton = new();

    public MainForm()
    {
        Text = "OLO API Test Tool";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(900, 650);
        Size = new Size(1100, 780);
        Font = new Font("Segoe UI", 9F);
        BuildLayout();
        methodComboBox.SelectedIndexChanged += (_, _) => UpdateBodyState();
        UpdateBodyState();
    }

    private void BuildLayout()
    {
        var tabs = new TabControl { Dock = DockStyle.Fill, Padding = new Point(14, 6) };
        tabs.TabPages.Add(BuildRestaurantTab());
        tabs.TabPages.Add(BuildRequestTab());
        tabs.TabPages.Add(BuildXmlTab());
        Controls.Add(tabs);
    }

    private TabPage BuildRestaurantTab()
    {
        var page = new TabPage("Restaurant JSON");
        var root = CreateRoot(7);
        AddTitle(root, "Restaurant JSON", "Enter the credentials and restaurant ID to retrieve restaurant details.", 0);
        AddField(root, "Key / Client ID", restaurantKeyTextBox, 1);
        AddField(root, "Secret", restaurantSecretTextBox, 2, password: true);
        AddField(root, "Restaurant ID", restaurantIdTextBox, 3);
        AddField(root, "Domain", restaurantDomainTextBox, 4, DefaultDomain);
        AddActionRow(root, restaurantButton, restaurantStatusLabel, "Get Restaurant JSON", 5);
        AddResult(root, restaurantResultTextBox, 6);
        restaurantButton.Click += async (_, _) => await GetRestaurantAsync();
        page.Controls.Add(root);
        return page;
    }

    private TabPage BuildRequestTab()
    {
        var page = new TabPage("Custom Request");
        var root = CreateRoot(9);
        AddTitle(root, "Custom OLO Request", "Send a signed GET, POST, or PUT request.", 0);
        AddField(root, "Domain", domainTextBox, 1, "https://api.example.com");
        AddMethodField(root, 2);
        AddField(root, "Path and Query", pathTextBox, 3, "/v1/restaurants");
        bodyTextBox.Multiline = true;
        bodyTextBox.ScrollBars = ScrollBars.Both;
        bodyTextBox.AcceptsTab = true;
        bodyTextBox.WordWrap = false;
        bodyTextBox.Font = new Font("Consolas", 9.5F);
        AddField(root, "Body (JSON)", bodyTextBox, 4, "{\r\n  \"example\": true\r\n}");
        AddField(root, "Key / Client ID", keyTextBox, 5);
        AddField(root, "Secret", secretTextBox, 6, password: true);
        AddActionRow(root, sendButton, requestStatusLabel, "Send Request", 7);
        AddResult(root, requestResultTextBox, 8);
        sendButton.Click += async (_, _) => await SendCustomRequestAsync();
        page.Controls.Add(root);
        return page;
    }

    private TabPage BuildXmlTab()
    {
        var page = new TabPage("Restaurant XML Export");
        var root = CreateRoot(7);
        AddTitle(root, "Restaurant XML Export", "Download restaurant menu XML using the OLO restaurant endpoint.", 0);
        AddField(root, "Key / Client ID", xmlKeyTextBox, 1);
        AddField(root, "Secret", xmlSecretTextBox, 2, password: true);
        AddField(root, "Restaurant ID", xmlRestaurantIdTextBox, 3);
        AddField(root, "Domain", xmlDomainTextBox, 4, DefaultDomain);
        var actions = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true };
        xmlButton.Text = "Fetch Restaurant XML";
        xmlButton.Width = 155;
        xmlButton.Height = 32;
        xmlButton.Click += async (_, _) => await GetRestaurantXmlAsync();
        actions.Controls.Add(xmlButton);
        saveXmlButton.Text = "Save XML As...";
        saveXmlButton.Width = 120;
        saveXmlButton.Height = 32;
        saveXmlButton.Enabled = false;
        saveXmlButton.Click += (_, _) => SaveXml();
        actions.Controls.Add(saveXmlButton);
        xmlStatusLabel.AutoSize = true;
        xmlStatusLabel.Margin = new Padding(12, 8, 0, 0);
        actions.Controls.Add(xmlStatusLabel);
        root.Controls.Add(actions, 1, 5);
        AddResult(root, xmlResultTextBox, 6);
        page.Controls.Add(root);
        return page;
    }

    private static TableLayoutPanel CreateRoot(int rows)
    {
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(18),
            ColumnCount = 2,
            RowCount = rows,
            BackColor = Color.FromArgb(248, 249, 251)
        };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 145));
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        for (var i = 0; i < rows; i++)
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
        root.RowStyles[rows - 1] = new RowStyle(SizeType.Percent, 100);
        return root;
    }

    private static void AddTitle(TableLayoutPanel root, string titleText, string subtitle, int row)
    {
        root.RowStyles[row] = new RowStyle(SizeType.Absolute, 58);
        var panel = new Panel { Dock = DockStyle.Fill };
        panel.Controls.Add(new Label
        {
            Text = titleText,
            Font = new Font("Segoe UI", 12F, FontStyle.Bold),
            ForeColor = Color.FromArgb(45, 55, 72),
            Dock = DockStyle.Top,
            Height = 27
        });
        panel.Controls.Add(new Label { Text = subtitle, ForeColor = Color.DimGray, Dock = DockStyle.Fill });
        root.Controls.Add(panel, 0, row);
        root.SetColumnSpan(panel, 2);
    }

    private static void AddField(TableLayoutPanel root, string labelText, TextBox textBox, int row, string initialValue = "", bool password = false)
    {
        root.Controls.Add(new Label { Text = labelText, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, row);
        textBox.Dock = DockStyle.Fill;
        textBox.Margin = new Padding(3, 5, 3, 5);
        textBox.Text = initialValue;
        textBox.UseSystemPasswordChar = password;
        root.Controls.Add(textBox, 1, row);
    }

    private void AddMethodField(TableLayoutPanel root, int row)
    {
        root.Controls.Add(new Label { Text = "Request Method", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, row);
        methodComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
        methodComboBox.Items.AddRange(new object[] { "GET", "POST", "PUT" });
        methodComboBox.SelectedIndex = 0;
        methodComboBox.Dock = DockStyle.Left;
        methodComboBox.Width = 150;
        root.Controls.Add(methodComboBox, 1, row);
    }

    private static void AddActionRow(TableLayoutPanel root, Button button, Label status, string buttonText, int row)
    {
        var panel = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true };
        button.Text = buttonText;
        button.Width = 145;
        button.Height = 32;
        button.BackColor = Color.FromArgb(31, 111, 235);
        button.ForeColor = Color.White;
        button.FlatStyle = FlatStyle.Flat;
        panel.Controls.Add(button);
        status.AutoSize = true;
        status.Margin = new Padding(12, 8, 0, 0);
        panel.Controls.Add(status);
        root.Controls.Add(panel, 1, row);
    }

    private static void AddResult(TableLayoutPanel root, TextBox output, int row)
    {
        var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, 4, 0, 0) };
        panel.Controls.Add(new Label { Text = "Response", Dock = DockStyle.Top, Height = 25 });
        output.Dock = DockStyle.Fill;
        panel.Controls.Add(output);
        root.Controls.Add(panel, 0, row);
        root.SetColumnSpan(panel, 2);
    }

    private void UpdateBodyState()
    {
        var hasBody = methodComboBox.SelectedItem?.ToString() is "POST" or "PUT";
        bodyTextBox.Enabled = hasBody;
        bodyTextBox.BackColor = hasBody ? Color.White : SystemColors.Control;
    }

    private async Task GetRestaurantAsync()
    {
        if (!TryReadQuickInputs(restaurantKeyTextBox, restaurantSecretTextBox, restaurantIdTextBox, restaurantStatusLabel, out var key, out var secret, out var id)) return;
        if (!TryNormalizeDomain(restaurantDomainTextBox.Text, out var domain))
        {
            SetStatus(restaurantStatusLabel, "Domain is invalid.", Color.Firebrick);
            return;
        }
        await RunQuickRequestAsync(restaurantStatusLabel, restaurantButton, domain, "GET", $"/v1.1/restaurants/{Uri.EscapeDataString(id)}", key, secret, restaurantResultTextBox);
    }

    private async Task GetRestaurantXmlAsync()
    {
        if (!TryReadQuickInputs(xmlKeyTextBox, xmlSecretTextBox, xmlRestaurantIdTextBox, xmlStatusLabel, out var key, out var secret, out var id)) return;
        SetBusy(xmlButton, true, "Fetching...");
        saveXmlButton.Enabled = false;
        try
        {
            var domain = xmlDomainTextBox.Text.Trim();
            if (!TryNormalizeDomain(domain, out domain))
            {
                SetStatus(xmlStatusLabel, "Domain is invalid.", Color.Firebrick);
                return;
            }
            var response = await SendSignedAsync(domain, "GET", $"/v1.1/vendorexport/xml?vendorid={Uri.EscapeDataString(id)}", key, secret, null);
            var bytes = await response.Content.ReadAsByteArrayAsync();
            var xml = ExtractXml(bytes);
            xmlResultTextBox.Text = xml;
            saveXmlButton.Enabled = !string.IsNullOrWhiteSpace(xml);
            SetStatus(xmlStatusLabel, response.IsSuccessStatusCode ? "XML loaded." : $"HTTP {(int)response.StatusCode}.", response.IsSuccessStatusCode ? Color.DarkGreen : Color.Firebrick);
        }
        catch (Exception ex)
        {
            xmlResultTextBox.Text = ex.Message;
            SetStatus(xmlStatusLabel, "Request failed.", Color.Firebrick);
        }
        finally { SetBusy(xmlButton, false, "Fetch Restaurant XML"); }
    }

    private async Task SendCustomRequestAsync()
    {
        if (!TryBuildUri(out var uri, out var error)) { SetStatus(requestStatusLabel, error, Color.Firebrick); return; }
        if (string.IsNullOrWhiteSpace(keyTextBox.Text) || string.IsNullOrWhiteSpace(secretTextBox.Text)) { SetStatus(requestStatusLabel, "Key and Secret are required.", Color.Firebrick); return; }
        var method = methodComboBox.SelectedItem?.ToString() ?? "GET";
        var body = method is "POST" or "PUT" ? bodyTextBox.Text : string.Empty;
        if (method is "POST" or "PUT" && !IsJson(body, out error)) { SetStatus(requestStatusLabel, error, Color.Firebrick); return; }
        SetBusy(sendButton, true, "Sending...");
        try
        {
            var response = await SendSignedAsync(uri.GetLeftPart(UriPartial.Authority), method, uri.PathAndQuery, keyTextBox.Text, secretTextBox.Text, body);
            var text = await response.Content.ReadAsStringAsync();
            requestResultTextBox.Text = FormatJsonOrText(text, response.StatusCode, response.ReasonPhrase);
            SetStatus(requestStatusLabel, $"HTTP {(int)response.StatusCode} {response.ReasonPhrase}", response.IsSuccessStatusCode ? Color.DarkGreen : Color.Firebrick);
        }
        catch (Exception ex) { requestResultTextBox.Text = ex.Message; SetStatus(requestStatusLabel, "Request failed.", Color.Firebrick); }
        finally { SetBusy(sendButton, false, "Send Request"); }
    }

    private async Task RunQuickRequestAsync(Label status, Button button, string domain, string method, string path, string key, string secret, TextBox output)
    {
        SetBusy(button, true, "Loading...");
        try
        {
            var response = await SendSignedAsync(domain, method, path, key, secret, null);
            var text = await response.Content.ReadAsStringAsync();
            output.Text = FormatJsonOrText(text, response.StatusCode, response.ReasonPhrase);
            SetStatus(status, $"HTTP {(int)response.StatusCode} {response.ReasonPhrase}", response.IsSuccessStatusCode ? Color.DarkGreen : Color.Firebrick);
        }
        catch (Exception ex) { output.Text = ex.Message; SetStatus(status, "Request failed.", Color.Firebrick); }
        finally { SetBusy(button, false, button == restaurantButton ? "Get Restaurant JSON" : button.Text); }
    }

    private async Task<HttpResponseMessage> SendSignedAsync(string domain, string method, string pathAndQuery, string key, string secret, string? body)
    {
        using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(60) };
        using var request = new HttpRequestMessage(new HttpMethod(method), new Uri(domain.TrimEnd('/') + "/" + pathAndQuery.TrimStart('/')));
        if (body != null) request.Content = new StringContent(body, Encoding.UTF8, "application/json");
        var timestamp = DateTime.UtcNow.ToString("R", System.Globalization.CultureInfo.InvariantCulture);
        var contentType = request.Content?.Headers.ContentType?.ToString() ?? string.Empty;
        var bodyHash = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(body ?? string.Empty)));
        var message = string.Join("\n", key, method, contentType, bodyHash, request.RequestUri!.PathAndQuery, timestamp);
        var signature = Convert.ToBase64String(HMACSHA256.HashData(Encoding.UTF8.GetBytes(secret), Encoding.UTF8.GetBytes(message)));
        request.Headers.Authorization = new AuthenticationHeaderValue("OloSignature", $"{key}:{signature}");
        request.Headers.TryAddWithoutValidation("Date", timestamp);
        request.Headers.TryAddWithoutValidation("X-Forwarded-For", "127.0.0.1");
        return await client.SendAsync(request);
    }

    private void SaveXml()
    {
        using var dialog = new SaveFileDialog { Filter = "XML files (*.xml)|*.xml|All files (*.*)|*.*", FileName = "restaurant.xml" };
        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            File.WriteAllText(dialog.FileName, xmlResultTextBox.Text, Encoding.UTF8);
            SetStatus(xmlStatusLabel, "XML saved.", Color.DarkGreen);
        }
    }

    private static string ExtractXml(byte[] bytes)
    {
        if (bytes.Length >= 2 && bytes[0] == (byte)'P' && bytes[1] == (byte)'K')
        {
            using var archive = new ZipArchive(new MemoryStream(bytes), ZipArchiveMode.Read);
            var entry = archive.Entries.FirstOrDefault(e => e.FullName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase));
            if (entry == null) return "No XML file was found in the downloaded archive.";
            using var reader = new StreamReader(entry.Open(), Encoding.UTF8);
            return FormatXml(reader.ReadToEnd());
        }
        var text = Encoding.UTF8.GetString(bytes);
        return text.TrimStart().StartsWith("<", StringComparison.Ordinal) ? FormatXml(text) : text;
    }

    private static string FormatXml(string text)
    {
        try { var document = new XmlDocument(); document.LoadXml(text); using var writer = new StringWriter(); document.Save(writer); return writer.ToString(); }
        catch (XmlException) { return text; }
    }

    private bool TryBuildUri(out Uri uri, out string error)
    {
        uri = null!;
        var domain = domainTextBox.Text.Trim();
        var path = pathTextBox.Text.Trim();
        if (!TryNormalizeDomain(domain, out domain) || !Uri.TryCreate(domain + "/" + path.TrimStart('/'), UriKind.Absolute, out uri!) || uri.Scheme is not ("http" or "https")) { error = "Domain or path is invalid."; return false; }
        error = string.Empty;
        return true;
    }

    private static bool TryNormalizeDomain(string value, out string domain)
    {
        domain = value.Trim().TrimEnd('/');
        if (!domain.StartsWith("http://", StringComparison.OrdinalIgnoreCase) && !domain.StartsWith("https://", StringComparison.OrdinalIgnoreCase)) domain = "https://" + domain;
        return Uri.TryCreate(domain, UriKind.Absolute, out var uri) && (uri.Scheme is "http" or "https") && string.IsNullOrEmpty(uri.PathAndQuery.Trim('/'));
    }

    private static bool TryReadQuickInputs(TextBox keyBox, TextBox secretBox, TextBox idBox, Label status, out string key, out string secret, out string id)
    {
        key = keyBox.Text.Trim(); secret = secretBox.Text; id = idBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(secret) || string.IsNullOrWhiteSpace(id)) { SetStatus(status, "Key, Secret, and Restaurant ID are required.", Color.Firebrick); return false; }
        return true;
    }

    private static bool IsJson(string value, out string error)
    {
        try { JsonDocument.Parse(value); error = string.Empty; return true; }
        catch (JsonException ex) { error = "Body must be valid JSON: " + ex.Message; return false; }
    }

    private static string FormatJsonOrText(string text, HttpStatusCode status, string? reason)
    {
        try { using var document = JsonDocument.Parse(text); return $"HTTP {(int)status} {reason}\r\n\r\n{JsonSerializer.Serialize(document.RootElement, new JsonSerializerOptions { WriteIndented = true })}"; }
        catch (JsonException) { return $"HTTP {(int)status} {reason}\r\n\r\n{text}"; }
    }

    private static TextBox CreateOutputBox() => new() { Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Both, WordWrap = false, Font = new Font("Consolas", 10F), BackColor = Color.White };

    private static void SetBusy(Button button, bool busy, string text) { button.Enabled = !busy; button.Text = text; }
    private static void SetStatus(Label label, string text, Color color) { label.Text = text; label.ForeColor = color; }
}
