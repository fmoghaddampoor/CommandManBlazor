# Deployment Guide for CommandManBlazor

This guide explains how to deploy CommandManBlazor and integrate it with Windows File Explorer using the System Tray Application.

## 1. Publishing the Application

To publish the application, run the `publish.ps1` script from the root of the source directory:

```powershell
./publish.ps1
```

This will create a `publish` directory containing:
- `CommandMan.Tray.exe`: The main launcher and system tray application.
- `CommandMan.Web.exe`: The backend server (managed automatically by the Tray App).
- `SetupContextMenu.ps1`: Script to register the context menu.

## 2. File Explorer Integration

Once published, you can enable the "Open with CommandMan" context menu entry:

1. Open PowerShell in the `publish` directory.
2. Run the setup script:
   ```powershell
   ./SetupContextMenu.ps1
   ```

Now, when you right-click any folder or right-click inside a folder background in File Explorer, you'll see **Open with CommandMan**. This will launch the Tray App.

## 3. Using the Application

- **Tray Icon**: The application runs in the system tray. Double-click the icon to show the control panel.
- **Control Panel**: Allows you to Start/Stop the server and manually Open the Browser.
- **Context Menu**: Right-click the tray icon to Exit the application (which also stops the server).
- **Multiple Windows**: Using "Open with CommandMan" on different folders will open new browser tabs/windows pointing to those folders, managed by the single background instance.

## 4. Manual Deployment

If you want to move the application to another location:
1. Copy the contents of the `publish` folder to your desired location (e.g., `C:\Tools\CommandMan`).
2. Run `SetupContextMenu.ps1` from the *new* location to update the registry paths.
