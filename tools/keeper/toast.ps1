# Shown when the keeper's pokes stop working (run under Windows PowerShell 5 for WinRT).
param([string]$Message = "Orchestrator unresponsive after 3 pokes - likely needs /login. Open the main-2 orchestrator terminal.")
[Windows.UI.Notifications.ToastNotificationManager, Windows.UI.Notifications, ContentType = WindowsRuntime] | Out-Null
$xml = [Windows.UI.Notifications.ToastNotificationManager]::GetTemplateContent([Windows.UI.Notifications.ToastTemplateType]::ToastText02)
$nodes = $xml.GetElementsByTagName("text")
$nodes.Item(0).AppendChild($xml.CreateTextNode("SBR Studio Keeper")) | Out-Null
$nodes.Item(1).AppendChild($xml.CreateTextNode($Message)) | Out-Null
[Windows.UI.Notifications.ToastNotificationManager]::CreateToastNotifier("SBR Studio Keeper").Show([Windows.UI.Notifications.ToastNotification]::new($xml))
