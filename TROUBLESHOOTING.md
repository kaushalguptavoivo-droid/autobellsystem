# Troubleshooting

## Bell not ringing at the scheduled time
- Make sure the app icon is present in the system tray (bottom-right,
  near the clock). If it's not there, the app isn't running.
- Right-click the tray icon → check it doesn't say "Resume Schedule"
  (that means it's currently paused).
- Open Dashboard → Schedules tab, and confirm the period's time and days
  are correct, and it's in the list.
- Click **🔔 Test Bell Now** (tray menu or Bluetooth & Sound tab) to check
  audio works at all, independent of scheduling.

## No sound / wrong speaker
- Open Dashboard → **Bluetooth & Sound** tab and check the selected
  device. If your Bluetooth speaker isn't listed, make sure it's paired
  **and currently turned on/connected** in Windows Settings → Bluetooth
  & devices, then click **Refresh**.
- If the speaker is off or out of range when a bell fires, the app
  automatically falls back to your PC's normal speakers — check those too.
- Check the volume slider in the Bluetooth & Sound tab, and also check
  Windows' own volume mixer isn't muted for this device.

## Custom sound file doesn't play
- If the selected .wav/.mp3 file was moved, renamed, or deleted, the app
  automatically falls back to the built-in bell tone instead of failing
  silently. Re-select the file in Edit → Browse if you want your custom
  sound back.

## App not starting automatically with Windows
- Open the Dashboard once (this re-registers auto-start).
- Check Task Manager → Startup apps tab, and ensure "AutoBellSystem" is
  enabled there.
- Auto-start is stored in
  `HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\Run` —
  if it's missing, re-launching the app once will recreate it.

## Closing the Dashboard window stopped the bells
This shouldn't happen — closing the Dashboard only hides it; the app
keeps running in the tray. If bells still aren't ringing, check the tray
icon is present at all (see "Bell not ringing" above); if it's gone,
the app was fully exited (via tray → Exit) and needs to be relaunched.
