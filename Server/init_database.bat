@echo off
SETLOCAL

:: ====== Chuyển thư mục sang nơi chứa init.sql ======
cd /d ".\extra_files\MyServerData"

:: ====== Config ======
SET SERVER=localhost
SET DB=base
SET BACKUP_FILE=%CD%\backup\backup.xlsx
SET SQL_FILE=init.sql
SET AUTH_MODE=1
SET USER=sa
SET PASSWORD=svcntt
SET APPEND_MODE=1

:: Tạo folder backup nếu chưa có
mkdir backup 2>nul

:: ====== Check if database exists ======
IF %AUTH_MODE%==1 (
    sqlcmd -S %SERVER% -d master -Q "IF DB_ID(N'%DB%') IS NOT NULL PRINT 'EXISTS'" -E -h -1 > db_check.txt
) ELSE (
    sqlcmd -S %SERVER% -d master -U %USER% -P %PASSWORD% -Q "IF DB_ID(N'%DB%') IS NOT NULL PRINT 'EXISTS'" -h -1 > db_check.txt
)
set /p DB_EXISTS=<db_check.txt
del db_check.txt

IF "%DB_EXISTS%"=="EXISTS" (
    echo Database %DB% exists. Starting backup...

    :: ====== Call PowerShell backup script ======
    powershell -NoProfile -ExecutionPolicy Bypass -Command ^
    "$tables=@('account','account_identity','cart','completed_orders','file','film','item','order','post','reaction','review','shop','user','user_bank','user_society','voucher');" ^
    "$excelFile='%BACKUP_FILE%';" ^
    "$excelFileAbs=[System.IO.Path]::GetFullPath($excelFile);" ^
    "Write-Host 'Target Excel file: '+$excelFileAbs;" ^
    "if(-not (Test-Path (Split-Path $excelFileAbs -Parent))) {Write-Host 'Backup directory does not exist or is inaccessible.'; exit 1;};" ^
    "if (Test-Path $excelFileAbs) {Write-Host 'Opening existing Excel file.'; $excel=New-Object -ComObject Excel.Application; try { $wb=$excel.Workbooks.Open($excelFileAbs) } catch { Write-Host 'Failed to open Excel file: $_'; $excel.Quit(); [System.Runtime.Interopservices.Marshal]::ReleaseComObject($excel) | Out-Null; exit 1; }} else {Write-Host 'Creating new Excel file.'; $excel=New-Object -ComObject Excel.Application; $wb=$excel.Workbooks.Add()};" ^
    "$excel.Visible=$false; $excel.DisplayAlerts=$false;" ^
    "foreach($table in $tables) {" ^
    "  $conn=New-Object System.Data.SqlClient.SqlConnection;" ^
    "  if ('%AUTH_MODE%' -eq '1') {$conn.ConnectionString='Server=%SERVER%;Database=%DB%;Integrated Security=True;'} else {$conn.ConnectionString='Server=%SERVER%;Database=%DB%;User ID=%USER%;Password=%PASSWORD%;';};" ^
    "  try { $conn.Open(); } catch { Write-Host 'Failed to connect to database: $_'; if ($wb) {$wb.Close($false)}; $excel.Quit(); [System.Runtime.Interopservices.Marshal]::ReleaseComObject($wb) | Out-Null; [System.Runtime.Interopservices.Marshal]::ReleaseComObject($excel) | Out-Null; exit 1; };" ^
    "  $cmd=$conn.CreateCommand(); $cmd.CommandText='SELECT * FROM ['+$table+']';" ^
    "  $adapter=New-Object System.Data.SqlClient.SqlDataAdapter $cmd;" ^
    "  $ds=New-Object System.Data.DataSet; try { $adapter.Fill($ds) | Out-Null; } catch { Write-Host 'Failed to query table '+$table+': $_'; $conn.Close(); if ($wb) {$wb.Close($false)}; $excel.Quit(); [System.Runtime.Interopservices.Marshal]::ReleaseComObject($wb) | Out-Null; [System.Runtime.Interopservices.Marshal]::ReleaseComObject($excel) | Out-Null; exit 1; }; $conn.Close();" ^
    "  $sheetName=($table -replace '[\\/:*?\[\]]',''); if($sheetName.Length -gt 31){$sheetName=$sheetName.Substring(0,31)};" ^
    "  try { $sheet=$wb.Sheets | Where-Object {$_.Name -eq $sheetName}; if (-not $sheet) {Write-Host 'Creating new sheet: '+$sheetName; $sheet=$wb.Sheets.Add(); $sheet.Name=$sheetName; $startRow=1;} else {if ('%APPEND_MODE%' -eq '1') {Write-Host 'Appending to existing sheet: '+$sheetName; $startRow=($sheet.UsedRange.Rows.Count + 1); if ($startRow -eq 1) {$startRow=2;};} else {Write-Host 'Overwriting existing sheet: '+$sheetName; $sheet.Cells.Clear(); $startRow=1;};}; } catch { Write-Host 'Failed to access or create sheet for table '+$table+': $_'; if ($wb) {$wb.Close($false)}; $excel.Quit(); [System.Runtime.Interopservices.Marshal]::ReleaseComObject($wb) | Out-Null; [System.Runtime.Interopservices.Marshal]::ReleaseComObject($excel) | Out-Null; exit 1; };" ^
    "  if ($startRow -eq 1) {for($col=0;$col -lt $ds.Tables[0].Columns.Count;$col++) {$sheet.Cells.Item(1,$col+1)=$ds.Tables[0].Columns[$col].ColumnName};};" ^
    "  for($row=0;$row -lt $ds.Tables[0].Rows.Count;$row++) {for($col=0;$col -lt $ds.Tables[0].Columns.Count;$col++) {$sheet.Cells.Item($startRow+$row,$col+1)=$ds.Tables[0].Rows[$row][$col]}};" ^
    "};" ^
    "try { $wb.SaveAs($excelFileAbs); Write-Host 'Excel file saved successfully: '+$excelFileAbs; } catch { Write-Host 'Error saving Excel file: $_'; if ($wb) {$wb.Close($false)}; $excel.Quit(); [System.Runtime.Interopservices.Marshal]::ReleaseComObject($wb) | Out-Null; [System.Runtime.Interopservices.Marshal]::ReleaseComObject($excel) | Out-Null; exit 1; };" ^
    "$wb.Close($false); $excel.Quit(); [System.Runtime.Interopservices.Marshal]::ReleaseComObject($wb) | Out-Null; [System.Runtime.Interopservices.Marshal]::ReleaseComObject($excel) | Out-Null;"
) ELSE (
    echo Database %DB% does not exist. No backup needed.
)

:: ====== Run init.sql ======
ECHO Running SQLCMD to execute %SQL_FILE%...
IF %AUTH_MODE%==1 (
    sqlcmd -S %SERVER% -d master -E -I -r0 -v DatabaseName="%DB%" -i %SQL_FILE% -o output.log
) ELSE (
    sqlcmd -S %SERVER% -d master -U %USER% -P %PASSWORD% -I -r0 -v DatabaseName="%DB%" -i %SQL_FILE% -o output.log
)

IF %ERRORLEVEL% EQU 0 (
    ECHO Script executed successfully. Check output.log for details.
    ECHO Database '%DB%' has been recreated and initialized.
) ELSE (
    ECHO An error occurred. Check output.log for details.
)

pause
ENDLOCAL