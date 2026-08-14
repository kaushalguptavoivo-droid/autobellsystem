# School Auto Bell System

A Windows desktop app (WPF, .NET 8) that automatically rings a school bell
on a schedule — through your PC's speakers or a paired Bluetooth speaker —
fully offline, and starts automatically with Windows.

## Features
- Add/edit/delete bell periods (time, days, name, ring pattern).
- Three ready-made ring patterns: Regular Period (1 ring), Assembly
  (3 long rings, "tan-tan-tan"), Lunch (2 rings) — plus Custom ring counts.
- Works with **any** Windows playback device, including a paired Bluetooth
  speaker — pick it from a dropdown in the "Bluetooth & Sound" tab.
- Built-in synthesized bell tone, so it works even if you never add your
  own .wav/.mp3 file — no internet connection required, ever.
- Runs in the system tray; closing the dashboard window does not stop it.
- Auto-starts with Windows (via the Registry Run key), hidden in the tray.
- Pause/Resume all bells and "Test Bell Now" from the tray right-click menu.

## Important: this only runs on Windows
This is a WPF desktop app — it will not run on macOS, Linux, phones, or in
a browser. You need a Windows 10/11 PC to run the finished `.exe`.

## Getting the .exe — you do NOT need to install anything yourself
This repository includes a GitHub Actions workflow that builds the `.exe`
for you automatically in the cloud:

1. Push/upload this project to a GitHub repository.
2. Go to the **Actions** tab → **Build Windows EXE** → **Run workflow**
   (or just push a commit — it runs automatically).
3. Wait ~2 minutes for it to finish.
4. Open the finished run, scroll to **Artifacts**, and download
   `AutoBellSystem-Windows-x64` — that's your `.exe`, ready to run on
   any Windows PC (no need to install .NET on that PC either, since it's
   published self-contained).

See `INSTALL.md` for step-by-step setup and `USER_GUIDE.md` for daily use.
