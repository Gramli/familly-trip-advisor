# How to Run — Family Trip Advisor

This guide will walk you through everything you need to get the app up and running on your computer. No technical experience required — just follow the steps in order.

---

## What You Will Need

Before starting the app, make sure you have all of the following installed and configured:

---

### 1. Ollama (the local AI engine)

Ollama runs the AI model on your own computer — no internet AI service needed.

1. Go to [https://ollama.com](https://ollama.com) and download the installer for your operating system.
2. Run the installer and follow the on-screen instructions.
3. Once installed, open **PowerShell as Administrator** (see tip below) and run this command to allow the app to talk to Ollama across your network:

```powershell
[System.Environment]::SetEnvironmentVariable('OLLAMA_HOST', '0.0.0.0:11434', 'Machine')
```

> **How to open PowerShell as Administrator:**
> Press the Windows key, type `PowerShell`, right-click on **Windows PowerShell**, and choose **Run as administrator**.

4. **Close and reopen your terminal** after running that command so the change takes effect.

---

### 2. The AI Model (`gemma4:e2b`)

After Ollama is installed and your computer is restarted, open a regular terminal (PowerShell or Command Prompt) and download the AI model:

```powershell
ollama pull gemma4:e2b
```

This may take several minutes depending on your internet speed. The model is around a few gigabytes.

---

### 3. Node.js (required for the website frontend)

1. Go to [https://nodejs.org](https://nodejs.org) and download the **LTS** version (recommended for most users).
2. Run the installer — you can leave all options at their defaults.
3. To verify it installed correctly, open a terminal and type:

```powershell
node --version
```

You should see a version number (e.g., `v22.x.x`).

---

### 4. .NET 10 SDK (required for the backend)

1. Go to [https://dotnet.microsoft.com/download](https://dotnet.microsoft.com/download) and download the **.NET 10 SDK**.
2. Run the installer.
3. To verify it installed correctly, open a terminal and type:

```powershell
dotnet --version
```

You should see `10.x.x`.

---

### 5. API Keys

The app needs two free API keys to fetch places and weather data.

#### Geoapify (for finding places)

1. Go to [https://www.geoapify.com](https://www.geoapify.com) and create a free account.
2. After logging in, go to your dashboard and copy your **API Key**.
3. Open the file `backend/appsettings.json` in a text editor and replace the value next to `"ApiKey"` under the `"Geoapify"` section with your key.

```json
"Geoapify": {
  "ApiKey": "PASTE_YOUR_GEOAPIFY_KEY_HERE"
}
```

#### Weatherbit via RapidAPI (for weather forecasts)

1. Go to [https://rapidapi.com](https://rapidapi.com) and create a free account.
2. Search for **Weatherbit** and subscribe to the free plan.
3. Copy your **X-RapidAPI-Key** from the API page.
4. Open `backend/appsettings.json` and replace the value next to `"XRapidAPIKey"` under the `"Weatherbit"` section:

```json
"Weatherbit": {
  "XRapidAPIKey": "PASTE_YOUR_RAPIDAPI_KEY_HERE"
}
```

---

## Starting the App

Once all prerequisites are in place, starting the app is a single command.

### On Windows (PowerShell)

Open **PowerShell** in the project folder and run:

```powershell
.\start-app.ps1
```

> **How to open PowerShell in the project folder:**
> Navigate to the project folder in File Explorer, click the address bar at the top, type `powershell`, and press Enter.

The script will open **two separate windows** — one for the backend and one for the frontend. You can watch each one start up independently.

Once both windows show they are running, open your browser and go to:

```
http://localhost:4200
```

---

### Script Arguments

The script accepts optional switches to control what gets started:

| Argument | Description |
|---|---|
| `-Backend` | Start the backend only |
| `-Frontend` | Start the frontend only |
| `-Expose` | Bind to `0.0.0.0` so the app is accessible from other devices on your local network |

**Examples:**

```powershell
# Start only the backend
.\start-app.ps1 -Backend

# Start only the frontend
.\start-app.ps1 -Frontend

# Start both and expose on your local network
.\start-app.ps1 -Expose

# Start only the backend and expose it
.\start-app.ps1 -Backend -Expose
```

When `-Expose` is used, the script will print the local IP address you can share with other devices on your network.

---

### Stopping the App

To stop a service, simply **close its window** or press **Ctrl + C** inside it. Close both windows to stop the app completely.

---

## Troubleshooting

| Problem | What to try |
|---|---|
| `ollama` command not found | Make sure Ollama is installed and your terminal was restarted after installation |
| `dotnet` command not found | Make sure .NET 10 SDK is installed and your terminal was restarted |
| `node` command not found | Make sure Node.js is installed and your terminal was restarted |
| App opens but shows no results | Double-check your API keys in `backend/appsettings.json` |
| AI responses are very slow | The first response may take a while as the model loads — this is normal |
| Script won't run (security error) | Run `Set-ExecutionPolicy -Scope CurrentUser RemoteSigned` in PowerShell first |

---

## Quick Summary Checklist

- [ ] Ollama installed
- [ ] `OLLAMA_HOST` environment variable set (Admin PowerShell) + computer restarted
- [ ] `gemma4:e2b` model downloaded via `ollama pull gemma4:e2b`
- [ ] Node.js installed
- [ ] .NET 10 SDK installed
- [ ] Geoapify API key added to `backend/appsettings.json`
- [ ] Weatherbit (RapidAPI) key added to `backend/appsettings.json`
- [ ] App started with `.\start-app.ps1` in PowerShell
- [ ] Browser opened at `http://localhost:4200`
