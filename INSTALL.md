# Install Guide

## Step 1 — Get the .exe (no Windows PC needed for this step)
Follow the "Getting the .exe" steps in `README.md` using GitHub Actions.
You'll end up with `AutoBellSystem.exe`.

*(Alternative, if you already have a Windows PC with the .NET 8 SDK:
run `dotnet publish AutoBellSystem/AutoBellSystem.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true`
from the project folder.)*

## Step 2 — Pair your Bluetooth speaker (one-time, in Windows itself)
1. Turn on your Bluetooth speaker and put it in pairing mode.
2. Windows Settings → Bluetooth & devices → Add device → pick your speaker.
3. Once it says "Connected", you're done with this step.

## Step 3 — Run the app
1. Copy `AutoBellSystem.exe` to any folder on the Windows PC that will run
   the bells (e.g. the school office computer).
2. Double-click it. It opens the Dashboard the first time.
3. Go to the **Bluetooth & Sound** tab → select your speaker from the
   dropdown → **Set as Bell Speaker**.
4. Go to the **Schedules** tab → **+ Add Bell Period** for each period,
   assembly, lunch, etc.
5. Close the dashboard window (it keeps running in the tray, bottom-right
   near the clock — look for the bell icon).

## Auto-start
The app registers itself to start automatically (hidden, in the tray)
the next time Windows boots — no extra setup needed.

## Uninstalling
Right-click the tray icon → Exit. To also remove the auto-start entry,
open the Dashboard once, and it will re-register the correct state, or
manually delete the `AutoBellSystem` entry from:
`Registry Editor → HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\Run`
